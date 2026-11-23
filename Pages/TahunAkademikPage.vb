''' <summary>Halaman CRUD data tahun akademik.</summary>
Public Class TahunAkademikPage
    Inherits PageBase

    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _numMulai As NumericUpDown
    Private ReadOnly _numSelesai As NumericUpDown
    Private ReadOnly _cmbSemester As ComboBox

    Private _selectedId As Integer?
    Private _loading As Boolean

    Public Sub New()
        MyBase.New("Tahun Akademik", "Kelola periode tahun akademik dan semesternya.")

        Dim layout = CreateSplitLayout(380)
        _form = New FormCard("Form Tahun Akademik", "Isi data, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Daftar Tahun Akademik")

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _numMulai = Theme.MakeNumeric(2000, 2100, 2025)
        _numSelesai = Theme.MakeNumeric(2000, 2100, 2026)
        _cmbSemester = Theme.MakeComboBox()
        _cmbSemester.Items.Add("GANJIL")
        _cmbSemester.Items.Add("GENAP")

        _form.AddField("Tahun Mulai", _numMulai)
        _form.AddField("Tahun Selesai", _numSelesai)
        _form.AddField("Semester", _cmbSemester)

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

        Theme.AddColumn(_dataCard.Grid, "Id", "ID", 15, False)
        Theme.AddColumn(_dataCard.Grid, "Periode", "Periode", 45)
        Theme.AddColumn(_dataCard.Grid, "Semester", "Semester", 25)
        Theme.AddColumn(_dataCard.Grid, "Mulai", "Tahun Mulai", 20, False)
        Theme.AddColumn(_dataCard.Grid, "Selesai", "Tahun Selesai", 20, False)

        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged

        LoadData()
    End Sub

    Private Sub LoadData()
        Try
            _loading = True
            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("Periode", GetType(String))
            table.Columns.Add("Semester", GetType(String))
            table.Columns.Add("Mulai", GetType(Integer))
            table.Columns.Add("Selesai", GetType(Integer))

            For Each item In TahunAkademik.All()
                table.Rows.Add(item.Id,
                               item.TahunMulai.ToString() & "/" & item.TahunSelesai.ToString(),
                               item.Semester,
                               item.TahunMulai,
                               item.TahunSelesai)
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
        _numMulai.Value = Convert.ToDecimal(row.Cells("Mulai").Value)
        _numSelesai.Value = Convert.ToDecimal(row.Cells("Selesai").Value)
        _cmbSemester.SelectedItem = Theme.CellText(row, "Semester")
    End Sub

    Private Function ValidateForm() As Boolean
        If _cmbSemester.SelectedIndex < 0 Then
            MessageBox.Show(Me, "Semester wajib dipilih.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        If _numSelesai.Value <= _numMulai.Value Then
            MessageBox.Show(Me, "Tahun selesai harus lebih besar dari tahun mulai.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        If Not ValidateForm() Then Return

        Try
            Dim item As New TahunAkademik With {
                .TahunMulai = Convert.ToInt32(_numMulai.Value),
                .TahunSelesai = Convert.ToInt32(_numSelesai.Value),
                .Semester = _cmbSemester.SelectedItem.ToString()
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
            Dim item As New TahunAkademik With {
                .Id = _selectedId.Value,
                .TahunMulai = Convert.ToInt32(_numMulai.Value),
                .TahunSelesai = Convert.ToInt32(_numSelesai.Value),
                .Semester = _cmbSemester.SelectedItem.ToString()
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
        If Not Theme.Confirm(Me, "Hapus data tahun akademik ini?") Then Return

        Try
            TahunAkademik.Delete(_selectedId.Value)
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
        _numMulai.Value = 2025
        _numSelesai.Value = 2026
        _cmbSemester.SelectedIndex = 0
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
