-- =====================================================
-- DATABASE SCRIPT: PVAI (MICROSOFT SQL SERVER)
-- =====================================================

-- 1. TABEL JURUSAN
CREATE TABLE jurusan (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nama VARCHAR(100) NOT NULL
);

-- 2. TABEL JENJANG
CREATE TABLE jenjang (
    id INT IDENTITY(1,1) PRIMARY KEY,
    kode VARCHAR(10) NOT NULL UNIQUE,
    masa_studi_normal INT NOT NULL,
    masa_studi_maksimal INT NOT NULL
);

-- 3. TABEL PROGRAM STUDI
CREATE TABLE prodi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_jurusan INT NOT NULL REFERENCES jurusan(id),
    id_jenjang INT NOT NULL REFERENCES jenjang(id),
    nama VARCHAR(100) NOT NULL
);

-- 4. TABEL TAHUN AKADEMIK
CREATE TABLE tahun_akademik (
    id INT IDENTITY(1,1) PRIMARY KEY,
    tahun_mulai INT NOT NULL,
    tahun_selesai INT NOT NULL,
    semester VARCHAR(10) NOT NULL
        CHECK (semester IN ('GANJIL', 'GENAP'))
);

-- 5. TABEL MAHASISWA
CREATE TABLE mahasiswa (
    nim VARCHAR(20) PRIMARY KEY,
    id_prodi INT NOT NULL REFERENCES prodi(id),
    nama VARCHAR(150) NOT NULL,
    angkatan INT NOT NULL,
    kelas VARCHAR(20) NOT NULL,
    semester_berjalan INT NOT NULL,
    masa_studi INT NOT NULL
);

-- 6. TABEL MATA KULIAH
CREATE TABLE mata_kuliah (
    id INT IDENTITY(1,1) PRIMARY KEY,
    kode VARCHAR(20) NOT NULL UNIQUE,
    nama VARCHAR(150) NOT NULL,
    semester INT NOT NULL CHECK (semester BETWEEN 1 AND 8),
    kategori VARCHAR(10) NOT NULL
        CHECK (kategori IN ('MPK', 'MKKU', 'UMUM')),
    sks INT NOT NULL
);

-- 7. TABEL PEMETAAN MATA KULIAH KE PRODI
CREATE TABLE mk_prodi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_mk INT NOT NULL REFERENCES mata_kuliah(id) ON DELETE CASCADE,
    id_prodi INT NOT NULL REFERENCES prodi(id) ON DELETE CASCADE,
    UNIQUE (id_mk, id_prodi)
);

-- 8. TABEL RIWAYAT STUDI
CREATE TABLE riwayat_studi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nim VARCHAR(20) NOT NULL REFERENCES mahasiswa(nim) ON DELETE CASCADE,
    id_tahun_akademik INT NOT NULL REFERENCES tahun_akademik(id),
    semester INT NOT NULL,
    mengulang BIT NOT NULL DEFAULT 0,
    status VARCHAR(20) NOT NULL
        CHECK (status IN ('aktif', 'cuti', 'cuti_akademik', 'mangkir')),
    ips DECIMAL(3,2),
    ipk DECIMAL(3,2)
);

-- 9. TABEL DETAIL PENGAMBILAN MATA KULIAH
CREATE TABLE detail_mk (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_riwayat_studi INT NOT NULL REFERENCES riwayat_studi(id) ON DELETE CASCADE,
    id_mk_prodi INT NOT NULL REFERENCES mk_prodi(id),
    nilai_akhir DECIMAL(5,2),
    alfa INT NOT NULL DEFAULT 0,
    izin INT NOT NULL DEFAULT 0,
    sakit INT NOT NULL DEFAULT 0,
    telat INT NOT NULL DEFAULT 0
);

-- =====================================================
-- INSERT DATA JURUSAN
-- =====================================================

SET IDENTITY_INSERT jurusan ON;

INSERT INTO jurusan (id, nama) VALUES
(1, 'Teknik Informatika dan Komputer');

SET IDENTITY_INSERT jurusan OFF;

-- =====================================================
-- INSERT DATA JENJANG
-- =====================================================

SET IDENTITY_INSERT jenjang ON;

INSERT INTO jenjang (id, kode, masa_studi_normal, masa_studi_maksimal) VALUES
(1, 'D3', 6, 10),
(2, 'D4', 8, 14);

