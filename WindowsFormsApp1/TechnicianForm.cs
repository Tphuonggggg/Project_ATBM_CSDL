using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class TechnicianForm : Form
    {
        private readonly string _connectionString;

        public TechnicianForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            Text = "Technician - Diagnostic Service Results (TC#4)";
        }
    }
}
