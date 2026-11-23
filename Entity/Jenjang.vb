Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel jenjang.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class Jenjang

    Public Property Id As Integer
    Public Property Kode As String
    Public Property MasaStudiNormal As Integer
    Public Property MasaStudiMaksimal As Integer

    ''' <summary>Menampilkan semua data jenjang.</summary>
    Public Shared Function All() As List(Of Jenjang)
        Return ConnectionHandler.GetList(Of Jenjang)(
            "SELECT id, kode, masa_studi_normal, masa_studi_maksimal FROM jenjang ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data jenjang berdasarkan primary key (id).</summary>
    Public Shared Function Find(id As Integer) As Jenjang
        Return ConnectionHandler.GetSingle(Of Jenjang)(
            "SELECT id, kode, masa_studi_normal, masa_studi_maksimal FROM jenjang WHERE id = @id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id", id))
    End Function

    ''' <summary>Menambahkan data jenjang baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO jenjang (kode, masa_studi_normal, masa_studi_maksimal) " &
            "VALUES (@kode, @normal, @maksimal); SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@kode", Kode),
            ConnectionHandler.Parameter("@normal", MasaStudiNormal),
            ConnectionHandler.Parameter("@maksimal", MasaStudiMaksimal)))
        Return True
    End Function

    ''' <summary>Mengubah data jenjang berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE jenjang SET kode = @kode, masa_studi_normal = @normal, " &
            "masa_studi_maksimal = @maksimal WHERE id = @id",
            ConnectionHandler.Parameter("@kode", Kode),
            ConnectionHandler.Parameter("@normal", MasaStudiNormal),
            ConnectionHandler.Parameter("@maksimal", MasaStudiMaksimal),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data jenjang berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM jenjang WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As Jenjang
        Return New Jenjang With {
            .Id = Convert.ToInt32(reader("id")),
            .Kode = reader("kode").ToString(),
            .MasaStudiNormal = Convert.ToInt32(reader("masa_studi_normal")),
            .MasaStudiMaksimal = Convert.ToInt32(reader("masa_studi_maksimal"))
        }
    End Function

End Class
