// Assuming the context of DockConsole() function

// Add consoleRichTextBox to tabConsole
 tabConsole.Controls.Add(consoleRichTextBox);

// Set Dock and ContextMenuStrip properties
consoleRichTextBox.Dock = DockStyle.Fill;
consoleRichTextBox.ContextMenuStrip = _consoleContextMenu;