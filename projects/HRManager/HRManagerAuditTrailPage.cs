using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRManager
{
    public partial class HRManagerAuditTrailPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerAuditTrailPage(HRManagerMainForm main)
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
                Text = "Audit Trail",
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
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 28;

            // Filter Panel
            Panel filterPanel = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 52, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            filterPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderLight, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, filterPanel.Width - 1, filterPanel.Height - 1);
            };

            ComboBox cmbAction = new ComboBox
            {
                Left = 12,
                Top = 12,
                Width = 200,
                Height = 28,
                Font = new Font("Segoe UI", 9f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAction.Items.AddRange(new[] { "All Actions", "Create", "Update", "Delete", "Login", "Logout", "Approve", "Reject" });
            cmbAction.SelectedIndex = 0;
            filterPanel.Controls.Add(cmbAction);

            ComboBox cmbTable = new ComboBox
            {
                Left = 220,
                Top = 12,
                Width = 200,
                Height = 28,
                Font = new Font("Segoe UI", 9f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTable.Items.AddRange(new[] { "All Tables", "applications", "users", "job_vacancies", "interview_schedules", "hiring_decisions" });
            cmbTable.SelectedIndex = 0;
            filterPanel.Controls.Add(cmbTable);

            DateTimePicker dtpFrom = new DateTimePicker { Left = 428, Top = 12, Width = 120, Height = 28 };
            dtpFrom.Value = DateTime.Now.AddDays(-30);
            filterPanel.Controls.Add(new Label { Text = "From:", Font = new Font("Segoe UI", 8f), ForeColor = TextSecondary, Left = 425, Top = -3, AutoSize = true });
            filterPanel.Controls.Add(dtpFrom);

            Button btnFilter = new Button
            {
                Text = "🔍 Filter",
                Left = filterPanel.Width - 100,
                Top = 10,
                Width = 80,
                Height = 32,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            filterPanel.Controls.Add(btnFilter);

            p.Controls.Add(filterPanel);
            top += 62;

            // Audit Log Container
            Panel logPanel = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 600, BackColor = Color.Transparent, AutoScroll = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            LoadAuditLogs(logPanel, "All Actions", "All Tables", dtpFrom.Value);

            btnFilter.Click += (s, e) =>
            {
                logPanel.Controls.Clear();
                LoadAuditLogs(logPanel, cmbAction.SelectedItem.ToString(), cmbTable.SelectedItem.ToString(), dtpFrom.Value);
            };

            cmbAction.SelectedIndexChanged += (s, e) =>
            {
                logPanel.Controls.Clear();
                LoadAuditLogs(logPanel, cmbAction.SelectedItem.ToString(), cmbTable.SelectedItem.ToString(), dtpFrom.Value);
            };

            cmbTable.SelectedIndexChanged += (s, e) =>
            {
                logPanel.Controls.Clear();
                LoadAuditLogs(logPanel, cmbAction.SelectedItem.ToString(), cmbTable.SelectedItem.ToString(), dtpFrom.Value);
            };

            p.Controls.Add(logPanel);
        }

        private void LoadAuditLogs(Panel logPanel, string actionFilter, string tableFilter, DateTime fromDate)
        {
            try
            {
                string query = @"SELECT action, username, table_name, record_id, details, logged_at FROM audit_trail WHERE logged_at >= @fromDate";

                if (actionFilter != "All Actions")
                    query += $" AND action LIKE '%{actionFilter}%'";

                if (tableFilter != "All Tables")
                    query += $" AND table_name = '{tableFilter}'";

                query += " ORDER BY logged_at DESC LIMIT 100";

                DataTable logs = db.Query(query, ("@fromDate", fromDate.ToString("yyyy-MM-dd")));

                if (logs.Rows.Count == 0)
                {
                    logPanel.Controls.Add(new Label { Text = "No audit logs found", Font = new Font("Segoe UI", 11f), ForeColor = TextMuted, Left = 0, Top = 20, AutoSize = true });
                    return;
                }

                int itemTop = 0;
                foreach (DataRow row in logs.Rows)
                {
                    string action = row["action"].ToString();
                    Color actionColor = action.Contains("Create") ? Color.FromArgb(34, 197, 94) :
                                       action.Contains("Update") ? Color.FromArgb(249, 115, 22) :
                                       action.Contains("Delete") ? Color.FromArgb(239, 68, 68) :
                                       action.Contains("Login") ? AccentBlue :
                                       action.Contains("Approve") ? Color.FromArgb(59, 130, 246) :
                                       Color.FromArgb(156, 163, 175);

                    Panel logCard = new Panel { Left = 0, Top = itemTop, Width = logPanel.Width - 20, Height = 76, BackColor = BgCard };
                    logCard.Paint += (s, e) =>
                    {
                        using (var pen = new Pen(BorderLight, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, logCard.Width - 1, logCard.Height - 1);
                    };

                    Panel actionBadge = new Panel { Left = 12, Top = 12, Width = 60, Height = 24, BackColor = Color.FromArgb(240, 248, 255) };
                    actionBadge.Paint += (s, e) =>
                    {
                        using (var pen = new Pen(actionColor, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, actionBadge.Width - 1, actionBadge.Height - 1);
                    };
                    actionBadge.Controls.Add(new Label { Text = action, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = actionColor, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                    logCard.Controls.Add(actionBadge);

                    logCard.Controls.Add(new Label { Text = row["table_name"].ToString() + " (ID: " + row["record_id"] + ")", Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold), ForeColor = TextPrimary, Left = 80, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                    logCard.Controls.Add(new Label { Text = "by " + row["username"], Font = new Font("Segoe UI", 8.5f), ForeColor = TextSecondary, Left = 80, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                    logCard.Controls.Add(new Label { Text = row["details"].ToString().Substring(0, Math.Min(50, row["details"].ToString().Length)) + "...", Font = new Font("Segoe UI", 8.5f), ForeColor = TextMuted, Left = 80, Top = 44, AutoSize = true, BackColor = Color.Transparent });
                    logCard.Controls.Add(new Label { Text = Convert.ToDateTime(row["logged_at"]).ToString("MMM dd, HH:mm:ss"), Font = new Font("Segoe UI", 8.5f), ForeColor = TextMuted, TextAlign = ContentAlignment.TopRight, Left = logCard.Width - 180, Top = 26, Width = 170, Height = 24, AutoSize = false, BackColor = Color.Transparent });

                    logPanel.Controls.Add(logCard);
                    itemTop += 82;
                }
            }
            catch (Exception ex)
            {
                logPanel.Controls.Add(new Label { Text = $"Error: {ex.Message}", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(239, 68, 68), Left = 0, Top = 0, AutoSize = true });
            }
        }
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantListForm
            // 
            ClientSize = new Size(284, 261);
            Name = "HRManagerAuditTrailPage ";
            Load += HRManagerAuditTrailPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerAuditTrailPage_Load(object sender, EventArgs e)
        {

        }
    }
}