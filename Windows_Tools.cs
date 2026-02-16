using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;

namespace SA_ToolBelt
{
    public class Windows_Tools
    {
        private readonly ConsoleForm _consoleForm;

        public Windows_Tools(ConsoleForm consoleForm = null)
        {
            _consoleForm = consoleForm;
        }

        #region Data Classes

        /// <summary>
        /// Holds a single HP BIOS setting name/value pair
        /// </summary>
        public class BiosSetting
        {
            public string Name { get; set; }
            public string CurrentValue { get; set; }
            public string Category { get; set; }

            public BiosSetting() { }
        }

        /// <summary>
        /// Aggregate result for a full BIOS query against a remote machine
        /// </summary>
        public class BiosQueryResult
        {
            public string ComputerName { get; set; }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }

            // Standard WMI BIOS info (works on any manufacturer)
            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string SerialNumber { get; set; }
            public string BiosVersion { get; set; }
            public string BiosDate { get; set; }
            public string OSName { get; set; }
            public string OSVersion { get; set; }
            public string OSArchitecture { get; set; }

            // TPM info
            public string TpmPresent { get; set; }
            public string TpmVersion { get; set; }
            public string TpmEnabled { get; set; }
            public string TpmActivated { get; set; }

            // Secure Boot
            public string SecureBootEnabled { get; set; }

            // HP-specific BIOS settings (only populated on HP machines)
            public List<BiosSetting> HpBiosSettings { get; set; } = new List<BiosSetting>();
            public bool IsHpMachine { get; set; }

            public BiosQueryResult() { }
        }

        #endregion

        #region Remote BIOS Query

        /// <summary>
        /// Query a remote computer for BIOS, hardware, TPM, and HP-specific settings
        /// </summary>
        public async Task<BiosQueryResult> QueryRemoteBiosAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new BiosQueryResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Connecting to {computerName} via WMI...");

                    // Build connection options
                    var connOptions = new ConnectionOptions
                    {
                        Username = $"{domain}\\{username}",
                        Password = password,
                        Impersonation = ImpersonationLevel.Impersonate,
                        EnablePrivileges = true,
                        Timeout = TimeSpan.FromSeconds(30)
                    };

                    // --- Standard Hardware Info (Win32_ComputerSystem) ---
                    QueryHardwareInfo(computerName, connOptions, result);

                    // --- BIOS Info (Win32_BIOS) ---
                    QueryBiosInfo(computerName, connOptions, result);

                    // --- OS Info (Win32_OperatingSystem) ---
                    QueryOsInfo(computerName, connOptions, result);

                    // --- TPM Info (Win32_Tpm) ---
                    QueryTpmInfo(computerName, connOptions, result);

                    // --- Secure Boot ---
                    QuerySecureBoot(computerName, connOptions, result);

