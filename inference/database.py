from __future__ import annotations

from pathlib import Path
from typing import Any

import pyodbc

DEFAULT_MDF = Path(__file__).resolve().parent.parent / "Database.mdf"
DEFAULT_SERVER = r"(LocalDB)\MSSQLLocalDB"
DRIVERS = (
    "ODBC Driver 18 for SQL Server",
    "ODBC Driver 17 for SQL Server",
    "ODBC Driver 13 for SQL Server",
    "SQL Server",
)


class Database:
    def __init__(
        self, mdf: str | Path = DEFAULT_MDF, server: str = DEFAULT_SERVER
    ) -> None:
        self.mdf = Path(mdf).expanduser().resolve()
        self.server = server
        self.driver = self._pick_driver()

    @staticmethod
    def _pick_driver() -> str:
        available = pyodbc.drivers()
        for name in DRIVERS:
            if name in available:
                return name
        raise RuntimeError(
            "ODBC driver SQL Server tidak ditemukan. "
            "Pasang 'ODBC Driver 17 for SQL Server'."
        )

    def _connect(self, database: str) -> pyodbc.Connection:
        return pyodbc.connect(
            f"DRIVER={{{self.driver}}};"
            f"SERVER={self.server};"
            f"DATABASE={database};"
            "Trusted_Connection=yes;"
            "TrustServerCertificate=yes;",
            autocommit=True,
            timeout=20,
        )

    def attached(self) -> str | None:
        try:
            conn = self._connect("master")
        except pyodbc.Error:
            return None
        try:
            row = conn.execute(
                "SELECT DB_NAME(database_id) FROM sys.master_files "
                "WHERE LOWER(physical_name) = LOWER(?)",
                str(self.mdf),
            ).fetchone()
            return str(row[0]) if row else None
        finally:
            conn.close()

    def connect(self) -> pyodbc.Connection:
        if not self.mdf.exists():
            raise FileNotFoundError(f"Database tidak ditemukan: {self.mdf}")
        name = self.attached()
        if name:
            return self._connect(name)
        return pyodbc.connect(
            f"DRIVER={{{self.driver}}};"
            f"SERVER={self.server};"
            f"AttachDbFilename={self.mdf};"
            f"DATABASE={self.mdf.stem};"
            "Trusted_Connection=yes;"
            "TrustServerCertificate=yes;",
            autocommit=True,
            timeout=20,
        )

    def query(self, sql: str, params: tuple[Any, ...] = ()) -> list[dict[str, Any]]:
        conn = self.connect()
        try:
            cursor = conn.cursor()
            cursor.execute(sql, params)
            if cursor.description is None:
                return []
            columns = [column[0] for column in cursor.description]
            return [dict(zip(columns, row)) for row in cursor.fetchall()]
        finally:
            conn.close()
