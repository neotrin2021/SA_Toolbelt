namespace SA_ToolBelt
{
    partial class ConsoleForm
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
            btnDockConsole = new Button();
            rtbConsoleBox = new RichTextBox();
            SuspendLayout();
            // 
            // btnDockConsole
            // 
            btnDockConsole.Dock = DockStyle.Bottom;
            btnDockConsole.Location = new Point(0, 390);
            btnDockConsole.Name = "btnDockConsole";
            btnDockConsole.Size = new Size(846, 28);
            btnDockConsole.TabIndex = 1;
            btnDockConsole.Text = "Dock Console";
            btnDockConsole.UseVisualStyleBackColor = true;
            btnDockConsole.Click += btnDockConsole_Click;
            // 
            // rtbConsoleBox
            // 
            rtbConsoleBox.BackColor = SystemColors.WindowText;
            rtbConsoleBox.Dock = DockStyle.Top;
            rtbConsoleBox.ForeColor = SystemColors.Window;
            rtbConsoleBox.Location = new Point(0, 0);
            rtbConsoleBox.Name = "rtbConsoleBox";
            rtbConsoleBox.Size = new Size(846, 360);
            rtbConsoleBox.TabIndex = 0;
            rtbConsoleBox.Text = "";
            // 
            // ConsoleForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(846, 418);
            Controls.Add(btnDockConsole);
            Controls.Add(rtbConsoleBox);
            Name = "ConsoleForm";
            Text = "SA_Toolbelt Console";
            ResumeLayout(false);
        }

        #endregion
        private Button btnDockConsole;
        private RichTextBox rtbConsoleBox;
    }
}