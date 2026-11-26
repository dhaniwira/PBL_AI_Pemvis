from __future__ import annotations

from collections import defaultdict
from collections.abc import Iterable
from typing import Any

from inference.administratif_evaluation import Administratif
from inference.akademik_evaluation import Akademik
from inference.database import Database
from inference.final_result_evaluation import FinalResult
from inference.kehadiran_evaluation import Kehadiran
from inference.subsystem import (
    AMAN,
    MEETINGS,
    MINUTES_PER_SKS,
    RENDAH,
    SEDANG,
    STATUS,
    TIDAK_ADA_DATA,
    TINGGI,
)

__all__ = [
    "AMAN",
    "MEETINGS",
    "MINUTES_PER_SKS",
    "RENDAH",
    "SEDANG",
    "STATUS",
    "TIDAK_ADA_DATA",
    "TINGGI",
    "Administratif",
    "Akademik",
    "FinalResult",
    "Kehadiran",
    "build_features",
    "grade",
    "infer",
    "run",
]


def infer(fitur: dict[str, Any]) -> dict[str, Any]:
    return FinalResult(fitur).to_dict()


def grade(value: float) -> str:
    if value >= 80:
        return "A"
    if value >= 70:
        return "B"
    if value >= 60:
        return "C"
    if value >= 50:
        return "D"
    return "E"


def history_features(rows: list[dict[str, Any]]) -> dict[str, Any]:
    scores = []
    for row in rows:
        value = row["ips"] if row["ips"] is not None else row["ipk"]
        if value is not None:
            scores.append(float(value))

    if not scores:
        return {"ips": None, "delta_ips": 0.0}
    if len(scores) == 1:
        return {"ips": scores[0], "delta_ips": 0.0}
    return {"ips": scores[-1], "delta_ips": scores[-1] - scores[-2]}


def penalty_features(subjects: list[dict[str, Any]]) -> dict[str, Any]:
    d_umum = 0
    d_mpk = 0
    d_mkku = 0
    count_e = 0

    for subject in subjects:
        if subject["nilai_akhir"] is None:
            continue

        letter = grade(float(subject["nilai_akhir"]))
        category = str(subject["kategori"]).upper()

        if letter == "E":
            count_e += 1
        elif letter == "D":
            if category == "UMUM":
                d_umum += 1
            elif category == "MPK":
                d_mpk += 1
            else:
                d_mkku += 1

    penalty = (1 * d_umum) + (3 * d_mpk) + (3 * d_mkku) + (3 * count_e)
    return {
        "penalti": min(10.0, float(penalty)),
        "jumlah_d": d_umum + d_mpk + d_mkku,
        "jumlah_e": count_e,
    }


def attendance_features(subjects: list[dict[str, Any]]) -> dict[str, float]:
    max_alfa = min(100.0, max(float(row["alfa"]) for row in subjects) / MEETINGS * 100)
    minutes = sum(
        (float(row["alfa"]) + float(row["telat"])) * int(row["sks"]) * MINUTES_PER_SKS
        for row in subjects
    )
    hours = (
        sum(
            (float(row["alfa"]) + float(row["izin"]) + float(row["sakit"]))
            * int(row["sks"])
            * MINUTES_PER_SKS
            for row in subjects
        )
        / 60.0
    )
    return {"max_alfa": max_alfa, "menit": minutes, "jam": hours}


def sort_key(row: dict[str, Any]) -> tuple[int, int, int, int, int]:
    semester = 1 if str(row["ta_semester"]).upper() == "GANJIL" else 2
    return (
        int(row["tahun_mulai"]),
        int(row["tahun_selesai"]),
        semester,
        int(row["semester"]),
        int(row["id"]),
    )


def read_status(rows: list[dict[str, Any]]) -> tuple[int, list[str]]:
    status = (rows[-1]["status"] or "").strip().lower()
    if status == "mangkir":
        consecutive = 0
        for row in reversed(rows):
            if (row["status"] or "").strip().lower() != "mangkir":
                break
            consecutive += 1
        if consecutive >= 2:
            return 4, [f"Terdeteksi mangkir beruntun {consecutive} semester."]
        return 3, []

    code = {"aktif": 0, "cuti": 1, "cuti_akademik": 2}.get(status)
    if code is None:
        return 0, [
            f"Status '{rows[-1]['status']}' tidak dikenal; dianggap Aktif Normal."
        ]
    return code, []


