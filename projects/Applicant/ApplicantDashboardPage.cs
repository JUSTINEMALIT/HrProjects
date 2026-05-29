using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HRApplicant
{
    public static class ApplicantDashboardPage
    {
        public static void Show(ApplicantMainForm main)
        {
            var db = new DatabaseConnection();
            main.ClearContent();
            var p = main.contentPanel;

            // ── Load data ────────────────────────────────────────────────
            DataTable appRow = db.Query(
                @"SELECT a.id, a.status, a.submitted_at, jv.title AS job_title, jv.employment_type,
                         d.name AS department, a.created_at
                  FROM applications a
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  JOIN departments d   ON d.id   = jv.department_id
                  WHERE a.applicant_id = @aid
                  ORDER BY a.created_at DESC LIMIT 1",
                ("@aid", Session.ApplicantId));

            int appId = 0;
            string appStatus = "No Application";
            string jobTitle = "—";
            string submittedAt = "—";

            if (appRow.Rows.Count > 0)
            {
                appId = Convert.ToInt32(appRow.Rows[0]["id"]);
                appStatus = appRow.Rows[0]["status"].ToString();
                jobTitle = appRow.Rows[0]["job_title"] + " — " + appRow.Rows[0]["department"];
                submittedAt = appRow.Rows[0]["submitted_at"] == DBNull.Value
                    ? "Not yet submitted"
                    : Convert.ToDateTime(appRow.Rows[0]["submitted_at"]).ToString("MMM dd, yyyy");
                main.applicationStatus = appStatus;
            }

            int missingCount = 0;
            if (appId > 0)
            {
                object mc = db.Scalar(
                    "SELECT COUNT(*) FROM applicant_documents WHERE application_id=@aid AND status='Missing'",
                    ("@aid", appId));
                missingCount = Convert.ToInt32(mc);
            }

            // Days since applied
            int daysSince = 0;
            if (appRow.Rows.Count > 0 && appRow.Rows[0]["created_at"] != DBNull.Value)
                daysSince = (int)(DateTime.Now - Convert.ToDateTime(appRow.Rows[0]["created_at"])).TotalDays;

            // ── Page header ──────────────────────────────────────────────
            p.Controls.Add(new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 235, 228),
                Left = 28,
                Top = 18,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            p.Controls.Add(new Label
            {
                Text = "Welcome back, " + project.Session.FullName + ". Here's your application summary.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 130, 115),
                Left = 29,
                Top = 46,
                AutoSize = true,
                BackColor = Color.Transparent
            });

            // ── Stat cards ───────────────────────────────────────────────
            string[] titles = { "Application Status", "Missing Documents", "Submitted", "Days Since Applied" };
            string[] values = { appStatus, missingCount.ToString(), submittedAt, daysSince.ToString() };
            Color[] accents = {
                GetStatusColor(appStatus),
                missingCount > 0 ? Color.FromArgb(220, 80, 80) : Color.FromArgb(60, 180, 100),
                Color.FromArgb(80, 160, 220),
                Color.FromArgb(80, 200, 130)
            };
            int cw = 188, cg = 14;
            for (int i = 0; i < titles.Length; i++)
            {
                var card = StatCard(titles[i], values[i], accents[i]);
                card.Left = 28 + i * (cw + cg);
                card.Top = 76;
                card.Width = cw;
                p.Controls.Add(card);
            }

            // ── Applied for ──────────────────────────────────────────────
            p.Controls.Add(SectionLabel("APPLIED FOR", 178));
            Panel jobBanner = new Panel
            {
                Left = 28,
                Top = 200,
                Width = p.Width - 56,
                Height = 48,
                BackColor = Color.FromArgb(22, 32, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            jobBanner.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 60, 44), 1); e.Graphics.DrawRectangle(pen, 0, 0, jobBanner.Width - 1, jobBanner.Height - 1); pen.Dispose(); };
            jobBanner.Controls.Add(new Label { Text = jobTitle, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 230, 215), Left = 14, Top = 6, AutoSize = true, BackColor = Color.Transparent });
            jobBanner.Controls.Add(new Label { Text = "Application ID: APP-" + appId.ToString("D4"), Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 130, 115), Left = 14, Top = 28, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(jobBanner);

            // ── Missing documents ────────────────────────────────────────
            p.Controls.Add(SectionLabel("MISSING REQUIREMENTS", 262));
            int top = 284;
            if (appId > 0)
            {
                DataTable missing = db.Query(
                    @"SELECT rt.name, ad.hr_remarks
                      FROM applicant_documents ad
                      JOIN requirement_types rt ON rt.id = ad.requirement_type_id
                      WHERE ad.application_id = @aid AND ad.status = 'Missing'",
                    ("@aid", appId));

                if (missing.Rows.Count == 0)
                {
                    p.Controls.Add(new Label { Text = "✔  No missing documents.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(60, 180, 100), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                    top += 28;
                }
                else
                {
                    foreach (DataRow row in missing.Rows)
                    {
                        Panel mr = MissingRow(row["name"].ToString(), row["hr_remarks"].ToString(), p.Width - 56);
                        mr.Left = 28; mr.Top = top;
                        mr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        p.Controls.Add(mr);
                        top += mr.Height + 4;
                    }
                }
            }
            else
            {
                p.Controls.Add(new Label { Text = "No application yet. Go to Job Vacancies to apply.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                top += 28;
            }

            // ── Interview schedule ───────────────────────────────────────
            p.Controls.Add(SectionLabel("UPCOMING INTERVIEW", top + 12));
            top += 34;
            if (appId > 0)
            {
                DataTable sched = db.Query(
                    @"SELECT interview_type, scheduled_date, scheduled_time, mode, location, interviewer
                      FROM interview_schedules
                      WHERE application_id = @aid AND status = 'Scheduled' AND scheduled_date >= CURDATE()
                      ORDER BY scheduled_date LIMIT 1",
                    ("@aid", appId));

                if (sched.Rows.Count > 0)
                {
                    var r = sched.Rows[0];
                    Panel ic = InterviewCard(
                        r["interview_type"].ToString(),
                        Convert.ToDateTime(r["scheduled_date"]).ToString("MMM dd, yyyy") + "  " + r["scheduled_time"],
                        r["mode"] + " — " + r["location"],
                        r["interviewer"].ToString(), p.Width - 56);
                    ic.Left = 28; ic.Top = top;
                    ic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    p.Controls.Add(ic);
                    top += ic.Height + 4;
                }
                else
                {
                    p.Controls.Add(new Label { Text = "No upcoming interview scheduled.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                    top += 28;
                }
            }

            // ── Recent updates ───────────────────────────────────────────
            p.Controls.Add(SectionLabel("RECENT UPDATES", top + 12));
            top += 34;
            if (appId > 0)
            {
                DataTable hist = db.Query(
                    @"SELECT status, remarks, changed_by, changed_at
                      FROM application_status_history
                      WHERE application_id = @aid
                      ORDER BY changed_at DESC LIMIT 5",
                    ("@aid", appId));

                foreach (DataRow row in hist.Rows)
                {
                    Panel ur = UpdateRow(
                        row["status"].ToString(),
                        row["remarks"] != DBNull.Value ? row["remarks"].ToString() : "",
                        Convert.ToDateTime(row["changed_at"]).ToString("MMM dd, yyyy"),
                        GetStatusColor(row["status"].ToString()), p.Width - 56);
                    ur.Left = 28; ur.Top = top;
                    ur.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    p.Controls.Add(ur);
                    top += ur.Height + 4;
                }
            }
        }

        // ── UI helpers ───────────────────────────────────────────────────

        private static Panel StatCard(string title, string value, Color accent)
        {
            Panel card = new Panel { Height = 88, BackColor = Color.FromArgb(24, 30, 26) };
            card.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); pen.Dispose(); };
            card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 88, BackColor = accent });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(225, 235, 230), Left = 14, Top = 16, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 120, 110), Left = 14, Top = 50, AutoSize = true, BackColor = Color.Transparent });
            return card;
        }

        private static Label SectionLabel(string text, int top)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent };
        }

        private static Panel MissingRow(string name, string note, int width)
        {
            Panel row = new Panel { Width = width, Height = 44, BackColor = Color.FromArgb(38, 24, 24) };
            row.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(70, 35, 35), 1); e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1); pen.Dispose(); };
            row.Controls.Add(new Panel { Left = 14, Top = 16, Width = 8, Height = 8, BackColor = Color.FromArgb(220, 80, 80) });
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 200, 200), Left = 34, Top = 8, AutoSize = true, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = note, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(130, 110, 110), Left = 34, Top = 26, AutoSize = true, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = "MISSING", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 80, 80), BackColor = Color.FromArgb(50, 30, 30), Width = 64, Height = 20, TextAlign = ContentAlignment.MiddleCenter, Left = width - 80, Top = 12, Anchor = AnchorStyles.Top | AnchorStyles.Right });
            return row;
        }

        private static Panel InterviewCard(string type, string when, string where, string by, int width)
        {
            Panel card = new Panel { Width = width, Height = 72, BackColor = Color.FromArgb(22, 32, 44) };
            card.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 60, 90), 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); pen.Dispose(); };
            card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 72, BackColor = Color.FromArgb(80, 160, 220) });
            card.Controls.Add(new Label { Text = type, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(160, 200, 240), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = when + "   |   " + where, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 150, 170), Left = 16, Top = 30, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = "Interviewer: " + by, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 130, 150), Left = 16, Top = 50, AutoSize = true, BackColor = Color.Transparent });
            return card;
        }

        private static Panel UpdateRow(string status, string note, string date, Color color, int width)
        {
            Panel row = new Panel { Width = width, Height = 46, BackColor = Color.FromArgb(22, 28, 24) };
            row.Paint += (s, e) =>
            {
                Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1);
                e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
                pen.Dispose();
                System.Drawing.Drawing2D.SmoothingMode sm = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                SolidBrush b = new SolidBrush(color);
                e.Graphics.FillEllipse(b, 10, 18, 8, 8);
                b.Dispose();
                e.Graphics.SmoothingMode = sm;
            };
            row.Controls.Add(new Label { Text = status, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = color, Left = 30, Top = 6, AutoSize = true, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = note, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(160, 180, 170), Left = 30, Top = 26, Width = width - 150, AutoSize = false, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = date, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(90, 110, 100), Width = 110, TextAlign = ContentAlignment.TopRight, Left = width - 120, Top = 8, BackColor = Color.Transparent });
            return row;
        }

        public static Color GetStatusColor(string status)
        {
            if (status == "Draft") return Color.FromArgb(130, 130, 160);
            if (status == "Submitted") return Color.FromArgb(80, 160, 220);
            if (status == "Under Review") return Color.FromArgb(230, 160, 50);
            if (status == "Shortlisted") return Color.FromArgb(80, 200, 160);
            if (status == "For Interview") return Color.FromArgb(140, 100, 220);
            if (status == "For Assessment") return Color.FromArgb(200, 130, 60);
            if (status == "For Final Review") return Color.FromArgb(160, 200, 80);
            if (status == "Accepted") return Color.FromArgb(60, 200, 100);
            if (status == "Rejected") return Color.FromArgb(220, 80, 80);
            if (status == "Withdrawn") return Color.FromArgb(160, 100, 80);
            return Color.FromArgb(150, 150, 150);
        }
    }
}