using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using project;

namespace HRApplicant
{
    public static class ApplicantStatusTrackingPage
    {
        
        private static readonly string[] AllStatuses =
        {
            "Draft","Submitted","Under Review","Shortlisted",
            "For Interview","For Assessment","For Final Review","Accepted"
        };

        private static readonly Color[] StepColors =
        {
            Color.FromArgb(130, 130, 160),
            Color.FromArgb(80, 160, 220),
            Color.FromArgb(230, 160, 50),
            Color.FromArgb(80, 200, 160),
            Color.FromArgb(140, 100, 220),
            Color.FromArgb(200, 130, 60),
            Color.FromArgb(160, 200, 80),
            Color.FromArgb(60, 200, 100)
        };

        public static void Show(ApplicantMainForm main)
        {
            var db = new DatabaseConnection();
            main.ClearContent();
            var p = main.contentPanel;

            p.Controls.Add(new Label { Text = "Application Status", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label { Text = "Track your application progress and view HR feedback.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 46, AutoSize = true, BackColor = Color.Transparent });

            // Load application
            DataTable appDt = db.Query(
                @"SELECT a.id, a.status, a.submitted_at, jv.title AS job_title
                  FROM applications a
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  WHERE a.applicant_id = @aid ORDER BY a.created_at DESC LIMIT 1",
                ("@aid", Session.ApplicantId));

            if (appDt.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "No application found.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = 76, AutoSize = true, BackColor = Color.Transparent });
                return;
            }

            DataRow app = appDt.Rows[0];
            int appId = Convert.ToInt32(app["id"]);
            string status = app["status"].ToString();
            main.applicationStatus = status;

            // ── Current status card ──────────────────────────────────────
            Color curColor = ApplicantDashboardPage.GetStatusColor(status);
            Panel curCard = new Panel { Left = 28, Top = 74, Width = p.Width - 56, Height = 68, BackColor = Color.FromArgb(28, 36, 30), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            curCard.Paint += (s, e) => { Pen pen = new Pen(curColor, 1); e.Graphics.DrawRectangle(pen, 0, 0, curCard.Width - 1, curCard.Height - 1); pen.Dispose(); };
            curCard.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 68, BackColor = curColor });
            curCard.Controls.Add(new Label { Text = "CURRENT STATUS", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 120, 95), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
            curCard.Controls.Add(new Label { Text = status, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = curColor, Left = 16, Top = 28, AutoSize = true, BackColor = Color.Transparent });
            curCard.Controls.Add(new Label { Text = "Job: " + app["job_title"] + "   |   App ID: APP-" + appId.ToString("D4"), Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 120, 110), Left = 16, Top = 50, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(curCard);

            // ── Stepper ──────────────────────────────────────────────────
            p.Controls.Add(new Label { Text = "APPLICATION FLOW", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = 154, AutoSize = true, BackColor = Color.Transparent });

            int currentIdx = Array.IndexOf(AllStatuses, status);

            Panel stepPanel = new Panel
            {
                Left = 28,
                Top = 174,
                Width = p.Width - 56,
                Height = 68,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            stepPanel.Paint += (s, e) =>
            {
                int dotSz = 14, total = AllStatuses.Length;
                int stepW = (stepPanel.Width - 40) / (total - 1);
                int lineY = 20;

                for (int i = 0; i < total - 1; i++)
                {
                    int x1 = 20 + i * stepW + dotSz / 2;
                    int x2 = 20 + (i + 1) * stepW - dotSz / 2;
                    Color lc = i < currentIdx ? Color.FromArgb(60, 160, 90) : Color.FromArgb(40, 55, 46);
                    Pen pen = new Pen(lc, 2);
                    e.Graphics.DrawLine(pen, x1, lineY, x2, lineY);
                    pen.Dispose();
                }
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                for (int i = 0; i < total; i++)
                {
                    int cx = 20 + i * stepW;
                    Color dc = i <= currentIdx ? StepColors[i] : Color.FromArgb(38, 50, 42);
                    SolidBrush fb = new SolidBrush(dc);
                    e.Graphics.FillEllipse(fb, cx, lineY - dotSz / 2, dotSz, dotSz);
                    fb.Dispose();
                    Pen bp = new Pen(i == currentIdx ? StepColors[i] : Color.FromArgb(40, 55, 46), i == currentIdx ? 2 : 1);
                    e.Graphics.DrawEllipse(bp, cx, lineY - dotSz / 2, dotSz, dotSz);
                    bp.Dispose();

                    string lbl = AllStatuses[i].Replace(" ", "\n");
                    Font sf = new Font("Segoe UI", 6.5f, i == currentIdx ? FontStyle.Bold : FontStyle.Regular);
                    SolidBrush tb = new SolidBrush(i <= currentIdx ? Color.FromArgb(170, 195, 182) : Color.FromArgb(70, 90, 80));
                    StringFormat sff = new StringFormat { Alignment = StringAlignment.Center };
                    e.Graphics.DrawString(lbl, sf, tb, new RectangleF(cx - 24, lineY + 12, 62, 44), sff);
                    sf.Dispose(); tb.Dispose();
                }
            };
            p.Controls.Add(stepPanel);

            // ── Status History ───────────────────────────────────────────
            p.Controls.Add(new Label { Text = "STATUS HISTORY", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = 252, AutoSize = true, BackColor = Color.Transparent });

            DataTable hist = db.Query(
                "SELECT status, remarks, changed_by, changed_at FROM application_status_history WHERE application_id=@id ORDER BY changed_at DESC",
                ("@id", appId));

            int timeTop = 274;
            if (hist.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "No history yet.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = timeTop, AutoSize = true, BackColor = Color.Transparent });
                timeTop += 28;
            }
            else
            {
                foreach (DataRow row in hist.Rows)
                {
                    Panel tRow = TimelineRow(
                        row["status"].ToString(),
                        row["remarks"] == DBNull.Value ? "" : row["remarks"].ToString(),
                        Convert.ToDateTime(row["changed_at"]).ToString("MMM dd, yyyy h:mm tt"),
                        row["changed_by"] == DBNull.Value ? "" : row["changed_by"].ToString(),
                        ApplicantDashboardPage.GetStatusColor(row["status"].ToString()),
                        p.Width - 56);
                    tRow.Left = 28; tRow.Top = timeTop;
                    tRow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    p.Controls.Add(tRow);
                    timeTop += tRow.Height + 4;
                }
            }

            // ── HR Remarks ───────────────────────────────────────────────
            p.Controls.Add(new Label { Text = "HR REMARKS", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = timeTop + 10, AutoSize = true, BackColor = Color.Transparent });

            // Get latest screening remark
            DataTable scrn = db.Query(
                "SELECT remarks FROM screening_results WHERE application_id=@id ORDER BY screened_at DESC LIMIT 1",
                ("@id", appId));
            // Get latest hiring decision remark
            DataTable hire = db.Query(
                "SELECT decision, remarks FROM hiring_decisions WHERE application_id=@id ORDER BY decided_at DESC LIMIT 1",
                ("@id", appId));

            string hrRemark = "No remarks from HR yet.";
            if (hire.Rows.Count > 0 && hire.Rows[0]["remarks"] != DBNull.Value && hire.Rows[0]["remarks"].ToString().Length > 0)
                hrRemark = "[" + hire.Rows[0]["decision"] + "] " + hire.Rows[0]["remarks"];
            else if (scrn.Rows.Count > 0 && scrn.Rows[0]["remarks"] != DBNull.Value)
                hrRemark = scrn.Rows[0]["remarks"].ToString();

            Panel remarkCard = new Panel { Left = 28, Top = timeTop + 30, Width = p.Width - 56, Height = 60, BackColor = Color.FromArgb(24, 32, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            remarkCard.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, remarkCard.Width - 1, remarkCard.Height - 1); pen.Dispose(); };
            remarkCard.Controls.Add(new Label { Text = hrRemark, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(180, 200, 190), Left = 14, Top = 10, Width = p.Width - 86, Height = 40, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
            p.Controls.Add(remarkCard);

            // ── Interview schedule ───────────────────────────────────────
            p.Controls.Add(new Label { Text = "INTERVIEW SCHEDULE", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = timeTop + 104, AutoSize = true, BackColor = Color.Transparent });

            DataTable sched = db.Query(
                @"SELECT interview_type, scheduled_date, scheduled_time, mode, location, interviewer, status
                  FROM interview_schedules WHERE application_id=@id ORDER BY scheduled_date DESC",
                ("@id", appId));

            int schedTop = timeTop + 124;
            if (sched.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "No interview scheduled yet.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = schedTop, AutoSize = true, BackColor = Color.Transparent });
            }
            else
            {
                foreach (DataRow row in sched.Rows)
                {
                    Panel ic = new Panel { Left = 28, Top = schedTop, Width = p.Width - 56, Height = 66, BackColor = Color.FromArgb(22, 32, 44), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                    ic.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 60, 90), 1); e.Graphics.DrawRectangle(pen, 0, 0, ic.Width - 1, ic.Height - 1); pen.Dispose(); };
                    ic.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 66, BackColor = Color.FromArgb(80, 160, 220) });
                    ic.Controls.Add(new Label { Text = row["interview_type"].ToString() + "  [" + row["status"] + "]", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(140, 190, 230), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                    ic.Controls.Add(new Label { Text = Convert.ToDateTime(row["scheduled_date"]).ToString("MMM dd, yyyy") + "  " + row["scheduled_time"] + "   |   " + row["mode"] + " — " + row["location"], Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(110, 145, 170), Left = 16, Top = 30, AutoSize = true, BackColor = Color.Transparent });
                    ic.Controls.Add(new Label { Text = "Interviewer: " + row["interviewer"], Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 130, 150), Left = 16, Top = 48, AutoSize = true, BackColor = Color.Transparent });
                    p.Controls.Add(ic);
                    schedTop += 74;
                }
            }
        }

        private static Panel TimelineRow(string status, string note, string date, string changedBy, Color color, int width)
        {
            Panel row = new Panel { Width = width, Height = 52, BackColor = Color.FromArgb(22, 28, 24) };
            row.Paint += (s, e) =>
            {
                Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1);
                e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
                pen.Dispose();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                SolidBrush b = new SolidBrush(color);
                e.Graphics.FillEllipse(b, 12, 20, 10, 10); b.Dispose();
            };
            row.Controls.Add(new Label { Text = status, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = color, Left = 34, Top = 6, AutoSize = true, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = note, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(160, 180, 170), Left = 34, Top = 26, Width = width - 200, AutoSize = false, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = date + (changedBy.Length > 0 ? " — " + changedBy : ""), Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(90, 110, 100), Width = 160, TextAlign = ContentAlignment.TopRight, Left = width - 170, Top = 8, BackColor = Color.Transparent });
            return row;
        }
    }
}