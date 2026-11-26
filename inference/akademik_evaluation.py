from __future__ import annotations

from typing import ClassVar

from inference.membership import Membership
from inference.subsystem import FuzzySets, Rule, Subsystem, singleton

SETS: FuzzySets = {
    "ips": {
        "Kurang": (Membership.linear_down, (1.8, 2.0)),
        "Cukup": (Membership.trapezoid, (1.8, 2.0, 2.8, 3.0)),
        "Baik": (Membership.linear_up, (2.8, 3.0)),
    },
    "tren_ips": {
        "Turun Drastis": (Membership.linear_down, (-0.55, -0.5)),
        "Turun Ringan": (Membership.trapezoid, (-0.55, -0.45, -0.3, -0.25)),
        "Naik / Stabil": (Membership.linear_up, (-0.3, -0.25)),
    },
    "kualitas_nilai": {
        "Bersih": (Membership.linear_down, (0.5, 1.0)),
        "Toleransi": (Membership.trapezoid, (0.5, 1.0, 2.0, 2.5)),
        "Tidak Lulus": (Membership.linear_up, (2.0, 3.0)),
    },
    "pengulangan": {
        "Normal": (singleton, (0.0,)),
        "Mengulang": (singleton, (1.0,)),
    },
}


class Akademik(Subsystem):
    sets: ClassVar[FuzzySets] = SETS

    def values(self) -> dict[str, float] | None:
        if self.fitur["ips"] is None:
            return None
        return {
            "ips": self.fitur["ips"],
            "tren_ips": self.fitur["delta_ips"],
            "kualitas_nilai": self.fitur["penalti"],
            "pengulangan": 1.0 if self.fitur["mengulang"] else 0.0,
        }

    def rules(self) -> list[Rule]:
        return [
            (10, self.aman),
            (35, self.rendah_nilai_bersih),
            (35, self.rendah_nilai_toleransi),
            (65, self.sedang_tren_drastis),
            (65, self.sedang_penalti_dua),
            (95, self.tinggi_ips_kurang),
            (95, self.tinggi_tidak_lulus),
            (95, self.tinggi_mengulang),
        ]

    def aman(self, m, values) -> float:
        return min(
            m["ips"]["Baik"],
            m["kualitas_nilai"]["Bersih"],
            m["pengulangan"]["Normal"],
            m["tren_ips"]["Naik / Stabil"],
        )

    def rendah_nilai_bersih(self, m, values) -> float:
        ips_atau_tren = max(m["ips"]["Cukup"], m["tren_ips"]["Turun Ringan"])
        return min(
            ips_atau_tren,
            m["kualitas_nilai"]["Bersih"],
            m["pengulangan"]["Normal"],
        )

    def rendah_nilai_toleransi(self, m, values) -> float:
        ips_atau_tren = max(m["ips"]["Cukup"], m["tren_ips"]["Turun Ringan"])
        return min(
            ips_atau_tren,
            m["kualitas_nilai"]["Toleransi"],
            m["pengulangan"]["Normal"],
        )

    def sedang_tren_drastis(self, m, values) -> float:
        return min(m["ips"]["Cukup"], m["tren_ips"]["Turun Drastis"])

    def sedang_penalti_dua(self, m, values) -> float:
        if values["kualitas_nilai"] < 2:
            return 0.0
        return m["kualitas_nilai"]["Toleransi"]

    def tinggi_ips_kurang(self, m, values) -> float:
        return m["ips"]["Kurang"]

    def tinggi_tidak_lulus(self, m, values) -> float:
        return m["kualitas_nilai"]["Tidak Lulus"]

    def tinggi_mengulang(self, m, values) -> float:
        return m["pengulangan"]["Mengulang"]
