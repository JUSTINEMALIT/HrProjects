using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HRApplicant
{
    public partial class ApplicantMyApplicationPage : Form
    {
        private ApplicantMainForm mainForm;
        private Panel contentPanel;

        public ApplicantMyApplicationPage(ApplicantMainForm main)
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

            p.Controls.Add(new Label { Text = "My Application", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });

            DataTable allApps = db.Query(
                @"SELECT a.id, a.status, jv.title AS job_title, d.name AS department, a.created_at
                  FROM applications a
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  JOIN departments d    ON d.id  = jv.department_id
                  WHERE a.applicant_id = @aid
                  ORDER BY a.created_at DESC",
                ("@aid", project.Session.ApplicantId));

            if (allApps.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "You have no applications yet.\nGo to Job Vacancies to apply.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = 70, AutoSize = true, BackColor = Color.Transparent });
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

            Panel appContent = new Panel
            {
                Left = 0,
                Top = 78,
                Width = p.Width,
                Height = p.Height - 78,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            p.Controls.Add(appContent);

            Action<int> loadApp = null;
            loadApp = (appId) =>
            {
                appContent.Controls.Clear();

                DataTable appDt = db.Query(
                    @"SELECT a.id, a.status, a.expected_salary, a.preferred_start_date,
                             a.employment_type_pref, a.referral_source, a.cover_letter, a.submitted_at,
                             jv.title AS job_title, jv.employment_type, d.name AS department
                      FROM applications a
                      JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                      JOIN departments d    ON d.id  = jv.department_id
                      WHERE a.id = @id",
                    ("@id", appId));

                if (appDt.Rows.Count == 0) return;
                DataRow app = appDt.Rows[0];
                string status = app["status"].ToString();
                bool isEditable = status == "Draft" || status == "Submitted";
                mainForm.applicationStatus = status;

                string SafeStr(object v) => v == DBNull.Value ? "" : v.ToString();
                string SafeDate(object v) => v == DBNull.Value ? "" : Convert.ToDateTime(v).ToString("yyyy-MM-dd");

                int top = 10;

                // Sub-label
                appContent.Controls.Add(new Label
                {
                    Text = isEditable ? "You can still edit this application." : "This application is locked — HR is reviewing it.",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = isEditable ? Color.FromArgb(100, 130, 115) : Color.FromArgb(200, 140, 60),
                    Left = 28,
                    Top = top,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
                top += 26;

                // Lock banner
                if (!isEditable)
                {
                    Panel lockBanner = new Panel { Left = 28, Top = top, Width = p.Width - 56, Height = 34, BackColor = Color.FromArgb(45, 38, 22), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                    lockBanner.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(80, 60, 20), 1); e.Graphics.DrawRectangle(pen, 0, 0, lockBanner.Width - 1, lockBanner.Height - 1); pen.Dispose(); };
                    lockBanner.Controls.Add(new Label { Text = "🔒  Locked — Status: " + status, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(220, 160, 60), Left = 12, Height = 34, Width = 500, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent });
                    appContent.Controls.Add(lockBanner);
                    top += 42;
                }

                // Job card
                Panel jobCard = new Panel { Left = 28, Top = top, Width = p.Width - 56, Height = 60, BackColor = Color.FromArgb(22, 32, 26), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                jobCard.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 60, 44), 1); e.Graphics.DrawRectangle(pen, 0, 0, jobCard.Width - 1, jobCard.Height - 1); pen.Dispose(); };
                jobCard.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 60, BackColor = Color.FromArgb(60, 180, 100) });
                jobCard.Controls.Add(new Label { Text = app["job_title"] + "  —  " + app["department"], Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 230, 215), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                jobCard.Controls.Add(new Label { Text = app["employment_type"] + "   •   App ID: APP-" + appId.ToString("D4") + "   •   Status: " + status, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 130, 115), Left = 16, Top = 36, AutoSize = true, BackColor = Color.Transparent });
                appContent.Controls.Add(jobCard);
                top += 70;

                // Form fields
                int colW = (p.Width - 56 - 28 - 12) / 2;
                Panel card = new Panel { Left = 28, Top = top, Width = p.Width - 56, Height = 290, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                card.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); pen.Dispose(); };
                card.Controls.Add(new Label { Text = "Application Details", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 200, 130), Left = 14, Top = 10, AutoSize = true, BackColor = Color.Transparent });
                card.Controls.Add(new Panel { Left = 0, Top = 34, Height = 1, Width = card.Width, BackColor = Color.FromArgb(35, 50, 42), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });

                string[] labels = { "Expected Salary *", "Preferred Start Date (YYYY-MM-DD) *", "Employment Type Preferred *", "Referral / Source" };
                string[] vals = { SafeStr(app["expected_salary"]), SafeDate(app["preferred_start_date"]), SafeStr(app["employment_type_pref"]), SafeStr(app["referral_source"]) };
                TextBox[] fTxt = new TextBox[labels.Length];

                bool hasUnsavedChanges = false;
                TextBox txtCover = null;
                System.Windows.Forms.Timer autoSaveTimer = new System.Windows.Forms.Timer();
                autoSaveTimer.Interval = 2000;
                autoSaveTimer.Tick += (s, e) =>
                {
                    if (isEditable && hasUnsavedChanges)
                    {
                        try
                        {
                            db.Execute(
                                @"UPDATE applications SET expected_salary=@sal, preferred_start_date=@psd,
                                  employment_type_pref=@etp, referral_source=@ref, cover_letter=@cl WHERE id=@id",
                                ("@sal", string.IsNullOrWhiteSpace(fTxt[0].Text) ? (object)DBNull.Value : fTxt[0].Text.Trim()),
                                ("@psd", string.IsNullOrWhiteSpace(fTxt[1].Text) ? (object)DBNull.Value : fTxt[1].Text.Trim()),
                                ("@etp", fTxt[2].Text.Trim()), ("@ref", fTxt[3].Text.Trim()),
                                ("@cl", txtCover.Text.Trim()), ("@id", appId));
                            hasUnsavedChanges = false;
                        }
                        catch { }
                    }
                };
                autoSaveTimer.Start();

                for (int i = 0; i < labels.Length; i++)
                {
                    int col = i % 2, row = i / 2;
                    int fx = col == 0 ? 14 : colW + 26, fy = 42 + row * 58;
                    card.Controls.Add(new Label { Text = labels[i], Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = fx, Top = fy, Width = colW, BackColor = Color.Transparent });
                    fTxt[i] = new TextBox { Text = vals[i], Left = fx, Top = fy + 16, Width = colW - 8, Height = 22, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f), ReadOnly = !isEditable };
                    fTxt[i].TextChanged += (s, e) => { hasUnsavedChanges = true; autoSaveTimer.Stop(); autoSaveTimer.Start(); };
                    card.Controls.Add(fTxt[i]);
                }

                card.Controls.Add(new Label { Text = "Cover Letter / Remarks *", Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = 14, Top = 158, AutoSize = true, BackColor = Color.Transparent });
                txtCover = new TextBox { Text = SafeStr(app["cover_letter"]), Left = 14, Top = 174, Width = card.Width - 28, Height = 72, Multiline = true, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f), ReadOnly = !isEditable, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                txtCover.TextChanged += (s, e) => { hasUnsavedChanges = true; autoSaveTimer.Stop(); autoSaveTimer.Start(); };
                card.Controls.Add(txtCover);

                card.Controls.Add(new Label { Text = "* Required fields", Font = new Font("Segoe UI", 7.5f, FontStyle.Italic), ForeColor = Color.FromArgb(220, 80, 80), Left = 14, Top = 258, AutoSize = true, BackColor = Color.Transparent });

                appContent.Controls.Add(card);
                top += 300;

                if (isEditable)
                {
                    Button btnSave = MakeBtn("Save Draft", Color.FromArgb(28, 70, 48), top, 28);
                    btnSave.Click += (s, e) =>
                    {
                        try
                        {
                            db.Execute(
                                @"UPDATE applications SET expected_salary=@sal, preferred_start_date=@psd,
                                  employment_type_pref=@etp, referral_source=@ref, cover_letter=@cl WHERE id=@id",
                                ("@sal", string.IsNullOrWhiteSpace(fTxt[0].Text) ? (object)DBNull.Value : fTxt[0].Text.Trim()),
                                ("@psd", string.IsNullOrWhiteSpace(fTxt[1].Text) ? (object)DBNull.Value : fTxt[1].Text.Trim()),
                                ("@etp", fTxt[2].Text.Trim()), ("@ref", fTxt[3].Text.Trim()),
                                ("@cl", txtCover.Text.Trim()), ("@id", appId));
                            Audit.Log("Saved draft", "applications", appId);
                            MessageBox.Show("Application saved as draft.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                    };

                    Button btnSubmit = MakeBtn("Submit Application", Color.FromArgb(20, 100, 60), top, 144);
                    btnSubmit.Width = 160;
                    btnSubmit.Click += (s, e) =>
                    {
                        // ── Profile validation ────────────────────────
                        DataTable profile = db.Query(
                            "SELECT province, city, barangay, street FROM applicant_profiles WHERE applicant_id=@aid",
                            ("@aid", project.Session.ApplicantId));

                        if (profile.Rows.Count == 0)
                        {
                            MessageBox.Show(
                                "Your profile is incomplete.\n\nPlease complete your profile first (Personal Information page) before submitting an application.",
                                "Incomplete Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        DataRow prof = profile.Rows[0];
                        if (string.IsNullOrWhiteSpace(prof["province"].ToString()))
                        { MessageBox.Show("Profile is missing: Province", "Incomplete Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                        if (string.IsNullOrWhiteSpace(prof["city"].ToString()))
                        { MessageBox.Show("Profile is missing: City", "Incomplete Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                        if (string.IsNullOrWhiteSpace(prof["street"].ToString()))
                        { MessageBox.Show("Profile is missing: Street Address", "Incomplete Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                        if (string.IsNullOrWhiteSpace(prof["barangay"].ToString()))
                        { MessageBox.Show("Profile is missing: Barangay", "Incomplete Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                        // ── Field validation ──────────────────────────
                        if (string.IsNullOrWhiteSpace(fTxt[0].Text))
                        { MessageBox.Show("Expected Salary is required.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning); fTxt[0].Focus(); return; }
                        if (string.IsNullOrWhiteSpace(fTxt[1].Text))
                        { MessageBox.Show("Preferred Start Date is required.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning); fTxt[1].Focus(); return; }
                        if (string.IsNullOrWhiteSpace(fTxt[2].Text))
                        { MessageBox.Show("Employment Type Preferred is required.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning); fTxt[2].Focus(); return; }

                        // ── Employment type match validation ──────────
                        object jobEmpType = db.Scalar(
                            "SELECT employment_type FROM job_vacancies WHERE id=(SELECT job_vacancy_id FROM applications WHERE id=@id)",
                            ("@id", appId));

                        if (jobEmpType != null && jobEmpType != DBNull.Value)
                        {
                            string jobType = jobEmpType.ToString().Trim().ToLower().Replace(" ", "").Replace("-", "");
                            string prefType = fTxt[2].Text.Trim().ToLower().Replace(" ", "").Replace("-", "");

                            if (jobType != prefType)
                            {
                                MessageBox.Show(
                                    $"Cannot submit application.\n\nThis position requires {jobEmpType.ToString().Trim()} employment type.\nYour preference is set to {fTxt[2].Text.Trim()}.\n\nPlease update your Employment Type Preferred to match the job requirement.",
                                    "Employment Type Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                fTxt[2].Focus();
                                return;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(txtCover.Text))
                        { MessageBox.Show("Cover Letter / Remarks is required.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCover.Focus(); return; }

                        // ── CV check ──────────────────────────────────
                        object cvStatus = db.Scalar(
                            @"SELECT ad.status FROM applicant_documents ad
                              JOIN requirement_types rt ON rt.id = ad.requirement_type_id
                              WHERE ad.application_id = @aid AND rt.name = 'Resume / CV'
                              LIMIT 1",
                            ("@aid", appId));

                        if (cvStatus == null || cvStatus == DBNull.Value || cvStatus.ToString() == "Missing")
                        {
                            MessageBox.Show(
                                "Cannot submit — Resume / CV is required.\n\nPlease upload your CV first in the My Documents page.",
                                "CV Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // ── Confirm and submit ────────────────────────
                        var r = MessageBox.Show("Submit this application?", "Submit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (r != DialogResult.Yes) return;
                        try
                        {
                            db.Execute(
                                @"UPDATE applications SET expected_salary=@sal, preferred_start_date=@psd,
                                  employment_type_pref=@etp, referral_source=@ref, cover_letter=@cl,
                                  status='Submitted', submitted_at=NOW() WHERE id=@id",
                                ("@sal", string.IsNullOrWhiteSpace(fTxt[0].Text) ? (object)DBNull.Value : fTxt[0].Text.Trim()),
                                ("@psd", string.IsNullOrWhiteSpace(fTxt[1].Text) ? (object)DBNull.Value : fTxt[1].Text.Trim()),
                                ("@etp", fTxt[2].Text.Trim()), ("@ref", fTxt[3].Text.Trim()),
                                ("@cl", txtCover.Text.Trim()), ("@id", appId));
                            db.Execute(
                                "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,'Submitted','Applicant submitted application.',@who)",
                                ("@id", appId), ("@who", project.Session.ApplicantName));
                            Audit.Log("Submitted application", "applications", appId);
                            MessageBox.Show("Application submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            new ApplicantMyApplicationPage(mainForm);
                        }
                        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                    };

                    Button btnWithdraw = MakeBtn("Withdraw", Color.FromArgb(70, 30, 30), top, 312);
                    btnWithdraw.ForeColor = Color.FromArgb(220, 130, 130);
                    btnWithdraw.Click += (s, e) =>
                    {
                        var r = MessageBox.Show("Withdraw this application?\nThis cannot be undone.", "Withdraw", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (r != DialogResult.Yes) return;
                        try
                        {
                            db.Execute("UPDATE applications SET status='Withdrawn' WHERE id=@id", ("@id", appId));
                            db.Execute(
                                "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,'Withdrawn','Applicant withdrew.',@who)",
                                ("@id", appId), ("@who", project.Session.ApplicantName));
                            Audit.Log("Withdrew application", "applications", appId);
                            MessageBox.Show("Application withdrawn.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            new ApplicantMyApplicationPage(mainForm);
                        }
                        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                    };

                    appContent.Controls.Add(btnSave);
                    appContent.Controls.Add(btnSubmit);
                    appContent.Controls.Add(btnWithdraw);
                }
            };

            int firstAppId = Convert.ToInt32(allApps.Rows[0]["id"]);
            loadApp(firstAppId);

            cmbJobs.SelectedIndexChanged += (s, e) =>
            {
                if (cmbJobs.SelectedIndex < 0) return;
                string selected = cmbJobs.SelectedItem.ToString();
                int selectedAppId = Convert.ToInt32(selected.Split('|')[1]);
                System.Threading.Thread.Sleep(500);
                loadApp(selectedAppId);
            };
        }

        private Button MakeBtn(string text, Color bg, int top, int left)
        {
            Button btn = new Button { Text = text, Left = left, Top = top, Width = 110, Height = 34, BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantStatusTrackingPage
            // 
            ClientSize = new Size(284, 261);
            Name = "ApplicantMyApplicationPage";
            Load += ApplicantMyApplicationPage_Load;
            ResumeLayout(false);

        }

        private void ApplicantMyApplicationPage_Load(object sender, EventArgs e)
        {

        }
    }
}