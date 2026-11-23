''' <summary>
''' Halaman detail mahasiswa: daftar riwayat studi milik mahasiswa tersebut.
''' </summary>
Public Class MahasiswaDetailPage
    Inherits PageBase

    Public Event OpenNilai(mahasiswa As Mahasiswa, riwayat As RiwayatStudi, periodeText As String)

    Private ReadOnly _mahasiswa As Mahasiswa
    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _cmbTahun As ComboBox
    Private ReadOnly _numSemester As NumericUpDown
    Private ReadOnly _chkMengulang As CheckBox
    Private ReadOnly _cmbStatus As ComboBox
    Private ReadOnly _txtIps As TextBox
    Private ReadOnly _txtIpk As TextBox
    Private ReadOnly _btnTambah As RoundedButton

    Private _selectedId As Integer?
    Private _loading As Boolean
    Private _riwayatById As New Dictionary(Of Integer, RiwayatStudi)()

    Public Sub New(mahasiswa As Mahasiswa)
        MyBase.New("Riwayat Studi",
                   "Riwayat studi " & mahasiswa.Nama & " (" & mahasiswa.Nim & ").", True)
        _mahasiswa = mahasiswa

        Dim layout = CreateSplitLayout(400)
        _form = New FormCard("Form Riwayat Studi", "Isi data, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Riwayat Studi " & mahasiswa.Nama)

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _cmbTahun = Theme.MakeComboBox()
        _numSemester = Theme.MakeNumeric(1, 14, 1)
        _chkMengulang = Theme.MakeCheckBox("Mengulang mata kuliah")
        _cmbStatus = Theme.MakeComboBox()
        _cmbStatus.Items.Add(New OptionItem("aktif", "Aktif"))
        _cmbStatus.Items.Add(New OptionItem("cuti", "Cuti"))
        _cmbStatus.Items.Add(New OptionItem("cuti_akademik", "Cuti Akademik"))
        _cmbStatus.Items.Add(New OptionItem("mangkir", "Mangkir"))

        _txtIps = Theme.MakeTextBox()
        _txtIpk = Theme.MakeTextBox()

        _form.AddField("Tahun Akademik", _cmbTahun)
        _form.AddField("Semester", _numSemester)
        _form.AddControl(_chkMengulang)
        _form.AddField("Status", _cmbStatus)
        _form.AddField("IPS (opsional)", _txtIps)
        _form.AddField("IPK (opsional)", _txtIpk)

        _btnTambah = Theme.PrimaryButton("Tambah")
        Dim btnUbah = Theme.SecondaryButton("Ubah")
        Dim btnHapus = Theme.DangerButton("Hapus")
        Dim btnReset = Theme.GhostButton("Reset")
        _form.Actions.Controls.Add(_btnTambah)
        _form.Actions.Controls.Add(btnUbah)
        _form.Actions.Controls.Add(btnHapus)
        _form.Actions.Controls.Add(btnReset)

        AddHandler _btnTambah.Click, AddressOf HandleTambah
        AddHandler btnUbah.Click, AddressOf HandleUbah
        AddHandler btnHapus.Click, AddressOf HandleHapus
        AddHandler btnReset.Click, AddressOf HandleReset

        Theme.AddColumn(_dataCard.Grid, "Id", "ID", 10, False)
        Theme.AddColumn(_dataCard.Grid, "Periode", "Tahun Akademik", 36)
        Theme.AddColumn(_dataCard.Grid, "Semester", "Semester", 16)
        Theme.AddColumn(_dataCard.Grid, "Mengulang", "Mengulang", 18)
        Theme.AddColumn(_dataCard.Grid, "Status", "Status", 24)
        Theme.AddColumn(_dataCard.Grid, "Ips", "IPS", 14, True, "0.00")
        Theme.AddColumn(_dataCard.Grid, "Ipk", "IPK", 14, True, "0.00")

        _dataCard.AddAction("Lihat Nilai", AddressOf HandleOpenNilai)
        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged
        AddHandler _dataCard.Grid.CellDoubleClick, AddressOf HandleGridDoubleClick

        LoadAll()
    End Sub

    Private Shared Function FormatPeriode(item As TahunAkademik) As String
        If item Is Nothing Then Return "-"
        Return item.TahunMulai.ToString() & "/" & item.TahunSelesai.ToString() & " " & item.Semester
    End Function

    Private Shared Function StatusText(value As String) As String
        Select Case value
            Case "aktif"
                Return "Aktif"
            Case "cuti"
                Return "Cuti"
            Case "cuti_akademik"
                Return "Cuti Akademik"
            Case "mangkir"
                Return "Mangkir"
            Case Else
                Return value
        End Select
    End Function

    Private Sub LoadAll()
        Try
            _loading = True

            Dim tahunList = TahunAkademik.All()
            Dim tahunById = tahunList.ToDictionary(Function(x) x.Id)

            _cmbTahun.Items.Clear()
            For Each item In tahunList
                _cmbTahun.Items.Add(New ComboItem(item.Id, FormatPeriode(item)))
            Next

            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("Periode", GetType(String))
            table.Columns.Add("Semester", GetType(Integer))
            table.Columns.Add("Mengulang", GetType(String))
            table.Columns.Add("Status", GetType(String))
            table.Columns.Add("Ips", GetType(Decimal))
            table.Columns.Add("Ipk", GetType(Decimal))

            _riwayatById.Clear()

            For Each item In RiwayatStudi.All().Where(Function(x) x.Nim = _mahasiswa.Nim)
                Dim tahun As TahunAkademik = Nothing
                Dim periode As String = "-"
                If tahunById.TryGetValue(item.IdTahunAkademik, tahun) Then
                    periode = FormatPeriode(tahun)
                End If

                table.Rows.Add(item.Id,
                               periode,
                               item.Semester,
                               If(item.Mengulang, "Ya", "Tidak"),
                               StatusText(item.Status),
                               Theme.FormatNullable(item.Ips),
                               Theme.FormatNullable(item.Ipk))
                _riwayatById(item.Id) = item
            Next

            _dataCard.BindData(table)
            _dataCard.Grid.ClearSelection()

            Dim tersedia As Boolean = _cmbTahun.Items.Count > 0
            _cmbTahun.Enabled = tersedia
            _btnTambah.Enabled = tersedia
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        Finally
            _loading = False
            ResetForm()
        End Try
    End Sub

    Private Sub HandleSelectionChanged(sender As Object, e As EventArgs)
        If _loading Then Return
        Dim grid = _dataCard.Grid
        If grid.SelectedRows.Count = 0 Then Return

        Dim row = grid.SelectedRows(0)
        _selectedId = Theme.CellInt(row, "Id")

        Dim item As RiwayatStudi = Nothing
        If Not _riwayatById.TryGetValue(_selectedId.Value, item) Then Return

        Theme.SelectComboById(_cmbTahun, item.IdTahunAkademik)
        _numSemester.Value = Convert.ToDecimal(item.Semester)
        _chkMengulang.Checked = item.Mengulang
        Theme.SelectOptionByValue(_cmbStatus, item.Status)
        _txtIps.Text = If(item.Ips.HasValue, item.Ips.Value.ToString("0.00"), "")
        _txtIpk.Text = If(item.Ipk.HasValue, item.Ipk.Value.ToString("0.00"), "")
    End Sub

    Private Function ValidateForm(ByRef ips As Decimal?, ByRef ipk As Decimal?) As Boolean
        If Theme.SelectedComboId(_cmbTahun) Is Nothing Then
            MessageBox.Show(Me, "Tahun akademik wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If String.IsNullOrEmpty(Theme.SelectedOptionValue(_cmbStatus)) Then
            MessageBox.Show(Me, "Status wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If Not Theme.ParseNullableDecimal(Me, _txtIps.Text, "IPS", ips) Then Return False
        If Not Theme.ParseNullableDecimal(Me, _txtIpk.Text, "IPK", ipk) Then Return False
        If ips.HasValue AndAlso (ips.Value < 0D OrElse ips.Value > 4D) Then
            MessageBox.Show(Me, "IPS harus berada di antara 0.00 dan 4.00.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If ipk.HasValue AndAlso (ipk.Value < 0D OrElse ipk.Value > 4D) Then
            MessageBox.Show(Me, "IPK harus berada di antara 0.00 dan 4.00.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        Dim ips As Decimal? = Nothing
        Dim ipk As Decimal? = Nothing
        If Not ValidateForm(ips, ipk) Then Return

        Try
            Dim item As New RiwayatStudi With {
                .Nim = _mahasiswa.Nim,
                .IdTahunAkademik = Theme.SelectedComboId(_cmbTahun).Value,
                .Semester = Convert.ToInt32(_numSemester.Value),
                .Mengulang = _chkMengulang.Checked,
                .Status = Theme.SelectedOptionValue(_cmbStatus),
                .Ips = ips,
                .Ipk = ipk
            }
            item.Create()
            LoadAll()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleUbah(sender As Object, e As EventArgs)
        If Not _selectedId.HasValue Then
            MessageBox.Show(Me, "Pilih data pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim ips As Decimal? = Nothing
        Dim ipk As Decimal? = Nothing
        If Not ValidateForm(ips, ipk) Then Return

        Try
            Dim item As New RiwayatStudi With {
                .Id = _selectedId.Value,
                .Nim = _mahasiswa.Nim,
                .IdTahunAkademik = Theme.SelectedComboId(_cmbTahun).Value,
                .Semester = Convert.ToInt32(_numSemester.Value),
                .Mengulang = _chkMengulang.Checked,
                .Status = Theme.SelectedOptionValue(_cmbStatus),
                .Ips = ips,
                .Ipk = ipk
            }
            item.Update()
            LoadAll()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleHapus(sender As Object, e As EventArgs)
        If Not _selectedId.HasValue Then
            MessageBox.Show(Me, "Pilih data pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If Not Theme.Confirm(Me, "Hapus riwayat studi ini beserta seluruh nilainya?") Then Return

        Try
            RiwayatStudi.Delete(_selectedId.Value)
            LoadAll()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleReset(sender As Object, e As EventArgs)
        ResetForm()
    End Sub

    Private Sub HandleOpenNilai(sender As Object, e As EventArgs)
        OpenSelectedNilai()
    End Sub

    Private Sub HandleGridDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        OpenSelectedNilai()
    End Sub

    Private Sub OpenSelectedNilai()
        If Not _selectedId.HasValue Then
            MessageBox.Show(Me, "Pilih riwayat studi pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim item As RiwayatStudi = Nothing
        If Not _riwayatById.TryGetValue(_selectedId.Value, item) Then Return

        Dim periode As String = Theme.CellText(_dataCard.Grid.SelectedRows(0), "Periode")
        RaiseEvent OpenNilai(_mahasiswa, item, periode)
    End Sub

    Private Sub ResetForm()
        _selectedId = Nothing
        If _cmbTahun.Items.Count > 0 Then _cmbTahun.SelectedIndex = 0
        _numSemester.Value = 1
        _chkMengulang.Checked = False
        If _cmbStatus.Items.Count > 0 Then _cmbStatus.SelectedIndex = 0
        _txtIps.Clear()
        _txtIpk.Clear()
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
