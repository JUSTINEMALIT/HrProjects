using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using project;

namespace HRApplicant
{
    public class ApplicantDashboardPage : Form
    {
        // ── Design Tokens ─────────────────────────────────────────────
        private static readonly Color BgPage = Color.FromArgb(13, 15, 18);
        private static readonly Color BgCard = Color.FromArgb(20, 24, 28);
        private static readonly Color BgCardHover = Color.FromArgb(26, 31, 36);
        private static readonly Color BorderSubtle = Color.FromArgb(32, 40, 48);
        private static readonly Color TextPrimary = Color.FromArgb(228, 234, 240);
        private static readonly Color TextSecondary = Color.FromArgb(88, 110, 128);
        private static readonly Color TextMuted = Color.FromArgb(52, 68, 80);
        private static readonly Color AccentBlue = Color.FromArgb(56, 149, 255);
        private static readonly Color AccentGreen = Color.FromArgb(52, 211, 153);
        private static readonly Color AccentAmber = Color.FromArgb(251, 176, 59);
        private static readonly Color AccentPurple = Color.FromArgb(167, 119, 255);
        private static readonly Color AccentRed = Color.FromArgb(248, 81, 73);

        private Panel contentPanel;
        private ApplicantMainForm mainForm;

        public ApplicantDashboardPage(ApplicantMainForm mainForm)
        {
            this.mainForm = mainForm;
            this.contentPanel = mainForm.contentPanel;
            InitializePage();
        }

