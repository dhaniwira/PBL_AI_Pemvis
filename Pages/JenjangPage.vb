''' <summary>Halaman CRUD data jenjang.</summary>
Public Class JenjangPage
    Inherits PageBase

    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _txtKode As TextBox
    Private ReadOnly _numNormal As NumericUpDown
    Private ReadOnly _numMaksimal As NumericUpDown

    Private _selectedId As Integer?
    Private _loading As Boolean

    Public Sub New()
        MyBase.New("Jenjang", "Kelola data jenjang pendidikan beserta masa studinya.")

        Dim layout = CreateSplitLayout(380)
        _form = New FormCard("Form Jenjang", "Isi data, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Daftar Jenjang")

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _txtKode = Theme.MakeTextBox()
        _numNormal = Theme.MakeNumeric(1, 30, 6)
        _numMaksimal = Theme.MakeNumeric(1, 30, 10)

        _form.AddField("Kode Jenjang", _txtKode)
        _form.AddField("Masa Studi Normal (semester)", _numNormal)
        _form.AddField("Masa Studi Maksimal (semester)", _numMaksimal)

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

        Theme.AddColumn(_dataCard.Grid, "Id", "ID", 20, False)
        Theme.AddColumn(_dataCard.Grid, "Kode", "Kode", 25)
        Theme.AddColumn(_dataCard.Grid, "Normal", "Masa Studi Normal", 40)
        Theme.AddColumn(_dataCard.Grid, "Maksimal", "Masa Studi Maksimal", 40)

        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged

        LoadData()
    End Sub

    Private Sub LoadData()
        Try
            _loading = True
            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("Kode", GetType(String))
            table.Columns.Add("Normal", GetType(Integer))
            table.Columns.Add("Maksimal", GetType(Integer))

            For Each item In Jenjang.All()
                table.Rows.Add(item.Id, item.Kode, item.MasaStudiNormal, item.MasaStudiMaksimal)
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
        _numNormal.Value = Convert.ToDecimal(row.Cells("Normal").Value)
        _numMaksimal.Value = Convert.ToDecimal(row.Cells("Maksimal").Value)
    End Sub

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        If Not Theme.RequireText(Me, _txtKode.Text, "Kode jenjang") Then Return

        Try
            Dim item As New Jenjang With {
                .Kode = _txtKode.Text.Trim(),
                .MasaStudiNormal = Convert.ToInt32(_numNormal.Value),
                .MasaStudiMaksimal = Convert.ToInt32(_numMaksimal.Value)
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
        If Not Theme.RequireText(Me, _txtKode.Text, "Kode jenjang") Then Return

        Try
            Dim item As New Jenjang With {
                .Id = _selectedId.Value,
                .Kode = _txtKode.Text.Trim(),
                .MasaStudiNormal = Convert.ToInt32(_numNormal.Value),
                .MasaStudiMaksimal = Convert.ToInt32(_numMaksimal.Value)
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
        If Not Theme.Confirm(Me, "Hapus data jenjang " & _txtKode.Text & "?") Then Return

        Try
            Jenjang.Delete(_selectedId.Value)
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
        _numNormal.Value = 6
        _numMaksimal.Value = 10
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
