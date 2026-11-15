using System;
using System.Collections.Generic;
using System.Data;
using QuanLyTongHop.DAL;
using QuanLyTongHop.DTO;

namespace QuanLyTongHop.BUS
{
    public class LibraryBUS
    {
        private readonly LibraryDAL _dal = new();

        public List<DocGiaDTO> GetDocGia() => _dal.GetDocGia();
        public List<DauSachDTO> GetDauSach() => _dal.GetDauSach();

        public void ThemDauSach(string ten, string theLoai, string slText)
        {
            if (string.IsNullOrWhiteSpace(ten)) throw new Exception("Tên sách không được để trống!");
            if (!int.TryParse(slText, out int sl) || sl < 0) throw new Exception("Số lượng sách phải là số nguyên >= 0!");
            _dal.ThemDauSach(new DauSachDTO { TenSach = ten, TheLoai = theLoai, SoLuongCon = sl });
        }

        public void XoaDauSach(int maSach) => _dal.XoaDauSach(maSach);

        public DataTable GetPhieuMuon(int maDocGia, string trangThai) => _dal.GetPhieuMuonFiltered(maDocGia, trangThai);

        public void LapPhieuMuon(int maDocGia, int maSach, string slText, DateTime hanTra)
        {
            if (!int.TryParse(slText, out int sl) || sl <= 0) throw new Exception("Số lượng mượn phải > 0!");
            if (hanTra <= DateTime.Now) throw new Exception("Hạn trả phải lớn hơn ngày hiện tại!");
            _dal.LapPhieuMuon(maDocGia, maSach, sl, hanTra);
        }

        public void TraSach(int maPhieu) => _dal.TraSach(maPhieu);
        public DataTable GetCanhBaoQuaHan() => _dal.GetCanhBaoQuaHanReader();
        public (int Tong, DataTable BangTheLoai) GetThongKe() => _dal.ThongKeMuonTheoTheLoai();
    }

    public class ClinicBUS
    {
        private readonly ClinicDAL _dal = new();

        public BenhNhanDTO? TimBenhNhan(string sdt)
        {
            if (string.IsNullOrWhiteSpace(sdt)) throw new Exception("Vui lòng nhập số điện thoại!");
            return _dal.TimBenhNhanBySDT(sdt.Trim());
        }

        public int TiepNhan(int maBN, string hoTen, string sdt, string namSinhText, string trieuChung, string chanDoan)
        {
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(sdt))
                throw new Exception("Họ tên và Số điện thoại không được để trống!");
            if (!int.TryParse(namSinhText, out int namSinh) || namSinh < 1900 || namSinh > DateTime.Now.Year)
                throw new Exception("Năm sinh không hợp lệ!");
            return _dal.TiepNhanVaTaoHoSo(new BenhNhanDTO { MaBN = maBN, HoTen = hoTen, SoDienThoai = sdt, NamSinh = namSinh }, trieuChung, chanDoan);
        }

        public DataTable GetHoSoKham() => _dal.GetDanhSachHoSo();
        public List<ThuocDTO> GetThuoc() => _dal.GetDanhMucThuoc();

        public void ThemThuoc(string ten, string dvt, string giaText, string slText)
        {
            if (string.IsNullOrWhiteSpace(ten)) throw new Exception("Tên thuốc không được để trống!");
            if (!decimal.TryParse(giaText, out decimal gia) || gia <= 0) throw new Exception("Đơn giá thuốc phải > 0!");
            if (!int.TryParse(slText, out int sl) || sl < 0) throw new Exception("Số lượng tồn phải >= 0!");
            _dal.ThemHoacNhapKhoThuoc(ten, dvt, gia, sl);
        }

        public (int MaDon, decimal TongTien) KeDonVaXuatKho(int maHoSo, string tienKhamText, List<ChiTietDonThuocDTO> dsThuoc)
        {
            if (maHoSo <= 0) throw new Exception("Vui lòng chọn hồ sơ khám!");
            if (!decimal.TryParse(tienKhamText, out decimal tienKham) || tienKham < 0) throw new Exception("Tiền khám không hợp lệ!");
            if (dsThuoc.Count == 0) throw new Exception("Đơn thuốc phải có ít nhất 1 loại thuốc!");
            return _dal.KeDonVaXuatKhoTransaction(maHoSo, tienKham, dsThuoc);
        }

        public void ThanhToan(int maDon) => _dal.ThanhToanDonThuoc(maDon);
    }

    public class BilliardsBUS
    {
        private readonly BilliardsDAL _dal = new();

        public List<BanBidaDTO> GetBan() => _dal.GetDanhSachBan();
        public List<MenuDichVuDTO> GetMenu() => _dal.GetMenuDichVu();
        public int GetMaHDActive(int maBan) => _dal.GetMaHoaDonChuaThanhToan(maBan);
        public void MoBan(int maBan) => _dal.MoBan(maBan);
        public DataTable GetChiTietHD(int maHD) => _dal.GetChiTietHoaDonDisconnected(maHD);

        public void GoiDichVu(int maHD, int maDV, string slText, decimal donGia)
        {
            if (maHD <= 0) throw new Exception("Bàn chưa mở! Vui lòng bấm 'Mở bàn' trước.");
            if (!int.TryParse(slText, out int sl) || sl <= 0) throw new Exception("Số lượng gọi phải > 0!");
            _dal.GoiThemDichVu(maHD, maDV, sl, donGia);
        }

        public (double SoGio, decimal TienGio, decimal TienDV, decimal TongCong) ThanhToanBan(int maBan, int maHD, decimal giaTheoGio)
        {
            if (maHD <= 0) throw new Exception("Bàn đang trống, không có hóa đơn để thanh toán!");
            return _dal.ThanhToanBanTransaction(maBan, maHD, giaTheoGio);
        }
    }
}