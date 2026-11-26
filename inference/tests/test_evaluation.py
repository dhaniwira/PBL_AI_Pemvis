from __future__ import annotations

import sys
import unittest
from pathlib import Path

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[2]))

from inference.administratif_evaluation import Administratif
from inference.akademik_evaluation import Akademik
from inference.final_result_evaluation import FinalResult
from inference.kehadiran_evaluation import Kehadiran
from inference.membership import Membership
from inference.process_evaluation import (
    AMAN,
    RENDAH,
    SEDANG,
    TIDAK_ADA_DATA,
    TINGGI,
    build_features,
    grade,
    infer,
    penalty_features,
)
from inference.subsystem import fuzzify_value


def _student(**overrides):
    row = {
        "nim": "2507419999",
        "nama": "Mahasiswa Uji",
        "prodi": "Teknik Informatika",
        "jenjang": "D4",
        "masa_studi_maksimal": 14,
        "angkatan": 2025,
        "kelas": "2A",
        "semester_berjalan": 2,
        "masa_studi": 2,
    }
    row.update(overrides)
    return row


def _riwayat(**overrides):
    row = {
        "id": 1,
        "nim": "2507419999",
        "tahun_mulai": 2025,
        "tahun_selesai": 2026,
        "ta_semester": "GENAP",
        "semester": 2,
        "mengulang": False,
        "status": "aktif",
        "ips": 3.50,
        "ipk": 3.50,
    }
    row.update(overrides)
    return row


def _detail(**overrides):
    row = {
        "id_riwayat_studi": 1,
        "nilai_akhir": 85,
        "alfa": 0,
        "izin": 0,
        "sakit": 0,
        "telat": 0,
        "kategori": "UMUM",
        "sks": 2,
    }
    row.update(overrides)
    return row


def _fitur(**overrides):
    row = {
        "nim": "2507419999",
        "nama": "Mahasiswa Uji",
        "prodi": "Teknik Informatika",
        "jenjang": "D4",
        "angkatan": 2025,
        "kelas": "2A",
        "semester_berjalan": 2,
        "masa_studi": 2,
        "masa_studi_maksimal": 14,
        "periode": "2025/2026 GENAP",
        "ips": 3.50,
        "delta_ips": 0.10,
        "penalti": 0.0,
        "jumlah_d": 0,
        "jumlah_e": 0,
        "mengulang": False,
        "max_alfa": 0.0,
        "menit": 0.0,
        "jam": 0.0,
        "status_kode": 0,
        "status_nama": "Aktif Normal",
        "sisa": 12.0,
        "jumlah_mk": 6,
        "catatan": [],
    }
    row.update(overrides)
    return row


class TestMembership(unittest.TestCase):
    def test_linear_up(self):
        self.assertEqual(Membership.linear_up(2.5, 2.8, 3.0), 0.0)
        self.assertEqual(Membership.linear_up(3.5, 2.8, 3.0), 1.0)
        self.assertAlmostEqual(Membership.linear_up(2.9, 2.8, 3.0), 0.5)

    def test_linear_down(self):
        self.assertEqual(Membership.linear_down(1.5, 1.8, 2.0), 1.0)
        self.assertEqual(Membership.linear_down(2.5, 1.8, 2.0), 0.0)
        self.assertAlmostEqual(Membership.linear_down(1.9, 1.8, 2.0), 0.5)

    def test_triangle(self):
        self.assertEqual(Membership.triangle(1.0, 0.5, 1.0, 2.0), 1.0)
        self.assertAlmostEqual(Membership.triangle(1.5, 0.5, 1.0, 2.0), 0.5)
        self.assertEqual(Membership.triangle(2.0, 0.5, 1.0, 2.0), 0.0)

    def test_trapezoid(self):
        self.assertEqual(Membership.trapezoid(1.7, 1.8, 2.0, 2.8, 3.0), 0.0)
        self.assertAlmostEqual(Membership.trapezoid(1.9, 1.8, 2.0, 2.8, 3.0), 0.5)
        self.assertEqual(Membership.trapezoid(2.4, 1.8, 2.0, 2.8, 3.0), 1.0)
        self.assertAlmostEqual(Membership.trapezoid(2.9, 1.8, 2.0, 2.8, 3.0), 0.5)

    def test_fuzzify(self):
        self.assertEqual(
            fuzzify_value("status", 2.0, Administratif.sets)["Cuti Akademik"], 1.0
        )
        self.assertEqual(
            fuzzify_value("pengulangan", 1.0, Akademik.sets)["Mengulang"], 1.0
        )