SET IDENTITY_INSERT jenjang OFF;

-- =====================================================
-- INSERT DATA PROGRAM STUDI
-- =====================================================

SET IDENTITY_INSERT prodi ON;

INSERT INTO prodi (id, id_jurusan, id_jenjang, nama) VALUES
(1, 1, 2, 'Teknik Informatika'),
(2, 1, 2, 'Teknik Multimedia dan Jaringan'),
(3, 1, 2, 'Teknik Multimedia Digital');

SET IDENTITY_INSERT prodi OFF;

-- =====================================================
-- INSERT DATA TAHUN AKADEMIK
-- =====================================================

SET IDENTITY_INSERT tahun_akademik ON;

INSERT INTO tahun_akademik (id, tahun_mulai, tahun_selesai, semester) VALUES
(1, 2025, 2026, 'GANJIL'),
(2, 2025, 2026, 'GENAP');

SET IDENTITY_INSERT tahun_akademik OFF;

-- =====================================================
-- INSERT DATA MATA KULIAH
-- =====================================================

SET IDENTITY_INSERT mata_kuliah ON;

INSERT INTO mata_kuliah (id, kode, nama, semester, kategori, sks) VALUES

-- SEMESTER 1
(1, 'TIK101', 'Pendidikan Agama', 1, 'MPK', 2),
(2, 'TIK102', 'Pancasila', 1, 'MPK', 2),
(3, 'TIK103', 'Bahasa Indonesia', 1, 'MPK', 2),
(4, 'TIK104', 'Kewarganegaraan', 1, 'MPK', 2),
(5, 'TIK105', 'Bahasa Inggris untuk TIK', 1, 'UMUM', 2),
(6, 'TIK106', 'Matematika Diskrit', 1, 'UMUM', 2),
(7, 'TIK107', 'Pengantar Teknologi Informasi & Komunikasi', 1, 'MKKU', 2),
(8, 'TIK108', 'Algoritma & Pemrograman', 1, 'MKKU', 2),
(9, 'TIK109', 'Teknologi Multimedia', 1, 'MKKU', 2),
(10, 'TIK110', 'Sistem Operasi', 1, 'MKKU', 2),
(11, 'TIK111', 'Organisasi & Arsitektur Komputer', 1, 'MKKU', 2),
(12, 'TIK112', 'Piranti Komputer', 1, 'MKKU', 2),
(13, 'TIK113', 'Jaringan Komputer', 1, 'MKKU', 2),
(14, 'TIK114', 'Pengantar Desain Komunikasi Visual', 1, 'MKKU', 2),
(15, 'TIK115', 'Pengantar Jaringan Komputer', 1, 'MKKU', 3),
(16, 'TIK116', 'Pengantar Multimedia', 1, 'MKKU', 3),

-- SEMESTER 2
(17, 'TIK201', 'Pendidikan Agama (Genap)', 2, 'MPK', 2),
(18, 'TIK202', 'Pancasila (Genap)', 2, 'MPK', 2),
(19, 'TIK203', 'Kepemimpinan & Pengembangan Karakter', 2, 'UMUM', 2),
(20, 'TIK204', 'English for IT Professional', 2, 'UMUM', 2),
(21, 'TIK205', 'Etika dan Hukum TIK', 2, 'UMUM', 2),
(22, 'TIK206', 'Aljabar Linier', 2, 'UMUM', 2),
(23, 'TIK207', 'Aljabar Linear Geometrik', 2, 'UMUM', 2),
(24, 'TIK208', 'Algoritma & Struktur Data', 2, 'MKKU', 2),
(25, 'TIK209', 'Sistem Basis Data', 2, 'MKKU', 2),
(26, 'TIK210', 'Pemrograman Berorientasi Objek', 2, 'MKKU', 2),
(27, 'TIK211', 'Pemrograman Web Dasar', 2, 'MKKU', 2),
(28, 'TIK212', 'Rekayasa Perangkat Lunak', 2, 'MKKU', 2),
(29, 'TIK213', 'Sistem Informasi Manajemen', 2, 'MKKU', 2),
(30, 'TIK214', 'Jaringan Komputer (Genap)', 2, 'MKKU', 2),
(31, 'TIK215', 'Teknik Digital', 2, 'MKKU', 2),
(32, 'TIK216', 'Jaringan Komputer Lanjut', 2, 'MKKU', 2),
(33, 'TIK217', 'Interaksi Manusia dan Komputer', 2, 'MKKU', 2),
(34, 'TIK218', 'Multimedia Lanjut', 2, 'MKKU', 3),
(35, 'TIK219', 'Struktur Data', 2, 'MKKU', 2),
(36, 'TIK220', 'Prinsip dan Desain Animasi', 2, 'MKKU', 2),
(37, 'TIK221', 'Desain UI/UX (TMD)', 2, 'MKKU', 3),
(38, 'TIK222', 'Sistem Basis Data (TMD)', 2, 'MKKU', 3),
(39, 'TIK223', 'Menggambar', 2, 'MKKU', 3),
(40, 'TIK224', 'Desain Komunikasi Visual Lanjut', 2, 'MKKU', 2),

