Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel detail_mk (detail pengambilan mata kuliah).
''' Catatan: Find() mencari berdasarkan id mahasiswa (nim), bukan primary key id.
''' Nim tidak ada di tabel detail_mk, sehingga pencarian dilakukan lewat
''' join ke tabel riwayat_studi.
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class DetailMk

    Public Property Id As Integer
    Public Property IdRiwayatStudi As Integer
    Public Property IdMkProdi As Integer
    Public Property NilaiAkhir As Decimal?
    Public Property Alfa As Integer
    Public Property Izin As Integer
    Public Property Sakit As Integer
    Public Property Telat As Integer

    ''' <summary>Menampilkan semua data detail pengambilan mata kuliah.</summary>
    Public Shared Function All() As List(Of DetailMk)
        Return ConnectionHandler.GetList(Of DetailMk)(
            "SELECT id, id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat " &
            "FROM detail_mk ORDER BY id",
            AddressOf FromReader)
    End Function

    ''' <summary>
    ''' Mencari data detail_mk berdasarkan id mahasiswa (nim).
    ''' Mengembalikan list karena satu mahasiswa punya banyak detail mata kuliah.
    ''' </summary>
    Public Shared Function Find(nim As String) As List(Of DetailMk)
        Return ConnectionHandler.GetList(Of DetailMk)(
            "SELECT d.id, d.id_riwayat_studi, d.id_mk_prodi, d.nilai_akhir, d.alfa, d.izin, d.sakit, d.telat " &
            "FROM detail_mk d " &
            "INNER JOIN riwayat_studi r ON r.id = d.id_riwayat_studi " &
            "WHERE r.nim = @nim ORDER BY d.id",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@nim", nim))
    End Function

    ''' <summary>Menambahkan data detail_mk baru. Id hasil IDENTITY diisi otomatis.</summary>
    Public Function Create() As Boolean
        Id = Convert.ToInt32(ConnectionHandler.ExecuteScalar(
            "INSERT INTO detail_mk (id_riwayat_studi, id_mk_prodi, nilai_akhir, alfa, izin, sakit, telat) " &
            "VALUES (@id_riwayat_studi, @id_mk_prodi, @nilai_akhir, @alfa, @izin, @sakit, @telat); " &
            "SELECT CAST(SCOPE_IDENTITY() AS INT);",
            ConnectionHandler.Parameter("@id_riwayat_studi", IdRiwayatStudi),
            ConnectionHandler.Parameter("@id_mk_prodi", IdMkProdi),
            ConnectionHandler.Parameter("@nilai_akhir", NilaiAkhir),
            ConnectionHandler.Parameter("@alfa", Alfa),
            ConnectionHandler.Parameter("@izin", Izin),
            ConnectionHandler.Parameter("@sakit", Sakit),
            ConnectionHandler.Parameter("@telat", Telat)))
        Return True
    End Function

    ''' <summary>Mengubah data detail_mk berdasarkan primary key (id).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE detail_mk SET id_riwayat_studi = @id_riwayat_studi, id_mk_prodi = @id_mk_prodi, " &
            "nilai_akhir = @nilai_akhir, alfa = @alfa, izin = @izin, sakit = @sakit, telat = @telat " &
            "WHERE id = @id",
            ConnectionHandler.Parameter("@id_riwayat_studi", IdRiwayatStudi),
            ConnectionHandler.Parameter("@id_mk_prodi", IdMkProdi),
            ConnectionHandler.Parameter("@nilai_akhir", NilaiAkhir),
            ConnectionHandler.Parameter("@alfa", Alfa),
            ConnectionHandler.Parameter("@izin", Izin),
            ConnectionHandler.Parameter("@sakit", Sakit),
            ConnectionHandler.Parameter("@telat", Telat),
            ConnectionHandler.Parameter("@id", Id)) > 0
    End Function

    ''' <summary>Menghapus data detail_mk berdasarkan primary key (id).</summary>
    Public Shared Function Delete(id As Integer) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM detail_mk WHERE id = @id",
            ConnectionHandler.Parameter("@id", id)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As DetailMk
        Return New DetailMk With {
            .Id = Convert.ToInt32(reader("id")),
            .IdRiwayatStudi = Convert.ToInt32(reader("id_riwayat_studi")),
            .IdMkProdi = Convert.ToInt32(reader("id_mk_prodi")),
            .NilaiAkhir = ConnectionHandler.ReadDecimal(reader, "nilai_akhir"),
            .Alfa = Convert.ToInt32(reader("alfa")),
            .Izin = Convert.ToInt32(reader("izin")),
            .Sakit = Convert.ToInt32(reader("sakit")),
            .Telat = Convert.ToInt32(reader("telat"))
        }
    End Function

End Class
