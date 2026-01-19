namespace Calculator.Client
{
    partial class LogsForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.ComboBox cmbFiles;
        private System.Windows.Forms.DataGridView gridLogs;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.lblFolder = new System.Windows.Forms.Label();
            this.cmbFiles = new System.Windows.Forms.ComboBox();
            this.gridLogs = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.gridLogs)).BeginInit();
            this.SuspendLayout();

            // btnSelectFolder
            this.btnSelectFolder.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(130, 29);
            this.btnSelectFolder.TabIndex = 0;
            this.btnSelectFolder.Text = "Select Logs Folder";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);

            // lblFolder
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(148, 16);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(120, 20);
            this.lblFolder.TabIndex = 1;
            this.lblFolder.Text = "No folder selected";

            // cmbFiles
            this.cmbFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFiles.Location = new System.Drawing.Point(12, 52);
            this.cmbFiles.Name = "cmbFiles";
            this.cmbFiles.Size = new System.Drawing.Size(260, 28);
            this.cmbFiles.TabIndex = 2;
            this.cmbFiles.SelectedIndexChanged += new System.EventHandler(this.cmbFiles_SelectedIndexChanged);

            // gridLogs
            this.gridLogs.Location = new System.Drawing.Point(12, 92);
            this.gridLogs.Name = "gridLogs";
            this.gridLogs.RowHeadersWidth = 51;
            this.gridLogs.Size = new System.Drawing.Size(760, 360);
            this.gridLogs.TabIndex = 3;

            // LogsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 464);
            this.Controls.Add(this.gridLogs);
            this.Controls.Add(this.cmbFiles);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.btnSelectFolder);
            this.Name = "LogsForm";
            this.Text = "User Logs";
            ((System.ComponentModel.ISupportInitialize)(this.gridLogs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
