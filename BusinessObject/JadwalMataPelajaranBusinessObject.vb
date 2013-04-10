﻿Namespace BusinessObject
    Public Class JadwalMataPelajaranBusinessObject

        Public Shared Function ListJadwalMataPelajaran()
            Using entity As New SiakSmanEntities()
                Dim query = (From data In entity.JadwalParent.Include("MasterKelas") Order By data.TahunAjaran Descending, data.MasterKelas.NamaKelas, data.Hari).ToList()
                Return query.Select(Function(a) New With {a.ID, a.Hari, a.Jurusan, a.TahunAjaran, .KelasID = a.MasterKelas.ID, a.MasterKelas.NamaKelas}).ToList()
            End Using
        End Function

        Public Shared Sub Save(ByVal parent As JadwalParent, ByVal details As List(Of JadwalDetail))
            Using entity As New SiakSmanEntities()
                Validation(parent, details)
                Dim jparent = New JadwalParent
                jparent.Hari = parent.Hari
                jparent.Jurusan = parent.Jurusan
                jparent.MasterKelas = (From data In entity.MasterKelas Where data.ID = parent.MasterKelas.ID).FirstOrDefault()
                jparent.TahunAjaran = parent.TahunAjaran

                Dim jdetail = (From detail In details Select New JadwalDetail() With { _
                        .JamIndex = detail.JamIndex, _
                        .MasterGuru = (From data In entity.MasterGuru Where data.ID = detail.MasterGuru.ID).FirstOrDefault(), _
                        .MasterMataPelajaran = (From data In entity.MasterMataPelajaran Where data.ID = detail.MasterMataPelajaran.ID).FirstOrDefault() _
                        }).ToList()

                entity.Connection.Open()
                Using transaction = entity.Connection.BeginTransaction
                    entity.AddToJadwalParent(jparent)
                    For Each item As JadwalDetail In jdetail
                        jparent.JadwalDetail.Add(item)
                    Next
                    entity.SaveChanges()
                    transaction.Commit()
                    entity.Connection.Close()
                End Using
            End Using
        End Sub

        Private Shared Sub Validation(ByVal parent As JadwalParent, ByVal details As List(Of JadwalDetail))

            If parent.TahunAjaran < 2010 Then
                Throw New Exception("Tahun Ajaran minimum 2010")
            End If
            If parent.Hari.Length = 0 Then
                Throw New Exception("Hari harus diisi")
            End If
            If parent.Jurusan.Length = 0 Then
                Throw New Exception("Nama Jurusan harus diiisi")
            End If
            If parent.MasterKelas Is Nothing Then
                Throw New Exception("Nama Kelas harus diiisi")
            End If

            For Each detail As JadwalDetail In details
                If detail.JamIndex < 1 Then
                    Throw New Exception("Index Waktu Jadwal Pelajaran lebih kecil dari 1")
                End If
                If detail.MasterGuru Is Nothing Then
                    Throw New Exception("Data Guru harus diisi")
                End If
                If detail.MasterMataPelajaran Is Nothing Then
                    Throw New Exception("Mata Pelajaran harus diisi")
                End If
            Next
        End Sub

        Public Shared Function GetJadwalMataPelajaran(ByVal id As Integer) As JadwalParent
            Using entity As New SiakSmanEntities()
                Dim query = (From data In entity.JadwalParent.Include("JadwalDetail") Where data.ID = id).ToList().FirstOrDefault()
                If query Is Nothing Then
                    Throw New Exception("Jadwal Mata Pelajaran dengan ID " + id + " tidak ditemukan")
                End If
                query.MasterKelasReference.Load()
                For Each item As JadwalDetail In query.JadwalDetail
                    item.MasterGuruReference.Load()
                    item.MasterMataPelajaranReference.Load()
                Next
                Return query
            End Using
        End Function

        Public Shared Sub Update(ByVal parent As JadwalParent, ByVal details As List(Of JadwalDetail))
            Using entity As New SiakSmanEntities()
                Validation(parent, details)

                Dim query = (From data In entity.JadwalParent.Include("JadwalDetail") Where data.ID = parent.ID).ToList().FirstOrDefault()
                If query Is Nothing Then
                    Throw New Exception("Jadwal Mata Pelajaran dengan ID " + parent.ID + " tidak ditemukan")
                End If
                query.MasterKelasReference.Load()
                For Each item As JadwalDetail In query.JadwalDetail
                    item.MasterGuruReference.Load()
                    item.MasterMataPelajaranReference.Load()
                Next

                query.Hari = parent.Hari
                query.Jurusan = parent.Jurusan
                query.MasterKelas = (From data In entity.MasterKelas Where data.ID = parent.MasterKelas.ID).FirstOrDefault()
                query.TahunAjaran = parent.TahunAjaran

                For i As Integer = 1 To 11
                    For Each detail As JadwalDetail In From detail1 In query.JadwalDetail Where detail1.JamIndex = i
                        For Each tdetail As JadwalDetail In From tdetail1 In details Where tdetail1.JamIndex = detail.JamIndex
                            detail.MasterGuru = (From data In entity.MasterGuru Where data.ID = tdetail.MasterGuru.ID).FirstOrDefault()
                            detail.MasterMataPelajaran = (From data In entity.MasterMataPelajaran Where data.ID = tdetail.MasterMataPelajaran.ID).FirstOrDefault()
                        Next
                    Next
                Next

                entity.Connection.Open()
                Using transaction = entity.Connection.BeginTransaction

                    entity.SaveChanges()
                    transaction.Commit()
                    entity.Connection.Close()
                End Using
            End Using
        End Sub

        Public Shared Sub Delete(ByVal currentid As Integer)
            Using entity As New SiakSmanEntities()

                Dim query = (From data In entity.JadwalParent.Include("JadwalDetail") Where data.ID = currentid).First()
                If query Is Nothing Then
                    Throw New Exception("Jadwal Mata Pelajaran dengan ID " + currentid + " tidak ditemukan")
                End If
                'query.MasterKelasReference.Load()
                'For Each item As JadwalDetail In query.JadwalDetail
                '    item.MasterGuruReference.Load()
                '    item.MasterMataPelajaranReference.Load()
                'Next
                entity.Connection.Open()
                Dim i = query.JadwalDetail.Count
                Using transaction = entity.Connection.BeginTransaction
                    For j As Integer = 1 To i
                        entity.DeleteObject(query.JadwalDetail(0))
                    Next
                    entity.DeleteObject(query)
                    entity.SaveChanges()
                    transaction.Commit()
                    entity.Connection.Close()
                End Using
            End Using
        End Sub
    End Class
End Namespace