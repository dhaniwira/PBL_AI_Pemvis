Imports System.Diagnostics
Imports System.IO

''' <summary>
''' Shell aplikasi: sidebar navigasi, area konten, dan riwayat navigasi bertingkat.
''' </summary>
Public Class Form1

    Private ReadOnly _contentHost As Panel
    Private ReadOnly _navButtons As New Dictionary(Of String, RoundedButton)()
    Private ReadOnly _navStack As New Stack(Of PageBase)()

    Private _currentPage As PageBase
    Private _activeNav As String = ""

    Public Sub New()
        InitializeComponent()

        Text = "PVAI - Sistem Informasi Akademik"
        StartPosition = FormStartPosition.CenterScreen
        ClientSize = New Size(1400, 860)
        MinimumSize = New Size(1180, 720)
        BackColor = Theme.Background
        Font = Theme.MakeFont(9.75)
        DoubleBuffered = True

        _contentHost = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Theme.Background
        }

        Dim sidebar = BuildSidebar()

        Controls.Add(_contentHost)
        Controls.Add(sidebar)

        ShowRoot(New DashboardPage(), "beranda")
    End Sub

    Private Function BuildSidebar() As Panel
        Dim sidebar As New Panel With {
            .Dock = DockStyle.Left,
            .Width = 250,
            .BackColor = Theme.Sidebar,
            .Padding = New Padding(14, 0, 14, 16)
        }

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = Color.Transparent
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 104))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 56))

        ' Merek aplikasi
        Dim brand As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }

        Dim lblLogo = Theme.MakeLabel("PVAI", 20, FontStyle.Bold)
        lblLogo.ForeColor = Color.White
        lblLogo.Location = New Point(6, 20)
        brand.Controls.Add(lblLogo)

        Dim lblBrand = Theme.MakeLabel("Sistem Informasi Akademik", 8.5)
        lblBrand.ForeColor = Color.FromArgb(148, 163, 184)
        lblBrand.Location = New Point(8, 60)
        brand.Controls.Add(lblBrand)

        layout.Controls.Add(brand, 0, 0)

        ' Menu navigasi
        Dim nav As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }

        nav.Controls.Add(CreateNavButton("Beranda", "beranda"))
        nav.Controls.Add(CreateNavButton("Jenjang", "jenjang"))
        nav.Controls.Add(CreateNavButton("Tahun Akademik", "tahun"))
        nav.Controls.Add(CreateNavButton("Mata Kuliah", "mk"))
        nav.Controls.Add(CreateNavButton("Jurusan", "jurusan"))
        nav.Controls.Add(CreateNavButton("Program Studi", "prodi"))
        nav.Controls.Add(CreateNavButton("Mahasiswa", "mahasiswa"))
        nav.Controls.Add(CreateActionButton("Ekspor Laporan", AddressOf HandleExportReport))

        layout.Controls.Add(nav, 0, 1)

        ' Kaki sidebar
        Dim footer As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }

        Dim lblFooter = Theme.MakeLabel("PBL AI Pemvis", 8.5, FontStyle.Bold)
        lblFooter.ForeColor = Color.FromArgb(148, 163, 184)
        lblFooter.Location = New Point(6, 10)
        footer.Controls.Add(lblFooter)

        Dim lblFooterSub = Theme.MakeLabel("Semester 3", 8)
        lblFooterSub.ForeColor = Color.FromArgb(100, 116, 139)
        lblFooterSub.Location = New Point(6, 29)
        footer.Controls.Add(lblFooterSub)

        layout.Controls.Add(footer, 0, 2)

        sidebar.Controls.Add(layout)
        Return sidebar
    End Function

    Private Function CreateNavButton(text As String, key As String) As RoundedButton
        Dim btn As New RoundedButton With {
            .Text = "    " & text,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Size = New Size(220, 42),
            .Margin = New Padding(0, 0, 0, 4),
            .Font = Theme.MakeFont(10),
            .BackColor = Theme.Sidebar,
            .ForeColor = Color.FromArgb(203, 213, 225),
            .Cursor = Cursors.Hand,
            .CornerRadius = 8
        }
        btn.FlatAppearance.MouseOverBackColor = Theme.SidebarHover
        btn.FlatAppearance.MouseDownBackColor = Theme.SidebarHover

        AddHandler btn.Click, Sub(sender, e) ShowRoot(CreatePage(key), key)

        _navButtons(key) = btn
        Return btn
    End Function

    Private Function CreateActionButton(text As String, handler As EventHandler) As RoundedButton
        Dim btn As New RoundedButton With {
            .Text = "    " & text,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Size = New Size(220, 42),
            .Margin = New Padding(0, 12, 0, 4),
            .Font = Theme.MakeFont(10, FontStyle.Bold),
            .BackColor = Color.FromArgb(22, 163, 74),
            .ForeColor = Color.White,
            .Cursor = Cursors.Hand,
            .CornerRadius = 8
        }
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(21, 128, 61)
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(21, 128, 61)
        AddHandler btn.Click, handler
        Return btn
    End Function

    Private Sub HandleExportReport(sender As Object, e As EventArgs)
        Using dialog As New SaveFileDialog()
            dialog.Title = "Ekspor Laporan CSV"
            dialog.Filter = "Laporan CSV (*.csv)|*.csv"
            dialog.FilterIndex = 1
            dialog.AddExtension = True
            dialog.RestoreDirectory = True
            dialog.OverwritePrompt = True
            dialog.FileName = "report.csv"

            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim outputPath = dialog.FileName
            If Path.GetExtension(outputPath).ToLowerInvariant() <> ".csv" Then
                outputPath &= ".csv"
            End If

            Try
                ExportReport(outputPath)
                MessageBox.Show(
                    Me,
                    "Laporan berhasil diekspor:" & Environment.NewLine & outputPath,
                    "Ekspor Laporan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show(
                    Me,
                    "Gagal mengekspor laporan:" & Environment.NewLine & ex.Message,
                    "Ekspor Laporan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub ExportReport(outputPath As String)
        Dim root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\.."))
        Dim script = Path.Combine(root, "inference", "main.py")
        If Not File.Exists(script) Then
            Throw New FileNotFoundException("Script ekspor laporan tidak ditemukan.", script)
        End If

        Dim python = FindPython()
        If String.IsNullOrEmpty(python) Then
            Throw New InvalidOperationException(
                "Python tidak ditemukan di PATH. Pasang Python 3 lalu jalankan ulang aplikasi.")
        End If

        Dim info As New ProcessStartInfo With {
            .FileName = python,
            .Arguments = Quote(script) & " --csv " & Quote(outputPath),
            .WorkingDirectory = root,
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True
        }

        Using proc = Process.Start(info)
            If proc Is Nothing Then
                Throw New InvalidOperationException("Python tidak dapat dijalankan.")
            End If

            Dim stdout = proc.StandardOutput.ReadToEndAsync()
            Dim stderr = proc.StandardError.ReadToEndAsync()
            proc.WaitForExit()

            Dim output = stdout.Result
            Dim errorText = stderr.Result
            If proc.ExitCode <> 0 Then
                Dim pesan = If(String.IsNullOrWhiteSpace(errorText), output, errorText)
                Throw New InvalidOperationException(pesan.Trim())
            End If
            If Not File.Exists(outputPath) Then
                Throw New InvalidOperationException("Script selesai, tetapi file laporan tidak terbentuk.")
            End If
        End Using
    End Sub

    Private Function FindPython() As String
        For Each candidate In {"python", "python3", "py"}
            Try
                Dim info As New ProcessStartInfo With {
                    .FileName = candidate,
                    .Arguments = "--version",
                    .UseShellExecute = False,
                    .CreateNoWindow = True,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True
                }
                Using proc = Process.Start(info)
                    If proc IsNot Nothing AndAlso
                       proc.WaitForExit(5000) AndAlso
                       proc.ExitCode = 0 Then
                        Return candidate
                    End If
                End Using
            Catch
            End Try
        Next
        Return Nothing
    End Function

    Private Function Quote(value As String) As String
        Dim q = ChrW(34).ToString()
        Return q & value.Replace(q, q & q) & q
    End Function

    Private Function CreatePage(key As String) As PageBase
        Select Case key
            Case "jenjang"
                Return New JenjangPage()
            Case "tahun"
                Return New TahunAkademikPage()
            Case "mk"
                Return New MataKuliahPage()
            Case "jurusan"
                Return New JurusanPage()
            Case "prodi"
                Return New ProdiPage()
            Case "mahasiswa"
                Return New MahasiswaPage()
            Case Else
                Return New DashboardPage()
        End Select
    End Function

    Private Sub AttachPageEvents(page As PageBase)
        AddHandler page.BackRequested, AddressOf HandleBack

        Dim prodiPage = TryCast(page, ProdiPage)
        If prodiPage IsNot Nothing Then
            AddHandler prodiPage.OpenProdiDetail, AddressOf HandleOpenProdiDetail
        End If

        Dim mahasiswaPage = TryCast(page, MahasiswaPage)
        If mahasiswaPage IsNot Nothing Then
            AddHandler mahasiswaPage.OpenMahasiswaDetail, AddressOf HandleOpenMahasiswaDetail
        End If

        Dim mahasiswaDetail = TryCast(page, MahasiswaDetailPage)
        If mahasiswaDetail IsNot Nothing Then
            AddHandler mahasiswaDetail.OpenNilai, AddressOf HandleOpenNilai
        End If
    End Sub

    Private Sub ShowRoot(page As PageBase, key As String)
        ClearStack()
        ReplaceCurrent(page)
        _activeNav = key
        UpdateNavStyle()
    End Sub

    Private Sub PushPage(page As PageBase)
        If _currentPage IsNot Nothing Then
            _contentHost.Controls.Remove(_currentPage)
            _navStack.Push(_currentPage)
        End If

        _currentPage = page
        AttachPageEvents(page)
        page.Dock = DockStyle.Fill
        _contentHost.Controls.Add(page)
    End Sub

    Private Sub ReplaceCurrent(page As PageBase)
        If _currentPage IsNot Nothing Then
            _contentHost.Controls.Remove(_currentPage)
            _currentPage.Dispose()
        End If

        _currentPage = page
        AttachPageEvents(page)
        page.Dock = DockStyle.Fill
        _contentHost.Controls.Add(page)
    End Sub

    Private Sub HandleBack()
        If _navStack.Count = 0 Then Return

        Dim previous = _navStack.Pop()

        If _currentPage IsNot Nothing Then
            _contentHost.Controls.Remove(_currentPage)
            _currentPage.Dispose()
        End If

        _currentPage = previous
        previous.Dock = DockStyle.Fill
        _contentHost.Controls.Add(previous)
    End Sub

    Private Sub ClearStack()
        While _navStack.Count > 0
            Dim page = _navStack.Pop()
            page.Dispose()
        End While
    End Sub

    Private Sub UpdateNavStyle()
        For Each pair In _navButtons
            If pair.Key = _activeNav Then
                pair.Value.BackColor = Theme.Primary
                pair.Value.ForeColor = Color.White
                pair.Value.FlatAppearance.MouseOverBackColor = Theme.PrimaryDark
                pair.Value.FlatAppearance.MouseDownBackColor = Theme.PrimaryDark
            Else
                pair.Value.BackColor = Theme.Sidebar
                pair.Value.ForeColor = Color.FromArgb(203, 213, 225)
                pair.Value.FlatAppearance.MouseOverBackColor = Theme.SidebarHover
                pair.Value.FlatAppearance.MouseDownBackColor = Theme.SidebarHover
            End If
        Next
    End Sub

    Private Sub HandleOpenProdiDetail(prodi As Prodi)
        PushPage(New ProdiDetailPage(prodi))
    End Sub

    Private Sub HandleOpenMahasiswaDetail(mahasiswa As Mahasiswa)
        PushPage(New MahasiswaDetailPage(mahasiswa))
    End Sub

    Private Sub HandleOpenNilai(mahasiswa As Mahasiswa, riwayat As RiwayatStudi, periodeText As String)
        PushPage(New RiwayatNilaiPage(mahasiswa, riwayat, periodeText))
    End Sub

End Class
