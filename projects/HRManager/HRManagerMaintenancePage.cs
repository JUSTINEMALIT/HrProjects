using project;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


//hr manager/admin maintenance

namespace projects.HRManager
{
    public partial class HRManagerMaintenancePage : Form
    {
        private static readonly Color BgPage = Color.FromArgb(241, 245, 249);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderLight = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        private static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentGreen = Color.FromArgb(34, 197, 94);
        private static readonly Color AccentRed = Color.FromArgb(220, 38, 38);

        private HRManagerMainForm mainForm;
        private Panel contentPanel;
        private DatabaseConnection db;

        public HRManagerMaintenancePage(HRManagerMainForm main)
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
                Text = "Maintenance",
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
                Text = "Manage system reference data: departments, employment types, etc.",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextSecondary,
                Left = 24,
                Top = 70,
                AutoSize = true,
                BackColor = Color.Transparent
            });
            top += 32;

            string[] categories = { "Departments", "Employment Types", "Requirement Types", "Interview Types", "Assessment Types" };
            string[] tables = { "departments", "employment_types", "requirement_types", "interview_types", "assessment_types" };
            string[] columns = { "name", "name", "name", "name", "name", "name" };

            for (int i = 0; i < categories.Length; i++)
            {
                int idx = i;
                string category = categories[idx];
                string table = tables[idx];
                string column = columns[idx];

                Panel card = new Panel
                {
                    Left = 24,
                    Top = top,
                    Width = p.Width - 56,
                    Height = 100,
                    BackColor = BgCard,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                card.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(BorderLight))
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };

                card.Controls.Add(new Label
                {
                    Text = category,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Left = 20,
                    Top = 16,
                    AutoSize = true,
                    BackColor = Color.Transparent
                });

                try
                {
                    object count = db.Scalar($"SELECT COUNT(*) FROM {table} WHERE is_active = 1");
                    int itemCount = count != null ? Convert.ToInt32(count) : 0;
                    card.Controls.Add(new Label
                    {
                        Text = $"{itemCount} items",
                        Font = new Font("Segoe UI", 9f),
                        ForeColor = TextSecondary,
                        Left = 20,
                        Top = 40,
                        AutoSize = true,
                        BackColor = Color.Transparent
                    });
                }
                catch { }

                Button btnManage = new Button
                {
                    Text = "Manage",
                    Left = card.Width - 160,
                    Top = 30,
                    Width = 130,
                    Height = 40,
                    BackColor = AccentBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnManage.FlatAppearance.BorderSize = 0;
                btnManage.Click += (s, e) => ShowManageDialog(category, table, column);
                card.Controls.Add(btnManage);

                p.Controls.Add(card);
                top += 110;
            }
        }

        private void ShowManageDialog(string categoryName, string tableName, string columnName)
        {
            Form dialog = new Form
            {
                Text = $"Manage {categoryName}",
                Width = 500,
                Height = 400,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(241, 245, 249)
            };

            int ctrlTop = 16;

            dialog.Controls.Add(new Label
            {
                Text = $"Manage {categoryName}",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Left = 20,
                Top = ctrlTop,
                AutoSize = true
            });
            ctrlTop += 32;

            ListBox lst = new ListBox
            {
                Left = 20,
                Top = ctrlTop,
                Width = 450,
                Height = 180,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9f)
            };

            void ReloadList()
            {
                try
                {
                    DataTable dt = db.Query($"SELECT id, {columnName} FROM {tableName} WHERE is_active = 1 ORDER BY {columnName}");
                    lst.Items.Clear();
                    foreach (DataRow row in dt.Rows)
                        lst.Items.Add(new ItemData { Id = Convert.ToInt32(row["id"]), Name = row[columnName].ToString() });
                }
                catch { }
            }

            ReloadList();
            dialog.Controls.Add(lst);
            ctrlTop += 190;

            dialog.Controls.Add(new Label
            {
                Text = "Add New:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Left = 20,
                Top = ctrlTop,
                AutoSize = true
            });
            ctrlTop += 24;

            TextBox txtNew = new TextBox
            {
                Left = 20,
                Top = ctrlTop,
                Width = 300,
                Height = 30,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f)
            };
            dialog.Controls.Add(txtNew);

            Button btnAdd = new Button
            {
                Text = "Add",
                Left = 330,
                Top = ctrlTop,
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNew.Text)) { MessageBox.Show("Please enter a value", "Required"); return; }
                try
                {
                    db.Execute($"INSERT INTO {tableName} ({columnName}, is_active) VALUES (@val, 1)", ("@val", txtNew.Text.Trim()));
                    Audit.Log($"Added {categoryName}", tableName);
                    MessageBox.Show("Item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNew.Clear();
                    ReloadList();
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            dialog.Controls.Add(btnAdd);
            ctrlTop += 40;

            Button btnDelete = new Button
            {
                Text = "Delete Selected",
                Left = 20,
                Top = ctrlTop,
                Width = 130,
                Height = 32,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                if (lst.SelectedItem == null) { MessageBox.Show("Please select an item", "Required"); return; }
                var item = (ItemData)lst.SelectedItem;
                var confirm = MessageBox.Show($"Delete '{item.Name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
                try
                {
                    db.Execute($"UPDATE {tableName} SET is_active = 0 WHERE id = @id", ("@id", item.Id));
                    Audit.Log($"Deleted {categoryName}", tableName, item.Id);
                    MessageBox.Show("Item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadList();
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            dialog.Controls.Add(btnDelete);

            dialog.ShowDialog();
        }

        private class ItemData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }

        private void InitializeComponent()
        {
            ClientSize = new Size(284, 261);
            Name = "HRManagerMaintenancePage";
            Load += HRManagerMaintenancePage_Load;
            ResumeLayout(false);

        }

        private void HRManagerMaintenancePage_Load(object sender, EventArgs e)
        {

        }
    }
}