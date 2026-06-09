using HRApplicant;
using project;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ApplicantPortal
{
    public partial class LoginForm : Form
    {
        // ── Palette ──────────────────────────────────────────────────
        static readonly Color BgDeep = Color.FromArgb(11, 14, 18);
        static readonly Color BgCard = Color.FromArgb(18, 23, 29);
        static readonly Color BgInput = Color.FromArgb(24, 30, 38);
        static readonly Color Border = Color.FromArgb(38, 48, 60);
        static readonly Color AccentA = Color.FromArgb(56, 149, 255);   // blue
        static readonly Color AccentB = Color.FromArgb(99, 102, 241);   // indigo
        static readonly Color TextHi = Color.FromArgb(236, 240, 245);
        static readonly Color TextMid = Color.FromArgb(140, 155, 170);
        static readonly Color TextDim = Color.FromArgb(72, 90, 108);
        static readonly Color ErrColor = Color.FromArgb(248, 81, 73);

        private TextBox txtEmail, txtPass;
        private Label lblError;
        private Button btnLogin;

        public LoginForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Applicant Portal";
            this.ClientSize = new Size(440, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDeep;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.5f);

            // ── Gradient top bar ─────────────────────────────────────
            Panel topBar = new Panel { Left = 0, Top = 0, Width = 440, Height = 4 };
            topBar.Paint += (s, e) =>
            {
                using var br = new LinearGradientBrush(topBar.ClientRectangle, AccentA, AccentB, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, topBar.ClientRectangle);
            };
            this.Controls.Add(topBar);

            // ── Logo mark ────────────────────────────────────────────
            Panel logo = new Panel { Size = new Size(56, 56), Location = new Point(192, 44), BackColor = Color.Transparent };
            logo.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var br = new LinearGradientBrush(logo.ClientRectangle, AccentA, AccentB, 45f);
                e.Graphics.FillEllipse(br, 0, 0, 55, 55);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("HR", new Font("Segoe UI", 13f, FontStyle.Bold), Brushes.White, new RectangleF(0, 0, 56, 56), sf);
            };
            this.Controls.Add(logo);

            // ── Title block ──────────────────────────────────────────
            Label title = new Label
            {
                Text = "Applicant Portal",
                ForeColor = TextHi,
                Font = new Font("Segoe UI", 17f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(440, 34),
                Location = new Point(0, 112),
                BackColor = Color.Transparent
            };
            this.Controls.Add(title);

            Label sub = new Label
            {
                Text = "HR Recruitment Management System",
                ForeColor = TextDim,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(440, 22),
                Location = new Point(0, 148),
                BackColor = Color.Transparent
            };
            this.Controls.Add(sub);

            // ── Card ─────────────────────────────────────────────────
            Panel card = new Panel
            {
                Size = new Size(340, 238),
                Location = new Point(50, 184),
                BackColor = BgCard
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Border, 1);
                DrawRoundRect(e.Graphics, pen, 0, 0, card.Width - 1, card.Height - 1, 10);
            };
            this.Controls.Add(card);

            // Email
            card.Controls.Add(FieldLabel("Email address", 24, 24));
            txtEmail = InputBox(24, 46, 292);
            card.Controls.Add(txtEmail);

            // Password
            card.Controls.Add(FieldLabel("Password", 24, 96));
            txtPass = InputBox(24, 118, 292);
            txtPass.UseSystemPasswordChar = true;
            card.Controls.Add(txtPass);

            // Error label
            lblError = new Label
            {
                Text = "",
                ForeColor = ErrColor,
                Font = new Font("Segoe UI", 8f),
                Location = new Point(24, 160),
                Size = new Size(292, 18),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblError);

            // Sign in button
            btnLogin = new Button
            {
                Text = "Sign in",
                Size = new Size(292, 42),
                Location = new Point(24, 182),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = AccentA
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Paint += PaintGradientBtn;
            btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(btnLogin);

            // ── Footer links ─────────────────────────────────────────
            Label noAcc = new Label
            {
                Text = "No account yet?",
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(100, 438),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(noAcc);

            LinkLabel lnkRegister = new LinkLabel
            {
                Text = "Register here",
                LinkColor = AccentA,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(222, 438),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lnkRegister.LinkClicked += (s, e) => { new RegisterForm().Show(); this.Hide(); };
            this.Controls.Add(lnkRegister);

            LinkLabel lnkAdmin = new LinkLabel
            {
                Text = "HR / Admin Login",
                LinkColor = TextDim,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(160, 462),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lnkAdmin.LinkClicked += (s, e) => { new AdminLoginForm().Show(); this.Hide(); };
            this.Controls.Add(lnkAdmin);

            // Enter key
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) { lblError.Text = "Email is required."; return; }
            if (string.IsNullOrWhiteSpace(txtPass.Text)) { lblError.Text = "Password is required."; return; }
            try
            {
                var db = new DatabaseConnection();
                DataTable result = db.Query(
                    "SELECT id, first_name, last_name, email FROM applicants WHERE email=@em AND password=@pw",
                    ("@em", txtEmail.Text.Trim()), ("@pw", txtPass.Text.Trim()));
                if (result.Rows.Count > 0)
                {
                    project.Session.ApplicantId = Convert.ToInt32(result.Rows[0]["id"]);
                    project.Session.ApplicantName = result.Rows[0]["first_name"] + " " + result.Rows[0]["last_name"];
                    project.Session.ApplicantEmail = result.Rows[0]["email"].ToString();
                    new ApplicantMainForm().Show();
                    this.Hide();
                }
                else
                {
                    lblError.Text = "Invalid email or password.";
                    txtPass.Clear();
                }
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; }
        }

        // ── Helpers ──────────────────────────────────────────────────
        private Label FieldLabel(string text, int x, int y) => new Label
        {
            Text = text,
            ForeColor = TextMid,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            Location = new Point(x, y),
            AutoSize = true,
            BackColor = Color.Transparent
        };

        private TextBox InputBox(int x, int y, int w)
        {
            var tb = new TextBox
            {
                Size = new Size(w, 34),
                Location = new Point(x, y),
                BackColor = BgInput,
                ForeColor = TextHi,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            return tb;
        }

        private void PaintGradientBtn(object sender, PaintEventArgs e)
        {
            var btn = (Button)sender;
            using var br = new LinearGradientBrush(btn.ClientRectangle, AccentA, AccentB, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(br, btn.ClientRectangle);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(btn.Text, btn.Font, Brushes.White, btn.ClientRectangle, sf);
        }

        private void DrawRoundRect(Graphics g, Pen pen, int x, int y, int w, int h, int r)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseAllFigures();
            g.DrawPath(pen, path);
        }

        private void LoginForm_Load(object sender, EventArgs e) { }
    }
}
