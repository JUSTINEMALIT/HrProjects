using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class ApplicantReviewForm : Form
    {
        private int _appId;

        public ApplicantReviewForm()
        {
            InitializeComponent();
            BuildUI(0);
        }

        public ApplicantReviewForm(int appId)
        {
            InitializeComponent();
            _appId = appId;
            BuildUI(appId);
        }

        private void BuildUI(int appId)
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.AutoScroll = true;

            this.Controls.Add(new Label { Text = "Applicant Review", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 20, AutoSize = true, BackColor = Color.Transparent });

            if (appId == 0)
            {
                this.Controls.Add(new Label { Text = "Select an applicant from the Applicant List to review.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(130, 150, 140), Left = 28, Top = 60, AutoSize = true, BackColor = Color.Transparent });
                return;
            }

            var db = new DatabaseConnection();

            // Load application + applicant + profile data
            DataTable appDt = db.Query(
                @"SELECT a.id, a.status, a.expected_salary, a.preferred_start_date,
                         a.employment_type_pref, a.referral_source, a.cover_letter, a.submitted_at,
                         ap.first_name, ap.last_name, ap.email, ap.mobile_number, ap.date_of_birth,
                         CONCAT(ap.first_name,' ',ap.last_name) AS full_name,
                         jv.title AS job, jv.employment_type, d.name AS department,
                         prof.middle_name, prof.gender, prof.civil_status, prof.nationality,
                         prof.province, prof.city, prof.barangay, prof.street, prof.zip_code,
                         prof.highest_degree, prof.school, prof.course, prof.year_graduated,
                         prof.skills, prof.work_company, prof.work_position, prof.work_duration
                  FROM applications a
                  JOIN applicants ap    ON ap.id  = a.applicant_id
                  JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                  JOIN departments d    ON d.id   = jv.department_id
                  LEFT JOIN applicant_profiles prof ON prof.applicant_id = ap.id
                  WHERE a.id = @id",
                ("@id", appId));

            if (appDt.Rows.Count == 0) { this.Controls.Add(new Label { Text = "Application not found.", ForeColor = Color.Red, Left = 28, Top = 60, AutoSize = true }); return; }

            DataRow app = appDt.Rows[0];
            string status = app["status"].ToString();
            bool isLocked = status != "Submitted";

            string S(string col) => app[col] == DBNull.Value ? "—" : app[col].ToString();

            // Sub label
            this.Controls.Add(new Label { Text = "APP-" + appId.ToString("D4") + "   |   Status: " + status, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 48, AutoSize = true, BackColor = Color.Transparent });

            // ── Action buttons ───────────────────────────────────────
            Panel actionBar = new Panel { Left = 28, Top = 68, Width = this.Width - 56, Height = 44, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            if (status == "Submitted")
            {
                Button btnReview = ABtn("Start Review", Color.FromArgb(28, 80, 52), 0);
                btnReview.Click += (s, e) => ChangeStatus(appId, "Under Review", "HR started reviewing the application.", db);
                actionBar.Controls.Add(btnReview);
            }
            if (status == "Under Review")
            {
                Button btnShortlist = ABtn("Shortlist", Color.FromArgb(25, 80, 70), 0);
                btnShortlist.Click += (s, e) => ChangeStatus(appId, "Shortlisted", "Applicant has been shortlisted.", db);
                Button btnReject = ABtn("Reject", Color.FromArgb(80, 28, 28), 120);
                btnReject.ForeColor = Color.FromArgb(255, 180, 180);
                btnReject.Click += (s, e) => ChangeStatus(appId, "Rejected", "Applicant did not pass initial review.", db);
                actionBar.Controls.Add(btnShortlist);
                actionBar.Controls.Add(btnReject);
            }
            if (status == "Shortlisted")
            {
                Button btnForInterview = ABtn("For Interview", Color.FromArgb(50, 30, 90), 0);
                btnForInterview.Click += (s, e) => ChangeStatus(appId, "For Interview", "Applicant scheduled for interview.", db);
                actionBar.Controls.Add(btnForInterview);
            }

            // Business rule: HR Staff cannot accept — show disabled Accept for reference
            bool isManager = project.Session.AdminRole == "Admin" || project.Session.AdminRole == "HR Manager";
            if (status == "For Final Review")
            {
                if (isManager)
                {
                    Button btnAccept = ABtn("Accept", Color.FromArgb(20, 80, 45), 0);
                    btnAccept.Click += (s, e) => FinalDecision(appId, "Accepted", db);
                    Button btnRejectFinal = ABtn("Reject", Color.FromArgb(80, 28, 28), 120);
                    btnRejectFinal.ForeColor = Color.FromArgb(255, 180, 180);
                    btnRejectFinal.Click += (s, e) => FinalDecision(appId, "Rejected", db);
                    Button btnHold = ABtn("On Hold", Color.FromArgb(60, 50, 20), 240);
                    btnHold.Click += (s, e) => FinalDecision(appId, "On Hold", db);
                    actionBar.Controls.AddRange(new Control[] { btnAccept, btnRejectFinal, btnHold });
                }
                else
                {
                    // Business rule: HR Staff cannot accept
                    Label lblNoAccept = new Label { Text = "⚠  Only HR Manager/Admin can make the final hiring decision.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(220, 160, 60), Left = 0, Top = 10, AutoSize = true, BackColor = Color.Transparent };
                    actionBar.Controls.Add(lblNoAccept);
                }
            }

            this.Controls.Add(actionBar);

            int top = 122;

            // ── Applicant Info ───────────────────────────────────────
            top = InfoSection("Applicant Information", top, new[]
            {
                ("Full Name",    S("full_name")),
                ("Email",        S("email")),
                ("Mobile",       S("mobile_number")),
                ("Date of Birth",app["date_of_birth"]==DBNull.Value?"—":Convert.ToDateTime(app["date_of_birth"]).ToString("MM-dd-yyyy")),
                ("Gender",       S("gender")),
                ("Civil Status", S("civil_status")),
                ("Nationality",  S("nationality")),
            });

            // ── Address ──────────────────────────────────────────────
            top = InfoSection("Address", top, new[]
            {
                ("Province",  S("province")),
                ("City",      S("city")),
                ("Barangay",  S("barangay")),
                ("Street",    S("street")),
                ("Zip Code",  S("zip_code")),
            });

            // ── Education ────────────────────────────────────────────
            top = InfoSection("Educational Background", top, new[]
            {
                ("Highest Degree", S("highest_degree")),
                ("School",         S("school")),
                ("Course",         S("course")),
                ("Year Graduated", S("year_graduated")),
                ("Skills",         S("skills")),
            });

            // ── Work ─────────────────────────────────────────────────
            top = InfoSection("Work Experience", top, new[]
            {
                ("Company",   S("work_company")),
                ("Position",  S("work_position")),
                ("Duration",  S("work_duration")),
            });

            // ── Job Applied ──────────────────────────────────────────
            top = InfoSection("Job Applied For", top, new[]
            {
                ("Position",    S("job")),
                ("Department",  S("department")),
                ("Type",        S("employment_type")),
                ("Submitted",   app["submitted_at"]==DBNull.Value?"—":Convert.ToDateTime(app["submitted_at"]).ToString("MMM dd, yyyy")),
                ("Exp. Salary", app["expected_salary"]==DBNull.Value?"—":"₱ "+app["expected_salary"]),
                ("Start Date",  app["preferred_start_date"]==DBNull.Value?"—":Convert.ToDateTime(app["preferred_start_date"]).ToString("MMM dd, yyyy")),
                ("Source",      S("referral_source")),
            });

            // ── Cover letter ─────────────────────────────────────────
            this.Controls.Add(new Label { Text = "COVER LETTER", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
            Panel coverCard = new Panel { Left = 28, Top = top + 20, Width = this.Width - 56, Height = 80, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            coverCard.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, coverCard.Width - 1, coverCard.Height - 1); pen.Dispose(); };
            coverCard.Controls.Add(new Label { Text = app["cover_letter"] == DBNull.Value ? "No cover letter." : app["cover_letter"].ToString(), Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(180, 200, 190), Left = 14, Top = 10, Width = coverCard.Width - 28, Height = 60, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
            this.Controls.Add(coverCard);
            top += 110;

            // ── Documents ───────────────────────────────────────────
            this.Controls.Add(new Label { Text = "SUBMITTED DOCUMENTS", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
            top += 22;

            // Doc table header
            Panel dHdr = new Panel { Left = 28, Top = top, Width = this.Width - 56, Height = 26, BackColor = Color.FromArgb(26, 34, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            dHdr.Controls.Add(DH("DOCUMENT", 12, 190));
            dHdr.Controls.Add(DH("STATUS", 210, 90));
            dHdr.Controls.Add(DH("FILE", 306, 160));
            dHdr.Controls.Add(DH("HR REMARKS", 474, 160));
            dHdr.Controls.Add(DH("VIEW", 644, 76));
            dHdr.Controls.Add(DH("ACTION", 728, 86));
            this.Controls.Add(dHdr);
            top += 28;

            DataTable docs = db.Query(
                @"SELECT ad.id, rt.name AS doc_name, ad.status, ad.file_name, ad.hr_remarks
                  FROM applicant_documents ad
                  JOIN requirement_types rt ON rt.id = ad.requirement_type_id
                  WHERE ad.application_id = @aid ORDER BY rt.id",
                ("@aid", appId));

            foreach (DataRow doc in docs.Rows)
            {
                Panel dr = DocRow(
                    Convert.ToInt32(doc["id"]),
                    doc["doc_name"].ToString(), doc["status"].ToString(),
                    doc["file_name"] == DBNull.Value ? "—" : doc["file_name"].ToString(),
                    doc["hr_remarks"] == DBNull.Value ? "" : doc["hr_remarks"].ToString(),
                    this.Width - 56, db, appId);
                dr.Left = 28; dr.Top = top;
                dr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                this.Controls.Add(dr);
                top += dr.Height + 2;
            }

            // ── Status History ───────────────────────────────────────
            this.Controls.Add(new Label { Text = "STATUS HISTORY", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 100, 85), Left = 28, Top = top + 12, AutoSize = true, BackColor = Color.Transparent });
            top += 34;

            DataTable hist = db.Query(
                "SELECT status, remarks, changed_by, changed_at FROM application_status_history WHERE application_id=@id ORDER BY changed_at DESC",
                ("@id", appId));

            foreach (DataRow row in hist.Rows)
            {
                Color sc = StatusColor(row["status"].ToString());
                Panel hr2 = new Panel { Left = 28, Top = top, Width = this.Width - 56, Height = 46, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                hr2.Paint += (s, e) =>
                {
                    Pen p = new Pen(Color.FromArgb(35, 50, 42), 1);
                    e.Graphics.DrawLine(p, 0, hr2.Height - 1, hr2.Width, hr2.Height - 1); p.Dispose();
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    SolidBrush b = new SolidBrush(sc); e.Graphics.FillEllipse(b, 12, 18, 10, 10); b.Dispose();
                };
                hr2.Controls.Add(new Label { Text = row["status"].ToString(), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = sc, Left = 34, Top = 6, AutoSize = true, BackColor = Color.Transparent });
                hr2.Controls.Add(new Label { Text = row["remarks"] == DBNull.Value ? "" : row["remarks"].ToString(), Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(160, 180, 170), Left = 34, Top = 26, Width = this.Width - 220, AutoSize = false, BackColor = Color.Transparent });
                hr2.Controls.Add(new Label { Text = Convert.ToDateTime(row["changed_at"]).ToString("MMM dd, yyyy h:mm tt") + " — " + row["changed_by"], Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(90, 110, 100), Width = 260, TextAlign = ContentAlignment.TopRight, Left = this.Width - 310, Top = 8, BackColor = Color.Transparent });
                this.Controls.Add(hr2);
                top += hr2.Height + 2;
            }
        }

        private void ChangeStatus(int appId, string newStatus, string remarks, DatabaseConnection db)
        {
            var r = MessageBox.Show("Change status to \"" + newStatus + "\"?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
            try
            {
                db.Execute("UPDATE applications SET status=@st WHERE id=@id", ("@st", newStatus), ("@id", appId));
                db.Execute(
                    "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,@st,@rm,@who)",
                    ("@id", appId), ("@st", newStatus), ("@rm", remarks), ("@who", project.Session.AdminUsername));
                Audit.Log("Changed status to " + newStatus, "applications", appId);
                MessageBox.Show("Status updated to: " + newStatus, "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Reload(appId);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void FinalDecision(int appId, string decision, DatabaseConnection db)
        {
            // Business rule: only HR Manager/Admin can accept
            if (decision == "Accepted" && project.Session.AdminRole == "HR Staff")
            {
                MessageBox.Show("Only HR Manager or Admin can accept applicants.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Form popup = new Form())
            {
                popup.Text = "Final Decision — " + decision;
                popup.Size = new Size(420, 220);
                popup.BackColor = Color.FromArgb(22, 28, 24);
                popup.StartPosition = FormStartPosition.CenterParent;
                popup.FormBorderStyle = FormBorderStyle.FixedDialog;

                popup.Controls.Add(new Label { Text = "Remarks:", ForeColor = Color.White, Left = 16, Top = 16, AutoSize = true });
                TextBox txtR = new TextBox { Left = 16, Top = 36, Width = 370, Height = 80, Multiline = true, BackColor = Color.FromArgb(28, 35, 30), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                popup.Controls.Add(txtR);

                Button btnOk = new Button { Text = "Confirm " + decision, Left = 16, Top = 128, Width = 180, Height = 32, BackColor = decision == "Accepted" ? Color.FromArgb(20, 80, 45) : Color.FromArgb(80, 28, 28), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) =>
                {
                    try
                    {
                        string newStatus = decision == "Accepted" ? "Accepted" : decision == "Rejected" ? "Rejected" : "For Final Review";
                        db.Execute("UPDATE applications SET status=@st WHERE id=@id", ("@st", newStatus), ("@id", appId));
                        // Upsert hiring_decisions
                        object exists = db.Scalar("SELECT COUNT(*) FROM hiring_decisions WHERE application_id=@id", ("@id", appId));
                        if (Convert.ToInt32(exists) > 0)
                            db.Execute("UPDATE hiring_decisions SET decision=@d, remarks=@r, decided_by=@by, decided_at=NOW() WHERE application_id=@id",
                                ("@d", decision), ("@r", txtR.Text.Trim()), ("@by", project.Session.AdminUsername), ("@id", appId));
                        else
                            db.Execute("INSERT INTO hiring_decisions (application_id, decision, remarks, decided_by) VALUES (@id,@d,@r,@by)",
                                ("@id", appId), ("@d", decision), ("@r", txtR.Text.Trim()), ("@by", project.Session.AdminUsername));
                        db.Execute(
                            "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,@st,@rm,@who)",
                            ("@id", appId), ("@st", newStatus), ("@rm", "Final decision: " + decision + ". " + txtR.Text.Trim()), ("@who", project.Session.AdminUsername));
                        Audit.Log("Final decision: " + decision, "hiring_decisions", appId);
                        MessageBox.Show("Decision saved: " + decision, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        popup.Close();
                        Reload(appId);
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                };
                popup.Controls.Add(btnOk);
                popup.ShowDialog();
            }
        }

        private void Reload(int appId)
        {
            var parent = this.Parent?.FindForm() as MainForm;
            if (parent != null) parent.LoadForm(new ApplicantReviewForm(appId));
            else { this.Controls.Clear(); BuildUI(appId); }
        }

        private int InfoSection(string title, int top, (string label, string value)[] fields)
        {
            int colW = (this.Width - 56 - 28) / 2;
            int cardH = 36 + (int)Math.Ceiling(fields.Length / 2.0) * 40 + 10;
            Panel card = new Panel { Left = 28, Top = top, Width = this.Width - 56, Height = cardH, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            card.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); pen.Dispose(); };
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 200, 130), Left = 14, Top = 10, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Panel { Left = 0, Top = 32, Height = 1, Width = card.Width, BackColor = Color.FromArgb(35, 50, 42) });
            for (int i = 0; i < fields.Length; i++)
            {
                int col = i % 2, row = i / 2;
                int fx = col == 0 ? 14 : colW + 28, fy = 38 + row * 40;
                card.Controls.Add(new Label { Text = fields[i].label, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = fx, Top = fy, AutoSize = true, BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = fields[i].value, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 218, 208), Left = fx, Top = fy + 16, Width = colW - 14, BackColor = Color.Transparent });
            }
            this.Controls.Add(card);
            return top + cardH + 10;
        }

        private Panel DocRow(int docId, string name, string status, string fileName, string remarks, int width, DatabaseConnection db, int appId)
        {
            // Get file_path from DB
            string filePath = "";
            try
            {
                object fp = db.Scalar("SELECT file_path FROM applicant_documents WHERE id=@id", ("@id", docId));
                if (fp != null && fp != DBNull.Value) filePath = fp.ToString();
            }
            catch { }

            bool hasFile = !string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath);

            Color sc = status == "Submitted" || status == "Validated"
                ? Color.FromArgb(60, 180, 100)
                : status == "Missing"
                    ? Color.FromArgb(220, 80, 80)
                    : Color.FromArgb(120, 120, 120);

            Panel row = new Panel { Width = width, Height = 50, BackColor = Color.FromArgb(22, 28, 24) };
            row.Paint += (s, e) =>
            {
                Pen p = new Pen(Color.FromArgb(35, 50, 42), 1);
                e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1); p.Dispose();
            };

            // Document name
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 218, 208), Left = 12, Top = 15, Width = 190, BackColor = Color.Transparent });

            // Status badge
            row.Controls.Add(new Label { Text = status.ToUpper(), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = sc, BackColor = Color.FromArgb(24, 28, 26), Width = 86, Height = 20, TextAlign = ContentAlignment.MiddleCenter, Left = 210, Top = 15 });

            // File name
            row.Controls.Add(new Label
            {
                Text = fileName == "—" ? "No file uploaded" : fileName,
                Font = new Font("Segoe UI", 8f),
                ForeColor = hasFile ? Color.FromArgb(110, 160, 200) : Color.FromArgb(130, 100, 100),
                Left = 306,
                Top = 15,
                Width = 160,
                BackColor = Color.Transparent
            });

            // HR Remarks
            TextBox txtR = new TextBox { Text = remarks, Left = 474, Top = 14, Width = 160, Height = 22, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 8.5f) };
            row.Controls.Add(txtR);

            // View File button — only if has file
            Button btnView = new Button
            {
                Text = "View File",
                Left = 644,
                Top = 13,
                Width = 76,
                Height = 26,
                BackColor = hasFile ? Color.FromArgb(25, 55, 80) : Color.FromArgb(35, 35, 35),
                ForeColor = hasFile ? Color.White : Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = hasFile ? Cursors.Hand : Cursors.Default,
                Enabled = hasFile
            };
            btnView.FlatAppearance.BorderSize = 0;
            string capturedPath = filePath;
            btnView.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = capturedPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex) { MessageBox.Show("Cannot open file:\n" + ex.Message); }
            };
            row.Controls.Add(btnView);

            // Validate button
            Button btnVal = new Button
            {
                Text = status == "Validated" ? "✔ Validated" : "Validate",
                Left = 728,
                Top = 13,
                Width = 86,
                Height = 26,
                BackColor = status == "Validated" ? Color.FromArgb(20, 50, 30) : Color.FromArgb(25, 70, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand,
                Enabled = status != "Validated"
            };
            btnVal.FlatAppearance.BorderSize = 0;
            int capturedDocId = docId;
            btnVal.Click += (s, e) =>
            {
                // Business rule: must have file before validating
                if (!hasFile)
                {
                    MessageBox.Show("Cannot validate — applicant has not uploaded this document yet.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show("Validate this document?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                try
                {
                    db.Execute(
                        "UPDATE applicant_documents SET status='Validated', hr_remarks=@rm WHERE id=@id",
                        ("@rm", txtR.Text.Trim()), ("@id", capturedDocId));
                    Audit.Log("Validated document", "applicant_documents", capturedDocId);
                    MessageBox.Show("Document validated successfully.", "Validated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Reload(appId);
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            row.Controls.Add(btnVal);
            return row;
        }

        private Button ABtn(string text, Color bg, int left)
        {
            Button btn = new Button { Text = text, Left = left, Top = 6, Width = 110, Height = 32, BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Label DH(string t, int l, int w) => new Label { Text = t, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 110, 95), Left = l, Top = 7, Width = w, BackColor = Color.Transparent };

        private Color StatusColor(string s)
        {
            if (s == "Submitted") return Color.FromArgb(80, 160, 220);
            if (s == "Under Review") return Color.FromArgb(230, 160, 50);
            if (s == "Shortlisted") return Color.FromArgb(80, 200, 160);
            if (s == "For Interview") return Color.FromArgb(140, 100, 220);
            if (s == "For Final Review") return Color.FromArgb(160, 200, 80);
            if (s == "Accepted") return Color.FromArgb(60, 200, 100);
            if (s == "Rejected") return Color.FromArgb(220, 80, 80);
            return Color.FromArgb(150, 150, 150);
        }

        private void ApplicantReviewForm_Load(object sender, EventArgs e) { }
    }
}