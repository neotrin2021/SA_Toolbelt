using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Threading.Tasks;

namespace SA_ToolBelt
{
    public class BIOS_Tools
    {
        private readonly ConsoleForm _consoleForm;

        // Caches Secure Boot result from elevated TPM query so we don't trigger a second UAC prompt
        private string _tpmFallbackSecureBoot;

        public BIOS_Tools(ConsoleForm consoleForm = null)
        {
            _consoleForm = consoleForm;
        }

        #region WMI Connection Helpers

        /// <summary>
        /// Determines if the target computer name refers to the local machine.
        /// </summary>
        private bool IsLocalComputer(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                return true;

            string name = computerName.Trim();
            return string.Equals(name, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, ".", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Builds WMI ConnectionOptions. For local connections credentials are omitted
        /// because WMI does not allow explicit user credentials for local connections.
        /// </summary>
        private ConnectionOptions BuildConnectionOptions(string computerName, string username, string password, string domain, int timeoutSeconds = 30)
        {
            if (IsLocalComputer(computerName))
            {
                return new ConnectionOptions
                {
                    Impersonation = ImpersonationLevel.Impersonate,
                    EnablePrivileges = true,
                    Timeout = TimeSpan.FromSeconds(timeoutSeconds)
                };
            }

            return new ConnectionOptions
            {
                Username = $"{domain}\\{username}",
                Password = password,
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true,
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
        }

        #endregion

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

                    // Build connection options (omits credentials for local machine)
                    var connOptions = BuildConnectionOptions(computerName, username, password, domain);

                    // --- Standard Hardware Info (Win32_ComputerSystem) ---
                    QueryHardwareInfo(computerName, connOptions, result);

                    // --- BIOS Info (Win32_BIOS) ---
                    QueryBiosInfo(computerName, connOptions, result);

                    // --- OS Info (Win32_OperatingSystem) ---
                    QueryOsInfo(computerName, connOptions, result);

                    // --- TPM Info (Win32_Tpm) ---
                    QueryTpmInfo(computerName, connOptions, result, username, password, domain);

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

        private void QueryTpmInfo(string computerName, ConnectionOptions connOptions, BiosQueryResult result, string username = null, string password = null, string domain = null)
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
            catch (Exception wmiEx)
            {
                // WMI TPM namespace often requires elevated privileges — fall back to PowerShell Get-Tpm
                _consoleForm?.WriteWarning($"  WMI TPM query failed ({wmiEx.Message}), trying PowerShell fallback...");
                try
                {
                    QueryTpmInfoViaPowerShell(computerName, result, username, password, domain);
                }
                catch (Exception psEx)
                {
                    _consoleForm?.WriteWarning($"  PowerShell TPM fallback also failed: {psEx.Message}");
                    result.TpmPresent = "Unknown (requires elevation)";
                    result.TpmVersion = "Unknown";
                    result.TpmEnabled = "Unknown";
                    result.TpmActivated = "Unknown";
                }
            }

            _consoleForm?.WriteInfo($"  TPM: {result.TpmPresent} (Version: {result.TpmVersion})");
        }

        private void QueryTpmInfoViaPowerShell(string computerName, BiosQueryResult result, string username, string password, string domain)
        {
            if (IsLocalComputer(computerName))
            {
                // Local: must use elevated external powershell.exe because in-process PS Core doesn't support Get-Tpm
                QueryLocalTpmViaElevatedPowerShell(result);
            }
            else
            {
                // Remote: no UAC needed, supplied credentials handle authorization on the remote end
                QueryRemoteTpmViaPowerShell(computerName, result, username, password, domain);
            }
        }

        /// <summary>
        /// Queries TPM info on the local machine by launching an elevated powershell.exe (Windows PowerShell 5.1) process.
        /// This triggers a one-time UAC prompt so the rest of the app can remain unelevated.
        /// Results are written to a temp file since stdout cannot be redirected from an elevated process.
        /// Both Get-Tpm and Confirm-SecureBootUEFI are bundled in one call to avoid a second UAC prompt.
        /// </summary>
        private void QueryLocalTpmViaElevatedPowerShell(BiosQueryResult result)
        {
            string tempOutput = Path.Combine(Path.GetTempPath(), $"sa_tpm_{Guid.NewGuid():N}.txt");
            string tempScript = Path.Combine(Path.GetTempPath(), $"sa_tpm_{Guid.NewGuid():N}.ps1");

            try
            {
                string script = $@"
try {{
    $tpm = Get-Tpm -ErrorAction Stop
    $tpmDetails = Get-CimInstance -Namespace root/CIMV2/Security/MicrosoftTpm -ClassName Win32_Tpm -ErrorAction SilentlyContinue
    $specVersion = if ($tpmDetails) {{ $tpmDetails.SpecVersion }} else {{ 'Unknown' }}
    try {{ $sb = Confirm-SecureBootUEFI -ErrorAction Stop }} catch {{ $sb = 'Unsupported' }}
    @(
        ""TpmPresent=$($tpm.TpmPresent)"",
        ""TpmReady=$($tpm.TpmReady)"",
        ""TpmEnabled=$($tpm.TpmEnabled)"",
        ""TpmActivated=$($tpm.TpmActivated)"",
        ""SpecVersion=$specVersion"",
        ""SecureBoot=$sb""
    ) | Out-File -FilePath '{tempOutput}' -Encoding UTF8
}} catch {{
    ""ERROR=$($_.Exception.Message)"" | Out-File -FilePath '{tempOutput}' -Encoding UTF8
}}";

                File.WriteAllText(tempScript, script);

                _consoleForm?.WriteInfo("  Requesting elevated privileges for TPM query (UAC prompt may appear)...");

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{tempScript}\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc == null)
                        throw new Exception("Failed to start elevated PowerShell process");

                    if (!proc.WaitForExit(30000))
                    {
                        proc.Kill();
                        throw new Exception("Elevated PowerShell TPM query timed out");
                    }
                }

                if (!File.Exists(tempOutput))
                    throw new Exception("Elevated PowerShell TPM query produced no output (UAC may have been declined)");

                ParseTpmOutput(File.ReadAllLines(tempOutput), result);
            }
            finally
            {
                try { if (File.Exists(tempScript)) File.Delete(tempScript); } catch { }
                try { if (File.Exists(tempOutput)) File.Delete(tempOutput); } catch { }
            }
        }

