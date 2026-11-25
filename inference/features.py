from __future__ import annotations

from collections import defaultdict
from collections.abc import Iterable
from typing import Any

MEETINGS = 16
MINUTES_PER_SKS = 50
STATUS_LABELS = {
    0: "Aktif Normal",
    1: "Cuti Non-Akademik",
    2: "Cuti Akademik",
    3: "Mangkir 1x",
    4: "Mangkir Beruntun",
}


def grade(nilai: float) -> str:
    if nilai >= 80:
        return "A"
    if nilai >= 70:
        return "B"
    if nilai >= 60:
        return "C"
    if nilai >= 50:
        return "D"
    return "E"


def _sort_key(row: dict[str, Any]) -> tuple[int, int, int, int, int]:
    urutan = 1 if str(row["ta_semester"]).upper() == "GANJIL" else 2
    return (
        int(row["tahun_mulai"]),
        int(row["tahun_selesai"]),
        urutan,
        int(row["semester"]),
        int(row["id"]),
    )


def _status(rows: list[dict[str, Any]]) -> tuple[int, list[str]]:
    status = (rows[-1]["status"] or "").strip().lower()
    if status == "mangkir":
        beruntun = 0
        for row in reversed(rows):
            if (row["status"] or "").strip().lower() != "mangkir":
                break
            beruntun += 1
        if beruntun >= 2:
            return 4, [f"Terdeteksi mangkir beruntun {beruntun} semester."]
        return 3, []
    kode = {"aktif": 0, "cuti": 1, "cuti_akademik": 2}.get(status)
    if kode is None:
        return 0, [
            f"Status '{rows[-1]['status']}' tidak dikenal; dianggap Aktif Normal."
        ]
    return kode, []


def _one(
    student: dict[str, Any],
    rows: list[dict[str, Any]],
    details: dict[int, list[dict[str, Any]]],
    attendance_unit: str,
) -> dict[str, Any]:
    masa = int(student["masa_studi"])
    maksimal = int(student["masa_studi_maksimal"])
    fitur = {
        "nim": str(student["nim"]).strip(),
        "nama": str(student["nama"]),
        "prodi": str(student["prodi"]),
        "jenjang": str(student["jenjang"]),
        "angkatan": int(student["angkatan"]),
        "kelas": str(student["kelas"]),
        "semester_berjalan": int(student["semester_berjalan"]),
        "masa_studi": masa,
        "masa_studi_maksimal": maksimal,
        "periode": None,
        "ips": None,
        "delta_ips": 0.0,
        "penalti": 0.0,
        "jumlah_d": 0,
        "jumlah_e": 0,
        "mengulang": False,
        "max_alfa": 0.0,
        "menit": 0.0,
        "jam": 0.0,
        "status_kode": 0,
        "status_nama": "Tidak Diketahui",
        "sisa": max(0.0, float(maksimal - masa)),
        "jumlah_mk": 0,
        "catatan": [],
    }
    if not rows:
        fitur["catatan"].append(
            "Belum ada riwayat studi; inferensi tidak dapat dijalankan."
        )
        return fitur

    rows = sorted(rows, key=_sort_key)
    terakhir = rows[-1]
    mk = details.get(int(terakhir["id"]), [])
    fitur["periode"] = (
        f"{terakhir['tahun_mulai']}/{terakhir['tahun_selesai']} "
        f"{terakhir['ta_semester']}"
    )
    fitur["mengulang"] = bool(terakhir["mengulang"])
    fitur["jumlah_mk"] = len(mk)

    nilai = []
    for row in rows:
        angka = row["ips"] if row["ips"] is not None else row["ipk"]
        if angka is not None:
            nilai.append(float(angka))
    if nilai:
        fitur["ips"] = nilai[-1]
        if len(nilai) >= 2:
            fitur["delta_ips"] = nilai[-1] - nilai[-2]
    else:
        fitur["catatan"].append("IPS/IPK belum diisi pada riwayat studi.")

    penalti = 0.0
    for row in mk:
        if row["nilai_akhir"] is None:
            continue
        huruf = grade(float(row["nilai_akhir"]))
        if huruf == "E":
            penalti += 3.0
            fitur["jumlah_e"] += 1
        elif huruf == "D":
            fitur["jumlah_d"] += 1
            penalti += 1.0 if str(row["kategori"]).upper() == "UMUM" else 3.0
    fitur["penalti"] = min(10.0, penalti)

    if mk:
        if attendance_unit == "meetings":
            fitur["max_alfa"] = max(float(row["alfa"]) for row in mk) / MEETINGS * 100
            fitur["menit"] = sum(
                (float(row["alfa"]) + float(row["telat"]))
                * int(row["sks"])
                * MINUTES_PER_SKS
                for row in mk
            )
            fitur["jam"] = (
                sum(
                    (float(row["alfa"]) + float(row["izin"]) + float(row["sakit"]))
                    * int(row["sks"])
                    * MINUTES_PER_SKS
                    for row in mk
                )
                / 60.0
            )
        else:
            fitur["max_alfa"] = max(
                float(row["alfa"])
                / (int(row["sks"]) * MINUTES_PER_SKS * MEETINGS)
                * 100
                for row in mk
            )
            fitur["menit"] = sum(float(row["alfa"]) + float(row["telat"]) for row in mk)
            fitur["jam"] = (
                sum(
                    float(row["alfa"]) + float(row["izin"]) + float(row["sakit"])
                    for row in mk
                )
                / 60.0
            )
        fitur["max_alfa"] = min(100.0, fitur["max_alfa"])
    else:
        fitur["catatan"].append("Detail mata kuliah periode terakhir belum tersedia.")

    kode, catatan = _status(rows)
    fitur["status_kode"] = kode
    fitur["status_nama"] = STATUS_LABELS[kode]
    fitur["catatan"].extend(catatan)
    return fitur


def build_features(
    students: Iterable[dict[str, Any]],
    riwayat: Iterable[dict[str, Any]],
    details: Iterable[dict[str, Any]],
    attendance_unit: str = "meetings",
) -> list[dict[str, Any]]:
    if attendance_unit not in ("meetings", "minutes"):
        raise ValueError("attendance_unit harus 'meetings' atau 'minutes'")

    by_nim: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for row in riwayat:
        by_nim[str(row["nim"]).strip()].append(row)

    by_riwayat: dict[int, list[dict[str, Any]]] = defaultdict(list)
    for row in details:
        by_riwayat[int(row["id_riwayat_studi"])].append(row)

    return [
        _one(
            student,
            by_nim.get(str(student["nim"]).strip(), []),
            by_riwayat,
            attendance_unit,
        )
        for student in students
    ]
