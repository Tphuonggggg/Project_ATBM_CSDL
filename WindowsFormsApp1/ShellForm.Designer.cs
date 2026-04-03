namespace WindowsFormsApp1
{
    partial class ShellForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menu = new System.Windows.Forms.MenuStrip();
            this.mnuSystem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabManage = new System.Windows.Forms.TabPage();
            this.gridManageRoles = new System.Windows.Forms.DataGridView();
            this.gridManageUsers = new System.Windows.Forms.DataGridView();
            this.grpRole = new System.Windows.Forms.GroupBox();
            this.btnRoleDrop = new System.Windows.Forms.Button();
            this.btnRoleCreate = new System.Windows.Forms.Button();
            this.txtRoleName = new System.Windows.Forms.TextBox();
            this.lblRoleName = new System.Windows.Forms.Label();
            this.grpUser = new System.Windows.Forms.GroupBox();
            this.btnUserDrop = new System.Windows.Forms.Button();
            this.btnUserAlter = new System.Windows.Forms.Button();
            this.btnUserCreate = new System.Windows.Forms.Button();
            this.txtUserPass = new System.Windows.Forms.TextBox();
            this.lblUserPass = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.btnManageRefresh = new System.Windows.Forms.Button();
            this.tabGrant = new System.Windows.Forms.TabPage();
            this.grpGrant = new System.Windows.Forms.GroupBox();
            this.btnGrantRefreshTargets = new System.Windows.Forms.Button();
            this.btnGrantExecute = new System.Windows.Forms.Button();
            this.btnPickColumns = new System.Windows.Forms.Button();
            this.btnLoadObjects = new System.Windows.Forms.Button();
            this.txtObjectFilter = new System.Windows.Forms.TextBox();
            this.lblObjectFilter = new System.Windows.Forms.Label();
            this.txtColumns = new System.Windows.Forms.TextBox();
            this.lblColumns = new System.Windows.Forms.Label();
            this.cboObject = new System.Windows.Forms.ComboBox();
            this.lblObject = new System.Windows.Forms.Label();
            this.chkWithGrantOption = new System.Windows.Forms.CheckBox();
            this.radGrantObjPriv = new System.Windows.Forms.RadioButton();
            this.radGrantSysPriv = new System.Windows.Forms.RadioButton();
            this.radGrantRole = new System.Windows.Forms.RadioButton();
            this.cboPrivilege = new System.Windows.Forms.ComboBox();
            this.lblPrivilege = new System.Windows.Forms.Label();
            this.cboObjectType = new System.Windows.Forms.ComboBox();
            this.lblObjectType = new System.Windows.Forms.Label();
            this.cboGrantee = new System.Windows.Forms.ComboBox();
            this.lblGrantee = new System.Windows.Forms.Label();
            this.tabAudit = new System.Windows.Forms.TabPage();
            this.gridAudit = new System.Windows.Forms.DataGridView();
            this.btnRevokeSelected = new System.Windows.Forms.Button();
            this.btnAuditRefresh = new System.Windows.Forms.Button();
            this.cboAuditKind = new System.Windows.Forms.ComboBox();
            this.lblAuditKind = new System.Windows.Forms.Label();
            this.txtAuditFilter = new System.Windows.Forms.TextBox();
            this.lblAuditFilter = new System.Windows.Forms.Label();
            this.menu.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabManage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridManageRoles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridManageUsers)).BeginInit();
            this.grpRole.SuspendLayout();
            this.grpUser.SuspendLayout();
            this.tabGrant.SuspendLayout();
            this.grpGrant.SuspendLayout();
            this.tabAudit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAudit)).BeginInit();
            this.SuspendLayout();
            // 
            // menu
            // 
            this.menu.ImageScalingSize = new System.Drawing.Size(18, 18);
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSystem});
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(1200, 25);
            this.menu.TabIndex = 0;
            this.menu.Text = "menuStrip1";
            // 
            // mnuSystem
            // 
            this.mnuSystem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuConnect,
            this.mnuRefresh,
            this.mnuLogout});
            this.mnuSystem.Name = "mnuSystem";
            this.mnuSystem.Size = new System.Drawing.Size(74, 21);
            this.mnuSystem.Text = "Hệ thống";
            // 
            // mnuConnect
            // 
            this.mnuConnect.Name = "mnuConnect";
            this.mnuConnect.Size = new System.Drawing.Size(141, 24);
            this.mnuConnect.Text = "Kết nối...";
            this.mnuConnect.Click += new System.EventHandler(this.mnuConnect_Click);
            // 
            // mnuRefresh
            // 
            this.mnuRefresh.Name = "mnuRefresh";
            this.mnuRefresh.Size = new System.Drawing.Size(141, 24);
            this.mnuRefresh.Text = "Làm mới";
            this.mnuRefresh.Click += new System.EventHandler(this.mnuRefresh_Click);
            // 
            // mnuLogout
            // 
            this.mnuLogout.Name = "mnuLogout";
            this.mnuLogout.Size = new System.Drawing.Size(141, 24);
            this.mnuLogout.Text = "Đăng xuất";
            this.mnuLogout.Click += new System.EventHandler(this.mnuLogout_Click);
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabManage);
            this.tabs.Controls.Add(this.tabGrant);
            this.tabs.Controls.Add(this.tabAudit);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 25);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(1200, 775);
            this.tabs.TabIndex = 1;
            // 
            // tabManage
            // 
            this.tabManage.Controls.Add(this.gridManageRoles);
            this.tabManage.Controls.Add(this.gridManageUsers);
            this.tabManage.Controls.Add(this.grpRole);
            this.tabManage.Controls.Add(this.grpUser);
            this.tabManage.Controls.Add(this.btnManageRefresh);
            this.tabManage.Location = new System.Drawing.Point(4, 22);
            this.tabManage.Name = "tabManage";
            this.tabManage.Padding = new System.Windows.Forms.Padding(10);
            this.tabManage.Size = new System.Drawing.Size(1192, 749);
            this.tabManage.TabIndex = 0;
            this.tabManage.Text = "Quản lý User/Role";
            this.tabManage.UseVisualStyleBackColor = true;
            // 
            // gridManageRoles
            // 
            this.gridManageRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridManageRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridManageRoles.Location = new System.Drawing.Point(610, 200);
            this.gridManageRoles.Name = "gridManageRoles";
            this.gridManageRoles.RowHeadersWidth = 45;
            this.gridManageRoles.Size = new System.Drawing.Size(557, 536);
            this.gridManageRoles.TabIndex = 4;
            // 
            // gridManageUsers
            // 
            this.gridManageUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridManageUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridManageUsers.Location = new System.Drawing.Point(13, 200);
            this.gridManageUsers.Name = "gridManageUsers";
            this.gridManageUsers.RowHeadersWidth = 45;
            this.gridManageUsers.Size = new System.Drawing.Size(573, 536);
            this.gridManageUsers.TabIndex = 3;
            // 
            // grpRole
            // 
            this.grpRole.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpRole.Controls.Add(this.btnRoleDrop);
            this.grpRole.Controls.Add(this.btnRoleCreate);
            this.grpRole.Controls.Add(this.txtRoleName);
            this.grpRole.Controls.Add(this.lblRoleName);
            this.grpRole.Location = new System.Drawing.Point(610, 42);
            this.grpRole.Name = "grpRole";
            this.grpRole.Size = new System.Drawing.Size(557, 152);
            this.grpRole.TabIndex = 2;
            this.grpRole.TabStop = false;
            this.grpRole.Text = "Vai trò";
            // 
            // btnRoleDrop
            // 
            this.btnRoleDrop.Location = new System.Drawing.Point(214, 66);
            this.btnRoleDrop.Name = "btnRoleDrop";
            this.btnRoleDrop.Size = new System.Drawing.Size(190, 23);
            this.btnRoleDrop.TabIndex = 3;
            this.btnRoleDrop.Text = "Xóa";
            this.btnRoleDrop.UseVisualStyleBackColor = true;
            this.btnRoleDrop.Click += new System.EventHandler(this.btnRoleDrop_Click);
            // 
            // btnRoleCreate
            // 
            this.btnRoleCreate.Location = new System.Drawing.Point(18, 66);
            this.btnRoleCreate.Name = "btnRoleCreate";
            this.btnRoleCreate.Size = new System.Drawing.Size(190, 23);
            this.btnRoleCreate.TabIndex = 2;
            this.btnRoleCreate.Text = "Tạo mới";
            this.btnRoleCreate.UseVisualStyleBackColor = true;
            this.btnRoleCreate.Click += new System.EventHandler(this.btnRoleCreate_Click);
            // 
            // txtRoleName
            // 
            this.txtRoleName.Location = new System.Drawing.Point(74, 31);
            this.txtRoleName.Name = "txtRoleName";
            this.txtRoleName.Size = new System.Drawing.Size(330, 20);
            this.txtRoleName.TabIndex = 1;
            // 
            // lblRoleName
            // 
            this.lblRoleName.AutoSize = true;
            this.lblRoleName.Location = new System.Drawing.Point(15, 34);
            this.lblRoleName.Name = "lblRoleName";
            this.lblRoleName.Size = new System.Drawing.Size(61, 13);
            this.lblRoleName.TabIndex = 0;
            this.lblRoleName.Text = "Tên vai trò:";
            // 
            // grpUser
            // 
            this.grpUser.Controls.Add(this.btnUserDrop);
            this.grpUser.Controls.Add(this.btnUserAlter);
            this.grpUser.Controls.Add(this.btnUserCreate);
            this.grpUser.Controls.Add(this.txtUserPass);
            this.grpUser.Controls.Add(this.lblUserPass);
            this.grpUser.Controls.Add(this.txtUserName);
            this.grpUser.Controls.Add(this.lblUserName);
            this.grpUser.Location = new System.Drawing.Point(13, 42);
            this.grpUser.Name = "grpUser";
            this.grpUser.Size = new System.Drawing.Size(573, 152);
            this.grpUser.TabIndex = 1;
            this.grpUser.TabStop = false;
            this.grpUser.Text = "Người dùng";
            // 
            // btnUserDrop
            // 
            this.btnUserDrop.Location = new System.Drawing.Point(258, 109);
            this.btnUserDrop.Name = "btnUserDrop";
            this.btnUserDrop.Size = new System.Drawing.Size(160, 23);
            this.btnUserDrop.TabIndex = 6;
            this.btnUserDrop.Text = "Xóa";
            this.btnUserDrop.UseVisualStyleBackColor = true;
            this.btnUserDrop.Click += new System.EventHandler(this.btnUserDrop_Click);
            // 
            // btnUserAlter
            // 
            this.btnUserAlter.Location = new System.Drawing.Point(92, 109);
            this.btnUserAlter.Name = "btnUserAlter";
            this.btnUserAlter.Size = new System.Drawing.Size(160, 23);
            this.btnUserAlter.TabIndex = 5;
            this.btnUserAlter.Text = "Sửa (đổi mật khẩu)";
            this.btnUserAlter.UseVisualStyleBackColor = true;
            this.btnUserAlter.Click += new System.EventHandler(this.btnUserAlter_Click);
            // 
            // btnUserCreate
            // 
            this.btnUserCreate.Location = new System.Drawing.Point(92, 80);
            this.btnUserCreate.Name = "btnUserCreate";
            this.btnUserCreate.Size = new System.Drawing.Size(160, 23);
            this.btnUserCreate.TabIndex = 4;
            this.btnUserCreate.Text = "Tạo mới";
            this.btnUserCreate.UseVisualStyleBackColor = true;
            this.btnUserCreate.Click += new System.EventHandler(this.btnUserCreate_Click);
            // 
            // txtUserPass
            // 
            this.txtUserPass.Location = new System.Drawing.Point(92, 54);
            this.txtUserPass.Name = "txtUserPass";
            this.txtUserPass.Size = new System.Drawing.Size(326, 20);
            this.txtUserPass.TabIndex = 3;
            this.txtUserPass.UseSystemPasswordChar = true;
            // 
            // lblUserPass
            // 
            this.lblUserPass.AutoSize = true;
            this.lblUserPass.Location = new System.Drawing.Point(15, 57);
            this.lblUserPass.Name = "lblUserPass";
            this.lblUserPass.Size = new System.Drawing.Size(55, 13);
            this.lblUserPass.TabIndex = 2;
            this.lblUserPass.Text = "Mật khẩu:";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(92, 28);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(326, 20);
            this.txtUserName.TabIndex = 1;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(15, 31);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(58, 13);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.Text = "Tài khoản:";
            // 
            // btnManageRefresh
            // 
            this.btnManageRefresh.Location = new System.Drawing.Point(13, 13);
            this.btnManageRefresh.Name = "btnManageRefresh";
            this.btnManageRefresh.Size = new System.Drawing.Size(140, 23);
            this.btnManageRefresh.TabIndex = 0;
            this.btnManageRefresh.Text = "Làm mới danh sách";
            this.btnManageRefresh.UseVisualStyleBackColor = true;
            this.btnManageRefresh.Click += new System.EventHandler(this.btnManageRefresh_Click);
            // 
            // tabGrant
            // 
            this.tabGrant.Controls.Add(this.grpGrant);
            this.tabGrant.Location = new System.Drawing.Point(4, 22);
            this.tabGrant.Name = "tabGrant";
            this.tabGrant.Padding = new System.Windows.Forms.Padding(10);
            this.tabGrant.Size = new System.Drawing.Size(1192, 749);
            this.tabGrant.TabIndex = 1;
            this.tabGrant.Text = "Cấp quyền";
            this.tabGrant.UseVisualStyleBackColor = true;
            // 
            // grpGrant
            // 
            this.grpGrant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpGrant.Controls.Add(this.btnGrantRefreshTargets);
            this.grpGrant.Controls.Add(this.btnGrantExecute);
            this.grpGrant.Controls.Add(this.btnPickColumns);
            this.grpGrant.Controls.Add(this.btnLoadObjects);
            this.grpGrant.Controls.Add(this.txtObjectFilter);
            this.grpGrant.Controls.Add(this.lblObjectFilter);
            this.grpGrant.Controls.Add(this.txtColumns);
            this.grpGrant.Controls.Add(this.lblColumns);
            this.grpGrant.Controls.Add(this.cboObject);
            this.grpGrant.Controls.Add(this.lblObject);
            this.grpGrant.Controls.Add(this.chkWithGrantOption);
            this.grpGrant.Controls.Add(this.radGrantObjPriv);
            this.grpGrant.Controls.Add(this.radGrantSysPriv);
            this.grpGrant.Controls.Add(this.radGrantRole);
            this.grpGrant.Controls.Add(this.cboPrivilege);
            this.grpGrant.Controls.Add(this.lblPrivilege);
            this.grpGrant.Controls.Add(this.cboObjectType);
            this.grpGrant.Controls.Add(this.lblObjectType);
            this.grpGrant.Controls.Add(this.cboGrantee);
            this.grpGrant.Controls.Add(this.lblGrantee);
            this.grpGrant.Location = new System.Drawing.Point(13, 13);
            this.grpGrant.Name = "grpGrant";
            this.grpGrant.Size = new System.Drawing.Size(1166, 264);
            this.grpGrant.TabIndex = 0;
            this.grpGrant.TabStop = false;
            this.grpGrant.Text = "Quy trình cấp quyền";
            // 
            // btnGrantRefreshTargets
            // 
            this.btnGrantRefreshTargets.Location = new System.Drawing.Point(608, 23);
            this.btnGrantRefreshTargets.Name = "btnGrantRefreshTargets";
            this.btnGrantRefreshTargets.Size = new System.Drawing.Size(218, 23);
            this.btnGrantRefreshTargets.TabIndex = 2;
            this.btnGrantRefreshTargets.Text = "Tải danh sách người dùng / vai trò";
            this.btnGrantRefreshTargets.UseVisualStyleBackColor = true;
            this.btnGrantRefreshTargets.Click += new System.EventHandler(this.btnGrantRefreshTargets_Click);
            // 
            // btnGrantExecute
            // 
            this.btnGrantExecute.Location = new System.Drawing.Point(709, 180);
            this.btnGrantExecute.Name = "btnGrantExecute";
            this.btnGrantExecute.Size = new System.Drawing.Size(230, 23);
            this.btnGrantExecute.TabIndex = 14;
            this.btnGrantExecute.Text = "Thực hiện cấp quyền (GRANT)";
            this.btnGrantExecute.UseVisualStyleBackColor = true;
            this.btnGrantExecute.Click += new System.EventHandler(this.btnGrantExecute_Click);
            // 
            // btnPickColumns
            // 
            this.btnPickColumns.Location = new System.Drawing.Point(709, 154);
            this.btnPickColumns.Name = "btnPickColumns";
            this.btnPickColumns.Size = new System.Drawing.Size(230, 23);
            this.btnPickColumns.TabIndex = 13;
            this.btnPickColumns.Text = "Cấp quyền mức cột...";
            this.btnPickColumns.UseVisualStyleBackColor = true;
            this.btnPickColumns.Click += new System.EventHandler(this.btnPickColumns_Click);
            // 
            // btnLoadObjects
            // 
            this.btnLoadObjects.Location = new System.Drawing.Point(709, 113);
            this.btnLoadObjects.Name = "btnLoadObjects";
            this.btnLoadObjects.Size = new System.Drawing.Size(190, 23);
            this.btnLoadObjects.TabIndex = 10;
            this.btnLoadObjects.Text = "Tải danh sách đối tượng";
            this.btnLoadObjects.UseVisualStyleBackColor = true;
            this.btnLoadObjects.Click += new System.EventHandler(this.btnLoadObjects_Click);
            // 
            // txtObjectFilter
            // 
            this.txtObjectFilter.Location = new System.Drawing.Point(334, 116);
            this.txtObjectFilter.Name = "txtObjectFilter";
            this.txtObjectFilter.Size = new System.Drawing.Size(357, 20);
            this.txtObjectFilter.TabIndex = 9;
            // 
            // lblObjectFilter
            // 
            this.lblObjectFilter.AutoSize = true;
            this.lblObjectFilter.Location = new System.Drawing.Point(250, 119);
            this.lblObjectFilter.Name = "lblObjectFilter";
            this.lblObjectFilter.Size = new System.Drawing.Size(78, 13);
            this.lblObjectFilter.TabIndex = 8;
            this.lblObjectFilter.Text = "Lọc (tùy chọn):";
            this.lblObjectFilter.Click += new System.EventHandler(this.lblObjectFilter_Click);
            // 
            // txtColumns
            // 
            this.txtColumns.Location = new System.Drawing.Point(141, 180);
            this.txtColumns.Name = "txtColumns";
            this.txtColumns.Size = new System.Drawing.Size(550, 20);
            this.txtColumns.TabIndex = 13;
            // 
            // lblColumns
            // 
            this.lblColumns.AutoSize = true;
            this.lblColumns.Location = new System.Drawing.Point(15, 183);
            this.lblColumns.Name = "lblColumns";
            this.lblColumns.Size = new System.Drawing.Size(68, 13);
            this.lblColumns.TabIndex = 12;
            this.lblColumns.Text = "Cột (nếu có):";
            // 
            // cboObject
            // 
            this.cboObject.FormattingEnabled = true;
            this.cboObject.Location = new System.Drawing.Point(141, 154);
            this.cboObject.Name = "cboObject";
            this.cboObject.Size = new System.Drawing.Size(550, 21);
            this.cboObject.TabIndex = 12;
            // 
            // lblObject
            // 
            this.lblObject.AutoSize = true;
            this.lblObject.Location = new System.Drawing.Point(15, 157);
            this.lblObject.Name = "lblObject";
            this.lblObject.Size = new System.Drawing.Size(120, 13);
            this.lblObject.TabIndex = 11;
            this.lblObject.Text = "Đối tượng (schema.tên):";
            // 
            // chkWithGrantOption
            // 
            this.chkWithGrantOption.AutoSize = true;
            this.chkWithGrantOption.Location = new System.Drawing.Point(709, 78);
            this.chkWithGrantOption.Name = "chkWithGrantOption";
            this.chkWithGrantOption.Size = new System.Drawing.Size(229, 17);
            this.chkWithGrantOption.TabIndex = 7;
            this.chkWithGrantOption.Text = "Cho phép cấp lại (WITH GRANT OPTION)";
            this.chkWithGrantOption.UseVisualStyleBackColor = true;
            this.chkWithGrantOption.CheckedChanged += new System.EventHandler(this.chkWithGrantOption_CheckedChanged);
            // 
            // radGrantObjPriv
            // 
            this.radGrantObjPriv.AutoSize = true;
            this.radGrantObjPriv.Location = new System.Drawing.Point(540, 77);
            this.radGrantObjPriv.Name = "radGrantObjPriv";
            this.radGrantObjPriv.Size = new System.Drawing.Size(125, 17);
            this.radGrantObjPriv.TabIndex = 6;
            this.radGrantObjPriv.Text = "Quyền trên đối tượng";
            this.radGrantObjPriv.UseVisualStyleBackColor = true;
            this.radGrantObjPriv.CheckedChanged += new System.EventHandler(this.radGrant_CheckedChanged);
            // 
            // radGrantSysPriv
            // 
            this.radGrantSysPriv.AutoSize = true;
            this.radGrantSysPriv.Location = new System.Drawing.Point(403, 78);
            this.radGrantSysPriv.Name = "radGrantSysPriv";
            this.radGrantSysPriv.Size = new System.Drawing.Size(101, 17);
            this.radGrantSysPriv.TabIndex = 5;
            this.radGrantSysPriv.Text = "Quyền hệ thống";
            this.radGrantSysPriv.UseVisualStyleBackColor = true;
            this.radGrantSysPriv.CheckedChanged += new System.EventHandler(this.radGrant_CheckedChanged);
            // 
            // radGrantRole
            // 
            this.radGrantRole.AutoSize = true;
            this.radGrantRole.Checked = true;
            this.radGrantRole.Location = new System.Drawing.Point(324, 78);
            this.radGrantRole.Name = "radGrantRole";
            this.radGrantRole.Size = new System.Drawing.Size(55, 17);
            this.radGrantRole.TabIndex = 4;
            this.radGrantRole.TabStop = true;
            this.radGrantRole.Text = "Vai trò";
            this.radGrantRole.UseVisualStyleBackColor = true;
            this.radGrantRole.CheckedChanged += new System.EventHandler(this.radGrant_CheckedChanged);
            // 
            // cboPrivilege
            // 
            this.cboPrivilege.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPrivilege.FormattingEnabled = true;
            this.cboPrivilege.Location = new System.Drawing.Point(324, 52);
            this.cboPrivilege.Name = "cboPrivilege";
            this.cboPrivilege.Size = new System.Drawing.Size(278, 21);
            this.cboPrivilege.TabIndex = 3;
            this.cboPrivilege.SelectedIndexChanged += new System.EventHandler(this.cboPrivilege_SelectedIndexChanged);
            // 
            // lblPrivilege
            // 
            this.lblPrivilege.AutoSize = true;
            this.lblPrivilege.Location = new System.Drawing.Point(277, 55);
            this.lblPrivilege.Name = "lblPrivilege";
            this.lblPrivilege.Size = new System.Drawing.Size(41, 13);
            this.lblPrivilege.TabIndex = 2;
            this.lblPrivilege.Text = "Quyền:";
            // 
            // cboObjectType
            // 
            this.cboObjectType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboObjectType.FormattingEnabled = true;
            this.cboObjectType.Location = new System.Drawing.Point(104, 52);
            this.cboObjectType.Name = "cboObjectType";
            this.cboObjectType.Size = new System.Drawing.Size(154, 21);
            this.cboObjectType.TabIndex = 1;
            this.cboObjectType.SelectedIndexChanged += new System.EventHandler(this.cboObjectType_SelectedIndexChanged);
            // 
            // lblObjectType
            // 
            this.lblObjectType.AutoSize = true;
            this.lblObjectType.Location = new System.Drawing.Point(15, 56);
            this.lblObjectType.Name = "lblObjectType";
            this.lblObjectType.Size = new System.Drawing.Size(78, 13);
            this.lblObjectType.TabIndex = 0;
            this.lblObjectType.Text = "Loại đối tượng:";
            // 
            // cboGrantee
            // 
            this.cboGrantee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGrantee.FormattingEnabled = true;
            this.cboGrantee.Location = new System.Drawing.Point(104, 25);
            this.cboGrantee.Name = "cboGrantee";
            this.cboGrantee.Size = new System.Drawing.Size(498, 21);
            this.cboGrantee.TabIndex = 1;
            // 
            // lblGrantee
            // 
            this.lblGrantee.AutoSize = true;
            this.lblGrantee.Location = new System.Drawing.Point(15, 28);
            this.lblGrantee.Name = "lblGrantee";
            this.lblGrantee.Size = new System.Drawing.Size(83, 13);
            this.lblGrantee.TabIndex = 0;
            this.lblGrantee.Text = "Đối tượng nhận:";
            // 
            // tabAudit
            // 
            this.tabAudit.Controls.Add(this.gridAudit);
            this.tabAudit.Controls.Add(this.btnRevokeSelected);
            this.tabAudit.Controls.Add(this.btnAuditRefresh);
            this.tabAudit.Controls.Add(this.cboAuditKind);
            this.tabAudit.Controls.Add(this.lblAuditKind);
            this.tabAudit.Controls.Add(this.txtAuditFilter);
            this.tabAudit.Controls.Add(this.lblAuditFilter);
            this.tabAudit.Location = new System.Drawing.Point(4, 22);
            this.tabAudit.Name = "tabAudit";
            this.tabAudit.Padding = new System.Windows.Forms.Padding(10);
            this.tabAudit.Size = new System.Drawing.Size(1192, 749);
            this.tabAudit.TabIndex = 2;
            this.tabAudit.Text = "Kiểm tra & thu hồi quyền";
            this.tabAudit.UseVisualStyleBackColor = true;
            // 
            // gridAudit
            // 
            this.gridAudit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAudit.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAudit.Location = new System.Drawing.Point(13, 52);
            this.gridAudit.Name = "gridAudit";
            this.gridAudit.RowHeadersWidth = 45;
            this.gridAudit.Size = new System.Drawing.Size(1148, 684);
            this.gridAudit.TabIndex = 6;
            // 
            // btnRevokeSelected
            // 
            this.btnRevokeSelected.Location = new System.Drawing.Point(914, 13);
            this.btnRevokeSelected.Name = "btnRevokeSelected";
            this.btnRevokeSelected.Size = new System.Drawing.Size(120, 23);
            this.btnRevokeSelected.TabIndex = 5;
            this.btnRevokeSelected.Text = "Thu hồi dòng chọn";
            this.btnRevokeSelected.UseVisualStyleBackColor = true;
            this.btnRevokeSelected.Click += new System.EventHandler(this.btnRevokeSelected_Click);
            // 
            // btnAuditRefresh
            // 
            this.btnAuditRefresh.Location = new System.Drawing.Point(732, 13);
            this.btnAuditRefresh.Name = "btnAuditRefresh";
            this.btnAuditRefresh.Size = new System.Drawing.Size(120, 23);
            this.btnAuditRefresh.TabIndex = 4;
            this.btnAuditRefresh.Text = "Làm mới";
            this.btnAuditRefresh.UseVisualStyleBackColor = true;
            this.btnAuditRefresh.Click += new System.EventHandler(this.btnAuditRefresh_Click);
            // 
            // cboAuditKind
            // 
            this.cboAuditKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAuditKind.FormattingEnabled = true;
            this.cboAuditKind.Items.AddRange(new object[] {
            "USER",
            "ROLE"});
            this.cboAuditKind.Location = new System.Drawing.Point(533, 15);
            this.cboAuditKind.Name = "cboAuditKind";
            this.cboAuditKind.Size = new System.Drawing.Size(121, 21);
            this.cboAuditKind.TabIndex = 3;
            // 
            // lblAuditKind
            // 
            this.lblAuditKind.AutoSize = true;
            this.lblAuditKind.Location = new System.Drawing.Point(439, 18);
            this.lblAuditKind.Name = "lblAuditKind";
            this.lblAuditKind.Size = new System.Drawing.Size(88, 13);
            this.lblAuditKind.TabIndex = 2;
            this.lblAuditKind.Text = "Loại (User/Role):";
            // 
            // txtAuditFilter
            // 
            this.txtAuditFilter.Location = new System.Drawing.Point(155, 15);
            this.txtAuditFilter.Name = "txtAuditFilter";
            this.txtAuditFilter.Size = new System.Drawing.Size(225, 20);
            this.txtAuditFilter.TabIndex = 1;
            // 
            // lblAuditFilter
            // 
            this.lblAuditFilter.AutoSize = true;
            this.lblAuditFilter.Location = new System.Drawing.Point(10, 18);
            this.lblAuditFilter.Name = "lblAuditFilter";
            this.lblAuditFilter.Size = new System.Drawing.Size(139, 13);
            this.lblAuditFilter.TabIndex = 0;
            this.lblAuditFilter.Text = "Lọc theo tài khoản / vai trò:";
            // 
            // ShellForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.menu);
            this.MainMenuStrip = this.menu;
            this.Name = "ShellForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NHOM 09 - Quản trị CSDL Oracle";
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.tabManage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridManageRoles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridManageUsers)).EndInit();
            this.grpRole.ResumeLayout(false);
            this.grpRole.PerformLayout();
            this.grpUser.ResumeLayout(false);
            this.grpUser.PerformLayout();
            this.tabGrant.ResumeLayout(false);
            this.grpGrant.ResumeLayout(false);
            this.grpGrant.PerformLayout();
            this.tabAudit.ResumeLayout(false);
            this.tabAudit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAudit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem mnuSystem;
        private System.Windows.Forms.ToolStripMenuItem mnuConnect;
        private System.Windows.Forms.ToolStripMenuItem mnuLogout;
        private System.Windows.Forms.ToolStripMenuItem mnuRefresh;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabManage;
        private System.Windows.Forms.TabPage tabGrant;
        private System.Windows.Forms.TabPage tabAudit;

        private System.Windows.Forms.Button btnManageRefresh;
        private System.Windows.Forms.GroupBox grpUser;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtUserPass;
        private System.Windows.Forms.Label lblUserPass;
        private System.Windows.Forms.Button btnUserCreate;
        private System.Windows.Forms.Button btnUserAlter;
        private System.Windows.Forms.Button btnUserDrop;
        private System.Windows.Forms.GroupBox grpRole;
        private System.Windows.Forms.TextBox txtRoleName;
        private System.Windows.Forms.Label lblRoleName;
        private System.Windows.Forms.Button btnRoleCreate;
        private System.Windows.Forms.Button btnRoleDrop;
        private System.Windows.Forms.DataGridView gridManageUsers;
        private System.Windows.Forms.DataGridView gridManageRoles;

        private System.Windows.Forms.GroupBox grpGrant;
        private System.Windows.Forms.ComboBox cboGrantee;
        private System.Windows.Forms.Label lblGrantee;
        private System.Windows.Forms.ComboBox cboObjectType;
        private System.Windows.Forms.Label lblObjectType;
        private System.Windows.Forms.ComboBox cboPrivilege;
        private System.Windows.Forms.Label lblPrivilege;
        private System.Windows.Forms.RadioButton radGrantRole;
        private System.Windows.Forms.RadioButton radGrantSysPriv;
        private System.Windows.Forms.RadioButton radGrantObjPriv;
        private System.Windows.Forms.CheckBox chkWithGrantOption;
        private System.Windows.Forms.ComboBox cboObject;
        private System.Windows.Forms.Label lblObject;
        private System.Windows.Forms.Label lblObjectFilter;
        private System.Windows.Forms.TextBox txtObjectFilter;
        private System.Windows.Forms.Button btnLoadObjects;
        private System.Windows.Forms.TextBox txtColumns;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.Button btnPickColumns;
        private System.Windows.Forms.Button btnGrantExecute;
        private System.Windows.Forms.Button btnGrantRefreshTargets;

        private System.Windows.Forms.Label lblAuditFilter;
        private System.Windows.Forms.TextBox txtAuditFilter;
        private System.Windows.Forms.Label lblAuditKind;
        private System.Windows.Forms.ComboBox cboAuditKind;
        private System.Windows.Forms.Button btnAuditRefresh;
        private System.Windows.Forms.Button btnRevokeSelected;
        private System.Windows.Forms.DataGridView gridAudit;
    }
}

