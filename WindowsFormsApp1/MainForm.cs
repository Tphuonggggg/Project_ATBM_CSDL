using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        private string _connectionString;

        public MainForm()
        {
            InitializeComponent();
            _connectionString = OracleDb.GetConnectionStringFromConfig();
            HydrateConnInputsFromConnectionString(_connectionString);
            UpdateConnPreview();
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            btnTestConn.Enabled = !busy;
            btnApplyConn.Enabled = !busy;
            tabs.Enabled = !busy;
        }

        private async void btnTestConn_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            try
            {
                _connectionString = OracleDb.WithSysdba(BuildConnFromInputs(), chkSysdba.Checked);
                await OracleDb.TestConnectionAsync(_connectionString);
                MessageBox.Show(this, "Kết nối thành công.", "NHOM 09", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "NHOM 09 - Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnApplyConn_Click(object sender, EventArgs e)
        {
            _connectionString = OracleDb.WithSysdba(BuildConnFromInputs(), chkSysdba.Checked);
            MessageBox.Show(this, "Đã áp dụng chuỗi kết nối cho toàn bộ ứng dụng (không ghi ra file).", "NHOM 09", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async Task LoadToGridAsync(DataGridView grid, string sql)
        {
            SetBusy(true);
            try
            {
                _connectionString = OracleDb.WithSysdba(BuildConnFromInputs(), chkSysdba.Checked);
                var dt = await OracleSql.QueryAsync(_connectionString, sql);
                grid.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "NHOM 09 - Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ExecSqlAsync(string sql)
        {
            SetBusy(true);
            try
            {
                _connectionString = OracleDb.WithSysdba(BuildConnFromInputs(), chkSysdba.Checked);
                var affected = await OracleSql.ExecuteAsync(_connectionString, sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "NHOM 09 - Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private string BuildConnFromInputs()
        {
            return OracleDb.BuildConnectionString(
                host: txtHost.Text,
                port: (int)numPort.Value,
                serviceName: txtService.Text,
                userId: txtUser.Text,
                password: txtPassword.Text,
                asSysdba: false);
        }

        private void UpdateConnPreview()
        {
            try
            {
                var cs = OracleDb.WithSysdba(BuildConnFromInputs(), chkSysdba.Checked);
                txtConnPreview.Text = cs;
            }
            catch
            {
                txtConnPreview.Text = string.Empty;
            }
        }

        private void HydrateConnInputsFromConnectionString(string cs)
        {
            // Best-effort parse: nếu parse fail thì để default UI.
            try
            {
                var b = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder(cs);

                if (!string.IsNullOrWhiteSpace(b.UserID)) txtUser.Text = b.UserID;
                if (!string.IsNullOrWhiteSpace(b.Password)) txtPassword.Text = b.Password;

                var ds = (b.DataSource ?? string.Empty).Trim();
                // Format: host:port/service
                var slashIdx = ds.IndexOf('/');
                if (slashIdx > 0 && slashIdx < ds.Length - 1)
                {
                    txtService.Text = ds.Substring(slashIdx + 1);
                    var left = ds.Substring(0, slashIdx);
                    var colonIdx = left.LastIndexOf(':');
                    if (colonIdx > 0 && colonIdx < left.Length - 1)
                    {
                        txtHost.Text = left.Substring(0, colonIdx);
                        if (int.TryParse(left.Substring(colonIdx + 1), out var p))
                            numPort.Value = Math.Max(numPort.Minimum, Math.Min(numPort.Maximum, p));
                    }
                    else
                    {
                        txtHost.Text = left;
                    }
                }

                chkSysdba.Checked = cs.IndexOf("DBA Privilege=SYSDBA", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                // ignore
            }
        }

        private void txtHost_TextChanged(object sender, EventArgs e) => UpdateConnPreview();
        private void txtService_TextChanged(object sender, EventArgs e) => UpdateConnPreview();
        private void txtUser_TextChanged(object sender, EventArgs e) => UpdateConnPreview();
        private void txtPassword_TextChanged(object sender, EventArgs e) => UpdateConnPreview();
        private void numPort_ValueChanged(object sender, EventArgs e) => UpdateConnPreview();
        private void chkSysdba_CheckedChanged(object sender, EventArgs e) => UpdateConnPreview();

        // USERS
        private async void btnUsersRefresh_Click(object sender, EventArgs e)
        {
            await LoadToGridAsync(gridUsers,
                "select username, account_status, created from dba_users order by username");
        }

        private async void btnUserCreate_Click(object sender, EventArgs e)
        {
            var u = (txtUserName.Text ?? "").Trim();
            var p = txtUserPass.Text ?? "";
            var sql = $"create user {OracleSql.Q(u)} identified by {OracleSql.QPassword(p)}";
            await ExecSqlAsync(sql);
        }

        private async void btnUserAlterPass_Click(object sender, EventArgs e)
        {
            var u = (txtUserName.Text ?? "").Trim();
            var p = txtUserPass.Text ?? "";
            var sql = $"alter user {OracleSql.Q(u)} identified by {OracleSql.QPassword(p)}";
            await ExecSqlAsync(sql);
        }

        private async void btnUserDrop_Click(object sender, EventArgs e)
        {
            var u = txtUserName.Text;
            var sql = $"drop user {OracleSql.Q(u)} cascade";
            await ExecSqlAsync(sql);
        }

        private async void btnUserLock_Click(object sender, EventArgs e)
        {
            var u = txtUserName.Text;
            var sql = $"alter user {OracleSql.Q(u)} account lock";
            await ExecSqlAsync(sql);
        }

        private async void btnUserUnlock_Click(object sender, EventArgs e)
        {
            var u = txtUserName.Text;
            var sql = $"alter user {OracleSql.Q(u)} account unlock";
            await ExecSqlAsync(sql);
        }

        // ROLES
        private async void btnRolesRefresh_Click(object sender, EventArgs e)
        {
            await LoadToGridAsync(gridRoles, "select role, authentication_type from dba_roles order by role");
        }

        private async void btnRoleCreate_Click(object sender, EventArgs e)
        {
            var r = txtRoleName.Text;
            var sql = $"create role {OracleSql.Q(r)}";
            await ExecSqlAsync(sql);
        }

        private async void btnRoleDrop_Click(object sender, EventArgs e)
        {
            var r = txtRoleName.Text;
            var sql = $"drop role {OracleSql.Q(r)}";
            await ExecSqlAsync(sql);
        }

        // GRANT / REVOKE
        private async void btnGrant_Click(object sender, EventArgs e)
        {
            var grantee = txtGrantee.Text;
            var privOrRole = txtGrantWhat.Text;

            var withOption = chkWithOption.Checked
                ? (radGrantRole.Checked ? " with admin option" : " with grant option")
                : string.Empty;

            string sql;
            if (radGrantRole.Checked)
            {
                sql = $"grant {OracleSql.Q(privOrRole)} to {OracleSql.Q(grantee)}{withOption}";
            }
            else if (radGrantSysPriv.Checked)
            {
                sql = $"grant {privOrRole.Trim()} to {OracleSql.Q(grantee)}{(chkWithOption.Checked ? " with admin option" : "")}";
            }
            else
            {
                // object priv
                var obj = txtObjectName.Text; // schema.object
                var cols = txtColumns.Text.Trim();
                var priv = privOrRole.Trim(); // select/update/insert/delete/execute...

                if ((priv.Equals("select", StringComparison.OrdinalIgnoreCase) ||
                     priv.Equals("update", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(cols))
                {
                    sql = $"grant {priv}({cols}) on {obj} to {OracleSql.Q(grantee)}{withOption}";
                }
                else
                {
                    sql = $"grant {priv} on {obj} to {OracleSql.Q(grantee)}{withOption}";
                }
            }

            await ExecSqlAsync(sql);
        }

        private async void btnRevoke_Click(object sender, EventArgs e)
        {
            var grantee = txtGrantee.Text;
            var privOrRole = txtGrantWhat.Text;

            string sql;
            if (radGrantRole.Checked)
            {
                sql = $"revoke {OracleSql.Q(privOrRole)} from {OracleSql.Q(grantee)}";
            }
            else if (radGrantSysPriv.Checked)
            {
                sql = $"revoke {privOrRole.Trim()} from {OracleSql.Q(grantee)}";
            }
            else
            {
                var obj = txtObjectName.Text;
                var cols = txtColumns.Text.Trim();
                var priv = privOrRole.Trim();

                if ((priv.Equals("select", StringComparison.OrdinalIgnoreCase) ||
                     priv.Equals("update", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(cols))
                {
                    sql = $"revoke {priv}({cols}) on {obj} from {OracleSql.Q(grantee)}";
                }
                else
                {
                    sql = $"revoke {priv} on {obj} from {OracleSql.Q(grantee)}";
                }
            }

            await ExecSqlAsync(sql);
        }

        // VIEW PRIVS
        private async void btnViewPrivs_Click(object sender, EventArgs e)
        {
            var name = txtViewName.Text.Trim();
            var kind = cboViewKind.SelectedItem?.ToString() ?? "USER";
            var upper = name.ToUpperInvariant();

            string sql;
            if (kind == "ROLE")
            {
                sql = $@"
select 'ROLE_PRIVS' as SRC, grantee, granted_role, admin_option as opt, default_role
from dba_role_privs where grantee = {OracleSql.QLit(upper)}
union all
select 'SYS_PRIVS' as SRC, grantee, privilege as granted_role, admin_option as opt, cast(null as varchar2(3)) as default_role
from dba_sys_privs where grantee = {OracleSql.QLit(upper)}
union all
select 'TAB_PRIVS' as SRC, grantee, privilege || ' ON ' || owner || '.' || table_name as granted_role, grantable as opt, cast(null as varchar2(3)) as default_role
from dba_tab_privs where grantee = {OracleSql.QLit(upper)}
union all
select 'COL_PRIVS' as SRC, grantee, privilege || '(' || column_name || ') ON ' || owner || '.' || table_name as granted_role, grantable as opt, cast(null as varchar2(3)) as default_role
from dba_col_privs where grantee = {OracleSql.QLit(upper)}";
            }
            else
            {
                sql = $@"
select 'ROLE_PRIVS' as SRC, grantee, granted_role, admin_option as opt, default_role
from dba_role_privs where grantee = {OracleSql.QLit(upper)}
union all
select 'SYS_PRIVS' as SRC, grantee, privilege as granted_role, admin_option as opt, cast(null as varchar2(3)) as default_role
from dba_sys_privs where grantee = {OracleSql.QLit(upper)}
union all
select 'TAB_PRIVS' as SRC, grantee, privilege || ' ON ' || owner || '.' || table_name as granted_role, grantable as opt, cast(null as varchar2(3)) as default_role
from dba_tab_privs where grantee = {OracleSql.QLit(upper)}
union all
select 'COL_PRIVS' as SRC, grantee, privilege || '(' || column_name || ') ON ' || owner || '.' || table_name as granted_role, grantable as opt, cast(null as varchar2(3)) as default_role
from dba_col_privs where grantee = {OracleSql.QLit(upper)}";
            }

            await LoadToGridAsync(gridPrivs, sql);
        }
    }
}

