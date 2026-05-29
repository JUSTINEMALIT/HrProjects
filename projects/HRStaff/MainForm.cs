using System;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class MainForm : Form
    {
        private Panel contentPanel;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();

            // Default page after login
            LoadForm(new DashboardForm());
        }

        private void SetupUI()
        {
            this.Text = "HR Staff System";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(18, 22, 28);

            // SIDEBAR
            Panel sidebar = new Panel();
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 220;
            sidebar.BackColor = Color.FromArgb(22, 27, 34);

            // CONTENT
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(18, 22, 28);

            // BUTTONS
            Button btnDashboard = CreateButton("Dashboard", 20);
            Button btnApplicantList = CreateButton("Applicant List", 70);
            Button btnApplicantReview = CreateButton("Applicant Review", 120);
            Button btnScreening = CreateButton("Screening", 170);
            Button btnInterview = CreateButton("Interview Schedule", 220);
            Button btnEvaluation = CreateButton("Evaluation", 270);

            // EVENTS
            btnDashboard.Click += (s, e) => LoadForm(new DashboardForm());
            btnApplicantList.Click += (s, e) => LoadForm(new ApplicantListForm());
            btnApplicantReview.Click += (s, e) => LoadForm(new ApplicantReviewForm());
            btnScreening.Click += (s, e) => LoadForm(new ScreeningForm());
            btnInterview.Click += (s, e) => LoadForm(new InterviewScheduleForm());
            btnEvaluation.Click += (s, e) => LoadForm(new EvaluationForm());

            sidebar.Controls.Add(btnDashboard);
            sidebar.Controls.Add(btnApplicantList);
            sidebar.Controls.Add(btnApplicantReview);
            sidebar.Controls.Add(btnScreening);
            sidebar.Controls.Add(btnInterview);
            sidebar.Controls.Add(btnEvaluation);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
        }

        private Button CreateButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Width = 180,
                Height = 40,
                Left = 20,
                Top = top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 45, 35),
                ForeColor = Color.White
            };
        }

        private void LoadForm(Form form)
        {
            contentPanel.Controls.Clear();

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            contentPanel.Controls.Add(form);
            form.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}