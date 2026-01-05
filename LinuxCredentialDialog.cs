using System;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    /// <summary>
    /// Simple dialog for collecting Linux SSH credentials
    /// </summary>
    public class LinuxCredentialDialog : Form
    {
        private Label lblHostname;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txbHostname;
        private TextBox txbUsername;
        private TextBox txbPassword;
        private Button btnOK;
        private Button btnCancel;

        public string Hostname { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public LinuxCredentialDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Linux SSH Credentials";
            this.Width = 400;
            this.Height = 220;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Hostname
            lblHostname = new Label
            {
                Text = "Linux Server Hostname:",
                Left = 20,
                Top = 20,
                Width = 150
            };

            txbHostname = new TextBox
            {
                Left = 180,
                Top = 20,
                Width = 180
            };

            // Username
            lblUsername = new Label
            {
                Text = "SSH Username:",
                Left = 20,
                Top = 60,
                Width = 150
            };

            txbUsername = new TextBox
            {
                Left = 180,
                Top = 60,
                Width = 180
            };

            // Password
            lblPassword = new Label
            {
                Text = "SSH Password:",
                Left = 20,
                Top = 100,
                Width = 150
            };

            txbPassword = new TextBox
            {
                Left = 180,
                Top = 100,
                Width = 180,
                PasswordChar = '*',
                UseSystemPasswordChar = true
            };

            // OK Button
            btnOK = new Button
            {
                Text = "OK",
                Left = 180,
                Top = 140,
                Width = 80,
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Cancel",
                Left = 280,
                Top = 140,
                Width = 80,
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblHostname);
            this.Controls.Add(txbHostname);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txbUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txbPassword);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txbHostname.Text))
            {
                MessageBox.Show("Please enter a hostname.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txbUsername.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txbPassword.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hostname = txbHostname.Text.Trim();
            Username = txbUsername.Text.Trim();
            Password = txbPassword.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Show the credential dialog and return the credentials
        /// </summary>
        public static (bool success, string hostname, string username, string password) GetCredentials()
        {
            using (var dialog = new LinuxCredentialDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return (true, dialog.Hostname, dialog.Username, dialog.Password);
                }
                return (false, null, null, null);
            }
        }
    }
}
