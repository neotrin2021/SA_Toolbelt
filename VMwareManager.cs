using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SA_ToolBelt
{
    public partial class VMwareManager
    {
        private readonly string _vCenterServer;
        private readonly string _username;
        private readonly string _password;
        private readonly ConsoleForm _consoleForm;
        private readonly string _powerCLIModulePath;

        public VMwareManager(string vCenterServer, string username, string password, ConsoleForm consoleForm, string powerCLIModulePath)
        {
            _vCenterServer = vCenterServer;
            _username = username;
            _password = password;
            _consoleForm = consoleForm;
            _powerCLIModulePath = powerCLIModulePath;
        }

        /// <summary>
        /// Set PowerShell execution policy to allow PowerCLI
        /// </summary>
        private async Task SetProcessScopeExecutionPolicyAsync()
        {
            try
            {
                using (PowerShell powerShell = PowerShell.Create())
                {
                    _consoleForm.WriteInfo("Setting PowerShell execution policy to RemoteSigned...");
                    powerShell.AddScript("Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force");
                    await Task.Run(() => powerShell.Invoke());

                    if (powerShell.HadErrors)
                    {
                        foreach (var error in powerShell.Streams.Error)
                        {
                            _consoleForm.WriteError($"Error setting execution policy: {error.Exception}");
                        }
                    }
                    else
                    {
                        _consoleForm.WriteSuccess("PowerShell execution policy set successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to set PowerShell execution policy: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Import VMware PowerCLI module from network share
        /// </summary>
        private async Task ImportPowerCLIModuleAsync()
        {
            try
            {
                using (PowerShell powerShell = PowerShell.Create())
                {
                    _consoleForm.WriteInfo($"Importing VMware.PowerCLI module from {_powerCLIModulePath}...");

                    // Import module from the specified network share path
                    powerShell.AddScript($"Import-Module '{_powerCLIModulePath}' -ErrorAction Stop");
                    await Task.Run(() => powerShell.Invoke());

                    if (powerShell.HadErrors)
                    {
                        foreach (var error in powerShell.Streams.Error)
                        {
                            _consoleForm.WriteError($"PowerCLI import error: {error.Exception}");
                        }
                        throw new Exception($"Failed to import VMware.PowerCLI module from {_powerCLIModulePath}");
                    }
                    else
                    {
                        _consoleForm.WriteSuccess($"VMware.PowerCLI module imported successfully from network share");
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to import PowerCLI module: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Configure PowerCLI settings (disable certificate warnings)
        /// </summary>
        private async Task ConfigurePowerCLISettingsAsync()
        {
            try
            {
                using (PowerShell powerShell = PowerShell.Create())
                {
                    _consoleForm.WriteInfo("Configuring PowerCLI settings...");
                    powerShell.AddScript("Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false -Scope Session");
                    await Task.Run(() => powerShell.Invoke());

                    if (powerShell.HadErrors)
                    {
                        foreach (var error in powerShell.Streams.Error)
                        {
                            _consoleForm.WriteWarning($"PowerCLI configuration warning: {error.Exception}");
                        }
                    }
                    else
                    {
                        _consoleForm.WriteSuccess("PowerCLI settings configured successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteWarning($"Failed to configure PowerCLI settings: {ex.Message}");
                // Don't throw - this is not critical
            }
        }

        /// <summary>
        /// Connect to vCenter Server and return PowerShell session
        /// </summary>
        private async Task<PowerShell> ConnectToVCenterAsync()
        {
            PowerShell powerShell = PowerShell.Create();
            try
            {
                _consoleForm.WriteInfo($"Connecting to vCenter server {_vCenterServer}...");
                powerShell.AddScript($@"Connect-VIServer -Server '{_vCenterServer}' -User 'SPECTRE\{_username}' -Password '{_password}'");

                await Task.Run(() => powerShell.Invoke());

                if (powerShell.HadErrors)
                {
                    foreach (var error in powerShell.Streams.Error)
                    {
                        _consoleForm.WriteError($"PowerShell Error: {error.Exception}");
                    }
                    throw new Exception("Failed to connect to vCenter");
                }

                _consoleForm.WriteSuccess("Successfully connected to vCenter");
                return powerShell;
            }
            catch (Exception ex)
            {
                powerShell.Dispose();
                _consoleForm.WriteError($"Failed to connect to vCenter: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize PowerCLI modules and execution policy
        /// </summary>
        public async Task InitializePowerCLIAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Initializing PowerCLI...");
                await SetProcessScopeExecutionPolicyAsync();
                await ImportPowerCLIModuleAsync();
                await ConfigurePowerCLISettingsAsync();
                _consoleForm.WriteSuccess("PowerCLI initialization completed");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to initialize PowerCLI: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get basic ESXi host information
        /// </summary>
        public async Task<List<VMHost>> GetESXiHostsAsync()
        {
            using (var powerShell = await ConnectToVCenterAsync())
            {
                try
                {
                    _consoleForm.WriteInfo("Retrieving ESXi hosts...");
                    powerShell.Commands.Clear();
                    powerShell.AddScript(@"
                        Get-VMHost | Select-Object Name, @{N='Hostname';E={$_.NetworkInfo.HostName}}, @{N='MaintenanceMode';E={$_.ConnectionState}} | ForEach-Object {
                            [PSCustomObject]@{
                                Hostname = $_.Hostname
                                IP = $_.Name
                                MaintenanceMode = $_.MaintenanceMode
                            }
                        }");

                    var results = await Task.Run(() => powerShell.Invoke());
                    var hosts = new List<VMHost>();

                    foreach (var result in results)
                    {
                        hosts.Add(new VMHost
                        {
                            Hostname = result.Properties["Hostname"].Value.ToString(),
                            IP = result.Properties["IP"].Value.ToString(),
                            MaintenanceMode = result.Properties["MaintenanceMode"].Value.ToString()
                        });
                    }

                    _consoleForm.WriteSuccess($"Retrieved {hosts.Count} ESXi hosts");
                    await DisconnectFromVCenterAsync(powerShell);
                    return hosts;
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Failed to retrieve ESXi hosts: {ex.Message}");
                    await DisconnectFromVCenterAsync(powerShell);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get detailed ESXi host information including resource usage and HA status
        /// </summary>
        public async Task<List<VMHostDetailed>> GetESXiHostsDetailedAsync()
        {
            using (var powerShell = await ConnectToVCenterAsync())
            {
                try
                {
                    _consoleForm.WriteInfo("Retrieving detailed ESXi host information...");
                    powerShell.Commands.Clear();
                    powerShell.AddScript(@"
                        $hosts = Get-VMHost | Select-Object Name,
                        @{N='Hostname';E={$_.NetworkInfo.HostName}},
                        @{N='ConnectionState';E={$_.ConnectionState}},
                        PowerState,
                        @{N='Status';E={$_.ExtensionData.Summary.OverallStatus}},
                        @{N='Cluster';E={$_.Parent.Name}},
                        @{N='ConsumedCPU';E={[math]::Round(($_.CpuUsageMhz/$_.CpuTotalMhz)*100,2)}},
                        @{N='ConsumedMemory';E={[math]::Round(($_.MemoryUsageGB/$_.MemoryTotalGB)*100,2)}}

                        $uptimeInfo = Get-View -ViewType HostSystem -Property Name,Runtime.BootTime | 
                        Select-Object Name, @{N='UptimeInDays';E={[math]::Round((((Get-Date) - $_.Runtime.BootTime).TotalDays),2)}}

                        $HostInfos = $hosts | ForEach-Object {
                            $currentHost = $_
                            $matchingUptime = $uptimeInfo | Where-Object { $_.Name -eq $currentHost.Name }
                            if ($matchingUptime) {
                                [PSCustomObject]@{
                                    Name = $currentHost.Name
                                    Hostname = $currentHost.Hostname
                                    ConnectionState = $currentHost.ConnectionState
                                    PowerState = $currentHost.PowerState
                                    Status = $currentHost.Status
                                    Cluster = $currentHost.Cluster
                                    ConsumedCPU = $currentHost.ConsumedCPU
                                    ConsumedMemory = $currentHost.ConsumedMemory
                                    UptimeInDays = $matchingUptime.UptimeInDays
                                }
                            }
                        }

                        $haStates = Get-VMHost | ForEach-Object {
                            $view = $_ | Get-View
                            $state = $view.Runtime.DasHostState.State
                    
                            $mappedState = switch ($state) {
                                'connectedToMaster' { 'Connected (Secondary)' }
                                'master' { 'Running (Primary)' }
                                default { $state }
                            }
                            @{
                                Name = $_.Name
                                HAState = $mappedState
                             }
                        }

                        $HostInfos | ForEach-Object {
                            $currentHost = $_
                            $matchingHAState = $haStates | Where-Object { $_.Name -eq $currentHost.Name }
                            [PSCustomObject]@{
                                Name = $currentHost.Name
                                Hostname = $currentHost.Hostname
                                ConnectionState = $currentHost.ConnectionState
                                PowerState = $currentHost.PowerState
                                Status = $currentHost.Status
                                Cluster = $currentHost.Cluster
                                ConsumedCPU = $currentHost.ConsumedCPU
                                ConsumedMemory = $currentHost.ConsumedMemory
                                UptimeInDays = $currentHost.UptimeInDays
                                HAAvailability = $matchingHAState.HAState
                            }
                        }"
                    );

                    var results = await Task.Run(() => powerShell.Invoke());
                    var hosts = new List<VMHostDetailed>();

                    foreach (var result in results)
                    {
                        try
                        {
                            hosts.Add(new VMHostDetailed
                            {
                                Name = result.Properties["Name"].Value?.ToString() ?? "N/A",
                                Hostname = result.Properties["Hostname"].Value?.ToString() ?? "N/A",
                                ConnectionState = result.Properties["ConnectionState"].Value?.ToString() ?? "N/A",
                                PowerStatus = result.Properties["PowerState"].Value?.ToString() ?? "N/A",
                                Status = result.Properties["Status"].Value?.ToString() ?? "N/A",
                                Cluster = result.Properties["Cluster"].Value?.ToString() ?? "N/A",
                                ConsumedCPU = ConvertToMHz(result, "ConsumedCPU"),
                                ConsumedMemory = ConvertToGB(result, "ConsumedMemory"),
                                HAState = result.Properties["HAAvailability"].Value?.ToString() ?? "N/A",
                                Uptime = result.Properties["UptimeInDays"].Value?.ToString() ?? "N/A"
                            });
                        }
                        catch (Exception ex)
                        {
                            _consoleForm.WriteError($"Error processing host: {ex.Message}");
                            continue;
                        }
                    }

                    _consoleForm.WriteSuccess($"Retrieved {hosts.Count} detailed ESXi hosts");
                    await DisconnectFromVCenterAsync(powerShell);
                    return hosts;
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Failed to retrieve detailed ESXi hosts: {ex.Message}");
                    await DisconnectFromVCenterAsync(powerShell);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get basic virtual machine information
        /// </summary>
        public async Task<List<VMachine>> GetVirtualMachinesAsync()
        {
            using (var powerShell = await ConnectToVCenterAsync())
            {
                try
                {
                    _consoleForm.WriteInfo("Retrieving Virtual Machines...");
                    powerShell.Commands.Clear();
                    powerShell.AddScript(@"
                        Get-VM | Select-Object Name, PowerState, @{N='ESXiHost';E={$_.VMHost.NetworkInfo.Hostname}}, @{N='ESXiHostIP';E={$_.VMHost.Name}} | ForEach-Object {
                            [PSCustomObject]@{
                                Name = $_.Name
                                PowerState = $_.PowerState
                                ESXiHostname = $_.ESXiHost
                                ESXiIP = $_.ESXiHostIP
                            }
                        }");

                    var results = await Task.Run(() => powerShell.Invoke());
                    var vms = new List<VMachine>();

                    foreach (var result in results)
                    {
                        vms.Add(new VMachine
                        {
                            Name = result.Properties["Name"].Value.ToString(),
                            PowerState = result.Properties["PowerState"].Value.ToString(),
                            ESXiHostname = result.Properties["ESXiHostname"].Value.ToString(),
                            ESXiIP = result.Properties["ESXiIP"].Value.ToString()
                        });
                    }

                    _consoleForm.WriteSuccess($"Retrieved {vms.Count} Virtual Machines");
                    await DisconnectFromVCenterAsync(powerShell);
                    return vms;
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Failed to retrieve Virtual Machines: {ex.Message}");
                    await DisconnectFromVCenterAsync(powerShell);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get detailed virtual machine information including resource usage
        /// </summary>
        public async Task<List<VMachineDetailed>> GetVirtualMachinesDetailedAsync()
        {
            using (var powerShell = await ConnectToVCenterAsync())
            {
                try
                {
                    _consoleForm.WriteInfo("Retrieving detailed Virtual Machine information...");
                    powerShell.Commands.Clear();
                    powerShell.AddScript(@"
                        Get-VM | Select-Object Name, 
                            PowerState, 
                            @{N='Status';E={$_.Status}}, 
                            @{N='ProvisionedSpace';E={[math]::Round($_.ProvisionedSpaceGB,2)}}, 
                            @{N='UsedSpace';E={[math]::Round($_.UsedSpaceGB,2)}}, 
                            @{N='HostCPU';E={[math]::Round($_.VMHost.CpuUsageMhz,2)}}, 
                            @{N='HostMemory';E={[math]::Round($_.MemoryGB,2)}}, 
                            @{N='ESXiHost';E={$_.VMHost.NetworkInfo.Hostname}}, 
                            @{N='ESXiHostIP';E={$_.VMHost.Name}} | 
                        ForEach-Object {
                            [PSCustomObject]@{
                                Name = $_.Name
                                PowerState = $_.PowerState
                                Status = $_.Status
                                ProvisionedSpace = $_.ProvisionedSpace
                                UsedSpace = $_.UsedSpace
                                HostCPU = $_.HostCPU
                                HostMemory = $_.HostMemory
                                ESXiHostname = $_.ESXiHost
                                ESXiHostIP = $_.ESXiHostIP
                            }
                        }");

                    var results = await Task.Run(() => powerShell.Invoke());
                    var vms = new List<VMachineDetailed>();

                    foreach (var result in results)
                    {
                        try
                        {
                            vms.Add(new VMachineDetailed
                            {
                                Name = result.Properties["Name"].Value?.ToString() ?? "N/A",
                                PowerState = result.Properties["PowerState"].Value?.ToString() ?? "N/A",
                                Status = result.Properties["Status"].Value?.ToString() ?? "N/A",
                                ProvisionedSpace = ConvertToGB(result, "ProvisionedSpace"),
                                UsedSpace = ConvertToGB(result, "UsedSpace"),
                                HostCPU = ConvertToMHz(result, "HostCPU"),
                                HostMemory = ConvertToGB(result, "HostMemory"),
                                ESXiHostname = result.Properties["ESXiHostname"].Value?.ToString() ?? "N/A",
                                ESXiIP = result.Properties["ESXiIP"].Value?.ToString() ?? "N/A"
                            });
                        }
                        catch (Exception ex)
                        {
                            _consoleForm.WriteError($"Error processing VM: {ex.Message}");
                            continue;
                        }
                    }

                    _consoleForm.WriteSuccess($"Retrieved {vms.Count} detailed Virtual Machines");
                    await DisconnectFromVCenterAsync(powerShell);
                    return vms;
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Failed to retrieve detailed Virtual Machines: {ex.Message}");
                    await DisconnectFromVCenterAsync(powerShell);
                    throw;
                }
            }
        }

        /// <summary>
        /// Convert values to GB with proper unit handling
        /// </summary>
        private double ConvertToGB(PSObject result, string propertyName)
        {
            try
            {
                if (result.Properties[propertyName] == null || result.Properties[propertyName].Value == null)
                    return 0;

                string value = result.Properties[propertyName].Value.ToString();
                var match = Regex.Match(value, @"[\d.]+");

                if (!match.Success)
                    return 0;

                double number = Convert.ToDouble(match.Value);
                return value.Contains("GB") ? number :
                       value.Contains("MB") ? number / 1024 :
                       value.Contains("TB") ? number * 1024 : number;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert values to MHz with proper unit handling
        /// </summary>
        private double ConvertToMHz(PSObject result, string propertyName)
        {
            try
            {
                if (result.Properties[propertyName] == null || result.Properties[propertyName].Value == null)
                    return 0;

                string value = result.Properties[propertyName].Value.ToString();
                var match = Regex.Match(value, @"[\d.]+");

                if (!match.Success)
                    return 0;

                double number = Convert.ToDouble(match.Value);
                return value.Contains("GHz") ? number * 1000 :
                       value.Contains("Hz") ? number / 1000000 :
                       value.Contains("MHz") ? number : number;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Disconnect from vCenter Server
        /// </summary>
        private async Task DisconnectFromVCenterAsync(PowerShell powerShell)
        {
            try
            {
                _consoleForm.WriteInfo("Disconnecting from vCenter...");
                powerShell.Commands.Clear();
                powerShell.AddScript("Disconnect-VIServer -Server * -Force -Confirm:$false");
                await Task.Run(() => powerShell.Invoke());
                _consoleForm.WriteSuccess("Successfully disconnected from vCenter");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during disconnect: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            // Connection cleanup handled by using statements in methods
        }
    }

    #region Data Classes

    public class VMHost
    {
        public string Hostname { get; set; }
        public string IP { get; set; }
        public string MaintenanceMode { get; set; }
    }

    public class VMHostDetailed
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public string ConnectionState { get; set; }
        public string PowerStatus { get; set; }
        public string Status { get; set; }
        public string Cluster { get; set; }
        public double ConsumedCPU { get; set; }
        public double ConsumedMemory { get; set; }
        public string HAState { get; set; }
        public string Uptime { get; set; }

        // Calculated properties for display
        public string CpuUsagePercent => $"{ConsumedCPU}%";
        public string MemoryUsagePercent => $"{ConsumedMemory}%";
    }

    public class VMachine
    {
        public string Name { get; set; }
        public string PowerState { get; set; }
        public string ESXiHostname { get; set; }
        public string ESXiIP { get; set; }
    }

    public class VMachineDetailed
    {
        public string Name { get; set; }
        public string PowerState { get; set; }
        public string Status { get; set; }
        public double ProvisionedSpace { get; set; }
        public double UsedSpace { get; set; }
        public double HostCPU { get; set; }
        public double HostMemory { get; set; }
        public string ESXiHostname { get; set; }
        public string ESXiIP { get; set; }
    }

    #endregion
}