using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class CoordinatorForm : Form
    {
        private readonly string _connectionString;
        private string _coordinatorId = "";
        private string _coordinatorName = "";

        private TabControl _tabs;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private Label _lblCoordinator;

        private DataGridView _gridPatients;
        private TextBox _txtPatientId;
        private TextBox _txtPatientName;
        private ComboBox _cboPatientGender;
        private TextBox _txtPatientBirthDate;
        private TextBox _txtPatientIdentity;
        private TextBox _txtPatientHouse;
        private TextBox _txtPatientStreet;
        private TextBox _txtPatientDistrict;
        private TextBox _txtPatientCity;
        private RichTextBox _txtPatientHistory;
        private RichTextBox _txtPatientFamilyHistory;
        private TextBox _txtPatientAllergy;
        private Button _btnPatientNew;
        private Button _btnPatientSave;
        private Button _btnPatientRefresh;
        private bool _isNewPatient;

        private DataGridView _gridRecords;
        private TextBox _txtRecordId;
        private ComboBox _cboRecordPatient;
        private TextBox _txtRecordDate;
        private TextBox _txtRecordDepartment;
        private ComboBox _cboRecordDoctor;
        private Button _btnRecordNew;
        private Button _btnRecordSave;
        private Button _btnRecordRefresh;
        private bool _isNewRecord;

        private DataGridView _gridServices;
        private ComboBox _cboServiceRecord;
        private TextBox _txtServiceType;
        private TextBox _txtServiceDate;
        private ComboBox _cboServiceTechnician;
        private RichTextBox _txtServiceResult;
        private Button _btnServiceNew;
        private Button _btnServiceSave;
        private Button _btnServiceRefresh;
        private bool _isNewService;
        private string _originalServiceRecord = "";
        private string _originalServiceType = "";
        private string _originalServiceDate = "";

        public CoordinatorForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            BuildUi();
            Load += async (s, e) => await InitAsync();
        }

        private void BuildUi()
        {
            Controls.Clear();
            Text = "Coordinator Portal - Dieu phoi benh nhan, bac si va ky thuat vien (TC#2)";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1260, 760);
            MinimumSize = new Size(1100, 640);
            Font = new Font("Segoe UI", 9.5f);
            BackColor = Color.FromArgb(245, 247, 250);

            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(33, 64, 107) };
            header.Controls.Add(new Label
            {
                Text = "CONG THONG TIN DIEU PHOI VIEN - TIEP NHAN, TAO HSBA, PHAN CONG NHAN SU",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0)
            });
            _lblCoordinator = new Label
            {
                Text = "Dieu phoi vien: dang xac minh...",
                Dock = DockStyle.Right,
                Width = 390,
                ForeColor = Color.FromArgb(193, 210, 240),
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 18, 0)
            };
            header.Controls.Add(_lblCoordinator);
            header.Controls.Add(SessionNavigation.CreateLogoutButton(this));

            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(230, 234, 240) };
            _lblStatus = new ToolStripStatusLabel("San sang.");
            _statusStrip.Items.Add(_lblStatus);

            _tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(10, 5), ItemSize = new Size(210, 32), SizeMode = TabSizeMode.Fixed };
            _tabs.TabPages.Add(BuildPatientTab());
            _tabs.TabPages.Add(BuildRecordTab());
            _tabs.TabPages.Add(BuildServiceTab());

            Controls.Add(_tabs);
            Controls.Add(header);
            Controls.Add(_statusStrip);
        }

        private TabPage BuildPatientTab()
        {
            var tab = new TabPage("1. Benh nhan") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));

            _gridPatients = CreateGrid();
            _gridPatients.SelectionChanged += (s, e) => BindSelectedPatient();
            layout.Controls.Add(CreateGroup("Danh sach benh nhan", _gridPatients), 0, 0);

            var detail = new GroupBox { Text = "Them / sua thong tin benh nhan", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 16 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 9; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _txtPatientId = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtPatientName = new TextBox { Dock = DockStyle.Fill };
            _cboPatientGender = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboPatientGender.Items.AddRange(new object[] { "Nam", "Nu" });
            _txtPatientBirthDate = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtPatientIdentity = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtPatientHouse = new TextBox { Dock = DockStyle.Fill };
            _txtPatientStreet = new TextBox { Dock = DockStyle.Fill };
            _txtPatientDistrict = new TextBox { Dock = DockStyle.Fill };
            _txtPatientCity = new TextBox { Dock = DockStyle.Fill };
            _txtPatientHistory = EditorBox();
            _txtPatientFamilyHistory = EditorBox();
            _txtPatientAllergy = new TextBox { Dock = DockStyle.Fill };

            AddRow(form, "Ma BN:", _txtPatientId, 0);
            AddRow(form, "Ho ten:", _txtPatientName, 1);
            AddRow(form, "Phai:", _cboPatientGender, 2);
            AddRow(form, "Ngay sinh:", _txtPatientBirthDate, 3);
            AddRow(form, "CCCD:", _txtPatientIdentity, 4);
            AddRow(form, "So nha:", _txtPatientHouse, 5);
            AddRow(form, "Ten duong:", _txtPatientStreet, 6);
            AddRow(form, "Quan/huyen:", _txtPatientDistrict, 7);
            AddRow(form, "Tinh/TP:", _txtPatientCity, 8);
            AddWide(form, "Tien su benh:", _txtPatientHistory, 9);
            AddWide(form, "Tien su benh gia dinh:", _txtPatientFamilyHistory, 11);
            AddRow(form, "Di ung thuoc:", _txtPatientAllergy, 13);

            var actions = CreateActions();
            _btnPatientRefresh = SecondaryButton("Tai lai");
            _btnPatientNew = SecondaryButton("Tao moi");
            _btnPatientSave = PrimaryButton("Luu benh nhan");
            _btnPatientRefresh.Click += async (s, e) => await LoadPatientsAsync();
            _btnPatientNew.Click += (s, e) => NewPatient();
            _btnPatientSave.Click += async (s, e) => await SavePatientAsync();
            actions.Controls.Add(_btnPatientRefresh, 0, 0);
            actions.Controls.Add(_btnPatientNew, 1, 0);
            actions.Controls.Add(_btnPatientSave, 2, 0);
            form.Controls.Add(actions, 0, 15);
            form.SetColumnSpan(actions, 2);

            detail.Controls.Add(form);
            layout.Controls.Add(detail, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildRecordTab()
        {
            var tab = new TabPage("2. Ho so benh an") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            _gridRecords = CreateGrid();
            _gridRecords.SelectionChanged += (s, e) => BindSelectedRecord();
            layout.Controls.Add(CreateGroup("Ho so benh an va bac si phu trach", _gridRecords), 0, 0);

            var detail = new GroupBox { Text = "Tao HSBA va phan cong bac si / khoa", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _txtRecordId = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _cboRecordPatient = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _txtRecordDate = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtRecordDepartment = new TextBox { Dock = DockStyle.Fill };
            _cboRecordDoctor = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

            AddRow(form, "Ma HSBA:", _txtRecordId, 0);
            AddRow(form, "Benh nhan:", _cboRecordPatient, 1);
            AddRow(form, "Ngay:", _txtRecordDate, 2);
            AddRow(form, "Ma khoa:", _txtRecordDepartment, 3);
            AddRow(form, "Bac si:", _cboRecordDoctor, 4);

            var actions = CreateActions();
            _btnRecordRefresh = SecondaryButton("Tai lai");
            _btnRecordNew = SecondaryButton("Tao moi");
            _btnRecordSave = PrimaryButton("Luu HSBA");
            _btnRecordRefresh.Click += async (s, e) => await LoadRecordsAsync();
            _btnRecordNew.Click += (s, e) => NewRecord();
            _btnRecordSave.Click += async (s, e) => await SaveRecordAsync();
            actions.Controls.Add(_btnRecordRefresh, 0, 0);
            actions.Controls.Add(_btnRecordNew, 1, 0);
            actions.Controls.Add(_btnRecordSave, 2, 0);
            form.Controls.Add(actions, 0, 6);
            form.SetColumnSpan(actions, 2);

            detail.Controls.Add(form);
            layout.Controls.Add(detail, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildServiceTab()
        {
            var tab = new TabPage("3. Phan cong KTV") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            _gridServices = CreateGrid();
            _gridServices.SelectionChanged += (s, e) => BindSelectedService();
            layout.Controls.Add(CreateGroup("Dich vu ho tro chan doan va KTV duoc dieu phoi", _gridServices), 0, 0);

            var detail = new GroupBox { Text = "Them dich vu / cap nhat ky thuat vien", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 4; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _cboServiceRecord = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _txtServiceType = new TextBox { Dock = DockStyle.Fill };
            _txtServiceDate = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _cboServiceTechnician = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _txtServiceResult = EditorBox();
            _txtServiceResult.ReadOnly = true;

            AddRow(form, "Ma HSBA:", _cboServiceRecord, 0);
            AddRow(form, "Loai DV:", _txtServiceType, 1);
            AddRow(form, "Ngay DV:", _txtServiceDate, 2);
            AddRow(form, "KTV:", _cboServiceTechnician, 3);
            AddWide(form, "Ket qua (chi xem):", _txtServiceResult, 4);

            var actions = CreateActions();
            _btnServiceRefresh = SecondaryButton("Tai lai");
            _btnServiceNew = SecondaryButton("Tao moi");
            _btnServiceSave = PrimaryButton("Luu phan cong");
            _btnServiceRefresh.Click += async (s, e) => await LoadServicesAsync();
            _btnServiceNew.Click += (s, e) => NewService();
            _btnServiceSave.Click += async (s, e) => await SaveServiceAsync();
            actions.Controls.Add(_btnServiceRefresh, 0, 0);
            actions.Controls.Add(_btnServiceNew, 1, 0);
            actions.Controls.Add(_btnServiceSave, 2, 0);
            form.Controls.Add(actions, 0, 7);
            form.SetColumnSpan(actions, 2);

            detail.Controls.Add(form);
            layout.Controls.Add(detail, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private async Task InitAsync()
        {
            SetBusy(true);
            try
            {
                var identity = await OracleSql.QueryAsync(_connectionString, "SELECT MA_NGUOIDUNG, HOTEN FROM CQ09.V_MY_ACCOUNT");
                if (identity.Rows.Count > 0)
                {
                    _coordinatorId = identity.Rows[0]["MA_NGUOIDUNG"]?.ToString() ?? "";
                    _coordinatorName = identity.Rows[0]["HOTEN"]?.ToString() ?? "";
                    _lblCoordinator.Text = $"Dieu phoi vien: {_coordinatorName} ({_coordinatorId})";
                }

                await LoadLookupDataAsync();
                await LoadPatientsAsync();
                await LoadRecordsAsync();
                await LoadServicesAsync();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task LoadLookupDataAsync()
        {
            var patients = await OracleSql.QueryAsync(_connectionString, "SELECT MABN, TENBN FROM CQ09.BENHNHAN ORDER BY MABN");
            AddDisplayColumn(patients, "MABN", "TENBN");
            BindCombo(_cboRecordPatient, patients, "MABN");

            var doctors = await OracleSql.QueryAsync(_connectionString, "SELECT MANV, HOTEN FROM CQ09.NHANVIEN WHERE VAITRO = N'Bac si/Y si' OR VAITRO = N'Bác sĩ/Y sĩ' ORDER BY MANV");
            AddDisplayColumn(doctors, "MANV", "HOTEN");
            BindCombo(_cboRecordDoctor, doctors, "MANV");

            var records = await OracleSql.QueryAsync(_connectionString, "SELECT MAHSBA, MABN FROM CQ09.HSBA ORDER BY MAHSBA");
            AddDisplayColumn(records, "MAHSBA", "MABN");
            BindCombo(_cboServiceRecord, records, "MAHSBA");

            var technicians = await OracleSql.QueryAsync(_connectionString, "SELECT MANV, HOTEN FROM CQ09.NHANVIEN WHERE VAITRO = N'Ky thuat vien' OR VAITRO = N'Kỹ thuật viên' ORDER BY MANV");
            AddDisplayColumn(technicians, "MANV", "HOTEN");
            BindCombo(_cboServiceTechnician, technicians, "MANV");
        }

        private async Task LoadPatientsAsync()
        {
            _lblStatus.Text = "Dang tai danh sach benh nhan...";
            var sql = "SELECT MABN, TENBN, PHAI, TO_CHAR(NGAYSINH, 'DD/MM/YYYY') AS NGAYSINH, CCCD, " +
                      "SONHA, TENDUONG, QUANHUYEN, TINHTP, TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC " +
                      "FROM CQ09.BENHNHAN ORDER BY MABN";
            _gridPatients.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _lblStatus.Text = $"Da tai {_gridPatients.Rows.Count} benh nhan.";
            BindSelectedPatient();
        }

        private async Task SavePatientAsync()
        {
            if (!ValidatePatient()) return;
            SetBusy(true);
            try
            {
                var birthDate = ToOracleDate(_txtPatientBirthDate.Text, "Ngay sinh");
                if (birthDate == null) return;

                string sql;
                if (_isNewPatient)
                {
                    sql = "INSERT INTO CQ09.BENHNHAN(MABN, TENBN, PHAI, NGAYSINH, CCCD, SONHA, TENDUONG, QUANHUYEN, TINHTP, TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC) VALUES (" +
                          OracleSql.QLit(_txtPatientId.Text) + ", " + OracleSql.QLit(_txtPatientName.Text) + ", " + GenderLiteral() + ", " +
                          birthDate + ", " + OracleSql.QLit(_txtPatientIdentity.Text) + ", " + OracleSql.QLit(_txtPatientHouse.Text) + ", " +
                          OracleSql.QLit(_txtPatientStreet.Text) + ", " + OracleSql.QLit(_txtPatientDistrict.Text) + ", " +
                          OracleSql.QLit(_txtPatientCity.Text) + ", " + OracleSql.QLit(_txtPatientHistory.Text) + ", " +
                          OracleSql.QLit(_txtPatientFamilyHistory.Text) + ", " + OracleSql.QLit(_txtPatientAllergy.Text) + ")";
                }
                else
                {
                    sql = "UPDATE CQ09.BENHNHAN SET " +
                          "TENBN = " + OracleSql.QLit(_txtPatientName.Text) + ", " +
                          "PHAI = " + GenderLiteral() + ", " +
                          "NGAYSINH = " + birthDate + ", " +
                          "CCCD = " + OracleSql.QLit(_txtPatientIdentity.Text) + ", " +
                          "SONHA = " + OracleSql.QLit(_txtPatientHouse.Text) + ", " +
                          "TENDUONG = " + OracleSql.QLit(_txtPatientStreet.Text) + ", " +
                          "QUANHUYEN = " + OracleSql.QLit(_txtPatientDistrict.Text) + ", " +
                          "TINHTP = " + OracleSql.QLit(_txtPatientCity.Text) + ", " +
                          "TIENSUBENH = " + OracleSql.QLit(_txtPatientHistory.Text) + ", " +
                          "TIENSUBENHGD = " + OracleSql.QLit(_txtPatientFamilyHistory.Text) + ", " +
                          "DIUNGTHUOC = " + OracleSql.QLit(_txtPatientAllergy.Text) + " " +
                          "WHERE MABN = " + OracleSql.QLit(_txtPatientId.Text);
                }

                await OracleSql.ExecuteAsync(_connectionString, sql);
                _isNewPatient = false;
                await LoadPatientsAsync();
                await LoadLookupDataAsync();
                _lblStatus.Text = "Da luu thong tin benh nhan.";
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task LoadRecordsAsync()
        {
            _lblStatus.Text = "Dang tai ho so benh an...";
            var sql = "SELECT h.MAHSBA, h.MABN, b.TENBN, TO_CHAR(h.NGAY, 'DD/MM/YYYY') AS NGAY, h.MAKHOA, h.MABS, nv.HOTEN AS TENBS " +
                      "FROM CQ09.HSBA h JOIN CQ09.BENHNHAN b ON b.MABN = h.MABN " +
                      "LEFT JOIN CQ09.NHANVIEN nv ON nv.MANV = h.MABS ORDER BY h.NGAY DESC, h.MAHSBA";
            _gridRecords.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _lblStatus.Text = $"Da tai {_gridRecords.Rows.Count} ho so.";
            BindSelectedRecord();
        }

        private async Task SaveRecordAsync()
        {
            if (!ValidateRecord()) return;
            SetBusy(true);
            try
            {
                var recordDate = ToOracleDate(_txtRecordDate.Text, "Ngay HSBA");
                if (recordDate == null) return;

                var patient = _cboRecordPatient.SelectedValue?.ToString() ?? "";
                var doctor = _cboRecordDoctor.SelectedValue?.ToString() ?? "";
                string sql;
                if (_isNewRecord)
                {
                    sql = "INSERT INTO CQ09.HSBA(MAHSBA, MABN, NGAY, CHANDOAN, DIEUTRI, MABS, MAKHOA, KETLUAN) VALUES (" +
                          OracleSql.QLit(_txtRecordId.Text) + ", " + OracleSql.QLit(patient) + ", " + recordDate +
                          ", NULL, NULL, " + OracleSql.QLit(doctor) + ", " + OracleSql.QLit(_txtRecordDepartment.Text) + ", NULL)";
                }
                else
                {
                    sql = "UPDATE CQ09.HSBA SET " +
                          "MABN = " + OracleSql.QLit(patient) + ", " +
                          "NGAY = " + recordDate + ", " +
                          "MAKHOA = " + OracleSql.QLit(_txtRecordDepartment.Text) + ", " +
                          "MABS = " + OracleSql.QLit(doctor) + " " +
                          "WHERE MAHSBA = " + OracleSql.QLit(_txtRecordId.Text);
                }

                await OracleSql.ExecuteAsync(_connectionString, sql);
                _isNewRecord = false;
                await LoadRecordsAsync();
                await LoadLookupDataAsync();
                _lblStatus.Text = "Da luu ho so benh an va phan cong bac si.";
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task LoadServicesAsync()
        {
            _lblStatus.Text = "Dang tai dich vu ho tro chan doan...";
            var sql = "SELECT dv.MAHSBA, h.MABN, b.TENBN, dv.LOAIDV, TO_CHAR(dv.NGAYDV, 'DD/MM/YYYY') AS NGAYDV, " +
                      "dv.MAKTV, nv.HOTEN AS TENKTV, dv.KETQUA " +
                      "FROM CQ09.HSBA_DV dv JOIN CQ09.HSBA h ON h.MAHSBA = dv.MAHSBA " +
                      "JOIN CQ09.BENHNHAN b ON b.MABN = h.MABN " +
                      "LEFT JOIN CQ09.NHANVIEN nv ON nv.MANV = dv.MAKTV " +
                      "ORDER BY dv.NGAYDV DESC, dv.MAHSBA, dv.LOAIDV";
            _gridServices.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _lblStatus.Text = $"Da tai {_gridServices.Rows.Count} dich vu.";
            BindSelectedService();
        }

        private async Task SaveServiceAsync()
        {
            if (!ValidateService()) return;
            SetBusy(true);
            try
            {
                var serviceDate = ToOracleDate(_txtServiceDate.Text, "Ngay DV");
                if (serviceDate == null) return;

                var record = _cboServiceRecord.SelectedValue?.ToString() ?? "";
                var technician = _cboServiceTechnician.SelectedValue?.ToString() ?? "";
                string sql;
                if (_isNewService)
                {
                    sql = "INSERT INTO CQ09.HSBA_DV(MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA) VALUES (" +
                          OracleSql.QLit(record) + ", " + OracleSql.QLit(_txtServiceType.Text) + ", " +
                          serviceDate + ", " + OracleSql.QLit(technician) + ", NULL)";
                }
                else
                {
                    var oldDate = ToOracleDate(string.IsNullOrWhiteSpace(_originalServiceDate) ? _txtServiceDate.Text : _originalServiceDate, "Ngay DV", false);
                    if (oldDate == null) return;

                    sql = "UPDATE CQ09.HSBA_DV SET " +
                          "MAKTV = " + OracleSql.QLit(technician) + " " +
                          "WHERE MAHSBA = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalServiceRecord) ? record : _originalServiceRecord) +
                          " AND LOAIDV = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalServiceType) ? _txtServiceType.Text : _originalServiceType) +
                          " AND NGAYDV = " + oldDate;
                }

                await OracleSql.ExecuteAsync(_connectionString, sql);
                _isNewService = false;
                await LoadServicesAsync();
                _lblStatus.Text = "Da luu phan cong ky thuat vien.";
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private void NewPatient()
        {
            _isNewPatient = true;
            _txtPatientId.ReadOnly = false;
            _txtPatientId.Text = "";
            _txtPatientName.Text = "";
            _cboPatientGender.SelectedIndex = 0;
            _txtPatientBirthDate.Text = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            _txtPatientIdentity.Text = "";
            _txtPatientHouse.Text = "";
            _txtPatientStreet.Text = "";
            _txtPatientDistrict.Text = "";
            _txtPatientCity.Text = "";
            _txtPatientHistory.Text = "";
            _txtPatientFamilyHistory.Text = "";
            _txtPatientAllergy.Text = "";
            _txtPatientId.Focus();
            _lblStatus.Text = "Dang tao benh nhan moi.";
        }

        private void NewRecord()
        {
            _isNewRecord = true;
            _txtRecordId.ReadOnly = false;
            _txtRecordId.Text = "";
            _txtRecordDate.Text = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            _txtRecordDepartment.Text = "";
            if (_cboRecordPatient.Items.Count > 0) _cboRecordPatient.SelectedIndex = 0;
            if (_cboRecordDoctor.Items.Count > 0) _cboRecordDoctor.SelectedIndex = 0;
            _txtRecordId.Focus();
            _lblStatus.Text = "Dang tao ho so benh an moi.";
        }

        private void NewService()
        {
            _isNewService = true;
            _txtServiceType.ReadOnly = false;
            if (_cboServiceRecord.Items.Count > 0) _cboServiceRecord.SelectedIndex = 0;
            _txtServiceType.Text = "";
            _txtServiceDate.Text = DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (_cboServiceTechnician.Items.Count > 0) _cboServiceTechnician.SelectedIndex = 0;
            _txtServiceResult.Text = "";
            _originalServiceRecord = "";
            _originalServiceType = "";
            _originalServiceDate = "";
            _txtServiceType.Focus();
            _lblStatus.Text = "Dang tao chi dinh dich vu moi.";
        }

        private void BindSelectedPatient()
        {
            if (_gridPatients.CurrentRow == null || _isNewPatient) return;
            var r = _gridPatients.CurrentRow;
            _txtPatientId.ReadOnly = true;
            _txtPatientId.Text = Cell(r, "MABN");
            _txtPatientName.Text = Cell(r, "TENBN");
            SelectComboText(_cboPatientGender, NormalizeGender(Cell(r, "PHAI")));
            _txtPatientBirthDate.Text = Cell(r, "NGAYSINH");
            _txtPatientIdentity.Text = Cell(r, "CCCD");
            _txtPatientHouse.Text = Cell(r, "SONHA");
            _txtPatientStreet.Text = Cell(r, "TENDUONG");
            _txtPatientDistrict.Text = Cell(r, "QUANHUYEN");
            _txtPatientCity.Text = Cell(r, "TINHTP");
            _txtPatientHistory.Text = Cell(r, "TIENSUBENH");
            _txtPatientFamilyHistory.Text = Cell(r, "TIENSUBENHGD");
            _txtPatientAllergy.Text = Cell(r, "DIUNGTHUOC");
        }

        private void BindSelectedRecord()
        {
            if (_gridRecords.CurrentRow == null || _isNewRecord) return;
            var r = _gridRecords.CurrentRow;
            _txtRecordId.ReadOnly = true;
            _txtRecordId.Text = Cell(r, "MAHSBA");
            _cboRecordPatient.SelectedValue = Cell(r, "MABN");
            _txtRecordDate.Text = Cell(r, "NGAY");
            _txtRecordDepartment.Text = Cell(r, "MAKHOA");
            _cboRecordDoctor.SelectedValue = Cell(r, "MABS");
        }

        private void BindSelectedService()
        {
            if (_gridServices.CurrentRow == null || _isNewService) return;
            var r = _gridServices.CurrentRow;
            _txtServiceType.ReadOnly = true;
            _cboServiceRecord.SelectedValue = Cell(r, "MAHSBA");
            _txtServiceType.Text = Cell(r, "LOAIDV");
            _txtServiceDate.Text = Cell(r, "NGAYDV");
            _cboServiceTechnician.SelectedValue = Cell(r, "MAKTV");
            _txtServiceResult.Text = Cell(r, "KETQUA");
            _originalServiceRecord = Cell(r, "MAHSBA");
            _originalServiceType = Cell(r, "LOAIDV");
            _originalServiceDate = Cell(r, "NGAYDV");
        }

        private bool ValidatePatient()
        {
            if (string.IsNullOrWhiteSpace(_txtPatientId.Text) || string.IsNullOrWhiteSpace(_txtPatientName.Text))
            {
                MessageBox.Show(this, "Vui long nhap Ma BN va Ho ten.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return ToOracleDate(_txtPatientBirthDate.Text, "Ngay sinh", false) != null;
        }

        private bool ValidateRecord()
        {
            if (string.IsNullOrWhiteSpace(_txtRecordId.Text) || _cboRecordPatient.SelectedValue == null || _cboRecordDoctor.SelectedValue == null)
            {
                MessageBox.Show(this, "Vui long nhap Ma HSBA, Benh nhan va Bac si.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return ToOracleDate(_txtRecordDate.Text, "Ngay HSBA", false) != null;
        }

        private bool ValidateService()
        {
            if (_cboServiceRecord.SelectedValue == null || string.IsNullOrWhiteSpace(_txtServiceType.Text) || _cboServiceTechnician.SelectedValue == null)
            {
                MessageBox.Show(this, "Vui long chon Ma HSBA, nhap Loai DV va chon KTV.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return ToOracleDate(_txtServiceDate.Text, "Ngay DV", false) != null;
        }

        private string ToOracleDate(string value, string fieldName, bool normalize = true)
        {
            if (!DateTime.TryParseExact(value?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                MessageBox.Show(this, $"{fieldName} phai co dinh dang DD/MM/YYYY.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            var normalized = date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (normalize)
            {
                if (fieldName == "Ngay sinh") _txtPatientBirthDate.Text = normalized;
                if (fieldName == "Ngay HSBA") _txtRecordDate.Text = normalized;
                if (fieldName == "Ngay DV") _txtServiceDate.Text = normalized;
            }

            return "TO_DATE(" + OracleSql.QLit(normalized) + ", 'DD/MM/YYYY')";
        }

        private string GenderLiteral()
        {
            var value = NormalizeGender(_cboPatientGender.Text);
            return "N" + OracleSql.QLit(value == "Nu" ? "Nữ" : "Nam");
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            foreach (Control c in Controls) c.Enabled = !busy;
            _statusStrip.Enabled = true;
        }

        private static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 238, 243);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return grid;
        }

        private static GroupBox CreateGroup(string text, Control child)
        {
            var group = new GroupBox { Text = text, Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 10), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            group.Controls.Add(child);
            return group;
        }

        private static TableLayoutPanel CreateActions()
        {
            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(0, 8, 0, 0) };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            return actions;
        }

        private static RichTextBox EditorBox() => new RichTextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f) };
        private static Button PrimaryButton(string text) => new Button { Text = text, Dock = DockStyle.Fill, BackColor = Color.FromArgb(33, 64, 107), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold) };
        private static Button SecondaryButton(string text) => new Button { Text = text, Dock = DockStyle.Fill, BackColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f) };

        private static void AddRow(TableLayoutPanel form, string label, Control control, int row)
        {
            form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            form.Controls.Add(control, 1, row);
        }

        private static void AddWide(TableLayoutPanel form, string label, Control control, int labelRow)
        {
            form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold) }, 0, labelRow);
            form.SetColumnSpan(form.GetControlFromPosition(0, labelRow), 2);
            form.Controls.Add(control, 0, labelRow + 1);
            form.SetColumnSpan(control, 2);
        }

        private static void AddDisplayColumn(DataTable dt, string keyColumn, string nameColumn)
        {
            if (!dt.Columns.Contains("DISPLAY")) dt.Columns.Add("DISPLAY", typeof(string));
            foreach (DataRow r in dt.Rows)
            {
                r["DISPLAY"] = $"{r[keyColumn]} - {r[nameColumn]}";
            }
        }

        private static void BindCombo(ComboBox combo, DataTable dt, string valueMember)
        {
            combo.DisplayMember = "DISPLAY";
            combo.ValueMember = valueMember;
            combo.DataSource = dt;
        }

        private static void SelectComboText(ComboBox combo, string text)
        {
            for (var i = 0; i < combo.Items.Count; i++)
            {
                if (string.Equals(combo.Items[i]?.ToString(), text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private static string NormalizeGender(string value)
        {
            var s = (value ?? "").Trim();
            return s == "Nữ" || s.Equals("Nu", StringComparison.OrdinalIgnoreCase) ? "Nu" : "Nam";
        }

        private static string Cell(DataGridViewRow row, string columnName)
        {
            return row.DataGridView.Columns.Contains(columnName) ? row.Cells[columnName].Value?.ToString() ?? "" : "";
        }

        private void ShowError(Exception ex)
        {
            _lblStatus.Text = "Loi Oracle.";
            MessageBox.Show(this, ex.Message, "NHOM 09 - Loi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
