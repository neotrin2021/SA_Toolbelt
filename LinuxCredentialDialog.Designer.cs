namespace SA_ToolBelt
{
    partial class LinuxCredentialDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblHostname = new Label();
            lblPassword = new Label();
            lblUsername = new Label();
            txbHostname = new TextBox();
            txbPassword = new TextBox();
            txbUsername = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblHostname
            // 
            lblHostname.AutoSize = true;
            lblHostname.Location = new Point(84, 33);
            lblHostname.Name = "lblHostname";
            lblHostname.Size = new Size(65, 15);
            lblHostname.TabIndex = 0;
            lblHostname.Text = "Hostname:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(84, 111);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 1;
            lblPassword.Text = "Password:";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(84, 71);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(63, 15);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username:";
            // 
            // txbHostname
            // 
            txbHostname.Location = new Point(147, 30);
            txbHostname.Name = "txbHostname";
            txbHostname.Size = new Size(137, 23);
            txbHostname.TabIndex = 3;
            txbHostname.Text = "ccesa1";
            // 
            // txbPassword
            // 
            txbPassword.AcceptsReturn = true;
            txbPassword.Location = new Point(150, 108);
            txbPassword.Name = "txbPassword";
            txbPassword.PasswordChar = '*';
            txbPassword.Size = new Size(137, 23);
            txbPassword.TabIndex = 4;
            // 
            // txbUsername
            // 
            txbUsername.Location = new Point(147, 68);
            txbUsername.Name = "txbUsername";
            txbUsername.ReadOnly = true;
            txbUsername.Size = new Size(137, 23);
            txbUsername.TabIndex = 5;
            txbUsername.Text = "root";
            // 
            // btnOK
            // 
            btnOK.Location = new Point(84, 156);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 6;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            btnOK.Enter += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(191, 156);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // LinuxCredentialDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(357, 241);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(txbUsername);
            Controls.Add(txbPassword);
            Controls.Add(txbHostname);
            Controls.Add(lblUsername);
            Controls.Add(lblPassword);
            Controls.Add(lblHostname);
            Name = "LinuxCredentialDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SSH Login";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblHostname;
        private Label lblPassword;
        private Label lblUsername;
        private TextBox txbHostname;
        private TextBox txbPassword;
        private TextBox txbUsername;
        private Button btnOK;
        private Button btnCancel;
    }
}