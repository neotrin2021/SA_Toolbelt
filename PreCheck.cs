using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    /// <summary>
    /// Handles mandatory settings validation for the SA Toolbelt.
    /// Checks the Windows Registry for the SQLite database path.
    /// No CSV fallback - the database is the only configuration source.
    /// </summary>
    public class PreCheck
    {
        private readonly ConsoleForm _consoleForm;

        /// <summary>
        /// Result of the PreCheck initialization.
        /// </summary>
        public enum InitResult
        {
            /// <summary>Registry key found, DB exists, config loaded - show all tabs.</summary>
            DatabaseFound,
            /// <summary>Registry key found, but DB file is missing at that path.</summary>
            RegistryExistsButDbMissing,
            /// <summary>No registry key at all - first-time setup.</summary>
            NoRegistryKey
        }

        /// <summary>
        /// The path from the registry (if it existed).
        /// </summary>
        public string RegistryPath { get; private set; } = string.Empty;

        public PreCheck(ConsoleForm consoleForm)
        {
            _consoleForm = consoleForm;
        }

        /// <summary>
        /// Checks the registry for the database path and returns the appropriate result.
        /// </summary>
        public InitResult Initialize()
        {
            try
            {
                _consoleForm.WriteInfo("Checking registry for database path...");

                string registryPath = DatabaseService.GetSqlPathFromRegistry();

                if (string.IsNullOrEmpty(registryPath))
                {
                    _consoleForm.WriteWarning("No registry key found. First-time setup required.");
                    return InitResult.NoRegistryKey;
                }

                RegistryPath = registryPath;
                string dbPath = Path.Combine(registryPath, "SA_Toolbelt.db");

                if (!File.Exists(dbPath))
                {
                    _consoleForm.WriteWarning($"Registry points to '{registryPath}' but SA_Toolbelt.db not found.");
                    return InitResult.RegistryExistsButDbMissing;
                }

                // DB file exists - verify if it has valid config and log accordingly
                var dbService = new DatabaseService(_consoleForm);
                var config = dbService.LoadToolbeltConfig();

                if (config != null && !string.IsNullOrEmpty(config.VCenterServer))
                {
                    _consoleForm.WriteSuccess($"Database found and loaded from: {dbPath}");
                }
                else
                {
                    _consoleForm.WriteWarning("Database exists but contains no valid configuration.");
                }

                return InitResult.DatabaseFound;
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during PreCheck: {ex.Message}");
                return InitResult.NoRegistryKey;
            }
        }

        /// <summary>
        /// Checks if a database file exists at the given folder path.
        /// </summary>
        public bool DatabaseExistsAtPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return false;
            string dbPath = Path.Combine(folderPath, "SA_Toolbelt.db");
            return File.Exists(dbPath);
        }

        /// <summary>
        /// Validates that the vCenter server is reachable via ping.
        /// </summary>
        public bool ValidateVCenterServer(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                _consoleForm.WriteWarning("VCenter server is empty.");
                return false;
            }

            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(server, 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        _consoleForm.WriteSuccess($"VCenter server '{server}' is reachable.");
                        return true;
                    }
                    else
                    {
                        _consoleForm.WriteError($"VCenter server '{server}' is not reachable: {reply.Status}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to ping VCenter server '{server}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Performs a ping test on the vCenter server and shows a message box with the result.
        /// </summary>
        public void VerifyVCenterServerWithFeedback(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                MessageBox.Show("Please enter a vCenter server address.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isValid = ValidateVCenterServer(server);

            if (isValid)
            {
                MessageBox.Show($"Successfully connected to vCenter server: {server}", "Verification Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Unable to reach vCenter server: {server}\n\nPlease verify the server address and try again.",
                    "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a folder browser dialog and returns the selected path.
        /// </summary>
        public string BrowseForFolder(string description)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }
            return null;
        }
    }
}
