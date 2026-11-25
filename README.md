# PVAI - Sistem Informasi Akademik & Inferensi Fuzzy Profil Risiko

Repositori ini berisi dua bagian yang saling terhubung:

1. **Aplikasi desktop VB.NET (WinForms)** untuk mengelola data akademik
   mahasiswa: jurusan, jenjang, prodi, tahun akademik, mata kuliah, mahasiswa,
   riwayat studi, nilai, dan presensi.
2. **Sistem inferensi Fuzzy Sugeno (Python)** yang membaca langsung
   `Database.mdf`, menghitung profil risiko akademik, dan menampilkan hasil
   **AMAN / RENDAH / SEDANG / TINGGI**.

Rancangan fuzzy lengkap ada di
[`inference/desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md`](inference/desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md).
Panduan khusus modul inferensi ada di [`inference/README.md`](inference/README.md).

---

## Fitur

### Aplikasi akademik (VB.NET)

- Dashboard ringkasan data.
- CRUD jurusan, jenjang, program studi, tahun akademik.
- CRUD mahasiswa dan riwayat studi per semester.
- Input nilai akhir, alfa, izin, sakit, dan telat per mata kuliah.
- Pemetaan mata kuliah ke program studi.
- Koneksi database terpusat melalui `ConnectionHandler.vb`.
- Tombol **Ekspor Laporan** di sidebar untuk menyimpan laporan risiko
  akademik ke CSV atau JSON.

### Inferensi fuzzy (Python)

- Fuzzifikasi 9 variabel input dengan fungsi trapesium, segitiga, dan singleton.
- Tiga sub-sistem: risiko akademik, kehadiran, dan administrasi.
- 25 aturan fuzzy sesuai dokumen desain.
- Defuzzifikasi Sugeno rata-rata terbobot.
- Agregasi akhir *worst-case priority* (`max`).
- Output tabel konsol, detail aturan per mahasiswa, CSV, JSON.
- Opsi menyimpan hasil ke tabel `hasil_fuzzy`.

---

## Teknologi

| Bagian | Teknologi |
| :--- | :--- |
| Aplikasi desktop | VB.NET WinForms, .NET Framework 4.7.2 |
| Akses database aplikasi | `Microsoft.Data.SqlClient` 7.1.2 |
| Database | Microsoft SQL Server LocalDB, file `Database.mdf` |
| Inferensi fuzzy | Python 3.10+, `pyodbc` |
| Unit test | `unittest` (standar Python) |

---

## Struktur folder

```text
PBL_AI_Pemvis/
├─ PBL_AI_Pemvis.sln
├─ PBL_AI_Pemvis.vbproj
├─ Form1.vb                     # shell aplikasi + navigasi
├─ ConnectionHandler.vb         # koneksi database terpusat
├─ App.config
├─ db.sql                       # skema + data master
├─ Database.mdf                 # dibuat lokal, diabaikan git
├─ Database_log.ldf
├─ Entity/                      # model tabel
├─ Pages/                       # halaman CRUD
├─ UI/                          # tema dan komponen UI
└─ inference/
   ├─ main.py                   # CLI inferensi
   ├─ fuzzy.py                  # fungsi keanggotaan + aturan + Sugeno
   ├─ features.py               # ekstraksi fitur dari database
   ├─ database.py               # koneksi LocalDB + simpan hasil
   ├─ report.py                 # konsol, CSV, JSON
   ├─ export_report.py          # script ekspor laporan untuk tombol sidebar
   ├─ setup_database.py         # buat MDF dari db.sql
   ├─ seed_demo.sql             # data demo
   ├─ requirements.txt
   ├─ README.md
   └─ tests/test_fuzzy.py
```

> `Database.mdf` dan `Database_log.ldf` masuk `.gitignore`. File ini harus
> dibuat lokal dari `db.sql` atau disalin dari mesin lain.

---

## Prasyarat

- Windows
- Visual Studio 2022 (workload **.NET desktop development**) untuk aplikasi VB
- SQL Server LocalDB
- Python 3.10 atau lebih baru
- ODBC Driver 17/18 for SQL Server

Cek LocalDB:

```bash
sqllocaldb info
```

Harus menampilkan instance `MSSQLLocalDB`.

