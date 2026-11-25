Imports System.Data
Imports Microsoft.Data.SqlClient

''' <summary>
''' Connection handler terpusat untuk database SQL Server LocalDB (Database.mdf).
''' Semua entity class wajib memakai class ini untuk mengakses database,
''' sehingga connection string dan cara eksekusi query hanya ada di satu tempat.
''' </summary>
Public Class ConnectionHandler

    ' Aplikasi VB dan inferensi Python memakai Database.mdf yang sama di root proyek.
    ' Path ..\.. mengarah dari bin\Debug (atau bin\Release) ke root proyek.
    ' Jangan menyalin Database.mdf ke folder output agar tidak ada dua database
    ' dengan nama logis yang sama.
    Public Shared ReadOnly ConnectionString As String =
        "Data Source=(LocalDB)\MSSQLLocalDB;" &
        "AttachDbFilename=" & IO.Path.GetFullPath(IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\..\Database.mdf")) & ";" &
        "Database=Database;" &
        "Integrated Security=True;TrustServerCertificate=True"

    ''' <summary>Membuka koneksi baru ke database dan mengembalikannya dalam keadaan terbuka.</summary>
    Public Shared Function OpenConnection() As SqlConnection
        Dim conn As New SqlConnection(ConnectionString)
        conn.Open()
        Return conn
    End Function

    ''' <summary>
    ''' Membuat parameter SQL. Nilai Nothing otomatis diubah menjadi DBNull.Value
    ''' agar aman untuk kolom yang boleh NULL.
    ''' </summary>
    Public Shared Function Parameter(nama As String, nilai As Object) As SqlParameter
        Return New SqlParameter(nama, If(nilai Is Nothing, CObj(DBNull.Value), nilai))
    End Function

    ''' <summary>Menjalankan INSERT / UPDATE / DELETE. Mengembalikan jumlah baris yang terpengaruh.</summary>
    Public Shared Function ExecuteNonQuery(sql As String, ParamArray parameters() As SqlParameter) As Integer
        Using conn As SqlConnection = OpenConnection()
            Using cmd As SqlCommand = BuildCommand(conn, sql, parameters)
                Return cmd.ExecuteNonQuery()
            End Using
        End Using
    End Function

    ''' <summary>Menjalankan query yang menghasilkan satu nilai (mis. SCOPE_IDENTITY()).</summary>
    Public Shared Function ExecuteScalar(sql As String, ParamArray parameters() As SqlParameter) As Object
        Using conn As SqlConnection = OpenConnection()
            Using cmd As SqlCommand = BuildCommand(conn, sql, parameters)
                Return cmd.ExecuteScalar()
            End Using
        End Using
    End Function

    ''' <summary>Menjalankan SELECT dan memetakan setiap baris menjadi objek entity.</summary>
    Public Shared Function GetList(Of T)(sql As String,
                                         map As Func(Of SqlDataReader, T),
                                         ParamArray parameters() As SqlParameter) As List(Of T)
        Dim hasil As New List(Of T)()

        Using conn As SqlConnection = OpenConnection()
            Using cmd As SqlCommand = BuildCommand(conn, sql, parameters)
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    Do While reader.Read()
                        hasil.Add(map(reader))
                    Loop
                End Using
            End Using
        End Using

        Return hasil
    End Function

    ''' <summary>Menjalankan SELECT dan memetakan satu baris menjadi objek entity (Nothing bila tidak ada).</summary>
    Public Shared Function GetSingle(Of T)(sql As String,
                                           map As Func(Of SqlDataReader, T),
                                           ParamArray parameters() As SqlParameter) As T
        Using conn As SqlConnection = OpenConnection()
            Using cmd As SqlCommand = BuildCommand(conn, sql, parameters)
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Return map(reader)
                    End If
                End Using
            End Using
        End Using

        Return Nothing
    End Function

    ''' <summary>Membaca kolom DECIMAL yang boleh bernilai NULL.</summary>
    Public Shared Function ReadDecimal(reader As SqlDataReader, namaKolom As String) As Decimal?
        If reader.IsDBNull(reader.GetOrdinal(namaKolom)) Then
            Return Nothing
        End If
        Return Convert.ToDecimal(reader(namaKolom))
    End Function

    Private Shared Function BuildCommand(conn As SqlConnection,
                                         sql As String,
                                         parameters() As SqlParameter) As SqlCommand
        Dim cmd As New SqlCommand(sql, conn)

        If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
            cmd.Parameters.AddRange(parameters)
        End If

        Return cmd
    End Function

End Class
