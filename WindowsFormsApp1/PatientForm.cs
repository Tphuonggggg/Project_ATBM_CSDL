using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class PatientForm : Form
    {
        private readonly string _connectionString;
        private string _maBN = "";

        // UI Controls
        private TextBox _txtMaBN;
        private TextBox _txtTenBN;
        private TextBox _txtPhai;
        private TextBox _txtNgaySinh;
        private TextBox _txtCCCD;
        private TextBox _txtSoNha;
        private TextBox _txtTenDuong;
        private TextBox _txtQuanHuyen;
        private TextBox _txtTinhTP;
        private RichTextBox _rtxtTienSuBenh;
        private RichTextBox _rtxtTienSuGiaDinh;
        private TextBox _txtDiUngThuoc;
        
        private Button _btnSave;
        private Button _btnRefresh;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private Label _lblPatientName;

        public PatientForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            
            BuildUi();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            _btnSave.Enabled = !busy;
            _btnRefresh.Enabled = !busy;
        }

        private void BuildUi()
        {
            Controls.Clear();

            Text = "Patient Portal — Cổng thông tin Bệnh nhân (TC#5)";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1180, 720);
            MinimumSize = new Size(1000, 600);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            BackColor = Color.FromArgb(245, 247, 250);

            // 1. Header Panel
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(33, 64, 107)
            };
            var lblTitle = new Label
            {
                Text = "CỒNG THÔNG TIN BỆNH NHÂN - TRA CỨU & CẬP NHẬT HỒ SƠ",
                Dock = DockStyle.Left,
                Width = 600,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            _lblPatientName = new Label
            {
                Text = "Bệnh nhân: Đang xác minh...",
                Dock = DockStyle.Right,
                Width = 400,
                ForeColor = Color.FromArgb(193, 210, 240),
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 20, 0)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(_lblPatientName);
            pnlHeader.Controls.Add(SessionNavigation.CreateLogoutButton(this));

            // 2. Status Strip
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(230, 234, 240) };
            _lblStatus = new ToolStripStatusLabel("Sẵn sàng. Đang nạp hồ sơ cá nhân bệnh nhân từ Oracle...");
            _statusStrip.Items.Add(_lblStatus);

            // 3. Workspace Layout
            var pnlWorkspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            pnlWorkspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); // Trái: Hành chính & Liên lạc
            pnlWorkspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); // Phải: Tiền sử & Dị ứng

            // --- CỘT TRÁI: HÀNH CHÍNH & LIÊN LẠC ---
            var pnlLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            // Nhóm 1: Thông tin hành chính (Chỉ xem - TC#5)
            var grpHanhChinh = new GroupBox
            {
                Text = "I. Thông tin hành chính bệnh nhân (Chỉ xem)",
                Width = 530,
                Height = 240,
                ForeColor = Color.FromArgb(33, 64, 107),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(10, 18, 10, 10)
            };
            var tlpAdmin = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(10) };
            tlpAdmin.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpAdmin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) tlpAdmin.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            _txtMaBN = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 10f) };
            _txtTenBN = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtPhai = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtNgaySinh = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtCCCD = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };

            Label L(string s) => new Label { Text = s, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(70, 75, 85), Font = new Font("Segoe UI", 9.25f, FontStyle.Regular) };

            tlpAdmin.Controls.Add(L("Mã số bệnh nhân:"), 0, 0); tlpAdmin.Controls.Add(_txtMaBN, 1, 0);
            tlpAdmin.Controls.Add(L("Họ và tên bệnh nhân:"), 0, 1); tlpAdmin.Controls.Add(_txtTenBN, 1, 1);
            tlpAdmin.Controls.Add(L("Giới tính:"), 0, 2); tlpAdmin.Controls.Add(_txtPhai, 1, 2);
            tlpAdmin.Controls.Add(L("Ngày sinh:"), 0, 3); tlpAdmin.Controls.Add(_txtNgaySinh, 1, 3);
            tlpAdmin.Controls.Add(L("Số căn cước (CCCD):"), 0, 4); tlpAdmin.Controls.Add(_txtCCCD, 1, 4);
            grpHanhChinh.Controls.Add(tlpAdmin);

            // Nhóm 2: Thông tin liên lạc (Cho phép sửa - TC#5)
            var grpLienLac = new GroupBox
            {
                Text = "II. Địa chỉ liên lạc thường trú (Có thể tự cập nhật)",
                Width = 530,
                Height = 200,
                ForeColor = Color.FromArgb(33, 64, 107),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(10, 18, 10, 10)
            };
            var tlpContact = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(10) };
            tlpContact.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tlpContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) tlpContact.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            _txtSoNha = new TextBox { Dock = DockStyle.Fill };
            _txtTenDuong = new TextBox { Dock = DockStyle.Fill };
            _txtQuanHuyen = new TextBox { Dock = DockStyle.Fill };
            _txtTinhTP = new TextBox { Dock = DockStyle.Fill };

            tlpContact.Controls.Add(L("Số nhà:"), 0, 0); tlpContact.Controls.Add(_txtSoNha, 1, 0);
            tlpContact.Controls.Add(L("Tên đường:"), 0, 1); tlpContact.Controls.Add(_txtTenDuong, 1, 1);
            tlpContact.Controls.Add(L("Quận / Huyện:"), 0, 2); tlpContact.Controls.Add(_txtQuanHuyen, 1, 2);
            tlpContact.Controls.Add(L("Tỉnh / Thành phố:"), 0, 3); tlpContact.Controls.Add(_txtTinhTP, 1, 3);
            grpLienLac.Controls.Add(tlpContact);

            pnlLeft.Controls.Add(grpHanhChinh);
            pnlLeft.Controls.Add(grpLienLac);

            // --- CỘT PHẢI: TIỀN SỬ BỆNH LÝ & BẢO MẬT ---
            var pnlRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            // Nhóm 3: Tiền sử & y khoa
            var grpMedical = new GroupBox
            {
                Text = "III. Tiền sử bệnh lý cá nhân & Dị ứng (Có thể tự cập nhật)",
                Width = 530,
                Height = 360,
                ForeColor = Color.FromArgb(33, 64, 107),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(12, 18, 12, 12)
            };
            var tlpMedical = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(0) };
            tlpMedical.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpMedical.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Lbl 1
            tlpMedical.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // Rich 1
            tlpMedical.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Lbl 2
            tlpMedical.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // Rich 2
            tlpMedical.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // Lbl 3
            tlpMedical.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Text 3

            _rtxtTienSuBenh = new RichTextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.25f, FontStyle.Regular) };
            _rtxtTienSuGiaDinh = new RichTextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.25f, FontStyle.Regular) };
            _txtDiUngThuoc = new TextBox { Dock = DockStyle.Fill };

            tlpMedical.Controls.Add(new Label { Text = "Tiền sử bệnh lý cá nhân:", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(70, 75, 85), Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) }, 0, 0);
            tlpMedical.Controls.Add(_rtxtTienSuBenh, 0, 1);
            tlpMedical.Controls.Add(new Label { Text = "Tiền sử bệnh lý gia đình:", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(70, 75, 85), Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) }, 0, 2);
            tlpMedical.Controls.Add(_rtxtTienSuGiaDinh, 0, 3);
            tlpMedical.Controls.Add(new Label { Text = "Các dị ứng thuốc đã ghi nhận:", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(70, 75, 85), Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) }, 0, 4);
            tlpMedical.Controls.Add(_txtDiUngThuoc, 0, 5);
            grpMedical.Controls.Add(tlpMedical);

            // Nhóm 4: Hộp hướng dẫn bảo mật
            var grpNotice = new GroupBox
            {
                Text = "Chính sách quyền riêng tư bệnh nhân (TC#5)",
                Width = 530,
                Height = 100,
                ForeColor = Color.FromArgb(120, 50, 50),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(12)
            };
            var lblNotice = new Label
            {
                Text = "• Theo điều khoản TC#5: Bạn chỉ xem được thông tin của chính mình và có quyền thay đổi địa chỉ thường trú, dị ứng thuốc và tiền sử. Các cột định danh cơ bản bắt buộc khóa để đảm bảo an toàn hồ sơ y khoa.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(80, 85, 95),
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                TextAlign = ContentAlignment.TopLeft
            };
            grpNotice.Controls.Add(lblNotice);

            pnlRight.Controls.Add(grpMedical);
            pnlRight.Controls.Add(grpNotice);

            pnlWorkspace.Controls.Add(pnlLeft, 0, 0);
            pnlWorkspace.Controls.Add(pnlRight, 1, 0);

            // 4. Action Panel ở đáy
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(238, 241, 246),
                Padding = new Padding(15, 10, 15, 10)
            };

            _btnSave = new Button
            {
                Text = "💾 Lưu các thay đổi",
                Width = 200,
                Height = 40,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(33, 64, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveChangesAsync();

            _btnRefresh = new Button
            {
                Text = "↻ Tải lại hồ sơ",
                Width = 160,
                Height = 40,
                Dock = DockStyle.Left,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
            };
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            pnlFooter.Controls.Add(_btnRefresh);
            pnlFooter.Controls.Add(_btnSave);

            // Gộp tất cả vào Form
            Controls.Add(pnlWorkspace);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Controls.Add(_statusStrip);
        }

        private async Task LoadDataAsync()
        {
            SetBusy(true);
            _lblStatus.Text = "Đang tải hồ sơ bệnh nhân từ Oracle...";
            try
            {
                // Truy vấn từ View bảo mật CQ09.vw_benhnhan
                var sql = "SELECT MABN, TENBN, PHAI, TO_CHAR(NGAYSINH, 'DD/MM/YYYY') AS NGAYSINH, CCCD, SONHA, TENDUONG, QUANHUYEN, TINHTP, TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC FROM CQ09.vw_benhnhan";
                var dt = await OracleSql.QueryAsync(_connectionString, sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    _maBN = r["MABN"]?.ToString() ?? "";
                    
                    _txtMaBN.Text = _maBN;
                    _txtTenBN.Text = r["TENBN"]?.ToString();
                    _txtPhai.Text = r["PHAI"]?.ToString();
                    _txtNgaySinh.Text = r["NGAYSINH"]?.ToString();
                    _txtCCCD.Text = r["CCCD"]?.ToString();
                    
                    _txtSoNha.Text = r["SONHA"]?.ToString();
                    _txtTenDuong.Text = r["TENDUONG"]?.ToString();
                    _txtQuanHuyen.Text = r["QUANHUYEN"]?.ToString();
                    _txtTinhTP.Text = r["TINHTP"]?.ToString();
                    
                    _rtxtTienSuBenh.Text = r["TIENSUBENH"]?.ToString();
                    _rtxtTienSuGiaDinh.Text = r["TIENSUBENHGD"]?.ToString();
                    _txtDiUngThuoc.Text = r["DIUNGTHUOC"]?.ToString();

                    _lblPatientName.Text = $"Bệnh nhân: {_txtTenBN.Text} ({_maBN})";
                    _lblStatus.Text = $"Tải thành công hồ sơ bệnh nhân: {_maBN}";
                }
                else
                {
                    _lblStatus.Text = "Lỗi: Không tìm thấy hồ sơ cá nhân trong CSDL.";
                    MessageBox.Show(this, "Không thể tìm thấy dòng dữ liệu cá nhân của bạn trong cơ sở dữ liệu. Vui lòng liên hệ DBA.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi nạp dữ liệu Oracle.";
                MessageBox.Show(this, "Lỗi khi nạp dữ liệu từ Oracle:\n" + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SaveChangesAsync()
        {
            if (string.IsNullOrEmpty(_maBN))
            {
                MessageBox.Show(this, "Không tìm thấy mã bệnh nhân hợp lệ để cập nhật.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            _lblStatus.Text = "Đang lưu thay đổi xuống Oracle...";
            try
            {
                // Cập nhật thông qua View CQ09.vw_benhnhan (sử dụng QLit để tránh SQL Injection)
                var sql = "UPDATE CQ09.vw_benhnhan SET " +
                          "SONHA = " + OracleSql.QLit(_txtSoNha.Text) + ", " +
                          "TENDUONG = " + OracleSql.QLit(_txtTenDuong.Text) + ", " +
                          "QUANHUYEN = " + OracleSql.QLit(_txtQuanHuyen.Text) + ", " +
                          "TINHTP = " + OracleSql.QLit(_txtTinhTP.Text) + ", " +
                          "TIENSUBENH = " + OracleSql.QLit(_rtxtTienSuBenh.Text) + ", " +
                          "TIENSUBENHGD = " + OracleSql.QLit(_rtxtTienSuGiaDinh.Text) + ", " +
                          "DIUNGTHUOC = " + OracleSql.QLit(_txtDiUngThuoc.Text) + " " +
                          "WHERE MABN = " + OracleSql.QLit(_maBN);

                int rows = await OracleSql.ExecuteAsync(_connectionString, sql);
                if (rows > 0)
                {
                    _lblStatus.Text = "Lưu hồ sơ bệnh án cá nhân thành công.";
                    MessageBox.Show(this, "Cập nhật hồ sơ bệnh nhân thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                else
                {
                    _lblStatus.Text = "Không có dòng dữ liệu nào được cập nhật.";
                    MessageBox.Show(this, "Cập nhật thất bại. Vui lòng thử lại hoặc liên hệ DBA.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi khi ghi dữ liệu xuống Oracle.";
                MessageBox.Show(this, "Lỗi lưu dữ liệu:\n" + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}
