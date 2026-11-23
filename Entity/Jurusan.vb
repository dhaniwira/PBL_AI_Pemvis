Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel jurusan.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class Jurusan

    Public Property Id As Integer
    Public Property Nama As String

    ''' <summary>Menampilkan semua data jurusan.</summary>
    Public Shared Function All() As List(Of Jurusan)
        Return ConnectionHandler.GetList(Of Jurusan)(
            "SELECT id, nama FROM jurusan ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data jurusan berdasarkan primary key (id).</summary>
    Public Shared Function Find(id As Integer) As Jurusan
        Return ConnectionHandler.GetSingle(Of Jurusan)(
            "SELECT id, nama FROM jurusan WHERE id = @id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id", id))
    End Function

    ''' <summary>Menambahkan data jurusan baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO jurusan (nama) VALUES (@nama); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@nama", Nama)))
        Return True
    End Function

    ''' <summary>Mengubah data jurusan berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE jurusan SET nama = @nama WHERE id = @id",
            ConnectionHandler.Parameter("@nama", Nama),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data jurusan berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM jurusan WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As Jurusan
        Return New Jurusan With {
            .Id = Convert.ToInt32(reader("id")),
            .Nama = reader("nama").ToString()
        }
    End Function

End Class
