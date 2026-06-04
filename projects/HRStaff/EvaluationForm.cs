using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class EvaluationForm : Form
    {
        public EvaluationForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.AutoScroll = true;

            var db = new DatabaseConnection();

            this.Controls.Add(new Label { Text = "Interview Evaluation", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 20, AutoSize = true, BackColor = Color.Transparent });
            this.Controls.Add(new Label { Text = "Record interview scores, results, and recommendations.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 48, AutoSize = true, BackColor = Color.Transparent });

            // Filter tabs
            ComboBox cmbFilter = new ComboBox
            {
                Left = 28,
                Top = 76,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbFilter.Items.AddRange(new object[] { "Pending Evaluation", "All Completed" });
            cmbFilter.SelectedIndex = 0;

            Button btnFilter = new Button
            {
                Text = "Filter",
                Left = 198,
                Top = 74,
                Width = 70,
                Height = 28,
                BackColor = Color.FromArgb(28, 80, 52),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            this.Controls.Add(cmbFilter);
            this.Controls.Add(btnFilter);

            Panel listPanel = new Panel
            {
                Left = 28,
                Top = 114,
                Width = this.Width - 56,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(listPanel);

            Action loadEvals = null;
            loadEvals = () =>
            {
                listPanel.Controls.Clear();
                bool pendingOnly = cmbFilter.SelectedIndex == 0;

                DataTable dt = db.Query(
                    @"SELECT is2.id AS sched_id,
                             CONCAT(ap.first_name,' ',ap.last_name) AS name,
                             jv.title AS job, is2.interview_type,
                             is2.scheduled_date, is2.interviewer,
                             ie.id AS eval_id, ie.score, ie.result,
                             ie.remarks, ie.recommendation
                      FROM interview_schedules is2
                      JOIN applications a    ON a.id  = is2.application_id
                      JOIN applicants ap     ON ap.id = a.applicant_id
                      JOIN job_vacancies jv  ON jv.id = a.job_vacancy_id
                      LEFT JOIN interview_evaluations ie ON ie.interview_id = is2.id
                      WHERE is2.status = 'Completed'
                        AND (@pending=0 OR ie.id IS NULL)
                      ORDER BY is2.scheduled_date DESC",
                    ("@pending", pendingOnly ? 1 : 0));

                if (dt.Rows.Count == 0)
                {
                    listPanel.Controls.Add(new Label { Text = pendingOnly ? "No pending evaluations." : "No completed interviews yet.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(130, 150, 140), Left = 0, Top = 10, AutoSize = true, BackColor = Color.Transparent });
                    listPanel.Height = 50;
                    return;
                }

                int top = 0;
                foreach (DataRow row in dt.Rows)
                {
                    Panel card = BuildEvalCard(
                        Convert.ToInt32(row["sched_id"]),
                        row["name"].ToString(), row["job"].ToString(),
                        row["interview_type"].ToString(),
                        Convert.ToDateTime(row["scheduled_date"]).ToString("MMM dd, yyyy"),
                        row["interviewer"].ToString(),
                        row["eval_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["eval_id"]),
                        row["score"] == DBNull.Value ? "" : row["score"].ToString(),
                        row["result"] == DBNull.Value ? "" : row["result"].ToString(),
                        row["remarks"] == DBNull.Value ? "" : row["remarks"].ToString(),
                        row["recommendation"] == DBNull.Value ? "" : row["recommendation"].ToString(),
                        listPanel.Width, db, loadEvals);
                    card.Left = 0; card.Top = top;
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    listPanel.Controls.Add(card);
                    top += card.Height + 12;
                }
                listPanel.Height = top;
            };

            btnFilter.Click += (s, e) => loadEvals();
            loadEvals();
        }

        private Panel BuildEvalCard(int schedId, string name, string job, string type,
            string date, string interviewer, int evalId, string score, string result,
            string remarks, string recommendation, int width, DatabaseConnection db, Action reload)
        {
            bool hasEval = evalId > 0;
            Panel card = new Panel { Width = width, Height = 210, BackColor = Color.FromArgb(22, 28, 24) };
            card.Paint += (s, e) =>
            {
                Pen p = new Pen(Color.FromArgb(35, 50, 42), 1);
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1); p.Dispose();
            };

            // Accent stripe
            card.Controls.Add(new Panel { Left = 0, Top = 0, Width = 4, Height = 210, BackColor = hasEval ? Color.FromArgb(60, 200, 100) : Color.FromArgb(80, 160, 220) });

            // Header
            card.Controls.Add(new Label { Text = name + "  —  " + job, Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.FromArgb(210, 225, 218), Left = 18, Top = 10, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = type + "   |   " + date + "   |   Interviewer: " + interviewer, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(110, 140, 125), Left = 18, Top = 32, AutoSize = true, BackColor = Color.Transparent });

            // Status badge
            Label lblStatus = new Label
            {
                Text = hasEval ? "EVALUATED" : "PENDING",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = hasEval ? Color.FromArgb(60, 200, 100) : Color.FromArgb(230, 160, 50),
                BackColor = hasEval ? Color.FromArgb(20, 45, 28) : Color.FromArgb(45, 38, 20),
                Width = 90,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = card.Width - 110,
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            card.Controls.Add(lblStatus);

            // Divider
            card.Controls.Add(new Panel { Left = 0, Top = 54, Height = 1, Width = card.Width, BackColor = Color.FromArgb(35, 50, 42) });

            // Row 1: Score, Result, Remarks
            card.Controls.Add(FL("Score (0-100)", 18, 64));
            TextBox txtScore = new TextBox { Text = score, Left = 18, Top = 82, Width = 100, Height = 22, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
            card.Controls.Add(txtScore);

            card.Controls.Add(FL("Result", 128, 64));
            ComboBox cmbResult = new ComboBox { Left = 128, Top = 80, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(26, 33, 28), ForeColor = Color.FromArgb(180, 200, 190), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f) };
            cmbResult.Items.AddRange(new object[] { "— Select —", "Pass", "Fail" });
            cmbResult.SelectedItem = string.IsNullOrEmpty(result) ? "— Select —" : result;
            if (cmbResult.SelectedIndex < 0) cmbResult.SelectedIndex = 0;
            card.Controls.Add(cmbResult);

            card.Controls.Add(FL("Remarks", 248, 64));
            TextBox txtRemarks = new TextBox { Text = remarks, Left = 248, Top = 80, Width = 300, Height = 22, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
            card.Controls.Add(txtRemarks);

            // Row 2: Recommendation
            card.Controls.Add(FL("Recommendation / Notes", 18, 120));
            TextBox txtReco = new TextBox { Text = recommendation, Left = 18, Top = 138, Width = 530, Height = 22, BackColor = Color.FromArgb(26, 34, 28), ForeColor = Color.FromArgb(190, 210, 200), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f) };
            card.Controls.Add(txtReco);

            // Save / Update button
            Button btnSave = new Button
            {
                Text = hasEval ? "Update Evaluation" : "Save Evaluation",
                Left = 18,
                Top = 172,
                Width = 160,
                Height = 28,
                BackColor = Color.FromArgb(25, 80, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            int capSchedId = schedId, capEvalId = evalId;
            btnSave.Click += (s, e) =>
            {
                // Validation
                if (cmbResult.SelectedIndex == 0) { MessageBox.Show("Please select a result.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!decimal.TryParse(txtScore.Text.Trim(), out decimal sc) || sc < 0 || sc > 100)
                {
                    MessageBox.Show("Score must be a number between 0 and 100.", "Invalid Score", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string res = cmbResult.SelectedItem.ToString();
                try
                {
                    if (capEvalId > 0)
                    {
                        db.Execute(
                            "UPDATE interview_evaluations SET score=@sc, result=@r, remarks=@rm, recommendation=@rc, evaluated_by=@by WHERE id=@id",
                            ("@sc", sc), ("@r", res), ("@rm", txtRemarks.Text.Trim()),
                            ("@rc", txtReco.Text.Trim()), ("@by", project.Session.AdminUsername), ("@id", capEvalId));
                    }
                    else
                    {
                        db.Execute(
                            "INSERT INTO interview_evaluations (interview_id, score, result, remarks, recommendation, evaluated_by) VALUES (@iid,@sc,@r,@rm,@rc,@by)",
                            ("@iid", capSchedId), ("@sc", sc), ("@r", res),
                            ("@rm", txtRemarks.Text.Trim()), ("@rc", txtReco.Text.Trim()),
                            ("@by", project.Session.AdminUsername));
                    }

                    // Update application status based on result
                    DataTable appInfo = db.Query("SELECT application_id FROM interview_schedules WHERE id=@id", ("@id", capSchedId));
                    if (appInfo.Rows.Count > 0)
                    {
                        int relAppId = Convert.ToInt32(appInfo.Rows[0]["application_id"]);
                        string newStatus = res == "Pass" ? "For Final Review" : "Rejected";
                        db.Execute("UPDATE applications SET status=@st WHERE id=@id", ("@st", newStatus), ("@id", relAppId));
                        db.Execute(
                            "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,@st,@rm,@who)",
                            ("@id", relAppId), ("@st", newStatus),
                            ("@rm", "Interview evaluation: " + res + ". Score: " + sc + ". " + txtRemarks.Text.Trim()),
                            ("@who", project.Session.AdminUsername));
                    }

                    Audit.Log("Saved interview evaluation", "interview_evaluations", capSchedId);
                    MessageBox.Show("Evaluation saved! Status updated.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    reload();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            card.Controls.Add(btnSave);
            return card;
        }

        private Label FL(string t, int l, int top) => new Label { Text = t, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = l, Top = top, AutoSize = true, BackColor = Color.Transparent };

        private void EvaluationForm_Load(object sender, EventArgs e) { }
    }
}