        /// <summary>
        /// Queries TPM info on a remote machine by launching powershell.exe (Windows PowerShell 5.1) externally.
        /// No elevation needed — the supplied credentials handle authorization on the remote end.
        /// Stdout is redirected directly since no UAC/RunAs is involved.
        /// </summary>
        private void QueryRemoteTpmViaPowerShell(string computerName, BiosQueryResult result, string username, string password, string domain)
        {
            string tempScript = Path.Combine(Path.GetTempPath(), $"sa_tpm_{Guid.NewGuid():N}.ps1");

            try
            {
                string script = $@"
try {{
    $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
    $tpmData = Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
        $tpm = Get-Tpm -ErrorAction Stop
        $tpmDetails = Get-CimInstance -Namespace root/CIMV2/Security/MicrosoftTpm -ClassName Win32_Tpm -ErrorAction SilentlyContinue
        $specVersion = if ($tpmDetails) {{ $tpmDetails.SpecVersion }} else {{ 'Unknown' }}
        try {{ $sb = Confirm-SecureBootUEFI -ErrorAction Stop }} catch {{ $sb = 'Unsupported' }}
        [PSCustomObject]@{{
            TpmPresent = $tpm.TpmPresent
            TpmReady = $tpm.TpmReady
            TpmEnabled = $tpm.TpmEnabled
            TpmActivated = $tpm.TpmActivated
            SpecVersion = $specVersion
            SecureBoot = $sb
        }}
    }} -ErrorAction Stop
    Write-Output ""TpmPresent=$($tpmData.TpmPresent)""
    Write-Output ""TpmReady=$($tpmData.TpmReady)""
    Write-Output ""TpmEnabled=$($tpmData.TpmEnabled)""
    Write-Output ""TpmActivated=$($tpmData.TpmActivated)""
    Write-Output ""SpecVersion=$($tpmData.SpecVersion)""
    Write-Output ""SecureBoot=$($tpmData.SecureBoot)""
}} catch {{
    Write-Output ""ERROR=$($_.Exception.Message)""
}}";

