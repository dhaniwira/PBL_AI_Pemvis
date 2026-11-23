''' <summary>
''' Dasar semua halaman: judul, subjudul, area konten, dan tombol kembali.
''' </summary>
Public MustInherit Class PageBase
    Inherits UserControl

    Public Event BackRequested()

    Protected ReadOnly Property ContentHost As Panel

    Public Sub New(title As String, subtitle As String, Optional showBack As Boolean = False)
        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        BackColor = Theme.Background
        Padding = New Padding(28, 18, 28, 22)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = Color.Transparent
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 74))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim header As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }

        Dim titleX As Integer = 0

        If showBack Then
            Dim btnBack = Theme.SecondaryButton("<-  Kembali")
            btnBack.Size = New Size(110, 34)
            btnBack.Location = New Point(0, 8)
            AddHandler btnBack.Click, Sub(sender, e) RaiseEvent BackRequested()
            header.Controls.Add(btnBack)
            titleX = 126
        End If

        Dim lblTitle = Theme.MakeLabel(title, 20, FontStyle.Bold)
        lblTitle.Location = New Point(titleX, 3)
        header.Controls.Add(lblTitle)

        Dim lblSubtitle = Theme.MakeLabel(subtitle, 9.25)
        lblSubtitle.ForeColor = Theme.TextMuted
        lblSubtitle.Location = New Point(titleX + 1, 40)
        header.Controls.Add(lblSubtitle)

        root.Controls.Add(header, 0, 0)

        ContentHost = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0, 8, 0, 0)
        }
        root.Controls.Add(ContentHost, 0, 1)

        Controls.Add(root)
    End Sub

    ''' <summary>Layout dua kolom: kartu form (lebar tetap) di kiri dan kartu data di kanan.</summary>
    Protected Function CreateSplitLayout(formWidth As Integer) As TableLayoutPanel
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = Color.Transparent
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, formWidth))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Return layout
    End Function

End Class
