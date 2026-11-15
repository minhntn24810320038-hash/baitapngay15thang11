using System;
using Microsoft.Data.SqlClient;

namespace QuanLyTongHop.DAL
{
    public static class DbHelper
    {
        private static string _activeServer = ".";

        public static string GetConnStr(string dbName) =>
            $"Server={_activeServer};Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";

        public static void InitializeAllDatabases()
        {
            string[] servers = { ".", @".\SQLEXPRESS", @"(localdb)\MSSQLLocalDB", "localhost" };
            bool connected = false;

            foreach (var srv in servers)
            {
                try
                {
                    using var conn = new SqlConnection($"Server={srv};Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;");
                    conn.Open();
                    _activeServer = srv;
                    connected = true;
                    break;
                }
                catch { }
            }

            if (!connected)
                throw new Exception("Không thể kết nối SQL Server cục bộ! Vui lòng kiểm tra dịch vụ SQL Server.");

            EnsureLibraryDB();
            EnsureClinicDB();
            EnsureBilliardsDB();
        }

        private static void ExecMaster(string sql)
        {
            using var conn = new SqlConnection(GetConnStr("master"));
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private static void ExecDb(string db, string sql)
        {
            using var conn = new SqlConnection(GetConnStr(db));
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private static void EnsureLibraryDB()
        {
            ExecMaster("IF DB_ID('LibraryDB') IS NULL CREATE DATABASE LibraryDB;");
            ExecDb("LibraryDB", @"
                IF OBJECT_ID('DocGia', 'U') IS NULL
                BEGIN
                    CREATE TABLE DocGia (MaDocGia INT PRIMARY KEY IDENTITY(1,1), HoTen NVARCHAR(100) NOT NULL, Email VARCHAR(100) UNIQUE, NgayHetHanThe DATETIME);
                    CREATE TABLE DauSach (MaSach INT PRIMARY KEY IDENTITY(1,1), TenSach NVARCHAR(200) NOT NULL, TheLoai NVARCHAR(50), SoLuongCon INT CHECK (SoLuongCon >= 0));
                    CREATE TABLE PhieuMuon (MaPhieu INT PRIMARY KEY IDENTITY(1,1), MaDocGia INT FOREIGN KEY REFERENCES DocGia(MaDocGia), NgayMuon DATETIME DEFAULT GETDATE(), HanTra DATETIME, TrangThai NVARCHAR(50));
                    CREATE TABLE ChiTietMuon (MaPhieu INT FOREIGN KEY REFERENCES PhieuMuon(MaPhieu), MaSach INT FOREIGN KEY REFERENCES DauSach(MaSach), SoLuong INT CHECK (SoLuong > 0), PRIMARY KEY (MaPhieu, MaSach));

                    INSERT INTO DocGia (HoTen, Email, NgayHetHanThe) VALUES
                    (N'Nguyễn Tiến Nhật Minh', 'minh@student.edu.vn', DATEADD(MONTH, 6, GETDATE())),
                    (N'Trần Thị Lan', 'lan.tran@gmail.com', DATEADD(MONTH, 3, GETDATE())),
                    (N'Lê Văn Hết Hạn', 'hethan@gmail.com', DATEADD(DAY, -10, GETDATE()));

                    INSERT INTO DauSach (TenSach, TheLoai, SoLuongCon) VALUES
                    (N'Lập trình C# WinForms 3-Tier', N'Công nghệ thông tin', 15),
                    (N'Cơ sở dữ liệu SQL Server', N'Công nghệ thông tin', 10),
                    (N'Kinh tế vi mô cơ bản', N'Kinh tế', 8),
                    (N'Nhà Giả Kim', N'Văn học', 12);

                    INSERT INTO PhieuMuon (MaDocGia, NgayMuon, HanTra, TrangThai) VALUES
                    (1, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, 5, GETDATE()), N'Đang mượn'),
                    (2, DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -3, GETDATE()), N'Quá hạn'),
                    (1, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -10, GETDATE()), N'Đã trả');

                    INSERT INTO ChiTietMuon (MaPhieu, MaSach, SoLuong) VALUES (1, 1, 1), (2, 3, 2), (3, 4, 1);
                END");
        }

