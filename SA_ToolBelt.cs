using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SA_ToolBelt.AD_Service;
using LdapSearchScope = System.DirectoryServices.Protocols.SearchScope;

// Surpresses warning about an object possibly being null
#pragma warning disable CS8602

namespace SA_ToolBelt
{
    public partial class SAToolBelt : Form
    {

        private readonly AD_Service _adService;
        private readonly RHDS_Service _rhdsService;
        private ConsoleForm _consoleForm;
        private Linux_Service _linuxService;
        private VMwareManager _vmwareManager;
        private PreCheck _preCheck;
        private DatabaseService _databaseService;

        // Startup Shutdown Variables
        public string VMMode = "NormalRun";
        public bool startUpShutdown = false;
        private Dictionary<string, bool> pingStatus = new Dictionary<string, bool>();

        // Configuration values - now populated from SQLite database
        private string _vCenterServer = string.Empty;
        private string POWERCLI_MODULE_PATH = string.Empty;
        private string _disabledUsersOu = string.Empty;
        private string _homeDirectoryPath = string.Empty;
        private string _excludedOUs = string.Empty;


        // Store the logged in SA's username globally
        public string _loggedInUsername = string.Empty;

        // Dictionary to store LDAP server instances (server -> instance name)
        private Dictionary<string, string> _ldapServerInstances = new Dictionary<string, string>();

        // Secret sequence tracking for hidden feature
        private readonly DateTime _magicDate = new DateTime(1973, 8, 6); // Monday, August 06, 1973
        private int _secretSequenceState = 0; // 0 = waiting for magic date, 1-4 = waiting for radio buttons
        private bool _magicDateSet = false;

        public SAToolBelt()
        {
            InitializeComponent();

            _consoleForm = new ConsoleForm();
            _adService = new AD_Service(_consoleForm);
            _linuxService = new Linux_Service(_consoleForm);
            _rhdsService = new RHDS_Service(_consoleForm);
            _preCheck = new PreCheck(_consoleForm);
            _databaseService = new DatabaseService(_consoleForm);

            this.KeyPreview = true;
            this.FormClosing += SAToolBelt_FormClosing;

            // Hide all tabs except Login initially
            HideAllTabsExceptLogin();
            SetupRadioButtonExclusivity();
            SetupSecretSequenceHandlers();
            SetupMandatorySettingsHandlers();

            // Hide all controls until successful login
            HideControlsAtStartUp();

            // Note: Configuration file loading is deferred until after PreCheck validation
            // This happens in the login flow after mandatory settings are verified

            // Populate Linux server dropdown for log fetching
            PopulateLinuxServerDropdown();

            tabConsole.Controls.Add(_consoleForm.GetConsoleRichTextBox());
            _consoleForm.GetConsoleRichTextBox().Dock = DockStyle.Fill;
            _consoleForm.GetConsoleRichTextBox().ContextMenuStrip = _consoleContextMenu;
            _consoleForm.Hide();

            // Setup for Docking and UnDocking of the Console Window
            InitializeConsoleDocking();

        }

        private void SAToolBelt_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Release the database lock so other SAs can use the database
            _databaseService?.Dispose();
        }

        #region Console Docking Management

        private bool _consoleIsDocked = true; // Track current state
        private ContextMenuStrip _consoleContextMenu;

        /// <summary>
        /// Initialize console docking functionality
        /// </summary>
        private void InitializeConsoleDocking()
        {
            try
            {
                // Start with console docked in tab
                DockConsole();

                // Create right-click context menu
                CreateConsoleContextMenu();

                // Wire up ConsoleForm's dock button (will be added to ConsoleForm)
                if (_consoleForm != null)
                {
                    _consoleForm.DockButtonClicked += () => DockConsole();
                }

                _consoleForm?.WriteInfo("Console docking system initialized.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error initializing console docking: {ex.Message}");
            }
        }

        /// <summary>
        /// Create right-click context menu for console
        /// </summary>
        private void CreateConsoleContextMenu()
        {
            _consoleContextMenu = new ContextMenuStrip();

            var dockItem = new ToolStripMenuItem("Dock Console")
            {
                Enabled = !_consoleIsDocked
            };
            dockItem.Click += (s, e) => DockConsole();

            var undockItem = new ToolStripMenuItem("Undock Console")
            {
                Enabled = _consoleIsDocked
            };
            undockItem.Click += (s, e) => UndockConsole();

            _consoleContextMenu.Items.AddRange(new ToolStripItem[] { dockItem, undockItem });

            // Attach context menu to both the tab and the console content
            tabConsole.ContextMenuStrip = _consoleContextMenu;
        }

        /// <summary>
        /// Update context menu item states
        /// </summary>
        private void UpdateContextMenuStates()
        {
            if (_consoleContextMenu != null)
            {
                _consoleContextMenu.Items[0].Enabled = !_consoleIsDocked; // Dock option
                _consoleContextMenu.Items[1].Enabled = _consoleIsDocked;  // Undock option
            }
        }

