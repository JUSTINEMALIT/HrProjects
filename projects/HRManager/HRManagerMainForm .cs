using project;
using System;
using System.Drawing;
using System.Windows.Forms;



//hr manager/admin mainform

namespace projects.HRManager
{
    public partial class HRManagerMainForm : Form
    {
        // Color Scheme
        private static readonly Color BgSidebar = Color.FromArgb(30, 41, 59);
        private static readonly Color UserBoxBg = Color.FromArgb(51, 65, 85);
        private static readonly Color BgContent = Color.FromArgb(241, 245, 249);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color HoverColor = Color.FromArgb(51, 65, 85);

        public Panel contentPanel;
        private Panel sidebarPanel;

        public HRManagerMainForm()
        {
            InitializeComponent();
            this.Text = "HR Manager - Recruitment Management System";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            SetupUI();
            new HRManagerDashboardPage(this);
        }

        private void SetupUI()
        {
            // ── SIDEBAR ─────────────────────────────────────────────
            sidebarPanel = new Panel { Left = 0, Top = 0, Width = 250, Height = this.Height, BackColor = BgSidebar, Dock = DockStyle.Left };
            sidebarPanel.AutoScroll = true;
            this.Controls.Add(sidebarPanel);

            int sidebarY = 16;

            // Brand
            sidebarPanel.Controls.Add(new Label { Text = "HR Recruitment", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.White, Left = 16, Top = sidebarY, AutoSize = true, BackColor = Color.Transparent });
            sidebarY += 20;
            sidebarPanel.Controls.Add(new Label { Text = "Management System", Font = new Font("Segoe UI", 8f), ForeColor = TextMuted, Left = 16, Top = sidebarY, AutoSize = true, BackColor = Color.Transparent });
            sidebarY += 20;

            // User Box
            Panel userBox = new Panel { Left = 12, Top = sidebarY, Width = 226, Height = 60, BackColor = UserBoxBg };
            userBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(AccentBlue, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, userBox.Width - 1, userBox.Height - 1);
            };

            string initials = AdminSession.AdminFullName.Split(' ')[0][0].ToString() +
                             (AdminSession.AdminFullName.Contains(" ") ? AdminSession.AdminFullName.Split(' ')[1][0].ToString() : "");
            Panel avatar = new Panel { Left = 8, Top = 6, Width = 48, Height = 48, BackColor = AccentBlue };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.Clear(AccentBlue);
                using (var f = new Font("Segoe UI Semibold", 11f))
                using (var br = new SolidBrush(Color.White))
                {
                    var sz = e.Graphics.MeasureString(initials, f);
                    e.Graphics.DrawString(initials, f, br, (avatar.Width - sz.Width) / 2, (avatar.Height - sz.Height) / 2);
                }
            };
            userBox.Controls.Add(avatar);

            userBox.Controls.Add(new Label { Text = AdminSession.AdminFullName, Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold), ForeColor = Color.White, Left = 62, Top = 8, Width = 150, AutoSize = false, BackColor = Color.Transparent });
            userBox.Controls.Add(new Label { Text = AdminSession.AdminRole, Font = new Font("Segoe UI", 8f), ForeColor = TextMuted, Left = 62, Top = 28, AutoSize = true, BackColor = Color.Transparent });
            userBox.Controls.Add(new Label { Text = "Account", Font = new Font("Segoe UI", 7f), ForeColor = TextMuted, Left = 62, Top = 44, AutoSize = true, BackColor = Color.Transparent });

            sidebarPanel.Controls.Add(userBox);
            sidebarY += 72;

            // ── NAVIGATION MENU (ROLE-BASED) ────────────────────────
            bool isAdmin = AdminSession.AdminRole == "Admin";

            sidebarY += 12;

            // Dashboard (All roles)
            AddSidebarButton(sidebarPanel, "📊 Dashboard", () => new HRManagerDashboardPage(this), ref sidebarY);
            sidebarY += 8;

            // ═══ RECRUITMENT SECTION (Both HR Manager and Admin) ═══
            sidebarPanel.Controls.Add(new Label { Text = "RECRUITMENT", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TextMuted, Left = 16, Top = sidebarY, AutoSize = true, BackColor = Color.Transparent });
            sidebarY += 28;

            // ✅ ALL HR MANAGER items (BOTH roles see these)
            AddSidebarButton(sidebarPanel, "📋 Applicant List", () => new HRManagerApplicantListForm(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "👁️ Applicant Review", () => new HRManagerApplicantReviewPage(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "🔍 Screening", () => new HRManagerScreeningPage(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "📅 Interview Schedule", () => new HRManagerInterviewSchedulePage(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "⭐ Interview Evaluation", () => new HRManagerInterviewEvaluationPage(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "✅ Hiring Decision", () => new HRManagerHiringDecisionPage(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "💼 Job Vacancies", () => new HRManagerJobVacancyPage(this), ref sidebarY);
            AddSidebarButton(sidebarPanel, "📈 Reports", () => new HRManagerReportsPage(this), ref sidebarY);

            sidebarY += 8;

            // ═══ ADMINISTRATION SECTION (ADMIN ONLY) ═══
            if (isAdmin)
            {
                sidebarPanel.Controls.Add(new Label { Text = "ADMINISTRATION", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TextMuted, Left = 16, Top = sidebarY, AutoSize = true, BackColor = Color.Transparent });
                sidebarY += 28;

                AddSidebarButton(sidebarPanel, "⚙️ Maintenance", () => new HRManagerMaintenancePage(this), ref sidebarY);
                AddSidebarButton(sidebarPanel, "👥 Manage Users", () => new HRManagerUsersPage(this), ref sidebarY);
                AddSidebarButton(sidebarPanel, "📋 Audit Trail", () => new HRManagerAuditTrailPage(this), ref sidebarY);

                sidebarY += 8;
            }

            // Logout
            Button btnLogout = new Button
            {
                Text = "🚪 Logout",
                Left = 12,
                Top = sidebarY,
                Width = 226,
                Height = 36,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) =>
            {
                AdminSession.ClearAll();
                this.Close();
                new ApplicantPortal.AdminLoginForm().Show();
            };
            sidebarPanel.Controls.Add(btnLogout);

            // ── CONTENT PANEL ────────────────────────────────────────
            contentPanel = new Panel
            {
                Left = 250,
                Top = 0,
                Width = this.Width - 250,
                Height = this.Height,
                BackColor = BgContent,
                AutoScroll = true
            };
            this.Controls.Add(contentPanel);

            this.Resize += (s, e) =>
            {
                sidebarPanel.Height = this.Height;
                contentPanel.Width = this.Width - 250;
                contentPanel.Height = this.Height;
            };
        }

        private void AddSidebarButton(Panel sidebar, string text, Action onClick, ref int y)
        {
            Button btn = new Button
            {
                Text = text,
                Left = 12,
                Top = y,
                Width = 226,
                Height = 40,
                Font = new Font("Segoe UI", 9f),
                BackColor = BgSidebar,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = HoverColor;
            btn.FlatAppearance.MouseOverBackColor = HoverColor;

            btn.Click += (s, e) => onClick?.Invoke();
            btn.MouseEnter += (s, e) => btn.BackColor = HoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = BgSidebar;

            sidebar.Controls.Add(btn);
            y += 44;
        }

        public void ClearContent()
        {
            contentPanel.Controls.Clear();
        }

        private void HRManagerMainForm_Load(object sender, EventArgs e)
        {

        }
    }
}