---

## Menyiapkan database

### Cara cepat (disarankan)

Dari folder `inference`:

```bash
cd inference
python -m pip install -r requirements.txt
python setup_database.py --with-demo
```

Perintah tersebut:

1. Membuat database `Database` dengan file `../Database.mdf`.
2. Memuat skema dan data master dari `../db.sql`.
3. Memuat 7 mahasiswa demo dari `seed_demo.sql`.

Jika hanya ingin data master tanpa data demo:

```bash
python setup_database.py
```

Membuat ulang dari nol (data lama hilang):

```bash
python setup_database.py --force --with-demo
```

### Cara manual

1. Buat database bernama `Database` dengan file `Database.mdf` di root proyek.
2. Jalankan `db.sql` pada database tersebut.
3. Opsional, jalankan `inference/seed_demo.sql`.

Contoh dengan `sqlcmd`:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE [Database] ON PRIMARY (NAME=N'Database', FILENAME=N'D:\projects\college\PBL_AI_Pemvis\Database.mdf') LOG ON (NAME=N'Database_log', FILENAME=N'D:\projects\college\PBL_AI_Pemvis\Database_log.ldf')"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i db.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i inference\seed_demo.sql
```

---

## Menjalankan aplikasi VB.NET

1. Buka `PBL_AI_Pemvis.sln` di Visual Studio 2022.
2. Restore NuGet package (klik kanan solution -> **Restore NuGet Packages**).
3. Pastikan `Database.mdf` ada di root proyek.
4. Tekan **F5**.

Proyek **tidak** menyalin `Database.mdf` ke folder output. Aplikasi VB dan
inferensi Python memakai file yang sama di root proyek. Connection string di
`ConnectionHandler.vb` naik dua folder dari `bin\Debug` (atau `bin\Release`):

```vb
Data Source=(LocalDB)\MSSQLLocalDB;
AttachDbFilename=<root proyek>\Database.mdf;
Database=Database;
Integrated Security=True;
TrustServerCertificate=True
```

Dengan `Database=Database`, aplikasi langsung terhubung ke database yang sudah
di-attach, bukan mencoba attach ulang. Jalankan `setup_database.py` saat
aplikasi VB sedang tertutup.

### Ekspor laporan

Tombol **Ekspor Laporan** ada di sidebar, di bawah menu **Mahasiswa**:

1. Klik **Ekspor Laporan**.
2. Pilih lokasi dan format file lewat dialog **Save File** (`.csv` atau `.json`).
3. Aplikasi menjalankan `inference/export_report.py`, lalu menampilkan pesan
   berhasil atau error.

CSV memakai UTF-8 BOM agar rapi saat dibuka di Excel. JSON memuat detail aturan
fuzzy per mahasiswa.

---

## Menjalankan inferensi fuzzy

Dari folder `inference`:

```bash
python main.py
```

Contoh:

```bash
# Ringkasan semua mahasiswa
python main.py

# Detail fuzzifikasi dan aturan untuk satu mahasiswa
python main.py --detail 2507419003

# Filter beberapa NIM
python main.py --nim 2507419001 --nim 2507419005

# Ekspor hasil
python main.py --csv hasil.csv --json hasil.json

# Simpan ke tabel hasil_fuzzy
python main.py --save

