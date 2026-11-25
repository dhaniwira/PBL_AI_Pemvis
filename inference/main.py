from __future__ import annotations

import argparse
import sys
from pathlib import Path

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

try:
    import pyodbc

    from inference.database import (
        DEFAULT_MDF,
        DEFAULT_SERVER,
        Database,
        load_data,
        save_results,
    )
    from inference.features import build_features
    from inference.fuzzy import infer, list_rules
    from inference.report import print_detail, print_summary, write_csv, write_json
except ModuleNotFoundError as exc:
    if exc.name == "pyodbc":
        print(
            "pyodbc belum terpasang. Jalankan:\n"
            "  python -m pip install -r requirements.txt",
            file=sys.stderr,
        )
        raise SystemExit(1) from exc
    raise


def parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description=(
            "Hitung profil risiko akademik mahasiswa dengan Fuzzy Sugeno "
            "berdasarkan data Database.mdf."
        )
    )
    p.add_argument("--db", type=Path, default=DEFAULT_MDF, help="Lokasi Database.mdf.")
    p.add_argument("--server", default=DEFAULT_SERVER, help="Instance SQL Server.")
    p.add_argument(
        "--nim",
        action="append",
        default=[],
        metavar="NIM",
        help="Batasi ke NIM tertentu; boleh diulang.",
    )
    p.add_argument("--detail", metavar="NIM", help="Rincian satu mahasiswa.")
    p.add_argument(
        "--attendance-unit",
        choices=["meetings", "minutes"],
        default="meetings",
        help="Satuan kolom presensi.",
    )
    p.add_argument("--csv", type=Path, help="Simpan hasil ke CSV.")
    p.add_argument("--json", type=Path, help="Simpan hasil ke JSON.")
    p.add_argument(
        "--save", action="store_true", help="Simpan hasil ke tabel hasil_fuzzy."
    )
    p.add_argument(
        "--list-rules", action="store_true", help="Tampilkan basis aturan lalu keluar."
    )
    return p


def main(argv: list[str] | None = None) -> int:
    args = parser().parse_args(argv)

    if args.list_rules:
        print(list_rules())
        return 0

    db = Database(args.db, args.server)
    try:
        students, riwayat, details = load_data(db)
    except FileNotFoundError as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 1
    except (pyodbc.Error, OSError, RuntimeError) as exc:
        print(f"Gagal membaca database: {exc}", file=sys.stderr)
        print(
            "Pastikan SQL Server LocalDB aktif dan Database.mdf dapat di-attach.\n"
            "Buat ulang database demo dengan: python setup_database.py --with-demo",
            file=sys.stderr,
        )
        return 1

    features = build_features(students, riwayat, details, args.attendance_unit)
    results = [infer(fitur) for fitur in features]

    if args.nim:
        wanted = {nim.strip() for nim in args.nim if nim.strip()}
        ditemukan = {result["fitur"]["nim"] for result in results}
        for nim in sorted(wanted - ditemukan):
            print(f"NIM tidak ditemukan: {nim}", file=sys.stderr)
        results = [result for result in results if result["fitur"]["nim"] in wanted]

    if args.detail:
        nim = args.detail.strip()
        results = [result for result in results if result["fitur"]["nim"] == nim]
        if not results:
            print(f"NIM tidak ditemukan: {nim}", file=sys.stderr)
            return 1

    if not results:
        print("Tidak ada data mahasiswa yang dapat diproses.", file=sys.stderr)
        return 0

    if args.detail:
        print_detail(results[0])
    else:
        print_summary(results)

    if args.csv:
        write_csv(results, args.csv)
        print(f"CSV ditulis: {args.csv.resolve()}")
    if args.json:
        write_json(results, args.json)
        print(f"JSON ditulis: {args.json.resolve()}")
    if args.save:
        try:
            jumlah = save_results(db, results)
        except (pyodbc.Error, OSError, RuntimeError) as exc:
            print(f"Gagal menyimpan hasil ke database: {exc}", file=sys.stderr)
            return 1
        print(f"Hasil disimpan ke tabel hasil_fuzzy: {jumlah} baris.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
