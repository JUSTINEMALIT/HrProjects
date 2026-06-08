using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


//hr manager/admin interview evaluation

namespace projects.HRManager
{
    public partial class HRManagerInterviewEvaluationPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);

        private HRManagerMainForm mainForm;
        private DatabaseConnection db;

        public HRManagerInterviewEvaluationPage(HRManagerMainForm main = null)
        {
            mainForm = main;
            db = new DatabaseConnection();

            if (mainForm != null)
            {
                mainForm.ClearContent();
                LoadIntoMainForm();
            }
        }

        private void LoadIntoMainForm()
        {
            var p = mainForm.contentPanel;
            p.BackColor = BgPage;
            p.AutoScroll = true;

            int top = 24;

            p.Controls.Add(new Label
            {
                Text = "Interview Evaluation",
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
                Text = "Record interview scores, remarks, and evaluation results.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 32;

            // ✅ UPDATED QUERY: Exclude Accepted/Rejected applicants
            DataTable interviews = db.Query(
                @"SELECT ist.id, a.id AS app_id, CONCAT(ap.first_name,' ',ap.last_name) AS name, jv.title AS job,
                         ist.scheduled_date, ist.interview_type, ie.score, ie.result, a.status
                  FROM interview_schedules ist
                  JOIN applications a ON a.id = ist.application_id
                  JOIN applicants ap ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  LEFT JOIN interview_evaluations ie ON ie.interview_id = ist.id
                  WHERE (ist.status = 'Scheduled' OR ie.score IS NOT NULL)
                  AND a.status NOT IN ('Accepted', 'Rejected', 'Withdrawn')
                  ORDER BY ist.scheduled_date DESC
                  LIMIT 20");

            if (interviews.Rows.Count == 0)
            {
                Panel emptyCard = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 80, BackColor = BgCard };
                emptyCard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, emptyCard.Width - 1, emptyCard.Height - 1);
                };
                emptyCard.Controls.Add(new Label { Text = "No interviews scheduled yet.", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = TextPrimary, Left = 24, Top = 12, AutoSize = true, BackColor = Color.Transparent });
                p.Controls.Add(emptyCard);
                return;
            }

            int itemTop = top;
            foreach (DataRow row in interviews.Rows)
            {
                int intId = Convert.ToInt32(row["id"]);
                int appId = Convert.ToInt32(row["app_id"]);
                bool hasEval = row["score"] != DBNull.Value;

                Panel intCard = new Panel { Left = 24, Top = itemTop, Width = p.Width - 56, Height = 110, BackColor = BgCard };
                intCard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, intCard.Width - 1, intCard.Height - 1);
                };

                intCard.Controls.Add(new Label { Text = row["name"].ToString(), Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold), ForeColor = TextPrimary, Left = 12, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"{row["job"]} • {row["interview_type"]}", Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 12, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"📅 {Convert.ToDateTime(row["scheduled_date"]):MMM dd, yyyy}", Font = new Font("Segoe UI", 8.5f), ForeColor = TextSecondary, Left = 12, Top = 46, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"Status: {row["status"]}", Font = new Font("Segoe UI", 8.5f), ForeColor = TextMuted, Left = 12, Top = 62, AutoSize = true, BackColor = Color.Transparent });

                if (hasEval)
                {
                    decimal score = Convert.ToDecimal(row["score"]);
                    string result = row["result"].ToString();
                    Color resultColor = result == "Pass" ? AccentGreen : result == "Fail" ? Color.FromArgb(220, 38, 38) : TextMuted;

                    intCard.Controls.Add(new Label { Text = $"Score: {score:F2}/100", Font = new Font("Segoe UI Semibold", 9f), ForeColor = resultColor, Left = intCard.Width - 200, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                    intCard.Controls.Add(new Label { Text = $"Result: {result}", Font = new Font("Segoe UI Semibold", 9f), ForeColor = resultColor, Left = intCard.Width - 200, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                }

                Button btnEval = new Button { Text = hasEval ? "Edit" : "Add Evaluation", Left = intCard.Width - 150, Top = 60, Width = 130, Height = 32, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
                btnEval.FlatAppearance.BorderSize = 0;

                int capturedIntId = intId;
                int capturedAppId = appId;
                btnEval.Click += (s, e) => ShowEvaluationDialog(capturedIntId, capturedAppId);
                intCard.Controls.Add(btnEval);

                p.Controls.Add(intCard);
                itemTop += 116;
            }

            p.AutoScrollMinSize = new Size(0, itemTop + 50);
        }

        private void ShowEvaluationDialog(int interviewId, int appId)
        {
            Form dialog = new Form
            {
                Text = "Interview Evaluation",
                Width = 500,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(241, 245, 249)
            };

            int y = 20;

            dialog.Controls.Add(new Label { Text = "Score (0-100):", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;

            NumericUpDown nudScore = new NumericUpDown
            {
                Left = 20,
                Top = y,
                Width = 100,
                Height = 28,
                Minimum = 0,
                Maximum = 100,
                BackColor = Color.White,
                ForeColor = TextPrimary
            };
            dialog.Controls.Add(nudScore);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Result:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;

            ComboBox cmbResult = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 440,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 9f)
            };
            cmbResult.Items.AddRange(new[] { "Pass", "Fail", "Pending" });
            cmbResult.SelectedIndex = 0;
            dialog.Controls.Add(cmbResult);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Remarks:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;

            RichTextBox txtRemarks = new RichTextBox
            {
                Left = 20,
                Top = y,
                Width = 440,
                Height = 100,
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.White,
                ForeColor = TextPrimary
            };
            dialog.Controls.Add(txtRemarks);
            y += 110;

            Button btnSave = new Button
            {
                Text = "Save Evaluation",
                Left = 20,
                Top = y,
                Width = 440,
                Height = 36,
                BackColor = AccentGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            btnSave.Click += (s, e) =>
            {
                try
                {
                    string result = cmbResult.SelectedItem.ToString();
                    decimal score = nudScore.Value;
                    string remarks = txtRemarks.Text.Trim();

                    db.Execute("DELETE FROM interview_evaluations WHERE interview_id = @id", ("@id", interviewId));

                    db.Execute(
                        @"INSERT INTO interview_evaluations (interview_id, score, result, remarks, evaluated_by, evaluated_at)
                          VALUES (@id, @score, @result, @remarks, @evaluator, NOW())",
                        ("@id", interviewId),
                        ("@score", score),
                        ("@result", result),
                        ("@remarks", remarks),
                        ("@evaluator", AdminSession.AdminFullName));

                    db.Execute(
                        "UPDATE applications SET status = 'For Final Review' WHERE id = @appId",
                        ("@appId", appId));

                    db.Execute(
                        @"INSERT INTO application_status_history (application_id, status, remarks, changed_by)
                          VALUES (@appId, 'For Final Review', @remarks, @by)",
                        ("@appId", appId),
                        ("@remarks", $"Interview evaluated: {result} (Score: {score:F2}/100)"),
                        ("@by", AdminSession.AdminFullName));

                    Audit.Log("Interview Evaluated", "interview_evaluations", appId, $"Score: {score}, Result: {result}");

                    MessageBox.Show("Evaluation saved successfully!\n\nApplicant moved to 'For Final Review' status.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialog.Close();

                    LoadIntoMainForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            dialog.Controls.Add(btnSave);

            dialog.ShowDialog();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerInterviewEvaluationPage";
            Load += HRManagerInterviewEvaluationPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerInterviewEvaluationPage_Load(object sender, EventArgs e)
        {

        }
    }
}