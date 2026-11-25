# Desain Sistem Inferensi Fuzzy Sugeno: Evaluasi Profil Risiko Akademik Mahasiswa PNJ

Dokumen ini berisi hasil ekstraksi variabel diskritisasi dan basis aturan (*rule base*) dari dokumen spesifikasi *Sistem Evaluasi Profil Risiko Masalah Akademik Mahasiswa PNJ* yang telah dikonversi menjadi rancangan model **Fuzzy Inference System (FIS) Metode Sugeno (Orde-Nol)**.

---

## 1. Ekstraksi & Pemodelan Variabel Input (Fuzzifikasi)

Berdasarkan variabel diskrit pada dokumen asli, berikut adalah pemetaan ke dalam **Himpunan Fuzzy** beserta penentuan **bentuk grafik fungsi keanggotaannya (*Membership Function*)**:

### A. Kelompok Komponen Akademik dan Nilai

#### 1. Indeks Prestasi (IPS / IPK)
* **Semesta Pembicaraan ($U$):** $[0.00, 4.00]$
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Kurang:** Grafik **Trapesium (Bahu Kiri)** $\rightarrow$ $[0.00, 0.00, 1.80, 2.00]$
  * **Cukup:** Grafik **Trapesium** $\rightarrow$ $[1.80, 2.00, 2.80, 3.00]$
  * **Baik:** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[2.80, 3.00, 4.00, 4.00]$

#### 2. Tren Perubahan IPS ($\Delta \text{IPS} = \text{IPS}_{\text{sekarang}} - \text{IPS}_{\text{lalu}}$)
* **Semesta Pembicaraan ($U$):** $[-4.00, +4.00]$
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Turun Drastis:** Grafik **Trapesium (Bahu Kiri)** $\rightarrow$ $[-4.00, -4.00, -0.55, -0.50]$
  * **Turun Ringan:** Grafik **Segitiga / Trapesium** $\rightarrow$ $[-0.55, -0.45, -0.30, -0.25]$
  * **Naik / Stabil:** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[-0.30, -0.25, 4.00, 4.00]$

#### 3. Kualitas Nilai Mata Kuliah (Indeks Penalti Nilai D & E)
*Catatan: Karena kategori asli bersifat kategorikal majemuk (kombinasi jumlah D pada UMUM, jumlah D pada MPK/MKKU, dan jumlah E), variabel ini direpresentasikan sebagai skor penalti kuantitatif $[0, 10]$ (0 = Bersih, 1–2 = D pada UMUM, $\ge 3$ = D pada MPK/MKKU atau E).*
* **Semesta Pembicaraan ($U$):** $[0, 10]$
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Bersih:** Grafik **Trapesium (Bahu Kiri)** / *Singleton* di $0$ $\rightarrow$ $[0, 0, 0.5, 1]$
  * **Toleransi:** Grafik **Trapesium** $\rightarrow$ $[0.5, 1, 2, 2.5]$
  * **Tidak Lulus Nilai:** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[2, 3, 10, 10]$

#### 4. Riwayat Pengulangan Semester (`mengulang`)
* **Semesta Pembicaraan ($U$):** $\{0, 1\}$ (Variabel Biner / *Crisp*)
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Normal:** Grafik **Singleton** pada $x = 0$ ($\mu = 1$ jika `FALSE`)
  * **Mengulang:** Grafik **Singleton** pada $x = 1$ ($\mu = 1$ jika `TRUE`)

---

### B. Kelompok Komponen Kehadiran (Presensi)

#### 5. Persentase Alfa Tertinggi per Mata Kuliah ($\text{Max}\% \text{Alfa}$)
* **Semesta Pembicaraan ($U$):** $[0\%, 100\%]$
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Sedikit:** Grafik **Trapesium (Bahu Kiri)** $\rightarrow$ $[0, 0, 8, 10]$
  * **Sedang:** Grafik **Trapesium** $\rightarrow$ $[8, 10, 18, 20]$
  * **Banyak:** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[18, 20, 100, 100]$

