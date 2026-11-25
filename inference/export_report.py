from __future__ import annotations

import sys
from pathlib import Path

import pyodbc

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from inference.database import Database, load_data
from inference.features import build_features
from inference.fuzzy import infer
from inference.report import write_csv, write_json


def main(argv: list[str] | None = None) -> int:
    args = sys.argv[1:] if argv is None else argv
    if not args:
        print(
            "Pemakaian: python export_report.py <file.csv|file.json>",
            file=sys.stderr,
        )
        return 2

    path = Path(args[0]).expanduser().resolve()
    try:
        students, riwayat, details = load_data(Database())
        results = [infer(fitur) for fitur in build_features(students, riwayat, details)]
        if path.suffix.lower() == ".json":
            write_json(results, path)
        else:
            write_csv(results, path)
    except (OSError, RuntimeError, pyodbc.Error) as exc:
        print(f"Gagal mengekspor laporan: {exc}", file=sys.stderr)
        return 1

    print(f"Laporan ditulis: {path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
