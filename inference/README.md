# Sistem Inferensi Fuzzy Sugeno - Profil Risiko Akademik PNJ

Implementasi Python dari rancangan pada
[`desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md`](desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md).

Program membaca data langsung dari `../Database.mdf` (SQL Server LocalDB),
mengekstraksi fitur, menjalankan tiga sub-sistem fuzzy, lalu menampilkan
profil risiko akhir: **AMAN / RENDAH / SEDANG / TINGGI**.

Kode sengaja dibuat singkat: data mengalir sebagai `dict`, tanpa layer
dataclass, dan seluruh penjelasan dipusatkan di README ini.

## Struktur file

| File | Fungsi |
| :--- | :--- |
| `main.py` | CLI: ringkasan, detail, CSV/JSON, simpan ke database |
| `fuzzy.py` | Fungsi keanggotaan, himpunan fuzzy, basis aturan, Sugeno |
| `features.py` | Ekstraksi fitur crisp dari baris database |
| `database.py` | Koneksi LocalDB, `load_data`, `save_results` |
| `report.py` | Tabel konsol, detail aturan, CSV, JSON |
| `export_report.py` | Script ekspor laporan untuk tombol sidebar aplikasi VB |
| `setup_database.py` | Membuat ulang `Database.mdf` dari `db.sql` + demo |
| `seed_demo.sql` | Data mahasiswa demo |
| `tests/test_fuzzy.py` | Unit test fungsi keanggotaan, aturan, dan fitur |

## Prasyarat

- Python 3.10+
- SQL Server LocalDB (`sqllocaldb info` menampilkan `MSSQLLocalDB`)
- ODBC Driver 17/18 for SQL Server

```bash
python -m pip install -r requirements.txt
```

## Menyiapkan database

Jika `../Database.mdf` sudah ada, langsung jalankan `main.py`.

Jika file MDF belum ada:

```bash
python setup_database.py --with-demo
```

Perintah tersebut:

1. Membuat database `Database` dengan file `../Database.mdf`.
2. Memuat skema dan data master dari `../db.sql`.
3. Memuat data mahasiswa demo dari `seed_demo.sql`.

Membuat ulang dari nol (data lama hilang):

```bash
python setup_database.py --force --with-demo
```

Tanpa `--with-demo`, database hanya berisi data master dari `db.sql`.

## Menjalankan inferensi

Dari folder `inference`:

```bash
python main.py
```

Contoh lain:

```bash
# Rincian fuzzifikasi + aturan aktif untuk satu mahasiswa
python main.py --detail 2507419003

# Filter beberapa NIM
python main.py --nim 2507419001 --nim 2507419005

# Ekspor hasil
python main.py --csv hasil.csv --json hasil.json

# Simpan ke tabel hasil_fuzzy di Database.mdf
python main.py --save

# Tampilkan seluruh basis aturan
python main.py --list-rules
```

Dari root proyek, modul juga bisa dijalankan langsung:

```bash
python -m inference.main
```

## Ekspor laporan

Tombol **Ekspor Laporan** di sidebar aplikasi VB membuka dialog lokasi
penyimpanan, lalu menjalankan `inference/export_report.py`. Format ditentukan
dari ekstensi file: `.csv` (default, bisa dibuka di Excel) atau `.json`.

```bash
python export_report.py laporan.csv
python export_report.py laporan.json
```

Script ini memakai data `Database.mdf` yang sama dengan aplikasi VB dan
menuliskan seluruh hasil inferensi risiko akademik.

## Opsi CLI

| Opsi | Keterangan |
| :--- | :--- |
| `--db PATH` | Lokasi MDF lain; default `../Database.mdf`. Path relatif dihitung dari folder tempat perintah dijalankan. |
| `--server NAMA` | Instance SQL Server; default `(LocalDB)\MSSQLLocalDB` |
| `--nim NIM` | Batasi ke NIM tertentu; boleh diulang |
| `--detail NIM` | Tampilkan rincian satu mahasiswa |
| `--attendance-unit meetings\|minutes` | Satuan kolom presensi; default `meetings` |
| `--csv PATH` | Simpan hasil ke CSV |
| `--json PATH` | Simpan hasil ke JSON |
| `--save` | Simpan hasil ke tabel `hasil_fuzzy` |
| `--list-rules` | Tampilkan basis aturan lalu keluar |

