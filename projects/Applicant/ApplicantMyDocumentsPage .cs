using project;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HRApplicant
{
    public class ApplicantMyDocumentsPage : Form
    {
        private ApplicantMainForm mainForm;
        private Panel contentPanel;

        public ApplicantMyDocumentsPage(ApplicantMainForm main)
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

            p.Controls.Add(new Label { Text = "My Documents", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });

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
                p.Controls.Add(new Label { Text = "No application yet. Apply for a job first.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = 60, AutoSize = true, BackColor = Color.Transparent });
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
                cmbJobs.Items.Add(r["job_title"] + " — " + r["department"] + "  [" + r["status"] + "]" + "|" + r["id"]);
            cmbJobs.SelectedIndex = 0;
            p.Controls.Add(cmbJobs);

            Panel docContent = new Panel
            {
                Left = 0,
                Top = 78,
                Width = p.Width,
                Height = p.Height - 78,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            p.Controls.Add(docContent);

            Action<int> loadDocs = null;
            loadDocs = (appId) =>
            {
                docContent.Controls.Clear();

                object statusObj = db.Scalar("SELECT status FROM applications WHERE id=@id", ("@id", appId));
                string appStatus = statusObj == null || statusObj == DBNull.Value ? "Draft" : statusObj.ToString();
                bool canUpload = appStatus == "Draft" || appStatus == "Submitted";

                DataTable docs = db.Query(
                    @"SELECT ad.id, rt.name AS doc_name, ad.status, ad.hr_remarks, ad.file_name, ad.uploaded_at
                      FROM applicant_documents ad
                      JOIN requirement_types rt ON rt.id = ad.requirement_type_id
                      WHERE ad.application_id = @aid ORDER BY rt.id",
                    ("@aid", appId));

                int submitted = 0, missing = 0;
                foreach (DataRow r in docs.Rows)
                {
                    if (r["status"].ToString() == "Submitted" || r["status"].ToString() == "Validated") submitted++;
                    if (r["status"].ToString() == "Missing") missing++;
                }

                // Summary badges
                Panel summaryBar = new Panel { Left = 28, Top = 10, Width = p.Width - 56, Height = 50, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                summaryBar.Controls.Add(SummaryBadge("Submitted", submitted.ToString(), Color.FromArgb(60, 180, 100), Color.FromArgb(22, 40, 28), 0));
                summaryBar.Controls.Add(SummaryBadge("Missing", missing.ToString(), Color.FromArgb(220, 80, 80), Color.FromArgb(40, 22, 22), 120));
                summaryBar.Controls.Add(SummaryBadge("Total", docs.Rows.Count.ToString(), Color.FromArgb(80, 160, 220), Color.FromArgb(20, 30, 44), 240));
                docContent.Controls.Add(summaryBar);

                int top = 68;

                // Lock banner
                if (!canUpload)
                {
                    Panel lockBanner = new Panel { Left = 28, Top = top, Width = p.Width - 56, Height = 34, BackColor = Color.FromArgb(45, 38, 22), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                    lockBanner.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(80, 60, 20), 1); e.Graphics.DrawRectangle(pen, 0, 0, lockBanner.Width - 1, lockBanner.Height - 1); pen.Dispose(); };
                    lockBanner.Controls.Add(new Label { Text = "🔒  Uploads locked — HR is reviewing this application.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(220, 160, 60), Left = 12, Height = 34, Width = 500, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent });
                    docContent.Controls.Add(lockBanner);
                    top += 42;
                }

                // Column headers
                Panel hdr = new Panel { Left = 28, Top = top, Width = p.Width - 56, Height = 28, BackColor = Color.FromArgb(26, 34, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                hdr.Controls.Add(MakeHdr("DOCUMENT", 12, 200));
                hdr.Controls.Add(MakeHdr("STATUS", 220, 100));
                hdr.Controls.Add(MakeHdr("FILE", 330, 180));
                hdr.Controls.Add(MakeHdr("UPLOADED", 518, 110));
                hdr.Controls.Add(MakeHdr("HR REMARKS", 636, 180));
                hdr.Controls.Add(MakeHdr("ACTION", 824, 80));
                docContent.Controls.Add(hdr);
                top += 30;

                if (docs.Rows.Count == 0)
                {
                    docContent.Controls.Add(new Label { Text = "No document slots found. Try re-applying.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = top, AutoSize = true, BackColor = Color.Transparent });
                    return;
                }

                foreach (DataRow doc in docs.Rows)
                {
                    Panel row = BuildDocRow(
                        Convert.ToInt32(doc["id"]),
                        doc["doc_name"].ToString(),
                        doc["status"].ToString(),
                        doc["file_name"] == DBNull.Value ? "—" : doc["file_name"].ToString(),
                        doc["uploaded_at"] == DBNull.Value ? "—" : Convert.ToDateTime(doc["uploaded_at"]).ToString("MMM dd, yyyy"),
                        doc["hr_remarks"] == DBNull.Value ? "" : doc["hr_remarks"].ToString(),
                        canUpload, p.Width - 56, mainForm, appId,
                        () => loadDocs(appId));
                    row.Left = 28; row.Top = top;
                    row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    docContent.Controls.Add(row);
                    top += row.Height + 2;
                }
            };

            int firstAppId = Convert.ToInt32(allApps.Rows[0]["id"]);
            loadDocs(firstAppId);

            cmbJobs.SelectedIndexChanged += (s, e) =>
            {
                if (cmbJobs.SelectedIndex < 0) return;
                string selected = cmbJobs.SelectedItem.ToString();
                int selectedAppId = Convert.ToInt32(selected.Split('|')[1]);
                loadDocs(selectedAppId);
            };
        }

        private Panel BuildDocRow(int docId, string name, string status, string fileName,
            string uploadedAt, string remarks, bool canUpload, int width,
            ApplicantMainForm main, int appId, Action refresh)
        {
            Color stColor;
            Color stBg;
            if (status == "Submitted" || status == "Validated")
            { stColor = Color.FromArgb(60, 180, 100); stBg = Color.FromArgb(22, 40, 28); }
            else if (status == "Missing")
            { stColor = Color.FromArgb(220, 80, 80); stBg = Color.FromArgb(40, 22, 22); }
            else if (status == "Rejected")
            { stColor = Color.FromArgb(220, 100, 60); stBg = Color.FromArgb(45, 24, 20); }
            else
            { stColor = Color.FromArgb(120, 120, 120); stBg = Color.FromArgb(30, 30, 30); }

            Panel row = new Panel { Width = width, Height = 46, BackColor = status == "Missing" ? Color.FromArgb(36, 24, 24) : Color.FromArgb(22, 28, 24) };
            row.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1); pen.Dispose(); };

            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 218, 208), Left = 12, Top = 13, Width = 200, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = status.ToUpper(), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = stColor, BackColor = stBg, Width = 90, Height = 20, TextAlign = ContentAlignment.MiddleCenter, Left = 220, Top = 13 });
            row.Controls.Add(new Label { Text = fileName, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(120, 145, 132), Left = 330, Top = 13, Width = 180, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = uploadedAt, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 120, 110), Left = 518, Top = 13, Width = 110, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = remarks, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(110, 130, 120), Left = 636, Top = 13, Width = 180, BackColor = Color.Transparent });

            // View button
            if (status == "Submitted" || status == "Validated")
            {
                Button btnView = new Button
                {
                    Text = "View",
                    Width = 70,
                    Height = 28,
                    Top = 9,
                    Left = width - 172,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.FromArgb(25, 55, 80),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5f),
                    Cursor = Cursors.Hand
                };
                btnView.FlatAppearance.BorderSize = 0;
                btnView.Click += (s, e) =>
                {
                    try
                    {
                        object fp = new DatabaseConnection().Scalar(
                            "SELECT file_path FROM applicant_documents WHERE id=@id",
                            ("@id", docId));

                        if (fp == null || fp == DBNull.Value || string.IsNullOrEmpty(fp.ToString()))
                        {
                            MessageBox.Show("File path not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string filePath = fp.ToString();
                        if (!System.IO.File.Exists(filePath))
                        {
                            MessageBox.Show("File not found:\n" + filePath, "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot open file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                row.Controls.Add(btnView);
            }

            if (canUpload && status != "Validated")
            {
                string btnTxt = status == "Submitted" ? "Replace" : "Upload";
                Color btnCol = status == "Submitted" ? Color.FromArgb(28, 60, 80) : Color.FromArgb(28, 70, 48);
                Button btn = new Button { Text = btnTxt, Left = 824, Top = 10, Width = 76, Height = 26, BackColor = btnCol, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
                int capturedDocId = docId;
                string capturedName = name;
                btn.Click += (s, e) =>
                {
                    OpenFileDialog ofd = new OpenFileDialog { Title = "Select file for: " + capturedName, Filter = "Documents|*.pdf;*.docx;*.jpg;*.jpeg;*.png|All Files|*.*" };
                    if (ofd.ShowDialog() != DialogResult.OK) { ofd.Dispose(); return; }
                    try
                    {
                        string uploadDir = Path.Combine(Application.StartupPath, "uploads");
                        Directory.CreateDirectory(uploadDir);
                        string ext = Path.GetExtension(ofd.FileName);
                        string safeName = "APP" + appId + "_DOC" + capturedDocId + ext;
                        string destPath = Path.Combine(uploadDir, safeName);
                        File.Copy(ofd.FileName, destPath, true);
                        new DatabaseConnection().Execute(
                            "UPDATE applicant_documents SET status='Submitted', file_name=@fn, file_path=@fp, uploaded_at=NOW() WHERE id=@id",
                            ("@fn", ofd.SafeFileName), ("@fp", destPath), ("@id", capturedDocId));
                        Audit.Log("Uploaded document: " + capturedName, "applicant_documents", capturedDocId);
                        MessageBox.Show(capturedName + " uploaded successfully.", "Uploaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        refresh();
                    }
                    catch (Exception ex) { MessageBox.Show("Upload failed:\n" + ex.Message); }
                    ofd.Dispose();
                };
                row.Controls.Add(btn);
            }
            return row;
        }

        private Panel SummaryBadge(string label, string value, Color accent, Color bg, int left)
        {
            Panel badge = new Panel { Left = left, Top = 4, Width = 108, Height = 42, BackColor = bg };
            badge.Paint += (s, e) => { Pen pen = new Pen(accent, 1); e.Graphics.DrawRectangle(pen, 0, 0, badge.Width - 1, badge.Height - 1); pen.Dispose(); };
            badge.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = accent, Left = 12, Top = 4, AutoSize = true, BackColor = Color.Transparent });
            badge.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = 12, Top = 26, AutoSize = true, BackColor = Color.Transparent });
            return badge;
        }

        private Label MakeHdr(string text, int left, int width)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 110, 95), Left = left, Top = 7, Width = width, BackColor = Color.Transparent };
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantStatusTrackingPage
            // 
            ClientSize = new Size(284, 261);
            Name = "ApplicantMyDocumentsPage";
            Load += ApplicantMyDocumentsPage_Load;
            ResumeLayout(false);

        }

        private void ApplicantMyDocumentsPage_Load(object sender, EventArgs e)
        {

        }
    }
}