# Dokumentasi Evaluasi dan Pengambilan Keputusan

Dokumen ini menjelaskan alur lengkap sistem inferensi Fuzzy Sugeno:

1. pengambilan data dari database,
2. pembentukan fitur crisp,
3. fuzzifikasi,
4. hirarki dan evaluasi aturan,
5. klasifikasi tiap sub-sistem,
6. klasifikasi akhir.

## 1. Peta alur

```text
Database.mdf
    |
    v
process_evaluation.py
  - query mahasiswa
  - query riwayat studi
  - query detail mata kuliah
  - bentuk fitur crisp
    |
    +--> Akademik      (akademik_evaluation.py)
    +--> Kehadiran     (kehadiran_evaluation.py)
    +--> Administratif (administratif_evaluation.py)
    |
    v
FinalResult (final_result_evaluation.py)
  - skor akhir
  - profil akhir
    |
    v
export.py -> report.csv
```

| Tahap | File |
| :--- | :--- |
| Pengambilan data dan fitur | `process_evaluation.py` |
| Aturan dan himpunan fuzzy akademik | `akademik_evaluation.py` |
| Aturan dan himpunan fuzzy kehadiran | `kehadiran_evaluation.py` |
| Aturan dan himpunan fuzzy administratif | `administratif_evaluation.py` |
| Agregasi dan keputusan akhir | `final_result_evaluation.py` |
| Basis umum, fuzzifikasi, Sugeno | `subsystem.py` |
| Fungsi keanggotaan | `membership.py` |
| Ekspor CSV | `export.py` |

## 2. Pengambilan data

`process_evaluation.py` menjalankan tiga query.

### 2.1 Data mahasiswa

```sql
SELECT m.nim, m.nama, p.nama AS prodi, j.kode AS jenjang,
       j.masa_studi_maksimal, m.angkatan, m.kelas,
       m.semester_berjalan, m.masa_studi
FROM mahasiswa m
INNER JOIN prodi p ON p.id = m.id_prodi
INNER JOIN jenjang j ON j.id = p.id_jenjang
ORDER BY m.nim
```

Sumber: `mahasiswa`, `prodi`, `jenjang`.

### 2.2 Riwayat studi

```sql
SELECT r.id, r.nim, t.tahun_mulai, t.tahun_selesai,
       t.semester AS ta_semester, r.semester, r.mengulang,
       r.status, r.ips, r.ipk
FROM riwayat_studi r
INNER JOIN tahun_akademik t ON t.id = r.id_tahun_akademik
ORDER BY r.nim, t.tahun_mulai, t.tahun_selesai,
         CASE t.semester WHEN 'GANJIL' THEN 1 ELSE 2 END,
         r.semester, r.id
```

Sumber: `riwayat_studi`, `tahun_akademik`.

Data diurutkan per NIM lalu per periode. Riwayat terakhir dipakai sebagai
periode berjalan.

### 2.3 Detail mata kuliah

```sql
SELECT d.id_riwayat_studi, d.nilai_akhir, d.alfa, d.izin,
       d.sakit, d.telat, mk.kategori, mk.sks
FROM detail_mk d
INNER JOIN mk_prodi mp ON mp.id = d.id_mk_prodi
INNER JOIN mata_kuliah mk ON mk.id = mp.id_mk
ORDER BY d.id_riwayat_studi, d.id
```

Sumber: `detail_mk`, `mk_prodi`, `mata_kuliah`.

Detail dikelompokkan berdasarkan `id_riwayat_studi`. Sub-sistem kehadiran hanya
memakai detail dari riwayat terakhir.

## 3. Pembentukan fitur crisp

Fitur dibentuk oleh `build_one()` di `process_evaluation.py`.

