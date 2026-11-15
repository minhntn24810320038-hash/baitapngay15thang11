using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyTongHop.BUS;
using QuanLyTongHop.DTO;

namespace QuanLyTongHop.GUI
{
    public class FormMain : Form
    {
        private readonly LibraryBUS _libBus = new();
        private readonly ClinicBUS _clinicBus = new();
        private readonly BilliardsBUS _bidaBus = new();

        private Panel pnlContent = new();

        // Controls Bài 6.6 (Thư viện)
        private DataGridView dgvSach = new(), dgvPhieuMuon = new(), dgvQuaHan = new(), dgvThongKe = new();
        private ComboBox cboDocGiaFilter = new(), cboTrangThaiFilter = new(), cboDocGiaMuon = new(), cboSachMuon = new();
        private TextBox txtTenSach = new(), txtTheLoai = new(), txtSLSach = new(), txtSLMuon = new();
        private DateTimePicker dtpHanTra = new();
        private Label lblTongLuotMuon = new();

        // Controls Bài 6.7 (Phòng khám)
        private TextBox txtSDT = new(), txtHoTenBN = new(), txtNamSinh = new(), txtTrieuChung = new(), txtChanDoan = new();
        private TextBox txtTenThuoc = new(), txtDVT = new(), txtGiaThuoc = new(), txtSLNhap = new(), txtTienKham = new(), txtSLKe = new(), txtLieuDung = new();
        private ComboBox cboHoSo = new(), cboThuocKe = new();
        private DataGridView dgvThuoc = new(), dgvDonTam = new();
        private List<ChiTietDonThuocDTO> _donThuocTam = new();
        private int _foundMaBN = 0, _lastMaDon = 0;

        // Controls Bài 6.8 (Bida)
        private FlowLayoutPanel flpBanBida = new();
        private DataGridView dgvChiTietBida = new();
        private ComboBox cboMenuDV = new();
        private TextBox txtSLDV = new();
        private Label lblBanDangChon = new();
        private BanBidaDTO? _selectedBan;
        private int _activeMaHD = 0;

        public FormMain()
        {
            Text = "HỆ THỐNG QUẢN LÝ TỔNG HỢP - BÀI TẬP 6.6, 6.7, 6.8 (3-TIER ARCHITECTURE)";
            Size = new Size(1180, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            BuildLayout();
            ShowLibraryPanel();
        }

        private void BuildLayout()
        {
            var pnlMenu = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = Color.FromArgb(30, 41, 59) };
            var lblTitle = new Label
            {
                Text = "MENU CHỨC NĂNG\n(3-TIER WINFORMS)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btn1 = CreateNavButton("1. Quản lý Thư viện (6.6)");
            var btn2 = CreateNavButton("2. Phòng khám & Kê đơn (6.7)");
            var btn3 = CreateNavButton("3. Quản lý Bàn Bida (6.8)");

            btn1.Click += (s, e) => ShowLibraryPanel();
            btn2.Click += (s, e) => ShowClinicPanel();
            btn3.Click += (s, e) => ShowBilliardsPanel();

            pnlMenu.Controls.Add(btn3);
            pnlMenu.Controls.Add(btn2);
            pnlMenu.Controls.Add(btn1);
            pnlMenu.Controls.Add(lblTitle);

            pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.WhiteSmoke };
            Controls.Add(pnlContent);
            Controls.Add(pnlMenu);
        }

        private Button CreateNavButton(string text) => new Button
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 48,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(51, 65, 85),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            Cursor = Cursors.Hand
        };

