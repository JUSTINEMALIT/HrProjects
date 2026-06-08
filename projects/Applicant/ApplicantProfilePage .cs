using project;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HRApplicant
{
    public class ApplicantProfilePage : Form
    {
        private ApplicantMainForm mainForm;
        private Panel contentPanel;

        // ── Design Tokens ─────────────────────────────────────────────
        private static readonly Color BgPage = Color.FromArgb(13, 15, 18);
        private static readonly Color BgCard = Color.FromArgb(20, 24, 28);
        private static readonly Color BgField = Color.FromArgb(16, 20, 24);
        private static readonly Color BorderSubtle = Color.FromArgb(32, 40, 48);
        private static readonly Color BorderFocus = Color.FromArgb(56, 149, 255);
        private static readonly Color TextPrimary = Color.FromArgb(228, 234, 240);
        private static readonly Color TextSecondary = Color.FromArgb(88, 110, 128);
        private static readonly Color TextMuted = Color.FromArgb(52, 68, 80);
        private static readonly Color AccentGreen = Color.FromArgb(52, 211, 153);
        private static readonly Color AccentBlue = Color.FromArgb(56, 149, 255);

        public ApplicantProfilePage(ApplicantMainForm main)
        {
            this.mainForm = main;
            this.contentPanel = main.contentPanel;
            InitializePage();
        }

        private void InitializePage()
        {
            var db = new DatabaseConnection();
            mainForm.ClearContent();
            var p = contentPanel;
            p.BackColor = BgPage;

            SetDoubleBuffered(p, true);
            SetControlStyle(p, ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            SetControlStyle(p, ControlStyles.Opaque, true);
            p.AutoScroll = true;

            p.SuspendLayout();

            // ── Load existing data ─────────────────────────────────────
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
                if (row[col] == DBNull.Value) return "";
                if (col == "date_of_birth" && row[col] is DateTime dt)
                    return dt.ToString("yyyy-MM-dd");
                return row[col].ToString();
            }

            int top = 0;

            // ── Page Header ────────────────────────────────────────────
            Panel header = new Panel
            {
                Left = 0,
                Top = 0,
                Width = p.Width,
                Height = 72,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetDoubleBuffered(header, true);
            SetControlStyle(header, ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            header.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(32, BorderSubtle), 1))
                    e.Graphics.DrawLine(pen, 32, header.Height - 1, header.Width - 32, header.Height - 1);
            };
            header.Controls.Add(new Label
            {
                Text = "My Profile",
                Font = new Font("Segoe UI Semibold", 17f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 32,
                Top = 14,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            header.Controls.Add(new Label
            {
                Text = "You can edit your profile details at any time.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextSecondary,
                Left = 33,
                Top = 42,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            p.Controls.Add(header);
            top += 82;

            // ── Avatar Block ───────────────────────────────────────────
            Panel avatar = new Panel
            {
                Left = 32,
                Top = top,
                Height = 88,
                BackColor = BgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            avatar.Width = p.Width - 64;
            SetDoubleBuffered(avatar, true);
            SetControlStyle(avatar, ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            p.SizeChanged += (s, e) => avatar.Width = p.Width - 64;

            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderSubtle, 1))
                    DrawRoundedRect(e.Graphics, pen, 0, 0, avatar.Width - 1, avatar.Height - 1, 8);
                using (var br = new SolidBrush(Color.FromArgb(30, 52, 211, 153)))
                    e.Graphics.FillEllipse(br, 20, 16, 56, 56);

                string initials =
                    (project.Session.FirstName.Length > 0 ? project.Session.FirstName[0].ToString() : "") +
                    (project.Session.LastName.Length > 0 ? project.Session.LastName[0].ToString() : "");

                using (var f = new Font("Segoe UI Semibold", 16f, FontStyle.Bold))
                using (var tb = new SolidBrush(AccentGreen))
                {
                    SizeF sz = e.Graphics.MeasureString(initials, f);
                    float ix = 20 + (56 - sz.Width) / 2f;
                    float iy = 16 + (56 - sz.Height) / 2f;
                    e.Graphics.DrawString(initials, f, tb, ix, iy);
                }

                using (var br = new SolidBrush(AccentGreen))
                    e.Graphics.FillRectangle(br, 0, 20, 3, 48);
            };

            avatar.Controls.Add(new Label
            {
                Text = project.Session.ApplicantName,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 92,
                Top = 20,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            avatar.Controls.Add(new Label
            {
                Text = "Applicant  ·  " + project.Session.ApplicantEmail,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextSecondary,
                Left = 93,
                Top = 44,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });
            p.Controls.Add(avatar);
            top += 100;

            // ── Sections ───────────────────────────────────────────────
            TextBox txtFirst, txtMiddle, txtLast, txtDob, txtGender, txtCivil, txtNat;
            TextBox txtEmail, txtMobile, txtProvince, txtCity, txtBrgy, txtStreet, txtZip;
            TextBox txtDegree, txtSchool, txtCourse, txtYearGrad;
            TextBox txtSkills;
            TextBox txtCompany, txtPosition, txtDuration, txtResponsibilities;

            top = AddSection(p, "Personal Information", top,
                new[] { "First Name", "Middle Name", "Last Name", "Date of Birth (YYYY-MM-DD)", "Gender", "Civil Status", "Nationality" },
                new[] { Get(b, "first_name"), Get(pr, "middle_name"), Get(b, "last_name"), Get(b, "date_of_birth"), Get(pr, "gender"), Get(pr, "civil_status"), Get(pr, "nationality") },
                out TextBox[] personalTxt);
            txtFirst = personalTxt[0]; txtMiddle = personalTxt[1]; txtLast = personalTxt[2];
            txtDob = personalTxt[3]; txtGender = personalTxt[4]; txtCivil = personalTxt[5]; txtNat = personalTxt[6];

            top = AddSection(p, "Contact & Address", top,
                new[] { "Email Address", "Mobile Number", "Province", "City / Municipality", "Barangay", "Street / Unit", "Zip Code" },
                new[] { Get(b, "email"), Get(b, "mobile_number"), Get(pr, "province"), Get(pr, "city"), Get(pr, "barangay"), Get(pr, "street"), Get(pr, "zip_code") },
                out TextBox[] contactTxt);
            txtEmail = contactTxt[0]; txtMobile = contactTxt[1]; txtProvince = contactTxt[2]; txtCity = contactTxt[3];
            txtBrgy = contactTxt[4]; txtStreet = contactTxt[5]; txtZip = contactTxt[6];

            top = AddSection(p, "Educational Background", top,
                new[] { "Highest Degree", "School / University", "Course / Program", "Year Graduated" },
                new[] { Get(pr, "highest_degree"), Get(pr, "school"), Get(pr, "course"), Get(pr, "year_graduated") },
                out TextBox[] eduTxt);
            txtDegree = eduTxt[0]; txtSchool = eduTxt[1]; txtCourse = eduTxt[2]; txtYearGrad = eduTxt[3];

            top = AddSection(p, "Skills", top,
                new[] { "Skills (comma-separated)" },
                new[] { Get(pr, "skills") },
                out TextBox[] skillTxt);
            txtSkills = skillTxt[0];

            top = AddSection(p, "Work Experience (Most Recent)", top,
                new[] { "Company", "Position", "Duration", "Responsibilities" },
                new[] { Get(pr, "work_company"), Get(pr, "work_position"), Get(pr, "work_duration"), Get(pr, "work_responsibilities") },
                out TextBox[] workTxt);
            txtCompany = workTxt[0]; txtPosition = workTxt[1]; txtDuration = workTxt[2]; txtResponsibilities = workTxt[3];

            // ── Save Button ────────────────────────────────────────────
            Button btnSave = new Button
            {
                Text = "Save Changes",
                Left = 32,
                Top = top + 14,
                Width = 148,
                Height = 38,
                BackColor = Color.FromArgb(30, 100, 65),
                ForeColor = Color.FromArgb(52, 211, 153),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(40, 130, 80);
            btnSave.FlatAppearance.BorderSize = 1;
            btnSave.MouseEnter += (s, e) => { btnSave.BackColor = Color.FromArgb(40, 130, 80); };
            btnSave.MouseLeave += (s, e) => { btnSave.BackColor = Color.FromArgb(30, 100, 65); };

            btnSave.Click += (s, e) =>
            {
                try
                {
                    db.Execute(
                        "UPDATE applicants SET first_name=@fn, last_name=@ln, mobile_number=@mn, date_of_birth=@dob WHERE id=@id",
                        ("@fn", txtFirst.Text.Trim()), ("@ln", txtLast.Text.Trim()),
                        ("@mn", txtMobile.Text.Trim()),
                        ("@dob", string.IsNullOrWhiteSpace(txtDob.Text) ? (object)DBNull.Value :
                            DateTime.TryParseExact(txtDob.Text.Trim(), "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime dob)
                            ? (object)dob.ToString("yyyy-MM-dd") : (object)DBNull.Value),
                        ("@id", project.Session.ApplicantId));

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

            p.ResumeLayout(false);
            p.PerformLayout();
        }

        // ── Section card builder ───────────────────────────────────────
        private int AddSection(Panel p, string title, int top,
            string[] labels, string[] values, out TextBox[] boxes)
        {
            const int PADDING = 20;
            const int GAP = 16;
            const int LABEL_H = 18;
            const int INPUT_H = 32;
            const int MULTI_H = 72;
            const int ROW_GAP = 14;
            const int HEADER_H = 46;
            const int BOTTOM_PAD = 18;

            boxes = new TextBox[labels.Length];

            int rows = (int)Math.Ceiling(labels.Length / 2.0);
            int cardH = HEADER_H + BOTTOM_PAD;
            for (int r = 0; r < rows; r++)
            {
                bool rowHasMulti = false;
                for (int c = 0; c < 2; c++)
                {
                    int idx = r * 2 + c;
                    if (idx < labels.Length && labels[idx] == "Responsibilities")
                        rowHasMulti = true;
                }
                cardH += LABEL_H + (rowHasMulti ? MULTI_H : INPUT_H) + ROW_GAP;
            }

            Panel card = new Panel
            {
                Left = 32,
                Top = top,
                Width = p.Width - 64,
                Height = cardH,
                BackColor = Color.FromArgb(20, 24, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetDoubleBuffered(card, true);
            SetControlStyle(card, ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(32, 40, 48), 1))
                    DrawRoundedRect(e.Graphics, pen, 0, 0, card.Width - 1, card.Height - 1, 8);
                using (var br = new SolidBrush(Color.FromArgb(56, 149, 255)))
                    e.Graphics.FillEllipse(br, PADDING, 17, 6, 6);
            };

            card.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 149, 255),
                Left = PADDING + 14,
                Top = 12,
                AutoSize = true,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            });

            Panel divider = new Panel
            {
                Left = 0,
                Top = HEADER_H - 10,
                Height = 1,
                Width = card.Width,
                BackColor = Color.FromArgb(32, 40, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.SizeChanged += (s, e) => divider.Width = card.Width;
            card.Controls.Add(divider);

            var lbls = new Label[labels.Length];
            var txts = new TextBox[labels.Length];

            int initialColW = Math.Max(60, (card.Width - PADDING * 2 - GAP) / 2);

            int fy = HEADER_H;
            for (int r = 0; r < rows; r++)
            {
                bool rowHasMulti = false;
                for (int c = 0; c < 2; c++)
                {
                    int idx2 = r * 2 + c;
                    if (idx2 < labels.Length && labels[idx2] == "Responsibilities")
                        rowHasMulti = true;
                }
                int inputH = rowHasMulti ? MULTI_H : INPUT_H;

                for (int c = 0; c < 2; c++)
                {
                    int i = r * 2 + c;
                    if (i >= labels.Length) break;

                    bool isMulti = labels[i] == "Responsibilities";
                    int fx = c == 0 ? PADDING : PADDING + initialColW + GAP;

                    Label lbl = new Label
                    {
                        Text = labels[i],
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = Color.FromArgb(88, 110, 128),
                        Left = fx,
                        Top = fy,
                        Width = initialColW,
                        Height = LABEL_H,
                        AutoSize = false,
                        BackColor = Color.Transparent,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left
                    };

                    TextBox txt = new TextBox
                    {
                        Text = values[i],
                        Left = fx,
                        Top = fy + LABEL_H + 2,
                        Width = initialColW,
                        Height = isMulti ? MULTI_H : INPUT_H,
                        Multiline = isMulti,
                        BackColor = Color.FromArgb(16, 20, 24),
                        ForeColor = Color.FromArgb(210, 220, 228),
                        BorderStyle = BorderStyle.FixedSingle,
                        Font = new Font("Segoe UI", 9.5f),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left
                    };

                    boxes[i] = txt;
                    lbls[i] = lbl;
                    txts[i] = txt;

                    card.Controls.Add(lbl);
                    card.Controls.Add(txt);
                }
                fy += LABEL_H + inputH + 2 + ROW_GAP;
            }

            card.SizeChanged += (s, e) =>
            {
                int colW = Math.Max(60, (card.Width - PADDING * 2 - GAP) / 2);
                int cy = HEADER_H;
                for (int r = 0; r < rows; r++)
                {
                    bool rowHasMulti = false;
                    for (int c = 0; c < 2; c++)
                    {
                        int idx2 = r * 2 + c;
                        if (idx2 < labels.Length && labels[idx2] == "Responsibilities")
                            rowHasMulti = true;
                    }
                    int inputH = rowHasMulti ? MULTI_H : INPUT_H;

                    for (int c = 0; c < 2; c++)
                    {
                        int i = r * 2 + c;
                        if (i >= labels.Length || lbls[i] == null) break;

                        int fx = c == 0 ? PADDING : PADDING + colW + GAP;
                        lbls[i].Left = fx;
                        lbls[i].Top = cy;
                        lbls[i].Width = colW;
                        txts[i].Left = fx;
                        txts[i].Top = cy + LABEL_H + 2;
                        txts[i].Width = colW;
                    }
                    cy += LABEL_H + inputH + 2 + ROW_GAP;
                }
            };

            p.Controls.Add(card);
            return top + cardH + 12;
        }

        // ── Drawing helpers ────────────────────────────────────────────
        private void DrawRoundedRect(Graphics g, Pen pen, int x, int y, int w, int h, int r)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseAllFigures();
                g.DrawPath(pen, path);
            }
        }

        private void SetDoubleBuffered(Control control, bool doubleBuffered)
        {
            try
            {
                var property = typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (property != null && property.CanWrite)
                    property.SetValue(control, doubleBuffered);
            }
            catch { }
        }

        private void SetControlStyle(Control control, ControlStyles styles, bool value)
        {
            try
            {
                var method = typeof(Control).GetMethod("SetStyle",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(control, new object[] { styles, value });
            }
            catch { }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ApplicantStatusTrackingPage
            // 
            ClientSize = new Size(284, 261);
            Name = "ApplicantProfilePage";
            Load += ApplicantProfilePage_Load;
            ResumeLayout(false);

        }

        private void ApplicantProfilePage_Load(object sender, EventArgs e)
        {

        }
    }
}