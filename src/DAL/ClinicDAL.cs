using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyTongHop.DTO;

namespace QuanLyTongHop.DAL
{
    public class ClinicDAL
    {
        private string ConnStr => DbHelper.GetConnStr("ClinicDB");

        public BenhNhanDTO? TimBenhNhanBySDT(string sdt)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT MaBN, HoTen, SoDienThoai, NamSinh FROM BenhNhan WHERE SoDienThoai = @SDT", conn);
            cmd.Parameters.AddWithValue("@SDT", sdt);
            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                return new BenhNhanDTO
                {
                    MaBN = rd.GetInt32(0),
                    HoTen = rd.GetString(1),
                    SoDienThoai = rd.GetString(2),
                    NamSinh = rd.GetInt32(3)
                };
            }
            return null;
        }

        public int TiepNhanVaTaoHoSo(BenhNhanDTO bn, string trieuChung, string chanDoan)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            int maBN = bn.MaBN;
            if (maBN == 0)
            {
                var cmdBN = new SqlCommand("INSERT INTO BenhNhan (HoTen, SoDienThoai, NamSinh) OUTPUT INSERTED.MaBN VALUES (@Ten, @SDT, @NS)", conn);
                cmdBN.Parameters.AddWithValue("@Ten", bn.HoTen);
                cmdBN.Parameters.AddWithValue("@SDT", bn.SoDienThoai);
                cmdBN.Parameters.AddWithValue("@NS", bn.NamSinh);
                maBN = Convert.ToInt32(cmdBN.ExecuteScalar());
            }

            var cmdHS = new SqlCommand("INSERT INTO HoSoKham (MaBN, TrieuChung, ChanDoan) OUTPUT INSERTED.MaHoSo VALUES (@MaBN, @TC, @CD)", conn);
            cmdHS.Parameters.AddWithValue("@MaBN", maBN);
            cmdHS.Parameters.AddWithValue("@TC", trieuChung);
            cmdHS.Parameters.AddWithValue("@CD", chanDoan);
            return Convert.ToInt32(cmdHS.ExecuteScalar());
        }

        public DataTable GetDanhSachHoSo()
        {
            using var conn = new SqlConnection(ConnStr);
            string sql = @"SELECT hs.MaHoSo, bn.HoTen AS BenhNhan, bn.SoDienThoai, hs.TrieuChung, hs.ChanDoan, hs.NgayKham
                           FROM HoSoKham hs JOIN BenhNhan bn ON hs.MaBN = bn.MaBN ORDER BY hs.MaHoSo DESC";
            using var da = new SqlDataAdapter(sql, conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public List<ThuocDTO> GetDanhMucThuoc()
        {
            var list = new List<ThuocDTO>();
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var cmd = new SqlCommand("SELECT MaThuoc, TenThuoc, DonViTinh, DonGia, SoLuongTon FROM Thuoc", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new ThuocDTO
                {
                    MaThuoc = rd.GetInt32(0),
                    TenThuoc = rd.GetString(1),
                    DonViTinh = rd.GetString(2),
                    DonGia = rd.GetDecimal(3),
                    SoLuongTon = rd.GetInt32(4)
                });
            }
            return list;
        }

        public void ThemHoacNhapKhoThuoc(string tenThuoc, string dvt, decimal donGia, int soLuongNhap)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Thuoc (TenThuoc, DonViTinh, DonGia, SoLuongTon) VALUES (@Ten, @DVT, @Gia, @SL)", conn);
            cmd.Parameters.AddWithValue("@Ten", tenThuoc);
            cmd.Parameters.AddWithValue("@DVT", dvt);
            cmd.Parameters.AddWithValue("@Gia", donGia);
            cmd.Parameters.AddWithValue("@SL", soLuongNhap);
            cmd.ExecuteNonQuery();
        }

        // Kê đơn & Xuất kho bằng SqlTransaction
        public (int MaDon, decimal TongTien) KeDonVaXuatKhoTransaction(int maHoSo, decimal tienKham, List<ChiTietDonThuocDTO> danhSachThuoc)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                decimal tienThuoc = 0;
                foreach (var item in danhSachThuoc)
                    tienThuoc += item.SoLuong * item.DonGia;

                decimal tongTien = tienKham + tienThuoc;

                // 1. Tạo bản ghi DonThuoc
                var cmdDon = new SqlCommand("INSERT INTO DonThuoc (MaHoSo, TienKham, TongTien, TrangThai) OUTPUT INSERTED.MaDon VALUES (@MaHS, @TK, @TT, N'Chờ lấy thuốc')", conn, trans);
                cmdDon.Parameters.AddWithValue("@MaHS", maHoSo);
                cmdDon.Parameters.AddWithValue("@TK", tienKham);
                cmdDon.Parameters.AddWithValue("@TT", tongTien);
                int maDon = Convert.ToInt32(cmdDon.ExecuteScalar());

                // 2. Tạo ChiTietDonThuoc & 3. Trừ SoLuongTon
                foreach (var item in danhSachThuoc)
                {
                    var cmdCheck = new SqlCommand("SELECT SoLuongTon FROM Thuoc WHERE MaThuoc = @MaT", conn, trans);
                    cmdCheck.Parameters.AddWithValue("@MaT", item.MaThuoc);
                    int ton = Convert.ToInt32(cmdCheck.ExecuteScalar());

                    if (ton < item.SoLuong)
                        throw new Exception($"Thuốc '{item.TenThuoc}' không đủ tồn kho (Tồn: {ton}, Kê: {item.SoLuong})! Giao dịch đã Rollback.");

                    var cmdCT = new SqlCommand("INSERT INTO ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung) VALUES (@MaD, @MaT, @SL, @LD)", conn, trans);
                    cmdCT.Parameters.AddWithValue("@MaD", maDon);
                    cmdCT.Parameters.AddWithValue("@MaT", item.MaThuoc);
                    cmdCT.Parameters.AddWithValue("@SL", item.SoLuong);
                    cmdCT.Parameters.AddWithValue("@LD", item.LieuDung);
                    cmdCT.ExecuteNonQuery();

                    var cmdTruKho = new SqlCommand("UPDATE Thuoc SET SoLuongTon = SoLuongTon - @SL WHERE MaThuoc = @MaT", conn, trans);
                    cmdTruKho.Parameters.AddWithValue("@SL", item.SoLuong);
                    cmdTruKho.Parameters.AddWithValue("@MaT", item.MaThuoc);
                    cmdTruKho.ExecuteNonQuery();
                }

                trans.Commit();
                return (maDon, tongTien);
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public void ThanhToanDonThuoc(int maDon)
        {
            using var conn = new SqlConnection(ConnStr);
            conn.Open();
            var cmd = new SqlCommand("UPDATE DonThuoc SET TrangThai = N'Đã thanh toán' WHERE MaDon = @MaD", conn);
            cmd.Parameters.AddWithValue("@MaD", maDon);
            cmd.ExecuteNonQuery();
        }
    }
}