| Kunci fitur | Sumber | Rumus |
| :--- | :--- | :--- |
| `ips` | `riwayat_studi.ips`, fallback `ipk` | Nilai terakhir yang tersedia |
| `delta_ips` | dua nilai terakhir | `ips_terakhir - ips_sebelumnya` |
| `penalti` | `detail_mk.nilai_akhir` | Penalti D/E, maksimum 10 |
| `jumlah_d` | detail MK | Jumlah nilai D |
| `jumlah_e` | detail MK | Jumlah nilai E |
| `mengulang` | `riwayat_studi.mengulang` | `0/1` |
| `max_alfa` | `detail_mk.alfa` | `max(alfa) / 16 x 100%` |
| `menit` | `alfa + telat` | `sum((alfa + telat) x sks x 50)` |
| `jam` | `alfa + izin + sakit` | `sum((alfa + izin + sakit) x sks x 50) / 60` |
| `status_kode` | `riwayat_studi.status` | Kode 0-4 |
| `sisa` | `jenjang.masa_studi_maksimal - mahasiswa.masa_studi` | Minimal 0 |
| `jumlah_mk` | detail MK terakhir | Jumlah mata kuliah |

### 3.1 Penalti nilai

| Kondisi | Tambahan |
| :--- | :---: |
| D pada mata kuliah `UMUM` | +1 |
| D pada `MPK` / `MKKU` | +3 |
| E pada semua kategori | +3 |

Total dibatasi maksimum 10.

### 3.2 Status registrasi

| Status DB | Kondisi | Kode |
| :--- | :--- | :---: |
| `aktif` | - | 0 |
| `cuti` | - | 1 |
| `cuti_akademik` | - | 2 |
| `mangkir` | 1 semester terakhir | 3 |
| `mangkir` | >= 2 semester beruntun | 4 |

Status yang tidak dikenal dianggap kode 0 dan dicatat di `catatan`.

## 4. Fuzzifikasi

Fuzzifikasi dilakukan oleh `Subsystem.evaluate()` di `subsystem.py`:

```python
memberships = fuzzify_all(values, self.sets)
```

Nilai crisp dibatasi dulu ke `RANGE` variabel, lalu dihitung derajat
keanggotaan setiap himpunan. `SETS` disimpan di masing-masing file evaluasi.

### 4.1 Fungsi keanggotaan

`membership.py` menyediakan:

```python
Membership.linear_up(x, a, b)
Membership.linear_down(x, a, b)
Membership.triangle(x, a, b, c)
Membership.trapezoid(x, a, b, c, d)
```

Singleton dipakai untuk variabel diskrit seperti status dan pengulangan.

### 4.2 Himpunan per sub-sistem

#### Akademik

| Variabel | Himpunan | Fungsi |
| :--- | :--- | :--- |
| `ips` | Kurang | linear down `1.8-2.0` |
| | Cukup | trapezoid `1.8-2.0-2.8-3.0` |
| | Baik | linear up `2.8-3.0` |
| `tren_ips` | Turun Drastis | linear down `-0.55--0.5` |
| | Turun Ringan | trapezoid `-0.55--0.45--0.3--0.25` |
| | Naik / Stabil | linear up `-0.3--0.25` |
| `kualitas_nilai` | Bersih | linear down `0.5-1.0` |
| | Toleransi | trapezoid `0.5-1.0-2.0-2.5` |
| | Tidak Lulus | linear up `2.0-3.0` |
| `pengulangan` | Normal | singleton 0 |
| | Mengulang | singleton 1 |

#### Kehadiran

| Variabel | Himpunan | Fungsi |
| :--- | :--- | :--- |
| `alfa_mk` | Sedikit | linear down `8-10` |
| | Sedang | trapezoid `8-10-18-20` |
| | Banyak | linear up `18-20` |
| `akumulasi_menit` | Sedikit | linear down `325-375` |
| | Sedang | trapezoid `325-375-700-750` |
| | Banyak (SP I) | trapezoid `700-750-1450-1500` |
| | Kritis (SP II/III/DO) | linear up `1450-1500` |
| `jam_gabungan` | Sedikit | linear down `33-38` |
| | Sedang | trapezoid `33-38-70-76` |
| | Banyak | trapezoid `70-76-108-114` |
| | Melebihi Batas | linear up `108-114` |

#### Administratif