-- SEMESTER 3
(41, 'TIK301', 'Bahasa Indonesia (Sem 3)', 3, 'MPK', 2),
(42, 'TIK302', 'Kewarganegaraan (Sem 3)', 3, 'MPK', 2),
(43, 'TIK303', 'Metode Numerik', 3, 'UMUM', 2),
(44, 'TIK304', 'English for IT Professional (Sem 3)', 3, 'UMUM', 2),
(45, 'TIK305', 'Hukum dan Etika', 3, 'UMUM', 2),
(46, 'TIK306', 'Kewirausahaan Bidang TIK', 3, 'UMUM', 2),
(47, 'TIK307', 'Desain UI/UX', 3, 'MKKU', 2),
(48, 'TIK308', 'Manajemen Proyek TIK', 3, 'MKKU', 2),
(49, 'TIK309', 'Analisis dan Desain Sistem Informasi', 3, 'MKKU', 2),
(50, 'TIK310', 'Pemrograman Visual', 3, 'MKKU', 2),
(51, 'TIK311', 'Pemrograman Basis Data', 3, 'MKKU', 2),
(52, 'TIK312', 'Kecerdasan Buatan', 3, 'MKKU', 2),
(53, 'TIK313', 'Grafika Komputer', 3, 'MKKU', 2),
(54, 'TIK314', 'Sistem Terdistribusi', 3, 'MKKU', 2),
(55, 'TIK315', 'Pemrograman Web Lanjut', 3, 'MKKU', 2),
(56, 'TIK316', 'Pemrograman Web (Front, Back End)', 3, 'MKKU', 2),
(57, 'TIK317', 'Sistem Embedded', 3, 'MKKU', 3),
(58, 'TIK318', 'Organisasi dan Arsitektur Komputer (Sem 3)', 3, 'MKKU', 2),
(59, 'TIK319', 'Pemrograman Berorientasi Objek (Sem 3)', 3, 'MKKU', 2),
(60, 'TIK320', 'Infrastruktur Jaringan / Perutean dan Penyambungan', 3, 'MKKU', 2),
(61, 'TIK321', 'Keamanan Komputer', 3, 'MKKU', 2),
(62, 'TIK322', 'Rekayasa Perangkat Lunak (Sem 3)', 3, 'MKKU', 2),
(63, 'TIK323', 'Rekayasa Perangkat Lunak Multimedia', 3, 'MKKU', 2),
(64, 'TIK324', 'Pemodelan 3D', 3, 'MKKU', 3),
(65, 'TIK325', 'Desain Web', 3, 'MKKU', 2),
(66, 'TIK326', 'Sistem Operasi (Sem 3)', 3, 'MKKU', 2),
(67, 'TIK327', 'Perancangan dan Pengembangan Game', 3, 'MKKU', 2),

