using System.Drawing;
using System.Windows.Forms;

namespace projects.HRStaff
{
    public partial class InterviewScheduleForm : Form
    {
        public InterviewScheduleForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(18, 22, 28);

            Label title = new Label
            {
                Text = "Interview Schedule",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Left = 30,
                Top = 30
            };

            Controls.Add(title);
        }

        private void InterviewScheduleForm_Load(object sender, EventArgs e)
        {

        }
    }
}