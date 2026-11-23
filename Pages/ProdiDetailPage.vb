''' <summary>
''' Halaman detail prodi: daftar mata kuliah yang dipetakan ke prodi tersebut.
''' </summary>
Public Class ProdiDetailPage
    Inherits PageBase

    Private ReadOnly _prodi As Prodi
    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _cmbMk As ComboBox
    Private ReadOnly _btnTambah As RoundedButton

    Private _selectedMappingId As Integer?
    Private _loading As Boolean
    Private _mkById As New Dictionary(Of Integer, MataKuliah)()

    Public Sub New(prodi As Prodi)
        MyBase.New("Mata Kuliah Prodi", "Daftar mata kuliah untuk program studi " & prodi.Nama & ".", True)
        _prodi = prodi

        Dim layout = CreateSplitLayout(380)
        _form = New FormCard("Tambah Mata Kuliah", "Pilih mata kuliah yang belum dipetakan ke prodi ini.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Mata Kuliah " & prodi.Nama)

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _cmbMk = Theme.MakeComboBox()
        _form.AddField("Mata Kuliah", _cmbMk)

        _btnTambah = Theme.PrimaryButton("Tambah")
        _btnTambah.Size = New Size(120, 34)
        Dim btnHapus = Theme.DangerButton("Hapus")
        btnHapus.Size = New Size(120, 34)
        _form.Actions.Controls.Add(_btnTambah)
        _form.Actions.Controls.Add(btnHapus)

        AddHandler _btnTambah.Click, AddressOf HandleTambah
        AddHandler btnHapus.Click, AddressOf HandleHapus

        Theme.AddColumn(_dataCard.Grid, "Id", "ID", 10, False)
        Theme.AddColumn(_dataCard.Grid, "IdMk", "ID MK", 10, False)
        Theme.AddColumn(_dataCard.Grid, "Kode", "Kode", 18)
        Theme.AddColumn(_dataCard.Grid, "Nama", "Mata Kuliah", 50)
        Theme.AddColumn(_dataCard.Grid, "Semester", "Semester", 16)
        Theme.AddColumn(_dataCard.Grid, "Kategori", "Kategori", 18)
        Theme.AddColumn(_dataCard.Grid, "Sks", "SKS", 10)

        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged

        LoadAll()
    End Sub

    Private Sub LoadAll()
        Try
            _loading = True

            Dim mkList = MataKuliah.All()
            _mkById = mkList.ToDictionary(Function(x) x.Id)

            Dim mappings = MkProdi.All().Where(Function(x) x.IdProdi = _prodi.Id).ToList()

            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("IdMk", GetType(Integer))
            table.Columns.Add("Kode", GetType(String))
            table.Columns.Add("Nama", GetType(String))
            table.Columns.Add("Semester", GetType(Integer))
            table.Columns.Add("Kategori", GetType(String))
            table.Columns.Add("Sks", GetType(Integer))

            For Each mapping In mappings
                Dim mk As MataKuliah = Nothing
                If _mkById.TryGetValue(mapping.IdMk, mk) Then
                    table.Rows.Add(mapping.Id, mapping.IdMk, mk.Kode, mk.Nama,
                                   mk.Semester, mk.Kategori, mk.Sks)
                Else
                    table.Rows.Add(mapping.Id, mapping.IdMk, "-", "(tidak ditemukan)", 0, "-", 0)
                End If
            Next

            _dataCard.BindData(table)
            _dataCard.Grid.ClearSelection()

            LoadCombo(mkList, mappings)
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        Finally
            _loading = False
            _selectedMappingId = Nothing
            _dataCard.Grid.ClearSelection()
        End Try
    End Sub

    Private Sub LoadCombo(mkList As List(Of MataKuliah), mappings As List(Of MkProdi))
        Dim used As New HashSet(Of Integer)()
        For Each mapping In mappings
            used.Add(mapping.IdMk)
        Next

        _cmbMk.Items.Clear()
        For Each mk In mkList.OrderBy(Function(x) x.Kode)
            If Not used.Contains(mk.Id) Then
                _cmbMk.Items.Add(New ComboItem(mk.Id,
                    mk.Kode & " - " & mk.Nama & " (" & mk.Sks & " sks)"))
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
        _selectedMappingId = Theme.CellInt(grid.SelectedRows(0), "Id")
    End Sub

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        Dim idMk = Theme.SelectedComboId(_cmbMk)
        If idMk Is Nothing Then
            MessageBox.Show(Me, "Pilih mata kuliah terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Try
            Dim item As New MkProdi With {
                .IdMk = idMk.Value,
                .IdProdi = _prodi.Id
            }
            item.Create()
            LoadAll()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleHapus(sender As Object, e As EventArgs)
        If Not _selectedMappingId.HasValue Then
            MessageBox.Show(Me, "Pilih mata kuliah pada tabel yang ingin dihapus dari prodi.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim namaMk As String = Theme.CellText(_dataCard.Grid.SelectedRows(0), "Nama")
        If Not Theme.Confirm(Me, "Hapus " & namaMk & " dari prodi " & _prodi.Nama & "?") Then Return

        Try
            MkProdi.Delete(_selectedMappingId.Value)
            LoadAll()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

End Class