#### 6. Akumulasi Menit Alfa & Keterlambatan Semester ($\sum (\text{alfa} + \text{telat})$)
* **Semesta Pembicaraan ($U$):** $[0, 2500]$ menit
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Sedikit:** Grafik **Trapesium (Bahu Kiri)** $\rightarrow$ $[0, 0, 325, 375]$
  * **Sedang:** Grafik **Trapesium** $\rightarrow$ $[325, 375, 700, 750]$
  * **Banyak (SP I):** Grafik **Trapesium** $\rightarrow$ $[700, 750, 1450, 1500]$
  * **Kritis (SP II/III/DO):** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[1450, 1500, 2500, 2500]$

#### 7. Total Jam Ketidakhadiran Gabungan ($\sum (\text{alfa} + \text{izin} + \text{sakit}) / 60$)
* **Semesta Pembicaraan ($U$):** $[0, 200]$ jam
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Sedikit:** Grafik **Trapesium (Bahu Kiri)** $\rightarrow$ $[0, 0, 33, 38]$
  * **Sedang:** Grafik **Trapesium** $\rightarrow$ $[33, 38, 70, 76]$
  * **Banyak:** Grafik **Trapesium** $\rightarrow$ $[70, 76, 108, 114]$
  * **Melebihi Batas:** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[108, 114, 200, 200]$

---

### C. Kelompok Komponen Administrasi dan Masa Studi

#### 8. Status Registrasi & Keaktifan (`status`)
* **Semesta Pembicaraan ($U$):** $[0, 4]$ (Kode Numerik Ordinal)
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Aktif Normal:** Grafik **Singleton** pada $x = 0$
  * **Cuti Non-Akademik:** Grafik **Singleton** pada $x = 1$
  * **Cuti Akademik:** Grafik **Singleton** pada $x = 2$
  * **Mangkir 1x:** Grafik **Singleton** pada $x = 3$
  * **Mangkir Beruntun:** Grafik **Singleton** pada $x = 4$

#### 9. Sisa Masa Studi ($\text{masa\_studi\_maksimal} - \text{masa\_studi}$)
* **Semesta Pembicaraan ($U$):** $[0, 14]$ semester
* **Himpunan Fuzzy & Bentuk Grafik:**
  * **Habis:** Grafik **Singleton / Bahu Kiri** $\rightarrow$ $[0, 0, 0.5, 1]$ (atau Singleton di $0$)
  * **Kritis:** Grafik **Segitiga** $\rightarrow$ $[0.5, 1, 2]$ (puncak di $1$)
  * **Menipis:** Grafik **Trapesium** $\rightarrow$ $[1.5, 2, 3, 4]$
  * **Longgar:** Grafik **Trapesium (Bahu Kanan)** $\rightarrow$ $[3, 4, 14, 14]$

---

## 2. Variabel Output Sugeno (Konsekuen / *Singleton*)

Dalam metode **Fuzzy Sugeno Orde-Nol**, setiap aturan menghasilkan nilai konstan (*crisp singleton*) pada rentang skor risiko $[0, 100]$:

| Kategori Profil Risiko | Bentuk Grafik Output | Nilai Konstanta ($z_i$) | Keterangan Tindakan |
| :--- | :--- | :---: | :--- |
| **AMAN** | **Singleton** | $z = 10$ | Pemantauan rutin berkala |
| **RENDAH** | **Singleton** | $z = 35$ | Notifikasi pengingat ringan di portal |
| **SEDANG** | **Singleton** | $z = 65$ | Konseling wajib dengan Dosen PA |
| **TINGGI** | **Singleton** | $z = 95$ | Intervensi Kajur, pemanggilan orang tua, potensi cuti/DO |

---

## 3. Ekstraksi Basis Aturan (*Fuzzy Rule Base*)

