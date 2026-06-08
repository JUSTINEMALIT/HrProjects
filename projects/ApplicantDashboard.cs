using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace project
{
    public partial class ApplicantDashboard : Form
    {
        // COLORS
        Color bg = Color.FromArgb(20, 20, 20);
        Color sidebar = Color.FromArgb(32, 32, 32);
        Color card = Color.FromArgb(28, 28, 28);
        Color blue = Color.FromArgb(26, 86, 219);
        Color textMuted = Color.FromArgb(150, 150, 150);

        // Global Panels para sa paglilipat ng screen
        private Panel mainPanel;
        private Panel sidebarPanel;

        public ApplicantDashboard()
        {
            this.Text = "Applicant Portal";
            this.Size = new Size(1100, 750); // Itinaas ang laki para maging swak sa profile data
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            BuildUI();
            LoadDashboardView(); // Unang ipapakita ang Dashboard summary
        }

        private void BuildUI()
        {
            // ================= TOP BAR =================
            Panel topbar = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = blue
            };
            this.Controls.Add(topbar);

            Label logo = new Label
            {
                Text = "HR   Applicant Portal",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 18),
                AutoSize = true
            };
            topbar.Controls.Add(logo);

            Button logout = new Button
            {
                Text = "Logout",
                Size = new Size(90, 32),
                Location = new Point(this.Width - 120, 14),
                BackColor = Color.White,
                ForeColor = blue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            logout.FlatAppearance.BorderSize = 0;
            topbar.Controls.Add(logout);

            // ================= SIDEBAR =================
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = sidebar
            };
            this.Controls.Add(sidebarPanel);

            string[] menus = {
                "Dashboard", "My Profile", "Job Vacancies",
                "My Application", "My Documents", "App Status", "Change Password"
            };

            int y = 30;
            foreach (string menu in menus)
            {
                Button btn = new Button
                {
                    Text = "   " + menu,
                    Size = new Size(170, 42),
                    Location = new Point(15, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = textMuted,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                btn.FlatAppearance.BorderSize = 0;

                // Click Event para sa Screen Switching
                btn.Click += MenuButton_Click;

                sidebarPanel.Controls.Add(btn);
                y += 50;
            }

            // Highlight-an natin ang unang active menu (Dashboard)
            HighlightMenu("Dashboard");

            // ================= MAIN CONTAINER PANEL =================
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = bg,
                AutoScroll = true // Para ma-scroll kapag mahaba ang form gaya ng profile
            };
            this.Controls.Add(mainPanel);
        }

        // Handles menu switching cleanly
        private void MenuButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string menuText = clickedButton.Text.Trim();

            HighlightMenu(menuText);

            if (menuText == "My Profile")
            {
                // Nililinis ang main content at ikinakarga ang Profile view
                mainPanel.Controls.Clear();
                MyProfileControl profileView = new MyProfileControl();
                profileView.Dock = DockStyle.Fill;
                mainPanel.Controls.Add(profileView);
            }
            else if (menuText == "Dashboard")
            {
                // Nililinis ang main content at ibinabalik ang Dashboard details
                mainPanel.Controls.Clear();
                LoadDashboardView();
            }
            else
            {
                // Placeholder para sa ibang tab
                mainPanel.Controls.Clear();
                Label placeholder = new Label { Text = menuText + " Content Coming Soon...", ForeColor = Color.White, Font = new Font("Segoe UI", 14), Location = new Point(40, 40), AutoSize = true };
                mainPanel.Controls.Add(placeholder);
            }
        }

        private void HighlightMenu(string menuName)
        {
            foreach (Control ctl in sidebarPanel.Controls)
            {
                if (ctl is Button btn)
                {
                    if (btn.Text.Trim() == menuName)
                    {
                        btn.BackColor = Color.FromArgb(45, 45, 45);
                        btn.ForeColor = Color.White;
                    }
                    else
                    {
                        btn.BackColor = Color.Transparent;
                        btn.ForeColor = textMuted;
                    }
                }
            }
        }

        // Dito inilagay ang default UI elements ng Dashboard dashboard mo
        private void LoadDashboardView()
        {
            // Panel wrapper para sa dashboard items para hindi magulo kapag niresize
            Panel wrapper = new Panel { Dock = DockStyle.Top, Height = 600, BackgroundImage = null, BackColor = Color.Transparent };
            mainPanel.Controls.Add(wrapper);

            Label welcome = new Label { Text = "Welcome back, Juan!", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, Location = new Point(30, 25), AutoSize = true };
            Label sub = new Label { Text = "Here is a summary of your current application status.", ForeColor = textMuted, Font = new Font("Segoe UI", 10), Location = new Point(32, 65), AutoSize = true };
            wrapper.Controls.AddRange(new Control[] { welcome, sub });

            // Modernized Cards setup
            CreateCard(wrapper, "Applications", "2", "1 active", 30, 110, Color.FromArgb(34, 197, 94));
            CreateCard(wrapper, "Current Status", "For Interview", "In progress", 215, 110, blue);
            CreateCard(wrapper, "Missing Docs", "2", "Action needed", 400, 110, Color.FromArgb(239, 68, 68));
            CreateCard(wrapper, "Next Interview", "Jun 5", "2:00 PM", 585, 110, Color.FromArgb(168, 85, 247));

            // Modern Info Panel
            Panel info = new Panel { Size = new Size(715, 55), Location = new Point(30, 220), BackColor = Color.FromArgb(30, 41, 59), Padding = new Padding(15, 0, 0, 0) };
            info.Paint += (s, e) => RoundControl(info, 8);
            Label infoText = new Label { Text = "💡 Upcoming interview on June 5 at 2:00 PM via Google Meet.", ForeColor = Color.FromArgb(147, 197, 253), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Location = new Point(15, 18), AutoSize = true };
            info.Controls.Add(infoText);
            wrapper.Controls.Add(info);

            // Activity Panel
            Panel activity = new Panel { Size = new Size(715, 240), Location = new Point(30, 295), BackColor = card };
            activity.Paint += (s, e) => RoundControl(activity, 12);
            wrapper.Controls.Add(activity);

            Label recent = new Label { Text = "Recent activity", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 18), AutoSize = true };
            activity.Controls.Add(recent);

            AddActivity(activity, "Application submitted", "May 20, 2026", 60);
            AddActivity(activity, "Under review", "May 22, 2026", 105);
            AddActivity(activity, "Shortlisted", "May 24, 2026", 150);
            AddActivity(activity, "For Interview", "May 27, 2026", 195);
        }

        private void CreateCard(Control parent, string title, string value, string subText, int x, int y, Color indicatorColor)
        {
            Panel p = new Panel { Size = new Size(170, 95), Location = new Point(x, y), BackColor = card };
            p.Paint += (s, e) => RoundControl(p, 10);

            Label t = new Label { Text = title, ForeColor = textMuted, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Location = new Point(12, 12), AutoSize = true };
            Label v = new Label { Text = value, ForeColor = Color.White, Font = new Font("Segoe UI", 15, FontStyle.Bold), Location = new Point(10, 34), AutoSize = true };
            Label s = new Label { Text = subText, ForeColor = indicatorColor, Font = new Font("Segoe UI", 8.5F), Location = new Point(12, 68), AutoSize = true };

            p.Controls.AddRange(new Control[] { t, v, s });
            parent.Controls.Add(p);
        }

        private void AddActivity(Control parent, string title, string date, int y)
        {
            Panel dot = new Panel { BackColor = Color.FromArgb(34, 197, 94), Size = new Size(10, 10), Location = new Point(25, y + 6) };
            dot.Paint += (s, e) => { GraphicsPath gp = new GraphicsPath(); gp.AddEllipse(0, 0, 9, 9); dot.Region = new Region(gp); };

            Label t = new Label { Text = title, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Location = new Point(50, y), AutoSize = true };
            Label d = new Label { Text = date, ForeColor = textMuted, Font = new Font("Segoe UI", 8.5F), Location = new Point(50, y + 20), AutoSize = true };

            parent.Controls.AddRange(new Control[] { dot, t, d });
        }

        private void RoundControl(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius - 1, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius - 1, control.Height - radius - 1, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius - 1, radius, radius, 90, 90);
            path.CloseAllFigures();
            control.Region = new Region(path);
        }
    }
}