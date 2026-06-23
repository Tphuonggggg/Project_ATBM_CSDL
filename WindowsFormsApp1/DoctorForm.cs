using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public partial class DoctorForm : Form
    {
        private readonly string _connectionString;
        private string _doctorId = "";
        private string _doctorName = "";

        private TabControl _tabs;
        private DataGridView _gridRecords;
        private DataGridView _gridPrescriptions;
        private DataGridView _gridServices;
        private TextBox _txtRecordId;
        private TextBox _txtPatientId;
        private TextBox _txtPatientName;
        private TextBox _txtRecordDate;
        private TextBox _txtDepartment;
        private RichTextBox _txtDiagnosis;
        private RichTextBox _txtTreatment;
        private RichTextBox _txtConclusion;
        private TextBox _txtRxRecordId;
        private TextBox _txtRxDate;
        private TextBox _txtMedicine;
        private TextBox _txtDosage;
        private TextBox _txtSvcRecordId;
        private TextBox _txtSvcType;
        private TextBox _txtSvcDate;
        private ComboBox _cboTechnician;
        private RichTextBox _txtSvcResult;
        private DataGridView _gridPatients;
        private TextBox _txtMedPatientId;
        private TextBox _txtMedPatientName;
        private TextBox _txtMedGender;
        private TextBox _txtMedBirthDate;
        private RichTextBox _txtMedicalHistory;
        private RichTextBox _txtFamilyHistory;
        private TextBox _txtDrugAllergy;
        private Button _btnRefreshPatients;
        private Button _btnSavePatientMedical;
        private Label _lblDoctor;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private Button _btnSaveRecord;
        private Button _btnRefreshRecords;
        private Button _btnAddRx;
        private Button _btnUpdateRx;
        private Button _btnDeleteRx;
        private Button _btnRefreshRx;
        private Button _btnAddSvc;
        private Button _btnUpdateSvc;
        private Button _btnDeleteSvc;
        private Button _btnRefreshSvc;
        private TextBox _txtEmpId;
        private TextBox _txtEmpName;
        private TextBox _txtEmpGender;
        private TextBox _txtEmpBirthDate;
        private TextBox _txtEmpIdentity;
        private TextBox _txtEmpRole;
        private TextBox _txtEmpDept;
        private TextBox _txtEmpHometown;
        private TextBox _txtEmpPhone;
        private Button _btnRefreshProfile;
        private Button _btnSaveProfile;
        private string _originalRxRecordId = "";
        private string _originalRxDate = "";
        private string _originalRxMedicine = "";
        private string _originalSvcRecordId = "";
        private string _originalSvcType = "";
        private string _originalSvcDate = "";

        public DoctorForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            BuildUi();
            Load += async (s, e) => await InitAsync();
        }

        private void BuildUi()
        {
            Controls.Clear();
            Text = "Doctor Portal - Ho so benh an va dieu tri (TC#3)";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1240, 760);
            MinimumSize = new Size(1060, 640);
            Font = new Font("Segoe UI", 9.5f);
            BackColor = Color.FromArgb(245, 247, 250);

            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(33, 64, 107) };
            header.Controls.Add(new Label
            {
                Text = "CONG THONG TIN BAC SI / Y SI - HO SO BENH AN, DON THUOC, CHI DINH DICH VU",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0)
            });
            _lblDoctor = new Label
            {
                Text = "Bac si: dang xac minh...",
                Dock = DockStyle.Right,
                Width = 360,
                ForeColor = Color.FromArgb(193, 210, 240),
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 18, 0)
            };
            header.Controls.Add(_lblDoctor);
            header.Controls.Add(SessionNavigation.CreateLogoutButton(this));

            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(230, 234, 240) };
            _lblStatus = new ToolStripStatusLabel("San sang.");
            _statusStrip.Items.Add(_lblStatus);

            _tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(10, 5), ItemSize = new Size(190, 32), SizeMode = TabSizeMode.Fixed };
            _tabs.TabPages.Add(BuildRecordTab());
            _tabs.TabPages.Add(BuildPrescriptionTab());
            _tabs.TabPages.Add(BuildServiceTab());
            _tabs.TabPages.Add(BuildPatientTab());
            _tabs.TabPages.Add(BuildProfileTab());

            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedTab != null)
                {
                    if (_tabs.SelectedTab.Text.Contains("Don thuoc"))
                    {
                        _gridPrescriptions.ClearSelection();
                        BindSelectedPrescription();
                    }
                    else if (_tabs.SelectedTab.Text.Contains("Dich vu"))
                    {
                        _gridServices.ClearSelection();
                        BindSelectedService();
                    }
                }
            };

            Controls.Add(_tabs);
            Controls.Add(header);
            Controls.Add(_statusStrip);
        }

        private TabPage BuildRecordTab()
        {
            var tab = new TabPage("1. Ho so benh an") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

            _gridRecords = CreateGrid();
            _gridRecords.SelectionChanged += (s, e) => BindSelectedRecord();
            var left = CreateGroup("Danh sach ho so benh an cua bac si dang dang nhap", _gridRecords);

            var detail = new GroupBox { Text = "Cap nhat chan doan, dieu tri va ket luan", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 12 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            _txtRecordId = ReadOnlyText();
            _txtPatientId = ReadOnlyText();
            _txtPatientName = ReadOnlyText();
            _txtRecordDate = ReadOnlyText();
            _txtDepartment = ReadOnlyText();
            _txtDiagnosis = EditorBox();
            _txtTreatment = EditorBox();
            _txtConclusion = EditorBox();

            AddRow(form, "Ma HSBA:", _txtRecordId, 0);
            AddRow(form, "Ma BN:", _txtPatientId, 1);
            AddRow(form, "Benh nhan:", _txtPatientName, 2);
            AddRow(form, "Ngay kham:", _txtRecordDate, 3);
            AddRow(form, "Khoa:", _txtDepartment, 4);
            AddWide(form, "Chan doan:", _txtDiagnosis, 5);
            AddWide(form, "Dieu tri:", _txtTreatment, 7);
            AddWide(form, "Ket luan:", _txtConclusion, 9);

            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            _btnRefreshRecords = SecondaryButton("Tai lai");
            _btnSaveRecord = PrimaryButton("Luu ho so");
            _btnRefreshRecords.Click += async (s, e) => await LoadRecordsAsync();
            _btnSaveRecord.Click += async (s, e) => await SaveRecordAsync();
            actions.Controls.Add(_btnRefreshRecords, 0, 0);
            actions.Controls.Add(_btnSaveRecord, 1, 0);
            form.Controls.Add(actions, 0, 11);
            form.SetColumnSpan(actions, 2);

            detail.Controls.Add(form);
            layout.Controls.Add(left, 0, 0);
            layout.Controls.Add(detail, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildProfileTab()
        {
            var tab = new TabPage("5. Ho so ca nhan") { Padding = new Padding(18), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

            var detail = new GroupBox { Text = "Thong tin nhan su cua bac si dang dang nhap", Dock = DockStyle.Fill, Padding = new Padding(14, 20, 14, 14), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 10 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 9; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _txtEmpId = ReadOnlyText();
            _txtEmpName = ReadOnlyText();
            _txtEmpGender = ReadOnlyText();
            _txtEmpBirthDate = ReadOnlyText();
            _txtEmpIdentity = ReadOnlyText();
            _txtEmpRole = ReadOnlyText();
            _txtEmpDept = ReadOnlyText();
            _txtEmpHometown = new TextBox { Dock = DockStyle.Fill };
            _txtEmpPhone = new TextBox { Dock = DockStyle.Fill };

            AddRow(form, "Ma nhan vien:", _txtEmpId, 0);
            AddRow(form, "Ho ten:", _txtEmpName, 1);
            AddRow(form, "Phai:", _txtEmpGender, 2);
            AddRow(form, "Ngay sinh:", _txtEmpBirthDate, 3);
            AddRow(form, "CMND/CCCD:", _txtEmpIdentity, 4);
            AddRow(form, "Vai tro:", _txtEmpRole, 5);
            AddRow(form, "Chuyen khoa:", _txtEmpDept, 6);
            AddRow(form, "Que quan:", _txtEmpHometown, 7);
            AddRow(form, "So dien thoai:", _txtEmpPhone, 8);

            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 8, 0, 0) };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            _btnRefreshProfile = SecondaryButton("Tai lai");
            _btnSaveProfile = PrimaryButton("Luu thong tin lien he");
            _btnRefreshProfile.Click += async (s, e) => await LoadProfileAsync();
            _btnSaveProfile.Click += async (s, e) => await SaveProfileAsync();
            actions.Controls.Add(_btnRefreshProfile, 0, 0);
            actions.Controls.Add(_btnSaveProfile, 1, 0);
            form.Controls.Add(actions, 0, 9);
            form.SetColumnSpan(actions, 2);
            detail.Controls.Add(form);

            var note = new GroupBox { Text = "Chinh sach VPD ap dung", Dock = DockStyle.Fill, Padding = new Padding(14, 20, 14, 14), ForeColor = Color.FromArgb(120, 50, 50), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            note.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Bac si/Y si chi xem duoc ho so ca nhan cua chinh minh qua CQ09.VW_NHANVIEN_CANHAN va chi cap nhat QUEQUAN, SODT. Cac cot dinh danh, vai tro va chuyen khoa la chi doc theo policy VPD.",
                ForeColor = Color.FromArgb(75, 80, 90),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Italic)
            });

            layout.Controls.Add(detail, 0, 0);
            layout.Controls.Add(note, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildPrescriptionTab()
        {
            var tab = new TabPage("2. Don thuoc") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            _gridPrescriptions = CreateGrid();
            _gridPrescriptions.SelectionChanged += (s, e) => BindSelectedPrescription();
            layout.Controls.Add(CreateGroup("Don thuoc thuoc cac ho so do bac si phu trach", _gridPrescriptions), 0, 0);

            var detail = new GroupBox { Text = "Them, sua, xoa thuoc", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 4; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

            _txtRxRecordId = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtRxDate = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtMedicine = new TextBox { Dock = DockStyle.Fill };
            _txtDosage = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            AddRow(form, "Ma HSBA:", _txtRxRecordId, 0);
            AddRow(form, "Ngay DT:", _txtRxDate, 1);
            AddRow(form, "Ten thuoc:", _txtMedicine, 2);
            form.Controls.Add(new Label { Text = "Lieu dung:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            form.Controls.Add(_txtDosage, 1, 3);
            form.SetRowSpan(_txtDosage, 2);

            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(0, 8, 0, 0) };
            for (var i = 0; i < 4; i++) actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            _btnRefreshRx = SecondaryButton("Tai lai");
            _btnAddRx = PrimaryButton("Them");
            _btnUpdateRx = SecondaryButton("Sua");
            _btnDeleteRx = SecondaryButton("Xoa");
            _btnRefreshRx.Click += async (s, e) => await LoadPrescriptionsAsync();
            _btnAddRx.Click += async (s, e) => await SavePrescriptionAsync(false);
            _btnUpdateRx.Click += async (s, e) => await SavePrescriptionAsync(true);
            _btnDeleteRx.Click += async (s, e) => await DeletePrescriptionAsync();
            actions.Controls.Add(_btnRefreshRx, 0, 0);
            actions.Controls.Add(_btnAddRx, 1, 0);
            actions.Controls.Add(_btnUpdateRx, 2, 0);
            actions.Controls.Add(_btnDeleteRx, 3, 0);
            form.Controls.Add(actions, 0, 5);
            form.SetColumnSpan(actions, 2);

            detail.Controls.Add(form);
            layout.Controls.Add(detail, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildServiceTab()
        {
            var tab = new TabPage("3. Dich vu chan doan") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

            _gridServices = CreateGrid();
            _gridServices.SelectionChanged += (s, e) => BindSelectedService();
            layout.Controls.Add(CreateGroup("Chi dinh dich vu va ket qua can lam sang", _gridServices), 0, 0);

            var detail = new GroupBox { Text = "Chi dinh KTV thuc hien dich vu", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

            _txtSvcRecordId = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _txtSvcType = new TextBox { Dock = DockStyle.Fill };
            _txtSvcDate = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _cboTechnician = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _txtSvcResult = EditorBox();
            _txtSvcResult.ReadOnly = true;
            AddRow(form, "Ma HSBA:", _txtSvcRecordId, 0);
            AddRow(form, "Loai DV:", _txtSvcType, 1);
            AddRow(form, "Ngay DV:", _txtSvcDate, 2);
            form.Controls.Add(new Label { Text = "KTV:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            form.Controls.Add(_cboTechnician, 1, 3);
            AddWide(form, "Ket qua (chi xem):", _txtSvcResult, 5);

            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(0, 8, 0, 0) };
            for (var i = 0; i < 4; i++) actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            _btnRefreshSvc = SecondaryButton("Tai lai");
            _btnAddSvc = PrimaryButton("Them");
            _btnUpdateSvc = SecondaryButton("Sua KTV");
            _btnDeleteSvc = SecondaryButton("Xoa");
            _btnRefreshSvc.Click += async (s, e) => await LoadServicesAsync();
            _btnAddSvc.Click += async (s, e) => await SaveServiceAsync(false);
            _btnUpdateSvc.Click += async (s, e) => await SaveServiceAsync(true);
            _btnDeleteSvc.Click += async (s, e) => await DeleteServiceAsync();
            actions.Controls.Add(_btnRefreshSvc, 0, 0);
            actions.Controls.Add(_btnAddSvc, 1, 0);
            actions.Controls.Add(_btnUpdateSvc, 2, 0);
            actions.Controls.Add(_btnDeleteSvc, 3, 0);
            form.Controls.Add(actions, 0, 7);
            form.SetColumnSpan(actions, 2);

            detail.Controls.Add(form);
            layout.Controls.Add(detail, 1, 0);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildPatientTab()
        {
            var tab = new TabPage("4. Benh nhan dieu tri") { Padding = new Padding(12), BackColor = Color.White };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

            _gridPatients = CreateGrid();
            _gridPatients.SelectionChanged += (s, e) => BindSelectedPatient();
            layout.Controls.Add(CreateGroup("Danh sach benh nhan lien quan den ho so bac si phu trach", _gridPatients), 0, 0);

            var detail = new GroupBox { Text = "Cap nhat tien su benh va di ung thuoc", Dock = DockStyle.Fill, Padding = new Padding(12, 18, 12, 12), ForeColor = Color.FromArgb(33, 64, 107), Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold) };
            var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 12 };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 4; i++) form.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            form.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            form.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            _txtMedPatientId = ReadOnlyText();
            _txtMedPatientName = ReadOnlyText();
            _txtMedGender = ReadOnlyText();
            _txtMedBirthDate = ReadOnlyText();
            _txtMedicalHistory = EditorBox();
            _txtFamilyHistory = EditorBox();
            _txtDrugAllergy = new TextBox { Dock = DockStyle.Fill };

            AddRow(form, "Ma BN:", _txtMedPatientId, 0);
            AddRow(form, "Ho ten:", _txtMedPatientName, 1);
            AddRow(form, "Phai:", _txtMedGender, 2);
            AddRow(form, "Ngay sinh:", _txtMedBirthDate, 3);
            AddWide(form, "Tien su benh:", _txtMedicalHistory, 4);
            AddWide(form, "Tien su benh gia dinh:", _txtFamilyHistory, 6);
            form.Controls.Add(new Label { Text = "Di ung thuoc:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 8);
            form.SetColumnSpan(form.GetControlFromPosition(0, 8), 2);
            form.Controls.Add(_txtDrugAllergy, 0, 9);
            form.SetColumnSpan(_txtDrugAllergy, 2);

            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 8, 0, 0) };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            _btnRefreshPatients = SecondaryButton("Tai lai");
            _btnSavePatientMedical = PrimaryButton("Luu benh su");
            _btnRefreshPatients.Click += async (s, e) => await LoadPatientsAsync();
            _btnSavePatientMedical.Click += async (s, e) => await SavePatientMedicalAsync();
            actions.Controls.Add(_btnRefreshPatients, 0, 0);
            actions.Controls.Add(_btnSavePatientMedical, 1, 0);
            form.Controls.Add(actions, 0, 11);
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
                    _doctorId = identity.Rows[0]["MA_NGUOIDUNG"]?.ToString() ?? "";
                    _doctorName = identity.Rows[0]["HOTEN"]?.ToString() ?? "";
                    _lblDoctor.Text = $"Bac si: {_doctorName} ({_doctorId})";
                }

                await LoadTechniciansAsync();
                await LoadRecordsAsync();
                await LoadPrescriptionsAsync();
                await LoadServicesAsync();
                await LoadPatientsAsync();
                await LoadProfileAsync();
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

        private async Task LoadRecordsAsync()
        {
            _lblStatus.Text = "Dang tai ho so benh an...";
            var sql = "SELECT MAHSBA, MABN, TENBN, TO_CHAR(NGAY, 'DD/MM/YYYY') AS NGAY, MAKHOA, CHANDOAN, DIEUTRI, KETLUAN FROM CQ09.VW_BACSI_HSBA ORDER BY NGAY DESC, MAHSBA";
            _gridRecords.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _lblStatus.Text = $"Da tai {_gridRecords.Rows.Count} ho so benh an.";
            BindSelectedRecord();
        }

        private async Task SaveRecordAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtRecordId.Text))
            {
                MessageBox.Show(this, "Vui long chon ho so benh an.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var sql = "UPDATE CQ09.VW_BACSI_HSBA SET " +
                          "CHANDOAN = " + OracleSql.QLit(_txtDiagnosis.Text) + ", " +
                          "DIEUTRI = " + OracleSql.QLit(_txtTreatment.Text) + ", " +
                          "KETLUAN = " + OracleSql.QLit(_txtConclusion.Text) + " " +
                          "WHERE MAHSBA = " + OracleSql.QLit(_txtRecordId.Text);
                var rows = await OracleSql.ExecuteAsync(_connectionString, sql);
                _lblStatus.Text = rows > 0 ? "Da luu ho so benh an." : "Khong co dong nao duoc cap nhat.";
                await LoadRecordsAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task LoadPrescriptionsAsync()
        {
            var sql = "SELECT MAHSBA, TO_CHAR(NGAYDT, 'DD/MM/YYYY') AS NGAYDT, TENTHUOC, LIEUDUNG FROM CQ09.VW_BACSI_DONTHUOC ORDER BY NGAYDT DESC, MAHSBA, TENTHUOC";
            _gridPrescriptions.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _gridPrescriptions.ClearSelection();
            BindSelectedPrescription();
        }

        private async Task SavePrescriptionAsync(bool update)
        {
            if (!ValidatePrescription()) return;
            SetBusy(true);
            try
            {
                var ngayDt = ToOracleDate(_txtRxDate.Text, "Ngay DT");
                if (ngayDt == null) return;

                string sql;
                if (update)
                {
                    var oldDate = ToOracleDate(string.IsNullOrWhiteSpace(_originalRxDate) ? _txtRxDate.Text : _originalRxDate, "Ngay DT", false);
                    if (oldDate == null) return;

                    sql = "UPDATE CQ09.VW_BACSI_DONTHUOC SET " +
                          "TENTHUOC = " + OracleSql.QLit(_txtMedicine.Text) + ", " +
                          "LIEUDUNG = " + OracleSql.QLit(_txtDosage.Text) + " " +
                          "WHERE MAHSBA = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalRxRecordId) ? _txtRxRecordId.Text : _originalRxRecordId) + " " +
                          "AND NGAYDT = " + oldDate + " " +
                          "AND TENTHUOC = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalRxMedicine) ? _txtMedicine.Text : _originalRxMedicine);
                }
                else
                {
                    sql = "INSERT INTO CQ09.VW_BACSI_DONTHUOC(MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG) VALUES (" +
                          OracleSql.QLit(_txtRxRecordId.Text) + ", " + ngayDt + ", " +
                          OracleSql.QLit(_txtMedicine.Text) + ", " + OracleSql.QLit(_txtDosage.Text) + ")";
                }

                await OracleSql.ExecuteAsync(_connectionString, sql);
                await LoadPrescriptionsAsync();
                _lblStatus.Text = update ? "Da cap nhat don thuoc." : "Da them thuoc.";
                _originalRxRecordId = _txtRxRecordId.Text;
                _originalRxDate = _txtRxDate.Text;
                _originalRxMedicine = _txtMedicine.Text;
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task DeletePrescriptionAsync()
        {
            if (!ValidatePrescription()) return;
            if (MessageBox.Show(this, "Xoa dong thuoc dang chon?", "Xac nhan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            SetBusy(true);
            try
            {
                var ngayDt = ToOracleDate(_txtRxDate.Text, "Ngay DT");
                if (ngayDt == null) return;

                var sql = "DELETE FROM CQ09.VW_BACSI_DONTHUOC WHERE MAHSBA = " + OracleSql.QLit(_txtRxRecordId.Text) +
                          " AND NGAYDT = " + ngayDt +
                          " AND TENTHUOC = " + OracleSql.QLit(_txtMedicine.Text);
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await LoadPrescriptionsAsync();
                _lblStatus.Text = "Da xoa dong thuoc.";
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task LoadTechniciansAsync()
        {
            var dt = await OracleSql.QueryAsync(_connectionString, "SELECT MANV, HOTEN FROM CQ09.VW_KTV_LIST ORDER BY MANV");
            dt.Columns.Add("DISPLAY", typeof(string));
            foreach (DataRow r in dt.Rows) r["DISPLAY"] = $"{r["MANV"]} - {r["HOTEN"]}";
            _cboTechnician.DisplayMember = "DISPLAY";
            _cboTechnician.ValueMember = "MANV";
            _cboTechnician.DataSource = dt;
        }

        private async Task LoadServicesAsync()
        {
            var sql = "SELECT dv.MAHSBA, dv.LOAIDV, TO_CHAR(dv.NGAYDV, 'DD/MM/YYYY') AS NGAYDV, dv.MAKTV, ktv.HOTEN AS HOTEN_KTV, dv.KETQUA " +
                      "FROM CQ09.VW_BACSI_HSBA_DV dv " +
                      "LEFT JOIN CQ09.VW_KTV_LIST ktv ON ktv.MANV = dv.MAKTV " +
                      "ORDER BY dv.NGAYDV DESC, dv.MAHSBA, dv.LOAIDV";
            _gridServices.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _gridServices.ClearSelection();
            BindSelectedService();
        }

        private async Task LoadPatientsAsync()
        {
            _lblStatus.Text = "Dang tai danh sach benh nhan cua bac si...";
            var sql = "SELECT MABN, TENBN, PHAI, TO_CHAR(NGAYSINH, 'DD/MM/YYYY') AS NGAYSINH, CCCD, " +
                      "SONHA, TENDUONG, QUANHUYEN, TINHTP, TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC " +
                      "FROM CQ09.VW_BACSI_BENHNHAN ORDER BY TENBN, MABN";
            _gridPatients.DataSource = await OracleSql.QueryAsync(_connectionString, sql);
            _lblStatus.Text = $"Da tai {_gridPatients.Rows.Count} benh nhan.";
            BindSelectedPatient();
        }

        private async Task SavePatientMedicalAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtMedPatientId.Text))
            {
                MessageBox.Show(this, "Vui long chon benh nhan.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var sql = "UPDATE CQ09.VW_BACSI_BENHNHAN SET " +
                          "TIENSUBENH = " + OracleSql.QLit(_txtMedicalHistory.Text) + ", " +
                          "TIENSUBENHGD = " + OracleSql.QLit(_txtFamilyHistory.Text) + ", " +
                          "DIUNGTHUOC = " + OracleSql.QLit(_txtDrugAllergy.Text) + " " +
                          "WHERE MABN = " + OracleSql.QLit(_txtMedPatientId.Text);
                var rows = await OracleSql.ExecuteAsync(_connectionString, sql);
                _lblStatus.Text = rows > 0 ? "Da cap nhat benh su benh nhan." : "Khong co dong nao duoc cap nhat.";
                await LoadPatientsAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task SaveServiceAsync(bool update)
        {
            if (!ValidateService()) return;
            SetBusy(true);
            try
            {
                var ktv = _cboTechnician.SelectedValue?.ToString() ?? "";
                var ngayDv = ToOracleDate(_txtSvcDate.Text, "Ngay DV");
                if (ngayDv == null) return;

                string sql;
                if (update)
                {
                    var oldDate = ToOracleDate(string.IsNullOrWhiteSpace(_originalSvcDate) ? _txtSvcDate.Text : _originalSvcDate, "Ngay DV", false);
                    if (oldDate == null) return;

                    sql = "UPDATE CQ09.VW_BACSI_HSBA_DV SET MAKTV = " + OracleSql.QLit(ktv) + " " +
                          "WHERE MAHSBA = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalSvcRecordId) ? _txtSvcRecordId.Text : _originalSvcRecordId) +
                          " AND LOAIDV = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalSvcType) ? _txtSvcType.Text : _originalSvcType) +
                          " AND NGAYDV = " + oldDate;
                }
                else
                {
                    sql = "INSERT INTO CQ09.VW_BACSI_HSBA_DV(MAHSBA, LOAIDV, NGAYDV, MAKTV) VALUES (" +
                          OracleSql.QLit(_txtSvcRecordId.Text) + ", " + OracleSql.QLit(_txtSvcType.Text) + ", " +
                          ngayDv + ", " + OracleSql.QLit(ktv) + ")";
                }
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await LoadServicesAsync();
                _lblStatus.Text = update ? "Da cap nhat KTV phu trach dich vu." : "Da them chi dinh dich vu.";
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task DeleteServiceAsync()
        {
            if (!ValidateService()) return;
            if (MessageBox.Show(this, "Xoa chi dinh dich vu dang chon?", "Xac nhan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            SetBusy(true);
            try
            {
                var ngayDv = ToOracleDate(string.IsNullOrWhiteSpace(_originalSvcDate) ? _txtSvcDate.Text : _originalSvcDate, "Ngay DV", false);
                if (ngayDv == null) return;

                var sql = "DELETE FROM CQ09.VW_BACSI_HSBA_DV WHERE MAHSBA = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalSvcRecordId) ? _txtSvcRecordId.Text : _originalSvcRecordId) +
                          " AND LOAIDV = " + OracleSql.QLit(string.IsNullOrWhiteSpace(_originalSvcType) ? _txtSvcType.Text : _originalSvcType) +
                          " AND NGAYDV = " + ngayDv;
                await OracleSql.ExecuteAsync(_connectionString, sql);
                await LoadServicesAsync();
                _lblStatus.Text = "Da xoa chi dinh dich vu.";
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private async Task LoadProfileAsync()
        {
            _lblStatus.Text = "Dang tai ho so ca nhan...";
            var sql = "SELECT MANV, HOTEN, PHAI, TO_CHAR(NGAYSINH, 'DD/MM/YYYY') AS NGAYSINH, CMND, QUEQUAN, SODT, VAITRO, CHUYENKHOA FROM CQ09.VW_NHANVIEN_CANHAN WHERE MANV = SYS_CONTEXT('USERENV', 'SESSION_USER')";
            var dt = await OracleSql.QueryAsync(_connectionString, sql);
            if (dt.Rows.Count == 0)
            {
                _lblStatus.Text = "Khong tim thay ho so ca nhan cua bac si dang dang nhap.";
                return;
            }

            var r = dt.Rows[0];
            _txtEmpId.Text = r["MANV"]?.ToString() ?? "";
            _txtEmpName.Text = r["HOTEN"]?.ToString() ?? "";
            _txtEmpGender.Text = r["PHAI"]?.ToString() ?? "";
            _txtEmpBirthDate.Text = r["NGAYSINH"]?.ToString() ?? "";
            _txtEmpIdentity.Text = r["CMND"]?.ToString() ?? "";
            _txtEmpRole.Text = r["VAITRO"]?.ToString() ?? "";
            _txtEmpDept.Text = r["CHUYENKHOA"]?.ToString() ?? "";
            _txtEmpHometown.Text = r["QUEQUAN"]?.ToString() ?? "";
            _txtEmpPhone.Text = r["SODT"]?.ToString() ?? "";
            _lblStatus.Text = "Da tai ho so ca nhan.";
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(_doctorId))
            {
                MessageBox.Show(this, "Khong tim thay ma bac si dang dang nhap.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var sql = "UPDATE CQ09.VW_NHANVIEN_CANHAN SET " +
                          "QUEQUAN = " + OracleSql.QLit(_txtEmpHometown.Text) + ", " +
                          "SODT = " + OracleSql.QLit(_txtEmpPhone.Text) + " " +
                          "WHERE MANV = " + OracleSql.QLit(_doctorId);
                var rows = await OracleSql.ExecuteAsync(_connectionString, sql);
                _lblStatus.Text = rows > 0 ? "Da cap nhat thong tin lien he." : "Khong co dong nao duoc cap nhat.";
                await LoadProfileAsync();
            }
            catch (Exception ex) { ShowError(ex); }
            finally { SetBusy(false); }
        }

        private void BindSelectedRecord()
        {
            if (_gridRecords.CurrentRow == null) return;
            var r = _gridRecords.CurrentRow;
            _txtRecordId.Text = Cell(r, "MAHSBA");
            _txtPatientId.Text = Cell(r, "MABN");
            _txtPatientName.Text = Cell(r, "TENBN");
            _txtRecordDate.Text = Cell(r, "NGAY");
            _txtDepartment.Text = Cell(r, "MAKHOA");
            _txtDiagnosis.Text = Cell(r, "CHANDOAN");
            _txtTreatment.Text = Cell(r, "DIEUTRI");
            _txtConclusion.Text = Cell(r, "KETLUAN");
            if (string.IsNullOrWhiteSpace(_txtRxRecordId.Text)) _txtRxRecordId.Text = _txtRecordId.Text;
            if (string.IsNullOrWhiteSpace(_txtSvcRecordId.Text)) _txtSvcRecordId.Text = _txtRecordId.Text;
        }

        private void BindSelectedPrescription()
        {
            if (_gridPrescriptions.CurrentRow == null || _gridPrescriptions.SelectedRows.Count == 0)
            {
                _txtRxRecordId.Text = _gridRecords.CurrentRow != null ? Cell(_gridRecords.CurrentRow, "MAHSBA") : "";
                _txtRxDate.Text = _gridRecords.CurrentRow != null ? Cell(_gridRecords.CurrentRow, "NGAY") : "";
                _txtMedicine.Text = "";
                _txtDosage.Text = "";
                _originalRxRecordId = "";
                _originalRxDate = "";
                _originalRxMedicine = "";
                return;
            }
            var r = _gridPrescriptions.SelectedRows[0];
            _txtRxRecordId.Text = Cell(r, "MAHSBA");
            _txtRxDate.Text = Cell(r, "NGAYDT");
            _txtMedicine.Text = Cell(r, "TENTHUOC");
            _txtDosage.Text = Cell(r, "LIEUDUNG");
            _originalRxRecordId = _txtRxRecordId.Text;
            _originalRxDate = _txtRxDate.Text;
            _originalRxMedicine = _txtMedicine.Text;
        }

        private void BindSelectedService()
        {
            if (_gridServices.CurrentRow == null || _gridServices.SelectedRows.Count == 0)
            {
                _txtSvcRecordId.Text = _gridRecords.CurrentRow != null ? Cell(_gridRecords.CurrentRow, "MAHSBA") : "";
                _txtSvcType.Text = "";
                _txtSvcDate.Text = _gridRecords.CurrentRow != null ? Cell(_gridRecords.CurrentRow, "NGAY") : "";
                _txtSvcResult.Text = "";
                _cboTechnician.SelectedIndex = -1;
                _originalSvcRecordId = "";
                _originalSvcType = "";
                _originalSvcDate = "";
                return;
            }
            var r = _gridServices.SelectedRows[0];
            _txtSvcRecordId.Text = Cell(r, "MAHSBA");
            _txtSvcType.Text = Cell(r, "LOAIDV");
            _txtSvcDate.Text = Cell(r, "NGAYDV");
            _txtSvcResult.Text = Cell(r, "KETQUA");
            var ktv = Cell(r, "MAKTV");
            if (!string.IsNullOrWhiteSpace(ktv)) _cboTechnician.SelectedValue = ktv;
            _originalSvcRecordId = _txtSvcRecordId.Text;
            _originalSvcType = _txtSvcType.Text;
            _originalSvcDate = _txtSvcDate.Text;
        }

        private void BindSelectedPatient()
        {
            if (_gridPatients.CurrentRow == null) return;
            var r = _gridPatients.CurrentRow;
            _txtMedPatientId.Text = Cell(r, "MABN");
            _txtMedPatientName.Text = Cell(r, "TENBN");
            _txtMedGender.Text = Cell(r, "PHAI");
            _txtMedBirthDate.Text = Cell(r, "NGAYSINH");
            _txtMedicalHistory.Text = Cell(r, "TIENSUBENH");
            _txtFamilyHistory.Text = Cell(r, "TIENSUBENHGD");
            _txtDrugAllergy.Text = Cell(r, "DIUNGTHUOC");
        }

        private bool ValidatePrescription()
        {
            if (!string.IsNullOrWhiteSpace(_txtRxRecordId.Text) && !string.IsNullOrWhiteSpace(_txtRxDate.Text) && !string.IsNullOrWhiteSpace(_txtMedicine.Text))
            {
                return ToOracleDate(_txtRxDate.Text, "Ngay DT", false) != null;
            }
            MessageBox.Show(this, "Vui long nhap Ma HSBA, Ngay DT (DD/MM/YYYY) va Ten thuoc.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private bool ValidateService()
        {
            if (!string.IsNullOrWhiteSpace(_txtSvcRecordId.Text) && !string.IsNullOrWhiteSpace(_txtSvcType.Text) && !string.IsNullOrWhiteSpace(_txtSvcDate.Text) && _cboTechnician.SelectedValue != null)
            {
                return ToOracleDate(_txtSvcDate.Text, "Ngay DV", false) != null;
            }
            MessageBox.Show(this, "Vui long nhap Ma HSBA, Loai DV, Ngay DV (DD/MM/YYYY) va KTV.", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
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
                if (fieldName == "Ngay DT") _txtRxDate.Text = normalized;
                if (fieldName == "Ngay DV") _txtSvcDate.Text = normalized;
            }

            return "TO_DATE(" + OracleSql.QLit(normalized) + ", 'DD/MM/YYYY')";
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

        private static TextBox ReadOnlyText() => new TextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(240, 242, 245), Font = new Font("Consolas", 9.5f) };
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