        /// <summary>
        /// Dock console into the tab
        /// </summary>
        private void DockConsole()
        {
            try
            {
                if (_consoleForm != null && !_consoleIsDocked)
                {
                    _consoleForm.RemoveConsoleControl();

                    tabControlMain.TabPages.Add(tabConsole);

                    tabConsole.Controls.Add(_consoleForm.GetConsoleRichTextBox());
                    _consoleForm.GetConsoleRichTextBox().Dock = DockStyle.Fill;
                    _consoleForm.GetConsoleRichTextBox().ContextMenuStrip = _consoleContextMenu;

                    _consoleForm.Hide();

                    _consoleIsDocked = true;
                    UpdateContextMenuStates();

                    _consoleForm.WriteInfo("Console docked to main window.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error docking console: {ex.Message}");
            }
        }

        /// <summary>
        /// Undock console to floating window
        /// </summary>
        private void UndockConsole()
        {
            try
            {
                tabConsole.Controls.Remove(_consoleForm.GetConsoleRichTextBox());
                // Remove from tab
                tabControlMain.TabPages.Remove(tabConsole);
                _consoleForm.AddConsoleControl(_consoleForm.GetConsoleRichTextBox());

                // Show the floating window
                _consoleForm.Show();
                _consoleForm.BringToFront();
                _consoleForm.Activate();

                _consoleIsDocked = false;
                UpdateContextMenuStates();

                _consoleForm.WriteInfo("Console undocked to floating window.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error undocking console: {ex.Message}");
            }
        }
        #endregion

        #region Console Docking Management Button Events
        /// <summary>
        /// Button event for undocking console from tab
        /// </summary>
        private void btnUndockConsole_Click(object sender, EventArgs e)
        {
            UndockConsole();
        }
        #endregion

        #region Login Tab functions
        private void SAToolBelt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tabControlMain.SelectedTab == tabLogin)
                {
                    btnLogin.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void panelLogin_Paint(object sender, PaintEventArgs e)
        {
            // Create a blue border with thickness
            Panel panel = sender as Panel;
            if (panel != null)
            {
                using (Pen bluePen = new Pen(Color.Blue, 3)) // 3 pixel thick blue border
                {
                    Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawRectangle(bluePen, rect);
                }
            }
        }
        private void HideAllTabsExceptLogin()
        {
            // Remove all tabs except Login from the TabControl (but keep references)
            tabControlMain.TabPages.Clear();
            tabControlMain.TabPages.Add(tabLogin);
        }

        private void HideControlsAtStartUp()
        {
            btnUndockConsole.Visible = false;
            btnLogout.Visible = false;
        }

        private void ShowAllTabs()
        {
            // Add all tabs EXCEPT Login after successful authentication
            tabControlMain.TabPages.Clear();
            tabControlMain.TabPages.Add(tabAD);
            tabControlMain.TabPages.Add(tabLDAP);
            // tabControlMain.TabPages.Add(tabRemoteTools);
            // tabControlMain.TabPages.Add(tabWindowsTools);
            // tabControlMain.TabPages.Add(tabLinuxTools);
            // tabControlMain.TabPages.Add(tabVMwareTools);
            tabControlMain.TabPages.Add(tabOnlineOffline);
            tabControlMain.TabPages.Add(tabSAPMIsSpice);
            // tabControlMain.TabPages.Add(tabStartupShutdownPt1);
            // tabControlMain.TabPages.Add(tabStartupShutdownPt2);
            tabControlMain.TabPages.Add(tabConfiguration);
            tabControlMain.TabPages.Add(tabConsole);
        }

        /// <summary>
        /// Shows only the Configuration tab for mandatory settings setup.
        /// Called when PreCheck validation fails and settings need to be configured.
        /// </summary>
        private void ShowOnlyConfigurationTab()
        {
            tabControlMain.TabPages.Clear();
            tabControlMain.TabPages.Add(tabConfiguration);
            tabControlMain.TabPages.Add(tabConsole); // Keep console visible for feedback

            gbxManditorySettings.Enabled = true;
        }

        /// <summary>
        /// Applies settings from the SQLite database to application variables
        /// and loads configuration data from the database tables.
        /// This is the ONLY settings-loading method. No CSV fallback.
        /// </summary>
        private void ApplyDatabaseSettings()
        {
            var config = _databaseService.LoadToolbeltConfig();
            if (config == null)
            {
                _consoleForm.WriteWarning("No configuration found in database.");
                return;
            }

            // Apply Toolbelt_Config values to application variables
            _vCenterServer = config.VCenterServer;
            POWERCLI_MODULE_PATH = !string.IsNullOrEmpty(config.PowerCLILocation)
                ? Path.Combine(config.PowerCLILocation, "VMware.PowerCLI")
                : string.Empty;
            _disabledUsersOu = config.DisabledUsersOu;
            _homeDirectoryPath = config.HomeDirectory;
            _excludedOUs = config.ExcludedOU;

            // Update the excluded OUs in the AD Service
            if (!string.IsNullOrEmpty(_excludedOUs))
            {
                var ouList = _excludedOUs.Split('|', StringSplitOptions.RemoveEmptyEntries);
                _adService.SetExcludedOUs(new HashSet<string>(ouList));
            }

            // Update the home directory base path in the Linux Service
            if (!string.IsNullOrEmpty(_homeDirectoryPath))
            {
                _linuxService.SetHomeDirectoryBasePath(_homeDirectoryPath);
            }

            // Update the configuration file path labels
            lblFilePathLocation.Text = _databaseService.DatabasePath;
            lblPowerCLIPathLocation.Text = POWERCLI_MODULE_PATH;

            // Load data from database tables into the UI
            LoadComputerListFromDB();
            LoadOUConfigurationFromDB();
            LoadImportantVariablesFromDB();
            LoadLogConfigurationFromDB();

            _consoleForm.WriteSuccess("Configuration settings loaded from database successfully.");
        }

        /// <summary>
        /// Populates the mandatory settings textboxes from the database.
        /// Called when the Configuration tab is shown for first-time or re-configuration.
        /// </summary>
        private void PopulateMandatorySettingsUI()
        {
            var config = _databaseService.LoadToolbeltConfig();
            if (config != null)
            {
                txbVCenterServer.Text = config.VCenterServer;
                txbSqlPath.Text = config.SqlPath;
                txbPowerCliModuleLocation.Text = config.PowerCLILocation;
                txbDisabledUsersLocation.Text = config.DisabledUsersOu;
                txbHomeDirectoryLocation.Text = config.HomeDirectory;
                txbLinuxDs.Text = config.LinuxDs;

                // Populate excluded OUs combobox
                if (!string.IsNullOrEmpty(config.ExcludedOU))
                {
                    cbxExcludeOu.Items.Clear();
                    var ous = config.ExcludedOU.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ou in ous)
                    {
                        cbxExcludeOu.Items.Add(ou);
                    }
                    if (cbxExcludeOu.Items.Count > 0)
                        cbxExcludeOu.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Disables all controls in gbxManditorySettings EXCEPT txbSqlPath and btnBrowseSqlPath.
        /// Used during first-time setup when no registry key exists - user must provide the DB path first.
        /// </summary>
        private void DisableMandatoryControlsExceptSqlPath()
        {
            foreach (Control ctrl in gbxManditorySettings.Controls)
            {
                ctrl.Enabled = false;
            }
            // Only these two stay enabled so the user can browse for the DB
            txbSqlPath.Enabled = true;
            btnBrowseSqlPath.Enabled = true;
        }

        /// <summary>
        /// Enables all controls in gbxManditorySettings.
        /// Called when the user needs to fill out all fields for a fresh setup.
        /// </summary>
        private void EnableAllMandatoryControls()
        {
            foreach (Control ctrl in gbxManditorySettings.Controls)
            {
                ctrl.Enabled = true;
            }
        }

        /// <summary>
        /// Sets up event handlers for the mandatory settings controls.
        /// Note: btnVerifyVCenterServer, btnBrowseSqlPath, btnBrowsePowerCLIModuleLocation,
        /// btnSetAll, btnSelectAddExcludeOu, btnDisabledUsersLocation, and btnLinuxDs
        /// are wired via the Designer.cs Click += events.
        /// </summary>
        private void SetupMandatorySettingsHandlers()
        {
            // Event handlers are wired in the Designer.cs file via Click += assignments.
            // No additional wiring needed here.
        }

        #region Mandatory Settings Button Handlers

        private void btnVerifyVCenterServer_Click(object sender, EventArgs e)
        {
            _preCheck.VerifyVCenterServerWithFeedback(txbVCenterServer.Text);
        }

        private void btnBrowseSqlPath_Click(object sender, EventArgs e)
        {
            string path = _preCheck.BrowseForFolder("Select the folder where the SA_Toolbelt database is stored (or will be created)");
            if (string.IsNullOrEmpty(path))
                return;

            txbSqlPath.Text = path;

            // Check if a database already exists at this path
            if (_preCheck.DatabaseExistsAtPath(path))
            {
                _consoleForm.WriteInfo($"Existing database found at: {path}");

                // Update the registry to point here and reload
                DatabaseService.SetSqlPathInRegistry(path);
                _databaseService?.Dispose();
                _databaseService = new DatabaseService(_consoleForm);

                // Check for lock before trying to read
                if (!_databaseService.CheckAndHandleLock())
                {
                    _consoleForm.WriteWarning("Database is locked. Cannot load configuration.");
                    return;
                }

                if (_databaseService.HasValidConfig())
                {
                    // DB has valid config - load it and open everything up
                    ApplyDatabaseSettings();
                    PopulateMandatorySettingsUI();
                    EnableAllMandatoryControls();
                    ShowAllTabs();

                    _consoleForm.WriteSuccess("Existing database loaded successfully. All tabs now available.");
                    MessageBox.Show("Existing database found and loaded successfully!\nAll features are now available.",
                        "Database Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // DB exists but has no valid config - enable controls for setup
                    _consoleForm.WriteWarning("Database found but contains no valid configuration. Please fill in the settings.");
                    EnableAllMandatoryControls();
                }
            }
            else
            {
                // No database at this path - this is a fresh setup
                _consoleForm.WriteInfo($"No existing database at: {path}. Enabling all settings for first-time configuration.");
                EnableAllMandatoryControls();
            }
        }

        private void btnBrowsePowerCLIModuleLocation_Click(object sender, EventArgs e)
        {
            string path = _preCheck.BrowseForFolder("Select the folder containing VMware.PowerCLI module");
            if (!string.IsNullOrEmpty(path))
            {
                txbPowerCliModuleLocation.Text = path;
            }
        }

        private void btnSetAll_Click(object sender, EventArgs e)
        {
            try
            {
                // Read values from all mandatory settings controls
                string vCenterServer = txbVCenterServer.Text.Trim();
                string sqlPath = txbSqlPath.Text.Trim();
                string powerCliLocation = txbPowerCliModuleLocation.Text.Trim();
                string excludedOu = cbxExcludeOu.Text.Trim();
                string disabledUsersOu = txbDisabledUsersLocation.Text.Trim();
                string homeDirectory = txbHomeDirectoryLocation.Text.Trim();
                string linuxDs = txbLinuxDs.Text.Trim();

                // Validate vCenter server
                bool vCenterValid = _preCheck.ValidateVCenterServer(vCenterServer);
                txbVCenterServer.BackColor = vCenterValid ? Color.White : Color.LightCoral;

                // Validate PowerCLI path
                bool powerCliValid = !string.IsNullOrEmpty(powerCliLocation) && Directory.Exists(Path.Combine(powerCliLocation, "VMware.PowerCLI"));
                txbPowerCliModuleLocation.BackColor = powerCliValid ? Color.White : Color.LightCoral;

                // Validate SQL path (must be a valid directory or we can create it)
                bool sqlPathValid = !string.IsNullOrEmpty(sqlPath);
                if (sqlPathValid && !Directory.Exists(sqlPath))
                {
                    try { Directory.CreateDirectory(sqlPath); }
                    catch { sqlPathValid = false; }
                }
                txbSqlPath.BackColor = sqlPathValid ? Color.White : Color.LightCoral;

                if (!vCenterValid || !powerCliValid || !sqlPathValid)
                {
                    var errors = new List<string>();
                    if (!vCenterValid) errors.Add("- VCenter Server is invalid or unreachable");
                    if (!powerCliValid) errors.Add("- PowerCLI Module path is invalid or VMware.PowerCLI folder not found");
                    if (!sqlPathValid) errors.Add("- SQL Path is empty or invalid");

                    MessageBox.Show($"The following settings need to be corrected:\n\n{string.Join("\n", errors)}",
                        "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Initialize database in Documents first
                _databaseService.InitializeDatabase();

                // Gather all excluded OUs from the combobox items
                var excludedOuList = new List<string>();
                foreach (var item in cbxExcludeOu.Items)
                {
                    excludedOuList.Add(item.ToString());
                }
                // Also include current text if it's not already in the list
                if (!string.IsNullOrEmpty(excludedOu) && !excludedOuList.Contains(excludedOu))
                {
                    excludedOuList.Add(excludedOu);
                }
                string excludedOuCombined = string.Join("|", excludedOuList);

                // Save configuration to database
                _databaseService.SaveToolbeltConfig(
                    vCenterServer,
                    powerCliLocation,
                    sqlPath,
                    excludedOuCombined,
                    disabledUsersOu,
                    homeDirectory,
                    linuxDs
                );

                // Move database to the user-specified SQL path
                if (!string.IsNullOrEmpty(sqlPath))
                {
                    bool moved = _databaseService.MoveDatabase(sqlPath);
                    if (!moved)
                    {
                        _consoleForm.WriteWarning("Database could not be moved to the specified path. It remains in Documents.");
                    }
                }

                // Store the Sql_Path in the registry
                DatabaseService.SetSqlPathInRegistry(sqlPath);

                // Apply the settings to application variables
                ApplyDatabaseSettings();
                ShowAllTabs();

                _consoleForm.WriteSuccess("All mandatory settings saved to database successfully.");
                MessageBox.Show("All settings validated and saved to database successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error saving settings: {ex.Message}");
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLinuxDs_Click(object sender, EventArgs e)
        {
            string path = _preCheck.BrowseForFolder("Select the Linux DS server path");
            if (!string.IsNullOrEmpty(path))
            {
                txbLinuxDs.Text = path;
            }
        }

        #endregion

        private void ShowControlsAfterLogin()
        {
            btnUndockConsole.Visible = true;
            btnLogout.Visible = true;
        }

        private async Task<bool> CheckDomainAdminMembership(string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // ==========================================================
                    // BACKDOOR FOR TESTING - REMOVE WHEN DEPLOYING TO PRODUCTION
                    // ==========================================================
                    if (username.Equals("logmein", StringComparison.OrdinalIgnoreCase))
                    {
                        _consoleForm.WriteWarning("BACKDOOR ACCESS USED - Remove for production!");
                        return true; // Grant access without password check
                    }
                    // ==========================================================
                    // END BACKDOOR - DELETE ABOVE SECTION WHEN READY
                    // ==========================================================

                    // For normal users, password is required
                    if (string.IsNullOrEmpty(password))
                    {
                        _consoleForm.WriteError("Password is required for authentication");
                        return false;
                    }

                    _consoleForm.WriteInfo($"Verifying domain admin privileges for {username}...");

                    // Use the existing AD_Service method to check domain admin privileges
                    bool isDomainAdmin = _adService.IsCurrentUserDomainAdmin();

                    if (isDomainAdmin)
                    {
                        _consoleForm.WriteSuccess($"Domain admin privileges confirmed for {username}");
                    }
                    else
                    {
                        _consoleForm.WriteWarning($"User {username} does not have domain admin privileges");
                    }

                    return isDomainAdmin;
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Error checking domain admin privileges: {ex.Message}");
                    return false;
                }
            });
        }
        /// <summary>
        /// Populate the Default Security Groups combobox from RHDS
        /// </summary>
        /// 
        private async Task PopulateDefaultSecurityGroupsAsync()
        {
            try
            {
                _consoleForm?.WriteInfo("Loading available security groups from Directory Services...");

                // Get the configured Security Groups OU
                string securityGroupsOU = GetSecurityGroupsOU();

                if (string.IsNullOrEmpty(securityGroupsOU))
                {
                    _consoleForm?.WriteWarning("No Security Groups OU configured. Please configure in Configuration tab.");
                    cbxDefaultSecurityGroups.Items.Add("(No Security Groups OU configured)");
                    cbxDefaultSecurityGroups.Enabled = false;
                    return;
                }

                // Get the filter keyword
                string filterKeyword = GetSecurityGroupKeyword();

                // Get all groups from the Security Groups OU
                await Task.Run(() =>
                {
                    try
                    {
                        var groups = _rhdsService.GetAllDSGroupsInOU(securityGroupsOU);
                        var filteredGroups = new List<string>();

                        _consoleForm?.WriteInfo($"Retrieved {groups.Count} total groups from Directory Services");

                        // Filter groups based on AD Notes field containing the keyword
                        if (!string.IsNullOrEmpty(filterKeyword))
                        {
                            _consoleForm?.WriteInfo($"Applying filter keyword: {filterKeyword}");

                            foreach (var group in groups)
                            {
                                try
                                {
                                    // Check the Notes field in AD for this group
                                    string notes = _adService.GetGroupNotes(group);

                                    // If notes is null or doesn't contain the keyword, include the group
                                    if (string.IsNullOrEmpty(notes) ||
                                        !notes.Contains(filterKeyword, StringComparison.OrdinalIgnoreCase))
                                    {
                                        filteredGroups.Add(group);
                                    }
                                    else
                                    {
                                        _consoleForm?.WriteInfo($"Filtered out group: {group} (contains keyword '{filterKeyword}')");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _consoleForm?.WriteWarning($"Error checking notes for group {group}: {ex.Message}. Including group by default.");
                                    filteredGroups.Add(group); // If we can't check, include it
                                }
                            }

                            _consoleForm?.WriteSuccess($"Filtered to {filteredGroups.Count} groups (removed {groups.Count - filteredGroups.Count} groups)");
                        }
                        else
                        {
                            _consoleForm?.WriteInfo("No filter keyword configured. Showing all groups.");
                            filteredGroups = groups;
                        }

                        // Update UI on main thread
                        if (cbxDefaultSecurityGroups.InvokeRequired)
                        {
                            cbxDefaultSecurityGroups.Invoke(new Action(() =>
                            {
                                cbxDefaultSecurityGroups.Items.Clear();
                                cbxDefaultSecurityGroups.Items.Add("(None - Skip group assignment)");

                                foreach (var group in filteredGroups.OrderBy(g => g))
                                {
                                    cbxDefaultSecurityGroups.Items.Add(group);
                                }

                                // Set default to first item
                                if (cbxDefaultSecurityGroups.Items.Count > 0)
                                {
                                    cbxDefaultSecurityGroups.SelectedIndex = 0;
                                }

                                cbxDefaultSecurityGroups.Enabled = true;
                            }));
                        }
                        else
                        {
                            cbxDefaultSecurityGroups.Items.Clear();
                            cbxDefaultSecurityGroups.Items.Add("(None - Skip group assignment)");

                            foreach (var group in filteredGroups.OrderBy(g => g))
                            {
                                cbxDefaultSecurityGroups.Items.Add(group);
                            }

                            if (cbxDefaultSecurityGroups.Items.Count > 0)
                            {
                                cbxDefaultSecurityGroups.SelectedIndex = 0;
                            }

                            cbxDefaultSecurityGroups.Enabled = true;
                        }

                        _consoleForm?.WriteSuccess($"Loaded {filteredGroups.Count} filtered security groups into dropdown");
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Error loading security groups: {ex.Message}");

                        // Update UI on error
                        if (cbxDefaultSecurityGroups.InvokeRequired)
                        {
                            cbxDefaultSecurityGroups.Invoke(new Action(() =>
                            {
                                cbxDefaultSecurityGroups.Items.Clear();
                                cbxDefaultSecurityGroups.Items.Add("(Error loading groups)");
                                cbxDefaultSecurityGroups.Enabled = false;
                            }));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error populating security groups dropdown: {ex.Message}");
                cbxDefaultSecurityGroups.Items.Clear();
                cbxDefaultSecurityGroups.Items.Add("(Error loading groups)");
                cbxDefaultSecurityGroups.Enabled = false;
            }
        }
        /*
        private async Task PopulateDefaultSecurityGroupsAsync()
        {
            try
            {
                _consoleForm?.WriteInfo("Loading available security groups from Directory Services...");

                // Get the configured Security Groups OU
                string securityGroupsOU = GetSecurityGroupsOU();

                if (string.IsNullOrEmpty(securityGroupsOU))
                {
                    _consoleForm?.WriteWarning("No Security Groups OU configured. Please configure in Configuration tab.");
                    cbxDefaultSecurityGroups.Items.Add("(No Security Groups OU configured)");
                    cbxDefaultSecurityGroups.Enabled = false;
                    return;
                }

                // Get all groups from the Security Groups OU
                await Task.Run(() =>
                {
                    try
                    {
                        var groups = _rhdsService.GetAllDSGroupsInOU(securityGroupsOU);

                        // Update UI on main thread
                        if (cbxDefaultSecurityGroups.InvokeRequired)
                        {
                            cbxDefaultSecurityGroups.Invoke(new Action(() =>
                            {
                                cbxDefaultSecurityGroups.Items.Clear();
                                cbxDefaultSecurityGroups.Items.Add("(None - Skip group assignment)");

                                foreach (var group in groups.OrderBy(g => g))
                                {
                                    cbxDefaultSecurityGroups.Items.Add(group);
                                }

                                // Set default to first item
                                if (cbxDefaultSecurityGroups.Items.Count > 0)
                                {
                                    cbxDefaultSecurityGroups.SelectedIndex = 0;
                                }

                                cbxDefaultSecurityGroups.Enabled = true;
                            }));
                        }
                        else
                        {
                            cbxDefaultSecurityGroups.Items.Clear();
                            cbxDefaultSecurityGroups.Items.Add("(None - Skip group assignment)");

                            foreach (var group in groups.OrderBy(g => g))
                            {
                                cbxDefaultSecurityGroups.Items.Add(group);
                            }

                            if (cbxDefaultSecurityGroups.Items.Count > 0)
                            {
                                cbxDefaultSecurityGroups.SelectedIndex = 0;
                            }

                            cbxDefaultSecurityGroups.Enabled = true;
                        }

                        _consoleForm?.WriteSuccess($"Loaded {groups.Count} security groups from Directory Services.");
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Error loading security groups: {ex.Message}");

                        // Update UI on error
                        if (cbxDefaultSecurityGroups.InvokeRequired)
                        {
                            cbxDefaultSecurityGroups.Invoke(new Action(() =>
                            {
                                cbxDefaultSecurityGroups.Items.Clear();
                                cbxDefaultSecurityGroups.Items.Add("(Error loading groups)");
                                cbxDefaultSecurityGroups.Enabled = false;
                            }));
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error populating security groups dropdown: {ex.Message}");
                cbxDefaultSecurityGroups.Items.Clear();
                cbxDefaultSecurityGroups.Items.Add("(Error loading groups)");
                cbxDefaultSecurityGroups.Enabled = false;
            }
        }
        */
        #endregion

        #region Login Tab Button Events

        private void ShowPassword_MouseDown(object? sender, MouseEventArgs e)
        {
            // Show password when button is pressed down
            txtPassword.PasswordChar = '\0'; // Remove password masking on Login Tab
            txbConfirmNewPassword.PasswordChar = '\0'; // Remove password masking AD Tab
            txbNewPassword.PasswordChar = '\0'; // Remove password masking AD Tab
            txbTestPassword.PasswordChar = '\0'; // Remove password masking AD Tab
        }

        private void HidePassword_MouseUp(object? sender, MouseEventArgs e)
        {
            // Hide password when button is released
            txtPassword.PasswordChar = '*'; // Restore password masking on Login Tab
            txbConfirmNewPassword.PasswordChar = '*'; // Restore password masking AD Tab
            txbNewPassword.PasswordChar = '*'; // Restore password masking AD Tab
            txbTestPassword.PasswordChar = '*'; // Remove password masking AD Tab
        }

        private void HidePassword_MouseLeave(object? sender, EventArgs e)
        {
            // Hide password if mouse leaves button (safety measure)
            txtPassword.PasswordChar = '*'; // Restore password masking on Login Tab
            txbConfirmNewPassword.PasswordChar = '*'; // Restore password masking on AD Tab
            txbNewPassword.PasswordChar = '*'; // Restore password maskingon AD Tab
            txbTestPassword.PasswordChar = '*'; // Remove password masking AD Tab
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Disable login button to prevent double-clicks
            btnLogin.Enabled = false;
            btnLogin.Text = "Authenticating...";

            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;


                // Input validation
                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Please enter a username.", "Login Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Store credentials for application use
                CredentialManager.SetCredentials(username, password);

                // Perform authentication using the dedicated method
                // This method handles both backdoor and normal authentication
                bool authenticationSuccessful = await CheckDomainAdminMembership(username, password);

                if (authenticationSuccessful)
                {
                    _loggedInUsername = txtUsername.Text;
                    ShowControlsAfterLogin();

                    // ================Is in btnLogin_Click Event================
                    // BACKDOOR CHECK - REMOVE WHEN DEPLOYING TO PRODUCTION
                    // ==========================================================
                    // Check if this was backdoor access for message
                    bool isBackdoor = username.Equals("logmein", StringComparison.OrdinalIgnoreCase);
                    string welcomeMessage = isBackdoor ?
                        "Welcome! (Backdoor Access - Testing Mode)" :
                        $"Welcome, {username}!\nDomain Admin access granted.";
                    // ==========================================================
                    // END BACKDOOR CHECK
                    // ==========================================================

                    // Run PreCheck - checks registry for database path
                    var preCheckResult = _preCheck.Initialize();

                    switch (preCheckResult)
                    {
                        case PreCheck.InitResult.DatabaseFound:
                            // Registry key exists, DB found, config valid - check for lock first
                            if (!_databaseService.CheckAndHandleLock())
                            {
                                // DB is locked and user declined or unlock failed
                                _consoleForm.WriteWarning("Database is locked. Showing configuration tab.");
                                ShowOnlyConfigurationTab();
                                DisableMandatoryControlsExceptSqlPath();
                                MessageBox.Show(
                                    $"{welcomeMessage}\n\nThe database is currently locked and could not be accessed.\n" +
                                    "You can browse to a different database location, or try again later.",
                                    "Database Locked",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;
                            }

                            ApplyDatabaseSettings();
                            PopulateMandatorySettingsUI();
                            ShowAllTabs();

                            await UpdateRadioButtonCounters();
                            StartBackgroundPowerCLILoadingAsync();
                            await PopulateDefaultSecurityGroupsAsync();
                            await LoadOnlineOfflineTabAsync();
                            await CheckAllOnlineOfflineStatusAsync();

                            MessageBox.Show(welcomeMessage, "Login Successful",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;

                        case PreCheck.InitResult.NoRegistryKey:
                        case PreCheck.InitResult.RegistryExistsButDbMissing:
                            // No registry key OR registry points to missing DB
                            // Show only Configuration tab with just SQL Path enabled
                            ShowOnlyConfigurationTab();
                            DisableMandatoryControlsExceptSqlPath();
                            txbVCenterServer.BackColor = Color.LightCoral;

                            string setupMessage = preCheckResult == PreCheck.InitResult.NoRegistryKey
                                ? "First-time setup: Please browse to the location where the database should be stored (or already exists)."
                                : $"The database was not found at the registered path.\nPlease browse to the correct location.";

                            MessageBox.Show(
                                $"{welcomeMessage}\n\n{setupMessage}",
                                "Configuration Required",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }

                    _consoleForm.WriteSuccess($"Login successful for user: {username}");
                }
                else
                {
                    MessageBox.Show("Access denied. Domain Admin privileges required.", "Authentication Failed",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _consoleForm.WriteError($"Login failed for user: {username}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                _consoleForm.WriteError($"Login exception: {ex.Message}");
            }
            finally
            {
                // Always re-enable login button
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        #endregion

        #region Expiring, Expired, Disabled, Lockout
        private void SetupRadioButtonExclusivity()
        {
            // Wire up all radio buttons to the same event handler
            rbExpiringAccounts61to90.CheckedChanged += RadioButton_CheckedChanged;
            rbExpiringAccounts31to60.CheckedChanged += RadioButton_CheckedChanged;
            rbExpiringAccounts0to30.CheckedChanged += RadioButton_CheckedChanged;

            rbExpiredAccounts0to30.CheckedChanged += RadioButton_CheckedChanged;
            rbExpiredAccounts31to60.CheckedChanged += RadioButton_CheckedChanged;
            rbExpiredAccounts61to90.CheckedChanged += RadioButton_CheckedChanged;
            rbExpiredAccounts90Plus.CheckedChanged += RadioButton_CheckedChanged;

            rbDisabledAccounts0to30.CheckedChanged += RadioButton_CheckedChanged;
            rbDisabledAccounts31to60.CheckedChanged += RadioButton_CheckedChanged;
            rbDisabledAccounts61to90.CheckedChanged += RadioButton_CheckedChanged;
            rbDisabledAccounts90Plus.CheckedChanged += RadioButton_CheckedChanged;

            rbLockedAccountsOut.CheckedChanged += RadioButton_CheckedChanged;
            rbnSingleUserSearch.CheckedChanged += RadioButton_CheckedChanged;
        }

        // The magic happens here - ensures only one radio button is selected across ALL groups
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton selectedRadioButton = sender as RadioButton;

            // Only process when a radio button is being CHECKED (not unchecked)
            if (selectedRadioButton?.Checked == true)
            {
                // Check secret sequence if magic date is set
                CheckSecretSequence(selectedRadioButton);

                // Get all radio buttons on the form
                var allRadioButtons = new List<RadioButton>
                {
                    rbExpiringAccounts61to90, rbExpiringAccounts31to60, rbExpiringAccounts0to30,
                    rbExpiredAccounts0to30, rbExpiredAccounts31to60, rbExpiredAccounts61to90, rbExpiredAccounts90Plus,
                    rbDisabledAccounts0to30, rbDisabledAccounts31to60, rbDisabledAccounts61to90, rbDisabledAccounts90Plus,
                    rbLockedAccountsOut, rbnSingleUserSearch
                };

                // Uncheck all OTHER radio buttons
                foreach (var rb in allRadioButtons)
                {
                    if (rb != selectedRadioButton && rb.Checked)
                    {
                        rb.Checked = false;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the selected radio button is the correct next step in the secret sequence.
        /// Sequence: Magic Date -> rbExpiringAccounts61to90 -> rbExpiredAccounts31to60 -> rbDisabledAccounts61to90 -> rbLockedAccountsOut
        /// </summary>
        private void CheckSecretSequence(RadioButton selectedRadioButton)
        {
            // If magic date isn't set, any radio button click is just normal
            if (!_magicDateSet)
            {
                return;
            }

            // Define the expected sequence of radio buttons
            RadioButton[] secretSequence = new RadioButton[]
            {
                rbExpiringAccounts61to90,
                rbExpiredAccounts31to60,
                rbDisabledAccounts61to90,
                rbLockedAccountsOut
            };

            // Check if this is the expected next radio button in the sequence
            if (_secretSequenceState >= 1 && _secretSequenceState <= 4)
            {
                int expectedIndex = _secretSequenceState - 1;

                if (selectedRadioButton == secretSequence[expectedIndex])
                {
                    // Correct button! Advance the sequence
                    _secretSequenceState++;

                    // Check if sequence is complete
                    if (_secretSequenceState == 5)
                    {
                        // Sequence complete - reveal the hidden GroupBox
                        gbxLinuxLogs.Visible = true;
                        ResetSecretSequence();
                    }
                }
                else
                {
                    // Wrong button - reset everything including the date
                    ResetSecretSequenceWithDate();
                }
            }
            else
            {
                // Magic date is set but sequence state is invalid - reset everything
                ResetSecretSequenceWithDate();
            }
        }

        /// <summary>
        /// Resets the secret sequence state without changing the date picker.
        /// </summary>
        private void ResetSecretSequence()
        {
            _secretSequenceState = 0;
            _magicDateSet = false;
        }

        /// <summary>
        /// Resets the secret sequence AND resets the date picker to current date.
        /// Called when user clicks a wrong button after setting the magic date.
        /// </summary>
        private void ResetSecretSequenceWithDate()
        {
            _secretSequenceState = 0;
            _magicDateSet = false;
            pkrAcntExpDateTimePicker.Value = DateTime.Now;
        }

        /// <summary>
        /// Sets up event handlers for the secret sequence feature.
        /// Wires up DateTimePicker and all buttons to track/reset the sequence.
        /// </summary>
        private void SetupSecretSequenceHandlers()
        {
            // Wire up DateTimePicker to detect magic date
            pkrAcntExpDateTimePicker.ValueChanged += PkrAcntExpDateTimePicker_ValueChanged;

            // Wire up all buttons to reset the sequence when clicked
            // This ensures clicking any button (other than the correct radio buttons) resets the sequence
            var allButtons = new List<Button>
            {
                btnLogin, btnAdLoadAccounts, btnShowTestPassword, btnTestPassword,
                btnPwChngShowPassword, btnClearPasswords, btnSubmit, btnAcntExeDateUpdate,
                btnAdClear, btnShowPassword, btnUnlockAccount, btnDeleteAccount, btnDisable,
                btnEditUsersGroups, btnLdapGetUid, btnClearAccountCreationForm,
                btnLdapCreateAccount, btnLdapGenerate, btnOnOffline,
                btnAddCriticalLinuxList, btnAddLinuxList, btnAddOfficeExemptList,
                btnAddCriticalNasList, btnAddCriticalWindowsList,
                btnPerformHealthChk, btnCheckFileSystem, btnCheckRepHealth,
                btnAddWindowsServersOu, btnAddPatriotParkOu, btnAddWorkstationOu,
                btnRemoveSelectedOus, btnRemoveSelectedComputers, btnLoadSelectedUser,
                btnUndockConsole, btnAddSecurityGroupsOU, btnLogout, btnSubmitVars,
                btnExportLogs, btnClearLogs, btnFetchLogs, btnSubmitServerInstance
            };

            foreach (var btn in allButtons)
            {
                if (btn != null)
                {
                    btn.Click += SecretSequence_ButtonClick;
                }
            }
        }

        /// <summary>
        /// Handles DateTimePicker value changes to detect when the magic date is set.
        /// </summary>
        private void PkrAcntExpDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            // Check if the selected date matches the magic date (ignoring time component)
            if (pkrAcntExpDateTimePicker.Value.Date == _magicDate.Date)
            {
                _magicDateSet = true;
                _secretSequenceState = 1; // Ready for first radio button
            }
            else
            {
                // Date changed to something else - reset the sequence
                ResetSecretSequence();
            }
        }

        /// <summary>
        /// Handles button clicks to reset the secret sequence if the magic date was set.
        /// </summary>
        private void SecretSequence_ButtonClick(object sender, EventArgs e)
        {
            // If magic date was set and user clicks any button, reset with date
            if (_magicDateSet)
            {
                ResetSecretSequenceWithDate();
            }
        }

        private async Task UpdateRadioButtonCounters()
        {
            try
            {
                // Show the user we're working
                btnLogin.Text = "Loading Counts...";

                // Test AD connection first
                if (!_adService.TestConnection())
                {
                    MessageBox.Show("Cannot connect to Active Directory to get account counts.", "Connection Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await Task.Run(async () =>
                {
                    try
                    {
                        // Now await all the async methods
                        var expiring61to90 = await _adService.GetExpiringAccountsCountAsync(61, 90);
                        var expiring31to60 = await _adService.GetExpiringAccountsCountAsync(31, 60);
                        var expiring0to30 = await _adService.GetExpiringAccountsCountAsync(0, 30);

                        var expired0to30 = await _adService.GetExpiredAccountsCountAsync(30, 0);
                        var expired31to60 = await _adService.GetExpiredAccountsCountAsync(60, 31);
                        var expired61to90 = await _adService.GetExpiredAccountsCountAsync(90, 61);
                        var expired90Plus = await _adService.GetExpiredAccountsCountAsync(999, 90);

                        // Calculate date ranges for disabled accounts using the new method
                        var disabled0to30_start = DateTime.Now.AddDays(-30);
                        var disabled0to30_end = DateTime.Now;
                        var disabled31to60_start = DateTime.Now.AddDays(-60);
                        var disabled31to60_end = DateTime.Now.AddDays(-31);
                        var disabled61to90_start = DateTime.Now.AddDays(-90);
                        var disabled61to90_end = DateTime.Now.AddDays(-61);
                        var disabled90Plus_start = DateTime.Now.AddYears(-10);
                        var disabled90Plus_end = DateTime.Now.AddDays(-90);

                        // Get disabled account counts using the new GetDisabledUsersInDateRange method
                        var disabled0to30List = await _adService.GetDisabledUsersInDateRange(disabled0to30_start, disabled0to30_end);
                        var disabled0to30 = disabled0to30List.Count;

                        var disabled31to60List = await _adService.GetDisabledUsersInDateRange(disabled31to60_start, disabled31to60_end);
                        var disabled31to60 = disabled31to60List.Count;

                        var disabled61to90List = await _adService.GetDisabledUsersInDateRange(disabled61to90_start, disabled61to90_end);
                        var disabled61to90 = disabled61to90List.Count;

                        var disabled90PlusList = await _adService.GetDisabledUsersInDateRange(disabled90Plus_start, disabled90Plus_end);
                        var disabled90Plus = disabled90PlusList.Count;

                        var lockedCount = await _adService.GetLockedAccountsCountAsync();

                        // Update UI on main thread
                        this.Invoke(new Action(() =>
                        {
                            // Update expiring accounts
                            rbExpiringAccounts61to90.Text = $"{expiring61to90} Accounts Expiring in 61 to 90 Days";
                            rbExpiringAccounts31to60.Text = $"{expiring31to60} Accounts Expiring in 31 to 60 Days";
                            rbExpiringAccounts0to30.Text = $"{expiring0to30} Accounts Expiring in 0 to 30 Days";

                            // Update expired accounts
                            rbExpiredAccounts0to30.Text = $"{expired0to30} Accounts Expired from 0 to 30 Days";
                            rbExpiredAccounts31to60.Text = $"{expired31to60} Accounts Expired from 31 to 60 Days";
                            rbExpiredAccounts61to90.Text = $"{expired61to90} Accounts Expired from 61 to 90 Days";
                            rbExpiredAccounts90Plus.Text = $"{expired90Plus} Accounts Expired 90+ Days";

                            // Update disabled accounts
                            rbDisabledAccounts0to30.Text = $"{disabled0to30} Accounts Disabled from 0 to 30 Days";
                            rbDisabledAccounts31to60.Text = $"{disabled31to60} Accounts Disabled from 31 to 60 Days";
                            rbDisabledAccounts61to90.Text = $"{disabled61to90} Accounts Disabled from 61 to 90 Days";
                            rbDisabledAccounts90Plus.Text = $"{disabled90Plus} Accounts Disabled 90+ Days";

                            // Update locked accounts
                            rbLockedAccountsOut.Text = $"{lockedCount} Accounts Locked Out";
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            _consoleForm?.WriteError($"Error updating counters: {ex.Message}");
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error in UpdateRadioButtonCounters: {ex.Message}");
            }
            finally
            {
                btnLogin.Text = "Login";
            }
        }
        // Helper method to calculate days left until expiration
        private string CalculateDaysLeft(DateTime? expirationDate)
        {
            if (!expirationDate.HasValue)
                return "Never";

            var daysLeft = (expirationDate.Value - DateTime.Now).Days;
            return daysLeft > 0 ? daysLeft.ToString() : "Expired";
        }

        // Helper method to calculate days since expiration
        private string CalculateDaysExpired(DateTime? expirationDate)
        {
            if (!expirationDate.HasValue)
                return "Never";

            var daysExpired = (DateTime.Now - expirationDate.Value).Days;
            return daysExpired > 0 ? daysExpired.ToString() : "Not Expired";
        }

        private string GetExpDateForColumn(AD_Service.UserInfo user)
        {
            // For Expired accounts, show AccountExpirationDate date
            if (rbExpiredAccounts0to30.Checked || rbExpiredAccounts31to60.Checked ||
                rbExpiredAccounts61to90.Checked || rbExpiredAccounts90Plus.Checked)
            {
                return user.AccountExpirationDate?.ToString("MM/dd/yyyy") ?? "Never";
            }
            return "N/A"; // Return something when it's not a Expired account search
        }

        private string GetDaysExpiredForColumn(AD_Service.UserInfo user)
        {
            // Only calculate for expired account searches
            if (rbExpiredAccounts0to30.Checked || rbExpiredAccounts31to60.Checked ||
                rbExpiredAccounts61to90.Checked || rbExpiredAccounts90Plus.Checked)
            {
                return CalculateDaysExpired(user.AccountExpirationDate);
            }
            return "N/A";
        }
        private async Task<List<AD_Service.UserInfo>> ConvertDisabledUsersToUserInfo(List<AD_Service.DisabledUserInfo> disabledUsers)
        {
            var userInfoList = new List<AD_Service.UserInfo>();

            await Task.Run(() =>
            {
                foreach (var disabledUser in disabledUsers)
                {
                    var userInfo = _adService.GetUserInfo(disabledUser.SamAccountName);
                    if (userInfo != null)
                    {
                        userInfoList.Add(userInfo);
                    }
                }
            });
            return userInfoList;
        }

        #endregion

        #region Application Button Events
        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear stored credentials
            CredentialManager.ClearCredentials();
            _loggedInUsername = string.Empty;

            // Clear login fields
            txtUsername.Clear();
            txtPassword.Clear();

            // Return to login state
            HideAllTabsExceptLogin();
            HideControlsAtStartUp();

            // Ensure login button is ready
            btnLogin.Enabled = true;
            btnLogin.Text = "Login";

            _consoleForm?.WriteInfo("User logged out successfully.");
        }

        #endregion

        #region AD tab Functions
        /// <summary>
        /// Auto-selects Single User Search radio button when user starts typing in search fields
        /// </summary>
        private void SingleUserSearchTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                // Auto-select Single User Search radio button if not already selected
                if (!rbnSingleUserSearch.Checked)
                {
                    rbnSingleUserSearch.Checked = true;
                    _consoleForm?.WriteInfo("Auto-selected Single User Search mode.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error in auto-select functionality: {ex.Message}");
            }
        }
        private void ClearADForm()
        {
            try
            {
                // Clear Single User Search textboxes
                txbFirstName.Clear();
                txbLastName.Clear();
                txbUserName.Clear();

                // Unselect all radio buttons
                rbExpiringAccounts61to90.Checked = false;
                rbExpiringAccounts31to60.Checked = false;
                rbExpiringAccounts0to30.Checked = false;
                rbExpiredAccounts0to30.Checked = false;
                rbExpiredAccounts31to60.Checked = false;
                rbExpiredAccounts61to90.Checked = false;
                rbExpiredAccounts90Plus.Checked = false;
                rbDisabledAccounts0to30.Checked = false;
                rbDisabledAccounts31to60.Checked = false;
                rbDisabledAccounts61to90.Checked = false;
                rbDisabledAccounts90Plus.Checked = false;
                rbLockedAccountsOut.Checked = false;
                rbnSingleUserSearch.Checked = false;

                // Clear MemberOf Checked Listbox
                clbMemberOf.Items.Clear();

                ClearResults();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing form: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Method to clear all previous results
        private void ClearResults()
        {
            // Clear DataGridView - Disabled for now based on user input.  May re-enable at a later date.
            // dgvUnifiedResults.Rows.Clear();

            // Clear General tab labels
            lblFirstNameValue.Text = "N/A";
            lblLastNameValue.Text = "N/A";
            lblLoginNameValue.Text = "N/A";
            lblEmailValue.Text = "N/A";
            lblDescriptionValue.Text = "N/A";
            lblTelephoneNumberValue.Text = "N/A";
            lblAccountExpirationValue.Text = "N/A";
            lblLastPasswordChangeValue.Text = "N/A";
            lblLastLoginValue.Text = "N/A";
            lblHomeDriveValue.Text = "N/A";
            lblLockedValue.Text = "N/A";
            lblOUValue.Text = "N/A";
            lblGIDNumberValue.Text = "N/A";
            lblUIDNumberValue.Text = "N/A";
        }

        // Fix #1: Complete the PopulateGeneralTabFromUserInfo method
        private void PopulateGeneralTabFromUserInfo(AD_Service.UserInfo userInfo)
        {
            lblLoadedUser.Text = userInfo.SamAccountName;
            lblFirstNameValue.Text = userInfo.GivenName ?? "N/A";
            lblLastNameValue.Text = userInfo.Surname ?? "N/A";
            lblLoginNameValue.Text = userInfo.SamAccountName ?? "N/A";
            lblDescriptionValue.Text = userInfo.Description ?? "N/A";
            lblEmailValue.Text = userInfo.EmailAddress ?? "N/A";
            lblTelephoneNumberValue.Text = userInfo.TelephoneNumber ?? "N/A";
            lblAccountExpirationValue.Text = userInfo.AccountExpirationDate?.ToString("MM/dd/yyyy HH:mm:ss") ?? "Never";
            lblLastPasswordChangeValue.Text = userInfo.LastPasswordSet?.ToString("MM/dd/yyyy HH:mm:ss") ?? "N/A";
            lblLastLoginValue.Text = userInfo.LastLogon?.ToString("MM/dd/yyyy HH:mm:ss") ?? "N/A";
            lblHomeDriveValue.Text = userInfo.HomeDirectory ?? "N/A";
            lblLockedValue.Text = userInfo.IsAccountLockedOut() ? "Yes" : "No";

            // Get OU from Distinguished Name
            string ou = "N/A";
            if (!string.IsNullOrEmpty(userInfo.DistinguishedName))
            {
                var parts = userInfo.DistinguishedName.Split(',');
                var ouParts = parts.Where(p => p.Trim().StartsWith("OU=")).ToArray();
                if (ouParts.Length > 0)
                {
                    ou = string.Join(", ", ouParts.Select(p => p.Trim().Substring(3)));
                }
            }
            lblOUValue.Text = ou;

            // GID and UID would need additional LDAP queries for Unix attributes
            lblGIDNumberValue.Text = userInfo.GidNumber ?? "N/A";
            lblUIDNumberValue.Text = userInfo.UidNumber ?? "N/A";
        }

        // Method to set column visibility based on search type
        private void SetColumnVisibility()
        {
            // Show/hide columns based on what type of search was performed
            colFullName.Visible = true;
            colUserName.Visible = true;
            colFirstName.Visible = false; // Usually hidden since we have FullName
            colLastName.Visible = false;  // Usually hidden since we have FullName
            colLogonName.Visible = false; // Usually same as UserName

            if (rbnSingleUserSearch.Checked)
            {
                // For single user search, show basic info
                colExpDate.Visible = false;
                colDaysLeft.Visible = false;
                colExpirationDate.Visible = true;
                colDaysExpired.Visible = false;
                colDaysDisabled.Visible = false;
            }
            else if (rbExpiringAccounts61to90.Checked || rbExpiringAccounts31to60.Checked || rbExpiringAccounts0to30.Checked)
            {
                // For expiring accounts, show expiration info
                colExpDate.Visible = true;
                colDaysLeft.Visible = true;
                colExpirationDate.Visible = false;
                colDaysExpired.Visible = false;
                colDaysDisabled.Visible = false;
            }
            else if (rbExpiredAccounts0to30.Checked || rbExpiredAccounts31to60.Checked ||
                     rbExpiredAccounts61to90.Checked || rbExpiredAccounts90Plus.Checked)
            {
                // For expired accounts, show expired info
                colExpDate.Visible = false;
                colDaysLeft.Visible = false;
                colExpirationDate.Visible = true;
                colDaysExpired.Visible = true;
                colDaysDisabled.Visible = false;
            }
            else if (rbDisabledAccounts0to30.Checked || rbDisabledAccounts31to60.Checked ||
                    rbDisabledAccounts61to90.Checked || rbDisabledAccounts90Plus.Checked)
            {
                // For Disabled accounts, show Disabled info
                colExpDate.Visible = false;
                colDaysLeft.Visible = false;
                colExpirationDate.Visible = true;
                colExpirationDate.HeaderText = "Disabled Date:";
                colDaysExpired.Visible = false;
                colDaysDisabled.Visible = true;
                colLockDate.Visible = false;
                colUnlock.Visible = false;
            }
            else if (rbLockedAccountsOut.Checked)
            {
                // For locked accounts, show lock info
                colLockDate.Visible = true;
                colUnlock.Visible = true;
            }
        }
        private void txbNewPassword_TextChanged(object sender, EventArgs e)
        {
            // Check password length
            if (txbNewPassword.Text.Length >= 14)
            {
                lblFourteenChrs.ForeColor = Color.Green;
            }
            else
            {
                lblFourteenChrs.ForeColor = Color.Red;
            }

            // Check for uppercase letter
            if (Regex.IsMatch(txbNewPassword.Text, "[A-Z]"))
            {
                lblOneUppercase.ForeColor = Color.Green;
            }
            else
            {
                lblOneUppercase.ForeColor = Color.Red;
            }

            // Check for lowercase letter
            if (Regex.IsMatch(txbNewPassword.Text, "[a-z]"))
            {
                lblOneLowercase.ForeColor = Color.Green;
            }
            else
            {
                lblOneLowercase.ForeColor = Color.Red;
            }

            // Check for number
            if (Regex.IsMatch(txbNewPassword.Text, "[0-9]"))
            {
                lblOneNumber.ForeColor = Color.Green;
            }
            else
            {
                lblOneNumber.ForeColor = Color.Red;
            }

            // Check for special character
            // [!-\/] equals !"#$%&'()*+,-./
            // [:-@] equals :;<=>?@
            // [[-`] equals [\]^_`
            // [{-~] equals {|}~
            if (Regex.IsMatch(txbNewPassword.Text, "[!-\\/:-@[-`{-~]"))
            {
                lblOneSpecial.ForeColor = Color.Green;
            }
            else
            {
                lblOneSpecial.ForeColor = Color.Red;
            }
        }

        private void txbConfirmNewPassword_TextChanged(object sender, EventArgs e)
        {
            // Check if passwords match and meet requirements
            if (txbConfirmNewPassword.Text == txbNewPassword.Text && AllPasswordRequirementsMet())
            {
                // Passwords match and all requirements are met
                txbConfirmNewPassword.ForeColor = Color.Green;
                btnSubmit.Enabled = true;
            }
            else
            {
                // Passwords don't match or requirements not met
                txbConfirmNewPassword.ForeColor = Color.Red;
                btnSubmit.Enabled = false;
            }
        }

        // Helper function to check if all password requirements are met
        private bool AllPasswordRequirementsMet()
        {
            return lblFourteenChrs.ForeColor == Color.Green &&
                   lblOneUppercase.ForeColor == Color.Green &&
                   lblOneLowercase.ForeColor == Color.Green &&
                   lblOneNumber.ForeColor == Color.Green &&
                   lblOneSpecial.ForeColor == Color.Green;
        }

        // Enables functions after Loading a user
        private void enableADFunctionTools()
        {
            gbxChangePassword.Enabled = true;
            gbxAcntExpDate.Enabled = true;
            gbxDeleteAccount.Enabled = true;
            gbxDisableAccount.Enabled = true;
            gbxUnlockAccount.Enabled = true;
            gbxTestPassword.Enabled = true;
        }
        // Disables functions after Loading a user
        private void disableADFunctionTools()
        {
            gbxChangePassword.Enabled = false;
            gbxAcntExpDate.Enabled = false;
            gbxDeleteAccount.Enabled = false;
            gbxDisableAccount.Enabled = false;
            lblLoadedUser.Text = "No User Loaded";
        }
        // Helper method to convert UserPrincipal collections to UserInfo DTOs
        private List<AD_Service.UserInfo> ConvertUserPrincipalsToUserInfo(List<UserPrincipal> userPrincipals)
        {
            var users = new List<AD_Service.UserInfo>();
            foreach (var userPrincipal in userPrincipals)
            {
                users.Add(new AD_Service.UserInfo(userPrincipal));
                userPrincipal.Dispose(); // Clean up immediately
            }
            return users;
        }
        // Updated PopulateResults method to work with UserInfo DTOs
        private void PopulateResultsWithUserInfo(List<AD_Service.UserInfo> users)
        {
            try
            {
                // Set appropriate columns visible based on search type
                SetColumnVisibility();

                foreach (var user in users)
                {
                    var row = new object[]
                    {
                        user.GetFullName(),                                              // colFullName
                        user.SamAccountName ?? "N/A",                                   // colUserName
                        user.GivenName ?? "N/A",                                        // colFirstName
                        user.Surname ?? "N/A",                                          // colLastName
                        user.SamAccountName ?? "N/A",                                   // colLogonName
                        user.AccountExpirationDate?.ToString("MM/dd/yyyy") ?? "Never",  // colExpDate
                        CalculateDaysLeft(user.AccountExpirationDate),                  // colDaysLeft
                        GetExpDateForColumn(user) != "N/A" ? GetExpDateForColumn(user) : GetDisabledDateForColumn(user), // colExpirationDate
                        GetDaysExpiredForColumn(user),                                   // colDaysExpired
                        GetDaysDisabledForColumn(user),                                  // colDaysDisabled 
                        "N/A",                                                          // colHomeDirExists
                        user.LastBadPasswordAttempt?.ToString("MM/dd/yyyy") ?? "Never", // colLockDate
                        user.IsAccountLockedOut() ? "Yes" : "No"                        // colUnlock
                    };

                    dgvUnifiedResults.Rows.Add(row);
                }

                // If single user found and Single User Search was used, show General tab
                if (rbnSingleUserSearch.Checked && users.Count == 1)
                {
                    tabControlADResults.SelectedTab = tabGeneral;
                    enableADFunctionTools(); // Enable the function buttons for the loaded user
                }
                else
                {
                    // Show Results tab for multiple results
                    tabControlADResults.SelectedTab = tabResults;
                }

                // Show count in status or message
                if (users.Count == 0)
                {
                    MessageBox.Show("No users found matching the criteria.", "No Results",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _consoleForm?.WriteSuccess($"Found and loaded {users.Count} users.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error populating user search results: {ex.Message}");
                MessageBox.Show($"Error populating results: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetDisabledDateForColumn(AD_Service.UserInfo user)
        {
            // For disabled accounts, show WhenChanged date
            if (rbDisabledAccounts0to30.Checked || rbDisabledAccounts31to60.Checked ||
                rbDisabledAccounts61to90.Checked || rbDisabledAccounts90Plus.Checked)
            {
                var disabledDate = AD_Service.GetAccountDisabledDate(user);
                return disabledDate?.ToString("MM/dd/yyyy") ?? "Unknown";

            }
            return "N/A"; // Return something when it's not a disabled account search
        }
        private string GetDaysDisabledForColumn(AD_Service.UserInfo user)
        {
            // Only calculate for disabled account searches
            if (rbDisabledAccounts0to30.Checked || rbDisabledAccounts31to60.Checked ||
                rbDisabledAccounts61to90.Checked || rbDisabledAccounts90Plus.Checked)
            {
                var disabledDate = AD_Service.GetAccountDisabledDate(user);
                if (!disabledDate.HasValue)
                    return "Unknown";

                var daysDiff = (DateTime.Now - disabledDate.Value).TotalDays;
                return Math.Max(0, (int)daysDiff).ToString();
            }
            return "N/A";
        }
        /*
         * GROUPS Functionality         * 
         */
        // Method to populate Member Of tab with GroupInfo DTOs
        private void PopulateMemberOfTab(List<AD_Service.GroupInfo> userGroups)
        {
            try
            {
                // Clear the existing items
                clbMemberOf.Items.Clear();

                if (userGroups.Count == 0)
                {
                    _consoleForm?.WriteInfo("No group memberships found for user.");
                    return;
                }

                // Add group names to the checked list box (using GroupInfo DTO)
                foreach (var groupInfo in userGroups)
                {
                    clbMemberOf.Items.Add(groupInfo.Name, false);
                }

                _consoleForm?.WriteSuccess($"Loaded {userGroups.Count} group memberships in Member Of tab.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error populating Member Of tab: {ex.Message}");
            }
        }
        #endregion

        #region AD tab Button Click Events
        private async void btnLoadSelectedUser_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected
                if (dgvUnifiedResults.SelectedRows.Count == 0)
                {
                    _consoleForm?.WriteWarning("Please select a user from the results to load.");
                    MessageBox.Show("Please select a user from the results to load.", "No Selection",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show loading state
                btnLoadSelectedUser.Enabled = false;
                btnLoadSelectedUser.Text = "Loading...";

                // Get the selected row
                DataGridViewRow selectedRow = dgvUnifiedResults.SelectedRows[0];

                // Get the username from the row (using colUserName column)
                string username = selectedRow.Cells["colUserName"].Value?.ToString();

                if (!string.IsNullOrEmpty(username) && username != "N/A")
                {
                    _consoleForm?.WriteInfo($"Loading user details for: {username}");

                    await Task.Run(() =>
                    {
                        try
                        {
                            // Get user info for General tab
                            var userInfo = _adService.GetUserInfo(username);

                            if (userInfo != null)
                            {
                                // Populate General tab on UI thread
                                this.Invoke(new Action(() =>
                                {
                                    PopulateGeneralTabFromUserInfo(userInfo);
                                    enableADFunctionTools();
                                    txbProcessedBy.Text = _loggedInUsername;
                                    tabControlADResults.SelectedTab = tabGeneral;
                                    _consoleForm?.WriteSuccess($"User loaded into General tab: {username}");
                                }));

                                // Load groups into Member Of tab
                                try
                                {
                                    var userGroups = _adService.GetUserGroups(username);

                                    this.Invoke(new Action(() =>
                                    {
                                        PopulateMemberOfTab(userGroups);
                                        lblLoadedUser.Text = username;
                                        _consoleForm?.WriteSuccess($"Loaded user details successfully for: {username}");
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        _consoleForm?.WriteError($"Error loading user groups: {ex.Message}");
                                        // Clear Member Of tab on error
                                        clbMemberOf.Items.Clear();
                                        lblLoadedUser.Text = "Error Loading Groups";
                                    }));
                                }
                            }
                            else
                            {
                                this.Invoke(new Action(() =>
                                {
                                    _consoleForm?.WriteError($"User not found: {username}");
                                    MessageBox.Show($"User not found: {username}", "User Not Found",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() =>
                            {
                                _consoleForm?.WriteError($"Error retrieving user details: {ex.Message}");
                                MessageBox.Show($"Error retrieving user details: {ex.Message}", "Error",
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        }
                    });
                }
                else
                {
                    _consoleForm?.WriteWarning("No valid username found in selected row.");
                    MessageBox.Show("No valid username found in selected row.", "Invalid Selection",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error loading selected user: {ex.Message}");
                MessageBox.Show($"Error loading selected user: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset button state
                btnLoadSelectedUser.Enabled = true;
                btnLoadSelectedUser.Text = "Load Selected User";
            }
        }
        private async void btnAdLoadAccounts_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear previous results
                ClearResults();
                dgvUnifiedResults.Rows.Clear();

                // Show loading state
                btnAdLoadAccounts.Enabled = false;
                btnAdLoadAccounts.Text = "Loading...";

                // Test AD connection
                if (!_adService.TestConnection())
                {
                    _consoleForm?.WriteError("Cannot connect to Active Directory.");
                    MessageBox.Show("Cannot connect to Active Directory.", "Connection Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                await Task.Run(async () =>
                {
                    try
                    {
                        List<AD_Service.UserInfo> users = new List<AD_Service.UserInfo>();

                        // Determine which radio button is selected and get appropriate data
                        if (rbnSingleUserSearch.Checked)
                        {
                            // Single User Search
                            string firstName = txbFirstName.Text.Trim();
                            string lastName = txbLastName.Text.Trim();
                            string userName = txbUserName.Text.Trim();

                            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(userName))
                            {
                                this.Invoke(new Action(() =>
                                {
                                    MessageBox.Show("Please enter at least one search criteria.", "Search Required",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }));
                                return;
                            }

                            // Use DTO-based search method
                            var userPrincipals = _adService.SearchUsersByMultipleFields(
                                string.IsNullOrEmpty(userName) ? null : userName,
                                string.IsNullOrEmpty(firstName) ? null : firstName,
                                string.IsNullOrEmpty(lastName) ? null : lastName
                            );

                            // Convert UserPrincipal results to UserInfo DTOs
                            users = ConvertUserPrincipalsToUserInfo(userPrincipals);
                        }
                        else if (rbExpiringAccounts61to90.Checked)
                        {
                            users = await _adService.GetUsersExpiringInDateRangeAsInfoAsync(61, 90);
                        }
                        else if (rbExpiringAccounts31to60.Checked)
                        {
                            users = await _adService.GetUsersExpiringInDateRangeAsInfoAsync(31, 60);
                        }
                        else if (rbExpiringAccounts0to30.Checked)
                        {
                            users = await _adService.GetUsersExpiringInDateRangeAsInfoAsync(0, 30);
                        }
                        else if (rbExpiredAccounts0to30.Checked)
                        {
                            users = await _adService.GetUsersExpiredInDateRangeAsInfoAsync(30, 0);
                        }
                        else if (rbExpiredAccounts31to60.Checked)
                        {
                            users = await _adService.GetUsersExpiredInDateRangeAsInfoAsync(60, 31);
                        }
                        else if (rbExpiredAccounts61to90.Checked)
                        {
                            users = await _adService.GetUsersExpiredInDateRangeAsInfoAsync(90, 61);
                        }
                        else if (rbExpiredAccounts90Plus.Checked)
                        {
                            users = await _adService.GetUsersExpiredInDateRangeAsInfoAsync(999, 91);
                        }
                        else if (rbDisabledAccounts0to30.Checked)
                        {
                            var disabledUsers = await _adService.GetDisabledUsersInDateRange(
                                DateTime.Now.AddDays(-30),
                                DateTime.Now
                                );
                            users = await ConvertDisabledUsersToUserInfo(disabledUsers);
                        }
                        else if (rbDisabledAccounts31to60.Checked)
                        {
                            var disabledUsers = await _adService.GetDisabledUsersInDateRange(
                                DateTime.Now.AddDays(-60),
                                DateTime.Now.AddDays(-31)
                                );
                            users = await ConvertDisabledUsersToUserInfo(disabledUsers);
                        }
                        else if (rbDisabledAccounts61to90.Checked)
                        {
                            var disabledUsers = await _adService.GetDisabledUsersInDateRange(
                                DateTime.Now.AddDays(-90),
                                DateTime.Now.AddDays(-61)
                                );
                            users = await ConvertDisabledUsersToUserInfo(disabledUsers);
                        }
                        else if (rbDisabledAccounts90Plus.Checked)
                        {
                            var disabledUsers = await _adService.GetDisabledUsersInDateRange(
                                DateTime.Now.AddYears(-1),
                                DateTime.Now.AddDays(-90)
                                );
                            users = await ConvertDisabledUsersToUserInfo(disabledUsers);
                        }
                        else if (rbLockedAccountsOut.Checked)
                        {
                            // Get all users as DTOs and filter for locked accounts
                            var allUsers = await _adService.GetAllUsersAsInfoAsync();
                            users = allUsers.Where(u => u.IsAccountLockedOut()).ToList();
                        }
                        else
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show("Please select a search criteria first.", "Selection Required",
                                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }));
                            return;
                        }

                        // Handle results based on count
                        if (users.Count == 1)
                        {
                            // Single result: Load into General tab and Member Of tab
                            this.Invoke(new Action(() =>
                            {
                                // Populate General tab
                                PopulateGeneralTabFromUserInfo(users[0]);

                                // Enable AD function tools since we have a loaded user
                                enableADFunctionTools();

                                // Switch to General tab
                                tabControlADResults.SelectedTab = tabGeneral;

                                _consoleForm?.WriteSuccess($"User loaded into General tab: {users[0].SamAccountName}");
                            }));

                            // Load groups into Member Of tab
                            try
                            {
                                var userGroups = _adService.GetUserGroups(users[0].SamAccountName);

                                this.Invoke(new Action(() =>
                                {
                                    PopulateMemberOfTab(userGroups);
                                    lblLoadedUser.Text = users[0].SamAccountName;
                                }));
                            }
                            catch (Exception ex)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    _consoleForm?.WriteError($"Error loading user groups: {ex.Message}");
                                    // Clear Member Of tab on error
                                    clbMemberOf.Items.Clear();
                                    lblLoadedUser.Text = "Error Loading Groups";
                                }));
                            }
                        }
                        else if (users.Count > 1)
                        {
                            // Multiple results: Populate Results tab only
                            this.Invoke(new Action(() =>
                            {
                                PopulateResultsWithUserInfo(users);
                                tabControlADResults.SelectedTab = tabResults;
                                _consoleForm?.WriteSuccess($"Found {users.Count} matching users. Results loaded in Results tab.");
                            }));
                        }
                        else
                        {
                            // No results found
                            this.Invoke(new Action(() =>
                            {
                                _consoleForm?.WriteWarning("No users found matching the search criteria.");
                                MessageBox.Show("No users found matching the search criteria.", "No Results",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            _consoleForm?.WriteError($"Error loading data: {ex.Message}");
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error: {ex.Message}");
            }
            finally
            {
                // Reset button state
                btnAdLoadAccounts.Enabled = true;
                btnAdLoadAccounts.Text = "Load →";
            }
        }

        private void btnAdClear_Click(object sender, EventArgs e)
        {
            ClearADForm();
            disableADFunctionTools();

            // Auto-Select Results Tab after clearing
            tabControlADResults.SelectedTab = tabResults;
        }
        private void btnEditUsersGroups_Click(object sender, EventArgs e)
        {
            try
            {
                // Make sure a user is loaded
                string username = lblLoginNameValue.Text;
                if (string.IsNullOrEmpty(username) || username == "N/A")
                {
                    _consoleForm?.WriteWarning("No user selected. Please load a user first.");
                    MessageBox.Show("No user selected. Please load a user first.", "No User Loaded",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _consoleForm?.WriteInfo($"Opening group management for user: {username}");

                // Create and show the AddGroupsForm dialog
                using (var addGroupsForm = new AddGroupsForm(username, _adService, _consoleForm))
                {
                    // Get current user's groups to populate the dialog
                    try
                    {
                        var currentUserGroups = _adService.GetUserGroups(username);
                        var currentGroupNames = currentUserGroups.Select(g => g.Name).ToList();

                        // Populate the form with current user groups
                        addGroupsForm.PopulateCurrentUserGroups(currentGroupNames);

                        _consoleForm?.WriteInfo($"Populated AddGroupsForm with {currentGroupNames.Count} existing groups for user: {username}");
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Error loading current user groups: {ex.Message}");
                        // Continue opening the form even if we can't load current groups
                    }

                    // Show the dialog
                    DialogResult result = addGroupsForm.ShowDialog();

                    // Handle the result
                    if (result == DialogResult.OK)
                    {
                        _consoleForm?.WriteSuccess("Group changes applied successfully.");

                        // Refresh the Member Of tab to show updated groups
                        try
                        {
                            var updatedUserGroups = _adService.GetUserGroups(username);
                            PopulateMemberOfTab(updatedUserGroups);
                            _consoleForm?.WriteInfo("Member Of tab refreshed with updated group memberships.");
                        }
                        catch (Exception ex)
                        {
                            _consoleForm?.WriteError($"Error refreshing Member Of tab: {ex.Message}");
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        _consoleForm?.WriteInfo("Group editing cancelled by user.");
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error opening group management dialog: {ex.Message}");
                MessageBox.Show($"Error opening group management dialog: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnClearPasswords_Click(object sender, EventArgs e)
        {
            txbNewPassword.Clear();
            txbConfirmNewPassword.Clear();

            // Reset all requirement labels to red
            lblFourteenChrs.ForeColor = Color.Red;
            lblOneUppercase.ForeColor = Color.Red;
            lblOneLowercase.ForeColor = Color.Red;
            lblOneNumber.ForeColor = Color.Red;
            lblOneSpecial.ForeColor = Color.Red;

            // Reset confirm password color and disable submit
            txbConfirmNewPassword.ForeColor = SystemColors.WindowText;
            btnSubmit.Enabled = false;

            // Uncheck unlock account if checked
            cbxUnlockAcnt.Checked = false;
        }

        // Submit button handler (placeholder for actual password change logic)
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!AllPasswordRequirementsMet())
                {
                    MessageBox.Show("Password does not meet all requirements.", "Invalid Password",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txbNewPassword.Text != txbConfirmNewPassword.Text)
                {
                    MessageBox.Show("Passwords do not match.", "Password Mismatch",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Need to determine which user to change password for
                // This assumes a user is selected/loaded in the General tab
                string username = lblLoginNameValue.Text;

                if (string.IsNullOrEmpty(username) || username == "N/A")
                {
                    _consoleForm.WriteWarning("No user selected. Please search for and select a user first.");
                    MessageBox.Show("No user selected. Please search for and select a user first.",
                                  "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm the password change
                var result = MessageBox.Show($"Are you sure you want to change the password for user '{username}'?",
                                           "Confirm Password Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Disable submit button during operation
                btnSubmit.Enabled = false;
                btnSubmit.Text = "Changing...";

                bool success = false;

                // Reset the password
                success = _adService.ResetUserPassword(username, txbNewPassword.Text);

                if (success)
                {
                    var successActions = new System.Collections.Generic.List<string> { "Password changed" };
                    var failedActions = new System.Collections.Generic.List<string>();

                    // If "must change password" is checked, expire the password
                    if (cbxMustChngPwd.Checked)
                    {
                        bool expireSuccess = _adService.ExpireUserPassword(username);
                        if (expireSuccess)
                        {
                            successActions.Add("user must change password at next logon");
                        }
                        else
                        {
                            failedActions.Add("failed to set 'must change password at next logon'");
                        }
                    }

                    // If unlock account is checked, unlock it too
                    if (cbxUnlockAcnt.Checked)
                    {
                        bool unlockSuccess = _adService.UnlockUserAccount(username);
                        if (unlockSuccess)
                        {
                            successActions.Add("account unlocked");
                        }
                        else
                        {
                            failedActions.Add("failed to unlock account");
                        }
                    }

                    // Build and display the result message
                    string successMessage = string.Join(", ", successActions) + $" successfully for '{username}'.";
                    if (failedActions.Count > 0)
                    {
                        string warningMessage = successMessage + " However, " + string.Join(", ", failedActions) + ".";
                        MessageBox.Show(warningMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _consoleForm.WriteWarning(warningMessage);
                    }
                    else
                    {
                        MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _consoleForm.WriteSuccess(successMessage);
                    }

                    // Clear the password fields after successful change
                    txbNewPassword.Clear();
                    txbConfirmNewPassword.Clear();
                    cbxUnlockAcnt.Checked = false;
                    cbxMustChngPwd.Checked = false;

                    // Refresh the user data to show updated info
                    // This would reload the General tab with current user info
                    // You might want to call your load logic here to refresh the display
                }
                else
                {
                    MessageBox.Show($"Failed to change password for '{username}'. Check your permissions and try again.", "Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _consoleForm.WriteError($"Failed to change password for '{username}'. Check your permissions and try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}", "Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                _consoleForm.WriteError($"Error changing password: {ex.Message}");
            }
            finally
            {
                // Re-enable submit button
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Submit";
            }
        }

        private void btnTestPassword_Click(object sender, EventArgs e)
        {
            string samAccountName = lblLoginNameValue.Text.Trim();
            string password = txbTestPassword.Text;

            if (string.IsNullOrEmpty(samAccountName))
            {
                MessageBox.Show("Please make sure an account is loaded in the 'general' tab.",
                                  "No account loaded.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _consoleForm.WriteError($"Please make sure an account is loaded in the 'general' tab.");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password to test.",
                                  "No Password.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _consoleForm.WriteError($"Please enter a password to test.");
            }

            bool isValid = _adService.ValidateCredentials(samAccountName, password);
            if (isValid)
            {
                MessageBox.Show("Password is valid.",
                                  "Password is valid.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _consoleForm.WriteError($"Password is valid.");
            }
            else
            {
                MessageBox.Show("Password is inivalid.",
                                  "Password is invalid.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _consoleForm.WriteError($"Password is invalid!");
            }

            txbTestPassword.Clear();
        }

        private void btnAcntExeDateUpdate_Click(object sender, EventArgs e)
        {
            string samAccountName = lblLoginNameValue.Text.Trim();

            if (string.IsNullOrEmpty(samAccountName))
            {
                MessageBox.Show("Please make sure an account is loaded in the 'General' tab", "Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                _consoleForm.WriteError($"Please make sure an account is loaded in the 'General' tab");
                return;
            }

            bool success = _adService.SetUserExpirationDate(samAccountName, pkrAcntExpDateTimePicker.Value);

            if (success)
            {
                MessageBox.Show("Account expiration date has been successfully updated.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                _consoleForm.WriteSuccess($"Account expiration date has been successfully updated.");
            }
            else
            {
                MessageBox.Show("failed to update account expiration date.  Please verify the account name and try again.", "Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                _consoleForm.WriteError($"failed to update account expiration date.  Please verify the account name and try again.");
            }
            UpdateRadioButtonCounters();
        }
        private void DgvUnifiedResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Make sure it's a valid row (not header) and not an empty selection
                if (e.RowIndex >= 0 && e.RowIndex < dgvUnifiedResults.Rows.Count)
                {
                    // Get the selected row
                    DataGridViewRow selectedRow = dgvUnifiedResults.Rows[e.RowIndex];

                    // Get the username from the row (using colUserName column)
                    string username = selectedRow.Cells["colUserName"].Value?.ToString();

                    if (!string.IsNullOrEmpty(username) && username != "N/A")
                    {
                        _consoleForm.WriteInfo($"Loading user details for: {username}");
                        enableADFunctionTools();
                    }
                    else
                    {
                        MessageBox.Show("No valid username selected.", "Warning",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _consoleForm.WriteWarning("No valid username selected.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user details.", "Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _consoleForm.WriteError($"Error loading user details: {ex.Message}");
            }
        }
        private void btnUnlockAccount_Click(object sender, EventArgs e)
        {
            string username = lblLoginNameValue.Text;

            if (_adService.UnlockUserAccount(username))
            {
                _consoleForm.WriteError($"User account unlocked successfully.");
                UpdateRadioButtonCounters();
            }
            else
            {
                MessageBox.Show("Failed to unlock user account.  User may not exist or you may not have sufficient permissions.", "Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _consoleForm.WriteError($"Failed to unlock user account.  User may not exist or you may not have sufficient permissions.");
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            string samAccountName = lblLoginNameValue.Text;

            if (string.IsNullOrEmpty(samAccountName))
            {
                _consoleForm.WriteError($"Please make sure an account is loaded in the 'general' tab.");
                return;
            }

            DialogResult firstConfirm = MessageBox.Show(
            $"Are you sure you want to DELETE the account '{samAccountName}'?\n\nThis action cannot be undone!",
            "Confirm Account Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
            if (firstConfirm == DialogResult.Yes)
            {
                DialogResult secondConfirm = MessageBox.Show(
                $"FINAL WARNING: You are about to permanently DELETE '{samAccountName}'?\n\nProceed with deletion?",
                "Final Deletion Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

                if (secondConfirm == DialogResult.Yes)
                {
                    bool success = _adService.DeleteUserAccount(samAccountName);

                    if (success)
                    {
                        MessageBox.Show($"Account '{samAccountName}' has been successfully deleted.", "Success",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _consoleForm.WriteSuccess($"Account '{samAccountName}' has been successfully deleted.");

                        UpdateRadioButtonCounters();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete account. Please verify the account name and your permissions", "FAILED",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _consoleForm.WriteError($"Failed to delete account. Please verify the account name and your permissions.");
                    }
                }
            }
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate that a user is loaded
                string username = lblLoginNameValue.Text.Trim();
                if (string.IsNullOrEmpty(username) || username == "N/A")
                {
                    _consoleForm?.WriteWarning("No user loaded. Please load a user first.");
                    MessageBox.Show("No user loaded. Please load a user first.", "No User Loaded",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate required fields
                string disabledReason = txbDisabledReason.Text.Trim();
                string processedBy = txbProcessedBy.Text.Trim();

                if (string.IsNullOrEmpty(disabledReason))
                {
                    _consoleForm?.WriteWarning("Please enter a reason for disabling the account.");
                    MessageBox.Show("Please enter a reason for disabling the account.", "Reason Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txbDisabledReason.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(processedBy))
                {
                    _consoleForm?.WriteWarning("Please enter who is processing this disable request.");
                    MessageBox.Show("Please enter who is processing this disable request.", "Processed By Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txbProcessedBy.Focus();
                    return;
                }

                // Format the disable date
                string disabledDateString = dtpDisabledDate.Value.ToString("dddd, MMMM dd, yyyy");

                // Create the description string
                string disableDescription = $"Account disabled on {disabledDateString} - {disabledReason}. Processed by: {processedBy}";

                // Confirmation dialog
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to disable the account for {username}?\n\n" +
                    $"This will:\n" +
                    $"• Disable the user account\n" +
                    $"• Update description: {disableDescription}\n" +
                    $"• Remove user from all AD security groups\n" +
                    $"• Add user to 'pending_removal' group\n" +
                    $"• Move user to Disabled Users OU\n\n" +
                    $"This action can be reversed, but the user will immediately lose access.",
                    "Confirm Account Disable",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    _consoleForm?.WriteInfo("Account disable operation cancelled by user.");
                    return;
                }

                // Show processing state
                btnDisable.Enabled = false;
                btnDisable.Text = "Disabling...";

                _consoleForm?.WriteInfo($"Starting disable process for user: {username}");

                // Read disabled users OU from database config; fall back to hardcoded default
                string targetOU = !string.IsNullOrEmpty(_disabledUsersOu)
                    ? _disabledUsersOu
                    : "OU=Disabled Users,OU=People,OU=CDC,OU=spectre,DC=spectre,DC=afspc,DC=af,DC=smil,DC=mil";

                // Step 1: Disable the account, update description, and move to Disabled Users OU (all via AD)
                bool disableSuccess = _adService.DisableAndMoveUser(username, disableDescription, targetOU);

                if (!disableSuccess)
                {
                    _consoleForm?.WriteError($"Failed to disable and move user: {username}");
                    MessageBox.Show($"Failed to disable account for {username}. Check the console for details.",
                                  "Disable Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Step 2: Remove from all AD security groups and add to "pending_removal" group
                bool groupCleanupSuccess = _adService.RemoveUserFromAllGroupsAndAddToPendingRemoval(username);

                if (groupCleanupSuccess)
                {
                    _consoleForm?.WriteSuccess("Group cleanup completed successfully");
                }
                else
                {
                    _consoleForm?.WriteWarning("Group cleanup completed with some errors");
                }

                // Refresh the user details to show updated status
                try
                {
                    var userInfo = _adService.GetUserInfo(username);
                    if (userInfo != null)
                    {
                        PopulateGeneralTabFromUserInfo(userInfo);
                        _consoleForm?.WriteInfo("User details refreshed to show disabled status.");
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Error refreshing user details: {ex.Message}");
                }

                // Clear the input fields
                txbDisabledReason.Clear();
                txbProcessedBy.Clear();
                dtpDisabledDate.Value = DateTime.Now;

                _consoleForm?.WriteSuccess($"Account disable process completed for: {username}");
                MessageBox.Show($"Account successfully disabled for {username}.", "Account Disabled",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error disabling account: {ex.Message}");
                MessageBox.Show($"Error disabling account: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset button state
                btnDisable.Enabled = true;
                btnDisable.Text = "Disable Account";
            }
        }

        private async void btnClearForm_Click(object sender, EventArgs e)
        {

        }
        // await UpdateRadioButtonCounters();++++++++++
        #endregion

        #region LDAP Tab functions

        /// <summary>
        /// Generate NT User ID from first name and last name
        /// </summary>
        private void GenerateNTUserID()
        {
            try
            {
                // Get the first character of the first name and convert it to lowercase
                string firstName = txbLdapFirstName.Text.Trim().ToLower();
                string firstLetter = firstName.Length > 0 ? firstName.Substring(0, 1) : "";

                // Get the last name and convert it to lowercase
                string lastName = txbLdapLastName.Text.Trim().ToLower();

                // Combine the first letter of the first name with the last name
                string combinedUsername = firstLetter + lastName;

                // Set the value of the NT User ID textbox to the combined username
                txbLdapNtUserId.Text = combinedUsername;

                _consoleForm.WriteSuccess($"Generated NT User ID: {combinedUsername}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error generating NT User ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Get next available Linux UID from LDAP
        /// </summary>
        /// 
        /*
        private void GetNextLinuxUID()
        {
            try
            {
                _consoleForm.WriteInfo("Retrieving next available Linux UID from LDAP...");

                // Create instance of LDAPService class
                var ldapService = new LDAPService();

                // Get LDAP connection
                using (var connection = ldapService.GetLdapConnection())
                {
                    string distinguishedName = "ou=people,dc=spectre,dc=afspc,dc=af,dc=smil,dc=mil";
                    string filter = "(uidNumber=*)";
                    var searchScope = System.DirectoryServices.Protocols.SearchScope.Subtree;

                    var uidSearchRequest = new SearchRequest(
                        distinguishedName,
                        filter,
                        searchScope,
                        new string[] { "uidNumber" }
                    );

                    var uidSearchResponse = (SearchResponse)connection.SendRequest(uidSearchRequest);

                    var currentUIDNumbers = new List<int>();

                    foreach (SearchResultEntry entry in uidSearchResponse.Entries)
                    {
                        if (entry.Attributes.Contains("uidNumber"))
                        {
                            string uidValue = entry.Attributes["uidNumber"][0].ToString();
                            if (int.TryParse(uidValue, out int uid))
                            {
                                currentUIDNumbers.Add(uid);
                            }
                        }
                    }

                    // Filter out specific UIDs
                    int[] excludedUIDs = { 101, 276, 6000, 22941, 22942, 22943 };
                    currentUIDNumbers = currentUIDNumbers.Where(uid => !excludedUIDs.Contains(uid)).ToList();

                    // Find the highest UID and add 1
                    int nextUID = currentUIDNumbers.Count > 0 ? currentUIDNumbers.Max() + 1 : 10000; // Start from 10000 if no UIDs found

                    // Set the Linux UID textbox
                    txbLdapLinuxUid.Text = nextUID.ToString();

                    _consoleForm.WriteSuccess($"Next available Linux UID: {nextUID}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error retrieving Linux UID: {ex.Message}");
                // Set a default value if LDAP lookup fails
                txbLdapLinuxUid.Text = "10000";
                _consoleForm.WriteWarning("Set default Linux UID: 10000");
            }
        }
        */
        /// <summary>
        /// Validate all required LDAP form fields
        /// </summary>
        private bool ValidateLdapFormFields()
        {
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(txbLdapFirstName.Text))
                missingFields.Add("First Name");
            if (string.IsNullOrWhiteSpace(txbLdapLastName.Text))
                missingFields.Add("Last Name");
            if (string.IsNullOrWhiteSpace(txbLdapNtUserId.Text))
                missingFields.Add("NT User ID");
            if (string.IsNullOrWhiteSpace(txbLdapEmail.Text))
                missingFields.Add("Email");
            if (string.IsNullOrWhiteSpace(txbLdapPhone.Text))
                missingFields.Add("Phone");
            if (string.IsNullOrWhiteSpace(txbLdapLinuxUid.Text))
                missingFields.Add("Linux UID");
            if (string.IsNullOrWhiteSpace(txbLdapTempPass.Text))
                missingFields.Add("Temporary Password");

            if (missingFields.Count > 0)
            {
                string missingFieldsMessage = string.Join(", ", missingFields);
                _consoleForm.WriteError($"Missing required fields: {missingFieldsMessage}");
                MessageBox.Show($"Please fill in all required fields:\n{missingFieldsMessage}",
                               "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clear all LDAP form fields
        /// </summary>
        private void ClearLdapForm()
        {
            txbLdapFirstName.Clear();
            txbLdapLastName.Clear();
            txbLdapNtUserId.Clear();
            txbLdapEmail.Clear();
            txbLdapPhone.Clear();
            txbLdapLinuxUid.Clear();
            txbLdapTempPass.Clear();

            // Show the generate and get UID buttons again
            btnLdapGenerate.Visible = true;
            btnLdapGetUid.Visible = true;

            _consoleForm.WriteInfo("LDAP form cleared successfully.");
        }

        #endregion

        #region LDAP Tab Button Event Handlers

        private void cbxDefaultSecurityGroups_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                // Only allow one item to be checked at a time
                if (e.NewValue == CheckState.Checked)
                {
                    // Uncheck all other items
                    for (int i = 0; i < cbxDefaultSecurityGroups.Items.Count; i++)
                    {
                        if (i != e.Index)
                        {
                            cbxDefaultSecurityGroups.SetItemChecked(i, false);
                        }
                    }

                    // Get the selected OU path
                    string selectedGroup = cbxDefaultSecurityGroups.Items[e.Index].ToString();

                    string configuredGroupOU = null;
                    foreach (int checkedIndex in cbxListSecurityGroupsOu.CheckedIndices)
                    {
                        configuredGroupOU = cbxListSecurityGroupsOu.Items[checkedIndex].ToString();
                        break;
                    }

                    // Get the select group name
                    try
                    {
                        // Need a method in RHDS_Service to get the OU's gidNumber
                        string gidNumber = _rhdsService.GetGroupGidNumber(selectedGroup, configuredGroupOU);

                        if (!string.IsNullOrEmpty(gidNumber))
                        {
                            txbSecurityGroupId.Text = gidNumber;
                            _consoleForm?.WriteSuccess($"Loaded gidNumber: {gidNumber} for group: {selectedGroup}");
                        }
                        else
                        {
                            txbSecurityGroupId.Text = "N/A";
                            _consoleForm?.WriteWarning($"No gidNumber found for OU: {selectedGroup}");
                        }
                    }
                    catch (Exception ex)
                    {
                        txbSecurityGroupId.Text = "Error";
                        _consoleForm?.WriteError($"Error getting gidNumber: {ex.Message}");
                    }
                }
                else
                {
                    // Item is being unchecked - clear the textbox
                    txbSecurityGroupId.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error in ItemCheck event: {ex.Message}");
            }
        }
        /// <summary>
        /// Event handler for Generate button - creates NT User ID from first and last name
        /// </summary>
        private void btnLdapGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                // Hide the generate button after use
                btnLdapGenerate.Visible = false;

                // Get the first character of the first name and convert it to lowercase
                string firstName = txbLdapFirstName.Text.Trim().ToLower();
                string firstLetter = firstName.Length > 0 ? firstName.Substring(0, 1) : "";

                // Get the last name and convert it to lowercase
                string lastName = txbLdapLastName.Text.Trim().ToLower();

                // Combine the first letter of the first name with the last name
                string ntUserId = firstLetter + lastName;

                // Set the NT User ID textbox
                txbLdapNtUserId.Text = ntUserId;

                _consoleForm?.WriteSuccess($"Generated NT User ID: {ntUserId}");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error generating NT User ID: {ex.Message}");
                // Show the button again if there was an error
                btnLdapGenerate.Visible = true;
            }
            txbLdapPhone.Focus();
        }

        /// <summary>
        /// Event handler for Get UID button - retrieves next available Linux UID from LDAP
        /// </summary>
        private async void btnLdapGetUid_Click(object sender, EventArgs e)
        {
            try
            {
                // Hide the get UID button after use
                btnLdapGetUid.Visible = false;

                _consoleForm?.WriteInfo("Getting next available Linux UID from Directory Services...");

                // Get next available UID from RHDS
                string nextUid = await Task.Run(() => _rhdsService.GetNextAvailableUid());

                if (!string.IsNullOrEmpty(nextUid))
                {
                    txbLdapLinuxUid.Text = nextUid;
                    _consoleForm?.WriteSuccess($"Next available Linux UID: {nextUid}");
                }
                else
                {
                    _consoleForm?.WriteError("Failed to get next available Linux UID");
                    MessageBox.Show("Failed to get next available Linux UID", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Show the button again if there was an error
                    btnLdapGetUid.Visible = true;
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting Linux UID: {ex.Message}");
                MessageBox.Show($"Error getting Linux UID: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Show the button again if there was an error
                btnLdapGetUid.Visible = true;
            }
            txbLdapTempPass.Focus();
        }

        /// <summary>
        /// Event handler for Clear Form button - clears all LDAP form fields
        /// </summary>
        private void btnClearAccountCreationForm_Click(object sender, EventArgs e)
        {
            try
            {
                ClearLdapForm();
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error clearing LDAP form: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for Create Account button - creates LDAP account and adds to default security group
        /// </summary>
        /// 
        private async void btnLdapCreateAccount_Click(object sender, EventArgs e)
        {
            // Disable the button while processing
            btnLdapCreateAccount.Enabled = false;
            string originalText = btnLdapCreateAccount.Text;
            btnLdapCreateAccount.Text = "Creating Account...";

            try
            {
                // Check authentication
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm?.WriteError("Please log in first before creating LDAP accounts.");
                    MessageBox.Show("Please log in first before creating LDAP accounts.", "Authentication Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate form fields
                if (!ValidateLdapFormFields())
                {
                    return;
                }

                // Get the selected security group from cbxDefaultSecurityGroups
                string selectedSecurityGroup = null;
                foreach (int checkedIndex in cbxDefaultSecurityGroups.CheckedIndices)
                {
                    selectedSecurityGroup = cbxDefaultSecurityGroups.Items[checkedIndex].ToString();
                    break; // Only get first checked item
                }

                bool shouldAddToGroup = !string.IsNullOrEmpty(selectedSecurityGroup);

                if (!shouldAddToGroup)
                {
                    _consoleForm?.WriteWarning("No security group selected. User will be created without group membership.");
                }

                // Get form values
                string ntUserId = txbLdapNtUserId.Text.Trim();
                string email = txbLdapEmail.Text.Trim();
                string firstName = txbLdapFirstName.Text.Trim();
                string lastName = txbLdapLastName.Text.Trim();
                string phone = txbLdapPhone.Text.Trim();
                string tempPassword = txbLdapTempPass.Text;
                string linuxUid = txbLdapLinuxUid.Text.Trim();
                string gidNumber = txbSecurityGroupId.Text.Trim();

                _consoleForm?.WriteInfo($"Starting account creation for user: {ntUserId}");

                // Step 1: Create RHDS user account
                await Task.Run(() =>
                {
                    _rhdsService.CreateNewUser(
                        ntUserId: ntUserId,
                        email: email,
                        firstName: firstName,
                        lastName: lastName,
                        phone: phone,
                        tempPassword: tempPassword,
                        linuxUid: linuxUid,
                        gidNumber: gidNumber,
                        securityGroup: selectedSecurityGroup
                    );
                });

                _consoleForm?.WriteSuccess("RHDS user account created successfully.");

                // Step 2: Wait for AD replication and find user
                int attempts = 0;
                const int maxAttempts = 3;
                bool userFound = false;
                AD_Service.UserInfo userInfo = null;

                while (attempts < maxAttempts && !userFound)
                {
                    attempts++;
                    _consoleForm?.WriteInfo($"Attempting to find user in AD (attempt {attempts}/{maxAttempts})...");

                    userInfo = _adService.GetUserInfo(ntUserId);

                    if (userInfo != null)
                    {
                        userFound = true;
                        _consoleForm?.WriteSuccess($"Found user in AD: {userInfo.GetFullName()} ({userInfo.SamAccountName})");
                    }
                    else if (attempts < maxAttempts)
                    {
                        _consoleForm?.WriteInfo($"User not found in AD. Waiting 6 seconds before next attempt...");
                        await Task.Delay(6000);
                    }
                }

                if (!userFound)
                {
                    _consoleForm?.WriteError($"User {ntUserId} not found in AD after {maxAttempts} attempts");
                    MessageBox.Show(
                        $"User '{ntUserId}' created in RHDS but not found in AD after {maxAttempts} attempts.\n\n" +
                        $"✓ RHDS account created\n" +
                        $"✗ AD replication pending\n\n" +
                        "User may need to be manually added to security groups once replication completes.",
                        "Partial Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Step 3: Set uidNumber and gidNumber in AD's attribute editor
                bool attributesSet = false;
                try
                {
                    _consoleForm?.WriteInfo($"Setting uidNumber ({linuxUid}) and gidNumber ({gidNumber}) in AD...");

                    attributesSet = _adService.SetUnixAttributes(userInfo.SamAccountName, linuxUid, gidNumber);

                    if (attributesSet)
                    {
                        _consoleForm?.WriteSuccess($"Successfully set Unix attributes in AD for {userInfo.GetFullName()}");
                    }
                    else
                    {
                        _consoleForm?.WriteError($"Failed to set Unix attributes in AD for {userInfo.GetFullName()}");
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Error setting Unix attributes in AD: {ex.Message}");
                }

                // Step 4: Add user to selected security group (if one was selected)
                bool adGroupSuccess = false;
                bool rhdsGroupSuccess = false;

                if (shouldAddToGroup)
                {
                    // Add to AD group
                    try
                    {
                        bool addResult = _adService.AddUserToGroup(userInfo.SamAccountName, selectedSecurityGroup);

                        if (addResult)
                        {
                            adGroupSuccess = true;
                            _consoleForm?.WriteSuccess($"User {userInfo.GetFullName()} successfully added to {selectedSecurityGroup} in AD");
                        }
                        else
                        {
                            _consoleForm?.WriteError($"Failed to add user {userInfo.GetFullName()} to {selectedSecurityGroup} in AD");
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Error adding user to AD group: {ex.Message}");
                    }

                    // Step 5: Add user to selected security group in RHDS
                    try
                    {
                        // Get the selected OU (should only be one checked)
                        string selectedOU = null;
                        if (cbxListSecurityGroupsOu.Items.Count > 0)
                        {
                            selectedOU = cbxListSecurityGroupsOu.Items[0].ToString();
                        }

                        bool rhdsAddResult = _rhdsService.AddUserToDSGroup(ntUserId, selectedSecurityGroup, selectedOU);

                        if (rhdsAddResult)
                        {
                            rhdsGroupSuccess = true;
                            _consoleForm?.WriteSuccess($"User {ntUserId} successfully added to {selectedSecurityGroup} in RHDS");
                        }
                        else
                        {
                            _consoleForm?.WriteError($"Failed to add user {ntUserId} to {selectedSecurityGroup} in RHDS");
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Error adding user to RHDS group: {ex.Message}");
                    }
                }

                // Step 6: Create home directory on Linux server
                bool homeDirectoryCreated = false;
                try
                {
                    _consoleForm?.WriteInfo("Prompting for Linux SSH credentials to create home directory...");
                    var (success, hostname, sshUsername, sshPassword) = LinuxCredentialDialog.GetCredentials();

                    if (success)
                    {
                        _consoleForm?.WriteInfo($"Creating home directory on {hostname}...");

                        homeDirectoryCreated = await _linuxService.CreateHomeDirectoryAsync(
                            hostname,
                            sshUsername,
                            sshPassword,
                            ntUserId
                            );
                        if (homeDirectoryCreated)
                        {
                            _consoleForm?.WriteSuccess($"Home Directory created successfully for {ntUserId}");
                        }
                        else
                        {
                            _consoleForm?.WriteError($"Failed to create home direectory for {ntUserId}");
                        }
                    }
                    else
                    {
                        _consoleForm?.WriteError($"Linux SSH credentials not provided. Home directory creation skipped.");
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Error during home directory creation: {ex.Message}");
                }

                // Comprehensive success messaging
                _consoleForm?.WriteSuccess("Account creation process completed!");

                // Build detailed success message based on what succeeded
                string successMessage = $"User account '{ntUserId}' created successfully!\n\n";

                successMessage += $"✓ RHDS account created\n";
                successMessage += $"✓ Replicated to Windows AD\n";

                if (attributesSet)
                {
                    successMessage += $"✓ Unix attributes set (UID: {linuxUid}, GID: {gidNumber})\n";
                }
                else
                {
                    successMessage += $"⚠ Failed to set Unix attributes\n";
                }

                if (shouldAddToGroup)
                {
                    if (adGroupSuccess && rhdsGroupSuccess)
                    {
                        successMessage += $"✓ Added to {selectedSecurityGroup} (AD & RHDS)\n";
                    }
                    else if (adGroupSuccess && !rhdsGroupSuccess)
                    {
                        successMessage += $"✓ Added to {selectedSecurityGroup} (AD only)\n";
                        successMessage += $"⚠ Failed to add to {selectedSecurityGroup} in RHDS\n";
                    }
                    else if (!adGroupSuccess && rhdsGroupSuccess)
                    {
                        successMessage += $"⚠ Failed to add to {selectedSecurityGroup} in AD\n";
                        successMessage += $"✓ Added to {selectedSecurityGroup} (RHDS only)\n";
                    }
                    else
                    {
                        successMessage += $"⚠ Failed to add to {selectedSecurityGroup} in both AD and RHDS\n";
                    }
                }
                else
                {
                    successMessage += "\nNo security group was selected.";
                }

                // Add home directory creation status
                if (homeDirectoryCreated)
                {
                    successMessage += $"✓ Home directory created at /net/cce-data/home/{ntUserId}\n";
                }
                else
                {
                    successMessage += $"\"⚠ Home directory creation was skipped or failed\n";
                }

                MessageBox.Show(successMessage, "Account Created Successfully",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear the form after successful creation
                ClearLdapForm();
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error during account creation: {ex.Message}");
                MessageBox.Show($"Error creating account: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset button state
                btnLdapCreateAccount.Enabled = true;
                btnLdapCreateAccount.Text = originalText;
            }
        }
        #endregion

        #region Spice PMI Tab Functions
        // VERSION 1: With auto-fill for Bind DN (ACTIVE)
        private async Task CheckServerReplicationHealth(string hostname, string username, string password, string serverLabel)
        {
            try
            {
                _consoleForm.WriteInfo($"Checking replication health on {hostname}...");

                // Single command to get all replication information
                string command = $"dsconf -D 'cn=Directory Manager' -w '{password}' ldap://{hostname}:389 replication monitor";

                // Prepare inputs for the interactive prompts:
                // 1. Bind DN for the other server (ccesa2 or ccesa1)
                // 2. Password for the other server
                // 3. Bind DN for the main server (the one we're connecting to)
                // 4. Password for the main server
                string[] inputs = new string[]
                {
                    "cn=Directory Manager",  // Bind DN for other server
                    password,                // Password for other server
                    "cn=Directory Manager",  // Bind DN for main server
                    password                 // Password for main server
                };

                // Execute the interactive command
                string output = await _linuxService.ExecuteInteractiveSSHCommandAsync(hostname, username, password, command, inputs);

                _consoleForm.WriteInfo($"Replication monitor output received ({output.Length} characters)");

                // Parse and display the results
                ParseReplicationMonitorOutput(output, password);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to check replication health on {hostname}: {ex.Message}");

                // Set all labels to error state
                SetReplicationLabelsToError();
            }
        }

        private void ParseReplicationMonitorOutput(string output, string password)
        {
            try
            {
                _consoleForm.WriteInfo("Parsing replication monitor output...");
                _consoleForm.WriteInfo($"Output length: {output.Length} characters");

                // Sanitize output before logging - replace password with ***
                string sanitizedOutput = output.Replace(password, "***");


                // Log the sanitized raw output for debugging
                _consoleForm.WriteInfo("=== RAW OUTPUT START (passwords masked) ===");
                _consoleForm.WriteInfo(sanitizedOutput);
                _consoleForm.WriteInfo("=== RAW OUTPUT END ===");

                // Split output into lines
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                _consoleForm.WriteInfo($"Total lines to parse: {lines.Length}");

                // Track which server section we're in (SA1 or SA2)
                int currentServer = 0; // 0 = none, 1 = SA1, 2 = SA2
                bool inSupplierSection = false;

                int lineNumber = 0;
                foreach (var line in lines)
                {
                    lineNumber++;
                    var trimmedLine = line.Trim();

                    // Detect supplier sections
                    if (trimmedLine.StartsWith("Supplier:", StringComparison.OrdinalIgnoreCase))
                    {
                        inSupplierSection = true;
                        currentServer++;

                        _consoleForm.WriteSuccess($"[Line {lineNumber}] Found Supplier Section #{currentServer}: {trimmedLine}");
                        continue;
                    }
                    if (!inSupplierSection || currentServer == 0)
                        continue;
                    // Log what we're parsing
                    if (!string.IsNullOrWhiteSpace(trimmedLine) && trimmedLine.Contains(":"))
                    {
                        _consoleForm.WriteInfo($"[Server {currentServer}, Line {lineNumber}] Parsing: {trimmedLine}");
                    }

                    // Parse each field and update the appropriate labels
                    // Using case-insensitive comparisons for more flexibility
                    if (trimmedLine.StartsWith("Replica Root:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Replica Root:");
                        if (currentServer == 1)

                            lblReplicaRootDataSa1.Text = value;
                        else if (currentServer == 2)

                            lblReplicaRootDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Replica Root for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Replica ID:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Replica ID:");
                        if (currentServer == 1)

                            lblReplicaIDDataSa1.Text = value;
                        else if (currentServer == 2)

                            lblReplicaIDDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Replica ID for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Replica Status:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Replica Status:");
                        if (currentServer == 1)

                            lblReplicaStatusSa1.Text = value;
                        else if (currentServer == 2)

                            lblReplicaStatusSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Replica Status for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Max CSN:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Max CSN:");
                        if (currentServer == 1)

                            lblMaxCSNDataSa1.Text = value;
                        else if (currentServer == 2)

                            lblMaxCSNDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Max CSN for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Replica Enabled:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Replica Enabled:");
                        if (currentServer == 1)

                            lblReplicaEnabledDataSa1.Text = value;
                        else if (currentServer == 2)

                            lblReplicaEnabledDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Replica Enabled for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Update In Progress:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Update In Progress:");
                        if (currentServer == 1)
                            lblUpdateInProgressDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblUpdateInProgressDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Update In Progress for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Last Update Start:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Last Update Start:");
                        if (currentServer == 1)
                            lblLastUpdateStartDataSa1.Text = FormatLdapTimestamp(value);
                        else if (currentServer == 2)
                            lblLastUpdateStartDataSa2.Text = FormatLdapTimestamp(value);
                        _consoleForm.WriteSuccess($"  -> Set Last Update Start for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Last Update End:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Last Update End:");
                        if (currentServer == 1)
                            lblLastUpdateEndDataSa1.Text = FormatLdapTimestamp(value);
                        else if (currentServer == 2)
                            lblLastUpdateEndDataSa2.Text = FormatLdapTimestamp(value);
                        _consoleForm.WriteSuccess($"  -> Set Last Update End for SA{currentServer}: {value}");
                    }
                    // More flexible matching for "Number of Changes Sent" - handles variations
                    else if (trimmedLine.IndexOf("Changes Sent:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string value = ExtractValueFlexible(trimmedLine, "Changes Sent:");
                        if (currentServer == 1)
                            lblChangesSentDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblChangesSentDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Changes Sent for SA{currentServer}: {value}");
                    }
                    // More flexible matching for "Number of Changes Skipped"
                    else if (trimmedLine.IndexOf("Changes Skipped:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string value = ExtractValueFlexible(trimmedLine, "Changes Skipped:");
                        if (currentServer == 1)
                            lblChangesSkippedDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblChangesSkippedDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Changes Skipped for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Last Update Status:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Last Update Status:");
                        if (currentServer == 1)
                            txbLastUpdateStatusDataSa1.Text = value;
                        else if (currentServer == 2)
                            txbLastUpdateStatusDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Last Update Status for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Last Init Start:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Last Init Start:");
                        if (currentServer == 1)
                            lblLastInitStartDataSa1.Text = FormatLdapTimestamp(value);
                        else if (currentServer == 2)
                            lblLastInitStartDataSa2.Text = FormatLdapTimestamp(value);
                        _consoleForm.WriteSuccess($"  -> Set Last Init Start for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Last Init End:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Last Init End:");
                        if (currentServer == 1)
                            lblLastInitEndDataSa1.Text = FormatLdapTimestamp(value);
                        else if (currentServer == 2)
                            lblLastInitEndDataSa2.Text = FormatLdapTimestamp(value);
                        _consoleForm.WriteSuccess($"  -> Set Last Init End for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Last Init Status:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Last Init Status:");
                        if (currentServer == 1)
                            lblLastInitStatusDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblLastInitStatusDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Last Init Status for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Reap Active:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Reap Active:");
                        if (currentServer == 1)
                            lblReapActiveDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblReapActiveDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Reap Active for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Replication Status:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Replication Status:");
                        if (currentServer == 1)
                            txbLastUpdateStatusDataSa1.Text = value;
                        else if (currentServer == 2)
                            txbLastUpdateStatusDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Replication Status for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Replication Lag Time:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Replication Lag Time:");
                        if (currentServer == 1)
                            lblReplicationStatusDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblReplicationStatusDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Replication Lag Time for SA{currentServer}: {value}");
                    }
                    else if (trimmedLine.StartsWith("Status For Agreement:", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = ExtractValue(trimmedLine, "Status For Agreement:");
                        if (currentServer == 1)
                            lblStatusForAgreementDataSa1.Text = value;
                        else if (currentServer == 2)
                            lblStatusForAgreementDataSa2.Text = value;
                        _consoleForm.WriteSuccess($"  -> Set Status Agreement for SA{currentServer}: {value}");
                    }
                }

                _consoleForm.WriteSuccess($"Successfully parsed replication data for {currentServer} server(s)");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error parsing replication output: {ex.Message}");
                _consoleForm.WriteError($"Stack trace: {ex.StackTrace}");
            }
        }
        private string ExtractValue(string line, string prefix)
        {
            int index = line.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return line.Substring(index + prefix.Length).Trim();
            }
            return line.Trim();
        }
        private string ExtractValueFlexible(string line, string searchText)
        {
            // Handle null or empty input
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            // Find the search text (case insensitive) and extract everything after it
            int index = line.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return line.Substring(index + searchText.Length).Trim();
            }

            // Default: return the whole line trimmed
            return line.Trim();
        }

        private string FormatLdapTimestamp(string ldapTimestamp)
        {
            // LDAP timestamp format: 20260108185252Z
            // Convert to readable format: 2026/01/08 18:52:52
            try
            {
                if (string.IsNullOrWhiteSpace(ldapTimestamp) || ldapTimestamp == "unavailable")
                    return ldapTimestamp;

                if (ldapTimestamp.Length >= 14)
                {
                    string year = ldapTimestamp.Substring(0, 4);
                    string month = ldapTimestamp.Substring(4, 2);
                    string day = ldapTimestamp.Substring(6, 2);
                    string hour = ldapTimestamp.Substring(8, 2);
                    string minute = ldapTimestamp.Substring(10, 2);
                    string second = ldapTimestamp.Substring(12, 2);

                    return $"{year}/{month}/{day} {hour}:{minute}:{second}";
                }

                return ldapTimestamp;
            }
            catch
            {
                return ldapTimestamp;
            }
        }

        private void SetReplicationLabelsToError()
        {
            // Set SA1 labels to error
            lblReplicaRootDataSa1.Text = "ERROR";
            lblReplicaIDDataSa1.Text = "ERROR";
            lblReplicaStatusDataSa1.Text = "ERROR";
            lblMaxCSNDataSa1.Text = "ERROR";
            lblStatusForAgreementDataSa1.Text = "ERROR";
            txbLastUpdateStatusDataSa1.Text = "ERROR";
            lblUpdateInProgressDataSa1.Text = "ERROR";
            lblReplicaEnabledDataSa1.Text = "ERROR";
            lblChangesSentDataSa1.Text = "ERROR";
            lblChangesSkippedDataSa1.Text = "ERROR";
            lblLastUpdateStartDataSa1.Text = "ERROR";
            lblLastUpdateEndDataSa1.Text = "ERROR";
            lblReplicationStatusDataSa1.Text = "ERROR";
            lblReplicationLagTimeDataSa1.Text = "ERROR";

            // Set SA1 labels to error
            txbLastUpdateStatusDataSa2.Text = "ERROR";
            lblStatusForAgreementDataSa2.Text = "ERROR";
            lblUpdateInProgressDataSa2.Text = "ERROR";
            lblReplicaEnabledDataSa2.Text = "ERROR";
            lblChangesSentDataSa2.Text = "ERROR";
            lblChangesSkippedDataSa2.Text = "ERROR";
            lblLastUpdateStartDataSa2.Text = "ERROR";
            lblLastUpdateEndDataSa2.Text = "ERROR";
            lblReplicationStatusDataSa2.Text = "ERROR";
            lblReplicationLagTimeDataSa2.Text = "ERROR";
            lblReplicaRootDataSa2.Text = "ERROR";
            lblReplicaIDDataSa2.Text = "ERROR";
            lblReplicaStatusDataSa2.Text = "ERROR";
            lblMaxCSNDataSa2.Text = "ERROR";
        }

        private void ClearReplicationResults()
        {
            // Clear SA1 data labels
            lblReplicaRootDataSa1.Text = "Checking...";
            lblReplicaIDDataSa1.Text = "Checking...";
            lblReplicaStatusDataSa1.Text = "Checking...";
            lblMaxCSNDataSa1.Text = "Checking...";
            lblStatusForAgreementDataSa1.Text = "Checking...";
            txbLastUpdateStatusDataSa1.Text = "Checking...";
            lblUpdateInProgressDataSa1.Text = "Checking...";
            lblReplicaEnabledDataSa1.Text = "Checking...";
            lblChangesSentDataSa1.Text = "Checking...";
            lblChangesSkippedDataSa1.Text = "Checking...";
            lblLastUpdateStartDataSa1.Text = "Checking...";
            lblLastUpdateEndDataSa1.Text = "Checking...";
            lblReplicationStatusDataSa1.Text = "Checking...";
            lblReplicationLagTimeDataSa1.Text = "Checking...";

            // Clear SA2 data labels
            txbLastUpdateStatusDataSa2.Text = "Checking...";
            lblStatusForAgreementDataSa2.Text = "Checking...";
            lblUpdateInProgressDataSa2.Text = "Checking...";
            lblReplicaEnabledDataSa2.Text = "Checking...";
            lblChangesSentDataSa2.Text = "Checking...";
            lblChangesSkippedDataSa2.Text = "Checking...";
            lblLastUpdateStartDataSa2.Text = "Checking...";
            lblLastUpdateEndDataSa2.Text = "Checking...";
            lblReplicationStatusDataSa2.Text = "Checking...";
            lblReplicationLagTimeDataSa2.Text = "Checking...";
            lblReplicaRootDataSa2.Text = "Checking...";
            lblReplicaIDDataSa2.Text = "Checking...";
            lblReplicaStatusDataSa2.Text = "Checking...";
            lblMaxCSNDataSa2.Text = "Checking...";
        }

        private Task UpdateDataGridView(DataGridView dgv, List<Linux_Service.DiskInfo> diskInfo)
        {
            // Ensure UI updates happen on the UI thread
            if (dgv.InvokeRequired)
            {
                return (Task)dgv.Invoke(new Func<Task>(() => UpdateDataGridView(dgv, diskInfo)));
            }

            try
            {
                dgv.SuspendLayout();
                dgv.Rows.Clear();

                foreach (var info in diskInfo)
                {
                    // Add color coding for disk usage
                    var row = new DataGridViewRow();
                    row.CreateCells(dgv,
                        info.Filesystem,
                        info.Size,
                        info.Used,
                        info.Avail,
                        info.UsePercent,
                        info.MountedOn
                    );

                    // Color coding based on usage percentage
                    if (int.TryParse(info.UsePercent.TrimEnd('%'), out int usagePercent))
                    {
                        if (usagePercent >= 90)
                        {
                            row.DefaultCellStyle.BackColor = Color.Red;
                            row.DefaultCellStyle.ForeColor = Color.White;
                        }
                        else if (usagePercent >= 80)
                        {
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                        }
                    }

                    dgv.Rows.Add(row);
                }

                // Auto-size columns for better visibility
                dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            finally
            {
                dgv.ResumeLayout();
            }

            return Task.CompletedTask;
        }

        private async Task InitializeVMwareAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Initializing PowerCLI modules...");
                await _vmwareManager.InitializePowerCLIAsync();
                _consoleForm.WriteSuccess("PowerCLI modules initialized successfully");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"VMware initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Start loading PowerCLI in the background after login
        /// </summary>
        private async void StartBackgroundPowerCLILoadingAsync()
        {
            try
            {
                // Initialize VMware Manager if not already done
                if (_vmwareManager == null)
                {
                    string vCenterUser = CredentialManager.GetUsername();
                    string vCenterPass = CredentialManager.GetPassword();
                    _vmwareManager = new VMwareManager(_vCenterServer, vCenterUser, vCenterPass, _consoleForm, POWERCLI_MODULE_PATH);
                }

                _consoleForm.WriteInfo("Starting PowerCLI background loading...");
                _consoleForm.WriteInfo("You can continue working while PowerCLI loads. VMware features will be available when loading completes.");

                // Load PowerCLI in background
                await Task.Run(async () =>
                {
                    await _vmwareManager.InitializePowerCLIAsync();
                });

                // Notify user when complete
                _consoleForm.WriteSuccess("========================================");
                _consoleForm.WriteSuccess("PowerCLI is now loaded and ready!");
                _consoleForm.WriteSuccess("VMware features are available.");
                _consoleForm.WriteSuccess("========================================");

                // Optional: Show a message box notification
                MessageBox.Show("PowerCLI has finished loading.\n\nVMware features are now ready to use.",
                    "PowerCLI Ready",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to load PowerCLI in background: {ex.Message}");
                _consoleForm.WriteWarning("VMware features may not be available. Please check the PowerCLI module path.");
            }
        }

        private async Task LoadESXiHostHealthAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Loading ESXi host health information...");

                // Get detailed host information
                var hosts = await _vmwareManager.GetESXiHostsDetailedAsync();

                // Populate the ESXi Health DataGridView
                foreach (var host in hosts)
                {
                    int rowIndex = dgvEsxiHealthCheck.Rows.Add(); // Use your actual DataGridView name
                    DataGridViewRow row = dgvEsxiHealthCheck.Rows[rowIndex];

                    // Update these column names to match your actual DataGridView columns
                    row.Cells["clmServerName"].Value = host.Name;
                    row.Cells["clmState"].Value = host.ConnectionState;
                    row.Cells["clmStatus"].Value = host.Status;
                    row.Cells["clmCluster"].Value = host.Cluster;
                    row.Cells["clmConsumedCpu"].Value = $"{host.ConsumedCPU:F1}%";
                    row.Cells["clmConsumedMemory"].Value = $"{host.ConsumedMemory:F1}%";
                    row.Cells["clmHaState"].Value = host.HAState;
                    row.Cells["clmUptime"].Value = $"{host.Uptime} days";

                    // Apply color coding for connection status
                    if (host.ConnectionState.Equals("Connected", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["clmState"].Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        row.Cells["clmState"].Style.ForeColor = Color.Red;
                    }

                    // Apply color coding for CPU usage
                    if (host.ConsumedCPU > 80)
                    {
                        row.Cells["clmConsumedCpu"].Style.ForeColor = Color.Red;
                        row.Cells["clmConsumedCpu"].Style.BackColor = Color.LightPink;
                    }
                    else if (host.ConsumedCPU > 60)
                    {
                        row.Cells["clmConsumedCpu"].Style.ForeColor = Color.Orange;
                    }

                    // Apply color coding for Memory usage
                    if (host.ConsumedMemory > 80)
                    {
                        row.Cells["clmConsumedMemory"].Style.ForeColor = Color.Red;
                        row.Cells["clmConsumedMemory"].Style.BackColor = Color.LightPink;
                    }
                    else if (host.ConsumedMemory > 60)
                    {
                        row.Cells["clmConsumedMemory"].Style.ForeColor = Color.Orange;
                    }
                }

                _consoleForm.WriteSuccess($"Loaded health information for {hosts.Count} ESXi hosts");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading ESXi host health: {ex.Message}");
                throw;
            }
        }

        private async Task LoadVMHealthAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Loading virtual machine health information...");

                // Get detailed VM information
                var vms = await _vmwareManager.GetVirtualMachinesDetailedAsync();

                // Populate the VM Health DataGridView
                foreach (var vm in vms)
                {
                    int rowIndex = dgvVmHealthCheck.Rows.Add(); // Use your actual DataGridView name
                    DataGridViewRow row = dgvVmHealthCheck.Rows[rowIndex];

                    // Update these column names to match your actual DataGridView columns
                    row.Cells["clmVmName"].Value = vm.Name;
                    row.Cells["clmPowerState"].Value = vm.PowerState;
                    row.Cells["clmVmStatus"].Value = vm.Status;
                    row.Cells["clmProvisionedSpace"].Value = $"{vm.ProvisionedSpace:F2} GB";
                    row.Cells["clmUsedSpace"].Value = $"{vm.UsedSpace:F2} GB";
                    row.Cells["clmHostCpu"].Value = $"{vm.HostCPU:F0} MHz";
                    row.Cells["clmHostMemory"].Value = $"{vm.HostMemory:F2} GB";

                    // Apply color coding for power state
                    if (vm.PowerState.Equals("PoweredOn", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["clmPowerState"].Style.ForeColor = Color.Green;
                    }
                    else if (vm.PowerState.Equals("PoweredOff", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["clmPowerState"].Style.ForeColor = Color.Red;
                    }
                    else
                    {
                        row.Cells["clmPowerState"].Style.ForeColor = Color.Orange;
                    }

                    // Apply color coding for VM status
                    if (vm.Status.Equals("green", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["clmVmStatus"].Style.ForeColor = Color.Green;
                    }
                    else if (vm.Status.Equals("red", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["clmVmStatus"].Style.ForeColor = Color.Red;
                    }
                    else
                    {
                        row.Cells["clmVmStatus"].Style.ForeColor = Color.Orange;
                    }
                }

                _consoleForm.WriteSuccess($"Loaded health information for {vms.Count} virtual machines");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading VM health: {ex.Message}");
                throw;
            }
        }

        private void ClearHealthCheckResults()
        {
            // Clear ESXi Host health results
            if (dgvEsxiHealthCheck != null)
            {
                dgvEsxiHealthCheck.Rows.Clear();
            }

            // Clear VM health results
            if (dgvVmHealthCheck != null)
            {
                dgvVmHealthCheck.Rows.Clear();
            }

            _consoleForm.WriteInfo("Cleared previous health check results");
        }

        #endregion

        #region Spice PMI tab button events
        private async void btnCheckFileSystem_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during operation
                btnCheckFileSystem.Enabled = false;
                btnCheckFileSystem.Text = "Checking...";

                _consoleForm.WriteInfo("Starting disk info update for all hosts");

                // Initialize LinuxService if not already done
                if (_linuxService == null)
                {
                    _linuxService = new Linux_Service(_consoleForm);
                }

                // Check if plink is available
                if (!_linuxService.IsPlinkAvailable())
                {
                    _consoleForm.WriteError("Plink.exe not found. Please ensure PuTTY is installed and plink.exe is in PATH or current directory.");
                    return;
                }

                // Define hosts to check
                string[] hosts = { "ccelpro1", "ccesec1", "ccegitsvr1", "ccesa1", "ccesa2" };

                // Use the credentials the user signed in with
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Not authenticated. Please log in first.");
                    return;
                }

                string sshUsername = CredentialManager.GetUsername();
                string sshPassword = CredentialManager.GetPassword();

                // Strip domain from username if present (e.g., "spectre\jblow" -> "jblow")
                if (sshUsername.Contains('\\'))
                {
                    sshUsername = sshUsername.Split('\\').Last();
                }

                // Cache host keys for all servers first (to avoid prompts during connection)
                foreach (string host in hosts)
                {
                    await _linuxService.CacheHostKeyAsync(host, sshUsername, sshPassword);
                }

                foreach (string host in hosts)
                {
                    try
                    {
                        _consoleForm.WriteInfo($"Processing host: {host}");

                        // Find the corresponding DataGridView for this host
                        DataGridView currentDgv = this.Controls.Find($"dgv{host}", true).FirstOrDefault() as DataGridView;

                        if (currentDgv != null)
                        {
                            _consoleForm.WriteInfo($"Found DataGridView for {host}, retrieving disk info");

                            // Get disk information from the server
                            var diskInfo = await _linuxService.GetDiskInfoAsync(host, sshUsername, sshPassword);

                            // Update the DataGridView with the disk information
                            await UpdateDataGridView(currentDgv, diskInfo);
                        }
                        else
                        {
                            _consoleForm.WriteError($"DataGridView not found for host: {host}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleForm.WriteError($"Error processing {host}: {ex.Message}");
                        // Continue with next host instead of breaking the entire operation
                        continue;
                    }
                }

                _consoleForm.WriteSuccess("Disk info update completed for all hosts");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during file system check: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                btnCheckFileSystem.Enabled = true;
                btnCheckFileSystem.Text = "Check File System";
            }
        }

        private async void btnPerformHealthChk_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during operation
                btnPerformHealthChk.Enabled = false;
                btnPerformHealthChk.Text = "Checking...";

                _consoleForm.WriteInfo("Starting ESXi and VM Health Check...");

                // Use the credentials the user signed in with for vCenter
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Not authenticated. Please log in first.");
                    return;
                }

                // Initialize VMware Manager if not already done
                if (_vmwareManager == null)
                {
                    string vCenterUser = CredentialManager.GetUsername();
                    string vCenterPass = CredentialManager.GetPassword();

                    _vmwareManager = new VMwareManager(_vCenterServer, vCenterUser, vCenterPass, _consoleForm, POWERCLI_MODULE_PATH);
                }

                // Initialize PowerCLI modules
                await InitializeVMwareAsync();

                // Clear existing data in DataGridViews
                ClearHealthCheckResults();

                // Get and display ESXi Host information
                await LoadESXiHostHealthAsync();

                // Get and display VM information  
                await LoadVMHealthAsync();

                _consoleForm.WriteSuccess("ESXi and VM Health Check completed successfully");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during health check: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                btnPerformHealthChk.Enabled = true;
                btnPerformHealthChk.Text = "Perform Health Check";
            }
        }

        private async void btnCheckRepHealth_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during operation
                btnCheckRepHealth.Enabled = false;
                btnCheckRepHealth.Text = "Checking...";

                _consoleForm.WriteInfo("Starting LDAP Replication Health Check...");

                // Initialize LinuxService if not already done
                if (_linuxService == null)
                {
                    _linuxService = new Linux_Service(_consoleForm);
                }

                // Check if plink is available
                if (!_linuxService.IsPlinkAvailable())
                {
                    _consoleForm.WriteError("Plink.exe not found. Please ensure PuTTY is installed and plink.exe is in PATH or current directory.");
                    return;
                }

                // Prompt user for Linux SSH credentials
                _consoleForm.WriteInfo("Please provide Linux SSH credentials for replication health check...");
                var (success, hostname, username, password) = LinuxCredentialDialog.GetCredentials();

                if (!success)
                {
                    _consoleForm.WriteWarning("Replication health check cancelled by user.");
                    return;
                }

                // Clear previous results
                ClearReplicationResults();

                // Build the dsconf command
                string command = $"dsconf -D 'cn=Directory Manager' -w '{password}' ldap://{hostname}:389 replication monitor";

                // Prepare credentials for both servers (2 prompts each = 4 total)
                string[] inputs = new string[]
                {
                    "cn=Directory Manager",  // First server Bind DN
                    password,                // First server password
                    "cn=Directory Manager",  // Second server Bind DN
                    password                 // Second server password
                };

                // Execute the interactive command and capture output
                string output = await _linuxService.ExecuteInteractiveSSHCommandAsync(hostname, username, password, command, inputs);

                // Parse and display the results
                ParseReplicationMonitorOutput(output, password);

                _consoleForm.WriteSuccess("LDAP Replication Health Check completed.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during replication health check: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                btnCheckRepHealth.Enabled = true;
                btnCheckRepHealth.Text = "Check Replication Health";
            }
        }
        /*
        private async void btnCheckRepHealth_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during operation
                btnCheckRepHealth.Enabled = false;
                btnCheckRepHealth.Text = "Opening SSH...";

                _consoleForm.WriteInfo("Starting LDAP Replication Health Check...");

                // Initialize LinuxService if not already done
                if (_linuxService == null)
                {
                    _linuxService = new Linux_Service(_consoleForm);
                }

                // Check if plink is available
                if (!_linuxService.IsPlinkAvailable())
                {
                    _consoleForm.WriteError("Plink.exe not found. Please ensure PuTTY is installed and plink.exe is in PATH or current directory.");
                    return;
                }

                // Prompt user for Linux SSH credentials
                _consoleForm.WriteInfo("Please provide Linux SSH credentials for replication health check...");
                var (success, hostname, username, password) = LinuxCredentialDialog.GetCredentials();
                
                if (!success)
                {
                    _consoleForm.WriteWarning("Replication health check cancelled by user.");
                    return;
                }

                _consoleForm.WriteInfo($"Opening visible SSH session to {hostname}...");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k plink.exe {username}@{hostname} -pw {password}",
                    UseShellExecute = true,
                    // RedirectStandardInput = true,
                    CreateNoWindow = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };

                var process = Process.Start(processInfo);


                // Wait for SSH connection to establish and shell prompt to appear
                _consoleForm.WriteSuccess($"Waiting 3 seconds for SSH connection to establish and shell prompt to appear");
                await Task.Delay(3000); // 8 seconds for SSH banner and prompt

                _consoleForm?.WriteInfo($" IntPtr = '{IntPtr.Zero}' ");
                _consoleForm?.WriteInfo($" Process MainWindow = '{process.MainWindowHandle}' ");

                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    _consoleForm.WriteSuccess($"Setting SSH Window as active....");
                    SetForegroundWindow(process.MainWindowHandle);
                }

                await Task.Delay(1000);

                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(2000);

                // Send the dsconf command
                string command = $"dsconf -D 'cn=Directory Manager' -w '{password}' ldap://{hostname}:389 replication monitor";
                string command1 = $"cn=Directory Manager";
                System.Windows.Forms.SendKeys.SendWait(command);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(1000);
                System.Windows.Forms.SendKeys.SendWait(command1);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(1000);
                System.Windows.Forms.SendKeys.SendWait(password);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(1000);
                System.Windows.Forms.SendKeys.SendWait(command1);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                await Task.Delay(1000);
                System.Windows.Forms.SendKeys.SendWait(password);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");

                // Wait for replication monitor to gather data
                await Task.Delay(3000);

                btnCheckRepHealth.Text = "Check Replication Health";
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error opening SSH session: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                btnCheckRepHealth.Enabled = true;
                btnCheckRepHealth.Text = "Check Replication Health";
            }
        }
        */
        #endregion

        #region Database Loading and Saving Methods

        private string GetSecurityGroupsOU()
        {
            try
            {
                var entries = _databaseService.LoadOUConfiguration();

                foreach (var entry in entries)
                {
                    if (entry.MiddleName.Equals("SecurityGroups", StringComparison.OrdinalIgnoreCase))
                    {
                        _consoleForm?.WriteInfo($"Found Security Groups OU: {entry.OU}");
                        return entry.OU;
                    }
                }

                _consoleForm?.WriteWarning("No Security Groups OU found in database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error reading Security Groups OU from database: {ex.Message}");
            }

            return null;
        }

        private void SaveComputerListToDB()
        {
            try
            {
                _consoleForm.WriteInfo("Saving Computer List to database...");

                var entries = new List<ComputerListEntry>();

                CollectComputersFromCheckedListBox(entries, cbxLinuxList, "Linux");
                CollectComputersFromCheckedListBox(entries, cbxCriticalLinuxList, "CriticalLinux");
                CollectComputersFromCheckedListBox(entries, cbxCriticalWindowsList, "CriticalWindows");
                CollectComputersFromCheckedListBox(entries, cbxCriticalNasList, "CriticalNas");
                CollectComputersFromCheckedListBox(entries, cbxOfficeExemptList, "OfficeExempt");

                _databaseService.SaveComputerList(entries);
                _consoleForm.WriteSuccess($"Computer List saved to database ({entries.Count} entries).");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error saving Computer List to database: {ex.Message}");
            }
        }

        private void CollectComputersFromCheckedListBox(List<ComputerListEntry> entries, CheckedListBox checkedListBox, string type)
        {
            foreach (string computerName in checkedListBox.Items)
            {
                entries.Add(new ComputerListEntry
                {
                    Computername = computerName,
                    Type = type,
                    VMWare = "N/A",
                    Instructions = "Added via Configuration"
                });
            }
        }

        private void SaveOUConfigurationToDB()
        {
            try
            {
                _consoleForm.WriteInfo("Saving OU configuration to database...");

                var entries = new List<OUConfigEntry>();

                CollectOUsFromCheckedListBox(entries, cbxListWorkStationOu);
                CollectOUsFromCheckedListBox(entries, cbxListPatriotParkOu);
                CollectOUsFromCheckedListBox(entries, cbxListWindowsServersOu);
                CollectOUsFromCheckedListBox(entries, cbxListSecurityGroupsOu);

                // Include sgfilter keyword if set
                string keyword = txbSecurityGroupKW.Text.Trim();
                if (!string.IsNullOrEmpty(keyword))
                {
                    entries.Add(new OUConfigEntry
                    {
                        OU = "NA",
                        MiddleName = "sgfilter",
                        Keyword = keyword
                    });
                }

                _databaseService.SaveOUConfiguration(entries);
                _consoleForm.WriteSuccess($"OU configuration saved to database ({entries.Count} entries).");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error saving OU configuration to database: {ex.Message}");
            }
        }

        private void CollectOUsFromCheckedListBox(List<OUConfigEntry> entries, CheckedListBox checkedListBox)
        {
            string middleName = GetMiddleNameFromControlName(checkedListBox.Name);

            foreach (string ou in checkedListBox.Items)
            {
                entries.Add(new OUConfigEntry
                {
                    OU = ou,
                    MiddleName = middleName,
                    Keyword = ""
                });
            }
        }

        private void LoadComputerListFromDB()
        {
            try
            {
                _consoleForm.WriteInfo("Loading Computer List from database...");

                ClearComputerListCheckedListBoxes();

                var entries = _databaseService.LoadComputerList();

                if (entries.Count == 0)
                {
                    _consoleForm.WriteWarning("No computer entries found in database.");
                    return;
                }

                foreach (var entry in entries)
                {
                    AddComputerToCheckedListBox(entry.Computername, entry.Type);
                }

                _consoleForm.WriteSuccess($"Loaded {entries.Count} computer entries from database.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading Computer List from database: {ex.Message}");
            }
        }

        private void LoadOUConfigurationFromDB()
        {
            try
            {
                _consoleForm.WriteInfo("Loading OU configuration from database...");

                ClearOUCheckedListBoxes();

                var entries = _databaseService.LoadOUConfiguration();

                if (entries.Count == 0)
                {
                    _consoleForm.WriteWarning("No OU configuration entries found in database.");
                    return;
                }

                int loadedCount = 0;
                foreach (var entry in entries)
                {
                    if (entry.MiddleName.Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                    {
                        txbSecurityGroupKW.Text = entry.Keyword;
                        _consoleForm.WriteInfo($"Loaded security group filter keyword: {entry.Keyword}");
                    }
                    else
                    {
                        AddOUToCheckedListBox(entry.OU, entry.MiddleName);
                        loadedCount++;
                    }
                }

                _consoleForm.WriteSuccess($"Loaded {loadedCount} OU configuration entries from database.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading OU configuration from database: {ex.Message}");
            }
        }

        private void LoadImportantVariablesFromDB()
        {
            try
            {
                _consoleForm.WriteInfo("Loading Important Variables from database...");

                ClearImportantTextBoxes();

                var entries = _databaseService.LoadOUConfiguration();

                foreach (var entry in entries)
                {
                    if (entry.MiddleName.Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                    {
                        txbSecurityGroupKW.Text = entry.Keyword;
                    }
                }

                _consoleForm.WriteSuccess("Important variables loaded from database.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading Important Variables from database: {ex.Message}");
            }
        }

        private void LoadLogConfigurationFromDB()
        {
            try
            {
                _consoleForm.WriteInfo("Loading Log Configuration from database...");

                _ldapServerInstances.Clear();

                var entries = _databaseService.LoadLogConfiguration();

                if (entries.Count == 0)
                {
                    _consoleForm.WriteWarning("No log configuration entries found in database.");
                    return;
                }

                foreach (var entry in entries)
                {
                    _ldapServerInstances[entry.Server] = entry.ServerInstance;
                }

                if (_ldapServerInstances.Count >= 1)
                {
                    var firstServer = _ldapServerInstances.ElementAt(0);
                    txbLdapServerInstace1.Text = $"{firstServer.Key}: {firstServer.Value}";
                }

                if (_ldapServerInstances.Count >= 2)
                {
                    var secondServer = _ldapServerInstances.ElementAt(1);
                    txbLdapServerInstace2.Text = $"{secondServer.Key}: {secondServer.Value}";
                }

                _consoleForm.WriteSuccess($"Loaded {entries.Count} LDAP server instance entries from database.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading Log Configuration from database: {ex.Message}");
            }
        }

        #endregion

        #region OU Configuration Management tab functions

        // Extract middleName from CheckedListBox control name
        private string GetMiddleNameFromControlName(string controlName)
        {
            // Remove "cbxList" prefix and "Ou" suffix to get the middle name
            if (controlName.StartsWith("cbxList") && controlName.EndsWith("Ou"))
            {
                string middleName = controlName.Substring(7, controlName.Length - 9); // Remove "cbxList" (7 chars) and "Ou" (2 chars)
                return middleName;
            }

            return "Unknown";
        }

        private void ClearImportantTextBoxes()
        {
            txbSecurityGroupKW.Text = "";
        }

        #region Log Configuration and Fetching

        private void btnSubmitServerInstance_Click(object sender, EventArgs e)
        {
            try
            {
                _consoleForm.WriteInfo("Saving LDAP Server Instance configuration...");

                // Parse textbox values (format: "servername: instance")
                var instances = new Dictionary<string, string>();

                if (!string.IsNullOrWhiteSpace(txbLdapServerInstace1.Text))
                {
                    string[] parts = txbLdapServerInstace1.Text.Split(':');
                    if (parts.Length == 2)
                    {
                        instances[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                if (!string.IsNullOrWhiteSpace(txbLdapServerInstace2.Text))
                {
                    string[] parts = txbLdapServerInstace2.Text.Split(':');
                    if (parts.Length == 2)
                    {
                        instances[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                // Save to database
                var entries = instances.Select(kvp => new LogConfigEntry
                {
                    Server = kvp.Key,
                    ServerInstance = kvp.Value
                }).ToList();

                _databaseService.SaveLogConfiguration(entries);

                // Update in-memory dictionary
                _ldapServerInstances = instances;

                _consoleForm.WriteSuccess("LDAP Server Instance configuration saved to database.");
                MessageBox.Show("LDAP Server Instance configuration saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error saving Log Configuration: {ex.Message}");
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateLinuxServerDropdown()
        {
            try
            {
                cbxServerSelection.Items.Clear();

                var entries = _databaseService.LoadComputerList();

                foreach (var entry in entries)
                {
                    if (entry.Type.Equals("Linux", StringComparison.OrdinalIgnoreCase) ||
                        entry.Type.Equals("CriticalLinux", StringComparison.OrdinalIgnoreCase))
                    {
                        cbxServerSelection.Items.Add(entry.Computername);
                    }
                }

                if (cbxServerSelection.Items.Count > 0)
                {
                    cbxServerSelection.SelectedIndex = 0;
                }

                _consoleForm.WriteSuccess($"Loaded {cbxServerSelection.Items.Count} Linux servers into dropdown.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error populating Linux server dropdown: {ex.Message}");
            }
        }

        // Fetch logs button click handler
        private async void btnFetchLogs_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during operation
                btnFetchLogs.Enabled = false;
                btnFetchLogs.Text = "Fetching...";

                // Check authentication
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Please log in first before fetching logs.");
                    MessageBox.Show("Please log in first.", "Authentication Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate selections
                if (cbxServerSelection.SelectedItem == null)
                {
                    MessageBox.Show("Please select a server.", "Server Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cbxLogSourceSelection.SelectedItem == null)
                {
                    MessageBox.Show("Please select a log source.", "Log Source Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get selected server
                string selectedServer = cbxServerSelection.SelectedItem.ToString();

                // Get credentials
                string sshUsername = CredentialManager.GetUsername();
                string sshPassword = CredentialManager.GetPassword();

                // Strip domain from username if present
                if (sshUsername.Contains('\\'))
                {
                    sshUsername = sshUsername.Split('\\').Last();
                }

                // Update status
                lblLogStatusResults.Text = "Fetching logs...";
                rtbLogOutput.Clear();

                // Build journalctl command based on filters
                string command = BuildLogFetchCommand();

                _consoleForm.WriteInfo($"Fetching logs from {selectedServer}...");
                _consoleForm.WriteInfo($"Command: {command}");

                // Execute SSH command
                string logOutput = await _linuxService.ExecuteSSHCommandAsync(selectedServer, sshUsername, sshPassword, command);

                // Display logs in RichTextBox
                rtbLogOutput.Text = logOutput;

                // Count lines
                int lineCount = logOutput.Split('\n').Length;
                lblLogStatusResults.Text = $"Ready - {lineCount} lines fetched";

                _consoleForm.WriteSuccess($"Fetched {lineCount} lines of logs from {selectedServer}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error fetching logs: {ex.Message}");
                lblLogStatusResults.Text = "Error occurred";
                rtbLogOutput.AppendText($"\n\n=== ERROR ===\n{ex.Message}");
            }
            finally
            {
                btnFetchLogs.Enabled = true;
                btnFetchLogs.Text = "Fetch Logs";
            }
        }

        // Build the log fetch command based on user selections
        private string BuildLogFetchCommand()
        {
            StringBuilder command = new StringBuilder();

            string logSource = cbxLogSourceSelection.SelectedItem?.ToString() ?? "All";
            string priority = cmbLogPriority.SelectedItem?.ToString() ?? "All Levels";
            string keyword = txbLogKeyword.Text.Trim();
            bool caseSensitive = cbxCaseSensitive.Checked;

            // Determine if we're using journalctl or file-based logs
            if (logSource == "All" || logSource.Contains("journalctl") || logSource == "Security/Authentication")
            {
                // Use journalctl
                command.Append("journalctl");

                // Add date range
                if (chkLastHourOnly.Checked)
                {
                    command.Append(" --since \"1 hour ago\"");
                }
                else
                {
                    string startDate = dtpLogStartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    string endDate = dtpLogEndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    command.Append($" --since \"{startDate}\" --until \"{endDate}\"");
                }

                // Add priority filter
                if (priority != "All Levels")
                {
                    string priorityLevel = priority.Split(' ')[0].ToLower(); // Extract "error" from "Error"
                    command.Append($" -p {priorityLevel}");
                }

                // Add specific unit filter for Security/Authentication
                if (logSource == "Security/Authentication")
                {
                    command.Append(" -u sshd -u systemd-logind");
                }

                // Add keyword filter if specified
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    string grepFlags = caseSensitive ? "" : "-i";
                    command.Append($" | grep {grepFlags} \"{keyword}\"");
                }
            }
            else if (logSource.Contains("Directory Server"))
            {
                // File-based Directory Server logs
                string selectedServer = cbxServerSelection.SelectedItem?.ToString() ?? "";
                string instanceName = "";

                // Get instance name for selected server
                if (_ldapServerInstances.ContainsKey(selectedServer))
                {
                    instanceName = _ldapServerInstances[selectedServer];
                }
                else
                {
                    // Default instance name if not configured
                    instanceName = $"slapd-{selectedServer}";
                }

                string logFile = "";
                if (logSource.Contains("Errors"))
                {
                    logFile = $"/var/log/dirsrv/{instanceName}/errors";
                }
                else if (logSource.Contains("Access"))
                {
                    logFile = $"/var/log/dirsrv/{instanceName}/access";
                }
                else if (logSource.Contains("Audit"))
                {
                    logFile = $"/var/log/dirsrv/{instanceName}/audit";
                }

                // Use grep with date filtering on log files
                command.Append($"cat {logFile}");

                // Add keyword filter (Directory Server logs don't have easy date filtering)
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    string grepFlags = caseSensitive ? "" : "-i";
                    command.Append($" | grep {grepFlags} \"{keyword}\"");
                }
            }

            return command.ToString();
        }

        // Clear logs button click handler
        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            rtbLogOutput.Clear();
            lblLogStatusResults.Text = "Ready";
            _consoleForm.WriteInfo("Log output cleared.");
        }

        // Export logs button click handler
        private void btnExportLogs_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rtbLogOutput.Text))
                {
                    MessageBox.Show("No logs to export.", "Nothing to Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create SaveFileDialog
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Export Logs";
                    saveFileDialog.FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, rtbLogOutput.Text);
                        _consoleForm.WriteSuccess($"Logs exported to: {saveFileDialog.FileName}");
                        MessageBox.Show($"Logs exported successfully to:\n{saveFileDialog.FileName}",
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error exporting logs: {ex.Message}");
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Last hour only checkbox handler
        private void chkLastHourOnly_CheckedChanged(object sender, EventArgs e)
        {
            // Disable/enable date pickers based on checkbox state
            dtpLogStartDate.Enabled = !chkLastHourOnly.Checked;
            dtpLogEndDate.Enabled = !chkLastHourOnly.Checked;
        }

        #endregion

        // Clear all OU CheckedListBoxes
        private void ClearOUCheckedListBoxes()
        {
            cbxListWorkStationOu.Items.Clear();
            cbxListPatriotParkOu.Items.Clear();
            cbxListWindowsServersOu.Items.Clear();
            cbxListSecurityGroupsOu.Items.Clear(); // ADD THIS LINE
        }

        // Add OU to appropriate CheckedListBox based on middleName
        private void AddOUToCheckedListBox(string ou, string middleName)
        {
            switch (middleName.ToLower())
            {
                case "workstation":
                    if (!cbxListWorkStationOu.Items.Contains(ou))
                        cbxListWorkStationOu.Items.Add(ou, false);
                    break;
                case "patriotpark":
                    if (!cbxListPatriotParkOu.Items.Contains(ou))
                        cbxListPatriotParkOu.Items.Add(ou, false);
                    break;
                case "windowsservers":
                    if (!cbxListWindowsServersOu.Items.Contains(ou))
                        cbxListWindowsServersOu.Items.Add(ou, false);
                    break;
                case "securitygroups": // ADD THIS CASE
                    cbxListSecurityGroupsOu.Items.Add(ou);
                    break;
                default:
                    _consoleForm.WriteWarning($"Unknown middleName '{middleName}' for OU: {ou}");
                    break;
            }
        }


        // Show native Windows OU selection dialog
        private string ShowOUSelectionDialog(string categoryName)
        {
            try
            {
                // Check authentication
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Cannot browse Active Directory: Please log in first.");
                    return null;
                }

                _consoleForm.WriteInfo($"Browsing Active Directory for {categoryName} OUs...");

                // Get list of OUs from AD using existing AD_Service method
                var organizationalUnits = _adService.BrowseOrganizationalUnits();

                if (organizationalUnits.Count == 0)
                {
                    _consoleForm.WriteWarning("No Organizational Units found in Active Directory.");
                    return null;
                }

                // Use simple selection dialog
                return ShowSimpleOUSelectionDialog(organizationalUnits.ToArray(), categoryName);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error browsing OUs for {categoryName}: {ex.Message}");
            }

            return null;
        }
        // Show RHDS OU selection dialog
        private string ShowRHDSOUSelectionDialog(string categoryName)
        {
            try
            {
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Cannot browse Directory Services: Please log in first.");
                    return null;
                }

                _consoleForm.WriteInfo($"Browsing Directory Services for {categoryName} OUs...");

                var organizationalUnits = _rhdsService.BrowseOrganizationalUnits();

                if (organizationalUnits.Count == 0)
                {
                    _consoleForm.WriteWarning("No Organizational Units found in Directory Services.");
                    return null;
                }

                return ShowSimpleOUSelectionDialog(organizationalUnits.ToArray(), categoryName);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error browsing RHDS OUs for {categoryName}: {ex.Message}");
            }

            return null;
        }
        // Simple OU selection dialog as final fallback
        private string ShowSimpleOUSelectionDialog(string[] organizationalUnits, string categoryName)
        {
            using (var selectionForm = new Form())
            {
                selectionForm.Text = $"Select OU for {categoryName}";
                selectionForm.Size = new Size(700, 500);
                selectionForm.StartPosition = FormStartPosition.CenterParent;

                var listBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9)
                };

                var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
                var okButton = new System.Windows.Forms.Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(530, 15),
                    Size = new Size(75, 25)
                };
                var cancelButton = new System.Windows.Forms.Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(610, 15),
                    Size = new Size(75, 25)
                };

                foreach (string ou in organizationalUnits.OrderBy(o => o))
                    listBox.Items.Add(ou);

                buttonPanel.Controls.AddRange(new Control[] { okButton, cancelButton });
                selectionForm.Controls.AddRange(new Control[] { buttonPanel, listBox });

                if (selectionForm.ShowDialog() == DialogResult.OK && listBox.SelectedItem != null)
                    return listBox.SelectedItem.ToString();
            }

            return null;
        }

        private void AddOUToConfiguration(CheckedListBox targetCheckedListBox)
        {
            try
            {
                // Extract middleName from control name dynamically
                string middleName = GetMiddleNameFromControlName(targetCheckedListBox.Name);

                string selectedOU = ShowOUSelectionDialog(middleName);

                if (!string.IsNullOrEmpty(selectedOU))
                {
                    // Check if OU already exists in the CheckedListBox
                    if (targetCheckedListBox.Items.Contains(selectedOU))
                    {
                        _consoleForm.WriteWarning($"OU already exists in {middleName} list: {selectedOU}");
                        return;
                    }

                    // Add to CheckedListBox
                    targetCheckedListBox.Items.Add(selectedOU, false);

                    // Save to database
                    SaveOUConfigurationToDB();

                    _consoleForm.WriteSuccess($"Added OU to {middleName}: {selectedOU}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error adding OU: {ex.Message}");
            }
        }
        private void AddRHDSOUToConfiguration(CheckedListBox targetCheckedListBox)
        {
            try
            {
                // Extract middleName from control name dynamically
                string middleName = GetMiddleNameFromControlName(targetCheckedListBox.Name);

                string selectedOU = ShowRHDSOUSelectionDialog(middleName);

                if (!string.IsNullOrEmpty(selectedOU))
                {
                    // Check if OU already exists in the CheckedListBox
                    if (targetCheckedListBox.Items.Contains(selectedOU))
                    {
                        _consoleForm.WriteWarning($"OU already exists in {middleName} list: {selectedOU}");
                        return;
                    }

                    // Add to CheckedListBox
                    targetCheckedListBox.Items.Add(selectedOU, false);

                    // Save to database
                    SaveOUConfigurationToDB();

                    _consoleForm.WriteSuccess($"Added OU to {middleName}: {selectedOU}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error adding RHDS OU: {ex.Message}");
            }
        }

        #endregion

        #region OU Configuration Management tab Button Event Handlers

        // Updated button event handlers using dynamic approach
        private void btnAddWorkStationOu_Click(object sender, EventArgs e)
        {
            AddOUToConfiguration(cbxListWorkStationOu);
        }

        private void btnAddPatriotParkOu_Click(object sender, EventArgs e)
        {
            AddOUToConfiguration(cbxListPatriotParkOu);
        }

        private void btnAddWindowsServersOu_Click(object sender, EventArgs e)
        {
            AddOUToConfiguration(cbxListWindowsServersOu);
        }
        private void btnAddSecurityGroupsOU_Click(object sender, EventArgs e)
        {
            AddRHDSOUToConfiguration(cbxListSecurityGroupsOu);
        }
        // Remove selected OUs button event handler using dynamic middleName extraction
        private void btnRemoveSelectedOus_Click(object sender, EventArgs e)
        {
            try
            {
                _consoleForm.WriteInfo("Removing selected OUs from all categories...");

                int totalRemoved = 0;

                // Remove from all CheckedListBoxes using dynamic approach
                totalRemoved += RemoveSelectedOUsFromCheckedListBox(cbxListWorkStationOu);
                totalRemoved += RemoveSelectedOUsFromCheckedListBox(cbxListPatriotParkOu);
                totalRemoved += RemoveSelectedOUsFromCheckedListBox(cbxListWindowsServersOu);
                totalRemoved += RemoveSelectedOUsFromCheckedListBox(cbxListSecurityGroupsOu); // ADD THIS LINE

                if (totalRemoved > 0)
                {
                    // Save updated configuration to database
                    SaveOUConfigurationToDB();
                    _consoleForm.WriteSuccess($"Removed {totalRemoved} selected OUs from configuration.");
                }
                else
                {
                    _consoleForm.WriteInfo("No OUs were selected for removal.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error removing selected OUs: {ex.Message}");
            }
        }

        // Helper method to remove selected OUs from a CheckedListBox using dynamic middleName
        private int RemoveSelectedOUsFromCheckedListBox(CheckedListBox checkedListBox)
        {
            // Extract category name dynamically
            string categoryName = GetMiddleNameFromControlName(checkedListBox.Name);

            var itemsToRemove = new List<string>();

            // Collect checked items
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                if (checkedListBox.GetItemChecked(i))
                {
                    itemsToRemove.Add(checkedListBox.Items[i].ToString());
                }
            }

            // Remove collected items
            foreach (string item in itemsToRemove)
            {
                checkedListBox.Items.Remove(item);
                _consoleForm.WriteInfo($"Removed OU from {categoryName}: {item}");
            }

            return itemsToRemove.Count;
        }

        #endregion

        #region Computer List Configuration Management Tab functions

        // Clear all Computer List CheckedListBoxes
        private void ClearComputerListCheckedListBoxes()
        {
            cbxLinuxList.Items.Clear();
            cbxCriticalLinuxList.Items.Clear();
            cbxCriticalWindowsList.Items.Clear();
            cbxCriticalNasList.Items.Clear();
            cbxOfficeExemptList.Items.Clear();
        }

        // Add computer to appropriate CheckedListBox based on type
        private void AddComputerToCheckedListBox(string computerName, string type)
        {
            switch (type.ToLower())
            {
                case "linux":
                    if (!cbxLinuxList.Items.Contains(computerName))
                        cbxLinuxList.Items.Add(computerName, false);
                    break;
                case "criticallinux":
                    if (!cbxCriticalLinuxList.Items.Contains(computerName))
                        cbxCriticalLinuxList.Items.Add(computerName, false);
                    break;
                case "criticalwindows":
                    if (!cbxCriticalWindowsList.Items.Contains(computerName))
                        cbxCriticalWindowsList.Items.Add(computerName, false);
                    break;
                case "criticalnas":
                    if (!cbxCriticalNasList.Items.Contains(computerName))
                        cbxCriticalNasList.Items.Add(computerName, false);
                    break;
                case "officeexempt":
                    if (!cbxOfficeExemptList.Items.Contains(computerName))
                        cbxOfficeExemptList.Items.Add(computerName, false);
                    break;
                default:
                    _consoleForm.WriteWarning($"Unknown computer type '{type}' for computer: {computerName}");
                    break;
            }
        }

        // Generic method to add computer to list
        private void AddComputerToList(System.Windows.Forms.TextBox sourceTextBox, CheckedListBox targetCheckedListBox, string computerType)
        {
            try
            {
                string computerName = sourceTextBox.Text.Trim();

                if (string.IsNullOrEmpty(computerName))
                {
                    _consoleForm.WriteWarning($"Please enter a computer name for {computerType} before adding.");
                    return;
                }

                // Check if computer already exists in the CheckedListBox
                if (targetCheckedListBox.Items.Contains(computerName))
                {
                    _consoleForm.WriteWarning($"Computer already exists in {computerType} list: {computerName}");
                    sourceTextBox.Clear();
                    return;
                }

                // Add to CheckedListBox
                targetCheckedListBox.Items.Add(computerName, false);

                // Clear the textbox
                sourceTextBox.Clear();

                // Save to database
                SaveComputerListToDB();

                _consoleForm.WriteSuccess($"Added computer to {computerType}: {computerName}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error adding computer to {computerType}: {ex.Message}");
            }
        }

        // Helper method to remove selected computers from a specific CheckedListBox
        private int RemoveSelectedComputersFromCheckedListBox(CheckedListBox checkedListBox, string categoryName)
        {
            var itemsToRemove = new List<string>();

            // Collect checked items
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                if (checkedListBox.GetItemChecked(i))
                {
                    itemsToRemove.Add(checkedListBox.Items[i].ToString());
                }
            }

            // Remove collected items
            foreach (string item in itemsToRemove)
            {
                checkedListBox.Items.Remove(item);
                _consoleForm.WriteInfo($"Removed computer from {categoryName}: {item}");
            }

            return itemsToRemove.Count;
        }

        #endregion

        #region Computer List Configuration Management Tab Button Event Handlers

        // Linux List button event handler
        private void btnAddLinuxList_Click(object sender, EventArgs e)
        {
            AddComputerToList(txbLinuxList, cbxLinuxList, "Linux");
        }

        // Critical Linux List button event handler
        private void btnAddCriticalLinuxList_Click(object sender, EventArgs e)
        {
            AddComputerToList(txbCriticalLinuxList, cbxCriticalLinuxList, "CriticalLinux");
        }

        // Critical Windows List button event handler
        private void btnAddCriticalWindowsList_Click(object sender, EventArgs e)
        {
            AddComputerToList(txbCriticalWindowsList, cbxCriticalWindowsList, "CriticalWindows");
        }

        // Critical NAS List button event handler
        private void btnAddCriticalNasList_Click(object sender, EventArgs e)
        {
            AddComputerToList(txbCriticalNasList, cbxCriticalNasList, "CriticalNas");
        }

        // Office Exempt List button event handler
        private void btnAddOfficeExemptList_Click(object sender, EventArgs e)
        {
            AddComputerToList(txbOfficeExemptList, cbxOfficeExemptList, "OfficeExempt");
        }

        // Remove selected computers button event handler
        private void btnRemoveSelectedComputers_Click(object sender, EventArgs e)
        {
            try
            {
                _consoleForm.WriteInfo("Removing selected computers from all lists...");

                int totalRemoved = 0;

                // Remove from Linux
                totalRemoved += RemoveSelectedComputersFromCheckedListBox(cbxLinuxList, "Linux");

                // Remove from Critical Linux
                totalRemoved += RemoveSelectedComputersFromCheckedListBox(cbxCriticalLinuxList, "CriticalLinux");

                // Remove from Critical Windows
                totalRemoved += RemoveSelectedComputersFromCheckedListBox(cbxCriticalWindowsList, "CriticalWindows");

                // Remove from Critical NAS
                totalRemoved += RemoveSelectedComputersFromCheckedListBox(cbxCriticalNasList, "CriticalNas");

                // Remove from Office Exempt
                totalRemoved += RemoveSelectedComputersFromCheckedListBox(cbxOfficeExemptList, "OfficeExempt");

                if (totalRemoved > 0)
                {
                    // Save updated configuration to database
                    SaveComputerListToDB();
                    _consoleForm.WriteSuccess($"Removed {totalRemoved} selected computers from configuration.");
                }
                else
                {
                    _consoleForm.WriteInfo("No computers were selected for removal.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error removing selected computers: {ex.Message}");
            }
        }

        private void btnAddExcludeOu_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedOU = ShowOUSelectionDialog("Exclude OU");

                if (!string.IsNullOrEmpty(selectedOU))
                {
                    // Check if OU already exists in the ComboBox
                    bool exists = false;
                    foreach (var item in cbxExcludeOu.Items)
                    {
                        if (item.ToString().Equals(selectedOU, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists)
                    {
                        _consoleForm.WriteWarning($"OU already in exclusion list: {selectedOU}");
                        return;
                    }

                    cbxExcludeOu.Items.Add(selectedOU);
                    cbxExcludeOu.SelectedItem = selectedOU;
                    _consoleForm.WriteSuccess($"Added OU to exclusion list: {selectedOU}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error adding excluded OU: {ex.Message}");
            }
        }

        private void btnDisabledUsersLocation_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedOU = ShowOUSelectionDialog("Disabled Users Location");

                if (!string.IsNullOrEmpty(selectedOU))
                {
                    txbDisabledUsersLocation.Text = selectedOU;
                    _consoleForm.WriteSuccess($"Set Disabled Users location: {selectedOU}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error selecting Disabled Users OU: {ex.Message}");
            }
        }
        #endregion

        #region Important Variables Configuration Management Tab Helpers and functions

        /// <summary>
        /// Save the security group filter keyword to ouConfiguration.csv
        /// </summary>
        private void SaveSecurityGroupKeyword(string keyword)
        {
            try
            {
                _consoleForm?.WriteInfo($"Saving security group filter keyword: {keyword}");

                // Load current OU config, update/add sgfilter entry, save back
                var entries = _databaseService.LoadOUConfiguration();

                bool found = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].MiddleName.Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                    {
                        entries[i] = new OUConfigEntry { OU = "NA", MiddleName = "sgfilter", Keyword = keyword };
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    entries.Add(new OUConfigEntry { OU = "NA", MiddleName = "sgfilter", Keyword = keyword });
                }

                _databaseService.SaveOUConfiguration(entries);
                _consoleForm?.WriteSuccess($"Security group filter keyword saved: {keyword}");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error saving security group keyword: {ex.Message}");
                throw;
            }
        }

        private string GetSecurityGroupKeyword()
        {
            try
            {
                var entries = _databaseService.LoadOUConfiguration();

                foreach (var entry in entries)
                {
                    if (entry.MiddleName.Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                    {
                        _consoleForm?.WriteInfo($"Found security group filter keyword: {entry.Keyword}");
                        return entry.Keyword;
                    }
                }

                _consoleForm?.WriteInfo("No security group filter keyword found in database.");
                return null;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error reading security group keyword from database: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Important Variables Configuration Management Tab Button Event Handlers
        private void btnSubmitVars_Click(object sender, EventArgs e)
        {
            try
            {
                string keyword = txbSecurityGroupKW.Text.Trim();

                if (string.IsNullOrEmpty(keyword))
                {
                    MessageBox.Show("Please enter a keyword first.", "No Keyword",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveSecurityGroupKeyword(keyword);

                MessageBox.Show($"Security group filter keyword saved:\n\n{keyword}",
                    "Keyword Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _consoleForm?.WriteSuccess($"Security group filter keyword saved: {keyword}");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error saving keyword: {ex.Message}");
                MessageBox.Show($"Error saving keyword:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Online/Offline Tab functions

        /// <summary>
        /// Load computers from configuration and populate all Online/Offline controls
        /// </summary>
        private async Task LoadOnlineOfflineTabAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Loading computers for Online/Offline monitoring...");

                // Clear all controls first
                ClearOnlineOfflineControls();

                // Load computers from each configured OU category
                // await LoadComputersToListBoxAsync(cbxListWindowsServersOu, lbxWindowsServers, "Windows Servers");
                await LoadComputersToDataGridViewAsync(cbxListPatriotParkOu, dgvPatriotPark, "Patriot Park");
                await LoadComputersToDataGridViewAsync(cbxListWorkStationOu, dgvWorkstations, "Workstations");
                await LoadComputersToListBoxAsync(cbxListWindowsServersOu, lbxWindowsServers, "Windows Servers");

                // Load computers for specialized categories (these would need their own configuration)
                // Note: These may need additional configuration setup if not already present
                await LoadCriticalSystemsAsync();

                _consoleForm.WriteSuccess("Online/Offline tab loaded successfully");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading Online/Offline tab: {ex.Message}");
            }
        }

        /// <summary>
        /// Load computers from CheckedListBox configuration to target ListBox
        /// </summary>
        private async Task LoadComputersToListBoxAsync(CheckedListBox sourceCheckedListBox, ListBox targetListBox, string categoryName)
        {
            try
            {
                if (sourceCheckedListBox.Items.Count == 0)
                {
                    _consoleForm.WriteInfo($"No OUs configured for {categoryName} category");
                    return;
                }

                _consoleForm.WriteInfo($"Loading computers for {categoryName}...");

                // Get computers from configured OUs
                var computers = await _adService.GetComputersFromCheckedListBoxAsync(sourceCheckedListBox);

                // Clear and populate target ListBox
                targetListBox.Items.Clear();
                foreach (var computer in computers)
                {
                    targetListBox.Items.Add(computer.Name);
                }

                _consoleForm.WriteSuccess($"Loaded {computers.Count} computers for {categoryName}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading computers for {categoryName}: {ex.Message}");
            }
        }


        /// <summary>
        /// Load computers from CheckedListBox configuration to target DataGridView
        /// </summary>
        private async Task LoadComputersToDataGridViewAsync(CheckedListBox sourceCheckedListBox, DataGridView targetDataGridView, string categoryName)
        {
            try
            {
                if (sourceCheckedListBox.Items.Count == 0)
                {
                    _consoleForm.WriteInfo($"No OUs configured for {categoryName} category");
                    return;
                }

                _consoleForm.WriteInfo($"Loading computer details for {categoryName}...");

                // Get computers from configured OUs
                var computers = await _adService.GetComputersFromCheckedListBoxAsync(sourceCheckedListBox);

                // Clear and populate target DataGridView
                targetDataGridView.Rows.Clear();
                foreach (var computer in computers)
                {
                    targetDataGridView.Rows.Add(
                        computer.Name,                    // Computer Name
                        computer.DisplayName ?? "",       // User Name (displayName from AD)
                        computer.Office ?? ""            // Location (physicalDeliveryOfficeName from AD)
                    );
                }

                _consoleForm.WriteSuccess($"Loaded details for {computers.Count} computers in {categoryName}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading computer details for {categoryName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Load critical systems from their respective configurations
        /// Note: This assumes you have separate configurations for critical systems
        /// </summary>
        private async Task LoadCriticalSystemsAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Loading critical systems from configuration...");

                // Load Critical Windows from CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxCriticalWindowsList, lbxCriticalWindows, "Critical Windows");

                // Load Critical NAS from CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxCriticalNasList, lbxCriticalNas, "Critical NAS");

                // Load Critical Linux from CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxCriticalLinuxList, lbxCriticalLinux, "Critical Linux");

                // Load Office Exempt from CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxOfficeExemptList, lbxOfficeExempt, "Office Exempt");

                // Load regular Linux from CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxLinuxList, lbxLinux, "Linux");

                _consoleForm.WriteSuccess("Critical systems configuration loaded.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading critical systems: {ex.Message}");
            }
        }

        private async Task LoadComputersFromCheckedListBoxToListBoxAsync(CheckedListBox sourceCheckedListBox, ListBox targetListBox, string categoryName)
        {
            try
            {
                if (sourceCheckedListBox.Items.Count == 0)
                {
                    _consoleForm.WriteInfo($"No computers configured for {categoryName} category.");
                    return;
                }

                await Task.Run(() =>
                {
                    // Clear and populate target ListBox from CheckedListBox items
                    targetListBox.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        targetListBox.Items.Clear();
                        foreach (string computerName in sourceCheckedListBox.Items)
                        {
                            targetListBox.Items.Add(computerName);
                        }
                    });
                });

                _consoleForm.WriteSuccess($"Loaded {sourceCheckedListBox.Items.Count} computers for {categoryName}.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading computers for {categoryName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all Online/Offline tab controls
        /// </summary>
        private void ClearOnlineOfflineControls()
        {
            // Clear ListBoxes
            lbxWindowsServers.Items.Clear();
            lbxCriticalWindows.Items.Clear();
            lbxCriticalNas.Items.Clear();
            lbxOfficeExempt.Items.Clear();
            lbxLinux.Items.Clear();
            lbxCriticalLinux.Items.Clear();

            // Clear DataGridViews
            dgvWorkstations.Rows.Clear();
            dgvPatriotPark.Rows.Clear();
        }

        /// <summary>
        /// Check online/offline status for all computers in the tab
        /// </summary>
        private async Task CheckAllOnlineOfflineStatusAsync()
        {
            try
            {
                _consoleForm.WriteInfo("Checking online/offline status for all systems...");

                var allComputers = new List<string>();

                // Collect all computer names from ListBoxes
                allComputers.AddRange(lbxWindowsServers.Items.Cast<string>());
                allComputers.AddRange(lbxCriticalWindows.Items.Cast<string>());
                allComputers.AddRange(lbxCriticalNas.Items.Cast<string>());
                allComputers.AddRange(lbxOfficeExempt.Items.Cast<string>());
                allComputers.AddRange(lbxLinux.Items.Cast<string>());
                allComputers.AddRange(lbxCriticalLinux.Items.Cast<string>());

                // Collect computer names from DataGridViews
                foreach (DataGridViewRow row in dgvWorkstations.Rows)
                {
                    if (row.Cells[0].Value != null)
                        allComputers.Add(row.Cells[0].Value.ToString());
                }

                foreach (DataGridViewRow row in dgvPatriotPark.Rows)
                {
                    if (row.Cells[0].Value != null)
                        allComputers.Add(row.Cells[0].Value.ToString());
                }

                // Remove duplicates
                allComputers = allComputers.Distinct().ToList();

                _consoleForm.WriteInfo($"Checking connectivity for {allComputers.Count} unique systems...");

                // Check each computer's online status
                var onlineResults = await CheckComputersOnlineStatusAsync(allComputers);

                // Update UI with results
                UpdateOnlineOfflineStatus(onlineResults);

                int onlineCount = onlineResults.Count(r => r.Value);
                int offlineCount = onlineResults.Count(r => !r.Value);

                _consoleForm.WriteSuccess($"Status check complete: {onlineCount} online, {offlineCount} offline");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error checking online/offline status: {ex.Message}");
            }
        }

        /// <summary>
        /// Check online status for a list of computers using ping
        /// </summary>
        private async Task<Dictionary<string, bool>> CheckComputersOnlineStatusAsync(List<string> computerNames)
        {
            var results = new Dictionary<string, bool>();
            var tasks = new List<Task>();

            // Create semaphore to limit concurrent pings
            using (var semaphore = new SemaphoreSlim(10, 10)) // Limit to 10 concurrent pings
            {
                foreach (string computerName in computerNames)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            bool isOnline = await PingComputerAsync(computerName);
                            lock (results)
                            {
                                results[computerName] = isOnline;
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }

            return results;
        }


        /// <summary>
        /// Ping a single computer to check if it's online
        /// </summary>
        private async Task<bool> PingComputerAsync(string computerName)
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = await ping.SendPingAsync(computerName, 3000); // 3 second timeout
                    return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Update UI controls with online/offline status results
        /// </summary>
        private void UpdateOnlineOfflineStatus(Dictionary<string, bool> onlineResults)
        {
            // Update ListBoxes with color coding
            UpdateListBoxStatus(lbxWindowsServers, onlineResults);
            UpdateListBoxStatus(lbxCriticalWindows, onlineResults);
            UpdateListBoxStatus(lbxCriticalNas, onlineResults);
            UpdateListBoxStatus(lbxOfficeExempt, onlineResults);
            UpdateListBoxStatus(lbxLinux, onlineResults);
            UpdateListBoxStatus(lbxCriticalLinux, onlineResults);

            // Update DataGridViews with color coding
            UpdateDataGridViewStatus(dgvWorkstations, onlineResults);
            UpdateDataGridViewStatus(dgvPatriotPark, onlineResults);
        }

        /// <summary>
        /// Update ListBox with online/offline color coding
        /// </summary>
        private void UpdateListBoxStatus(ListBox listBox, Dictionary<string, bool> onlineResults)
        {
            listBox.DrawMode = DrawMode.OwnerDrawFixed;
            listBox.DrawItem -= ListBox_DrawItem; // Remove existing handler if any
            listBox.DrawItem += (sender, e) =>
            {
                if (e.Index >= 0)
                {
                    string computerName = listBox.Items[e.Index].ToString();
                    bool isOnline = onlineResults.ContainsKey(computerName) && onlineResults[computerName];

                    // Set colors
                    Color backColor = isOnline ? Color.LightGreen : Color.LightCoral;
                    Color textColor = isOnline ? Color.DarkGreen : Color.DarkRed;

                    // Draw background
                    e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);

                    // Draw text
                    e.Graphics.DrawString(computerName, listBox.Font, new SolidBrush(textColor), e.Bounds);
                }
            };

            listBox.Invalidate(); // Force redraw
        }

        /// <summary>
        /// Update DataGridView with online/offline color coding
        /// </summary>
        private void UpdateDataGridViewStatus(DataGridView dataGridView, Dictionary<string, bool> onlineResults)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    string computerName = row.Cells[0].Value.ToString();
                    bool isOnline = onlineResults.ContainsKey(computerName) && onlineResults[computerName];

                    // Set row colors based on online status
                    Color backColor = isOnline ? Color.LightGreen : Color.LightCoral;
                    Color textColor = isOnline ? Color.DarkGreen : Color.DarkRed;

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = backColor;
                        cell.Style.ForeColor = textColor;
                    }
                }
            }
        }

        #endregion

        #region Online/Offline Tab Button Event Handlers

        /// <summary>
        /// Event handler for ReCheck Online/Offline Status button
        /// </summary>
        private async void btnOnOffline_Click(object sender, EventArgs e)
        {
            try
            {
                btnOnOffline.Enabled = false;
                btnOnOffline.Text = "Checking Status...";

                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Please log in first before checking online/offline status.");
                    return;
                }

                // Reload computers and re-check all statuses
                await LoadOnlineOfflineTabAsync();
                await CheckAllOnlineOfflineStatusAsync();
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during online/offline status check: {ex.Message}");
            }
            finally
            {
                btnOnOffline.Enabled = true;
                btnOnOffline.Text = "ReCheck Online/Offline Status";
            }
        }

        /// <summary>
        /// Generic event handler for ListBox drawing (used by UpdateListBoxStatus)
        /// </summary>
        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            // This method signature is required for the DrawItem event
            // The actual implementation is handled in UpdateListBoxStatus method
        }

        #endregion
    }
}




