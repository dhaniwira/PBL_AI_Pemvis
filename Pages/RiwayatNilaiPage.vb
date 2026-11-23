''' <summary>
''' Halaman detail nilai: daftar mata kuliah dan nilai pada satu riwayat studi.
''' </summary>
Public Class RiwayatNilaiPage
    Inherits PageBase

    Private ReadOnly _mahasiswa As Mahasiswa
    Private ReadOnly _riwayat As RiwayatStudi
    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _cmbMk As ComboBox
    Private ReadOnly _txtNilai As TextBox
    Private ReadOnly _numAlfa As NumericUpDown
    Private ReadOnly _numIzin As NumericUpDown
    Private ReadOnly _numSakit As NumericUpDown
    Private ReadOnly _numTelat As NumericUpDown
    Private ReadOnly _btnTambah As RoundedButton

    Private _selectedId As Integer?
    Private _selectedMappingId As Integer?
    Private _loading As Boolean
    Private _allMappings As New List(Of MkProdi)()
    Private _mkById As New Dictionary(Of Integer, MataKuliah)()

    Public Sub New(mahasiswa As Mahasiswa, riwayat As RiwayatStudi, periodeText As String)
        MyBase.New("Nilai Mata Kuliah",
                   mahasiswa.Nama & " (" & mahasiswa.Nim & ") - " & periodeText &
                   " - Semester " & riwayat.Semester & ".", True)
        _mahasiswa = mahasiswa
        _riwayat = riwayat

        Dim layout = CreateSplitLayout(420)
        _form = New FormCard("Form Nilai", "Pilih mata kuliah prodi, lalu isi nilainya.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Nilai " & mahasiswa.Nama)

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _cmbMk = Theme.MakeComboBox()
        _txtNilai = Theme.MakeTextBox()
        _numAlfa = Theme.MakeNumeric(0, 99, 0)
        _numIzin = Theme.MakeNumeric(0, 99, 0)
        _numSakit = Theme.MakeNumeric(0, 99, 0)
        _numTelat = Theme.MakeNumeric(0, 99, 0)

        _form.AddField("Mata Kuliah Prodi", _cmbMk)
        _form.AddField("Nilai Akhir (opsional)", _txtNilai)
        _form.AddField("Alfa", _numAlfa)
        _form.AddField("Izin", _numIzin)
        _form.AddField("Sakit", _numSakit)
        _form.AddField("Telat", _numTelat)

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

        Theme.AddColumn(_dataCard.Grid, "Id", "ID", 8, False)
        Theme.AddColumn(_dataCard.Grid, "IdMkProdi", "ID MK Prodi", 8, False)
        Theme.AddColumn(_dataCard.Grid, "Kode", "Kode", 16)
        Theme.AddColumn(_dataCard.Grid, "Nama", "Mata Kuliah", 46)
        Theme.AddColumn(_dataCard.Grid, "Sks", "SKS", 10)
        Theme.AddColumn(_dataCard.Grid, "Nilai", "Nilai", 14, True, "0.00")
        Theme.AddColumn(_dataCard.Grid, "Alfa", "Alfa", 12)
        Theme.AddColumn(_dataCard.Grid, "Izin", "Izin", 12)
        Theme.AddColumn(_dataCard.Grid, "Sakit", "Sakit", 12)
        Theme.AddColumn(_dataCard.Grid, "Telat", "Telat", 12)

        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged

        LoadAll()
    End Sub

    Private Sub LoadAll()
        Try
            _loading = True

            _allMappings = MkProdi.All().Where(Function(x) x.IdProdi = _mahasiswa.IdProdi).ToList()
            _mkById = MataKuliah.All().ToDictionary(Function(x) x.Id)
            Dim mappingById = _allMappings.ToDictionary(Function(x) x.Id)

            Dim details = DetailMk.Find(_mahasiswa.Nim).Where(Function(x) x.IdRiwayatStudi = _riwayat.Id).ToList()
            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("IdMkProdi", GetType(Integer))
            table.Columns.Add("Kode", GetType(String))
            table.Columns.Add("Nama", GetType(String))
            table.Columns.Add("Sks", GetType(Integer))
            table.Columns.Add("Nilai", GetType(Decimal))
            table.Columns.Add("Alfa", GetType(Integer))
            table.Columns.Add("Izin", GetType(Integer))
            table.Columns.Add("Sakit", GetType(Integer))
            table.Columns.Add("Telat", GetType(Integer))

            For Each detail In details
                Dim mapping As MkProdi = Nothing
                Dim mk As MataKuliah = Nothing

                If mappingById.TryGetValue(detail.IdMkProdi, mapping) AndAlso
                   _mkById.TryGetValue(mapping.IdMk, mk) Then
                    table.Rows.Add(detail.Id, detail.IdMkProdi, mk.Kode, mk.Nama, mk.Sks,
                                   Theme.FormatNullable(detail.NilaiAkhir),
                                   detail.Alfa, detail.Izin, detail.Sakit, detail.Telat)
                Else
                    table.Rows.Add(detail.Id, detail.IdMkProdi, "-", "(tidak ditemukan)", 0,
                                   Theme.FormatNullable(detail.NilaiAkhir),
                                   detail.Alfa, detail.Izin, detail.Sakit, detail.Telat)
                End If
            Next

            _dataCard.BindData(table)
            _dataCard.Grid.ClearSelection()
            LoadCombo(Nothing)
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        Finally
            _loading = False
            _selectedId = Nothing
            _selectedMappingId = Nothing
            _dataCard.Grid.ClearSelection()
            ResetForm()
        End Try
    End Sub

    Private Sub LoadCombo(selectedMappingId As Integer?)
        Dim used As New HashSet(Of Integer)()
        For Each row As DataGridViewRow In _dataCard.Grid.Rows
            Dim id = Theme.CellInt(row, "IdMkProdi")
            If Not selectedMappingId.HasValue OrElse id <> selectedMappingId.Value Then
                used.Add(id)
            End If
        Next

        Dim ordered = _allMappings.OrderBy(
            Function(m) If(_mkById.ContainsKey(m.IdMk), _mkById(m.IdMk).Kode, "")).ToList()

        _cmbMk.Items.Clear()
        For Each mapping In ordered
            If Not used.Contains(mapping.Id) Then
                Dim mk As MataKuliah = Nothing
                If _mkById.TryGetValue(mapping.IdMk, mk) Then
                    _cmbMk.Items.Add(New ComboItem(mapping.Id,
                        mk.Kode & " - " & mk.Nama & " (" & mk.Sks & " SKS)"))
                End If
            End If
        Next

        Dim tersedia As Boolean = _cmbMk.Items.Count > 0
        _cmbMk.Enabled = tersedia
        _btnTambah.Enabled = tersedia
        If tersedia Then _cmbMk.SelectedIndex = 0
    End Sub

    Private Sub HandleSelectionChanged(sender As Object, e As EventArgs)
        If _loading Then Return
        Dim grid = _dataCard.Grid
        If grid.SelectedRows.Count = 0 Then Return

        Dim row = grid.SelectedRows(0)
        _selectedId = Theme.CellInt(row, "Id")
        _selectedMappingId = Theme.CellInt(row, "IdMkProdi")

        _txtNilai.Text = If(row.Cells("Nilai").Value Is DBNull.Value, "",
                            Convert.ToString(row.Cells("Nilai").Value))
        _numAlfa.Value = Convert.ToDecimal(row.Cells("Alfa").Value)
        _numIzin.Value = Convert.ToDecimal(row.Cells("Izin").Value)
        _numSakit.Value = Convert.ToDecimal(row.Cells("Sakit").Value)
        _numTelat.Value = Convert.ToDecimal(row.Cells("Telat").Value)

        LoadCombo(_selectedMappingId)
        Theme.SelectComboById(_cmbMk, _selectedMappingId.Value)
    End Sub

    Private Function ValidateForm(ByRef nilai As Decimal?) As Boolean
        If Theme.SelectedComboId(_cmbMk) Is Nothing Then
            MessageBox.Show(Me, "Mata kuliah wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If Not Theme.ParseNullableDecimal(Me, _txtNilai.Text, "Nilai akhir", nilai) Then Return False
        If nilai.HasValue AndAlso (nilai.Value < 0D OrElse nilai.Value > 100D) Then
            MessageBox.Show(Me, "Nilai akhir harus berada di antara 0 dan 100.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Function MappingAlreadyUsed(idMkProdi As Integer) As Boolean
        For Each row As DataGridViewRow In _dataCard.Grid.Rows
            If Theme.CellInt(row, "IdMkProdi") = idMkProdi Then Return True
        Next
        Return False
    End Function

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        Dim nilai As Decimal? = Nothing
        If Not ValidateForm(nilai) Then Return

        Dim idMkProdi = Theme.SelectedComboId(_cmbMk).Value
        If MappingAlreadyUsed(idMkProdi) Then
            MessageBox.Show(Me, "Mata kuliah ini sudah memiliki nilai pada riwayat studi tersebut.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Dim item As New DetailMk With {
                .IdRiwayatStudi = _riwayat.Id,
                .IdMkProdi = idMkProdi,
                .NilaiAkhir = nilai,
                .Alfa = Convert.ToInt32(_numAlfa.Value),
                .Izin = Convert.ToInt32(_numIzin.Value),
                .Sakit = Convert.ToInt32(_numSakit.Value),
                .Telat = Convert.ToInt32(_numTelat.Value)
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

        Dim nilai As Decimal? = Nothing
        If Not ValidateForm(nilai) Then Return

        Try
            Dim item As New DetailMk With {
                .Id = _selectedId.Value,
                .IdRiwayatStudi = _riwayat.Id,
                .IdMkProdi = Theme.SelectedComboId(_cmbMk).Value,
                .NilaiAkhir = nilai,
                .Alfa = Convert.ToInt32(_numAlfa.Value),
                .Izin = Convert.ToInt32(_numIzin.Value),
                .Sakit = Convert.ToInt32(_numSakit.Value),
                .Telat = Convert.ToInt32(_numTelat.Value)
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

        Dim namaMk As String = Theme.CellText(_dataCard.Grid.SelectedRows(0), "Nama")
        If Not Theme.Confirm(Me, "Hapus nilai " & namaMk & "?") Then Return

        Try
            DetailMk.Delete(_selectedId.Value)
            LoadAll()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleReset(sender As Object, e As EventArgs)
        ResetForm()
    End Sub

    Private Sub ResetForm()
        _selectedId = Nothing
        _selectedMappingId = Nothing
        _txtNilai.Clear()
        _numAlfa.Value = 0
        _numIzin.Value = 0
        _numSakit.Value = 0
        _numTelat.Value = 0
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
            LoadCombo(Nothing)
        End If
    End Sub

End Class