-- SEMESTER 4
(68, 'TIK401', 'Statistika dan Probabilitas', 4, 'UMUM', 2),
(69, 'TIK402', 'Etika dan Hukum TIK (Sem 4)', 4, 'UMUM', 2),
(70, 'TIK403', 'Kewirausahaan Bidang TIK (Sem 4)', 4, 'UMUM', 2),
(71, 'TIK404', 'Metode Penelitian', 4, 'UMUM', 2),
(72, 'TIK405', 'Bahasa Inggris untuk Komunikasi', 4, 'UMUM', 2),
(73, 'TIK406', 'Metode Statistika', 4, 'UMUM', 2),
(74, 'TIK407', 'Cloud Computing', 4, 'MKKU', 2),
(75, 'TIK408', 'Sistem Pendukung Keputusan', 4, 'MKKU', 2),
(76, 'TIK409', 'Mobile Computing', 4, 'MKKU', 2),
(77, 'TIK410', 'Pengolahan Citra Digital', 4, 'MKKU', 2),
(78, 'TIK411', 'Software Quality Assurance', 4, 'MKKU', 2),
(79, 'TIK412', 'Mobile Programming', 4, 'MKKU', 2),
(80, 'TIK413', 'Keamanan Informasi', 4, 'MKKU', 2),
(81, 'TIK414', 'Framework Programming', 4, 'MKKU', 2),
(82, 'TIK415', 'Teknologi Nirkabel', 4, 'MKKU', 2),
(83, 'TIK416', 'Keamanan Sistem Informasi', 4, 'MKKU', 2),
(84, 'TIK417', 'Teknologi Virtualisasi', 4, 'MKKU', 2),
(85, 'TIK418', 'Pemrograman Perangkat Jaringan', 4, 'MKKU', 2),
(86, 'TIK419', 'Wireless Sensor Network', 4, 'MKKU', 2),
(87, 'TIK420', 'Kriptografi', 4, 'MKKU', 2),
(88, 'TIK421', 'Jaringan Server', 4, 'MKKU', 2),
(89, 'TIK422', 'Manajemen Proyek Multimedia', 4, 'MKKU', 2),
(90, 'TIK423', 'Pemrograman Game 2D', 4, 'MKKU', 3),
(91, 'TIK424', 'Teknik Audio Video', 4, 'MKKU', 3),
(92, 'TIK425', 'Pemrograman Web', 4, 'MKKU', 2),
(93, 'TIK426', 'Multimedia Authoring & Scripting', 4, 'MKKU', 3),
(94, 'TIK427', 'Animasi 3D', 4, 'MKKU', 3),

-- SEMESTER 5
(95, 'TIK501', 'Etika Profesional', 5, 'UMUM', 2),
(96, 'TIK502', 'Komunikasi Profesional', 5, 'UMUM', 3),
(97, 'TIK503', 'Perencanaan Sistem', 5, 'MKKU', 3),
(98, 'TIK504', 'Dokumentasi Sistem', 5, 'MKKU', 3),
(99, 'TIK505', 'Requirement Engineering', 5, 'MKKU', 3),
(100, 'TIK506', 'Implementasi Sistem', 5, 'MKKU', 3),
(101, 'TIK507', 'Pemodelan Sistem', 5, 'MKKU', 3),
(102, 'TIK508', 'Dokumentasi Jaringan', 5, 'MKKU', 3),
(103, 'TIK509', 'Pemodelan Jaringan', 5, 'MKKU', 3),
(104, 'TIK510', 'Perancangan dan Perencanaan Jaringan', 5, 'MKKU', 3),
(105, 'TIK511', 'Analisis Permintaan Jaringan', 5, 'MKKU', 3),
(106, 'TIK512', 'Implementasi Jaringan', 5, 'MKKU', 3),
(107, 'TIK513', 'Pra Produksi', 5, 'MKKU', 3),
(108, 'TIK514', 'Produksi', 5, 'MKKU', 4),
(109, 'TIK515', 'Pengujian dan Evaluasi', 5, 'MKKU', 3),
(110, 'TIK516', 'Pasca Produksi', 5, 'MKKU', 4),
(111, 'TIK517', 'Dokumentasi Produk', 5, 'MKKU', 2),

