from __future__ import annotations

from typing import ClassVar

from inference.membership import Membership
from inference.subsystem import FuzzySets, Rule, Subsystem, singleton

SETS: FuzzySets = {
    "status": {
        "Aktif Normal": (singleton, (0.0,)),
        "Cuti Non-Akademik": (singleton, (1.0,)),
        "Cuti Akademik": (singleton, (2.0,)),
        "Mangkir 1x": (singleton, (3.0,)),
        "Mangkir Beruntun": (singleton, (4.0,)),
    },
    "sisa_masa_studi": {
        "Habis": (Membership.linear_down, (0.5, 1.0)),
        "Kritis": (Membership.triangle, (0.5, 1.0, 2.0)),
        "Menipis": (Membership.trapezoid, (1.5, 2.0, 3.0, 4.0)),
        "Longgar": (Membership.linear_up, (3.0, 4.0)),
    },
}


class Administratif(Subsystem):
    sets: ClassVar[FuzzySets] = SETS

    def values(self) -> dict[str, float] | None:
        if not self.fitur["periode"]:
            return None
        return {
            "status": self.fitur["status_kode"],
            "sisa_masa_studi": self.fitur["sisa"],
        }

    def rules(self) -> list[Rule]:
        return [
            (10, self.aman),
            (35, self.rendah_cuti),
            (65, self.sedang_cuti_akademik),
            (65, self.sedang_sisa_menipis),
            (95, self.tinggi_mangkir),
            (95, self.tinggi_mangkir_beruntun),
            (95, self.tinggi_sisa_kritis),
            (95, self.tinggi_sisa_habis),
        ]

    def aman(self, m, values) -> float:
        return min(
            m["status"]["Aktif Normal"],
            m["sisa_masa_studi"]["Longgar"],
        )

    def rendah_cuti(self, m, values) -> float:
        return min(
            m["status"]["Cuti Non-Akademik"],
            m["sisa_masa_studi"]["Longgar"],
        )

    def sedang_cuti_akademik(self, m, values) -> float:
        return m["status"]["Cuti Akademik"]

    def sedang_sisa_menipis(self, m, values) -> float:
        return m["sisa_masa_studi"]["Menipis"]

    def tinggi_mangkir(self, m, values) -> float:
        return m["status"]["Mangkir 1x"]

    def tinggi_mangkir_beruntun(self, m, values) -> float:
        return m["status"]["Mangkir Beruntun"]

    def tinggi_sisa_kritis(self, m, values) -> float:
        return m["sisa_masa_studi"]["Kritis"]

    def tinggi_sisa_habis(self, m, values) -> float:
        return m["sisa_masa_studi"]["Habis"]