Tidak ada opsi `--limit`, `--quiet`, atau `--debug`. Jumlah pertemuan per
semester dan menit per SKS adalah konstanta di `features.py`:

```python
MEETINGS = 16
MINUTES_PER_SKS = 50
```

## Pemetaan data database ke variabel fuzzy

| Variabel desain | Sumber data | Perhitungan |
| :--- | :--- | :--- |
| IPS / IPK | `riwayat_studi.ips`, fallback `ipk` | Periode terakhir; IPS diutamakan |
| Tren IPS | dua periode terakhir | `IPS_sekarang - IPS_sebelumnya` |
| Kualitas nilai | `detail_mk.nilai_akhir` periode terakhir | Skor penalti D/E |
| Mengulang | `riwayat_studi.mengulang` | `0/1` |
| Max % alfa | `detail_mk.alfa` | `alfa / 16 x 100%` (mode `meetings`) |
| Akumulasi menit | `detail_mk.alfa + telat` | `(alfa + telat) x sks x 50` menit |
| Jam ketidakhadiran | `alfa + izin + sakit` | `sum(...) x sks x 50 / 60` jam |
| Status | `riwayat_studi.status` | Kode ordinal 0-4 |
| Sisa masa studi | `jenjang.masa_studi_maksimal - mahasiswa.masa_studi` | Minimal 0 |

### Skor penalti nilai D/E

| Kondisi | Tambahan skor |
| :--- | :---: |
| Nilai D pada mata kuliah `UMUM` | +1 |
| Nilai D pada `MPK` / `MKKU` | +3 |
| Nilai E (semua kategori) | +3 |

Skor dibatasi maksimum 10. Pemetaan hasilnya:

- `0` -> Bersih
- `1-2` -> Toleransi (D pada UMUM)
- `>= 3` -> Tidak Lulus Nilai (D MPK/MKKU atau E)

Aturan `R-AKD-5` hanya aktif jika penalti `>= 2`.

### Status dan mangkir

| Status DB | Kondisi | Kode |
| :--- | :--- | :---: |
| `aktif` | - | 0 (Aktif Normal) |
| `cuti` | - | 1 (Cuti Non-Akademik) |
| `cuti_akademik` | - | 2 (Cuti Akademik) |
| `mangkir` | 1 semester terakhir | 3 (Mangkir 1x) |
| `mangkir` | >= 2 semester beruntun | 4 (Mangkir Beruntun) |

Status yang tidak dikenal dianggap `Aktif Normal` dan dicatat di `catatan`.

### Konversi presensi

Default `--attendance-unit meetings`:

- Kolom `alfa`, `izin`, `sakit`, `telat` dianggap **jumlah pertemuan**.
- Satu pertemuan = `sks x 50` menit.
- Satu semester efektif = 16 pertemuan.

Jika database menyimpan **menit** langsung:

```bash
python main.py --attendance-unit minutes
```

Mode `minutes` memakai `alfa` sebagai menit, `menit = sum(alfa + telat)`, dan
`jam = sum(alfa + izin + sakit) / 60`.

## Model fuzzy

- 9 variabel input: IPS, tren IPS, kualitas nilai, pengulangan, max alfa,
  akumulasi menit, jam gabungan, status, sisa masa studi.
- 3 sub-sistem: `akademik`, `kehadiran`, `administrasi`.
- Output singleton Sugeno: AMAN=10, RENDAH=35, SEDANG=65, TINGGI=95.

Defuzzifikasi tiap sub-sistem:

```text
Z = sum(alpha_i * z_i) / sum(alpha_i)
```

Agregasi akhir memakai *worst-case priority*:

```text
Skor_Akhir = max(Z_Akademik, Z_Kehadiran, Z_Administrasi)
```

