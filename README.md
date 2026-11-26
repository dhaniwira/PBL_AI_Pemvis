# PVAI - Sistem Informasi Akademik & Inferensi Fuzzy Profil Risiko

Repositori ini berisi dua bagian:

1. **Aplikasi desktop VB.NET (WinForms)** untuk mengelola data akademik.
2. **Sistem inferensi Fuzzy Sugeno (Python)** yang membaca `Database.mdf`,
   menghitung profil risiko akademik, dan mengekspor laporan CSV.

Rancangan fuzzy ada di
[`inference/desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md`](inference/desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md).
Panduan modul inferensi ada di [`inference/README.md`](inference/README.md).
Dokumentasi alur keputusan ada di
[`inference/DOKUMENTASI_EVALUASI.md`](inference/DOKUMENTASI_EVALUASI.md).

---

## Fitur

### Aplikasi akademik (VB.NET)

- Dashboard ringkasan data.
- CRUD jurusan, jenjang, program studi, tahun akademik.
- CRUD mahasiswa dan riwayat studi per semester.
- Input nilai akhir, alfa, izin, sakit, dan telat per mata kuliah.
- Pemetaan mata kuliah ke program studi.
- Koneksi database terpusat melalui `ConnectionHandler.vb`.
- Tombol **Ekspor Laporan** di sidebar untuk menyimpan laporan CSV.

### Inferensi fuzzy (Python)

- Fuzzifikasi 9 variabel input.
- Tiga sub-sistem: akademik, kehadiran, administrasi.
- 25 aturan fuzzy sesuai dokumen desain.
- Defuzzifikasi Sugeno rata-rata terbobot.
- Agregasi akhir *worst-case priority* (`max`).
- Ringkasan konsol dan ekspor CSV (default `./report.csv`).

---

## Teknologi

| Bagian | Teknologi |
| :--- | :--- |
| Aplikasi desktop | VB.NET WinForms, .NET Framework 4.7.2 |
| Akses database aplikasi | `Microsoft.Data.SqlClient` 7.1.2 |
| Database | Microsoft SQL Server LocalDB, file `Database.mdf` |
| Inferensi fuzzy | Python 3.10+, `pyodbc` |
| Unit test | `unittest` |

---

## Struktur folder

```text
PBL_AI_Pemvis/
├─ PBL_AI_Pemvis.sln
├─ PBL_AI_Pemvis.vbproj
├─ Form1.vb                     # shell aplikasi + sidebar
├─ ConnectionHandler.vb         # koneksi database terpusat
├─ App.config
├─ db.sql                       # skema + data master
├─ Database.mdf                 # dibuat lokal, diabaikan git
├─ Database_log.ldf
├─ Entity/                      # model tabel
├─ Pages/                       # halaman CRUD
├─ UI/                          # tema dan komponen UI
└─ inference/
   ├─ main.py                   # main process
   ├─ database.py               # Database: koneksi + query
   ├─ membership.py             # Membership: fungsi keanggotaan
   ├─ subsystem.py              # base class + himpunan fuzzy
   ├─ akademik_evaluation.py               # class Akademik
   ├─ kehadiran_evaluation.py              # class Kehadiran
   ├─ administratif_evaluation.py          # class Administratif
   ├─ final_result_evaluation.py           # class FinalResult
   ├─ process_evaluation.py             # query + fitur + pipeline
   ├─ export.py                 # ekspor CSV
   ├─ seed_demo.sql             # data demo
   ├─ requirements.txt
   ├─ README.md
   ├─ DOKUMENTASI_EVALUASI.md
   └─ tests/test_evaluation.py
```

> `Database.mdf` dan `Database_log.ldf` masuk `.gitignore`.

---

## Prasyarat

- Windows
- Visual Studio 2022 (workload **.NET desktop development**)
- SQL Server LocalDB
- Python 3.10+
- ODBC Driver 17/18 for SQL Server

Cek LocalDB:

```bash
sqllocaldb info
```

---

## Menyiapkan database

Database `Database` memakai file `Database.mdf` di root proyek.

Dari folder `inference`:

```bash
python -m pip install -r requirements.txt
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE [Database] ON PRIMARY (NAME=N'Database', FILENAME=N'<root>\Database.mdf') LOG ON (NAME=N'Database_log', FILENAME=N'<root>\Database_log.ldf')"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i ..\db.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i seed_demo.sql
```

Ganti `<root>` dengan path absolut root proyek. Langkah terakhir opsional
untuk memuat 7 mahasiswa demo.

Jika `Database.mdf` sudah ada, cukup jalankan aplikasi atau inferensi.

---

## Menjalankan aplikasi VB.NET

1. Buka `PBL_AI_Pemvis.sln` di Visual Studio 2022.
2. Restore NuGet package.
3. Pastikan `Database.mdf` ada di root proyek.
4. Tekan **F5**.

Aplikasi dan inferensi Python memakai file database yang sama. Connection
string di `ConnectionHandler.vb` naik dua folder dari `bin\Debug`:

```vb
Data Source=(LocalDB)\MSSQLLocalDB;
AttachDbFilename=<root>\Database.mdf;
Database=Database;
Integrated Security=True;
TrustServerCertificate=True
```

### Ekspor laporan

Tombol **Ekspor Laporan** ada di sidebar, di bawah menu **Mahasiswa**:

1. Klik **Ekspor Laporan**.
2. Pilih lokasi file CSV.
3. Aplikasi menjalankan `python inference/main.py --csv <lokasi>`.
4. Pesan sukses atau error ditampilkan.

CSV memakai UTF-8 BOM agar rapi saat dibuka di Excel.