Arsitektur inferensi disusun secara **bertingkat (*Hierarchical Fuzzy System*)** agar sesuai dengan logika pada dokumen asli:

### Tahap 1: Evaluasi Sub-Sistem Kelompok Komponen

#### A. Sub-Sistem Risiko Akademik (`Risiko_Akademik`)
* **R-AKD-1:** `IF` IPS = *Baik* `AND` Kualitas_Nilai = *Bersih* `AND` Pengulangan = *Normal* `AND` Tren_IPS = *Naik/Stabil* `THEN` Risiko_Akademik = **AMAN** ($10$)
* **R-AKD-2:** `IF` (IPS = *Cukup* `OR` Tren_IPS = *Turun Ringan*) `AND` Kualitas_Nilai = *Bersih* `AND` Pengulangan = *Normal* `THEN` Risiko_Akademik = **RENDAH** ($35$)
* **R-AKD-3:** `IF` (IPS = *Cukup* `OR` Tren_IPS = *Turun Ringan*) `AND` Kualitas_Nilai = *Toleransi* `AND` Pengulangan = *Normal* `THEN` Risiko_Akademik = **RENDAH** ($35$)
* **R-AKD-4:** `IF` IPS = *Cukup* `AND` Tren_IPS = *Turun Drastis* `THEN` Risiko_Akademik = **SEDANG** ($65$)
* **R-AKD-5:** `IF` Kualitas_Nilai = *Toleransi* (dengan $D=2$) `THEN` Risiko_Akademik = **SEDANG** ($65$)
* **R-AKD-6:** `IF` IPS = *Kurang* `THEN` Risiko_Akademik = **TINGGI** ($95$)
* **R-AKD-7:** `IF` Kualitas_Nilai = *Tidak Lulus Nilai* `THEN` Risiko_Akademik = **TINGGI** ($95$)
* **R-AKD-8:** `IF` Pengulangan = *Mengulang* `THEN` Risiko_Akademik = **TINGGI** ($95$)

#### B. Sub-Sistem Risiko Kehadiran (`Risiko_Kehadiran`)
* **R-HDR-1:** `IF` Alfa_MK = *Sedikit* `AND` Akumulasi_Menit = *Sedikit* `AND` Jam_Gabungan = *Sedikit* `THEN` Risiko_Kehadiran = **AMAN** ($10$)
* **R-HDR-2:** `IF` Alfa_MK = *Sedang* `AND` Akumulasi_Menit $\neq$ (*Banyak* `OR` *Kritis*) `AND` Jam_Gabungan $\neq$ (*Banyak* `OR` *Melebihi Batas*) `THEN` Risiko_Kehadiran = **RENDAH** ($35$)
* **R-HDR-3:** `IF` Akumulasi_Menit = *Sedang* `AND` Alfa_MK $\neq$ *Banyak* `AND` Jam_Gabungan $\neq$ (*Banyak* `OR` *Melebihi Batas*) `THEN` Risiko_Kehadiran = **RENDAH** ($35$)
* **R-HDR-4:** `IF` Jam_Gabungan = *Sedang* `AND` Alfa_MK $\neq$ *Banyak* `AND` Akumulasi_Menit $\neq$ (*Banyak* `OR` *Kritis*) `THEN` Risiko_Kehadiran = **RENDAH** ($35$)
* **R-HDR-5:** `IF` Akumulasi_Menit = *Banyak (SP I)* `AND` Alfa_MK $\neq$ *Banyak* `THEN` Risiko_Kehadiran = **SEDANG** ($65$)
* **R-HDR-6:** `IF` Jam_Gabungan = *Banyak* `AND` Alfa_MK $\neq$ *Banyak* `THEN` Risiko_Kehadiran = **SEDANG** ($65$)
* **R-HDR-7:** `IF` Alfa_MK = *Banyak* `THEN` Risiko_Kehadiran = **TINGGI** ($95$)
* **R-HDR-8:** `IF` Akumulasi_Menit = *Kritis (SP II/III/DO)* `THEN` Risiko_Kehadiran = **TINGGI** ($95$)
* **R-HDR-9:** `IF` Jam_Gabungan = *Melebihi Batas* `THEN` Risiko_Kehadiran = **TINGGI** ($95$)

