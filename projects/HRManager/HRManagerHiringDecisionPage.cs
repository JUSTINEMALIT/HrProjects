using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


//hr manager/admin hiringdecision

namespace projects.HRManager
{
    public partial class HRManagerHiringDecisionPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AcceptGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color RejectRed = Color.FromArgb(239, 68, 68);
        private static readonly Color HoldOrange = Color.FromArgb(249, 115, 22);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerHiringDecisionPage(HRManagerMainForm main)
        {
            this.mainForm = main;
            this.contentPanel = main.contentPanel;
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

            p.Controls.Add(new Label
            {
                Text = "Final Hiring Decision",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 24,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 36;

            p.Controls.Add(new Label
            {
                Text = "Review and make final hiring decisions for applicants.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = 70,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 28;

            // ✅ UPDATED QUERY: Show "For Final Review" OR "On Hold" (pending decisions)
            DataTable pending = db.Query(
                @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS applicant_name, ap.email,
                         jv.title AS job_title, d.name AS department,
                         (SELECT GROUP_CONCAT(remarks) FROM screening_results WHERE application_id = a.id) AS screening_remarks,
                         (SELECT AVG(score) FROM interview_evaluations ie 
                          JOIN interview_schedules ist ON ist.id = ie.interview_id 
                          WHERE ist.application_id = a.id) AS avg_score
                  FROM applications a
                  JOIN applicants ap ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  JOIN departments d ON d.id = jv.department_id
                  WHERE a.status IN ('For Final Review', 'On Hold')
                  ORDER BY a.created_at DESC");

            if (pending.Rows.Count == 0)
            {
                Panel emptyCard = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 80, BackColor = BgCard };
                emptyCard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, emptyCard.Width - 1, emptyCard.Height - 1);
                };
                emptyCard.Controls.Add(new Label { Text = "✓ No pending final decisions", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = TextPrimary, Left = 24, Top = 12, AutoSize = true, BackColor = Color.Transparent });
                emptyCard.Controls.Add(new Label { Text = "All applicants have been reviewed and decided upon.", Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 24, Top = 38, AutoSize = true, BackColor = Color.Transparent });
                p.Controls.Add(emptyCard);
                return;
            }

            foreach (DataRow row in pending.Rows)
            {
                int appId = Convert.ToInt32(row["id"]);

                Panel appCard = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 200, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                appCard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, appCard.Width - 1, appCard.Height - 1);
                };

                appCard.Controls.Add(new Label { Text = row["applicant_name"].ToString(), Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold), ForeColor = TextPrimary, Left = 24, Top = 16, AutoSize = true, BackColor = Color.Transparent });
                appCard.Controls.Add(new Label { Text = row["email"].ToString(), Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 24, Top = 36, AutoSize = true, BackColor = Color.Transparent });
                appCard.Controls.Add(new Label { Text = "Position: " + row["job_title"] + " • " + row["department"], Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 24, Top = 52, AutoSize = true, BackColor = Color.Transparent });

                if (row["avg_score"] != DBNull.Value)
                {
                    decimal score = Convert.ToDecimal(row["avg_score"]);
                    appCard.Controls.Add(new Label { Text = $"Interview Score: {score:F2}/100", Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 24, Top = 68, AutoSize = true, BackColor = Color.Transparent });
                }

                if (row["screening_remarks"] != DBNull.Value && !string.IsNullOrEmpty(row["screening_remarks"].ToString()))
                {
                    appCard.Controls.Add(new Label { Text = "Screening: " + row["screening_remarks"].ToString().Substring(0, Math.Min(60, row["screening_remarks"].ToString().Length)) + "...", Font = new Font("Segoe UI", 8.5f), ForeColor = TextMuted, Left = 24, Top = 84, Width = appCard.Width - 48, AutoSize = false, BackColor = Color.Transparent });
                }

                Button btnAccept = new Button { Text = "✓ Accept", Left = 24, Top = 140, Width = 100, Height = 36, BackColor = AcceptGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f), Cursor = Cursors.Hand };
                btnAccept.FlatAppearance.BorderSize = 0;
                btnAccept.Click += (s, e) => ShowDecisionDialog(appId, "Accepted");
                appCard.Controls.Add(btnAccept);

