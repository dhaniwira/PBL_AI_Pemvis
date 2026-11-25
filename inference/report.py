from __future__ import annotations

import csv
import json
from collections import Counter
from collections.abc import Iterable
from pathlib import Path
from typing import Any

from inference.fuzzy import AMAN, RENDAH, SEDANG, TINGGI, kategori

PROFILE_ORDER = [AMAN, RENDAH, SEDANG, TINGGI, "TIDAK ADA DATA"]
LABELS = {
    "ips": "IPS / IPK",
    "tren_ips": "Tren IPS",
    "kualitas_nilai": "Kualitas Nilai",
    "pengulangan": "Mengulang",
    "alfa_mk": "Max Alfa per MK (%)",
    "akumulasi_menit": "Akumulasi Menit",
    "jam_gabungan": "Jam Ketidakhadiran",
    "status": "Status Registrasi",
    "sisa_masa_studi": "Sisa Masa Studi",
}
SUBSYSTEMS = {
    "akademik": "Akademik",
    "kehadiran": "Kehadiran",
    "administrasi": "Administrasi",
}


def _num(value: float | None, digits: int = 2) -> str:
    return "-" if value is None else f"{value:.{digits}f}"


def _signed(value: float | None, digits: int = 2) -> str:
    return "-" if value is None else f"{value:+.{digits}f}"


def _cut(text: str, width: int) -> str:
    text = str(text)
    return text if len(text) <= width else text[: width - 1] + "~"


def _sub_score(result: dict[str, Any], key: str) -> float | None:
    sub = result[key]
    return None if sub is None else sub["skor"]


def print_summary(results: Iterable[dict[str, Any]]) -> None:
    results = list(results)
    columns = [
        ("No", 3),
        ("NIM", 12),
        ("Nama", 24),
        ("IPS", 5),
        ("dIPS", 6),
        ("Penalti", 7),
        ("Alfa%", 6),
        ("Menit", 6),
        ("Jam", 6),
        ("Sisa", 5),
        ("Z-AKD", 6),
        ("Z-HDR", 6),
        ("Z-ADM", 6),
        ("Skor", 6),
        ("Profil", 16),
    ]
    header = "  ".join(f"{name:<{width}}" for name, width in columns)
    print(header)
    print("-" * len(header))

    for index, result in enumerate(results, start=1):
        fitur = result["fitur"]
        row = [
            str(index),
            fitur["nim"],
            _cut(fitur["nama"], 24),
            _num(fitur["ips"], 2),
            _signed(fitur["delta_ips"], 2),
            _num(fitur["penalti"], 1),
            _num(fitur["max_alfa"], 2),
            _num(fitur["menit"], 0),
            _num(fitur["jam"], 1),
            _num(fitur["sisa"], 0),
            _num(_sub_score(result, "akademik"), 1),
            _num(_sub_score(result, "kehadiran"), 1),
            _num(_sub_score(result, "administrasi"), 1),
            _num(result["skor"], 1),
            result["profil"],
        ]
        print("  ".join(f"{value:<{width}}" for value, (_, width) in zip(row, columns)))

    counts = Counter(result["profil"] for result in results)
    ringkasan = " | ".join(f"{nama}={counts.get(nama, 0)}" for nama in PROFILE_ORDER)
    print("-" * len(header))
    print(f"Total {len(results)} mahasiswa | {ringkasan}")


def _print_subsystem(key: str, sub: dict[str, Any]) -> None:
    print()
    print(f"Sub-sistem {SUBSYSTEMS[key]}")
    print("-" * 78)
    print("  Fuzzifikasi:")
    for var, mu in sub["mu"].items():
        bagian = "  ".join(f"{nama}={nilai:.3f}" for nama, nilai in mu.items())
        print(f"    {LABELS.get(var, var):<26}: {bagian}")
    print("  Aturan aktif:")
    if not sub["aktif"]:
        print("    (tidak ada aturan aktif)")
    for rid, alpha, z, desc in sorted(sub["aktif"], key=lambda item: item[0]):
        print(f"    {rid:<8} alpha={alpha:.3f}  z={z:>4.0f}  {desc}")
    if sub["skor"] is None:
        print("  Z = - (tidak ada aturan aktif)")
    else:
        print(f"  Z = {sub['skor']:.2f} -> {sub['kategori']}")


