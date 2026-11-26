-- =====================================================
-- DATA DEMO UNTUK PENGUJIAN SISTEM FUZZY
-- NIM demo memakai prefix 2507419xxx.
-- Jalankan setelah db.sql:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -d Database -i seed_demo.sql
-- =====================================================

IF OBJECT_ID(N'mahasiswa', N'U') IS NULL
BEGIN
    RAISERROR('Tabel mahasiswa belum ada. Jalankan db.sql terlebih dahulu.', 16, 1);
    RETURN;
END;

DELETE FROM mahasiswa
WHERE nim IN (
    '2507419001', '2507419002', '2507419003', '2507419004',
    '2507419005', '2507419006', '2507419007'
);
GO

-- -----------------------------------------------------
-- 2507419001 - profil AMAN
-- -----------------------------------------------------
DECLARE @nim VARCHAR(20) = '2507419001';
DECLARE @prodi INT = 1;
DECLARE @r1 INT;
DECLARE @r2 INT;

INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES (@nim, @prodi, 'Demo Aman', 2025, '2A', 2, 2);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 1, 1, 0, 'aktif', 3.40, 3.40);
SET @r1 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 2, 2, 0, 'aktif', 3.50, 3.45);
SET @r2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat)
SELECT @r2, mp.id, v.nilai, v.alfa, v.izin, v.sakit, v.telat
FROM (VALUES
    ('TIK203', 88, 0, 0, 0, 0),
    ('TIK204', 85, 0, 0, 0, 0),
    ('TIK206', 90, 1, 0, 0, 0),
    ('TIK208', 82, 0, 0, 0, 0),
    ('TIK209', 86, 0, 0, 0, 0),
    ('TIK210', 84, 0, 0, 0, 0)
) AS v(kode, nilai, alfa, izin, sakit, telat)
JOIN mata_kuliah mk ON mk.kode = v.kode
JOIN mk_prodi mp ON mp.id_mk = mk.id AND mp.id_prodi = @prodi;
GO

-- -----------------------------------------------------
-- 2507419002 - profil RENDAH (tren turun ringan, presensi sedang)
-- -----------------------------------------------------
DECLARE @nim VARCHAR(20) = '2507419002';
DECLARE @prodi INT = 1;
DECLARE @r2 INT;

INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES (@nim, @prodi, 'Demo Rendah', 2025, '2A', 2, 2);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 1, 1, 0, 'aktif', 2.80, 2.80);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 2, 2, 0, 'aktif', 2.50, 2.65);
SET @r2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat)
SELECT @r2, mp.id, v.nilai, v.alfa, v.izin, v.sakit, v.telat
FROM (VALUES
    ('TIK203', 75, 1, 0, 0, 0),
    ('TIK204', 78, 0, 0, 0, 0),
    ('TIK206', 72, 2, 0, 0, 0),
    ('TIK208', 70, 0, 0, 0, 0),
    ('TIK209', 74, 1, 0, 0, 0),
    ('TIK210', 76, 0, 0, 0, 0)
) AS v(kode, nilai, alfa, izin, sakit, telat)
JOIN mata_kuliah mk ON mk.kode = v.kode
JOIN mk_prodi mp ON mp.id_mk = mk.id AND mp.id_prodi = @prodi;
GO

-- -----------------------------------------------------
-- 2507419003 - profil SEDANG (D=2, tren turun, sisa masa studi menipis)
-- -----------------------------------------------------
DECLARE @nim VARCHAR(20) = '2507419003';
DECLARE @prodi INT = 1;
DECLARE @r2 INT;

INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES (@nim, @prodi, 'Demo Sedang', 2021, '2A', 2, 12);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 1, 1, 0, 'aktif', 2.60, 2.60);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 2, 2, 0, 'aktif', 2.20, 2.40);
SET @r2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat)
SELECT @r2, mp.id, v.nilai, v.alfa, v.izin, v.sakit, v.telat
FROM (VALUES
    ('TIK203', 55, 0, 0, 0, 0),
    ('TIK204', 58, 0, 0, 0, 0),
    ('TIK206', 65, 2, 0, 0, 0),
    ('TIK208', 62, 1, 0, 0, 0),
    ('TIK209', 60, 1, 0, 0, 0),
    ('TIK210', 66, 1, 1, 0, 0)
) AS v(kode, nilai, alfa, izin, sakit, telat)
JOIN mata_kuliah mk ON mk.kode = v.kode
JOIN mk_prodi mp ON mp.id_mk = mk.id AND mp.id_prodi = @prodi;
GO

