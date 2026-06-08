using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HRApplicant
{
    public class ApplicantStatusTrackingPage : Form
    {
        private ApplicantMainForm mainForm;
        private Panel contentPanel;

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

        public ApplicantStatusTrackingPage(ApplicantMainForm main)
        {
            this.mainForm = main;
            this.contentPanel = main.contentPanel;
            InitializePage();
        }

        private void InitializePage()
        {
            mainForm.ClearContent();
            var p = contentPanel;
            var db = new DatabaseConnection();

            p.Controls.Add(new Label { Text = "Application Status", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });

            DataTable allApps = db.Query(
                @"SELECT a.id, a.status, jv.title AS job_title, d.name AS department
                  FROM applications a
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  JOIN departments d    ON d.id  = jv.department_id
                  WHERE a.applicant_id = @aid
                  ORDER BY a.created_at DESC",
                ("@aid", project.Session.ApplicantId));

            if (allApps.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "No application found.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = 60, AutoSize = true, BackColor = Color.Transparent });
                return;
            }

            // ── Job selector dropdown ────────────────────────────────
            p.Controls.Add(new Label { Text = "Select Application:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 130, 115), Left = 28, Top = 46, AutoSize = true, BackColor = Color.Transparent });

            ComboBox cmbJobs = new ComboBox
            {
                Left = 170,
                Top = 42,
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            foreach (DataRow r in allApps.Rows)
                cmbJobs.Items.Add(r["job_title"] + " — " + r["department"] + "  [" + r["status"] + "]|" + r["id"]);
            cmbJobs.SelectedIndex = 0;
            p.Controls.Add(cmbJobs);

            Panel trackContent = new Panel
            {
                Left = 0,
                Top = 78,
                Width = p.Width,
                Height = p.Height - 78,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            p.Controls.Add(trackContent);

            Action<int> loadTracking = null;
            loadTracking = (appId) =>
            {
                trackContent.Controls.Clear();

                DataTable appDt = db.Query(
                    @"SELECT a.id, a.status, a.submitted_at, jv.title AS job_title
                      FROM applications a
                      JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                      WHERE a.id = @id",
                    ("@id", appId));

                if (appDt.Rows.Count == 0) return;

                DataRow app = appDt.Rows[0];
                string status = app["status"].ToString();
                mainForm.applicationStatus = status;

                int top = 10;

                // ── Current status card ──────────────────────────────
                Color curColor = GetStatusColor(status);
                Panel curCard = new Panel { Left = 28, Top = top, Width = trackContent.Width - 56, Height = 68, BackColor = Color.FromArgb(28, 36, 30), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                curCard.Paint += (s, e) => { Pen pen = new Pen(curColor, 1); e.Graphics.DrawRectangle(pen, 0, 0, curCard.Width - 1, curCard.Height - 1); pen.Dispose(); };
                curCard.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 68, BackColor = curColor });
                curCard.Controls.Add(new Label { Text = "CURRENT STATUS", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 120, 95), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                curCard.Controls.Add(new Label { Text = status, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = curColor, Left = 16, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                curCard.Controls.Add(new Label { Text = "Job: " + app["job_title"] + "   |   App ID: APP-" + appId.ToString("D4"), Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 120, 110), Left = 16, Top = 50, AutoSize = true, BackColor = Color.Transparent });
                trackContent.Controls.Add(curCard);
                top += 78;

                // ── Stepper ──────────────────────────────────────────
                trackContent.Controls.Add(new Label { Text = "APPLICATION FLOW", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                top += 22;

                int currentIdx = Array.IndexOf(AllStatuses, status);
                Panel stepPanel = new Panel
                {
                    Left = 28,
                    Top = top,
                    Width = trackContent.Width - 56,
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
                        Pen pen = new Pen(lc, 2); e.Graphics.DrawLine(pen, x1, lineY, x2, lineY); pen.Dispose();
                    }
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    for (int i = 0; i < total; i++)
                    {
                        int cx = 20 + i * stepW;
                        Color dc = i <= currentIdx ? StepColors[i] : Color.FromArgb(38, 50, 42);
                        SolidBrush fb = new SolidBrush(dc); e.Graphics.FillEllipse(fb, cx, lineY - dotSz / 2, dotSz, dotSz); fb.Dispose();
                        Pen bp = new Pen(i == currentIdx ? StepColors[i] : Color.FromArgb(40, 55, 46), i == currentIdx ? 2 : 1);
                        e.Graphics.DrawEllipse(bp, cx, lineY - dotSz / 2, dotSz, dotSz); bp.Dispose();
                        string lbl = AllStatuses[i].Replace(" ", "\n");
                        Font sf = new Font("Segoe UI", 6.5f, i == currentIdx ? FontStyle.Bold : FontStyle.Regular);
                        SolidBrush tb = new SolidBrush(i <= currentIdx ? Color.FromArgb(170, 195, 182) : Color.FromArgb(70, 90, 80));
                        StringFormat sff = new StringFormat { Alignment = StringAlignment.Center };
                        e.Graphics.DrawString(lbl, sf, tb, new System.Drawing.RectangleF(cx - 24, lineY + 12, 62, 44), sff);
                        sf.Dispose(); tb.Dispose();
                    }
                };
                trackContent.Controls.Add(stepPanel);
                top += 78;

                // ── Status History ────────────────────────────────────
                trackContent.Controls.Add(new Label { Text = "STATUS HISTORY", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                top += 22;

                DataTable hist = db.Query(
                    "SELECT status, remarks, changed_by, changed_at FROM application_status_history WHERE application_id=@id ORDER BY changed_at DESC",
                    ("@id", appId));

                if (hist.Rows.Count == 0)
                {
                    trackContent.Controls.Add(new Label { Text = "No history yet.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                    top += 28;
                }
                else
                {
                    foreach (DataRow row in hist.Rows)
                    {
                        Color sc = GetStatusColor(row["status"].ToString());
                        Panel tRow = new Panel { Left = 28, Top = top, Width = trackContent.Width - 56, Height = 52, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                        tRow.Paint += (s, e) =>
                        {
                            Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1);
                            e.Graphics.DrawLine(pen, 0, tRow.Height - 1, tRow.Width, tRow.Height - 1); pen.Dispose();
                            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            SolidBrush bru = new SolidBrush(sc); e.Graphics.FillEllipse(bru, 12, 20, 10, 10); bru.Dispose();
                        };
                        tRow.Controls.Add(new Label { Text = row["status"].ToString(), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = sc, Left = 34, Top = 6, AutoSize = true, BackColor = Color.Transparent });
                        tRow.Controls.Add(new Label { Text = row["remarks"] == DBNull.Value ? "" : row["remarks"].ToString(), Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(160, 180, 170), Left = 34, Top = 28, Width = trackContent.Width - 220, AutoSize = false, BackColor = Color.Transparent });
                        tRow.Controls.Add(new Label { Text = Convert.ToDateTime(row["changed_at"]).ToString("MMM dd, yyyy h:mm tt") + (row["changed_by"] != DBNull.Value ? " — " + row["changed_by"] : ""), Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(90, 110, 100), Width = 260, TextAlign = ContentAlignment.TopRight, Left = trackContent.Width - 310, Top = 8, BackColor = Color.Transparent });
                        trackContent.Controls.Add(tRow);
                        top += tRow.Height + 2;
                    }
                }

                // ── HR Remarks ────────────────────────────────────────
                trackContent.Controls.Add(new Label { Text = "HR REMARKS", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top + 10, AutoSize = true, BackColor = Color.Transparent });

                DataTable scrn = db.Query("SELECT remarks FROM screening_results WHERE application_id=@id ORDER BY screened_at DESC LIMIT 1", ("@id", appId));
                DataTable hire = db.Query("SELECT decision, remarks FROM hiring_decisions WHERE application_id=@id ORDER BY decided_at DESC LIMIT 1", ("@id", appId));

                string hrRemark = "No remarks from HR yet.";
                if (hire.Rows.Count > 0 && hire.Rows[0]["remarks"] != DBNull.Value && hire.Rows[0]["remarks"].ToString().Length > 0)
                    hrRemark = "[" + hire.Rows[0]["decision"] + "] " + hire.Rows[0]["remarks"];
                else if (scrn.Rows.Count > 0 && scrn.Rows[0]["remarks"] != DBNull.Value)
                    hrRemark = scrn.Rows[0]["remarks"].ToString();

                Panel remarkCard = new Panel { Left = 28, Top = top + 30, Width = trackContent.Width - 56, Height = 60, BackColor = Color.FromArgb(24, 32, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                remarkCard.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, remarkCard.Width - 1, remarkCard.Height - 1); pen.Dispose(); };
                remarkCard.Controls.Add(new Label { Text = hrRemark, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(180, 200, 190), Left = 14, Top = 10, Width = trackContent.Width - 86, Height = 40, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
                trackContent.Controls.Add(remarkCard);
                top += 100;

                // ── Interview schedule ────────────────────────────────
                trackContent.Controls.Add(new Label { Text = "INTERVIEW SCHEDULE", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                top += 22;

                DataTable sched = db.Query(
                    @"SELECT interview_type, scheduled_date, scheduled_time, mode, location, interviewer, status
                      FROM interview_schedules WHERE application_id=@id ORDER BY scheduled_date DESC",
                    ("@id", appId));

                if (sched.Rows.Count == 0)
                {
                    trackContent.Controls.Add(new Label { Text = "No interview scheduled yet.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                }
                else
                {
                    foreach (DataRow row in sched.Rows)
                    {
                        Color sc = row["status"].ToString() == "Scheduled" ? Color.FromArgb(80, 160, 220) : row["status"].ToString() == "Completed" ? Color.FromArgb(60, 200, 100) : Color.FromArgb(180, 80, 80);
                        Panel ic = new Panel { Left = 28, Top = top, Width = trackContent.Width - 56, Height = 66, BackColor = Color.FromArgb(22, 32, 44), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                        ic.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 60, 90), 1); e.Graphics.DrawRectangle(pen, 0, 0, ic.Width - 1, ic.Height - 1); pen.Dispose(); };
                        ic.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 66, BackColor = sc });
                        ic.Controls.Add(new Label { Text = row["interview_type"].ToString() + "  [" + row["status"] + "]", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(140, 190, 230), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                        ic.Controls.Add(new Label { Text = Convert.ToDateTime(row["scheduled_date"]).ToString("MMM dd, yyyy") + "  " + row["scheduled_time"] + "   |   " + row["mode"] + " — " + row["location"], Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(110, 145, 170), Left = 16, Top = 30, AutoSize = true, BackColor = Color.Transparent });
                        ic.Controls.Add(new Label { Text = "Interviewer: " + row["interviewer"], Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 130, 150), Left = 16, Top = 48, AutoSize = true, BackColor = Color.Transparent });
                        trackContent.Controls.Add(ic);
                        top += 74;
                    }
                }
            };

            int firstAppId = Convert.ToInt32(allApps.Rows[0]["id"]);
            loadTracking(firstAppId);

            cmbJobs.SelectedIndexChanged += (s, e) =>
            {
                if (cmbJobs.SelectedIndex < 0) return;
                string selected = cmbJobs.SelectedItem.ToString();
                int selectedAppId = Convert.ToInt32(selected.Split('|')[1]);
                loadTracking(selectedAppId);
            };
        }

        private Color GetStatusColor(string status)
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
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantStatusTrackingPage
            // 
            ClientSize = new Size(284, 261);
            Name = "ApplicantStatusTrackingPage";
            Load += ApplicantStatusTrackingPage_Load;
            ResumeLayout(false);

        }

        private void ApplicantStatusTrackingPage_Load(object sender, EventArgs e)
        {

        }
    }
}