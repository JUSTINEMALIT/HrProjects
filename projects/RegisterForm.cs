using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ApplicantPortal
{
    public class RegisterForm : Form
    {
        public RegisterForm()
        {
            // FORM
            this.Text = "Create Applicant Account";
            this.ClientSize = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 24);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // TITLE
            Label title = new Label();
            title.Text = "Create Applicant Account";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(20, 20);

            // SUBTITLE
            Label subtitle = new Label();
            subtitle.Text = "ACCOUNT INFORMATION";
            subtitle.ForeColor = Color.Gray;
            subtitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            subtitle.Location = new Point(20, 60);
            subtitle.AutoSize = true;

            // PANEL CARD
            Panel card = new Panel();
            card.Size = new Size(480, 360);
            card.Location = new Point(20, 90);
            card.BackColor = Color.FromArgb(35, 35, 35);

            // STYLE HELPERS
            TextBox CreateBox(string placeholder, int x, int y, int w = 220)
            {
                TextBox tb = new TextBox();
                tb.Size = new Size(w, 28);
                tb.Location = new Point(x, y);
                tb.BackColor = Color.FromArgb(45, 45, 45);
                tb.ForeColor = Color.White;
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.Text = placeholder;
                return tb;
            }

            Label CreateLabel(string text, int x, int y)
            {
                Label lbl = new Label();
                lbl.Text = text;
                lbl.ForeColor = Color.White;
                lbl.Font = new Font("Segoe UI", 8);
                lbl.AutoSize = true;
                lbl.Location = new Point(x, y);
                return lbl;
            }

            // FIRST NAME
            Label fnLbl = CreateLabel("First name", 15, 15);
            TextBox txtFirst = CreateBox("", 15, 35);

            // LAST NAME
            Label lnLbl = CreateLabel("Last name", 250, 15);
            TextBox txtLast = CreateBox("", 250, 35);

            // EMAIL
            Label emailLbl = CreateLabel("Email address", 15, 75);
            TextBox txtEmail = new TextBox();
            txtEmail.Size = new Size(440, 28);
            txtEmail.Location = new Point(15, 95);
            txtEmail.BackColor = Color.FromArgb(45, 45, 45);
            txtEmail.ForeColor = Color.White;
            txtEmail.Text = "";

            // PASSWORD
            Label passLbl = CreateLabel("Password", 15, 135);
            TextBox txtPass = CreateBox("", 15, 155);
            txtPass.UseSystemPasswordChar = true;

            // CONFIRM PASSWORD
            Label confirmLbl = CreateLabel("Confirm password", 250, 135);
            TextBox txtConfirm = CreateBox("", 250, 155);
            txtConfirm.UseSystemPasswordChar = true;

            // MOBILE
            Label mobileLbl = CreateLabel("Mobile number", 15, 195);
            TextBox txtMobile = CreateBox("", 15, 215);

            // DATE OF BIRTH
            Label dobLbl = CreateLabel("Date of birth", 250, 195);
            DateTimePicker dob = new DateTimePicker();
            dob.Size = new Size(220, 28);
            dob.Location = new Point(250, 215);
            dob.Format = DateTimePickerFormat.Short;
            dob.CalendarMonthBackground = Color.FromArgb(45, 45, 45);

            // INFO BOX
            Label info = new Label();
            info.Text = "ℹ Duplicate emails will be rejected. Use different and active email.";
            info.ForeColor = Color.LightSkyBlue;
            info.BackColor = Color.FromArgb(30, 30, 30);
            info.Size = new Size(440, 35);
            info.Location = new Point(15, 260);
            info.TextAlign = ContentAlignment.MiddleLeft;

            // CREATE BUTTON
            Button btnCreate = new Button();
            btnCreate.Text = "Create Account";
            btnCreate.Size = new Size(200, 35);
            btnCreate.Location = new Point(15, 310);
            btnCreate.BackColor = Color.FromArgb(0, 120, 215);
            btnCreate.ForeColor = Color.White;
            btnCreate.FlatStyle = FlatStyle.Flat;

            btnCreate.Click += (s, e) =>
            {
                try
                {
                    // Validation
                    if (txtPass.Text != txtConfirm.Text)
                    {
                        MessageBox.Show("Passwords do not match.");
                        return;
                    }

                    using (var conn = new project.DatabaseConnection().GetConnection())
                    {
                        conn.Open();

                        // CHECK DUPLICATE EMAIL
                        string checkQuery = "SELECT COUNT(*) FROM applicants WHERE email=@em";

                        MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@em", txtEmail.Text);

                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (exists > 0)
                        {
                            MessageBox.Show("Email already exists.");
                            return;
                        }

                        // INSERT DATA
                        string insertQuery = @"
                        INSERT INTO applicants
                        (first_name, last_name, email, password, mobile_number, date_of_birth)
                        VALUES
                        (@fn, @ln, @em, @pw, @mob, @dob)";

                        MySqlCommand cmd = new MySqlCommand(insertQuery, conn);

                        cmd.Parameters.AddWithValue("@fn", txtFirst.Text);
                        cmd.Parameters.AddWithValue("@ln", txtLast.Text);
                        cmd.Parameters.AddWithValue("@em", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@pw", txtPass.Text);
                        cmd.Parameters.AddWithValue("@mob", txtMobile.Text);
                        cmd.Parameters.AddWithValue("@dob", dob.Value);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Account Created Successfully!");

                        // balik login
                        this.Hide();
                        new LoginForm().Show();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };

            // BACK BUTTON
            Button btnBack = new Button();
            btnBack.Text = "Back to Login";
            btnBack.Size = new Size(200, 35);
            btnBack.Location = new Point(255, 310);
            btnBack.BackColor = Color.FromArgb(55, 55, 55);
            btnBack.ForeColor = Color.White;
            btnBack.FlatStyle = FlatStyle.Flat;

            btnBack.Click += (s, e) =>
            {
                this.Hide();
                new LoginForm().Show();
            };

            // ADD TO CARD
            card.Controls.Add(fnLbl);
            card.Controls.Add(txtFirst);
            card.Controls.Add(lnLbl);
            card.Controls.Add(txtLast);
            card.Controls.Add(emailLbl);
            card.Controls.Add(txtEmail);
            card.Controls.Add(passLbl);
            card.Controls.Add(txtPass);
            card.Controls.Add(confirmLbl);
            card.Controls.Add(txtConfirm);
            card.Controls.Add(mobileLbl);
            card.Controls.Add(txtMobile);
            card.Controls.Add(dobLbl);
            card.Controls.Add(dob);
            card.Controls.Add(info);
            card.Controls.Add(btnCreate);
            card.Controls.Add(btnBack);

            // ADD TO FORM
            this.Controls.Add(title);
            this.Controls.Add(subtitle);
            this.Controls.Add(card);
        }
    }
}