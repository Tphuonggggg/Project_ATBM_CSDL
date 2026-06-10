using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class OlsDemoForm : Form
    {
        private readonly string _connectionString;
        private readonly string _username;

        // UI Controls
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblUserInfo;
        
        private GroupBox _grpExplanation;
        private Label _lblExplanation;
        
        private DataGridView _gridNotifications;
        private Button _btnRefresh;
        private Button _btnExit;
        private Label _lblResultCount;

        public OlsDemoForm(string connectionString, string username)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _username = username ?? throw new ArgumentNullException(nameof(username));

            // Cấu hình Form chung
            Text = "OLS Security Policy Demo - NHOM 09";
            Size = new Size(800, 550);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(700, 450);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            BuildUi();
            Load += async (s, e) => await LoadDataAsync();
        }

        private void BuildUi()
        {
            // 1. Header Panel
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.FromArgb(41, 56, 78),
                Padding = new Padding(15, 10, 15, 10)
            };

            _lblTitle = new Label
            {
                Text = "DEMO CHÍNH SÁCH BẢO MẬT ORACLE LABEL SECURITY (OLS)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                Location = new Point(15, 10),
                AutoSize = true
            };

            _lblUserInfo = new Label
            {
                Text = $"Tài khoản đăng nhập: {_username.ToUpper()}  |  Quyền hạn: OLS Demo User",
                ForeColor = Color.FromArgb(200, 214, 229),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                Location = new Point(15, 40),
                AutoSize = true
            };

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblUserInfo);

            // 2. Info/Explanation Panel (Top area of the client region)
            _grpExplanation = new GroupBox
            {
                Text = "Thông tin nhãn bảo mật OLS & Dữ liệu kỳ vọng",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(15, 90),
                Size = new Size(755, 95),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _lblExplanation = new Label
            {
                Text = GetOlsExplanation(_username),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(45, 52, 54),
                Location = new Point(15, 22),
                Size = new Size(725, 65),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            _grpExplanation.Controls.Add(_lblExplanation);

            // 3. Grid for Notifications
            var lblGridHeader = new Label
            {
                Text = "Dữ liệu đọc được từ CQ09.THONGBAO sau khi Oracle Label Security tự lọc:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 56, 78),
                Location = new Point(15, 195),
                AutoSize = true
            };

            _lblResultCount = new Label
            {
                Text = "Đang tải dữ liệu...",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(90, 98, 112),
                Location = new Point(15, 448),
                Size = new Size(440, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            _gridNotifications = new DataGridView
            {
                Location = new Point(15, 220),
                Size = new Size(755, 225),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // 4. Buttons at the Bottom
            _btnRefresh = new Button
            {
                Text = "Tải lại dữ liệu (Refresh)",
                Location = new Point(480, 460),
                Size = new Size(140, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(9, 132, 227),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            _btnExit = new Button
            {
                Text = "Đăng xuất",
                Location = new Point(630, 460),
                Size = new Size(140, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(214, 48, 49),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnExit.FlatAppearance.BorderSize = 0;
            _btnExit.Click += (s, e) => SessionNavigation.Logout(this);

            // Thêm các control vào Form
            Controls.Add(_headerPanel);
            Controls.Add(_grpExplanation);
            Controls.Add(lblGridHeader);
            Controls.Add(_gridNotifications);
            Controls.Add(_lblResultCount);
            Controls.Add(_btnRefresh);
            Controls.Add(_btnExit);
        }

        private async Task LoadDataAsync()
        {
            UseWaitCursor = true;
            _btnRefresh.Enabled = false;

            try
            {
                var dt = await QueryNotificationsAsync();
                _gridNotifications.DataSource = dt;
                _lblResultCount.Text = $"Tài khoản {_username.ToUpper()} đọc được {dt.Rows.Count} thông báo.";

                // Format Grid
                if (_gridNotifications.Columns.Count > 0)
                {
                    _gridNotifications.Columns["MATHONGBAO"].HeaderText = "Mã TB";
                    _gridNotifications.Columns["MATHONGBAO"].FillWeight = 40;
                    _gridNotifications.Columns["NOIDUNG"].HeaderText = "Nội dung thông báo khẩn";
                    _gridNotifications.Columns["NOIDUNG"].FillWeight = 200;
                    _gridNotifications.Columns["NGAYGIO"].HeaderText = "Ngày giờ phát";
                    _gridNotifications.Columns["NGAYGIO"].FillWeight = 90;
                    _gridNotifications.Columns["DIADIEM"].HeaderText = "Địa điểm áp dụng";
                    _gridNotifications.Columns["DIADIEM"].FillWeight = 120;
                    if (_gridNotifications.Columns.Contains("NHAN_OLS"))
                    {
                        _gridNotifications.Columns["NHAN_OLS"].HeaderText = "Nhãn OLS";
                        _gridNotifications.Columns["NHAN_OLS"].FillWeight = 85;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Lỗi khi tải danh sách thông báo OLS: {ex.Message}", "Lỗi Oracle", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefresh.Enabled = true;
                UseWaitCursor = false;
            }
        }

        private async Task<DataTable> QueryNotificationsAsync()
        {
            var sqlBasic = "SELECT MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM FROM CQ09.THONGBAO ORDER BY MATHONGBAO";
            return await OracleHelper.QueryAsync(_connectionString, sqlBasic);
        }

        private string GetOlsExplanation(string username)
        {
            switch (username.ToLower())
            {
                case "u1": 
                    return "• Nhãn OLS: GD:TH,TK,TM:HCM,HP,HN\n• Vai trò: Ban Giám Đốc toàn viện (Cấp độ GD, đầy đủ khoa, đầy đủ cơ sở).\n• Kết quả kỳ vọng: Xem được TOÀN BỘ 7 thông báo (t1 đến t7) trong hệ thống.";
                case "u2": 
                    return "• Nhãn OLS: LDK:TM:HCM\n• Vai trò: Lãnh đạo khoa Tim mạch tại TP.HCM.\n• Kết quả kỳ vọng: Xem được t1 và t3.";
                case "u3": 
                    return "• Nhãn OLS: LDK:TK:HN\n• Vai trò: Lãnh đạo khoa Thần kinh tại Hà Nội.\n• Kết quả kỳ vọng: Xem được t1 và t3.";
                case "u4": 
                    return "• Nhãn OLS: NV:TK:HCM\n• Vai trò: Nhân viên khoa Thần kinh tại TP.HCM (Cấp độ NV, khoa TK, cơ sở HCM).\n• Kết quả kỳ vọng: Chỉ xem được đúng 1 thông báo: t1 (Thông báo chung cấp NV toàn viện).";
                case "u5": 
                    return "• Nhãn OLS: NV:TM:HCM\n• Vai trò: Nhân viên khoa Tim mạch tại TP.HCM (Cấp độ NV, khoa TM, cơ sở HCM).\n• Kết quả kỳ vọng: Chỉ xem được đúng 1 thông báo: t1 (Thông báo chung cấp NV toàn viện).";
                case "u6": 
                    return "• Nhãn OLS: LDK:TM:HCM\n• Vai trò: Lãnh đạo phòng/khoa Tim mạch tại TP.HCM.\n• Kết quả kỳ vọng: Xem được t1 và t3.";
                case "u7": 
                    return "• Nhãn OLS: LDK:TH,TK,TM:HCM,HP,HN\n• Vai trò: Lãnh đạo toàn khoa/toàn cơ sở ở cấp LDK.\n• Kết quả kỳ vọng: Xem được mọi thông báo mức LDK/NV phù hợp, trừ thông báo cấp GD.";
                case "u8": 
                    return "• Nhãn OLS: NV:TH:HN\n• Vai trò: Nhân viên khoa Tiêu hóa tại Hà Nội (Cấp độ NV, khoa TH, cơ sở HN).\n• Kết quả kỳ vọng: Xem được 2 thông báo: t1 (Thông báo chung NV) và t6 (Thông báo riêng khoa Tiêu hóa tại HN).";
                default: 
                    return $"• Tài khoản: {username}\n• Chưa cấu hình mô tả OLS demo cho tài khoản này.";
            }
        }
    }
}
