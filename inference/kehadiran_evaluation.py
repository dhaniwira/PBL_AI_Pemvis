from __future__ import annotations

from typing import ClassVar

from inference.membership import Membership
from inference.subsystem import FuzzySets, Rule, Subsystem

SETS: FuzzySets = {
    "alfa_mk": {
        "Sedikit": (Membership.linear_down, (8.0, 10.0)),
        "Sedang": (Membership.trapezoid, (8.0, 10.0, 18.0, 20.0)),
        "Banyak": (Membership.linear_up, (18.0, 20.0)),
    },
    "akumulasi_menit": {
        "Sedikit": (Membership.linear_down, (325.0, 375.0)),
        "Sedang": (Membership.trapezoid, (325.0, 375.0, 700.0, 750.0)),
        "Banyak (SP I)": (Membership.trapezoid, (700.0, 750.0, 1450.0, 1500.0)),
        "Kritis (SP II/III/DO)": (Membership.linear_up, (1450.0, 1500.0)),
    },
    "jam_gabungan": {
        "Sedikit": (Membership.linear_down, (33.0, 38.0)),
        "Sedang": (Membership.trapezoid, (33.0, 38.0, 70.0, 76.0)),
        "Banyak": (Membership.trapezoid, (70.0, 76.0, 108.0, 114.0)),
        "Melebihi Batas": (Membership.linear_up, (108.0, 114.0)),
    },
}


class Kehadiran(Subsystem):
    sets: ClassVar[FuzzySets] = SETS

    def values(self) -> dict[str, float] | None:
        if not self.fitur["jumlah_mk"]:
            return None
        return {
            "alfa_mk": self.fitur["max_alfa"],
            "akumulasi_menit": self.fitur["menit"],
            "jam_gabungan": self.fitur["jam"],
        }

    def rules(self) -> list[Rule]:
        return [
            (10, self.aman),
            (35, self.rendah_alfa_sedang),
            (35, self.rendah_menit_sedang),
            (35, self.rendah_jam_sedang),
            (65, self.sedang_menit_banyak),
            (65, self.sedang_jam_banyak),
            (95, self.tinggi_alfa_banyak),
            (95, self.tinggi_menit_kritis),
            (95, self.tinggi_jam_melebihi),
        ]

    def menit_tinggi(self, m) -> float:
        return max(
            m["akumulasi_menit"]["Banyak (SP I)"],
            m["akumulasi_menit"]["Kritis (SP II/III/DO)"],
        )

    def jam_tinggi(self, m) -> float:
        return max(
            m["jam_gabungan"]["Banyak"],
            m["jam_gabungan"]["Melebihi Batas"],
        )

    def aman(self, m, values) -> float:
        return min(
            m["alfa_mk"]["Sedikit"],
            m["akumulasi_menit"]["Sedikit"],
            m["jam_gabungan"]["Sedikit"],
        )

    def rendah_alfa_sedang(self, m, values) -> float:
        return min(
            m["alfa_mk"]["Sedang"],
            1.0 - self.menit_tinggi(m),
            1.0 - self.jam_tinggi(m),
        )

    def rendah_menit_sedang(self, m, values) -> float:
        return min(
            m["akumulasi_menit"]["Sedang"],
            1.0 - m["alfa_mk"]["Banyak"],
            1.0 - self.jam_tinggi(m),
        )

    def rendah_jam_sedang(self, m, values) -> float:
        return min(
            m["jam_gabungan"]["Sedang"],
            1.0 - m["alfa_mk"]["Banyak"],
            1.0 - self.menit_tinggi(m),
        )

    def sedang_menit_banyak(self, m, values) -> float:
        return min(
            m["akumulasi_menit"]["Banyak (SP I)"],
            1.0 - m["alfa_mk"]["Banyak"],
        )

    def sedang_jam_banyak(self, m, values) -> float:
        return min(
            m["jam_gabungan"]["Banyak"],
            1.0 - m["alfa_mk"]["Banyak"],
        )

    def tinggi_alfa_banyak(self, m, values) -> float:
        return m["alfa_mk"]["Banyak"]

    def tinggi_menit_kritis(self, m, values) -> float:
        return m["akumulasi_menit"]["Kritis (SP II/III/DO)"]

    def tinggi_jam_melebihi(self, m, values) -> float:
        return m["jam_gabungan"]["Melebihi Batas"]