| Variabel | Himpunan | Fungsi |
| :--- | :--- | :--- |
| `status` | Aktif Normal | singleton 0 |
| | Cuti Non-Akademik | singleton 1 |
| | Cuti Akademik | singleton 2 |
| | Mangkir 1x | singleton 3 |
| | Mangkir Beruntun | singleton 4 |
| `sisa_masa_studi` | Habis | linear down `0.5-1.0` |
| | Kritis | triangle `0.5-1.0-2.0` |
| | Menipis | trapezoid `1.5-2.0-3.0-4.0` |
| | Longgar | linear up `3.0-4.0` |

## 5. Hirarki dan evaluasi aturan

Aturan tiap sub-sistem berada di `rules()` masing-masing class. Aturan
**tidak berhenti pada kecocokan pertama**. Semua aturan dievaluasi, lalu
kontribusinya digabung dengan Sugeno.

Urutan himpunan risiko pada setiap aturan:

```text
10 = AMAN
35 = RENDAH
65 = SEDANG
95 = TINGGI
```

Semakin tinggi nilai konsekuensi, semakin tinggi risiko.

### 5.1 Operator fuzzy

| Operator | Implementasi |
| :--- | :--- |
| AND | `min(...)` |
| OR | `max(...)` |
| NOT | `1 - nilai` |

Kekuatan aturan (`alpha`) dihitung dari kombinasi derajat keanggotaan sesuai
kondisi aturan.

### 5.2 Aturan Akademik

| Z | Method | Kondisi |
| :---: | :--- | :--- |
| 10 | `aman` | IPS Baik ∧ Nilai Bersih ∧ Tidak Mengulang ∧ Tren Naik/Stabil |
| 35 | `rendah_nilai_bersih` | (IPS Cukup ∨ Tren Turun Ringan) ∧ Nilai Bersih ∧ Tidak Mengulang |
| 35 | `rendah_nilai_toleransi` | (IPS Cukup ∨ Tren Turun Ringan) ∧ Nilai Toleransi ∧ Tidak Mengulang |
| 65 | `sedang_tren_drastis` | IPS Cukup ∧ Tren Turun Drastis |
| 65 | `sedang_penalti_dua` | Nilai Toleransi, hanya jika penalti >= 2 |
| 95 | `tinggi_ips_kurang` | IPS Kurang |
| 95 | `tinggi_tidak_lulus` | Nilai Tidak Lulus |
| 95 | `tinggi_mengulang` | Mengulang mata kuliah |

### 5.3 Aturan Kehadiran

| Z | Method | Kondisi |
| :---: | :--- | :--- |
| 10 | `aman` | Alfa Sedikit ∧ Menit Sedikit ∧ Jam Sedikit |
| 35 | `rendah_alfa_sedang` | Alfa Sedang ∧ Menit tidak tinggi ∧ Jam tidak tinggi |
| 35 | `rendah_menit_sedang` | Menit Sedang ∧ Alfa tidak Banyak ∧ Jam tidak tinggi |
| 35 | `rendah_jam_sedang` | Jam Sedang ∧ Alfa tidak Banyak ∧ Menit tidak tinggi |
| 65 | `sedang_menit_banyak` | Menit Banyak (SP I) ∧ Alfa tidak Banyak |
| 65 | `sedang_jam_banyak` | Jam Banyak ∧ Alfa tidak Banyak |
| 95 | `tinggi_alfa_banyak` | Alfa Banyak |
| 95 | `tinggi_menit_kritis` | Menit Kritis (SP II/III/DO) |
| 95 | `tinggi_jam_melebihi` | Jam Melebihi Batas |

`Menit tidak tinggi` = NOT (Menit Banyak atau Menit Kritis).
`Jam tidak tinggi` = NOT (Jam Banyak atau Jam Melebihi Batas).

### 5.4 Aturan Administratif