                File.WriteAllText(tempScript, script);

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{tempScript}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                string output;
                using (var proc = Process.Start(psi))
                {
                    if (proc == null)
                        throw new Exception("Failed to start PowerShell process for remote TPM query");

                    output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(30000);

                    if (!proc.HasExited)
                    {
                        proc.Kill();
                        throw new Exception("PowerShell remote TPM query timed out");
                    }
                }

                if (string.IsNullOrWhiteSpace(output))
                    throw new Exception("PowerShell remote TPM query produced no output");

                ParseTpmOutput(output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), result);
            }
            finally
            {
                try { if (File.Exists(tempScript)) File.Delete(tempScript); } catch { }
            }
        }

        /// <summary>
        /// Parses KEY=VALUE lines from PowerShell TPM output into a BiosQueryResult.
        /// Used by both the local elevated and remote query paths.
        /// </summary>
        private void ParseTpmOutput(string[] lines, BiosQueryResult result)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                int eqIndex = trimmed.IndexOf('=');
                if (eqIndex > 0)
                {
                    string key = trimmed.Substring(0, eqIndex);
                    string value = trimmed.Substring(eqIndex + 1);
                    data[key] = value;
                }
            }

            if (data.TryGetValue("ERROR", out string error))
                throw new Exception(error);

            bool tpmPresent = data.TryGetValue("TpmPresent", out string present) &&
                              string.Equals(present, "True", StringComparison.OrdinalIgnoreCase);

            result.TpmPresent = tpmPresent ? "Yes" : "No";

            if (tpmPresent)
            {
                result.TpmVersion = data.TryGetValue("SpecVersion", out string ver) ? ver.Trim() : "Unknown";
                result.TpmEnabled = data.TryGetValue("TpmEnabled", out string en) ? en : "Unknown";
                result.TpmActivated = data.TryGetValue("TpmActivated", out string act) ? act : "Unknown";
            }
            else
            {
                result.TpmVersion = "N/A";
                result.TpmEnabled = "N/A";
                result.TpmActivated = "N/A";
            }

            // Secure Boot (bundled into the same elevated PS process to avoid a second UAC prompt)
            if (data.TryGetValue("SecureBoot", out string sb))
            {
                if (string.Equals(sb, "True", StringComparison.OrdinalIgnoreCase))
                    _tpmFallbackSecureBoot = "Enabled";
                else if (string.Equals(sb, "False", StringComparison.OrdinalIgnoreCase))
                    _tpmFallbackSecureBoot = "Disabled";
                else
                    _tpmFallbackSecureBoot = sb;
            }
        }

        private void QuerySecureBoot(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            // If the elevated PowerShell TPM fallback already determined Secure Boot status, use that.
            if (!string.IsNullOrEmpty(_tpmFallbackSecureBoot))
            {
                result.SecureBootEnabled = _tpmFallbackSecureBoot;
                _consoleForm?.WriteInfo($"  Secure Boot: {result.SecureBootEnabled} (from elevated PowerShell)");
                return;
            }

            // Otherwise try Confirm-SecureBootUEFI via an external powershell.exe process.
            try
            {
                QuerySecureBootViaPowerShell(computerName, result);
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"  Secure Boot query failed: {ex.Message}");
                result.SecureBootEnabled = "Unable to determine";
            }
        }

        private void QuerySecureBootViaPowerShell(string computerName, BiosQueryResult result)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-ExecutionPolicy Bypass -NoProfile -Command \"try { $r = Confirm-SecureBootUEFI -ErrorAction Stop; Write-Output $r } catch { Write-Output 'Unsupported' }\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            string output;
            using (var proc = Process.Start(psi))
            {
                if (proc == null)
                    throw new Exception("Failed to start PowerShell process");

                output = proc.StandardOutput.ReadToEnd().Trim();
                proc.WaitForExit(15000);

                if (!proc.HasExited)
                {
                    proc.Kill();
                    throw new Exception("Secure Boot query timed out");
                }
            }

            if (string.Equals(output, "True", StringComparison.OrdinalIgnoreCase))
                result.SecureBootEnabled = "Enabled";
            else if (string.Equals(output, "False", StringComparison.OrdinalIgnoreCase))
                result.SecureBootEnabled = "Disabled";
            else
                result.SecureBootEnabled = output;
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
                    var connOptions = BuildConnectionOptions(computerName, username, password, domain, timeoutSeconds: 10);

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
