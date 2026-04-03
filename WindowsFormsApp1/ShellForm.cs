using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ShellForm : Form
    {
        private string _connectionString;

        public ShellForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            SetupGrantUi();
        }

        private void SetupGrantUi()
        {
            cboObjectType.Items.Clear();
            cboObjectType.Items.AddRange(new object[] { "Bảng (TABLE)", "Khung nhìn (VIEW)", "Thủ tục (PROCEDURE)", "Hàm (FUNCTION)" });
            cboObjectType.SelectedIndex = 0;

            PopulatePrivilegesForSelectedType();
            UpdateColumnControlsState();
        }

        private string GetSelectedObjectType()
        {
            var s = cboObjectType.SelectedItem?.ToString() ?? "";
            if (s.Contains("TABLE")) return "TABLE";
            if (s.Contains("VIEW")) return "VIEW";
            if (s.Contains("PROCEDURE")) return "PROCEDURE";
            if (s.Contains("FUNCTION")) return "FUNCTION";
            return "TABLE";
        }

        private void PopulatePrivilegesForSelectedType()
        {
            var type = GetSelectedObjectType();
            cboPrivilege.Items.Clear();

            if (radGrantRole.Checked)
            {
                cboPrivilege.Items.Add("CONNECT");
                cboPrivilege.Items.Add("RESOURCE");
                cboPrivilege.Items.Add("DBA");
            }
            else if (radGrantSysPriv.Checked)
            {
                cboPrivilege.Items.Add("CREATE SESSION");
                cboPrivilege.Items.Add("CREATE USER");
                cboPrivilege.Items.Add("ALTER USER");
                cboPrivilege.Items.Add("DROP USER");
                cboPrivilege.Items.Add("CREATE ROLE");
                cboPrivilege.Items.Add("DROP ROLE");
                cboPrivilege.Items.Add("GRANT ANY PRIVILEGE");
                cboPrivilege.Items.Add("GRANT ANY ROLE");
            }
            else
            {
                // Object privileges by type
                if (type == "TABLE" || type == "VIEW")
                {
                    cboPrivilege.Items.AddRange(new object[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
                }
                else if (type == "PROCEDURE" || type == "FUNCTION")
                {
                    cboPrivilege.Items.Add("EXECUTE");
                }
                else
                {
                    cboPrivilege.Items.AddRange(new object[] { "SELECT", "INSERT", "UPDATE", "DELETE" });
                }
            }

            if (cboPrivilege.Items.Count > 0)
                cboPrivilege.SelectedIndex = 0;
        }

        private void UpdateColumnControlsState()
        {
            var type = GetSelectedObjectType();
            var priv = (cboPrivilege.SelectedItem?.ToString() ?? "").Trim().ToUpperInvariant();

            var allowColumn =
                radGrantObjPriv.Checked &&
                (type == "TABLE" || type == "VIEW") &&
                (priv == "SELECT" || priv == "UPDATE");

            btnPickColumns.Enabled = allowColumn;
            txtColumns.ReadOnly = !allowColumn;
            if (!allowColumn) txtColumns.Text = string.Empty;
        }

        private void mnuConnect_Click(object sender, EventArgs e)
        {
            using (var login = new LoginForm())
            {
                if (login.ShowDialog(this) == DialogResult.OK)
                    _connectionString = login.ConnectionString;
            }
        }

        private void mnuLogout_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void mnuRefresh_Click(object sender, EventArgs e)
        {
            if (tabs.SelectedTab == tabManage)
                await RefreshManageAsync();
            else if (tabs.SelectedTab == tabGrant)
                await RefreshGrantTargetsAsync();
            else if (tabs.SelectedTab == tabAudit)
                await RefreshAuditAsync();
        }

        private void ShowOracleError(Exception ex)
        {
            MessageBox.Show(this, ex.Message, "NHOM 09 - Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ===== TAB 1: Manage (Users/Roles) =====
        private async Task RefreshManageAsync()
        {
            try
            {
                var dtUsers = await OracleSql.QueryAsync(_connectionString,
                    "select username, account_status, created from dba_users order by username");
                gridManageUsers.DataSource = dtUsers;

                var dtRoles = await OracleSql.QueryAsync(_connectionString,
                    "select role, authentication_type from dba_roles order by role");
                gridManageRoles.DataSource = dtRoles;
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnManageRefresh_Click(object sender, EventArgs e) => await RefreshManageAsync();

        private async void btnUserCreate_Click(object sender, EventArgs e)
        {
            var user = (txtUserName.Text ?? "").Trim();
            var pwd = txtUserPass.Text ?? "";
            if (string.IsNullOrEmpty(user))
            {
                MessageBox.Show(this, "Vui lòng nhập tên tài khoản.", "NHOM 09", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(pwd))
            {
                MessageBox.Show(this, "Vui lòng nhập mật khẩu (không để trống).", "NHOM 09", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var sql = $"create user {OracleSql.Q(user)} identified by {OracleSql.QPassword(pwd)}";
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await RefreshManageAsync();
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnUserAlter_Click(object sender, EventArgs e)
        {
            var user = (txtUserName.Text ?? "").Trim();
            var pwd = txtUserPass.Text ?? "";
            if (string.IsNullOrEmpty(user))
            {
                MessageBox.Show(this, "Vui lòng nhập tên tài khoản.", "NHOM 09", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(pwd))
            {
                MessageBox.Show(this, "Vui lòng nhập mật khẩu mới.", "NHOM 09", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var sql = $"alter user {OracleSql.Q(user)} identified by {OracleSql.QPassword(pwd)}";
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await RefreshManageAsync();
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnUserDrop_Click(object sender, EventArgs e)
        {
            try
            {
                var sql = $"drop user {OracleSql.Q(txtUserName.Text)} cascade";
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await RefreshManageAsync();
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnRoleCreate_Click(object sender, EventArgs e)
        {
            try
            {
                var sql = $"create role {OracleSql.Q(txtRoleName.Text)}";
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await RefreshManageAsync();
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnRoleDrop_Click(object sender, EventArgs e)
        {
            try
            {
                var sql = $"drop role {OracleSql.Q(txtRoleName.Text)}";
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await RefreshManageAsync();
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        // ===== TAB 2: Grant =====
        private async Task RefreshGrantTargetsAsync()
        {
            try
            {
                var dt = await OracleSql.QueryAsync(_connectionString,
                    "select username as name, 'USER' as kind from dba_users " +
                    "union all " +
                    "select role as name, 'ROLE' as kind from dba_roles " +
                    "order by kind, name");

                cboGrantee.DisplayMember = "name";
                cboGrantee.ValueMember = "name";
                cboGrantee.DataSource = dt;
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnGrantRefreshTargets_Click(object sender, EventArgs e) => await RefreshGrantTargetsAsync();

        private async void btnPickColumns_Click(object sender, EventArgs e)
        {
            try
            {
                var obj = cboObject.Text.Trim(); // schema.object
                var parts = obj.Split('.');
                if (parts.Length != 2) throw new InvalidOperationException("Object phải theo dạng SCHEMA.OBJECT");

                var owner = parts[0].ToUpperInvariant();
                var table = parts[1].ToUpperInvariant();

                var dt = await OracleSql.QueryAsync(_connectionString,
                    $"select column_name from dba_tab_columns where owner = {OracleSql.QLit(owner)} and table_name = {OracleSql.QLit(table)} order by column_id");

                var cols = dt.Rows.Cast<DataRow>().Select(r => r[0]?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                using (var dlg = new ColumnSelectForm(cols))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        txtColumns.Text = string.Join(", ", dlg.SelectedColumns);
                }
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private void cboPrivilege_SelectedIndexChanged(object sender, EventArgs e) => UpdateColumnControlsState();
        private void cboObjectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePrivilegesForSelectedType();
            UpdateColumnControlsState();
        }

        private void radGrant_CheckedChanged(object sender, EventArgs e)
        {
            PopulatePrivilegesForSelectedType();
            UpdateColumnControlsState();
        }

        private async void btnLoadObjects_Click(object sender, EventArgs e)
        {
            try
            {
                var type = GetSelectedObjectType();
                var filter = (txtObjectFilter.Text ?? "").Trim().ToUpperInvariant();
                var like = string.IsNullOrWhiteSpace(filter) ? "" : $" and (owner||'.'||object_name) like {OracleSql.QLit("%" + filter + "%")}";

                string sql;
                if (type == "TABLE")
                {
                    sql = $"select owner||'.'||table_name as name from dba_tables where 1=1{like} order by owner, table_name";
                }
                else if (type == "VIEW")
                {
                    sql = $"select owner||'.'||view_name as name from dba_views where 1=1{like} order by owner, view_name";
                }
                else if (type == "PROCEDURE")
                {
                    sql = $"select owner||'.'||object_name as name from dba_objects where object_type='PROCEDURE'{like} order by owner, object_name";
                }
                else // FUNCTION
                {
                    sql = $"select owner||'.'||object_name as name from dba_objects where object_type='FUNCTION'{like} order by owner, object_name";
                }

                var dt = await OracleSql.QueryAsync(_connectionString, sql);
                cboObject.DisplayMember = "name";
                cboObject.ValueMember = "name";
                cboObject.DataSource = dt;
                if (dt.Rows.Count > 0) cboObject.SelectedIndex = 0;
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnGrantExecute_Click(object sender, EventArgs e)
        {
            try
            {
                var grantee = cboGrantee.SelectedValue?.ToString() ?? "";
                var priv = (cboPrivilege.SelectedItem?.ToString() ?? "").Trim();
                var withOption = chkWithGrantOption.Checked ? " with grant option" : "";

                string sql;
                if (radGrantRole.Checked)
                {
                    sql = $"grant {OracleSql.Q(priv)} to {OracleSql.Q(grantee)}{(chkWithGrantOption.Checked ? " with admin option" : "")}";
                }
                else if (radGrantSysPriv.Checked)
                {
                    sql = $"grant {priv} to {OracleSql.Q(grantee)}{(chkWithGrantOption.Checked ? " with admin option" : "")}";
                }
                else
                {
                    var obj = cboObject.Text.Trim();
                    var cols = txtColumns.Text.Trim();

                    if ((priv.Equals("select", StringComparison.OrdinalIgnoreCase) || priv.Equals("update", StringComparison.OrdinalIgnoreCase))
                        && !string.IsNullOrWhiteSpace(cols))
                        sql = $"grant {priv}({cols}) on {obj} to {OracleSql.Q(grantee)}{withOption}";
                    else
                        sql = $"grant {priv} on {obj} to {OracleSql.Q(grantee)}{withOption}";
                }

                await OracleSql.ExecuteAsync(_connectionString, sql);
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        // ===== TAB 3: Audit/Revoke =====
        private async Task RefreshAuditAsync()
        {
            try
            {
                var filter = (txtAuditFilter.Text ?? "").Trim().ToUpperInvariant();
                var kind = (cboAuditKind.SelectedItem?.ToString() ?? "USER");

                var where = string.IsNullOrWhiteSpace(filter) ? "" : $" where grantee = {OracleSql.QLit(filter)}";

                var sql = $@"
select grantee,
       privilege as priv,
       owner || '.' || table_name as obj,
       cast(null as varchar2(4000)) as cols,
       grantable as opt
from dba_tab_privs{where}
union all
select grantee,
       privilege as priv,
       owner || '.' || table_name as obj,
       column_name as cols,
       grantable as opt
from dba_col_privs{where}
union all
select grantee,
       privilege as priv,
       cast(null as varchar2(4000)) as obj,
       cast(null as varchar2(4000)) as cols,
       admin_option as opt
from dba_sys_privs{where}
union all
select grantee,
       granted_role as priv,
       cast(null as varchar2(4000)) as obj,
       cast(null as varchar2(4000)) as cols,
       admin_option as opt
from dba_role_privs{where}";

                var dt = await OracleSql.QueryAsync(_connectionString, sql);
                gridAudit.DataSource = dt;
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private async void btnAuditRefresh_Click(object sender, EventArgs e) => await RefreshAuditAsync();

        private async void btnRevokeSelected_Click(object sender, EventArgs e)
        {
            try
            {
                if (gridAudit.CurrentRow == null) return;
                var r = ((DataRowView)gridAudit.CurrentRow.DataBoundItem)?.Row;
                if (r == null) return;

                var grantee = r["grantee"]?.ToString();
                var priv = r["priv"]?.ToString();
                var obj = r["obj"]?.ToString();
                var cols = r["cols"]?.ToString();

                string sql;
                if (!string.IsNullOrWhiteSpace(obj))
                {
                    if (!string.IsNullOrWhiteSpace(cols) &&
                        (priv.Equals("select", StringComparison.OrdinalIgnoreCase) || priv.Equals("update", StringComparison.OrdinalIgnoreCase)))
                        sql = $"revoke {priv}({cols}) on {obj} from {OracleSql.Q(grantee)}";
                    else
                        sql = $"revoke {priv} on {obj} from {OracleSql.Q(grantee)}";
                }
                else
                {
                    // sys priv hoặc role: không có obj
                    sql = $"revoke {priv} from {OracleSql.Q(grantee)}";
                }

                await OracleSql.ExecuteAsync(_connectionString, sql);
                await RefreshAuditAsync();
            }
            catch (Exception ex) { ShowOracleError(ex); }
        }

        private void chkWithGrantOption_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void lblObjectFilter_Click(object sender, EventArgs e)
        {

        }
    }
}

