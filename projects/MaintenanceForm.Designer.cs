namespace project
{
    partial class MaintenanceForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.cmbTableSelect = new System.Windows.Forms.ComboBox();
            this.dgvMaintenanceData = new System.Windows.Forms.DataGridView();
            this.lblID = new System.Windows.Forms.Label();
            this.txtID = new System.Windows.Forms.TextBox();
            this.lblValueName = new System.Windows.Forms.Label();
            this.txtValueName = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaintenanceData)).BeginInit();
            this.SuspendLayout();
             
            // cmbTableSelect            
            this.cmbTableSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTableSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbTableSelect.FormattingEnabled = true;
            this.cmbTableSelect.Items.AddRange(new object[] {
            "Departments",
            "Positions",
            "Employment Types",
            "Requirement Types",
            "Interview Types",
            "Assessment Types"});
            this.cmbTableSelect.Location = new System.Drawing.Point(30, 60);
            this.cmbTableSelect.Name = "cmbTableSelect";
            this.cmbTableSelect.Size = new System.Drawing.Size(220, 28);
            this.cmbTableSelect.TabIndex = 0;
            this.cmbTableSelect.SelectedIndexChanged += new System.EventHandler(this.cmbTableSelect_SelectedIndexChanged);
            
            // dgvMaintenanceData           
            this.dgvMaintenanceData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMaintenanceData.Location = new System.Drawing.Point(280, 60);
            this.dgvMaintenanceData.Name = "dgvMaintenanceData";
            this.dgvMaintenanceData.RowHeadersWidth = 51;
            this.dgvMaintenanceData.Size = new System.Drawing.Size(530, 380);
            this.dgvMaintenanceData.TabIndex = 1;
            this.dgvMaintenanceData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMaintenanceData_CellClick);
            
            // lblID           
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(30, 110);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(73, 16);
            this.lblID.TabIndex = 2;
            this.lblID.Text = "Record ID:";
            
            // txtID           
            this.txtID.Location = new System.Drawing.Point(30, 135);
            this.txtID.Name = "txtID";
            this.txtID.ReadOnly = true;
            this.txtID.Size = new System.Drawing.Size(220, 22);
            this.txtID.TabIndex = 3;
             
            // lblValueName           
            this.lblValueName.AutoSize = true;
            this.lblValueName.Location = new System.Drawing.Point(30, 180);
            this.lblValueName.Name = "lblValueName";
            this.lblValueName.Size = new System.Drawing.Size(121, 16);
            this.lblValueName.TabIndex = 4;
            this.lblValueName.Text = "Value Name / Title:";
            
            // txtValueName            
            this.txtValueName.Location = new System.Drawing.Point(30, 205);
            this.txtValueName.Name = "txtValueName";
            this.txtValueName.Size = new System.Drawing.Size(220, 22);
            this.txtValueName.TabIndex = 5;
            
            // btnAdd          
            this.btnAdd.Location = new System.Drawing.Point(30, 260);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 35);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "Add New";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            
            // btnUpdate
            this.btnUpdate.Location = new System.Drawing.Point(150, 260);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(100, 35);
            this.btnUpdate.TabIndex = 7;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            
            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(30, 310);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 35);
            this.btnDelete.TabIndex = 8;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            
            // btnClear
            this.btnClear.Location = new System.Drawing.Point(150, 310);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 35);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear Fields";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            
            // lblHeader
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(26, 25);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(250, 20);
            this.lblHeader.TabIndex = 10;
            this.lblHeader.Text = "Select Maintenance Category:";
            
            // MaintenanceForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 470);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtValueName);
            this.Controls.Add(this.lblValueName);
            this.Controls.Add(this.txtID);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.dgvMaintenanceData);
            this.Controls.Add(this.cmbTableSelect);
            this.Name = "MaintenanceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Maintenance Module";
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaintenanceData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbTableSelect;
        private System.Windows.Forms.DataGridView dgvMaintenanceData;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.Label lblValueName;
        private System.Windows.Forms.TextBox txtValueName;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblHeader;
    }
}