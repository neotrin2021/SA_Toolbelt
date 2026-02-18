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
    public class Windows_Tools
    {
        private readonly ConsoleForm _consoleForm;

        public Windows_Tools(ConsoleForm consoleForm = null)
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

        /// <summary>
        /// Represents a single Group Policy Object
        /// </summary>
        public class GpoInfo
        {
            public string DisplayName { get; set; }
            public string Id { get; set; }
            public string DomainName { get; set; }
            public string Owner { get; set; }
            public string GpoStatus { get; set; }
            public string Description { get; set; }
            public DateTime? CreationTime { get; set; }
            public DateTime? ModificationTime { get; set; }
            public List<string> LinksTo { get; set; } = new List<string>();
        }

        /// <summary>
        /// Result wrapper for GPO listing operations
        /// </summary>
        public class GpoListResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public List<GpoInfo> Gpos { get; set; } = new List<GpoInfo>();
        }

        /// <summary>
        /// Represents GPO settings applied to a specific computer or user (RSOP)
        /// </summary>
        public class GpoAppliedResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ComputerName { get; set; }
            public List<AppliedGpo> AppliedGpos { get; set; } = new List<AppliedGpo>();
            public string RsopReportHtml { get; set; }
        }

        /// <summary>
        /// A GPO that was actually applied during policy processing
        /// </summary>
        public class AppliedGpo
        {
            public string Name { get; set; }
            public string GpoId { get; set; }
            public string LinkedTo { get; set; }
            public string FilterStatus { get; set; }
            public bool IsApplied { get; set; }
            public int Order { get; set; }
        }

        /// <summary>
        /// Represents a single GPO link to an OU/Domain/Site
        /// </summary>
        public class GpoLink
        {
            public string GpoName { get; set; }
            public string GpoId { get; set; }
            public string Target { get; set; }
            public bool Enabled { get; set; }
            public bool Enforced { get; set; }
            public int Order { get; set; }
        }

        /// <summary>
        /// Result wrapper for GPO link queries
        /// </summary>
        public class GpoLinkResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public List<GpoLink> Links { get; set; } = new List<GpoLink>();
        }

        /// <summary>
        /// Represents a firewall rule with its policy source
        /// </summary>
        public class FirewallRuleInfo
        {
            public string DisplayName { get; set; }
            public string Name { get; set; }
            public string Direction { get; set; }
            public string Action { get; set; }
            public string Profile { get; set; }
            public bool Enabled { get; set; }
            public string Protocol { get; set; }
            public string LocalPort { get; set; }
            public string RemotePort { get; set; }
            public string LocalAddress { get; set; }
            public string RemoteAddress { get; set; }
            public string Program { get; set; }
            public string PolicySource { get; set; }
            public string GpoName { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Result wrapper for firewall rule queries
        /// </summary>
        public class FirewallRuleResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ComputerName { get; set; }
            public List<FirewallRuleInfo> Rules { get; set; } = new List<FirewallRuleInfo>();
        }

        /// <summary>
        /// Represents the firewall profile status (Domain/Private/Public)
        /// </summary>
        public class FirewallProfileInfo
        {
            public string Profile { get; set; }
            public bool Enabled { get; set; }
            public string DefaultInboundAction { get; set; }
            public string DefaultOutboundAction { get; set; }
            public string LogFileName { get; set; }
            public bool LogAllowed { get; set; }
            public bool LogBlocked { get; set; }
            public string PolicySource { get; set; }
        }

        /// <summary>
        /// Result wrapper for firewall profile queries
        /// </summary>
        public class FirewallProfileResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ComputerName { get; set; }
            public List<FirewallProfileInfo> Profiles { get; set; } = new List<FirewallProfileInfo>();
        }

        /// <summary>
        /// Represents a single GPO setting extracted from an HTML or XML report
        /// </summary>
        public class GpoSettingInfo
        {
            public string GpoName { get; set; }
            public string Category { get; set; }
            public string SettingName { get; set; }
            public string SettingValue { get; set; }
            public string Scope { get; set; }
        }

        /// <summary>
        /// Result wrapper for GPO setting search
        /// </summary>
        public class GpoSettingSearchResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string SearchTerm { get; set; }
            public List<GpoSettingInfo> MatchingSettings { get; set; } = new List<GpoSettingInfo>();
        }

        /// <summary>
        /// Represents an installed Windows service
        /// </summary>
        public class WindowsServiceInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Status { get; set; }
            public string StartType { get; set; }
            public string Account { get; set; }
            public string Path { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Result wrapper for Windows service queries
        /// </summary>
        public class WindowsServiceResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ComputerName { get; set; }
            public List<WindowsServiceInfo> Services { get; set; } = new List<WindowsServiceInfo>();
        }

        /// <summary>
        /// Represents an installed program/application
        /// </summary>
        public class InstalledProgramInfo
        {
            public string DisplayName { get; set; }
            public string DisplayVersion { get; set; }
            public string Publisher { get; set; }
            public string InstallDate { get; set; }
            public string InstallLocation { get; set; }
            public string UninstallString { get; set; }
        }

        /// <summary>
        /// Result wrapper for installed programs query
        /// </summary>
        public class InstalledProgramResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ComputerName { get; set; }
            public List<InstalledProgramInfo> Programs { get; set; } = new List<InstalledProgramInfo>();
        }

        /// <summary>
        /// Represents a scheduled task on a remote machine
        /// </summary>
        public class ScheduledTaskInfo
        {
            public string TaskName { get; set; }
            public string TaskPath { get; set; }
            public string State { get; set; }
            public string LastRunTime { get; set; }
            public string NextRunTime { get; set; }
            public string LastResult { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Result wrapper for scheduled task queries
        /// </summary>
        public class ScheduledTaskResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string ComputerName { get; set; }
            public List<ScheduledTaskInfo> Tasks { get; set; } = new List<ScheduledTaskInfo>();
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
                QueryLocalTpmViaElevatedPowerShell(result);
            }
            else
            {
                QueryRemoteTpmViaPowerShell(computerName, result, username, password, domain);
            }
        }

        /// <summary>
        /// Queries TPM info on the local machine by launching an elevated powershell.exe (Windows PowerShell 5.1) process.
        /// This triggers a one-time UAC prompt so the rest of the app can remain unelevated.
        /// Results are written to a temp file since stdout cannot be redirected from an elevated process.
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
                    result.SecureBootEnabled = "Enabled";
                else if (string.Equals(sb, "False", StringComparison.OrdinalIgnoreCase))
                    result.SecureBootEnabled = "Disabled";
                else
                    result.SecureBootEnabled = sb;
            }
        }

        private void QuerySecureBoot(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            // If the elevated PowerShell TPM fallback already determined Secure Boot status, use that.
            if (!string.IsNullOrEmpty(result.SecureBootEnabled))
            {
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

        #region GPO Operations

        /// <summary>
        /// Get all GPOs in the domain. Requires RSAT GroupPolicy module.
        /// </summary>
        public async Task<GpoListResult> GetAllGposAsync(string domain = null)
        {
            return await Task.Run(() =>
            {
                var result = new GpoListResult();

                try
                {
                    _consoleForm?.WriteInfo("Retrieving all Group Policy Objects...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();

                        if (ps.HadErrors)
                        {
                            result.Success = false;
                            result.ErrorMessage = "Failed to load GroupPolicy module. Ensure RSAT is installed.";
                            _consoleForm?.WriteError(result.ErrorMessage);
                            return result;
                        }

                        ps.Commands.Clear();

                        string script = string.IsNullOrEmpty(domain)
                            ? "Get-GPO -All | Select-Object DisplayName, Id, DomainName, Owner, GpoStatus, Description, CreationTime, ModificationTime"
                            : $"Get-GPO -All -Domain '{domain}' | Select-Object DisplayName, Id, DomainName, Owner, GpoStatus, Description, CreationTime, ModificationTime";

                        ps.AddScript(script);
                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            var gpo = new GpoInfo
                            {
                                DisplayName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                Id = obj.Properties["Id"]?.Value?.ToString() ?? "",
                                DomainName = obj.Properties["DomainName"]?.Value?.ToString() ?? "",
                                Owner = obj.Properties["Owner"]?.Value?.ToString() ?? "",
                                GpoStatus = obj.Properties["GpoStatus"]?.Value?.ToString() ?? "",
                                Description = obj.Properties["Description"]?.Value?.ToString() ?? ""
                            };

                            if (obj.Properties["CreationTime"]?.Value is DateTime ct)
                                gpo.CreationTime = ct;
                            if (obj.Properties["ModificationTime"]?.Value is DateTime mt)
                                gpo.ModificationTime = mt;

                            result.Gpos.Add(gpo);
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  PS Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Retrieved {result.Gpos.Count} GPOs");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve GPOs: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Get a specific GPO by name, including its link locations
        /// </summary>
        public async Task<GpoListResult> GetGpoByNameAsync(string gpoName, string domain = null)
        {
            return await Task.Run(() =>
            {
                var result = new GpoListResult();

                try
                {
                    _consoleForm?.WriteInfo($"Looking up GPO: {gpoName}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";
                        ps.AddScript($"Get-GPO -Name '{gpoName}'{domainParam} -ErrorAction Stop | Select-Object DisplayName, Id, DomainName, Owner, GpoStatus, Description, CreationTime, ModificationTime");
                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            var gpo = new GpoInfo
                            {
                                DisplayName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                Id = obj.Properties["Id"]?.Value?.ToString() ?? "",
                                DomainName = obj.Properties["DomainName"]?.Value?.ToString() ?? "",
                                Owner = obj.Properties["Owner"]?.Value?.ToString() ?? "",
                                GpoStatus = obj.Properties["GpoStatus"]?.Value?.ToString() ?? "",
                                Description = obj.Properties["Description"]?.Value?.ToString() ?? ""
                            };

                            if (obj.Properties["CreationTime"]?.Value is DateTime ct)
                                gpo.CreationTime = ct;
                            if (obj.Properties["ModificationTime"]?.Value is DateTime mt)
                                gpo.ModificationTime = mt;

                            result.Gpos.Add(gpo);
                        }

                        if (ps.HadErrors)
                        {
                            var errorMsg = ps.Streams.Error.FirstOrDefault()?.Exception?.Message ?? "GPO not found";
                            result.Success = false;
                            result.ErrorMessage = errorMsg;
                            _consoleForm?.WriteError($"Error looking up GPO '{gpoName}': {errorMsg}");
                            return result;
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found GPO: {gpoName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to look up GPO: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Get all GPO links for a specific OU, domain, or site
        /// </summary>
        public async Task<GpoLinkResult> GetGpoLinksForTargetAsync(string targetDn, string domain = null)
        {
            return await Task.Run(() =>
            {
                var result = new GpoLinkResult();

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving GPO links for: {targetDn}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";
                        ps.AddScript($@"
                            (Get-GPInheritance -Target '{targetDn}'{domainParam} -ErrorAction Stop).GpoLinks |
                            Select-Object DisplayName, GpoId, Target, Enabled, Enforced, Order
                        ");
                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Links.Add(new GpoLink
                            {
                                GpoName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                GpoId = obj.Properties["GpoId"]?.Value?.ToString() ?? "",
                                Target = obj.Properties["Target"]?.Value?.ToString() ?? "",
                                Enabled = obj.Properties["Enabled"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                Enforced = obj.Properties["Enforced"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                Order = int.TryParse(obj.Properties["Order"]?.Value?.ToString(), out int o) ? o : 0
                            });
                        }

                        if (ps.HadErrors)
                        {
                            var errorMsg = ps.Streams.Error.FirstOrDefault()?.Exception?.Message ?? "Unknown error";
                            result.Success = false;
                            result.ErrorMessage = errorMsg;
                            _consoleForm?.WriteError($"Error retrieving GPO links: {errorMsg}");
                            return result;
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found {result.Links.Count} GPO links on {targetDn}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve GPO links: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Get all OUs/sites/domains where a specific GPO is linked
        /// </summary>
        public async Task<GpoLinkResult> GetGpoLinkLocationsAsync(string gpoName, string domain = null)
        {
            return await Task.Run(() =>
            {
                var result = new GpoLinkResult();

                try
                {
                    _consoleForm?.WriteInfo($"Finding link locations for GPO: {gpoName}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop; Import-Module ActiveDirectory -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";
                        string serverParam = string.IsNullOrEmpty(domain) ? "" : $" -Server '{domain}'";

                        // Search the domain, all OUs, and all sites for links to this GPO
                        ps.AddScript($@"
                            $gpo = Get-GPO -Name '{gpoName}'{domainParam} -ErrorAction Stop
                            $gpoGuid = $gpo.Id.ToString()
                            $links = @()

                            # Check domain root
                            $domainDN = (Get-ADDomain{serverParam}).DistinguishedName
                            $inheritance = Get-GPInheritance -Target $domainDN{domainParam} -ErrorAction SilentlyContinue
                            if ($inheritance) {{
                                $inheritance.GpoLinks | Where-Object {{ $_.GpoId -eq $gpoGuid }} | ForEach-Object {{
                                    $links += [PSCustomObject]@{{
                                        DisplayName = $_.DisplayName
                                        GpoId = $_.GpoId
                                        Target = $domainDN
                                        Enabled = $_.Enabled
                                        Enforced = $_.Enforced
                                        Order = $_.Order
                                    }}
                                }}
                            }}

                            # Check all OUs
                            Get-ADOrganizationalUnit -Filter *{serverParam} | ForEach-Object {{
                                $ou = $_.DistinguishedName
                                $inheritance = Get-GPInheritance -Target $ou{domainParam} -ErrorAction SilentlyContinue
                                if ($inheritance) {{
                                    $inheritance.GpoLinks | Where-Object {{ $_.GpoId -eq $gpoGuid }} | ForEach-Object {{
                                        $links += [PSCustomObject]@{{
                                            DisplayName = $_.DisplayName
                                            GpoId = $_.GpoId
                                            Target = $ou
                                            Enabled = $_.Enabled
                                            Enforced = $_.Enforced
                                            Order = $_.Order
                                        }}
                                    }}
                                }}
                            }}

                            $links
                        ");
                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Links.Add(new GpoLink
                            {
                                GpoName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                GpoId = obj.Properties["GpoId"]?.Value?.ToString() ?? "",
                                Target = obj.Properties["Target"]?.Value?.ToString() ?? "",
                                Enabled = obj.Properties["Enabled"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                Enforced = obj.Properties["Enforced"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                Order = int.TryParse(obj.Properties["Order"]?.Value?.ToString(), out int o) ? o : 0
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  PS Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"GPO '{gpoName}' is linked to {result.Links.Count} location(s)");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to find GPO links: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Get the resultant set of policy (RSOP) for a computer - shows which GPOs are actually applied
        /// </summary>
        public async Task<GpoAppliedResult> GetAppliedGposAsync(string computerName, string domain = null)
        {
            return await Task.Run(() =>
            {
                var result = new GpoAppliedResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Generating resultant set of policy for {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        // Get-GPResultantSetOfPolicy generates an XML/HTML report
                        // We use gpresult-style command to get applied GPOs
                        ps.AddScript($@"
                            $rsop = Get-GPResultantSetOfPolicy -Computer '{computerName}' -ReportType Xml -ErrorAction Stop
                            [xml]$xml = $rsop

                            $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
                            $ns.AddNamespace('rsop', 'http://www.microsoft.com/GroupPolicy/Rsop')
                            $ns.AddNamespace('settings', 'http://www.microsoft.com/GroupPolicy/Settings')

                            $applied = @()

                            # Computer GPOs
                            $computerGpos = $xml.SelectNodes('//rsop:ComputerResults/rsop:GPO', $ns)
                            if (-not $computerGpos) {{ $computerGpos = $xml.SelectNodes('//ComputerResults/GPO') }}
                            $order = 1
                            foreach ($g in $computerGpos) {{
                                $name = if ($g.Name) {{ $g.Name.InnerText ?? $g.Name }} else {{ 'Unknown' }}
                                $path = if ($g.Path) {{ $g.Path.InnerText ?? $g.Path }} else {{ '' }}
                                $link = if ($g.Link) {{ $g.Link.InnerText ?? $g.Link }} else {{ '' }}
                                $filterStatus = if ($g.FilterAllowed) {{ $g.FilterAllowed }} else {{ 'True' }}

                                $applied += [PSCustomObject]@{{
                                    Name = $name
                                    GpoId = $path
                                    LinkedTo = $link
                                    FilterStatus = $filterStatus
                                    IsApplied = $true
                                    Order = $order
                                    Scope = 'Computer'
                                }}
                                $order++
                            }}

                            # User GPOs
                            $userGpos = $xml.SelectNodes('//rsop:UserResults/rsop:GPO', $ns)
                            if (-not $userGpos) {{ $userGpos = $xml.SelectNodes('//UserResults/GPO') }}
                            $order = 1
                            foreach ($g in $userGpos) {{
                                $name = if ($g.Name) {{ $g.Name.InnerText ?? $g.Name }} else {{ 'Unknown' }}
                                $path = if ($g.Path) {{ $g.Path.InnerText ?? $g.Path }} else {{ '' }}
                                $link = if ($g.Link) {{ $g.Link.InnerText ?? $g.Link }} else {{ '' }}
                                $filterStatus = if ($g.FilterAllowed) {{ $g.FilterAllowed }} else {{ 'True' }}

                                $applied += [PSCustomObject]@{{
                                    Name = $name
                                    GpoId = $path
                                    LinkedTo = $link
                                    FilterStatus = $filterStatus
                                    IsApplied = $true
                                    Order = $order
                                    Scope = 'User'
                                }}
                                $order++
                            }}

                            $applied
                        ");
                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.AppliedGpos.Add(new AppliedGpo
                            {
                                Name = obj.Properties["Name"]?.Value?.ToString() ?? "",
                                GpoId = obj.Properties["GpoId"]?.Value?.ToString() ?? "",
                                LinkedTo = obj.Properties["LinkedTo"]?.Value?.ToString() ?? "",
                                FilterStatus = obj.Properties["FilterStatus"]?.Value?.ToString() ?? "",
                                IsApplied = true,
                                Order = int.TryParse(obj.Properties["Order"]?.Value?.ToString(), out int o) ? o : 0
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  RSOP Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found {result.AppliedGpos.Count} applied GPOs on {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to generate RSOP: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Generate an HTML RSOP report for a computer (for display in a WebBrowser or save to disk)
        /// </summary>
        public async Task<GpoAppliedResult> GenerateGpoHtmlReportAsync(string computerName)
        {
            return await Task.Run(() =>
            {
                var result = new GpoAppliedResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Generating HTML GPO report for {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        ps.AddScript($"Get-GPResultantSetOfPolicy -Computer '{computerName}' -ReportType Html -ErrorAction Stop");
                        var results = ps.Invoke();

                        if (results.Count > 0)
                        {
                            result.RsopReportHtml = results[0]?.BaseObject?.ToString() ?? "";
                        }

                        if (ps.HadErrors)
                        {
                            var errorMsg = ps.Streams.Error.FirstOrDefault()?.Exception?.Message ?? "Unknown error";
                            result.Success = false;
                            result.ErrorMessage = errorMsg;
                            _consoleForm?.WriteError($"Error generating report: {errorMsg}");
                            return result;
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"HTML RSOP report generated for {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to generate HTML report: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Generate an XML GPO report for a single GPO (shows all settings configured in it)
        /// </summary>
        public async Task<string> GetGpoReportXmlAsync(string gpoName, string domain = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _consoleForm?.WriteInfo($"Generating XML report for GPO: {gpoName}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";
                        ps.AddScript($"Get-GPOReport -Name '{gpoName}'{domainParam} -ReportType Xml -ErrorAction Stop");
                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var errorMsg = ps.Streams.Error.FirstOrDefault()?.Exception?.Message ?? "Unknown error";
                            _consoleForm?.WriteError($"Error generating GPO report: {errorMsg}");
                            return null;
                        }

                        _consoleForm?.WriteSuccess($"XML report generated for GPO: {gpoName}");
                        return results.FirstOrDefault()?.BaseObject?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to generate GPO report: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Search all GPOs for a specific setting keyword (searches GPO report XML)
        /// </summary>
        public async Task<GpoSettingSearchResult> SearchGpoSettingsAsync(string searchTerm, string domain = null)
        {
            return await Task.Run(() =>
            {
                var result = new GpoSettingSearchResult { SearchTerm = searchTerm };

                try
                {
                    _consoleForm?.WriteInfo($"Searching all GPOs for settings matching '{searchTerm}'...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";

                        // Get all GPO reports as XML and search through them
                        ps.AddScript($@"
                            $searchTerm = '{searchTerm}'
                            $allGpos = Get-GPO -All{domainParam}
                            $matches = @()

                            foreach ($gpo in $allGpos) {{
                                try {{
                                    $report = Get-GPOReport -Guid $gpo.Id{domainParam} -ReportType Xml -ErrorAction SilentlyContinue
                                    if ($report -and $report -match [regex]::Escape($searchTerm)) {{
                                        [xml]$xml = $report

                                        # Extract computer settings
                                        $computerExt = $xml.GPO.Computer.ExtensionData
                                        if ($computerExt) {{
                                            foreach ($ext in $computerExt) {{
                                                $extXml = $ext.OuterXml
                                                if ($extXml -match [regex]::Escape($searchTerm)) {{
                                                    $matches += [PSCustomObject]@{{
                                                        GpoName = $gpo.DisplayName
                                                        Category = $ext.Name
                                                        SettingName = 'See GPO report for details'
                                                        SettingValue = ''
                                                        Scope = 'Computer'
                                                    }}
                                                }}
                                            }}
                                        }}

                                        # Extract user settings
                                        $userExt = $xml.GPO.User.ExtensionData
                                        if ($userExt) {{
                                            foreach ($ext in $userExt) {{
                                                $extXml = $ext.OuterXml
                                                if ($extXml -match [regex]::Escape($searchTerm)) {{
                                                    $matches += [PSCustomObject]@{{
                                                        GpoName = $gpo.DisplayName
                                                        Category = $ext.Name
                                                        SettingName = 'See GPO report for details'
                                                        SettingValue = ''
                                                        Scope = 'User'
                                                    }}
                                                }}
                                            }}
                                        }}

                                        # If we matched the raw XML but didn't find specific extensions, note the GPO
                                        if ($matches.Count -eq 0 -or $matches[-1].GpoName -ne $gpo.DisplayName) {{
                                            $matches += [PSCustomObject]@{{
                                                GpoName = $gpo.DisplayName
                                                Category = 'General'
                                                SettingName = 'Contains matching setting'
                                                SettingValue = $searchTerm
                                                Scope = 'Unknown'
                                            }}
                                        }}
                                    }}
                                }} catch {{
                                    # Skip GPOs we can't read
                                }}
                            }}

                            $matches
                        ");

                        _consoleForm?.WriteInfo("  This may take a while depending on the number of GPOs...");
                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.MatchingSettings.Add(new GpoSettingInfo
                            {
                                GpoName = obj.Properties["GpoName"]?.Value?.ToString() ?? "",
                                Category = obj.Properties["Category"]?.Value?.ToString() ?? "",
                                SettingName = obj.Properties["SettingName"]?.Value?.ToString() ?? "",
                                SettingValue = obj.Properties["SettingValue"]?.Value?.ToString() ?? "",
                                Scope = obj.Properties["Scope"]?.Value?.ToString() ?? ""
                            });
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found {result.MatchingSettings.Count} matching settings across GPOs");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to search GPO settings: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Force a Group Policy update on a remote computer
        /// </summary>
        public async Task<bool> ForceGpUpdateAsync(string computerName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _consoleForm?.WriteInfo($"Forcing Group Policy update on {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript($"Invoke-GPUpdate -Computer '{computerName}' -Force -ErrorAction Stop");
                        ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteError($"  GPUpdate error: {error.Exception?.Message}");
                            return false;
                        }

                        _consoleForm?.WriteSuccess($"Group Policy update triggered on {computerName}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to force GP update on {computerName}: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Backup a specific GPO to a local path
        /// </summary>
        public async Task<bool> BackupGpoAsync(string gpoName, string backupPath, string domain = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _consoleForm?.WriteInfo($"Backing up GPO '{gpoName}' to {backupPath}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";
                        ps.AddScript($"Backup-GPO -Name '{gpoName}'{domainParam} -Path '{backupPath}' -ErrorAction Stop");
                        ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteError($"  Backup error: {error.Exception?.Message}");
                            return false;
                        }

                        _consoleForm?.WriteSuccess($"GPO '{gpoName}' backed up to {backupPath}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to backup GPO: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Backup ALL GPOs in the domain to a local path
        /// </summary>
        public async Task<bool> BackupAllGposAsync(string backupPath, string domain = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _consoleForm?.WriteInfo($"Backing up all GPOs to {backupPath}...");

                    using (var ps = PowerShell.Create())
                    {
                        ps.AddScript("Import-Module GroupPolicy -ErrorAction Stop");
                        ps.Invoke();
                        ps.Commands.Clear();

                        string domainParam = string.IsNullOrEmpty(domain) ? "" : $" -Domain '{domain}'";
                        ps.AddScript($"Get-GPO -All{domainParam} | ForEach-Object {{ Backup-GPO -Guid $_.Id{domainParam} -Path '{backupPath}' -ErrorAction SilentlyContinue }}");
                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Backup warning: {error.Exception?.Message}");
                        }

                        _consoleForm?.WriteSuccess($"GPO backup completed. {results.Count} GPO(s) backed up to {backupPath}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to backup GPOs: {ex.Message}");
                    return false;
                }
            });
        }

        #endregion

        #region Firewall Operations

        /// <summary>
        /// Get all firewall rules on a remote computer, including which GPO deployed them
        /// </summary>
        public async Task<FirewallRuleResult> GetFirewallRulesAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new FirewallRuleResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving firewall rules from {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        string credential = $@"
                            $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
                            $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
                        ";

                        ps.AddScript($@"
                            {credential}
                            Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
                                Get-NetFirewallRule | ForEach-Object {{
                                    $rule = $_
                                    $portFilter = $_ | Get-NetFirewallPortFilter -ErrorAction SilentlyContinue
                                    $addrFilter = $_ | Get-NetFirewallAddressFilter -ErrorAction SilentlyContinue
                                    $appFilter = $_ | Get-NetFirewallApplicationFilter -ErrorAction SilentlyContinue

                                    [PSCustomObject]@{{
                                        DisplayName = $rule.DisplayName
                                        Name = $rule.Name
                                        Direction = $rule.Direction.ToString()
                                        Action = $rule.Action.ToString()
                                        Profile = $rule.Profile.ToString()
                                        Enabled = $rule.Enabled.ToString()
                                        Protocol = if ($portFilter) {{ $portFilter.Protocol }} else {{ 'Any' }}
                                        LocalPort = if ($portFilter) {{ $portFilter.LocalPort -join ',' }} else {{ 'Any' }}
                                        RemotePort = if ($portFilter) {{ $portFilter.RemotePort -join ',' }} else {{ 'Any' }}
                                        LocalAddress = if ($addrFilter) {{ $addrFilter.LocalAddress -join ',' }} else {{ 'Any' }}
                                        RemoteAddress = if ($addrFilter) {{ $addrFilter.RemoteAddress -join ',' }} else {{ 'Any' }}
                                        Program = if ($appFilter) {{ $appFilter.Program }} else {{ 'Any' }}
                                        PolicySource = $rule.PolicyStoreSource
                                        GpoName = $rule.PolicyStoreSourceType.ToString()
                                        Description = $rule.Description
                                    }}
                                }}
                            }} -ErrorAction Stop
                        ");

                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Rules.Add(new FirewallRuleInfo
                            {
                                DisplayName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                Name = obj.Properties["Name"]?.Value?.ToString() ?? "",
                                Direction = obj.Properties["Direction"]?.Value?.ToString() ?? "",
                                Action = obj.Properties["Action"]?.Value?.ToString() ?? "",
                                Profile = obj.Properties["Profile"]?.Value?.ToString() ?? "",
                                Enabled = obj.Properties["Enabled"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                Protocol = obj.Properties["Protocol"]?.Value?.ToString() ?? "Any",
                                LocalPort = obj.Properties["LocalPort"]?.Value?.ToString() ?? "Any",
                                RemotePort = obj.Properties["RemotePort"]?.Value?.ToString() ?? "Any",
                                LocalAddress = obj.Properties["LocalAddress"]?.Value?.ToString() ?? "Any",
                                RemoteAddress = obj.Properties["RemoteAddress"]?.Value?.ToString() ?? "Any",
                                Program = obj.Properties["Program"]?.Value?.ToString() ?? "Any",
                                PolicySource = obj.Properties["PolicySource"]?.Value?.ToString() ?? "",
                                GpoName = obj.Properties["GpoName"]?.Value?.ToString() ?? "",
                                Description = obj.Properties["Description"]?.Value?.ToString() ?? ""
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Firewall query warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Retrieved {result.Rules.Count} firewall rules from {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve firewall rules: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Get only GPO-deployed firewall rules on a remote computer
        /// </summary>
        public async Task<FirewallRuleResult> GetGpoFirewallRulesAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new FirewallRuleResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving GPO-deployed firewall rules from {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        string credential = $@"
                            $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
                            $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
                        ";

                        ps.AddScript($@"
                            {credential}
                            Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
                                # Get rules from the GroupPolicy store specifically
                                Get-NetFirewallRule -PolicyStore ActiveStore |
                                    Where-Object {{ $_.PolicyStoreSourceType -eq 'GroupPolicy' }} |
                                    ForEach-Object {{
                                        $rule = $_
                                        $portFilter = $_ | Get-NetFirewallPortFilter -ErrorAction SilentlyContinue
                                        $addrFilter = $_ | Get-NetFirewallAddressFilter -ErrorAction SilentlyContinue
                                        $appFilter = $_ | Get-NetFirewallApplicationFilter -ErrorAction SilentlyContinue

                                        [PSCustomObject]@{{
                                            DisplayName = $rule.DisplayName
                                            Name = $rule.Name
                                            Direction = $rule.Direction.ToString()
                                            Action = $rule.Action.ToString()
                                            Profile = $rule.Profile.ToString()
                                            Enabled = $rule.Enabled.ToString()
                                            Protocol = if ($portFilter) {{ $portFilter.Protocol }} else {{ 'Any' }}
                                            LocalPort = if ($portFilter) {{ $portFilter.LocalPort -join ',' }} else {{ 'Any' }}
                                            RemotePort = if ($portFilter) {{ $portFilter.RemotePort -join ',' }} else {{ 'Any' }}
                                            LocalAddress = if ($addrFilter) {{ $addrFilter.LocalAddress -join ',' }} else {{ 'Any' }}
                                            RemoteAddress = if ($addrFilter) {{ $addrFilter.RemoteAddress -join ',' }} else {{ 'Any' }}
                                            Program = if ($appFilter) {{ $appFilter.Program }} else {{ 'Any' }}
                                            PolicySource = $rule.PolicyStoreSource
                                            GpoName = $rule.PolicyStoreSource
                                            Description = $rule.Description
                                        }}
                                    }}
                            }} -ErrorAction Stop
                        ");

                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Rules.Add(new FirewallRuleInfo
                            {
                                DisplayName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                Name = obj.Properties["Name"]?.Value?.ToString() ?? "",
                                Direction = obj.Properties["Direction"]?.Value?.ToString() ?? "",
                                Action = obj.Properties["Action"]?.Value?.ToString() ?? "",
                                Profile = obj.Properties["Profile"]?.Value?.ToString() ?? "",
                                Enabled = obj.Properties["Enabled"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                Protocol = obj.Properties["Protocol"]?.Value?.ToString() ?? "Any",
                                LocalPort = obj.Properties["LocalPort"]?.Value?.ToString() ?? "Any",
                                RemotePort = obj.Properties["RemotePort"]?.Value?.ToString() ?? "Any",
                                LocalAddress = obj.Properties["LocalAddress"]?.Value?.ToString() ?? "Any",
                                RemoteAddress = obj.Properties["RemoteAddress"]?.Value?.ToString() ?? "Any",
                                Program = obj.Properties["Program"]?.Value?.ToString() ?? "Any",
                                PolicySource = obj.Properties["PolicySource"]?.Value?.ToString() ?? "",
                                GpoName = obj.Properties["GpoName"]?.Value?.ToString() ?? "",
                                Description = obj.Properties["Description"]?.Value?.ToString() ?? ""
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found {result.Rules.Count} GPO-deployed firewall rules on {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve GPO firewall rules: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Get firewall profile status (Domain/Private/Public) on a remote computer
        /// </summary>
        public async Task<FirewallProfileResult> GetFirewallProfilesAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new FirewallProfileResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving firewall profile status from {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        string credential = $@"
                            $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
                            $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
                        ";

                        ps.AddScript($@"
                            {credential}
                            Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
                                Get-NetFirewallProfile | ForEach-Object {{
                                    [PSCustomObject]@{{
                                        Profile = $_.Name
                                        Enabled = $_.Enabled.ToString()
                                        DefaultInboundAction = $_.DefaultInboundAction.ToString()
                                        DefaultOutboundAction = $_.DefaultOutboundAction.ToString()
                                        LogFileName = $_.LogFileName
                                        LogAllowed = $_.LogAllowed.ToString()
                                        LogBlocked = $_.LogBlocked.ToString()
                                        PolicySource = $_.PolicyStoreSource
                                    }}
                                }}
                            }} -ErrorAction Stop
                        ");

                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Profiles.Add(new FirewallProfileInfo
                            {
                                Profile = obj.Properties["Profile"]?.Value?.ToString() ?? "",
                                Enabled = obj.Properties["Enabled"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                DefaultInboundAction = obj.Properties["DefaultInboundAction"]?.Value?.ToString() ?? "",
                                DefaultOutboundAction = obj.Properties["DefaultOutboundAction"]?.Value?.ToString() ?? "",
                                LogFileName = obj.Properties["LogFileName"]?.Value?.ToString() ?? "",
                                LogAllowed = obj.Properties["LogAllowed"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                LogBlocked = obj.Properties["LogBlocked"]?.Value?.ToString()?.Equals("True", StringComparison.OrdinalIgnoreCase) ?? false,
                                PolicySource = obj.Properties["PolicySource"]?.Value?.ToString() ?? ""
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Retrieved {result.Profiles.Count} firewall profiles from {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve firewall profiles: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        #endregion

        #region Remote Service Operations

        /// <summary>
        /// Get all Windows services on a remote computer via WMI
        /// </summary>
        public async Task<WindowsServiceResult> GetRemoteServicesAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new WindowsServiceResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving services from {computerName}...");

                    var connOptions = BuildConnectionOptions(computerName, username, password, domain);

                    var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
                    scope.Connect();

                    using (var searcher = new ManagementObjectSearcher(scope,
                        new ObjectQuery("SELECT Name, DisplayName, State, StartMode, StartName, PathName, Description FROM Win32_Service")))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            result.Services.Add(new WindowsServiceInfo
                            {
                                Name = obj["Name"]?.ToString() ?? "",
                                DisplayName = obj["DisplayName"]?.ToString() ?? "",
                                Status = obj["State"]?.ToString() ?? "",
                                StartType = obj["StartMode"]?.ToString() ?? "",
                                Account = obj["StartName"]?.ToString() ?? "",
                                Path = obj["PathName"]?.ToString() ?? "",
                                Description = obj["Description"]?.ToString() ?? ""
                            });
                        }
                    }

                    result.Success = true;
                    _consoleForm?.WriteSuccess($"Retrieved {result.Services.Count} services from {computerName}");
                }
                catch (UnauthorizedAccessException)
                {
                    result.Success = false;
                    result.ErrorMessage = "Access denied. Verify credentials.";
                    _consoleForm?.WriteError($"Access denied on {computerName}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve services: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        /// <summary>
        /// Start, stop, or restart a service on a remote computer
        /// </summary>
        public async Task<bool> ManageRemoteServiceAsync(string computerName, string serviceName, string action, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _consoleForm?.WriteInfo($"{action} service '{serviceName}' on {computerName}...");

                    var connOptions = BuildConnectionOptions(computerName, username, password, domain);

                    var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
                    scope.Connect();

                    using (var searcher = new ManagementObjectSearcher(scope,
                        new ObjectQuery($"SELECT * FROM Win32_Service WHERE Name='{serviceName}'")))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            ManagementBaseObject outParams;
                            switch (action.ToLowerInvariant())
                            {
                                case "start":
                                    outParams = obj.InvokeMethod("StartService", null, null);
                                    break;
                                case "stop":
                                    outParams = obj.InvokeMethod("StopService", null, null);
                                    break;
                                case "restart":
                                    obj.InvokeMethod("StopService", null, null);
                                    System.Threading.Thread.Sleep(2000);
                                    outParams = obj.InvokeMethod("StartService", null, null);
                                    break;
                                default:
                                    _consoleForm?.WriteError($"Unknown action: {action}. Use start, stop, or restart.");
                                    return false;
                            }

                            _consoleForm?.WriteSuccess($"Service '{serviceName}' {action} completed on {computerName}");
                            return true;
                        }
                    }

                    _consoleForm?.WriteError($"Service '{serviceName}' not found on {computerName}");
                    return false;
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to {action} service '{serviceName}': {ex.Message}");
                    return false;
                }
            });
        }

        #endregion

        #region Installed Programs

        /// <summary>
        /// Get installed programs on a remote computer via WMI (registry-based, much faster than Win32_Product)
        /// </summary>
        public async Task<InstalledProgramResult> GetInstalledProgramsAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new InstalledProgramResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving installed programs from {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        string credential = $@"
                            $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
                            $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
                        ";

                        // Use registry instead of Win32_Product (which is slow and triggers reconfiguration)
                        ps.AddScript($@"
                            {credential}
                            Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
                                $paths = @(
                                    'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*',
                                    'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
                                )

                                Get-ItemProperty $paths -ErrorAction SilentlyContinue |
                                    Where-Object {{ $_.DisplayName -and $_.DisplayName -ne '' }} |
                                    Select-Object DisplayName, DisplayVersion, Publisher, InstallDate, InstallLocation, UninstallString |
                                    Sort-Object DisplayName
                            }} -ErrorAction Stop
                        ");

                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Programs.Add(new InstalledProgramInfo
                            {
                                DisplayName = obj.Properties["DisplayName"]?.Value?.ToString() ?? "",
                                DisplayVersion = obj.Properties["DisplayVersion"]?.Value?.ToString() ?? "",
                                Publisher = obj.Properties["Publisher"]?.Value?.ToString() ?? "",
                                InstallDate = obj.Properties["InstallDate"]?.Value?.ToString() ?? "",
                                InstallLocation = obj.Properties["InstallLocation"]?.Value?.ToString() ?? "",
                                UninstallString = obj.Properties["UninstallString"]?.Value?.ToString() ?? ""
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found {result.Programs.Count} installed programs on {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve installed programs: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        #endregion

        #region Scheduled Tasks

        /// <summary>
        /// Get scheduled tasks from a remote computer
        /// </summary>
        public async Task<ScheduledTaskResult> GetScheduledTasksAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new ScheduledTaskResult { ComputerName = computerName };

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving scheduled tasks from {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        string credential = $@"
                            $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
                            $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
                        ";

                        ps.AddScript($@"
                            {credential}
                            Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
                                Get-ScheduledTask | ForEach-Object {{
                                    $task = $_
                                    $info = $_ | Get-ScheduledTaskInfo -ErrorAction SilentlyContinue

                                    [PSCustomObject]@{{
                                        TaskName = $task.TaskName
                                        TaskPath = $task.TaskPath
                                        State = $task.State.ToString()
                                        LastRunTime = if ($info) {{ $info.LastRunTime.ToString('yyyy-MM-dd HH:mm:ss') }} else {{ 'N/A' }}
                                        NextRunTime = if ($info) {{ $info.NextRunTime.ToString('yyyy-MM-dd HH:mm:ss') }} else {{ 'N/A' }}
                                        LastResult = if ($info) {{ $info.LastTaskResult }} else {{ 'N/A' }}
                                        Author = $task.Author
                                        Description = $task.Description
                                    }}
                                }}
                            }} -ErrorAction Stop
                        ");

                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            result.Tasks.Add(new ScheduledTaskInfo
                            {
                                TaskName = obj.Properties["TaskName"]?.Value?.ToString() ?? "",
                                TaskPath = obj.Properties["TaskPath"]?.Value?.ToString() ?? "",
                                State = obj.Properties["State"]?.Value?.ToString() ?? "",
                                LastRunTime = obj.Properties["LastRunTime"]?.Value?.ToString() ?? "N/A",
                                NextRunTime = obj.Properties["NextRunTime"]?.Value?.ToString() ?? "N/A",
                                LastResult = obj.Properties["LastResult"]?.Value?.ToString() ?? "",
                                Author = obj.Properties["Author"]?.Value?.ToString() ?? "",
                                Description = obj.Properties["Description"]?.Value?.ToString() ?? ""
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Warning: {error.Exception?.Message}");
                        }

                        result.Success = true;
                        _consoleForm?.WriteSuccess($"Found {result.Tasks.Count} scheduled tasks on {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Failed to retrieve scheduled tasks: {ex.Message}";
                    _consoleForm?.WriteError(result.ErrorMessage);
                }

                return result;
            });
        }

        #endregion

        #region Remote Event Log

        /// <summary>
        /// Query Windows Event Log entries from a remote computer
        /// </summary>
        public async Task<List<Dictionary<string, string>>> GetEventLogEntriesAsync(
            string computerName, string username, string password, string domain,
            string logName = "System", int maxEntries = 100, string entryType = null)
        {
            return await Task.Run(() =>
            {
                var entries = new List<Dictionary<string, string>>();

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving last {maxEntries} {logName} events from {computerName}...");

                    using (var ps = PowerShell.Create())
                    {
                        string credential = $@"
                            $secPass = ConvertTo-SecureString '{password}' -AsPlainText -Force
                            $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
                        ";

                        string filterClause = string.IsNullOrEmpty(entryType)
                            ? ""
                            : $" | Where-Object {{ $_.EntryType -eq '{entryType}' }}";

                        ps.AddScript($@"
                            {credential}
                            Invoke-Command -ComputerName '{computerName}' -Credential $cred -ScriptBlock {{
                                Get-EventLog -LogName '{logName}' -Newest {maxEntries} -ErrorAction Stop{filterClause} |
                                    Select-Object TimeGenerated, EntryType, Source, EventID, Message |
                                    ForEach-Object {{
                                        [PSCustomObject]@{{
                                            TimeGenerated = $_.TimeGenerated.ToString('yyyy-MM-dd HH:mm:ss')
                                            EntryType = $_.EntryType.ToString()
                                            Source = $_.Source
                                            EventID = $_.EventID.ToString()
                                            Message = if ($_.Message.Length -gt 500) {{ $_.Message.Substring(0, 500) + '...' }} else {{ $_.Message }}
                                        }}
                                    }}
                            }} -ErrorAction Stop
                        ");

                        var results = ps.Invoke();

                        foreach (var obj in results)
                        {
                            entries.Add(new Dictionary<string, string>
                            {
                                ["TimeGenerated"] = obj.Properties["TimeGenerated"]?.Value?.ToString() ?? "",
                                ["EntryType"] = obj.Properties["EntryType"]?.Value?.ToString() ?? "",
                                ["Source"] = obj.Properties["Source"]?.Value?.ToString() ?? "",
                                ["EventID"] = obj.Properties["EventID"]?.Value?.ToString() ?? "",
                                ["Message"] = obj.Properties["Message"]?.Value?.ToString() ?? ""
                            });
                        }

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                                _consoleForm?.WriteWarning($"  Warning: {error.Exception?.Message}");
                        }

                        _consoleForm?.WriteSuccess($"Retrieved {entries.Count} event log entries from {computerName}");
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to retrieve event logs: {ex.Message}");
                }

                return entries;
            });
        }

        #endregion

        #region Network Shares

        /// <summary>
        /// Get network shares on a remote computer
        /// </summary>
        public async Task<List<Dictionary<string, string>>> GetNetworkSharesAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var shares = new List<Dictionary<string, string>>();

                try
                {
                    _consoleForm?.WriteInfo($"Retrieving network shares from {computerName}...");

                    var connOptions = BuildConnectionOptions(computerName, username, password, domain);

                    var scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", connOptions);
                    scope.Connect();

                    using (var searcher = new ManagementObjectSearcher(scope,
                        new ObjectQuery("SELECT Name, Path, Description, Type FROM Win32_Share")))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string shareType = obj["Type"]?.ToString() ?? "";
                            string friendlyType = shareType switch
                            {
                                "0" => "Disk Drive",
                                "1" => "Print Queue",
                                "2" => "Device",
                                "3" => "IPC",
                                "2147483648" => "Admin Disk",
                                "2147483649" => "Admin Print",
                                "2147483650" => "Admin Device",
                                "2147483651" => "Admin IPC",
                                _ => shareType
                            };

                            shares.Add(new Dictionary<string, string>
                            {
                                ["Name"] = obj["Name"]?.ToString() ?? "",
                                ["Path"] = obj["Path"]?.ToString() ?? "",
                                ["Description"] = obj["Description"]?.ToString() ?? "",
                                ["Type"] = friendlyType
                            });
                        }
                    }

                    _consoleForm?.WriteSuccess($"Found {shares.Count} shares on {computerName}");
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Failed to retrieve shares: {ex.Message}");
                }

                return shares;
            });
        }

        #endregion
    }
}
