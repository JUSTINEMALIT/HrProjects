using System;
using System.Data;
using System.Drawing;
using project;
using System.IO;
using System.Windows.Forms;

namespace HRApplicant
{
    public static class ApplicantMyDocumentsPage
    {
        public static void Show(ApplicantMainForm main)
        {
            var db = new DatabaseConnection();
            main.ClearContent();
            var p = main.contentPanel;

            p.Controls.Add(new Label { Text = "My Documents", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label { Text = "Upload or replace your application requirements.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 46, AutoSize = true, BackColor = Color.Transparent });

            // Get latest application
            DataTable appDt = db.Query(
                "SELECT id, status FROM applications WHERE applicant_id=@aid ORDER BY created_at DESC LIMIT 1",
                ("@aid", Session.ApplicantId));

            if (appDt.Rows.Count == 0)
            {
                p.Controls.Add(new Label { Text = "No application found. Apply for a job first.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(150, 150, 150), Left = 28, Top = 76, AutoSize = true, BackColor = Color.Transparent });
                return;
            }

            int appId = Convert.ToInt32(appDt.Rows[0]["id"]);
            string appStatus = appDt.Rows[0]["status"].ToString();
            bool canUpload = appStatus == "Draft" || appStatus == "Submitted";

            // Get documents
            DataTable docs = db.Query(
                @"SELECT ad.id, rt.name AS doc_name, ad.status, ad.hr_remarks, ad.file_name, ad.uploaded_at, rt.id AS req_type_id
                  FROM applicant_documents ad
                  JOIN requirement_types rt ON rt.id = ad.requirement_type_id
                  WHERE ad.application_id = @aid
                  ORDER BY rt.id",
                ("@aid", appId));

            // Summary counts
            int submitted = 0, missing = 0, total = 0;
            foreach (DataRow r in docs.Rows)
            {
                total++;
                if (r["status"].ToString() == "Submitted" || r["status"].ToString() == "Validated") submitted++;
                if (r["status"].ToString() == "Missing") missing++;
            }

            // Summary badges
            Panel summaryBar = new Panel { Left = 28, Top = 70, Width = p.Width - 56, Height = 50, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            summaryBar.Controls.Add(SummaryBadge("Submitted", submitted.ToString(), Color.FromArgb(60, 180, 100), Color.FromArgb(22, 40, 28), 0));
            summaryBar.Controls.Add(SummaryBadge("Missing", missing.ToString(), Color.FromArgb(220, 80, 80), Color.FromArgb(40, 22, 22), 120));
            summaryBar.Controls.Add(SummaryBadge("Total", total.ToString(), Color.FromArgb(80, 160, 220), Color.FromArgb(20, 30, 44), 240));
            p.Controls.Add(summaryBar);

            // Lock banner
            if (!canUpload)
            {
                Panel lockBanner = new Panel { Left = 28, Top = 126, Width = p.Width - 56, Height = 34, BackColor = Color.FromArgb(45, 38, 22), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                lockBanner.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(80, 60, 20), 1); e.Graphics.DrawRectangle(pen, 0, 0, lockBanner.Width - 1, lockBanner.Height - 1); pen.Dispose(); };
                lockBanner.Controls.Add(new Label { Text = "Document uploads are locked — HR is now reviewing your application.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(220, 160, 60), Left = 12, Height = 34, AutoSize = false, Width = 500, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent });
                p.Controls.Add(lockBanner);
            }

            int tableTop = canUpload ? 128 : 168;

            // Column headers
            Panel hdr = new Panel { Left = 28, Top = tableTop, Width = p.Width - 56, Height = 28, BackColor = Color.FromArgb(26, 34, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            hdr.Controls.Add(MakeHdr("DOCUMENT", 12, 200));
            hdr.Controls.Add(MakeHdr("STATUS", 220, 100));
            hdr.Controls.Add(MakeHdr("FILE", 330, 180));
            hdr.Controls.Add(MakeHdr("HR REMARKS", 520, 200));
            hdr.Controls.Add(MakeHdr("ACTION", 730, 80));
            p.Controls.Add(hdr);

            int rowTop = tableTop + 30;
            foreach (DataRow doc in docs.Rows)
            {
                int docId = Convert.ToInt32(doc["id"]);
                string docName = doc["doc_name"].ToString();
                string docSt = doc["status"].ToString();
                string fileName = doc["file_name"] == DBNull.Value ? "—" : doc["file_name"].ToString();
                string remarks = doc["hr_remarks"] == DBNull.Value ? "" : doc["hr_remarks"].ToString();

                Panel row = BuildDocRow(docId, docName, docSt, fileName, remarks, canUpload, p.Width - 56, main, appId);
                row.Left = 28; row.Top = rowTop;
                row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                p.Controls.Add(row);
                rowTop += row.Height + 2;
            }
        }

        private static Panel BuildDocRow(int docId, string name, string status, string fileName,
            string remarks, bool canUpload, int width, ApplicantMainForm main, int appId)
        {
            Color rowBg = status == "Missing" ? Color.FromArgb(36, 24, 24) : Color.FromArgb(22, 28, 24);
            Panel row = new Panel { Width = width, Height = 46, BackColor = rowBg };
            row.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1); pen.Dispose(); };

            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 218, 208), Left = 12, Top = 14, Width = 200, BackColor = Color.Transparent });

            // Status badge
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

            row.Controls.Add(new Label { Text = status.ToUpper(), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = stColor, BackColor = stBg, Width = 84, Height = 20, TextAlign = ContentAlignment.MiddleCenter, Left = 220, Top = 13 });
            row.Controls.Add(new Label { Text = fileName, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(120, 145, 132), Left = 330, Top = 14, Width = 180, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = remarks, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(110, 130, 120), Left = 520, Top = 14, Width = 200, BackColor = Color.Transparent });

            if (canUpload && status != "Validated")
            {
                string btnTxt = (status == "Submitted") ? "Replace" : "Upload";
                Color btnCol = (status == "Submitted") ? Color.FromArgb(28, 60, 80) : Color.FromArgb(28, 70, 48);
                Button btn = new Button
                {
                    Text = btnTxt,
                    Left = 730,
                    Top = 10,
                    Width = 76,
                    Height = 26,
                    BackColor = btnCol,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5f),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                int capturedDocId = docId;
                string capturedName = name;
                btn.Click += (s, e) =>
                {
                    var db = new DatabaseConnection();
                    OpenFileDialog ofd = new OpenFileDialog
                    {
                        Title = "Select file for: " + capturedName,
                        Filter = "Documents|*.pdf;*.docx;*.jpg;*.jpeg;*.png|All Files|*.*"
                    };
                    if (ofd.ShowDialog() != DialogResult.OK) { ofd.Dispose(); return; }

                    try
                    {
                        // Save file to uploads folder
                        string uploadDir = Path.Combine(Application.StartupPath, "uploads");
                        Directory.CreateDirectory(uploadDir);
                        string ext = Path.GetExtension(ofd.FileName);
                        string safeName = "APP" + appId + "_DOC" + capturedDocId + ext;
                        string destPath = Path.Combine(uploadDir, safeName);
                        File.Copy(ofd.FileName, destPath, true);

                        db.Execute(
                            @"UPDATE applicant_documents SET status='Submitted', file_name=@fn, file_path=@fp, uploaded_at=NOW()
                              WHERE id=@id",
                            ("@fn", ofd.SafeFileName), ("@fp", destPath), ("@id", capturedDocId));

                        Audit.Log("Uploaded document: " + capturedName, "applicant_documents", capturedDocId);
                        MessageBox.Show(capturedName + " uploaded successfully.", "Uploaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ApplicantMyDocumentsPage.Show(main); // refresh
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Upload failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    ofd.Dispose();
                };
                row.Controls.Add(btn);
            }

            return row;
        }

        private static Panel SummaryBadge(string label, string value, Color accent, Color bg, int left)
        {
            Panel badge = new Panel { Left = left, Top = 4, Width = 108, Height = 42, BackColor = bg };
            badge.Paint += (s, e) => { Pen pen = new Pen(accent, 1); e.Graphics.DrawRectangle(pen, 0, 0, badge.Width - 1, badge.Height - 1); pen.Dispose(); };
            badge.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = accent, Left = 12, Top = 4, AutoSize = true, BackColor = Color.Transparent });
            badge.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = 12, Top = 26, AutoSize = true, BackColor = Color.Transparent });
            return badge;
        }

        private static Label MakeHdr(string text, int left, int width)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 110, 95), Left = left, Top = 7, Width = width, BackColor = Color.Transparent };
        }
    }
}