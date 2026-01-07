using Microsoft.Win32;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell;
using System.Linq.Expressions;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Threading.Tasks;
using System.IO;
using System.DirectoryServices.AccountManagement;
using static SA_ToolBelt.Linux_Service;
using static SA_ToolBelt.AddGroupsForm;
using LdapSearchScope = System.DirectoryServices.Protocols.SearchScope;
using System.Text;
using System.Reflection.PortableExecutable;
using static SA_ToolBelt.AD_Service;
using System.Net.NetworkInformation;
using System.Threading;
using System.Numerics;
using System.Security.Principal;

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

        // Startup Shutdown Variables
        public string VMMode = "NormalRun";
        public bool startUpShutdown = false;
        private Dictionary<string, bool> pingStatus = new Dictionary<string, bool>();

        // Add this initialization in your form constructor or after successful login
        private readonly string _vCenterServer = "your_vcenter_server"; // Configure this

        // Computer List CSV file path
        private readonly string COMPUTER_LIST_FILE_PATH = @"C:\SA_ToolBelt\Config\ComputerList.csv";

        // OU Configuration CSV file path
        private readonly string OU_CONFIG_FILE_PATH = @"C:\SA_ToolBelt\Config\ouConfiguration.csv";

        // Store the logged in SA's username globally
        public string _loggedInUsername = string.Empty;


        public SAToolBelt()
        {
            InitializeComponent();

            _consoleForm = new ConsoleForm();
            _adService = new AD_Service(_consoleForm);
            _linuxService = new Linux_Service(_consoleForm);
            _rhdsService = new RHDS_Service(_consoleForm);

            this.KeyPreview = true;



            // Hide all tabs except Login initially
            HideAllTabsExceptLogin();
            SetupRadioButtonExclusivity();

            // Load OU configuration from CSV file  
            LoadOUConfigurationFromCSV();

            // Load Computer List from CSV file
            LoadComputerListFromCSV();

            // Load important Variables
            LoadImportantVariablesFromCSV();

            tabConsole.Controls.Add(_consoleForm.GetConsoleRichTextBox());
            _consoleForm.GetConsoleRichTextBox().Dock = DockStyle.Fill;
            _consoleForm.GetConsoleRichTextBox().ContextMenuStrip = _consoleContextMenu;
            _consoleForm.Hide();

            // Setup for Docking and UnDocking of the Console Window
            InitializeConsoleDocking();

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
                tabConsole.Controls.Remove(tabConsole);
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

        private void ShowAllTabs()
        {
            // Add all tabs EXCEPT Login after successful authentication
            tabControlMain.TabPages.Clear();
            tabControlMain.TabPages.Add(tabAD);
            tabControlMain.TabPages.Add(tabLDAP);
            tabControlMain.TabPages.Add(tabRemoteTools);
            tabControlMain.TabPages.Add(tabWindowsTools);
            tabControlMain.TabPages.Add(tabLinuxTools);
            tabControlMain.TabPages.Add(tabVMwareTools);
            tabControlMain.TabPages.Add(tabOnlineOffline);
            tabControlMain.TabPages.Add(tabSAPMIsSpice);
            tabControlMain.TabPages.Add(tabStartupShutdownPt1);
            tabControlMain.TabPages.Add(tabStartupShutdownPt2);
            tabControlMain.TabPages.Add(tabConfiguration);
            tabControlMain.TabPages.Add(tabConsole);
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
        /// <summary>
        /// Populate the Default Security Groups combobox from RHDS (with filtering)
        /// </summary>
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
                    // Authentication successful
                    ShowAllTabs();

                    // Update radio button counters INSIDE try-catch
                    await UpdateRadioButtonCounters();

                    // NEW: Populate LDAP security groups dropdown
                    await PopulateDefaultSecurityGroupsAsync();

                    // Online/Offline status check here
                    await LoadOnlineOfflineTabAsync();
                    await CheckAllOnlineOfflineStatusAsync();

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
                    MessageBox.Show(welcomeMessage, "Login Successful",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            try
            {
                // Get the selected security group from cbxDefaultSecurityGroups
                string selectedSecurityGroup = null;
                foreach (int checkedIndex in cbxDefaultSecurityGroups.CheckedIndices)
                {
                    selectedSecurityGroup = cbxDefaultSecurityGroups.Items[checkedIndex].ToString();
                    break; // Only get first checked item
                }

                if (string.IsNullOrEmpty(selectedSecurityGroup))
                {
                    MessageBox.Show("Please select a security group first.", "No Selection",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _consoleForm?.WriteInfo($"Retrieving notes for security group: {selectedSecurityGroup}");

                // Get the Notes field from the group
                string notes = _adService.GetGroupNotes(selectedSecurityGroup);

                if (!string.IsNullOrEmpty(notes))
                {
                    MessageBox.Show($"Notes for '{selectedSecurityGroup}':\n\n{notes}",
                        "Security Group Notes",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    _consoleForm?.WriteSuccess($"Retrieved notes for group: {selectedSecurityGroup}");
                }
                else
                {
                    MessageBox.Show($"No notes found for security group: {selectedSecurityGroup}",
                        "No Notes",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    _consoleForm?.WriteInfo($"No notes found for group: {selectedSecurityGroup}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error retrieving group notes: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    // If unlock account is checked, unlock it too
                    if (cbxUnlockAcnt.Checked)
                    {
                        bool unlockSuccess = _adService.UnlockUserAccount(username);
                        if (unlockSuccess)
                        {
                            MessageBox.Show($"Password changed and account unlocked successfully for '{username}'.", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _consoleForm.WriteSuccess($"Password changed and account unlocked successfully for '{username}'.");
                        }
                        else
                        {
                            MessageBox.Show($"Password changed successfully for '{username}', but failed to unlock account.", "Warning.",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            _consoleForm.WriteWarning($"Password changed successfully for '{username}', but failed to unlock account.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Password changed successfully for '{username}'.", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _consoleForm.WriteSuccess($"Password changed successfully for '{username}'.");
                    }

                    // Clear the password fields after successful change
                    txbNewPassword.Clear();
                    txbConfirmNewPassword.Clear();
                    cbxUnlockAcnt.Checked = false;

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

                // Perform the disable operation
                using (var context = new PrincipalContext(ContextType.Domain))
                using (var user = UserPrincipal.FindByIdentity(context, username))
                {
                    if (user == null)
                    {
                        _consoleForm?.WriteError($"User not found: {username}");
                        MessageBox.Show($"User not found: {username}", "User Not Found",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Check if user is already disabled
                    if (user.Enabled.HasValue && !user.Enabled.Value)
                    {
                        _consoleForm?.WriteWarning($"User {username} is already disabled.");
                        MessageBox.Show($"User {username} is already disabled.", "Already Disabled",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Disable the account
                    user.Enabled = false;

                    // Update the description
                    user.Description = disableDescription;

                    // Save the changes
                    user.Save();

                    _consoleForm?.WriteSuccess($"Account disabled and description updated for: {username}");

                    // Move user to Disabled Users OU
                    var userEntry = user.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;
                    if (userEntry != null)
                    {
                        try
                        {
                            string targetOU = "OU=Disabled Users,OU=People,OU=CDC,OU=spectre,DC=spectre,DC=afspc,DC=af,DC=smil,DC=mil";

                            // Create DirectoryEntry for target OU
                            using (var targetOUEntry = new System.DirectoryServices.DirectoryEntry($"LDAP://{targetOU}"))
                            {
                                // Move the user
                                userEntry.MoveTo(targetOUEntry);
                                userEntry.CommitChanges();

                                _consoleForm?.WriteSuccess($"User {username} moved to Disabled Users OU successfully.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _consoleForm?.WriteError($"Account disabled but failed to move to Disabled Users OU: {ex.Message}");
                            MessageBox.Show($"Account was disabled successfully, but failed to move to Disabled Users OU:\n\n{ex.Message}",
                                          "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    // NEW: Remove user from ALL Directory Services (RHDS) security groups
                    try
                    {
                        _consoleForm?.WriteInfo($"Removing {username} from all Directory Services security groups...");

                        // Get configured Security Groups OU from CSV
                        string securityGroupsOU = GetSecurityGroupsOU();

                        if (string.IsNullOrEmpty(securityGroupsOU))
                        {
                            _consoleForm?.WriteWarning("No Security Groups OU configured. Skipping Directory Services group removal.");
                            _consoleForm?.WriteInfo("Configure Security Groups OU in the Configuration tab to enable this feature.");
                        }
                        else
                        {
                            // Remove user from all DS groups
                            int groupsRemoved = _rhdsService.RemoveUserFromAllDSGroups(username, securityGroupsOU);

                            if (groupsRemoved > 0)
                            {
                                _consoleForm?.WriteSuccess($"Removed {username} from {groupsRemoved} Directory Services security groups.");
                            }
                            else
                            {
                                _consoleForm?.WriteInfo($"User {username} was not a member of any Directory Services security groups.");
                            }
                        }
                    }
                    catch (Exception dsEx)
                    {
                        _consoleForm?.WriteError($"Error removing user from Directory Services groups: {dsEx.Message}");
                        _consoleForm?.WriteWarning("User account was disabled in AD and moved to Disabled Users OU, but DS group removal failed.");
                        _consoleForm?.WriteWarning("Manual cleanup of Directory Services groups may be required.");

                        // Don't fail the whole operation - AD disable was successful
                        MessageBox.Show(
                            $"Account was successfully disabled in AD, but there was an error removing the user from Directory Services groups:\n\n{dsEx.Message}\n\nManual cleanup may be required.",
                            "Partial Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }

                // After disabling the account, remove from all groups and add to group "pending_removal"
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

        /// <summary>
        /// Create LDAP account and attempt to add to AD group
        /// </summary>
        /*
        private async Task CreateLdapAccountAsync()
        {
            string userToFind = txbLdapNtUserId.Text.Trim();
            int attempts = 0;
            const int maxAttempts = 3;
            bool userFound = false;

            try
            {
                _consoleForm.WriteInfo($"Starting LDAP account creation for user: {userToFind}");

                // First, create user in LDAP
                try
                {
                    var ldapService = new LDAPService();

                    // Create LDAP user
                    ldapService.CreateNewUser(
                        ntUserId: txbLdapNtUserId.Text.Trim(),
                        email: txbLdapEmail.Text.Trim(),
                        firstName: txbLdapFirstName.Text.Trim(),
                        lastName: txbLdapLastName.Text.Trim(),
                        phone: txbLdapPhone.Text.Trim(),
                        tempPassword: txbLdapTempPass.Text,
                        linuxUid: txbLdapLinuxUid.Text.Trim()
                    );

                    // Create user directory using plink
                    ldapService.CreateUserDirectory(txbLdapNtUserId.Text.Trim());

                    _consoleForm.WriteSuccess("LDAP user created successfully. Now attempting to add to AD...");
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Error creating LDAP user: {ex.Message}");
                    return; // Exit if LDAP creation fails
                }

                // Then proceed with AD operations
                while (attempts < maxAttempts && !userFound)
                {
                    attempts++;
                    _consoleForm.WriteInfo($"Attempting to find user in AD (attempt {attempts}/{maxAttempts})...");

                    var userInfo = _adService.GetUserInfo(userToFind);

                    if (userInfo != null)
                    {
                        userFound = true;
                        _consoleForm.WriteSuccess($"Found user in AD: {userInfo.GetFullName()} ({userInfo.SamAccountName})");
                        /*
                        try
                        {
                            // Add user to Shared_Group using the DTO's SamAccountName
                            bool result = _adService.AddUserToGroup(userInfo.SamAccountName, "Shared_Group");

                            if (result)
                            {
                                _consoleForm.WriteSuccess($"User {userInfo.GetFullName()} successfully added to Shared_Group");
                            }
                            else
                            {
                                _consoleForm.WriteError($"Failed to add user {userInfo.GetFullName()} to Shared_Group");
                            }
                        }
                        catch (Exception ex)
                        {
                            _consoleForm.WriteError($"Error adding user to AD group: {ex.Message}");
                        }
                        */ /*
                        break;
                    }


                    if (attempts < maxAttempts)
                    {
                        _consoleForm.WriteInfo($"User not found in AD. Waiting 6 seconds before next attempt...");
                        await Task.Delay(6000); // Wait 6 seconds before next attempt
                    }
                    else
                    {
                        _consoleForm.WriteError($"User {userToFind} not found in AD after {maxAttempts} attempts");
                    }
                }

                if (userFound)
                {
                    _consoleForm.WriteSuccess("LDAP account creation and AD integration completed successfully.");
                }
                else
                {
                    _consoleForm.WriteWarning("LDAP account created, but user was not found in AD for group assignment.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during LDAP account creation process: {ex.Message}");
            }
        }
        */
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

                    // Prompt user for Linux SSH credentials
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
                            _consoleForm?.WriteSuccess($"Home directory created successfully for {ntUserId}");
                        }
                        else
                        {
                            _consoleForm?.WriteError($"Failed to create home directory for {ntUserId}");
                        }
                    }
                    else
                    {
                        _consoleForm?.WriteWarning("Linux SSH credentials not provided. Home directory creation skipped.");
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
                    successMessage += "⚠ Home directory creation was skipped or failed\n";
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
        private async Task CheckServerReplicationHealth(string hostname, string username, string password, string serverLabel)
        {
            try
            {
                _consoleForm.WriteInfo($"Checking replication health on {hostname} ({serverLabel})...");

                // Commands to check LDAP replication health
                // These are typical Red Hat Directory Service replication commands
                // Note: Using the hostname from the credential dialog instead of "localhost"
                // dsconf commands require bind DN and password for LDAP authentication
                string[] healthCommands = {
            // Check replication status (dsctl doesn't need bind credentials)
            $"dsctl {hostname} status",

            // Check replication agreements (requires Directory Manager credentials)
            $"dsconf -D 'cn=Directory Manager' -w '{password}' ldap://{hostname}:389 replication get-ruv --suffix dc=spectre,dc=afspc,dc=af,dc=smil,dc=mil",

            // Check replication lag (requires Directory Manager credentials)
            $"dsconf -D 'cn=Directory Manager' -w '{password}' ldap://{hostname}:389 replication monitor",

            // Check last update times (requires Directory Manager credentials)
            $"dsconf -D 'cn=Directory Manager' -w '{password}' ldap://{hostname}:389 replication status --suffix dc=spectre,dc=afspc,dc=af,dc=smil,dc=mil"
        };

                var results = await _linuxService.ExecuteMultipleSSHCommandsAsync(hostname, username, password, healthCommands);

                // Parse and display results
                ParseAndDisplayReplicationResults(results, serverLabel);
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Failed to check replication health on {hostname}: {ex.Message}");

                // Update UI to show error status
                UpdateServerStatus(serverLabel, "ERROR", "Connection Failed", "N/A");
            }
        }

        private void ParseAndDisplayReplicationResults(Dictionary<string, string> results, string serverLabel)
        {
            try
            {
                // This is where you'll parse the command outputs and update the UI labels
                // The exact parsing will depend on the actual command outputs

                string status = "Unknown";
                string lastUpdate = "Unknown";
                string updateEnd = "Unknown";

                // Parse results - you'll need to adjust this based on actual command outputs
                foreach (var result in results)
                {
                    string command = result.Key;
                    string output = result.Value;

                    if (command.Contains("status"))
                    {
                        // Parse status information
                        if (output.Contains("running") || output.Contains("active"))
                        {
                            status = "Running";
                        }
                        else if (output.Contains("stopped") || output.Contains("inactive"))
                        {
                            status = "Stopped";
                        }
                    }
                    else if (command.Contains("monitor") || command.Contains("ruv"))
                    {
                        // Parse replication timing information
                        // This will need to be customized based on actual output format
                        // Look for timestamps, lag information, etc.
                    }
                }

                // Update the UI with parsed information
                UpdateServerStatus(serverLabel, status, lastUpdate, updateEnd);

                _consoleForm.WriteInfo($"Replication status for {serverLabel}: {status}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error parsing replication results for {serverLabel}: {ex.Message}");
            }
        }

        private void UpdateServerStatus(string serverLabel, string status, string startTime, string endTime)
        {
            // Update the appropriate labels based on server (SA1 or SA2)
            if (serverLabel == "SA1")
            {
                lblLastUpdatedStatusSa1.Text = status;
                lblUpdateStartTimeSa1.Text = startTime;
                lblUpdateEndedTimeSa1.Text = endTime;
                lblTargetCcesa1.Text = "ccesa1";

                // Set color based on status
                if (status.ToLower().Contains("running") || status.ToLower().Contains("active"))
                {
                    lblLastUpdatedStatusSa1.ForeColor = System.Drawing.Color.Green;
                }
                else if (status.ToLower().Contains("error") || status.ToLower().Contains("stopped"))
                {
                    lblLastUpdatedStatusSa1.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    lblLastUpdatedStatusSa1.ForeColor = System.Drawing.Color.Orange;
                }
            }
            else if (serverLabel == "SA2")
            {
                lblUpdateStatusSa2.Text = status;
                lblUpdateStartTimeSa2.Text = startTime;
                lblUpdateEndTimeSa2.Text = endTime;
                lblTargetCcesa2.Text = "ccesa2";

                // Set color based on status
                if (status.ToLower().Contains("running") || status.ToLower().Contains("active"))
                {
                    lblUpdateStatusSa2.ForeColor = System.Drawing.Color.Green;
                }
                else if (status.ToLower().Contains("error") || status.ToLower().Contains("stopped"))
                {
                    lblUpdateStatusSa2.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    lblUpdateStatusSa2.ForeColor = System.Drawing.Color.Orange;
                }
            }
        }

        private void ClearReplicationResults()
        {
            // Clear SA1 labels
            lblLastUpdatedStatusSa1.Text = "Checking...";
            lblUpdateStartTimeSa1.Text = "Checking...";
            lblUpdateEndedTimeSa1.Text = "Checking...";
            lblLastUpdatedStatusSa1.ForeColor = System.Drawing.Color.Black;

            // Clear SA2 labels
            lblUpdateStatusSa2.Text = "Checking...";
            lblUpdateStartTimeSa2.Text = "Checking...";
            lblUpdateEndTimeSa2.Text = "Checking...";
            lblUpdateStatusSa2.ForeColor = System.Drawing.Color.Black;
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
                    row.Cells["ESXiServerNamePmiClm"].Value = host.Hostname;
                    row.Cells["ESXiStatePmiClm"].Value = host.ConnectionState;
                    row.Cells["ESXiStatusPmiClm"].Value = host.PowerStatus;
                    row.Cells["ESXiClusterPmiClm"].Value = host.Cluster;
                    row.Cells["ESXiConsumedCPUPmiClm"].Value = $"{host.ConsumedCPU:F1}%";
                    row.Cells["ESXiConsumedMemoryPmiClm"].Value = $"{host.ConsumedMemory:F1}%";
                    row.Cells["ESXiHAStatePmiClm"].Value = host.HAState;
                    row.Cells["ESXiUptimePmiClm"].Value = $"{host.Uptime:F1} days";

                    // Apply color coding for connection status
                    if (host.ConnectionState.Equals("Connected", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["ESXiStatePmiClm"].Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        row.Cells["ESXiStatePmiClm"].Style.ForeColor = Color.Red;
                    }

                    // Apply color coding for CPU usage
                    if (host.ConsumedCPU > 80)
                    {
                        row.Cells["ESXiConsumedCPUPmiClm"].Style.ForeColor = Color.Red;
                        row.Cells["ESXiConsumedCPUPmiClm"].Style.BackColor = Color.LightPink;
                    }
                    else if (host.ConsumedCPU > 60)
                    {
                        row.Cells["ESXiConsumedCPUPmiClm"].Style.ForeColor = Color.Orange;
                    }

                    // Apply color coding for Memory usage
                    if (host.ConsumedMemory > 80)
                    {
                        row.Cells["ESXiConsumedMemoryPmiClm"].Style.ForeColor = Color.Red;
                        row.Cells["ESXiConsumedMemoryPmiClm"].Style.BackColor = Color.LightPink;
                    }
                    else if (host.ConsumedMemory > 60)
                    {
                        row.Cells["ESXiConsumedMemoryPmiClm"].Style.ForeColor = Color.Orange;
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
                    row.Cells["ESXiVMNamePmiClm"].Value = vm.Name;
                    row.Cells["ESXiVMStatePmiClm"].Value = vm.PowerState;
                    row.Cells["ESXiVMStatusPmiClm"].Value = vm.Status;
                    row.Cells["ESXiProvSpacePmiClm"].Value = $"{vm.ProvisionedSpace:F2} GB";
                    row.Cells["ESXiUsedSpacePmiClm"].Value = $"{vm.UsedSpace:F2} GB";
                    row.Cells["ESXiVMHostCpuPmiClm"].Value = $"{vm.HostCPU:F0} MHz";
                    row.Cells["ESXiVMHostMemPmiClm"].Value = $"{vm.HostMemory:F2} GB";

                    // Apply color coding for power state
                    if (vm.PowerState.Equals("PoweredOn", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["ESXiVMStatePmiClm"].Style.ForeColor = Color.Green;
                    }
                    else if (vm.PowerState.Equals("PoweredOff", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["ESXiVMStatePmiClm"].Style.ForeColor = Color.Red;
                    }
                    else
                    {
                        row.Cells["ESXiVMStatePmiClm"].Style.ForeColor = Color.Orange;
                    }

                    // Apply color coding for VM status
                    if (vm.Status.Equals("green", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["ESXiVMStatusPmiClm"].Style.ForeColor = Color.Green;
                    }
                    else if (vm.Status.Equals("red", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells["ESXiVMStatusPmiClm"].Style.ForeColor = Color.Red;
                    }
                    else
                    {
                        row.Cells["ESXiVMStatusPmiClm"].Style.ForeColor = Color.Orange;
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
        // Add this to your SA_ToolBelt.cs file in the SPICE event handlers region

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

                foreach (string host in hosts)
                {
                    try
                    {
                        _consoleForm.WriteInfo($"Processing host: {host}");

                        // Find the corresponding DataGridView for this host
                        DataGridView currentDgv = this.Controls.Find($"{host}Dgv", true).FirstOrDefault() as DataGridView;

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

                    _vmwareManager = new VMwareManager(_vCenterServer, vCenterUser, vCenterPass, _consoleForm);
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

                _consoleForm.WriteInfo($"Credentials provided for server: {hostname}");

                // Clear previous results
                ClearReplicationResults();

                // Test connection first
                _consoleForm.WriteInfo($"Testing SSH connection to {hostname}...");

                bool serverConnected = await _linuxService.TestSSHConnectionAsync(hostname, username, password);

                if (!serverConnected)
                {
                    _consoleForm.WriteError($"Failed to connect to {hostname}. Please check credentials and network connectivity.");
                    return;
                }

                _consoleForm.WriteSuccess($"Successfully connected to {hostname}");

                // Check replication health on the server
                await CheckServerReplicationHealth(hostname, username, password, hostname);

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
        #endregion

        #region CSV Functionality

        // Parse a CSV line handling quotes and commas
        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            result.Add(currentField); // Add the last field
            return result.ToArray();
        }

        // Escape CSV field (add quotes if needed)
        private string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Add quotes if field contains comma, quote, or newline
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        // Load Computer List configuration from CSV and populate CheckedListBoxes
        private void LoadComputerListFromCSV()
        {
            try
            {
                _consoleForm.WriteInfo("Loading Computer List configuration from CSV file...");

                // Clear all Computer List CheckedListBoxes first
                ClearComputerListCheckedListBoxes();

                // Check if CSV file exists
                if (!File.Exists(COMPUTER_LIST_FILE_PATH))
                {
                    _consoleForm.WriteWarning($"Computer List file not found at: {COMPUTER_LIST_FILE_PATH}");
                    CreateComputerListCSV();
                    return;
                }

                // Read CSV file
                string[] csvLines = File.ReadAllLines(COMPUTER_LIST_FILE_PATH);

                if (csvLines.Length <= 1) // Header only or empty
                {
                    _consoleForm.WriteWarning("Computer List file is empty or contains only headers.");
                    return;
                }

                // Parse CSV data (skip header row)
                int loadedCount = 0;
                for (int i = 1; i < csvLines.Length; i++)
                {
                    string line = csvLines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] columns = ParseCSVLine(line);
                    if (columns.Length >= 2) // At minimum need ComputerName and Type
                    {
                        string computerName = columns[0].Trim().Trim('"');
                        string type = columns[1].Trim().Trim('"');

                        // Add to appropriate CheckedListBox based on type
                        AddComputerToCheckedListBox(computerName, type);
                        loadedCount++;
                    }
                }

                _consoleForm.WriteSuccess($"Loaded {loadedCount} computer entries from CSV file.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading Computer List from CSV: {ex.Message}");
            }
        }

        // Create Computer List CSV file with headers
        private void CreateComputerListCSV()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(COMPUTER_LIST_FILE_PATH);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create CSV with headers
                string csvContent = "ComputerName,Type,VMWare,Instructions\n";
                File.WriteAllText(COMPUTER_LIST_FILE_PATH, csvContent);

                _consoleForm.WriteSuccess($"Created Computer List file: {COMPUTER_LIST_FILE_PATH}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error creating Computer List CSV: {ex.Message}");
            }
        }


        // Save Computer List configuration to CSV
        private void SaveComputerListToCSV()
        {
            try
            {
                _consoleForm.WriteInfo("Saving Computer List to CSV file...");

                var csvContent = new StringBuilder();
                csvContent.AppendLine("ComputerName,Type,VMWare,Instructions");

                // Collect all computers from CheckedListBoxes
                AddComputersFromCheckedListBox(csvContent, cbxLinuxList, "Linux");
                AddComputersFromCheckedListBox(csvContent, cbxCriticalLinuxList, "CriticalLinux");
                AddComputersFromCheckedListBox(csvContent, cbxCriticalWindowsList, "CriticalWindows");
                AddComputersFromCheckedListBox(csvContent, cbxCriticalNasList, "CriticalNas");
                AddComputersFromCheckedListBox(csvContent, cbxOfficeExemptList, "OfficeExempt");

                // Write to file
                File.WriteAllText(COMPUTER_LIST_FILE_PATH, csvContent.ToString());

                _consoleForm.WriteSuccess($"Computer List saved to: {COMPUTER_LIST_FILE_PATH}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error saving Computer List to CSV: {ex.Message}");
            }
        }
        /// <summary>
        /// Get the configured Security Groups OU from ouConfiguration.csv
        /// </summary>
        private string GetSecurityGroupsOU()
        {
            try
            {
                if (!File.Exists(OU_CONFIG_FILE_PATH))
                {
                    _consoleForm?.WriteWarning($"OU configuration file not found: {OU_CONFIG_FILE_PATH}");
                    return null;
                }

                string[] lines = File.ReadAllLines(OU_CONFIG_FILE_PATH);

                // Skip header, look for SecurityGroups middleName
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] columns = ParseCSVLine(line);
                    if (columns.Length >= 2)
                    {
                        string ou = columns[0].Trim().Trim('"');
                        string middleName = columns[1].Trim().Trim('"');

                        if (middleName.Equals("SecurityGroups", StringComparison.OrdinalIgnoreCase))
                        {
                            _consoleForm?.WriteInfo($"Found Security Groups OU: {ou}");
                            return ou;
                        }
                    }
                }

                _consoleForm?.WriteWarning("No Security Groups OU found in configuration file.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error reading Security Groups OU from config: {ex.Message}");
            }

            return null;
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

        // Load OU configuration from CSV and populate CheckedListBoxes
        private void LoadOUConfigurationFromCSV()
        {
            try
            {
                _consoleForm.WriteInfo("Loading OU configuration from CSV file...");

                // Clear all OU CheckedListBoxes first
                ClearOUCheckedListBoxes();

                // Check if CSV file exists
                if (!File.Exists(OU_CONFIG_FILE_PATH))
                {
                    _consoleForm.WriteWarning($"OU configuration file not found at: {OU_CONFIG_FILE_PATH}");
                    CreateOUConfigurationCSV();
                    return;
                }

                // Read CSV file
                string[] csvLines = File.ReadAllLines(OU_CONFIG_FILE_PATH);

                if (csvLines.Length <= 1) // Header only or empty
                {
                    _consoleForm.WriteWarning("OU configuration file is empty or contains only headers.");
                    return;
                }

                // Parse CSV data (skip header row)
                int loadedCount = 0;
                for (int i = 1; i < csvLines.Length; i++)
                {
                    string line = csvLines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] columns = ParseCSVLine(line);
                    if (columns.Length >= 2)
                    {
                        string ou = columns[0].Trim().Trim('"');
                        string middleName = columns[1].Trim().Trim('"');
                        string keyWord = columns.Length >= 3 ? columns[2].Trim().Trim('"') : "";

                        if (middleName.Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                        {
                            txbSecurityGroupKW.Text = keyWord;
                            _consoleForm.WriteInfo($"Loaded security group filter keyword: {keyWord}");
                        }
                        else
                        {
                            // Add to appropriate CheckedListBox based on middleName
                            AddOUToCheckedListBox(ou, middleName);
                            loadedCount++;
                        }
                    }
                }

                _consoleForm.WriteSuccess($"Loaded {loadedCount} OU configuration entries from CSV file.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading OU configuration from CSV: {ex.Message}");
            }
        }
        private void LoadImportantVariablesFromCSV()
        {
            try
            {
                _consoleForm.WriteInfo("Loading Important Variables from CSV file...");

                // Clear all Keyword text box's first
                ClearImportantTextBoxes();

                // Check if CSV file exists
                if (!File.Exists(OU_CONFIG_FILE_PATH))
                {
                    _consoleForm.WriteWarning($"Configuration file not found at: {OU_CONFIG_FILE_PATH}");
                    CreateOUConfigurationCSV();
                    return;
                }

                // Read CSV file
                string[] csvLines = File.ReadAllLines(OU_CONFIG_FILE_PATH);

                if (csvLines.Length <= 1) // Header only or empty
                {
                    _consoleForm.WriteWarning("OU configuration file is empty or contains only headers.");
                    return;
                }

                // Parse CSV data (skip header row)
                int loadedCount = 0;
                for (int i = 1; i < csvLines.Length; i++)
                {
                    string line = csvLines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] columns = ParseCSVLine(line);
                    if (columns.Length >= 2)
                    {
                        string middleName = columns[1].Trim().Trim('"');
                        string kw = columns[2].Trim().Trim('"');

                        // Add to appropriate CheckedListBox based on middleName
                       if (middleName == "sgfilter")
                        {
                            txbSecurityGroupKW.Text = kw;
                        }
                        loadedCount++;
                    }
                }

                _consoleForm.WriteSuccess($"Loaded {loadedCount} OU configuration entries from CSV file.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error loading OU configuration from CSV: {ex.Message}");
            }
        }
        private void ClearImportantTextBoxes()
        {
            txbSecurityGroupKW.Text = "";
        }
        // Create OU configuration CSV file with headers
        private void CreateOUConfigurationCSV()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(OU_CONFIG_FILE_PATH);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Create CSV with headers
                string csvContent = "ou,middleName,keyWord\n";
                File.WriteAllText(OU_CONFIG_FILE_PATH, csvContent);

                _consoleForm.WriteSuccess($"Created OU configuration file: {OU_CONFIG_FILE_PATH}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error creating OU configuration CSV: {ex.Message}");
            }
        }

        // Clear all OU CheckedListBoxes
        private void ClearOUCheckedListBoxes()
        {
            cbxListWorkStationOu.Items.Clear();
            cbxListPatriotParkOu.Items.Clear();
            cbxListWindowsServersOu.Items.Clear();
            cbxListGangsOu.Items.Clear();
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
                case "windows":
                    if (!cbxListWindowsServersOu.Items.Contains(ou))
                        cbxListWindowsServersOu.Items.Add(ou, false);
                    break;
                case "gangs":
                    if (!cbxListGangsOu.Items.Contains(ou))
                        cbxListGangsOu.Items.Add(ou, false);
                    break;
                case "securitygroups": // ADD THIS CASE
                    cbxListSecurityGroupsOu.Items.Add(ou);
                    break;
                default:
                    _consoleForm.WriteWarning($"Unknown middleName '{middleName}' for OU: {ou}");
                    break;
            }
        }


        // Save OU configuration to CSV
        private void SaveOUConfigurationToCSV()
        {
            try
            {
                _consoleForm.WriteInfo("Saving OU configuration to CSV file...");

                var csvContent = new StringBuilder();
                csvContent.AppendLine("ou,middleName,keyWord"); // ADD keyWord COLUMN

                // Collect all OUs from CheckedListBoxes using dynamic extraction
                AddOUsFromCheckedListBox(csvContent, cbxListWorkStationOu);
                AddOUsFromCheckedListBox(csvContent, cbxListPatriotParkOu);
                AddOUsFromCheckedListBox(csvContent, cbxListWindowsServersOu);
                AddOUsFromCheckedListBox(csvContent, cbxListGangsOu);
                AddOUsFromCheckedListBox(csvContent, cbxListSecurityGroupsOu); // ADD THIS LINE

                // Write to file
                File.WriteAllText(OU_CONFIG_FILE_PATH, csvContent.ToString());

                _consoleForm.WriteSuccess($"OU configuration saved to: {OU_CONFIG_FILE_PATH}");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error saving OU configuration to CSV: {ex.Message}");
            }
        }

        // Helper method to add OUs from a CheckedListBox to CSV content
        private void AddOUsFromCheckedListBox(StringBuilder csvContent, CheckedListBox checkedListBox)
        {
            // Extract middleName dynamically from control name
            string middleName = GetMiddleNameFromControlName(checkedListBox.Name);

            foreach (string ou in checkedListBox.Items)
            {
                // Escape CSV field if needed
                string escapedOU = EscapeCSVField(ou);
                csvContent.AppendLine($"{escapedOU},{middleName},"); // ADD EMPTY keyWord COLUMN
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

                    // Save to CSV
                    SaveOUConfigurationToCSV();

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

                    // Save to CSV
                    SaveOUConfigurationToCSV();

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

        private void btnAddGangsOu_Click(object sender, EventArgs e)
        {
            AddOUToConfiguration(cbxListGangsOu);
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
                totalRemoved += RemoveSelectedOUsFromCheckedListBox(cbxListGangsOu);
                totalRemoved += RemoveSelectedOUsFromCheckedListBox(cbxListSecurityGroupsOu); // ADD THIS LINE

                if (totalRemoved > 0)
                {
                    // Save updated configuration to CSV
                    SaveOUConfigurationToCSV();
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

        // Helper method to add computers from a CheckedListBox to CSV content
        private void AddComputersFromCheckedListBox(StringBuilder csvContent, CheckedListBox checkedListBox, string type)
        {
            foreach (string computerName in checkedListBox.Items)
            {
                // Escape CSV fields if needed
                string escapedComputerName = EscapeCSVField(computerName);
                string escapedType = EscapeCSVField(type);
                string vmware = EscapeCSVField("N/A"); // Default value
                string instructions = EscapeCSVField("Added via Configuration"); // Default value

                csvContent.AppendLine($"{escapedComputerName},{escapedType},{vmware},{instructions}");
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

                // Save to CSV
                SaveComputerListToCSV();

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
                    // Save updated configuration to CSV
                    SaveComputerListToCSV();
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

                if (!File.Exists(OU_CONFIG_FILE_PATH))
                {
                    _consoleForm?.WriteError("OU configuration file not found. Creating new file...");
                    CreateOUConfigurationCSV();
                }

                var lines = File.ReadAllLines(OU_CONFIG_FILE_PATH).ToList();
                bool keywordLineExists = false;

                // Look for existing sgfilter line and update it
                for (int i = 1; i < lines.Count; i++)
                {
                    string[] columns = ParseCSVLine(lines[i]);
                    if (columns.Length >= 2 && columns[1].Trim().Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                    {
                        // Update existing line
                        lines[i] = $"NA,sgfilter,{EscapeCSVField(keyword)}";
                        keywordLineExists = true;
                        _consoleForm?.WriteInfo("Updated existing security group filter keyword");
                        break;
                    }
                }

                // If no sgfilter line exists, add it
                if (!keywordLineExists)
                {
                    lines.Add($"NA,sgfilter,{EscapeCSVField(keyword)}");
                    _consoleForm?.WriteInfo("Added new security group filter keyword");
                }

                // Write back to file
                File.WriteAllLines(OU_CONFIG_FILE_PATH, lines);
                _consoleForm?.WriteSuccess($"Security group filter keyword saved: {keyword}");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error saving security group keyword: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the security group filter keyword from ouConfiguration.csv
        /// </summary>
        private string GetSecurityGroupKeyword()
        {
            try
            {
                if (!File.Exists(OU_CONFIG_FILE_PATH))
                {
                    _consoleForm?.WriteWarning($"OU configuration file not found: {OU_CONFIG_FILE_PATH}");
                    return null;
                }

                string[] lines = File.ReadAllLines(OU_CONFIG_FILE_PATH);

                // Skip header, look for sgfilter middleName
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] columns = ParseCSVLine(line);
                    if (columns.Length >= 3)
                    {
                        string middleName = columns[1].Trim().Trim('"');

                        if (middleName.Equals("sgfilter", StringComparison.OrdinalIgnoreCase))
                        {
                            string keyword = columns[2].Trim().Trim('"');
                            _consoleForm?.WriteInfo($"Found security group filter keyword: {keyword}");
                            return keyword;
                        }
                    }
                }

                _consoleForm?.WriteInfo("No security group filter keyword found in configuration.");
                return null;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error reading security group keyword from config: {ex.Message}");
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
                await LoadComputersToListBoxAsync(cbxListGangsOu, lbxGangs, "Gangs");

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
                _consoleForm.WriteInfo("Loading critical systems from CSV configuration...");

                // Load Critical Windows from CSV-based CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxCriticalWindowsList, lbxCriticalWindows, "Critical Windows");

                // Load Critical NAS from CSV-based CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxCriticalNasList, lbxCriticalNas, "Critical NAS");

                // Load Critical Linux from CSV-based CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxCriticalLinuxList, lbxCriticalLinux, "Critical Linux");

                // Load Office Exempt from CSV-based CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxOfficeExemptList, lbxOfficeExempt, "Office Exempt");

                // Load regular Linux from CSV-based CheckedListBox
                await LoadComputersFromCheckedListBoxToListBoxAsync(cbxLinuxList, lbxLinux, "Linux");

                _consoleForm.WriteSuccess("Critical systems configuration loaded from CSV");
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
                    _consoleForm.WriteInfo($"No computers configured for {categoryName} category in CSV");
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

                _consoleForm.WriteSuccess($"Loaded {sourceCheckedListBox.Items.Count} computers for {categoryName} from CSV configuration");
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
            lbxGangs.Items.Clear();
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
                allComputers.AddRange(lbxGangs.Items.Cast<string>());
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
            UpdateListBoxStatus(lbxGangs, onlineResults);
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

            /*
            try
            {
                // Disable button during operation
                btnOnOffline.Enabled = false;
                btnOnOffline.Text = "Checking Status...";

                // Check authentication
                if (!CredentialManager.IsAuthenticated)
                {
                    _consoleForm.WriteError("Please log in first before checking online/offline status.");
                    return;
                }

                // Load computers from configuration if not already loaded
                if (lbxWindows.Items.Count == 0 && dgvWorkstations.Rows.Count == 0)
                {
                    _consoleForm.WriteInfo("Loading computers from configuration...");
                    await LoadOnlineOfflineTabAsync();
                }

                // Check online/offline status
                await CheckAllOnlineOfflineStatusAsync();
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error during online/offline status check: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                btnOnOffline.Enabled = true;
                btnOnOffline.Text = "ReCheck Online/Offline Status";
            }
            */
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
