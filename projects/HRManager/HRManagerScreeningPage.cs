using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRManager
{
    public partial class HRManagerScreeningPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerScreeningPage(HRManagerMainForm main)
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

            p.Controls.Add(new Label { Text = "Screening", Font = new Font("Segoe UI", 26f, FontStyle.Bold), ForeColor = TextPrimary, Left = 24, Top = top, AutoSize = true, BackColor = Color.Transparent });
            top += 36;
            p.Controls.Add(new Label { Text = "Mark applicants as Qualified or Not Qualified.", Font = new Font("Segoe UI", 10f), ForeColor = TextSecondary, Left = 24, Top = 80, AutoSize = true, BackColor = Color.Transparent });
            top += 28;

            DataTable applicants = db.Query(
                @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS name, jv.title AS job,
                         sr.result AS screening_result
                  FROM applications a
                  JOIN applicants ap ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  LEFT JOIN screening_results sr ON sr.application_id = a.id
                  WHERE a.status = 'Submitted' OR a.status = 'Under Review'
                  LIMIT 20");

            int itemTop = top;
            foreach (DataRow row in applicants.Rows)
            {
                int appId = Convert.ToInt32(row["id"]);
                string screeningResult = row["screening_result"] == DBNull.Value ? "Pending" : row["screening_result"].ToString();

                Panel appCard = new Panel { Left = 24, Top = itemTop, Width = p.Width - 56, Height = 100, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                appCard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, appCard.Width - 1, appCard.Height - 1);
                };

                appCard.Controls.Add(new Label { Text = row["name"].ToString(), Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold), ForeColor = TextPrimary, Left = 12, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                appCard.Controls.Add(new Label { Text = "Position: " + row["job"], Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 12, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                appCard.Controls.Add(new Label { Text = "Result: " + screeningResult, Font = new Font("Segoe UI", 9f), ForeColor = screeningResult == "Qualified" ? AccentGreen : screeningResult == "Not Qualified" ? AccentRed : TextSecondary, Left = 12, Top = 46, AutoSize = true, BackColor = Color.Transparent });

                Button btnQualified = new Button { Text = "✓ Qualified", Left = appCard.Width - 260, Top = 24, Width = 80, Height = 32, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                btnQualified.FlatAppearance.BorderSize = 0;
                btnQualified.Click += (s, e) => ShowScreeningDialog(appId, "Qualified");
                appCard.Controls.Add(btnQualified);

                Button btnNotQualified = new Button { Text = "✗ Not Qualified", Left = appCard.Width - 170, Top = 24, Width = 150, Height = 32, BackColor = AccentRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                btnNotQualified.FlatAppearance.BorderSize = 0;
                btnNotQualified.Click += (s, e) => ShowScreeningDialog(appId, "Not Qualified");
                appCard.Controls.Add(btnNotQualified);

                p.Controls.Add(appCard);
                itemTop += 106;
            }
        }

        private void ShowScreeningDialog(int appId, string result)
        {
            Form dialog = new Form
            {
                Text = result == "Qualified" ? "Screening — Qualified" : "Screening — Not Qualified",
                Width = 450,
                Height = 300,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            int y = 20;

            dialog.Controls.Add(new Label
            {
                Text = result == "Qualified" ? "Remarks (optional):" : "Remarks (required):",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Left = 20,
                Top = y,
                AutoSize = true
            });
            y += 24;

            RichTextBox txtRemarks = new RichTextBox
            {
                Left = 20,
                Top = y,
                Width = 390,
                Height = 120,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.White
            };
            dialog.Controls.Add(txtRemarks);
            y += 130;

            Button btnSave = new Button
            {
                Text = result == "Qualified" ? "✓ Mark as Qualified" : "✗ Mark as Not Qualified",
                Left = 20,
                Top = y,
                Width = 200,
                Height = 36,
                BackColor = result == "Qualified" ? AccentGreen : AccentRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                string remarks = txtRemarks.Text.Trim();
                if (result == "Not Qualified" && remarks.Length == 0)
                {
                    MessageBox.Show("Please enter remarks for disqualification.", "Remarks Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                RecordScreening(appId, result, remarks);
                dialog.Close();
            };
            dialog.Controls.Add(btnSave);
            dialog.ShowDialog();
        }

        private void RecordScreening(int appId, string result, string remarks = "")
        {
            db.Execute("DELETE FROM screening_results WHERE application_id = @id", ("@id", appId));
            db.Execute(
                @"INSERT INTO screening_results (application_id, result, remarks, screened_by, screened_at)
                  VALUES (@appId, @result, @remarks, @screened_by, NOW())",
                ("@appId", appId),
                ("@result", result),
                ("@remarks", remarks),
                ("@screened_by", Session.AdminFullName));

            string newStatus = result == "Qualified" ? "Shortlisted" : "Rejected";
            db.Execute("UPDATE applications SET status = @status WHERE id = @id",
                ("@status", newStatus), ("@id", appId));

            string historyRemark = result == "Qualified"
                ? (remarks.Length > 0 ? remarks : "Applicant passed initial screening.")
                : remarks;

            db.Execute(
                @"INSERT INTO application_status_history (application_id, status, remarks, changed_by)
                  VALUES (@id, @status, @remarks, @who)",
                ("@id", appId),
                ("@status", newStatus),
                ("@remarks", historyRemark),
                ("@who", Session.AdminFullName));

            Audit.Log("Screening Recorded", "screening_results", appId, result);
            MessageBox.Show($"Applicant marked as {result}.", "Screening Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            InitializePage();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerScreeningPage";
            Load += HRManagerScreeningPage_Load;
            ResumeLayout(false);
        }

        private void HRManagerScreeningPage_Load(object sender, EventArgs e) { }
    }
}
