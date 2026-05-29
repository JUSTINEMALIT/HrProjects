using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HRApplicant
{
    public static class ApplicantProfilePage
    {
        public static void Show(ApplicantMainForm main)
        {
            var db = new DatabaseConnection();
            main.ClearContent();
            var p = main.contentPanel;

            // ── Load existing data ───────────────────────────────────────
            DataTable baseRow = db.Query(
                "SELECT first_name, last_name, email, mobile_number, date_of_birth FROM applicants WHERE id=@id",
                ("@id", project.Session.ApplicantId));

            DataTable profRow = db.Query(
                "SELECT * FROM applicant_profiles WHERE applicant_id=@id",
                ("@id", project.Session.ApplicantId));

            DataRow b = baseRow.Rows.Count > 0 ? baseRow.Rows[0] : null;
            DataRow pr = profRow.Rows.Count > 0 ? profRow.Rows[0] : null;

            string Get(DataRow row, string col)
            {
                if (row == null || !row.Table.Columns.Contains(col)) return "";
                return row[col] == DBNull.Value ? "" : row[col].ToString();
            }

            // ── Page header ──────────────────────────────────────────────
            p.Controls.Add(new Label { Text = "My Profile", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 28, Top = 18, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(new Label { Text = "You can edit your profile details at any time.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 130, 115), Left = 29, Top = 46, AutoSize = true, BackColor = Color.Transparent });

            // ── Avatar block ─────────────────────────────────────────────
            Panel avatar = new Panel { Left = 28, Top = 74, Width = p.Width - 56, Height = 76, BackColor = Color.FromArgb(24, 30, 26), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                SolidBrush brush = new SolidBrush(Color.FromArgb(40, 100, 70));
                e.Graphics.FillEllipse(brush, 14, 10, 52, 52); brush.Dispose();
                Font f = new Font("Segoe UI", 15f, FontStyle.Bold);
                SolidBrush tb = new SolidBrush(Color.FromArgb(80, 210, 130));
                string initials = (project.Session.FirstName.Length > 0 ? project.Session.FirstName[0].ToString() : "") + (project.Session.LastName.Length > 0 ? project.Session.LastName[0].ToString() : "");
                e.Graphics.DrawString(initials, f, tb, 18, 16);
                f.Dispose(); tb.Dispose();
            };
            avatar.Controls.Add(new Label { Text = project.Session.ApplicantName, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(220, 235, 228), Left = 84, Top = 14, AutoSize = true, BackColor = Color.Transparent });
            avatar.Controls.Add(new Label { Text = "Applicant  •  " + project.Session.ApplicantEmail, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 130, 115), Left = 85, Top = 38, AutoSize = true, BackColor = Color.Transparent });
            p.Controls.Add(avatar);

            int top = 164;

            // ── Build all textbox fields ─────────────────────────────────
            // We'll collect all textboxes for save
            TextBox txtFirst, txtMiddle, txtLast, txtDob, txtGender, txtCivil, txtNat;
            TextBox txtEmail, txtMobile, txtProvince, txtCity, txtBrgy, txtStreet, txtZip;
            TextBox txtDegree, txtSchool, txtCourse, txtYearGrad;
            TextBox txtSkills;
            TextBox txtCompany, txtPosition, txtDuration, txtResponsibilities;

            // Personal Info
            top = AddSection(p, "Personal Information", top,
                new[] { "First Name", "Middle Name", "Last Name", "Date of Birth (YYYY-MM-DD)", "Gender", "Civil Status", "Nationality" },
                new[] {
                    Get(b,"first_name"), Get(pr,"middle_name"), Get(b,"last_name"),
                    Get(b,"date_of_birth"), Get(pr,"gender"), Get(pr,"civil_status"), Get(pr,"nationality")
                },
                out TextBox[] personalTxt);
            txtFirst = personalTxt[0]; txtMiddle = personalTxt[1]; txtLast = personalTxt[2];
            txtDob = personalTxt[3]; txtGender = personalTxt[4]; txtCivil = personalTxt[5]; txtNat = personalTxt[6];

            // Contact
            top = AddSection(p, "Contact & Address", top,
                new[] { "Email Address", "Mobile Number", "Province", "City / Municipality", "Barangay", "Street / Unit", "Zip Code" },
                new[] { Get(b, "email"), Get(b, "mobile_number"), Get(pr, "province"), Get(pr, "city"), Get(pr, "barangay"), Get(pr, "street"), Get(pr, "zip_code") },
                out TextBox[] contactTxt);
            txtEmail = contactTxt[0]; txtMobile = contactTxt[1]; txtProvince = contactTxt[2]; txtCity = contactTxt[3];
            txtBrgy = contactTxt[4]; txtStreet = contactTxt[5]; txtZip = contactTxt[6];

            // Education
            top = AddSection(p, "Educational Background", top,
                new[] { "Highest Degree", "School / University", "Course / Program", "Year Graduated" },
                new[] { Get(pr, "highest_degree"), Get(pr, "school"), Get(pr, "course"), Get(pr, "year_graduated") },
                out TextBox[] eduTxt);
            txtDegree = eduTxt[0]; txtSchool = eduTxt[1]; txtCourse = eduTxt[2]; txtYearGrad = eduTxt[3];

            // Skills
            top = AddSection(p, "Skills", top,
                new[] { "Skills (comma-separated)" },
                new[] { Get(pr, "skills") },
                out TextBox[] skillTxt);
            txtSkills = skillTxt[0];

            // Work Experience
            top = AddSection(p, "Work Experience (Most Recent)", top,
                new[] { "Company", "Position", "Duration", "Responsibilities" },
                new[] { Get(pr, "work_company"), Get(pr, "work_position"), Get(pr, "work_duration"), Get(pr, "work_responsibilities") },
                out TextBox[] workTxt);
            txtCompany = workTxt[0]; txtPosition = workTxt[1]; txtDuration = workTxt[2]; txtResponsibilities = workTxt[3];

            // ── Save button ──────────────────────────────────────────────
            Button btnSave = new Button
            {
                Text = "Save Changes",
                Left = 28,
                Top = top + 10,
                Width = 140,
                Height = 36,
                BackColor = Color.FromArgb(30, 100, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                try
                {
                    // Update base applicants table
                    db.Execute(
                        "UPDATE applicants SET first_name=@fn, last_name=@ln, mobile_number=@mn, date_of_birth=@dob WHERE id=@id",
                        ("@fn", txtFirst.Text.Trim()), ("@ln", txtLast.Text.Trim()),
                        ("@mn", txtMobile.Text.Trim()),
                        ("@dob", string.IsNullOrWhiteSpace(txtDob.Text) ? (object)DBNull.Value :
                        DateTime.TryParseExact(txtDob.Text.Trim(), "MM-dd-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime dob)
                        ? (object)dob.ToString("yyyy-MM-dd") : (object)DBNull.Value),
                        ("@id", project.Session.ApplicantId));

                    // Upsert applicant_profiles
                    bool exists = Convert.ToInt32(db.Scalar(
                    "SELECT COUNT(*) FROM applicant_profiles WHERE applicant_id=@aid",
                    ("@aid", project.Session.ApplicantId))) > 0;
                    if (exists)
                    {
                        db.Execute(
                            @"UPDATE applicant_profiles SET middle_name=@mn, gender=@gn, civil_status=@cs, nationality=@nat,
                              province=@prov, city=@city, barangay=@brgy, street=@st, zip_code=@zip,
                              highest_degree=@deg, school=@sch, course=@crs, year_graduated=@yr,
                              skills=@sk, work_company=@wc, work_position=@wp, work_duration=@wd, work_responsibilities=@wr
                              WHERE applicant_id=@aid",
                            ("@mn", txtMiddle.Text.Trim()), ("@gn", txtGender.Text.Trim()),
                            ("@cs", txtCivil.Text.Trim()), ("@nat", txtNat.Text.Trim()),
                            ("@prov", txtProvince.Text.Trim()), ("@city", txtCity.Text.Trim()),
                            ("@brgy", txtBrgy.Text.Trim()), ("@st", txtStreet.Text.Trim()),
                            ("@zip", txtZip.Text.Trim()), ("@deg", txtDegree.Text.Trim()),
                            ("@sch", txtSchool.Text.Trim()), ("@crs", txtCourse.Text.Trim()),
                            ("@yr", string.IsNullOrWhiteSpace(txtYearGrad.Text) ? (object)DBNull.Value : txtYearGrad.Text.Trim()),
                            ("@sk", txtSkills.Text.Trim()), ("@wc", txtCompany.Text.Trim()),
                            ("@wp", txtPosition.Text.Trim()), ("@wd", txtDuration.Text.Trim()),
                            ("@wr", txtResponsibilities.Text.Trim()),
                            ("@aid", project.Session.ApplicantId));
                    }
                    else
                    {
                        db.Execute(
                            @"INSERT INTO applicant_profiles (applicant_id, middle_name, gender, civil_status, nationality,
                              province, city, barangay, street, zip_code, highest_degree, school, course, year_graduated,
                              skills, work_company, work_position, work_duration, work_responsibilities)
                              VALUES (@aid,@mn,@gn,@cs,@nat,@prov,@city,@brgy,@st,@zip,@deg,@sch,@crs,@yr,@sk,@wc,@wp,@wd,@wr)",
                            ("@aid", project.Session.ApplicantId),
                            ("@mn", txtMiddle.Text.Trim()), ("@gn", txtGender.Text.Trim()),
                            ("@cs", txtCivil.Text.Trim()), ("@nat", txtNat.Text.Trim()),
                            ("@prov", txtProvince.Text.Trim()), ("@city", txtCity.Text.Trim()),
                            ("@brgy", txtBrgy.Text.Trim()), ("@st", txtStreet.Text.Trim()),
                            ("@zip", txtZip.Text.Trim()), ("@deg", txtDegree.Text.Trim()),
                            ("@sch", txtSchool.Text.Trim()), ("@crs", txtCourse.Text.Trim()),
                            ("@yr", string.IsNullOrWhiteSpace(txtYearGrad.Text) ? (object)DBNull.Value : txtYearGrad.Text.Trim()),
                            ("@sk", txtSkills.Text.Trim()), ("@wc", txtCompany.Text.Trim()),
                            ("@wp", txtPosition.Text.Trim()), ("@wd", txtDuration.Text.Trim()),
                            ("@wr", txtResponsibilities.Text.Trim()));
                    }

                    project.Session.ApplicantName = txtFirst.Text.Trim() + " " + txtLast.Text.Trim();
                    Audit.Log("Updated profile");
                    MessageBox.Show("Profile saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving profile:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            p.Controls.Add(btnSave);
        }

        // ── Section card builder ─────────────────────────────────────────
        private static int AddSection(Panel p, string title, int top, string[] labels, string[] values, out TextBox[] boxes)
        {
            int colW = (p.Width - 56 - 28 - 12) / 2;
            int rows = (int)Math.Ceiling(labels.Length / 2.0);
            int cardH = 38 + rows * 58 + 12;
            boxes = new TextBox[labels.Length];

            Panel card = new Panel { Left = 28, Top = top, Width = p.Width - 56, Height = cardH, BackColor = Color.FromArgb(22, 28, 24), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            card.Paint += (s, e) => { Pen pen = new Pen(Color.FromArgb(35, 50, 42), 1); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); pen.Dispose(); };
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 200, 130), Left = 14, Top = 10, AutoSize = true, BackColor = Color.Transparent });
            card.Controls.Add(new Panel { Left = 0, Top = 34, Height = 1, Width = card.Width, BackColor = Color.FromArgb(35, 50, 42), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right });

            for (int i = 0; i < labels.Length; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int fx = col == 0 ? 14 : colW + 26;
                int fy = 38 + row * 58;

                card.Controls.Add(new Label { Text = labels[i], Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 120, 110), Left = fx, Top = fy + 2, Width = colW, BackColor = Color.Transparent });
                bool multiline = labels[i] == "Responsibilities";
                TextBox txt = new TextBox
                {
                    Text = values[i],
                    Left = fx,
                    Top = fy + 18,
                    Width = colW - 8,
                    Height = multiline ? 50 : 22,
                    Multiline = multiline,
                    BackColor = Color.FromArgb(28, 35, 30),
                    ForeColor = Color.FromArgb(200, 218, 208),
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 9f)
                };
                boxes[i] = txt;
                card.Controls.Add(txt);
            }

            p.Controls.Add(card);
            return top + cardH + 12;
        }
    }
}