''' <summary>Halaman CRUD data program studi, dengan akses ke halaman mata kuliah prodi.</summary>
Public Class ProdiPage
    Inherits PageBase

    Public Event OpenProdiDetail(prodi As Prodi)

    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _txtNama As TextBox
    Private ReadOnly _cmbJurusan As ComboBox
    Private ReadOnly _cmbJenjang As ComboBox

    Private _selectedId As Integer?
    Private _loading As Boolean
    Private _jurusanList As New List(Of Jurusan)()
    Private _jenjangList As New List(Of Jenjang)()
    Private _prodiById As New Dictionary(Of Integer, Prodi)()

    Public Sub New()
        MyBase.New("Program Studi", "Kelola program studi dan pemetaan mata kuliahnya.")

        Dim layout = CreateSplitLayout(380)
        _form = New FormCard("Form Program Studi", "Isi data, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Daftar Program Studi")

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _txtNama = Theme.MakeTextBox()
        _cmbJurusan = Theme.MakeComboBox()
        _cmbJenjang = Theme.MakeComboBox()

        _form.AddField("Nama Program Studi", _txtNama)
        _form.AddField("Jurusan", _cmbJurusan)
        _form.AddField("Jenjang", _cmbJenjang)

        Dim btnTambah = Theme.PrimaryButton("Tambah")
        Dim btnUbah = Theme.SecondaryButton("Ubah")
        Dim btnHapus = Theme.DangerButton("Hapus")
        Dim btnReset = Theme.GhostButton("Reset")
        _form.Actions.Controls.Add(btnTambah)
        _form.Actions.Controls.Add(btnUbah)
        _form.Actions.Controls.Add(btnHapus)
        _form.Actions.Controls.Add(btnReset)

        AddHandler btnTambah.Click, AddressOf HandleTambah
        AddHandler btnUbah.Click, AddressOf HandleUbah
        AddHandler btnHapus.Click, AddressOf HandleHapus
        AddHandler btnReset.Click, AddressOf HandleReset

        Theme.AddColumn(_dataCard.Grid, "Id", "ID", 12, False)
        Theme.AddColumn(_dataCard.Grid, "Nama", "Nama Program Studi", 50)
        Theme.AddColumn(_dataCard.Grid, "Jurusan", "Jurusan", 40)
        Theme.AddColumn(_dataCard.Grid, "Jenjang", "Jenjang", 20)

        _dataCard.AddAction("Mata Kuliah", AddressOf HandleOpenDetail)
        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged
        AddHandler _dataCard.Grid.CellDoubleClick, AddressOf HandleGridDoubleClick

        LoadLookups()
        LoadData()
    End Sub

    Private Sub LoadLookups()
        Try
            _jurusanList = Jurusan.All()
            _jenjangList = Jenjang.All()

            _cmbJurusan.Items.Clear()
            For Each item In _jurusanList
                _cmbJurusan.Items.Add(New ComboItem(item.Id, item.Nama))
            Next

            _cmbJenjang.Items.Clear()
            For Each item In _jenjangList
                _cmbJenjang.Items.Add(New ComboItem(item.Id, item.Kode & " (" & item.MasaStudiNormal & " semester)"))
            Next
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub LoadData()
        Try
            _loading = True
            Dim jurusanById = _jurusanList.ToDictionary(Function(x) x.Id, Function(x) x.Nama)
            Dim jenjangById = _jenjangList.ToDictionary(Function(x) x.Id, Function(x) x.Kode)

            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("Nama", GetType(String))
            table.Columns.Add("Jurusan", GetType(String))
            table.Columns.Add("Jenjang", GetType(String))

            _prodiById.Clear()

            For Each item In Prodi.All()
                Dim namaJurusan As String = "-"
                Dim kodeJenjang As String = "-"
                If jurusanById.ContainsKey(item.IdJurusan) Then namaJurusan = jurusanById(item.IdJurusan)
                If jenjangById.ContainsKey(item.IdJenjang) Then kodeJenjang = jenjangById(item.IdJenjang)

                table.Rows.Add(item.Id, item.Nama, namaJurusan, kodeJenjang)
                _prodiById(item.Id) = item
            Next

            _dataCard.BindData(table)
            _dataCard.Grid.ClearSelection()
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
        _txtNama.Text = Theme.CellText(row, "Nama")

        Dim prodiItem As Prodi = Nothing
        If _prodiById.TryGetValue(_selectedId.Value, prodiItem) Then
            Theme.SelectComboById(_cmbJurusan, prodiItem.IdJurusan)
            Theme.SelectComboById(_cmbJenjang, prodiItem.IdJenjang)
        End If
    End Sub

    Private Function ValidateForm() As Boolean
        If Not Theme.RequireText(Me, _txtNama.Text, "Nama program studi") Then Return False
        If Theme.SelectedComboId(_cmbJurusan) Is Nothing Then
            MessageBox.Show(Me, "Jurusan wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If Theme.SelectedComboId(_cmbJenjang) Is Nothing Then
            MessageBox.Show(Me, "Jenjang wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        If Not ValidateForm() Then Return

        Try
            Dim item As New Prodi With {
                .Nama = _txtNama.Text.Trim(),
                .IdJurusan = Theme.SelectedComboId(_cmbJurusan).Value,
                .IdJenjang = Theme.SelectedComboId(_cmbJenjang).Value
            }
            item.Create()
            LoadData()
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
        If Not ValidateForm() Then Return

        Try
            Dim item As New Prodi With {
                .Id = _selectedId.Value,
                .Nama = _txtNama.Text.Trim(),
                .IdJurusan = Theme.SelectedComboId(_cmbJurusan).Value,
                .IdJenjang = Theme.SelectedComboId(_cmbJenjang).Value
            }
            item.Update()
            LoadData()
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
        If Not Theme.Confirm(Me, "Hapus program studi " & _txtNama.Text & "?") Then Return

        Try
            Prodi.Delete(_selectedId.Value)
            LoadData()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleReset(sender As Object, e As EventArgs)
        ResetForm()
    End Sub

    Private Sub HandleOpenDetail(sender As Object, e As EventArgs)
        OpenSelectedDetail()
    End Sub

    Private Sub HandleGridDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        OpenSelectedDetail()
    End Sub

    Private Sub OpenSelectedDetail()
        If Not _selectedId.HasValue Then
            MessageBox.Show(Me, "Pilih program studi pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim item As Prodi = Nothing
        If _prodiById.TryGetValue(_selectedId.Value, item) Then
            RaiseEvent OpenProdiDetail(item)
        End If
    End Sub

    Private Sub ResetForm()
        _selectedId = Nothing
        _txtNama.Clear()
        If _cmbJurusan.Items.Count > 0 Then _cmbJurusan.SelectedIndex = 0
        If _cmbJenjang.Items.Count > 0 Then _cmbJenjang.SelectedIndex = 0
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
