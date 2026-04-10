using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        // ===== Tài khoản bypass (không cần kết nối Oracle) =====
        // Nhập đúng user/password bên dưới -> bỏ qua test kết nối và mở thẳng MainForm.
        // Lưu ý: các chức năng thao tác DB sẽ không chạy được vì không có Oracle thật.
        private const string BYPASS_USER = "demo";
        private const string BYPASS_PASSWORD = "demo";
        private const string BYPASS_CONNECTION_STRING = "User Id=demo;Password=demo;Data Source=OFFLINE_DEMO";

        public string ConnectionString { get; private set; }
        public bool IsBypassMode { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            numPort.Value = 1521;
            txtHost.Text = "localhost";
            txtService.Text = "XEPDB1";
            txtUser.Text = "sys";
            chkSysdba.Checked = true;
            UpdatePreview();
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

                cs = OracleDb.WithSysdba(cs, chkSysdba.Checked);
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
                // Bypass: nếu user + password khớp với tài khoản demo, bỏ qua test kết nối Oracle.
                if (string.Equals(txtUser.Text?.Trim(), BYPASS_USER, StringComparison.Ordinal) &&
                    string.Equals(txtPassword.Text, BYPASS_PASSWORD, StringComparison.Ordinal))
                {
                    IsBypassMode = true;
                    ConnectionString = BYPASS_CONNECTION_STRING;
                    MessageBox.Show(this,
                        "Đăng nhập bypass thành công. Ứng dụng sẽ mở ở chế độ XEM TRƯỚC (không kết nối Oracle).\n" +
                        "Các thao tác truy vấn/grant/revoke sẽ báo lỗi vì không có Oracle thật.",
                        "NHOM 09 - Bypass",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                var cs = txtPreview.Text;
                await OracleDb.TestConnectionAsync(cs);
                ConnectionString = cs;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "NHOM 09 - Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

