from __future__ import annotations

import csv
from collections.abc import Iterable
from pathlib import Path
from typing import Any

DEFAULT_REPORT = Path("report.csv")

FIELDS = [
    "nim",
    "nama",
    "prodi",
    "jenjang",
    "angkatan",
    "kelas",
    "semester_berjalan",
    "periode",
    "ips",
    "delta_ips",
    "penalti",
    "jumlah_d",
    "jumlah_e",
    "mengulang",
    "max_alfa",
    "menit",
    "jam",
    "status_kode",
    "status_nama",
    "sisa",
    "jumlah_mk",
    "z_akademik",
    "z_kehadiran",
    "z_administrasi",
    "skor",
    "profil",
    "catatan",
]


def subsystem_score(result: dict[str, Any], name: str) -> Any:
    subsystem = result[name]
    if subsystem is None or subsystem["skor"] is None:
        return ""
    return round(subsystem["skor"], 2)


def to_row(result: dict[str, Any]) -> dict[str, Any]:
    fitur = result["fitur"]
    return {
        "nim": fitur["nim"],
        "nama": fitur["nama"],
        "prodi": fitur["prodi"],
        "jenjang": fitur["jenjang"],
        "angkatan": fitur["angkatan"],
        "kelas": fitur["kelas"],
        "semester_berjalan": fitur["semester_berjalan"],
        "periode": fitur["periode"] or "",
        "ips": "" if fitur["ips"] is None else round(fitur["ips"], 2),
        "delta_ips": round(fitur["delta_ips"], 4),
        "penalti": fitur["penalti"],
        "jumlah_d": fitur["jumlah_d"],
        "jumlah_e": fitur["jumlah_e"],
        "mengulang": "Ya" if fitur["mengulang"] else "Tidak",
        "max_alfa": round(fitur["max_alfa"], 2),
        "menit": round(fitur["menit"], 2),
        "jam": round(fitur["jam"], 2),
        "status_kode": fitur["status_kode"],
        "status_nama": fitur["status_nama"],
        "sisa": round(fitur["sisa"], 2),
        "jumlah_mk": fitur["jumlah_mk"],
        "z_akademik": subsystem_score(result, "akademik"),
        "z_kehadiran": subsystem_score(result, "kehadiran"),
        "z_administrasi": subsystem_score(result, "administrasi"),
        "skor": "" if result["skor"] is None else round(result["skor"], 2),
        "profil": result["profil"],
        "catatan": " | ".join(fitur["catatan"]),
    }


def write_csv(
    results: Iterable[dict[str, Any]], path: str | Path = DEFAULT_REPORT
) -> Path:
    path = Path(path).expanduser()
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=FIELDS)
        writer.writeheader()
        writer.writerows(to_row(result) for result in results)
    return path.resolve()
