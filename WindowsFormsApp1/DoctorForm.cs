using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class DoctorForm : Form
    {
        private readonly string _connectionString;

        public DoctorForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            Text = "Doctor/Physician - Patient Medical Records & Treatment (TC#3)";
        }
    }
}
