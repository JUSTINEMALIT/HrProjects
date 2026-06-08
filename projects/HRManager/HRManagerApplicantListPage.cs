using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRManager
{
    public partial class HRManagerApplicantListForm : Form
    {
        private DatabaseConnection db;
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private ComboBox cmbJob;
        private Button btnSearch;
        private Button btnExport;
        private Panel filterPanel;

        // Color constants
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color HoverLight = Color.FromArgb(248, 250, 252);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;

        public HRManagerApplicantListForm(HRManagerMainForm main)
        {
            this.mainForm = main;
            this.contentPanel = main.contentPanel;
            db = new DatabaseConnection();
            mainForm.ClearContent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            contentPanel.BackColor = BgPage;
            contentPanel.AutoScroll = true;

            int top = 25;

            // ── Title and Subtitle ────────────────────────────────────
            Label title = new Label
            {
                Text = "Applicant List",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 30,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label subtitle = new Label
            {
                Text = "Click a row to review an applicant.",
                Font = new Font("Segoe UI", 9),
                ForeColor = TextMuted,
                Left = 30,
                Top = top + 35,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            contentPanel.Controls.Add(title);
            contentPanel.Controls.Add(subtitle);

            top += 85;

            // ── Search / Filter Panel ─────────────────────────────────
            filterPanel = new Panel
            {
                Left = 30,
                Top = top,
                Width = contentPanel.Width - 70,
                Height = 60,
                BackColor = BgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            filterPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(BorderLight))
                    e.Graphics.DrawRectangle(pen, 0, 0, filterPanel.Width - 1, filterPanel.Height - 1);
            };

            txtSearch = new TextBox
            {
                Left = 18,
                Top = 15,
                Width = 200,
                Height = 28,
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Search name or email...",
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            cmbStatus = new ComboBox
            {
                Left = 230,
                Top = 15,
                Width = 150,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            cmbStatus.Items.AddRange(new[]
            {
                "All Status", "Draft", "Submitted", "Under Review", "Shortlisted",
                "For Interview", "For Assessment", "For Final Review",
                "Accepted", "Rejected", "Withdrawn"
            });
            cmbStatus.SelectedIndex = 0;

            cmbJob = new ComboBox
            {
                Left = 390,
                Top = 15,
                Width = 180,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            cmbJob.Items.Add("All Jobs");
            DataTable jobs = db.Query("SELECT id, title FROM job_vacancies ORDER BY title");
            foreach (DataRow jr in jobs.Rows)
                cmbJob.Items.Add(jr["title"].ToString());
            cmbJob.SelectedIndex = 0;

            btnSearch = new Button
            {
                Text = "Search",
                Left = 580,
                Top = 14,
                Width = 80,
                Height = 32,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadData();

            btnExport = new Button
            {
                Text = "Export CSV",
                Left = 670,
                Top = 14,
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(cmbJob);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(btnExport);

            contentPanel.Controls.Add(filterPanel);

            top += 80;

            // ── DataGridView ──────────────────────────────────────────
            dgv = new DataGridView
            {
                Left = 30,
                Top = top,
                Width = contentPanel.Width - 70,
                Height = contentPanel.Height - top - 20,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = BgPage,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = BorderLight,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 45 },
                Font = new Font("Segoe UI", 9f),
                ScrollBars = ScrollBars.Both,
                Cursor = Cursors.Hand
            };

            dgv.DefaultCellStyle.BackColor = BgCard;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Padding = new Padding(6, 0, 4, 0);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = HoverLight;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dgv.EnableHeadersVisualStyles = false;

            // ── Columns ───────────────────────────────────────────────
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "app_id", HeaderText = "APP ID", FillWeight = 8, MinimumWidth = 75 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "name", HeaderText = "APPLICANT", FillWeight = 16, MinimumWidth = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "email", HeaderText = "EMAIL", FillWeight = 18, MinimumWidth = 160 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "job", HeaderText = "JOB", FillWeight = 20, MinimumWidth = 180 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "status", HeaderText = "STATUS", FillWeight = 13, MinimumWidth = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "submitted", HeaderText = "SUBMITTED", FillWeight = 12, MinimumWidth = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "missing", HeaderText = "MISSING", FillWeight = 10, MinimumWidth = 90 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "raw_id", Visible = false });

            // Status + missing colors
            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.Value == null) return;
                if (dgv.Columns[e.ColumnIndex].Name == "status")
                    e.CellStyle.ForeColor = StatusColor(e.Value.ToString());
                if (dgv.Columns[e.ColumnIndex].Name == "missing")
                    e.CellStyle.ForeColor = e.Value.ToString().Contains("missing")
                        ? Color.FromArgb(220, 88, 88)
                        : Color.FromArgb(34, 197, 94);
            };

            // Click row → open applicant review
            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int appId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["raw_id"].Value);
                new HRManagerApplicantReviewPage(mainForm, appId);
            };

            // Hover highlight
            dgv.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(229, 231, 235);
            };
            dgv.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                        e.RowIndex % 2 == 0 ? BgCard : HoverLight;
            };

            contentPanel.Controls.Add(dgv);

            // Event Handlers
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
            cmbStatus.SelectedIndexChanged += (s, e) => LoadData();
            cmbJob.SelectedIndexChanged += (s, e) => LoadData();
        }

        private void LoadData()
        {
            dgv.Rows.Clear();
            string kw = txtSearch.Text.Trim();
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
                ("@kw", kw), ("@kw2", "%" + kw + "%"), ("@st", st), ("@jb", job));

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
                    Convert.ToInt32(row["id"])
                );
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = db.Query("SELECT * FROM vw_hr_applicant_report");
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV|*.csv",
                    FileName = "ApplicantReport_" + DateTime.Now.ToString("yyyyMMdd")
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (DataColumn col in dt.Columns)
                    sb.Append(col.ColumnName + ",");
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
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message);
            }
        }

        private static Color StatusColor(string s)
        {
            if (s == "Draft") return Color.FromArgb(100, 116, 139);
            if (s == "Submitted") return Color.FromArgb(59, 130, 246);
            if (s == "Under Review") return Color.FromArgb(217, 119, 6);
            if (s == "Shortlisted") return Color.FromArgb(34, 197, 94);
            if (s == "For Interview") return Color.FromArgb(139, 92, 246);
            if (s == "For Assessment") return Color.FromArgb(249, 115, 22);
            if (s == "For Final Review") return Color.FromArgb(132, 204, 22);
            if (s == "Accepted") return Color.FromArgb(34, 197, 94);
            if (s == "Rejected") return Color.FromArgb(220, 38, 38);
            if (s == "Withdrawn") return Color.FromArgb(180, 83, 9);
            return Color.FromArgb(100, 116, 139);
        }

        private void HRManagerApplicantListForm_Load(object sender, EventArgs e) { }
    }
}