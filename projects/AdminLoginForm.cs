using MySql.Data.MySqlClient;
using project;
using projects.HRStaff;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ApplicantPortal
{
    public partial class AdminLoginForm : Form
    {
        public AdminLoginForm()
        {
            InitializeComponent();
            // FORM
            this.Text = "HR/Admin Portal";
            this.ClientSize = new Size(450, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 24);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ICON
            Label icon = new Label();
            icon.Text = "👤";
            icon.Font = new Font("Segoe UI Symbol", 22);
            icon.AutoSize = true;
            icon.Location = new Point(195, 20);

            // TITLE
            Label title = new Label();
            title.Text = "HR / Admin Portal";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(135, 60);
            icon.ForeColor = Color.White;

            // SUBTITLE
            Label subtitle = new Label();
            subtitle.Text = "Recruitment Management System";
            subtitle.ForeColor = Color.Gray;
            subtitle.Font = new Font("Segoe UI", 8);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(120, 90);

            // CARD PANEL
            Panel card = new Panel();
            card.Size = new Size(300, 240);
            card.Location = new Point(70, 120);
            card.BackColor = Color.FromArgb(35, 35, 35);
            card.BorderStyle = BorderStyle.FixedSingle;

            // USERNAME LABEL
            Label userLbl = new Label();
            userLbl.Text = "Username / Email";
            userLbl.ForeColor = Color.White;
            userLbl.Font = new Font("Segoe UI", 8);
            userLbl.AutoSize = true;
            userLbl.Location = new Point(15, 15);

            // USERNAME TEXTBOX
            TextBox txtUser = new TextBox();
            txtUser.Size = new Size(240, 25);
            txtUser.Location = new Point(15, 35);
            txtUser.BackColor = Color.FromArgb(45, 45, 45);
            txtUser.ForeColor = Color.White;
            txtUser.BorderStyle = BorderStyle.FixedSingle;

            // PASSWORD LABEL
            Label passLbl = new Label();
            passLbl.Text = "Password";
            passLbl.ForeColor = Color.White;
            passLbl.Font = new Font("Segoe UI", 8);
            passLbl.AutoSize = true;
            passLbl.Location = new Point(15, 70);

            // PASSWORD TEXTBOX
            TextBox txtPass = new TextBox();
            txtPass.Size = new Size(240, 25);
            txtPass.Location = new Point(15, 90);
            txtPass.BackColor = Color.FromArgb(45, 45, 45);
            txtPass.ForeColor = Color.White;
            txtPass.BorderStyle = BorderStyle.FixedSingle;
            txtPass.UseSystemPasswordChar = true;

            // ROLE LABEL
            Label roleLbl = new Label();
            roleLbl.Text = "Role";
            roleLbl.ForeColor = Color.White;
            roleLbl.Font = new Font("Segoe UI", 8);
            roleLbl.AutoSize = true;
            roleLbl.Location = new Point(15, 125);

            // COMBOBOX
            ComboBox cmbRole = new ComboBox();
            cmbRole.Size = new Size(240, 25);
            cmbRole.Location = new Point(15, 145);
            cmbRole.BackColor = Color.FromArgb(45, 45, 45);
            cmbRole.ForeColor = Color.White;
            cmbRole.FlatStyle = FlatStyle.Flat;

            cmbRole.Items.Add("HR Staff");
            cmbRole.Items.Add("Admin");
            cmbRole.SelectedIndex = 0;

            // BUTTON
            Button btnLogin = new Button();
            btnLogin.Text = "Sign in";
            btnLogin.Size = new Size(240, 35);
            btnLogin.Location = new Point(15, 185);
            btnLogin.BackColor = Color.FromArgb(55, 55, 55);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;

            btnLogin.Click += (s, e) =>
            {
                try
                {
                    using (var conn = new DatabaseConnection().GetConnection())
                    {
                        conn.Open();

                        string query = @"SELECT COUNT(*) FROM admins
                         WHERE username=@user
                         AND password=@pass
                         AND role=@role";

                        MySqlCommand cmd = new MySqlCommand(query, conn);

                        cmd.Parameters.AddWithValue("@user", txtUser.Text);
                        cmd.Parameters.AddWithValue("@pass", txtPass.Text);
                        cmd.Parameters.AddWithValue("@role", cmbRole.Text);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count > 0)
                        {

                            MainForm dashboard = new MainForm();
                            dashboard.Show();

                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid credentials.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            };

            // ADD TO CARD
            card.Controls.Add(userLbl);
            card.Controls.Add(txtUser);
            card.Controls.Add(passLbl);
            card.Controls.Add(txtPass);
            card.Controls.Add(roleLbl);
            card.Controls.Add(cmbRole);
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
            // 
            // AdminLoginForm
            // 
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