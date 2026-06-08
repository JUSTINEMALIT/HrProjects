using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class ScreeningForm : Form
    {
        public ScreeningForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);
            this.AutoScroll = true;

            var db = new DatabaseConnection();

            this.Controls.Add(new Label { Text = "Screening", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 20, AutoSize = true, BackColor = Color.Transparent });
            this.Controls.Add(new Label { Text = "Mark applicants as Qualified or Not Qualified.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 48, AutoSize = true, BackColor = Color.Transparent });

            // ── How it works banner ──────────────────────────────────
            Panel infoBanner = new Panel
            {
                Left = 28,
                Top = 68,
                Width = this.Width - 56,
                Height = 44,
                BackColor = Color.FromArgb(22, 36, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            infoBanner.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(40, 100, 65), 1); e.Graphics.DrawRectangle(p, 0, 0, infoBanner.Width - 1, infoBanner.Height - 1); p.Dispose(); };
            infoBanner.Controls.Add(new Label
            {
                Text = "ℹ  Only applicants with status \"Under Review\" appear here. Go to Applicant List → click a row → Start Review first.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 200, 140),
                Left = 12,
                Top = 0,
                Height = 44,
                Width = this.Width - 80,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            this.Controls.Add(infoBanner);

            // ── Count badge ──────────────────────────────────────────
            int underReviewCount = Convert.ToInt32(db.Scalar(
                "SELECT COUNT(*) FROM applications WHERE status = 'Under Review'"));

            Panel countBadge = new Panel
            {
                Left = 28,
                Top = 122,
                Width = 220,
                Height = 44,
                BackColor = underReviewCount > 0 ? Color.FromArgb(22, 40, 28) : Color.FromArgb(35, 28, 22)
            };
            countBadge.Paint += (s, e) =>
            {
                Color bc = underReviewCount > 0 ? Color.FromArgb(50, 120, 80) : Color.FromArgb(100, 80, 40);
                Pen p = new Pen(bc, 1); e.Graphics.DrawRectangle(p, 0, 0, countBadge.Width - 1, countBadge.Height - 1); p.Dispose();
            };
            countBadge.Controls.Add(new Label
            {
                Text = underReviewCount > 0
                    ? $"✔  {underReviewCount} applicant(s) ready for screening"
                    : "⚠  No applicants under review yet",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = underReviewCount > 0 ? Color.FromArgb(80, 200, 130) : Color.FromArgb(220, 160, 60),
                Left = 12,
                Top = 0,
                Height = 44,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            });
            this.Controls.Add(countBadge);

            if (underReviewCount == 0)
            {
                // Show clear instructions
                Panel guideCard = new Panel
                {
                    Left = 28,
                    Top = 178,
                    Width = this.Width - 56,
                    Height = 130,
                    BackColor = Color.FromArgb(24, 30, 26),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                guideCard.Paint += (s, e) => { Pen p = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(p, 0, 0, guideCard.Width - 1, guideCard.Height - 1); p.Dispose(); };
                guideCard.Controls.Add(new Label { Text = "How to screen an applicant:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(180, 210, 195), Left = 16, Top = 12, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "1.  Go to Applicant List (sidebar)", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 36, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "2.  Click any applicant row to open the review", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 58, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "3.  Click \"Start Review\" button", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 80, AutoSize = true, BackColor = Color.Transparent });
                guideCard.Controls.Add(new Label { Text = "4.  Come back here — the applicant will appear below", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(150, 175, 162), Left = 16, Top = 102, AutoSize = true, BackColor = Color.Transparent });
                this.Controls.Add(guideCard);
                return;
            }

            // ── Table header ─────────────────────────────────────────
            Panel hdr = new Panel
            {
                Left = 28,
                Top = 178,
                Width = this.Width - 56,
                Height = 28,
                BackColor = Color.FromArgb(26, 34, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            hdr.Controls.Add(H("APP ID", 12, 75));
            hdr.Controls.Add(H("APPLICANT", 95, 170));
            hdr.Controls.Add(H("JOB", 273, 180));
            hdr.Controls.Add(H("RESULT", 461, 130));
            hdr.Controls.Add(H("REMARKS", 599, 220));
            hdr.Controls.Add(H("ACTION", 827, 70));
            this.Controls.Add(hdr);

            DataTable dt = db.Query(
                @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name) AS name,
                         jv.title AS job, a.status,
                         sr.id AS screen_id, sr.result, sr.remarks
                  FROM applications a
                  JOIN applicants ap    ON ap.id  = a.applicant_id
                  JOIN job_vacancies jv ON jv.id  = a.job_vacancy_id
                  LEFT JOIN screening_results sr ON sr.application_id = a.id
                  WHERE a.status = 'Under Review'
                  ORDER BY a.submitted_at DESC");

            int rowTop = 208;
            foreach (DataRow row in dt.Rows)
            {
                Panel tr = BuildRow(
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString(), row["job"].ToString(), row["status"].ToString(),
                    row["screen_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["screen_id"]),
                    row["result"] == DBNull.Value ? "" : row["result"].ToString(),
                    row["remarks"] == DBNull.Value ? "" : row["remarks"].ToString(),
                    this.Width - 56, db);
                tr.Left = 28; tr.Top = rowTop;
                tr.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                this.Controls.Add(tr);
                rowTop += tr.Height + 2;
            }
        }

        private Panel BuildRow(int appId, string name, string job, string appStatus,
            int screenId, string result, string remarks, int width, DatabaseConnection db)
        {
            Panel row = new Panel { Width = width, Height = 54, BackColor = Color.FromArgb(22, 28, 24) };
            row.Paint += (s, e) =>
            {
                Pen p = new Pen(Color.FromArgb(32, 45, 38), 1);
                e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1); p.Dispose();
            };

            row.Controls.Add(new Label { Text = "APP-" + appId.ToString("D4"), Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(140, 160, 150), Left = 12, Top = 18, Width = 75, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(210, 225, 218), Left = 95, Top = 18, Width = 170, BackColor = Color.Transparent });
            row.Controls.Add(new Label { Text = job, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(140, 160, 150), Left = 273, Top = 18, Width = 180, BackColor = Color.Transparent });

            ComboBox cmbResult = new ComboBox
            {
                Left = 461,
                Top = 16,
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(26, 33, 28),
                ForeColor = Color.FromArgb(180, 200, 190),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cmbResult.Items.AddRange(new object[] { "— Select Result —", "Qualified", "Not Qualified" });
            cmbResult.SelectedItem = string.IsNullOrEmpty(result) ? "— Select Result —" : result;
            if (cmbResult.SelectedIndex < 0) cmbResult.SelectedIndex = 0;
            row.Controls.Add(cmbResult);

            TextBox txtRemarks = new TextBox
            {
                Text = remarks,
                Left = 599,
                Top = 16,
                Width = 220,
                Height = 22,
                BackColor = Color.FromArgb(26, 34, 28),
                ForeColor = Color.FromArgb(190, 210, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8.5f)
            };
            row.Controls.Add(txtRemarks);

            Button btnSave = new Button
            {
                Text = "Save",
                Left = 827,
                Top = 14,
                Width = 66,
                Height = 26,
                BackColor = Color.FromArgb(25, 70, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            int capAppId = appId, capScrId = screenId;
            btnSave.Click += (s, e) =>
            {
                if (cmbResult.SelectedIndex == 0)
                {
                    MessageBox.Show("Please select a result — Qualified or Not Qualified.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string res = cmbResult.SelectedItem.ToString();
                string newStatus = res == "Qualified" ? "Shortlisted" : "Rejected";

                var confirm = MessageBox.Show(
                    $"Mark APP-{capAppId:D4} as {res}?\nStatus will change to: {newStatus}",
                    "Confirm Screening", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                try
                {
                    if (capScrId > 0)
                        db.Execute(
                            "UPDATE screening_results SET result=@r, remarks=@rm, screened_by=@by WHERE id=@id",
                            ("@r", res), ("@rm", txtRemarks.Text.Trim()),
                            ("@by", project.Session.AdminUsername), ("@id", capScrId));
                    else
                        db.Execute(
                            "INSERT INTO screening_results (application_id, result, remarks, screened_by) VALUES (@aid,@r,@rm,@by)",
                            ("@aid", capAppId), ("@r", res),
                            ("@rm", txtRemarks.Text.Trim()), ("@by", project.Session.AdminUsername));

                    db.Execute("UPDATE applications SET status=@st WHERE id=@id", ("@st", newStatus), ("@id", capAppId));
                    db.Execute(
                        "INSERT INTO application_status_history (application_id, status, remarks, changed_by) VALUES (@id,@st,@rm,@who)",
                        ("@id", capAppId), ("@st", newStatus),
                        ("@rm", "Screening result: " + res + ". " + txtRemarks.Text.Trim()),
                        ("@who", project.Session.AdminUsername));

                    Audit.Log("Screening: " + res, "screening_results", capAppId);
                    MessageBox.Show(
                        $"Saved! APP-{capAppId:D4} is now {newStatus}.",
                        "Screening Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Controls.Clear();
                    BuildUI();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            row.Controls.Add(btnSave);
            return row;
        }

        private Label H(string t, int l, int w) => new Label { Text = t, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 110, 95), Left = l, Top = 7, Width = w, BackColor = Color.Transparent };

        private void ScreeningForm_Load(object sender, EventArgs e) { }
    }
}