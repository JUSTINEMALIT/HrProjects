using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class ApplicantListForm : Form
    {
        public ApplicantListForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);

            var db = new DatabaseConnection();

            this.Controls.Add(new Label { Text = "Applicant List", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 20, AutoSize = true, BackColor = Color.Transparent });
            this.Controls.Add(new Label { Text = "Click a row to review an applicant.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 48, AutoSize = true, BackColor = Color.Transparent });

            // ── Search / Filter bar ──────────────────────────────────
            TextBox txtSearch = new TextBox
            {
                Left = 28,
                Top = 78,
                Width = 200,
                Height = 26,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f),
                Text = "Search name or email..."
            };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text.StartsWith("Search")) txtSearch.Text = ""; };

            ComboBox cmbStatus = new ComboBox
            {
                Left = 238,
                Top = 78,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbStatus.Items.AddRange(new object[] { "All Status", "Draft", "Submitted", "Under Review", "Shortlisted", "For Interview", "For Assessment", "For Final Review", "Accepted", "Rejected", "Withdrawn" });
            cmbStatus.SelectedIndex = 0;

            ComboBox cmbJob = new ComboBox
            {
                Left = 398,
                Top = 78,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbJob.Items.Add("All Jobs");
            DataTable jobs = db.Query("SELECT id, title FROM job_vacancies ORDER BY title");
            foreach (DataRow jr in jobs.Rows) cmbJob.Items.Add(jr["title"].ToString());
            cmbJob.SelectedIndex = 0;

            Button btnSearch = new Button
            {
                Text = "Search",
                Left = 588,
                Top = 76,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(28, 80, 52),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;

            Button btnExport = new Button
            {
                Text = "Export CSV",
                Left = 678,
                Top = 76,
                Width = 90,
                Height = 28,
                BackColor = Color.FromArgb(25, 55, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;

            this.Controls.Add(txtSearch);
            this.Controls.Add(cmbStatus);
            this.Controls.Add(cmbJob);
            this.Controls.Add(btnSearch);
            this.Controls.Add(btnExport);

            // ── DataGridView ─────────────────────────────────────────
            DataGridView dgv = new DataGridView
            {
                Left = 28,
                Top = 116,
                Width = this.Width - 56,
                Height = this.Height - 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.FromArgb(18, 22, 28),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(35, 50, 42),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 46 },
                Font = new Font("Segoe UI", 9f),
                ScrollBars = ScrollBars.Both,
                Cursor = Cursors.Hand
            };

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(22, 28, 24);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(200, 218, 208);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 55, 42);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(100, 220, 150);
            dgv.DefaultCellStyle.Padding = new Padding(6, 0, 4, 0);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(24, 32, 26);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(200, 218, 208);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 55, 42);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(26, 34, 28);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 110, 95);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(26, 34, 28);
            dgv.EnableHeadersVisualStyles = false;

            // ── Columns — no Action column ───────────────────────────
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "app_id", HeaderText = "APP ID", FillWeight = 8, MinimumWidth = 75 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "name", HeaderText = "APPLICANT", FillWeight = 16, MinimumWidth = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "email", HeaderText = "EMAIL", FillWeight = 18, MinimumWidth = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "job", HeaderText = "JOB", FillWeight = 20, MinimumWidth = 180 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "status", HeaderText = "STATUS", FillWeight = 13, MinimumWidth = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "submitted", HeaderText = "SUBMITTED", FillWeight = 12, MinimumWidth = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "missing", HeaderText = "MISSING", FillWeight = 10, MinimumWidth = 90 });
            // Hidden column for app id number
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "raw_id", Visible = false });

            // Status + missing colors
            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.Value == null) return;
                if (dgv.Columns[e.ColumnIndex].Name == "status")
                    e.CellStyle.ForeColor = StatusColor(e.Value.ToString());
                if (dgv.Columns[e.ColumnIndex].Name == "missing")
                    e.CellStyle.ForeColor = e.Value.ToString().Contains("missing")
                        ? Color.FromArgb(220, 80, 80)
                        : Color.FromArgb(60, 180, 100);
            };

            // Click row → open ApplicantReview
            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int appId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["raw_id"].Value);
                NavigateToReview(appId);
            };

            // Hover highlight hint
            dgv.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0) dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(28, 44, 36);
            };
            dgv.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0) dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = e.RowIndex % 2 == 0 ? Color.FromArgb(22, 28, 24) : Color.FromArgb(24, 32, 26);
            };

            this.Controls.Add(dgv);

            // ── Load data ────────────────────────────────────────────
            Action loadList = () =>
            {
                dgv.Rows.Clear();
                string kw = txtSearch.Text.StartsWith("Search") ? "" : txtSearch.Text.Trim();
                string st = cmbStatus.SelectedIndex > 0 ? cmbStatus.SelectedItem.ToString() : "";
                string job = cmbJob.SelectedIndex > 0 ? cmbJob.SelectedItem.ToString() : "";

                DataTable dt = db.Query(
                    @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS name,
                             ap.email, jv.title AS job, a.status, a.submitted_at,
                             (SELECT COUNT(*) FROM applicant_documents ad
                              WHERE ad.application_id = a.id AND ad.status='Missing') AS missing_docs
                      FROM applications a
                      JOIN applicants ap    ON ap.id  = a.applicant_id
                      JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                      WHERE (@kw='' OR ap.first_name LIKE @kw2 OR ap.last_name LIKE @kw2 OR ap.email LIKE @kw2)
                        AND (@st='' OR a.status=@st)
                        AND (@jb='' OR jv.title=@jb)
                      ORDER BY a.created_at DESC",
                    ("@kw", kw), ("@kw2", "%" + kw + "%"),
                    ("@st", st), ("@jb", job));

                if (dt.Rows.Count == 0) return;

                foreach (DataRow row in dt.Rows)
                {
                    int missing = Convert.ToInt32(row["missing_docs"]);
                    dgv.Rows.Add(
                        "APP-" + Convert.ToInt32(row["id"]).ToString("D4"),
                        row["name"].ToString(),
                        row["email"].ToString(),
                        row["job"].ToString(),
                        row["status"].ToString(),
                        row["submitted_at"] == DBNull.Value ? "—" : Convert.ToDateTime(row["submitted_at"]).ToString("MMM dd, yyyy"),
                        missing > 0 ? missing + " missing" : "✔ Complete",
                        Convert.ToInt32(row["id"])  // raw_id hidden
                    );
                }
            };

            btnSearch.Click += (s, e) => loadList();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) loadList(); };

            btnExport.Click += (s, e) =>
            {
                try
                {
                    DataTable dt = db.Query("SELECT * FROM vw_hr_applicant_report");
                    SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "ApplicantReport_" + DateTime.Now.ToString("yyyyMMdd") };
                    if (sfd.ShowDialog() != DialogResult.OK) return;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (DataColumn col in dt.Columns) sb.Append(col.ColumnName + ",");
                    sb.AppendLine();
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (var item in row.ItemArray)
                            sb.Append("\"" + item.ToString().Replace("\"", "\"\"") + "\",");
                        sb.AppendLine();
                    }
                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Exported successfully!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show("Export error: " + ex.Message); }
            };

            loadList();
        }

        private void NavigateToReview(int appId)
        {
            Control ctrl = this.Parent;
            while (ctrl != null)
            {
                if (ctrl is MainForm main)
                {
                    main.SetActive(main.BtnApplicantReview);
                    main.LoadForm(new ApplicantReviewForm(appId));
                    return;
                }
                ctrl = ctrl.Parent;
            }
            // Fallback
            new ApplicantReviewForm(appId).Show();
        }

        private Color StatusColor(string s)
        {
            if (s == "Draft") return Color.FromArgb(130, 130, 160);
            if (s == "Submitted") return Color.FromArgb(80, 160, 220);
            if (s == "Under Review") return Color.FromArgb(230, 160, 50);
            if (s == "Shortlisted") return Color.FromArgb(80, 200, 160);
            if (s == "For Interview") return Color.FromArgb(140, 100, 220);
            if (s == "For Assessment") return Color.FromArgb(200, 130, 60);
            if (s == "For Final Review") return Color.FromArgb(160, 200, 80);
            if (s == "Accepted") return Color.FromArgb(60, 200, 100);
            if (s == "Rejected") return Color.FromArgb(220, 80, 80);
            if (s == "Withdrawn") return Color.FromArgb(160, 100, 80);
            return Color.FromArgb(150, 150, 150);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantListForm
            // 
            ClientSize = new Size(284, 261);
            Name = "ApplicantListForm";
            Load += ApplicantListForm_Load;
            ResumeLayout(false);

        }

        private void ApplicantListForm_Load(object sender, EventArgs e)
        {

        }
    }
}