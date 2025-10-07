namespace SA_ToolBelt
{
    partial class ConsoleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        public System.ComponentModel.IContainer components = null;

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
        public void InitializeComponent()
        {
            rtbConsoleFormMsgs = new RichTextBox();
            btnDockConsole = new Button();
            SuspendLayout();
            // 
            // rtbConsoleFormMsgs
            // 
            rtbConsoleFormMsgs.BackColor = SystemColors.WindowText;
            rtbConsoleFormMsgs.Dock = DockStyle.Top;
            rtbConsoleFormMsgs.ForeColor = SystemColors.Window;
            rtbConsoleFormMsgs.Location = new Point(0, 0);
            rtbConsoleFormMsgs.Name = "rtbConsoleFormMsgs";
            rtbConsoleFormMsgs.Size = new Size(800, 360);
            rtbConsoleFormMsgs.TabIndex = 0;
            rtbConsoleFormMsgs.Text = "";
            // 
            // btnDockConsole
            // 
            btnDockConsole.Dock = DockStyle.Bottom;
            btnDockConsole.Location = new Point(333, 366);
            btnDockConsole.Name = "btnDockConsole";
            btnDockConsole.Size = new Size(96, 28);
            btnDockConsole.TabIndex = 1;
            btnDockConsole.Text = "Dock Console";
            btnDockConsole.UseVisualStyleBackColor = true;
            btnDockConsole.Click += btnDockConsole_Click;
            // 
            // ConsoleForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 406);
            Controls.Add(btnDockConsole);
            Controls.Add(rtbConsoleFormMsgs);
            Name = "ConsoleForm";
            Text = "Form2";
            ResumeLayout(false);
        }

        #endregion

        public RichTextBox rtbConsoleFormMsgs;
        public Button btnDockConsole;
    }
}