#### C. Sub-Sistem Risiko Administrasi (`Risiko_Administrasi`)
* **R-ADM-1:** `IF` Status = *Aktif Normal* `AND` Sisa_Masa_Studi = *Longgar* `THEN` Risiko_Administrasi = **AMAN** ($10$)
* **R-ADM-2:** `IF` Status = *Cuti Non-Akademik* `AND` Sisa_Masa_Studi = *Longgar* `THEN` Risiko_Administrasi = **RENDAH** ($35$)
* **R-ADM-3:** `IF` Status = *Cuti Akademik* `THEN` Risiko_Administrasi = **SEDANG** ($65$)
* **R-ADM-4:** `IF` Sisa_Masa_Studi = *Menipis* `THEN` Risiko_Administrasi = **SEDANG** ($65$)
* **R-ADM-5:** `IF` Status = *Mangkir 1x* `THEN` Risiko_Administrasi = **TINGGI** ($95$)
* **R-ADM-6:** `IF` Status = *Mangkir Beruntun* `THEN` Risiko_Administrasi = **TINGGI** ($95$)
* **R-ADM-7:** `IF` Sisa_Masa_Studi = *Kritis* `THEN` Risiko_Administrasi = **TINGGI** ($95$)
* **R-ADM-8:** `IF` Sisa_Masa_Studi = *Habis* `THEN` Risiko_Administrasi = **TINGGI** ($95$)

---

### Tahap 2: Aturan Agregasi Profil Risiko Akhir (*Worst-Case Priority*)

Untuk mempertahankan prinsip *worst-case priority* pada metode Sugeno, agregasi akhir menggunakan operator maksimum ($\max$) dari hasil defuzzifikasi (*Weighted Average*) ketiga sub-sistem, atau didefinisikan melalui aturan agregasi berikut:

1. **R-FINAL-1 (AMAN):** `IF` Risiko_Akademik = *AMAN* `AND` Risiko_Kehadiran = *AMAN* `AND` Risiko_Administrasi = *AMAN* `THEN` Profil_Risiko = **AMAN** ($10$)
2. **R-FINAL-2 (RENDAH):** `IF` (Risiko_Akademik = *RENDAH* `OR` Risiko_Kehadiran = *RENDAH* `OR` Risiko_Administrasi = *RENDAH*) `AND` tidak ada yang *SEDANG/TINGGI* `THEN` Profil_Risiko = **RENDAH** ($35$)
3. **R-FINAL-3 (SEDANG):** `IF` (Risiko_Akademik = *SEDANG* `OR` Risiko_Kehadiran = *SEDANG* `OR` Risiko_Administrasi = *SEDANG*) `AND` tidak ada yang *TINGGI* `THEN` Profil_Risiko = **SEDANG** ($65$)
4. **R-FINAL-4 (TINGGI):** `IF` Risiko_Akademik = *TINGGI* `OR` Risiko_Kehadiran = *TINGGI* `OR` Risiko_Administrasi = *TINGGI* `THEN` Profil_Risiko = **TINGGI** ($95$)

> **Catatan Defuzzifikasi Sugeno:**
> Pada masing-masing sub-sistem, nilai akhir dihitung dengan metode rata-rata terbobot (*Weighted Average*):
> $$Z = \frac{\sum (\alpha_i \cdot z_i)}{\sum \alpha_i}$$
> Kemudian skor akhir mahasiswa ditentukan melalui:
> $$\text{Skor\_Risiko\_Akhir} = \max(Z_{\text{Akademik}}, Z_{\text{Kehadiran}}, Z_{\text{Administrasi}})$$