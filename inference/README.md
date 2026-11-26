# Sistem Inferensi Fuzzy Sugeno - Profil Risiko Akademik PNJ

Implementasi Python dari rancangan pada
[`desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md`](desain_sistem_fuzzy_sugeno_profil_risiko_akademik_pnj.md).

Alur keputusan lengkap: [`DOKUMENTASI_EVALUASI.md`](DOKUMENTASI_EVALUASI.md).

Program membaca `../Database.mdf` (SQL Server LocalDB), menghitung tiga
sub-sistem fuzzy, lalu menulis laporan risiko akademik ke CSV. Profil akhir:
**AMAN / RENDAH / SEDANG / TINGGI / TIDAK ADA DATA**.

## Scope kode

| File | Scope |
| :--- | :--- |
| `main.py` | Main process: CLI, ringkasan konsol, pemanggilan evaluasi dan ekspor |
| `database.py` | `Database`: koneksi LocalDB dan query |
| `membership.py` | `Membership`: linear up/down, triangle, trapezoid |
| `subsystem.py` | Konstanta, himpunan fuzzy, class dasar `Subsystem`, Sugeno |
| `akademik_evaluation.py` | Class `Akademik` |
| `kehadiran_evaluation.py` | Class `Kehadiran` |
| `administratif_evaluation.py` | Class `Administratif` |
| `final_result_evaluation.py` | Class `FinalResult` dan agregasi akhir |
| `process_evaluation.py` | Query data, fitur crisp, dan pipeline evaluasi |
| `export.py` | Ekspor CSV, default `./report.csv` |
| `tests/test_evaluation.py` | Unit test membership, aturan, dan fitur |
| `DOKUMENTASI_EVALUASI.md` | Alur data, hirarki aturan, dan keputusan akhir |
| `seed_demo.sql` | Data mahasiswa demo |

## Prasyarat

- Python 3.10+
- SQL Server LocalDB (`sqllocaldb info` menampilkan `MSSQLLocalDB`)
- ODBC Driver 17/18 for SQL Server

```bash
python -m pip install -r requirements.txt
```

## Database

Jika `../Database.mdf` sudah ada, langsung jalankan program.

Jika belum, buat database bernama `Database` dengan file di root proyek, lalu
jalankan `db.sql` dan opsional `seed_demo.sql`:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE [Database] ON PRIMARY (NAME=N'Database', FILENAME=N'<root>\Database.mdf') LOG ON (NAME=N'Database_log', FILENAME=N'<root>\Database_log.ldf')"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i ..\db.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i seed_demo.sql
```

`db.sql` ada di root proyek. `seed_demo.sql` berisi 7 mahasiswa demo.

## Menjalankan

Dari folder `inference`:

```bash
python main.py
```

Hasil:

- ringkasan NIM, nama, skor, profil di konsol;
- file `./report.csv`.

Pilih lokasi lain:

```bash
python main.py --csv hasil\laporan.csv
```

Opsi CLI:

| Opsi | Keterangan |
| :--- | :--- |
| `--db PATH` | Lokasi MDF lain; default `../Database.mdf` |
| `--server NAMA` | Instance SQL Server; default `(LocalDB)\MSSQLLocalDB` |
| `--csv PATH` | Lokasi CSV; default `./report.csv` |

Dari root proyek:

```bash
python -m inference.main
```

## Alur proses

1. **Main process** (`main.py`) membaca argumen, membuat `Database`, memanggil
   `evaluation.run`, mencetak ringkasan, lalu memanggil `export.write_csv`.
2. **Database connection** (`database.py`) memilih ODBC driver, mendeteksi
   database yang sudah ter-attach, dan menyediakan `query()`.
3. **Membership** (`membership.py`) menyediakan fungsi keanggotaan umum.
4. **Evaluation** (`process_evaluation.py`) menjalankan query, membentuk fitur crisp,
   lalu memanggil class `Akademik`, `Kehadiran`, `Administratif`, dan
   `FinalResult`.
5. **Export** (`export.py`) menulis CSV dengan encoding UTF-8 BOM.

## Membership

`Membership` hanya berisi fungsi umum. Semua menerima nilai `x`:

```python
Membership.linear_up(x, a, b)
Membership.linear_down(x, a, b)
Membership.triangle(x, a, b, c)
Membership.trapezoid(x, a, b, c, d)
```

Contoh:

```python
from inference.membership import Membership

