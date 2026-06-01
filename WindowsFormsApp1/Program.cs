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
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK)
                    return;

                Form mainForm;
                switch (login.UserRole)
                {
                    case "Bệnh nhân":
                        mainForm = new PatientForm(login.ConnectionString);
                        break;
                    case "Kỹ thuật viên":
                        mainForm = new TechnicianForm(login.ConnectionString);
                        break;
                    case "Bác sĩ/Y sĩ":
                        mainForm = new DoctorForm(login.ConnectionString);
                        break;
                    case "Điều phối viên":
                        mainForm = new CoordinatorForm(login.ConnectionString);
                        break;
                    default:
                        mainForm = new MainForm(login.ConnectionString);
                        break;
                }

                Application.Run(mainForm);
            }
        }
    }
}
