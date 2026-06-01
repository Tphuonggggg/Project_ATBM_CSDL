using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class CoordinatorForm : Form
    {
        private readonly string _connectionString;

        public CoordinatorForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            Text = "Coordinator - Medical Services Scheduling (TC#2)";
        }
    }
}
