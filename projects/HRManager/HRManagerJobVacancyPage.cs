using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRManager
{
    public partial class HRManagerJobVacancyPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerJobVacancyPage(HRManagerMainForm main)
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

            int top = 30;

            p.Controls.Add(new Label { Text = "Job Vacancy Management", Font = new Font("Segoe UI", 26f, FontStyle.Bold), ForeColor = TextPrimary, Left = 24, Top = top, AutoSize = true, BackColor = Color.Transparent });
            top += 55;

            Button btnAdd = new Button { Text = "➕ Add Job Vacancy", Left = 24, Top = top, Width = 190, Height = 40, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f), Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowAddVacancyDialog();
            p.Controls.Add(btnAdd);
            top += 48;

            DataTable vacancies = db.Query(
                @"SELECT jv.id, jv.title, d.name AS department, jv.employment_type, jv.slots, jv.status,
                         COUNT(DISTINCT a.id) AS total_applicants
                  FROM job_vacancies jv
                  LEFT JOIN departments d ON d.id = jv.department_id
                  LEFT JOIN applications a ON a.job_vacancy_id = jv.id
                  GROUP BY jv.id
                  ORDER BY jv.posted_at DESC
                  LIMIT 20");

            int itemTop = 15;

            Panel listPanel = new Panel
            {
                Left = 24,
                Top = top,
                Width = p.Width - 56,
                Height = p.Height - top - 30,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            p.Controls.Add(listPanel);

            foreach (DataRow row in vacancies.Rows)
            {
                int jobId = Convert.ToInt32(row["id"]);
                string status = row["status"].ToString();
                Color statusColor = status == "Open" ? AccentGreen : AccentRed;

                Panel jobCard = new Panel { Left = 10, Top = itemTop, Width = listPanel.Width - 25, Height = 120, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                jobCard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, jobCard.Width - 1, jobCard.Height - 1);
                };

                jobCard.Controls.Add(new Label { Text = row["title"].ToString(), Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold), ForeColor = TextPrimary, Left = 12, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                jobCard.Controls.Add(new Label { Text = $"{row["department"]} • {row["employment_type"]} • {row["slots"]} slots", Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 12, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                jobCard.Controls.Add(new Label { Text = $"Applicants: {row["total_applicants"]}", Font = new Font("Segoe UI", 8.5f), ForeColor = TextSecondary, Left = 12, Top = 46, AutoSize = true, BackColor = Color.Transparent });
                jobCard.Controls.Add(new Label { Text = status, Font = new Font("Segoe UI Semibold", 9f), ForeColor = statusColor, Left = jobCard.Width - 100, Top = 8, AutoSize = true, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Right });

                Button btnToggle = new Button { Text = status == "Open" ? "Close" : "Open", Left = jobCard.Width - 140, Top = 32, Width = 120, Height = 32, BackColor = statusColor == AccentGreen ? AccentRed : AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                btnToggle.FlatAppearance.BorderSize = 0;
                btnToggle.Click += (s, e) =>
                {
                    string newStatus = status == "Open" ? "Closed" : "Open";
                    db.Execute("UPDATE job_vacancies SET status = @status WHERE id = @id", ("@status", newStatus), ("@id", jobId));
                    Audit.Log($"Vacancy {newStatus}", "job_vacancies", jobId, row["title"].ToString());
                    MessageBox.Show($"Vacancy {newStatus}!", "Success");
                };
                jobCard.Controls.Add(btnToggle);

                listPanel.Controls.Add(jobCard);
                itemTop += 140;
            }
        }

        private void ShowAddVacancyDialog()
        {
            Form dialog = new Form { Text = "Add Job Vacancy", Width = 500, Height = 500, StartPosition = FormStartPosition.CenterParent, BackColor = Color.FromArgb(241, 245, 249) };
            int y = 20;

            dialog.Controls.Add(new Label { Text = "Job Title:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtTitle = new TextBox { Left = 20, Top = y, Width = 440, Height = 28 };
            dialog.Controls.Add(txtTitle);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Department:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Left = 20, Top = y, AutoSize = true });
            y += 24;
            ComboBox cmbDept = new ComboBox { Left = 20, Top = y, Width = 440, Height = 28, DropDownStyle = ComboBoxStyle.DropDownList };
            DataTable depts = db.Query("SELECT id, name FROM departments WHERE is_active = 1");
            foreach (DataRow row in depts.Rows)
                cmbDept.Items.Add(new { Id = row["id"], Text = row["name"].ToString() }.Text);
            dialog.Controls.Add(cmbDept);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Employment Type:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Left = 20, Top = y, AutoSize = true });
            y += 24;
            ComboBox cmbType = new ComboBox { Left = 20, Top = y, Width = 440, Height = 28, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "Full-time", "Part-time", "Contractual", "Project-based", "Internship" });
            cmbType.SelectedIndex = 0;
            dialog.Controls.Add(cmbType);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Slots:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Left = 20, Top = y, AutoSize = true });
            y += 24;
            NumericUpDown nudSlots = new NumericUpDown { Left = 20, Top = y, Width = 100, Height = 28, Value = 1, Minimum = 1 };
            dialog.Controls.Add(nudSlots);
            y += 44;

            Button btnSave = new Button { Text = "Add Vacancy", Left = 20, Top = y, Width = 440, Height = 36, BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text)) { MessageBox.Show("Enter job title"); return; }
                db.Execute(
                    @"INSERT INTO job_vacancies (title, department_id, employment_type, slots, status)
                      VALUES (@title, @dept, @type, @slots, 'Open')",
                    ("@title", txtTitle.Text),
                    ("@dept", 1),
                    ("@type", cmbType.SelectedItem.ToString()),
                    ("@slots", nudSlots.Value));
                MessageBox.Show("Vacancy added!", "Success");
                dialog.Close();
            };
            dialog.Controls.Add(btnSave);

            dialog.ShowDialog();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerJobVacancyPage";
            Load += HRManagerJobVacancyPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerJobVacancyPage_Load(object sender, EventArgs e)
        {

        }
    }
}