# Tampilkan seluruh basis aturan
python main.py --list-rules
```

Opsi penting:

| Opsi | Keterangan |
| :--- | :--- |
| `--db PATH` | Lokasi `Database.mdf` lain |
| `--server NAMA` | Instance SQL Server lain |
| `--nim NIM` | Filter NIM; boleh diulang |
| `--detail NIM` | Rincian satu mahasiswa |
| `--attendance-unit meetings\|minutes` | Satuan kolom presensi |
| `--csv PATH` | Simpan hasil ke CSV |
| `--json PATH` | Simpan hasil ke JSON |
| `--save` | Simpan ke tabel `hasil_fuzzy` |
| `--list-rules` | Tampilkan basis aturan |

Jumlah pertemuan per semester (`16`) dan menit per SKS (`50`) adalah konstanta
di `inference/features.py`. Tidak ada opsi `--limit`, `--quiet`, atau
`--debug`; seluruh data diproses dan pesan error ditampilkan ringkas.

---

## Skema database

| Tabel | Isi |
| :--- | :--- |
| `jurusan` | Nama jurusan |
| `jenjang` | Kode jenjang, masa studi normal, masa studi maksimal |
| `prodi` | Program studi + relasi jurusan/jenjang |
| `tahun_akademik` | Tahun mulai, tahun selesai, semester GANJIL/GENAP |
| `mahasiswa` | NIM, prodi, nama, angkatan, kelas, semester berjalan, masa studi |
| `mata_kuliah` | Kode, nama, semester, kategori (MPK/MKKU/UMUM), SKS |
| `mk_prodi` | Pemetaan mata kuliah ke prodi |
| `riwayat_studi` | Periode, semester, mengulang, status, IPS, IPK |
| `detail_mk` | Nilai akhir, alfa, izin, sakit, telat per mata kuliah |
| `hasil_fuzzy` | Dibuat otomatis oleh `--save`; menyimpan skor dan profil |

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

Kategori akhir mengikuti *worst-case priority*: TINGGI > SEDANG > RENDAH > AMAN.

### Asumsi data

- Kolom `alfa`, `izin`, `sakit`, `telat` default dianggap **jumlah pertemuan**.
- 1 pertemuan = `sks x 50` menit; 1 semester efektif = 16 pertemuan.
- Gunakan `--attendance-unit minutes` jika database menyimpan menit langsung.
- Penalti nilai: D `UMUM` +1, D `MPK/MKKU` +3, E +3, maksimal 10.
- Status `mangkir` 1 semester -> kode 3; >= 2 semester berturut -> kode 4.

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

Data demo hanya untuk pengujian. Ganti dengan data asli di `Database.mdf`
sesuai kebutuhan.

---

## Pengujian

Dari root proyek:

```bash
python -m unittest discover -s inference/tests -v
```

Atau dari folder `inference`:

```bash
python -m unittest discover -s tests -v
```

Cakupan test: fungsi keanggotaan, aturan akademik/kehadiran/administrasi,
agregasi worst-case, penalti D/E, konversi presensi, dan deteksi mangkir
beruntun.

---

## Troubleshooting

### `Cannot open database "PBL_AI_Pemvis"` atau login gagal

Database yang dibuat `setup_database.py` bernama `Database` (mengikuti nama
file MDF), bukan `PBL_AI_Pemvis`. Untuk `sqlcmd`, gunakan:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -Q "SELECT COUNT(*) FROM mahasiswa"
```

Program Python otomatis mendeteksi nama database yang ter-attach.

### `pyodbc belum terpasang`

```bash
python -m pip install -r inference/requirements.txt
```

### `ODBC driver SQL Server tidak ditemukan`

Pasang **Microsoft ODBC Driver 17 for SQL Server**, lalu cek:

```bash
python -c "import pyodbc; print(pyodbc.drivers())"
```

### MDF terkunci / sudah ter-attach

Program Python menangani file yang sudah ter-attach dengan mencari nama
database di `sys.master_files`. Jika perlu membuat ulang, tutup aplikasi VB
lalu jalankan:

```bash
python inference/setup_database.py --force --with-demo
```

### `Database` tidak bisa konek karena nama dipakai file lain

Gejala: `Cannot attach the file ... as database 'Database'` atau pesan bahwa
nama database `Database` sudah dipakai.

Penyebab: salinan lama `bin\Debug\Database.mdf` masih ter-attach dengan nama
logis yang sama seperti file root.

Solusi:

1. Tutup Visual Studio dan aplikasi VB.
2. Hapus salinan lama `bin\Debug\Database.mdf` dan
   `bin\Debug\Database_log.ldf` (jangan hapus file di root proyek).
3. Jalankan `python inference\setup_database.py --force --with-demo`.
4. Jalankan ulang aplikasi. Connection string sekarang memakai file root,
   bukan salinan di folder output.

### Build VB gagal karena `Database.mdf` tidak ditemukan

Buat database terlebih dahulu:

```bash
cd inference
python setup_database.py --with-demo
```

---

## Lisensi

Proyek ini dibuat untuk keperluan pembelajaran (PBL) Program Studi Teknik
Informatika, Politeknik Negeri Jakarta.
