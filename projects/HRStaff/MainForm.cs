using project;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class MainForm : Form
    {
        private Panel contentPanel;
        private Panel sidebarPanel;

        private Button btnDashboard, btnApplicantList, btnApplicantReview,
                       btnScreening, btnInterview, btnEvaluation;

        // Exposed para ma-access ng ApplicantListForm
        public Button BtnApplicantReview => btnApplicantReview;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            SetActive(btnDashboard);
            LoadForm(new DashboardForm());
        }

        private void SetupUI()
        {
            this.Text = "HR Staff System";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.Font = new Font("Segoe UI", 9f);

            // ── Header ───────────────────────────────────────────────
            Panel header = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Color.FromArgb(15, 60, 40) };

            Label lblLogo = new Label
            {
                Text = "  HR Recruitment System",
                ForeColor = Color.FromArgb(100, 220, 150),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = false,
                Width = 260,
                Height = 54,
                TextAlign = ContentAlignment.MiddleLeft,
                Left = 16,
                BackColor = Color.Transparent
            };

            // Role badge
            Panel roleBadge = new Panel { Width = 110, Height = 26, Top = 14, Left = 290, BackColor = Color.FromArgb(25, 50, 38) };
            roleBadge.Paint += (s, e) =>
            {
                Pen p = new Pen(Color.FromArgb(50, 120, 80), 1);
                e.Graphics.DrawRectangle(p, 0, 0, roleBadge.Width - 1, roleBadge.Height - 1); p.Dispose();
            };
            Label lblRole = new Label { Text = project.Session.AdminRole, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 200, 130), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            roleBadge.Controls.Add(lblRole);

            Label lblUser = new Label
            {
                Text = "  " + project.Session.AdminFullName,
                ForeColor = Color.FromArgb(200, 220, 210),
                Font = new Font("Segoe UI", 9f),
                AutoSize = false,
                Width = 200,
                Height = 54,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Left = header.Width - 290,
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
                Left = header.Width - 95,
                Top = 12,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f)
            };
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(60, 140, 90);
            btnLogout.Click += (s, e) =>
            {
                var r = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                Audit.Log("Logged out");
                project.Session.ClearAll();
                this.Close();
            };

            header.Controls.AddRange(new Control[] { lblLogo, roleBadge, lblUser, btnLogout });
            this.Controls.Add(header);

            // ── Sidebar ──────────────────────────────────────────────
            sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(20, 26, 32) };

            // Left accent
            sidebarPanel.Controls.Add(new Panel { Left = 0, Top = 0, Width = 3, Height = 2000, BackColor = Color.FromArgb(30, 80, 55) });

            int y = 20;
            SectionLbl("MAIN", y); y += 28;
            btnDashboard = SidebarBtn("  Dashboard", y, "📊"); y += 46;
            SectionLbl("RECRUITMENT", y); y += 28;
            btnApplicantList = SidebarBtn("  Applicant List", y, "👥"); y += 46;
            btnApplicantReview = SidebarBtn("  Applicant Review", y, "🔍"); y += 46;
            btnScreening = SidebarBtn("  Screening", y, "✅"); y += 46;
            btnInterview = SidebarBtn("  Interview Schedule", y, "📅"); y += 46;
            btnEvaluation = SidebarBtn("  Evaluation", y, "📝"); y += 46;

            // Wire events
            btnDashboard.Click += (s, e) => { SetActive(btnDashboard); LoadForm(new DashboardForm()); };
            btnApplicantList.Click += (s, e) => { SetActive(btnApplicantList); LoadForm(new ApplicantListForm()); };
            btnApplicantReview.Click += (s, e) => { SetActive(btnApplicantReview); LoadForm(new ApplicantReviewForm()); };
            btnScreening.Click += (s, e) => { SetActive(btnScreening); LoadForm(new ScreeningForm()); };
            btnInterview.Click += (s, e) => { SetActive(btnInterview); LoadForm(new InterviewScheduleForm()); };
            btnEvaluation.Click += (s, e) => { SetActive(btnEvaluation); LoadForm(new EvaluationForm()); };

            // ── Content ──────────────────────────────────────────────
            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 22, 28) };

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
        }

        private void SectionLbl(string text, int y)
        {
            sidebarPanel.Controls.Add(new Label
            {
                Text = text,
                ForeColor = Color.FromArgb(60, 85, 72),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Left = 20,
                Top = y,
                Width = 180,
                Height = 20,
                BackColor = Color.Transparent
            });
        }

        private Button SidebarBtn(string text, int y, string icon)
        {
            Button btn = new Button
            {
                Text = icon + text,
                Left = 8,
                Top = y,
                Width = 204,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(160, 180, 170),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 44, 36);
            sidebarPanel.Controls.Add(btn);
            return btn;
        }

        private Button[] AllButtons => new[] { btnDashboard, btnApplicantList, btnApplicantReview, btnScreening, btnInterview, btnEvaluation };

        public void SetActive(Button btn)
        {
            foreach (var b in AllButtons)
            {
                b.BackColor = Color.Transparent;
                b.ForeColor = Color.FromArgb(160, 180, 170);
                b.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            }
            btn.BackColor = Color.FromArgb(25, 50, 38);
            btn.ForeColor = Color.FromArgb(80, 210, 130);
            btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        public void LoadForm(Form form)
        {
            contentPanel.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(form);
            form.Show();
        }

        private void MainForm_Load(object sender, EventArgs e) { }
    }
}