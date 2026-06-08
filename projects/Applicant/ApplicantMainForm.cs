using System;
using System.Drawing;
using System.Windows.Forms;
using project;

namespace HRApplicant
{
    public partial class ApplicantMainForm : Form
    {
        public Panel contentPanel;
        private Panel sidebarPanel;
        private Panel headerPanel;
        public string applicationStatus = "Draft";

        private Button btnDashboard, btnProfile, btnJobVacancies,
                       btnMyApplication, btnMyDocuments, btnStatusTracking;

        public ApplicantMainForm()
        {
            InitializeComponent();
            SetupForm();
            BuildHeader();
            BuildSidebar();
            BuildContentArea();
            ShowDashboard();
        }

        private void SetupForm()
        {
            this.WindowState = FormWindowState.Maximized;
            this.Text = "HR Applicant Portal";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(900, 600);
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);
        }

        private void BuildHeader()
        {
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Color.FromArgb(15, 60, 40) };

            Label lblLogo = new Label
            {
                Text = "  HR Applicant Portal",
                ForeColor = Color.FromArgb(100, 220, 150),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = false,
                Width = 240,
                Height = 54,
                TextAlign = ContentAlignment.MiddleLeft,
                Left = 16,
                BackColor = Color.Transparent
            };

            Label lblUser = new Label
            {
                Text = "  " + project.Session.FullName,
                ForeColor = Color.FromArgb(200, 220, 210),
                Font = new Font("Segoe UI", 9f),
                AutoSize = false,
                Width = 180,
                Height = 54,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Left = headerPanel.Width - 270,
                BackColor = Color.Transparent
            };

            Button btnLogout = new Button
            {
                Text = "Logout",
                BackColor = Color.FromArgb(35, 90, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 75,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Left = headerPanel.Width - 95,
                Top = 12,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f)
            };
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(60, 140, 90);
            btnLogout.Click += (s, e) =>
            {
                Session.Clear();
                this.Close();
            };

            headerPanel.Controls.AddRange(new Control[] { lblLogo, lblUser, btnLogout });
            this.Controls.Add(headerPanel);
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 210, BackColor = Color.FromArgb(20, 26, 32) };

            int y = 18;
            Label lblMain = SectionLabel("OVERVIEW", y); y += 28;
            btnDashboard = SidebarBtn("   Dashboard", y, true); y += 46;
            Label lblProf = SectionLabel("MY PROFILE", y); y += 28;
            btnProfile = SidebarBtn("   My Profile", y, false); y += 46;
            Label lblJobs = SectionLabel("JOBS", y); y += 28;
            btnJobVacancies = SidebarBtn("   Job Vacancies", y, false); y += 46;
            Label lblApp = SectionLabel("APPLICATION", y); y += 28;
            btnMyApplication = SidebarBtn("   My Application", y, false); y += 46;
            btnMyDocuments = SidebarBtn("   My Documents", y, false); y += 46;
            btnStatusTracking = SidebarBtn("   Status Tracking", y, false);

            sidebarPanel.Controls.AddRange(new Control[] {
                lblMain, btnDashboard, lblProf, btnProfile,
                lblJobs, btnJobVacancies, lblApp, btnMyApplication, btnMyDocuments, btnStatusTracking
            });

            btnDashboard.Click += (s, e) => { SetActive(btnDashboard); ShowDashboard(); };
            btnProfile.Click += (s, e) => { SetActive(btnProfile); ShowProfile(); };
            btnJobVacancies.Click += (s, e) => { SetActive(btnJobVacancies); ShowJobVacancies(); };
            btnMyApplication.Click += (s, e) => { SetActive(btnMyApplication); ShowMyApplication(); };
            btnMyDocuments.Click += (s, e) => { SetActive(btnMyDocuments); ShowMyDocuments(); };
            btnStatusTracking.Click += (s, e) => { SetActive(btnStatusTracking); ShowStatusTracking(); };

            this.Controls.Add(sidebarPanel);
        }

        private Label SectionLabel(string text, int y)
        {
            return new Label { Text = text, ForeColor = Color.FromArgb(70, 90, 80), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), Left = 16, Top = y, Width = 180, Height = 20, BackColor = Color.Transparent };
        }

        private Button SidebarBtn(string text, int y, bool active)
        {
            Button btn = new Button
            {
                Text = text,
                Left = 8,
                Top = y,
                Width = 194,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                ForeColor = active ? Color.FromArgb(80, 210, 130) : Color.FromArgb(170, 185, 178),
                BackColor = active ? Color.FromArgb(25, 50, 38) : Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, active ? FontStyle.Bold : FontStyle.Regular)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 44, 36);
            return btn;
        }

        private void BuildContentArea()
        {
            contentPanel = new Panel
            {
                Left = 210,
                Top = 54,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(18, 22, 28),
                AutoScroll = true
            };
            contentPanel.Width = this.ClientSize.Width - 210;
            contentPanel.Height = this.ClientSize.Height - 54;
            this.Controls.Add(contentPanel);
            this.Resize += (s, e) => { contentPanel.Width = this.ClientSize.Width - 210; contentPanel.Height = this.ClientSize.Height - 54; };
        }

        public void ClearContent() => contentPanel.Controls.Clear();

        private Button[] AllButtons => new[] { btnDashboard, btnProfile, btnJobVacancies, btnMyApplication, btnMyDocuments, btnStatusTracking };

        private void SetActive(Button active)
        {
            foreach (var btn in AllButtons)
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(170, 185, 178);
                btn.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            }
            active.BackColor = Color.FromArgb(25, 50, 38);
            active.ForeColor = Color.FromArgb(80, 210, 130);
            active.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        private void ShowDashboard() => new ApplicantDashboardPage(this);
        private void ShowProfile() => new ApplicantProfilePage(this);
        private void ShowJobVacancies() => new ApplicantJobVacanciesPage(this);
        private void ShowMyApplication() => new ApplicantMyApplicationPage(this);
        private void ShowMyDocuments() => new ApplicantMyDocumentsPage(this);
        private void ShowStatusTracking() => new ApplicantStatusTrackingPage(this);

        private void ApplicantMainForm_Load(object sender, EventArgs e)
        {

        }
    }
}