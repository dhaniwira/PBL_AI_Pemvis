from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import pyodbc

from inference.database import DEFAULT_MDF, DEFAULT_SERVER, driver

PROJECT_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_SQL = PROJECT_ROOT / "db.sql"
DEFAULT_SEED = Path(__file__).resolve().with_name("seed_demo.sql")


def batches(script: str) -> list[str]:
    hasil: list[str] = []
    current: list[str] = []
    for line in script.splitlines():
        if re.match(r"^\s*GO\s*(?:--.*)?$", line, re.IGNORECASE):
            if current:
                hasil.append("\n".join(current))
                current = []
        else:
            current.append(line)
    if current:
        hasil.append("\n".join(current))
    return [batch for batch in hasil if batch.strip()]


def quote(name: str) -> str:
    return "[" + name.replace("]", "]]") + "]"


def main(argv: list[str] | None = None) -> int:
    p = argparse.ArgumentParser(
        description="Buat Database.mdf dari db.sql (opsional dengan data demo)."
    )
    p.add_argument("--mdf", type=Path, default=DEFAULT_MDF, help="Lokasi file MDF.")
    p.add_argument("--server", default=DEFAULT_SERVER, help="Instance SQL Server.")
    p.add_argument("--sql", type=Path, default=DEFAULT_SQL, help="Script skema + data.")
    p.add_argument(
        "--with-demo",
        action="store_true",
        help="Ikut memasukkan data demo dari seed_demo.sql.",
    )
    p.add_argument(
        "--force",
        action="store_true",
        help="Hapus database lama lalu buat ulang (DATA LAMA HILANG).",
    )
    args = p.parse_args(argv)

    mdf = args.mdf.expanduser().resolve()
    ldf = mdf.with_name(mdf.stem + "_log.ldf")
    sql_path = args.sql.expanduser().resolve()
    seed_path = DEFAULT_SEED.resolve()

    if mdf.exists() and not args.force:
        print(f"Database sudah ada: {mdf}")
        print("Gunakan --force untuk membuat ulang (data lama akan hilang).")
        return 0
    if not sql_path.exists():
        print(f"Error: script SQL tidak ditemukan: {sql_path}", file=sys.stderr)
        return 1
    if args.with_demo and not seed_path.exists():
        print(f"Error: seed demo tidak ditemukan: {seed_path}", file=sys.stderr)
        return 1

    mdf.parent.mkdir(parents=True, exist_ok=True)
    nama = mdf.stem
    odbc = driver()

    def connect(database: str = "master") -> pyodbc.Connection:
        conn_str = (
            f"DRIVER={{{odbc}}};"
            f"SERVER={args.server};"
            f"DATABASE={database};"
            "Trusted_Connection=yes;"
            "TrustServerCertificate=yes;"
        )
        return pyodbc.connect(conn_str, autocommit=True, timeout=20)

    def run(database: str, script: str) -> None:
        conn = connect(database)
        try:
            for batch in batches(script):
                conn.execute(batch)
        finally:
            conn.close()

    try:
        master = connect()
        try:
            if args.force:
                row = master.execute(
                    "SELECT DB_NAME(database_id) FROM sys.master_files "
                    "WHERE LOWER(physical_name) = LOWER(?)",
                    str(mdf),
                ).fetchone()
                lama = row[0] if row else None
                if (
                    lama is None
                    and master.execute("SELECT DB_ID(?)", nama).fetchone()[0]
                ):
                    lama = nama
                if lama:
                    master.execute(
                        f"ALTER DATABASE {quote(lama)} "
                        "SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
                    )
                    master.execute(f"DROP DATABASE {quote(lama)}")
            for path in (mdf, ldf):
                if path.exists():
                    path.unlink()
            master.execute(
                f"CREATE DATABASE {quote(nama)} ON PRIMARY "
                f"(NAME=N'{nama}', FILENAME=N'{mdf!s}') "
                f"LOG ON (NAME=N'{nama}_log', FILENAME=N'{ldf!s}')"
            )
        finally:
            master.close()

        print(f"Membuat database: {mdf}")
        run(nama, sql_path.read_text(encoding="utf-8"))
        print(f"Skema dan data awal dimuat dari: {sql_path}")

        if args.with_demo:
            run(nama, seed_path.read_text(encoding="utf-8"))
            print(f"Data demo dimuat dari: {seed_path}")
    except (pyodbc.Error, OSError, RuntimeError) as exc:
        print(f"Gagal membuat database: {exc}", file=sys.stderr)
        return 1

    print("Selesai. Jalankan: python main.py")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