        // ==========================================
        // PANEL BÀI 6.6: QUẢN LÝ THƯ VIỆN
        // ==========================================
        private void ShowLibraryPanel()
        {
            pnlContent.Controls.Clear();
            var tabs = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Quản lý Đầu sách (CRUD)
            var tabSach = new TabPage("1. Quản lý Đầu sách (CRUD)");
            var pnlTopSach = new Panel { Dock = DockStyle.Top, Height = 55 };
            txtTenSach = new TextBox { PlaceholderText = "Tên đầu sách...", Location = new Point(10, 14), Width = 230 };
            txtTheLoai = new TextBox { PlaceholderText = "Thể loại (CNTT, Kinh tế...)", Location = new Point(250, 14), Width = 170 };
            txtSLSach = new TextBox { PlaceholderText = "Số lượng", Location = new Point(430, 14), Width = 80 };
            var btnThemSach = new Button { Text = "Thêm Sách", Location = new Point(520, 12), Width = 100, Height = 28, BackColor = Color.SeaGreen, ForeColor = Color.White };
            var btnXoaSach = new Button { Text = "Xóa Sách Chọn", Location = new Point(630, 12), Width = 120, Height = 28, BackColor = Color.IndianRed, ForeColor = Color.White };

            btnThemSach.Click += (s, e) => {
                try { _libBus.ThemDauSach(txtTenSach.Text, txtTheLoai.Text, txtSLSach.Text); LoadLibraryData(); MessageBox.Show("Thêm sách thành công!"); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            btnXoaSach.Click += (s, e) => {
                try
                {
                    if (dgvSach.CurrentRow == null) return;
                    int ma = Convert.ToInt32(dgvSach.CurrentRow.Cells["MaSach"].Value);
                    _libBus.XoaDauSach(ma);
                    LoadLibraryData();
                }
                catch (Exception ex) { MessageBox.Show("Không thể xóa sách đang có phiếu mượn: " + ex.Message, "Lỗi ràng buộc", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            pnlTopSach.Controls.AddRange(new Control[] { txtTenSach, txtTheLoai, txtSLSach, btnThemSach, btnXoaSach });
            dgvSach = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            tabSach.Controls.Add(dgvSach);
            tabSach.Controls.Add(pnlTopSach);

            // Tab 2: Mượn / Trả & Lọc trạng thái
            var tabMuon = new TabPage("2. Mượn / Trả & Lọc Phiếu");
            var pnlMuonTop = new Panel { Dock = DockStyle.Top, Height = 95 };
            cboDocGiaMuon = new ComboBox { Location = new Point(10, 12), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboSachMuon = new ComboBox { Location = new Point(200, 12), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            txtSLMuon = new TextBox { Text = "1", Location = new Point(410, 12), Width = 45 };
            dtpHanTra = new DateTimePicker { Location = new Point(465, 12), Width = 130, Value = DateTime.Now.AddDays(7) };
            var btnMuon = new Button { Text = "Lập Phiếu Mượn", Location = new Point(605, 10), Width = 130, Height = 28, BackColor = Color.RoyalBlue, ForeColor = Color.White };

            cboDocGiaFilter = new ComboBox { Location = new Point(10, 55), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTrangThaiFilter = new ComboBox { Location = new Point(200, 55), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTrangThaiFilter.Items.AddRange(new object[] { "Tất cả", "Đang mượn", "Đã trả", "Quá hạn" });
            cboTrangThaiFilter.SelectedIndex = 0;
            var btnLoc = new Button { Text = "Lọc Phiếu", Location = new Point(360, 53), Width = 90, Height = 28 };
            var btnTra = new Button { Text = "Trả Sách (Cập nhật trạng thái)", Location = new Point(460, 53), Width = 210, Height = 28, BackColor = Color.DarkOrange, ForeColor = Color.White };

            btnMuon.Click += (s, e) => {
                try
                {
                    int maDG = Convert.ToInt32(cboDocGiaMuon.SelectedValue);
                    int maS = Convert.ToInt32(cboSachMuon.SelectedValue);
                    _libBus.LapPhieuMuon(maDG, maS, txtSLMuon.Text, dtpHanTra.Value);
                    LoadLibraryData();
                    MessageBox.Show("Lập phiếu mượn thành công!");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Cảnh báo nghiệp vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            btnLoc.Click += (s, e) => FilterPhieuMuon();
            btnTra.Click += (s, e) => {
                try
                {
                    if (dgvPhieuMuon.CurrentRow == null) return;
                    int maPhieu = Convert.ToInt32(dgvPhieuMuon.CurrentRow.Cells["MaPhieu"].Value);
                    _libBus.TraSach(maPhieu);
                    LoadLibraryData();
                    MessageBox.Show("Đã trả sách và cộng lại tồn kho!");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };

            pnlMuonTop.Controls.AddRange(new Control[] { cboDocGiaMuon, cboSachMuon, txtSLMuon, dtpHanTra, btnMuon, cboDocGiaFilter, cboTrangThaiFilter, btnLoc, btnTra });
            dgvPhieuMuon = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            tabMuon.Controls.Add(dgvPhieuMuon);
            tabMuon.Controls.Add(pnlMuonTop);

            // Tab 3: Cảnh báo Quá hạn & Thống kê
            var tabBaoCao = new TabPage("3. Cảnh báo Quá hạn (SqlDataReader) & Thống kê");
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };
            var lblQH = new Label { Text = "⚠️ DANH SÁCH PHIẾU QUÁ HẠN (SQLDATAREADER):", ForeColor = Color.Red, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 28 };
            dgvQuaHan = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            split.Panel1.Controls.Add(dgvQuaHan);
            split.Panel1.Controls.Add(lblQH);

            lblTongLuotMuon = new Label { Text = "Tổng số lượt mượn (ExecuteScalar): 0", ForeColor = Color.Blue, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
            dgvThongKe = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            split.Panel2.Controls.Add(dgvThongKe);
            split.Panel2.Controls.Add(lblTongLuotMuon);
            tabBaoCao.Controls.Add(split);

            tabs.TabPages.AddRange(new[] { tabSach, tabMuon, tabBaoCao });
            pnlContent.Controls.Add(tabs);
            LoadLibraryData();
        }

        private void LoadLibraryData()
        {
            dgvSach.DataSource = _libBus.GetDauSach();
            var dsDG = _libBus.GetDocGia();
            cboDocGiaMuon.DataSource = new List<DocGiaDTO>(dsDG);
            cboDocGiaMuon.DisplayMember = "HoTen";
            cboDocGiaMuon.ValueMember = "MaDocGia";

            var filterList = new List<DocGiaDTO> { new DocGiaDTO { MaDocGia = 0, HoTen = "-- Tất cả Độc giả --" } };
            filterList.AddRange(dsDG);
            cboDocGiaFilter.DataSource = filterList;
            cboDocGiaFilter.DisplayMember = "HoTen";
            cboDocGiaFilter.ValueMember = "MaDocGia";

            cboSachMuon.DataSource = _libBus.GetDauSach();
            cboSachMuon.DisplayMember = "TenSach";
            cboSachMuon.ValueMember = "MaSach";

            FilterPhieuMuon();
            dgvQuaHan.DataSource = _libBus.GetCanhBaoQuaHan();
            var tk = _libBus.GetThongKe();
            lblTongLuotMuon.Text = $"📊 THỐNG KÊ THEO THỂ LOẠI (GROUP BY) — Tổng lượt mượn (ExecuteScalar): {tk.Tong} cuốn";
            dgvThongKe.DataSource = tk.BangTheLoai;
        }

        private void FilterPhieuMuon()
        {
            int maDG = cboDocGiaFilter.SelectedValue is int id ? id : 0;
            string tt = cboTrangThaiFilter.SelectedItem?.ToString() ?? "Tất cả";
            dgvPhieuMuon.DataSource = _libBus.GetPhieuMuon(maDG, tt);
        }

        // ==========================================
        // PANEL BÀI 6.7: PHÒNG KHÁM & KÊ ĐƠN THUỐC
        // ==========================================
        private void ShowClinicPanel()
        {
            pnlContent.Controls.Clear();
            var tabs = new TabControl { Dock = DockStyle.Fill };

            // Tab 1: Tiếp nhận & Danh mục thuốc
            var tabTiepNhan = new TabPage("1. Tiếp nhận Bệnh nhân & Kho Thuốc");
            var grpBN = new GroupBox { Text = "Tra cứu & Tiếp nhận Bệnh nhân (SqlParameter)", Dock = DockStyle.Top, Height = 135 };
            txtSDT = new TextBox { PlaceholderText = "Số điện thoại...", Location = new Point(15, 28), Width = 150 };
            var btnTim = new Button { Text = "Tra cứu SĐT", Location = new Point(175, 26), Width = 105, Height = 27 };
            txtHoTenBN = new TextBox { PlaceholderText = "Họ tên bệnh nhân...", Location = new Point(290, 28), Width = 200 };
            txtNamSinh = new TextBox { PlaceholderText = "Năm sinh", Location = new Point(500, 28), Width = 80 };
            txtTrieuChung = new TextBox { PlaceholderText = "Triệu chứng...", Location = new Point(15, 68), Width = 265 };
            txtChanDoan = new TextBox { PlaceholderText = "Chẩn đoán ban đầu...", Location = new Point(290, 68), Width = 290 };
            var btnTaoHS = new Button { Text = "Lưu & Tạo Hồ Sơ Khám", Location = new Point(595, 45), Width = 170, Height = 36, BackColor = Color.RoyalBlue, ForeColor = Color.White };

            btnTim.Click += (s, e) => {
                try
                {
                    var bn = _clinicBus.TimBenhNhan(txtSDT.Text);
                    if (bn != null) { _foundMaBN = bn.MaBN; txtHoTenBN.Text = bn.HoTen; txtNamSinh.Text = bn.NamSinh.ToString(); MessageBox.Show("Đã tìm thấy bệnh nhân cũ!"); }
                    else { _foundMaBN = 0; MessageBox.Show("Bệnh nhân mới! Mời nhập thông tin để tạo hồ sơ."); }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            };
            btnTaoHS.Click += (s, e) => {
                try
                {
                    int maHS = _clinicBus.TiepNhan(_foundMaBN, txtHoTenBN.Text, txtSDT.Text, txtNamSinh.Text, txtTrieuChung.Text, txtChanDoan.Text);
                    LoadClinicData();
                    MessageBox.Show($"Tiếp nhận thành công! Mã Hồ sơ: {maHS}");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            grpBN.Controls.AddRange(new Control[] { txtSDT, btnTim, txtHoTenBN, txtNamSinh, txtTrieuChung, txtChanDoan, btnTaoHS });

            var grpThuoc = new GroupBox { Text = "Quản lý Danh mục Thuốc (CRUD)", Dock = DockStyle.Fill };
            var pnlT = new Panel { Dock = DockStyle.Top, Height = 45 };
            txtTenThuoc = new TextBox { PlaceholderText = "Tên thuốc...", Location = new Point(15, 10), Width = 200 };
            txtDVT = new TextBox { PlaceholderText = "ĐVT (Viên/Vỉ)", Location = new Point(225, 10), Width = 100 };
            txtGiaThuoc = new TextBox { PlaceholderText = "Đơn giá", Location = new Point(335, 10), Width = 100 };
            txtSLNhap = new TextBox { PlaceholderText = "SL nhập", Location = new Point(445, 10), Width = 80 };
            var btnThemThuoc = new Button { Text = "Thêm Thuốc Mới", Location = new Point(535, 8), Width = 135, Height = 28, BackColor = Color.SeaGreen, ForeColor = Color.White };
            btnThemThuoc.Click += (s, e) => {
                try { _clinicBus.ThemThuoc(txtTenThuoc.Text, txtDVT.Text, txtGiaThuoc.Text, txtSLNhap.Text); LoadClinicData(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            pnlT.Controls.AddRange(new Control[] { txtTenThuoc, txtDVT, txtGiaThuoc, txtSLNhap, btnThemThuoc });
            dgvThuoc = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            grpThuoc.Controls.Add(dgvThuoc);
            grpThuoc.Controls.Add(pnlT);

            tabTiepNhan.Controls.Add(grpThuoc);
            tabTiepNhan.Controls.Add(grpBN);

            // Tab 2: Kê đơn & Xuất kho (SqlTransaction)
            var tabKeDon = new TabPage("2. Kê Đơn Thuốc & Xuất Kho (SqlTransaction)");
            var pnlKD = new Panel { Dock = DockStyle.Top, Height = 95 };
            cboHoSo = new ComboBox { Location = new Point(15, 12), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            txtTienKham = new TextBox { Text = "100000", Location = new Point(305, 12), Width = 110 };
            cboThuocKe = new ComboBox { Location = new Point(15, 52), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            txtSLKe = new TextBox { Text = "2", Location = new Point(245, 52), Width = 50 };
            txtLieuDung = new TextBox { Text = "Sáng 1, Chiều 1 sau ăn", Location = new Point(305, 52), Width = 200 };
            var btnChonThuoc = new Button { Text = "+ Thêm vào đơn", Location = new Point(515, 50), Width = 120, Height = 28 };
            var btnLuuDon = new Button { Text = "Lưu & Xuất Đơn (Transaction)", Location = new Point(645, 12), Width = 210, Height = 32, BackColor = Color.Crimson, ForeColor = Color.White };
            var btnThanhToanDon = new Button { Text = "Thanh Toán Đơn", Location = new Point(645, 50), Width = 210, Height = 28, BackColor = Color.SeaGreen, ForeColor = Color.White };

            btnChonThuoc.Click += (s, e) => {
                if (cboThuocKe.SelectedItem is ThuocDTO t && int.TryParse(txtSLKe.Text, out int sl) && sl > 0)
                {
                    _donThuocTam.Add(new ChiTietDonThuocDTO { MaThuoc = t.MaThuoc, TenThuoc = t.TenThuoc, SoLuong = sl, DonGia = t.DonGia, LieuDung = txtLieuDung.Text });
                    dgvDonTam.DataSource = null;
                    dgvDonTam.DataSource = _donThuocTam;
                }
            };
            btnLuuDon.Click += (s, e) => {
                try
                {
                    int maHS = Convert.ToInt32(cboHoSo.SelectedValue);
                    var res = _clinicBus.KeDonVaXuatKho(maHS, txtTienKham.Text, _donThuocTam);
                    _lastMaDon = res.MaDon;
                    _donThuocTam.Clear();
                    dgvDonTam.DataSource = null;
                    LoadClinicData();
                    MessageBox.Show($"Lưu đơn #{res.MaDon} thành công! Tổng tiền: {res.TongTien:N0} VNĐ", "Transaction Committed");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Transaction Rollback!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            btnThanhToanDon.Click += (s, e) => {
                if (_lastMaDon > 0) { _clinicBus.ThanhToan(_lastMaDon); MessageBox.Show($"Đơn #{_lastMaDon} đã chuyển sang trạng thái: Đã thanh toán!"); }
            };

            pnlKD.Controls.AddRange(new Control[] { cboHoSo, txtTienKham, cboThuocKe, txtSLKe, txtLieuDung, btnChonThuoc, btnLuuDon, btnThanhToanDon });
            dgvDonTam = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            tabKeDon.Controls.Add(dgvDonTam);
            tabKeDon.Controls.Add(pnlKD);

            tabs.TabPages.AddRange(new[] { tabTiepNhan, tabKeDon });
            pnlContent.Controls.Add(tabs);
            LoadClinicData();
        }

        private void LoadClinicData()
        {
            var dsThuoc = _clinicBus.GetThuoc();
            dgvThuoc.DataSource = dsThuoc;
            cboThuocKe.DataSource = new List<ThuocDTO>(dsThuoc);
            cboThuocKe.DisplayMember = "TenThuoc";
            cboThuocKe.ValueMember = "MaThuoc";

            cboHoSo.DataSource = _clinicBus.GetHoSoKham();
            cboHoSo.DisplayMember = "BenhNhan";
            cboHoSo.ValueMember = "MaHoSo";
        }

        // ==========================================
        // PANEL BÀI 6.8: QUẢN LÝ BÀN BIDA
        // ==========================================
        private void ShowBilliardsPanel()
        {
            pnlContent.Controls.Clear();
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 420 };

            // Trái: Visual Table Grid
            flpBanBida = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };
            split.Panel1.Controls.Add(flpBanBida);

            // Phải: Điều khiển Mở bàn, Gọi dịch vụ (Disconnected) & Thanh toán (Transaction)
            var pnlRightTop = new Panel { Dock = DockStyle.Top, Height = 105 };
            lblBanDangChon = new Label { Text = "Chọn một bàn bida bên trái...", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            var btnMoBan = new Button { Text = "Mở Bàn", Location = new Point(10, 42), Width = 110, Height = 32, BackColor = Color.RoyalBlue, ForeColor = Color.White };
            var btnThanhToanBida = new Button { Text = "Thanh Toán (Transaction)", Location = new Point(130, 42), Width = 190, Height = 32, BackColor = Color.Crimson, ForeColor = Color.White };

            cboMenuDV = new ComboBox { Location = new Point(10, 80), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            txtSLDV = new TextBox { Text = "1", Location = new Point(220, 80), Width = 45 };
            var btnGoiDV = new Button { Text = "+ Gọi Dịch Vụ", Location = new Point(275, 78), Width = 120, Height = 27, BackColor = Color.SeaGreen, ForeColor = Color.White };

            btnMoBan.Click += (s, e) => {
                if (_selectedBan == null) return;
                if (_selectedBan.TrangThai == "Đang chơi") { MessageBox.Show("Bàn này đang chơi!"); return; }
                _bidaBus.MoBan(_selectedBan.MaBan);
                LoadBilliardsData();
            };

            btnGoiDV.Click += (s, e) => {
                try
                {
                    if (cboMenuDV.SelectedItem is MenuDichVuDTO dv)
                    {
                        _bidaBus.GoiDichVu(_activeMaHD, dv.MaDV, txtSLDV.Text, dv.DonGia);
                        dgvChiTietBida.DataSource = _bidaBus.GetChiTietHD(_activeMaHD);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };

            btnThanhToanBida.Click += (s, e) => {
                try
                {
                    if (_selectedBan == null) return;
                    var bill = _bidaBus.ThanhToanBan(_selectedBan.MaBan, _activeMaHD, _selectedBan.GiaTheoGio);
                    MessageBox.Show($"Thanh toán {_selectedBan.TenBan} thành công!\n- Thời gian: {bill.SoGio} giờ\n- Tiền giờ: {bill.TienGio:N0} VNĐ\n- Tiền dịch vụ: {bill.TienDV:N0} VNĐ\n=> TỔNG CỘNG: {bill.TongCong:N0} VNĐ", "Hóa đơn Thanh toán");
                    LoadBilliardsData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };

            pnlRightTop.Height = 115;
            pnlRightTop.Controls.AddRange(new Control[] { lblBanDangChon, btnMoBan, btnThanhToanBida, cboMenuDV, txtSLDV, btnGoiDV });
            dgvChiTietBida = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            split.Panel2.Controls.Add(dgvChiTietBida);
            split.Panel2.Controls.Add(pnlRightTop);

            pnlContent.Controls.Add(split);
            LoadBilliardsData();
        }

        private void LoadBilliardsData()
        {
            flpBanBida.Controls.Clear();
            var dsBan = _bidaBus.GetBan();
            foreach (var ban in dsBan)
            {
                var btn = new Button
                {
                    Text = $"{ban.TenBan}\n({ban.LoaiBan})\n{ban.GiaTheoGio:N0}đ/h\n[{ban.TrangThai}]",
                    Width = 175,
                    Height = 115,
                    Margin = new Padding(8),
                    BackColor = ban.TrangThai == "Trống" ? Color.LightGreen : Color.Orange,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                btn.Click += (s, e) => SelectBanBida(ban);
                flpBanBida.Controls.Add(btn);
            }

            cboMenuDV.DataSource = _bidaBus.GetMenu();
            cboMenuDV.DisplayMember = "TenDV";
            cboMenuDV.ValueMember = "MaDV";

            if (dsBan.Count > 0) SelectBanBida(dsBan[0]);
        }

        private void SelectBanBida(BanBidaDTO ban)
        {
            _selectedBan = ban;
            lblBanDangChon.Text = $"Đang chọn: {ban.TenBan} ({ban.TrangThai})";
            _activeMaHD = _bidaBus.GetMaHDActive(ban.MaBan);
            dgvChiTietBida.DataSource = _activeMaHD > 0 ? _bidaBus.GetChiTietHD(_activeMaHD) : null;
        }
    }
}