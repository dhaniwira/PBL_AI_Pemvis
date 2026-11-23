Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel mata_kuliah.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class MataKuliah

    Public Property Id As Integer
    Public Property Kode As String
    Public Property Nama As String
    Public Property Semester As Integer
    Public Property Kategori As String
    Public Property Sks As Integer

    ''' <summary>Menampilkan semua data mata kuliah.</summary>
    Public Shared Function All() As List(Of MataKuliah)
        Return ConnectionHandler.GetList(Of MataKuliah)(
            "SELECT id, kode, nama, semester, kategori, sks FROM mata_kuliah ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data mata kuliah berdasarkan primary key (id).</summary>
    Public Shared Function Find(id As Integer) As MataKuliah
        Return ConnectionHandler.GetSingle(Of MataKuliah)(
            "SELECT id, kode, nama, semester, kategori, sks FROM mata_kuliah WHERE id = @id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id", id))
    End Function

    ''' <summary>Menambahkan data mata kuliah baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO mata_kuliah (kode, nama, semester, kategori, sks) " &
            "VALUES (@kode, @nama, @semester, @kategori, @sks); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@kode", Kode),
            ConnectionHandler.Parameter("@nama", Nama),
            ConnectionHandler.Parameter("@semester", Semester),
            ConnectionHandler.Parameter("@kategori", Kategori),
            ConnectionHandler.Parameter("@sks", Sks)))
        Return True
    End Function

    ''' <summary>Mengubah data mata kuliah berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE mata_kuliah SET kode = @kode, nama = @nama, semester = @semester, " &
            "kategori = @kategori, sks = @sks WHERE id = @id",
            ConnectionHandler.Parameter("@kode", Kode),
            ConnectionHandler.Parameter("@nama", Nama),
            ConnectionHandler.Parameter("@semester", Semester),
            ConnectionHandler.Parameter("@kategori", Kategori),
            ConnectionHandler.Parameter("@sks", Sks),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data mata kuliah berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM mata_kuliah WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As MataKuliah
        Return New MataKuliah With {
            .Id = Convert.ToInt32(reader("id")),
            .Kode = reader("kode").ToString(),
            .Nama = reader("nama").ToString(),
            .Semester = Convert.ToInt32(reader("semester")),
            .Kategori = reader("kategori").ToString(),
            .Sks = Convert.ToInt32(reader("sks"))
        }
    End Function

End Class
