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
    public partial class ConsoleForm : Form
    {
        // Add this event for communication back to main form
        public event Action DockButtonClicked;

        public ConsoleForm()
        {
            InitializeComponent();

            // Prevent the form from closing when X is clicked
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }

        /// <summary>
        /// Get the console RichTextBox for sharing between forms
        /// </summary>
        public RichTextBox GetConsoleRichTextBox()
        {
            return rtbConsoleBox;
        }

        /// <summary>
        /// Remove console control from this form (for moving to main form)
        /// </summary>
        public void RemoveConsoleControl()
        {
            if (rtbConsoleBox != null && this.Controls.Contains(rtbConsoleBox))
            {
                this.Controls.Remove(rtbConsoleBox);
            }
        }

        /// <summary>
        /// Add console control back to this form (for moving from main form)
        /// </summary>
        public void AddConsoleControl(RichTextBox richTextBox)
        {
            // Clear existing controls except the dock button
            var controlsToRemove = this.Controls.OfType<Control>().Where(c => c != btnDockConsole).ToList();
            foreach (var control in controlsToRemove)
            {
                this.Controls.Remove(control);
            }

            // Add the RichTextBox
            this.Controls.Add(richTextBox);
            richTextBox.Dock = DockStyle.Top;
            richTextBox.Height = this.Height - 50; // Leave room for dock button

            // Bring dock button to front
            btnDockConsole.BringToFront();
        }

        public void WriteToConsole(string text, Color color)
        {
            if (rtbConsoleBox.InvokeRequired)
            {
                rtbConsoleBox.Invoke(new Action(() => WriteToConsole(text, color)));
                return;
            }

            rtbConsoleBox.SelectionStart = rtbConsoleBox.TextLength;
            rtbConsoleBox.SelectionLength = 0;
            rtbConsoleBox.SelectionColor = color;
            rtbConsoleBox.AppendText($"{DateTime.Now:HH:mm:ss}: {text}{Environment.NewLine}");
            rtbConsoleBox.ScrollToCaret();
            rtbConsoleBox.Refresh();
        }

        public void ClearConsole()
        {
            if (rtbConsoleBox.InvokeRequired)
            {
                rtbConsoleBox.Invoke(new Action(() => ClearConsole()));
                return;
            }

            rtbConsoleBox.Clear();
        }
        public void WriteError(string text) => WriteToConsole(text, Color.Red);
        public void WriteSuccess(string text) => WriteToConsole(text, Color.Green);
        public void WriteWarning(string text) => WriteToConsole(text, Color.Yellow);
        public void WriteInfo(string text) => WriteToConsole(text, Color.White);

        /// <summary>
        /// Handle dock button click in floating window
        /// </summary>
        private void btnDockConsole_Click(object sender, EventArgs e)
        {
            DockButtonClicked?.Invoke();
        }

    }
}
