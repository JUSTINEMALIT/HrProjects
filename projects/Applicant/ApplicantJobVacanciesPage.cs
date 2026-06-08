using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HRApplicant
{
    public static class ApplicantJobVacanciesPage
    {
        public static void Show(ApplicantMainForm main)
        {
            var db = new DatabaseConnection();
            main.ClearContent();
            var p = main.contentPanel;

            p.Controls.Add(new Label { Text = "Job Vacancies", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label { Text = "Browse open positions and submit your application.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 46, AutoSize = true, BackColor = Color.Transparent });

            // Search box
            TextBox txtSearch = new TextBox
            {
                Left = 28,
                Top = 76,
                Width = 260,
                Height = 26,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
                Text = "Search..."
            };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text == "Search...") txtSearch.Text = ""; };

            // Employment type filter
            ComboBox cmbType = new ComboBox
            {
                Left = 298,
                Top = 76,
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbType.Items.AddRange(new object[] { "All Types", "Full-time", "Part-time", "Contractual" });
            cmbType.SelectedIndex = 0;

            // Status filter
            ComboBox cmbStatus = new ComboBox
            {
                Left = 440,
                Top = 76,
                Width = 110,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbStatus.Items.AddRange(new object[] { "Open Only", "All" });
            cmbStatus.SelectedIndex = 0;

            Button btnSearch = new Button
            {
                Text = "Search",
                Left = 562,
                Top = 74,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(28, 80, 52),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;

            // Job list panel (scrollable)
            Panel listPanel = new Panel
            {
                Left = 28,
                Top = 114,
                Width = p.Width - 56,
                Height = p.Height - 130,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            p.Controls.Add(txtSearch);
            p.Controls.Add(cmbType);
            p.Controls.Add(cmbStatus);
            p.Controls.Add(btnSearch);
            p.Controls.Add(listPanel);

            Action loadJobs = () =>
            {
                listPanel.Controls.Clear();

                string keyword = txtSearch.Text == "Search..." ? "" : txtSearch.Text.Trim();
                string typeFilter = cmbType.SelectedIndex > 0 ? cmbType.SelectedItem.ToString() : "";
                bool openOnly = cmbStatus.SelectedIndex == 0;

                string sql = @"SELECT jv.id, jv.title, d.name AS department, jv.employment_type,
                                      jv.slots, jv.qualifications, jv.status, jv.posted_at
                               FROM job_vacancies jv
                               JOIN departments d ON d.id = jv.department_id
                               WHERE (@kw = '' OR jv.title LIKE @kw2 OR d.name LIKE @kw2)
                                 AND (@type = '' OR jv.employment_type = @type)
                                 AND (@open = 0 OR jv.status = 'Open')
                               ORDER BY jv.posted_at DESC";

                DataTable dt = db.Query(sql,
                    ("@kw", keyword),
                    ("@kw2", "%" + keyword + "%"),
                    ("@type", typeFilter),
                    ("@open", openOnly ? 1 : 0));

                // Get jobs this applicant already applied for
                DataTable applied = db.Query(
                    "SELECT job_vacancy_id FROM applications WHERE applicant_id=@aid",
                    ("@aid", project.Session.ApplicantId));
                System.Collections.Generic.HashSet<int> appliedIds = new System.Collections.Generic.HashSet<int>();
                foreach (DataRow r in applied.Rows)
                    appliedIds.Add(Convert.ToInt32(r["job_vacancy_id"]));

                int cardTop = 0;
                if (dt.Rows.Count == 0)
                {
                    listPanel.Controls.Add(new Label { Text = "No vacancies found.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(130, 150, 140), Left = 10, Top = 20, AutoSize = true, BackColor = Color.Transparent });
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    int jobId = Convert.ToInt32(row["id"]);
                    bool isOpen = row["status"].ToString() == "Open";
                    bool alreadyApplied = appliedIds.Contains(jobId);
                    string postedAt = Convert.ToDateTime(row["posted_at"]).ToString("MMM dd, yyyy");

                    Panel card = BuildJobCard(
                        jobId, row["title"].ToString(), row["department"].ToString(),
                        row["employment_type"].ToString(), row["slots"].ToString(),
                        row["status"].ToString(), postedAt,
                        row["qualifications"].ToString(),
                        isOpen, alreadyApplied, listPanel.Width - 4, main);

                    card.Left = 0; card.Top = cardTop;
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    listPanel.Controls.Add(card);
                    cardTop += card.Height + 8;
                }
            };

            btnSearch.Click += (s, e) => loadJobs();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) loadJobs(); };
            loadJobs();
        }

        private static Panel BuildJobCard(int jobId, string title, string dept, string type,
            string slots, string status, string posted, string qualifications,
            bool isOpen, bool alreadyApplied, int width, ApplicantMainForm main)
        {
            Panel card = new Panel { Width = width, Height = 110, BackColor = Color.FromArgb(22, 28, 24) };
            card.Paint += (s, e) =>
            {
                Color borderCol = isOpen ? Color.FromArgb(35, 60, 44) : Color.FromArgb(50, 35, 35);
                Pen pen = new Pen(borderCol, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                pen.Dispose();
            };
            card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 110, BackColor = isOpen ? Color.FromArgb(60, 180, 100) : Color.FromArgb(120, 60, 60) });

            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = isOpen ? Color.FromArgb(200, 230, 215) : Color.FromArgb(160, 140, 140), Left = 16, Top = 10, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = dept + "   •   " + type + "   •   " + slots + " slot(s)   •   Posted: " + posted, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(110, 130, 120), Left = 16, Top = 34, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = qualifications, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(90, 115, 102), Left = 16, Top = 56, Width = width - 200, Height = 18, AutoSize = false, BackColor = Color.Transparent });

            // Status label
            Label lblSt = new Label
            {
                Text = status.ToUpper(),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = isOpen ? Color.FromArgb(60, 180, 100) : Color.FromArgb(180, 70, 70),
                BackColor = isOpen ? Color.FromArgb(22, 45, 30) : Color.FromArgb(45, 22, 22),
                Width = 64,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = card.Width - 148,
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(lblSt);

            // Apply button
            string btnText;
            Color btnBg;
            bool enabled;
            if (alreadyApplied) { btnText = "Applied"; btnBg = Color.FromArgb(30, 55, 40); enabled = false; }
            else if (!isOpen) { btnText = "Closed"; btnBg = Color.FromArgb(40, 35, 35); enabled = false; }
            else { btnText = "Apply"; btnBg = Color.FromArgb(30, 100, 60); enabled = true; }

            Button btnApply = new Button
            {
                Text = btnText,
                Left = card.Width - 118,
                Top = 38,
                Width = 100,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = btnBg,
                ForeColor = enabled ? Color.White : Color.FromArgb(120, 130, 125),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = enabled ? Cursors.Hand : Cursors.Default,
                Enabled = enabled
            };
            btnApply.FlatAppearance.BorderSize = 0;

            if (enabled)
            {
                int capturedJobId = jobId;
                string capturedTitle = title;
                btnApply.Click += (s, e) =>
                {
                    var db = new DatabaseConnection();
                    var result = MessageBox.Show(
                        "Apply for \"" + capturedTitle + "\"?\n\nThis will create a new Draft application.",
                        "Confirm Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;
                    try
                    {
                        // Business rule: no duplicate application
                        object dup = db.Scalar(
                            "SELECT COUNT(*) FROM applications WHERE applicant_id=@aid AND job_vacancy_id=@jid",
                            ("@aid", project.Session.ApplicantId), ("@jid", capturedJobId));
                        if (Convert.ToInt32(dup) > 0)
                        {
                            MessageBox.Show("You already applied for this position.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Business rule: job must be open
                        object openCheck = db.Scalar("SELECT status FROM job_vacancies WHERE id=@jid", ("@jid", capturedJobId));
                        if (openCheck.ToString() != "Open")
                        {
                            MessageBox.Show("This job vacancy is no longer open.", "Closed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int newAppId = db.InsertGetId(
                            "INSERT INTO applications (applicant_id, job_vacancy_id, status) VALUES (@aid,@jid,'Draft')",
                            ("@aid", project.Session.ApplicantId), ("@jid", capturedJobId));

                        // Initial status history
                        db.Execute(
                            "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,'Draft','Application created.',@who)",
                            ("@id", newAppId), ("@who", project.Session.FullName));

                        // Auto-create document slots for required types
                        DataTable reqTypes = db.Query("SELECT id FROM requirement_types WHERE is_active=1");
                        foreach (DataRow rt in reqTypes.Rows)
                        {
                            db.Execute(
                                "INSERT INTO applicant_documents (application_id, requirement_type_id, status) VALUES (@aid,@rid,'Missing')",
                                ("@aid", newAppId), ("@rid", Convert.ToInt32(rt["id"])));
                        }

                        Audit.Log("Applied for job: " + capturedTitle, "applications", newAppId);
                        MessageBox.Show("Application created! Status: Draft.\nGo to My Application to fill in details.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ApplicantJobVacanciesPage.Show(main); // refresh
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
            }
            card.Controls.Add(btnApply);
            return card;
        }
    }
}