-- SEMESTER 6
(112, 'TIK601', 'Metodologi Penelitian (Sem 6)', 6, 'UMUM', 2),
(113, 'TIK602', 'English Proficiency Test Preparation', 6, 'UMUM', 2),
(114, 'TIK603', 'Kewirausahaan Bidang TIK (Sem 6)', 6, 'UMUM', 2),
(115, 'TIK604', 'Kewirausahaan Multimedia', 6, 'UMUM', 2),
(116, 'TIK605', 'Machine Learning', 6, 'MKKU', 2),
(117, 'TIK606', 'Proyek Kekhususan Bidang Teknik Informatika', 6, 'MKKU', 2),
(118, 'TIK607', 'Natural Language Processing', 6, 'MKKU', 2),
(119, 'TIK608', 'Internet of Things', 6, 'MKKU', 2),
(120, 'TIK609', 'Data Mining & Big Data', 6, 'MKKU', 2),
(121, 'TIK610', 'Keamanan Cloud / Keamanan Cyber', 6, 'MKKU', 2),
(122, 'TIK611', 'Pemrograman Bergerak', 6, 'MKKU', 2),
(123, 'TIK612', 'Jaringan VoIP', 6, 'MKKU', 2),
(124, 'TIK613', 'Proyek Kekhususan Bidang TMJ', 6, 'MKKU', 2),
(125, 'TIK614', 'Kompresi Data', 6, 'MKKU', 2),
(126, 'TIK615', 'Jaringan Enterprise', 6, 'MKKU', 2),
(127, 'TIK616', 'Sistem Terdistribusi (Sem 6)', 6, 'MKKU', 2),
(128, 'TIK617', 'Kecerdasan Buatan untuk Game', 6, 'MKKU', 3),
(129, 'TIK618', 'Mixed Reality', 6, 'MKKU', 2),
(130, 'TIK619', 'Project Kekhususan Multimedia', 6, 'MKKU', 2),
(131, 'TIK620', 'Sistem Embedded Multimedia', 6, 'MKKU', 3),
(132, 'TIK621', 'Pemrograman Game 3D', 6, 'MKKU', 3),

-- SEMESTER 8
(133, 'TIK801', 'Kewarganegaraan (Sem 8)', 8, 'MPK', 2),
(134, 'TIK802', 'Kecerdasan Buatan (Sem 8)', 8, 'MKKU', 2),
(135, 'TIK803', 'English Proficiency Test Preparation (Sem 8)', 8, 'UMUM', 2);

SET IDENTITY_INSERT mata_kuliah OFF;

-- =====================================================
-- INSERT DATA PEMETAAN MATA KULIAH KE PRODI
-- =====================================================

INSERT INTO mk_prodi (id_mk, id_prodi) VALUES

-- PRODI 1: TEKNIK INFORMATIKA
(1,1),(2,1),(3,1),(5,1),(6,1),(7,1),(8,1),(9,1),(10,1),(11,1),
(19,1),(20,1),(22,1),(24,1),(25,1),(26,1),(27,1),(28,1),(29,1),(30,1),
(43,1),(47,1),(48,1),(49,1),(50,1),(51,1),(52,1),(53,1),(54,1),(55,1),
(68,1),(69,1),(74,1),(75,1),(76,1),(77,1),(78,1),(79,1),(80,1),(81,1),
(95,1),(96,1),(97,1),(98,1),(99,1),(100,1),(101,1),
(112,1),(113,1),(114,1),(116,1),(117,1),(118,1),(119,1),(120,1),
(133,1),

-- PRODI 2: TEKNIK MULTIMEDIA DAN JARINGAN
(3,2),(4,2),(5,2),(6,2),(7,2),(8,2),(9,2),(10,2),(12,2),(13,2),
(17,2),(18,2),(19,2),(21,2),(22,2),(24,2),(25,2),(31,2),(32,2),(33,2),
(43,2),(44,2),(46,2),(56,2),(57,2),(58,2),(59,2),(60,2),(61,2),(62,2),
(68,2),(70,2),(71,2),(82,2),(83,2),(84,2),(85,2),(86,2),(87,2),(88,2),
(95,2),(96,2),(102,2),(103,2),(104,2),(105,2),(106,2),
(113,2),(119,2),(121,2),(122,2),(123,2),(124,2),(125,2),(126,2),(127,2),
(134,2),

-- PRODI 3: TEKNIK MULTIMEDIA DIGITAL
(1,3),(2,3),(5,3),(6,3),(7,3),(8,3),(14,3),(15,3),(16,3),
(23,3),(34,3),(35,3),(36,3),(37,3),(38,3),(39,3),(40,3),
(41,3),(42,3),(45,3),(53,3),(59,3),(63,3),(64,3),(65,3),(66,3),(67,3),
(72,3),(73,3),(89,3),(90,3),(91,3),(92,3),(93,3),(94,3),
(95,3),(96,3),(107,3),(108,3),(109,3),(110,3),(111,3),
(112,3),(115,3),(122,3),(128,3),(129,3),(130,3),(131,3),(132,3);