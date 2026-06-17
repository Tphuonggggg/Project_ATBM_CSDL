using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class TechnicianForm : Form
    {
        private readonly string _connectionString;
        private string _ktvName = "";
        private string _ktvId = "";

        // UI Main Controls
        private TabControl _tabControl;
        private TabPage _tabServices;
        private TabPage _tabProfile;

        // UI Controls - Tab Services (TC#4)
        private DataGridView _gridServices;
        private TextBox _txtMaHSBA;
        private TextBox _txtLoaiDV;
        private TextBox _txtNgayDV;
        private RichTextBox _rtxtKetQua;
        private Button _btnSave;
        private Button _btnRefresh;

        // UI Controls - Tab Profile (TC#5)
        private TextBox _txtEmpId;
        private TextBox _txtEmpName;
        private TextBox _txtEmpGender;
        private TextBox _txtEmpBirthDate;
        private TextBox _txtEmpIdentity;
        private TextBox _txtEmpRole;
        private TextBox _txtEmpDept;
        private TextBox _txtEmpHometown;
        private TextBox _txtEmpPhone;
        private Button _btnSaveProfile;
        private Button _btnRefreshProfile;

        // Generic UI Controls
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private Label _lblKtvName;

        public TechnicianForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            BuildUi();
            InitFormAsync();
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            _btnSave.Enabled = !busy;
            _btnRefresh.Enabled = !busy;
            _gridServices.Enabled = !busy;
            
            _btnSaveProfile.Enabled = !busy;
            _btnRefreshProfile.Enabled = !busy;
        }

        private void BuildUi()
        {
            Controls.Clear();

            Text = "Technician Portal — Hệ thống Nghiệp vụ & Cá nhân (TC#4, TC#5)";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 740);
            MinimumSize = new Size(1000, 620);
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
                Text = "CỔNG THÔNG TIN KỸ THUẬT VIÊN xét nghiệm & chẩn đoán",
                Dock = DockStyle.Left,
                Width = 600,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            _lblKtvName = new Label
            {
                Text = "Kỹ thuật viên: Đang xác minh...",
                Dock = DockStyle.Right,
                Width = 400,
                ForeColor = Color.FromArgb(193, 210, 240),
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 20, 0)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(_lblKtvName);
            pnlHeader.Controls.Add(SessionNavigation.CreateLogoutButton(this));

            // 2. Status Strip
            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(230, 234, 240) };
            _lblStatus = new ToolStripStatusLabel("Sẵn sàng. Đang nạp danh sách dữ liệu từ Oracle...");
            _statusStrip.Items.Add(_lblStatus);

            // 3. Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                ItemSize = new Size(200, 32),
                SizeMode = TabSizeMode.Fixed,
                Padding = new Point(10, 5)
            };

            // Build Tab 1 & Tab 2
            BuildTabServices();
            BuildTabProfile();

            _tabControl.TabPages.Add(_tabServices);
            _tabControl.TabPages.Add(_tabProfile);

            // Gộp tất cả vào Form
            Controls.Add(_tabControl);
            Controls.Add(pnlHeader);
            Controls.Add(_statusStrip);
        }

        // =========================================================================
        // TAB 1: NGHIỆP VỤ CHỈ ĐỊNH DỊCH VỤ (TC#4)
        // =========================================================================
        private void BuildTabServices()
        {
            _tabServices = new TabPage("🔧 1. Nghiệp vụ Chỉ định") { Padding = new Padding(12), BackColor = Color.White };

            var pnlWorkspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            pnlWorkspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Trái: Grid danh sách dịch vụ
            pnlWorkspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Phải: Nhập kết quả chi tiết

            // --- BÊN TRÁI: DANH SÁCH DỊCH VỤ ---
            var grpList = new GroupBox
            {
                Text = "Danh sách dịch vụ cận lâm sàng được phân công (CQ09.vw_ktv_HSBA_DV)",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(33, 64, 107),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(10, 18, 10, 10)
            };

            _gridServices = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            _gridServices.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 238, 243);
            _gridServices.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
            _gridServices.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _gridServices.SelectionChanged += GridServices_SelectionChanged;

            grpList.Controls.Add(_gridServices);

            // --- BÊN PHẢI: CHI TIẾT & NHẬP KẾT QUẢ ---
            var grpDetails = new GroupBox
            {
                Text = "Cập nhật kết quả y khoa (Chỉ sửa trường KẾT QUẢ)",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(33, 64, 107),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(12, 18, 12, 12)
            };

            var tlpDetails = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 9 };
            tlpDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Lbl Mã HSBA
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));  // Txt Mã HSBA
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Lbl Loại DV
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));  // Txt Loại DV
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Lbl Ngày DV
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));  // Txt Ngày DV
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // Lbl Kết quả
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Rich Kết quả (Fill)
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Buttons ở đáy panel chi tiết

            _txtMaHSBA = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 9.5f) };
            _txtLoaiDV = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtNgayDV = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 9.5f) };
            _rtxtKetQua = new RichTextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.75f, FontStyle.Regular) };

            Label L(string s) => new Label { Text = s, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(70, 75, 85), Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) };

            tlpDetails.Controls.Add(L("Mã hồ sơ bệnh án:"), 0, 0); tlpDetails.Controls.Add(_txtMaHSBA, 0, 1);
            tlpDetails.Controls.Add(L("Loại dịch vụ y tế chỉ định:"), 0, 2); tlpDetails.Controls.Add(_txtLoaiDV, 0, 3);
            tlpDetails.Controls.Add(L("Ngày chỉ định dịch vụ:"), 0, 4); tlpDetails.Controls.Add(_txtNgayDV, 0, 5);
            tlpDetails.Controls.Add(L("Kết quả kỹ thuật (KẾT QUẢ):"), 0, 6); tlpDetails.Controls.Add(_rtxtKetQua, 0, 7);

            // Nút bấm lưu & tải lại riêng của Tab chỉ định
            var pnlTabActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            pnlTabActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            pnlTabActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

            _btnRefresh = new Button
            {
                Text = "↻ Tải lại dịch vụ",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            _btnRefresh.Click += async (s, e) => await RefreshListAsync();

            _btnSave = new Button
            {
                Text = "💾 Lưu kết quả y tế",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(33, 64, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.25f, FontStyle.Bold)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await SaveKetQuaAsync();

            pnlTabActions.Controls.Add(_btnRefresh, 0, 0);
            pnlTabActions.Controls.Add(_btnSave, 1, 0);
            tlpDetails.Controls.Add(pnlTabActions, 0, 8);

            grpDetails.Controls.Add(tlpDetails);

            pnlWorkspace.Controls.Add(grpList, 0, 0);
            pnlWorkspace.Controls.Add(grpDetails, 1, 0);
            _tabServices.Controls.Add(pnlWorkspace);
        }

        // =========================================================================
        // TAB 2: THÔNG TIN HỒ SƠ NHÂN SỰ CÁ NHÂN (TC#5)
        // =========================================================================
        private void BuildTabProfile()
        {
            _tabProfile = new TabPage("👤 2. Hồ sơ cá nhân") { Padding = new Padding(20), BackColor = Color.White };

            var tlpProfile = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            tlpProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); // Trái: Chi tiết profile
            tlpProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45)); // Phải: Ghi chú bảo mật/Hình ảnh

            // --- BÊN TRÁI: HỒ SƠ CHI TIẾT ---
            var grpProfile = new GroupBox
            {
                Text = "Thông tin chi tiết cán bộ nhân sự (CQ09.vw_nhanvien_canhan)",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(33, 64, 107),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(15, 20, 15, 15)
            };

            var tlpForm = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 10, Padding = new Padding(10) };
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tlpForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 9; i++) tlpForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            tlpForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Hàng nút bấm cuối

            _txtEmpId = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 10f) };
            _txtEmpName = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtEmpGender = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtEmpBirthDate = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 9.5f) };
            _txtEmpIdentity = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 9.5f) };
            _txtEmpRole = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            _txtEmpDept = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245) };
            
            // Hai trường liên lạc có thể sửa
            _txtEmpHometown = new TextBox { Dock = DockStyle.Fill };
            _txtEmpPhone = new TextBox { Dock = DockStyle.Fill };

            Label LP(string s) => new Label { Text = s, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(70, 75, 85), Font = new Font("Segoe UI", 9.25f, FontStyle.Regular) };

            tlpForm.Controls.Add(LP("Mã nhân viên:"), 0, 0); tlpForm.Controls.Add(_txtEmpId, 1, 0);
            tlpForm.Controls.Add(LP("Họ và tên:"), 0, 1); tlpForm.Controls.Add(_txtEmpName, 1, 1);
            tlpForm.Controls.Add(LP("Giới tính:"), 0, 2); tlpForm.Controls.Add(_txtEmpGender, 1, 2);
            tlpForm.Controls.Add(LP("Ngày sinh:"), 0, 3); tlpForm.Controls.Add(_txtEmpBirthDate, 1, 3);
            tlpForm.Controls.Add(LP("Số CMND / CCCD:"), 0, 4); tlpForm.Controls.Add(_txtEmpIdentity, 1, 4);
            tlpForm.Controls.Add(LP("Vai trò hệ thống:"), 0, 5); tlpForm.Controls.Add(_txtEmpRole, 1, 5);
            tlpForm.Controls.Add(LP("Chuyên khoa:"), 0, 6); tlpForm.Controls.Add(_txtEmpDept, 1, 6);
            
            tlpForm.Controls.Add(new Label { Text = "Quê quán (*):", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.25f, FontStyle.Bold) }, 0, 7);
            tlpForm.Controls.Add(_txtEmpHometown, 1, 7);
            tlpForm.Controls.Add(new Label { Text = "Số điện thoại (*):", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.25f, FontStyle.Bold) }, 0, 8);
            tlpForm.Controls.Add(_txtEmpPhone, 1, 8);

            // Nút bấm lưu thông tin nhân sự
            var pnlProfileActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 10, 0, 0) };
            pnlProfileActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            pnlProfileActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            _btnRefreshProfile = new Button
            {
                Text = "↻ Tải lại hồ sơ",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
                Height = 36
            };
            _btnRefreshProfile.Click += async (s, e) => await LoadProfileAsync();

            _btnSaveProfile = new Button
            {
                Text = "💾 Cập nhật thông tin liên hệ",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(33, 64, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.25f, FontStyle.Bold),
                Height = 36
            };
            _btnSaveProfile.FlatAppearance.BorderSize = 0;
            _btnSaveProfile.Click += async (s, e) => await SaveProfileAsync();

            pnlProfileActions.Controls.Add(_btnRefreshProfile, 0, 0);
            pnlProfileActions.Controls.Add(_btnSaveProfile, 1, 0);

            tlpForm.Controls.Add(pnlProfileActions, 0, 9);
            tlpForm.SetColumnSpan(pnlProfileActions, 2);

            grpProfile.Controls.Add(tlpForm);

            // --- BÊN PHẢI: THÔNG BÁO BẢO MẬT & HƯỚNG DẪN ---
            var grpNotice = new GroupBox
            {
                Text = "Chính sách bảo mật nhân sự (TC#5)",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(120, 50, 50),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Padding = new Padding(15, 20, 15, 15)
            };
            var lblNotice = new Label
            {
                Text = "• Theo Quy định an toàn y tế và chính sách đóng bảo mật hệ thống (TC#5):\n\n" +
                       "1. Nhân viên y tế KHÔNG ĐƯỢC PHÉP tự ý thay đổi các thông tin định danh như Họ tên, Ngày sinh, Số CMND/CCCD, Vai trò hệ thống hay Chuyên khoa điều phối. Các thông tin này do phòng Hành chính & Nhân sự quản trị.\n\n" +
                       "2. Bạn ĐƯỢC QUYỀN tự cập nhật thông tin liên lạc cá nhân bao gồm: Quê quán và Số điện thoại di động thông qua cổng thông tin này.\n\n" +
                       "3. Mọi hành vi cập nhật hồ sơ cá nhân đều được lưu vết kiểm toán (Audit Trail) trong Oracle DB để phục vụ công tác giám sát định kỳ.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(80, 85, 95),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Italic),
                TextAlign = ContentAlignment.TopLeft
            };
            grpNotice.Controls.Add(lblNotice);

            tlpProfile.Controls.Add(grpProfile, 0, 0);
            tlpProfile.Controls.Add(grpNotice, 1, 0);
            _tabProfile.Controls.Add(tlpProfile);
        }

        // =========================================================================
        // ORACLE DATABASE LOGIC & CONNECTIONS
        // =========================================================================
        private async void InitFormAsync()
        {
            SetBusy(true);
            try
            {
                // 1. Tải định danh KTV
                var sqlIdentity = "SELECT MA_NGUOIDUNG, HOTEN FROM CQ09.V_MY_ACCOUNT";
                var dtId = await OracleSql.QueryAsync(_connectionString, sqlIdentity);
                if (dtId != null && dtId.Rows.Count > 0)
                {
                    _ktvId = dtId.Rows[0]["MA_NGUOIDUNG"]?.ToString() ?? "";
                    _ktvName = dtId.Rows[0]["HOTEN"]?.ToString() ?? "";
                    _lblKtvName.Text = $"Kỹ thuật viên: {_ktvName} ({_ktvId})";
                }

                // 2. Tải danh sách dịch vụ y khoa (Tab 1)
                await RefreshListAsync();

                // 3. Tải hồ sơ nhân sự (Tab 2)
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // --- Logic Tab 1: Dịch vụ y tế ---
        private async Task RefreshListAsync()
        {
            SetBusy(true);
            _lblStatus.Text = "Đang tải danh sách dịch vụ do bạn phụ trách...";
            try
            {
                var sql = "SELECT MAHSBA AS \"Mã Bệnh Án\", " +
                          "LOAIDV AS \"Loại Dịch Vụ\", " +
                          "TO_CHAR(NGAYDV, 'DD/MM/YYYY') AS \"Ngày Chỉ Định\", " +
                          "MAKTV AS \"Mã KTV\", " +
                          "KETQUA AS \"Kết Quả\" " +
                          "FROM CQ09.vw_ktv_HSBA_DV " +
                          "ORDER BY TO_DATE(\"Ngày Chỉ Định\", 'DD/MM/YYYY') DESC";

                var dt = await OracleSql.QueryAsync(_connectionString, sql);
                _gridServices.DataSource = dt;
                
                _lblStatus.Text = $"Đã tải {_gridServices.Rows.Count} chỉ định dịch vụ y tế.";
                
                _txtMaHSBA.Text = "";
                _txtLoaiDV.Text = "";
                _txtNgayDV.Text = "";
                _rtxtKetQua.Text = "";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi khi tải dữ liệu từ Oracle.";
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void GridServices_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (_gridServices.CurrentRow != null)
                {
                    var r = _gridServices.CurrentRow;
                    _txtMaHSBA.Text = r.Cells["Mã Bệnh Án"].Value?.ToString();
                    _txtLoaiDV.Text = r.Cells["Loại Dịch Vụ"].Value?.ToString();
                    _txtNgayDV.Text = r.Cells["Ngày Chỉ Định"].Value?.ToString();
                    _rtxtKetQua.Text = r.Cells["Kết Quả"].Value?.ToString();
                }
            }
            catch { }
        }

        private async Task SaveKetQuaAsync()
        {
            var maHSBA = _txtMaHSBA.Text.Trim();
            var loaiDV = _txtLoaiDV.Text.Trim();
            var ngayDV = _txtNgayDV.Text.Trim();
            var ketQua = _rtxtKetQua.Text;

            if (string.IsNullOrEmpty(maHSBA) || string.IsNullOrEmpty(loaiDV) || string.IsNullOrEmpty(ngayDV))
            {
                MessageBox.Show(this, "Vui lòng chọn 1 dịch vụ y tế trên bảng danh sách trước khi lưu.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            _lblStatus.Text = "Đang cập nhật kết quả dịch vụ lên Oracle...";
            try
            {
                var sql = "UPDATE CQ09.vw_ktv_HSBA_DV SET " +
                          "KETQUA = " + OracleSql.QLit(ketQua) + " " +
                          "WHERE MAHSBA = " + OracleSql.QLit(maHSBA) + " " +
                          "AND LOAIDV = " + OracleSql.QLit(loaiDV) + " " +
                          "AND NGAYDV = TO_DATE(" + OracleSql.QLit(ngayDV) + ", 'DD/MM/YYYY')";

                int rows = await OracleSql.ExecuteAsync(_connectionString, sql);
                if (rows > 0)
                {
                    _lblStatus.Text = $"Cập nhật kết quả cho hồ sơ {maHSBA} thành công.";
                    MessageBox.Show(this, "Cập nhật kết quả thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RefreshListAsync();
                }
                else
                {
                    _lblStatus.Text = "Cập nhật thất bại. Số dòng ảnh hưởng là 0.";
                    MessageBox.Show(this, "Không thể cập nhật kết quả. Có thể dịch vụ này không do bạn phụ trách.", "Lỗi phân quyền", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi ghi kết quả Oracle.";
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // --- Logic Tab 2: Hồ sơ nhân sự cá nhân ---
        private async Task LoadProfileAsync()
        {
            SetBusy(true);
            _lblStatus.Text = "Đang tải thông tin hồ sơ nhân sự từ Oracle...";
            try
            {
                var sql = "SELECT MANV, HOTEN, PHAI, TO_CHAR(NGAYSINH, 'DD/MM/YYYY') AS NGAYSINH, CMND, QUEQUAN, SODT, VAITRO, CHUYENKHOA FROM CQ09.vw_nhanvien_canhan WHERE MANV = SYS_CONTEXT('USERENV', 'SESSION_USER')";
                var dt = await OracleSql.QueryAsync(_connectionString, sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    _txtEmpId.Text = r["MANV"]?.ToString();
                    _txtEmpName.Text = r["HOTEN"]?.ToString();
                    _txtEmpGender.Text = r["PHAI"]?.ToString();
                    _txtEmpBirthDate.Text = r["NGAYSINH"]?.ToString();
                    _txtEmpIdentity.Text = r["CMND"]?.ToString();
                    _txtEmpRole.Text = r["VAITRO"]?.ToString();
                    _txtEmpDept.Text = r["CHUYENKHOA"]?.ToString();
                    
                    _txtEmpHometown.Text = r["QUEQUAN"]?.ToString();
                    _txtEmpPhone.Text = r["SODT"]?.ToString();

                    _lblStatus.Text = "Đã tải xong thông tin hồ sơ nhân sự.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi nạp dữ liệu hồ sơ nhân viên.";
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrEmpty(_ktvId))
            {
                MessageBox.Show(this, "Không tìm thấy mã nhân viên hợp lệ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            _lblStatus.Text = "Đang cập nhật hồ sơ nhân sự lên Oracle...";
            try
            {
                // Cập nhật thông qua View cá nhân (được bảo mật mức dòng)
                var sql = "UPDATE CQ09.vw_nhanvien_canhan SET " +
                          "QUEQUAN = " + OracleSql.QLit(_txtEmpHometown.Text) + ", " +
                          "SODT = " + OracleSql.QLit(_txtEmpPhone.Text) + " " +
                          "WHERE MANV = " + OracleSql.QLit(_ktvId);

                int rows = await OracleSql.ExecuteAsync(_connectionString, sql);
                if (rows > 0)
                {
                    _lblStatus.Text = "Cập nhật hồ sơ cá nhân thành công.";
                    MessageBox.Show(this, "Cập nhật thông tin liên hệ thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadProfileAsync();
                }
                else
                {
                    _lblStatus.Text = "Cập nhật thất bại.";
                    MessageBox.Show(this, "Không thể cập nhật hồ sơ cá nhân.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Lỗi cập nhật hồ sơ nhân sự.";
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(this, "Lỗi xảy ra trong quá trình kết nối Oracle:\n" + ex.Message, "NHOM 09 - Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
