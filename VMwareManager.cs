using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace SA_ToolBelt
{
    /// <summary>
    /// Manages VMware vCenter operations using PowerCLI
    /// </summary>
    public class VMwareManager
    {
        private readonly string _vCenterServer;
        private readonly string _username;
        private readonly string _password;
        private readonly ConsoleForm _console;
        private Runspace _runspace;

        public VMwareManager(string vCenterServer, string username, string password, ConsoleForm console)
        {
            _vCenterServer = vCenterServer;
            _username = username;
            _password = password;
            _console = console;
        }

        /// <summary>
        /// Initialize PowerCLI modules and connect to vCenter
        /// </summary>
        public async Task InitializePowerCLIAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Create runspace
                    _runspace = RunspaceFactory.CreateRunspace();
                    _runspace.Open();

                    // Import VMware PowerCLI module
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;

                        // Set execution policy for the session
                        ps.AddScript("Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force");
                        ps.Invoke();
                        ps.Commands.Clear();

                        // Import VMware.PowerCLI module
                        _console?.WriteInfo("Importing VMware.PowerCLI module...");
                        ps.AddScript("Import-Module VMware.PowerCLI -ErrorAction Stop");
                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                            {
                                _console?.WriteError($"PowerCLI import error: {error}");
                            }
                            throw new Exception("Failed to import VMware.PowerCLI module");
                        }

                        ps.Commands.Clear();

                        // Disable certificate warnings
                        ps.AddScript("Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false -Scope Session");
                        ps.Invoke();
                        ps.Commands.Clear();

                        // Connect to vCenter
                        _console?.WriteInfo($"Connecting to vCenter: {_vCenterServer}...");
                        ps.AddCommand("Connect-VIServer");
                        ps.AddParameter("Server", _vCenterServer);
                        ps.AddParameter("User", _username);
                        ps.AddParameter("Password", _password);
                        ps.AddParameter("Force", true);

                        var connectResults = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                            {
                                _console?.WriteError($"vCenter connection error: {error}");
                            }
                            throw new Exception($"Failed to connect to vCenter: {_vCenterServer}");
                        }

                        _console?.WriteSuccess($"Connected to vCenter: {_vCenterServer}");
                    }
                }
                catch (Exception ex)
                {
                    _console?.WriteError($"PowerCLI initialization failed: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Get detailed ESXi host information
        /// </summary>
        public async Task<List<ESXiHostInfo>> GetESXiHostsDetailedAsync()
        {
            return await Task.Run(() =>
            {
                var hosts = new List<ESXiHostInfo>();

                try
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;

                        // Get all VMHosts with detailed properties
                        string script = @"
                            Get-VMHost | Select-Object @{N='Name';E={$_.Name}},
                                @{N='State';E={$_.ConnectionState}},
                                @{N='PowerState';E={$_.PowerState}},
                                @{N='Cluster';E={$_.Parent.Name}},
                                @{N='CpuUsageMhz';E={$_.CpuUsageMhz}},
                                @{N='CpuTotalMhz';E={$_.CpuTotalMhz}},
                                @{N='MemoryUsageGB';E={[math]::Round($_.MemoryUsageGB, 2)}},
                                @{N='MemoryTotalGB';E={[math]::Round($_.MemoryTotalGB, 2)}},
                                @{N='HAState';E={(Get-VMHost $_).ExtensionData.Runtime.DasHostState.State}},
                                @{N='Uptime';E={[math]::Round($_.ExtensionData.Summary.QuickStats.Uptime / 86400, 1)}}
                        ";

                        ps.AddScript(script);
                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                            {
                                _console?.WriteError($"ESXi query error: {error}");
                            }
                            throw new Exception("Failed to retrieve ESXi host information");
                        }

                        foreach (PSObject result in results)
                        {
                            var host = new ESXiHostInfo
                            {
                                Name = result.Properties["Name"]?.Value?.ToString(),
                                State = result.Properties["State"]?.Value?.ToString(),
                                PowerState = result.Properties["PowerState"]?.Value?.ToString(),
                                Cluster = result.Properties["Cluster"]?.Value?.ToString(),
                                CpuUsageMhz = Convert.ToInt32(result.Properties["CpuUsageMhz"]?.Value ?? 0),
                                CpuTotalMhz = Convert.ToInt32(result.Properties["CpuTotalMhz"]?.Value ?? 0),
                                MemoryUsageGB = Convert.ToDouble(result.Properties["MemoryUsageGB"]?.Value ?? 0),
                                MemoryTotalGB = Convert.ToDouble(result.Properties["MemoryTotalGB"]?.Value ?? 0),
                                HAState = result.Properties["HAState"]?.Value?.ToString(),
                                UptimeDays = Convert.ToDouble(result.Properties["Uptime"]?.Value ?? 0)
                            };

                            hosts.Add(host);
                        }

                        _console?.WriteSuccess($"Retrieved {hosts.Count} ESXi hosts");
                    }
                }
                catch (Exception ex)
                {
                    _console?.WriteError($"Failed to get ESXi hosts: {ex.Message}");
                    throw;
                }

                return hosts;
            });
        }

        /// <summary>
        /// Get detailed virtual machine information
        /// </summary>
        public async Task<List<VMInfo>> GetVirtualMachinesDetailedAsync()
        {
            return await Task.Run(() =>
            {
                var vms = new List<VMInfo>();

                try
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;

                        // Get all VMs with detailed properties
                        string script = @"
                            Get-VM | Select-Object @{N='Name';E={$_.Name}},
                                @{N='PowerState';E={$_.PowerState}},
                                @{N='NumCpu';E={$_.NumCpu}},
                                @{N='MemoryGB';E={$_.MemoryGB}},
                                @{N='UsedSpaceGB';E={[math]::Round($_.UsedSpaceGB, 2)}},
                                @{N='ProvisionedSpaceGB';E={[math]::Round($_.ProvisionedSpaceGB, 2)}},
                                @{N='VMHost';E={$_.VMHost.Name}},
                                @{N='GuestOS';E={$_.Guest.OSFullName}},
                                @{N='ToolsStatus';E={$_.ExtensionData.Guest.ToolsStatus}},
                                @{N='UptimeDays';E={if ($_.Uptime) {[math]::Round($_.Uptime.TotalDays, 1)} else {0}}}
                        ";

                        ps.AddScript(script);
                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            foreach (var error in ps.Streams.Error)
                            {
                                _console?.WriteError($"VM query error: {error}");
                            }
                            throw new Exception("Failed to retrieve virtual machine information");
                        }

                        foreach (PSObject result in results)
                        {
                            var vm = new VMInfo
                            {
                                Name = result.Properties["Name"]?.Value?.ToString(),
                                PowerState = result.Properties["PowerState"]?.Value?.ToString(),
                                NumCpu = Convert.ToInt32(result.Properties["NumCpu"]?.Value ?? 0),
                                MemoryGB = Convert.ToDouble(result.Properties["MemoryGB"]?.Value ?? 0),
                                UsedSpaceGB = Convert.ToDouble(result.Properties["UsedSpaceGB"]?.Value ?? 0),
                                ProvisionedSpaceGB = Convert.ToDouble(result.Properties["ProvisionedSpaceGB"]?.Value ?? 0),
                                VMHost = result.Properties["VMHost"]?.Value?.ToString(),
                                GuestOS = result.Properties["GuestOS"]?.Value?.ToString(),
                                ToolsStatus = result.Properties["ToolsStatus"]?.Value?.ToString(),
                                UptimeDays = Convert.ToDouble(result.Properties["UptimeDays"]?.Value ?? 0)
                            };

                            vms.Add(vm);
                        }

                        _console?.WriteSuccess($"Retrieved {vms.Count} virtual machines");
                    }
                }
                catch (Exception ex)
                {
                    _console?.WriteError($"Failed to get VMs: {ex.Message}");
                    throw;
                }

                return vms;
            });
        }

        /// <summary>
        /// Disconnect from vCenter and clean up
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_runspace != null)
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;
                        ps.AddCommand("Disconnect-VIServer");
                        ps.AddParameter("Confirm", false);
                        ps.Invoke();
                    }

                    _runspace.Close();
                    _runspace.Dispose();
                    _runspace = null;
                }
            }
            catch (Exception ex)
            {
                _console?.WriteError($"Error disconnecting from vCenter: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// ESXi host information
    /// </summary>
    public class ESXiHostInfo
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string PowerState { get; set; }
        public string Cluster { get; set; }
        public int CpuUsageMhz { get; set; }
        public int CpuTotalMhz { get; set; }
        public double MemoryUsageGB { get; set; }
        public double MemoryTotalGB { get; set; }
        public string HAState { get; set; }
        public double UptimeDays { get; set; }

        public string CpuUsagePercent => CpuTotalMhz > 0
            ? $"{Math.Round((double)CpuUsageMhz / CpuTotalMhz * 100, 1)}%"
            : "0%";

        public string MemoryUsagePercent => MemoryTotalGB > 0
            ? $"{Math.Round(MemoryUsageGB / MemoryTotalGB * 100, 1)}%"
            : "0%";
    }

    /// <summary>
    /// Virtual machine information
    /// </summary>
    public class VMInfo
    {
        public string Name { get; set; }
        public string PowerState { get; set; }
        public int NumCpu { get; set; }
        public double MemoryGB { get; set; }
        public double UsedSpaceGB { get; set; }
        public double ProvisionedSpaceGB { get; set; }
        public string VMHost { get; set; }
        public string GuestOS { get; set; }
        public string ToolsStatus { get; set; }
        public double UptimeDays { get; set; }
    }
}
