using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyTongHop.DTO;

namespace QuanLyTongHop.DAL
{
    public class LibraryDAL
    {
        private string ConnStr => DbHelper.GetConnStr("LibraryDB");

        public List<DocGiaDTO> GetDocGia()
        {
            var list = new List<DocGiaDTO>();
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM DocGia", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new DocGiaDTO
                {
                    MaDocGia = rd.GetInt32(0),
                    HoTen = rd.GetString(1),
                    Email = rd.IsDBNull(2) ? "" : rd.GetString(2),
                    NgayHetHanThe = rd.GetDateTime(3)
                });
            }
            return list;
        }

        public void ThemDocGia(DocGiaDTO dg)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("INSERT INTO DocGia (HoTen, Email, NgayHetHanThe) VALUES (@Ten, @Email, @Han)", conn);
            cmd.Parameters.AddWithValue("@Ten", dg.HoTen);
            cmd.Parameters.AddWithValue("@Email", dg.Email);
            cmd.Parameters.AddWithValue("@Han", dg.NgayHetHanThe);
            cmd.ExecuteNonQuery();
        }

        public List<DauSachDTO> GetDauSach()
        {
            var list = new List<DauSachDTO>();
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT MaSach, TenSach, TheLoai, SoLuongCon FROM DauSach", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new DauSachDTO
                {
                    MaSach = rd.GetInt32(0),
                    TenSach = rd.GetString(1),
                    TheLoai = rd.IsDBNull(2) ? "" : rd.GetString(2),
                    SoLuongCon = rd.GetInt32(3)
                });
            }
            return list;
        }

        public void ThemDauSach(DauSachDTO s)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("INSERT INTO DauSach (TenSach, TheLoai, SoLuongCon) VALUES (@Ten, @TL, @SL)", conn);
            cmd.Parameters.AddWithValue("@Ten", s.TenSach);
            cmd.Parameters.AddWithValue("@TL", s.TheLoai);
            cmd.Parameters.AddWithValue("@SL", s.SoLuongCon);
            cmd.ExecuteNonQuery();
        }

        public void XoaDauSach(int maSach)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM DauSach WHERE MaSach = @Ma", conn);
            cmd.Parameters.AddWithValue("@Ma", maSach);
            cmd.ExecuteNonQuery();
        }

        public DataTable GetPhieuMuonFiltered(int maDocGia, string trangThai)
        {
            using var conn = new SqlConnection(ConnStr);
            string sql = @"SELECT pm.MaPhieu, dg.HoTen AS DocGia, ds.TenSach, ct.SoLuong, pm.NgayMuon, pm.HanTra, pm.TrangThai
                           FROM PhieuMuon pm
                           JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia
                           JOIN ChiTietMuon ct ON pm.MaPhieu = ct.MaPhieu
                           JOIN DauSach ds ON ct.MaSach = ds.MaSach
                           WHERE (@MaDG = 0 OR pm.MaDocGia = @MaDG)
                             AND (@TT = N'Tất cả' OR pm.TrangThai = @TT)";
            using var da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.AddWithValue("@MaDG", maDocGia);
            da.SelectCommand.Parameters.AddWithValue("@TT", trangThai);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void LapPhieuMuon(int maDocGia, int maSach, int soLuong, DateTime hanTra)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                var cmdCheckThe = new SqlCommand("SELECT NgayHetHanThe FROM DocGia WHERE MaDocGia = @MaDG", conn, trans);
                cmdCheckThe.Parameters.AddWithValue("@MaDG", maDocGia);
                var hanThe = Convert.ToDateTime(cmdCheckThe.ExecuteScalar());
                if (hanThe < DateTime.Now)
                    throw new Exception("Thẻ độc giả đã hết hạn! Không thể lập phiếu mượn.");

                var cmdCheckSL = new SqlCommand("SELECT SoLuongCon FROM DauSach WHERE MaSach = @MaSach", conn, trans);
                cmdCheckSL.Parameters.AddWithValue("@MaSach", maSach);
                int slCon = Convert.ToInt32(cmdCheckSL.ExecuteScalar());
                if (slCon < soLuong)
                    throw new Exception($"Sách trong kho không đủ (Còn {slCon} cuốn)!");

                var cmdPM = new SqlCommand("INSERT INTO PhieuMuon (MaDocGia, NgayMuon, HanTra, TrangThai) OUTPUT INSERTED.MaPhieu VALUES (@MaDG, GETDATE(), @HanTra, N'Đang mượn')", conn, trans);
                cmdPM.Parameters.AddWithValue("@MaDG", maDocGia);
                cmdPM.Parameters.AddWithValue("@HanTra", hanTra);
                int maPhieu = Convert.ToInt32(cmdPM.ExecuteScalar());

                var cmdCT = new SqlCommand("INSERT INTO ChiTietMuon (MaPhieu, MaSach, SoLuong) VALUES (@MaP, @MaS, @SL)", conn, trans);
                cmdCT.Parameters.AddWithValue("@MaP", maPhieu);
                cmdCT.Parameters.AddWithValue("@MaS", maSach);
                cmdCT.Parameters.AddWithValue("@SL", soLuong);
                cmdCT.ExecuteNonQuery();

                var cmdUpd = new SqlCommand("UPDATE DauSach SET SoLuongCon = SoLuongCon - @SL WHERE MaSach = @MaS", conn, trans);
                cmdUpd.Parameters.AddWithValue("@SL", soLuong);
                cmdUpd.Parameters.AddWithValue("@MaS", maSach);
                cmdUpd.ExecuteNonQuery();

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public void TraSach(int maPhieu)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                var cmdGet = new SqlCommand("SELECT MaSach, SoLuong FROM ChiTietMuon WHERE MaPhieu = @MaP", conn, trans);
                cmdGet.Parameters.AddWithValue("@MaP", maPhieu);
                int maSach = 0, sl = 0;
                using (var rd = cmdGet.ExecuteReader())
                {
                    if (rd.Read()) { maSach = rd.GetInt32(0); sl = rd.GetInt32(1); }
                }

                var cmdUpdPM = new SqlCommand("UPDATE PhieuMuon SET TrangThai = N'Đã trả' WHERE MaPhieu = @MaP AND TrangThai != N'Đã trả'", conn, trans);
                cmdUpdPM.Parameters.AddWithValue("@MaP", maPhieu);
                int rows = cmdUpdPM.ExecuteNonQuery();
                if (rows == 0) throw new Exception("Phiếu này đã được trả trước đó!");

                var cmdUpdSach = new SqlCommand("UPDATE DauSach SET SoLuongCon = SoLuongCon + @SL WHERE MaSach = @MaS", conn, trans);
                cmdUpdSach.Parameters.AddWithValue("@SL", sl);
                cmdUpdSach.Parameters.AddWithValue("@MaS", maSach);
                cmdUpdSach.ExecuteNonQuery();

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // Dùng SqlDataReader cảnh báo quá hạn
        public DataTable GetCanhBaoQuaHanReader()
        {
            var dt = new DataTable();
            dt.Columns.Add("Mã Phiếu", typeof(int));
            dt.Columns.Add("Độc Giả", typeof(string));
            dt.Columns.Add("Hạn Trả", typeof(DateTime));
            dt.Columns.Add("Trạng Thái", typeof(string));

            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            string sql = @"SELECT pm.MaPhieu, dg.HoTen, pm.HanTra, pm.TrangThai
                           FROM PhieuMuon pm JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia
                           WHERE pm.HanTra < GETDATE() AND pm.TrangThai != N'Đã trả'";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dt.Rows.Add(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2), reader.GetString(3));
            }
            return dt;
        }

        // Thống kê GROUP BY + ExecuteScalar
        public (int TongLuotMuon, DataTable ChiTietTheLoai) ThongKeMuonTheoTheLoai()
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmdScalar = new SqlCommand("SELECT ISNULL(SUM(SoLuong), 0) FROM ChiTietMuon", conn);
            int tong = Convert.ToInt32(cmdScalar.ExecuteScalar());

            string sqlGroup = @"SELECT ds.TheLoai AS [Thể Loại], SUM(ct.SoLuong) AS [Tổng Lượt Mượn]
                                FROM ChiTietMuon ct JOIN DauSach ds ON ct.MaSach = ds.MaSach
                                GROUP BY ds.TheLoai";
            using var da = new SqlDataAdapter(sqlGroup, conn);
            var dt = new DataTable();
            da.Fill(dt);
            return (tong, dt);
        }
    }
}