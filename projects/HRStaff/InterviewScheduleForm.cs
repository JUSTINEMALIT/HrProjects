using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class InterviewScheduleForm : Form
    {
        public InterviewScheduleForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.AutoScroll = true;

            var db = new DatabaseConnection();

            this.Controls.Add(new Label { Text = "Interview Schedule", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 20, AutoSize = true, BackColor = Color.Transparent });
            this.Controls.Add(new Label { Text = "Schedule and manage applicant interviews.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 48, AutoSize = true, BackColor = Color.Transparent });

            // ── Info banner ──────────────────────────────────────────
            Panel infoBanner = new Panel
            {
                Left = 28,
                Top = 68,
                Width = this.Width - 56,
                Height = 44,
                BackColor = Color.FromArgb(22, 36, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            infoBanner.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(40, 100, 65), 1); e.Graphics.DrawRectangle(p, 0, 0, infoBanner.Width - 1, infoBanner.Height - 1); p.Dispose(); };
            infoBanner.Controls.Add(new Label
            {
                Text = "ℹ  Only Shortlisted applicants appear in the dropdown. Flow: Submitted → Start Review → Screen (Qualified) → Shortlisted → Schedule here.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 200, 140),
                Left = 12,
                Top = 0,
                Height = 44,
                Width = this.Width - 80,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            this.Controls.Add(infoBanner);

            // ── Count badge ──────────────────────────────────────────
            int shortlistedCount = Convert.ToInt32(db.Scalar(
                "SELECT COUNT(*) FROM applications WHERE status = 'Shortlisted'"));

            Panel countBadge = new Panel
            {
                Left = 28,
                Top = 122,
                Width = 260,
                Height = 44,
                BackColor = shortlistedCount > 0 ? Color.FromArgb(22, 40, 28) : Color.FromArgb(35, 28, 22)
            };
            countBadge.Paint += (s, e) =>
            {
                Color bc = shortlistedCount > 0 ? Color.FromArgb(50, 120, 80) : Color.FromArgb(100, 80, 40);
                Pen p = new Pen(bc, 1); e.Graphics.DrawRectangle(p, 0, 0, countBadge.Width - 1, countBadge.Height - 1); p.Dispose();
            };
            countBadge.Controls.Add(new Label
            {
                Text = shortlistedCount > 0
                    ? $"✔  {shortlistedCount} shortlisted applicant(s) available"
                    : "⚠  No shortlisted applicants yet",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = shortlistedCount > 0 ? Color.FromArgb(80, 200, 130) : Color.FromArgb(220, 160, 60),
                Left = 12,
                Top = 0,
                Height = 44,
                Width = 240,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            });
            this.Controls.Add(countBadge);

            if (shortlistedCount == 0)
            {
                Panel guideCard = new Panel
                {
                    Left = 28,
                    Top = 178,
                    Width = this.Width - 56,
                    Height = 150,
                    BackColor = Color.FromArgb(24, 30, 26),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                guideCard.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(p, 0, 0, guideCard.Width - 1, guideCard.Height - 1); p.Dispose(); };
                guideCard.Controls.Add(new Label { Text = "How to schedule an interview:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(180, 210, 195), Left = 16, Top = 12, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "1.  Go to Applicant List → click a row", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 38, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "2.  Click \"Start Review\" → status: Under Review", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 60, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "3.  Go to Screening → mark as Qualified → status: Shortlisted", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 82, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "4.  Come back here — the applicant will appear in the dropdown", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 104, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "5.  Fill in schedule details and click Schedule Interview", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 126, AutoSize = true, BackColor = Color.Transparent });
                this.Controls.Add(guideCard);

                // Still show existing schedules
                ShowExistingSchedules(db, 340);
                return;
            }

            // ── Schedule form ────────────────────────────────────────
            this.Controls.Add(new Label { Text = "SCHEDULE NEW INTERVIEW", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = 176, AutoSize = true, BackColor = Color.Transparent });

            Panel schedCard = new Panel
            {
                Left = 28,
                Top = 196,
                Width = this.Width - 56,
                Height = 190,
                BackColor = Color.FromArgb(22, 28, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            schedCard.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(p, 0, 0, schedCard.Width - 1, schedCard.Height - 1); p.Dispose(); };

            schedCard.Controls.Add(FL("Applicant (Shortlisted)", 14, 12));
            ComboBox cmbApp = new ComboBox
            {
                Left = 14,
                Top = 30,
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            DataTable shortlisted = db.Query(
                @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name,' — ',jv.title) AS label
                  FROM applications a
                  JOIN applicants ap    ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  WHERE a.status = 'Shortlisted'");
            cmbApp.Items.Add("-- Select Applicant --");
            foreach (DataRow r in shortlisted.Rows)
                cmbApp.Items.Add(r["label"] + "|" + r["id"]);
            cmbApp.SelectedIndex = 0;
            schedCard.Controls.Add(cmbApp);

            schedCard.Controls.Add(FL("Interview Type", 344, 12));
            ComboBox cmbType = new ComboBox
            {
                Left = 344,
                Top = 30,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            DataTable iTypes = db.Query("SELECT name FROM interview_types WHERE is_active=1");
            foreach (DataRow r in iTypes.Rows) cmbType.Items.Add(r["name"].ToString());
            if (cmbType.Items.Count > 0) cmbType.SelectedIndex = 0;
            schedCard.Controls.Add(cmbType);

            schedCard.Controls.Add(FL("Mode", 514, 12));
            ComboBox cmbMode = new ComboBox
            {
                Left = 514,
                Top = 30,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbMode.Items.AddRange(new object[] { "Online", "On-site", "Phone" });
            cmbMode.SelectedIndex = 0;
            schedCard.Controls.Add(cmbMode);

            schedCard.Controls.Add(FL("Date", 14, 70));
            DateTimePicker dtpDate = new DateTimePicker
            {
                Left = 14,
                Top = 88,
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(3),
                MinDate = DateTime.Today.AddDays(1)
            };
            schedCard.Controls.Add(dtpDate);

            schedCard.Controls.Add(FL("Time (HH:MM)", 184, 70));
            TextBox txtTime = new TextBox
            {
                Left = 184,
                Top = 88,
                Width = 100,
                Height = 22,
                BackColor = Color.FromArgb(26, 34, 28),
                ForeColor = Color.FromArgb(190, 210, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
                Text = "10:00"
            };
            schedCard.Controls.Add(txtTime);

            schedCard.Controls.Add(FL("Interviewer", 294, 70));
            TextBox txtInterviewer = new TextBox
            {
                Left = 294,
                Top = 88,
                Width = 200,
                Height = 22,
                BackColor = Color.FromArgb(26, 34, 28),
                ForeColor = Color.FromArgb(190, 210, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
                Text = project.Session.AdminFullName
            };
            schedCard.Controls.Add(txtInterviewer);

            schedCard.Controls.Add(FL("Location / Meeting URL", 14, 126));
            TextBox txtLoc = new TextBox
            {
                Left = 14,
                Top = 144,
                Width = 480,
                Height = 22,
                BackColor = Color.FromArgb(26, 34, 28),
                ForeColor = Color.FromArgb(190, 210, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
            schedCard.Controls.Add(txtLoc);

            Button btnSchedule = new Button
            {
                Text = "Schedule Interview",
                Left = 504,
                Top = 142,
                Width = 160,
                Height = 28,
                BackColor = Color.FromArgb(25, 80, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSchedule.FlatAppearance.BorderSize = 0;
            btnSchedule.Click += (s, e) =>
            {
                if (cmbApp.SelectedIndex == 0) { MessageBox.Show("Please select an applicant.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (cmbType.SelectedIndex < 0) { MessageBox.Show("Please select an interview type.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                // Business rule: date must not be in the past
                if (dtpDate.Value.Date < DateTime.Today.AddDays(1))
                {
                    MessageBox.Show("Interview date must be at least tomorrow.", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!TimeSpan.TryParse(txtTime.Text.Trim(), out _))
                {
                    MessageBox.Show("Invalid time format. Use HH:MM (e.g. 10:00)", "Invalid Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    string selected = cmbApp.SelectedItem.ToString();
                    int selAppId = Convert.ToInt32(selected.Split('|')[1]);

                    db.Execute(
                        @"INSERT INTO interview_schedules (application_id, interview_type, scheduled_date, scheduled_time, mode, location, interviewer, status)
                          VALUES (@aid,@type,@dt,@tm,@mode,@loc,@iv,'Scheduled')",
                        ("@aid", selAppId), ("@type", cmbType.SelectedItem.ToString()),
                        ("@dt", dtpDate.Value.ToString("yyyy-MM-dd")),
                        ("@tm", txtTime.Text.Trim()),
                        ("@mode", cmbMode.SelectedItem.ToString()),
                        ("@loc", txtLoc.Text.Trim()),
                        ("@iv", txtInterviewer.Text.Trim()));

                    db.Execute("UPDATE applications SET status='For Interview' WHERE id=@id", ("@id", selAppId));
                    db.Execute(
                        "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,'For Interview',@rm,@who)",
                        ("@id", selAppId),
                        ("@rm", cmbType.SelectedItem + " on " + dtpDate.Value.ToString("MMM dd, yyyy") + " via " + cmbMode.SelectedItem),
                        ("@who", project.Session.AdminUsername));

                    Audit.Log("Scheduled interview", "interview_schedules", selAppId);
                    MessageBox.Show("Interview scheduled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Controls.Clear();
                    BuildUI();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            schedCard.Controls.Add(btnSchedule);
            this.Controls.Add(schedCard);

            ShowExistingSchedules(db, 400);
        }

        private void ShowExistingSchedules(DatabaseConnection db, int startTop)
        {
            this.Controls.Add(new Label { Text = "EXISTING SCHEDULES", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = startTop, AutoSize = true, BackColor = Color.Transparent });

            DataTable schedules = db.Query(
                @"SELECT is2.id, CONCAT(ap.first_name,' ',ap.last_name) AS name,
                         jv.title AS job, is2.interview_type, is2.scheduled_date,
                         is2.scheduled_time, is2.mode, is2.location, is2.interviewer, is2.status
                  FROM interview_schedules is2
                  JOIN applications a   ON a.id  = is2.application_id
                  JOIN applicants ap    ON ap.id  = a.applicant_id
                  JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                  ORDER BY is2.scheduled_date DESC LIMIT 20");

            int rowTop = startTop + 22;
            if (schedules.Rows.Count == 0)
            {
                this.Controls.Add(new Label { Text = "No interview schedules yet.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = rowTop, AutoSize = true, BackColor = Color.Transparent });
                return;
            }

            foreach (DataRow row in schedules.Rows)
            {
                Panel ic = SchedRow(
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString(), row["job"].ToString(),
                    row["interview_type"].ToString(),
                    Convert.ToDateTime(row["scheduled_date"]).ToString("MMM dd, yyyy"),
                    row["scheduled_time"].ToString(), row["mode"].ToString(),
                    row["location"].ToString(), row["interviewer"].ToString(),
                    row["status"].ToString(), this.Width - 56, db);
                ic.Left = 28; ic.Top = rowTop;
                ic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                this.Controls.Add(ic);
                rowTop += ic.Height + 6;
            }
        }

        private Panel SchedRow(int schedId, string name, string job, string type, string date,
            string time, string mode, string location, string interviewer, string status, int width, DatabaseConnection db)
        {
            Color sc = status == "Scheduled" ? Color.FromArgb(80, 160, 220) : status == "Completed" ? Color.FromArgb(60, 200, 100) : Color.FromArgb(180, 80, 80);
            Panel card = new Panel { Width = width, Height = 72, BackColor = Color.FromArgb(22, 32, 44) };
            card.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(35, 60, 90), 1); e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1); p.Dispose(); };
            card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 72, BackColor = sc });
            card.Controls.Add(new Label { Text = name + "  —  " + job, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(180, 210, 235), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = type + "   |   " + date + "  " + time + "   |   " + mode, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(110, 145, 170), Left = 16, Top = 30, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = "Location: " + location + "   |   Interviewer: " + interviewer, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(90, 120, 145), Left = 16, Top = 50, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = status.ToUpper(), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = sc, BackColor = Color.FromArgb(22, 28, 38), Width = 90, Height = 20, TextAlign = ContentAlignment.MiddleCenter, Left = card.Width - 220, Top = 8, Anchor = AnchorStyles.Top | AnchorStyles.Right });

            if (status == "Scheduled")
            {
                Button btnDone = new Button { Text = "Mark Done", Left = card.Width - 120, Top = 8, Width = 100, Height = 26, Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(25, 70, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand };
                btnDone.FlatAppearance.BorderSize = 0;
                Button btnCancel = new Button { Text = "Cancel", Left = card.Width - 120, Top = 40, Width = 100, Height = 24, Anchor = AnchorStyles.Top | AnchorStyles.Right, BackColor = Color.FromArgb(60, 30, 30), ForeColor = Color.FromArgb(220, 160, 160), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand };
                btnCancel.FlatAppearance.BorderSize = 0;
                int capturedId = schedId;
                btnDone.Click += (s, e) =>
                {
                    db.Execute("UPDATE interview_schedules SET status='Completed' WHERE id=@id", ("@id", capturedId));
                    Audit.Log("Interview completed", "interview_schedules", capturedId);
                    MessageBox.Show("Marked as completed.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Controls.Clear(); BuildUI();
                };
                btnCancel.Click += (s, e) =>
                {
                    var r = MessageBox.Show("Cancel this interview?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r != DialogResult.Yes) return;
                    db.Execute("UPDATE interview_schedules SET status='Cancelled' WHERE id=@id", ("@id", capturedId));
                    Audit.Log("Interview cancelled", "interview_schedules", capturedId);
                    this.Controls.Clear(); BuildUI();
                };
                card.Controls.Add(btnDone);
                card.Controls.Add(btnCancel);
            }
            return card;
        }

        private Label FL(string t, int l, int top) => new Label { Text = t, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = l, Top = top, AutoSize = true, BackColor = Color.Transparent };

        private void InterviewScheduleForm_Load(object sender, EventArgs e) { }
    }
}