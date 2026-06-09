using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRManager
{
    public partial class HRManagerDashboardPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentOrange = Color.FromArgb(249, 115, 22);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;

        public HRManagerDashboardPage(HRManagerMainForm main)
        {
            mainForm = main;
            contentPanel = main.contentPanel;
            InitializePage();
        }

        private void InitializePage()
        {
            mainForm.ClearContent();
            var p = contentPanel;
            p.BackColor = BgPage;
            p.AutoScroll = true;

            p.Controls.Clear();
            p.AutoScrollPosition = new Point(0, 0);

            var db = new DatabaseConnection();
            int top = 24;

            // Title
            Label title = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 24,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            p.Controls.Add(title);
            top += 36;

            Label subtitle = new Label
            {
                Text = "Overview of pending decisions, interviews, and system activity.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = 70,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            p.Controls.Add(subtitle);
            top += 32;

            try
            {
                // ─── STATISTICS CARDS ────────────────────────────────────
                DataTable stats = db.Query("SELECT metric, count FROM vw_hr_dashboard_summary");
                int pendingApps = 0, pendingDecisions = 0, accepted = 0, openVacancies = 0, interviews = 0;

                foreach (DataRow row in stats.Rows)
                {
                    string metric = row["metric"].ToString();
                    int count = Convert.ToInt32(row["count"]);

                    if (metric == "Pending Applications") pendingApps = count;
                    else if (metric == "Pending Final Decisions") pendingDecisions = count;
                    else if (metric == "Accepted Applicants") accepted = count;
                    else if (metric == "Open Vacancies") openVacancies = count;
                    else if (metric == "Scheduled Interviews") interviews = count;
                }

                var statData = new[] {
                    ("📋 PENDING\nAPPLICATIONS", pendingApps.ToString(),   AccentBlue),
                    ("⏳ PENDING\nDECISIONS",    pendingDecisions.ToString(), AccentOrange),
                    ("✅ ACCEPTED",              accepted.ToString(),       AccentGreen),
                    ("📅 INTERVIEWS",            interviews.ToString(),     Color.FromArgb(139, 92, 246))
                };

                // Calculate stat card width dynamically
                int usableWidth = p.ClientSize.Width - 48;  // 24px margin each side
                int statWidth = Math.Max(140, usableWidth / 4 - 12);  // 12px gap between cards

                int statLeft = 24;
                foreach (var (label, val, color) in statData)
                {
                    Panel stat = CreateStatCard(label, val, color, statWidth);
                    stat.Left = statLeft;
                    stat.Top = top;
                    p.Controls.Add(stat);
                    statLeft += statWidth + 12;
                }
                top += 140;

                // ─── PENDING DECISIONS SECTION ────────────────────────────
                Label sectionLabel1 = new Label
                {
                    Text = "Pending Final Decisions",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Left = 24,
                    Top = top,
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                p.Controls.Add(sectionLabel1);
                top += 32;

                DataTable pending = db.Query(
                    @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS name, jv.title AS job_title,
                              d.name AS department, ap.email
                      FROM applications a
                      JOIN applicants ap ON ap.id = a.applicant_id
                      JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                      JOIN departments d ON d.id = jv.department_id
                      WHERE a.status = 'For Final Review'
                      LIMIT 5");

                if (pending.Rows.Count == 0)
                {
                    Panel emptyCard = new Panel
                    {
                        Left = 24,
                        Top = top,
                        Width = p.ClientSize.Width - 48,
                        Height = 60,
                        BackColor = BgCard,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    emptyCard.Paint += (s, e) =>
                    {
                        using (var pen = new Pen(BorderLight, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, emptyCard.Width - 1, emptyCard.Height - 1);
                    };
                    emptyCard.Controls.Add(new Label
                    {
                        Text = "✓ No pending decisions",
                        Font = new Font("Segoe UI", 10f),
                        ForeColor = TextMuted,
                        Left = 12,
                        Top = 18,
                        AutoSize = true,
                        BackColor = Color.Transparent
                    });
                    p.Controls.Add(emptyCard);
                    top += 72;
                }
                else
                {
                    foreach (DataRow row in pending.Rows)
                    {
                        int appId = Convert.ToInt32(row["id"]);
                        Panel card = new Panel
                        {
                            Left = 24,
                            Top = top,
                            Width = p.ClientSize.Width - 48,
                            Height = 80,
                            BackColor = BgCard,
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                        };
                        card.Paint += (s, e) =>
                        {
                            using (var pen = new Pen(BorderLight, 1))
                                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                            e.Graphics.FillRectangle(new SolidBrush(AccentOrange), 0, 0, card.Width, 3);
                        };

                        card.Controls.Add(new Label
                        {
                            Text = row["name"].ToString(),
                            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                            ForeColor = TextPrimary,
                            Left = 12,
                            Top = 6,
                            AutoSize = true,
                            BackColor = Color.Transparent
                        });
                        card.Controls.Add(new Label
                        {
                            Text = row["job_title"] + " • " + row["department"],
                            Font = new Font("Segoe UI", 9f),
                            ForeColor = TextSecondary,
                            Left = 12,
                            Top = 28,
                            AutoSize = true,
                            BackColor = Color.Transparent
                        });
                        card.Controls.Add(new Label
                        {
                            Text = row["email"].ToString(),
                            Font = new Font("Segoe UI", 8.5f),
                            ForeColor = TextMuted,
                            Left = 12,
                            Top = 46,
                            AutoSize = true,
                            BackColor = Color.Transparent
                        });

                        Button btnDecide = new Button
                        {
                            Text = "📋 Make Decision",
                            Left = card.Width - 180,
                            Top = 16,
                            Width = 160,
                            Height = 34,
                            BackColor = AccentOrange,
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                            Cursor = Cursors.Hand,
                            Anchor = AnchorStyles.Top | AnchorStyles.Right
                        };
                        btnDecide.FlatAppearance.BorderSize = 0;
                        btnDecide.Click += (s, e) =>
                        {
                            MessageBox.Show($"Opening Hiring Decision for Application #{appId}", "Navigate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        };
                        card.Controls.Add(btnDecide);

                        p.Controls.Add(card);
                        top += 88;
                    }
                }

                top += 20;

                // ─── RECENT ACTIVITY SECTION ────────────────────────────
                Label sectionLabel2 = new Label
                {
                    Text = "Recent Activity",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Left = 24,
                    Top = top,
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                p.Controls.Add(sectionLabel2);
                top += 32;

                DataTable activity = db.Query(
                    @"SELECT action, username, table_name, details, logged_at
                      FROM audit_trail
                      ORDER BY logged_at DESC
                      LIMIT 6");

                if (activity.Rows.Count == 0)
                {
                    Panel emptyActivity = new Panel
                    {
                        Left = 24,
                        Top = top,
                        Width = p.ClientSize.Width - 48,
                        Height = 60,
                        BackColor = BgCard,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    emptyActivity.Paint += (s, e) =>
                    {
                        using (var pen = new Pen(BorderLight, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, emptyActivity.Width - 1, emptyActivity.Height - 1);
                    };
                    emptyActivity.Controls.Add(new Label
                    {
                        Text = "No activity yet",
                        Font = new Font("Segoe UI", 10f),
                        ForeColor = TextMuted,
                        Left = 12,
                        Top = 18,
                        AutoSize = true,
                        BackColor = Color.Transparent
                    });
                    p.Controls.Add(emptyActivity);
                    top += 72;
                }
                else
                {
                    foreach (DataRow row in activity.Rows)
                    {
                        string action = row["action"].ToString();
                        Color actionColor = action.Contains("Create") ? AccentGreen :
                                           action.Contains("Update") ? AccentOrange :
                                           action.Contains("Delete") ? AccentRed : AccentBlue;

                        Panel actCard = new Panel
                        {
                            Left = 24,
                            Top = top,
                            Width = p.ClientSize.Width - 48,
                            Height = 70,
                            BackColor = BgCard,
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                        };
                        actCard.Paint += (s, e) =>
                        {
                            using (var pen = new Pen(BorderLight, 1))
                                e.Graphics.DrawRectangle(pen, 0, 0, actCard.Width - 1, actCard.Height - 1);
                        };

                        actCard.Controls.Add(new Label
                        {
                            Text = action,
                            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                            ForeColor = actionColor,
                            Left = 12,
                            Top = 8,
                            AutoSize = true,
                            BackColor = Color.Transparent
                        });
                        actCard.Controls.Add(new Label
                        {
                            Text = "by " + row["username"],
                            Font = new Font("Segoe UI", 8.5f),
                            ForeColor = TextSecondary,
                            Left = 12,
                            Top = 28,
                            AutoSize = true,
                            BackColor = Color.Transparent
                        });

                        string timeStr = Convert.ToDateTime(row["logged_at"]).ToString("MMM dd, hh:mm tt");
                        actCard.Controls.Add(new Label
                        {
                            Text = timeStr,
                            Font = new Font("Segoe UI", 8f),
                            ForeColor = TextMuted,
                            Left = actCard.Width - 160,
                            Top = 12,
                            Width = 150,
                            Height = 35,
                            TextAlign = ContentAlignment.MiddleRight,
                            BackColor = Color.Transparent,
                            Anchor = AnchorStyles.Top | AnchorStyles.Right
                        });

                        p.Controls.Add(actCard);
                        top += 76;
                    }
                }

                p.AutoScrollMinSize = new Size(0, top + 40);
            }
            catch (Exception ex)
            {
                p.Controls.Add(new Label
                {
                    Text = $"❌ Error loading dashboard: {ex.Message}",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = AccentRed,
                    Left = 24,
                    Top = top,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
            }
        }

        private Panel CreateStatCard(string label, string value, Color color, int width)
        {
            Panel card = new Panel
            {
                Width = width,
                Height = 120,
                BackColor = BgCard
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderLight, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.FillRectangle(new SolidBrush(color), 0, 0, card.Width, 4);
            };

            card.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TextMuted,
                Left = 8,
                Top = 12,
                Width = width - 16,
                Height = 40,
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent
            });

            card.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = color,
                Left = 8,
                Top = 54,
                AutoSize = true,
                BackColor = Color.Transparent
            });

            return card;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerDashboardPage";
            Load += HRManagerDashboardPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerDashboardPage_Load(object sender, EventArgs e)
        {

        }
    }
}