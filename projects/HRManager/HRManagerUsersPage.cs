using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace projects.HRManager
{
    public partial class HRManagerUsersPage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerUsersPage(HRManagerMainForm main)
        {
            this.mainForm = main;
            this.contentPanel = main.contentPanel;
            this.db = new DatabaseConnection();
            InitializePage();
        }

        private void InitializePage()
        {
            mainForm.ClearContent();
            var p = contentPanel;
            p.BackColor = BgPage;
            p.AutoScroll = true;

            int top = 24;

            p.Controls.Add(new Label
            {
                Text = "User Management",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = TextPrimary,
                Left = 24,
                Top = top,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 36;

            p.Controls.Add(new Label
            {
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = 70,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 28;

            // Add User Button
            Button btnAddUser = new Button
            {
                Text = "➕ Add New User",
                Left = 24,
                Top = top,
                Width = 140,
                Height = 36,
                BackColor = AccentGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9f),
                Cursor = Cursors.Hand
            };
            btnAddUser.FlatAppearance.BorderSize = 0;
            btnAddUser.Click += (s, e) => ShowAddUserDialog();
            p.Controls.Add(btnAddUser);
            top += 48;

            // Users List
            Panel listPanel = new Panel { Left = 24, Top = top, Width = p.Width - 56, Height = 600, BackColor = Color.Transparent, AutoScroll = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            LoadUsersList(listPanel);

            p.Controls.Add(listPanel);
        }

        private void LoadUsersList(Panel listPanel)
        {
            listPanel.Controls.Clear();
            try
            {
                DataTable users = db.Query(
                    @"SELECT u.id, u.username, u.full_name, u.email, r.role_name, u.is_active, u.created_at
                      FROM hr_users u
                      JOIN roles r ON r.id = u.role_id
                      ORDER BY u.created_at DESC");

                if (users.Rows.Count == 0)
                {
                    Label emptyLabel = new Label { Text = "No users found", Font = new Font("Segoe UI", 11f), ForeColor = TextMuted, Left = 0, Top = 20, AutoSize = true };
                    listPanel.Controls.Add(emptyLabel);
                    return;
                }

                int itemTop = 0;
                foreach (DataRow row in users.Rows)
                {
                    int userId = Convert.ToInt32(row["id"]);
                    bool isActive = Convert.ToBoolean(row["is_active"]);
                    string role = row["role_name"].ToString();

                    Panel userCard = new Panel { Left = 0, Top = itemTop, Width = listPanel.Width - 20, Height = 80, BackColor = BgCard, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                    userCard.Paint += (s, e) =>
                    {
                        using (var pen = new Pen(BorderLight, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, userCard.Width - 1, userCard.Height - 1);
                    };

                    // Avatar
                    Panel avatar = new Panel { Left = 12, Top = 12, Width = 48, Height = 48, BackColor = AccentBlue };
                    avatar.Paint += (s, e) =>
                    {
                        e.Graphics.Clear(AccentBlue);
                        string initials = row["full_name"].ToString().Split(' ')[0][0].ToString() +
                                         (row["full_name"].ToString().Contains(" ") ? row["full_name"].ToString().Split(' ')[1][0].ToString() : "");
                        using (var f = new Font("Segoe UI Semibold", 12f))
                        using (var br = new SolidBrush(Color.White))
                        {
                            var sz = e.Graphics.MeasureString(initials, f);
                            e.Graphics.DrawString(initials, f, br, (avatar.Width - sz.Width) / 2, (avatar.Height - sz.Height) / 2);
                        }
                    };
                    userCard.Controls.Add(avatar);

                    // User info
                    userCard.Controls.Add(new Label { Text = row["full_name"].ToString(), Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold), ForeColor = TextPrimary, Left = 68, Top = 6, AutoSize = true, BackColor = Color.Transparent });
                    userCard.Controls.Add(new Label { Text = "@" + row["username"].ToString(), Font = new Font("Segoe UI", 8.5f), ForeColor = TextSecondary, Left = 68, Top = 26, AutoSize = true, BackColor = Color.Transparent });
                    userCard.Controls.Add(new Label { Text = row["email"].ToString(), Font = new Font("Segoe UI", 8.5f), ForeColor = TextMuted, Left = 68, Top = 44, AutoSize = true, BackColor = Color.Transparent });

                    // Role badge
                    Panel roleBadge = new Panel { Left = userCard.Width - 250, Top = 14, Width = 80, Height = 24, BackColor = Color.FromArgb(220, 240, 255), Anchor = AnchorStyles.Top | AnchorStyles.Right };
                    roleBadge.Paint += (s, e) =>
                    {
                        using (var pen = new Pen(AccentBlue, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, roleBadge.Width - 1, roleBadge.Height - 1);
                    };
                    roleBadge.Controls.Add(new Label { Text = role, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = AccentBlue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                    userCard.Controls.Add(roleBadge);

                    // Status
                    Panel statusPill = new Panel { Left = userCard.Width - 160, Top = 14, Width = 65, Height = 24, BackColor = isActive ? Color.FromArgb(220, 252, 231) : Color.FromArgb(254, 242, 242), Anchor = AnchorStyles.Top | AnchorStyles.Right };
                    statusPill.Paint += (s, e) =>
                    {
                        Color statusColor = isActive ? AccentGreen : AccentRed;
                        using (var pen = new Pen(statusColor, 1))
                            e.Graphics.DrawRectangle(pen, 0, 0, statusPill.Width - 1, statusPill.Height - 1);
                    };
                    statusPill.Controls.Add(new Label { Text = isActive ? "Active" : "Inactive", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = isActive ? AccentGreen : AccentRed, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                    userCard.Controls.Add(statusPill);

                    // Edit Button
                    Button btnEdit = new Button { Text = "Edit", Left = userCard.Width - 90, Top = 22, Width = 35, Height = 28, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                    btnEdit.FlatAppearance.BorderSize = 0;
                    btnEdit.Click += (s, e) => ShowEditUserDialog(userId);
                    userCard.Controls.Add(btnEdit);

                    // Deactivate Button
                    Button btnAction = new Button { Text = isActive ? "Deactivate" : "Activate", Left = userCard.Width - 50, Top = 22, Width = 40, Height = 28, BackColor = isActive ? AccentRed : AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 7.5f), Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
                    btnAction.FlatAppearance.BorderSize = 0;
                    btnAction.Click += (s, e) =>
                    {
                        bool newStatus = !isActive;
                        db.Execute("UPDATE hr_users SET is_active = @status WHERE id = @id", ("@status", newStatus ? 1 : 0), ("@id", userId));
                        Audit.Log(newStatus ? "User Activated" : "User Deactivated", "hr_users", userId, row["full_name"].ToString());
                        LoadUsersList(listPanel);
                    };
                    userCard.Controls.Add(btnAction);

                    listPanel.Controls.Add(userCard);
                    itemTop += 86;
                }
            }
            catch (Exception ex)
            {
                Label errLabel = new Label { Text = $"Error: {ex.Message}", Font = new Font("Segoe UI", 10f), ForeColor = AccentRed, Left = 0, Top = 0, AutoSize = true };
                listPanel.Controls.Add(errLabel);
            }
        }

        private void ShowAddUserDialog()
        {
            Form dialog = new Form
            {
                Text = "Add New User",
                Width = 450,
                Height = 400,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = BgPage
            };

            int y = 20;

            // Full Name
            dialog.Controls.Add(new Label { Text = "Full Name:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtFullName = new TextBox { Left = 20, Top = y, Width = 390, Height = 28, Font = new Font("Segoe UI", 9f), BackColor = BgCard };
            dialog.Controls.Add(txtFullName);
            y += 36;

            // Username
            dialog.Controls.Add(new Label { Text = "Username:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtUsername = new TextBox { Left = 20, Top = y, Width = 390, Height = 28, Font = new Font("Segoe UI", 9f), BackColor = BgCard };
            dialog.Controls.Add(txtUsername);
            y += 36;

            // Email
            dialog.Controls.Add(new Label { Text = "Email:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtEmail = new TextBox { Left = 20, Top = y, Width = 390, Height = 28, Font = new Font("Segoe UI", 9f), BackColor = BgCard };
            dialog.Controls.Add(txtEmail);
            y += 36;

            // Password
            dialog.Controls.Add(new Label { Text = "Password:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;
            TextBox txtPassword = new TextBox { Left = 20, Top = y, Width = 390, Height = 28, Font = new Font("Segoe UI", 9f), BackColor = BgCard, UseSystemPasswordChar = true };
            dialog.Controls.Add(txtPassword);
            y += 36;

            // Role
            dialog.Controls.Add(new Label { Text = "Role:", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = TextPrimary, Left = 20, Top = y, AutoSize = true });
            y += 24;
            ComboBox cmbRole = new ComboBox { Left = 20, Top = y, Width = 390, Height = 28, Font = new Font("Segoe UI", 9f), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "HR Staff", "HR Manager", "Admin" });
            cmbRole.SelectedIndex = 0;
            dialog.Controls.Add(cmbRole);
            y += 44;

            // Save Button
            Button btnSave = new Button { Text = "Save", Left = 20, Top = y, Width = 180, Height = 36, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9f) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error");
                    return;
                }

                try
                {
                    int roleId = cmbRole.SelectedIndex == 0 ? 3 : cmbRole.SelectedIndex == 1 ? 2 : 1;
                    db.Execute(
                        @"INSERT INTO hr_users (username, password, full_name, email, role_id, is_active)
                          VALUES (@username, @password, @fullName, @email, @roleId, 1)",
                        ("@username", txtUsername.Text),
                        ("@password", txtPassword.Text),
                        ("@fullName", txtFullName.Text),
                        ("@email", txtEmail.Text),
                        ("@roleId", roleId));

                    Audit.Log("New User Created", "hr_users", 0, txtFullName.Text);
                    MessageBox.Show("User created successfully!", "Success");
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error");
                }
            };
            dialog.Controls.Add(btnSave);

            Button btnCancel = new Button { Text = "Cancel", Left = 210, Top = y, Width = 200, Height = 36, BackColor = Color.FromArgb(200, 200, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            btnCancel.FlatAppearance.BorderSize = 0;
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }

        private void ShowEditUserDialog(int userId)
        {
            MessageBox.Show("Edit user functionality coming soon!", "Feature", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            SuspendLayout(); 
            ClientSize = new Size(284, 261);
            Name = "HRManagerUsersPage";
            Load += HRManagerUsersPage_Load;
            ResumeLayout(false);

        }

        private void HRManagerUsersPage_Load(object sender, EventArgs e)
        {

        }
    }
}