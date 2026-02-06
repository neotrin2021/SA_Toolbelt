using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    /// <summary>
    /// Handles mandatory settings validation and management for the SA Toolbelt.
    /// Now checks SQLite database first, with CSV file as legacy fallback.
    /// </summary>
    public class PreCheck
    {
        private readonly ConsoleForm _consoleForm;
        private readonly Color _errorColor = Color.LightCoral;
        private readonly Color _validColor = Color.White;

        // Settings file location (legacy CSV)
        private readonly string _settingsFolder;
        private readonly string _settingsFilePath;

        // Current settings values
        public string VCenterServer { get; private set; } = string.Empty;
        public string BasePath { get; private set; } = string.Empty;
        public string PowerCLIModulePath { get; private set; } = string.Empty;

        // Whether config was loaded from database
        public bool LoadedFromDatabase { get; private set; } = false;

        // Full paths with filenames (all CSV files are in BasePath) - legacy support
        public string ComputerListFullPath => string.IsNullOrEmpty(BasePath) ? string.Empty : Path.Combine(BasePath, "ComputerList.csv");
        public string OUConfigFullPath => string.IsNullOrEmpty(BasePath) ? string.Empty : Path.Combine(BasePath, "ouConfiguration.csv");
        public string LogConfigFullPath => string.IsNullOrEmpty(BasePath) ? string.Empty : Path.Combine(BasePath, "LogConfiguration.csv");
        public string PowerCLIModuleFullPath => string.IsNullOrEmpty(PowerCLIModulePath) ? string.Empty : Path.Combine(PowerCLIModulePath, "VMware.PowerCLI");

        // Validation status
        public bool IsVCenterServerValid { get; private set; } = false;
        public bool IsBasePathValid { get; private set; } = false;
        public bool IsPowerCLIModulePathValid { get; private set; } = false;

        public bool AllSettingsValid => IsVCenterServerValid && IsPowerCLIModulePathValid && (LoadedFromDatabase || IsBasePathValid);

        public bool SettingsFileExists => File.Exists(_settingsFilePath);

        public PreCheck(ConsoleForm consoleForm)
        {
            _consoleForm = consoleForm;

            // Set up the settings folder in user's Documents\SA_Toolbelt
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _settingsFolder = Path.Combine(documentsPath, "SA_Toolbelt");
            _settingsFilePath = Path.Combine(_settingsFolder, "Mandatory_Settings.csv");
        }

        /// <summary>
        /// Initializes the PreCheck system.
        /// First checks for a SQLite database (via registry or Documents).
        /// Falls back to CSV settings file if no database is found.
        /// Returns true if all settings are valid and the app can proceed normally.
        /// Returns false if settings need to be configured.
        /// </summary>
        public bool Initialize()
        {
            try
            {
                _consoleForm.WriteInfo("Checking mandatory settings...");

                // Check for SQLite database first
                if (TryLoadFromDatabase())
                {
                    _consoleForm.WriteSuccess("Configuration loaded from SQLite database.");
                    return true;
                }

                _consoleForm.WriteInfo("No valid database found. Checking legacy CSV settings...");

                // Ensure the settings folder exists
                if (!Directory.Exists(_settingsFolder))
                {
                    Directory.CreateDirectory(_settingsFolder);
                    _consoleForm.WriteInfo($"Created settings folder: {_settingsFolder}");
                }

                // Check if settings file exists
                if (!File.Exists(_settingsFilePath))
                {
                    _consoleForm.WriteWarning("No configuration found. First-time setup required.");
                    CreateEmptySettingsFile();
                    return false; // Settings need to be configured
                }

                // Load and validate settings from CSV
                LoadSettings();
                ValidateAllSettings();

                if (AllSettingsValid)
                {
                    _consoleForm.WriteSuccess("All mandatory settings validated successfully!");
                    return true;
                }
                else
                {
                    _consoleForm.WriteWarning("Some mandatory settings are missing or invalid. Configuration required.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error initializing PreCheck: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to load configuration from the SQLite database.
        /// Checks the registry for the database path, then falls back to Documents.
        /// </summary>
        private bool TryLoadFromDatabase()
        {
            try
            {
                // Check registry for database path
                string registryPath = DatabaseService.GetSqlPathFromRegistry();
                if (!string.IsNullOrEmpty(registryPath))
                {
                    string dbPath = Path.Combine(registryPath, "SA_Toolbelt.db");
                    if (File.Exists(dbPath))
                    {
                        _consoleForm.WriteInfo($"Found database at registry path: {dbPath}");
                        var dbService = new DatabaseService(_consoleForm);
                        var config = dbService.LoadToolbeltConfig();
                        if (config != null && !string.IsNullOrEmpty(config.VCenterServer))
                        {
                            VCenterServer = config.VCenterServer;
                            PowerCLIModulePath = config.PowerCLILocation;
                            BasePath = config.SqlPath;
                            IsVCenterServerValid = true; // Trust the database - it was validated when saved
                            IsPowerCLIModulePathValid = !string.IsNullOrEmpty(config.PowerCLILocation);
                            LoadedFromDatabase = true;
                            return true;
                        }
                    }
                }

                // Check Documents folder
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string defaultDbPath = Path.Combine(documentsPath, "SA_Toolbelt", "SA_Toolbelt.db");
                if (File.Exists(defaultDbPath))
                {
                    _consoleForm.WriteInfo($"Found database in Documents: {defaultDbPath}");
                    var dbService = new DatabaseService(_consoleForm);
                    var config = dbService.LoadToolbeltConfig();
                    if (config != null && !string.IsNullOrEmpty(config.VCenterServer))
                    {
                        VCenterServer = config.VCenterServer;
                        PowerCLIModulePath = config.PowerCLILocation;
                        BasePath = config.SqlPath;
                        IsVCenterServerValid = true;
                        IsPowerCLIModulePathValid = !string.IsNullOrEmpty(config.PowerCLILocation);
                        LoadedFromDatabase = true;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteWarning($"Error checking database: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Creates an empty settings file with headers and empty values.
        /// </summary>
        private void CreateEmptySettingsFile()
        {
            try
            {
                var lines = new List<string>
                {
                    "Item,Location",
                    "VCenter_Server,",
                    "Base_Path,",
                    "PowerCLI_Module,"
                };

                File.WriteAllLines(_settingsFilePath, lines);
                _consoleForm.WriteInfo($"Created empty settings file: {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to create settings file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads settings from the CSV file (legacy support).
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    _consoleForm.WriteWarning("Settings file not found.");
                    return;
                }

                var lines = File.ReadAllLines(_settingsFilePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Item,"))
                        continue;

                    var parts = line.Split(new[] { ',' }, 2);
                    if (parts.Length < 2)
                        continue;

                    string item = parts[0].Trim();
                    string location = parts[1].Trim();

                    switch (item)
                    {
                        case "VCenter_Server":
                            VCenterServer = location;
                            break;
                        case "Base_Path":
                            BasePath = location;
                            break;
                        case "PowerCLI_Module":
                            PowerCLIModulePath = location;
                            break;
                    }
                }

                _consoleForm.WriteInfo("Settings loaded from CSV file.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to load settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves current settings to the CSV file (legacy support).
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var lines = new List<string>
                {
                    "Item,Location",
                    $"VCenter_Server,{VCenterServer}",
                    $"Base_Path,{BasePath}",
                    $"PowerCLI_Module,{PowerCLIModulePath}"
                };

                File.WriteAllLines(_settingsFilePath, lines);
                _consoleForm.WriteSuccess($"Settings saved to: {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to save settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validates all settings and updates validation status properties.
        /// </summary>
        public void ValidateAllSettings()
        {
            IsVCenterServerValid = ValidateVCenterServer(VCenterServer);
            IsBasePathValid = ValidateBasePath(BasePath);
            IsPowerCLIModulePathValid = ValidatePath(PowerCLIModulePath, "VMware.PowerCLI", isDirectory: true);
        }

        /// <summary>
        /// Validates the base path by checking all three required CSV files exist.
        /// </summary>
        private bool ValidateBasePath(string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                _consoleForm.WriteWarning("Base path is empty.");
                return false;
            }

            if (!Directory.Exists(basePath))
            {
                _consoleForm.WriteWarning($"Base path directory not found: {basePath}");
                return false;
            }

            // Check all three required CSV files
            string[] requiredFiles = { "ComputerList.csv", "ouConfiguration.csv", "LogConfiguration.csv" };
            var missingFiles = new List<string>();

            foreach (var file in requiredFiles)
            {
                string fullPath = Path.Combine(basePath, file);
                if (!File.Exists(fullPath))
                {
                    missingFiles.Add(file);
                }
            }

            if (missingFiles.Count > 0)
            {
                _consoleForm.WriteWarning($"Missing files in base path: {string.Join(", ", missingFiles)}");
                return false;
            }

            _consoleForm.WriteSuccess($"Base path validated: {basePath} (all CSV files found)");
            return true;
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
                    var reply = ping.Send(server, 3000); // 3 second timeout
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
        /// Validates that a path exists and optionally that a file/folder exists within it.
        /// </summary>
        private bool ValidatePath(string basePath, string itemName, bool isDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return false;
            }

            try
            {
                string fullPath = Path.Combine(basePath, itemName);

                if (isDirectory)
                {
                    if (Directory.Exists(fullPath))
                    {
                        _consoleForm.WriteSuccess($"Path validated: {fullPath}");
                        return true;
                    }
                    else
                    {
                        _consoleForm.WriteWarning($"Directory not found: {fullPath}");
                        return false;
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        _consoleForm.WriteSuccess($"File validated: {fullPath}");
                        return true;
                    }
                    else
                    {
                        _consoleForm.WriteWarning($"File not found: {fullPath}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error validating path: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets the vCenter server value.
        /// </summary>
        public void SetVCenterServer(string value)
        {
            VCenterServer = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Sets the Base path.
        /// </summary>
        public void SetBasePath(string value)
        {
            BasePath = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Sets the PowerCLI Module path.
        /// </summary>
        public void SetPowerCLIModulePath(string value)
        {
            PowerCLIModulePath = value?.Trim() ?? string.Empty;
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
            IsVCenterServerValid = isValid;

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
    }
}
