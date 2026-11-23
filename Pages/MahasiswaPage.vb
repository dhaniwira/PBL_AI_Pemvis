''' <summary>Halaman CRUD data mahasiswa, dengan akses ke riwayat studi.</summary>
Public Class MahasiswaPage
    Inherits PageBase

    Public Event OpenMahasiswaDetail(mahasiswa As Mahasiswa)

    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _txtNim As TextBox
    Private ReadOnly _txtNama As TextBox
    Private ReadOnly _cmbProdi As ComboBox
    Private ReadOnly _numAngkatan As NumericUpDown
    Private ReadOnly _txtKelas As TextBox
    Private ReadOnly _numSemester As NumericUpDown
    Private ReadOnly _numMasaStudi As NumericUpDown

    Private _selectedNim As String = Nothing
    Private _loading As Boolean
    Private _prodiList As New List(Of Prodi)()
    Private _mahasiswaByNim As New Dictionary(Of String, Mahasiswa)()

    Public Sub New()
        MyBase.New("Mahasiswa", "Kelola data mahasiswa, program studi, dan masa studinya.")

        Dim layout = CreateSplitLayout(400)
        _form = New FormCard("Form Mahasiswa", "Isi data, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Daftar Mahasiswa")

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _txtNim = Theme.MakeTextBox()
        _txtNama = Theme.MakeTextBox()
        _cmbProdi = Theme.MakeComboBox()
        _numAngkatan = Theme.MakeNumeric(2000, 2100, 2025)
        _txtKelas = Theme.MakeTextBox()
        _numSemester = Theme.MakeNumeric(1, 14, 1)
        _numMasaStudi = Theme.MakeNumeric(0, 20, 0)

        _form.AddField("NIM", _txtNim)
        _form.AddField("Nama Mahasiswa", _txtNama)
        _form.AddField("Program Studi", _cmbProdi)
        _form.AddField("Angkatan", _numAngkatan)
        _form.AddField("Kelas", _txtKelas)
        _form.AddField("Semester Berjalan", _numSemester)
        _form.AddField("Masa Studi", _numMasaStudi)

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

        Theme.AddColumn(_dataCard.Grid, "Nim", "NIM", 25)
        Theme.AddColumn(_dataCard.Grid, "Nama", "Nama Mahasiswa", 45)
        Theme.AddColumn(_dataCard.Grid, "Prodi", "Program Studi", 40)
        Theme.AddColumn(_dataCard.Grid, "Angkatan", "Angkatan", 18)
        Theme.AddColumn(_dataCard.Grid, "Kelas", "Kelas", 16)
        Theme.AddColumn(_dataCard.Grid, "Semester", "Semester", 16)
        Theme.AddColumn(_dataCard.Grid, "MasaStudi", "Masa Studi", 18)

        _dataCard.AddAction("Riwayat Studi", AddressOf HandleOpenDetail)
        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged
        AddHandler _dataCard.Grid.CellDoubleClick, AddressOf HandleGridDoubleClick

        LoadLookups()
        LoadData()
    End Sub

    Private Sub LoadLookups()
        Try
            _prodiList = Prodi.All()
            _cmbProdi.Items.Clear()
            For Each item In _prodiList
                _cmbProdi.Items.Add(New ComboItem(item.Id, item.Nama))
            Next
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub LoadData()
        Try
            _loading = True
            Dim prodiById = _prodiList.ToDictionary(Function(x) x.Id, Function(x) x.Nama)

            Dim table As New DataTable()
            table.Columns.Add("Nim", GetType(String))
            table.Columns.Add("Nama", GetType(String))
            table.Columns.Add("Prodi", GetType(String))
            table.Columns.Add("Angkatan", GetType(Integer))
            table.Columns.Add("Kelas", GetType(String))
            table.Columns.Add("Semester", GetType(Integer))
            table.Columns.Add("MasaStudi", GetType(Integer))

            _mahasiswaByNim.Clear()

            For Each item In Mahasiswa.All()
                Dim namaProdi As String = "-"
                If prodiById.ContainsKey(item.IdProdi) Then namaProdi = prodiById(item.IdProdi)

                table.Rows.Add(item.Nim, item.Nama, namaProdi, item.Angkatan,
                               item.Kelas, item.SemesterBerjalan, item.MasaStudi)
                _mahasiswaByNim(item.Nim) = item
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
        _selectedNim = Theme.CellText(row, "Nim")
        _txtNim.Text = _selectedNim
        _txtNim.ReadOnly = True
        _txtNim.BackColor = Color.FromArgb(248, 250, 252)
        _txtNama.Text = Theme.CellText(row, "Nama")
        _numAngkatan.Value = Convert.ToDecimal(row.Cells("Angkatan").Value)
        _txtKelas.Text = Theme.CellText(row, "Kelas")
        _numSemester.Value = Convert.ToDecimal(row.Cells("Semester").Value)
        _numMasaStudi.Value = Convert.ToDecimal(row.Cells("MasaStudi").Value)

        Dim item As Mahasiswa = Nothing
        If _mahasiswaByNim.TryGetValue(_selectedNim, item) Then
            Theme.SelectComboById(_cmbProdi, item.IdProdi)
        End If
    End Sub

    Private Function ValidateForm(isEdit As Boolean) As Boolean
        If Not isEdit AndAlso Not Theme.RequireText(Me, _txtNim.Text, "NIM") Then Return False
        If Not Theme.RequireText(Me, _txtNama.Text, "Nama mahasiswa") Then Return False
        If Not Theme.RequireText(Me, _txtKelas.Text, "Kelas") Then Return False
        If Theme.SelectedComboId(_cmbProdi) Is Nothing Then
            MessageBox.Show(Me, "Program studi wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        If Not ValidateForm(False) Then Return

        Try
            Dim item As New Mahasiswa With {
                .Nim = _txtNim.Text.Trim(),
                .Nama = _txtNama.Text.Trim(),
                .IdProdi = Theme.SelectedComboId(_cmbProdi).Value,
                .Angkatan = Convert.ToInt32(_numAngkatan.Value),
                .Kelas = _txtKelas.Text.Trim(),
                .SemesterBerjalan = Convert.ToInt32(_numSemester.Value),
                .MasaStudi = Convert.ToInt32(_numMasaStudi.Value)
            }
            item.Create()
            LoadData()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleUbah(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(_selectedNim) Then
            MessageBox.Show(Me, "Pilih data pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If Not ValidateForm(True) Then Return

        Try
            Dim item As New Mahasiswa With {
                .Nim = _selectedNim,
                .Nama = _txtNama.Text.Trim(),
                .IdProdi = Theme.SelectedComboId(_cmbProdi).Value,
                .Angkatan = Convert.ToInt32(_numAngkatan.Value),
                .Kelas = _txtKelas.Text.Trim(),
                .SemesterBerjalan = Convert.ToInt32(_numSemester.Value),
                .MasaStudi = Convert.ToInt32(_numMasaStudi.Value)
            }
            item.Update()
            LoadData()
        Catch ex As Exception
            Theme.ShowError(Me, ex)
        End Try
    End Sub

    Private Sub HandleHapus(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(_selectedNim) Then
            MessageBox.Show(Me, "Pilih data pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If Not Theme.Confirm(Me, "Hapus mahasiswa " & _txtNama.Text & " (" & _selectedNim & ")?") Then Return

        Try
            Mahasiswa.Delete(_selectedNim)
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
        If String.IsNullOrEmpty(_selectedNim) Then
            MessageBox.Show(Me, "Pilih mahasiswa pada tabel terlebih dahulu.", "Informasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim item As Mahasiswa = Nothing
        If _mahasiswaByNim.TryGetValue(_selectedNim, item) Then
            RaiseEvent OpenMahasiswaDetail(item)
        End If
    End Sub

    Private Sub ResetForm()
        _selectedNim = Nothing
        _txtNim.Clear()
        _txtNim.ReadOnly = False
        _txtNim.BackColor = Color.White
        _txtNama.Clear()
        _numAngkatan.Value = 2025
        _txtKelas.Clear()
        _numSemester.Value = 1
        _numMasaStudi.Value = 0
        If _cmbProdi.Items.Count > 0 Then _cmbProdi.SelectedIndex = 0
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
