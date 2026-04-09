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

        private void BuildUi()
        {
            // Bỏ toàn bộ UI cũ do Designer tạo (tab Kết nối + các tab cũ)
            Controls.Clear();

            Text = "NHOM 09 - Quản trị CSDL Oracle (MainForm)";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 750);

            _tabs = new TabControl { Dock = DockStyle.Fill };
            Controls.Add(_tabs);

            _tabs.TabPages.Add(BuildTabUsers());
            _tabs.TabPages.Add(BuildTabRoles());
            _tabs.TabPages.Add(BuildTabGrant());
            _tabs.TabPages.Add(BuildTabRevoke());
            _tabs.TabPages.Add(BuildTabViewPrivileges());
            _tabs.TabPages.Add(BuildTabObjectBrowser());
        }

        private TabPage BuildTabUsers()
        {
            var tab = new TabPage("User") { Padding = new Padding(10) };

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 80,
                ColumnCount = 10,
                RowCount = 2,
                AutoSize = false
            };
            for (int i = 0; i < top.ColumnCount; i++) top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            _txtUserName = new TextBox { Dock = DockStyle.Fill };
            _txtUserPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            _cboAccountStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboAccountStatus.Items.AddRange(new object[] { "OPEN", "LOCKED" });
            _cboAccountStatus.SelectedIndex = 0;

            _btnUserAdd = new Button { Text = "Thêm", Dock = DockStyle.Fill };
            _btnUserEdit = new Button { Text = "Sửa", Dock = DockStyle.Fill };
            _btnUserDelete = new Button { Text = "Xóa", Dock = DockStyle.Fill };
            _btnUserRefresh = new Button { Text = "Refresh", Dock = DockStyle.Fill };

            top.Controls.Add(new Label { Text = "Username", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            top.SetColumnSpan(_txtUserName, 2);
            top.Controls.Add(_txtUserName, 1, 0);

            top.Controls.Add(new Label { Text = "Password", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 3, 0);
            top.SetColumnSpan(_txtUserPassword, 2);
            top.Controls.Add(_txtUserPassword, 4, 0);

            top.Controls.Add(new Label { Text = "Account status", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 6, 0);
            top.Controls.Add(_cboAccountStatus, 7, 0);

            top.Controls.Add(_btnUserAdd, 0, 1);
            top.Controls.Add(_btnUserEdit, 1, 1);
            top.Controls.Add(_btnUserDelete, 2, 1);
            top.Controls.Add(_btnUserRefresh, 3, 1);

            _gridUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _btnUserRefresh.Click += async (s, e) => await RefreshUsersAsync();
            _btnUserAdd.Click += async (s, e) => await UserAddAsync();
            _btnUserEdit.Click += async (s, e) => await UserEditAsync();
            _btnUserDelete.Click += async (s, e) => await UserDeleteAsync();
            _gridUsers.SelectionChanged += (s, e) => HydrateUserInputsFromGrid();

            tab.Controls.Add(_gridUsers);
            tab.Controls.Add(top);
            return tab;
        }

        private TabPage BuildTabRoles()
        {
            var tab = new TabPage("Role") { Padding = new Padding(10) };

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                ColumnCount = 10,
                RowCount = 2
            };
            for (int i = 0; i < top.ColumnCount; i++) top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            _txtRoleName = new TextBox { Dock = DockStyle.Fill };
            _chkRolePassword = new CheckBox { Text = "Password role", Dock = DockStyle.Fill };
            _txtRolePassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Enabled = false };
            _chkRolePassword.CheckedChanged += (s, e) => _txtRolePassword.Enabled = _chkRolePassword.Checked;

            _btnRoleAdd = new Button { Text = "Thêm", Dock = DockStyle.Fill };
            _btnRoleEdit = new Button { Text = "Sửa", Dock = DockStyle.Fill, Enabled = false }; // Oracle role "alter" không phổ biến theo yêu cầu này
            _btnRoleDelete = new Button { Text = "Xóa", Dock = DockStyle.Fill };
            _btnRoleRefresh = new Button { Text = "Refresh", Dock = DockStyle.Fill };

            top.Controls.Add(new Label { Text = "Role name", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            top.SetColumnSpan(_txtRoleName, 2);
            top.Controls.Add(_txtRoleName, 1, 0);

            top.Controls.Add(_chkRolePassword, 3, 0);
            top.SetColumnSpan(_txtRolePassword, 2);
            top.Controls.Add(_txtRolePassword, 4, 0);

            top.Controls.Add(_btnRoleAdd, 0, 1);
            top.Controls.Add(_btnRoleEdit, 1, 1);
            top.Controls.Add(_btnRoleDelete, 2, 1);
            top.Controls.Add(_btnRoleRefresh, 3, 1);

            _gridRoles = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _btnRoleRefresh.Click += async (s, e) => await RefreshRolesAsync();
            _btnRoleAdd.Click += async (s, e) => await RoleAddAsync();
            _btnRoleDelete.Click += async (s, e) => await RoleDeleteAsync();
            _gridRoles.SelectionChanged += (s, e) => HydrateRoleInputsFromGrid();

            tab.Controls.Add(_gridRoles);
            tab.Controls.Add(top);
            return tab;
        }

        private TabPage BuildTabGrant()
        {
            var tab = new TabPage("Grant") { Padding = new Padding(10) };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            top.Controls.Add(new Label { Text = "Grantee:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 0, 0) });
            _cboGrantGrantee = new ComboBox { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnLoad = new Button { Text = "Load users/roles", Width = 140 };
            btnLoad.Click += async (s, e) => await LoadGranteesAsync();
            top.Controls.Add(_cboGrantGrantee);
            top.Controls.Add(btnLoad);

            _grpGrantSysPriv = new GroupBox { Text = "Grant system privilege", Dock = DockStyle.Fill };
            _cboGrantSysPriv = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _chkGrantSysWithAdmin = new CheckBox { Text = "WITH ADMIN OPTION", Dock = DockStyle.Top };
            _btnGrantSys = new Button { Text = "Grant system priv", Dock = DockStyle.Top, Height = 30 };
            _btnGrantSys.Click += async (s, e) => await GrantSysAsync();
            _grpGrantSysPriv.Controls.Add(_btnGrantSys);
            _grpGrantSysPriv.Controls.Add(_chkGrantSysWithAdmin);
            _grpGrantSysPriv.Controls.Add(_cboGrantSysPriv);

            _grpGrantRole = new GroupBox { Text = "Grant role", Dock = DockStyle.Fill };
            _cboGrantRole = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _chkGrantRoleWithAdmin = new CheckBox { Text = "WITH ADMIN OPTION", Dock = DockStyle.Top };
            _btnGrantRole = new Button { Text = "Grant role", Dock = DockStyle.Top, Height = 30 };
            _btnGrantRole.Click += async (s, e) => await GrantRoleAsync();
            _grpGrantRole.Controls.Add(_btnGrantRole);
            _grpGrantRole.Controls.Add(_chkGrantRoleWithAdmin);
            _grpGrantRole.Controls.Add(_cboGrantRole);

            _grpGrantObjPriv = new GroupBox { Text = "Grant object privilege", Dock = DockStyle.Fill };
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
            _btnGrantObj = new Button { Text = "Grant object priv", Dock = DockStyle.Fill };
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
            var tab = new TabPage("Revoke") { Padding = new Padding(10) };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            top.Controls.Add(new Label { Text = "Grantee:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 8, 0, 0) });
            _cboRevokeGrantee = new ComboBox { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnLoad = new Button { Text = "Load users/roles", Width = 140 };
            btnLoad.Click += async (s, e) => await LoadGranteesAsync();
            top.Controls.Add(_cboRevokeGrantee);
            top.Controls.Add(btnLoad);

            _grpRevokeSysPriv = new GroupBox { Text = "Revoke system privilege", Dock = DockStyle.Fill };
            _cboRevokeSysPriv = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _btnRevokeSys = new Button { Text = "Revoke system priv", Dock = DockStyle.Top, Height = 30 };
            _btnRevokeSys.Click += async (s, e) => await RevokeSysAsync();
            _grpRevokeSysPriv.Controls.Add(_btnRevokeSys);
            _grpRevokeSysPriv.Controls.Add(_cboRevokeSysPriv);

            _grpRevokeRole = new GroupBox { Text = "Revoke role", Dock = DockStyle.Fill };
            _cboRevokeRole = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            _btnRevokeRole = new Button { Text = "Revoke role", Dock = DockStyle.Top, Height = 30 };
            _btnRevokeRole.Click += async (s, e) => await RevokeRoleAsync();
            _grpRevokeRole.Controls.Add(_btnRevokeRole);
            _grpRevokeRole.Controls.Add(_cboRevokeRole);

            _grpRevokeObjPriv = new GroupBox { Text = "Revoke object privilege", Dock = DockStyle.Fill };
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
            _btnRevokeObj = new Button { Text = "Revoke object priv", Dock = DockStyle.Fill };
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
            var tab = new TabPage("Xem quyền") { Padding = new Padding(10) };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            top.Controls.Add(new Label { Text = "Tên:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            _txtViewName = new TextBox { Width = 220 };
            _radViewUser = new RadioButton { Text = "User", Checked = true, AutoSize = true, Padding = new Padding(10, 8, 0, 0) };
            _radViewRole = new RadioButton { Text = "Role", AutoSize = true, Padding = new Padding(10, 8, 0, 0) };
            _btnViewPrivs = new Button { Text = "Xem", Width = 90 };
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

            grids.Controls.Add(WrapGrid("System privileges", _gridViewSys), 0, 0);
            grids.Controls.Add(WrapGrid("Roles đã nhận", _gridViewRoles), 0, 1);
            grids.Controls.Add(WrapGrid("Object/Column privileges", _gridViewObj), 0, 2);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(grids, 0, 1);
            tab.Controls.Add(root);
            return tab;
        }

        private TabPage BuildTabObjectBrowser()
        {
            var tab = new TabPage("Object Browser") { Padding = new Padding(10) };

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            top.Controls.Add(new Label { Text = "Owner:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            _cboOwner = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(_cboOwner);
            var btnLoadOwners = new Button { Text = "Load owners", Width = 120 };
            btnLoadOwners.Click += async (s, e) => await LoadOwnersAsync();
            top.Controls.Add(btnLoadOwners);

            top.Controls.Add(new Label { Text = "Object type:", AutoSize = true, Padding = new Padding(15, 8, 0, 0) });
            _cboObjectType = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboObjectType.Items.AddRange(new object[] { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" });
            _cboObjectType.SelectedIndex = 0;
            top.Controls.Add(_cboObjectType);

            _btnBrowse = new Button { Text = "Browse", Width = 90 };
            _btnBrowse.Click += async (s, e) => await BrowseObjectsAsync();
            top.Controls.Add(_btnBrowse);

            var grids = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };
            _gridObjects = NewGrid();
            _gridColumns = NewGrid();
            grids.Panel1.Controls.Add(WrapGrid("Objects", _gridObjects));
            grids.Panel2.Controls.Add(WrapGrid("Columns (TABLE/VIEW)", _gridColumns));

            _gridObjects.SelectionChanged += async (s, e) => await LoadColumnsForSelectedObjectAsync();

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(grids, 0, 1);
            tab.Controls.Add(root);
            return tab;
        }

        private static DataGridView NewGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
        }

        private static Control WrapGrid(string title, Control grid)
        {
            var grp = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8) };
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

        // ===== Grant =====
        private async Task GrantSysAsync()
        {
            SetBusy(true);
            try
            {
                var g = _cboGrantGrantee.SelectedValue?.ToString();
                var p = _cboGrantSysPriv.SelectedValue?.ToString() ?? _cboGrantSysPriv.Text;
                await _admin.GrantSystemPrivilegeAsync(g, p, _chkGrantSysWithAdmin.Checked);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task GrantRoleAsync()
        {
            SetBusy(true);
            try
            {
                var g = _cboGrantGrantee.SelectedValue?.ToString();
                var r = _cboGrantRole.SelectedValue?.ToString() ?? _cboGrantRole.Text;
                await _admin.GrantRoleAsync(g, r, _chkGrantRoleWithAdmin.Checked);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task GrantObjAsync()
        {
            SetBusy(true);
            try
            {
                var g = _cboGrantGrantee.SelectedValue?.ToString();
                var p = _cboGrantObjPriv.SelectedItem?.ToString() ?? _cboGrantObjPriv.Text;
                await _admin.GrantObjectPrivilegeAsync(g, p, _txtGrantObjName.Text, _txtGrantObjCols.Text, _chkGrantObjWithGrant.Checked);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== Revoke =====
        private async Task RevokeSysAsync()
        {
            SetBusy(true);
            try
            {
                var g = _cboRevokeGrantee.SelectedValue?.ToString();
                var p = _cboRevokeSysPriv.SelectedValue?.ToString() ?? _cboRevokeSysPriv.Text;
                await _admin.RevokeSystemPrivilegeAsync(g, p);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task RevokeRoleAsync()
        {
            SetBusy(true);
            try
            {
                var g = _cboRevokeGrantee.SelectedValue?.ToString();
                var r = _cboRevokeRole.SelectedValue?.ToString() ?? _cboRevokeRole.Text;
                await _admin.RevokeRoleAsync(g, r);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task RevokeObjAsync()
        {
            SetBusy(true);
            try
            {
                var g = _cboRevokeGrantee.SelectedValue?.ToString();
                var p = _cboRevokeObjPriv.SelectedItem?.ToString() ?? _cboRevokeObjPriv.Text;
                await _admin.RevokeObjectPrivilegeAsync(g, p, _txtRevokeObjName.Text, _txtRevokeObjCols.Text);
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        // ===== View privileges =====
        private async Task ViewPrivsAsync()
        {
            SetBusy(true);
            try
            {
                var name = (_txtViewName.Text ?? "").Trim().ToUpperInvariant();
                var dt = await _admin.GetPrivilegesOfGranteeAsync(name);

                _gridViewSys.DataSource = SelectRows(dt, "src = 'SYS'", new[] { "grantee", "sys_priv", "opt" });
                _gridViewRoles.DataSource = SelectRows(dt, "src = 'ROLE'", new[] { "grantee", "role", "opt" });
                _gridViewObj.DataSource = SelectRows(dt, "src in ('TAB','COL')", new[] { "grantee", "obj", "col", "opt" });
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private static DataTable SelectRows(DataTable source, string filter, string[] cols)
        {
            var view = new DataView(source) { RowFilter = filter };
            var dt = view.ToTable(false, cols);
            return dt;
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

