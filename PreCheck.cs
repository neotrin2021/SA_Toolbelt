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
    /// Manages the Mandatory_Settings.csv file in the user's Documents\SA_Toolbelt folder.
    /// </summary>
    public class PreCheck
    {
        private readonly ConsoleForm _consoleForm;
        private readonly Color _errorColor = Color.LightCoral;
        private readonly Color _validColor = Color.White;

        // Settings file location
        private readonly string _settingsFolder;
        private readonly string _settingsFilePath;

        // Current settings values
        public string VCenterServer { get; private set; } = string.Empty;
        public string ComputerListPath { get; private set; } = string.Empty;
        public string OUConfigPath { get; private set; } = string.Empty;
        public string PowerCLIModulePath { get; private set; } = string.Empty;
        public string LogConfigPath { get; private set; } = string.Empty;

        // Full paths with filenames
        public string ComputerListFullPath => string.IsNullOrEmpty(ComputerListPath) ? string.Empty : Path.Combine(ComputerListPath, "ComputerList.csv");
        public string OUConfigFullPath => string.IsNullOrEmpty(OUConfigPath) ? string.Empty : Path.Combine(OUConfigPath, "ouConfiguration.csv");
        public string PowerCLIModuleFullPath => string.IsNullOrEmpty(PowerCLIModulePath) ? string.Empty : Path.Combine(PowerCLIModulePath, "VMware.PowerCLI");
        public string LogConfigFullPath => string.IsNullOrEmpty(LogConfigPath) ? string.Empty : Path.Combine(LogConfigPath, "LogConfiguration.csv");

        // Validation status
        public bool IsVCenterServerValid { get; private set; } = false;
        public bool IsComputerListPathValid { get; private set; } = false;
        public bool IsOUConfigPathValid { get; private set; } = false;
        public bool IsPowerCLIModulePathValid { get; private set; } = false;
        public bool IsLogConfigPathValid { get; private set; } = false;

        public bool AllSettingsValid => IsVCenterServerValid && IsComputerListPathValid &&
                                        IsOUConfigPathValid && IsPowerCLIModulePathValid &&
                                        IsLogConfigPathValid;

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
        /// Initializes the PreCheck system. Creates settings file if it doesn't exist.
        /// Returns true if all settings are valid and the app can proceed normally.
        /// Returns false if settings need to be configured.
        /// </summary>
        public bool Initialize()
        {
            try
            {
                _consoleForm.WriteInfo("Checking mandatory settings...");

                // Ensure the settings folder exists
                if (!Directory.Exists(_settingsFolder))
                {
                    Directory.CreateDirectory(_settingsFolder);
                    _consoleForm.WriteInfo($"Created settings folder: {_settingsFolder}");
                }

                // Check if settings file exists
                if (!File.Exists(_settingsFilePath))
                {
                    _consoleForm.WriteWarning("Mandatory_Settings.csv not found. Creating new file...");
                    CreateEmptySettingsFile();
                    return false; // Settings need to be configured
                }

                // Load and validate settings
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
                    "Computer_List,",
                    "OU_Config,",
                    "PowerCLI_Module,",
                    "Log_Config,"
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
        /// Loads settings from the CSV file.
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
                        case "Computer_List":
                            ComputerListPath = location;
                            break;
                        case "OU_Config":
                            OUConfigPath = location;
                            break;
                        case "PowerCLI_Module":
                            PowerCLIModulePath = location;
                            break;
                        case "Log_Config":
                            LogConfigPath = location;
                            break;
                    }
                }

                _consoleForm.WriteInfo("Settings loaded from file.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to load settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves current settings to the CSV file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var lines = new List<string>
                {
                    "Item,Location",
                    $"VCenter_Server,{VCenterServer}",
                    $"Computer_List,{ComputerListPath}",
                    $"OU_Config,{OUConfigPath}",
                    $"PowerCLI_Module,{PowerCLIModulePath}",
                    $"Log_Config,{LogConfigPath}"
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
            IsComputerListPathValid = ValidatePath(ComputerListPath, "ComputerList.csv");
            IsOUConfigPathValid = ValidatePath(OUConfigPath, "ouConfiguration.csv");
            IsPowerCLIModulePathValid = ValidatePath(PowerCLIModulePath, "VMware.PowerCLI", isDirectory: true);
            IsLogConfigPathValid = ValidatePath(LogConfigPath, "LogConfiguration.csv");
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
        /// Sets the Computer List path.
        /// </summary>
        public void SetComputerListPath(string value)
        {
            ComputerListPath = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Sets the OU Config path.
        /// </summary>
        public void SetOUConfigPath(string value)
        {
            OUConfigPath = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Sets the PowerCLI Module path.
        /// </summary>
        public void SetPowerCLIModulePath(string value)
        {
            PowerCLIModulePath = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Sets the Log Config path.
        /// </summary>
        public void SetLogConfigPath(string value)
        {
            LogConfigPath = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Updates textbox background colors based on validation status.
        /// </summary>
        public void UpdateTextBoxColors(TextBox txbVCenterServer, TextBox txbComputerList,
                                        TextBox txbOUConfigFilePath, TextBox txbPowerCLIModuleLocation,
                                        TextBox txbLogConfigFilePath)
        {
            // Update colors based on empty or invalid status
            txbVCenterServer.BackColor = string.IsNullOrWhiteSpace(txbVCenterServer.Text) || !IsVCenterServerValid
                ? _errorColor : _validColor;

            txbComputerList.BackColor = string.IsNullOrWhiteSpace(txbComputerList.Text) || !IsComputerListPathValid
                ? _errorColor : _validColor;

            txbOUConfigFilePath.BackColor = string.IsNullOrWhiteSpace(txbOUConfigFilePath.Text) || !IsOUConfigPathValid
                ? _errorColor : _validColor;

            txbPowerCLIModuleLocation.BackColor = string.IsNullOrWhiteSpace(txbPowerCLIModuleLocation.Text) || !IsPowerCLIModulePathValid
                ? _errorColor : _validColor;

            txbLogConfigFilePath.BackColor = string.IsNullOrWhiteSpace(txbLogConfigFilePath.Text) || !IsLogConfigPathValid
                ? _errorColor : _validColor;
        }

        /// <summary>
        /// Populates textboxes with current settings values.
        /// </summary>
        public void PopulateTextBoxes(TextBox txbVCenterServer, TextBox txbComputerList,
                                      TextBox txbOUConfigFilePath, TextBox txbPowerCLIModuleLocation,
                                      TextBox txbLogConfigFilePath)
        {
            txbVCenterServer.Text = VCenterServer;
            txbComputerList.Text = ComputerListPath;
            txbOUConfigFilePath.Text = OUConfigPath;
            txbPowerCLIModuleLocation.Text = PowerCLIModulePath;
            txbLogConfigFilePath.Text = LogConfigPath;
        }

        /// <summary>
        /// Reads values from textboxes and updates internal settings.
        /// </summary>
        public void ReadFromTextBoxes(TextBox txbVCenterServer, TextBox txbComputerList,
                                      TextBox txbOUConfigFilePath, TextBox txbPowerCLIModuleLocation,
                                      TextBox txbLogConfigFilePath)
        {
            SetVCenterServer(txbVCenterServer.Text);
            SetComputerListPath(txbComputerList.Text);
            SetOUConfigPath(txbOUConfigFilePath.Text);
            SetPowerCLIModulePath(txbPowerCLIModuleLocation.Text);
            SetLogConfigPath(txbLogConfigFilePath.Text);
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

        /// <summary>
        /// Validates and saves all settings. Returns true if all settings are valid.
        /// </summary>
        public bool ValidateAndSaveAll(TextBox txbVCenterServer, TextBox txbComputerList,
                                       TextBox txbOUConfigFilePath, TextBox txbPowerCLIModuleLocation,
                                       TextBox txbLogConfigFilePath)
        {
            // Read current values from textboxes
            ReadFromTextBoxes(txbVCenterServer, txbComputerList, txbOUConfigFilePath,
                             txbPowerCLIModuleLocation, txbLogConfigFilePath);

            // Validate all settings
            ValidateAllSettings();

            // Update textbox colors
            UpdateTextBoxColors(txbVCenterServer, txbComputerList, txbOUConfigFilePath,
                               txbPowerCLIModuleLocation, txbLogConfigFilePath);

            if (AllSettingsValid)
            {
                // Save settings to file
                SaveSettings();
                MessageBox.Show("All settings validated and saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            else
            {
                // Build error message
                var errors = new List<string>();
                if (!IsVCenterServerValid) errors.Add("- VCenter Server is invalid or unreachable");
                if (!IsComputerListPathValid) errors.Add("- Computer List path is invalid or file not found");
                if (!IsOUConfigPathValid) errors.Add("- OU Config path is invalid or file not found");
                if (!IsPowerCLIModulePathValid) errors.Add("- PowerCLI Module path is invalid or folder not found");
                if (!IsLogConfigPathValid) errors.Add("- Log Config path is invalid or file not found");

                MessageBox.Show($"The following settings need to be corrected:\n\n{string.Join("\n", errors)}",
                    "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}