                Button btnReject = new Button { Text = "✗ Reject", Left = 132, Top = 140, Width = 100, Height = 36, BackColor = RejectRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f), Cursor = Cursors.Hand };
                btnReject.FlatAppearance.BorderSize = 0;
                btnReject.Click += (s, e) => ShowDecisionDialog(appId, "Rejected");
                appCard.Controls.Add(btnReject);

                Button btnHold = new Button { Text = "⏸ Hold", Left = 240, Top = 140, Width = 100, Height = 36, BackColor = HoldOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f), Cursor = Cursors.Hand };
                btnHold.FlatAppearance.BorderSize = 0;
                btnHold.Click += (s, e) => ShowDecisionDialog(appId, "On Hold");
                appCard.Controls.Add(btnHold);

                Button btnDetails = new Button { Text = "View Details", Left = appCard.Width - 144, Top = 140, Width = 120, Height = 36, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
                btnDetails.FlatAppearance.BorderSize = 0;
                int capturedAppId = appId;
                btnDetails.Click += (s, e) => new HRManagerApplicantReviewPage(mainForm, capturedAppId);
                appCard.Controls.Add(btnDetails);

                p.Controls.Add(appCard);
                top += 210;
            }
        }

        private void ShowDecisionDialog(int applicationId, string decision)
        {
            Form dialogForm = new Form
            {
                Text = $"Hiring Decision - {decision}",
                Width = 500,
                Height = 350,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(241, 245, 249)
            };

            int y = 20;

            dialogForm.Controls.Add(new Label { Text = "Final Remarks:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 28;

            RichTextBox txtRemarks = new RichTextBox { Left = 20, Top = y, Width = 440, Height = 100, Font = new Font("Segoe UI", 9f), BackColor = Color.White, ForeColor = Color.FromArgb(15, 23, 42) };
            dialogForm.Controls.Add(txtRemarks);
            y += 110;

            DateTimePicker dtpStartDate = null;
            if (decision == "Accepted")
            {
                dialogForm.Controls.Add(new Label { Text = "Start Date:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
                y += 28;
                dtpStartDate = new DateTimePicker { Left = 20, Top = y, Width = 200 };
                dialogForm.Controls.Add(dtpStartDate);
                y += 40;
            }

            Button btnConfirm = new Button
            {
                Text = $"Confirm {decision}",
                Left = 20,
                Top = y,
                Width = 200,
                Height = 36,
                BackColor = decision == "Accepted" ? Color.FromArgb(34, 197, 94) : decision == "Rejected" ? Color.FromArgb(239, 68, 68) : Color.FromArgb(249, 115, 22),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10f),
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += (s, e) =>
            {
                try
                {
                    string remarks = txtRemarks.Text;
                    string startDate = decision == "Accepted" && dtpStartDate != null
                        ? dtpStartDate.Value.ToString("yyyy-MM-dd")
                        : null;

                    // ✅ CHECK if hiring decision already exists
                    object existingCount = db.Scalar(
                        "SELECT COUNT(*) FROM hiring_decisions WHERE application_id = @appId",
                        ("@appId", applicationId));

                    bool exists = existingCount != null && Convert.ToInt32(existingCount) > 0;

                    if (exists)
                    {
                        // ✅ UPDATE existing hiring decision
                        db.Execute(
                            @"UPDATE hiring_decisions 
                              SET decision = @decision, remarks = @remarks, decided_by = @decidedBy, 
                                  start_date = @startDate, decided_at = NOW()
                              WHERE application_id = @appId",
                            ("@decision", decision),
                            ("@remarks", remarks),
                            ("@decidedBy", AdminSession.AdminFullName),
                            ("@startDate", (object)startDate ?? DBNull.Value),
                            ("@appId", applicationId));
                    }
                    else
                    {
                        // ✅ INSERT new hiring decision
                        db.Execute(
                            @"INSERT INTO hiring_decisions (application_id, decision, remarks, decided_by, start_date)
                              VALUES (@appId, @decision, @remarks, @decidedBy, @startDate)",
                            ("@appId", applicationId),
                            ("@decision", decision),
                            ("@remarks", remarks),
                            ("@decidedBy", AdminSession.AdminFullName),
                            ("@startDate", (object)startDate ?? DBNull.Value));
                    }

                    // ✅ UPDATE application status
                    db.Execute(
                        "UPDATE applications SET status = @status WHERE id = @appId",
                        ("@status", decision), ("@appId", applicationId));

                    // ✅ INSERT status history
                    db.Execute(
                        @"INSERT INTO application_status_history (application_id, status, remarks, changed_by)
                          VALUES (@appId, @status, @remarks, @changedBy)",
                        ("@appId", applicationId),
                        ("@status", decision),
                        ("@remarks", remarks),
                        ("@changedBy", AdminSession.AdminFullName));

                    // ✅ Log audit
                    Audit.Log($"Hiring Decision: {decision}", "hiring_decisions", applicationId, remarks);

                    MessageBox.Show($"Decision recorded successfully!\n\nApplicant: {decision}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialogForm.Close();

                    // ✅ RELOAD PAGE
                    InitializePage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            dialogForm.Controls.Add(btnConfirm);

            Button btnCancel = new Button { Text = "Cancel", Left = 230, Top = y, Width = 230, Height = 36, BackColor = Color.FromArgb(200, 200, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            btnCancel.FlatAppearance.BorderSize = 0;
            dialogForm.Controls.Add(btnCancel);

            dialogForm.ShowDialog();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerHiringDecisionPage";
            Load += HRManagerHiringDecisionPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerHiringDecisionPage_Load(object sender, EventArgs e)
        {

        }
    }
}