---

## Menjalankan inferensi

Dari folder `inference`:

```bash
python main.py
```

Contoh:

```bash
# Lokasi CSV lain
python main.py --csv hasil\laporan.csv

# Database lain
python main.py --db ..\Database.mdf

# Instance SQL Server lain
python main.py --server "(LocalDB)\MSSQLLocalDB"
```

Opsi:

| Opsi | Keterangan |
| :--- | :--- |
| `--db PATH` | Lokasi `Database.mdf` lain |
| `--server NAMA` | Instance SQL Server lain |
| `--csv PATH` | Lokasi CSV; default `./report.csv` |

Dari root proyek:

```bash
python -m inference.main
```

---

## Scope kode inferensi

| File | Isi |
| :--- | :--- |
| `main.py` | Main process: argumen, ringkasan, panggil evaluasi + ekspor |
| `database.py` | Class `Database`: driver, attach, koneksi, query |
| `membership.py` | Class `Membership`: linear up/down, triangle, trapezoid |
| `subsystem.py` | Konstanta, himpunan fuzzy, class dasar `Subsystem`, Sugeno |
| `akademik_evaluation.py` | Class `Akademik` |
| `kehadiran_evaluation.py` | Class `Kehadiran` |
| `administratif_evaluation.py` | Class `Administratif` |
| `final_result_evaluation.py` | Class `FinalResult`: agregasi skor + profil |
| `process_evaluation.py` | Query, fitur crisp, dan pipeline evaluasi |
| `export.py` | `write_csv`, default `./report.csv` |

`Membership` hanya berisi fungsi umum:

```python
Membership.linear_up(x, a, b)
Membership.linear_down(x, a, b)
Membership.triangle(x, a, b, c)
Membership.trapezoid(x, a, b, c, d)
```

---

## Skema database

| Tabel | Isi |
| :--- | :--- |
| `jurusan` | Nama jurusan |
| `jenjang` | Kode jenjang, masa studi normal/maksimal |
| `prodi` | Program studi + relasi jurusan/jenjang |
| `tahun_akademik` | Tahun mulai, selesai, semester GANJIL/GENAP |
| `mahasiswa` | NIM, prodi, nama, angkatan, kelas, semester, masa studi |
| `mata_kuliah` | Kode, nama, semester, kategori, SKS |
| `mk_prodi` | Pemetaan mata kuliah ke prodi |
| `riwayat_studi` | Periode, semester, mengulang, status, IPS, IPK |
| `detail_mk` | Nilai akhir, alfa, izin, sakit, telat |

---

## Ringkasan model fuzzy

### Variabel input

1. IPS / IPK
2. Tren perubahan IPS
3. Kualitas nilai (penalti D/E)
4. Riwayat mengulang
5. Persentase alfa tertinggi per mata kuliah
6. Akumulasi menit alfa + telat
7. Total jam ketidakhadiran
8. Status registrasi
9. Sisa masa studi

### Output Sugeno

| Profil | Skor |
| :--- | :---: |
| AMAN | 10 |
| RENDAH | 35 |
| SEDANG | 65 |
| TINGGI | 95 |

### Defuzzifikasi

```text
Z = sum(alpha_i * z_i) / sum(alpha_i)
Skor_Akhir = max(Z_Akademik, Z_Kehadiran, Z_Administrasi)
```

Kategori akhir mengikuti *worst-case priority*:
TINGGI > SEDANG > RENDAH > AMAN.

### Asumsi data

- Kolom `alfa`, `izin`, `sakit`, `telat` dianggap jumlah pertemuan.
- 1 pertemuan = `sks x 50` menit; 1 semester efektif = 16 pertemuan.
- Penalti nilai: `(1 x D_UMUM) + (3 x D_MPK) + (3 x D_MKKU) + (3 x E)`, maksimal 10.
- Status `mangkir` 1 semester -> kode 3; >= 2 berturut -> kode 4.

---

## Data demo

`inference/seed_demo.sql` membuat 7 mahasiswa:

| NIM | Profil yang diharapkan |
| :--- | :--- |
| 2507419001 | AMAN |
| 2507419002 | RENDAH |
| 2507419003 | SEDANG |
| 2507419004 | TINGGI (akademik) |
| 2507419005 | TINGGI (kehadiran) |
| 2507419006 | TINGGI (administrasi) |
| 2507419007 | TIDAK ADA DATA |

---

## Pengujian

Dari root proyek:

```bash
python -m unittest discover -s inference/tests -v
```

Dari folder `inference`:

```bash
python -m unittest discover -s tests -v
```

Cakupan: membership linear up/down, triangle, trapezoid, aturan fuzzy,
penalti D/E, presensi, dan mangkir beruntun.

---

## Troubleshooting

### `pyodbc belum terpasang`

```bash
python -m pip install -r inference/requirements.txt
```

### ODBC driver tidak ditemukan

Pasang **Microsoft ODBC Driver 17 for SQL Server**, lalu cek:

```bash
python -c "import pyodbc; print(pyodbc.drivers())"
```

### Database tidak bisa di-attach

Pastikan `Database.mdf` ada di root proyek. Aplikasi VB dan Python memakai file
yang sama. Tutup proses yang menahan file jika perlu membuat ulang database.

### Build VB gagal karena `Database.mdf` tidak ditemukan

Buat database terlebih dahulu mengikuti bagian **Menyiapkan database**.

---

## Lisensi

Proyek ini dibuat untuk keperluan pembelajaran (PBL) Program Studi Teknik
Informatika, Politeknik Negeri Jakarta.
