from __future__ import annotations

from collections.abc import Callable
from typing import Any, ClassVar

AMAN = "AMAN"
RENDAH = "RENDAH"
SEDANG = "SEDANG"
TINGGI = "TINGGI"
TIDAK_ADA_DATA = "TIDAK ADA DATA"
PROFILE = {AMAN: 0, RENDAH: 1, SEDANG: 2, TINGGI: 3}

MEETINGS = 16
MINUTES_PER_SKS = 50
STATUS = {
    0: "Aktif Normal",
    1: "Cuti Non-Akademik",
    2: "Cuti Akademik",
    3: "Mangkir 1x",
    4: "Mangkir Beruntun",
}

RANGE = {
    "ips": (0.0, 4.0),
    "tren_ips": (-4.0, 4.0),
    "kualitas_nilai": (0.0, 10.0),
    "pengulangan": (0.0, 1.0),
    "alfa_mk": (0.0, 100.0),
    "akumulasi_menit": (0.0, 2500.0),
    "jam_gabungan": (0.0, 200.0),
    "status": (0.0, 4.0),
    "sisa_masa_studi": (0.0, 14.0),
}

MembershipFn = Callable[..., float]
FuzzySets = dict[str, dict[str, tuple[MembershipFn, tuple[float, ...]]]]
Rule = tuple[int, Callable[[dict[str, dict[str, float]], dict[str, float]], float]]


def singleton(x: float, value: float) -> float:
    return 1.0 if abs(x - value) < 1e-9 else 0.0


def fuzzify_value(name: str, value: float, sets: FuzzySets) -> dict[str, float]:
    low, high = RANGE[name]
    x = max(low, min(high, float(value)))
    return {label: function(x, *args) for label, (function, args) in sets[name].items()}


def fuzzify_all(
    values: dict[str, float], sets: FuzzySets
) -> dict[str, dict[str, float]]:
    return {name: fuzzify_value(name, value, sets) for name, value in values.items()}


def clamp(value: float) -> float:
    return max(0.0, min(1.0, value))


def sugeno(active: list[tuple[float, int]]) -> float | None:
    total = sum(alpha for alpha, _ in active)
    if total == 0:
        return None
    return sum(alpha * consequence for alpha, consequence in active) / total


def category(score: float | None) -> str | None:
    if score is None:
        return None
    if score <= 22.5:
        return AMAN
    if score <= 50:
        return RENDAH
    if score <= 80:
        return SEDANG
    return TINGGI


def worst(categories: list[str]) -> str:
    if not categories:
        return TIDAK_ADA_DATA
    return max(categories, key=lambda value: PROFILE.get(value, -1))


class Subsystem:
    sets: ClassVar[FuzzySets] = {}

    def __init__(self, fitur: dict[str, Any]) -> None:
        self.fitur = fitur

    def values(self) -> dict[str, float] | None:
        raise NotImplementedError

    def rules(self) -> list[Rule]:
        raise NotImplementedError

    def evaluate(self) -> dict[str, Any] | None:
        values = self.values()
        if values is None:
            return None

        memberships = fuzzify_all(values, self.sets)
        active = []
        for consequence, condition in self.rules():
            alpha = clamp(condition(memberships, values))
            if alpha > 0:
                active.append((alpha, consequence))

        score = sugeno(active)
        return {"skor": score, "kategori": category(score)}
