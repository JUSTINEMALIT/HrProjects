using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class AuditTrailForm : Form
    {
        private DatabaseConnection db = new DatabaseConnection();

        // ── Controls ─────────────────────────────────────────────────
        private DataGridView dgv;
        private DateTimePicker dtpFrom, dtpTo;
        private ComboBox cboUserType;
        private TextBox txtKeyword;

        public AuditTrailForm()
        {
            InitializeComponent();
            BuildUI();
            LoadAuditTrail();
        }

        // ── UI Builder ────────────────────────────────────────────────
        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.Size = new Size(1100, 680);

            // Title
            Controls.Add(new Label
            {
                Text = "Audit Trail",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 210, 130),
                Left = 20, Top = 18,
                AutoSize = true
            });

            Controls.Add(new Label
            {
                Text = "Read-only log of all important user actions in the system.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(120, 140, 130),
                Left = 22, Top = 52,
                AutoSize = true
            });

            // ── Filter bar ───────────────────────────────────────────
            Panel filterBar = new Panel
            {
                Left = 20, Top = 75,
                Width = 1050, Height = 50,
                BackColor = Color.FromArgb(22, 28, 34)
            };
            Controls.Add(filterBar);

            // From date
            filterBar.Controls.Add(MakeLabel("From:", 10, 14));
            dtpFrom = new DateTimePicker
            {
                Left = 55, Top = 11, Width = 130,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1),
                CalendarForeColor = Color.White,
                CalendarMonthBackground = Color.FromArgb(30, 40, 35),
                BackColor = Color.FromArgb(35, 45, 40),
                ForeColor = Color.White
            };
            filterBar.Controls.Add(dtpFrom);

            // To date
            filterBar.Controls.Add(MakeLabel("To:", 200, 14));
            dtpTo = new DateTimePicker
            {
                Left = 222, Top = 11, Width = 130,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                CalendarForeColor = Color.White,
                CalendarMonthBackground = Color.FromArgb(30, 40, 35),
                BackColor = Color.FromArgb(35, 45, 40),
                ForeColor = Color.White
            };
            filterBar.Controls.Add(dtpTo);

            // User type filter
            filterBar.Controls.Add(MakeLabel("User Type:", 370, 14));
            cboUserType = new ComboBox
            {
                Left = 445, Top = 11, Width = 110,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(35, 45, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cboUserType.Items.AddRange(new object[] { "All", "HR", "Applicant" });
            cboUserType.SelectedIndex = 0;
            filterBar.Controls.Add(cboUserType);

            // Keyword search
            filterBar.Controls.Add(MakeLabel("Search:", 570, 14));
            txtKeyword = new TextBox
            {
                Left = 625, Top = 11, Width = 200,
                BackColor = Color.FromArgb(35, 45, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            filterBar.Controls.Add(txtKeyword);

            // Search button
            Button btnSearch = MakeButton("🔍 Search", Color.FromArgb(35, 90, 60), 840, 10);
            btnSearch.Click += (s, e) => LoadAuditTrail();
            filterBar.Controls.Add(btnSearch);

            // Clear button
            Button btnClear = MakeButton("✕ Clear", Color.FromArgb(55, 60, 65), 950, 10);
            btnClear.Width = 80;
            btnClear.Click += (s, e) =>
            {
                dtpFrom.Value = DateTime.Today.AddMonths(-1);
                dtpTo.Value = DateTime.Today;
                cboUserType.SelectedIndex = 0;
                txtKeyword.Clear();
                LoadAuditTrail();
            };
            filterBar.Controls.Add(btnClear);

            // ── DataGridView ─────────────────────────────────────────
            dgv = new DataGridView
            {
                Left = 20, Top = 138,
                Width = 1050, Height = 480,
                BackgroundColor = Color.FromArgb(22, 28, 34),
                ForeColor = Color.FromArgb(210, 225, 218),
                GridColor = Color.FromArgb(30, 40, 35),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 8.5f),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 36
            };

            // Header style
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(15, 55, 38),
                ForeColor = Color.FromArgb(100, 220, 150),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding = new Padding(6, 0, 0, 0)
            };

            // Row style
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(22, 28, 34),
                ForeColor = Color.FromArgb(210, 225, 218),
                SelectionBackColor = Color.FromArgb(30, 65, 48),
                SelectionForeColor = Color.White,
                Padding = new Padding(4, 0, 0, 0)
            };

            // Alternating row color
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(26, 33, 40),
                ForeColor = Color.FromArgb(210, 225, 218),
                SelectionBackColor = Color.FromArgb(30, 65, 48),
                SelectionForeColor = Color.White
            };

            Controls.Add(dgv);

            // Row count label
            Controls.Add(new Label
            {
                Name = "lblCount",
                Text = "",
                Left = 20, Top = 625,
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 120, 110),
                Font = new Font("Segoe UI", 8f)
            });
        }

        // ── Data loader ───────────────────────────────────────────────
        private void LoadAuditTrail()
        {
            try
            {
                // Build WHERE clause dynamically based on filters
                string userTypeFilter = cboUserType.SelectedItem?.ToString() == "All"
                    ? "" : " AND user_type = @userType";

                string keywordFilter = string.IsNullOrWhiteSpace(txtKeyword.Text)
                    ? "" : " AND (action LIKE @kw OR username LIKE @kw OR details LIKE @kw OR table_name LIKE @kw)";

                string sql = $@"
                    SELECT
                        id          AS '#',
                        created_at  AS 'Date & Time',
                        user_type   AS 'User Type',
                        username    AS 'Username',
                        action      AS 'Action',
                        table_name  AS 'Table',
                        record_id   AS 'Record ID',
                        details     AS 'Details'
                    FROM audit_trail
                    WHERE DATE(created_at) BETWEEN @dateFrom AND @dateTo
                    {userTypeFilter}
                    {keywordFilter}
                    ORDER BY created_at DESC
                    LIMIT 500
                ";

                // Build parameters list
                var parms = new System.Collections.Generic.List<(string, object)>
                {
                    ("@dateFrom", dtpFrom.Value.Date),
                    ("@dateTo",   dtpTo.Value.Date)
                };

                if (!string.IsNullOrEmpty(userTypeFilter))
                    parms.Add(("@userType", cboUserType.SelectedItem.ToString()));

                if (!string.IsNullOrEmpty(keywordFilter))
                    parms.Add(("@kw", "%" + txtKeyword.Text.Trim() + "%"));

                DataTable dt = db.Query(sql, parms.ToArray());
                dgv.DataSource = dt;

                // Show row count
                var lblCount = Controls["lblCount"] as Label;
                if (lblCount != null)
                    lblCount.Text = $"Showing {dt.Rows.Count} record(s)  •  Last refreshed: {DateTime.Now:hh:mm:ss tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading audit trail:\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────
        private Label MakeLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Left = left, Top = top,
                AutoSize = true,
                ForeColor = Color.FromArgb(160, 180, 170),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.Transparent
            };
        }

        private Button MakeButton(string text, Color color, int left, int top)
        {
            var btn = new Button
            {
                Text = text,
                Left = left, Top = top,
                Width = 100, Height = 30,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void AuditTrailForm_Load(object sender, EventArgs e) { }
    }
}
