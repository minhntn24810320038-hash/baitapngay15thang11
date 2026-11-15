using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyTongHop.DTO;

namespace QuanLyTongHop.DAL
{
    public class BilliardsDAL
    {
        private string ConnStr => DbHelper.GetConnStr("BilliardsDB");

        public List<BanBidaDTO> GetDanhSachBan()
        {
            var list = new List<BanBidaDTO>();
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT MaBan, TenBan, LoaiBan, GiaTheoGio, TrangThai FROM BanBida", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new BanBidaDTO
                {
                    MaBan = rd.GetInt32(0),
                    TenBan = rd.GetString(1),
                    LoaiBan = rd.GetString(2),
                    GiaTheoGio = rd.GetDecimal(3),
                    TrangThai = rd.GetString(4)
                });
            }
            return list;
        }

        public List<MenuDichVuDTO> GetMenuDichVu()
        {
            var list = new List<MenuDichVuDTO>();
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT MaDV, TenDV, DonGia, TonKho FROM MenuDichVu", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new MenuDichVuDTO
                {
                    MaDV = rd.GetInt32(0),
                    TenDV = rd.GetString(1),
                    DonGia = rd.GetDecimal(2),
                    TonKho = rd.GetInt32(3)
                });
            }
            return list;
        }

        public int GetMaHoaDonChuaThanhToan(int maBan)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT TOP 1 MaHD FROM HoaDon WHERE MaBan = @MaB AND TrangThai = 0 ORDER BY MaHD DESC", conn);
            cmd.Parameters.AddWithValue("@MaB", maBan);
            var res = cmd.ExecuteScalar();
            return res == null ? 0 : Convert.ToInt32(res);
        }

        public void MoBan(int maBan)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                var cmdHD = new SqlCommand("INSERT INTO HoaDon (MaBan, GioBatDau, TrangThai) VALUES (@MaB, GETDATE(), 0)", conn, trans);
                cmdHD.Parameters.AddWithValue("@MaB", maBan);
                cmdHD.ExecuteNonQuery();

                var cmdBan = new SqlCommand("UPDATE BanBida SET TrangThai = N'Đang chơi' WHERE MaBan = @MaB", conn, trans);
                cmdBan.Parameters.AddWithValue("@MaB", maBan);
                cmdBan.ExecuteNonQuery();

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // Disconnected Model: SqlDataAdapter + DataTable
        public DataTable GetChiTietHoaDonDisconnected(int maHD)
        {
            using var conn = new SqlConnection(ConnStr);
            string sql = @"SELECT ct.MaDV, dv.TenDV AS [Tên Dịch Vụ], ct.SoLuong AS [Số Lượng], ct.DonGia AS [Đơn Giá], (ct.SoLuong * ct.DonGia) AS [Thành Tiền]
                           FROM ChiTietHoaDon ct JOIN MenuDichVu dv ON ct.MaDV = dv.MaDV
                           WHERE ct.MaHD = @MaHD";
            using var da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.AddWithValue("@MaHD", maHD);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void GoiThemDichVu(int maHD, int maDV, int soLuong, decimal donGia)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            string sql = @"IF EXISTS (SELECT 1 FROM ChiTietHoaDon WHERE MaHD = @MaHD AND MaDV = @MaDV)
                               UPDATE ChiTietHoaDon SET SoLuong = SoLuong + @SL WHERE MaHD = @MaHD AND MaDV = @MaDV
                           ELSE
                               INSERT INTO ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) VALUES (@MaHD, @MaDV, @SL, @Gia)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaHD", maHD);
            cmd.Parameters.AddWithValue("@MaDV", maDV);
            cmd.Parameters.AddWithValue("@SL", soLuong);
            cmd.Parameters.AddWithValue("@Gia", donGia);
            cmd.ExecuteNonQuery();
        }

        // Thanh toán bằng SqlTransaction
        public (double SoGio, decimal TienGio, decimal TienDV, decimal TongCong) ThanhToanBanTransaction(int maBan, int maHD, decimal giaTheoGio)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                var cmdGio = new SqlCommand("SELECT GioBatDau FROM HoaDon WHERE MaHD = @MaHD", conn, trans);
                cmdGio.Parameters.AddWithValue("@MaHD", maHD);
                DateTime gioBatDau = Convert.ToDateTime(cmdGio.ExecuteScalar());
                DateTime gioKetThuc = DateTime.Now;

                double phut = (gioKetThuc - gioBatDau).TotalMinutes;
                if (phut < 15) phut = 15; // Làm tròn tối thiểu block 15 phút
                double soGio = Math.Round(phut / 60.0, 2);
                decimal tienGio = Math.Round((decimal)soGio * giaTheoGio, 0);

                var cmdDV = new SqlCommand("SELECT ISNULL(SUM(SoLuong * DonGia), 0) FROM ChiTietHoaDon WHERE MaHD = @MaHD", conn, trans);
                cmdDV.Parameters.AddWithValue("@MaHD", maHD);
                decimal tienDV = Convert.ToDecimal(cmdDV.ExecuteScalar());

                // 1. Cập nhật HoaDon
                var cmdUpdHD = new SqlCommand(@"UPDATE HoaDon SET GioKetThuc = @GKT, TienGioChoi = @TG, TienDichVu = @TDV, TrangThai = 1 WHERE MaHD = @MaHD", conn, trans);
                cmdUpdHD.Parameters.AddWithValue("@GKT", gioKetThuc);
                cmdUpdHD.Parameters.AddWithValue("@TG", tienGio);
                cmdUpdHD.Parameters.AddWithValue("@TDV", tienDV);
                cmdUpdHD.Parameters.AddWithValue("@MaHD", maHD);
                cmdUpdHD.ExecuteNonQuery();

                // 2. Trừ tồn kho (bỏ qua dịch vụ Thuê gậy)
                var cmdTruKho = new SqlCommand(@"
                    UPDATE MenuDichVu
                    SET TonKho = TonKho - ct.SoLuong
                    FROM MenuDichVu dv JOIN ChiTietHoaDon ct ON dv.MaDV = ct.MaDV
                    WHERE ct.MaHD = @MaHD AND dv.TenDV NOT LIKE N'%Thuê%'", conn, trans);
                cmdTruKho.Parameters.AddWithValue("@MaHD", maHD);
                cmdTruKho.ExecuteNonQuery();

                // 3. Cập nhật BanBida về Trống
                var cmdBan = new SqlCommand("UPDATE BanBida SET TrangThai = N'Trống' WHERE MaBan = @MaB", conn, trans);
                cmdBan.Parameters.AddWithValue("@MaB", maBan);
                cmdBan.ExecuteNonQuery();

                trans.Commit();
                return (soGio, tienGio, tienDV, tienGio + tienDV);
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
    }
}