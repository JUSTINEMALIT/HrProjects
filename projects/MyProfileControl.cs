// File: MyProfileControl.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace project
{
    public class MyProfileControl : UserControl
    {
        // Colors base sa iyong dashboard themes
        private Color bg = Color.FromArgb(20, 20, 20);
        private Color card = Color.FromArgb(28, 28, 28);
        private Color inputBg = Color.FromArgb(42, 42, 42);
        private Color textColor = Color.FromArgb(240, 240, 240);
        private Color mutedText = Color.FromArgb(150, 150, 150);

        private Panel cardContainer;
        private TableLayoutPanel masterLayout;
        private TableLayoutPanel personalGrid;
        private TableLayoutPanel educationGrid;
        private Panel headerSection;
        private Panel skillsGroup;
        private Panel expGroup;
        private Button btnEditPhoto;

        public MyProfileControl()
        {
            this.BackColor = bg;
            this.Padding = new Padding(25); // Puwang para hindi dumikit sa sidebar at bintana
            
            BuildUI();
        }

        private void BuildUI()
        {
            // 1. Title block sa itaas
            Panel titlePanel = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.Transparent };
            Label lblTitle = new Label { Text = "My Profile", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, Location = new Point(0, 0), AutoSize = true };
            Label lblSubtitle = new Label { Text = "Personal, address, education, and work information. You can edit this anytime.", Font = new Font("Segoe UI", 9.5F), ForeColor = mutedText, Location = new Point(0, 35), AutoSize = true };
            titlePanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });
            this.Controls.Add(titlePanel);

            // 2. Ang Main Card Panel (Wrapper na may Scrollbar)
            cardContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = card,
                Padding = new Padding(25, 25, 35, 25), // Binigyan ng 35px padding sa kanan para sa safe zone ng scrollbar
                AutoScroll = true
            };
            cardContainer.SizeChanged += (s, e) => RoundControl(cardContainer, 14);
            this.Controls.Add(cardContainer);
            
            titlePanel.SendToBack();

            // 3. Master Layout (100% dynamic width - WALANG HARDCODED WIDTHS)
            masterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top, // Dynamic na susunod sa lapad ng cardContainer
                ColumnCount = 1,
                RowCount = 10,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            masterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cardContainer.Controls.Add(masterLayout);

            // --- SECTION A: AVATAR USER INFO ---
            headerSection = new Panel { Dock = DockStyle.Top, Height = 85, Margin = new Padding(0) };
            headerSection.SizeChanged += (s, e) => { btnEditPhoto.Left = headerSection.Width - btnEditPhoto.Width - 5; };

            Panel avatar = new Panel { Size = new Size(60, 60), Location = new Point(0, 10), BackColor = Color.FromArgb(219, 234, 254) };
            avatar.Paint += (s, e) => { GraphicsPath gp = new GraphicsPath(); gp.AddEllipse(0, 0, 59, 59); avatar.Region = new Region(gp); };
            Label avatarText = new Label { Text = "JA", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(30, 64, 175), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            avatar.Controls.Add(avatarText);

            Label lblName = new Label { Text = "Juan C. Applicant", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, Location = new Point(75, 10), AutoSize = true };
            Label lblEmail = new Label { Text = "juan.applicant@email.com", Font = new Font("Segoe UI", 9.5F), ForeColor = mutedText, Location = new Point(75, 34), AutoSize = true };
            Label lblBadge = new Label { Text = "Active account", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(21, 128, 61), BackColor = Color.FromArgb(220, 252, 231), Location = new Point(75, 58), AutoSize = true, Padding = new Padding(6, 2, 6, 2) };
            lblBadge.Paint += (s, e) => RoundControl(lblBadge, 8);

            btnEditPhoto = new Button { Text = "Edit photo", FlatStyle = FlatStyle.Flat, Size = new Size(95, 30), Top = 15, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            btnEditPhoto.FlatAppearance.BorderSize = 0;

            headerSection.Controls.AddRange(new Control[] { avatar, lblName, lblEmail, lblBadge, btnEditPhoto });
            masterLayout.Controls.Add(headerSection, 0, 0);
            masterLayout.Controls.Add(CreateDivider(), 0, 1);

            // --- SECTION B: PERSONAL INFORMATION ---
            masterLayout.Controls.Add(CreateSectionTitle("PERSONAL INFORMATION"), 0, 2);
            personalGrid = CreateGridBase(4, 2);
            personalGrid.Controls.Add(CreateFieldGroup("First name", "Juan"), 0, 0);
            personalGrid.Controls.Add(CreateFieldGroup("Last name", "Applicant"), 1, 0);
            personalGrid.Controls.Add(CreateFieldGroup("Middle name", "Cruz"), 0, 1);
            personalGrid.Controls.Add(CreateFieldGroup("Suffix", "—"), 1, 1);
            personalGrid.Controls.Add(CreateFieldGroup("Date of birth", "May 15, 1998"), 0, 2);
            personalGrid.Controls.Add(CreateFieldGroup("Gender", "Male"), 1, 2);
            personalGrid.Controls.Add(CreateFieldGroup("Civil status", "Single"), 0, 3);
            personalGrid.Controls.Add(CreateFieldGroup("Contact number", "09171234567"), 1, 3);
            masterLayout.Controls.Add(personalGrid, 0, 3);
            masterLayout.Controls.Add(CreateDivider(), 0, 4);

            // --- SECTION C: EDUCATION ---
            masterLayout.Controls.Add(CreateSectionTitle("EDUCATION"), 0, 5);
            educationGrid = CreateGridBase(2, 2);
            educationGrid.Controls.Add(CreateFieldGroup("Highest education", "College"), 0, 0);
            educationGrid.Controls.Add(CreateFieldGroup("Year graduated", "2020"), 1, 0);
            educationGrid.Controls.Add(CreateFieldGroup("School", "University of the Philippines"), 0, 1);
            educationGrid.Controls.Add(CreateFieldGroup("Degree", "BS Computer Science"), 1, 1);
            masterLayout.Controls.Add(educationGrid, 0, 6);
            masterLayout.Controls.Add(CreateDivider(), 0, 7);

            // --- SECTION D: SKILLS & EXPERIENCE ---
            skillsGroup = CreateFullWidthFieldGroup("Skills", "C#, .NET, SQL Server, HTML, CSS, JavaScript", 38);
            expGroup = CreateFullWidthFieldGroup("Work experience", "Software Intern, TechCorp Inc. (2019-2020)", 75, true);
            masterLayout.Controls.Add(skillsGroup, 0, 8);
            masterLayout.Controls.Add(expGroup, 0, 9);

            // --- SECTION E: ACTION BUTTONS ---
            Panel actionBtnPanel = new Panel { Height = 50, Margin = new Padding(0, 15, 0, 0), BackColor = Color.Transparent, Dock = DockStyle.Top };
            Button btnEditProfile = new Button { Text = "✎ Edit profile", FlatStyle = FlatStyle.Flat, Size = new Size(120, 38), Location = new Point(0, 5), BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            btnEditProfile.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            Button btnViewFull = new Button { Text = "View full profile", FlatStyle = FlatStyle.Flat, Size = new Size(130, 38), Location = new Point(130, 5), BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            btnViewFull.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            actionBtnPanel.Controls.AddRange(new Control[] { btnEditProfile, btnViewFull });
            
            masterLayout.RowCount++;
            masterLayout.Controls.Add(actionBtnPanel, 0, masterLayout.RowCount - 1);
        }

        // UI Helpers (DOCKING MECHANISM - Automatic Fitting)
        private Label CreateSectionTitle(string text) => new Label { Text = text, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = mutedText, Margin = new Padding(0, 15, 0, 5), AutoSize = true };
        private Panel CreateDivider() => new Panel { Height = 1, BackColor = Color.FromArgb(45, 45, 45), Margin = new Padding(0, 10, 0, 10), Dock = DockStyle.Top };

        private TableLayoutPanel CreateGridBase(int rows, int cols)
        {
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Top, Height = rows * 62, ColumnCount = cols, RowCount = rows, Margin = new Padding(0), BackColor = Color.Transparent };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // Tamang hating 50%
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < rows; i++) tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            return tlp;
        }

        private Panel CreateFieldGroup(string labelName, string value)
        {
            Panel container = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 15, 0), BackColor = Color.Transparent };
            Label lbl = new Label { Text = labelName, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = mutedText, Top = 0, AutoSize = true };
            
            Panel txtBg = new Panel { Top = 20, Left = 0, Height = 36, BackColor = inputBg, Padding = new Padding(10, 8, 10, 8), Dock = DockStyle.Top };
            txtBg.Paint += (s, e) => RoundControl(txtBg, 6);
            
            TextBox txt = new TextBox { Text = value, BackColor = inputBg, ForeColor = textColor, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Segoe UI", 10F) };
            
            txtBg.Controls.Add(txt);
            container.Controls.AddRange(new Control[] { lbl, txtBg });
            return container;
        }

        private Panel CreateFullWidthFieldGroup(string labelName, string value, int textHeight, bool isMultiline = false)
        {
            // Ginawang DockStyle.Top para sumunod sa 100% na lapad ng master layout kung gaano man kalaki ang form
            Panel container = new Panel { Dock = DockStyle.Top, Height = textHeight + 28, Margin = new Padding(0, 5, 15, 5), BackColor = Color.Transparent };
            Label lbl = new Label { Text = labelName, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = mutedText, Top = 0, AutoSize = true };
            
            Panel txtBg = new Panel { Top = 20, Left = 0, Height = textHeight, BackColor = inputBg, Padding = new Padding(10, 8, 10, 8), Dock = DockStyle.Top };
            txtBg.Paint += (s, e) => RoundControl(txtBg, 6);
            
            TextBox txt = new TextBox { Text = value, BackColor = inputBg, ForeColor = textColor, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill, ReadOnly = true, Multiline = isMultiline, Font = new Font("Segoe UI", 10F) };
            
            txtBg.Controls.Add(txt);
            container.Controls.AddRange(new Control[] { lbl, txtBg });
            return container;
        }

        private void RoundControl(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius - 1, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius - 1, control.Height - radius - 1, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius - 1, radius, radius, 90, 90);
            path.CloseAllFigures();
            control.Region = new Region(path);
        }
    }
}