        private static void EnsureClinicDB()
        {
            ExecMaster("IF DB_ID('ClinicDB') IS NULL CREATE DATABASE ClinicDB;");
            ExecDb("ClinicDB", @"
                IF OBJECT_ID('BenhNhan', 'U') IS NULL
                BEGIN
                    CREATE TABLE BenhNhan (MaBN INT PRIMARY KEY IDENTITY(1,1), HoTen NVARCHAR(100) NOT NULL, SoDienThoai VARCHAR(15) UNIQUE, NamSinh INT);
                    CREATE TABLE HoSoKham (MaHoSo INT PRIMARY KEY IDENTITY(1,1), MaBN INT FOREIGN KEY REFERENCES BenhNhan(MaBN), NgayKham DATETIME DEFAULT GETDATE(), TrieuChung NVARCHAR(MAX), ChanDoan NVARCHAR(250));
                    CREATE TABLE Thuoc (MaThuoc INT PRIMARY KEY IDENTITY(1,1), TenThuoc NVARCHAR(150) NOT NULL, DonViTinh NVARCHAR(20), DonGia DECIMAL(18,2), SoLuongTon INT CHECK (SoLuongTon >= 0));
                    CREATE TABLE DonThuoc (MaDon INT PRIMARY KEY IDENTITY(1,1), MaHoSo INT FOREIGN KEY REFERENCES HoSoKham(MaHoSo), TienKham DECIMAL(18,2), TongTien DECIMAL(18,2), TrangThai NVARCHAR(30));
                    CREATE TABLE ChiTietDonThuoc (MaDon INT FOREIGN KEY REFERENCES DonThuoc(MaDon), MaThuoc INT FOREIGN KEY REFERENCES Thuoc(MaThuoc), SoLuong INT CHECK (SoLuong > 0), LieuDung NVARCHAR(100), PRIMARY KEY (MaDon, MaThuoc));

                    INSERT INTO BenhNhan (HoTen, SoDienThoai, NamSinh) VALUES (N'Phạm Minh Tuấn', '0901234567', 1998), (N'Hoàng Thu Trang', '0912345678', 2001);
                    INSERT INTO HoSoKham (MaBN, TrieuChung, ChanDoan) VALUES (1, N'Sốt nhẹ, đau họng', N'Viêm họng cấp'), (2, N'Đau đầu, chóng mặt', N'Rối loạn tiền đình');
                    INSERT INTO Thuoc (TenThuoc, DonViTinh, DonGia, SoLuongTon) VALUES
                    (N'Paracetamol 500mg', N'Viên', 2000, 100),
                    (N'Amoxicillin 500mg', N'Vỉ', 15000, 50),
                    (N'Siro Ho Bảo Thanh', N'Chai', 45000, 5);
                END");
        }

        private static void EnsureBilliardsDB()
        {
            ExecMaster("IF DB_ID('BilliardsDB') IS NULL CREATE DATABASE BilliardsDB;");
            ExecDb("BilliardsDB", @"
                IF OBJECT_ID('BanBida', 'U') IS NULL
                BEGIN
                    CREATE TABLE BanBida (MaBan INT PRIMARY KEY IDENTITY(1,1), TenBan NVARCHAR(50) NOT NULL, LoaiBan NVARCHAR(30), GiaTheoGio DECIMAL(18,2), TrangThai NVARCHAR(20) DEFAULT N'Trống');
                    CREATE TABLE MenuDichVu (MaDV INT PRIMARY KEY IDENTITY(1,1), TenDV NVARCHAR(100) NOT NULL, DonGia DECIMAL(18,2), TonKho INT);
                    CREATE TABLE HoaDon (MaHD INT PRIMARY KEY IDENTITY(1,1), MaBan INT FOREIGN KEY REFERENCES BanBida(MaBan), GioBatDau DATETIME NOT NULL, GioKetThuc DATETIME NULL, TienGioChoi DECIMAL(18,2) DEFAULT 0, TienDichVu DECIMAL(18,2) DEFAULT 0, TrangThai BIT DEFAULT 0);
                    CREATE TABLE ChiTietHoaDon (MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD), MaDV INT FOREIGN KEY REFERENCES MenuDichVu(MaDV), SoLuong INT CHECK (SoLuong > 0), DonGia DECIMAL(18,2), PRIMARY KEY (MaHD, MaDV));

                    INSERT INTO BanBida (TenBan, LoaiBan, GiaTheoGio, TrangThai) VALUES
                    (N'Bàn Lỗ 01', N'Pool (Lỗ)', 60000, N'Trống'),
                    (N'Bàn Lỗ 02', N'Pool (Lỗ)', 60000, N'Đang chơi'),
                    (N'Bàn Carom 01', N'Carom (Phăng)', 50000, N'Trống'),
                    (N'Bàn Snooker VIP', N'Snooker', 90000, N'Trống');

                    INSERT INTO MenuDichVu (TenDV, DonGia, TonKho) VALUES
                    (N'Nước tăng lực Sting', 15000, 120),
                    (N'Mì xào bò', 35000, 40),
                    (N'Bia Heineken', 25000, 100),
                    (N'Thuê gậy Carbon', 30000, 20);

                    INSERT INTO HoaDon (MaBan, GioBatDau, TrangThai) VALUES (2, DATEADD(MINUTE, -45, GETDATE()), 0);
                    INSERT INTO ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) VALUES (1, 1, 2, 15000), (1, 4, 1, 30000);
                END");
        }
    }
}