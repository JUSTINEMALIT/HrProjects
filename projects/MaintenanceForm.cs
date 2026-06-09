using System;
using System.Data;
using System.Windows.Forms;

namespace project
{
    public partial class MaintenanceForm : Form
    {
        
        DatabaseConnection db = new DatabaseConnection();
        
       
        string currentTable = ""; 
        string primaryKeyColumn = "";
        string valueColumn = "";

        public MaintenanceForm()
        {
            InitializeComponent();
            
            
            cmbTableSelect.SelectedIndex = 0; 
        }

        
        void SetTableMapping()
        {
            if (cmbTableSelect.SelectedItem == null)
            {
                return;
            }

            string selected = cmbTableSelect.SelectedItem.ToString();

           
            if (selected == "Departments")
            {
                currentTable = "departments";
                primaryKeyColumn = "id";
                valueColumn = "name";
            }
            else if (selected == "Positions")
            {
                currentTable = "job_vacancies";
                primaryKeyColumn = "id";
                valueColumn = "title";
            }
            else if (selected == "Employment Types")
            {
                currentTable = "employment_types";
                primaryKeyColumn = "id";
                valueColumn = "type_name";
            }
            else if (selected == "Requirement Types")
            {
                currentTable = "requirement_types";
                primaryKeyColumn = "id";
                valueColumn = "name";
            }
            else if (selected == "Interview Types")
            {
                currentTable = "interview_types";
                primaryKeyColumn = "id";
                valueColumn = "type_name";
            }
            else if (selected == "Assessment Types")
            {
                currentTable = "assessment_types";
                primaryKeyColumn = "id";
                valueColumn = "type_name";
            }
        }

        
        public void LoadMaintenanceData()
        {
            try
            {
                SetTableMapping();
                
                if (currentTable == "")
                {
                    return;
                }

               
                string query = "SELECT " + primaryKeyColumn + " AS ID, " + valueColumn + " AS Name FROM " + currentTable;
                
                DataTable dt = db.Query(query);
                dgvMaintenanceData.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void cmbTableSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMaintenanceData();
            ClearFields();
        }

        
        private void dgvMaintenanceData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMaintenanceData.Rows[e.RowIndex];
                txtID.Text = row.Cells["ID"].Value.ToString();
                txtValueName.Text = row.Cells["Name"].Value.ToString();
            }
        }

        

        // 1. ADD NEW RECORD
        private void btnAdd_Click(object sender, EventArgs e)
        {
            
            if (txtValueName.Text.Trim() == "")
            {
                MessageBox.Show("Please fill up the Value Name textbox first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = "INSERT INTO " + currentTable + " (" + valueColumn + ") VALUES (@val)";
                
                
                var p1 = ("@val", (object)txtValueName.Text.Trim());
                int result = db.Execute(query, p1);

                if (result > 0)
                {
                    MessageBox.Show("Record has been added successfully!", "Success");
                    
                    
                    Audit.Log("Added reusable value", currentTable, 0, "Value: " + txtValueName.Text.Trim());
                    
                    LoadMaintenanceData();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on inserting record: " + ex.Message);
            }
        }

        // 2. UPDATE EXISTING RECORD
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            
            if (txtID.Text == "")
            {
                MessageBox.Show("Please select a record from the list first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtValueName.Text.Trim() == "")
            {
                MessageBox.Show("Textbox cannot be blank.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = "UPDATE " + currentTable + " SET " + valueColumn + " = @val WHERE " + primaryKeyColumn + " = @id";
                
                var p1 = ("@val", (object)txtValueName.Text.Trim());
                var p2 = ("@id", (object)txtID.Text);
                int result = db.Execute(query, p1, p2);

                if (result > 0)
                {
                    MessageBox.Show("Record has been updated!", "Success");
                    
                    Audit.Log("Updated reusable value", currentTable, Convert.ToInt32(txtID.Text), "New Value: " + txtValueName.Text.Trim());
                    
                    LoadMaintenanceData();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on updating record: " + ex.Message);
            }
        }

        // 3. DELETE RECORD
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtID.Text == "")
            {
                MessageBox.Show("Please select a record to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            DialogResult response = MessageBox.Show("Are you sure you really want to delete this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (response == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM " + currentTable + " WHERE " + primaryKeyColumn + " = @id";
                    
                    var p1 = ("@id", (object)txtID.Text);
                    int result = db.Execute(query, p1);

                    if (result > 0)
                    {
                        MessageBox.Show("Record deleted successfully.", "Success");
                        
                        Audit.Log("Deleted reusable value", currentTable, Convert.ToInt32(txtID.Text), "Deleted ID: " + txtID.Text);
                        
                        LoadMaintenanceData();
                        ClearFields();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error on deleting record: " + ex.Message);
                }
            }
        }

       
        void ClearFields()
        {
            txtID.Text = "";
            txtValueName.Text = "";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
    }
}