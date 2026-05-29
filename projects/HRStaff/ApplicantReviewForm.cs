using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class ApplicantReviewForm : Form
    {
        public ApplicantReviewForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);

            Label title = new Label
            {
                Text = "Applicant Review",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Left = 30,
                Top = 30
            };

            Controls.Add(title);
        }

        private void ApplicantReviewForm_Load(object sender, EventArgs e)
        {

        }
    }
}