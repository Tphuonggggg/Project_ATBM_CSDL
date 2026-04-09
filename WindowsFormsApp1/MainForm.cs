using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        private readonly string _connectionString;
        private readonly AdminService _admin;

        // ===== Controls (dựng UI bằng code để đúng đề bài 6 tab) =====
        private TabControl _tabs;

        // Tab 1: User
        private DataGridView _gridUsers;
        private TextBox _txtUserName;
        private TextBox _txtUserPassword;
        private ComboBox _cboAccountStatus;
        private Button _btnUserAdd;
        private Button _btnUserEdit;
        private Button _btnUserDelete;
        private Button _btnUserRefresh;

        // Tab 2: Role
        private DataGridView _gridRoles;
        private TextBox _txtRoleName;
        private CheckBox _chkRolePassword;
        private TextBox _txtRolePassword;
        private Button _btnRoleAdd;
        private Button _btnRoleEdit;
        private Button _btnRoleDelete;
        private Button _btnRoleRefresh;

        // Tab 3: Grant
        private ComboBox _cboGrantGrantee;
        private GroupBox _grpGrantSysPriv;
        private ComboBox _cboGrantSysPriv;
        private CheckBox _chkGrantSysWithAdmin;
        private Button _btnGrantSys;
        private GroupBox _grpGrantRole;
        private ComboBox _cboGrantRole;
        private CheckBox _chkGrantRoleWithAdmin;
        private Button _btnGrantRole;
        private GroupBox _grpGrantObjPriv;
        private ComboBox _cboGrantObjType;
        private ComboBox _cboGrantObjPriv;
        private TextBox _txtGrantObjName; // OWNER.OBJECT
        private TextBox _txtGrantObjCols;
        private CheckBox _chkGrantObjWithGrant;
        private Button _btnGrantObj;

        // Tab 4: Revoke
        private ComboBox _cboRevokeGrantee;
        private GroupBox _grpRevokeSysPriv;
        private ComboBox _cboRevokeSysPriv;
        private Button _btnRevokeSys;
        private GroupBox _grpRevokeRole;
        private ComboBox _cboRevokeRole;
        private Button _btnRevokeRole;
        private GroupBox _grpRevokeObjPriv;
        private ComboBox _cboRevokeObjType;
        private ComboBox _cboRevokeObjPriv;
        private TextBox _txtRevokeObjName;
        private TextBox _txtRevokeObjCols;
        private Button _btnRevokeObj;

        // Tab 5: Xem quyền
        private TextBox _txtViewName;
        private RadioButton _radViewUser;
        private RadioButton _radViewRole;
        private Button _btnViewPrivs;
        private DataGridView _gridViewSys;
        private DataGridView _gridViewRoles;
        private DataGridView _gridViewObj;

        // Tab 6: Object Browser
        private ComboBox _cboOwner;
        private ComboBox _cboObjectType;
        private Button _btnBrowse;
        private DataGridView _gridObjects;
        private DataGridView _gridColumns;

        public MainForm(string connectionString)
        {
            InitializeComponent();

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _admin = new AdminService(_connectionString);

            BuildUi();
        }

        // Giữ form responsive khi chạy async
        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            if (_tabs != null) _tabs.Enabled = !busy;
        }

        private ToolTip _tips;
        private StatusStrip _status;
        private ToolStripStatusLabel _lblStatus;

        private void BuildUi()
        {
            // Bỏ toàn bộ UI cũ do Designer tạo (tab Kết nối + các tab cũ)
            Controls.Clear();

            Text = "NHOM 09 — Quản trị CSDL Oracle";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 780);
            Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);
            BackColor = Color.FromArgb(245, 247, 250);

            _tips = new ToolTip { AutoPopDelay = 8000, InitialDelay = 400, ReshowDelay = 200, ShowAlways = true };

            // Thanh tiêu đề phía trên
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(33, 64, 107)
            };
            var lblTitle = new Label
            {
                Text = "NHOM 09 — Ứng dụng Quản trị CSDL Oracle",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };
            header.Controls.Add(lblTitle);

            // Thanh trạng thái phía dưới
            _status = new StatusStrip { BackColor = Color.FromArgb(230, 234, 240) };
            _lblStatus = new ToolStripStatusLabel("Sẵn sàng. Hãy chọn tab để bắt đầu thao tác trực tiếp với Oracle.");
            _status.Items.Add(_lblStatus);

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal,
                ItemSize = new Size(140, 28),
                SizeMode = TabSizeMode.Fixed,
                Padding = new Point(14, 6)
            };

            Controls.Add(_tabs);
            Controls.Add(_status);
            Controls.Add(header);

            _tabs.TabPages.Add(BuildTabUsers());
            _tabs.TabPages.Add(BuildTabRoles());
            _tabs.TabPages.Add(BuildTabGrant());
            _tabs.TabPages.Add(BuildTabRevoke());
            _tabs.TabPages.Add(BuildTabViewPrivileges());
            _tabs.TabPages.Add(BuildTabObjectBrowser());

            _tabs.SelectedIndexChanged += (s, e) =>
                SetStatus($"Tab hiện tại: {_tabs.SelectedTab?.Text}");
        }

        private void SetStatus(string text)
        {
            if (_lblStatus != null) _lblStatus.Text = text;
        }

        private static Label Hint(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.FromArgb(90, 95, 105),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
            Padding = new Padding(4, 2, 0, 4)
        };

        private static Button PrimaryButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(33, 64, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                Height = 32
            };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 32
            };
        }

        private static Button DangerButton(string text)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                Height = 32
            };
        }

        private TabPage BuildTabUsers()
        {
            var tab = new TabPage("1. User") { Padding = new Padding(12), BackColor = Color.White };

            var form = new GroupBox
            {
                Text = "Thông tin user",
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(12, 18, 12, 12)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            _txtUserName = new TextBox { Dock = DockStyle.Fill };
            _txtUserPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            _cboAccountStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboAccountStatus.Items.AddRange(new object[] { "OPEN", "LOCKED" });
            _cboAccountStatus.SelectedIndex = 0;

            grid.Controls.Add(new Label { Text = "Username:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            grid.Controls.Add(_txtUserName, 1, 0);
            grid.Controls.Add(new Label { Text = "Password:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 2, 0);
            grid.Controls.Add(_txtUserPassword, 3, 0);

            grid.Controls.Add(new Label { Text = "Account status:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            grid.Controls.Add(_cboAccountStatus, 1, 1);

            var btnBar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) btnBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _btnUserAdd = PrimaryButton("＋ Thêm user");
            _btnUserEdit = SecondaryButton("✎ Sửa (password/khóa)");
            _btnUserDelete = DangerButton("✕ Xóa (DROP CASCADE)");
            _btnUserRefresh = SecondaryButton("↻ Tải lại");

            btnBar.Controls.Add(_btnUserAdd, 0, 0);
            btnBar.Controls.Add(_btnUserEdit, 1, 0);
            btnBar.Controls.Add(_btnUserDelete, 2, 0);
            btnBar.Controls.Add(_btnUserRefresh, 3, 0);

            grid.Controls.Add(btnBar, 0, 2);
            grid.SetColumnSpan(btnBar, 4);

            form.Controls.Add(grid);

            var hint = Hint("• Chọn 1 dòng trên lưới để điền sẵn username. • Nhập password mới để đổi mật khẩu. • DROP dùng CASCADE nên sẽ xóa mọi object thuộc user.");

            _gridUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };

            var gridBox = new GroupBox { Text = "Danh sách user (dba_users)", Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 8) };
            gridBox.Controls.Add(_gridUsers);

            _btnUserRefresh.Click += async (s, e) => await RefreshUsersAsync();
            _btnUserAdd.Click += async (s, e) => await UserAddAsync();
            _btnUserEdit.Click += async (s, e) => await UserEditAsync();
            _btnUserDelete.Click += async (s, e) => await UserDeleteAsync();
            _gridUsers.SelectionChanged += (s, e) => HydrateUserInputsFromGrid();

            _tips.SetToolTip(_btnUserAdd, "CREATE USER \"name\" IDENTIFIED BY \"password\"");
            _tips.SetToolTip(_btnUserEdit, "ALTER USER: đổi password và/hoặc ACCOUNT LOCK/UNLOCK");
            _tips.SetToolTip(_btnUserDelete, "DROP USER \"name\" CASCADE");
            _tips.SetToolTip(_btnUserRefresh, "SELECT username, account_status, created FROM dba_users");

            tab.Controls.Add(gridBox);
            tab.Controls.Add(hint);
            tab.Controls.Add(form);
            return tab;
        }

        private TabPage BuildTabRoles()
        {
            var tab = new TabPage("2. Role") { Padding = new Padding(12), BackColor = Color.White };

            var form = new GroupBox
            {
                Text = "Thông tin role",
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(12, 18, 12, 12)
            };

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            _txtRoleName = new TextBox { Dock = DockStyle.Fill };
            _chkRolePassword = new CheckBox { Text = "Role có password", Dock = DockStyle.Fill };
            _txtRolePassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Enabled = false };
            _chkRolePassword.CheckedChanged += (s, e) => _txtRolePassword.Enabled = _chkRolePassword.Checked;

            grid.Controls.Add(new Label { Text = "Role name:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            grid.Controls.Add(_txtRoleName, 1, 0);
            grid.Controls.Add(_chkRolePassword, 2, 0);
            grid.Controls.Add(_txtRolePassword, 3, 0);

            var btnBar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) btnBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _btnRoleAdd = PrimaryButton("＋ Thêm role");
            _btnRoleEdit = SecondaryButton("✎ Sửa (không hỗ trợ)");
            _btnRoleEdit.Enabled = false;
            _btnRoleDelete = DangerButton("✕ Xóa role");
            _btnRoleRefresh = SecondaryButton("↻ Tải lại");

            btnBar.Controls.Add(_btnRoleAdd, 0, 0);
            btnBar.Controls.Add(_btnRoleEdit, 1, 0);
            btnBar.Controls.Add(_btnRoleDelete, 2, 0);
            btnBar.Controls.Add(_btnRoleRefresh, 3, 0);

            grid.Controls.Add(btnBar, 0, 2);
            grid.SetColumnSpan(btnBar, 4);
            form.Controls.Add(grid);

            var hint = Hint("• Oracle không hỗ trợ ALTER ROLE đổi tên. • Tick \"Role có password\" để tạo role dạng IDENTIFIED BY.");

            _gridRoles = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            var gridBox = new GroupBox { Text = "Danh sách role (dba_roles)", Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 8) };
            gridBox.Controls.Add(_gridRoles);

            _btnRoleRefresh.Click += async (s, e) => await RefreshRolesAsync();
            _btnRoleAdd.Click += async (s, e) => await RoleAddAsync();
            _btnRoleDelete.Click += async (s, e) => await RoleDeleteAsync();
            _gridRoles.SelectionChanged += (s, e) => HydrateRoleInputsFromGrid();

            _tips.SetToolTip(_btnRoleAdd, "CREATE ROLE \"name\" [IDENTIFIED BY \"password\"]");
            _tips.SetToolTip(_btnRoleDelete, "DROP ROLE \"name\"");

            tab.Controls.Add(gridBox);
            tab.Controls.Add(hint);
            tab.Controls.Add(form);
            return tab;
        }

        private TabPage BuildTabGrant()
        {
            var tab = new TabPage("3. Grant — Cấp quyền") { Padding = new Padding(12), BackColor = Color.White };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "Người/role nhận quyền (Grantee):", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 10, 6, 0) });
            _cboGrantGrantee = new ComboBox { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnLoad = new Button { Text = "↻ Tải danh sách users/roles", Width = 200, Height = 28, BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLoad.Click += async (s, e) => await LoadGranteesAsync();
            top.Controls.Add(_cboGrantGrantee);
            top.Controls.Add(btnLoad);

            _grpGrantSysPriv = new GroupBox { Text = "① Cấp quyền hệ thống (system privilege)", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10) };
            _cboGrantSysPriv = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _chkGrantSysWithAdmin = new CheckBox { Text = "WITH ADMIN OPTION (cho phép grantee cấp lại quyền này)", Dock = DockStyle.Top, Height = 26 };
            _btnGrantSys = PrimaryButton("Cấp system privilege");
            _btnGrantSys.Dock = DockStyle.Top;
            _btnGrantSys.Click += async (s, e) => await GrantSysAsync();
            _grpGrantSysPriv.Controls.Add(_btnGrantSys);
            _grpGrantSysPriv.Controls.Add(_chkGrantSysWithAdmin);
            _grpGrantSysPriv.Controls.Add(_cboGrantSysPriv);

            _grpGrantRole = new GroupBox { Text = "② Cấp role cho user/role", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10) };
            _cboGrantRole = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _chkGrantRoleWithAdmin = new CheckBox { Text = "WITH ADMIN OPTION", Dock = DockStyle.Top, Height = 26 };
            _btnGrantRole = PrimaryButton("Cấp role");
            _btnGrantRole.Dock = DockStyle.Top;
            _btnGrantRole.Click += async (s, e) => await GrantRoleAsync();
            _grpGrantRole.Controls.Add(_btnGrantRole);
            _grpGrantRole.Controls.Add(_chkGrantRoleWithAdmin);
            _grpGrantRole.Controls.Add(_cboGrantRole);

            _grpGrantObjPriv = new GroupBox { Text = "③ Cấp quyền trên đối tượng (table/view/procedure/function). SELECT/UPDATE có thể giới hạn theo cột.", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10) };
            var pnlObj = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5 };
            pnlObj.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            pnlObj.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            _cboGrantObjType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboGrantObjType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            _cboGrantObjType.SelectedIndex = 0;

            _cboGrantObjPriv = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboGrantObjType.SelectedIndexChanged += (s, e) => PopulateObjectPrivileges(_cboGrantObjType, _cboGrantObjPriv);
            PopulateObjectPrivileges(_cboGrantObjType, _cboGrantObjPriv);

            _txtGrantObjName = new TextBox { Dock = DockStyle.Fill, Text = "HR.EMPLOYEES" };
            _txtGrantObjCols = new TextBox { Dock = DockStyle.Fill };
            _chkGrantObjWithGrant = new CheckBox { Text = "WITH GRANT OPTION", Dock = DockStyle.Fill };
            _btnGrantObj = PrimaryButton("Cấp quyền object");
            _btnGrantObj.Dock = DockStyle.Fill;
            _tips.SetToolTip(_txtGrantObjCols, "Chỉ áp dụng khi Privilege = SELECT hoặc UPDATE. Nhập danh sách cột cách nhau bằng dấu phẩy. Để trống = cấp trên toàn bộ object.");
            _tips.SetToolTip(_txtGrantObjName, "Dạng OWNER.OBJECT, ví dụ HR.EMPLOYEES");
            _btnGrantObj.Click += async (s, e) => await GrantObjAsync();

            pnlObj.Controls.Add(new Label { Text = "Object type", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            pnlObj.Controls.Add(_cboGrantObjType, 1, 0);
            pnlObj.Controls.Add(new Label { Text = "Privilege", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            pnlObj.Controls.Add(_cboGrantObjPriv, 1, 1);
            pnlObj.Controls.Add(new Label { Text = "Object (OWNER.OBJECT)", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            pnlObj.Controls.Add(_txtGrantObjName, 1, 2);
            pnlObj.Controls.Add(new Label { Text = "Columns (CSV)", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            pnlObj.Controls.Add(_txtGrantObjCols, 1, 3);
            pnlObj.Controls.Add(_chkGrantObjWithGrant, 0, 4);
            pnlObj.Controls.Add(_btnGrantObj, 1, 4);

            _grpGrantObjPriv.Controls.Add(pnlObj);

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(_grpGrantSysPriv, 0, 1);
            layout.Controls.Add(_grpGrantRole, 0, 2);
            layout.Controls.Add(_grpGrantObjPriv, 0, 3);

            tab.Controls.Add(layout);
            tab.Enter += async (s, e) => await EnsurePrivilegeListsLoadedAsync();
            return tab;
        }

        private TabPage BuildTabRevoke()
        {
            var tab = new TabPage("4. Revoke — Thu hồi") { Padding = new Padding(12), BackColor = Color.White };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "Thu hồi quyền của (Grantee):", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 10, 6, 0) });
            _cboRevokeGrantee = new ComboBox { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnLoad = new Button { Text = "↻ Tải danh sách users/roles", Width = 200, Height = 28, BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLoad.Click += async (s, e) => await LoadGranteesAsync();
            top.Controls.Add(_cboRevokeGrantee);
            top.Controls.Add(btnLoad);

            _grpRevokeSysPriv = new GroupBox { Text = "① Thu hồi quyền hệ thống", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10) };
            _cboRevokeSysPriv = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _btnRevokeSys = DangerButton("Thu hồi system privilege");
            _btnRevokeSys.Dock = DockStyle.Top;
            _btnRevokeSys.Click += async (s, e) => await RevokeSysAsync();
            _grpRevokeSysPriv.Controls.Add(_btnRevokeSys);
            _grpRevokeSysPriv.Controls.Add(_cboRevokeSysPriv);

            _grpRevokeRole = new GroupBox { Text = "② Thu hồi role", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10) };
            _cboRevokeRole = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _btnRevokeRole = DangerButton("Thu hồi role");
            _btnRevokeRole.Dock = DockStyle.Top;
            _btnRevokeRole.Click += async (s, e) => await RevokeRoleAsync();
            _grpRevokeRole.Controls.Add(_btnRevokeRole);
            _grpRevokeRole.Controls.Add(_cboRevokeRole);

            _grpRevokeObjPriv = new GroupBox { Text = "③ Thu hồi quyền trên đối tượng (để trống Columns = thu hồi toàn object)", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10) };
            var pnlObj = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5 };
            pnlObj.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            pnlObj.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            pnlObj.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            _cboRevokeObjType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboRevokeObjType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            _cboRevokeObjType.SelectedIndex = 0;

            _cboRevokeObjPriv = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboRevokeObjType.SelectedIndexChanged += (s, e) => PopulateObjectPrivileges(_cboRevokeObjType, _cboRevokeObjPriv);
            PopulateObjectPrivileges(_cboRevokeObjType, _cboRevokeObjPriv);

            _txtRevokeObjName = new TextBox { Dock = DockStyle.Fill, Text = "HR.EMPLOYEES" };
            _txtRevokeObjCols = new TextBox { Dock = DockStyle.Fill };
            _btnRevokeObj = DangerButton("Thu hồi quyền object");
            _btnRevokeObj.Dock = DockStyle.Fill;
            _tips.SetToolTip(_txtRevokeObjCols, "Chỉ có tác dụng với SELECT/UPDATE. Để trống = thu hồi quyền trên toàn object.");
            _btnRevokeObj.Click += async (s, e) => await RevokeObjAsync();

            pnlObj.Controls.Add(new Label { Text = "Object type", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            pnlObj.Controls.Add(_cboRevokeObjType, 1, 0);
            pnlObj.Controls.Add(new Label { Text = "Privilege", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            pnlObj.Controls.Add(_cboRevokeObjPriv, 1, 1);
            pnlObj.Controls.Add(new Label { Text = "Object (OWNER.OBJECT)", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            pnlObj.Controls.Add(_txtRevokeObjName, 1, 2);
            pnlObj.Controls.Add(new Label { Text = "Columns (CSV)", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            pnlObj.Controls.Add(_txtRevokeObjCols, 1, 3);
            pnlObj.Controls.Add(_btnRevokeObj, 1, 4);

            _grpRevokeObjPriv.Controls.Add(pnlObj);

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(_grpRevokeSysPriv, 0, 1);
            layout.Controls.Add(_grpRevokeRole, 0, 2);
            layout.Controls.Add(_grpRevokeObjPriv, 0, 3);

            tab.Controls.Add(layout);
            tab.Enter += async (s, e) => await EnsurePrivilegeListsLoadedAsync();
            return tab;
        }

        private TabPage BuildTabViewPrivileges()
        {
            var tab = new TabPage("5. Xem quyền") { Padding = new Padding(12), BackColor = Color.White };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "Tên user/role (sẽ tự IN HOA):", AutoSize = true, Padding = new Padding(0, 10, 6, 0) });
            _txtViewName = new TextBox { Width = 260 };
            _radViewUser = new RadioButton { Text = "User", Checked = true, AutoSize = true, Padding = new Padding(12, 10, 0, 0) };
            _radViewRole = new RadioButton { Text = "Role", AutoSize = true, Padding = new Padding(6, 10, 0, 0) };
            _btnViewPrivs = new Button { Text = "🔍 Xem quyền", Width = 140, Height = 30, BackColor = Color.FromArgb(33, 64, 107), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold) };
            _tips.SetToolTip(_btnViewPrivs, "Chạy 3 query trực tiếp tới dba_sys_privs, dba_role_privs, dba_tab_privs/dba_col_privs.");
            _btnViewPrivs.Click += async (s, e) => await ViewPrivsAsync();
            top.Controls.Add(_txtViewName);
            top.Controls.Add(_radViewUser);
            top.Controls.Add(_radViewRole);
            top.Controls.Add(_btnViewPrivs);

            var grids = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            grids.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            grids.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            grids.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            _gridViewSys = NewGrid();
            _gridViewRoles = NewGrid();
            _gridViewObj = NewGrid();

            grids.Controls.Add(WrapGrid("① System privileges — dba_sys_privs", _gridViewSys), 0, 0);
            grids.Controls.Add(WrapGrid("② Roles đã được cấp — dba_role_privs", _gridViewRoles), 0, 1);
            grids.Controls.Add(WrapGrid("③ Object & Column privileges — dba_tab_privs ∪ dba_col_privs", _gridViewObj), 0, 2);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(grids, 0, 1);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage BuildTabObjectBrowser()
        {
            var tab = new TabPage("6. Object Browser") { Padding = new Padding(12), BackColor = Color.White };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "Schema (Owner):", AutoSize = true, Padding = new Padding(0, 10, 4, 0) });
            _cboOwner = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(_cboOwner);
            var btnLoadOwners = new Button { Text = "↻ Tải owners", Width = 130, Height = 28, BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLoadOwners.Click += async (s, e) => await LoadOwnersAsync();
            top.Controls.Add(btnLoadOwners);

            top.Controls.Add(new Label { Text = "Loại đối tượng:", AutoSize = true, Padding = new Padding(15, 10, 4, 0) });
            _cboObjectType = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboObjectType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            _cboObjectType.SelectedIndex = 0;
            top.Controls.Add(_cboObjectType);

            _btnBrowse = new Button { Text = "🔎 Duyệt", Width = 110, Height = 28, BackColor = Color.FromArgb(33, 64, 107), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold) };
            _btnBrowse.Click += async (s, e) => await BrowseObjectsAsync();
            top.Controls.Add(_btnBrowse);

            var grids = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };
            _gridObjects = NewGrid();
            _gridColumns = NewGrid();
            grids.Panel1.Controls.Add(WrapGrid("Danh sách đối tượng (dba_tables / dba_views / dba_objects)", _gridObjects));
            grids.Panel2.Controls.Add(WrapGrid("Cột của TABLE/VIEW (dba_tab_columns) — chọn 1 dòng object phía trên", _gridColumns));

            _gridObjects.SelectionChanged += async (s, e) => await LoadColumnsForSelectedObjectAsync();

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(grids, 0, 1);
            tab.Controls.Add(root);
            return tab;
        }

        private static DataGridView NewGrid()
        {
            var g = new DataGridView
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
                EnableHeadersVisualStyles = false
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 238, 243);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return g;
        }

        private static Control WrapGrid(string title, Control grid)
        {
            var grp = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 8) };
            grp.Controls.Add(grid);
            return grp;
        }

        private void ShowError(Exception ex) =>
            MessageBox.Show(this, ex.Message, "NHOM 09 - Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private bool _privListsLoaded;
        private async Task EnsurePrivilegeListsLoadedAsync()
        {
            if (_privListsLoaded) return;
            _privListsLoaded = true;

            SetBusy(true);
            try
            {
                // System privileges
                var dtSys = await OracleHelper.QueryAsync(_connectionString, "select name from system_privilege_map order by name");
                BindCombo(_cboGrantSysPriv, dtSys, "name");
                BindCombo(_cboRevokeSysPriv, dtSys, "name");

                // Roles
                var dtRoles = await OracleHelper.QueryAsync(_connectionString, "select role from dba_roles order by role");
                BindCombo(_cboGrantRole, dtRoles, "role");
                BindCombo(_cboRevokeRole, dtRoles, "role");
            }
            catch (Exception ex)
            {
                _privListsLoaded = false;
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static void BindCombo(ComboBox cbo, DataTable dt, string col)
        {
            if (cbo == null) return;
            cbo.DisplayMember = col;
            cbo.ValueMember = col;
            cbo.DataSource = dt == null ? null : dt.Copy();
            if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
        }

        private static void PopulateObjectPrivileges(ComboBox cboType, ComboBox cboPriv)
        {
            if (cboPriv == null) return;
            var t = (cboType?.SelectedItem?.ToString() ?? "TABLE").ToUpperInvariant();
            cboPriv.Items.Clear();
            if (t == "PROCEDURE" || t == "FUNCTION")
                cboPriv.Items.Add("EXECUTE");
            else
                cboPriv.Items.AddRange(new object[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
            if (cboPriv.Items.Count > 0) cboPriv.SelectedIndex = 0;
        }

        // ===== Users =====
        private async Task RefreshUsersAsync()
        {
            SetBusy(true);
            try { _gridUsers.DataSource = await _admin.GetUsersAsync(); }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private void HydrateUserInputsFromGrid()
        {
            try
            {
                if (_gridUsers?.CurrentRow?.DataBoundItem is DataRowView rv)
                {
                    _txtUserName.Text = rv.Row["username"]?.ToString();
                    var st = (rv.Row["account_status"]?.ToString() ?? "").ToUpperInvariant();
                    _cboAccountStatus.SelectedIndex = st.Contains("LOCK") ? 1 : 0;
                }
            }
            catch { }
        }

        private async Task UserAddAsync()
        {
            SetBusy(true);
            try
            {
                await _admin.CreateUserAsync(_txtUserName.Text, _txtUserPassword.Text);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task UserEditAsync()
        {
            SetBusy(true);
            try
            {
                if (!string.IsNullOrEmpty(_txtUserPassword.Text))
                    await _admin.AlterUserPasswordAsync(_txtUserName.Text, _txtUserPassword.Text);

                var locked = (_cboAccountStatus.SelectedItem?.ToString() ?? "OPEN") == "LOCKED";
                await _admin.SetUserLockAsync(_txtUserName.Text, locked);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task UserDeleteAsync()
        {
            if (MessageBox.Show(this, "DROP USER sẽ xóa user và cascade object phụ thuộc. Bạn chắc chắn?", "NHOM 09",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            SetBusy(true);
            try
            {
                await _admin.DropUserAsync(_txtUserName.Text);
                await RefreshUsersAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== Roles =====
        private async Task RefreshRolesAsync()
        {
            SetBusy(true);
            try { _gridRoles.DataSource = await _admin.GetRolesAsync(); }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private void HydrateRoleInputsFromGrid()
        {
            try
            {
                if (_gridRoles?.CurrentRow?.DataBoundItem is DataRowView rv)
                {
                    _txtRoleName.Text = rv.Row["role"]?.ToString();
                }
            }
            catch { }
        }

        private async Task RoleAddAsync()
        {
            SetBusy(true);
            try
            {
                await _admin.CreateRoleAsync(_txtRoleName.Text, _chkRolePassword.Checked, _txtRolePassword.Text);
                await RefreshRolesAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task RoleDeleteAsync()
        {
            SetBusy(true);
            try
            {
                await _admin.DropRoleAsync(_txtRoleName.Text);
                await RefreshRolesAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== Grantee list used by Grant/Revoke =====
        private async Task LoadGranteesAsync()
        {
            SetBusy(true);
            try
            {
                var dt = await OracleHelper.QueryAsync(_connectionString,
                    "select username as name, 'USER' as kind from dba_users " +
                    "union all " +
                    "select role as name, 'ROLE' as kind from dba_roles " +
                    "order by kind, name");

                void Bind(ComboBox cbo)
                {
                    if (cbo == null) return;
                    cbo.DisplayMember = "name";
                    cbo.ValueMember = "name";
                    cbo.DataSource = dt.Copy();
                    if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
                }

                Bind(_cboGrantGrantee);
                Bind(_cboRevokeGrantee);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private bool RequireGrantee(ComboBox cbo, out string grantee)
        {
            grantee = cbo?.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(grantee))
            {
                MessageBox.Show(this, "Vui lòng bấm \"Tải danh sách users/roles\" và chọn một grantee trước.",
                    "NHOM 09", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        // ===== Grant =====
        private async Task GrantSysAsync()
        {
            if (!RequireGrantee(_cboGrantGrantee, out var g)) return;
            SetBusy(true);
            try
            {
                var p = _cboGrantSysPriv.SelectedValue?.ToString() ?? _cboGrantSysPriv.Text;
                await _admin.GrantSystemPrivilegeAsync(g, p, _chkGrantSysWithAdmin.Checked);
                SetStatus($"Đã cấp system privilege {p} cho {g}.");
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task GrantRoleAsync()
        {
            if (!RequireGrantee(_cboGrantGrantee, out var g)) return;
            SetBusy(true);
            try
            {
                var r = _cboGrantRole.SelectedValue?.ToString() ?? _cboGrantRole.Text;
                await _admin.GrantRoleAsync(g, r, _chkGrantRoleWithAdmin.Checked);
                SetStatus($"Đã cấp role {r} cho {g}.");
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task GrantObjAsync()
        {
            if (!RequireGrantee(_cboGrantGrantee, out var g)) return;
            SetBusy(true);
            try
            {
                var p = _cboGrantObjPriv.SelectedItem?.ToString() ?? _cboGrantObjPriv.Text;
                await _admin.GrantObjectPrivilegeAsync(g, p, _txtGrantObjName.Text, _txtGrantObjCols.Text, _chkGrantObjWithGrant.Checked);
                SetStatus($"Đã cấp {p} trên {_txtGrantObjName.Text} cho {g}.");
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== Revoke =====
        private bool ConfirmRevoke(string what)
        {
            return MessageBox.Show(this,
                $"Xác nhận thu hồi: {what} ?",
                "NHOM 09 - Revoke",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private async Task RevokeSysAsync()
        {
            if (!RequireGrantee(_cboRevokeGrantee, out var g)) return;
            var p = _cboRevokeSysPriv.SelectedValue?.ToString() ?? _cboRevokeSysPriv.Text;
            if (!ConfirmRevoke($"system privilege {p} từ {g}")) return;
            SetBusy(true);
            try
            {
                await _admin.RevokeSystemPrivilegeAsync(g, p);
                SetStatus($"Đã thu hồi {p} từ {g}.");
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task RevokeRoleAsync()
        {
            if (!RequireGrantee(_cboRevokeGrantee, out var g)) return;
            var r = _cboRevokeRole.SelectedValue?.ToString() ?? _cboRevokeRole.Text;
            if (!ConfirmRevoke($"role {r} từ {g}")) return;
            SetBusy(true);
            try
            {
                await _admin.RevokeRoleAsync(g, r);
                SetStatus($"Đã thu hồi role {r} từ {g}.");
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task RevokeObjAsync()
        {
            if (!RequireGrantee(_cboRevokeGrantee, out var g)) return;
            var p = _cboRevokeObjPriv.SelectedItem?.ToString() ?? _cboRevokeObjPriv.Text;
            if (!ConfirmRevoke($"{p} trên {_txtRevokeObjName.Text} từ {g}")) return;
            SetBusy(true);
            try
            {
                await _admin.RevokeObjectPrivilegeAsync(g, p, _txtRevokeObjName.Text, _txtRevokeObjCols.Text);
                SetStatus($"Đã thu hồi {p} trên {_txtRevokeObjName.Text} từ {g}.");
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== View privileges =====
        // Gọi 3 query riêng trực tiếp tới Oracle, bind thẳng vào 3 grid,
        // không thực hiện bất kỳ bước lọc/biến đổi dữ liệu nào ở phía client.
        private async Task ViewPrivsAsync()
        {
            SetBusy(true);
            try
            {
                var name = (_txtViewName.Text ?? "").Trim().ToUpperInvariant();
                if (name.Length == 0)
                {
                    MessageBox.Show(this, "Vui lòng nhập tên user/role cần xem quyền.", "NHOM 09",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var sysTask = _admin.GetSystemPrivilegesOfGranteeAsync(name);
                var roleTask = _admin.GetRolePrivilegesOfGranteeAsync(name);
                var objTask = _admin.GetObjectPrivilegesOfGranteeAsync(name);
                await Task.WhenAll(sysTask, roleTask, objTask);

                _gridViewSys.DataSource = sysTask.Result;
                _gridViewRoles.DataSource = roleTask.Result;
                _gridViewObj.DataSource = objTask.Result;
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== Object browser =====
        private async Task LoadOwnersAsync()
        {
            SetBusy(true);
            try
            {
                var dt = await _admin.GetOwnersAsync();
                _cboOwner.DisplayMember = "username";
                _cboOwner.ValueMember = "username";
                _cboOwner.DataSource = dt;
                if (_cboOwner.Items.Count > 0) _cboOwner.SelectedIndex = 0;
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task BrowseObjectsAsync()
        {
            SetBusy(true);
            try
            {
                var owner = _cboOwner.SelectedValue?.ToString();
                var type = _cboObjectType.SelectedItem?.ToString();
                _gridObjects.DataSource = await _admin.GetObjectsAsync(owner, type);
                _gridColumns.DataSource = null;
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task LoadColumnsForSelectedObjectAsync()
        {
            try
            {
                if (!(_gridObjects?.CurrentRow?.DataBoundItem is DataRowView rv)) return;
                var owner = _cboOwner.SelectedValue?.ToString();
                var type = (rv.Row["type"]?.ToString() ?? "").ToUpperInvariant();
                var name = rv.Row["name"]?.ToString();
                if (type != "TABLE" && type != "VIEW") { _gridColumns.DataSource = null; return; }

                SetBusy(true);
                _gridColumns.DataSource = await _admin.GetColumnsAsync(owner, name);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== Các handler cũ để Designer compile (không còn dùng UI cũ) =====
        private void chkSysdba_CheckedChanged(object sender, EventArgs e) { }
        private async void btnApplyConn_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnTestConn_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private void txtPassword_TextChanged(object sender, EventArgs e) { }
        private void txtUser_TextChanged(object sender, EventArgs e) { }
        private void txtService_TextChanged(object sender, EventArgs e) { }
        private void numPort_ValueChanged(object sender, EventArgs e) { }
        private void txtHost_TextChanged(object sender, EventArgs e) { }
        private async void btnUserUnlock_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnUserLock_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnUserDrop_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnUserAlterPass_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnUserCreate_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnUsersRefresh_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnRoleDrop_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnRoleCreate_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnRolesRefresh_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnRevoke_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnGrant_Click(object sender, EventArgs e) { await Task.CompletedTask; }
        private async void btnViewPrivs_Click(object sender, EventArgs e) { await Task.CompletedTask; }
    }
}