Kategori akhir diambil dari kategori terburuk sub-sistem yang tersedia
(TINGGI > SEDANG > RENDAH > AMAN).

### Himpunan fuzzy

| Variabel | Himpunan |
| :--- | :--- |
| `ips` | Kurang, Cukup, Baik |
| `tren_ips` | Turun Drastis, Turun Ringan, Naik / Stabil |
| `kualitas_nilai` | Bersih, Toleransi, Tidak Lulus |
| `pengulangan` | Normal, Mengulang |
| `alfa_mk` | Sedikit, Sedang, Banyak |
| `akumulasi_menit` | Sedikit, Sedang, Banyak (SP I), Kritis (SP II/III/DO) |
| `jam_gabungan` | Sedikit, Sedang, Banyak, Melebihi Batas |
| `status` | Aktif Normal, Cuti Non-Akademik, Cuti Akademik, Mangkir 1x, Mangkir Beruntun |
| `sisa_masa_studi` | Habis, Kritis, Menipis, Longgar |

Basis aturan lengkap bisa dilihat dengan:

```bash
python main.py --list-rules
```

### Penanganan data kosong

- Tidak ada `riwayat_studi` -> profil `TIDAK ADA DATA`.
- IPS/IPK kosong -> sub-sistem akademik dilewati.
- Tidak ada `detail_mk` -> sub-sistem kehadiran dilewati.
- Sub-sistem tanpa aturan aktif tidak dihitung.
- Skor akhir dihitung dari sub-sistem yang tersedia.

## Struktur data internal

`features.build_features()` mengembalikan `list[dict]`. Setiap fitur memakai
kunci:

```text
nim, nama, prodi, jenjang, angkatan, kelas, semester_berjalan,
masa_studi, masa_studi_maksimal, periode, ips, delta_ips, penalti,
jumlah_d, jumlah_e, mengulang, max_alfa, menit, jam, status_kode,
status_nama, sisa, jumlah_mk, catatan
```

`fuzzy.infer(fitur)` mengembalikan satu `dict`:

| Kunci | Isi |
| :--- | :--- |
| `fitur` | Dict fitur di atas |
| `akademik` | Hasil sub-sistem atau `None` |
| `kehadiran` | Hasil sub-sistem atau `None` |
| `administrasi` | Hasil sub-sistem atau `None` |
| `skor` | Skor akhir atau `None` |
| `profil` | `AMAN` / `RENDAH` / `SEDANG` / `TINGGI` / `TIDAK ADA DATA` |

Setiap sub-sistem berisi:

| Kunci | Isi |
| :--- | :--- |
| `nilai` | Nilai crisp yang dipakai sub-sistem |
| `mu` | Derajat keanggotaan per variabel dan himpunan |
| `aktif` | List `(id_aturan, alpha, z, deskripsi)` |
| `skor` | Hasil defuzzifikasi atau `None` |
| `kategori` | Kategori skor sub-sistem |

`database.load_data(db)` mengembalikan tuple
`(students, riwayat, details)`; ketiganya `list[dict]` hasil query SQL.
`database.save_results(db, results)` menyimpan hasil ke `hasil_fuzzy`.

## Output

Tabel ringkasan memuat IPS, tren IPS, penalti, max alfa, menit, jam, sisa masa
studi, skor tiap sub-sistem, skor akhir, dan profil.

Mode `--detail NIM` menampilkan fitur crisp, derajat keanggotaan, aturan aktif
beserta `alpha` dan `z`, skor sub-sistem, hasil akhir, dan catatan.

### CSV

Encoding UTF-8 BOM agar mudah dibuka di Excel. Kolom:

```text
nim, nama, prodi, jenjang, angkatan, kelas, semester_berjalan,
masa_studi, masa_studi_maksimal, periode, ips, delta_ips, penalti,
jumlah_d, jumlah_e, mengulang, max_alfa, menit, jam, status_kode,
status_nama, sisa, jumlah_mk, z_akademik, z_kehadiran, z_administrasi,
skor_akhir, profil, catatan
```

### JSON

