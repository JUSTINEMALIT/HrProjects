using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


//hr manager/admin interview schedule

namespace projects.HRManager
{
    public partial class HRManagerInterviewSchedulePage : Form
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
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerInterviewSchedulePage(HRManagerMainForm main)
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
                Text = "Interview Scheduling",
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
                Text = "Schedule interviews for shortlisted applicants",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = 70,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 32;

            Button btnSchedule = new Button
            {
                Text = "➕ Schedule Interview",
                Left = 24,
                Top = top,
                Width = 160,
                Height = 36,
                BackColor = AccentGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f),
                Cursor = Cursors.Hand
            };
            btnSchedule.FlatAppearance.BorderSize = 0;
            btnSchedule.Click += (s, e) => ShowScheduleDialog();
            p.Controls.Add(btnSchedule);
            top += 48;

            // ✅ UPDATED QUERY: Exclude Accepted/Rejected applicants
            DataTable interviews = db.Query(
                @"SELECT ist.id, a.id AS app_id, CONCAT(ap.first_name,' ',ap.last_name) AS name, 
                         jv.title AS job, ist.scheduled_date, ist.scheduled_time, ist.interview_type, 
                         ist.mode, ist.location, ist.status, a.status AS app_status
                  FROM interview_schedules ist
                  JOIN applications a ON a.id = ist.application_id
                  JOIN applicants ap ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  WHERE a.status NOT IN ('Accepted', 'Rejected', 'Withdrawn')
                  ORDER BY ist.scheduled_date DESC");

            foreach (DataRow row in interviews.Rows)
            {
                int intId = Convert.ToInt32(row["id"]);
                int appId = Convert.ToInt32(row["app_id"]);
                string appStatus = row["app_status"].ToString();

                Panel intCard = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 110, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                intCard.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(BorderLight))
                        e.Graphics.DrawRectangle(pen, 0, 0, intCard.Width - 1, intCard.Height - 1);
                };

                intCard.Controls.Add(new Label { Text = row["name"].ToString(), Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold), ForeColor = TextPrimary, Left = 12, Top = 8, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"{row["job"]} • {row["interview_type"]}", Font = new Font("Segoe UI", 9f), ForeColor = TextSecondary, Left = 12, Top = 28, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"📅 {Convert.ToDateTime(row["scheduled_date"]):MMM dd, yyyy} at {row["scheduled_time"]} • {row["mode"]}", Font = new Font("Segoe UI", 8.5f), ForeColor = TextSecondary, Left = 12, Top = 46, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"📍 {row["location"]}", Font = new Font("Segoe UI", 8.5f), ForeColor = TextSecondary, Left = 12, Top = 62, AutoSize = true, BackColor = Color.Transparent });
                intCard.Controls.Add(new Label { Text = $"App Status: {appStatus}", Font = new Font("Segoe UI Semibold", 8f), ForeColor = AccentBlue, Left = intCard.Width - 160, Top = 8, AutoSize = true, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Right });

                Button btnReschedule = new Button { Text = "Reschedule", Left = intCard.Width - 160, Top = 30, Width = 130, Height = 32, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                btnReschedule.FlatAppearance.BorderSize = 0;
                btnReschedule.Click += (s, e) => MessageBox.Show("Reschedule feature coming soon", "Info");
                intCard.Controls.Add(btnReschedule);

                p.Controls.Add(intCard);
                top += 116;
            }
        }

        private void ShowScheduleDialog()
        {
            Form dialog = new Form
            {
                Text = "Schedule Interview",
                Width = 600,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(241, 245, 249)
            };

            int y = 20;

            dialog.Controls.Add(new Label { Text = "Select Applicant:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 24;

            ComboBox cmbApplicant = new ComboBox { Left = 20, Top = y, Width = 540, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 9f) };

            DataTable applicants = db.Query(
                @"SELECT a.id, CONCAT(ap.first_name,' ',ap.last_name,' - ',jv.title) AS display
                  FROM applications a
                  JOIN applicants ap ON ap.id = a.applicant_id
                  JOIN job_vacancies jv ON jv.id = a.job_vacancy_id
                  WHERE a.status = 'Shortlisted'
                  ORDER BY ap.first_name");

            foreach (DataRow row in applicants.Rows)
                cmbApplicant.Items.Add(new { Id = Convert.ToInt32(row["id"]), Display = row["display"].ToString() });

            cmbApplicant.DisplayMember = "Display";
            dialog.Controls.Add(cmbApplicant);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Interview Type:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 24;
            ComboBox cmbType = new ComboBox { Left = 20, Top = y, Width = 540, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 9f) };
            cmbType.Items.AddRange(new[] { "Screening", "Technical", "HR Round", "Final Round" });
            cmbType.SelectedIndex = 0;
            dialog.Controls.Add(cmbType);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Mode:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 24;
            ComboBox cmbMode = new ComboBox { Left = 20, Top = y, Width = 540, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.FromArgb(15, 23, 42), Font = new Font("Segoe UI", 9f) };
            cmbMode.Items.AddRange(new[] { "Online", "Onsite", "Phone" });
            cmbMode.SelectedIndex = 0;
            dialog.Controls.Add(cmbMode);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Date:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 24;
            DateTimePicker dtDate = new DateTimePicker { Left = 20, Top = y, Width = 540, Format = DateTimePickerFormat.Short };
            dialog.Controls.Add(dtDate);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Time:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtTime = new TextBox { Left = 20, Top = y, Width = 540, Height = 30, BackColor = Color.White, ForeColor = Color.FromArgb(15, 23, 42), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f), Text = "09:00 AM" };
            dialog.Controls.Add(txtTime);
            y += 36;

            dialog.Controls.Add(new Label { Text = "Location:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtLocation = new TextBox { Left = 20, Top = y, Width = 540, Height = 30, BackColor = Color.White, ForeColor = Color.FromArgb(15, 23, 42), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f), Text = "Conference Room A" };
            dialog.Controls.Add(txtLocation);
            y += 40;

            Button btnSched = new Button { Text = "Schedule", Left = 20, Top = y, Width = 250, Height = 36, BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            btnSched.FlatAppearance.BorderSize = 0;
            btnSched.Click += (s, e) =>
            {
                if (cmbApplicant.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select an applicant", "Required");
                    return;
                }

                try
                {
                    var selected = (dynamic)cmbApplicant.SelectedItem;
                    int appId = selected.Id;

                    db.Execute(
                        @"INSERT INTO interview_schedules (application_id, interview_type, scheduled_date, scheduled_time, mode, location, status)
                          VALUES (@appId, @type, @date, @time, @mode, @location, 'Scheduled')",
                        ("@appId", appId),
                        ("@type", cmbType.SelectedItem.ToString()),
                        ("@date", dtDate.Value.ToString("yyyy-MM-dd")),
                        ("@time", txtTime.Text.Trim()),
                        ("@mode", cmbMode.SelectedItem.ToString()),
                        ("@location", txtLocation.Text.Trim()));

                    db.Execute(
                        "UPDATE applications SET status = 'For Interview' WHERE id = @appId",
                        ("@appId", appId));

                    db.Execute(
                        @"INSERT INTO application_status_history (application_id, status, remarks, changed_by)
                          VALUES (@appId, 'For Interview', @remarks, @by)",
                        ("@appId", appId),
                        ("@remarks", $"Scheduled {cmbType.SelectedItem.ToString()} interview on {dtDate.Value:MMM dd, yyyy}"),
                        ("@by", AdminSession.AdminFullName));

                    Audit.Log("Scheduled interview", "interview_schedules", appId, $"{cmbType.SelectedItem} - {dtDate.Value:MMM dd, yyyy}");

                    MessageBox.Show("Interview scheduled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialog.Close();

                    InitializePage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            dialog.Controls.Add(btnSched);

            Button btnCancel = new Button { Text = "Cancel", Left = 310, Top = y, Width = 250, Height = 36, BackColor = Color.FromArgb(200, 200, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            btnCancel.FlatAppearance.BorderSize = 0;
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }


        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "HRManagerInterviewSchedulePage";
            Load += HRManagerInterviewSchedulePage_Load;
            ResumeLayout(false);

        }

        private void HRManagerInterviewSchedulePage_Load(object sender, EventArgs e)
        {

        }
    }
}