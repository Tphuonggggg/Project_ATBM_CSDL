using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private const string BYPASS_USER = "demo";
        private const string BYPASS_PASSWORD = "demo";
        private const string BYPASS_CONNECTION_STRING = "User Id=demo;Password=demo;Data Source=OFFLINE_DEMO";
        private const string ROLE_QUERY = "SELECT LOAI_NGUOIDUNG FROM CQ09.V_MY_ACCOUNT";

        public string ConnectionString { get; private set; }
        public bool IsBypassMode { get; private set; }
        public string UserRole { get; private set; }
        public string Username => txtUser.Text?.Trim();

        public LoginForm()
        {
            InitializeComponent();
            numPort.Value = 1521;
            txtHost.Text = "localhost";
            txtService.Text = "XEPDB1";
            txtUser.Text = "CQ09";
            txtPassword.Text = "ATBM123";
            chkSysdba.Checked = false;
            UpdatePreview();
        }

        private static bool IsSysdbaUser(string username)
        {
            var userLower = (username ?? string.Empty).Trim().ToLowerInvariant();
            return userLower == "sys" || userLower == "system";
        }

        private static bool IsAdminUser(string username)
        {
            var userLower = (username ?? string.Empty).Trim().ToLowerInvariant();
            return IsSysdbaUser(userLower) || userLower == "cq09";
        }

        private async Task<string> GetSessionUserAsync(string connectionString)
        {
            try
            {
                var dt = await OracleHelper.QueryAsync(
                    connectionString,
                    "SELECT SYS_CONTEXT('USERENV', 'SESSION_USER') AS SESSION_USER FROM DUAL");

                if (dt != null && dt.Rows.Count > 0)
                    return dt.Rows[0]["SESSION_USER"]?.ToString() ?? "(null)";
            }
            catch (Exception ex)
            {
                return "(khong doc duoc SESSION_USER: " + ex.Message + ")";
            }

            return "(khong co du lieu)";
        }

        private void ShowRoleLookupError(string username, string sessionUser, string detail)
        {
            MessageBox.Show(this,
                $"Khong xac dinh duoc vai tro nghiep vu cho tai khoan '{username}'.\n" +
                "Ung dung se giu lai man hinh dang nhap thay vi chuyen sang giao dien DBA.\n\n" +
                "Debug:\n" +
                $"- User nhap: {username}\n" +
                $"- Oracle SESSION_USER: {sessionUser}\n" +
                $"- Query: {ROLE_QUERY}\n" +
                $"- Chi tiet: {detail}\n\n" +
                "Hay kiem tra: da bo chon SYSDBA, dang ket noi dung PDB XEPDB1, " +
                "da chay 02_schema_data.sql va 03_role.sql bang dung schema CQ09.",
                "NHOM 09 - Loi nhan dien vai tro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void UpdatePreview()
        {
            try
            {
                var cs = OracleDb.BuildConnectionString(
                    host: txtHost.Text,
                    port: (int)numPort.Value,
                    serviceName: txtService.Text,
                    userId: txtUser.Text,
                    password: txtPassword.Text,
                    asSysdba: false);

                cs = OracleDb.WithSysdba(cs, chkSysdba.Checked && IsSysdbaUser(txtUser.Text));
                txtPreview.Text = cs;
            }
            catch
            {
                txtPreview.Text = string.Empty;
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            try
            {
                var username = Username ?? string.Empty;
                var userLower = username.ToLowerInvariant();

                if (string.Equals(username, BYPASS_USER, StringComparison.Ordinal) &&
                    string.Equals(txtPassword.Text, BYPASS_PASSWORD, StringComparison.Ordinal))
                {
                    IsBypassMode = true;
                    ConnectionString = BYPASS_CONNECTION_STRING;
                    UserRole = "DBA";
                    MessageBox.Show(this,
                        "Dang nhap bypass thanh cong. Ung dung se mo o che do xem truoc, khong ket noi Oracle.\n" +
                        "Cac thao tac truy van/grant/revoke se bao loi vi khong co Oracle that.",
                        "NHOM 09 - Bypass",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                if (chkSysdba.Checked && !IsSysdbaUser(username))
                {
                    chkSysdba.Checked = false;
                    UpdatePreview();
                    MessageBox.Show(this,
                        "SYSDBA chi duoc dung cho tai khoan SYS hoac SYSTEM.\n" +
                        $"Tai khoan '{username}' la user nghiep vu/du an nen da bo chon SYSDBA.\n\n" +
                        "Bam Dang nhap lai de ket noi bang dung SESSION_USER.",
                        "NHOM 09 - Cau hinh dang nhap",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var cs = txtPreview.Text;
                await OracleDb.TestConnectionAsync(cs);
                ConnectionString = cs;
                var sessionUser = await GetSessionUserAsync(ConnectionString);

                try
                {
                    if (IsAdminUser(username))
                    {
                        UserRole = "DBA";
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(userLower, "^u[1-8]$"))
                    {
                        UserRole = "OLS_DEMO";
                    }
                    else
                    {
                        var dt = await OracleHelper.QueryAsync(ConnectionString, ROLE_QUERY);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            UserRole = dt.Rows[0]["LOAI_NGUOIDUNG"]?.ToString();
                            if (string.IsNullOrWhiteSpace(UserRole))
                            {
                                ShowRoleLookupError(username, sessionUser, "Cot LOAI_NGUOIDUNG bi rong.");
                                return;
                            }
                        }
                        else
                        {
                            ShowRoleLookupError(
                                username,
                                sessionUser,
                                "CQ09.V_MY_ACCOUNT khong tra ve dong nao cho SESSION_USER hien tai.");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (IsAdminUser(username))
                    {
                        UserRole = "DBA";
                    }
                    else
                    {
                        ShowRoleLookupError(username, sessionUser, ex.Message);
                        return;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "NHOM 09 - Loi ket noi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtHost_TextChanged(object sender, EventArgs e) => UpdatePreview();
        private void numPort_ValueChanged(object sender, EventArgs e) => UpdatePreview();
        private void txtService_TextChanged(object sender, EventArgs e) => UpdatePreview();
        private void txtUser_TextChanged(object sender, EventArgs e) => UpdatePreview();
        private void txtPassword_TextChanged(object sender, EventArgs e) => UpdatePreview();
        private void chkSysdba_CheckedChanged(object sender, EventArgs e) => UpdatePreview();
    }
}