        private void InitializePage()
        {
            mainForm.ClearContent();
            DatabaseConnection db = new DatabaseConnection();

            int top = 0;

            // ── Page Header ───────────────────────────────────────────
            Panel header = new Panel
            {
                Left = 0,
                Top = 0,
                Width = contentPanel.Width,
                Height = 80,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            header.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderSubtle, 1))
                    e.Graphics.DrawLine(pen, 32, header.Height - 1, header.Width - 32, header.Height - 1);
            };
            header.Controls.Add(new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI Semibold", 17f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 32,
                Top = 14,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            header.Controls.Add(new Label
            {
                Text = "Welcome back, " + project.Session.ApplicantName,
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextSecondary,
                Left = 33,
                Top = 44,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            contentPanel.Controls.Add(header);
            top += 90;

            // ── Stat Cards ────────────────────────────────────────────
            int totalApps = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE applicant_id=@aid", ("@aid", Session.ApplicantId)));
            int submitted = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE applicant_id=@aid AND status != 'Draft'", ("@aid", Session.ApplicantId)));
            int underReview = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE applicant_id=@aid AND status='Under Review'", ("@aid", Session.ApplicantId)));
            int interviews = ToInt(db.Scalar("SELECT COUNT(*) FROM interview_schedules is2 JOIN applications a ON a.id=is2.application_id WHERE a.applicant_id=@aid AND is2.status='Scheduled'", ("@aid", Session.ApplicantId)));

            var statDefs = new[]
            {
                ("Total Applications",  totalApps.ToString(),    AccentBlue,   "📋"),
                ("Submitted",           submitted.ToString(),    AccentGreen,  "✅"),
                ("Under Review",        underReview.ToString(),  AccentAmber,  "🔍"),
                ("Upcoming Interviews", interviews.ToString(),   AccentPurple, "📅"),
            };

            TableLayoutPanel statRow = new TableLayoutPanel
            {
                Left = 32,
                Top = top,
                Width = contentPanel.Width - 64,
                Height = 96,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 4,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            statRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 4; i++)
                statRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            int colIdx = 0;
            foreach (var (title, val, accent, icon) in statDefs)
            {
                Panel sc = StatCard(title, val, accent, icon);
                sc.Dock = DockStyle.Fill;
                sc.Margin = new Padding(colIdx == 0 ? 0 : 6, 0, colIdx == 3 ? 0 : 6, 0);
                statRow.Controls.Add(sc, colIdx, 0);
                colIdx++;
            }
            contentPanel.Controls.Add(statRow);
            top += 112;

            // ── Missing Documents ─────────────────────────────────────
            top = AddSectionLabel(contentPanel, "MISSING DOCUMENTS", top);

            DataTable missingDocs = db.Query(
                @"SELECT DISTINCT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS name, jv.title AS job,
                         COUNT(ad.id) AS missing_count
                  FROM applications a
                  JOIN applicants ap ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  JOIN applicant_documents ad ON ad.application_id = a.id AND ad.status = 'Missing'
                  WHERE a.applicant_id = @aid AND a.status NOT IN ('Draft','Withdrawn','Rejected')
                  GROUP BY a.id
                  ORDER BY missing_count DESC LIMIT 5",
                ("@aid", project.Session.ApplicantId));

            if (missingDocs.Rows.Count == 0)
            {
                contentPanel.Controls.Add(EmptyState("✓  No missing documents — you're all caught up!", AccentGreen, 32, top));
                top += 40;
            }
            else
            {
                foreach (DataRow row in missingDocs.Rows)
                {
                    int count = Convert.ToInt32(row["missing_count"]);
                    Panel card = CreateCard(28, top, contentPanel.Width - 56, 56);
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                    card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 3, Height = 56, BackColor = AccentRed, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom });

                    Panel dot = new Panel { Left = 18, Top = 20, Width = 8, Height = 8, BackColor = AccentRed, Anchor = AnchorStyles.Top | AnchorStyles.Left };
                    MakeRound(dot, 4);
                    card.Controls.Add(dot);

                    card.Controls.Add(new Label
                    {
                        Text = row["job"].ToString(),
                        Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                        ForeColor = TextPrimary,
                        Left = 34,
                        Top = 8,
                        Width = 200,
                        AutoSize = false,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    });
                    card.Controls.Add(new Label
                    {
                        Text = row["name"].ToString(),
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = TextSecondary,
                        Left = 34,
                        Top = 28,
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left
                    });
                    card.Controls.Add(new Label
                    {
                        Text = count + (count == 1 ? " document missing" : " documents missing"),
                        Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                        ForeColor = AccentRed,
                        TextAlign = ContentAlignment.MiddleRight,
                        Width = 170,
                        Height = 56,
                        Top = 0,
                        AutoSize = false,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
                    });
                    Label missingLbl = (Label)card.Controls[card.Controls.Count - 1];
                    card.SizeChanged += (s, e) => missingLbl.Left = card.Width - 180;
                    missingLbl.Left = card.Width - 180;

                    contentPanel.Controls.Add(card);
                    top += 62;
                }
            }
            top += 12;

            // ── Recent Applications ───────────────────────────────────
            top = AddSectionLabel(contentPanel, "RECENT APPLICATIONS", top);

            DataTable recent = db.Query(
                @"SELECT a.id, a.status, jv.title AS job, a.submitted_at, a.created_at,
                         (SELECT COUNT(*) FROM applicant_documents ad WHERE ad.application_id=a.id AND ad.status='Missing') AS missing_docs
                  FROM applications a
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  WHERE a.applicant_id = @aid
                  ORDER BY a.created_at DESC LIMIT 5",
                ("@aid", project.Session.ApplicantId));

            if (recent.Rows.Count == 0)
            {
                contentPanel.Controls.Add(EmptyState("No applications yet.", TextMuted, 32, top));
                top += 40;
            }
            else
            {
                foreach (DataRow row in recent.Rows)
                {
                    string status = row["status"].ToString();
                    Color statusColor = GetStatusColor(status);
                    int missingDocs2 = Convert.ToInt32(row["missing_docs"]);
                    string dateStr = row["submitted_at"] == DBNull.Value
                        ? "Draft"
                        : "Submitted " + Convert.ToDateTime(row["submitted_at"]).ToString("MMM dd");

                    Panel card = CreateCard(28, top, contentPanel.Width - 56, 58);
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                    card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 3, Height = 58, BackColor = statusColor, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom });

                    card.Controls.Add(new Label
                    {
                        Text = row["job"].ToString(),
                        Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                        ForeColor = TextPrimary,
                        Left = 18,
                        Top = 8,
                        AutoSize = false,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    });
                    Label jobLbl = (Label)card.Controls[card.Controls.Count - 1];
                    card.SizeChanged += (s, e) => jobLbl.Width = card.Width - 18 - 140;
                    jobLbl.Width = card.Width - 18 - 140;

                    Panel pill = MakePill(status, statusColor, 0, 10);
                    pill.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    card.Controls.Add(pill);
                    card.SizeChanged += (s, e) => pill.Left = card.Width - 134;
                    pill.Left = card.Width - 134;

                    card.Controls.Add(new Label
                    {
                        Text = missingDocs2 > 0 ? $"⚠  {missingDocs2} missing" : "✓  Complete",
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = missingDocs2 > 0 ? AccentRed : Color.FromArgb(52, 160, 100),
                        Left = 18,
                        Top = 34,
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                    });

                    Label dateLbl = new Label
                    {
                        Text = dateStr,
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = TextSecondary,
                        TextAlign = ContentAlignment.MiddleRight,
                        Width = 150,
                        Top = 34,
                        AutoSize = false,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                    };
                    card.Controls.Add(dateLbl);
                    card.SizeChanged += (s, e) => dateLbl.Left = card.Width - 164;
                    dateLbl.Left = card.Width - 164;

                    contentPanel.Controls.Add(card);
                    top += 64;
                }
            }
            top += 12;

            // ── Upcoming Interviews ───────────────────────────────────
            top = AddSectionLabel(contentPanel, "UPCOMING INTERVIEWS", top);

            DataTable interviews2 = db.Query(
                @"SELECT jv.title AS job, is2.interview_type, is2.scheduled_date, is2.scheduled_time,
                         is2.mode, is2.interviewer
                  FROM interview_schedules is2
                  JOIN applications a ON a.id = is2.application_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  WHERE a.applicant_id = @aid AND is2.status = 'Scheduled' AND is2.scheduled_date >= CURDATE()
                  ORDER BY is2.scheduled_date LIMIT 5",
                ("@aid", project.Session.ApplicantId));

            if (interviews2.Rows.Count == 0)
            {
                contentPanel.Controls.Add(EmptyState("No upcoming interviews scheduled.", TextMuted, 32, top));
                top += 40;
            }
            else
            {
                foreach (DataRow row in interviews2.Rows)
                {
                    Panel card = CreateCard(28, top, contentPanel.Width - 56, 68);
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 3, Height = 68, BackColor = AccentBlue, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom });

                    Panel dateBadge = new Panel { Left = 14, Top = 10, Width = 48, Height = 48, BackColor = Color.FromArgb(30, 56, 149, 255), Anchor = AnchorStyles.Top | AnchorStyles.Left };
                    MakeRound(dateBadge, 6);
                    dateBadge.Controls.Add(new Label
                    {
                        Text = Convert.ToDateTime(row["scheduled_date"]).ToString("MMM").ToUpper(),
                        Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                        ForeColor = AccentBlue,
                        Left = 0,
                        Top = 7,
                        Width = 48,
                        TextAlign = ContentAlignment.TopCenter,
                        BackColor = Color.Transparent
                    });
                    dateBadge.Controls.Add(new Label
                    {
                        Text = Convert.ToDateTime(row["scheduled_date"]).ToString("dd"),
                        Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                        ForeColor = AccentBlue,
                        Left = 0,
                        Top = 20,
                        Width = 48,
                        TextAlign = ContentAlignment.TopCenter,
                        BackColor = Color.Transparent
                    });
                    card.Controls.Add(dateBadge);

                    card.Controls.Add(new Label
                    {
                        Text = row["job"].ToString() + "  ·  " + row["interview_type"],
                        Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                        ForeColor = TextPrimary,
                        Left = 72,
                        Top = 8,
                        AutoSize = false,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    });
                    Label intTitleLbl = (Label)card.Controls[card.Controls.Count - 1];
                    card.SizeChanged += (s, e) => intTitleLbl.Width = card.Width - 72 - 12;
                    intTitleLbl.Width = card.Width - 72 - 12;

                    card.Controls.Add(new Label
                    {
                        Text = row["scheduled_time"] + "  ·  " + row["mode"],
                        Font = new Font("Segoe UI", 8.5f),
                        ForeColor = TextSecondary,
                        Left = 72,
                        Top = 30,
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left
                    });
                    card.Controls.Add(new Label
                    {
                        Text = "Interviewer: " + row["interviewer"],
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = TextMuted,
                        Left = 72,
                        Top = 50,
                        AutoSize = true,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left
                    });

                    contentPanel.Controls.Add(card);
                    top += 74;
                }
            }

            top += 24;
        }

        // ── Helpers ───────────────────────────────────────────────────

        private Panel StatCard(string title, string value, Color accent, string icon)
        {
            Panel card = new Panel { Height = 94, BackColor = BgCard };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderSubtle, 1))
                    DrawRoundedRect(e.Graphics, pen, 0, 0, card.Width - 1, card.Height - 1, 6);
                using (var br = new LinearGradientBrush(new Rectangle(0, 0, card.Width, 2), accent, Color.Transparent, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(br, 4, 0, card.Width - 8, 2);
            };
            card.Controls.Add(new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 14f),
                ForeColor = Color.FromArgb(180, accent),
                Left = 12,
                Top = 10,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            card.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI Semibold", 22f, FontStyle.Bold),
                ForeColor = accent,
                Left = 12,
                Top = 34,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            card.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = TextSecondary,
                Left = 12,
                Top = 70,
                Width = 100,
                AutoSize = false,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            Label titleLbl = (Label)card.Controls[card.Controls.Count - 1];
            card.SizeChanged += (s, e) => titleLbl.Width = card.Width - 20;
            return card;
        }

        private Panel MakePill(string text, Color color, int left, int top)
        {
            Panel pill = new Panel { Left = left, Top = top, Width = 120, Height = 20, BackColor = Color.FromArgb(30, color) };
            MakeRound(pill, 10);
            pill.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = color,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });
            return pill;
        }

        private Panel CreateCard(int left, int top, int width, int height)
        {
            Panel card = new Panel
            {
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                BackColor = BgCard
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderSubtle, 1))
                    DrawRoundedRect(e.Graphics, pen, 0, 0, card.Width - 1, card.Height - 1, 5);
            };
            card.MouseEnter += (s, e) => { card.BackColor = BgCardHover; card.Invalidate(); };
            card.MouseLeave += (s, e) => { card.BackColor = BgCard; card.Invalidate(); };
            return card;
        }

        private Label EmptyState(string text, Color color, int left, int top)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f),
                ForeColor = color,
                Left = left,
                Top = top,
                AutoSize = true,
                Height = 30,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
        }

        private int AddSectionLabel(Panel p, string text, int top)
        {
            p.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TextMuted,
                Left = 32,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            return top + 26;
        }

        private void MakeRound(Control ctrl, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(ctrl.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(ctrl.Width - radius * 2, ctrl.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, ctrl.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            ctrl.Region = new Region(path);
        }

        private void DrawRoundedRect(Graphics g, Pen pen, int x, int y, int w, int h, int r)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseAllFigures();
                g.DrawPath(pen, path);
            }
        }

        private Color GetStatusColor(string status)
        {
            if (status == "Draft") return Color.FromArgb(130, 130, 160);
            if (status == "Submitted") return Color.FromArgb(56, 149, 255);
            if (status == "Under Review") return Color.FromArgb(251, 176, 59);
            if (status == "Shortlisted") return Color.FromArgb(52, 211, 153);
            if (status == "For Interview") return Color.FromArgb(167, 119, 255);
            if (status == "For Assessment") return Color.FromArgb(200, 130, 60);
            if (status == "For Final Review") return Color.FromArgb(160, 200, 80);
            if (status == "Accepted") return Color.FromArgb(52, 211, 100);
            if (status == "Rejected") return Color.FromArgb(248, 81, 73);
            if (status == "Withdrawn") return Color.FromArgb(160, 100, 80);
            return Color.FromArgb(150, 150, 150);
        }

        private int ToInt(object v) => v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantStatusTrackingPage
            // 
            ClientSize = new Size(284, 261);
            Name = "ApplicantDashboardPage";
            Load += ApplicantDashboardPage_Load;
            ResumeLayout(false);

        }

        private void ApplicantDashboardPage_Load(object sender, EventArgs e)
        {

        }
    }
}