using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA_ToolBelt
{
    public class Linux_Service
    {
        private readonly ConsoleForm _consoleForm;

        public Linux_Service(ConsoleForm consoleForm = null)
        {
            _consoleForm = consoleForm;
        }

        #region Data Classes Shit

        /// <summary>
        /// Disk information structure
        /// </summary>
        public class DiskInfo
        {
            public string Filesystem { get; set; }
            public string Size { get; set; }
            public string Used { get; set; }
            public string Avail { get; set; }
            public string UsePercent { get; set; }
            public string MountedOn { get; set; }
        }

        /// <summary>
        /// Replication status structure
        /// </summary>
        public class ReplicationStatus
        {
            public string AgreementName { get; set; }
            public string LastUpdateStarted { get; set; }
            public string LastUpdateEnded { get; set; }
            public string LastUpdateStatus { get; set; }
            public string ConsumerReplica { get; set; }
            public TimeSpan? UpdateDuration { get; set; }
            public bool IsHealthy { get; set; }
        }

        #endregion

        #region SSH Connection Shit

        /// <summary>
        /// Execute SSH command using plink with automatic handling of interactive prompts
        /// </summary>
        /// <param name="hostname">Target server hostname</param>
        /// <param name="username">SSH username</param>
        /// <param name="password">SSH password</param>
        /// <param name="command">Command to execute (optional - if null, just establishes connection)</param>
        /// <returns>Command output or connection status</returns>
        public async Task<string> ExecuteSSHCommandAsync(string hostname, string username, string password, string command = null)
        {
            try
            {
                _consoleForm?.WriteInfo($"Connecting to {hostname} via SSH...");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "plink.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                // Build plink arguments - KISS approach
                var args = new StringBuilder();
                args.Append($"-batch {username}@{hostname}");
                args.Append($" -pw {password}");

                if (!string.IsNullOrEmpty(command))
                {
                    args.Append($" \"{command}\"");
                }

                processInfo.Arguments = args.ToString();

                _consoleForm?.WriteInfo($"Executing: plink {args.ToString().Replace($"-pw {password}", "-pw ***")}");

                using (var process = new Process { StartInfo = processInfo })
                {
                    var output = new StringBuilder();
                    var error = new StringBuilder();

                    // Handle output data
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    // Handle error data
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // No need to send return key with -batch mode
                    // -batch handles the interactive prompt automatically

                    // Wait for process to complete with timeout
                    var completed = await Task.Run(() => process.WaitForExit(30000)); // 30 second timeout

                    if (!completed)
                    {
                        process.Kill();
                        throw new TimeoutException("SSH connection timed out after 30 seconds");
                    }

                    string result = output.ToString();
                    string errorOutput = error.ToString();

                    if (process.ExitCode == 0)
                    {
                        _consoleForm?.WriteSuccess($"SSH command completed successfully on {hostname}");
                        return result;
                    }
                    else
                    {
                        string errorMsg = $"SSH command failed with exit code {process.ExitCode}. Error: {errorOutput}";
                        _consoleForm?.WriteError(errorMsg);
                        throw new Exception(errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"SSH connection error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Test SSH connection to a server
        /// </summary>
        public async Task<bool> TestSSHConnectionAsync(string hostname, string username, string password)
        {
            try
            {
                _consoleForm?.WriteInfo($"Testing SSH connection to {hostname}...");

                string result = await ExecuteSSHCommandAsync(hostname, username, password, "echo 'Connection test successful'");

                bool success = result.Contains("Connection test successful");
                if (success)
                {
                    _consoleForm?.WriteSuccess($"SSH connection to {hostname} successful");
                }
                else
                {
                    _consoleForm?.WriteWarning($"SSH connection to {hostname} established but response unexpected");
                }

                return success;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"SSH connection test failed for {hostname}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute multiple SSH commands in sequence
        /// </summary>
        public async Task<Dictionary<string, string>> ExecuteMultipleSSHCommandsAsync(string hostname, string username, string password, params string[] commands)
        {
            var results = new Dictionary<string, string>();

            foreach (string command in commands)
            {
                try
                {
                    _consoleForm?.WriteInfo($"Executing command: {command}");
                    string result = await ExecuteSSHCommandAsync(hostname, username, password, command);
                    results[command] = result;
                }
                catch (Exception ex)
                {
                    results[command] = $"ERROR: {ex.Message}";
                    _consoleForm?.WriteError($"Command '{command}' failed: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Check if plink is available in the system
        /// </summary>
        public bool IsPlinkAvailable()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "plink.exe";
                    process.StartInfo.Arguments = "-V";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    process.WaitForExit(5000);

                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region HDD Shit

        /// <summary>
        /// Get disk information from a Linux server
        /// </summary>
        /// <param name="hostname">Target server hostname</param>
        /// <param name="username">SSH username</param>
        /// <param name="password">SSH password</param>
        /// <returns>List of disk information</returns>
        public async Task<List<DiskInfo>> GetDiskInfoAsync(string hostname, string username, string password)
        {
            try
            {
                _consoleForm?.WriteInfo($"Getting disk information from {hostname}...");

                // Execute df command to get disk usage
                string command = "df -h";
                string output = await ExecuteSSHCommandAsync(hostname, username, password, command);

                var diskInfoList = ParseDiskInfo(output);

                _consoleForm?.WriteSuccess($"Retrieved disk information for {diskInfoList.Count} filesystems from {hostname}");
                return diskInfoList;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to get disk info from {hostname}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Parse df command output into DiskInfo objects
        /// </summary>
        private List<DiskInfo> ParseDiskInfo(string dfOutput)
        {
            var diskInfoList = new List<DiskInfo>();

            if (string.IsNullOrWhiteSpace(dfOutput))
            {
                return diskInfoList;
            }

            var lines = dfOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Skip the header line (Filesystem Size Used Avail Use% Mounted on)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Skip unwanted entries
                if (line.Contains("Filesystem") || line.Contains("/dev/") || line.Contains("tmpfs"))
                    continue;

                try
                {
                    // Split the line into columns (handle multiple spaces)
                    var parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 6)
                    {
                        var diskInfo = new DiskInfo
                        {
                            Filesystem = parts[0],
                            Size = parts[1],
                            Used = parts[2],
                            Avail = parts[3],
                            UsePercent = parts[4],
                            MountedOn = parts[5]
                        };

                        // Handle cases where mount point might have spaces (rejoin remaining parts)
                        if (parts.Length > 6)
                        {
                            diskInfo.MountedOn = string.Join(" ", parts.Skip(5));
                        }

                        diskInfoList.Add(diskInfo);
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteWarning($"Failed to parse disk info line: {line}. Error: {ex.Message}");
                }
            }

            return diskInfoList;
        }

        #endregion

        #region System Info Shit

        /// <summary>
        /// Get system uptime from a Linux server
        /// </summary>
        public async Task<string> GetSystemUptimeAsync(string hostname, string username, string password)
        {
            try
            {
                string output = await ExecuteSSHCommandAsync(hostname, username, password, "uptime");
                return output.Trim();
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to get uptime from {hostname}: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get memory information from a Linux server
        /// </summary>
        public async Task<string> GetMemoryInfoAsync(string hostname, string username, string password)
        {
            try
            {
                string output = await ExecuteSSHCommandAsync(hostname, username, password, "free -h");
                return output.Trim();
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to get memory info from {hostname}: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion

        #region Replication Health Shit

        /// <summary>
        /// Get LDAP replication health status from a server
        /// </summary>
        public async Task<List<ReplicationStatus>> GetReplicationHealthAsync(string hostname, string username, string password)
        {
            List<ReplicationStatus> replStatuses = new List<ReplicationStatus>();

            try
            {
                _consoleForm?.WriteInfo($"Checking replication health on {hostname}...");

                // Using dsconf to check replication status
                string command = "dsconf -D 'cn=Directory Manager' ldap://localhost:389 replication monitor";

                string output = await ExecuteSSHCommandAsync(hostname, username, password, command);

                if (string.IsNullOrWhiteSpace(output))
                {
                    _consoleForm?.WriteError($"No replication status output from {hostname}");
                    return replStatuses;
                }

                ReplicationStatus status = new ReplicationStatus
                {
                    AgreementName = hostname
                };

                string[] lines = output.Split('\n');
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("Last Update Start:"))
                    {
                        status.LastUpdateStarted = trimmedLine.Replace("Last Update Start:", "").Trim();
                    }
                    else if (trimmedLine.StartsWith("Last Update End:"))
                    {
                        status.LastUpdateEnded = trimmedLine.Replace("Last Update End:", "").Trim();
                    }
                    else if (trimmedLine.StartsWith("Last Update Status:"))
                    {
                        status.LastUpdateStatus = trimmedLine.Replace("Last Update Status:", "").Trim();
                        status.IsHealthy = status.LastUpdateStatus.Contains("0") ||
                                         status.LastUpdateStatus.ToLower().Contains("success");
                    }
                }

                // Log the results
                _consoleForm?.WriteInfo($"Server: {hostname}");
                _consoleForm?.WriteInfo($"Last Update Started: {status.LastUpdateStarted}");
                _consoleForm?.WriteInfo($"Last Update Ended: {status.LastUpdateEnded}");
                _consoleForm?.WriteInfo($"Status: {(status.IsHealthy ? "Healthy" : "Unhealthy")}");
                _consoleForm?.WriteInfo("-------------------");

                replStatuses.Add(status);
                return replStatuses;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting replication status from {hostname}: {ex.Message}");
                return replStatuses;
            }
        }

        #endregion
    }
}