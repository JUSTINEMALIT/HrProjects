using HRApplicant;
using MySql.Data.MySqlClient;
using project;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


//loginform

namespace ApplicantPortal
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {

            InitializeComponent();
            // FORM
            this.Text = "Applicant Portal";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 24);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ICON CIRCLE
            Panel iconCircle = new Panel();
            iconCircle.Size = new Size(50, 50);
            iconCircle.Location = new Point(215, 20);
            iconCircle.BackColor = Color.FromArgb(200, 230, 210);

            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, 50, 50);
            iconCircle.Region = new Region(gp);

            Label icon = new Label();
            icon.Text = "👤";
            icon.Font = new Font("Segoe UI Emoji", 18);
            icon.AutoSize = true;
            icon.Location = new Point(5, 8);

            iconCircle.Controls.Add(icon);

            // TITLE
            Label title = new Label();
            title.Text = "Applicant Portal";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(155, 80);

            // SUBTITLE
            Label subtitle = new Label();
            subtitle.Text = "HR Recruitment System";
            subtitle.ForeColor = Color.Gray;
            subtitle.Font = new Font("Segoe UI", 9);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(180, 110);

            // PANEL CARD
            Panel card = new Panel();
            card.Size = new Size(300, 170);
            card.Location = new Point(95, 140);
            card.BackColor = Color.FromArgb(35, 35, 35);
            card.BorderStyle = BorderStyle.FixedSingle;

            // EMAIL LABEL
            Label emailLbl = new Label();
            emailLbl.Text = "Email address";
            emailLbl.ForeColor = Color.White;
            emailLbl.Font = new Font("Segoe UI", 8);
            emailLbl.AutoSize = true;
            emailLbl.Location = new Point(15, 15);

            // EMAIL TEXTBOX
            TextBox txtEmail = new TextBox();
            txtEmail.Size = new Size(240, 25);
            txtEmail.Location = new Point(15, 35);
            txtEmail.BackColor = Color.FromArgb(45, 45, 45);
            txtEmail.ForeColor = Color.White;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;

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

            // LOGIN BUTTON
            Button btnLogin = new Button();
            btnLogin.Text = "Sign in";
            btnLogin.Size = new Size(240, 35);
            btnLogin.Location = new Point(15, 125);
            btnLogin.BackColor = Color.FromArgb(55, 55, 55);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;

            btnLogin.FlatAppearance.BorderColor = Color.Gray;


            btnLogin.Click += (s, e) =>
            {
                try
                {
                    var db = new DatabaseConnection();
                    DataTable result = db.Query(
                        "SELECT id, first_name, last_name, email FROM applicants WHERE email=@em AND password=@pw",
                        ("@em", txtEmail.Text.Trim()),
                        ("@pw", txtPass.Text.Trim()));

                    if (result.Rows.Count > 0)
                    {

                        project.Session.ApplicantId = Convert.ToInt32(result.Rows[0]["id"]);
                        project.Session.ApplicantName = result.Rows[0]["first_name"] + " " + result.Rows[0]["last_name"];
                        project.Session.ApplicantEmail = result.Rows[0]["email"].ToString();
                        


                        ApplicantMainForm dash = new ApplicantMainForm();
                        dash.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Invalid Email or Password.", "Login Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };



            // ADD TO CARD
            card.Controls.Add(emailLbl);
            card.Controls.Add(txtEmail);
            card.Controls.Add(passLbl);
            card.Controls.Add(txtPass);
            card.Controls.Add(btnLogin);

            // REGISTER TEXT
            LinkLabel register = new LinkLabel();
            register.Text = "Register here";
            register.LinkColor = Color.DeepSkyBlue;
            register.Location = new Point(240, 320);
            register.AutoSize = true;

            register.LinkClicked += (s, e) =>
            {
                RegisterForm regForm = new RegisterForm();
                regForm.Show();
                this.Hide();
            };

            Label noAcc = new Label();
            noAcc.Text = "No account yet?";
            noAcc.ForeColor = Color.White;
            noAcc.Location = new Point(135, 320);
            noAcc.AutoSize = true;

            // ADMIN LOGIN 
            LinkLabel adminLogin = new LinkLabel();
            adminLogin.Text = "HR/Admin Login here";
            adminLogin.LinkColor = Color.DeepSkyBlue;
            adminLogin.Location = new Point(170, 350);
            adminLogin.AutoSize = true;

            adminLogin.Click += (s, e) =>
            {
                AdminLoginForm adminForm = new AdminLoginForm();
                adminForm.Show();
                this.Hide();
            };


            // ADD CONTROLS
            this.Controls.Add(iconCircle);
            this.Controls.Add(title);
            this.Controls.Add(subtitle);
            this.Controls.Add(card);
            this.Controls.Add(noAcc);
            this.Controls.Add(register);
            this.Controls.Add(adminLogin);
        }

    private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}