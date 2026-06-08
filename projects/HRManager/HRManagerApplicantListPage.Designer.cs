namespace projects.HRManager
{
    partial class HRManagerApplicantListForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // HRManagerApplicantListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 700);
            Name = "HRManagerApplicantListForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Applicant List";
            Load += HRManagerApplicantListForm_Load;
            ResumeLayout(false);
        }
    }
}