| Z | Method | Kondisi |
| :---: | :--- | :--- |
| 10 | `aman` | Aktif Normal ∧ Sisa Longgar |
| 35 | `rendah_cuti` | Cuti Non-Akademik ∧ Sisa Longgar |
| 65 | `sedang_cuti_akademik` | Cuti Akademik |
| 65 | `sedang_sisa_menipis` | Sisa Masa Studi Menipis |
| 95 | `tinggi_mangkir` | Mangkir 1x |
| 95 | `tinggi_mangkir_beruntun` | Mangkir Beruntun |
| 95 | `tinggi_sisa_kritis` | Sisa Masa Studi Kritis |
| 95 | `tinggi_sisa_habis` | Sisa Masa Studi Habis |

## 6. Defuzzifikasi Sugeno

Setelah semua aturan dievaluasi:

```text
Z = sum(alpha_i * z_i) / sum(alpha_i)
```

Implementasi ada di `subsystem.sugeno()`.

Contoh:

| Aturan | Alpha | Z |
| :--- | :---: | :---: |
| Rendah | 1.0 | 35 |
| Sedang | 1.0 | 65 |

```text
Z = (1.0 x 35 + 1.0 x 65) / (1.0 + 1.0) = 50
```

Jika tidak ada aturan aktif, `skor` sub-sistem menjadi `None`.

## 7. Klasifikasi hirarki sub-sistem

Setiap skor sub-sistem dipetakan ke kategori di `subsystem.category()`:

```python
if score <= 22.5:
    return AMAN
if score <= 50:
    return RENDAH
if score <= 80:
    return SEDANG
return TINGGI
```

| Rentang skor | Kategori |
| :--- | :--- |
| `0 <= skor <= 22.5` | AMAN |
| `22.5 < skor <= 50` | RENDAH |
| `50 < skor <= 80` | SEDANG |
| `skor > 80` | TINGGI |

Hasil tiap sub-sistem:

```python
{"skor": 50.0, "kategori": "RENDAH"}
```

Sub-sistem yang tidak punya data atau tidak punya aturan aktif tidak masuk
perhitungan akhir.

## 8. Klasifikasi akhir

Keputusan akhir ditentukan di `final_result_evaluation.py`.

### 8.1 Skor akhir

```python
@property
def score(self):
    return max(result["skor"] for result in self.subsystems())
```

Skor akhir adalah **nilai maksimum** dari sub-sistem yang tersedia.

### 8.2 Profil akhir

```python
@property
def profile(self):
    available = self.subsystems()
    if not available:
        return TIDAK_ADA_DATA
    return worst([result["kategori"] for result in available])
```

Profil akhir memakai **worst-case priority**:

```text
TINGGI > SEDANG > RENDAH > AMAN
```

Artinya, jika satu sub-sistem berisiko TINGGI, profil akhir tetap TINGGI
walaupun sub-sistem lain AMAN.

### 8.3 Kondisi tanpa data

`TIDAK ADA DATA` dipakai jika tidak ada satu pun sub-sistem yang menghasilkan
skor.

Sub-sistem dilewati jika:

- IPS/IPK kosong → akademik dilewati,
- tidak ada detail mata kuliah → kehadiran dilewati,
- tidak ada periode riwayat → administratif dilewati.

### 8.4 Output akhir

`FinalResult.to_dict()` menghasilkan:

```python
{
    "fitur": fitur,
    "akademik": hasil_akademik,
    "kehadiran": hasil_kehadiran,
    "administrasi": hasil_administratif,
    "skor": skor_akhir,
    "profil": profil_akhir,
}
```

`export.py` menuliskan hasil ini ke `report.csv`.

## 9. Ringkasan keputusan

```text
Data mahasiswa
  -> fitur crisp
  -> fuzzifikasi per sub-sistem
  -> semua aturan dievaluasi
  -> Sugeno per sub-sistem
  -> kategori per sub-sistem
  -> skor akhir = max(skor sub-sistem)
  -> profil akhir = kategori terburuk
  -> CSV
```

Titik keputusan utama:

| Keputusan | Lokasi |
| :--- | :--- |
| Skor sub-sistem | `subsystem.sugeno()` |
| Kategori sub-sistem | `subsystem.category()` |
| Skor akhir | `FinalResult.score` |
| Profil akhir | `FinalResult.profile` |
| Profil tanpa data | `FinalResult.profile` |
