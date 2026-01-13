using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SA_ToolBelt
{
    public class Linux_Service
    {
        private readonly ConsoleForm _consoleForm;

        // Windows API for setting window focus
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

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
        /// Execute an interactive SSH command that prompts for input
        /// </summary>
        /// <param name="hostname">Target server hostname</param>
        /// <param name="username">SSH username</param>
        /// <param name="password">SSH password</param>
        /// <param name="command">Command to execute</param>
        /// <param name="inputs">Array of inputs to send when prompted (e.g., bind DN, passwords)</param>
        /// <returns>Command output</returns>
        public async Task<string> ExecuteInteractiveSSHCommandAsync(string hostname, string username, string password, string command, string[] inputs = null)
        {
            try
            {
                _consoleForm?.WriteInfo($"Connecting to {hostname} via SSH for interactive command...");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k plink.exe {username}@{hostname} -pw {password}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false, // Keep it visible so SendKeys works
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                var process = Process.Start(processInfo);

                var output = new StringBuilder();
                var error = new StringBuilder();

                // Start reading output asynchronously
                var outputTask = Task.Run(async () =>
                {
                    using (var reader = process.StandardOutput)
                    {
                        char[] buffer = new char[1024];
                        int bytesRead;
                        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Append(buffer, 0, bytesRead);
                        }
                    }
                });

                var errorTask = Task.Run(async () =>
                {
                    using (var reader = process.StandardError)
                    {
                        char[] buffer = new char[1024];
                        int bytesRead;
                        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            error.Append(buffer, 0, bytesRead);
                        }
                    }
                });

                // Wait for SSH connection and "Access Granted" prompt
                _consoleForm?.WriteInfo("Waiting for SSH connection...");
                await Task.Delay(12000); // 12 seconds for SSH connection

                // Bring window to foreground for SendKeys
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(process.MainWindowHandle);
                }
                await Task.Delay(200);

                // Send Enter to bypass "Access Granted. Press Return to begin session" prompt
                _consoleForm?.WriteInfo("Bypassing Access Granted prompt...");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(2000); // Wait for shell prompt

                // Send the dsconf command and press Enter
                string sanitizedCommand = command.Replace(password, "***");
                _consoleForm?.WriteInfo($"Sending command: {sanitizedCommand}");
                System.Windows.Forms.SendKeys.SendWait(command);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(1000); // Wait for first Bind DN prompt

                // Send credentials (twice - once for each server) using SendKeys
                if (inputs != null && inputs.Length >= 4)
                {
                    // First server credentials
                    _consoleForm?.WriteInfo("Sending first Bind DN...");
                    System.Windows.Forms.SendKeys.SendWait(inputs[0]); // cn=Directory Manager
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    await Task.Delay(1000);

                    _consoleForm?.WriteInfo("Sending first password...");
                    System.Windows.Forms.SendKeys.SendWait(inputs[1]); // password
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    await Task.Delay(1000);

                    // Second server credentials
                    _consoleForm?.WriteInfo("Sending second Bind DN...");
                    System.Windows.Forms.SendKeys.SendWait(inputs[2]); // cn=Directory Manager
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    await Task.Delay(1000);

                    _consoleForm?.WriteInfo("Sending second password...");
                    System.Windows.Forms.SendKeys.SendWait(inputs[3]); // password
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                }

                // Wait for replication monitor to gather data from both servers
                _consoleForm?.WriteInfo("Waiting for replication data...");
                await Task.Delay(7000); // 7 seconds for data gathering

                // Send exit command using SendKeys
                System.Windows.Forms.SendKeys.SendWait("exit");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");

                // Wait for process to complete
                var completed = await Task.Run(() => process.WaitForExit(10000)); // 10 second timeout

                if (!completed)
                {
                    process.Kill();
                    _consoleForm?.WriteWarning("Process killed due to timeout");
                }

                // Wait for output reading tasks to complete
                await Task.WhenAll(outputTask, errorTask);

                string result = output.ToString();
                _consoleForm?.WriteInfo($"Command completed. Output length: {result.Length} characters");

                return result;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Interactive SSH command error: {ex.Message}");
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

        #region Home Directory Management

        /// <summary>
        /// Create a home directory for a user on a Linux server with proper permissions
        /// </summary>
        /// <param name="hostname">Target server hostname</param>
        /// <param name="username">SSH username</param>
        /// <param name="password">SSH password</param>
        /// <param name="ntUserId">The NT User ID (directory name)</param>
        /// <returns>True if directory was created successfully, false otherwise</returns>
        public async Task<bool> CreateHomeDirectoryAsync(string hostname, string username, string password, string ntUserId)
        {
            try
            {
                _consoleForm?.WriteInfo($"Creating home directory for user: {ntUserId}");
                _consoleForm?.WriteInfo($"SSH Connection - Host: {hostname}, User: {username}");

                // Directory path where we'll create the user's home directory
                string directoryPath = $"/net/cce-data/home/{ntUserId}";

                _consoleForm?.WriteInfo($"Target directory: {directoryPath}");

                // First, test if user has sudo privileges
                _consoleForm?.WriteInfo($"Testing sudo access for user '{username}'...");
                string sudoTestCommand = $"echo '{password}' | sudo -S -v 2>&1";
                string sudoTestOutput = string.Empty;

                try
                {
                    sudoTestOutput = await ExecuteSSHCommandAsync(hostname, username, password, sudoTestCommand);
                    _consoleForm?.WriteInfo($"Sudo test output: {sudoTestOutput.Trim()}");
                }
                catch (Exception sudoEx)
                {
                    // Sudo test failed - extract error details from exception message
                    _consoleForm?.WriteError($"Sudo test failed: {sudoEx.Message}");

                    // Try to get error details from the exception message
                    if (sudoEx.Message.Contains("Error:"))
                    {
                        int errorIndex = sudoEx.Message.IndexOf("Error:");
                        sudoTestOutput = sudoEx.Message.Substring(errorIndex + 7).Trim();
                        _consoleForm?.WriteInfo($"Sudo error output: {sudoTestOutput}");
                    }

                    // Check for common sudo issues in exception message
                    if (sudoEx.Message.Contains("Sorry, try again") ||
                        sudoEx.Message.Contains("authentication failure") ||
                        sudoEx.Message.Contains("incorrect password"))
                    {
                        _consoleForm?.WriteError($"PERMISSION DENIED: Sudo password is incorrect.");
                        _consoleForm?.WriteError($"The password provided does not work for sudo on {hostname}");
                        return false;
                    }
                    else if (sudoEx.Message.Contains("not in the sudoers file") ||
                             sudoEx.Message.Contains("is not allowed to"))
                    {
                        _consoleForm?.WriteError($"PERMISSION DENIED: User '{username}' is not in the sudoers file.");
                        _consoleForm?.WriteError($"The user '{username}' does not have sudo privileges on {hostname}");
                        _consoleForm?.WriteError($"Contact your system administrator to grant sudo access.");
                        return false;
                    }
                    else if (sudoEx.Message.Contains("command not found"))
                    {
                        _consoleForm?.WriteError($"ERROR: 'sudo' command not found on {hostname}");
                        _consoleForm?.WriteError($"The target server may not have sudo installed.");
                        return false;
                    }
                    else
                    {
                        _consoleForm?.WriteError($"SUDO TEST FAILED: {sudoEx.Message}");
                        _consoleForm?.WriteError($"Unable to verify sudo access for user '{username}'");
                        return false;
                    }
                }

                // Check for common sudo issues in successful output (sometimes sudo succeeds but warns)
                if (sudoTestOutput.Contains("Sorry, try again") || sudoTestOutput.Contains("authentication failure"))
                {
                    _consoleForm?.WriteError($"PERMISSION DENIED: Sudo password is incorrect.");
                    _consoleForm?.WriteError($"The password provided does not have sudo privileges on {hostname}");
                    return false;
                }
                else if (sudoTestOutput.Contains("not in the sudoers file"))
                {
                    _consoleForm?.WriteError($"PERMISSION DENIED: User '{username}' is not in the sudoers file.");
                    _consoleForm?.WriteError($"The user '{username}' does not have sudo privileges on {hostname}");
                    _consoleForm?.WriteError($"Contact your system administrator to grant sudo access.");
                    return false;
                }
                else if (sudoTestOutput.Contains("command not found"))
                {
                    _consoleForm?.WriteError($"ERROR: 'sudo' command not found on {hostname}");
                    _consoleForm?.WriteError($"The target server may not have sudo installed.");
                    return false;
                }

                _consoleForm?.WriteSuccess($"Sudo access verified for user '{username}'");

                // Build command to:
                // 1. Check if directory already exists
                // 2. Create directory if it doesn't exist
                // 3. Set chmod to 755 (rwxr-xr-x)
                // 4. Set ownership to ntUserId:share_Group
                // Note: Using 'echo password | sudo -S' to provide password to sudo
                string command = $"if [ ! -d '{directoryPath}' ]; then " +
                                $"echo '{password}' | sudo -S mkdir -p '{directoryPath}' 2>&1 && " +
                                $"echo '{password}' | sudo -S chmod 755 '{directoryPath}' 2>&1 && " +
                                $"echo '{password}' | sudo -S chown {ntUserId}:share_Group '{directoryPath}' 2>&1 && " +
                                $"echo 'Directory created successfully' || echo 'Failed to create directory'; " +
                                $"else echo 'Directory already exists'; fi";

                _consoleForm?.WriteInfo($"Executing directory creation command...");

                string output = string.Empty;

                try
                {
                    output = await ExecuteSSHCommandAsync(hostname, username, password, command);

                    // Log the raw output for debugging
                    _consoleForm?.WriteInfo($"Command output: {output.Trim()}");

                    // Check the output for success indicators
                    bool success = output.Contains("Directory created successfully") ||
                                  output.Contains("Directory already exists");

                    if (success)
                    {
                        if (output.Contains("Directory already exists"))
                        {
                            _consoleForm?.WriteWarning($"Home directory {directoryPath} already exists on {hostname}");
                        }
                        else
                        {
                            _consoleForm?.WriteSuccess($"Home directory created successfully: {directoryPath}");
                            _consoleForm?.WriteSuccess($"Permissions set to: 755 (rwxr-xr-x)");
                            _consoleForm?.WriteSuccess($"Ownership set to: {ntUserId}:share_Group");
                        }

                        // Verify the directory and permissions
                        await VerifyHomeDirectoryAsync(hostname, username, password, ntUserId);

                        return true;
                    }
                    else
                    {
                        // Provide detailed error analysis
                        _consoleForm?.WriteError($"=== HOME DIRECTORY CREATION FAILED ===");
                        _consoleForm?.WriteError($"Raw output from server: {output.Trim()}");

                        AnalyzeDirectoryCreationError(output, hostname, username, ntUserId);

                        return false;
                    }
                }
                catch (Exception cmdEx)
                {
                    // Directory creation command threw an exception (exit code != 0)
                    _consoleForm?.WriteError($"=== HOME DIRECTORY CREATION FAILED ===");
                    _consoleForm?.WriteError($"Command execution failed: {cmdEx.Message}");

                    // Try to extract error output from exception message
                    string errorOutput = string.Empty;
                    if (cmdEx.Message.Contains("Error:"))
                    {
                        int errorIndex = cmdEx.Message.IndexOf("Error:");
                        errorOutput = cmdEx.Message.Substring(errorIndex + 7).Trim();
                        _consoleForm?.WriteError($"Error details: {errorOutput}");
                    }

                    // Analyze the error
                    AnalyzeDirectoryCreationError(cmdEx.Message + " " + errorOutput, hostname, username, ntUserId);

                    return false;
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"=== EXCEPTION DURING HOME DIRECTORY CREATION ===");
                _consoleForm?.WriteError($"User: {ntUserId}");
                _consoleForm?.WriteError($"Exception Type: {ex.GetType().Name}");
                _consoleForm?.WriteError($"Error Message: {ex.Message}");
                _consoleForm?.WriteError($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Analyze directory creation errors and provide detailed feedback
        /// </summary>
        /// <param name="errorMessage">The error message or output to analyze</param>
        /// <param name="hostname">Target server hostname</param>
        /// <param name="username">SSH username</param>
        /// <param name="ntUserId">The NT User ID (directory name)</param>
        private void AnalyzeDirectoryCreationError(string errorMessage, string hostname, string username, string ntUserId)
        {
            // Check for specific error patterns
            if (errorMessage.Contains("Permission denied") || errorMessage.Contains("permission denied"))
            {
                _consoleForm?.WriteError($"CAUSE: Permission denied");
                _consoleForm?.WriteError($"Possible reasons:");
                _consoleForm?.WriteError($"  1. User '{username}' lacks sudo privileges for mkdir/chmod/chown");
                _consoleForm?.WriteError($"  2. The parent directory '/net/cce-data/home' doesn't exist");
                _consoleForm?.WriteError($"  3. The parent directory '/net/cce-data/home' has restricted permissions");
                _consoleForm?.WriteError($"  4. The filesystem is mounted read-only");
                _consoleForm?.WriteError($"  5. SELinux or AppArmor is blocking the operation");
            }
            else if (errorMessage.Contains("No such file or directory"))
            {
                _consoleForm?.WriteError($"CAUSE: Directory path does not exist");
                _consoleForm?.WriteError($"The parent directory '/net/cce-data/home' may not exist on {hostname}");
                _consoleForm?.WriteError($"Action: Manually verify the path exists: ls -ld /net/cce-data/home");
            }
            else if (errorMessage.Contains("Unknown user") || errorMessage.Contains("invalid user"))
            {
                _consoleForm?.WriteError($"CAUSE: User '{ntUserId}' does not exist on the Linux system");
                _consoleForm?.WriteError($"The chown command requires that user '{ntUserId}' exists on {hostname}");
                _consoleForm?.WriteError($"Action: Create the user first or adjust the chown command");
            }
            else if (errorMessage.Contains("group") && (errorMessage.Contains("invalid") || errorMessage.Contains("not found")))
            {
                _consoleForm?.WriteError($"CAUSE: Group 'share_Group' does not exist on the Linux system");
                _consoleForm?.WriteError($"The chown command requires that group 'share_Group' exists on {hostname}");
                _consoleForm?.WriteError($"Action: Create the group or use a different group name");
            }
            else if (errorMessage.Contains("Read-only file system"))
            {
                _consoleForm?.WriteError($"CAUSE: Filesystem is mounted read-only");
                _consoleForm?.WriteError($"The directory '/net/cce-data/home' is on a read-only filesystem");
                _consoleForm?.WriteError($"Action: Remount the filesystem as read-write");
            }
            else if (errorMessage.Contains("Disk quota exceeded") || errorMessage.Contains("No space left"))
            {
                _consoleForm?.WriteError($"CAUSE: Insufficient disk space or quota exceeded");
                _consoleForm?.WriteError($"The server {hostname} has run out of disk space or hit a quota limit");
                _consoleForm?.WriteError($"Action: Free up disk space or increase quota");
            }
            else if (errorMessage.Contains("exit code 1") || errorMessage.Contains("failed with exit code"))
            {
                _consoleForm?.WriteError($"CAUSE: Command failed with non-zero exit code");
                _consoleForm?.WriteError($"One or more commands (mkdir, chmod, or chown) failed");
                _consoleForm?.WriteError($"Action: Check the full error message above for specific details");
            }
            else
            {
                _consoleForm?.WriteError($"CAUSE: Unknown error");
                _consoleForm?.WriteError($"Full error message: {errorMessage}");
                _consoleForm?.WriteError($"Action: Review the error message and contact system administrator if needed");
            }
        }

        /// <summary>
        /// Verify that a home directory exists with correct permissions
        /// </summary>
        /// <param name="hostname">Target server hostname</param>
        /// <param name="username">SSH username</param>
        /// <param name="password">SSH password</param>
        /// <param name="ntUserId">The NT User ID (directory name)</param>
        /// <returns>Verification output</returns>
        private async Task<string> VerifyHomeDirectoryAsync(string hostname, string username, string password, string ntUserId)
        {
            try
            {
                string directoryPath = $"/net/cce-data/home/{ntUserId}";

                _consoleForm?.WriteInfo($"Verifying home directory: {directoryPath}");

                // Use ls -ld to show directory details (permissions, owner, group)
                string verifyCommand = $"ls -ld '{directoryPath}'";

                string output = await ExecuteSSHCommandAsync(hostname, username, password, verifyCommand);

                _consoleForm?.WriteInfo($"Directory details: {output.Trim()}");

                return output;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"Could not verify directory: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion
    }
}