from __future__ import annotations

from typing import Any

from inference.administratif_evaluation import Administratif
from inference.akademik_evaluation import Akademik
from inference.kehadiran_evaluation import Kehadiran
from inference.subsystem import TIDAK_ADA_DATA, worst


class FinalResult:
    def __init__(self, fitur: dict[str, Any]) -> None:
        self.fitur = fitur
        self.akademik = Akademik(fitur).evaluate()
        self.kehadiran = Kehadiran(fitur).evaluate()
        self.administratif = Administratif(fitur).evaluate()

    def subsystems(self) -> list[dict[str, Any]]:
        return [
            result
            for result in (self.akademik, self.kehadiran, self.administratif)
            if result is not None and result["skor"] is not None
        ]

    @property
    def score(self) -> float | None:
        return max((result["skor"] for result in self.subsystems()), default=None)

    @property
    def profile(self) -> str:
        available = self.subsystems()
        if not available:
            return TIDAK_ADA_DATA
        return worst([result["kategori"] for result in available])

    def to_dict(self) -> dict[str, Any]:
        return {
            "fitur": self.fitur,
            "akademik": self.akademik,
            "kehadiran": self.kehadiran,
            "administrasi": self.administratif,
            "skor": self.score,
            "profil": self.profile,
        }
