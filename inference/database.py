from __future__ import annotations

import json
from collections.abc import Iterable
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


def driver() -> str:
    tersedia = pyodbc.drivers()
    for nama in DRIVERS:
        if nama in tersedia:
            return nama
    raise RuntimeError(
        "ODBC driver SQL Server tidak ditemukan. "
        "Pasang 'ODBC Driver 17 for SQL Server'."
    )


class Database:
    def __init__(
        self, mdf: str | Path = DEFAULT_MDF, server: str = DEFAULT_SERVER
    ) -> None:
        self.mdf = Path(mdf).expanduser().resolve()
        self.server = server
        self.driver = driver()

    def _connect(self, database: str) -> pyodbc.Connection:
        conn_str = (
            f"DRIVER={{{self.driver}}};"
            f"SERVER={self.server};"
            f"DATABASE={database};"
            "Trusted_Connection=yes;"
            "TrustServerCertificate=yes;"
        )
        return pyodbc.connect(conn_str, autocommit=True, timeout=10)

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
            if row:
                return str(row[0])
            row = conn.execute(
                "SELECT physical_name FROM sys.master_files "
                "WHERE database_id = DB_ID(?)",
                self.mdf.stem,
            ).fetchone()
            if row:
                raise RuntimeError(
                    f"Nama database '{self.mdf.stem}' sudah dipakai file lain: "
                    f"{row[0]}\n"
                    "Tutup aplikasi yang memakai database itu, lalu jalankan:\n"
                    "  python setup_database.py --force --with-demo"
                )
            return None
        finally:
            conn.close()

    def connect(self) -> pyodbc.Connection:
        if not self.mdf.exists():
            raise FileNotFoundError(
                f"Database tidak ditemukan: {self.mdf}\n"
                "Jalankan: python setup_database.py --with-demo"
            )
        nama = self.attached()
        if nama:
            return self._connect(nama)
        conn_str = (
            f"DRIVER={{{self.driver}}};"
            f"SERVER={self.server};"
            f"AttachDbFilename={self.mdf};"
            f"DATABASE={self.mdf.stem};"
            "Trusted_Connection=yes;"
            "TrustServerCertificate=yes;"
        )
        return pyodbc.connect(conn_str, autocommit=True, timeout=20)

    def query(self, sql: str, params: tuple[Any, ...] = ()) -> list[dict[str, Any]]:
        conn = self.connect()
        try:
            cursor = conn.cursor()
            cursor.execute(sql, params)
            if cursor.description is None:
                return []
            kolom = [col[0] for col in cursor.description]
            return [dict(zip(kolom, row)) for row in cursor.fetchall()]
        finally:
            conn.close()


def load_data(
    db: Database,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    students = db.query(
        """
        SELECT m.nim, m.nama, m.id_prodi, p.nama AS prodi, j.kode AS jenjang,
               j.masa_studi_maksimal, m.angkatan, m.kelas,
               m.semester_berjalan, m.masa_studi
        FROM mahasiswa m
        INNER JOIN prodi p ON p.id = m.id_prodi
        INNER JOIN jenjang j ON j.id = p.id_jenjang
        ORDER BY m.nim
        """
    )
    riwayat = db.query(
        """
        SELECT r.id, r.nim, t.tahun_mulai, t.tahun_selesai,
               t.semester AS ta_semester, r.semester, r.mengulang,
               r.status, r.ips, r.ipk
        FROM riwayat_studi r
        INNER JOIN tahun_akademik t ON t.id = r.id_tahun_akademik
        ORDER BY r.nim, t.tahun_mulai, t.tahun_selesai,
                 CASE t.semester WHEN 'GANJIL' THEN 1 ELSE 2 END,
                 r.semester, r.id
        """
    )
    details = db.query(
        """
        SELECT d.id_riwayat_studi, d.nilai_akhir, d.alfa, d.izin,
               d.sakit, d.telat, mk.kategori, mk.sks, mk.kode, mk.nama
        FROM detail_mk d
        INNER JOIN mk_prodi mp ON mp.id = d.id_mk_prodi
        INNER JOIN mata_kuliah mk ON mk.id = mp.id_mk
        ORDER BY d.id_riwayat_studi, d.id
        """
    )
    return students, riwayat, details


def _round(value: float | None, digits: int = 2) -> float | None:
    return None if value is None else round(float(value), digits)


def _score(subsystem: dict[str, Any] | None) -> float | None:
    return None if subsystem is None else subsystem["skor"]


def _subsystem(subsystem: dict[str, Any] | None) -> dict[str, Any] | None:
    if subsystem is None:
        return None
    return {
        "skor": _round(subsystem["skor"]),
        "kategori": subsystem["kategori"],
        "aturan_aktif": [
            {"id": rid, "alpha": round(alpha, 4), "z": z}
            for rid, alpha, z, _ in subsystem["aktif"]
        ],
    }


def payload(result: dict[str, Any]) -> dict[str, Any]:
    fitur = result["fitur"]
    return {
        "nim": fitur["nim"],
        "nama": fitur["nama"],
        "prodi": fitur["prodi"],
        "jenjang": fitur["jenjang"],
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
        "status": fitur["status_nama"],
        "sisa": fitur["sisa"],
        "subsistem": {
            key: _subsystem(result[key])
            for key in ("akademik", "kehadiran", "administrasi")
        },
        "skor_akhir": _round(result["skor"]),
        "profil": result["profil"],
        "catatan": fitur["catatan"],
    }


def save_results(db: Database, results: Iterable[dict[str, Any]]) -> int:
    conn = db.connect()
    jumlah = 0
    try:
        cursor = conn.cursor()
        cursor.execute(
            """
            IF OBJECT_ID(N'hasil_fuzzy', N'U') IS NULL
            BEGIN
                CREATE TABLE hasil_fuzzy (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    nim VARCHAR(20) NOT NULL,
                    nama VARCHAR(150) NOT NULL,
                    skor_akademik DECIMAL(6,2) NULL,
                    skor_kehadiran DECIMAL(6,2) NULL,
                    skor_administrasi DECIMAL(6,2) NULL,
                    skor_akhir DECIMAL(6,2) NULL,
                    profil VARCHAR(20) NOT NULL,
                    detail NVARCHAR(MAX) NULL,
                    dihitung_pada DATETIME2 NOT NULL DEFAULT SYSDATETIME()
                )
            END
            """
        )
        for result in results:
            fitur = result["fitur"]
            cursor.execute("DELETE FROM hasil_fuzzy WHERE nim = ?", fitur["nim"])
            cursor.execute(
                """
                INSERT INTO hasil_fuzzy
                    (nim, nama, skor_akademik, skor_kehadiran,
                     skor_administrasi, skor_akhir, profil, detail)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    fitur["nim"],
                    fitur["nama"],
                    _round(_score(result["akademik"])),
                    _round(_score(result["kehadiran"])),
                    _round(_score(result["administrasi"])),
                    _round(result["skor"]),
                    result["profil"],
                    json.dumps(payload(result), ensure_ascii=False),
                ),
            )
            jumlah += 1
    finally:
        conn.close()
    return jumlah
