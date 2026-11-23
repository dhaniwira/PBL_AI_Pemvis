''' <summary>Halaman beranda dengan ringkasan jumlah data.</summary>
Public Class DashboardPage
    Inherits PageBase

    Private ReadOnly _errorLabel As Label
    Private _loadError As String = ""

    Public Sub New()
        MyBase.New("Beranda", "Ringkasan data Sistem Informasi Akademik PVAI.")

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = Color.Transparent
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 148))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim stats As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = True,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }

        Try
            stats.Controls.Add(CreateStatCard("Mahasiswa", Mahasiswa.All().Count.ToString(), Theme.Primary))
            stats.Controls.Add(CreateStatCard("Program Studi", Prodi.All().Count.ToString(), Color.FromArgb(5, 150, 105)))
            stats.Controls.Add(CreateStatCard("Mata Kuliah", MataKuliah.All().Count.ToString(), Color.FromArgb(217, 119, 6)))
            stats.Controls.Add(CreateStatCard("Jenjang", Jenjang.All().Count.ToString(), Color.FromArgb(124, 58, 237)))
        Catch ex As Exception
            _loadError = "Gagal memuat ringkasan data: " & ex.Message
            stats.Controls.Add(CreateStatCard("Data", "-", Theme.Danger))
        End Try

        root.Controls.Add(stats, 0, 0)

        Dim welcome As New RoundedPanel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(28, 24, 28, 24),
            .Margin = New Padding(0, 4, 0, 0)
        }

        Dim welcomeLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 5,
            .BackColor = Color.Transparent
        }
        welcomeLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        For i As Integer = 0 To 4
            welcomeLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next

        Dim lblTitle = Theme.MakeLabel("Selamat datang di PVAI", 16, FontStyle.Bold)
        lblTitle.Margin = New Padding(0, 0, 0, 8)
        welcomeLayout.Controls.Add(lblTitle, 0, 0)

        Dim lblDesc = Theme.MakeLabel(
            "Aplikasi ini mengelola data akademik mulai dari jenjang, tahun akademik, mata kuliah, " &
            "jurusan, program studi, sampai mahasiswa beserta riwayat studi dan nilainya.",
            9.75)
        lblDesc.ForeColor = Theme.TextMuted
        lblDesc.Margin = New Padding(0, 0, 0, 20)
        lblDesc.MaximumSize = New Size(780, 0)
        welcomeLayout.Controls.Add(lblDesc, 0, 1)

        Dim lblGuideTitle = Theme.MakeLabel("Cara menggunakan", 11, FontStyle.Bold)
        lblGuideTitle.Margin = New Padding(0, 0, 0, 10)
        welcomeLayout.Controls.Add(lblGuideTitle, 0, 2)

        Dim guide As New Label With {
            .Text = "1.  Pilih menu di sidebar untuk membuka halaman data." & Environment.NewLine &
                    "2.  Gunakan tombol Tambah, Ubah, dan Hapus pada form di sisi kiri." & Environment.NewLine &
                    "3.  Klik dua kali baris tabel atau tombol Detail untuk membuka halaman bertingkat." & Environment.NewLine &
                    "     Prodi -> Mata Kuliah Prodi, Mahasiswa -> Riwayat Studi -> Nilai Mata Kuliah.",
            .AutoSize = True,
            .Font = Theme.MakeFont(9.75),
            .ForeColor = Theme.TextDark,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0, 0, 0, 14)
        }
        welcomeLayout.Controls.Add(guide, 0, 3)

        _errorLabel = Theme.MakeLabel(_loadError, 8.75)
        _errorLabel.ForeColor = Theme.Danger
        _errorLabel.Margin = New Padding(0, 4, 0, 0)
        welcomeLayout.Controls.Add(_errorLabel, 0, 4)

        welcome.Controls.Add(welcomeLayout)
        root.Controls.Add(welcome, 0, 1)

        ContentHost.Controls.Add(root)
    End Sub

    Private Function CreateStatCard(title As String, value As String, accent As Color) As RoundedPanel
        Dim card As New RoundedPanel With {
            .Size = New Size(200, 120),
            .Margin = New Padding(0, 0, 14, 14),
            .Padding = New Padding(18, 16, 18, 14)
        }

        Dim lblTitle = Theme.MakeLabel(title, 8.75, FontStyle.Bold)
        lblTitle.ForeColor = Theme.TextMuted
        lblTitle.Location = New Point(18, 15)
        card.Controls.Add(lblTitle)

        Dim lblValue = Theme.MakeLabel(value, 26, FontStyle.Bold)
        lblValue.ForeColor = accent
        lblValue.Location = New Point(16, 38)
        card.Controls.Add(lblValue)

        Dim lblHint = Theme.MakeLabel("total data", 8.5)
        lblHint.ForeColor = Theme.TextMuted
        lblHint.Location = New Point(19, 87)
        card.Controls.Add(lblHint)

        Return card
    End Function

End Class