def print_detail(result: dict[str, Any]) -> None:
    fitur = result["fitur"]
    print("=" * 78)
    print(f"{fitur['nim']} - {fitur['nama']}")
    print("=" * 78)
    print(
        f"Prodi       : {fitur['prodi']} ({fitur['jenjang']})\n"
        f"Angkatan    : {fitur['angkatan']} | Kelas: {fitur['kelas']} | "
        f"Semester berjalan: {fitur['semester_berjalan']}\n"
        f"Masa studi  : {fitur['masa_studi']}/{fitur['masa_studi_maksimal']} semester\n"
        f"Periode     : {fitur['periode'] or '-'}"
    )
    print()
    print("Fitur crisp:")
    print(f"  IPS / IPK             : {_num(fitur['ips'], 2)}")
    print(f"  Tren IPS (delta)      : {_signed(fitur['delta_ips'], 2)}")
    print(
        f"  Penalti nilai D/E     : {_num(fitur['penalti'], 1)} "
        f"(D={fitur['jumlah_d']}, E={fitur['jumlah_e']})"
    )
    print(f"  Mengulang             : {'Ya' if fitur['mengulang'] else 'Tidak'}")
    print(f"  Max alfa per MK       : {_num(fitur['max_alfa'], 2)} %")
    print(f"  Akumulasi menit       : {_num(fitur['menit'], 0)} menit")
    print(f"  Jam ketidakhadiran    : {_num(fitur['jam'], 2)} jam")
    print(
        f"  Status                : {fitur['status_nama']} (kode {fitur['status_kode']})"
    )
    print(f"  Sisa masa studi       : {_num(fitur['sisa'], 0)} semester")
    print(f"  Jumlah MK periode ini : {fitur['jumlah_mk']}")

    for key in SUBSYSTEMS:
        sub = result[key]
        if sub is not None:
            _print_subsystem(key, sub)

    print()
    print("=" * 78)
    if result["skor"] is None:
        print(f"HASIL AKHIR: {result['profil']}")
    else:
        print(f"HASIL AKHIR: skor {result['skor']:.2f} -> {result['profil']}")
        angka = kategori(result["skor"])
        if angka and angka != result["profil"]:
            print(f"Kategori skor numerik: {angka} (kategori akhir memakai worst-case)")
    print("=" * 78)

    if fitur["catatan"]:
        print("Catatan:")
        for catatan in fitur["catatan"]:
            print(f"  - {catatan}")


def _subsystem_dict(sub: dict[str, Any] | None) -> dict[str, Any] | None:
    if sub is None:
        return None
    return {
        "skor": None if sub["skor"] is None else round(sub["skor"], 4),
        "kategori": sub["kategori"],
        "nilai_input": sub["nilai"],
        "aturan_aktif": [
            {
                "id": rid,
                "alpha": round(alpha, 4),
                "z": z,
                "teks": desc,
            }
            for rid, alpha, z, desc in sub["aktif"]
        ],
    }


def result_to_dict(result: dict[str, Any]) -> dict[str, Any]:
    fitur = result["fitur"]
    return {
        "nim": fitur["nim"],
        "nama": fitur["nama"],
        "prodi": fitur["prodi"],
        "jenjang": fitur["jenjang"],
        "angkatan": fitur["angkatan"],
        "kelas": fitur["kelas"],
        "semester_berjalan": fitur["semester_berjalan"],
        "masa_studi": fitur["masa_studi"],
        "masa_studi_maksimal": fitur["masa_studi_maksimal"],
        "periode": fitur["periode"],
        "ips": fitur["ips"],
        "delta_ips": round(fitur["delta_ips"], 4),
        "penalti": fitur["penalti"],
        "jumlah_d": fitur["jumlah_d"],
        "jumlah_e": fitur["jumlah_e"],
        "mengulang": fitur["mengulang"],
        "max_alfa": round(fitur["max_alfa"], 4),
        "menit": round(fitur["menit"], 4),
        "jam": round(fitur["jam"], 4),
        "status_kode": fitur["status_kode"],
        "status_nama": fitur["status_nama"],
        "sisa": fitur["sisa"],
        "jumlah_mk": fitur["jumlah_mk"],
        "subsistem": {key: _subsystem_dict(result[key]) for key in SUBSYSTEMS},
        "skor_akhir": None if result["skor"] is None else round(result["skor"], 4),
        "profil": result["profil"],
        "catatan": fitur["catatan"],
    }


def write_csv(results: Iterable[dict[str, Any]], path: Path) -> None:
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    fields = [
        "nim",
        "nama",
        "prodi",
        "jenjang",
        "angkatan",
        "kelas",
        "semester_berjalan",
        "masa_studi",
        "masa_studi_maksimal",
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
        "skor_akhir",
        "profil",
        "catatan",
    ]
    with path.open("w", newline="", encoding="utf-8-sig") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
        writer.writeheader()
        for result in results:
            data = result_to_dict(result)
            subs = data["subsistem"]
            writer.writerow(
                {
                    **{key: data[key] for key in fields if key in data},
                    "ips": "" if data["ips"] is None else f"{data['ips']:.2f}",
                    "delta_ips": f"{data['delta_ips']:.4f}",
                    "penalti": f"{data['penalti']:.2f}",
                    "mengulang": "Ya" if data["mengulang"] else "Tidak",
                    "max_alfa": f"{data['max_alfa']:.4f}",
                    "menit": f"{data['menit']:.2f}",
                    "jam": f"{data['jam']:.4f}",
                    "sisa": f"{data['sisa']:.2f}",
                    "z_akademik": ""
                    if subs["akademik"] is None
                    else f"{subs['akademik']['skor']:.4f}",
                    "z_kehadiran": ""
                    if subs["kehadiran"] is None
                    else f"{subs['kehadiran']['skor']:.4f}",
                    "z_administrasi": ""
                    if subs["administrasi"] is None
                    else f"{subs['administrasi']['skor']:.4f}",
                    "skor_akhir": ""
                    if data["skor_akhir"] is None
                    else f"{data['skor_akhir']:.4f}",
                    "catatan": " | ".join(data["catatan"]),
                }
            )


def write_json(results: Iterable[dict[str, Any]], path: Path) -> None:
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    payload = [result_to_dict(result) for result in results]
    with path.open("w", encoding="utf-8") as handle:
        json.dump(payload, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