class TestInference(unittest.TestCase):
    def test_classes(self):
        fitur = _fitur()
        self.assertEqual(Akademik(fitur).evaluate()["skor"], 10.0)
        self.assertEqual(Kehadiran(fitur).evaluate()["skor"], 10.0)
        self.assertEqual(Administratif(fitur).evaluate()["skor"], 10.0)
        self.assertEqual(FinalResult(fitur).to_dict()["profil"], AMAN)

    def test_profil_aman(self):
        result = infer(_fitur())
        self.assertEqual(result["akademik"]["skor"], 10.0)
        self.assertEqual(result["kehadiran"]["skor"], 10.0)
        self.assertEqual(result["administrasi"]["skor"], 10.0)
        self.assertEqual(result["skor"], 10.0)
        self.assertEqual(result["profil"], AMAN)

    def test_akademik_tinggi(self):
        result = infer(_fitur(ips=1.50, delta_ips=-1.30, penalti=6.0, mengulang=True))
        self.assertEqual(result["akademik"]["skor"], 95.0)
        self.assertEqual(result["profil"], TINGGI)

    def test_kehadiran_tinggi(self):
        result = infer(_fitur(max_alfa=50.0, menit=2000.0, jam=38.33))
        self.assertEqual(result["kehadiran"]["skor"], 95.0)
        self.assertEqual(result["profil"], TINGGI)

    def test_administrasi_tinggi(self):
        result = infer(_fitur(status_kode=4, sisa=1.0))
        self.assertEqual(result["administrasi"]["skor"], 95.0)
        self.assertEqual(result["profil"], TINGGI)

    def test_penalti_dua_mengaktifkan_aturan_khusus(self):
        satu = infer(_fitur(ips=2.50, delta_ips=-0.30, penalti=1.0))
        self.assertEqual(satu["akademik"]["skor"], 35.0)
        dua = infer(_fitur(ips=2.50, delta_ips=-0.30, penalti=2.0))
        self.assertEqual(dua["akademik"]["skor"], 50.0)

    def test_worst_case(self):
        result = infer(
            _fitur(
                ips=2.50,
                delta_ips=-0.30,
                penalti=0.0,
                max_alfa=12.5,
                menit=400.0,
                jam=6.67,
                status_kode=0,
                sisa=2.0,
            )
        )
        self.assertEqual(result["akademik"]["kategori"], RENDAH)
        self.assertEqual(result["kehadiran"]["kategori"], RENDAH)
        self.assertEqual(result["administrasi"]["kategori"], SEDANG)
        self.assertEqual(result["profil"], SEDANG)
        self.assertEqual(result["skor"], 65.0)

    def test_tanpa_riwayat(self):
        result = infer(_fitur(periode=None, ips=None, jumlah_mk=0))
        self.assertIsNone(result["skor"])
        self.assertEqual(result["profil"], TIDAK_ADA_DATA)


class TestFeatures(unittest.TestCase):
    def test_grade(self):
        self.assertEqual(grade(85), "A")
        self.assertEqual(grade(75), "B")
        self.assertEqual(grade(65), "C")
        self.assertEqual(grade(55), "D")
        self.assertEqual(grade(45), "E")

    def test_formula_penalti_dan_e(self):
        self.assertEqual(
            penalty_features([_detail(nilai_akhir=55, kategori="UMUM")])["penalti"],
            1.0,
        )
        self.assertEqual(
            penalty_features([_detail(nilai_akhir=55, kategori="MPK")])["penalti"],
            3.0,
        )
        self.assertEqual(
            penalty_features([_detail(nilai_akhir=55, kategori="MKKU")])["penalti"],
            3.0,
        )
        self.assertEqual(
            penalty_features([_detail(nilai_akhir=40, kategori="UMUM")])["penalti"],
            3.0,
        )

    def test_penalti_dibatasi_sepuluh(self):
        subjects = [
            _detail(nilai_akhir=55, kategori="UMUM"),
            _detail(nilai_akhir=55, kategori="MPK"),
            _detail(nilai_akhir=55, kategori="MKKU"),
            _detail(nilai_akhir=40, kategori="UMUM"),
        ]
        fitur = penalty_features(subjects)
        self.assertEqual(fitur["penalti"], 10.0)
        self.assertEqual(fitur["jumlah_d"], 3)
        self.assertEqual(fitur["jumlah_e"], 1)

    def test_penalti_presensi_dan_status(self):
        student = _student(masa_studi=12)
        riwayat = [
            _riwayat(id=1, ta_semester="GANJIL", semester=1, ips=2.50, ipk=2.50),
            _riwayat(
                id=2,
                semester=2,
                mengulang=True,
                status="mangkir",
                ips=2.80,
                ipk=2.60,
            ),
        ]
        details = [
            _detail(id_riwayat_studi=2, nilai_akhir=55, alfa=4, izin=1, telat=2),
            _detail(
                id_riwayat_studi=2,
                nilai_akhir=58,
                alfa=2,
                kategori="MKKU",
                sks=3,
            ),
            _detail(
                id_riwayat_studi=2,
                nilai_akhir=40,
                alfa=1,
                sakit=1,
                kategori="MPK",
            ),
            _detail(id_riwayat_studi=2, nilai_akhir=85, alfa=0),
        ]

        fitur = build_features([student], riwayat, details)[0]

        self.assertAlmostEqual(fitur["ips"], 2.80)
        self.assertAlmostEqual(fitur["delta_ips"], 0.30)
        self.assertEqual(fitur["jumlah_d"], 2)
        self.assertEqual(fitur["jumlah_e"], 1)
        self.assertEqual(fitur["penalti"], 7.0)
        self.assertTrue(fitur["mengulang"])
        self.assertEqual(fitur["status_kode"], 3)
        self.assertEqual(fitur["status_nama"], "Mangkir 1x")
        self.assertAlmostEqual(fitur["sisa"], 2.0)
        self.assertAlmostEqual(fitur["max_alfa"], 25.0)
        self.assertEqual(
            fitur["menit"],
            (4 + 2) * 2 * 50 + (2 + 0) * 3 * 50 + (1 + 0) * 2 * 50,
        )

    def test_mangkir_beruntun(self):
        student = _student()
        riwayat = [
            _riwayat(id=1, ta_semester="GANJIL", semester=1, status="mangkir"),
            _riwayat(id=2, semester=2, status="mangkir"),
        ]
        fitur = build_features([student], riwayat, [])[0]
        self.assertEqual(fitur["status_kode"], 4)
        self.assertEqual(fitur["status_nama"], "Mangkir Beruntun")

    def test_tanpa_riwayat(self):
        fitur = build_features([_student()], [], [])[0]
        self.assertIsNone(fitur["periode"])
        self.assertEqual(fitur["jumlah_mk"], 0)
        self.assertEqual(fitur["status_nama"], "Tidak Diketahui")


if __name__ == "__main__":
    unittest.main()
