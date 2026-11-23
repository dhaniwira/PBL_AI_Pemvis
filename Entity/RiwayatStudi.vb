Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel riwayat_studi.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class RiwayatStudi

    Public Property Id As Integer
    Public Property Nim As String
    Public Property IdTahunAkademik As Integer
    Public Property Semester As Integer
    Public Property Mengulang As Boolean
    Public Property Status As String
    Public Property Ips As Decimal?
    Public Property Ipk As Decimal?

    ''' <summary>Menampilkan semua data riwayat studi.</summary>
    Public Shared Function All() As List(Of RiwayatStudi)
        Return ConnectionHandler.GetList(Of RiwayatStudi)(
            "SELECT id, nim, id_tahun_akademik, semester, mengulang, status, ips, ipk " &
            "FROM riwayat_studi ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data riwayat studi berdasarkan primary key (id).</summary>
    Public Shared Function Find(id As Integer) As RiwayatStudi
        Return ConnectionHandler.GetSingle(Of RiwayatStudi)(
            "SELECT id, nim, id_tahun_akademik, semester, mengulang, status, ips, ipk " &
            "FROM riwayat_studi WHERE id = @id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@id", id))
    End Function

    ''' <summary>Menambahkan data riwayat studi baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO riwayat_studi (nim, id_tahun_akademik, semester, mengulang, status, ips, ipk) " &
            "VALUES (@nim, @id_tahun_akademik, @semester, @mengulang, @status, @ips, @ipk); " &
            "SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@nim", Nim),
            ConnectionHandler.Parameter("@id_tahun_akademik", IdTahunAkademik),
            ConnectionHandler.Parameter("@semester", Semester),
            ConnectionHandler.Parameter("@mengulang", Mengulang),
            ConnectionHandler.Parameter("@status", Status),
            ConnectionHandler.Parameter("@ips", Ips),
            ConnectionHandler.Parameter("@ipk", Ipk)))
        Return True
    End Function

    ''' <summary>Mengubah data riwayat studi berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE riwayat_studi SET nim = @nim, id_tahun_akademik = @id_tahun_akademik, " &
            "semester = @semester, mengulang = @mengulang, status = @status, ips = @ips, ipk = @ipk " &
            "WHERE id = @id",
            ConnectionHandler.Parameter("@nim", Nim),
            ConnectionHandler.Parameter("@id_tahun_akademik", IdTahunAkademik),
            ConnectionHandler.Parameter("@semester", Semester),
            ConnectionHandler.Parameter("@mengulang", Mengulang),
            ConnectionHandler.Parameter("@status", Status),
            ConnectionHandler.Parameter("@ips", Ips),
            ConnectionHandler.Parameter("@ipk", Ipk),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data riwayat studi berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM riwayat_studi WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As RiwayatStudi
        Return New RiwayatStudi With {
            .Id = Convert.ToInt32(reader("id")),
            .Nim = reader("nim").ToString(),
            .IdTahunAkademik = Convert.ToInt32(reader("id_tahun_akademik")),
            .Semester = Convert.ToInt32(reader("semester")),
            .Mengulang = Convert.ToBoolean(reader("mengulang")),
            .Status = reader("status").ToString(),
            .Ips = ConnectionHandler.ReadDecimal(reader, "ips"),
            .Ipk = ConnectionHandler.ReadDecimal(reader, "ipk")
        }
    End Function

End Class
