Imports System.Drawing.Drawing2D

''' <summary>Panel putih dengan sudut membulat dan garis tepi halus.</summary>
Public Class RoundedPanel
    Inherits Panel

    Public Property CornerRadius As Integer = 12
    Public Property BorderColor As Color = Theme.Border

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        BackColor = Theme.Card
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        UpdateRegion()
    End Sub

    Private Sub UpdateRegion()
        If Width <= 0 OrElse Height <= 0 Then Return
        Using path = Theme.RoundedRect(New Rectangle(0, 0, Width, Height), CornerRadius)
            Dim oldRegion = Region
            Region = New Region(path)
            If oldRegion IsNot Nothing Then oldRegion.Dispose()
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        If Width <= 0 OrElse Height <= 0 Then Return

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using path = Theme.RoundedRect(New Rectangle(0, 0, Width - 1, Height - 1), CornerRadius)
            Using pen As New Pen(BorderColor)
                e.Graphics.DrawPath(pen, path)
            End Using
        End Using
    End Sub
End Class

''' <summary>Tombol flat dengan sudut membulat.</summary>
Public Class RoundedButton
    Inherits Button

    Public Property CornerRadius As Integer = 8

    Public Sub New()
        FlatStyle = FlatStyle.Flat
        FlatAppearance.BorderSize = 0
        UseVisualStyleBackColor = False
        TextAlign = ContentAlignment.MiddleCenter
        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If Width <= 0 OrElse Height <= 0 Then Return
        Using path = Theme.RoundedRect(New Rectangle(0, 0, Width, Height), CornerRadius)
            Dim oldRegion = Region
            Region = New Region(path)
            If oldRegion IsNot Nothing Then oldRegion.Dispose()
        End Using
    End Sub
End Class

''' <summary>Item ComboBox yang menyimpan Id dan teks tampilan.</summary>
Public Class ComboItem

    Public Property Id As Integer
    Public Property Text As String

    Public Sub New(id As Integer, text As String)
        Me.Id = id
        Me.Text = text
    End Sub

    Public Overrides Function ToString() As String
        Return Text
    End Function

End Class

''' <summary>Item ComboBox untuk pilihan teks (misalnya status) dengan nilai tersimpan.</summary>
Public Class OptionItem

    Public Property Value As String
    Public Property Text As String

    Public Sub New(value As String, text As String)
        Me.Value = value
        Me.Text = text
    End Sub

    Public Overrides Function ToString() As String
        Return Text
    End Function

End Class

''' <summary>Kartu form di sisi kiri halaman: judul, isian, dan tombol aksi.</summary>
Public Class FormCard
    Inherits RoundedPanel

    Public ReadOnly Property Body As TableLayoutPanel
    Public ReadOnly Property Actions As FlowLayoutPanel

    Public Sub New(title As String, subtitle As String)
        Dock = DockStyle.Fill
        Padding = New Padding(22, 20, 22, 18)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 4,
            .BackColor = Color.Transparent
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim lblTitle = Theme.MakeLabel(title, 13, FontStyle.Bold)
        lblTitle.Margin = New Padding(0, 0, 0, 2)
        root.Controls.Add(lblTitle, 0, 0)

        Dim lblSubtitle = Theme.MakeLabel(subtitle, 8.75)
        lblSubtitle.ForeColor = Theme.TextMuted
        lblSubtitle.Margin = New Padding(0, 0, 0, 12)
        root.Controls.Add(lblSubtitle, 0, 1)

        Body = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .AutoScroll = True,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }
        Body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.Controls.Add(Body, 0, 2)

        Actions = New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = True,
            .AutoSize = True,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0, 10, 0, 0)
        }
        root.Controls.Add(Actions, 0, 3)

        Controls.Add(root)
    End Sub

    ''' <summary>Menambahkan label + kontrol isian ke form.</summary>
    Public Sub AddField(caption As String, control As Control)
        Dim lbl = Theme.FieldLabel(caption)
        lbl.Margin = New Padding(0, 0, 0, 4)
        Body.RowCount += 1
        Body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Body.Controls.Add(lbl, 0, Body.RowCount - 1)

        control.Dock = DockStyle.Top
        control.Margin = New Padding(0, 0, 0, 12)
        Body.RowCount += 1
        Body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Body.Controls.Add(control, 0, Body.RowCount - 1)
    End Sub

    ''' <summary>Menambahkan kontrol tanpa label (misalnya CheckBox).</summary>
    Public Sub AddControl(control As Control)
        control.Dock = DockStyle.Top
        control.Margin = New Padding(0, 0, 0, 12)
        Body.RowCount += 1
        Body.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Body.Controls.Add(control, 0, Body.RowCount - 1)
    End Sub

End Class

''' <summary>Kartu data di sisi kanan halaman: judul, jumlah data, tombol aksi, dan grid.</summary>
Public Class DataGridCard
    Inherits RoundedPanel

    Public ReadOnly Property Grid As DataGridView
    Public ReadOnly Property CountLabel As Label
    Public ReadOnly Property ActionButton As RoundedButton

    Public Sub New(title As String)
        Dock = DockStyle.Fill
        Padding = New Padding(18, 14, 18, 18)

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .BackColor = Color.Transparent
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim header As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))

        Dim left As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .BackColor = Color.Transparent,
            .Margin = New Padding(0)
        }

        Dim lblTitle = Theme.MakeLabel(title, 13, FontStyle.Bold)
        lblTitle.Margin = New Padding(0, 5, 10, 0)
        left.Controls.Add(lblTitle)

        CountLabel = Theme.MakeLabel("0 data", 8.75)
        CountLabel.ForeColor = Theme.TextMuted
        CountLabel.Margin = New Padding(0, 9, 0, 0)
        left.Controls.Add(CountLabel)

        header.Controls.Add(left, 0, 0)

        ActionButton = Theme.PrimaryButton("Detail")
        ActionButton.Visible = False
        ActionButton.Size = New Size(130, 34)
        ActionButton.Margin = New Padding(8, 0, 0, 0)
        header.Controls.Add(ActionButton, 1, 0)

        root.Controls.Add(header, 0, 0)

        Grid = New DataGridView()
        Theme.StyleGrid(Grid)
        Grid.Dock = DockStyle.Fill
        Grid.Margin = New Padding(0, 6, 0, 0)
        root.Controls.Add(Grid, 0, 1)

        Controls.Add(root)
    End Sub

    Public Sub AddAction(text As String, handler As EventHandler)
        ActionButton.Text = text
        ActionButton.Visible = True
        AddHandler ActionButton.Click, handler
    End Sub

    Public Sub BindData(table As DataTable)
        Grid.DataSource = table
        CountLabel.Text = table.Rows.Count.ToString() & " data"
    End Sub

End Class
