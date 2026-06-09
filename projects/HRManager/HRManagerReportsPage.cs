using project;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;


//hr manager/admin reportpage

namespace projects.HRManager
{
    public partial class HRManagerReportsPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        private static readonly Color AccentOrange = Color.FromArgb(249, 115, 22);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        // ─── Report catalogue ─────────────────────────────────────────────────
        private static readonly (string Icon, string Title, string Type, string Desc)[] ReportItems =
        {
            ("📊", "Recruitment Statistics",  "recruitment_stats", "Total applicants, pass/fail rates and pipeline overview."),
            ("📋", "Applicant Report",         "applicant_report",  "Full list of applicants with status and job details."),
            ("✅", "Hiring Results",            "hiring_results",    "Accepted, rejected and on-hold hiring decisions."),
            ("❌", "Missing Documents",         "missing_docs",      "Applicants with incomplete document submissions."),
            ("⏳", "Pending Applications",      "pending_apps",      "Applications still awaiting action or review."),
        };

        // ─── Constructor ──────────────────────────────────────────────────────
        public HRManagerReportsPage(HRManagerMainForm main)
        {
            mainForm = main;
            contentPanel = main.contentPanel;
            db = new DatabaseConnection();
            InitializePage();
        }

        // ─── Build UI ─────────────────────────────────────────────────────────
        private void InitializePage()
        {
            mainForm.ClearContent();
            var p = contentPanel;
            p.BackColor = BgPage;
            p.AutoScroll = true;

            int top = 24;

            p.Controls.Add(new Label
            {
                Text = "Reports",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 24,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            });

            p.Controls.Add(new Label
            {
                Text = "Generate, preview and export recruitment reports to Excel or PDF.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = 68,
                AutoSize = true,
                BackColor = Color.Transparent
            });

            top = 108;

            foreach (var (icon, title, reportType, desc) in ReportItems)
            {
                string capType = reportType;
                string capTitle = title;

                Panel card = new Panel
                {
                    Left = 24,
                    Top = top,
                    Width = p.Width - 56,
                    Height = 72,
                    BackColor = BgCard,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                card.Paint += (s, e) =>
                {
                    using (var pen = new Pen(BorderLight, 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };

                // Icon + title
                card.Controls.Add(new Label
                {
                    Text = icon,
                    Font = new Font("Segoe UI", 18f),
                    ForeColor = TextPrimary,
                    Left = 14,
                    Top = 12,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
                card.Controls.Add(new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Left = 65,
                    Top = 12,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });
                card.Controls.Add(new Label
                {
                    Text = desc,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = TextSecondary,
                    Left = 65,
                    Top = 34,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });

                // ── View button ──────────────────────────────────────────────
                Button btnView = MakeBtn("👁 View", AccentBlue, card.Width - 310, 18, 90, 34);
                btnView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnView.Click += (s, e) => ShowReportViewer(capTitle, capType);
                card.Controls.Add(btnView);

                // ── Excel button ─────────────────────────────────────────────
                Button btnExcel = MakeBtn("⬇ Excel", Color.FromArgb(21, 128, 61), card.Width - 210, 18, 96, 34);
                btnExcel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnExcel.Click += (s, e) => ExportExcel(capTitle, capType);
                card.Controls.Add(btnExcel);

                // ── PDF button ───────────────────────────────────────────────
                Button btnPdf = MakeBtn("⬇ PDF", AccentRed, card.Width - 104, 18, 90, 34);
                btnPdf.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnPdf.Click += (s, e) => ExportPdf(capTitle, capType);
                card.Controls.Add(btnPdf);

                p.Controls.Add(card);
                top += 82;
            }

            p.AutoScrollMinSize = new Size(0, top + 40);
        }

        // ─── Load report data ─────────────────────────────────────────────────
        private DataTable LoadReport(string reportType)
        {
            try
            {
                return reportType switch
                {
                    "recruitment_stats" => db.Query(
                        @"SELECT
                            COUNT(*) AS total_applicants,
                            SUM(CASE WHEN status='Accepted'  THEN 1 ELSE 0 END) AS accepted,
                            SUM(CASE WHEN status='Rejected'  THEN 1 ELSE 0 END) AS rejected,
                            SUM(CASE WHEN status='On Hold'   THEN 1 ELSE 0 END) AS on_hold,
                            SUM(CASE WHEN status='For Interview' THEN 1 ELSE 0 END) AS for_interview,
                            SUM(CASE WHEN status='Shortlisted'  THEN 1 ELSE 0 END) AS shortlisted,
                            SUM(CASE WHEN status='Under Review' THEN 1 ELSE 0 END) AS under_review,
                            SUM(CASE WHEN status='Submitted'    THEN 1 ELSE 0 END) AS submitted
                          FROM applications"),

                    "applicant_report" => db.Query(
                        @"SELECT
                            a.id AS app_id,
                            CONCAT(ap.first_name,' ',ap.last_name) AS applicant_name,
                            ap.email,
                            jv.title AS position,
                            d.name AS department,
                            a.status,
                            DATE_FORMAT(a.submitted_at,'%b %d, %Y') AS submitted
                          FROM applications a
                          JOIN applicants ap    ON ap.id = a.applicant_id
                          JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                          JOIN departments d    ON d.id  = jv.department_id
                          ORDER BY a.submitted_at DESC"),

                    "hiring_results" => db.Query(
                        @"SELECT
                            a.id AS app_id,
                            CONCAT(ap.first_name,' ',ap.last_name) AS applicant_name,
                            jv.title AS position,
                            a.status,
                            hd.decision,
                            hd.remarks,
                            hd.decided_by,
                            DATE_FORMAT(hd.decided_at,'%b %d, %Y') AS decided_at
                          FROM applications a
                          JOIN applicants ap    ON ap.id = a.applicant_id
                          JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                          LEFT JOIN hiring_decisions hd ON hd.application_id = a.id
                          WHERE a.status IN ('Accepted','Rejected','On Hold')
                          ORDER BY hd.decided_at DESC"),

                    "missing_docs" => db.Query(
                        @"SELECT
                            a.id AS app_id,
                            CONCAT(ap.first_name,' ',ap.last_name) AS applicant_name,
                            jv.title AS position,
                            a.status,
                            rt.name AS missing_document
                          FROM applications a
                          JOIN applicants ap    ON ap.id = a.applicant_id
                          JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                          JOIN applicant_documents ad ON ad.application_id = a.id
                          JOIN requirement_types rt   ON rt.id = ad.requirement_type_id
                          WHERE ad.status = 'Missing'
                          ORDER BY a.id"),

                    "pending_apps" => db.Query(
                        @"SELECT
                            a.id AS app_id,
                            CONCAT(ap.first_name,' ',ap.last_name) AS applicant_name,
                            jv.title AS position,
                            d.name AS department,
                            a.status,
                            DATE_FORMAT(a.submitted_at,'%b %d, %Y') AS submitted,
                            DATEDIFF(NOW(), a.submitted_at) AS days_pending
                          FROM applications a
                          JOIN applicants ap    ON ap.id = a.applicant_id
                          JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                          JOIN departments d    ON d.id  = jv.department_id
                          WHERE a.status NOT IN ('Accepted','Rejected','Withdrawn')
                          ORDER BY a.submitted_at ASC"),

                    _ => new DataTable()
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new DataTable();
            }
        }

        // ─── View in popup DataGrid ───────────────────────────────────────────
        private void ShowReportViewer(string title, string reportType)
        {
            DataTable dt = LoadReport(reportType);
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No data found for this report.", "Empty Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Form viewer = new Form
            {
                Text = "Report — " + title,
                Width = 1000,
                Height = 600,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = BgPage,
                MinimizeBox = true,
                MaximizeBox = true
            };

            // Header
            viewer.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 16,
                Top = 12,
                AutoSize = true
            });
            viewer.Controls.Add(new Label
            {
                Text = dt.Rows.Count + " record(s)  —  " + DateTime.Now.ToString("MMMM dd, yyyy"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextSecondary,
                Left = 16,
                Top = 38,
                AutoSize = true
            });

            // Export buttons in viewer too
            Button btnExcelV = MakeBtn("⬇ Export Excel", Color.FromArgb(21, 128, 61), viewer.Width - 300, 18, 130, 32);
            btnExcelV.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExcelV.Click += (s, e) => ExportExcel(title, reportType);
            viewer.Controls.Add(btnExcelV);

            Button btnPdfV = MakeBtn("⬇ Export PDF", AccentRed, viewer.Width - 160, 18, 130, 32);
            btnPdfV.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPdfV.Click += (s, e) => ExportPdf(title, reportType);
            viewer.Controls.Add(btnPdfV);

            // Grid
            DataGridView dgv = new DataGridView
            {
                Left = 0,
                Top = 62,
                Width = viewer.ClientSize.Width,
                Height = viewer.ClientSize.Height - 62,
                DataSource = dt,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 9f),
                GridColor = BorderLight,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            viewer.Controls.Add(dgv);
            viewer.ShowDialog();
        }

        // ─── Export Excel (CSV) ───────────────────────────────────────────────
        private void ExportExcel(string title, string reportType)
        {
            DataTable dt = LoadReport(reportType);
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Empty Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Save Excel File",
                Filter = "Excel CSV (*.csv)|*.csv",
                FileName = title.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd") + ".csv",
                DefaultExt = "csv"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (StreamWriter sw = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
                {
                    // Title row
                    sw.WriteLine("\"" + title + "\"");
                    sw.WriteLine("\"Generated: " + DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt") + "\"");
                    sw.WriteLine();

                    // Header row
                    var headers = new StringBuilder();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (headers.Length > 0) headers.Append(",");
                        headers.Append("\"" + col.ColumnName.Replace("_", " ").ToUpper() + "\"");
                    }
                    sw.WriteLine(headers.ToString());

                    // Data rows
                    foreach (DataRow row in dt.Rows)
                    {
                        var line = new StringBuilder();
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (line.Length > 0) line.Append(",");
                            string val = row[col] == DBNull.Value ? "" : row[col].ToString();
                            val = val.Replace("\"", "\"\""); // escape quotes
                            line.Append("\"" + val + "\"");
                        }
                        sw.WriteLine(line.ToString());
                    }
                }

                var result = MessageBox.Show(
                    $"✅ Excel file saved!\n\n{dlg.FileName}\n\nDo you want to open it now?",
                    "Export Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dlg.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Export PDF (Print to PDF) ────────────────────────────────────────
        private void ExportPdf(string title, string reportType)
        {
            DataTable dt = LoadReport(reportType);
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Empty Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Build column widths map
            int colCount = dt.Columns.Count;
            float[] colW = new float[colCount];
            float pageW = 740f; // usable width in points (landscape A4 minus margins)
            float baseW = pageW / colCount;
            for (int i = 0; i < colCount; i++) colW[i] = baseW;

            int currentRow = 0;
            bool firstPage = true;
            Font fontTitle = new Font("Segoe UI", 14f, FontStyle.Bold);
            Font fontSub = new Font("Segoe UI", 8f);
            Font fontHeader = new Font("Segoe UI", 8f, FontStyle.Bold);
            Font fontCell = new Font("Segoe UI", 8f);

            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);

            pd.PrintPage += (s, e) =>
            {
                Graphics g = e.Graphics;
                float x = e.MarginBounds.Left;
                float y = e.MarginBounds.Top;
                float rowH = 22f;
                float headerH = 28f;

                if (firstPage)
                {
                    // Report title
                    g.DrawString(title, fontTitle, Brushes.Black, x, y);
                    y += 24f;
                    g.DrawString("Generated: " + DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt") +
                                 "   |   Total Records: " + dt.Rows.Count,
                                 fontSub, new SolidBrush(Color.FromArgb(100, 116, 139)), x, y);
                    y += 20f;

                    // Divider
                    g.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), x, y, e.MarginBounds.Right, y);
                    y += 10f;
                    firstPage = false;
                }

                // Recalculate colW based on actual usable width
                float usableW = e.MarginBounds.Width;
                for (int i = 0; i < colCount; i++) colW[i] = usableW / colCount;

                // Column headers
                float hx = x;
                g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), x, y, usableW, headerH);
                g.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240), 0.5f), x, y, usableW, headerH);
                for (int c = 0; c < colCount; c++)
                {
                    string hdr = dt.Columns[c].ColumnName.Replace("_", " ").ToUpper();
                    g.DrawString(hdr, fontHeader, new SolidBrush(Color.FromArgb(100, 116, 139)),
                        new RectangleF(hx + 3, y + 6, colW[c] - 6, headerH),
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                    hx += colW[c];
                }
                y += headerH;

                // Data rows
                while (currentRow < dt.Rows.Count)
                {
                    if (y + rowH > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    DataRow row = dt.Rows[currentRow];
                    Brush bgBrush = currentRow % 2 == 0
                        ? Brushes.White
                        : new SolidBrush(Color.FromArgb(248, 250, 252));

                    g.FillRectangle(bgBrush, x, y, usableW, rowH);
                    g.DrawLine(new Pen(Color.FromArgb(241, 245, 249), 0.5f), x, y + rowH, x + usableW, y + rowH);

                    float cx = x;
                    for (int c = 0; c < colCount; c++)
                    {
                        string val = row[c] == DBNull.Value ? "—" : row[c].ToString();
                        g.DrawString(val, fontCell, Brushes.Black,
                            new RectangleF(cx + 3, y + 4, colW[c] - 6, rowH - 4),
                            new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                        cx += colW[c];
                    }

                    y += rowH;
                    currentRow++;
                }

                // Footer
                g.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), x, e.MarginBounds.Bottom - 14, e.MarginBounds.Right, e.MarginBounds.Bottom - 14);
                g.DrawString("HR Applicant Processing System  —  Page " +
                             (currentRow / (int)((e.MarginBounds.Height / rowH) + 1) + 1),
                             fontSub, new SolidBrush(Color.FromArgb(148, 163, 184)),
                             x, e.MarginBounds.Bottom - 12);

                e.HasMorePages = false;
            };

            // Show print preview so user can print to PDF or printer
            PrintPreviewDialog preview = new PrintPreviewDialog
            {
                Document = pd,
                Width = 1100,
                Height = 750,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Print Preview — " + title
            };

            try
            {
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF preview error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Helper ───────────────────────────────────────────────────────────
        private Button MakeBtn(string text, Color bg, int left, int top, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = w,
                Height = h,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerReportsPage ";
            Load += HRManagerReportsPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerReportsPage_Load(object sender, EventArgs e)
        {

        }
    }
}