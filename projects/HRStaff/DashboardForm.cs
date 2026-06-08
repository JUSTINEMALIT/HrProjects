using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.AutoScroll = true;

            var db = new DatabaseConnection();

            // ── Stats ────────────────────────────────────────────────
            int total = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE status != 'Draft'"));
            int pending = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE status = 'Submitted'"));
            int underReview = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE status = 'Under Review'"));
            int forInterview = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE status = 'For Interview'"));
            int accepted = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE status = 'Accepted'"));
            int rejected = ToInt(db.Scalar("SELECT COUNT(*) FROM applications WHERE status = 'Rejected'"));

            // ── Header ───────────────────────────────────────────────
            Add(new Label { Text = "Dashboard", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 20, AutoSize = true, BackColor = Color.Transparent });
            Add(new Label { Text = "Welcome, " + project.Session.AdminFullName + "   |   " + DateTime.Now.ToString("MMMM dd, yyyy"), Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 48, AutoSize = true, BackColor = Color.Transparent });

            // ── Stat cards ───────────────────────────────────────────
            string[] titles = { "Total Applications", "Pending Review", "Under Review", "For Interview", "Accepted", "Rejected" };
            string[] vals = { total.ToString(), pending.ToString(), underReview.ToString(), forInterview.ToString(), accepted.ToString(), rejected.ToString() };
            Color[] colors = { Color.FromArgb(80, 160, 220), Color.FromArgb(230, 160, 50), Color.FromArgb(140, 100, 220), Color.FromArgb(80, 200, 160), Color.FromArgb(60, 200, 100), Color.FromArgb(220, 80, 80) };
            int cw = 168, cg = 12;
            for (int i = 0; i < 6; i++)
            {
                var c = StatCard(titles[i], vals[i], colors[i]);
                c.Left = 28 + i * (cw + cg); c.Top = 76; c.Width = cw;
                Add(c);
            }

            // ── Recent Applications ──────────────────────────────────
            Add(SectionLbl("RECENT APPLICATIONS", 182));

            Panel tHdr = HdrPanel(28, 204);
            tHdr.Controls.Add(Hdr("APP ID", 12, 80));
            tHdr.Controls.Add(Hdr("APPLICANT", 100, 180));
            tHdr.Controls.Add(Hdr("JOB", 288, 190));
            tHdr.Controls.Add(Hdr("STATUS", 486, 130));
            tHdr.Controls.Add(Hdr("SUBMITTED", 624, 130));
            Add(tHdr);

            DataTable recent = db.Query(
                @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS name,
                         jv.title AS job, a.status, a.submitted_at
                  FROM applications a
                  JOIN applicants ap    ON ap.id  = a.applicant_id
                  JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                  ORDER BY a.submitted_at DESC LIMIT 10");

            int rowTop = 234;
            foreach (DataRow row in recent.Rows)
            {
                Panel tr = AppRow(
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString(), row["job"].ToString(),
                    row["status"].ToString(),
                    row["submitted_at"] == DBNull.Value ? "—" : Convert.ToDateTime(row["submitted_at"]).ToString("MMM dd, yyyy"),
                    this.Width - 56);
                tr.Left = 28; tr.Top = rowTop;
                tr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                Add(tr); rowTop += tr.Height + 2;
            }
            if (recent.Rows.Count == 0)
            {
                Add(new Label { Text = "No applications yet.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = rowTop, AutoSize = true, BackColor = Color.Transparent });
                rowTop += 28;
            }

            // ── Upcoming Interviews ──────────────────────────────────
            Add(SectionLbl("UPCOMING INTERVIEWS", rowTop + 12));
            rowTop += 34;

            DataTable interviews = db.Query(
                @"SELECT CONCAT(ap.first_name,' ',ap.last_name) AS name,
                         jv.title AS job, is2.interview_type,
                         is2.scheduled_date, is2.scheduled_time, is2.mode, is2.interviewer
                  FROM interview_schedules is2
                  JOIN applications a   ON a.id  = is2.application_id
                  JOIN applicants ap    ON ap.id  = a.applicant_id
                  JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                  WHERE is2.status = 'Scheduled' AND is2.scheduled_date >= CURDATE()
                  ORDER BY is2.scheduled_date LIMIT 5");

            if (interviews.Rows.Count == 0)
            {
                Add(new Label { Text = "No upcoming interviews.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = rowTop, AutoSize = true, BackColor = Color.Transparent });
            }
            else
            {
                foreach (DataRow row in interviews.Rows)
                {
                    Panel ic = InterviewRow(
                        row["name"].ToString(), row["job"].ToString(),
                        row["interview_type"].ToString(),
                        Convert.ToDateTime(row["scheduled_date"]).ToString("MMM dd, yyyy") + "  " + row["scheduled_time"],
                        row["mode"].ToString(), row["interviewer"].ToString(), this.Width - 56);
                    ic.Left = 28; ic.Top = rowTop;
                    ic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    Add(ic); rowTop += ic.Height + 6;
                }
            }

            // ── Missing docs summary ─────────────────────────────────
            Add(SectionLbl("APPLICANTS WITH MISSING DOCUMENTS", rowTop + 12));
            rowTop += 34;

            DataTable missing = db.Query(
                @"SELECT CONCAT(ap.first_name,' ',ap.last_name) AS name,
                         jv.title AS job, a.status,
                         COUNT(ad.id) AS missing_count
                  FROM applications a
                  JOIN applicants ap    ON ap.id  = a.applicant_id
                  JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                  JOIN applicant_documents ad ON ad.application_id = a.id AND ad.status = 'Missing'
                  WHERE a.status NOT IN ('Draft','Withdrawn','Rejected')
                  GROUP BY a.id
                  ORDER BY missing_count DESC LIMIT 5");

            if (missing.Rows.Count == 0)
            {
                Add(new Label { Text = "No missing documents.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(60, 180, 100), Left = 28, Top = rowTop, AutoSize = true, BackColor = Color.Transparent });
            }
            else
            {
                foreach (DataRow row in missing.Rows)
                {
                    Panel mr = MissingRow(row["name"].ToString(), row["job"].ToString(), row["status"].ToString(), Convert.ToInt32(row["missing_count"]), this.Width - 56);
                    mr.Left = 28; mr.Top = rowTop;
                    mr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    Add(mr); rowTop += mr.Height + 4;
                }
            }
        }

        // ── UI Helpers ───────────────────────────────────────────────
        private void Add(Control c) => this.Controls.Add(c);

        private Panel StatCard(string title, string value, Color accent)
        {
            Panel card = new Panel { Height = 88, BackColor = Color.FromArgb(24, 30, 26) };
            card.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1); p.Dispose(); };
            card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 88, BackColor = accent });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = accent, Left = 14, Top = 10, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 120, 110), Left = 14, Top = 54, AutoSize = true, BackColor = Color.Transparent });
            return card;
        }

        private Label SectionLbl(string t, int top) => new Label { Text = t, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent };

        private Panel HdrPanel(int left, int top) => new Panel { Left = left, Top = top, Width = this.Width - 56, Height = 28, BackColor = Color.FromArgb(26, 34, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

        private Label Hdr(string t, int l, int w) => new Label { Text = t, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 110, 95), Left = l, Top = 7, Width = w, BackColor = Color.Transparent };

        private Panel AppRow(int id, string name, string job, string status, string date, int width)
        {
            Color sc = StatusColor(status);
            Panel row = new Panel { Width = width, Height = 40, BackColor = Color.FromArgb(22, 28, 24) };
            row.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(32, 45, 38), 1); e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1); p.Dispose(); };
            row.Controls.Add(new Label { Text = "APP-" + id.ToString("D4"), Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(140, 160, 150), Left = 12, Top = 11, Width = 80, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(210, 225, 218), Left = 100, Top = 11, Width = 180, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = job, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(140, 160, 150), Left = 288, Top = 11, Width = 190, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = status, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = sc, BackColor = Color.FromArgb(24, 28, 26), Width = 120, Height = 20, TextAlign = ContentAlignment.MiddleCenter, Left = 486, Top = 10 });
            row.Controls.Add(new Label { Text = date, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(110, 130, 120), Left = 624, Top = 11, Width = 130, BackColor = Color.Transparent });
            return row;
        }

        private Panel InterviewRow(string name, string job, string type, string when, string mode, string interviewer, int width)
        {
            Panel row = new Panel { Width = width, Height = 56, BackColor = Color.FromArgb(22, 32, 44) };
            row.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(35, 60, 90), 1); e.Graphics.DrawRectangle(p, 0, 0, row.Width - 1, row.Height - 1); p.Dispose(); };
            row.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 56, BackColor = Color.FromArgb(80, 160, 220) });
            row.Controls.Add(new Label { Text = name + " — " + job, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(180, 210, 235), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = type + "   |   " + when + "   |   " + mode + "   |   " + interviewer, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(110, 145, 170), Left = 16, Top = 32, AutoSize = true, BackColor = Color.Transparent });
            return row;
        }

        private Panel MissingRow(string name, string job, string status, int count, int width)
        {
            Panel row = new Panel { Width = width, Height = 40, BackColor = Color.FromArgb(36, 24, 24) };
            row.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(60, 35, 35), 1); e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1); p.Dispose(); };
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(210, 200, 200), Left = 12, Top = 11, Width = 200, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = job, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(140, 130, 130), Left = 220, Top = 11, Width = 200, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = count + " missing doc(s)", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 80, 80), Left = 428, Top = 11, AutoSize = true, BackColor = Color.Transparent });
            return row;
        }

        private Color StatusColor(string s)
        {
            if (s == "Submitted") return Color.FromArgb(80, 160, 220);
            if (s == "Under Review") return Color.FromArgb(230, 160, 50);
            if (s == "Shortlisted") return Color.FromArgb(80, 200, 160);
            if (s == "For Interview") return Color.FromArgb(140, 100, 220);
            if (s == "For Assessment") return Color.FromArgb(200, 130, 60);
            if (s == "For Final Review") return Color.FromArgb(160, 200, 80);
            if (s == "Accepted") return Color.FromArgb(60, 200, 100);
            if (s == "Rejected") return Color.FromArgb(220, 80, 80);
            if (s == "Withdrawn") return Color.FromArgb(160, 100, 80);
            return Color.FromArgb(150, 150, 150);
        }

        private int ToInt(object v) => v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);

        private void DashboardForm_Load(object sender, EventArgs e) { }
    }
}