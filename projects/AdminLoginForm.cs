using MySql.Data.MySqlClient;
using project;
using projects.HRManager;
using projects.HRStaff;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


//adminloginform

namespace ApplicantPortal
{
    public partial class AdminLoginForm : Form
    {
        private DatabaseConnection db;

        public AdminLoginForm()
        {
            InitializeComponent();
            db = new DatabaseConnection();

            // FORM
            this.Text = "HR/Admin Portal";
            this.ClientSize = new Size(420, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 24);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            int formWidth = this.ClientSize.Width;

            // ICON
            Label icon = new Label();
            icon.Text = "👤";
            icon.Font = new Font("Segoe UI Symbol", 32);
            icon.AutoSize = true;
            icon.ForeColor = Color.White;
            icon.Location = new Point((formWidth - icon.Width) / 2, 30);

            // TITLE
            Label title = new Label();
            title.Text = "HR / Admin Portal";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.AutoSize = false;
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.Size = new Size(formWidth, 35);
            title.Location = new Point(0, 90);


            // SUBTITLE
            Label subtitle = new Label();
            subtitle.Text = "Recruitment Management System";
            subtitle.ForeColor = Color.FromArgb(180, 180, 180);
            subtitle.Font = new Font("Segoe UI", 9);
            subtitle.AutoSize = false;
            subtitle.TextAlign = ContentAlignment.MiddleCenter;
            subtitle.Size = new Size(formWidth, 25);
            subtitle.Location = new Point(0, 125);


            // CARD PANEL
            Panel card = new Panel();
            card.Size = new Size(320, 260);
            card.Location = new Point((formWidth - card.Width) / 2, 170);
            card.BackColor = Color.FromArgb(35, 35, 35);
            card.BorderStyle = BorderStyle.FixedSingle;

            // USERNAME LABEL
            Label userLbl = new Label();
            userLbl.Text = "Username / Email";
            userLbl.ForeColor = Color.White;
            userLbl.Font = new Font("Segoe UI", 9);
            userLbl.AutoSize = true;
            userLbl.Location = new Point(25, 25);

            // USERNAME TEXTBOX
            TextBox txtUser = new TextBox();
            txtUser.Size = new Size(270, 32);
            txtUser.Location = new Point(25, 48);
            txtUser.BackColor = Color.FromArgb(45, 45, 45);
            txtUser.ForeColor = Color.White;
            txtUser.BorderStyle = BorderStyle.FixedSingle;
            txtUser.Font = new Font("Segoe UI", 10);
            txtUser.Padding = new Padding(8);

            // PASSWORD LABEL
            Label passLbl = new Label();
            passLbl.Text = "Password";
            passLbl.ForeColor = Color.White;
            passLbl.Font = new Font("Segoe UI", 9);
            passLbl.AutoSize = true;
            passLbl.Location = new Point(25, 95);

            // PASSWORD TEXTBOX
            TextBox txtPass = new TextBox();
            txtPass.Size = new Size(270, 32);
            txtPass.Location = new Point(25, 118);
            txtPass.BackColor = Color.FromArgb(45, 45, 45);
            txtPass.ForeColor = Color.White;
            txtPass.BorderStyle = BorderStyle.FixedSingle;
            txtPass.Font = new Font("Segoe UI", 10);
            txtPass.UseSystemPasswordChar = true;
            txtPass.Padding = new Padding(8);

            // ERROR LABEL
            Label lblError = new Label();
            lblError.Text = "";
            lblError.ForeColor = Color.FromArgb(255, 120, 120);
            lblError.Font = new Font("Segoe UI", 8);
            lblError.Location = new Point(25, 160);
            lblError.Size = new Size(270, 30);
            lblError.AutoSize = false;

            // LOGIN BUTTON
            Button btnLogin = new Button();
            btnLogin.Text = "Sign in";
            btnLogin.Size = new Size(270, 40);
            btnLogin.Location = new Point(25, 200);
            btnLogin.BackColor = Color.FromArgb(66, 135, 245);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;

            btnLogin.Click += (s, e) =>
            {
                lblError.Text = "";

                if (string.IsNullOrWhiteSpace(txtUser.Text))
                {
                    lblError.Text = "Username is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPass.Text))
                {
                    lblError.Text = "Password is required";
                    return;
                }

                try
                {
                    // ✅ TRY HR_USERS TABLE FIRST (new system with roles)
                    DataTable hrResult = db.Query(
                        @"SELECT u.id, u.username, u.full_name, u.email, r.role_name
                           FROM hr_users u
                           JOIN roles r ON r.id = u.role_id
                           WHERE u.username = @user AND u.password = @pass AND u.is_active = 1",
                        ("@user", txtUser.Text.Trim()),
                        ("@pass", txtPass.Text.Trim())
                    );

                    if (hrResult != null && hrResult.Rows.Count > 0)
                    {
                        DataRow user = hrResult.Rows[0];

                        // ✅ SET AdminSession PROPERTIES (NOT Session!)
                        AdminSession.AdminId = Convert.ToInt32(user["id"]);
                        AdminSession.AdminUsername = user["username"].ToString();
                        AdminSession.AdminFullName = user["full_name"].ToString();
                        AdminSession.AdminEmail = user["email"].ToString();
                        AdminSession.AdminRole = user["role_name"].ToString();

                        // Log audit
                        Audit.Log("Admin Login", "hr_users", AdminSession.AdminId, $"Role: {AdminSession.AdminRole}");

                        // ✅ NAVIGATE TO CORRECT FORM BASED ON ROLE
                        this.Hide();

                        if (AdminSession.IsManager)  // HR Manager or Admin
                        {
                            new projects.HRManager.HRManagerMainForm().Show();
                        }
                        else if (AdminSession.IsStaff)  // HR Staff
                        {
                            new projects.HRStaff.MainForm().Show();
                        }
                        else
                        {
                            MessageBox.Show($"Unknown role: {AdminSession.AdminRole}", "Error");
                            AdminSession.ClearAll();
                            this.Show();
                            return;
                        }

                        this.Close();
                        return;
                    }

                    // ✅ TRY LEGACY ADMINS TABLE
                    DataTable adminResult = db.Query(
                        "SELECT id, username, role FROM admins WHERE username = @user AND password = @pass",
                        ("@user", txtUser.Text.Trim()),
                        ("@pass", txtPass.Text.Trim())
                    );

                    if (adminResult != null && adminResult.Rows.Count > 0)
                    {
                        DataRow admin = adminResult.Rows[0];

                        // ✅ SET AdminSession PROPERTIES
                        AdminSession.AdminId = Convert.ToInt32(admin["id"]);
                        AdminSession.AdminUsername = admin["username"].ToString();
                        AdminSession.AdminRole = admin["role"].ToString();
                        AdminSession.AdminFullName = "Admin User";
                        AdminSession.AdminEmail = "";

                        // Log audit
                        Audit.Log("Admin Login", "admins", AdminSession.AdminId, "Legacy admin");

                        // ✅ NAVIGATE
                        this.Hide();

                        if (AdminSession.IsManager)  // Admin or HR Manager
                        {
                            new projects.HRManager.HRManagerMainForm().Show();
                        }
                        else if (AdminSession.IsStaff)  // HR Staff
                        {
                            new projects.HRStaff.MainForm().Show();
                        }
                        else
                        {
                            MessageBox.Show($"Unknown role: {AdminSession.AdminRole}", "Error");
                            AdminSession.ClearAll();
                            this.Show();
                            return;
                        }

                        this.Close();
                        return;
                    }

                    // No user found
                    lblError.Text = "Invalid credentials or user is inactive";
                    txtPass.Clear();
                    txtUser.Focus();
                }
                catch (Exception ex)
                {
                    lblError.Text = $"Error: {ex.Message}";
                    MessageBox.Show($"Login Error:\n{ex.Message}\n\nDetails:\n{ex.StackTrace}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // ADD TO CARD
            card.Controls.Add(userLbl);
            card.Controls.Add(txtUser);
            card.Controls.Add(passLbl);
            card.Controls.Add(txtPass);
            card.Controls.Add(lblError);
            card.Controls.Add(btnLogin);

            // ADD CONTROLS
            this.Controls.Add(icon);
            this.Controls.Add(title);
            this.Controls.Add(subtitle);
            this.Controls.Add(card);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(284, 261);
            Name = "AdminLoginForm";
            Load += AdminLoginForm_Load;
            ResumeLayout(false);
        }

        private void AdminLoginForm_Load(object sender, EventArgs e)
        {
        }
    }
}