                    // --- HP-specific BIOS settings ---
                    if (result.Manufacturer != null &&
                        result.Manufacturer.IndexOf("HP", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        result.Manufacturer != null &&
                        result.Manufacturer.IndexOf("Hewlett", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result.IsHpMachine = true;
                        QueryHpBiosSettings(computerName, connOptions, result);
                    }

                    result.Success = true;
                    _consoleForm?.WriteSuccess($"Successfully queried BIOS info from {computerName}");
                }
                catch (UnauthorizedAccessException)
                {
                    result.Success = false;
                    result.ErrorMessage = "Access denied. Verify your credentials have admin rights on the remote machine.";
                    _consoleForm?.WriteError($"Access denied on {computerName}");
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"WMI connection failed: {ex.Message}";
                    _consoleForm?.WriteError($"WMI COM error on {computerName}: {ex.Message}");
                }
                catch (ManagementException ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"WMI query error: {ex.Message}";
                    _consoleForm?.WriteError($"WMI query error on {computerName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Unexpected error: {ex.Message}";
                    _consoleForm?.WriteError($"Error querying {computerName}: {ex.Message}");
                }

                return result;
            });
        }

        #endregion

        #region Individual WMI Queries

        private void QueryHardwareInfo(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
            scope.Connect();

            using (var searcher = new ManagementObjectSearcher(scope,
                new ObjectQuery("SELECT Manufacturer, Model FROM Win32_ComputerSystem")))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    result.Manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "Unknown";
                    result.Model = obj["Model"]?.ToString()?.Trim() ?? "Unknown";
                }
            }

            _consoleForm?.WriteInfo($"  Hardware: {result.Manufacturer} {result.Model}");
        }

        private void QueryBiosInfo(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
            scope.Connect();

            using (var searcher = new ManagementObjectSearcher(scope,
                new ObjectQuery("SELECT SerialNumber, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS")))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    result.SerialNumber = obj["SerialNumber"]?.ToString()?.Trim() ?? "Unknown";
                    result.BiosVersion = obj["SMBIOSBIOSVersion"]?.ToString()?.Trim() ?? "Unknown";

                    string rawDate = obj["ReleaseDate"]?.ToString();
                    if (!string.IsNullOrEmpty(rawDate) && rawDate.Length >= 8)
                    {
                        try
                        {
                            result.BiosDate = ManagementDateTimeConverter.ToDateTime(rawDate).ToString("yyyy-MM-dd");
                        }
                        catch
                        {
                            result.BiosDate = rawDate;
                        }
                    }
                    else
                    {
                        result.BiosDate = "Unknown";
                    }
                }
            }

            _consoleForm?.WriteInfo($"  BIOS Version: {result.BiosVersion} ({result.BiosDate})");
        }

        private void QueryOsInfo(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
            scope.Connect();

            using (var searcher = new ManagementObjectSearcher(scope,
                new ObjectQuery("SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem")))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    result.OSName = obj["Caption"]?.ToString()?.Trim() ?? "Unknown";
                    result.OSVersion = obj["Version"]?.ToString()?.Trim() ?? "Unknown";
                    result.OSArchitecture = obj["OSArchitecture"]?.ToString()?.Trim() ?? "Unknown";
                }
            }
        }

        private void QueryTpmInfo(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            try
            {
                var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2\\Security\\MicrosoftTpm", connOptions);
                scope.Connect();

                using (var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT * FROM Win32_Tpm")))
                {
                    var collection = searcher.Get();
                    if (collection.Count > 0)
                    {
                        result.TpmPresent = "Yes";
                        foreach (ManagementObject obj in collection)
                        {
                            result.TpmVersion = obj["SpecVersion"]?.ToString()?.Trim() ?? "Unknown";
                            result.TpmEnabled = obj["IsEnabled_InitialValue"]?.ToString() ?? "Unknown";
                            result.TpmActivated = obj["IsActivated_InitialValue"]?.ToString() ?? "Unknown";
                        }
                    }
                    else
                    {
                        result.TpmPresent = "No";
                        result.TpmVersion = "N/A";
                        result.TpmEnabled = "N/A";
                        result.TpmActivated = "N/A";
                    }
                }
            }
            catch
            {
                // TPM WMI namespace may not exist on all machines
                result.TpmPresent = "Unknown";
                result.TpmVersion = "Unknown";
                result.TpmEnabled = "Unknown";
                result.TpmActivated = "Unknown";
            }

            _consoleForm?.WriteInfo($"  TPM: {result.TpmPresent} (Version: {result.TpmVersion})");
        }

        private void QuerySecureBoot(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            try
            {
                // Secure Boot is exposed via MSFT_SecureBootUEFI in root\Microsoft\Windows\SecureBoot\UEFI
                // but requires specific permissions. Fallback: check via registry or HP BIOS settings.
                var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
                scope.Connect();

                // Use the registry provider as a fallback approach
                using (var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT * FROM Win32_OptionalFeature WHERE Name='SecureBoot'")))
                {
                    // This query may not return results on all systems
                    var collection = searcher.Get();
                    bool found = false;
                    foreach (ManagementObject obj in collection)
                    {
                        result.SecureBootEnabled = obj["InstallState"]?.ToString() == "1" ? "Enabled" : "Disabled";
                        found = true;
                    }
                    if (!found)
                    {
                        result.SecureBootEnabled = "Check HP BIOS Settings";
                    }
                }
            }
            catch
            {
                result.SecureBootEnabled = "Unable to determine";
            }
        }

        private void QueryHpBiosSettings(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            try
            {
                var scope = new ManagementScope($"\\\\{computerName}\\root\\HP\\InstrumentedBIOS", connOptions);
                scope.Connect();

                using (var searcher = new ManagementObjectSearcher(scope,
                    new ObjectQuery("SELECT Name, CurrentValue FROM HP_BIOSSetting")))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString()?.Trim();
                        string value = obj["CurrentValue"]?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(name))
                        {
                            // Categorize the setting
                            string category = CategorizeBiosSetting(name);

                            result.HpBiosSettings.Add(new BiosSetting
                            {
                                Name = name,
                                CurrentValue = string.IsNullOrEmpty(value) ? "(not set)" : value,
                                Category = category
                            });
                        }
                    }
                }

                _consoleForm?.WriteInfo($"  Retrieved {result.HpBiosSettings.Count} HP BIOS settings");
            }
            catch (ManagementException ex)
            {
                _consoleForm?.WriteWarning($"  HP BIOS namespace not available on {computerName}: {ex.Message}");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Categorize HP BIOS settings into logical groups for display
        /// </summary>
        private string CategorizeBiosSetting(string settingName)
        {
            string lower = settingName.ToLowerInvariant();

            if (lower.Contains("boot") || lower.Contains("uefi") || lower.Contains("legacy"))
                return "Boot Configuration";
            if (lower.Contains("secure") || lower.Contains("tpm") || lower.Contains("password") ||
                lower.Contains("drivelock") || lower.Contains("encryption"))
                return "Security";
            if (lower.Contains("virtualization") || lower.Contains("vtx") || lower.Contains("vtd") ||
                lower.Contains("iommu") || lower.Contains("hyper"))
                return "Virtualization";
            if (lower.Contains("power") || lower.Contains("wake") || lower.Contains("battery") ||
                lower.Contains("energy") || lower.Contains("thermal"))
                return "Power Management";
            if (lower.Contains("usb") || lower.Contains("port") || lower.Contains("serial") ||
                lower.Contains("parallel") || lower.Contains("bluetooth") || lower.Contains("wifi") ||
                lower.Contains("lan") || lower.Contains("network") || lower.Contains("nfc"))
                return "Ports & Devices";
            if (lower.Contains("display") || lower.Contains("video") || lower.Contains("gpu") ||
                lower.Contains("graphics") || lower.Contains("audio") || lower.Contains("speaker"))
                return "Display & Audio";
            if (lower.Contains("memory") || lower.Contains("cpu") || lower.Contains("processor") ||
                lower.Contains("core") || lower.Contains("cache"))
                return "Performance";

            return "General";
        }

        /// <summary>
        /// Quick connectivity test via standard WMI before doing full query
        /// </summary>
        public async Task<bool> TestWmiConnectivity(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var connOptions = new ConnectionOptions
                    {
                        Username = $"{domain}\\{username}",
                        Password = password,
                        Impersonation = ImpersonationLevel.Impersonate,
                        Timeout = TimeSpan.FromSeconds(10)
                    };

                    var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
                    scope.Connect();
                    return scope.IsConnected;
                }
                catch
                {
                    return false;
                }
            });
        }

        #endregion
    }
}
