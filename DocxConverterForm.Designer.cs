namespace SA_ToolBelt
{
    partial class DocxConverterForm
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
            btnSelectFile = new Button();
            btnConvert = new Button();
            lblSelectedFile = new Label();
            lblStatus = new Label();
            lblStatusLabel = new Label();
            SuspendLayout();

            // btnSelectFile
            btnSelectFile.Location = new System.Drawing.Point(20, 20);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new System.Drawing.Size(100, 30);
            btnSelectFile.TabIndex = 0;
            btnSelectFile.Text = "Select File";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;

            // lblSelectedFile
            lblSelectedFile.AutoSize = true;
            lblSelectedFile.Location = new System.Drawing.Point(130, 27);
            lblSelectedFile.Name = "lblSelectedFile";
            lblSelectedFile.Size = new System.Drawing.Size(100, 15);
            lblSelectedFile.TabIndex = 1;
            lblSelectedFile.Text = "No file selected";

            // btnConvert
            btnConvert.Location = new System.Drawing.Point(20, 65);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new System.Drawing.Size(100, 30);
            btnConvert.TabIndex = 2;
            btnConvert.Text = "Convert";
            btnConvert.UseVisualStyleBackColor = true;
            btnConvert.Click += btnConvert_Click;

            // lblStatusLabel
            lblStatusLabel.AutoSize = true;
            lblStatusLabel.Location = new System.Drawing.Point(130, 72);
            lblStatusLabel.Name = "lblStatusLabel";
            lblStatusLabel.Size = new System.Drawing.Size(45, 15);
            lblStatusLabel.TabIndex = 3;
            lblStatusLabel.Text = "Status:";
            lblStatusLabel.Font = new System.Drawing.Font(lblStatusLabel.Font, System.Drawing.FontStyle.Bold);

            // lblStatus
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(180, 72);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(30, 15);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Idle";

            // DocxConverterForm
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(420, 115);
            Controls.Add(btnSelectFile);
            Controls.Add(lblSelectedFile);
            Controls.Add(btnConvert);
            Controls.Add(lblStatusLabel);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "DocxConverterForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "DOCX to Markdown Converter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectFile;
        private Button btnConvert;
        private Label lblSelectedFile;
        private Label lblStatus;
        private Label lblStatusLabel;
    }
}
