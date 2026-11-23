Imports System.Drawing.Drawing2D
Imports System.Globalization

''' <summary>
''' Tema dan helper tampilan yang dipakai seluruh halaman.
''' </summary>
Public Module Theme

    ' Palet warna
    Public ReadOnly Primary As Color = Color.FromArgb(37, 99, 235)
    Public ReadOnly PrimaryDark As Color = Color.FromArgb(29, 78, 216)
    Public ReadOnly Sidebar As Color = Color.FromArgb(15, 23, 42)
    Public ReadOnly SidebarHover As Color = Color.FromArgb(30, 41, 59)
    Public ReadOnly Background As Color = Color.FromArgb(243, 244, 246)
    Public ReadOnly Card As Color = Color.White
    Public ReadOnly Border As Color = Color.FromArgb(226, 232, 240)
    Public ReadOnly TextDark As Color = Color.FromArgb(15, 23, 42)
    Public ReadOnly TextMuted As Color = Color.FromArgb(100, 116, 139)
    Public ReadOnly Danger As Color = Color.FromArgb(220, 38, 38)
    Public ReadOnly DangerDark As Color = Color.FromArgb(185, 28, 28)
    Public ReadOnly Neutral As Color = Color.FromArgb(241, 245, 249)
    Public ReadOnly NeutralText As Color = Color.FromArgb(51, 65, 85)

    Public Function MakeFont(size As Single, Optional style As FontStyle = FontStyle.Regular) As Font
        Return New Font("Segoe UI", size, style, GraphicsUnit.Point)
    End Function

    Public Function RoundedRect(rect As Rectangle, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()

        If radius <= 0 Then
            path.AddRectangle(rect)
            Return path
        End If

        Dim d As Integer = radius * 2
        path.AddArc(rect.X, rect.Y, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Public Function MakeLabel(text As String, size As Single, Optional style As FontStyle = FontStyle.Regular) As Label
        Return New Label With {
            .Text = text,
            .AutoSize = True,
            .Font = MakeFont(size, style),
            .ForeColor = TextDark,
            .BackColor = Color.Transparent
        }
    End Function

    Public Function FieldLabel(text As String) As Label
        Dim lbl = MakeLabel(text, 8.75, FontStyle.Bold)
        lbl.ForeColor = TextMuted
        Return lbl
    End Function

    Public Function MakeTextBox() As TextBox
        Return New TextBox With {
            .Font = MakeFont(10),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.White,
            .ForeColor = TextDark
        }
    End Function

    Public Function MakeComboBox() As ComboBox
        Return New ComboBox With {
            .Font = MakeFont(10),
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.White,
            .ForeColor = TextDark,
            .IntegralHeight = False,
            .DropDownHeight = 240
        }
    End Function

    Public Function MakeNumeric(minimum As Decimal, maximum As Decimal, value As Decimal,
                                Optional decimals As Integer = 0) As NumericUpDown
        Return New NumericUpDown With {
            .Font = MakeFont(10),
            .Minimum = minimum,
            .Maximum = maximum,
            .Value = value,
            .DecimalPlaces = decimals,
            .ThousandsSeparator = False,
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.White,
            .ForeColor = TextDark
        }
    End Function

    Public Function MakeCheckBox(text As String) As CheckBox
        Return New CheckBox With {
            .Text = text,
            .AutoSize = True,
            .Font = MakeFont(9.5),
            .ForeColor = TextDark,
            .BackColor = Color.Transparent,
            .Cursor = Cursors.Hand
        }
    End Function

    Public Function MakeButton(text As String, backColor As Color, foreColor As Color, hoverColor As Color) As RoundedButton
        Dim btn As New RoundedButton With {
            .Text = text,
            .BackColor = backColor,
            .ForeColor = foreColor,
            .Size = New Size(72, 34),
            .Font = MakeFont(9, FontStyle.Bold),
            .Cursor = Cursors.Hand,
            .Margin = New Padding(0, 0, 6, 6)
        }
        btn.FlatAppearance.MouseOverBackColor = hoverColor
        btn.FlatAppearance.MouseDownBackColor = hoverColor
        Return btn
    End Function

    Public Function PrimaryButton(text As String) As RoundedButton
        Return MakeButton(text, Primary, Color.White, PrimaryDark)
    End Function

    Public Function SecondaryButton(text As String) As RoundedButton
        Return MakeButton(text, Neutral, NeutralText, Color.FromArgb(226, 232, 240))
    End Function

    Public Function DangerButton(text As String) As RoundedButton
        Return MakeButton(text, Danger, Color.White, DangerDark)
    End Function

    Public Function GhostButton(text As String) As RoundedButton
        Return MakeButton(text, Color.FromArgb(248, 250, 252), TextMuted, Color.FromArgb(241, 245, 249))
    End Function

    Public Sub StyleGrid(grid As DataGridView)
        grid.BorderStyle = BorderStyle.None
        grid.BackgroundColor = Color.White
        grid.GridColor = Color.FromArgb(241, 245, 249)
        grid.EnableHeadersVisualStyles = False

        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59)
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font = MakeFont(9, FontStyle.Bold)
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 41, 59)
        grid.ColumnHeadersDefaultCellStyle.Padding = New Padding(6, 0, 6, 0)
        grid.ColumnHeadersHeight = 40
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        grid.RowHeadersVisible = False
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.ReadOnly = True
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoGenerateColumns = False
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal

        grid.DefaultCellStyle.Font = MakeFont(9.5)
        grid.DefaultCellStyle.BackColor = Color.White
        grid.DefaultCellStyle.ForeColor = TextDark
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254)
        grid.DefaultCellStyle.SelectionForeColor = TextDark
        grid.DefaultCellStyle.Padding = New Padding(6, 0, 6, 0)
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251)
        grid.RowTemplate.Height = 34
    End Sub

    Public Function AddColumn(grid As DataGridView, name As String, header As String, weight As Single,
                              Optional visible As Boolean = True,
                              Optional format As String = Nothing) As DataGridViewTextBoxColumn
        Dim col As New DataGridViewTextBoxColumn With {
            .Name = name,
            .DataPropertyName = name,
            .HeaderText = header,
            .FillWeight = weight,
            .Visible = visible,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
        If format IsNot Nothing Then
            col.DefaultCellStyle.Format = format
        End If
        grid.Columns.Add(col)
        Return col
    End Function

    Public Function SelectedComboId(cmb As ComboBox) As Integer?
        Dim item = TryCast(cmb.SelectedItem, ComboItem)
        If item Is Nothing Then Return Nothing
        Return item.Id
    End Function

    Public Function SelectComboById(cmb As ComboBox, id As Integer) As Boolean
        For i As Integer = 0 To cmb.Items.Count - 1
            Dim item = TryCast(cmb.Items(i), ComboItem)
            If item IsNot Nothing AndAlso item.Id = id Then
                cmb.SelectedIndex = i
                Return True
            End If
        Next
        cmb.SelectedIndex = -1
        Return False
    End Function

    Public Function SelectedOptionValue(cmb As ComboBox) As String
        Dim item = TryCast(cmb.SelectedItem, OptionItem)
        If item IsNot Nothing Then Return item.Value
        If cmb.SelectedItem IsNot Nothing Then Return cmb.SelectedItem.ToString()
        Return Nothing
    End Function

    Public Function SelectOptionByValue(cmb As ComboBox, value As String) As Boolean
        For i As Integer = 0 To cmb.Items.Count - 1
            Dim item = TryCast(cmb.Items(i), OptionItem)
            If item IsNot Nothing AndAlso item.Value = value Then
                cmb.SelectedIndex = i
                Return True
            End If
        Next
        cmb.SelectedIndex = -1
        Return False
    End Function

    Public Function CellText(row As DataGridViewRow, columnName As String) As String
        Return Convert.ToString(row.Cells(columnName).Value)
    End Function

    Public Function CellInt(row As DataGridViewRow, columnName As String) As Integer
        Return Convert.ToInt32(row.Cells(columnName).Value)
    End Function

    Public Function Confirm(owner As IWin32Window, text As String) As Boolean
        Return MessageBox.Show(owner, text, "Konfirmasi",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes
    End Function

    Public Sub ShowError(owner As IWin32Window, ex As Exception)
        MessageBox.Show(owner, "Terjadi kesalahan saat mengakses database." & Environment.NewLine & ex.Message,
                        "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Public Function RequireText(owner As IWin32Window, value As String, fieldName As String) As Boolean
        If String.IsNullOrWhiteSpace(value) Then
            MessageBox.Show(owner, fieldName & " wajib diisi.", "Validasi",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Public Function ParseNullableDecimal(owner As IWin32Window, text As String,
                                         fieldName As String, ByRef result As Decimal?) As Boolean
        If String.IsNullOrWhiteSpace(text) Then
            result = Nothing
            Return True
        End If

        Dim teks = text.Trim()
        Dim parsed As Decimal

        If Decimal.TryParse(teks, NumberStyles.Number, CultureInfo.CurrentCulture, parsed) Then
            result = parsed
            Return True
        End If

        If Decimal.TryParse(teks, NumberStyles.Number, CultureInfo.InvariantCulture, parsed) Then
            result = parsed
            Return True
        End If

        Dim normal = teks.Replace(","c, "."c)
        If Decimal.TryParse(normal, NumberStyles.Number, CultureInfo.InvariantCulture, parsed) Then
            result = parsed
            Return True
        End If

        MessageBox.Show(owner, fieldName & " harus berupa angka yang valid.", "Validasi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Return False
    End Function

    Public Function FormatNullable(value As Decimal?) As Object
        If value.HasValue Then Return value.Value
        Return DBNull.Value
    End Function

End Module