Array objek. Setiap objek memuat data mahasiswa, fitur crisp, `subsistem`
(`skor`, `kategori`, `nilai_input`, `aturan_aktif`), `skor_akhir`, `profil`,
dan `catatan`.

### Tabel `hasil_fuzzy`

Dibuat otomatis oleh `--save`:

| Kolom | Tipe | Isi |
| :--- | :--- | :--- |
| `id` | INT IDENTITY | Primary key |
| `nim` | VARCHAR(20) | NIM mahasiswa |
| `nama` | VARCHAR(150) | Nama mahasiswa |
| `skor_akademik` | DECIMAL(6,2) | Z akademik |
| `skor_kehadiran` | DECIMAL(6,2) | Z kehadiran |
| `skor_administrasi` | DECIMAL(6,2) | Z administrasi |
| `skor_akhir` | DECIMAL(6,2) | Skor akhir |
| `profil` | VARCHAR(20) | Profil risiko |
| `detail` | NVARCHAR(MAX) | JSON ringkas hasil |
| `dihitung_pada` | DATETIME2 | Waktu simpan |

Satu NIM hanya punya satu baris; `--save` menghapus baris lama NIM tersebut
sebelum menyisipkan hasil baru.

## Unit test

Dari root proyek:

```bash
python -m unittest discover -s inference/tests -v
```

Atau dari folder `inference`:

```bash
python -m unittest discover -s tests -v
```

Cakupan: fungsi trapesium/segitiga/singleton, fuzzifikasi status dan
pengulangan, aturan akademik/kehadiran/administrasi, `R-AKD-5`, agregasi
worst-case, penalti D/E, konversi presensi, dan deteksi mangkir beruntun.

## Data demo

`seed_demo.sql` membuat 7 mahasiswa:

| NIM | Profil yang diharapkan |
| :--- | :--- |
| 2507419001 | AMAN |
| 2507419002 | RENDAH |
| 2507419003 | SEDANG |
| 2507419004 | TINGGI (akademik) |
| 2507419005 | TINGGI (kehadiran) |
| 2507419006 | TINGGI (administrasi) |
| 2507419007 | TIDAK ADA DATA |

Data demo hanya untuk pengujian; ganti dengan data asli di `Database.mdf`
sesuai kebutuhan.

## Troubleshooting

### `Cannot open database "PBL_AI_Pemvis"`

Database yang dibuat `setup_database.py` bernama `Database` (mengikuti nama
file MDF), bukan `PBL_AI_Pemvis`. Program Python otomatis mendeteksi nama
database yang ter-attach lewat `sys.master_files`.

### `pyodbc belum terpasang`

```bash
python -m pip install -r requirements.txt
```

### `ODBC driver SQL Server tidak ditemukan`

Pasang **Microsoft ODBC Driver 17 for SQL Server**, lalu cek:

```bash
python -c "import pyodbc; print(pyodbc.drivers())"
```

### MDF terkunci / sudah ter-attach

Program mencari nama database di `sys.master_files`. Jika perlu membuat ulang,
tutup aplikasi VB lalu jalankan:

```bash
python setup_database.py --force --with-demo
```

### Database tidak bisa konek karena nama `Database` bentrok

Gejala: `Cannot attach the file ... as database 'Database'`, atau pesan bahwa
nama database `Database` sudah dipakai file lain.

Penyebab: ada salinan lama `bin\Debug\Database.mdf` yang masih ter-attach
dengan nama logis yang sama seperti file root.

Solusi:

1. Tutup Visual Studio dan aplikasi VB.
2. Hapus salinan lama `bin\Debug\Database.mdf` dan
   `bin\Debug\Database_log.ldf` (jangan hapus file root).
3. Jalankan `python setup_database.py --force --with-demo`.
4. Aplikasi VB sekarang diarahkan ke `..\..\Database.mdf` (file root), bukan
   salinan di folder output.

`--db ../Database.mdf` valid selama dijalankan dari folder `inference`.
Dari root proyek, gunakan default tanpa `--db` atau
`python -m inference.main`.
