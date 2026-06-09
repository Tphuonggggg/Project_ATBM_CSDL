using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal static class SessionNavigation
    {
        private const string LogoutTag = "NHOM09_LOGOUT";

        public static bool IsLogoutRequested(Form form)
        {
            return string.Equals(form?.Tag as string, LogoutTag, StringComparison.Ordinal);
        }

        public static void Logout(Form form)
        {
            if (form == null) return;
            form.Tag = LogoutTag;
            form.Close();
        }

        public static Button CreateLogoutButton(Form owner)
        {
            var button = new Button
            {
                Text = "Đăng xuất",
                Dock = DockStyle.Right,
                Width = 118,
                BackColor = Color.FromArgb(185, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) => Logout(owner);
            return button;
        }
    }
}
