using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class PatientForm : Form
    {
        private readonly string _connectionString;

        public PatientForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            Text = "Patient - Personal Information Management (TC#5)";
        }
    }
}
