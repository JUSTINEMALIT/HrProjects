using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class FinalDecisionForm : Form
    {
        private DatabaseConnection db = new DatabaseConnection();

        private DataGridView dgv;
        private TextBox txtRemarks;

        public FinalDecisionForm()
        {
            InitializeComponent(); 
            BuildUI();
            LoadApplications();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.Size = new Size(1100, 650);

            Label title = new Label
            {
                Text = "Final Decisions",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 235, 228),
                Left = 20,
                Top = 20,
                AutoSize = true
            };
            Controls.Add(title);

            dgv = new DataGridView
            {
                Left = 20,
                Top = 60,
                Width = 1050,
                Height = 380,
                BackgroundColor = Color.FromArgb(24, 30, 26),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            Controls.Add(dgv);

            Label lblRemarks = new Label
            {
                Text = "Remarks",
                Left = 20,
                Top = 460,
                ForeColor = Color.White,
                AutoSize = true
            };
            Controls.Add(lblRemarks);

            txtRemarks = new TextBox
            {
                Left = 20,
                Top = 485,
                Width = 500,
                Height = 80,
                Multiline = true,
                BackColor = Color.FromArgb(35, 35, 35),
                ForeColor = Color.White
            };
            Controls.Add(txtRemarks);

            Controls.Add(CreateButton("Accept", Color.FromArgb(60, 200, 100), 550, 485, () => UpdateDecision("Accepted")));
            Controls.Add(CreateButton("Reject", Color.FromArgb(220, 80, 80), 680, 485, () => UpdateDecision("Rejected")));
            Controls.Add(CreateButton("On Hold", Color.FromArgb(230, 160, 50), 810, 485, () => UpdateDecision("On Hold")));
        }

        private Button CreateButton(string text, Color color, int left, int top, Action action)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 110,
                Height = 40,
                Left = left,
                Top = top,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();

            return btn;
        }

        private void LoadApplications()
        {
            try
            {
                dgv.DataSource = db.Query(@"
                    SELECT
                        a.id,
                        CONCAT(ap.first_name,' ',ap.last_name) AS Applicant,
                        ap.email,
                        jv.title AS Position,
                        a.status
                    FROM applications a
                    INNER JOIN applicants ap ON ap.id = a.applicant_id
                    INNER JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                    WHERE a.status IN
                    (
                        'For Final Review',
                        'For Assessment',
                        'For Interview',
                        'Under Review'
                    )
                    ORDER BY a.id DESC
                ");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateDecision(string decision)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an application.");
                return;
            }

            int applicationId = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
            string remarks = txtRemarks.Text.Trim();

            try
            {
                db.Execute(
                    @"UPDATE applications SET status=@status WHERE id=@id",
                    ("@status", decision),
                    ("@id", applicationId));

                db.Execute(
                    @"INSERT INTO hiring_decisions
                      (application_id, decision, remarks, decided_by, decided_at)
                      VALUES (@app,@decision,@remarks,@user,NOW())",
                    ("@app", applicationId),
                    ("@decision", decision),
                    ("@remarks", remarks),
                    ("@user",
                        string.IsNullOrWhiteSpace(Session.AdminUsername)
                        ? "HR Manager"
                        : Session.AdminUsername));

                db.Execute(
                    @"INSERT INTO application_status_history
                      (application_id, status, remarks, changed_by, changed_at)
                      VALUES (@app,@status,@remarks,@user,NOW())",
                    ("@app", applicationId),
                    ("@status", decision),
                    ("@remarks", remarks),
                    ("@user",
                        string.IsNullOrWhiteSpace(Session.AdminUsername)
                        ? "HR Manager"
                        : Session.AdminUsername));

                Audit.Log("Final Decision", "applications", applicationId, decision + " | " + remarks);

                MessageBox.Show("Decision saved successfully.");

                txtRemarks.Clear();
                LoadApplications();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FinalDecisionForm_Load(object sender, EventArgs e) { }
    }
}