-- -----------------------------------------------------
-- 2507419004 - profil TINGGI dari akademik (E + D MKKU + mengulang)
-- -----------------------------------------------------
DECLARE @nim VARCHAR(20) = '2507419004';
DECLARE @prodi INT = 1;
DECLARE @r2 INT;

INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES (@nim, @prodi, 'Demo Tinggi Akademik', 2025, '2A', 2, 3);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 1, 1, 0, 'aktif', 2.80, 2.80);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 2, 2, 1, 'aktif', 1.50, 2.10);
SET @r2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat)
SELECT @r2, mp.id, v.nilai, v.alfa, v.izin, v.sakit, v.telat
FROM (VALUES
    ('TIK203', 70, 0, 0, 0, 0),
    ('TIK204', 72, 0, 0, 0, 0),
    ('TIK206', 65, 1, 0, 0, 0),
    ('TIK208', 45, 0, 0, 0, 0),
    ('TIK209', 55, 1, 0, 0, 0),
    ('TIK210', 68, 0, 0, 0, 0)
) AS v(kode, nilai, alfa, izin, sakit, telat)
JOIN mata_kuliah mk ON mk.kode = v.kode
JOIN mk_prodi mp ON mp.id_mk = mk.id AND mp.id_prodi = @prodi;
GO

-- -----------------------------------------------------
-- 2507419005 - profil TINGGI dari kehadiran (alfa > 20%, menit kritis)
-- -----------------------------------------------------
DECLARE @nim VARCHAR(20) = '2507419005';
DECLARE @prodi INT = 1;
DECLARE @r2 INT;

INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES (@nim, @prodi, 'Demo Tinggi Kehadiran', 2025, '2A', 2, 2);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 1, 1, 0, 'aktif', 3.10, 3.10);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 2, 2, 0, 'aktif', 3.20, 3.15);
SET @r2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat)
SELECT @r2, mp.id, v.nilai, v.alfa, v.izin, v.sakit, v.telat
FROM (VALUES
    ('TIK203', 85, 8, 1, 0, 0),
    ('TIK204', 82, 4, 0, 0, 0),
    ('TIK206', 80, 3, 0, 0, 0),
    ('TIK208', 84, 2, 0, 0, 0),
    ('TIK209', 83, 2, 0, 1, 0),
    ('TIK210', 86, 1, 1, 0, 0)
) AS v(kode, nilai, alfa, izin, sakit, telat)
JOIN mata_kuliah mk ON mk.kode = v.kode
JOIN mk_prodi mp ON mp.id_mk = mk.id AND mp.id_prodi = @prodi;
GO

-- -----------------------------------------------------
-- 2507419006 - profil TINGGI dari administrasi (mangkir beruntun, sisa kritis)
-- -----------------------------------------------------
DECLARE @nim VARCHAR(20) = '2507419006';
DECLARE @prodi INT = 1;
DECLARE @r2 INT;

INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES (@nim, @prodi, 'Demo Tinggi Administrasi', 2018, '2A', 2, 13);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 1, 1, 0, 'mangkir', 3.00, 3.00);

INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk)
VALUES (@nim, 2, 2, 0, 'mangkir', 3.00, 3.00);
SET @r2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat)
SELECT @r2, mp.id, v.nilai, v.alfa, v.izin, v.sakit, v.telat
FROM (VALUES
    ('TIK203', 85, 0, 0, 0, 0),
    ('TIK204', 84, 0, 0, 0, 0),
    ('TIK206', 86, 0, 0, 0, 0),
    ('TIK208', 83, 0, 0, 0, 0),
    ('TIK209', 85, 0, 0, 0, 0),
    ('TIK210', 87, 0, 0, 0, 0)
) AS v(kode, nilai, alfa, izin, sakit, telat)
JOIN mata_kuliah mk ON mk.kode = v.kode
JOIN mk_prodi mp ON mp.id_mk = mk.id AND mp.id_prodi = @prodi;
GO

-- -----------------------------------------------------
-- 2507419007 - belum punya riwayat studi (uji penanganan data kosong)
-- -----------------------------------------------------
INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi)
VALUES ('2507419007', 1, 'Demo Tanpa Data', 2025, '2A', 2, 2);
GO
