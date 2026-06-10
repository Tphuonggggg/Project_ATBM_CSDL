using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var showLoginAgain = true;
            while (showLoginAgain)
            {
                showLoginAgain = false;
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                        return;

                    using (var mainForm = CreateMainForm(login))
                    {
                        Application.Run(mainForm);
                        showLoginAgain = SessionNavigation.IsLogoutRequested(mainForm);
                    }
                }
            }
        }

        private static Form CreateMainForm(LoginForm login)
        {
            switch (login.UserRole)
            {
                case "Bệnh nhân":
                    return new PatientForm(login.ConnectionString);
                case "Kỹ thuật viên":
                    return new TechnicianForm(login.ConnectionString);
                case "Bác sĩ/Y sĩ":
                    return new DoctorForm(login.ConnectionString);
                case "Điều phối viên":
                    return new CoordinatorForm(login.ConnectionString);
                case "OLS_DEMO":
                    return new OlsDemoForm(login.ConnectionString, login.Username);
                default:
                    return new MainForm(login.ConnectionString);
            }
        }
    }
}
