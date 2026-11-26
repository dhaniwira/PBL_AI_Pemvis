from __future__ import annotations

import argparse
import sys
from pathlib import Path

try:
    import pyodbc
except ModuleNotFoundError as exc:
    print(
        "pyodbc belum terpasang. Jalankan:\n"
        "  python -m pip install -r requirements.txt",
        file=sys.stderr,
    )
    raise SystemExit(1) from exc

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from inference.database import DEFAULT_MDF, DEFAULT_SERVER, Database
from inference.export import DEFAULT_REPORT, write_csv
from inference.process_evaluation import run


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Profil risiko akademik dengan Fuzzy Sugeno."
    )
    parser.add_argument(
        "--db", type=Path, default=DEFAULT_MDF, help="Lokasi Database.mdf."
    )
    parser.add_argument("--server", default=DEFAULT_SERVER, help="Instance SQL Server.")
    parser.add_argument(
        "--csv",
        type=Path,
        default=DEFAULT_REPORT,
        help="Lokasi laporan CSV (default: ./report.csv).",
    )
    args = parser.parse_args(argv)

    try:
        results = run(Database(args.db, args.server))
    except (pyodbc.Error, OSError, RuntimeError) as exc:
        print(f"Gagal membaca database: {exc}", file=sys.stderr)
        return 1

    if not results:
        print("Tidak ada data mahasiswa.", file=sys.stderr)
        return 1

    print(f"{'NIM':<12} {'Nama':<24} {'Skor':>6}  Profil")
    print("-" * 58)
    for result in results:
        fitur = result["fitur"]
        score = "-" if result["skor"] is None else f"{result['skor']:.2f}"
        print(
            f"{fitur['nim']:<12} {fitur['nama'][:24]:<24} {score:>6}  {result['profil']}"
        )

    try:
        path = write_csv(results, args.csv)
    except OSError as exc:
        print(f"Gagal menulis CSV: {exc}", file=sys.stderr)
        return 1

    print(f"\nCSV: {path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
