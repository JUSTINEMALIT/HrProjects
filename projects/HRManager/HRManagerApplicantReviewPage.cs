using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


//hr manager/admin reviewpage 

namespace projects.HRManager
{
    public partial class HRManagerApplicantReviewPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentRed = Color.FromArgb(220, 38, 38);
        private static readonly Color AccentOrange = Color.FromArgb(217, 119, 6);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private int appId;
        private DatabaseConnection db;

        public HRManagerApplicantReviewPage(HRManagerMainForm main, int appId = 0)
        {
            this.mainForm = main;
            this.contentPanel = main.contentPanel;
            this.appId = appId;
            this.db = new DatabaseConnection();
            InitializePage();
        }

        private void InitializePage()
        {
            mainForm.ClearContent();
            var p = contentPanel;
            p.BackColor = BgPage;
            p.AutoScroll = true;

            int top = 24;

            p.Controls.Add(new Label { Text = "Applicant Review", Font = new Font("Segoe UI", 26f, FontStyle.Bold), ForeColor = TextPrimary, Left = 10, Top = top, AutoSize = true, BackColor = Color.Transparent });
            top += 36;

            if (appId == 0)
            {
                p.Controls.Add(new Label { Text = "Select an applicant from the Applicant List to review.", Font = new Font("Segoe UI", 11f), ForeColor = TextMuted, Left = 24, Top = 80, AutoSize = true, BackColor = Color.Transparent });
                return;
            }

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

            if (appDt.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "Application not found.", ForeColor = AccentRed, Left = 24, Top = top, AutoSize = true });
                return;
            }

            DataRow app = appDt.Rows[0];
            string status = app["status"].ToString();
            bool isManager = AdminSession.AdminRole == "Admin" || AdminSession.AdminRole == "HR Manager";

            string S(string col) => app[col] == DBNull.Value ? "—" : app[col].ToString();

            p.Controls.Add(new Label { Text = "APP-" + appId.ToString("D4") + "   |   Status: " + status, Font = new Font("Segoe UI", 8f), ForeColor = TextMuted, Left = 24, Top = 75, AutoSize = true, BackColor = Color.Transparent });
            top += 28;

            // ── Action bar (only active buttons — no For Interview here) ──
            Panel actionBar = new Panel
            {
                Left = 24,
                Top = top,
                Width = p.Width - 48,
                Height = 44,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int btnLeft = 0;

            if (status == "Submitted")
            {
                Button btnReview = new Button { Text = "Start Review", Left = btnLeft, Top = 6, Width = 120, Height = 32, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
                btnReview.FlatAppearance.BorderSize = 0;
                btnReview.Click += (s, e) => ChangeStatus(appId, "Under Review", "HR started reviewing the application.");
                actionBar.Controls.Add(btnReview);
            }

            if (status == "For Final Review" && isManager)
            {
                Button btnAccept = new Button { Text = "Accept", Left = btnLeft, Top = 6, Width = 100, Height = 32, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
                btnAccept.FlatAppearance.BorderSize = 0;
                btnAccept.Click += (s, e) => FinalDecision(appId, "Accepted");
                actionBar.Controls.Add(btnAccept);
                btnLeft += 110;

                Button btnRejectFinal = new Button { Text = "Reject", Left = btnLeft, Top = 6, Width = 100, Height = 32, BackColor = AccentRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
                btnRejectFinal.FlatAppearance.BorderSize = 0;
                btnRejectFinal.Click += (s, e) => FinalDecision(appId, "Rejected");
                actionBar.Controls.Add(btnRejectFinal);
                btnLeft += 110;

                Button btnHold = new Button { Text = "On Hold", Left = btnLeft, Top = 6, Width = 100, Height = 32, BackColor = AccentOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
                btnHold.FlatAppearance.BorderSize = 0;
                btnHold.Click += (s, e) => FinalDecision(appId, "On Hold");
                actionBar.Controls.Add(btnHold);
            }
            else if (status == "For Final Review" && !isManager)
            {
                actionBar.Controls.Add(new Label { Text = "⚠  Only HR Manager/Admin can make final hiring decisions.", Font = new Font("Segoe UI", 9f), ForeColor = AccentOrange, Left = 0, Top = 12, AutoSize = true, BackColor = Color.Transparent });
            }

            p.Controls.Add(actionBar);
            top += 56;

            // ── Status-based info banner ─────────────────────────────
            string bannerText = null;
            Color bannerBg = Color.Transparent;
            Color bannerBorder = Color.Transparent;
            Color bannerFore = Color.White;

            if (status == "Submitted")
            {
                bannerText = "ℹ  Click \"Start Review\" then go to Screening to mark as Qualified or Not Qualified.";
                bannerBg = Color.FromArgb(230, 245, 237);
                bannerBorder = AccentGreen;
                bannerFore = Color.FromArgb(22, 120, 60);
            }
            else if (status == "Under Review")
            {
                bannerText = "⚠  Go to Screening page to mark this applicant as Qualified or Not Qualified.";
                bannerBg = Color.FromArgb(255, 243, 224);
                bannerBorder = AccentOrange;
                bannerFore = Color.FromArgb(150, 80, 0);
            }
            else if (status == "Shortlisted")
            {
                bannerText = "ℹ  Go to Interview Schedule page to schedule this applicant's interview.";
                bannerBg = Color.FromArgb(219, 234, 254);
                bannerBorder = AccentBlue;
                bannerFore = Color.FromArgb(30, 64, 175);
            }

            if (bannerText != null)
            {
                Panel infoBanner = new Panel
                {
                    Left = 24,
                    Top = top,
                    Width = p.Width - 48,
                    Height = 46,
                    BackColor = bannerBg,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                Color bColor = bannerBorder;
                infoBanner.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(bColor, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, infoBanner.Width - 1, infoBanner.Height - 1);
                };
                infoBanner.Controls.Add(new Label
                {
                    Text = bannerText,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = bannerFore,
                    Left = 16,
                    Top = 13,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
                p.Controls.Add(infoBanner);
                top += 58;
            }

            top = InfoSection(p, "Applicant Information", top, new[] { ("Full Name", S("full_name")), ("Email", S("email")), ("Mobile", S("mobile_number")), ("Date of Birth", app["date_of_birth"] == DBNull.Value ? "—" : Convert.ToDateTime(app["date_of_birth"]).ToString("MM-dd-yyyy")), ("Gender", S("gender")), ("Civil Status", S("civil_status")), ("Nationality", S("nationality")), });

            top = InfoSection(p, "Address", top, new[] { ("Province", S("province")), ("City", S("city")), ("Barangay", S("barangay")), ("Street", S("street")), ("Zip Code", S("zip_code")), });

            top = InfoSection(p, "Educational Background", top, new[] { ("Highest Degree", S("highest_degree")), ("School", S("school")), ("Course", S("course")), ("Year Graduated", S("year_graduated")), ("Skills", S("skills")), });

            top = InfoSection(p, "Work Experience", top, new[] { ("Company", S("work_company")), ("Position", S("work_position")), ("Duration", S("work_duration")), });

            top = InfoSection(p, "Job Applied For", top, new[] { ("Position", S("job")), ("Department", S("department")), ("Type", S("employment_type")), ("Submitted", app["submitted_at"] == DBNull.Value ? "—" : Convert.ToDateTime(app["submitted_at"]).ToString("MMM dd, yyyy")), ("Exp. Salary", app["expected_salary"] == DBNull.Value ? "—" : "₱ " + app["expected_salary"]), ("Start Date", app["preferred_start_date"] == DBNull.Value ? "—" : Convert.ToDateTime(app["preferred_start_date"]).ToString("MMM dd, yyyy")), ("Source", S("referral_source")), });

            p.Controls.Add(new Label { Text = "COVER LETTER", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AccentBlue, Left = 24, Top = top, AutoSize = true, BackColor = Color.Transparent });

            Panel coverCard = new Panel { Left = 24, Top = top + 20, Width = p.Width - 48, Height = 100, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            coverCard.Paint += (s, e) => { using (Pen pen = new Pen(BorderLight, 1)) e.Graphics.DrawRectangle(pen, 0, 0, coverCard.Width - 1, coverCard.Height - 1); };
            coverCard.Controls.Add(new Label { Text = app["cover_letter"] == DBNull.Value ? "No cover letter provided." : app["cover_letter"].ToString(), Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 16, Top = 12, Width = coverCard.Width - 32, Height = 76, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });
            p.Controls.Add(coverCard);
            top += 130;

            p.Controls.Add(new Label { Text = "SUBMITTED DOCUMENTS", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AccentBlue, Left = 24, Top = top, AutoSize = true, BackColor = Color.Transparent });
            top += 24;

            Panel dHdr = new Panel { Left = 24, Top = top, Width = p.Width - 48, Height = 36, BackColor = Color.FromArgb(248, 250, 252), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            dHdr.Paint += (s, e) => { using (Pen pen = new Pen(BorderLight, 1)) e.Graphics.DrawRectangle(pen, 0, 0, dHdr.Width - 1, dHdr.Height - 1); };
            dHdr.Controls.Add(DH("DOCUMENT", 16, 180)); dHdr.Controls.Add(DH("STATUS", 210, 100)); dHdr.Controls.Add(DH("FILE", 320, 180)); dHdr.Controls.Add(DH("REMARKS", 510, 180)); dHdr.Controls.Add(DH("VIEW", 700, 80)); dHdr.Controls.Add(DH("ACTION", 790, 100));
            p.Controls.Add(dHdr);
            top += 40;

            DataTable docs = db.Query(@"SELECT ad.id, rt.name AS doc_name, ad.status, ad.file_name, ad.hr_remarks FROM applicant_documents ad JOIN requirement_types rt ON rt.id = ad.requirement_type_id WHERE ad.application_id = @aid ORDER BY rt.id", ("@aid", appId));

            foreach (DataRow doc in docs.Rows)
            {
                Panel dr = BuildDocRow(Convert.ToInt32(doc["id"]), doc["doc_name"].ToString(), doc["status"].ToString(), doc["file_name"] == DBNull.Value ? "—" : doc["file_name"].ToString(), doc["hr_remarks"] == DBNull.Value ? "" : doc["hr_remarks"].ToString(), p.Width - 48);
                dr.Left = 24; dr.Top = top; dr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                p.Controls.Add(dr);
                top += dr.Height;
            }

            top += 16;

            p.Controls.Add(new Label { Text = "STATUS HISTORY", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AccentBlue, Left = 24, Top = top, AutoSize = true, BackColor = Color.Transparent });
            top += 28;

            DataTable hist = db.Query("SELECT status, remarks, changed_by, changed_at FROM application_status_history WHERE application_id=@id ORDER BY changed_at DESC", ("@id", appId));

            foreach (DataRow row in hist.Rows)
            {
                Color sc = StatusColor(row["status"].ToString());
                Panel hr2 = new Panel { Left = 24, Top = top, Width = p.Width - 48, Height = 60, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                hr2.Paint += (s, e) => { using (Pen pen = new Pen(BorderLight, 1)) e.Graphics.DrawRectangle(pen, 0, 0, hr2.Width - 1, hr2.Height - 1); e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; using (SolidBrush br = new SolidBrush(sc)) e.Graphics.FillEllipse(br, 16, 22, 12, 12); };
                hr2.Controls.Add(new Label { Text = row["status"].ToString(), Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = sc, Left = 40, Top = 10, AutoSize = true, BackColor = Color.Transparent });
                hr2.Controls.Add(new Label { Text = row["remarks"] == DBNull.Value ? "" : row["remarks"].ToString(), Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 40, Top = 30, Width = hr2.Width - 280, AutoSize = false, BackColor = Color.Transparent });
                hr2.Controls.Add(new Label { Text = Convert.ToDateTime(row["changed_at"]).ToString("MMM dd, yyyy h:mm tt") + " — " + row["changed_by"], Font = new Font("Segoe UI", 8f), ForeColor = TextMuted, Width = 280, TextAlign = ContentAlignment.TopRight, Left = hr2.Width - 300, Top = 14, BackColor = Color.Transparent });
                p.Controls.Add(hr2);
                top += hr2.Height + 8;
            }

            p.AutoScrollMinSize = new Size(0, top + 50);
        }

        private void ChangeStatus(int id, string newStatus, string remarks)
        {
            var r = MessageBox.Show("Change status to \"" + newStatus + "\"?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
            try
            {
                db.Execute("UPDATE applications SET status=@st WHERE id=@id", ("@st", newStatus), ("@id", id));
                db.Execute("INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,@st,@rm,@who)", ("@id", id), ("@st", newStatus), ("@rm", remarks), ("@who", AdminSession.AdminUsername));
                Audit.Log("Changed status to " + newStatus, "applications", id);
                MessageBox.Show("Status updated to: " + newStatus, "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new HRManagerApplicantReviewPage(mainForm, id);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void FinalDecision(int id, string decision)
        {
            Form popup = new Form { Text = "Final Decision — " + decision, Width = 450, Height = 240, BackColor = BgPage, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
            popup.Controls.Add(new Label { Text = "Remarks:", ForeColor = TextPrimary, Font = new Font("Segoe UI", 9f), Left = 20, Top = 20, AutoSize = true });
            TextBox txtR = new TextBox { Left = 20, Top = 44, Width = 390, Height = 100, Multiline = true, BackColor = BgCard, ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
            popup.Controls.Add(txtR);
            Color decisionColor = decision == "Accepted" ? AccentGreen : decision == "Rejected" ? AccentRed : AccentOrange;
            Button btnOk = new Button { Text = "Confirm " + decision, Left = 20, Top = 152, Width = 180, Height = 36, BackColor = decisionColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                try
                {
                    string newStatus = decision == "Accepted" ? "Accepted" : decision == "Rejected" ? "Rejected" : "For Final Review";
                    db.Execute("UPDATE applications SET status=@st WHERE id=@id", ("@st", newStatus), ("@id", id));
                    object exists = db.Scalar("SELECT COUNT(*) FROM hiring_decisions WHERE application_id=@id", ("@id", id));
                    if (Convert.ToInt32(exists) > 0)
                        db.Execute("UPDATE hiring_decisions SET decision=@d, remarks=@r, decided_by=@by, decided_at=NOW() WHERE application_id=@id", ("@d", decision), ("@r", txtR.Text.Trim()), ("@by", AdminSession.AdminUsername), ("@id", id));
                    else
                        db.Execute("INSERT INTO hiring_decisions (application_id, decision, remarks, decided_by) VALUES (@id,@d,@r,@by)", ("@id", id), ("@d", decision), ("@r", txtR.Text.Trim()), ("@by", AdminSession.AdminUsername));
                    db.Execute("INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,@st,@rm,@who)", ("@id", id), ("@st", newStatus), ("@rm", "Final decision: " + decision + ". " + txtR.Text.Trim()), ("@who", AdminSession.AdminUsername));
                    Audit.Log("Final decision: " + decision, "hiring_decisions", id);
                    MessageBox.Show("Decision saved: " + decision, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    popup.Close();
                    new HRManagerApplicantReviewPage(mainForm, id);
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            popup.Controls.Add(btnOk);
            Button btnCancel = new Button { Text = "Cancel", Left = 210, Top = 152, Width = 100, Height = 36, BackColor = Color.FromArgb(200, 200, 200), ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => popup.Close();
            popup.Controls.Add(btnCancel);
            popup.ShowDialog();
        }

        private int InfoSection(Panel p, string title, int top, (string label, string value)[] fields)
        {
            int colW = (p.Width - 48 - 32) / 2;
            int cardH = 40 + (int)Math.Ceiling(fields.Length / 2.0) * 50 + 14;
            Panel card = new Panel { Left = 24, Top = top, Width = p.Width - 48, Height = cardH, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            card.Paint += (s, e) => { using (Pen pen = new Pen(BorderLight, 1)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AccentBlue, Left = 16, Top = 12, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Panel { Left = 0, Top = 36, Height = 1, Width = card.Width, BackColor = BorderLight });
            for (int i = 0; i < fields.Length; i++)
            {
                int col = i % 2, row = i / 2;
                int fx = col == 0 ? 16 : colW + 32, fy = 42 + row * 50;
                card.Controls.Add(new Label { Text = fields[i].label, Font = new Font("Segoe UI", 8f), ForeColor = TextMuted, Left = fx, Top = fy, AutoSize = true, BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = fields[i].value, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = fx, Top = fy + 18, Width = colW - 16, BackColor = Color.Transparent });
            }
            p.Controls.Add(card);
            return top + cardH + 12;
        }

        private Panel BuildDocRow(int docId, string name, string status, string fileName, string remarks, int width)
        {
            string filePath = "";
            try { object fp = db.Scalar("SELECT file_path FROM applicant_documents WHERE id=@id", ("@id", docId)); if (fp != null && fp != DBNull.Value) filePath = fp.ToString(); }
            catch { }
            bool hasFile = !string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath);
            Color sc = status == "Submitted" || status == "Validated" ? AccentGreen : status == "Missing" ? AccentRed : TextMuted;
            Panel row = new Panel { Width = width, Height = 54, BackColor = BgCard };
            row.Paint += (s, e) => { using (Pen pen = new Pen(BorderLight, 1)) e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1); };
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 16, Top = 16, Width = 180, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = status.ToUpper(), Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = sc, BackColor = Color.FromArgb(248, 250, 252), Width = 96, Height = 24, TextAlign = ContentAlignment.MiddleCenter, Left = 210, Top = 15 });
            row.Controls.Add(new Label { Text = fileName == "—" ? "No file uploaded" : fileName, Font = new Font("Segoe UI", 9f), ForeColor = hasFile ? AccentBlue : TextMuted, Left = 320, Top = 16, Width = 180, BackColor = Color.Transparent });
            TextBox txtR = new TextBox { Text = remarks, Left = 510, Top = 14, Width = 180, Height = 26, BackColor = Color.FromArgb(248, 250, 252), ForeColor = TextPrimary, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
            row.Controls.Add(txtR);
            Button btnView = new Button { Text = "View File", Left = 700, Top = 13, Width = 80, Height = 28, BackColor = hasFile ? AccentBlue : Color.FromArgb(200, 200, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = hasFile ? Cursors.Hand : Cursors.Default, Enabled = hasFile };
            btnView.FlatAppearance.BorderSize = 0;
            string capturedPath = filePath;
            btnView.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = capturedPath, UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show("Cannot open file:\n" + ex.Message); } };
            row.Controls.Add(btnView);
            Button btnVal = new Button { Text = status == "Validated" ? "✔ Valid" : "Validate", Left = 790, Top = 13, Width = 100, Height = 28, BackColor = status == "Validated" ? Color.FromArgb(187, 247, 208) : AccentGreen, ForeColor = status == "Validated" ? AccentGreen : Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand, Enabled = status != "Validated" };
            btnVal.FlatAppearance.BorderSize = 0;
            int capturedDocId = docId; int capturedAppId = appId;
            btnVal.Click += (s, e) =>
            {
                if (!hasFile) { MessageBox.Show("Cannot validate — applicant has not uploaded this document yet.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var confirm = MessageBox.Show("Validate this document?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
                try { db.Execute("UPDATE applicant_documents SET status='Validated', hr_remarks=@rm WHERE id=@id", ("@rm", txtR.Text.Trim()), ("@id", capturedDocId)); Audit.Log("Validated document", "applicant_documents", capturedDocId); MessageBox.Show("Document validated successfully.", "Validated", MessageBoxButtons.OK, MessageBoxIcon.Information); new HRManagerApplicantReviewPage(mainForm, capturedAppId); }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            row.Controls.Add(btnVal);
            return row;
        }

        private Label DH(string t, int l, int w) => new Label { Text = t, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = TextMuted, Left = l, Top = 10, Width = w, BackColor = Color.Transparent };

        private Color StatusColor(string s)
        {
            if (s == "Submitted") return AccentBlue;
            if (s == "Under Review") return AccentOrange;
            if (s == "Shortlisted") return AccentGreen;
            if (s == "For Interview") return Color.FromArgb(139, 92, 246);
            if (s == "For Final Review") return Color.FromArgb(180, 176, 66);
            if (s == "Accepted") return AccentGreen;
            if (s == "Rejected") return AccentRed;
            return TextMuted;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantListForm
            // 
            ClientSize = new Size(284, 261);
            Name = "HRManagerApplicantReviewPage";
            Load += HRManagerApplicantReviewPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerApplicantReviewPage_Load(object sender, EventArgs e)
        {

        }
    }
}