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
            return rtbConsoleFormMsgs;
        }

        /// <summary>
        /// Remove console control from this form (for moving to main form)
        /// </summary>
        public void RemoveConsoleControl()
        {
            if (rtbConsoleFormMsgs != null && this.Controls.Contains(rtbConsoleFormMsgs))
            {
                this.Controls.Remove(rtbConsoleFormMsgs);
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
            if (rtbConsoleFormMsgs.InvokeRequired)
            {
                rtbConsoleFormMsgs.Invoke(new Action(() => WriteToConsole(text, color)));
                return;
            }

            rtbConsoleFormMsgs.SelectionStart = rtbConsoleFormMsgs.TextLength;
            rtbConsoleFormMsgs.SelectionLength = 0;
            rtbConsoleFormMsgs.SelectionColor = color;
            rtbConsoleFormMsgs.AppendText($"{DateTime.Now:HH:mm:ss}: {text}{Environment.NewLine}");
            rtbConsoleFormMsgs.ScrollToCaret();
        }

        public void ClearConsole()
        {
            if (rtbConsoleFormMsgs.InvokeRequired)
            {
                rtbConsoleFormMsgs.Invoke(new Action(() => ClearConsole()));
                return;
            }

            rtbConsoleFormMsgs.Clear();
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
