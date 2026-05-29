using System;
using System.Data;
using System.Drawing;
using project;
using System.Windows.Forms;

namespace HRApplicant
{
    public static class ApplicantMyApplicationPage
    {
        public static void Show(ApplicantMainForm main)
        {
            var db = new DatabaseConnection();
            main.ClearContent();
            var p = main.contentPanel;

            p.Controls.Add(new Label { Text = "My Application", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });

            // Load application
            DataTable appDt = db.Query(
                @"SELECT a.id, a.status, a.expected_salary, a.preferred_start_date, a.employment_type_pref,
                         a.referral_source, a.cover_letter, a.submitted_at, a.created_at,
                         jv.title AS job_title, jv.employment_type, d.name AS department
                  FROM applications a
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  JOIN departments d   ON d.id   = jv.department_id
                  WHERE a.applicant_id = @aid
                  ORDER BY a.created_at DESC LIMIT 1",
                ("@aid", Session.ApplicantId));

            if (appDt.Rows.Count == 0)
            {
                p.Controls.Add(new Label
                {
                    Text = "You have no application yet.\nGo to Job Vacancies to apply for a position.",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.FromArgb(150, 150, 150),
                    Left = 28,
                    Top = 70,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
                return;
            }

            DataRow app = appDt.Rows[0];
            int appId = Convert.ToInt32(app["id"]);
            string status = app["status"].ToString();
            bool isEditable = status == "Draft" || status == "Submitted";

            main.applicationStatus = status;

            // Sub-label
            p.Controls.Add(new Label
            {
                Text = isEditable ? "You can still edit your application." : "Application is locked — HR is reviewing your submission.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = isEditable ? Color.FromArgb(100, 130, 115) : Color.FromArgb(200, 140, 60),
                Left = 29,
                Top = 46,
                AutoSize = true,
                BackColor = Color.Transparent
            });

            // Lock banner
            if (!isEditable)
            {
                Panel lockBanner = new Panel { Left = 28, Top = 70, Width = p.Width - 56, Height = 34, BackColor = Color.FromArgb(45, 38, 22), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                lockBanner.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(80, 60, 20), 1); e.Graphics.DrawRectangle(pen, 0, 0, lockBanner.Width - 1, lockBanner.Height - 1); pen.Dispose(); };
                lockBanner.Controls.Add(new Label { Text = "Application locked — Status: " + status, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(220, 160, 60), Left = 12, Top = 0, Height = 34, Width = 500, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent });
                p.Controls.Add(lockBanner);
            }

            int formTop = isEditable ? 76 : 114;

            // Job applied for
            Panel jobCard = new Panel { Left = 28, Top = formTop, Width = p.Width - 56, Height = 66, BackColor = Color.FromArgb(22, 32, 26), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            jobCard.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 60, 44), 1); e.Graphics.DrawRectangle(pen, 0, 0, jobCard.Width - 1, jobCard.Height - 1); pen.Dispose(); };
            jobCard.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 66, BackColor = Color.FromArgb(60, 180, 100) });
            jobCard.Controls.Add(new Label { Text = "JOB APPLIED FOR", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 130, 100), Left = 16, Top = 8, AutoSize = true, BackColor = Color.Transparent });
            jobCard.Controls.Add(new Label { Text = app["job_title"] + "  —  " + app["department"], Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(200, 230, 215), Left = 16, Top = 26, AutoSize = true, BackColor = Color.Transparent });
            jobCard.Controls.Add(new Label
            {
                Text = app["employment_type"] + "   •   App ID: APP-" + appId.ToString("D4") + "   •   Status: " + status,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(100, 130, 115),
                Left = 16,
                Top = 48,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            p.Controls.Add(jobCard);
            formTop += 76;

            // Form fields
            int colW = (p.Width - 56 - 28 - 12) / 2;
            Panel card = new Panel { Left = 28, Top = formTop, Width = p.Width - 56, Height = 280, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            card.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); pen.Dispose(); };
            card.Controls.Add(new Label { Text = "Application Details", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 200, 130), Left = 14, Top = 10, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Panel { Left = 0, Top = 34, Height = 1, Width = card.Width, BackColor = Color.FromArgb(35, 50, 42), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });

            string SafeStr(object v) => v == DBNull.Value ? "" : v.ToString();
            string SafeDate(object v) => v == DBNull.Value ? "" : Convert.ToDateTime(v).ToString("yyyy-MM-dd");

            string[] fieldLabels = { "Expected Salary", "Preferred Start Date (YYYY-MM-DD)", "Employment Type Preferred", "Referral / Source" };
            string[] fieldVals = {
                SafeStr(app["expected_salary"]),
                SafeDate(app["preferred_start_date"]),
                SafeStr(app["employment_type_pref"]),
                SafeStr(app["referral_source"])
            };

            TextBox[] fTxt = new TextBox[fieldLabels.Length];
            for (int i = 0; i < fieldLabels.Length; i++)
            {
                int col = i % 2, row = i / 2;
                int fx = col == 0 ? 14 : colW + 26;
                int fy = 42 + row * 58;
                card.Controls.Add(new Label { Text = fieldLabels[i], Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = fx, Top = fy, Width = colW, BackColor = Color.Transparent });
                TextBox t = new TextBox { Text = fieldVals[i], Left = fx, Top = fy + 16, Width = colW - 8, Height = 22, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f), ReadOnly = !isEditable };
                fTxt[i] = t;
                card.Controls.Add(t);
            }

            // Cover letter
            card.Controls.Add(new Label { Text = "Cover Letter / Remarks", Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = 14, Top = 158, BackColor = Color.Transparent, AutoSize = true });
            TextBox txtCover = new TextBox
            {
                Text = SafeStr(app["cover_letter"]),
                Left = 14,
                Top = 174,
                Width = card.Width - 28,
                Height = 72,
                Multiline = true,
                BackColor = Color.FromArgb(26, 34, 28),
                ForeColor = Color.FromArgb(190, 210, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
                ReadOnly = !isEditable,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(txtCover);
            p.Controls.Add(card);
            formTop += 290;

            // Action buttons
            if (isEditable)
            {
                Button btnSave = MakeBtn("Save Draft", Color.FromArgb(28, 70, 48), formTop, 28);
                btnSave.Click += (s, e) =>
                {
                    try
                    {
                        db.Execute(
                            @"UPDATE applications SET expected_salary=@sal, preferred_start_date=@psd,
                              employment_type_pref=@etp, referral_source=@ref, cover_letter=@cl
                              WHERE id=@id",
                            ("@sal", string.IsNullOrWhiteSpace(fTxt[0].Text) ? (object)DBNull.Value : fTxt[0].Text.Trim()),
                            ("@psd", string.IsNullOrWhiteSpace(fTxt[1].Text) ? (object)DBNull.Value : fTxt[1].Text.Trim()),
                            ("@etp", fTxt[2].Text.Trim()),
                            ("@ref", fTxt[3].Text.Trim()),
                            ("@cl", txtCover.Text.Trim()),
                            ("@id", appId));
                        Audit.Log("Saved application draft", "applications", appId);
                        MessageBox.Show("Application saved as draft.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                };

                Button btnSubmit = MakeBtn("Submit Application", Color.FromArgb(20, 100, 60), formTop, 144);
                btnSubmit.Width = 160;
                btnSubmit.Click += (s, e) =>
                {
                    if (status != "Draft" && status != "Submitted")
                    {
                        MessageBox.Show("Application cannot be submitted at this stage.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var r = MessageBox.Show("Submit your application?\nYou can still edit until HR starts review.", "Submit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r != DialogResult.Yes) return;
                    try
                    {
                        db.Execute(
                            @"UPDATE applications SET expected_salary=@sal, preferred_start_date=@psd,
                              employment_type_pref=@etp, referral_source=@ref, cover_letter=@cl,
                              status='Submitted', submitted_at=NOW() WHERE id=@id",
                            ("@sal", string.IsNullOrWhiteSpace(fTxt[0].Text) ? (object)DBNull.Value : fTxt[0].Text.Trim()),
                            ("@psd", string.IsNullOrWhiteSpace(fTxt[1].Text) ? (object)DBNull.Value : fTxt[1].Text.Trim()),
                            ("@etp", fTxt[2].Text.Trim()),
                            ("@ref", fTxt[3].Text.Trim()),
                            ("@cl", txtCover.Text.Trim()),
                            ("@id", appId));
                        db.Execute(
                            "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,'Submitted','Applicant submitted application.',@who)",
                            ("@id", appId), ("@who", Session.FullName));
                        Audit.Log("Submitted application", "applications", appId);
                        MessageBox.Show("Application submitted successfully!", "Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ApplicantMyApplicationPage.Show(main);
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                };

                // Withdraw button
                Button btnWithdraw = MakeBtn("Withdraw", Color.FromArgb(70, 30, 30), formTop, 318);
                btnWithdraw.ForeColor = Color.FromArgb(220, 130, 130);
                btnWithdraw.Click += (s, e) =>
                {
                    var r = MessageBox.Show("Are you sure you want to withdraw your application?\nThis action cannot be undone.", "Withdraw", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                    try
                    {
                        db.Execute("UPDATE applications SET status='Withdrawn' WHERE id=@id", ("@id", appId));
                        db.Execute(
                            "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,'Withdrawn','Applicant withdrew application.',@who)",
                            ("@id", appId), ("@who", Session.FullName));
                        Audit.Log("Withdrew application", "applications", appId);
                        MessageBox.Show("Application withdrawn.", "Withdrawn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ApplicantMyApplicationPage.Show(main);
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                };

                p.Controls.Add(btnSave);
                p.Controls.Add(btnSubmit);
                p.Controls.Add(btnWithdraw);
            }
        }

        private static Button MakeBtn(string text, Color bg, int top, int left)
        {
            Button btn = new Button { Text = text, Left = left, Top = top, Width = 110, Height = 34, BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}