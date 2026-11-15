using System;

namespace QuanLyTongHop.DTO
{
    // --- BÀI 6.6: THƯ VIỆN ---
    public class DocGiaDTO
    {
        public int MaDocGia { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime NgayHetHanThe { get; set; }
    }

    public class DauSachDTO
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; } = "";
        public string TheLoai { get; set; } = "";
        public int SoLuongCon { get; set; }
    }

    public class PhieuMuonDTO
    {
        public int MaPhieu { get; set; }
        public int MaDocGia { get; set; }
        public string TenDocGia { get; set; } = "";
        public DateTime NgayMuon { get; set; }
        public DateTime HanTra { get; set; }
        public string TrangThai { get; set; } = "";
    }

    // --- BÀI 6.7: PHÒNG KHÁM ---
    public class BenhNhanDTO
    {
        public int MaBN { get; set; }
        public string HoTen { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public int NamSinh { get; set; }
    }

    public class HoSoKhamDTO
    {
        public int MaHoSo { get; set; }
        public int MaBN { get; set; }
        public string TenBenhNhan { get; set; } = "";
        public DateTime NgayKham { get; set; }
        public string TrieuChung { get; set; } = "";
        public string ChanDoan { get; set; } = "";
    }

    public class ThuocDTO
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = "";
        public string DonViTinh { get; set; } = "";
        public decimal DonGia { get; set; }
        public int SoLuongTon { get; set; }
    }

    public class ChiTietDonThuocDTO
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = "";
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string LieuDung { get; set; } = "";
    }

    // --- BÀI 6.8: BÀN BIDA ---
    public class BanBidaDTO
    {
        public int MaBan { get; set; }
        public string TenBan { get; set; } = "";
        public string LoaiBan { get; set; } = "";
        public decimal GiaTheoGio { get; set; }
        public string TrangThai { get; set; } = "";
    }

    public class MenuDichVuDTO
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; } = "";
        public decimal DonGia { get; set; }
        public int TonKho { get; set; }
    }
}