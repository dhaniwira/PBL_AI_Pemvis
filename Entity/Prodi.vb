Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel prodi.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class Prodi

    Public Property Id As Integer
    Public Property IdJurusan As Integer
    Public Property IdJenjang As Integer
    Public Property Nama As String

    ''' <summary>Menampilkan semua data prodi.</summary>
    Public Shared Function All() As List(Of Prodi)
        Return ConnectionHandler.GetList(Of Prodi)(
            "SELECT id, id_jurusan, id_jenjang, nama FROM prodi ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data prodi berdasarkan primary key (id).</summary>
    Public Shared Function Find(id As Integer) As Prodi
        Return ConnectionHandler.GetSingle(Of Prodi)(
            "SELECT id, id_jurusan, id_jenjang, nama FROM prodi WHERE id = @id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id", id))
    End Function

    ''' <summary>Menambahkan data prodi baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO prodi (id_jurusan, id_jenjang, nama) " &
            "VALUES (@id_jurusan, @id_jenjang, @nama); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@id_jurusan", IdJurusan),
            ConnectionHandler.Parameter("@id_jenjang", IdJenjang),
            ConnectionHandler.Parameter("@nama", Nama)))
        Return True
    End Function

    ''' <summary>Mengubah data prodi berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE prodi SET id_jurusan = @id_jurusan, id_jenjang = @id_jenjang, nama = @nama " &
            "WHERE id = @id",
            ConnectionHandler.Parameter("@id_jurusan", IdJurusan),
            ConnectionHandler.Parameter("@id_jenjang", IdJenjang),
            ConnectionHandler.Parameter("@nama", Nama),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data prodi berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM prodi WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As Prodi
        Return New Prodi With {
            .Id = Convert.ToInt32(reader("id")),
            .IdJurusan = Convert.ToInt32(reader("id_jurusan")),
            .IdJenjang = Convert.ToInt32(reader("id_jenjang")),
            .Nama = reader("nama").ToString()
        }
    End Function

End Class
