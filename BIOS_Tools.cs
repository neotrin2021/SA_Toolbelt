using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SA_ToolBelt
{
    public class BIOS_Tools : IDisposable
    {
        private readonly ConsoleForm _consoleForm;

        // In-process PowerShell runspace for CMSL operations (mirrors VMwareManager pattern)
        private PowerShell _psRunspace;
        private bool _isCmslLoaded = false;
        private bool _disposed = false;

        // Serializes all _psRunspace operations — concurrent BIOS queries share this instance
        // and racing on Commands.Clear/AddScript/Invoke causes PSInvalidOperationException.
        private readonly object _psLock = new object();

        // Caches Secure Boot result from elevated TPM query so we don't trigger a second UAC prompt
        private string _tpmFallbackSecureBoot;

        public bool IsCmslLoaded => _isCmslLoaded;

        public BIOS_Tools(ConsoleForm consoleForm = null)
        {
            _consoleForm = consoleForm;
            _psRunspace = PowerShell.Create();
        }

        #region CMSL Initialization

        /// <summary>
        /// Loads the HP.ClientManagement (CMSL) module into the persistent runspace.
        /// <paramref name="cmslModulePath"/> should be the full path to the HP.ClientManagement
        /// folder (e.g. \\share\CMSL\HP.ClientManagement), matching the PowerCLI pattern.
        /// Returns true if CMSL loaded successfully. On failure, WMI fallbacks remain active.
        /// </summary>
        public async Task<bool> InitializeCmslAsync(string cmslModulePath = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (_psLock)
                    {
                        EnsureRunspaceValid();

                        _consoleForm?.WriteInfo("Initializing HP CMSL module...");

                        _psRunspace.Commands.Clear();
                        _psRunspace.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force");
                        _psRunspace.Invoke();

                        // Load from configured path if provided, otherwise try system-installed module
                        string importScript = !string.IsNullOrWhiteSpace(cmslModulePath)
                            ? $"Import-Module '{cmslModulePath.Replace("'", "''")}' -ErrorAction Stop"
                            : "Import-Module HP.ClientManagement -ErrorAction Stop";

                        _consoleForm?.WriteInfo(!string.IsNullOrWhiteSpace(cmslModulePath)
                            ? $"  Loading HP CMSL from: {cmslModulePath}"
                            : "  Loading HP CMSL from system module path");

                        _psRunspace.Commands.Clear();
                        _psRunspace.AddScript(importScript);
                        _psRunspace.Invoke();

                        if (_psRunspace.HadErrors)
                        {
                            var errors = string.Join("; ", _psRunspace.Streams.Error
                                .Select(e => e.Exception?.Message ?? e.ToString()));
                            _consoleForm?.WriteWarning($"HP CMSL load warning: {errors}");

                            // Verify module actually loaded despite errors
                            _psRunspace.Commands.Clear();
                            _psRunspace.AddScript("($null -ne (Get-Module -Name HP.ClientManagement))");
                            var check = _psRunspace.Invoke();
                            _isCmslLoaded = check.Count > 0 &&
                                            check[0]?.BaseObject is bool loaded && loaded;
                        }
                        else
                        {
                            _isCmslLoaded = true;
                        }

                        if (_isCmslLoaded)
                            _consoleForm?.WriteSuccess("HP CMSL module loaded — CMSL features enabled");
                        else
                            _consoleForm?.WriteWarning("HP CMSL not available — falling back to WMI for HP BIOS operations");
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteWarning($"HP CMSL not available ({ex.Message}) — falling back to WMI");
                    _isCmslLoaded = false;
                }

                return _isCmslLoaded;
            });
        }

        /// <summary>
        /// Verifies the runspace is still usable and recreates it if stale.
        /// </summary>
        // NOTE: callers must already hold _psLock before calling this method.
        private void EnsureRunspaceValid()
        {
            try
            {
                _psRunspace.Commands.Clear();
                _psRunspace.AddScript("$null");
                _psRunspace.Invoke();
            }
            catch
            {
                _psRunspace?.Dispose();
                _psRunspace = PowerShell.Create();
                _isCmslLoaded = false;
                _consoleForm?.WriteWarning("BIOS_Tools PowerShell runspace recreated");
            }
        }

        #endregion

        #region WMI Connection Helpers

        /// <summary>
        /// Determines if the target computer name refers to the local machine.
        /// </summary>
        private bool IsLocalComputer(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                return true;

            string name = computerName.Trim();

            // Strip domain suffix so FQDNs like MYPC.domain.com match the local machine name
            string shortName = name.Contains('.')
                ? name.Substring(0, name.IndexOf('.'))
                : name;

            return string.Equals(name, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(shortName, Environment.MachineName, StringComparison.OrdinalIgnoreCase)
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
            public bool IsRpcDisabled { get; set; }

            public BiosQueryResult() { }
        }

        /// <summary>
        /// Result from a BIOS setting modification attempt
        /// </summary>
        public class BiosSetResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public int ReturnCode { get; set; }
            public string SettingName { get; set; }
            public string NewValue { get; set; }
            public bool RequiresReboot => Success;

            public BiosSetResult() { }
        }

        #endregion

        #region Remote BIOS Query

        /// <summary>
        /// Query a remote computer for BIOS, hardware, TPM, and HP-specific settings.
        /// Uses WMI for standard queries. HP BIOS settings use CMSL (PSRemoting) when
        /// available, falling back to the HP WMI provider.
        /// </summary>
        /// <summary>
        /// Tests whether TCP port 135 (RPC Endpoint Mapper) is reachable on the remote computer.
        /// A fast pre-check that avoids long WMI timeouts when RPC is disabled or blocked.
        /// </summary>
        private static bool IsRpcAvailable(string computerName, int timeoutMs = 2000)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var ar = client.BeginConnect(computerName, 135, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(timeoutMs);
                    if (success && client.Connected)
                    {
                        client.EndConnect(ar);
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<BiosQueryResult> QueryRemoteBiosAsync(string computerName, string username, string password, string domain)
        {
            return await Task.Run(() =>
            {
                var result = new BiosQueryResult { ComputerName = computerName };

                try
                {
                    // Pre-check: verify RPC port 135 is reachable before attempting WMI connections.
                    // WMI relies on RPC; if it's blocked the connection will hang then fail anyway.
                    if (!IsRpcAvailable(computerName))
                    {
                        result.Success = false;
                        result.IsRpcDisabled = true;
                        result.ErrorMessage = "RPC Disabled";
                        _consoleForm?.WriteWarning($"{computerName}: RPC port 135 unreachable — skipping WMI query.");
                        return result;
                    }

                    _consoleForm?.WriteInfo($"Connecting to {computerName} via WMI...");

                    var connOptions = BuildConnectionOptions(computerName, username, password, domain);

                    // Standard WMI queries (manufacturer-agnostic)
                    QueryHardwareInfo(computerName, connOptions, result);
                    QueryBiosInfo(computerName, connOptions, result);
                    QueryOsInfo(computerName, connOptions, result);
                    QueryTpmInfo(computerName, connOptions, result, username, password, domain);
                    QuerySecureBoot(computerName, connOptions, result);

                    // HP-specific BIOS settings
                    bool isHp = result.Manufacturer != null && (
                        result.Manufacturer.IndexOf("HP", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        result.Manufacturer.IndexOf("Hewlett", StringComparison.OrdinalIgnoreCase) >= 0);

                    if (isHp)
                    {
                        result.IsHpMachine = true;

                        // Try CMSL first; fall back to WMI HP provider
                        bool cmslSucceeded = false;
                        if (_isCmslLoaded)
                        {
                            try
                            {
                                QueryHpBiosSettingsViaCmsl(computerName, username, password, domain, result);
                                cmslSucceeded = true;
                            }
                            catch (Exception cmslEx)
                            {
                                _consoleForm?.WriteWarning($"  CMSL HP BIOS query failed ({cmslEx.Message}), falling back to WMI...");
                            }
                        }

                        if (!cmslSucceeded)
                            QueryHpBiosSettingsViaWmi(computerName, connOptions, result);
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
        /// Queries TPM info on a remote machine using the in-process PowerShell runspace.
        /// No temp files needed — stdout is captured directly through the runspace.
        /// No elevation needed — the supplied credentials handle authorization on the remote end.
        /// </summary>
        private void QueryRemoteTpmViaPowerShell(string computerName, BiosQueryResult result, string username, string password, string domain)
        {
            string escapedPass = password?.Replace("'", "''") ?? string.Empty;
            string script = $@"
try {{
    $secPass = ConvertTo-SecureString '{escapedPass}' -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
    $tpmData = Invoke-Command -ComputerName '{computerName}' -Credential $cred -ErrorAction Stop -ScriptBlock {{
        $tpm = Get-Tpm -ErrorAction Stop
        $tpmDetails = Get-CimInstance -Namespace root/CIMV2/Security/MicrosoftTpm -ClassName Win32_Tpm -ErrorAction SilentlyContinue
        $specVersion = if ($tpmDetails) {{ $tpmDetails.SpecVersion }} else {{ 'Unknown' }}
        try {{ $sb = Confirm-SecureBootUEFI -ErrorAction Stop }} catch {{ $sb = 'Unsupported' }}
        [PSCustomObject]@{{
            TpmPresent  = $tpm.TpmPresent
            TpmReady    = $tpm.TpmReady
            TpmEnabled  = $tpm.TpmEnabled
            TpmActivated = $tpm.TpmActivated
            SpecVersion = $specVersion
            SecureBoot  = $sb
        }}
    }}
    Write-Output ""TpmPresent=$($tpmData.TpmPresent)""
    Write-Output ""TpmReady=$($tpmData.TpmReady)""
    Write-Output ""TpmEnabled=$($tpmData.TpmEnabled)""
    Write-Output ""TpmActivated=$($tpmData.TpmActivated)""
    Write-Output ""SpecVersion=$($tpmData.SpecVersion)""
    Write-Output ""SecureBoot=$($tpmData.SecureBoot)""
}} catch {{
    Write-Output ""ERROR=$($_.Exception.Message)""
}}";

            System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject> psResults;
            lock (_psLock)
            {
                EnsureRunspaceValid();
                _psRunspace.Commands.Clear();
                _psRunspace.AddScript(script);
                psResults = _psRunspace.Invoke();

                if (_psRunspace.HadErrors && psResults.Count == 0)
                {
                    var errorMsg = string.Join("; ", _psRunspace.Streams.Error
                        .Select(e => e.Exception?.Message ?? e.ToString()));
                    throw new Exception($"PowerShell remote TPM query failed: {errorMsg}");
                }
            }

            var lines = psResults
                .Where(r => r != null)
                .Select(r => r.ToString())
                .ToArray();

            if (lines.Length == 0)
                throw new Exception("PowerShell remote TPM query produced no output");

            ParseTpmOutput(lines, result);
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
                _consoleForm?.WriteInfo($"  Secure Boot: {result.SecureBootEnabled} (from PowerShell)");
                return;
            }

            // Otherwise try Confirm-SecureBootUEFI via the in-process runspace.
            try
            {
                QuerySecureBootViaRunspace(computerName, result);
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"  Secure Boot query failed: {ex.Message}");
                result.SecureBootEnabled = "Unable to determine";
            }
        }

        private void QuerySecureBootViaRunspace(string computerName, BiosQueryResult result)
        {
            System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject> psResults;
            lock (_psLock)
            {
                EnsureRunspaceValid();
                _psRunspace.Commands.Clear();
                _psRunspace.AddScript(
                    "try { $r = Confirm-SecureBootUEFI -ErrorAction Stop; Write-Output $r } catch { Write-Output 'Unsupported' }");
                psResults = _psRunspace.Invoke();
            }

            string output = psResults.Count > 0 ? psResults[0]?.ToString()?.Trim() : string.Empty;

            if (string.Equals(output, "True", StringComparison.OrdinalIgnoreCase))
                result.SecureBootEnabled = "Enabled";
            else if (string.Equals(output, "False", StringComparison.OrdinalIgnoreCase))
                result.SecureBootEnabled = "Disabled";
            else
                result.SecureBootEnabled = string.IsNullOrEmpty(output) ? "Unable to determine" : output;
        }

        #endregion

        #region HP BIOS Settings — CMSL Path

        /// <summary>
        /// Retrieves all HP BIOS settings using HP CMSL (HP.ClientManagement module).
        /// For local machine: calls Get-HPBIOSSettingsList directly in the persistent runspace.
        /// For remote machine: uses PSRemoting (Invoke-Command) with CMSL on the target.
        /// Requires WinRM enabled on the target for remote queries.
        /// </summary>
        private void QueryHpBiosSettingsViaCmsl(string computerName, string username, string password, string domain, BiosQueryResult result)
        {
            string script;

            if (IsLocalComputer(computerName))
            {
                _consoleForm?.WriteInfo("  Querying HP BIOS settings via CMSL (local)...");
                script = @"
Get-HPBIOSSettingsList | ForEach-Object {
    [PSCustomObject]@{
        Name  = $_.Name
        Value = if ($_.Value) { $_.Value } else { '(not set)' }
    }
}";
            }
            else
            {
                _consoleForm?.WriteInfo($"  Querying HP BIOS settings via CMSL (remote PSRemoting to {computerName})...");
                string escapedPass = password?.Replace("'", "''") ?? string.Empty;
                script = $@"
$secPass = ConvertTo-SecureString '{escapedPass}' -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
Invoke-Command -ComputerName '{computerName}' -Credential $cred -ErrorAction Stop -ScriptBlock {{
    Import-Module HP.ClientManagement -ErrorAction Stop
    Get-HPBIOSSettingsList | ForEach-Object {{
        [PSCustomObject]@{{
            Name  = $_.Name
            Value = if ($_.Value) {{ $_.Value }} else {{ '(not set)' }}
        }}
    }}
}}";
            }

            System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject> psResults;
            lock (_psLock)
            {
                EnsureRunspaceValid();
                _psRunspace.Commands.Clear();
                _psRunspace.AddScript(script);
                psResults = _psRunspace.Invoke();

                if (_psRunspace.HadErrors && psResults.Count == 0)
                {
                    var errorMsg = string.Join("; ", _psRunspace.Streams.Error
                        .Select(e => e.Exception?.Message ?? e.ToString()));
                    throw new Exception($"CMSL HP BIOS settings query failed: {errorMsg}");
                }
            }

            foreach (var obj in psResults)
            {
                if (obj == null) continue;

                string name = obj.Properties["Name"]?.Value?.ToString()?.Trim();
                string value = obj.Properties["Value"]?.Value?.ToString()?.Trim() ?? "(not set)";

                if (!string.IsNullOrEmpty(name))
                {
                    result.HpBiosSettings.Add(new BiosSetting
                    {
                        Name = name,
                        CurrentValue = value,
                        Category = CategorizeBiosSetting(name)
                    });
                }
            }

            _consoleForm?.WriteInfo($"  Retrieved {result.HpBiosSettings.Count} HP BIOS settings via CMSL");
        }

        #endregion

        #region HP BIOS Settings — WMI Fallback

        /// <summary>
        /// Retrieves HP BIOS settings via the HP WMI provider (root\HP\InstrumentedBIOS).
        /// Used as fallback when CMSL is not available or PSRemoting fails.
        /// Does not require WinRM — uses standard WMI remoting.
        /// </summary>
        private void QueryHpBiosSettingsViaWmi(string computerName, ConnectionOptions connOptions, BiosQueryResult result)
        {
            try
            {
                _consoleForm?.WriteInfo("  Querying HP BIOS settings via WMI (HP provider)...");

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
                            result.HpBiosSettings.Add(new BiosSetting
                            {
                                Name = name,
                                CurrentValue = string.IsNullOrEmpty(value) ? "(not set)" : value,
                                Category = CategorizeBiosSetting(name)
                            });
                        }
                    }
                }

                _consoleForm?.WriteInfo($"  Retrieved {result.HpBiosSettings.Count} HP BIOS settings via WMI");
            }
            catch (ManagementException ex)
            {
                _consoleForm?.WriteWarning($"  HP BIOS namespace not available on {computerName}: {ex.Message}");
            }
        }

        #endregion

        #region BIOS Setting Modification (HP Only)

        // Known HP TPM setting names with their enable values, in priority order
        private static readonly (string Name, string EnableValue)[] KnownTpmSettings =
        {
            ("TPM State", "Enable"),
            ("TPM Device", "Available"),
            ("Activate TPM On Next Boot", "Enable"),
            ("Embedded Security Device", "Device available"),
            ("Embedded Security Device Availability", "Available"),
        };

        // Known HP Secure Boot setting names with their enable values, in priority order
        private static readonly (string Name, string EnableValue)[] KnownSecureBootSettings =
        {
            ("Secure Boot", "Enable"),
            ("SecureBoot", "Enable"),
            ("Configure Legacy Support and Secure Boot", "Legacy Support Disable and Secure Boot Enable"),
        };

        /// <summary>
        /// Sets a single HP BIOS setting.
        /// Uses CMSL (Set-HPBIOSSettingValue) when available; falls back to WMI HP_BIOSSettingInterface.
        /// </summary>
        public async Task<BiosSetResult> SetHpBiosSettingAsync(
            string computerName, string username, string password, string domain,
            string settingName, string newValue, string biosPassword)
        {
            if (_isCmslLoaded)
            {
                try
                {
                    return await SetHpBiosSettingViaCmslAsync(computerName, username, password, domain,
                        settingName, newValue, biosPassword);
                }
                catch (Exception cmslEx)
                {
                    _consoleForm?.WriteWarning($"  CMSL set failed ({cmslEx.Message}), falling back to WMI...");
                }
            }

            return await SetHpBiosSettingViaWmiAsync(computerName, username, password, domain,
                settingName, newValue, biosPassword);
        }

        /// <summary>
        /// Sets a HP BIOS setting using CMSL (Set-HPBIOSSettingValue) via external powershell.exe.
        /// In-process PS Core lacks CimCmdlets, so we shell out to full Windows PowerShell 5.1.
        /// For local machine: elevated via Verb="runas" (UAC prompt), output via temp file.
        /// For remote machine: non-elevated, uses PSRemoting with credentials, output via temp file.
        /// </summary>
        private async Task<BiosSetResult> SetHpBiosSettingViaCmslAsync(
            string computerName, string username, string password, string domain,
            string settingName, string newValue, string biosPassword)
        {
            return await Task.Run(() =>
            {
                var result = new BiosSetResult { SettingName = settingName, NewValue = newValue };
                string tempOutput = Path.Combine(Path.GetTempPath(), $"sa_bios_set_{Guid.NewGuid():N}.txt");
                string tempScript = Path.Combine(Path.GetTempPath(), $"sa_bios_set_{Guid.NewGuid():N}.ps1");

                try
                {
                    bool isLocal = IsLocalComputer(computerName);
                    _consoleForm?.WriteInfo($"  Setting '{settingName}' to '{newValue}' on {computerName} via CMSL (external PS)...");

                    string escapedPass = password?.Replace("'", "''") ?? string.Empty;
                    string escapedBiosPass = biosPassword?.Replace("'", "''") ?? string.Empty;
                    string escapedSettingName = settingName?.Replace("'", "''") ?? string.Empty;
                    string escapedNewValue = newValue?.Replace("'", "''") ?? string.Empty;

                    string biosPasswordParam = string.IsNullOrEmpty(biosPassword)
                        ? string.Empty
                        : $" -Password '{escapedBiosPass}'";

                    string script;
                    if (isLocal)
                    {
                        script = $@"
try {{
    Import-Module HP.ClientManagement -ErrorAction Stop
    Set-HPBIOSSettingValue -Name '{escapedSettingName}' -Value '{escapedNewValue}'{biosPasswordParam} -ErrorAction Stop
    'SUCCESS' | Out-File -FilePath '{tempOutput}' -Encoding UTF8
}} catch {{
    ""ERROR=$($_.Exception.Message)"" | Out-File -FilePath '{tempOutput}' -Encoding UTF8
}}";
                    }
                    else
                    {
                        script = $@"
try {{
    $secPass = ConvertTo-SecureString '{escapedPass}' -AsPlainText -Force
    $cred = New-Object System.Management.Automation.PSCredential('{domain}\{username}', $secPass)
    Invoke-Command -ComputerName '{computerName}' -Credential $cred -ErrorAction Stop -ScriptBlock {{
        Import-Module HP.ClientManagement -ErrorAction Stop
        Set-HPBIOSSettingValue -Name '{escapedSettingName}' -Value '{escapedNewValue}'{biosPasswordParam} -ErrorAction Stop
    }}
    'SUCCESS' | Out-File -FilePath '{tempOutput}' -Encoding UTF8
}} catch {{
    ""ERROR=$($_.Exception.Message)"" | Out-File -FilePath '{tempOutput}' -Encoding UTF8
}}";
                    }

                    File.WriteAllText(tempScript, script);

                    ProcessStartInfo psi;
                    if (isLocal)
                    {
                        _consoleForm?.WriteInfo("  Requesting elevated privileges for BIOS setting (UAC prompt may appear)...");
                        psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{tempScript}\"",
                            Verb = "runas",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                    }
                    else
                    {
                        psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{tempScript}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                    }

                    using (var proc = Process.Start(psi))
                    {
                        if (proc == null)
                            throw new Exception("Failed to start external PowerShell process for BIOS setting");

                        if (!proc.WaitForExit(60000))
                        {
                            proc.Kill();
                            throw new Exception("External PowerShell BIOS setting timed out after 60 seconds");
                        }
                    }

                    if (!File.Exists(tempOutput))
                        throw new Exception("External PowerShell produced no output (UAC may have been declined)");

                    string[] lines = File.ReadAllLines(tempOutput);
                    string output = string.Join(" ", lines).Trim();

                    if (output.StartsWith("ERROR=", StringComparison.OrdinalIgnoreCase))
                    {
                        string errorMsg = output.Substring("ERROR=".Length);
                        throw new Exception(errorMsg);
                    }

                    result.Success = true;
                    result.ReturnCode = 0;
                    _consoleForm?.WriteSuccess($"  '{settingName}' set to '{newValue}' via CMSL — reboot required to take effect");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    _consoleForm?.WriteError($"  CMSL error setting '{settingName}': {ex.Message}");
                    throw; // Re-throw so caller can fall back to WMI
                }
                finally
                {
                    try { if (File.Exists(tempScript)) File.Delete(tempScript); } catch { }
                    try { if (File.Exists(tempOutput)) File.Delete(tempOutput); } catch { }
                }

                return result;
            });
        }

        /// <summary>
        /// Sets a HP BIOS setting via the HP_BIOSSettingInterface WMI class.
        /// Used as fallback when CMSL is not available or fails.
        /// </summary>
        private async Task<BiosSetResult> SetHpBiosSettingViaWmiAsync(
            string computerName, string username, string password, string domain,
            string settingName, string newValue, string biosPassword)
        {
            return await Task.Run(() =>
            {
                var result = new BiosSetResult { SettingName = settingName, NewValue = newValue };

                try
                {
                    _consoleForm?.WriteInfo($"  Setting '{settingName}' to '{newValue}' on {computerName} via WMI...");

                    var connOptions = BuildConnectionOptions(computerName, username, password, domain);
                    var scope = new ManagementScope($"\\\\{computerName}\\root\\HP\\InstrumentedBIOS", connOptions);
                    scope.Connect();

                    using (var settingInterface = new ManagementClass(scope,
                        new ManagementPath("HP_BIOSSettingInterface"), null))
                    {
                        var inParams = settingInterface.GetMethodParameters("SetBIOSSetting");
                        inParams["Setting"] = settingName;
                        inParams["Value"] = string.IsNullOrEmpty(biosPassword)
                            ? newValue
                            : $"{newValue},<utf-16/>{biosPassword}";

                        var outParams = settingInterface.InvokeMethod("SetBIOSSetting", inParams, null);
                        int returnCode = Convert.ToInt32(outParams["return"]);

                        result.ReturnCode = returnCode;
                        result.Success = returnCode == 0;

                        if (result.Success)
                            _consoleForm?.WriteSuccess($"  '{settingName}' set to '{newValue}' via WMI — reboot required to take effect");
                        else
                        {
                            result.ErrorMessage = GetHpReturnCodeMessage(returnCode);
                            _consoleForm?.WriteError($"  Failed to set '{settingName}': {result.ErrorMessage}");
                        }
                    }
                }
                catch (ManagementException ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"WMI error: {ex.Message}";
                    _consoleForm?.WriteError($"  WMI error setting '{settingName}': {ex.Message}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    _consoleForm?.WriteError($"  Error setting '{settingName}': {ex.Message}");
                }

                return result;
            });
        }

        /// <summary>
        /// Enables TPM on an HP machine by setting all known TPM-related BIOS settings.
        /// Searches the machine's queried settings to find the correct setting names.
        /// </summary>
        public async Task<List<BiosSetResult>> EnableTpmAsync(
            string computerName, string username, string password, string domain,
            string biosPassword, List<BiosSetting> currentSettings)
        {
            var results = new List<BiosSetResult>();

            foreach (var (name, enableValue) in KnownTpmSettings)
            {
                var match = currentSettings.FirstOrDefault(s =>
                    s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (match != null && !match.CurrentValue.Equals(enableValue, StringComparison.OrdinalIgnoreCase))
                {
                    var r = await SetHpBiosSettingAsync(computerName, username, password, domain,
                        name, enableValue, biosPassword);
                    results.Add(r);
                }
            }

            if (results.Count == 0)
            {
                _consoleForm?.WriteWarning("No known TPM settings found in queried data, trying 'TPM State'...");
                var r = await SetHpBiosSettingAsync(computerName, username, password, domain,
                    "TPM State", "Enable", biosPassword);
                results.Add(r);
            }

            return results;
        }

        /// <summary>
        /// Enables Secure Boot on an HP machine by setting the appropriate BIOS setting.
        /// Searches the machine's queried settings to find the correct setting name.
        /// </summary>
        public async Task<List<BiosSetResult>> EnableSecureBootAsync(
            string computerName, string username, string password, string domain,
            string biosPassword, List<BiosSetting> currentSettings)
        {
            var results = new List<BiosSetResult>();

            foreach (var (name, enableValue) in KnownSecureBootSettings)
            {
                var match = currentSettings.FirstOrDefault(s =>
                    s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (match != null && !match.CurrentValue.Equals(enableValue, StringComparison.OrdinalIgnoreCase))
                {
                    var r = await SetHpBiosSettingAsync(computerName, username, password, domain,
                        name, enableValue, biosPassword);
                    results.Add(r);
                    break; // Only need to set one Secure Boot setting
                }
            }

            if (results.Count == 0)
            {
                _consoleForm?.WriteWarning("No known Secure Boot settings found in queried data, trying 'Secure Boot'...");
                var r = await SetHpBiosSettingAsync(computerName, username, password, domain,
                    "Secure Boot", "Enable", biosPassword);
                results.Add(r);
            }

            return results;
        }

        private static string GetHpReturnCodeMessage(int code)
        {
            switch (code)
            {
                case 0: return "Success";
                case 1: return "Not Supported";
                case 2: return "Unspecified Error";
                case 3: return "Timeout";
                case 4: return "Failed \u2014 verify BIOS password is correct";
                case 5: return "Invalid Parameter";
                case 6: return "Access Denied \u2014 BIOS password may be required";
                default: return $"Unknown error (code {code})";
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

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _psRunspace?.Dispose();
                _psRunspace = null;
                _disposed = true;
            }
        }

        #endregion
    }
}