Membership.linear_up(2.9, 2.8, 3.0)  # 0.5
Membership.linear_down(1.9, 1.8, 2.0)  # 0.5
Membership.triangle(1.5, 0.5, 1.0, 2.0)  # 0.5
Membership.trapezoid(1.9, 1.8, 2.0, 2.8, 3.0)  # 0.5
```

Singleton dipakai khusus untuk variabel status dan pengulangan di
`subsystem.py`.

## Data dan fitur

| Variabel | Sumber | Perhitungan |
| :--- | :--- | :--- |
| IPS / IPK | `riwayat_studi.ips`, fallback `ipk` | Periode terakhir |
| Tren IPS | dua periode terakhir | `IPS_sekarang - IPS_sebelumnya` |
| Kualitas nilai | `detail_mk.nilai_akhir` | Penalti D/E |
| Mengulang | `riwayat_studi.mengulang` | `0/1` |
| Max alfa | `detail_mk.alfa` | `alfa / 16 x 100%` |
| Akumulasi menit | `detail_mk.alfa + telat` | `(alfa + telat) x sks x 50` |
| Jam ketidakhadiran | `alfa + izin + sakit` | `sum(...) x sks x 50 / 60` |
| Status | `riwayat_studi.status` | Kode 0-4 |
| Sisa masa studi | `jenjang.masa_studi_maksimal - mahasiswa.masa_studi` | Minimal 0 |

### Penalti nilai

```text
Penalti = (1 x D_UMUM) + (3 x D_MPK) + (3 x D_MKKU) + (3 x E)
```

| Komponen | Arti |
| :--- | :--- |
| `D_UMUM` | Jumlah nilai D pada mata kuliah `UMUM` |
| `D_MPK` | Jumlah nilai D pada mata kuliah `MPK` |
| `D_MKKU` | Jumlah nilai D pada mata kuliah `MKKU` |
| `E` | Jumlah nilai E dari semua kategori |

Total penalti dibatasi maksimum 10. Aturan khusus penalti (`R-AKD-5`) aktif
jika penalti `>= 2`.

### Status

| Status DB | Kondisi | Kode |
| :--- | :--- | :---: |
| `aktif` | - | 0 |
| `cuti` | - | 1 |
| `cuti_akademik` | - | 2 |
| `mangkir` | 1 semester | 3 |
| `mangkir` | >= 2 beruntun | 4 |

### Presensi

- `alfa`, `izin`, `sakit`, `telat` dianggap jumlah pertemuan.
- 1 pertemuan = `sks x 50` menit.
- 1 semester efektif = 16 pertemuan.

## Model fuzzy

9 variabel input: IPS, tren IPS, kualitas nilai, pengulangan, max alfa,
akumulasi menit, jam gabungan, status, sisa masa studi.

3 sub-sistem: `akademik`, `kehadiran`, `administrasi`.

Output singleton Sugeno: AMAN=10, RENDAH=35, SEDANG=65, TINGGI=95.

```text
Z = sum(alpha_i * z_i) / sum(alpha_i)
Skor_Akhir = max(Z_Akademik, Z_Kehadiran, Z_Administrasi)
```

Kategori akhir memakai worst-case: TINGGI > SEDANG > RENDAH > AMAN.

Aturan lengkap ada di `akademik_evaluation.py`, `kehadiran_evaluation.py`, dan `administratif_evaluation.py`.

### Data kosong

- Tidak ada riwayat -> `TIDAK ADA DATA`.
- IPS kosong -> sub-sistem akademik dilewati.
- Tidak ada detail MK -> sub-sistem kehadiran dilewati.
- Sub-sistem tanpa aturan aktif tidak dihitung.

## CSV

Default: `./report.csv`. Kolom:

```text
nim, nama, prodi, jenjang, angkatan, kelas, semester_berjalan, periode,
ips, delta_ips, penalti, jumlah_d, jumlah_e, mengulang, max_alfa, menit, jam,
status_kode, status_nama, sisa, jumlah_mk, z_akademik, z_kehadiran,
z_administrasi, skor, profil, catatan
```

Encoding UTF-8 BOM agar rapi di Excel.

## Tombol sidebar aplikasi VB

Tombol **Ekspor Laporan** memilih lokasi CSV, lalu menjalankan:

```bash
python inference/main.py --csv <lokasi-pilihan>
```

## Unit test

Dari root proyek:

```bash
python -m unittest discover -s inference/tests -v
```

Dari folder `inference`:

```bash
python -m unittest discover -s tests -v
```

Cakupan: linear up/down, triangle, trapezoid, fuzzifikasi, aturan akademik,
kehadiran, administrasi, penalti, dan deteksi mangkir beruntun.

## Data demo

| NIM | Profil |
| :--- | :--- |
| 2507419001 | AMAN |
| 2507419002 | RENDAH |
| 2507419003 | SEDANG |
| 2507419004 | TINGGI (akademik) |
| 2507419005 | TINGGI (kehadiran) |
| 2507419006 | TINGGI (administrasi) |
| 2507419007 | TIDAK ADA DATA |

## Troubleshooting

### `pyodbc belum terpasang`

```bash
python -m pip install -r requirements.txt
```

### ODBC driver tidak ditemukan

Pasang **Microsoft ODBC Driver 17 for SQL Server**, lalu cek:

```bash
python -c "import pyodbc; print(pyodbc.drivers())"
```

### Database tidak bisa di-attach

Pastikan `Database.mdf` ada di root proyek dan tidak sedang dipakai proses lain.
Aplikasi VB dan Python memakai file yang sama. Tutup aplikasi yang menahan file
jika perlu membuat ulang database.
