Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel mk_prodi (pemetaan mata kuliah ke prodi).
''' Catatan: Find() mencari berdasarkan id mata kuliah (id_mk), bukan primary key id,
''' karena satu mata kuliah bisa dipetakan ke beberapa prodi.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class MkProdi

    Public Property Id As Integer
    Public Property IdMk As Integer
    Public Property IdProdi As Integer

    ''' <summary>Menampilkan semua data pemetaan mata kuliah ke prodi.</summary>
    Public Shared Function All() As List(Of MkProdi)
        Return ConnectionHandler.GetList(Of MkProdi)(
            "SELECT id, id_mk, id_prodi FROM mk_prodi ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>
    ''' Mencari data mk_prodi berdasarkan id mata kuliah (id_mk).
    ''' Mengembalikan list karena hasilnya bisa lebih dari satu baris.
    ''' </summary>
    Public Shared Function Find(idMk As Integer) As List(Of MkProdi)
        Return ConnectionHandler.GetList(Of MkProdi)(
            "SELECT id, id_mk, id_prodi FROM mk_prodi WHERE id_mk = @id_mk ORDER BY id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id_mk", idMk))
    End Function

    ''' <summary>Menambahkan data pemetaan baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO mk_prodi (id_mk, id_prodi) VALUES (@id_mk, @id_prodi); " &
            "SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@id_mk", IdMk),
            ConnectionHandler.Parameter("@id_prodi", IdProdi)))
        Return True
    End Function

    ''' <summary>Mengubah data pemetaan berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE mk_prodi SET id_mk = @id_mk, id_prodi = @id_prodi WHERE id = @id",
            ConnectionHandler.Parameter("@id_mk", IdMk),
            ConnectionHandler.Parameter("@id_prodi", IdProdi),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data pemetaan berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM mk_prodi WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As MkProdi
        Return New MkProdi With {
            .Id = Convert.ToInt32(reader("id")),
            .IdMk = Convert.ToInt32(reader("id_mk")),
            .IdProdi = Convert.ToInt32(reader("id_prodi"))
        }
    End Function

End Class
