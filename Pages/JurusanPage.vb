''' <summary>Halaman CRUD data jurusan.</summary>
Public Class JurusanPage
    Inherits PageBase

    Private ReadOnly _form As FormCard
    Private ReadOnly _dataCard As DataGridCard
    Private ReadOnly _txtNama As TextBox

    Private _selectedId As Integer?
    Private _loading As Boolean

    Public Sub New()
        MyBase.New("Jurusan", "Kelola data jurusan yang menaungi program studi.")

        Dim layout = CreateSplitLayout(380)
        _form = New FormCard("Form Jurusan", "Isi nama jurusan, lalu klik Tambah atau Ubah.")
        _form.Margin = New Padding(0, 0, 16, 0)
        _dataCard = New DataGridCard("Daftar Jurusan")

        layout.Controls.Add(_form, 0, 0)
        layout.Controls.Add(_dataCard, 1, 0)
        ContentHost.Controls.Add(layout)

        _txtNama = Theme.MakeTextBox()
        _form.AddField("Nama Jurusan", _txtNama)

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
        Theme.AddColumn(_dataCard.Grid, "Nama", "Nama Jurusan", 100)

        AddHandler _dataCard.Grid.SelectionChanged, AddressOf HandleSelectionChanged

        LoadData()
    End Sub

    Private Sub LoadData()
        Try
            _loading = True
            Dim table As New DataTable()
            table.Columns.Add("Id", GetType(Integer))
            table.Columns.Add("Nama", GetType(String))

            For Each item In Jurusan.All()
                table.Rows.Add(item.Id, item.Nama)
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
    End Sub

    Private Sub HandleTambah(sender As Object, e As EventArgs)
        If Not Theme.RequireText(Me, _txtNama.Text, "Nama jurusan") Then Return

        Try
            Dim item As New Jurusan With {.Nama = _txtNama.Text.Trim()}
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
        If Not Theme.RequireText(Me, _txtNama.Text, "Nama jurusan") Then Return

        Try
            Dim item As New Jurusan With {
                .Id = _selectedId.Value,
                .Nama = _txtNama.Text.Trim()
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
        If Not Theme.Confirm(Me, "Hapus jurusan " & _txtNama.Text & "?") Then Return

        Try
            Jurusan.Delete(_selectedId.Value)
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
        _txtNama.Clear()
        If _dataCard IsNot Nothing Then
            _dataCard.Grid.ClearSelection()
        End If
    End Sub

End Class
