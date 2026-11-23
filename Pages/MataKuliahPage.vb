''' <summary>Halaman CRUD data mata kuliah.</summary>
Public Class MataKuliahPage
    Inherits PageBase

    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _txtKode As TextBox
    Private ReadOnly _txtNama As TextBox
    Private ReadOnly _numSemester As NumericUpDown
    Private ReadOnly _cmbKategori As ComboBox
    Private ReadOnly _numSks As NumericUpDown

    Private _selectedId As Integer?
    Private _loading As Boolean

    Public Sub New()
        MyBase.New("Mata Kuliah", "Kelola daftar mata kuliah beserta semester, kategori, dan SKS.")

        Dim layout = CreateSplitLayout(380)
        _form = New FormCard("Form Mata Kuliah", "Isi data, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Daftar Mata Kuliah")

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _txtKode = Theme.MakeTextBox()
        _txtNama = Theme.MakeTextBox()
        _numSemester = Theme.MakeNumeric(1, 8, 1)
        _cmbKategori = Theme.MakeComboBox()
        _cmbKategori.Items.Add("MPK")
        _cmbKategori.Items.Add("MKKU")
        _cmbKategori.Items.Add("UMUM")
        _numSks = Theme.MakeNumeric(1, 8, 2)

        _form.AddField("Kode Mata Kuliah", _txtKode)
        _form.AddField("Nama Mata Kuliah", _txtNama)
        _form.AddField("Semester", _numSemester)
        _form.AddField("Kategori", _cmbKategori)
        _form.AddField("SKS", _numSks)

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
        Theme.AddColumn(_dataCard.Grid, "Kode", "Kode", 20)
        Theme.AddColumn(_dataCard.Grid, "Nama", "Nama Mata Kuliah", 55)
        Theme.AddColumn(_dataCard.Grid, "Semester", "Semester", 18)
        Theme.AddColumn(_dataCard.Grid, "Kategori", "Kategori", 22)
        Theme.AddColumn(_dataCard.Grid, "Sks", "SKS", 12)

        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged

        LoadData()
    End Sub

    Private Sub LoadData()
        Try
            _loading = True
            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("Kode", GetType(String))
            table.Columns.Add("Nama", GetType(String))
            table.Columns.Add("Semester", GetType(Integer))
            table.Columns.Add("Kategori", GetType(String))
            table.Columns.Add("Sks", GetType(Integer))

            For Each item In MataKuliah.All()
                table.Rows.Add(item.Id, item.Kode, item.Nama, item.Semester, item.Kategori, item.Sks)
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
        _txtKode.Text = Theme.CellText(row, "Kode")
        _txtNama.Text = Theme.CellText(row, "Nama")
        _numSemester.Value = Convert.ToDecimal(row.Cells("Semester").Value)
        _cmbKategori.SelectedItem = Theme.CellText(row, "Kategori")
        _numSks.Value = Convert.ToDecimal(row.Cells("Sks").Value)
    End Sub

    Private Function ValidateForm() As Boolean
        If Not Theme.RequireText(Me, _txtKode.Text, "Kode mata kuliah") Then Return False
        If Not Theme.RequireText(Me, _txtNama.Text, "Nama mata kuliah") Then Return False
        If _cmbKategori.SelectedIndex < 0 Then
            MessageBox.Show(Me, "Kategori wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        If Not ValidateForm() Then Return

        Try
            Dim item As New MataKuliah With {
                .Kode = _txtKode.Text.Trim(),
                .Nama = _txtNama.Text.Trim(),
                .Semester = Convert.ToInt32(_numSemester.Value),
                .Kategori = _cmbKategori.SelectedItem.ToString(),
                .Sks = Convert.ToInt32(_numSks.Value)
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
            Dim item As New MataKuliah With {
                .Id = _selectedId.Value,
                .Kode = _txtKode.Text.Trim(),
                .Nama = _txtNama.Text.Trim(),
                .Semester = Convert.ToInt32(_numSemester.Value),
                .Kategori = _cmbKategori.SelectedItem.ToString(),
                .Sks = Convert.ToInt32(_numSks.Value)
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
        If Not Theme.Confirm(Me, "Hapus mata kuliah " & _txtNama.Text & "?") Then Return

        Try
            MataKuliah.Delete(_selectedId.Value)
            LoadData()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleReset(sender As Object, e As EventArgs)
        ResetForm()
    End Sub

    Private Sub ResetForm()
        _selectedId = Nothing
        _txtKode.Clear()
        _txtNama.Clear()
        _numSemester.Value = 1
        _cmbKategori.SelectedIndex = 0
        _numSks.Value = 2
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
