Imports Microsoft.Data.SqlClient

''' <summary>
''' Entity class untuk tabel mahasiswa.
''' Primary key tabel ini adalah nim (bukan id).
''' Semua akses database melalui ConnectionHandler.
''' </summary>
Public Class Mahasiswa

    Public Property Nim As String
    Public Property IdProdi As Integer
    Public Property Nama As String
    Public Property Angkatan As Integer
    Public Property Kelas As String
    Public Property SemesterBerjalan As Integer
    Public Property MasaStudi As Integer

    ''' <summary>Menampilkan semua data mahasiswa.</summary>
    Public Shared Function All() As List(Of Mahasiswa)
        Return ConnectionHandler.GetList(Of Mahasiswa)(
            "SELECT nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi " &
            "FROM mahasiswa ORDER BY nim",
            AddressOf FromReader)
    End Function

    ''' <summary>Mencari data mahasiswa berdasarkan primary key (nim).</summary>
    Public Shared Function Find(nim As String) As Mahasiswa
        Return ConnectionHandler.GetSingle(Of Mahasiswa)(
            "SELECT nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi " &
            "FROM mahasiswa WHERE nim = @nim",
            AddressOf FromReader,
            ConnectionHandler.Parameter("@nim", nim))
    End Function

    ''' <summary>Menambahkan data mahasiswa baru (nim diisi manual, bukan IDENTITY).</summary>
    Public Function Create() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "INSERT INTO mahasiswa (nim, id_prodi, nama, angkatan, kelas, semester_berjalan, masa_studi) " &
            "VALUES (@nim, @id_prodi, @nama, @angkatan, @kelas, @semester_berjalan, @masa_studi)",
            ConnectionHandler.Parameter("@nim", Nim),
            ConnectionHandler.Parameter("@id_prodi", IdProdi),
            ConnectionHandler.Parameter("@nama", Nama),
            ConnectionHandler.Parameter("@angkatan", Angkatan),
            ConnectionHandler.Parameter("@kelas", Kelas),
            ConnectionHandler.Parameter("@semester_berjalan", SemesterBerjalan),
            ConnectionHandler.Parameter("@masa_studi", MasaStudi)) > 0
    End Function

    ''' <summary>Mengubah data mahasiswa berdasarkan primary key (nim).</summary>
    Public Function Update() As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "UPDATE mahasiswa SET id_prodi = @id_prodi, nama = @nama, angkatan = @angkatan, " &
            "kelas = @kelas, semester_berjalan = @semester_berjalan, masa_studi = @masa_studi " &
            "WHERE nim = @nim",
            ConnectionHandler.Parameter("@id_prodi", IdProdi),
            ConnectionHandler.Parameter("@nama", Nama),
            ConnectionHandler.Parameter("@angkatan", Angkatan),
            ConnectionHandler.Parameter("@kelas", Kelas),
            ConnectionHandler.Parameter("@semester_berjalan", SemesterBerjalan),
            ConnectionHandler.Parameter("@masa_studi", MasaStudi),
            ConnectionHandler.Parameter("@nim", Nim)) > 0
    End Function

    ''' <summary>Menghapus data mahasiswa berdasarkan primary key (nim).</summary>
    Public Shared Function Delete(nim As String) As Boolean
        Return ConnectionHandler.ExecuteNonQuery(
            "DELETE FROM mahasiswa WHERE nim = @nim",
            ConnectionHandler.Parameter("@nim", nim)) > 0
    End Function

    Private Shared Function FromReader(reader As SqlDataReader) As Mahasiswa
        Return New Mahasiswa With {
            .Nim = reader("nim").ToString(),
            .IdProdi = Convert.ToInt32(reader("id_prodi")),
            .Nama = reader("nama").ToString(),
            .Angkatan = Convert.ToInt32(reader("angkatan")),
            .Kelas = reader("kelas").ToString(),
            .SemesterBerjalan = Convert.ToInt32(reader("semester_berjalan")),
            .MasaStudi = Convert.ToInt32(reader("masa_studi"))
        }
    End Function

End Class