def base_features(student: dict[str, Any]) -> dict[str, Any]:
    masa_studi = int(student["masa_studi"])
    maksimal = int(student["masa_studi_maksimal"])
    return {
        "nim": str(student["nim"]).strip(),
        "nama": str(student["nama"]),
        "prodi": str(student["prodi"]),
        "jenjang": str(student["jenjang"]),
        "angkatan": int(student["angkatan"]),
        "kelas": str(student["kelas"]),
        "semester_berjalan": int(student["semester_berjalan"]),
        "masa_studi": masa_studi,
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
        "sisa": max(0.0, float(maksimal - masa_studi)),
        "jumlah_mk": 0,
        "catatan": [],
    }


def build_one(
    student: dict[str, Any],
    rows: list[dict[str, Any]],
    details: dict[int, list[dict[str, Any]]],
) -> dict[str, Any]:
    fitur = base_features(student)
    if not rows:
        fitur["catatan"].append("Belum ada riwayat studi.")
        return fitur

    rows = sorted(rows, key=sort_key)
    last = rows[-1]
    subjects = details.get(int(last["id"]), [])

    fitur["periode"] = (
        f"{last['tahun_mulai']}/{last['tahun_selesai']} {last['ta_semester']}"
    )
    fitur["mengulang"] = bool(last["mengulang"])
    fitur["jumlah_mk"] = len(subjects)

    fitur.update(history_features(rows))
    if fitur["ips"] is None:
        fitur["catatan"].append("IPS/IPK belum diisi.")

    fitur.update(penalty_features(subjects))

    if subjects:
        fitur.update(attendance_features(subjects))
    else:
        fitur["catatan"].append("Detail mata kuliah belum tersedia.")

    fitur["status_kode"], notes = read_status(rows)
    fitur["status_nama"] = STATUS[fitur["status_kode"]]
    fitur["catatan"].extend(notes)
    return fitur


def build_features(
    students: Iterable[dict[str, Any]],
    riwayat: Iterable[dict[str, Any]],
    details: Iterable[dict[str, Any]],
) -> list[dict[str, Any]]:
    by_student: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for row in riwayat:
        by_student[str(row["nim"]).strip()].append(row)

    by_history: dict[int, list[dict[str, Any]]] = defaultdict(list)
    for row in details:
        by_history[int(row["id_riwayat_studi"])].append(row)

    return [
        build_one(student, by_student.get(str(student["nim"]).strip(), []), by_history)
        for student in students
    ]


STUDENTS_SQL = """
SELECT m.nim, m.nama, p.nama AS prodi, j.kode AS jenjang,
       j.masa_studi_maksimal, m.angkatan, m.kelas,
       m.semester_berjalan, m.masa_studi
FROM mahasiswa m
INNER JOIN prodi p ON p.id = m.id_prodi
INNER JOIN jenjang j ON j.id = p.id_jenjang
ORDER BY m.nim
"""

RIWAYAT_SQL = """
SELECT r.id, r.nim, t.tahun_mulai, t.tahun_selesai,
       t.semester AS ta_semester, r.semester, r.mengulang,
       r.status, r.ips, r.ipk
FROM riwayat_studi r
INNER JOIN tahun_akademik t ON t.id = r.id_tahun_akademik
ORDER BY r.nim, t.tahun_mulai, t.tahun_selesai,
         CASE t.semester WHEN 'GANJIL' THEN 1 ELSE 2 END,
         r.semester, r.id
"""

DETAILS_SQL = """
SELECT d.id_riwayat_studi, d.nilai_akhir, d.alfa, d.izin,
       d.sakit, d.telat, mk.kategori, mk.sks
FROM detail_mk d
INNER JOIN mk_prodi mp ON mp.id = d.id_mk_prodi
INNER JOIN mata_kuliah mk ON mk.id = mp.id_mk
ORDER BY d.id_riwayat_studi, d.id
"""


def run(db: Database) -> list[dict[str, Any]]:
    students = db.query(STUDENTS_SQL)
    riwayat = db.query(RIWAYAT_SQL)
    details = db.query(DETAILS_SQL)
    return [infer(fitur) for fitur in build_features(students, riwayat, details)]
