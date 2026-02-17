using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    public partial class LinuxCredentialDialog : Form
    {
        public string Hostname { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public LinuxCredentialDialog(string defaultHostname = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(defaultHostname))
            {
                txbHostname.Text = defaultHostname;
                this.Shown += (s, e) => txbUsername.Focus();
            }
            else
            {
                this.Shown += (s, e) => txbHostname.Focus();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
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
                MessageBox.Show("Please enter a Password.", "Validation Error",
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
        /// 
        public static (bool success, string hostname, string username, string password) GetCredentials(string defaultHostname = null)
        {
            using (var dialog = new LinuxCredentialDialog(defaultHostname))
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
