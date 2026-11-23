Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel tahun_akademik.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class TahunAkademik

    Public Property Id As Integer
    Public Property TahunMulai As Integer
    Public Property TahunSelesai As Integer
    Public Property Semester As String

    ''' <summary>Menampilkan semua data tahun akademik.</summary>
    Public Shared Function All() As List(Of TahunAkademik)
        Return ConnectionHandler.GetList(Of TahunAkademik)(
            "SELECT id, tahun_mulai, tahun_selesai, semester FROM tahun_akademik ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data tahun akademik berdasarkan primary key (id).</summary>
    Public Shared Function Find(id As Integer) As TahunAkademik
        Return ConnectionHandler.GetSingle(Of TahunAkademik)(
            "SELECT id, tahun_mulai, tahun_selesai, semester FROM tahun_akademik WHERE id = @id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id", id))
    End Function

    ''' <summary>Menambahkan data tahun akademik baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO tahun_akademik (tahun_mulai, tahun_selesai, semester) " &
            "VALUES (@mulai, @selesai, @semester); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@mulai", TahunMulai),
            ConnectionHandler.Parameter("@selesai", TahunSelesai),
            ConnectionHandler.Parameter("@semester", Semester)))
        Return True
    End Function

    ''' <summary>Mengubah data tahun akademik berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE tahun_akademik SET tahun_mulai = @mulai, tahun_selesai = @selesai, " &
            "semester = @semester WHERE id = @id",
            ConnectionHandler.Parameter("@mulai", TahunMulai),
            ConnectionHandler.Parameter("@selesai", TahunSelesai),
            ConnectionHandler.Parameter("@semester", Semester),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data tahun akademik berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM tahun_akademik WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As TahunAkademik
        Return New TahunAkademik With {
            .Id = Convert.ToInt32(reader("id")),
            .TahunMulai = Convert.ToInt32(reader("tahun_mulai")),
            .TahunSelesai = Convert.ToInt32(reader("tahun_selesai")),
            .Semester = reader("semester").ToString()
        }
    End Function

End Class
