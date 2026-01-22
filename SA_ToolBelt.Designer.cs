namespace SA_ToolBelt
{
    partial class SAToolBelt
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SAToolBelt));
            tabControlMain = new TabControl();
            tabLogin = new TabPage();
            panelLogin = new Panel();
            btnShowPassword = new Button();
            btnLogin = new Button();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            tabAD = new TabPage();
            btnLoadSelectedUser = new Button();
            cbxShowConsole = new CheckBox();
            lblDisableAccount = new Label();
            gbxDisableAccount = new GroupBox();
            txbProcessedBy = new TextBox();
            lblProcessedBy = new Label();
            txbDisabledReason = new TextBox();
            lblDisabledReason = new Label();
            lblDateDisabled = new Label();
            btnDisable = new Button();
            dtpDisabledDate = new DateTimePicker();
            lblDeleteAccount = new Label();
            gbxDeleteAccount = new GroupBox();
            btnDeleteAccount = new Button();
            lblUnlockAccount = new Label();
            gbxUnlockAccount = new GroupBox();
            btnUnlockAccount = new Button();
            btnAdClear = new Button();
            gbxSingleUserSearch = new GroupBox();
            txbUserName = new TextBox();
            txbFirstName = new TextBox();
            txbLastName = new TextBox();
            lblUsersName = new Label();
            lblLastName = new Label();
            lblFirstName = new Label();
            rbnSingleUserSearch = new RadioButton();
            tabControlADResults = new TabControl();
            tabResults = new TabPage();
            dgvUnifiedResults = new DataGridView();
            colFullName = new DataGridViewTextBoxColumn();
            colUserName = new DataGridViewTextBoxColumn();
            colFirstName = new DataGridViewTextBoxColumn();
            colLastName = new DataGridViewTextBoxColumn();
            colLogonName = new DataGridViewTextBoxColumn();
            colExpDate = new DataGridViewTextBoxColumn();
            colDaysLeft = new DataGridViewTextBoxColumn();
            colExpirationDate = new DataGridViewTextBoxColumn();
            colDaysExpired = new DataGridViewTextBoxColumn();
            colDaysDisabled = new DataGridViewTextBoxColumn();
            colHomeDirExists = new DataGridViewTextBoxColumn();
            colLockDate = new DataGridViewTextBoxColumn();
            colUnlock = new DataGridViewTextBoxColumn();
            tabGeneral = new TabPage();
            lblUIDNumberValue = new Label();
            lblUIDNumber = new Label();
            lblGIDNumberValue = new Label();
            lblGIDNumber = new Label();
            lblOUValue = new Label();
            lblOU = new Label();
            lblLockedValue = new Label();
            lblLocked = new Label();
            lblHomeDriveValue = new Label();
            lblHomeDrive = new Label();
            lblLastLoginValue = new Label();
            lblLastLogin = new Label();
            lblLastPasswordChangeValue = new Label();
            lblLastPasswordChange = new Label();
            lblAccountExpirationValue = new Label();
            lblAccountExpiration = new Label();
            lblTelephoneNumberValue = new Label();
            lblTelephoneNumber = new Label();
            lblDescriptionValue = new Label();
            lblDescription = new Label();
            lblEmailValue = new Label();
            lblEmail = new Label();
            lblLoginNameValue = new Label();
            lblLoginName = new Label();
            lblGenFirstName = new Label();
            lblFirstNameValue = new Label();
            lblGenLastName = new Label();
            lblLastNameValue = new Label();
            tabMemberOf = new TabPage();
            btnEditUsersGroups = new Button();
            lblLoadedUser = new Label();
            clbMemberOf = new CheckedListBox();
            btnAdLoadAccounts = new Button();
            gbxLockedAccounts = new GroupBox();
            rbLockedAccountsOut = new RadioButton();
            gbxDisabledAccounts = new GroupBox();
            rbDisabledAccounts0to30 = new RadioButton();
            rbDisabledAccounts31to60 = new RadioButton();
            rbDisabledAccounts90Plus = new RadioButton();
            rbDisabledAccounts61to90 = new RadioButton();
            gbxExpiredAccounts = new GroupBox();
            rbExpiredAccounts0to30 = new RadioButton();
            rbExpiredAccounts31to60 = new RadioButton();
            rbExpiredAccounts61to90 = new RadioButton();
            rbExpiredAccounts90Plus = new RadioButton();
            gbxExpiringAccounts = new GroupBox();
            rbExpiringAccounts0to30 = new RadioButton();
            rbExpiringAccounts31to60 = new RadioButton();
            rbExpiringAccounts61to90 = new RadioButton();
            lblChangePassword = new Label();
            gbxChangePassword = new GroupBox();
            cbxUnlockAcnt = new CheckBox();
            btnClearPasswords = new Button();
            btnSubmit = new Button();
            btnPwChngShowPassword = new Button();
            txbConfirmNewPassword = new TextBox();
            lblConfirmNewPassword = new Label();
            txbNewPassword = new TextBox();
            lblPwdRequirements = new Label();
            lblNewPassword = new Label();
            lblOneSpecial = new Label();
            lblOneNumber = new Label();
            lblOneLowercase = new Label();
            lblOneUppercase = new Label();
            lblFourteenChrs = new Label();
            lblTestPassword = new Label();
            gbxTestPassword = new GroupBox();
            btnTestPassword = new Button();
            txbTestPassword = new TextBox();
            btnShowTestPassword = new Button();
            lblActExpDate = new Label();
            gbxAcntExpDate = new GroupBox();
            btnAcntExeDateUpdate = new Button();
            pkrAcntExpDateTimePicker = new DateTimePicker();
            tabLDAP = new TabPage();
            gbxUserAccountCreation = new GroupBox();
            rbnSecurityGroupOther = new RadioButton();
            txbSecurityGroupOther = new TextBox();
            txbSecurityGroupId = new TextBox();
            lblDefaultSecurityGroupId = new Label();
            cbxDefaultSecurityGroups = new CheckedListBox();
            lblDefaultSecurityGroup = new Label();
            lblLdapTempPass = new Label();
            lblLdapFirstName = new Label();
            txbLdapTempPass = new TextBox();
            txbLdapPhone = new TextBox();
            btnClearAccountCreationForm = new Button();
            btnLdapGetUid = new Button();
            lblLdapLinuxUid = new Label();
            txbLdapLinuxUid = new TextBox();
            btnLdapCreateAccount = new Button();
            txbLdapEmail = new TextBox();
            txbLdapLastName = new TextBox();
            txbLdapFirstName = new TextBox();
            lblLdapLastName = new Label();
            lblLdapPhone = new Label();
            lblLdapEmail = new Label();
            btnLdapGenerate = new Button();
            lblLdapNtUserId = new Label();
            txbLdapNtUserId = new TextBox();
            lblNoteLowerCase = new Label();
            lblNewUserAcntCreation = new Label();
            tabRemoteTools = new TabPage();
            tabWindowsTools = new TabPage();
            tabLinuxTools = new TabPage();
            tabVMwareTools = new TabPage();
            tabOnlineOffline = new TabPage();
            lbxCriticalLinux = new ListBox();
            lbxLinux = new ListBox();
            lblCriticalLinux = new Label();
            lblLinux = new Label();
            btnOnOffline = new Button();
            lbxOfficeExempt = new ListBox();
            lbxCriticalWindows = new ListBox();
            lbxCriticalNas = new ListBox();
            lbxWindowsServers = new ListBox();
            lblCriticalNAS = new Label();
            lblOfficeExempt = new Label();
            lblCriticalWindows = new Label();
            lblWorkstations = new Label();
            dgvWorkstations = new DataGridView();
            clmWksComputerName = new DataGridViewTextBoxColumn();
            clmWksUserName = new DataGridViewTextBoxColumn();
            clmWksLocation = new DataGridViewTextBoxColumn();
            lblPatriotPark = new Label();
            dgvPatriotPark = new DataGridView();
            dgvPpComputerName = new DataGridViewTextBoxColumn();
            clmPpUserName = new DataGridViewTextBoxColumn();
            clmPpLocation = new DataGridViewTextBoxColumn();
            lblWindowsServers = new Label();
            tabSAPMIsSpice = new TabPage();
            btnPerformHealthChk = new Button();
            btnCheckFileSystem = new Button();
            gbxLDAPReplicationChk = new GroupBox();
            lblMaxCSNSa2 = new Label();
            lblMaxCSNDataSa2 = new Label();
            lblReplicaIDSa2 = new Label();
            lblReplicationStatusDataSa2 = new Label();
            lblReplicaIDDataSa2 = new Label();
            lblReplicaRootSa2 = new Label();
            lblReplicaStatusSa2 = new Label();
            lblReplicaStatusDataSa2 = new Label();
            lblReplicaRootDataSa2 = new Label();
            lblMaxCSNSa1 = new Label();
            lblMaxCSNDataSa1 = new Label();
            lblReplicaIDSa1 = new Label();
            lblReplicaIDDataSa1 = new Label();
            lblReplicaRootSa1 = new Label();
            lblReplicaStatusSa1 = new Label();
            lblReplicaStatusDataSa1 = new Label();
            lblReplicaRootDataSa1 = new Label();
            lblLastInitEndDataSa1 = new Label();
            lblLastInitStatusDataSa1 = new Label();
            lblLastInitStartDataSa1 = new Label();
            lblReapActiveDataSa1 = new Label();
            lblReplicationLagTimeDataSa1 = new Label();
            lblTargetCcesa1 = new Label();
            lblReplicaEnabledSa1 = new Label();
            lblReplicationStatusDataSa1 = new Label();
            lblLastInitStartSa1 = new Label();
            lblLastInitEndSa1 = new Label();
            lblChangesSkippedDataSa1 = new Label();
            lblLastUpdateStatusSa1 = new Label();
            lblLastInitStatusSa1 = new Label();
            lblReplicaEnabledDataSa1 = new Label();
            lblStatusForAgreementSa1 = new Label();
            lblUpdateInProgressSa1 = new Label();
            lblUpdateInProgressDataSa1 = new Label();
            lblStatusForAgreementDataSa1 = new Label();
            lblChangesSentDataSa1 = new Label();
            lblLastUpdateStartSa1 = new Label();
            txbLastUpdateStatusDataSa1 = new Label();
            lblChangesSkippedSa1 = new Label();
            lblLastUpdateEndDataSa1 = new Label();
            lblLastUpdateEndSa1 = new Label();
            lblReplicationLagTimeSa1 = new Label();
            lblReapActiveSa1 = new Label();
            lblLastUpdateStartDataSa1 = new Label();
            lblChangesSentSa1 = new Label();
            lblReplicationStatusSa1 = new Label();
            lblLastInitEndSa2 = new Label();
            lblLastInitEndDataSa2 = new Label();
            lblLastInitStatusSa2 = new Label();
            lblLastInitStatusDataSa2 = new Label();
            lblLastInitStartSa2 = new Label();
            lblLastInitStartDataSa2 = new Label();
            lblReapActiveSa2 = new Label();
            lblReapActiveDataSa2 = new Label();
            lblReplicationLagTimeSa2 = new Label();
            lblReplicationLagTimeDataSa2 = new Label();
            lblLastUpdateStatusSa2 = new Label();
            lblChangesSkippedSa2 = new Label();
            lblChangesSkippedDataSa2 = new Label();
            lblReplicaEnabledSa2 = new Label();
            lblReplicaEnabledDataSa2 = new Label();
            lblUpdateInProgressSa2 = new Label();
            lblUpdateInProgressDataSa2 = new Label();
            lblReplicationStatusSa2 = new Label();
            txbLastUpdateStatusDataSa2 = new Label();
            lblStatusForAgreementDataSa2 = new Label();
            lblStatusForAgreementSa2 = new Label();
            lblLastUpdateStartSa2 = new Label();
            lblLastUpdateStartDataSa2 = new Label();
            lblLastUpdateEndSa2 = new Label();
            lblLastUpdateEndDataSa2 = new Label();
            lblChangesSentSa2 = new Label();
            lblChangesSentDataSa2 = new Label();
            btnCheckRepHealth = new Button();
            lblTargetCceSa2 = new Label();
            dgvEsxiVmHealthChk = new TabControl();
            tabEsxiHealthPmi = new TabPage();
            dgvEsxiHealthCheck = new DataGridView();
            clmServerName = new DataGridViewTextBoxColumn();
            clmState = new DataGridViewTextBoxColumn();
            clmStatus = new DataGridViewTextBoxColumn();
            clmCluster = new DataGridViewTextBoxColumn();
            clmConsumedCpu = new DataGridViewTextBoxColumn();
            clmConsumedMemory = new DataGridViewTextBoxColumn();
            clmHaState = new DataGridViewTextBoxColumn();
            clmUptime = new DataGridViewTextBoxColumn();
            tabVmHealthChkPmi = new TabPage();
            dgvVmHealthCheck = new DataGridView();
            clmVmName = new DataGridViewTextBoxColumn();
            clmPowerState = new DataGridViewTextBoxColumn();
            clmVmStatus = new DataGridViewTextBoxColumn();
            clmProvisionedSpace = new DataGridViewTextBoxColumn();
            clmUsedSpace = new DataGridViewTextBoxColumn();
            clmHostCpu = new DataGridViewTextBoxColumn();
            clmHostMemory = new DataGridViewTextBoxColumn();
            TcFileSystemCheck = new TabControl();
            tabCcelpro = new TabPage();
            dgvCcelpro1 = new DataGridView();
            clmFileSystemLpro1 = new DataGridViewTextBoxColumn();
            clmSizeLpro1 = new DataGridViewTextBoxColumn();
            clmUsedLpro1 = new DataGridViewTextBoxColumn();
            clmAvailableLpro1 = new DataGridViewTextBoxColumn();
            clmUsedPercentLpro1 = new DataGridViewTextBoxColumn();
            clmMountedOnLpro1 = new DataGridViewTextBoxColumn();
            tabccesec1 = new TabPage();
            dgvCcesec1 = new DataGridView();
            clmFileSystemSec1 = new DataGridViewTextBoxColumn();
            clmSizeSec1 = new DataGridViewTextBoxColumn();
            clmUsedSec1 = new DataGridViewTextBoxColumn();
            clmAvailableSec1 = new DataGridViewTextBoxColumn();
            clmUsedPercentSec1 = new DataGridViewTextBoxColumn();
            clmMountedOnSec1 = new DataGridViewTextBoxColumn();
            tabCcegitsvr1 = new TabPage();
            dgvCcegitsvr1 = new DataGridView();
            clmFileSystemSvr1 = new DataGridViewTextBoxColumn();
            clmSizeSvr1 = new DataGridViewTextBoxColumn();
            clmUsedSvr1 = new DataGridViewTextBoxColumn();
            clmAvailableSvr1 = new DataGridViewTextBoxColumn();
            clmUsedPercentSvr1 = new DataGridViewTextBoxColumn();
            clmMountedOnSvr1 = new DataGridViewTextBoxColumn();
            tabccesa1 = new TabPage();
            dgvCcesa1 = new DataGridView();
            clmFileSystemSa1 = new DataGridViewTextBoxColumn();
            clmSizeSa1 = new DataGridViewTextBoxColumn();
            clmUsedSa1 = new DataGridViewTextBoxColumn();
            clmAvailableSa1 = new DataGridViewTextBoxColumn();
            clmUsedPercentSa1 = new DataGridViewTextBoxColumn();
            clmMountedOnSa1 = new DataGridViewTextBoxColumn();
            tabCcesa2 = new TabPage();
            dgvCcesa2 = new DataGridView();
            clmFileSystemSa2 = new DataGridViewTextBoxColumn();
            clmSizeSa2 = new DataGridViewTextBoxColumn();
            clmUsedSa2 = new DataGridViewTextBoxColumn();
            clmAvailableSa2 = new DataGridViewTextBoxColumn();
            clmUsedPercentSa2 = new DataGridViewTextBoxColumn();
            clmMountedOnSa2 = new DataGridViewTextBoxColumn();
            lblEsxiAndVmPmi = new Label();
            lblFileSystemCheckPmi = new Label();
            tabStartupShutdownPt1 = new TabPage();
            tabStartupShutdownPt2 = new TabPage();
            tabConfiguration = new TabPage();
            lblPowerCLIPathLocation = new Label();
            lblPowerCLIPathLabel = new Label();
            lblFilePathLocation = new Label();
            lblConfigFileLocationLabel = new Label();
            gbxImportantVariables = new GroupBox();
            btnSubmitVars = new Button();
            txbSecurityGroupKW = new TextBox();
            lblSecurityGroupKW = new Label();
            gbxComputerList = new GroupBox();
            lblLinuxSelectionList = new Label();
            cbxIsVm = new CheckBox();
            btnRemoveSelectedComputers = new Button();
            lblOfficeExemptList = new Label();
            btnAddOfficeExemptList = new Button();
            lblCriticalLinuxList = new Label();
            txbOfficeExemptList = new TextBox();
            lblCriticalWindowsList = new Label();
            cbxOfficeExemptList = new CheckedListBox();
            txbLinuxList = new TextBox();
            cbxCriticalNasList = new CheckedListBox();
            btnAddLinuxList = new Button();
            txbCriticalLinuxList = new TextBox();
            btnAddCriticalNasList = new Button();
            cbxCriticalLinuxList = new CheckedListBox();
            btnAddCriticalLinuxList = new Button();
            btnAddCriticalWindowsList = new Button();
            cbxLinuxList = new CheckedListBox();
            lblCriticalNasList = new Label();
            txbCriticalNasList = new TextBox();
            cbxCriticalWindowsList = new CheckedListBox();
            txbCriticalWindowsList = new TextBox();
            gbxImportantOUs = new GroupBox();
            btnAddSecurityGroupsOU = new Button();
            cbxListSecurityGroupsOu = new CheckedListBox();
            lblSecurityGroups = new Label();
            btnAddWindowsServersOu = new Button();
            btnAddPatriotParkOu = new Button();
            btnAddWorkstationOu = new Button();
            cbxListWorkStationOu = new CheckedListBox();
            btnRemoveSelectedOus = new Button();
            lblWorkstationOu = new Label();
            cbxListWindowsServersOu = new CheckedListBox();
            lblPatriotParkOu = new Label();
            lblWindowsServersOu = new Label();
            cbxListPatriotParkOu = new CheckedListBox();
            tabConsole = new TabPage();
            btnUndockConsole = new Button();
            btnLogout = new Button();
            dtpLogStartDate = new DateTimePicker();
            dtpLogEndDate = new DateTimePicker();
            lblLogStartDate = new Label();
            lblLogEndDate = new Label();
            chkLastHourOnly = new CheckBox();
            cmbLogPriority = new ComboBox();
            lblPrioritySeverityLevel = new Label();
            lblKeywordSearch = new Label();
            txbLogKeyword = new TextBox();
            cbxCaseSensitive = new CheckBox();
            label1 = new Label();
            cbxLogSourceSelection = new ComboBox();
            cbxServerSelection = new ComboBox();
            lblServerSelection = new Label();
            lblLogStatus = new Label();
            lblLogStatusResults = new Label();
            rtbLogOutput = new RichTextBox();
            btnFetchLogs = new Button();
            btnClearLogs = new Button();
            btnExportLogs = new Button();
            gbxServerInstances = new GroupBox();
            lblLdapServerInstance1 = new Label();
            txbLdapServerInstace1 = new TextBox();
            txbLdapServerInstace2 = new TextBox();
            lblLdapServerInstance2 = new Label();
            btnSubmitServerInstance = new Button();
            tabControlMain.SuspendLayout();
            tabLogin.SuspendLayout();
            panelLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            tabAD.SuspendLayout();
            gbxDisableAccount.SuspendLayout();
            gbxDeleteAccount.SuspendLayout();
            gbxUnlockAccount.SuspendLayout();
            gbxSingleUserSearch.SuspendLayout();
            tabControlADResults.SuspendLayout();
            tabResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUnifiedResults).BeginInit();
            tabGeneral.SuspendLayout();
            tabMemberOf.SuspendLayout();
            gbxLockedAccounts.SuspendLayout();
            gbxDisabledAccounts.SuspendLayout();
            gbxExpiredAccounts.SuspendLayout();
            gbxExpiringAccounts.SuspendLayout();
            gbxChangePassword.SuspendLayout();
            gbxTestPassword.SuspendLayout();
            gbxAcntExpDate.SuspendLayout();
            tabLDAP.SuspendLayout();
            gbxUserAccountCreation.SuspendLayout();
            tabLinuxTools.SuspendLayout();
            tabOnlineOffline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWorkstations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPatriotPark).BeginInit();
            tabSAPMIsSpice.SuspendLayout();
            gbxLDAPReplicationChk.SuspendLayout();
            dgvEsxiVmHealthChk.SuspendLayout();
            tabEsxiHealthPmi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEsxiHealthCheck).BeginInit();
            tabVmHealthChkPmi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVmHealthCheck).BeginInit();
            TcFileSystemCheck.SuspendLayout();
            tabCcelpro.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCcelpro1).BeginInit();
            tabccesec1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCcesec1).BeginInit();
            tabCcegitsvr1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCcegitsvr1).BeginInit();
            tabccesa1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCcesa1).BeginInit();
            tabCcesa2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCcesa2).BeginInit();
            tabConfiguration.SuspendLayout();
            gbxImportantVariables.SuspendLayout();
            gbxComputerList.SuspendLayout();
            gbxImportantOUs.SuspendLayout();
            gbxServerInstances.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabLogin);
            tabControlMain.Controls.Add(tabAD);
            tabControlMain.Controls.Add(tabLDAP);
            tabControlMain.Controls.Add(tabRemoteTools);
            tabControlMain.Controls.Add(tabWindowsTools);
            tabControlMain.Controls.Add(tabLinuxTools);
            tabControlMain.Controls.Add(tabVMwareTools);
            tabControlMain.Controls.Add(tabOnlineOffline);
            tabControlMain.Controls.Add(tabSAPMIsSpice);
            tabControlMain.Controls.Add(tabStartupShutdownPt1);
            tabControlMain.Controls.Add(tabStartupShutdownPt2);
            tabControlMain.Controls.Add(tabConfiguration);
            tabControlMain.Controls.Add(tabConsole);
            tabControlMain.Dock = DockStyle.Top;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(1659, 875);
            tabControlMain.TabIndex = 0;
            // 
            // tabLogin
            // 
            tabLogin.BackColor = Color.White;
            tabLogin.Controls.Add(panelLogin);
            tabLogin.Controls.Add(pictureBox1);
            tabLogin.Controls.Add(pictureBox2);
            tabLogin.Location = new Point(4, 24);
            tabLogin.Name = "tabLogin";
            tabLogin.Padding = new Padding(3);
            tabLogin.Size = new Size(1651, 847);
            tabLogin.TabIndex = 0;
            tabLogin.Text = "Login";
            // 
            // panelLogin
            // 
            panelLogin.BackColor = SystemColors.Control;
            panelLogin.Controls.Add(btnShowPassword);
            panelLogin.Controls.Add(btnLogin);
            panelLogin.Controls.Add(txtPassword);
            panelLogin.Controls.Add(lblPassword);
            panelLogin.Controls.Add(txtUsername);
            panelLogin.Controls.Add(lblUsername);
            panelLogin.Location = new Point(589, 318);
            panelLogin.Name = "panelLogin";
            panelLogin.Size = new Size(300, 200);
            panelLogin.TabIndex = 0;
            // 
            // btnShowPassword
            // 
            btnShowPassword.Location = new Point(173, 145);
            btnShowPassword.Name = "btnShowPassword";
            btnShowPassword.Size = new Size(107, 30);
            btnShowPassword.TabIndex = 5;
            btnShowPassword.Text = "Show Password";
            btnShowPassword.UseVisualStyleBackColor = true;
            btnShowPassword.MouseDown += ShowPassword_MouseDown;
            btnShowPassword.MouseLeave += HidePassword_MouseLeave;
            btnShowPassword.MouseUp += HidePassword_MouseUp;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(20, 145);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(80, 30);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(20, 105);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(260, 23);
            txtPassword.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(20, 85);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(20, 50);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(260, 23);
            txtUsername.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(20, 30);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(63, 15);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username:";
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.Location = new Point(404, 6);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(738, 59);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackgroundImage = (Image)resources.GetObject("pictureBox2.BackgroundImage");
            pictureBox2.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox2.Location = new Point(1071, 494);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(390, 287);
            pictureBox2.TabIndex = 2;
            pictureBox2.TabStop = false;
            // 
            // tabAD
            // 
            tabAD.Controls.Add(btnLoadSelectedUser);
            tabAD.Controls.Add(cbxShowConsole);
            tabAD.Controls.Add(lblDisableAccount);
            tabAD.Controls.Add(gbxDisableAccount);
            tabAD.Controls.Add(lblDeleteAccount);
            tabAD.Controls.Add(gbxDeleteAccount);
            tabAD.Controls.Add(lblUnlockAccount);
            tabAD.Controls.Add(gbxUnlockAccount);
            tabAD.Controls.Add(btnAdClear);
            tabAD.Controls.Add(gbxSingleUserSearch);
            tabAD.Controls.Add(tabControlADResults);
            tabAD.Controls.Add(btnAdLoadAccounts);
            tabAD.Controls.Add(gbxLockedAccounts);
            tabAD.Controls.Add(gbxDisabledAccounts);
            tabAD.Controls.Add(gbxExpiredAccounts);
            tabAD.Controls.Add(gbxExpiringAccounts);
            tabAD.Controls.Add(lblChangePassword);
            tabAD.Controls.Add(gbxChangePassword);
            tabAD.Controls.Add(lblTestPassword);
            tabAD.Controls.Add(gbxTestPassword);
            tabAD.Controls.Add(lblActExpDate);
            tabAD.Controls.Add(gbxAcntExpDate);
            tabAD.Location = new Point(4, 24);
            tabAD.Name = "tabAD";
            tabAD.Padding = new Padding(3);
            tabAD.Size = new Size(1651, 847);
            tabAD.TabIndex = 1;
            tabAD.Text = "AD";
            tabAD.UseVisualStyleBackColor = true;
            // 
            // btnLoadSelectedUser
            // 
            btnLoadSelectedUser.Location = new Point(527, 718);
            btnLoadSelectedUser.Name = "btnLoadSelectedUser";
            btnLoadSelectedUser.Size = new Size(132, 41);
            btnLoadSelectedUser.TabIndex = 93;
            btnLoadSelectedUser.Text = "Load Selected User";
            btnLoadSelectedUser.UseVisualStyleBackColor = true;
            btnLoadSelectedUser.Click += btnLoadSelectedUser_Click;
            // 
            // cbxShowConsole
            // 
            cbxShowConsole.AutoSize = true;
            cbxShowConsole.Location = new Point(1557, 822);
            cbxShowConsole.Name = "cbxShowConsole";
            cbxShowConsole.Size = new Size(101, 19);
            cbxShowConsole.TabIndex = 92;
            cbxShowConsole.Text = "Show Console";
            cbxShowConsole.UseVisualStyleBackColor = true;
            // 
            // lblDisableAccount
            // 
            lblDisableAccount.AutoSize = true;
            lblDisableAccount.Font = new Font("Segoe UI", 14F);
            lblDisableAccount.Location = new Point(1221, 512);
            lblDisableAccount.Margin = new Padding(4, 0, 4, 0);
            lblDisableAccount.Name = "lblDisableAccount";
            lblDisableAccount.Size = new Size(148, 25);
            lblDisableAccount.TabIndex = 90;
            lblDisableAccount.Text = "Disable Account";
            // 
            // gbxDisableAccount
            // 
            gbxDisableAccount.Controls.Add(txbProcessedBy);
            gbxDisableAccount.Controls.Add(lblProcessedBy);
            gbxDisableAccount.Controls.Add(txbDisabledReason);
            gbxDisableAccount.Controls.Add(lblDisabledReason);
            gbxDisableAccount.Controls.Add(lblDateDisabled);
            gbxDisableAccount.Controls.Add(btnDisable);
            gbxDisableAccount.Controls.Add(dtpDisabledDate);
            gbxDisableAccount.Enabled = false;
            gbxDisableAccount.Location = new Point(1123, 540);
            gbxDisableAccount.Margin = new Padding(4, 3, 4, 3);
            gbxDisableAccount.Name = "gbxDisableAccount";
            gbxDisableAccount.Padding = new Padding(4, 3, 4, 3);
            gbxDisableAccount.Size = new Size(341, 167);
            gbxDisableAccount.TabIndex = 91;
            gbxDisableAccount.TabStop = false;
            // 
            // txbProcessedBy
            // 
            txbProcessedBy.Location = new Point(115, 87);
            txbProcessedBy.Name = "txbProcessedBy";
            txbProcessedBy.Size = new Size(216, 23);
            txbProcessedBy.TabIndex = 22;
            // 
            // lblProcessedBy
            // 
            lblProcessedBy.AutoSize = true;
            lblProcessedBy.Location = new Point(30, 87);
            lblProcessedBy.Name = "lblProcessedBy";
            lblProcessedBy.Size = new Size(79, 15);
            lblProcessedBy.TabIndex = 21;
            lblProcessedBy.Text = "Processed By:";
            // 
            // txbDisabledReason
            // 
            txbDisabledReason.Location = new Point(115, 58);
            txbDisabledReason.Name = "txbDisabledReason";
            txbDisabledReason.Size = new Size(216, 23);
            txbDisabledReason.TabIndex = 20;
            // 
            // lblDisabledReason
            // 
            lblDisabledReason.AutoSize = true;
            lblDisabledReason.Location = new Point(13, 61);
            lblDisabledReason.Name = "lblDisabledReason";
            lblDisabledReason.Size = new Size(96, 15);
            lblDisabledReason.TabIndex = 19;
            lblDisabledReason.Text = "Disabled Reason:";
            // 
            // lblDateDisabled
            // 
            lblDateDisabled.AutoSize = true;
            lblDateDisabled.Location = new Point(12, 31);
            lblDateDisabled.Name = "lblDateDisabled";
            lblDateDisabled.Size = new Size(82, 15);
            lblDateDisabled.TabIndex = 18;
            lblDateDisabled.Text = "Date Disabled:";
            // 
            // btnDisable
            // 
            btnDisable.Location = new Point(127, 128);
            btnDisable.Margin = new Padding(4, 3, 4, 3);
            btnDisable.Name = "btnDisable";
            btnDisable.Size = new Size(88, 27);
            btnDisable.TabIndex = 17;
            btnDisable.Text = "Disable";
            btnDisable.UseVisualStyleBackColor = true;
            btnDisable.Click += btnDisable_Click;
            // 
            // dtpDisabledDate
            // 
            dtpDisabledDate.Location = new Point(98, 25);
            dtpDisabledDate.Margin = new Padding(4, 3, 4, 3);
            dtpDisabledDate.Name = "dtpDisabledDate";
            dtpDisabledDate.Size = new Size(233, 23);
            dtpDisabledDate.TabIndex = 15;
            // 
            // lblDeleteAccount
            // 
            lblDeleteAccount.AutoSize = true;
            lblDeleteAccount.Font = new Font("Segoe UI", 14F);
            lblDeleteAccount.Location = new Point(1221, 414);
            lblDeleteAccount.Margin = new Padding(4, 0, 4, 0);
            lblDeleteAccount.Name = "lblDeleteAccount";
            lblDeleteAccount.Size = new Size(140, 25);
            lblDeleteAccount.TabIndex = 88;
            lblDeleteAccount.Text = "Delete Account";
            // 
            // gbxDeleteAccount
            // 
            gbxDeleteAccount.Controls.Add(btnDeleteAccount);
            gbxDeleteAccount.Enabled = false;
            gbxDeleteAccount.Location = new Point(1123, 442);
            gbxDeleteAccount.Margin = new Padding(4, 3, 4, 3);
            gbxDeleteAccount.Name = "gbxDeleteAccount";
            gbxDeleteAccount.Padding = new Padding(4, 3, 4, 3);
            gbxDeleteAccount.Size = new Size(341, 67);
            gbxDeleteAccount.TabIndex = 89;
            gbxDeleteAccount.TabStop = false;
            // 
            // btnDeleteAccount
            // 
            btnDeleteAccount.BackColor = Color.Red;
            btnDeleteAccount.ForeColor = Color.Black;
            btnDeleteAccount.Location = new Point(127, 22);
            btnDeleteAccount.Margin = new Padding(4, 3, 4, 3);
            btnDeleteAccount.Name = "btnDeleteAccount";
            btnDeleteAccount.Size = new Size(88, 27);
            btnDeleteAccount.TabIndex = 17;
            btnDeleteAccount.Text = "Update";
            btnDeleteAccount.UseVisualStyleBackColor = false;
            btnDeleteAccount.Click += btnDeleteAccount_Click;
            // 
            // lblUnlockAccount
            // 
            lblUnlockAccount.AutoSize = true;
            lblUnlockAccount.Font = new Font("Segoe UI", 14F);
            lblUnlockAccount.Location = new Point(906, 682);
            lblUnlockAccount.Margin = new Padding(4, 0, 4, 0);
            lblUnlockAccount.Name = "lblUnlockAccount";
            lblUnlockAccount.Size = new Size(144, 25);
            lblUnlockAccount.TabIndex = 86;
            lblUnlockAccount.Text = "Unlock Account";
            // 
            // gbxUnlockAccount
            // 
            gbxUnlockAccount.Controls.Add(btnUnlockAccount);
            gbxUnlockAccount.Enabled = false;
            gbxUnlockAccount.Location = new Point(853, 710);
            gbxUnlockAccount.Margin = new Padding(4, 3, 4, 3);
            gbxUnlockAccount.Name = "gbxUnlockAccount";
            gbxUnlockAccount.Padding = new Padding(4, 3, 4, 3);
            gbxUnlockAccount.Size = new Size(260, 67);
            gbxUnlockAccount.TabIndex = 87;
            gbxUnlockAccount.TabStop = false;
            // 
            // btnUnlockAccount
            // 
            btnUnlockAccount.Location = new Point(87, 22);
            btnUnlockAccount.Margin = new Padding(4, 3, 4, 3);
            btnUnlockAccount.Name = "btnUnlockAccount";
            btnUnlockAccount.Size = new Size(88, 27);
            btnUnlockAccount.TabIndex = 17;
            btnUnlockAccount.Text = "Update";
            btnUnlockAccount.UseVisualStyleBackColor = true;
            btnUnlockAccount.Click += btnUnlockAccount_Click;
            // 
            // btnAdClear
            // 
            btnAdClear.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAdClear.Location = new Point(176, 718);
            btnAdClear.Name = "btnAdClear";
            btnAdClear.Size = new Size(100, 35);
            btnAdClear.TabIndex = 12;
            btnAdClear.Text = "Clear";
            btnAdClear.UseVisualStyleBackColor = true;
            btnAdClear.Click += btnAdClear_Click;
            // 
            // gbxSingleUserSearch
            // 
            gbxSingleUserSearch.Controls.Add(txbUserName);
            gbxSingleUserSearch.Controls.Add(txbFirstName);
            gbxSingleUserSearch.Controls.Add(txbLastName);
            gbxSingleUserSearch.Controls.Add(lblUsersName);
            gbxSingleUserSearch.Controls.Add(lblLastName);
            gbxSingleUserSearch.Controls.Add(lblFirstName);
            gbxSingleUserSearch.Controls.Add(rbnSingleUserSearch);
            gbxSingleUserSearch.Location = new Point(20, 565);
            gbxSingleUserSearch.Name = "gbxSingleUserSearch";
            gbxSingleUserSearch.Size = new Size(280, 137);
            gbxSingleUserSearch.TabIndex = 11;
            gbxSingleUserSearch.TabStop = false;
            // 
            // txbUserName
            // 
            txbUserName.Location = new Point(100, 103);
            txbUserName.Name = "txbUserName";
            txbUserName.Size = new Size(174, 23);
            txbUserName.TabIndex = 13;
            txbUserName.KeyPress += SingleUserSearchTextbox_KeyPress;
            // 
            // txbFirstName
            // 
            txbFirstName.Location = new Point(100, 50);
            txbFirstName.Name = "txbFirstName";
            txbFirstName.Size = new Size(174, 23);
            txbFirstName.TabIndex = 14;
            txbFirstName.KeyPress += SingleUserSearchTextbox_KeyPress;
            // 
            // txbLastName
            // 
            txbLastName.Location = new Point(100, 76);
            txbLastName.Name = "txbLastName";
            txbLastName.Size = new Size(174, 23);
            txbLastName.TabIndex = 12;
            txbLastName.KeyPress += SingleUserSearchTextbox_KeyPress;
            // 
            // lblUsersName
            // 
            lblUsersName.AutoSize = true;
            lblUsersName.Location = new Point(15, 103);
            lblUsersName.Name = "lblUsersName";
            lblUsersName.Size = new Size(63, 15);
            lblUsersName.TabIndex = 12;
            lblUsersName.Text = "Username:";
            // 
            // lblLastName
            // 
            lblLastName.AutoSize = true;
            lblLastName.Location = new Point(15, 79);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(66, 15);
            lblLastName.TabIndex = 13;
            lblLastName.Text = "Last Name:";
            // 
            // lblFirstName
            // 
            lblFirstName.AutoSize = true;
            lblFirstName.Location = new Point(15, 53);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(67, 15);
            lblFirstName.TabIndex = 14;
            lblFirstName.Text = "First Name:";
            // 
            // rbnSingleUserSearch
            // 
            rbnSingleUserSearch.AutoSize = true;
            rbnSingleUserSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            rbnSingleUserSearch.Location = new Point(15, 22);
            rbnSingleUserSearch.Name = "rbnSingleUserSearch";
            rbnSingleUserSearch.Size = new Size(129, 19);
            rbnSingleUserSearch.TabIndex = 12;
            rbnSingleUserSearch.TabStop = true;
            rbnSingleUserSearch.Text = "Single User Search";
            rbnSingleUserSearch.UseVisualStyleBackColor = true;
            // 
            // tabControlADResults
            // 
            tabControlADResults.Controls.Add(tabResults);
            tabControlADResults.Controls.Add(tabGeneral);
            tabControlADResults.Controls.Add(tabMemberOf);
            tabControlADResults.Location = new Point(320, 20);
            tabControlADResults.Name = "tabControlADResults";
            tabControlADResults.SelectedIndex = 0;
            tabControlADResults.Size = new Size(528, 682);
            tabControlADResults.TabIndex = 10;
            // 
            // tabResults
            // 
            tabResults.Controls.Add(dgvUnifiedResults);
            tabResults.Location = new Point(4, 24);
            tabResults.Name = "tabResults";
            tabResults.Padding = new Padding(3);
            tabResults.Size = new Size(520, 654);
            tabResults.TabIndex = 0;
            tabResults.Text = "Results";
            tabResults.UseVisualStyleBackColor = true;
            // 
            // dgvUnifiedResults
            // 
            dgvUnifiedResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUnifiedResults.Columns.AddRange(new DataGridViewColumn[] { colFullName, colUserName, colFirstName, colLastName, colLogonName, colExpDate, colDaysLeft, colExpirationDate, colDaysExpired, colDaysDisabled, colHomeDirExists, colLockDate, colUnlock });
            dgvUnifiedResults.Dock = DockStyle.Fill;
            dgvUnifiedResults.Location = new Point(3, 3);
            dgvUnifiedResults.Name = "dgvUnifiedResults";
            dgvUnifiedResults.Size = new Size(514, 648);
            dgvUnifiedResults.TabIndex = 0;
            // 
            // colFullName
            // 
            colFullName.HeaderText = "Full Name";
            colFullName.Name = "colFullName";
            colFullName.ReadOnly = true;
            colFullName.Width = 110;
            // 
            // colUserName
            // 
            colUserName.HeaderText = "User Name";
            colUserName.Name = "colUserName";
            colUserName.ReadOnly = true;
            colUserName.Width = 90;
            // 
            // colFirstName
            // 
            colFirstName.HeaderText = "First Name";
            colFirstName.Name = "colFirstName";
            colFirstName.ReadOnly = true;
            colFirstName.Visible = false;
            colFirstName.Width = 135;
            // 
            // colLastName
            // 
            colLastName.HeaderText = "Last Name";
            colLastName.Name = "colLastName";
            colLastName.ReadOnly = true;
            colLastName.Visible = false;
            colLastName.Width = 135;
            // 
            // colLogonName
            // 
            colLogonName.HeaderText = "Logon Name";
            colLogonName.Name = "colLogonName";
            colLogonName.ReadOnly = true;
            colLogonName.Visible = false;
            colLogonName.Width = 135;
            // 
            // colExpDate
            // 
            colExpDate.HeaderText = "Exp Date";
            colExpDate.Name = "colExpDate";
            colExpDate.ReadOnly = true;
            colExpDate.Visible = false;
            // 
            // colDaysLeft
            // 
            colDaysLeft.HeaderText = "Days Left";
            colDaysLeft.Name = "colDaysLeft";
            colDaysLeft.ReadOnly = true;
            colDaysLeft.Visible = false;
            colDaysLeft.Width = 80;
            // 
            // colExpirationDate
            // 
            colExpirationDate.HeaderText = "Expiration Date";
            colExpirationDate.Name = "colExpirationDate";
            colExpirationDate.ReadOnly = true;
            colExpirationDate.Visible = false;
            colExpirationDate.Width = 115;
            // 
            // colDaysExpired
            // 
            colDaysExpired.HeaderText = "Days Expired";
            colDaysExpired.Name = "colDaysExpired";
            colDaysExpired.ReadOnly = true;
            colDaysExpired.Visible = false;
            colDaysExpired.Width = 110;
            // 
            // colDaysDisabled
            // 
            colDaysDisabled.HeaderText = "Days Disabled";
            colDaysDisabled.Name = "colDaysDisabled";
            colDaysDisabled.ReadOnly = true;
            colDaysDisabled.Visible = false;
            colDaysDisabled.Width = 105;
            // 
            // colHomeDirExists
            // 
            colHomeDirExists.HeaderText = "Home Dir Exists";
            colHomeDirExists.Name = "colHomeDirExists";
            colHomeDirExists.ReadOnly = true;
            colHomeDirExists.Visible = false;
            colHomeDirExists.Width = 125;
            // 
            // colLockDate
            // 
            colLockDate.HeaderText = "Lock Date";
            colLockDate.Name = "colLockDate";
            colLockDate.ReadOnly = true;
            colLockDate.Visible = false;
            colLockDate.Width = 115;
            // 
            // colUnlock
            // 
            colUnlock.HeaderText = "Unlock";
            colUnlock.Name = "colUnlock";
            colUnlock.ReadOnly = true;
            colUnlock.Visible = false;
            colUnlock.Width = 110;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(lblUIDNumberValue);
            tabGeneral.Controls.Add(lblUIDNumber);
            tabGeneral.Controls.Add(lblGIDNumberValue);
            tabGeneral.Controls.Add(lblGIDNumber);
            tabGeneral.Controls.Add(lblOUValue);
            tabGeneral.Controls.Add(lblOU);
            tabGeneral.Controls.Add(lblLockedValue);
            tabGeneral.Controls.Add(lblLocked);
            tabGeneral.Controls.Add(lblHomeDriveValue);
            tabGeneral.Controls.Add(lblHomeDrive);
            tabGeneral.Controls.Add(lblLastLoginValue);
            tabGeneral.Controls.Add(lblLastLogin);
            tabGeneral.Controls.Add(lblLastPasswordChangeValue);
            tabGeneral.Controls.Add(lblLastPasswordChange);
            tabGeneral.Controls.Add(lblAccountExpirationValue);
            tabGeneral.Controls.Add(lblAccountExpiration);
            tabGeneral.Controls.Add(lblTelephoneNumberValue);
            tabGeneral.Controls.Add(lblTelephoneNumber);
            tabGeneral.Controls.Add(lblDescriptionValue);
            tabGeneral.Controls.Add(lblDescription);
            tabGeneral.Controls.Add(lblEmailValue);
            tabGeneral.Controls.Add(lblEmail);
            tabGeneral.Controls.Add(lblLoginNameValue);
            tabGeneral.Controls.Add(lblLoginName);
            tabGeneral.Controls.Add(lblGenFirstName);
            tabGeneral.Controls.Add(lblFirstNameValue);
            tabGeneral.Controls.Add(lblGenLastName);
            tabGeneral.Controls.Add(lblLastNameValue);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(520, 654);
            tabGeneral.TabIndex = 1;
            tabGeneral.Text = "General";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // lblUIDNumberValue
            // 
            lblUIDNumberValue.AutoSize = true;
            lblUIDNumberValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblUIDNumberValue.Location = new Point(298, 558);
            lblUIDNumberValue.Name = "lblUIDNumberValue";
            lblUIDNumberValue.Size = new Size(35, 20);
            lblUIDNumberValue.TabIndex = 27;
            lblUIDNumberValue.Text = "N/A";
            lblUIDNumberValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUIDNumber
            // 
            lblUIDNumber.AutoSize = true;
            lblUIDNumber.Font = new Font("Microsoft Sans Serif", 12F);
            lblUIDNumber.Location = new Point(103, 558);
            lblUIDNumber.Name = "lblUIDNumber";
            lblUIDNumber.Size = new Size(102, 20);
            lblUIDNumber.TabIndex = 26;
            lblUIDNumber.Text = "UID Number:";
            lblUIDNumber.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGIDNumberValue
            // 
            lblGIDNumberValue.AutoSize = true;
            lblGIDNumberValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblGIDNumberValue.Location = new Point(298, 522);
            lblGIDNumberValue.Name = "lblGIDNumberValue";
            lblGIDNumberValue.Size = new Size(35, 20);
            lblGIDNumberValue.TabIndex = 25;
            lblGIDNumberValue.Text = "N/A";
            lblGIDNumberValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGIDNumber
            // 
            lblGIDNumber.AutoSize = true;
            lblGIDNumber.Font = new Font("Microsoft Sans Serif", 12F);
            lblGIDNumber.Location = new Point(103, 522);
            lblGIDNumber.Name = "lblGIDNumber";
            lblGIDNumber.Size = new Size(103, 20);
            lblGIDNumber.TabIndex = 24;
            lblGIDNumber.Text = "GID Number:";
            lblGIDNumber.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblOUValue
            // 
            lblOUValue.AutoSize = true;
            lblOUValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblOUValue.Location = new Point(298, 486);
            lblOUValue.Name = "lblOUValue";
            lblOUValue.Size = new Size(35, 20);
            lblOUValue.TabIndex = 23;
            lblOUValue.Text = "N/A";
            lblOUValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblOU
            // 
            lblOU.AutoSize = true;
            lblOU.Font = new Font("Microsoft Sans Serif", 12F);
            lblOU.Location = new Point(103, 486);
            lblOU.Name = "lblOU";
            lblOU.Size = new Size(148, 20);
            lblOU.TabIndex = 22;
            lblOU.Text = "Organizational Unit:";
            lblOU.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLockedValue
            // 
            lblLockedValue.AutoSize = true;
            lblLockedValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblLockedValue.Location = new Point(298, 450);
            lblLockedValue.Name = "lblLockedValue";
            lblLockedValue.Size = new Size(35, 20);
            lblLockedValue.TabIndex = 21;
            lblLockedValue.Text = "N/A";
            lblLockedValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLocked
            // 
            lblLocked.AutoSize = true;
            lblLocked.Font = new Font("Microsoft Sans Serif", 12F);
            lblLocked.Location = new Point(103, 450);
            lblLocked.Name = "lblLocked";
            lblLocked.Size = new Size(65, 20);
            lblLocked.TabIndex = 20;
            lblLocked.Text = "Locked:";
            lblLocked.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblHomeDriveValue
            // 
            lblHomeDriveValue.AutoSize = true;
            lblHomeDriveValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblHomeDriveValue.Location = new Point(298, 414);
            lblHomeDriveValue.Name = "lblHomeDriveValue";
            lblHomeDriveValue.Size = new Size(35, 20);
            lblHomeDriveValue.TabIndex = 19;
            lblHomeDriveValue.Text = "N/A";
            lblHomeDriveValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblHomeDrive
            // 
            lblHomeDrive.AutoSize = true;
            lblHomeDrive.Font = new Font("Microsoft Sans Serif", 12F);
            lblHomeDrive.Location = new Point(103, 414);
            lblHomeDrive.Name = "lblHomeDrive";
            lblHomeDrive.Size = new Size(96, 20);
            lblHomeDrive.TabIndex = 18;
            lblHomeDrive.Text = "Home Drive:";
            lblHomeDrive.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastLoginValue
            // 
            lblLastLoginValue.AutoSize = true;
            lblLastLoginValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblLastLoginValue.Location = new Point(298, 378);
            lblLastLoginValue.Name = "lblLastLoginValue";
            lblLastLoginValue.Size = new Size(35, 20);
            lblLastLoginValue.TabIndex = 17;
            lblLastLoginValue.Text = "N/A";
            lblLastLoginValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastLogin
            // 
            lblLastLogin.AutoSize = true;
            lblLastLogin.Font = new Font("Microsoft Sans Serif", 12F);
            lblLastLogin.Location = new Point(103, 378);
            lblLastLogin.Name = "lblLastLogin";
            lblLastLogin.Size = new Size(87, 20);
            lblLastLogin.TabIndex = 16;
            lblLastLogin.Text = "Last Login:";
            lblLastLogin.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastPasswordChangeValue
            // 
            lblLastPasswordChangeValue.AutoSize = true;
            lblLastPasswordChangeValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblLastPasswordChangeValue.Location = new Point(298, 342);
            lblLastPasswordChangeValue.Name = "lblLastPasswordChangeValue";
            lblLastPasswordChangeValue.Size = new Size(35, 20);
            lblLastPasswordChangeValue.TabIndex = 15;
            lblLastPasswordChangeValue.Text = "N/A";
            lblLastPasswordChangeValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastPasswordChange
            // 
            lblLastPasswordChange.AutoSize = true;
            lblLastPasswordChange.Font = new Font("Microsoft Sans Serif", 12F);
            lblLastPasswordChange.Location = new Point(103, 342);
            lblLastPasswordChange.Name = "lblLastPasswordChange";
            lblLastPasswordChange.Size = new Size(177, 20);
            lblLastPasswordChange.TabIndex = 14;
            lblLastPasswordChange.Text = "Last Password Change:";
            lblLastPasswordChange.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAccountExpirationValue
            // 
            lblAccountExpirationValue.AutoSize = true;
            lblAccountExpirationValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblAccountExpirationValue.Location = new Point(298, 306);
            lblAccountExpirationValue.Name = "lblAccountExpirationValue";
            lblAccountExpirationValue.Size = new Size(35, 20);
            lblAccountExpirationValue.TabIndex = 13;
            lblAccountExpirationValue.Text = "N/A";
            lblAccountExpirationValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAccountExpiration
            // 
            lblAccountExpiration.AutoSize = true;
            lblAccountExpiration.Font = new Font("Microsoft Sans Serif", 12F);
            lblAccountExpiration.Location = new Point(103, 306);
            lblAccountExpiration.Name = "lblAccountExpiration";
            lblAccountExpiration.Size = new Size(146, 20);
            lblAccountExpiration.TabIndex = 12;
            lblAccountExpiration.Text = "Account Expiration:";
            lblAccountExpiration.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTelephoneNumberValue
            // 
            lblTelephoneNumberValue.AutoSize = true;
            lblTelephoneNumberValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblTelephoneNumberValue.Location = new Point(298, 270);
            lblTelephoneNumberValue.Name = "lblTelephoneNumberValue";
            lblTelephoneNumberValue.Size = new Size(35, 20);
            lblTelephoneNumberValue.TabIndex = 11;
            lblTelephoneNumberValue.Text = "N/A";
            lblTelephoneNumberValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTelephoneNumber
            // 
            lblTelephoneNumber.AutoSize = true;
            lblTelephoneNumber.Font = new Font("Microsoft Sans Serif", 12F);
            lblTelephoneNumber.Location = new Point(103, 270);
            lblTelephoneNumber.Name = "lblTelephoneNumber";
            lblTelephoneNumber.Size = new Size(148, 20);
            lblTelephoneNumber.TabIndex = 10;
            lblTelephoneNumber.Text = "Telephone Number:";
            lblTelephoneNumber.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescriptionValue
            // 
            lblDescriptionValue.AutoSize = true;
            lblDescriptionValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblDescriptionValue.Location = new Point(298, 236);
            lblDescriptionValue.Name = "lblDescriptionValue";
            lblDescriptionValue.Size = new Size(35, 20);
            lblDescriptionValue.TabIndex = 9;
            lblDescriptionValue.Text = "N/A";
            lblDescriptionValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Font = new Font("Microsoft Sans Serif", 12F);
            lblDescription.Location = new Point(103, 234);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(93, 20);
            lblDescription.TabIndex = 8;
            lblDescription.Text = "Description:";
            lblDescription.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEmailValue
            // 
            lblEmailValue.AutoSize = true;
            lblEmailValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblEmailValue.Location = new Point(298, 201);
            lblEmailValue.Name = "lblEmailValue";
            lblEmailValue.Size = new Size(35, 20);
            lblEmailValue.TabIndex = 7;
            lblEmailValue.Text = "N/A";
            lblEmailValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Font = new Font("Microsoft Sans Serif", 12F);
            lblEmail.Location = new Point(103, 198);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(52, 20);
            lblEmail.TabIndex = 6;
            lblEmail.Text = "Email:";
            lblEmail.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLoginNameValue
            // 
            lblLoginNameValue.AutoSize = true;
            lblLoginNameValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblLoginNameValue.Location = new Point(298, 160);
            lblLoginNameValue.Name = "lblLoginNameValue";
            lblLoginNameValue.Size = new Size(35, 20);
            lblLoginNameValue.TabIndex = 5;
            lblLoginNameValue.Text = "N/A";
            lblLoginNameValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLoginName
            // 
            lblLoginName.AutoSize = true;
            lblLoginName.Font = new Font("Microsoft Sans Serif", 12F);
            lblLoginName.Location = new Point(103, 162);
            lblLoginName.Name = "lblLoginName";
            lblLoginName.Size = new Size(98, 20);
            lblLoginName.TabIndex = 4;
            lblLoginName.Text = "Login Name:";
            lblLoginName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGenFirstName
            // 
            lblGenFirstName.AutoSize = true;
            lblGenFirstName.Font = new Font("Microsoft Sans Serif", 12F);
            lblGenFirstName.Location = new Point(103, 90);
            lblGenFirstName.Name = "lblGenFirstName";
            lblGenFirstName.Size = new Size(90, 20);
            lblGenFirstName.TabIndex = 0;
            lblGenFirstName.Text = "First Name:";
            lblGenFirstName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFirstNameValue
            // 
            lblFirstNameValue.AutoSize = true;
            lblFirstNameValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblFirstNameValue.Location = new Point(298, 90);
            lblFirstNameValue.Name = "lblFirstNameValue";
            lblFirstNameValue.Size = new Size(35, 20);
            lblFirstNameValue.TabIndex = 1;
            lblFirstNameValue.Text = "N/A";
            lblFirstNameValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGenLastName
            // 
            lblGenLastName.AutoSize = true;
            lblGenLastName.Font = new Font("Microsoft Sans Serif", 12F);
            lblGenLastName.Location = new Point(103, 126);
            lblGenLastName.Name = "lblGenLastName";
            lblGenLastName.Size = new Size(90, 20);
            lblGenLastName.TabIndex = 2;
            lblGenLastName.Text = "Last Name:";
            lblGenLastName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastNameValue
            // 
            lblLastNameValue.AutoSize = true;
            lblLastNameValue.Font = new Font("Microsoft Sans Serif", 12F);
            lblLastNameValue.Location = new Point(298, 126);
            lblLastNameValue.Name = "lblLastNameValue";
            lblLastNameValue.Size = new Size(35, 20);
            lblLastNameValue.TabIndex = 3;
            lblLastNameValue.Text = "N/A";
            lblLastNameValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabMemberOf
            // 
            tabMemberOf.Controls.Add(btnEditUsersGroups);
            tabMemberOf.Controls.Add(lblLoadedUser);
            tabMemberOf.Controls.Add(clbMemberOf);
            tabMemberOf.Location = new Point(4, 24);
            tabMemberOf.Name = "tabMemberOf";
            tabMemberOf.Padding = new Padding(3);
            tabMemberOf.Size = new Size(520, 654);
            tabMemberOf.TabIndex = 2;
            tabMemberOf.Text = "Member Of";
            tabMemberOf.UseVisualStyleBackColor = true;
            // 
            // btnEditUsersGroups
            // 
            btnEditUsersGroups.Location = new Point(147, 507);
            btnEditUsersGroups.Name = "btnEditUsersGroups";
            btnEditUsersGroups.Size = new Size(248, 35);
            btnEditUsersGroups.TabIndex = 5;
            btnEditUsersGroups.Text = "Edit Users Groups";
            btnEditUsersGroups.UseVisualStyleBackColor = true;
            btnEditUsersGroups.Click += btnEditUsersGroups_Click;
            // 
            // lblLoadedUser
            // 
            lblLoadedUser.AutoSize = true;
            lblLoadedUser.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblLoadedUser.Location = new Point(209, 68);
            lblLoadedUser.Name = "lblLoadedUser";
            lblLoadedUser.Size = new Size(131, 21);
            lblLoadedUser.TabIndex = 4;
            lblLoadedUser.Text = "No User Loaded";
            // 
            // clbMemberOf
            // 
            clbMemberOf.FormattingEnabled = true;
            clbMemberOf.Location = new Point(147, 93);
            clbMemberOf.Name = "clbMemberOf";
            clbMemberOf.Size = new Size(248, 400);
            clbMemberOf.TabIndex = 0;
            // 
            // btnAdLoadAccounts
            // 
            btnAdLoadAccounts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAdLoadAccounts.Location = new Point(35, 718);
            btnAdLoadAccounts.Name = "btnAdLoadAccounts";
            btnAdLoadAccounts.Size = new Size(100, 35);
            btnAdLoadAccounts.TabIndex = 4;
            btnAdLoadAccounts.Text = "Load →";
            btnAdLoadAccounts.UseVisualStyleBackColor = true;
            btnAdLoadAccounts.Click += btnAdLoadAccounts_Click;
            // 
            // gbxLockedAccounts
            // 
            gbxLockedAccounts.Controls.Add(rbLockedAccountsOut);
            gbxLockedAccounts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbxLockedAccounts.Location = new Point(20, 480);
            gbxLockedAccounts.Name = "gbxLockedAccounts";
            gbxLockedAccounts.Size = new Size(280, 70);
            gbxLockedAccounts.TabIndex = 3;
            gbxLockedAccounts.TabStop = false;
            gbxLockedAccounts.Text = "Locked Accounts";
            // 
            // rbLockedAccountsOut
            // 
            rbLockedAccountsOut.AutoSize = true;
            rbLockedAccountsOut.Font = new Font("Segoe UI", 9F);
            rbLockedAccountsOut.Location = new Point(15, 30);
            rbLockedAccountsOut.Name = "rbLockedAccountsOut";
            rbLockedAccountsOut.Size = new Size(156, 19);
            rbLockedAccountsOut.TabIndex = 0;
            rbLockedAccountsOut.Text = "XX Accounts Locked Out";
            rbLockedAccountsOut.UseVisualStyleBackColor = true;
            // 
            // gbxDisabledAccounts
            // 
            gbxDisabledAccounts.Controls.Add(rbDisabledAccounts0to30);
            gbxDisabledAccounts.Controls.Add(rbDisabledAccounts31to60);
            gbxDisabledAccounts.Controls.Add(rbDisabledAccounts90Plus);
            gbxDisabledAccounts.Controls.Add(rbDisabledAccounts61to90);
            gbxDisabledAccounts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbxDisabledAccounts.Location = new Point(20, 320);
            gbxDisabledAccounts.Name = "gbxDisabledAccounts";
            gbxDisabledAccounts.Size = new Size(280, 140);
            gbxDisabledAccounts.TabIndex = 2;
            gbxDisabledAccounts.TabStop = false;
            gbxDisabledAccounts.Text = "Disabled Accounts";
            // 
            // rbDisabledAccounts0to30
            // 
            rbDisabledAccounts0to30.AutoSize = true;
            rbDisabledAccounts0to30.Font = new Font("Segoe UI", 9F);
            rbDisabledAccounts0to30.Location = new Point(15, 25);
            rbDisabledAccounts0to30.Name = "rbDisabledAccounts0to30";
            rbDisabledAccounts0to30.Size = new Size(166, 19);
            rbDisabledAccounts0to30.TabIndex = 0;
            rbDisabledAccounts0to30.Text = "XX Accounts Disabled < 30";
            rbDisabledAccounts0to30.UseVisualStyleBackColor = true;
            // 
            // rbDisabledAccounts31to60
            // 
            rbDisabledAccounts31to60.AutoSize = true;
            rbDisabledAccounts31to60.Font = new Font("Segoe UI", 9F);
            rbDisabledAccounts31to60.Location = new Point(15, 53);
            rbDisabledAccounts31to60.Name = "rbDisabledAccounts31to60";
            rbDisabledAccounts31to60.Size = new Size(241, 19);
            rbDisabledAccounts31to60.TabIndex = 1;
            rbDisabledAccounts31to60.Text = "XX Accounts Disabled from 30 to 59 Days";
            rbDisabledAccounts31to60.UseVisualStyleBackColor = true;
            // 
            // rbDisabledAccounts90Plus
            // 
            rbDisabledAccounts90Plus.AutoSize = true;
            rbDisabledAccounts90Plus.Font = new Font("Segoe UI", 9F);
            rbDisabledAccounts90Plus.Location = new Point(15, 109);
            rbDisabledAccounts90Plus.Name = "rbDisabledAccounts90Plus";
            rbDisabledAccounts90Plus.Size = new Size(209, 19);
            rbDisabledAccounts90Plus.TabIndex = 3;
            rbDisabledAccounts90Plus.Text = "XX Accounts Disabled for 90+ Days";
            rbDisabledAccounts90Plus.UseVisualStyleBackColor = true;
            // 
            // rbDisabledAccounts61to90
            // 
            rbDisabledAccounts61to90.AutoSize = true;
            rbDisabledAccounts61to90.Font = new Font("Segoe UI", 9F);
            rbDisabledAccounts61to90.Location = new Point(15, 81);
            rbDisabledAccounts61to90.Name = "rbDisabledAccounts61to90";
            rbDisabledAccounts61to90.Size = new Size(241, 19);
            rbDisabledAccounts61to90.TabIndex = 2;
            rbDisabledAccounts61to90.Text = "XX Accounts Disabled from 60 to 89 Days";
            rbDisabledAccounts61to90.UseVisualStyleBackColor = true;
            // 
            // gbxExpiredAccounts
            // 
            gbxExpiredAccounts.Controls.Add(rbExpiredAccounts0to30);
            gbxExpiredAccounts.Controls.Add(rbExpiredAccounts31to60);
            gbxExpiredAccounts.Controls.Add(rbExpiredAccounts61to90);
            gbxExpiredAccounts.Controls.Add(rbExpiredAccounts90Plus);
            gbxExpiredAccounts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbxExpiredAccounts.Location = new Point(20, 160);
            gbxExpiredAccounts.Name = "gbxExpiredAccounts";
            gbxExpiredAccounts.Size = new Size(280, 140);
            gbxExpiredAccounts.TabIndex = 1;
            gbxExpiredAccounts.TabStop = false;
            gbxExpiredAccounts.Text = "Expired Accounts";
            // 
            // rbExpiredAccounts0to30
            // 
            rbExpiredAccounts0to30.AutoSize = true;
            rbExpiredAccounts0to30.Font = new Font("Segoe UI", 9F);
            rbExpiredAccounts0to30.Location = new Point(15, 25);
            rbExpiredAccounts0to30.Name = "rbExpiredAccounts0to30";
            rbExpiredAccounts0to30.Size = new Size(229, 19);
            rbExpiredAccounts0to30.TabIndex = 0;
            rbExpiredAccounts0to30.Text = "XX Accounts Expired from 0 to 30 Days";
            rbExpiredAccounts0to30.UseVisualStyleBackColor = true;
            // 
            // rbExpiredAccounts31to60
            // 
            rbExpiredAccounts31to60.AutoSize = true;
            rbExpiredAccounts31to60.Font = new Font("Segoe UI", 9F);
            rbExpiredAccounts31to60.Location = new Point(15, 55);
            rbExpiredAccounts31to60.Name = "rbExpiredAccounts31to60";
            rbExpiredAccounts31to60.Size = new Size(235, 19);
            rbExpiredAccounts31to60.TabIndex = 1;
            rbExpiredAccounts31to60.Text = "XX Accounts Expired from 31 to 60 Days";
            rbExpiredAccounts31to60.UseVisualStyleBackColor = true;
            // 
            // rbExpiredAccounts61to90
            // 
            rbExpiredAccounts61to90.AutoSize = true;
            rbExpiredAccounts61to90.Font = new Font("Segoe UI", 9F);
            rbExpiredAccounts61to90.Location = new Point(15, 85);
            rbExpiredAccounts61to90.Name = "rbExpiredAccounts61to90";
            rbExpiredAccounts61to90.Size = new Size(235, 19);
            rbExpiredAccounts61to90.TabIndex = 2;
            rbExpiredAccounts61to90.Text = "XX Accounts Expired from 61 to 90 Days";
            rbExpiredAccounts61to90.UseVisualStyleBackColor = true;
            // 
            // rbExpiredAccounts90Plus
            // 
            rbExpiredAccounts90Plus.AutoSize = true;
            rbExpiredAccounts90Plus.Font = new Font("Segoe UI", 9F);
            rbExpiredAccounts90Plus.Location = new Point(15, 115);
            rbExpiredAccounts90Plus.Name = "rbExpiredAccounts90Plus";
            rbExpiredAccounts90Plus.Size = new Size(185, 19);
            rbExpiredAccounts90Plus.TabIndex = 3;
            rbExpiredAccounts90Plus.Text = "XX Accounts Expired 90+ Days";
            rbExpiredAccounts90Plus.UseVisualStyleBackColor = true;
            // 
            // gbxExpiringAccounts
            // 
            gbxExpiringAccounts.Controls.Add(rbExpiringAccounts0to30);
            gbxExpiringAccounts.Controls.Add(rbExpiringAccounts31to60);
            gbxExpiringAccounts.Controls.Add(rbExpiringAccounts61to90);
            gbxExpiringAccounts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbxExpiringAccounts.Location = new Point(20, 20);
            gbxExpiringAccounts.Name = "gbxExpiringAccounts";
            gbxExpiringAccounts.Size = new Size(280, 120);
            gbxExpiringAccounts.TabIndex = 0;
            gbxExpiringAccounts.TabStop = false;
            gbxExpiringAccounts.Text = "Expiring Accounts";
            // 
            // rbExpiringAccounts0to30
            // 
            rbExpiringAccounts0to30.AutoSize = true;
            rbExpiringAccounts0to30.Font = new Font("Segoe UI", 9F);
            rbExpiringAccounts0to30.Location = new Point(15, 95);
            rbExpiringAccounts0to30.Name = "rbExpiringAccounts0to30";
            rbExpiringAccounts0to30.Size = new Size(217, 19);
            rbExpiringAccounts0to30.TabIndex = 2;
            rbExpiringAccounts0to30.Text = "XX Accounts Expiring in 0 to 30 Days";
            rbExpiringAccounts0to30.UseVisualStyleBackColor = true;
            // 
            // rbExpiringAccounts31to60
            // 
            rbExpiringAccounts31to60.AutoSize = true;
            rbExpiringAccounts31to60.Font = new Font("Segoe UI", 9F);
            rbExpiringAccounts31to60.Location = new Point(15, 60);
            rbExpiringAccounts31to60.Name = "rbExpiringAccounts31to60";
            rbExpiringAccounts31to60.Size = new Size(223, 19);
            rbExpiringAccounts31to60.TabIndex = 1;
            rbExpiringAccounts31to60.Text = "XX Accounts Expiring in 31 to 60 Days";
            rbExpiringAccounts31to60.UseVisualStyleBackColor = true;
            // 
            // rbExpiringAccounts61to90
            // 
            rbExpiringAccounts61to90.AutoSize = true;
            rbExpiringAccounts61to90.Font = new Font("Segoe UI", 9F);
            rbExpiringAccounts61to90.Location = new Point(15, 25);
            rbExpiringAccounts61to90.Name = "rbExpiringAccounts61to90";
            rbExpiringAccounts61to90.Size = new Size(223, 19);
            rbExpiringAccounts61to90.TabIndex = 0;
            rbExpiringAccounts61to90.Text = "XX Accounts Expiring in 61 to 90 Days";
            rbExpiringAccounts61to90.UseVisualStyleBackColor = true;
            // 
            // lblChangePassword
            // 
            lblChangePassword.AutoSize = true;
            lblChangePassword.Font = new Font("Segoe UI", 14F);
            lblChangePassword.Location = new Point(974, 7);
            lblChangePassword.Margin = new Padding(4, 0, 4, 0);
            lblChangePassword.Name = "lblChangePassword";
            lblChangePassword.Size = new Size(161, 25);
            lblChangePassword.TabIndex = 2;
            lblChangePassword.Text = "Change Password";
            // 
            // gbxChangePassword
            // 
            gbxChangePassword.Controls.Add(cbxUnlockAcnt);
            gbxChangePassword.Controls.Add(btnClearPasswords);
            gbxChangePassword.Controls.Add(btnSubmit);
            gbxChangePassword.Controls.Add(btnPwChngShowPassword);
            gbxChangePassword.Controls.Add(txbConfirmNewPassword);
            gbxChangePassword.Controls.Add(lblConfirmNewPassword);
            gbxChangePassword.Controls.Add(txbNewPassword);
            gbxChangePassword.Controls.Add(lblPwdRequirements);
            gbxChangePassword.Controls.Add(lblNewPassword);
            gbxChangePassword.Controls.Add(lblOneSpecial);
            gbxChangePassword.Controls.Add(lblOneNumber);
            gbxChangePassword.Controls.Add(lblOneLowercase);
            gbxChangePassword.Controls.Add(lblOneUppercase);
            gbxChangePassword.Controls.Add(lblFourteenChrs);
            gbxChangePassword.Enabled = false;
            gbxChangePassword.Location = new Point(855, 35);
            gbxChangePassword.Margin = new Padding(4, 3, 4, 3);
            gbxChangePassword.Name = "gbxChangePassword";
            gbxChangePassword.Padding = new Padding(4, 3, 4, 3);
            gbxChangePassword.Size = new Size(433, 362);
            gbxChangePassword.TabIndex = 62;
            gbxChangePassword.TabStop = false;
            // 
            // cbxUnlockAcnt
            // 
            cbxUnlockAcnt.AutoSize = true;
            cbxUnlockAcnt.CheckAlign = ContentAlignment.MiddleRight;
            cbxUnlockAcnt.Location = new Point(49, 283);
            cbxUnlockAcnt.Margin = new Padding(4, 3, 4, 3);
            cbxUnlockAcnt.Name = "cbxUnlockAcnt";
            cbxUnlockAcnt.Size = new Size(114, 19);
            cbxUnlockAcnt.TabIndex = 71;
            cbxUnlockAcnt.Text = "Unlock Account:";
            cbxUnlockAcnt.UseVisualStyleBackColor = true;
            // 
            // btnClearPasswords
            // 
            btnClearPasswords.Location = new Point(230, 312);
            btnClearPasswords.Margin = new Padding(4, 3, 4, 3);
            btnClearPasswords.Name = "btnClearPasswords";
            btnClearPasswords.Size = new Size(88, 30);
            btnClearPasswords.TabIndex = 69;
            btnClearPasswords.Text = "Clear";
            btnClearPasswords.UseVisualStyleBackColor = true;
            btnClearPasswords.Click += btnClearPasswords_Click;
            // 
            // btnSubmit
            // 
            btnSubmit.Location = new Point(135, 312);
            btnSubmit.Margin = new Padding(4, 3, 4, 3);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(88, 30);
            btnSubmit.TabIndex = 68;
            btnSubmit.Text = "Submit";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // btnPwChngShowPassword
            // 
            btnPwChngShowPassword.Location = new Point(327, 204);
            btnPwChngShowPassword.Margin = new Padding(4, 3, 4, 3);
            btnPwChngShowPassword.Name = "btnPwChngShowPassword";
            btnPwChngShowPassword.Size = new Size(77, 53);
            btnPwChngShowPassword.TabIndex = 67;
            btnPwChngShowPassword.Text = "Show Password";
            btnPwChngShowPassword.UseVisualStyleBackColor = true;
            btnPwChngShowPassword.MouseDown += ShowPassword_MouseDown;
            btnPwChngShowPassword.MouseLeave += HidePassword_MouseLeave;
            btnPwChngShowPassword.MouseUp += HidePassword_MouseUp;
            // 
            // txbConfirmNewPassword
            // 
            txbConfirmNewPassword.Location = new Point(162, 237);
            txbConfirmNewPassword.Margin = new Padding(4, 3, 4, 3);
            txbConfirmNewPassword.Name = "txbConfirmNewPassword";
            txbConfirmNewPassword.PasswordChar = '*';
            txbConfirmNewPassword.Size = new Size(157, 23);
            txbConfirmNewPassword.TabIndex = 66;
            txbConfirmNewPassword.TextChanged += txbNewPassword_TextChanged;
            // 
            // lblConfirmNewPassword
            // 
            lblConfirmNewPassword.AutoSize = true;
            lblConfirmNewPassword.Location = new Point(20, 237);
            lblConfirmNewPassword.Margin = new Padding(4, 0, 4, 0);
            lblConfirmNewPassword.Name = "lblConfirmNewPassword";
            lblConfirmNewPassword.Size = new Size(134, 15);
            lblConfirmNewPassword.TabIndex = 65;
            lblConfirmNewPassword.Text = "Confirm New Password:";
            // 
            // txbNewPassword
            // 
            txbNewPassword.Location = new Point(162, 198);
            txbNewPassword.Margin = new Padding(4, 3, 4, 3);
            txbNewPassword.Name = "txbNewPassword";
            txbNewPassword.PasswordChar = '*';
            txbNewPassword.Size = new Size(157, 23);
            txbNewPassword.TabIndex = 64;
            txbNewPassword.TextChanged += txbNewPassword_TextChanged;
            // 
            // lblPwdRequirements
            // 
            lblPwdRequirements.AutoSize = true;
            lblPwdRequirements.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Underline, GraphicsUnit.Point, 0);
            lblPwdRequirements.Location = new Point(96, 22);
            lblPwdRequirements.Margin = new Padding(4, 0, 4, 0);
            lblPwdRequirements.Name = "lblPwdRequirements";
            lblPwdRequirements.Size = new Size(182, 20);
            lblPwdRequirements.TabIndex = 51;
            lblPwdRequirements.Text = "Password Requirements";
            // 
            // lblNewPassword
            // 
            lblNewPassword.AutoSize = true;
            lblNewPassword.Location = new Point(67, 202);
            lblNewPassword.Margin = new Padding(4, 0, 4, 0);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new Size(87, 15);
            lblNewPassword.TabIndex = 63;
            lblNewPassword.Text = "New Password:";
            // 
            // lblOneSpecial
            // 
            lblOneSpecial.AutoSize = true;
            lblOneSpecial.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblOneSpecial.ForeColor = Color.Red;
            lblOneSpecial.Location = new Point(135, 173);
            lblOneSpecial.Margin = new Padding(4, 0, 4, 0);
            lblOneSpecial.Name = "lblOneSpecial";
            lblOneSpecial.Size = new Size(132, 17);
            lblOneSpecial.TabIndex = 61;
            lblOneSpecial.Text = "1 Special Character";
            // 
            // lblOneNumber
            // 
            lblOneNumber.AutoSize = true;
            lblOneNumber.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblOneNumber.ForeColor = Color.Red;
            lblOneNumber.Location = new Point(135, 143);
            lblOneNumber.Margin = new Padding(4, 0, 4, 0);
            lblOneNumber.Name = "lblOneNumber";
            lblOneNumber.Size = new Size(70, 17);
            lblOneNumber.TabIndex = 60;
            lblOneNumber.Text = "1 Number";
            // 
            // lblOneLowercase
            // 
            lblOneLowercase.AutoSize = true;
            lblOneLowercase.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblOneLowercase.ForeColor = Color.Red;
            lblOneLowercase.Location = new Point(135, 113);
            lblOneLowercase.Margin = new Padding(4, 0, 4, 0);
            lblOneLowercase.Name = "lblOneLowercase";
            lblOneLowercase.Size = new Size(88, 17);
            lblOneLowercase.TabIndex = 59;
            lblOneLowercase.Text = "1 Lowercase";
            // 
            // lblOneUppercase
            // 
            lblOneUppercase.AutoSize = true;
            lblOneUppercase.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblOneUppercase.ForeColor = Color.Red;
            lblOneUppercase.Location = new Point(135, 83);
            lblOneUppercase.Margin = new Padding(4, 0, 4, 0);
            lblOneUppercase.Name = "lblOneUppercase";
            lblOneUppercase.Size = new Size(89, 17);
            lblOneUppercase.TabIndex = 58;
            lblOneUppercase.Text = "1 Uppercase";
            // 
            // lblFourteenChrs
            // 
            lblFourteenChrs.AutoSize = true;
            lblFourteenChrs.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFourteenChrs.ForeColor = Color.Red;
            lblFourteenChrs.Location = new Point(135, 53);
            lblFourteenChrs.Margin = new Padding(4, 0, 4, 0);
            lblFourteenChrs.Name = "lblFourteenChrs";
            lblFourteenChrs.Size = new Size(97, 17);
            lblFourteenChrs.TabIndex = 57;
            lblFourteenChrs.Text = "14 Characters";
            // 
            // lblTestPassword
            // 
            lblTestPassword.AutoSize = true;
            lblTestPassword.Font = new Font("Segoe UI", 14F);
            lblTestPassword.Location = new Point(919, 414);
            lblTestPassword.Margin = new Padding(4, 0, 4, 0);
            lblTestPassword.Name = "lblTestPassword";
            lblTestPassword.Size = new Size(128, 25);
            lblTestPassword.TabIndex = 84;
            lblTestPassword.Text = "Test Password";
            // 
            // gbxTestPassword
            // 
            gbxTestPassword.Controls.Add(btnTestPassword);
            gbxTestPassword.Controls.Add(txbTestPassword);
            gbxTestPassword.Controls.Add(btnShowTestPassword);
            gbxTestPassword.Enabled = false;
            gbxTestPassword.Location = new Point(855, 442);
            gbxTestPassword.Margin = new Padding(4, 3, 4, 3);
            gbxTestPassword.Name = "gbxTestPassword";
            gbxTestPassword.Padding = new Padding(4, 3, 4, 3);
            gbxTestPassword.Size = new Size(260, 108);
            gbxTestPassword.TabIndex = 85;
            gbxTestPassword.TabStop = false;
            // 
            // btnTestPassword
            // 
            btnTestPassword.Location = new Point(85, 60);
            btnTestPassword.Margin = new Padding(4, 3, 4, 3);
            btnTestPassword.Name = "btnTestPassword";
            btnTestPassword.Size = new Size(88, 27);
            btnTestPassword.TabIndex = 17;
            btnTestPassword.Text = "Test";
            btnTestPassword.UseVisualStyleBackColor = true;
            btnTestPassword.Click += btnTestPassword_Click;
            // 
            // txbTestPassword
            // 
            txbTestPassword.Location = new Point(21, 25);
            txbTestPassword.Margin = new Padding(4, 3, 4, 3);
            txbTestPassword.Name = "txbTestPassword";
            txbTestPassword.PasswordChar = '*';
            txbTestPassword.Size = new Size(222, 23);
            txbTestPassword.TabIndex = 18;
            // 
            // btnShowTestPassword
            // 
            btnShowTestPassword.BackgroundImage = (Image)resources.GetObject("btnShowTestPassword.BackgroundImage");
            btnShowTestPassword.Location = new Point(191, 55);
            btnShowTestPassword.Margin = new Padding(4, 3, 4, 3);
            btnShowTestPassword.Name = "btnShowTestPassword";
            btnShowTestPassword.Size = new Size(52, 40);
            btnShowTestPassword.TabIndex = 72;
            btnShowTestPassword.UseVisualStyleBackColor = true;
            btnShowTestPassword.MouseDown += ShowPassword_MouseDown;
            btnShowTestPassword.MouseLeave += HidePassword_MouseLeave;
            btnShowTestPassword.MouseUp += HidePassword_MouseUp;
            // 
            // lblActExpDate
            // 
            lblActExpDate.AutoSize = true;
            lblActExpDate.Font = new Font("Segoe UI", 14F);
            lblActExpDate.Location = new Point(904, 553);
            lblActExpDate.Margin = new Padding(4, 0, 4, 0);
            lblActExpDate.Name = "lblActExpDate";
            lblActExpDate.Size = new Size(141, 25);
            lblActExpDate.TabIndex = 82;
            lblActExpDate.Text = "Expiration Date";
            // 
            // gbxAcntExpDate
            // 
            gbxAcntExpDate.Controls.Add(btnAcntExeDateUpdate);
            gbxAcntExpDate.Controls.Add(pkrAcntExpDateTimePicker);
            gbxAcntExpDate.Enabled = false;
            gbxAcntExpDate.Location = new Point(851, 581);
            gbxAcntExpDate.Margin = new Padding(4, 3, 4, 3);
            gbxAcntExpDate.Name = "gbxAcntExpDate";
            gbxAcntExpDate.Padding = new Padding(4, 3, 4, 3);
            gbxAcntExpDate.Size = new Size(260, 98);
            gbxAcntExpDate.TabIndex = 83;
            gbxAcntExpDate.TabStop = false;
            // 
            // btnAcntExeDateUpdate
            // 
            btnAcntExeDateUpdate.Location = new Point(85, 60);
            btnAcntExeDateUpdate.Margin = new Padding(4, 3, 4, 3);
            btnAcntExeDateUpdate.Name = "btnAcntExeDateUpdate";
            btnAcntExeDateUpdate.Size = new Size(88, 27);
            btnAcntExeDateUpdate.TabIndex = 17;
            btnAcntExeDateUpdate.Text = "Update";
            btnAcntExeDateUpdate.UseVisualStyleBackColor = true;
            btnAcntExeDateUpdate.Click += btnAcntExeDateUpdate_Click;
            // 
            // pkrAcntExpDateTimePicker
            // 
            pkrAcntExpDateTimePicker.Location = new Point(14, 29);
            pkrAcntExpDateTimePicker.Margin = new Padding(4, 3, 4, 3);
            pkrAcntExpDateTimePicker.Name = "pkrAcntExpDateTimePicker";
            pkrAcntExpDateTimePicker.Size = new Size(233, 23);
            pkrAcntExpDateTimePicker.TabIndex = 15;
            // 
            // tabLDAP
            // 
            tabLDAP.Controls.Add(gbxUserAccountCreation);
            tabLDAP.Controls.Add(lblNoteLowerCase);
            tabLDAP.Controls.Add(lblNewUserAcntCreation);
            tabLDAP.Location = new Point(4, 24);
            tabLDAP.Name = "tabLDAP";
            tabLDAP.Padding = new Padding(3);
            tabLDAP.Size = new Size(1651, 847);
            tabLDAP.TabIndex = 2;
            tabLDAP.Text = "LDAP";
            tabLDAP.UseVisualStyleBackColor = true;
            // 
            // gbxUserAccountCreation
            // 
            gbxUserAccountCreation.Controls.Add(rbnSecurityGroupOther);
            gbxUserAccountCreation.Controls.Add(txbSecurityGroupOther);
            gbxUserAccountCreation.Controls.Add(txbSecurityGroupId);
            gbxUserAccountCreation.Controls.Add(lblDefaultSecurityGroupId);
            gbxUserAccountCreation.Controls.Add(cbxDefaultSecurityGroups);
            gbxUserAccountCreation.Controls.Add(lblDefaultSecurityGroup);
            gbxUserAccountCreation.Controls.Add(lblLdapTempPass);
            gbxUserAccountCreation.Controls.Add(lblLdapFirstName);
            gbxUserAccountCreation.Controls.Add(txbLdapTempPass);
            gbxUserAccountCreation.Controls.Add(txbLdapPhone);
            gbxUserAccountCreation.Controls.Add(btnClearAccountCreationForm);
            gbxUserAccountCreation.Controls.Add(btnLdapGetUid);
            gbxUserAccountCreation.Controls.Add(lblLdapLinuxUid);
            gbxUserAccountCreation.Controls.Add(txbLdapLinuxUid);
            gbxUserAccountCreation.Controls.Add(btnLdapCreateAccount);
            gbxUserAccountCreation.Controls.Add(txbLdapEmail);
            gbxUserAccountCreation.Controls.Add(txbLdapLastName);
            gbxUserAccountCreation.Controls.Add(txbLdapFirstName);
            gbxUserAccountCreation.Controls.Add(lblLdapLastName);
            gbxUserAccountCreation.Controls.Add(lblLdapPhone);
            gbxUserAccountCreation.Controls.Add(lblLdapEmail);
            gbxUserAccountCreation.Controls.Add(btnLdapGenerate);
            gbxUserAccountCreation.Controls.Add(lblLdapNtUserId);
            gbxUserAccountCreation.Controls.Add(txbLdapNtUserId);
            gbxUserAccountCreation.Location = new Point(13, 71);
            gbxUserAccountCreation.Name = "gbxUserAccountCreation";
            gbxUserAccountCreation.Size = new Size(489, 710);
            gbxUserAccountCreation.TabIndex = 18;
            gbxUserAccountCreation.TabStop = false;
            // 
            // rbnSecurityGroupOther
            // 
            rbnSecurityGroupOther.AutoSize = true;
            rbnSecurityGroupOther.Location = new Point(78, 631);
            rbnSecurityGroupOther.Name = "rbnSecurityGroupOther";
            rbnSecurityGroupOther.Size = new Size(55, 19);
            rbnSecurityGroupOther.TabIndex = 28;
            rbnSecurityGroupOther.TabStop = true;
            rbnSecurityGroupOther.Text = "Other";
            rbnSecurityGroupOther.UseVisualStyleBackColor = true;
            // 
            // txbSecurityGroupOther
            // 
            txbSecurityGroupOther.Location = new Point(139, 630);
            txbSecurityGroupOther.Name = "txbSecurityGroupOther";
            txbSecurityGroupOther.Size = new Size(195, 23);
            txbSecurityGroupOther.TabIndex = 8;
            // 
            // txbSecurityGroupId
            // 
            txbSecurityGroupId.Location = new Point(242, 180);
            txbSecurityGroupId.Name = "txbSecurityGroupId";
            txbSecurityGroupId.Size = new Size(121, 23);
            txbSecurityGroupId.TabIndex = 7;
            // 
            // lblDefaultSecurityGroupId
            // 
            lblDefaultSecurityGroupId.AutoSize = true;
            lblDefaultSecurityGroupId.Location = new Point(95, 183);
            lblDefaultSecurityGroupId.Name = "lblDefaultSecurityGroupId";
            lblDefaultSecurityGroupId.Size = new Size(136, 15);
            lblDefaultSecurityGroupId.TabIndex = 25;
            lblDefaultSecurityGroupId.Text = "Deault Security Group ID";
            // 
            // cbxDefaultSecurityGroups
            // 
            cbxDefaultSecurityGroups.ColumnWidth = 210;
            cbxDefaultSecurityGroups.FormattingEnabled = true;
            cbxDefaultSecurityGroups.Location = new Point(6, 237);
            cbxDefaultSecurityGroups.MultiColumn = true;
            cbxDefaultSecurityGroups.Name = "cbxDefaultSecurityGroups";
            cbxDefaultSecurityGroups.Size = new Size(449, 382);
            cbxDefaultSecurityGroups.TabIndex = 23;
            cbxDefaultSecurityGroups.ItemCheck += cbxDefaultSecurityGroups_ItemCheck;
            // 
            // lblDefaultSecurityGroup
            // 
            lblDefaultSecurityGroup.AutoSize = true;
            lblDefaultSecurityGroup.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblDefaultSecurityGroup.Location = new Point(112, 209);
            lblDefaultSecurityGroup.Name = "lblDefaultSecurityGroup";
            lblDefaultSecurityGroup.Size = new Size(222, 25);
            lblDefaultSecurityGroup.TabIndex = 22;
            lblDefaultSecurityGroup.Text = "Default Security Group:";
            // 
            // lblLdapTempPass
            // 
            lblLdapTempPass.AutoSize = true;
            lblLdapTempPass.Location = new Point(238, 145);
            lblLdapTempPass.Name = "lblLdapTempPass";
            lblLdapTempPass.Size = new Size(89, 15);
            lblLdapTempPass.TabIndex = 21;
            lblLdapTempPass.Text = "Temp Password";
            // 
            // lblLdapFirstName
            // 
            lblLdapFirstName.AutoSize = true;
            lblLdapFirstName.Location = new Point(23, 33);
            lblLdapFirstName.Name = "lblLdapFirstName";
            lblLdapFirstName.Size = new Size(67, 15);
            lblLdapFirstName.TabIndex = 9;
            lblLdapFirstName.Text = "First Name:";
            // 
            // txbLdapTempPass
            // 
            txbLdapTempPass.Location = new Point(334, 142);
            txbLdapTempPass.Name = "txbLdapTempPass";
            txbLdapTempPass.Size = new Size(121, 23);
            txbLdapTempPass.TabIndex = 6;
            txbLdapTempPass.Text = "!QW@AZXS1qw2azxs";
            // 
            // txbLdapPhone
            // 
            txbLdapPhone.Location = new Point(334, 103);
            txbLdapPhone.Name = "txbLdapPhone";
            txbLdapPhone.Size = new Size(121, 23);
            txbLdapPhone.TabIndex = 4;
            // 
            // btnClearAccountCreationForm
            // 
            btnClearAccountCreationForm.Location = new Point(298, 674);
            btnClearAccountCreationForm.Name = "btnClearAccountCreationForm";
            btnClearAccountCreationForm.Size = new Size(100, 30);
            btnClearAccountCreationForm.TabIndex = 10;
            btnClearAccountCreationForm.Text = "Clear Form";
            btnClearAccountCreationForm.UseVisualStyleBackColor = true;
            btnClearAccountCreationForm.Click += btnClearAccountCreationForm_Click;
            // 
            // btnLdapGetUid
            // 
            btnLdapGetUid.Location = new Point(96, 142);
            btnLdapGetUid.Name = "btnLdapGetUid";
            btnLdapGetUid.Size = new Size(100, 23);
            btnLdapGetUid.TabIndex = 5;
            btnLdapGetUid.Text = "Get UID";
            btnLdapGetUid.UseVisualStyleBackColor = true;
            btnLdapGetUid.Click += btnLdapGetUid_Click;
            // 
            // lblLdapLinuxUid
            // 
            lblLdapLinuxUid.AutoSize = true;
            lblLdapLinuxUid.Location = new Point(29, 145);
            lblLdapLinuxUid.Name = "lblLdapLinuxUid";
            lblLdapLinuxUid.Size = new Size(61, 15);
            lblLdapLinuxUid.TabIndex = 19;
            lblLdapLinuxUid.Text = "Linux UID:";
            // 
            // txbLdapLinuxUid
            // 
            txbLdapLinuxUid.Location = new Point(96, 142);
            txbLdapLinuxUid.Name = "txbLdapLinuxUid";
            txbLdapLinuxUid.Size = new Size(100, 23);
            txbLdapLinuxUid.TabIndex = 13;
            // 
            // btnLdapCreateAccount
            // 
            btnLdapCreateAccount.Location = new Point(112, 674);
            btnLdapCreateAccount.Name = "btnLdapCreateAccount";
            btnLdapCreateAccount.Size = new Size(100, 30);
            btnLdapCreateAccount.TabIndex = 9;
            btnLdapCreateAccount.Text = "Create Account";
            btnLdapCreateAccount.UseVisualStyleBackColor = true;
            btnLdapCreateAccount.Click += btnLdapCreateAccount_Click;
            // 
            // txbLdapEmail
            // 
            txbLdapEmail.Location = new Point(96, 63);
            txbLdapEmail.Name = "txbLdapEmail";
            txbLdapEmail.Size = new Size(359, 23);
            txbLdapEmail.TabIndex = 2;
            // 
            // txbLdapLastName
            // 
            txbLdapLastName.Location = new Point(334, 33);
            txbLdapLastName.Name = "txbLdapLastName";
            txbLdapLastName.Size = new Size(121, 23);
            txbLdapLastName.TabIndex = 1;
            // 
            // txbLdapFirstName
            // 
            txbLdapFirstName.Location = new Point(96, 30);
            txbLdapFirstName.Name = "txbLdapFirstName";
            txbLdapFirstName.Size = new Size(121, 23);
            txbLdapFirstName.TabIndex = 0;
            // 
            // lblLdapLastName
            // 
            lblLdapLastName.AutoSize = true;
            lblLdapLastName.Location = new Point(262, 38);
            lblLdapLastName.Name = "lblLdapLastName";
            lblLdapLastName.Size = new Size(66, 15);
            lblLdapLastName.TabIndex = 8;
            lblLdapLastName.Text = "Last Name:";
            // 
            // lblLdapPhone
            // 
            lblLdapPhone.AutoSize = true;
            lblLdapPhone.Location = new Point(283, 107);
            lblLdapPhone.Name = "lblLdapPhone";
            lblLdapPhone.Size = new Size(44, 15);
            lblLdapPhone.TabIndex = 5;
            lblLdapPhone.Text = "Phone:";
            // 
            // lblLdapEmail
            // 
            lblLdapEmail.AutoSize = true;
            lblLdapEmail.Location = new Point(6, 66);
            lblLdapEmail.Name = "lblLdapEmail";
            lblLdapEmail.Size = new Size(84, 15);
            lblLdapEmail.TabIndex = 7;
            lblLdapEmail.Text = "Email Address:";
            // 
            // btnLdapGenerate
            // 
            btnLdapGenerate.Location = new Point(96, 103);
            btnLdapGenerate.Name = "btnLdapGenerate";
            btnLdapGenerate.Size = new Size(100, 23);
            btnLdapGenerate.TabIndex = 3;
            btnLdapGenerate.Text = "Generate";
            btnLdapGenerate.UseVisualStyleBackColor = true;
            btnLdapGenerate.Click += btnLdapGenerate_Click;
            // 
            // lblLdapNtUserId
            // 
            lblLdapNtUserId.AutoSize = true;
            lblLdapNtUserId.Location = new Point(25, 107);
            lblLdapNtUserId.Name = "lblLdapNtUserId";
            lblLdapNtUserId.Size = new Size(65, 15);
            lblLdapNtUserId.TabIndex = 6;
            lblLdapNtUserId.Text = "NT User ID:";
            // 
            // txbLdapNtUserId
            // 
            txbLdapNtUserId.Location = new Point(96, 103);
            txbLdapNtUserId.Name = "txbLdapNtUserId";
            txbLdapNtUserId.Size = new Size(100, 23);
            txbLdapNtUserId.TabIndex = 15;
            // 
            // lblNoteLowerCase
            // 
            lblNoteLowerCase.AutoSize = true;
            lblNoteLowerCase.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblNoteLowerCase.Location = new Point(99, 44);
            lblNoteLowerCase.Name = "lblNoteLowerCase";
            lblNoteLowerCase.Size = new Size(312, 21);
            lblNoteLowerCase.TabIndex = 10;
            lblNoteLowerCase.Text = "NOTE: Everything must be in lower case";
            // 
            // lblNewUserAcntCreation
            // 
            lblNewUserAcntCreation.AutoSize = true;
            lblNewUserAcntCreation.Font = new Font("Segoe UI", 15F);
            lblNewUserAcntCreation.Location = new Point(125, 16);
            lblNewUserAcntCreation.Name = "lblNewUserAcntCreation";
            lblNewUserAcntCreation.Size = new Size(251, 28);
            lblNewUserAcntCreation.TabIndex = 4;
            lblNewUserAcntCreation.Text = "New User Account Creation";
            // 
            // tabRemoteTools
            // 
            tabRemoteTools.Location = new Point(4, 24);
            tabRemoteTools.Name = "tabRemoteTools";
            tabRemoteTools.Padding = new Padding(3);
            tabRemoteTools.Size = new Size(1651, 847);
            tabRemoteTools.TabIndex = 3;
            tabRemoteTools.Text = "Remote Tools";
            tabRemoteTools.UseVisualStyleBackColor = true;
            // 
            // tabWindowsTools
            // 
            tabWindowsTools.Location = new Point(4, 24);
            tabWindowsTools.Name = "tabWindowsTools";
            tabWindowsTools.Padding = new Padding(3);
            tabWindowsTools.Size = new Size(1651, 847);
            tabWindowsTools.TabIndex = 4;
            tabWindowsTools.Text = "Windows Tools";
            tabWindowsTools.UseVisualStyleBackColor = true;
            // 
            // tabLinuxTools
            // 
            tabLinuxTools.Controls.Add(btnExportLogs);
            tabLinuxTools.Controls.Add(btnClearLogs);
            tabLinuxTools.Controls.Add(btnFetchLogs);
            tabLinuxTools.Controls.Add(rtbLogOutput);
            tabLinuxTools.Controls.Add(lblLogStatusResults);
            tabLinuxTools.Controls.Add(lblLogStatus);
            tabLinuxTools.Controls.Add(cbxServerSelection);
            tabLinuxTools.Controls.Add(lblServerSelection);
            tabLinuxTools.Controls.Add(cbxLogSourceSelection);
            tabLinuxTools.Controls.Add(label1);
            tabLinuxTools.Controls.Add(cbxCaseSensitive);
            tabLinuxTools.Controls.Add(txbLogKeyword);
            tabLinuxTools.Controls.Add(lblKeywordSearch);
            tabLinuxTools.Controls.Add(lblPrioritySeverityLevel);
            tabLinuxTools.Controls.Add(cmbLogPriority);
            tabLinuxTools.Controls.Add(chkLastHourOnly);
            tabLinuxTools.Controls.Add(lblLogEndDate);
            tabLinuxTools.Controls.Add(lblLogStartDate);
            tabLinuxTools.Controls.Add(dtpLogEndDate);
            tabLinuxTools.Controls.Add(dtpLogStartDate);
            tabLinuxTools.Location = new Point(4, 24);
            tabLinuxTools.Name = "tabLinuxTools";
            tabLinuxTools.Padding = new Padding(3);
            tabLinuxTools.Size = new Size(1651, 847);
            tabLinuxTools.TabIndex = 5;
            tabLinuxTools.Text = "Linux Tools";
            tabLinuxTools.UseVisualStyleBackColor = true;
            // 
            // tabVMwareTools
            // 
            tabVMwareTools.Location = new Point(4, 24);
            tabVMwareTools.Name = "tabVMwareTools";
            tabVMwareTools.Padding = new Padding(3);
            tabVMwareTools.Size = new Size(1651, 847);
            tabVMwareTools.TabIndex = 6;
            tabVMwareTools.Text = "VMware Tools";
            tabVMwareTools.UseVisualStyleBackColor = true;
            // 
            // tabOnlineOffline
            // 
            tabOnlineOffline.Controls.Add(lbxCriticalLinux);
            tabOnlineOffline.Controls.Add(lbxLinux);
            tabOnlineOffline.Controls.Add(lblCriticalLinux);
            tabOnlineOffline.Controls.Add(lblLinux);
            tabOnlineOffline.Controls.Add(btnOnOffline);
            tabOnlineOffline.Controls.Add(lbxOfficeExempt);
            tabOnlineOffline.Controls.Add(lbxCriticalWindows);
            tabOnlineOffline.Controls.Add(lbxCriticalNas);
            tabOnlineOffline.Controls.Add(lbxWindowsServers);
            tabOnlineOffline.Controls.Add(lblCriticalNAS);
            tabOnlineOffline.Controls.Add(lblOfficeExempt);
            tabOnlineOffline.Controls.Add(lblCriticalWindows);
            tabOnlineOffline.Controls.Add(lblWorkstations);
            tabOnlineOffline.Controls.Add(dgvWorkstations);
            tabOnlineOffline.Controls.Add(lblPatriotPark);
            tabOnlineOffline.Controls.Add(dgvPatriotPark);
            tabOnlineOffline.Controls.Add(lblWindowsServers);
            tabOnlineOffline.Location = new Point(4, 24);
            tabOnlineOffline.Name = "tabOnlineOffline";
            tabOnlineOffline.Padding = new Padding(3);
            tabOnlineOffline.Size = new Size(1651, 847);
            tabOnlineOffline.TabIndex = 7;
            tabOnlineOffline.Text = "Online/Offline";
            tabOnlineOffline.UseVisualStyleBackColor = true;
            // 
            // lbxCriticalLinux
            // 
            lbxCriticalLinux.FormattingEnabled = true;
            lbxCriticalLinux.ItemHeight = 15;
            lbxCriticalLinux.Location = new Point(1110, 473);
            lbxCriticalLinux.Name = "lbxCriticalLinux";
            lbxCriticalLinux.Size = new Size(189, 109);
            lbxCriticalLinux.TabIndex = 113;
            // 
            // lbxLinux
            // 
            lbxLinux.FormattingEnabled = true;
            lbxLinux.ItemHeight = 15;
            lbxLinux.Location = new Point(1321, 45);
            lbxLinux.Name = "lbxLinux";
            lbxLinux.Size = new Size(150, 394);
            lbxLinux.TabIndex = 112;
            // 
            // lblCriticalLinux
            // 
            lblCriticalLinux.AutoSize = true;
            lblCriticalLinux.Font = new Font("Segoe UI", 15F);
            lblCriticalLinux.Location = new Point(1147, 442);
            lblCriticalLinux.Name = "lblCriticalLinux";
            lblCriticalLinux.Size = new Size(122, 28);
            lblCriticalLinux.TabIndex = 111;
            lblCriticalLinux.Text = "Critical Linux";
            // 
            // lblLinux
            // 
            lblLinux.AutoSize = true;
            lblLinux.Font = new Font("Segoe UI", 15F);
            lblLinux.Location = new Point(1369, 14);
            lblLinux.Name = "lblLinux";
            lblLinux.Size = new Size(57, 28);
            lblLinux.TabIndex = 110;
            lblLinux.Text = "Linux";
            // 
            // btnOnOffline
            // 
            btnOnOffline.Font = new Font("Segoe UI", 12F);
            btnOnOffline.Location = new Point(1079, 608);
            btnOnOffline.Name = "btnOnOffline";
            btnOnOffline.Size = new Size(239, 64);
            btnOnOffline.TabIndex = 53;
            btnOnOffline.Text = "ReCheck Online/Offline Status";
            btnOnOffline.UseVisualStyleBackColor = true;
            btnOnOffline.Click += btnOnOffline_Click;
            // 
            // lbxOfficeExempt
            // 
            lbxOfficeExempt.FormattingEnabled = true;
            lbxOfficeExempt.ItemHeight = 15;
            lbxOfficeExempt.Location = new Point(910, 418);
            lbxOfficeExempt.Name = "lbxOfficeExempt";
            lbxOfficeExempt.Size = new Size(187, 139);
            lbxOfficeExempt.TabIndex = 50;
            // 
            // lbxCriticalWindows
            // 
            lbxCriticalWindows.FormattingEnabled = true;
            lbxCriticalWindows.ItemHeight = 15;
            lbxCriticalWindows.Location = new Point(910, 222);
            lbxCriticalWindows.Name = "lbxCriticalWindows";
            lbxCriticalWindows.Size = new Size(187, 139);
            lbxCriticalWindows.TabIndex = 49;
            // 
            // lbxCriticalNas
            // 
            lbxCriticalNas.FormattingEnabled = true;
            lbxCriticalNas.ItemHeight = 15;
            lbxCriticalNas.Location = new Point(910, 45);
            lbxCriticalNas.Name = "lbxCriticalNas";
            lbxCriticalNas.Size = new Size(187, 139);
            lbxCriticalNas.TabIndex = 47;
            // 
            // lbxWindowsServers
            // 
            lbxWindowsServers.FormattingEnabled = true;
            lbxWindowsServers.ItemHeight = 15;
            lbxWindowsServers.Location = new Point(1113, 45);
            lbxWindowsServers.Name = "lbxWindowsServers";
            lbxWindowsServers.Size = new Size(187, 394);
            lbxWindowsServers.TabIndex = 46;
            // 
            // lblCriticalNAS
            // 
            lblCriticalNAS.AutoSize = true;
            lblCriticalNAS.Font = new Font("Segoe UI", 15F);
            lblCriticalNAS.Location = new Point(934, 14);
            lblCriticalNAS.Name = "lblCriticalNAS";
            lblCriticalNAS.Size = new Size(116, 28);
            lblCriticalNAS.TabIndex = 45;
            lblCriticalNAS.Text = "Critical NAS";
            // 
            // lblOfficeExempt
            // 
            lblOfficeExempt.AutoSize = true;
            lblOfficeExempt.Font = new Font("Segoe UI", 15F);
            lblOfficeExempt.Location = new Point(934, 384);
            lblOfficeExempt.Name = "lblOfficeExempt";
            lblOfficeExempt.Size = new Size(133, 28);
            lblOfficeExempt.TabIndex = 44;
            lblOfficeExempt.Text = "Office Exempt";
            // 
            // lblCriticalWindows
            // 
            lblCriticalWindows.AutoSize = true;
            lblCriticalWindows.Font = new Font("Segoe UI", 14F);
            lblCriticalWindows.Location = new Point(925, 191);
            lblCriticalWindows.Name = "lblCriticalWindows";
            lblCriticalWindows.Size = new Size(154, 25);
            lblCriticalWindows.TabIndex = 41;
            lblCriticalWindows.Text = "Critical Windows";
            // 
            // lblWorkstations
            // 
            lblWorkstations.AutoSize = true;
            lblWorkstations.Font = new Font("Segoe UI", 15F);
            lblWorkstations.Location = new Point(144, 14);
            lblWorkstations.Name = "lblWorkstations";
            lblWorkstations.Size = new Size(129, 28);
            lblWorkstations.TabIndex = 40;
            lblWorkstations.Text = "WorkStations";
            // 
            // dgvWorkstations
            // 
            dgvWorkstations.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvWorkstations.Columns.AddRange(new DataGridViewColumn[] { clmWksComputerName, clmWksUserName, clmWksLocation });
            dgvWorkstations.Location = new Point(8, 45);
            dgvWorkstations.Name = "dgvWorkstations";
            dgvWorkstations.Size = new Size(435, 723);
            dgvWorkstations.TabIndex = 39;
            // 
            // clmWksComputerName
            // 
            clmWksComputerName.HeaderText = "Computer Name";
            clmWksComputerName.Name = "clmWksComputerName";
            clmWksComputerName.Width = 130;
            // 
            // clmWksUserName
            // 
            clmWksUserName.HeaderText = "User Name";
            clmWksUserName.Name = "clmWksUserName";
            clmWksUserName.Width = 130;
            // 
            // clmWksLocation
            // 
            clmWksLocation.HeaderText = "Location";
            clmWksLocation.Name = "clmWksLocation";
            clmWksLocation.Width = 130;
            // 
            // lblPatriotPark
            // 
            lblPatriotPark.AutoSize = true;
            lblPatriotPark.Font = new Font("Segoe UI", 15F);
            lblPatriotPark.Location = new Point(604, 14);
            lblPatriotPark.Name = "lblPatriotPark";
            lblPatriotPark.Size = new Size(112, 28);
            lblPatriotPark.TabIndex = 38;
            lblPatriotPark.Text = "Patriot Park";
            // 
            // dgvPatriotPark
            // 
            dgvPatriotPark.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPatriotPark.Columns.AddRange(new DataGridViewColumn[] { dgvPpComputerName, clmPpUserName, clmPpLocation });
            dgvPatriotPark.Location = new Point(459, 45);
            dgvPatriotPark.Name = "dgvPatriotPark";
            dgvPatriotPark.Size = new Size(435, 576);
            dgvPatriotPark.TabIndex = 37;
            // 
            // dgvPpComputerName
            // 
            dgvPpComputerName.HeaderText = "Computer Name";
            dgvPpComputerName.Name = "dgvPpComputerName";
            dgvPpComputerName.Width = 130;
            // 
            // clmPpUserName
            // 
            clmPpUserName.HeaderText = "User Name";
            clmPpUserName.Name = "clmPpUserName";
            clmPpUserName.Width = 130;
            // 
            // clmPpLocation
            // 
            clmPpLocation.HeaderText = "Location";
            clmPpLocation.Name = "clmPpLocation";
            clmPpLocation.Width = 130;
            // 
            // lblWindowsServers
            // 
            lblWindowsServers.AutoSize = true;
            lblWindowsServers.Font = new Font("Segoe UI", 15F);
            lblWindowsServers.Location = new Point(1128, 14);
            lblWindowsServers.Name = "lblWindowsServers";
            lblWindowsServers.Size = new Size(161, 28);
            lblWindowsServers.TabIndex = 35;
            lblWindowsServers.Text = "Windows Servers";
            // 
            // tabSAPMIsSpice
            // 
            tabSAPMIsSpice.Controls.Add(btnPerformHealthChk);
            tabSAPMIsSpice.Controls.Add(btnCheckFileSystem);
            tabSAPMIsSpice.Controls.Add(gbxLDAPReplicationChk);
            tabSAPMIsSpice.Controls.Add(dgvEsxiVmHealthChk);
            tabSAPMIsSpice.Controls.Add(TcFileSystemCheck);
            tabSAPMIsSpice.Controls.Add(lblEsxiAndVmPmi);
            tabSAPMIsSpice.Controls.Add(lblFileSystemCheckPmi);
            tabSAPMIsSpice.Location = new Point(4, 24);
            tabSAPMIsSpice.Name = "tabSAPMIsSpice";
            tabSAPMIsSpice.Padding = new Padding(3);
            tabSAPMIsSpice.Size = new Size(1651, 847);
            tabSAPMIsSpice.TabIndex = 8;
            tabSAPMIsSpice.Text = "SA PMIs SPICE";
            tabSAPMIsSpice.UseVisualStyleBackColor = true;
            // 
            // btnPerformHealthChk
            // 
            btnPerformHealthChk.Font = new Font("Segoe UI", 11F);
            btnPerformHealthChk.Location = new Point(1150, 506);
            btnPerformHealthChk.Name = "btnPerformHealthChk";
            btnPerformHealthChk.Size = new Size(174, 31);
            btnPerformHealthChk.TabIndex = 6;
            btnPerformHealthChk.Text = "Perform Health Check";
            btnPerformHealthChk.UseVisualStyleBackColor = true;
            btnPerformHealthChk.Click += btnPerformHealthChk_Click;
            // 
            // btnCheckFileSystem
            // 
            btnCheckFileSystem.Font = new Font("Segoe UI", 11F);
            btnCheckFileSystem.Location = new Point(296, 506);
            btnCheckFileSystem.Name = "btnCheckFileSystem";
            btnCheckFileSystem.Size = new Size(174, 31);
            btnCheckFileSystem.TabIndex = 5;
            btnCheckFileSystem.Text = "Check Filesystem";
            btnCheckFileSystem.UseVisualStyleBackColor = true;
            btnCheckFileSystem.Click += btnCheckFileSystem_Click;
            // 
            // gbxLDAPReplicationChk
            // 
            gbxLDAPReplicationChk.Controls.Add(lblMaxCSNSa2);
            gbxLDAPReplicationChk.Controls.Add(lblMaxCSNDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaIDSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationStatusDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaIDDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaRootSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaStatusSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaStatusDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaRootDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblMaxCSNSa1);
            gbxLDAPReplicationChk.Controls.Add(lblMaxCSNDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaIDSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaIDDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaRootSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaStatusSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaStatusDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaRootDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitEndDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStatusDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStartDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReapActiveDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationLagTimeDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblTargetCcesa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaEnabledSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationStatusDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStartSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitEndSa1);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSkippedDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateStatusSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStatusSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaEnabledDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblStatusForAgreementSa1);
            gbxLDAPReplicationChk.Controls.Add(lblUpdateInProgressSa1);
            gbxLDAPReplicationChk.Controls.Add(lblUpdateInProgressDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblStatusForAgreementDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSentDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateStartSa1);
            gbxLDAPReplicationChk.Controls.Add(txbLastUpdateStatusDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSkippedSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateEndDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateEndSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationLagTimeSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReapActiveSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateStartDataSa1);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSentSa1);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationStatusSa1);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitEndSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitEndDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStatusSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStatusDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStartSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastInitStartDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReapActiveSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReapActiveDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationLagTimeSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationLagTimeDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateStatusSa2);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSkippedSa2);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSkippedDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaEnabledSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicaEnabledDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblUpdateInProgressSa2);
            gbxLDAPReplicationChk.Controls.Add(lblUpdateInProgressDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblReplicationStatusSa2);
            gbxLDAPReplicationChk.Controls.Add(txbLastUpdateStatusDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblStatusForAgreementDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblStatusForAgreementSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateStartSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateStartDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateEndSa2);
            gbxLDAPReplicationChk.Controls.Add(lblLastUpdateEndDataSa2);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSentSa2);
            gbxLDAPReplicationChk.Controls.Add(lblChangesSentDataSa2);
            gbxLDAPReplicationChk.Controls.Add(btnCheckRepHealth);
            gbxLDAPReplicationChk.Controls.Add(lblTargetCceSa2);
            gbxLDAPReplicationChk.Location = new Point(28, 543);
            gbxLDAPReplicationChk.Name = "gbxLDAPReplicationChk";
            gbxLDAPReplicationChk.Size = new Size(1581, 298);
            gbxLDAPReplicationChk.TabIndex = 4;
            gbxLDAPReplicationChk.TabStop = false;
            gbxLDAPReplicationChk.Text = "LDAP Replication Check";
            // 
            // lblMaxCSNSa2
            // 
            lblMaxCSNSa2.AutoSize = true;
            lblMaxCSNSa2.Location = new Point(980, 145);
            lblMaxCSNSa2.Name = "lblMaxCSNSa2";
            lblMaxCSNSa2.Size = new Size(59, 15);
            lblMaxCSNSa2.TabIndex = 53;
            lblMaxCSNSa2.Text = "Max CSN:";
            // 
            // lblMaxCSNDataSa2
            // 
            lblMaxCSNDataSa2.AutoSize = true;
            lblMaxCSNDataSa2.Location = new Point(1058, 145);
            lblMaxCSNDataSa2.Name = "lblMaxCSNDataSa2";
            lblMaxCSNDataSa2.Size = new Size(58, 15);
            lblMaxCSNDataSa2.TabIndex = 54;
            lblMaxCSNDataSa2.Text = "Unknown";
            // 
            // lblReplicaIDSa2
            // 
            lblReplicaIDSa2.AutoSize = true;
            lblReplicaIDSa2.Location = new Point(977, 95);
            lblReplicaIDSa2.Name = "lblReplicaIDSa2";
            lblReplicaIDSa2.Size = new Size(62, 15);
            lblReplicaIDSa2.TabIndex = 48;
            lblReplicaIDSa2.Text = "Replica ID:";
            // 
            // lblReplicationStatusDataSa2
            // 
            lblReplicationStatusDataSa2.AutoSize = true;
            lblReplicationStatusDataSa2.Location = new Point(1058, 270);
            lblReplicationStatusDataSa2.Name = "lblReplicationStatusDataSa2";
            lblReplicationStatusDataSa2.Size = new Size(58, 15);
            lblReplicationStatusDataSa2.TabIndex = 33;
            lblReplicationStatusDataSa2.Text = "Unknown";
            // 
            // lblReplicaIDDataSa2
            // 
            lblReplicaIDDataSa2.AutoSize = true;
            lblReplicaIDDataSa2.Location = new Point(1058, 95);
            lblReplicaIDDataSa2.Name = "lblReplicaIDDataSa2";
            lblReplicaIDDataSa2.Size = new Size(58, 15);
            lblReplicaIDDataSa2.TabIndex = 52;
            lblReplicaIDDataSa2.Text = "Unknown";
            // 
            // lblReplicaRootSa2
            // 
            lblReplicaRootSa2.AutoSize = true;
            lblReplicaRootSa2.Location = new Point(963, 70);
            lblReplicaRootSa2.Name = "lblReplicaRootSa2";
            lblReplicaRootSa2.Size = new Size(76, 15);
            lblReplicaRootSa2.TabIndex = 49;
            lblReplicaRootSa2.Text = "Replica Root:";
            // 
            // lblReplicaStatusSa2
            // 
            lblReplicaStatusSa2.AutoSize = true;
            lblReplicaStatusSa2.Location = new Point(956, 120);
            lblReplicaStatusSa2.Name = "lblReplicaStatusSa2";
            lblReplicaStatusSa2.Size = new Size(83, 15);
            lblReplicaStatusSa2.TabIndex = 47;
            lblReplicaStatusSa2.Text = "Replica Status:";
            // 
            // lblReplicaStatusDataSa2
            // 
            lblReplicaStatusDataSa2.AutoSize = true;
            lblReplicaStatusDataSa2.Location = new Point(1058, 120);
            lblReplicaStatusDataSa2.Name = "lblReplicaStatusDataSa2";
            lblReplicaStatusDataSa2.Size = new Size(58, 15);
            lblReplicaStatusDataSa2.TabIndex = 51;
            lblReplicaStatusDataSa2.Text = "Unknown";
            // 
            // lblReplicaRootDataSa2
            // 
            lblReplicaRootDataSa2.AutoSize = true;
            lblReplicaRootDataSa2.Location = new Point(1058, 70);
            lblReplicaRootDataSa2.Name = "lblReplicaRootDataSa2";
            lblReplicaRootDataSa2.Size = new Size(58, 15);
            lblReplicaRootDataSa2.TabIndex = 50;
            lblReplicaRootDataSa2.Text = "Unknown";
            // 
            // lblMaxCSNSa1
            // 
            lblMaxCSNSa1.AutoSize = true;
            lblMaxCSNSa1.Location = new Point(85, 145);
            lblMaxCSNSa1.Name = "lblMaxCSNSa1";
            lblMaxCSNSa1.Size = new Size(59, 15);
            lblMaxCSNSa1.TabIndex = 45;
            lblMaxCSNSa1.Text = "Max CSN:";
            // 
            // lblMaxCSNDataSa1
            // 
            lblMaxCSNDataSa1.AutoSize = true;
            lblMaxCSNDataSa1.Location = new Point(147, 145);
            lblMaxCSNDataSa1.Name = "lblMaxCSNDataSa1";
            lblMaxCSNDataSa1.Size = new Size(58, 15);
            lblMaxCSNDataSa1.TabIndex = 46;
            lblMaxCSNDataSa1.Text = "Unknown";
            // 
            // lblReplicaIDSa1
            // 
            lblReplicaIDSa1.AutoSize = true;
            lblReplicaIDSa1.Location = new Point(82, 95);
            lblReplicaIDSa1.Name = "lblReplicaIDSa1";
            lblReplicaIDSa1.Size = new Size(62, 15);
            lblReplicaIDSa1.TabIndex = 40;
            lblReplicaIDSa1.Text = "Replica ID:";
            // 
            // lblReplicaIDDataSa1
            // 
            lblReplicaIDDataSa1.AutoSize = true;
            lblReplicaIDDataSa1.Location = new Point(147, 95);
            lblReplicaIDDataSa1.Name = "lblReplicaIDDataSa1";
            lblReplicaIDDataSa1.Size = new Size(58, 15);
            lblReplicaIDDataSa1.TabIndex = 44;
            lblReplicaIDDataSa1.Text = "Unknown";
            // 
            // lblReplicaRootSa1
            // 
            lblReplicaRootSa1.AutoSize = true;
            lblReplicaRootSa1.Location = new Point(68, 70);
            lblReplicaRootSa1.Name = "lblReplicaRootSa1";
            lblReplicaRootSa1.Size = new Size(76, 15);
            lblReplicaRootSa1.TabIndex = 41;
            lblReplicaRootSa1.Text = "Replica Root:";
            // 
            // lblReplicaStatusSa1
            // 
            lblReplicaStatusSa1.AutoSize = true;
            lblReplicaStatusSa1.Location = new Point(61, 120);
            lblReplicaStatusSa1.Name = "lblReplicaStatusSa1";
            lblReplicaStatusSa1.Size = new Size(83, 15);
            lblReplicaStatusSa1.TabIndex = 39;
            lblReplicaStatusSa1.Text = "Replica Status:";
            // 
            // lblReplicaStatusDataSa1
            // 
            lblReplicaStatusDataSa1.AutoSize = true;
            lblReplicaStatusDataSa1.Location = new Point(150, 120);
            lblReplicaStatusDataSa1.Name = "lblReplicaStatusDataSa1";
            lblReplicaStatusDataSa1.Size = new Size(58, 15);
            lblReplicaStatusDataSa1.TabIndex = 43;
            lblReplicaStatusDataSa1.Text = "Unknown";
            // 
            // lblReplicaRootDataSa1
            // 
            lblReplicaRootDataSa1.AutoSize = true;
            lblReplicaRootDataSa1.Location = new Point(145, 70);
            lblReplicaRootDataSa1.Name = "lblReplicaRootDataSa1";
            lblReplicaRootDataSa1.Size = new Size(58, 15);
            lblReplicaRootDataSa1.TabIndex = 42;
            lblReplicaRootDataSa1.Text = "Unknown";
            // 
            // lblLastInitEndDataSa1
            // 
            lblLastInitEndDataSa1.AutoSize = true;
            lblLastInitEndDataSa1.Location = new Point(485, 193);
            lblLastInitEndDataSa1.Name = "lblLastInitEndDataSa1";
            lblLastInitEndDataSa1.Size = new Size(58, 15);
            lblLastInitEndDataSa1.TabIndex = 38;
            lblLastInitEndDataSa1.Text = "Unknown";
            // 
            // lblLastInitStatusDataSa1
            // 
            lblLastInitStatusDataSa1.AutoSize = true;
            lblLastInitStatusDataSa1.Location = new Point(485, 218);
            lblLastInitStatusDataSa1.Name = "lblLastInitStatusDataSa1";
            lblLastInitStatusDataSa1.Size = new Size(58, 15);
            lblLastInitStatusDataSa1.TabIndex = 37;
            lblLastInitStatusDataSa1.Text = "Unknown";
            // 
            // lblLastInitStartDataSa1
            // 
            lblLastInitStartDataSa1.AutoSize = true;
            lblLastInitStartDataSa1.Location = new Point(485, 168);
            lblLastInitStartDataSa1.Name = "lblLastInitStartDataSa1";
            lblLastInitStartDataSa1.Size = new Size(58, 15);
            lblLastInitStartDataSa1.TabIndex = 36;
            lblLastInitStartDataSa1.Text = "Unknown";
            // 
            // lblReapActiveDataSa1
            // 
            lblReapActiveDataSa1.AutoSize = true;
            lblReapActiveDataSa1.Location = new Point(485, 243);
            lblReapActiveDataSa1.Name = "lblReapActiveDataSa1";
            lblReapActiveDataSa1.Size = new Size(58, 15);
            lblReapActiveDataSa1.TabIndex = 35;
            lblReapActiveDataSa1.Text = "Unknown";
            // 
            // lblReplicationLagTimeDataSa1
            // 
            lblReplicationLagTimeDataSa1.AutoSize = true;
            lblReplicationLagTimeDataSa1.Location = new Point(484, 268);
            lblReplicationLagTimeDataSa1.Name = "lblReplicationLagTimeDataSa1";
            lblReplicationLagTimeDataSa1.Size = new Size(58, 15);
            lblReplicationLagTimeDataSa1.TabIndex = 34;
            lblReplicationLagTimeDataSa1.Text = "Unknown";
            // 
            // lblTargetCcesa1
            // 
            lblTargetCcesa1.AutoSize = true;
            lblTargetCcesa1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTargetCcesa1.Location = new Point(24, 16);
            lblTargetCcesa1.Name = "lblTargetCcesa1";
            lblTargetCcesa1.Size = new Size(68, 21);
            lblTargetCcesa1.TabIndex = 0;
            lblTargetCcesa1.Text = "CCESA1";
            // 
            // lblReplicaEnabledSa1
            // 
            lblReplicaEnabledSa1.AutoSize = true;
            lblReplicaEnabledSa1.Location = new Point(51, 170);
            lblReplicaEnabledSa1.Name = "lblReplicaEnabledSa1";
            lblReplicaEnabledSa1.Size = new Size(93, 15);
            lblReplicaEnabledSa1.TabIndex = 18;
            lblReplicaEnabledSa1.Text = "Replica Enabled:";
            // 
            // lblReplicationStatusDataSa1
            // 
            lblReplicationStatusDataSa1.AutoSize = true;
            lblReplicationStatusDataSa1.Location = new Point(147, 270);
            lblReplicationStatusDataSa1.Name = "lblReplicationStatusDataSa1";
            lblReplicationStatusDataSa1.Size = new Size(58, 15);
            lblReplicationStatusDataSa1.TabIndex = 33;
            lblReplicationStatusDataSa1.Text = "Unknown";
            // 
            // lblLastInitStartSa1
            // 
            lblLastInitStartSa1.AutoSize = true;
            lblLastInitStartSa1.Location = new Point(405, 168);
            lblLastInitStartSa1.Name = "lblLastInitStartSa1";
            lblLastInitStartSa1.Size = new Size(78, 15);
            lblLastInitStartSa1.TabIndex = 19;
            lblLastInitStartSa1.Text = "Last Init Start:";
            // 
            // lblLastInitEndSa1
            // 
            lblLastInitEndSa1.AutoSize = true;
            lblLastInitEndSa1.Location = new Point(407, 193);
            lblLastInitEndSa1.Name = "lblLastInitEndSa1";
            lblLastInitEndSa1.Size = new Size(74, 15);
            lblLastInitEndSa1.TabIndex = 17;
            lblLastInitEndSa1.Text = "Last Init End:";
            // 
            // lblChangesSkippedDataSa1
            // 
            lblChangesSkippedDataSa1.AutoSize = true;
            lblChangesSkippedDataSa1.Location = new Point(484, 95);
            lblChangesSkippedDataSa1.Name = "lblChangesSkippedDataSa1";
            lblChangesSkippedDataSa1.Size = new Size(58, 15);
            lblChangesSkippedDataSa1.TabIndex = 32;
            lblChangesSkippedDataSa1.Text = "Unknown";
            // 
            // lblLastUpdateStatusSa1
            // 
            lblLastUpdateStatusSa1.AutoSize = true;
            lblLastUpdateStatusSa1.Location = new Point(375, 120);
            lblLastUpdateStatusSa1.Name = "lblLastUpdateStatusSa1";
            lblLastUpdateStatusSa1.Size = new Size(107, 15);
            lblLastUpdateStatusSa1.TabIndex = 20;
            lblLastUpdateStatusSa1.Text = "Last Update Status:";
            // 
            // lblLastInitStatusSa1
            // 
            lblLastInitStatusSa1.AutoSize = true;
            lblLastInitStatusSa1.Location = new Point(393, 218);
            lblLastInitStatusSa1.Name = "lblLastInitStatusSa1";
            lblLastInitStatusSa1.Size = new Size(86, 15);
            lblLastInitStatusSa1.TabIndex = 16;
            lblLastInitStatusSa1.Text = "Last Init Status:";
            // 
            // lblReplicaEnabledDataSa1
            // 
            lblReplicaEnabledDataSa1.AutoSize = true;
            lblReplicaEnabledDataSa1.Location = new Point(147, 170);
            lblReplicaEnabledDataSa1.Name = "lblReplicaEnabledDataSa1";
            lblReplicaEnabledDataSa1.Size = new Size(58, 15);
            lblReplicaEnabledDataSa1.TabIndex = 31;
            lblReplicaEnabledDataSa1.Text = "Unknown";
            // 
            // lblStatusForAgreementSa1
            // 
            lblStatusForAgreementSa1.AutoSize = true;
            lblStatusForAgreementSa1.Location = new Point(20, 45);
            lblStatusForAgreementSa1.Name = "lblStatusForAgreementSa1";
            lblStatusForAgreementSa1.Size = new Size(124, 15);
            lblStatusForAgreementSa1.TabIndex = 21;
            lblStatusForAgreementSa1.Text = "Status For Agreement:";
            // 
            // lblUpdateInProgressSa1
            // 
            lblUpdateInProgressSa1.AutoSize = true;
            lblUpdateInProgressSa1.Location = new Point(35, 195);
            lblUpdateInProgressSa1.Name = "lblUpdateInProgressSa1";
            lblUpdateInProgressSa1.Size = new Size(109, 15);
            lblUpdateInProgressSa1.TabIndex = 15;
            lblUpdateInProgressSa1.Text = "Update In Progress:";
            // 
            // lblUpdateInProgressDataSa1
            // 
            lblUpdateInProgressDataSa1.AutoSize = true;
            lblUpdateInProgressDataSa1.Location = new Point(147, 195);
            lblUpdateInProgressDataSa1.Name = "lblUpdateInProgressDataSa1";
            lblUpdateInProgressDataSa1.Size = new Size(58, 15);
            lblUpdateInProgressDataSa1.TabIndex = 30;
            lblUpdateInProgressDataSa1.Text = "Unknown";
            // 
            // lblStatusForAgreementDataSa1
            // 
            lblStatusForAgreementDataSa1.AutoSize = true;
            lblStatusForAgreementDataSa1.Location = new Point(147, 45);
            lblStatusForAgreementDataSa1.Name = "lblStatusForAgreementDataSa1";
            lblStatusForAgreementDataSa1.Size = new Size(58, 15);
            lblStatusForAgreementDataSa1.TabIndex = 22;
            lblStatusForAgreementDataSa1.Text = "Unknown";
            // 
            // lblChangesSentDataSa1
            // 
            lblChangesSentDataSa1.AutoSize = true;
            lblChangesSentDataSa1.Location = new Point(484, 70);
            lblChangesSentDataSa1.Name = "lblChangesSentDataSa1";
            lblChangesSentDataSa1.Size = new Size(58, 15);
            lblChangesSentDataSa1.TabIndex = 1;
            lblChangesSentDataSa1.Text = "Unknown";
            // 
            // lblLastUpdateStartSa1
            // 
            lblLastUpdateStartSa1.AutoSize = true;
            lblLastUpdateStartSa1.Location = new Point(51, 220);
            lblLastUpdateStartSa1.Name = "lblLastUpdateStartSa1";
            lblLastUpdateStartSa1.Size = new Size(99, 15);
            lblLastUpdateStartSa1.TabIndex = 6;
            lblLastUpdateStartSa1.Text = "Last Update Start:";
            // 
            // txbLastUpdateStatusDataSa1
            // 
            txbLastUpdateStatusDataSa1.AllowDrop = true;
            txbLastUpdateStatusDataSa1.Location = new Point(484, 114);
            txbLastUpdateStatusDataSa1.Name = "txbLastUpdateStatusDataSa1";
            txbLastUpdateStatusDataSa1.Size = new Size(261, 46);
            txbLastUpdateStatusDataSa1.TabIndex = 28;
            txbLastUpdateStatusDataSa1.Text = "Unknown";
            txbLastUpdateStatusDataSa1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblChangesSkippedSa1
            // 
            lblChangesSkippedSa1.AutoSize = true;
            lblChangesSkippedSa1.Location = new Point(355, 95);
            lblChangesSkippedSa1.Name = "lblChangesSkippedSa1";
            lblChangesSkippedSa1.Size = new Size(127, 15);
            lblChangesSkippedSa1.TabIndex = 23;
            lblChangesSkippedSa1.Text = "# Of Changes Skipped:";
            // 
            // lblLastUpdateEndDataSa1
            // 
            lblLastUpdateEndDataSa1.AutoSize = true;
            lblLastUpdateEndDataSa1.Location = new Point(145, 245);
            lblLastUpdateEndDataSa1.Name = "lblLastUpdateEndDataSa1";
            lblLastUpdateEndDataSa1.Size = new Size(58, 15);
            lblLastUpdateEndDataSa1.TabIndex = 2;
            lblLastUpdateEndDataSa1.Text = "Unknown";
            // 
            // lblLastUpdateEndSa1
            // 
            lblLastUpdateEndSa1.AutoSize = true;
            lblLastUpdateEndSa1.Location = new Point(49, 245);
            lblLastUpdateEndSa1.Name = "lblLastUpdateEndSa1";
            lblLastUpdateEndSa1.Size = new Size(95, 15);
            lblLastUpdateEndSa1.TabIndex = 5;
            lblLastUpdateEndSa1.Text = "Last Update End:";
            // 
            // lblReplicationLagTimeSa1
            // 
            lblReplicationLagTimeSa1.AutoSize = true;
            lblReplicationLagTimeSa1.Location = new Point(363, 268);
            lblReplicationLagTimeSa1.Name = "lblReplicationLagTimeSa1";
            lblReplicationLagTimeSa1.Size = new Size(120, 15);
            lblReplicationLagTimeSa1.TabIndex = 26;
            lblReplicationLagTimeSa1.Text = "Replication Lag Time:";
            // 
            // lblReapActiveSa1
            // 
            lblReapActiveSa1.AutoSize = true;
            lblReapActiveSa1.Location = new Point(405, 243);
            lblReapActiveSa1.Name = "lblReapActiveSa1";
            lblReapActiveSa1.Size = new Size(72, 15);
            lblReapActiveSa1.TabIndex = 24;
            lblReapActiveSa1.Text = "Reap Active:";
            // 
            // lblLastUpdateStartDataSa1
            // 
            lblLastUpdateStartDataSa1.AutoSize = true;
            lblLastUpdateStartDataSa1.Location = new Point(147, 220);
            lblLastUpdateStartDataSa1.Name = "lblLastUpdateStartDataSa1";
            lblLastUpdateStartDataSa1.Size = new Size(58, 15);
            lblLastUpdateStartDataSa1.TabIndex = 3;
            lblLastUpdateStartDataSa1.Text = "Unknown";
            // 
            // lblChangesSentSa1
            // 
            lblChangesSentSa1.AutoSize = true;
            lblChangesSentSa1.Location = new Point(375, 70);
            lblChangesSentSa1.Name = "lblChangesSentSa1";
            lblChangesSentSa1.Size = new Size(108, 15);
            lblChangesSentSa1.TabIndex = 4;
            lblChangesSentSa1.Text = "# Of Changes Sent:";
            // 
            // lblReplicationStatusSa1
            // 
            lblReplicationStatusSa1.AutoSize = true;
            lblReplicationStatusSa1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblReplicationStatusSa1.ForeColor = Color.Red;
            lblReplicationStatusSa1.Location = new Point(29, 270);
            lblReplicationStatusSa1.Name = "lblReplicationStatusSa1";
            lblReplicationStatusSa1.Size = new Size(110, 15);
            lblReplicationStatusSa1.TabIndex = 25;
            lblReplicationStatusSa1.Text = "Replication Status:";
            // 
            // lblLastInitEndSa2
            // 
            lblLastInitEndSa2.AutoSize = true;
            lblLastInitEndSa2.Location = new Point(1221, 195);
            lblLastInitEndSa2.Name = "lblLastInitEndSa2";
            lblLastInitEndSa2.Size = new Size(74, 15);
            lblLastInitEndSa2.TabIndex = 17;
            lblLastInitEndSa2.Text = "Last Init End:";
            // 
            // lblLastInitEndDataSa2
            // 
            lblLastInitEndDataSa2.AutoSize = true;
            lblLastInitEndDataSa2.Location = new Point(1307, 195);
            lblLastInitEndDataSa2.Name = "lblLastInitEndDataSa2";
            lblLastInitEndDataSa2.Size = new Size(58, 15);
            lblLastInitEndDataSa2.TabIndex = 38;
            lblLastInitEndDataSa2.Text = "Unknown";
            // 
            // lblLastInitStatusSa2
            // 
            lblLastInitStatusSa2.AutoSize = true;
            lblLastInitStatusSa2.Location = new Point(1209, 220);
            lblLastInitStatusSa2.Name = "lblLastInitStatusSa2";
            lblLastInitStatusSa2.Size = new Size(86, 15);
            lblLastInitStatusSa2.TabIndex = 16;
            lblLastInitStatusSa2.Text = "Last Init Status:";
            // 
            // lblLastInitStatusDataSa2
            // 
            lblLastInitStatusDataSa2.AutoSize = true;
            lblLastInitStatusDataSa2.Location = new Point(1307, 220);
            lblLastInitStatusDataSa2.Name = "lblLastInitStatusDataSa2";
            lblLastInitStatusDataSa2.Size = new Size(58, 15);
            lblLastInitStatusDataSa2.TabIndex = 37;
            lblLastInitStatusDataSa2.Text = "Unknown";
            // 
            // lblLastInitStartSa2
            // 
            lblLastInitStartSa2.AutoSize = true;
            lblLastInitStartSa2.Location = new Point(1217, 170);
            lblLastInitStartSa2.Name = "lblLastInitStartSa2";
            lblLastInitStartSa2.Size = new Size(78, 15);
            lblLastInitStartSa2.TabIndex = 19;
            lblLastInitStartSa2.Text = "Last Init Start:";
            // 
            // lblLastInitStartDataSa2
            // 
            lblLastInitStartDataSa2.AutoSize = true;
            lblLastInitStartDataSa2.Location = new Point(1307, 170);
            lblLastInitStartDataSa2.Name = "lblLastInitStartDataSa2";
            lblLastInitStartDataSa2.Size = new Size(58, 15);
            lblLastInitStartDataSa2.TabIndex = 36;
            lblLastInitStartDataSa2.Text = "Unknown";
            // 
            // lblReapActiveSa2
            // 
            lblReapActiveSa2.AutoSize = true;
            lblReapActiveSa2.Location = new Point(1224, 245);
            lblReapActiveSa2.Name = "lblReapActiveSa2";
            lblReapActiveSa2.Size = new Size(72, 15);
            lblReapActiveSa2.TabIndex = 24;
            lblReapActiveSa2.Text = "Reap Active:";
            // 
            // lblReapActiveDataSa2
            // 
            lblReapActiveDataSa2.AutoSize = true;
            lblReapActiveDataSa2.Location = new Point(1307, 245);
            lblReapActiveDataSa2.Name = "lblReapActiveDataSa2";
            lblReapActiveDataSa2.Size = new Size(58, 15);
            lblReapActiveDataSa2.TabIndex = 35;
            lblReapActiveDataSa2.Text = "Unknown";
            // 
            // lblReplicationLagTimeSa2
            // 
            lblReplicationLagTimeSa2.AutoSize = true;
            lblReplicationLagTimeSa2.Location = new Point(1175, 270);
            lblReplicationLagTimeSa2.Name = "lblReplicationLagTimeSa2";
            lblReplicationLagTimeSa2.Size = new Size(120, 15);
            lblReplicationLagTimeSa2.TabIndex = 26;
            lblReplicationLagTimeSa2.Text = "Replication Lag Time:";
            // 
            // lblReplicationLagTimeDataSa2
            // 
            lblReplicationLagTimeDataSa2.AutoSize = true;
            lblReplicationLagTimeDataSa2.Location = new Point(1307, 270);
            lblReplicationLagTimeDataSa2.Name = "lblReplicationLagTimeDataSa2";
            lblReplicationLagTimeDataSa2.Size = new Size(58, 15);
            lblReplicationLagTimeDataSa2.TabIndex = 34;
            lblReplicationLagTimeDataSa2.Text = "Unknown";
            // 
            // lblLastUpdateStatusSa2
            // 
            lblLastUpdateStatusSa2.AutoSize = true;
            lblLastUpdateStatusSa2.Location = new Point(1188, 120);
            lblLastUpdateStatusSa2.Name = "lblLastUpdateStatusSa2";
            lblLastUpdateStatusSa2.Size = new Size(107, 15);
            lblLastUpdateStatusSa2.TabIndex = 20;
            lblLastUpdateStatusSa2.Text = "Last Update Status:";
            // 
            // lblChangesSkippedSa2
            // 
            lblChangesSkippedSa2.AutoSize = true;
            lblChangesSkippedSa2.Location = new Point(1169, 95);
            lblChangesSkippedSa2.Name = "lblChangesSkippedSa2";
            lblChangesSkippedSa2.Size = new Size(127, 15);
            lblChangesSkippedSa2.TabIndex = 23;
            lblChangesSkippedSa2.Text = "# Of Changes Skipped:";
            // 
            // lblChangesSkippedDataSa2
            // 
            lblChangesSkippedDataSa2.AutoSize = true;
            lblChangesSkippedDataSa2.Location = new Point(1306, 95);
            lblChangesSkippedDataSa2.Name = "lblChangesSkippedDataSa2";
            lblChangesSkippedDataSa2.Size = new Size(58, 15);
            lblChangesSkippedDataSa2.TabIndex = 32;
            lblChangesSkippedDataSa2.Text = "Unknown";
            // 
            // lblReplicaEnabledSa2
            // 
            lblReplicaEnabledSa2.AutoSize = true;
            lblReplicaEnabledSa2.Location = new Point(946, 170);
            lblReplicaEnabledSa2.Name = "lblReplicaEnabledSa2";
            lblReplicaEnabledSa2.Size = new Size(93, 15);
            lblReplicaEnabledSa2.TabIndex = 18;
            lblReplicaEnabledSa2.Text = "Replica Enabled:";
            // 
            // lblReplicaEnabledDataSa2
            // 
            lblReplicaEnabledDataSa2.AutoSize = true;
            lblReplicaEnabledDataSa2.Location = new Point(1058, 170);
            lblReplicaEnabledDataSa2.Name = "lblReplicaEnabledDataSa2";
            lblReplicaEnabledDataSa2.Size = new Size(58, 15);
            lblReplicaEnabledDataSa2.TabIndex = 31;
            lblReplicaEnabledDataSa2.Text = "Unknown";
            // 
            // lblUpdateInProgressSa2
            // 
            lblUpdateInProgressSa2.AutoSize = true;
            lblUpdateInProgressSa2.Location = new Point(930, 195);
            lblUpdateInProgressSa2.Name = "lblUpdateInProgressSa2";
            lblUpdateInProgressSa2.Size = new Size(109, 15);
            lblUpdateInProgressSa2.TabIndex = 15;
            lblUpdateInProgressSa2.Text = "Update In Progress:";
            // 
            // lblUpdateInProgressDataSa2
            // 
            lblUpdateInProgressDataSa2.AutoSize = true;
            lblUpdateInProgressDataSa2.Location = new Point(1058, 195);
            lblUpdateInProgressDataSa2.Name = "lblUpdateInProgressDataSa2";
            lblUpdateInProgressDataSa2.Size = new Size(58, 15);
            lblUpdateInProgressDataSa2.TabIndex = 30;
            lblUpdateInProgressDataSa2.Text = "Unknown";
            // 
            // lblReplicationStatusSa2
            // 
            lblReplicationStatusSa2.AutoSize = true;
            lblReplicationStatusSa2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblReplicationStatusSa2.ForeColor = Color.Red;
            lblReplicationStatusSa2.Location = new Point(924, 270);
            lblReplicationStatusSa2.Name = "lblReplicationStatusSa2";
            lblReplicationStatusSa2.Size = new Size(110, 15);
            lblReplicationStatusSa2.TabIndex = 25;
            lblReplicationStatusSa2.Text = "Replication Status:";
            // 
            // txbLastUpdateStatusDataSa2
            // 
            txbLastUpdateStatusDataSa2.AllowDrop = true;
            txbLastUpdateStatusDataSa2.Location = new Point(1307, 114);
            txbLastUpdateStatusDataSa2.Name = "txbLastUpdateStatusDataSa2";
            txbLastUpdateStatusDataSa2.Size = new Size(261, 46);
            txbLastUpdateStatusDataSa2.TabIndex = 28;
            txbLastUpdateStatusDataSa2.Text = "Unknown";
            txbLastUpdateStatusDataSa2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStatusForAgreementDataSa2
            // 
            lblStatusForAgreementDataSa2.AutoSize = true;
            lblStatusForAgreementDataSa2.Location = new Point(1058, 45);
            lblStatusForAgreementDataSa2.Name = "lblStatusForAgreementDataSa2";
            lblStatusForAgreementDataSa2.Size = new Size(58, 15);
            lblStatusForAgreementDataSa2.TabIndex = 22;
            lblStatusForAgreementDataSa2.Text = "Unknown";
            // 
            // lblStatusForAgreementSa2
            // 
            lblStatusForAgreementSa2.AutoSize = true;
            lblStatusForAgreementSa2.Location = new Point(915, 45);
            lblStatusForAgreementSa2.Name = "lblStatusForAgreementSa2";
            lblStatusForAgreementSa2.Size = new Size(124, 15);
            lblStatusForAgreementSa2.TabIndex = 21;
            lblStatusForAgreementSa2.Text = "Status For Agreement:";
            // 
            // lblLastUpdateStartSa2
            // 
            lblLastUpdateStartSa2.AutoSize = true;
            lblLastUpdateStartSa2.Location = new Point(940, 220);
            lblLastUpdateStartSa2.Name = "lblLastUpdateStartSa2";
            lblLastUpdateStartSa2.Size = new Size(99, 15);
            lblLastUpdateStartSa2.TabIndex = 6;
            lblLastUpdateStartSa2.Text = "Last Update Start:";
            // 
            // lblLastUpdateStartDataSa2
            // 
            lblLastUpdateStartDataSa2.AutoSize = true;
            lblLastUpdateStartDataSa2.Location = new Point(1058, 220);
            lblLastUpdateStartDataSa2.Name = "lblLastUpdateStartDataSa2";
            lblLastUpdateStartDataSa2.Size = new Size(58, 15);
            lblLastUpdateStartDataSa2.TabIndex = 3;
            lblLastUpdateStartDataSa2.Text = "Unknown";
            // 
            // lblLastUpdateEndSa2
            // 
            lblLastUpdateEndSa2.AutoSize = true;
            lblLastUpdateEndSa2.Location = new Point(944, 245);
            lblLastUpdateEndSa2.Name = "lblLastUpdateEndSa2";
            lblLastUpdateEndSa2.Size = new Size(95, 15);
            lblLastUpdateEndSa2.TabIndex = 5;
            lblLastUpdateEndSa2.Text = "Last Update End:";
            // 
            // lblLastUpdateEndDataSa2
            // 
            lblLastUpdateEndDataSa2.AutoSize = true;
            lblLastUpdateEndDataSa2.Location = new Point(1058, 245);
            lblLastUpdateEndDataSa2.Name = "lblLastUpdateEndDataSa2";
            lblLastUpdateEndDataSa2.Size = new Size(58, 15);
            lblLastUpdateEndDataSa2.TabIndex = 2;
            lblLastUpdateEndDataSa2.Text = "Unknown";
            // 
            // lblChangesSentSa2
            // 
            lblChangesSentSa2.AutoSize = true;
            lblChangesSentSa2.Location = new Point(1188, 70);
            lblChangesSentSa2.Name = "lblChangesSentSa2";
            lblChangesSentSa2.Size = new Size(108, 15);
            lblChangesSentSa2.TabIndex = 4;
            lblChangesSentSa2.Text = "# Of Changes Sent:";
            // 
            // lblChangesSentDataSa2
            // 
            lblChangesSentDataSa2.AutoSize = true;
            lblChangesSentDataSa2.Location = new Point(1305, 70);
            lblChangesSentDataSa2.Name = "lblChangesSentDataSa2";
            lblChangesSentDataSa2.Size = new Size(58, 15);
            lblChangesSentDataSa2.TabIndex = 1;
            lblChangesSentDataSa2.Text = "Unknown";
            // 
            // btnCheckRepHealth
            // 
            btnCheckRepHealth.Location = new Point(593, 239);
            btnCheckRepHealth.Name = "btnCheckRepHealth";
            btnCheckRepHealth.Size = new Size(187, 44);
            btnCheckRepHealth.TabIndex = 14;
            btnCheckRepHealth.Text = "Check Replication Health";
            btnCheckRepHealth.UseVisualStyleBackColor = true;
            btnCheckRepHealth.Click += btnCheckRepHealth_Click;
            // 
            // lblTargetCceSa2
            // 
            lblTargetCceSa2.AutoSize = true;
            lblTargetCceSa2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTargetCceSa2.Location = new Point(952, 16);
            lblTargetCceSa2.Name = "lblTargetCceSa2";
            lblTargetCceSa2.Size = new Size(68, 21);
            lblTargetCceSa2.TabIndex = 0;
            lblTargetCceSa2.Text = "CCESA2";
            // 
            // dgvEsxiVmHealthChk
            // 
            dgvEsxiVmHealthChk.Controls.Add(tabEsxiHealthPmi);
            dgvEsxiVmHealthChk.Controls.Add(tabVmHealthChkPmi);
            dgvEsxiVmHealthChk.Location = new Point(763, 62);
            dgvEsxiVmHealthChk.Name = "dgvEsxiVmHealthChk";
            dgvEsxiVmHealthChk.SelectedIndex = 0;
            dgvEsxiVmHealthChk.Size = new Size(895, 438);
            dgvEsxiVmHealthChk.TabIndex = 3;
            // 
            // tabEsxiHealthPmi
            // 
            tabEsxiHealthPmi.Controls.Add(dgvEsxiHealthCheck);
            tabEsxiHealthPmi.Location = new Point(4, 24);
            tabEsxiHealthPmi.Name = "tabEsxiHealthPmi";
            tabEsxiHealthPmi.Padding = new Padding(3);
            tabEsxiHealthPmi.Size = new Size(887, 410);
            tabEsxiHealthPmi.TabIndex = 0;
            tabEsxiHealthPmi.Text = "ESXi Health Check";
            tabEsxiHealthPmi.UseVisualStyleBackColor = true;
            // 
            // dgvEsxiHealthCheck
            // 
            dgvEsxiHealthCheck.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEsxiHealthCheck.Columns.AddRange(new DataGridViewColumn[] { clmServerName, clmState, clmStatus, clmCluster, clmConsumedCpu, clmConsumedMemory, clmHaState, clmUptime });
            dgvEsxiHealthCheck.Location = new Point(3, 0);
            dgvEsxiHealthCheck.Name = "dgvEsxiHealthCheck";
            dgvEsxiHealthCheck.Size = new Size(875, 403);
            dgvEsxiHealthCheck.TabIndex = 1;
            // 
            // clmServerName
            // 
            clmServerName.HeaderText = "Server Name";
            clmServerName.Name = "clmServerName";
            // 
            // clmState
            // 
            clmState.HeaderText = "State";
            clmState.Name = "clmState";
            clmState.Width = 80;
            // 
            // clmStatus
            // 
            clmStatus.HeaderText = "Status";
            clmStatus.Name = "clmStatus";
            // 
            // clmCluster
            // 
            clmCluster.HeaderText = "Cluster";
            clmCluster.Name = "clmCluster";
            clmCluster.Width = 90;
            // 
            // clmConsumedCpu
            // 
            clmConsumedCpu.HeaderText = "Consumed CPU %";
            clmConsumedCpu.Name = "clmConsumedCpu";
            clmConsumedCpu.Width = 130;
            // 
            // clmConsumedMemory
            // 
            clmConsumedMemory.HeaderText = "Consumed Memory";
            clmConsumedMemory.Name = "clmConsumedMemory";
            clmConsumedMemory.Width = 140;
            // 
            // clmHaState
            // 
            clmHaState.HeaderText = "HA State";
            clmHaState.Name = "clmHaState";
            clmHaState.Width = 90;
            // 
            // clmUptime
            // 
            clmUptime.HeaderText = "Uptime";
            clmUptime.Name = "clmUptime";
            // 
            // tabVmHealthChkPmi
            // 
            tabVmHealthChkPmi.Controls.Add(dgvVmHealthCheck);
            tabVmHealthChkPmi.Location = new Point(4, 24);
            tabVmHealthChkPmi.Name = "tabVmHealthChkPmi";
            tabVmHealthChkPmi.Padding = new Padding(3);
            tabVmHealthChkPmi.Size = new Size(887, 410);
            tabVmHealthChkPmi.TabIndex = 1;
            tabVmHealthChkPmi.Text = "VM Health Check";
            tabVmHealthChkPmi.UseVisualStyleBackColor = true;
            // 
            // dgvVmHealthCheck
            // 
            dgvVmHealthCheck.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVmHealthCheck.Columns.AddRange(new DataGridViewColumn[] { clmVmName, clmPowerState, clmVmStatus, clmProvisionedSpace, clmUsedSpace, clmHostCpu, clmHostMemory });
            dgvVmHealthCheck.Location = new Point(6, 4);
            dgvVmHealthCheck.Name = "dgvVmHealthCheck";
            dgvVmHealthCheck.Size = new Size(875, 474);
            dgvVmHealthCheck.TabIndex = 2;
            // 
            // clmVmName
            // 
            clmVmName.HeaderText = "VM Name";
            clmVmName.Name = "clmVmName";
            // 
            // clmPowerState
            // 
            clmPowerState.HeaderText = "Power State";
            clmPowerState.Name = "clmPowerState";
            // 
            // clmVmStatus
            // 
            clmVmStatus.HeaderText = "Status";
            clmVmStatus.Name = "clmVmStatus";
            // 
            // clmProvisionedSpace
            // 
            clmProvisionedSpace.HeaderText = "Provisioned Space";
            clmProvisionedSpace.Name = "clmProvisionedSpace";
            clmProvisionedSpace.Width = 130;
            // 
            // clmUsedSpace
            // 
            clmUsedSpace.HeaderText = "Used Space";
            clmUsedSpace.Name = "clmUsedSpace";
            clmUsedSpace.Width = 130;
            // 
            // clmHostCpu
            // 
            clmHostCpu.HeaderText = "Host CPU";
            clmHostCpu.Name = "clmHostCpu";
            clmHostCpu.Width = 140;
            // 
            // clmHostMemory
            // 
            clmHostMemory.HeaderText = "Host Memory";
            clmHostMemory.Name = "clmHostMemory";
            clmHostMemory.Width = 110;
            // 
            // TcFileSystemCheck
            // 
            TcFileSystemCheck.Controls.Add(tabCcelpro);
            TcFileSystemCheck.Controls.Add(tabccesec1);
            TcFileSystemCheck.Controls.Add(tabCcegitsvr1);
            TcFileSystemCheck.Controls.Add(tabccesa1);
            TcFileSystemCheck.Controls.Add(tabCcesa2);
            TcFileSystemCheck.Location = new Point(21, 62);
            TcFileSystemCheck.Name = "TcFileSystemCheck";
            TcFileSystemCheck.SelectedIndex = 0;
            TcFileSystemCheck.Size = new Size(727, 438);
            TcFileSystemCheck.TabIndex = 2;
            // 
            // tabCcelpro
            // 
            tabCcelpro.Controls.Add(dgvCcelpro1);
            tabCcelpro.Location = new Point(4, 24);
            tabCcelpro.Name = "tabCcelpro";
            tabCcelpro.Padding = new Padding(3);
            tabCcelpro.Size = new Size(719, 410);
            tabCcelpro.TabIndex = 0;
            tabCcelpro.Text = "ccelpro1";
            tabCcelpro.UseVisualStyleBackColor = true;
            // 
            // dgvCcelpro1
            // 
            dgvCcelpro1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCcelpro1.Columns.AddRange(new DataGridViewColumn[] { clmFileSystemLpro1, clmSizeLpro1, clmUsedLpro1, clmAvailableLpro1, clmUsedPercentLpro1, clmMountedOnLpro1 });
            dgvCcelpro1.Location = new Point(3, 3);
            dgvCcelpro1.Name = "dgvCcelpro1";
            dgvCcelpro1.Size = new Size(710, 400);
            dgvCcelpro1.TabIndex = 0;
            // 
            // clmFileSystemLpro1
            // 
            clmFileSystemLpro1.HeaderText = "File System";
            clmFileSystemLpro1.Name = "clmFileSystemLpro1";
            // 
            // clmSizeLpro1
            // 
            clmSizeLpro1.HeaderText = "Size";
            clmSizeLpro1.Name = "clmSizeLpro1";
            // 
            // clmUsedLpro1
            // 
            clmUsedLpro1.HeaderText = "Used";
            clmUsedLpro1.Name = "clmUsedLpro1";
            // 
            // clmAvailableLpro1
            // 
            clmAvailableLpro1.HeaderText = "Available";
            clmAvailableLpro1.Name = "clmAvailableLpro1";
            // 
            // clmUsedPercentLpro1
            // 
            clmUsedPercentLpro1.HeaderText = "Used %";
            clmUsedPercentLpro1.Name = "clmUsedPercentLpro1";
            // 
            // clmMountedOnLpro1
            // 
            clmMountedOnLpro1.HeaderText = "Mounted On";
            clmMountedOnLpro1.Name = "clmMountedOnLpro1";
            // 
            // tabccesec1
            // 
            tabccesec1.Controls.Add(dgvCcesec1);
            tabccesec1.Location = new Point(4, 24);
            tabccesec1.Name = "tabccesec1";
            tabccesec1.Padding = new Padding(3);
            tabccesec1.Size = new Size(719, 410);
            tabccesec1.TabIndex = 1;
            tabccesec1.Text = "ccesec1";
            tabccesec1.UseVisualStyleBackColor = true;
            // 
            // dgvCcesec1
            // 
            dgvCcesec1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCcesec1.Columns.AddRange(new DataGridViewColumn[] { clmFileSystemSec1, clmSizeSec1, clmUsedSec1, clmAvailableSec1, clmUsedPercentSec1, clmMountedOnSec1 });
            dgvCcesec1.Location = new Point(4, 4);
            dgvCcesec1.Name = "dgvCcesec1";
            dgvCcesec1.Size = new Size(710, 474);
            dgvCcesec1.TabIndex = 1;
            // 
            // clmFileSystemSec1
            // 
            clmFileSystemSec1.HeaderText = "File System";
            clmFileSystemSec1.Name = "clmFileSystemSec1";
            // 
            // clmSizeSec1
            // 
            clmSizeSec1.HeaderText = "Size";
            clmSizeSec1.Name = "clmSizeSec1";
            // 
            // clmUsedSec1
            // 
            clmUsedSec1.HeaderText = "Used";
            clmUsedSec1.Name = "clmUsedSec1";
            // 
            // clmAvailableSec1
            // 
            clmAvailableSec1.HeaderText = "Available";
            clmAvailableSec1.Name = "clmAvailableSec1";
            // 
            // clmUsedPercentSec1
            // 
            clmUsedPercentSec1.HeaderText = "Used %";
            clmUsedPercentSec1.Name = "clmUsedPercentSec1";
            // 
            // clmMountedOnSec1
            // 
            clmMountedOnSec1.HeaderText = "Mounted On";
            clmMountedOnSec1.Name = "clmMountedOnSec1";
            // 
            // tabCcegitsvr1
            // 
            tabCcegitsvr1.Controls.Add(dgvCcegitsvr1);
            tabCcegitsvr1.Location = new Point(4, 24);
            tabCcegitsvr1.Name = "tabCcegitsvr1";
            tabCcegitsvr1.Padding = new Padding(3);
            tabCcegitsvr1.Size = new Size(719, 410);
            tabCcegitsvr1.TabIndex = 2;
            tabCcegitsvr1.Text = "ccegitsvr1";
            tabCcegitsvr1.UseVisualStyleBackColor = true;
            // 
            // dgvCcegitsvr1
            // 
            dgvCcegitsvr1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCcegitsvr1.Columns.AddRange(new DataGridViewColumn[] { clmFileSystemSvr1, clmSizeSvr1, clmUsedSvr1, clmAvailableSvr1, clmUsedPercentSvr1, clmMountedOnSvr1 });
            dgvCcegitsvr1.Location = new Point(4, 4);
            dgvCcegitsvr1.Name = "dgvCcegitsvr1";
            dgvCcegitsvr1.Size = new Size(710, 474);
            dgvCcegitsvr1.TabIndex = 1;
            // 
            // clmFileSystemSvr1
            // 
            clmFileSystemSvr1.HeaderText = "File System";
            clmFileSystemSvr1.Name = "clmFileSystemSvr1";
            // 
            // clmSizeSvr1
            // 
            clmSizeSvr1.HeaderText = "Size";
            clmSizeSvr1.Name = "clmSizeSvr1";
            // 
            // clmUsedSvr1
            // 
            clmUsedSvr1.HeaderText = "Used";
            clmUsedSvr1.Name = "clmUsedSvr1";
            // 
            // clmAvailableSvr1
            // 
            clmAvailableSvr1.HeaderText = "Available";
            clmAvailableSvr1.Name = "clmAvailableSvr1";
            // 
            // clmUsedPercentSvr1
            // 
            clmUsedPercentSvr1.HeaderText = "Used %";
            clmUsedPercentSvr1.Name = "clmUsedPercentSvr1";
            // 
            // clmMountedOnSvr1
            // 
            clmMountedOnSvr1.HeaderText = "Mounted On";
            clmMountedOnSvr1.Name = "clmMountedOnSvr1";
            // 
            // tabccesa1
            // 
            tabccesa1.Controls.Add(dgvCcesa1);
            tabccesa1.Location = new Point(4, 24);
            tabccesa1.Name = "tabccesa1";
            tabccesa1.Padding = new Padding(3);
            tabccesa1.Size = new Size(719, 410);
            tabccesa1.TabIndex = 3;
            tabccesa1.Text = "ccesa1";
            tabccesa1.UseVisualStyleBackColor = true;
            // 
            // dgvCcesa1
            // 
            dgvCcesa1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCcesa1.Columns.AddRange(new DataGridViewColumn[] { clmFileSystemSa1, clmSizeSa1, clmUsedSa1, clmAvailableSa1, clmUsedPercentSa1, clmMountedOnSa1 });
            dgvCcesa1.Location = new Point(4, 4);
            dgvCcesa1.Name = "dgvCcesa1";
            dgvCcesa1.Size = new Size(710, 399);
            dgvCcesa1.TabIndex = 1;
            // 
            // clmFileSystemSa1
            // 
            clmFileSystemSa1.HeaderText = "File System";
            clmFileSystemSa1.Name = "clmFileSystemSa1";
            // 
            // clmSizeSa1
            // 
            clmSizeSa1.HeaderText = "Size";
            clmSizeSa1.Name = "clmSizeSa1";
            // 
            // clmUsedSa1
            // 
            clmUsedSa1.HeaderText = "Used";
            clmUsedSa1.Name = "clmUsedSa1";
            // 
            // clmAvailableSa1
            // 
            clmAvailableSa1.HeaderText = "Available";
            clmAvailableSa1.Name = "clmAvailableSa1";
            // 
            // clmUsedPercentSa1
            // 
            clmUsedPercentSa1.HeaderText = "Used %";
            clmUsedPercentSa1.Name = "clmUsedPercentSa1";
            // 
            // clmMountedOnSa1
            // 
            clmMountedOnSa1.HeaderText = "Mounted On";
            clmMountedOnSa1.Name = "clmMountedOnSa1";
            // 
            // tabCcesa2
            // 
            tabCcesa2.Controls.Add(dgvCcesa2);
            tabCcesa2.Location = new Point(4, 24);
            tabCcesa2.Name = "tabCcesa2";
            tabCcesa2.Padding = new Padding(3);
            tabCcesa2.Size = new Size(719, 410);
            tabCcesa2.TabIndex = 4;
            tabCcesa2.Text = "ccesa2";
            tabCcesa2.UseVisualStyleBackColor = true;
            // 
            // dgvCcesa2
            // 
            dgvCcesa2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCcesa2.Columns.AddRange(new DataGridViewColumn[] { clmFileSystemSa2, clmSizeSa2, clmUsedSa2, clmAvailableSa2, clmUsedPercentSa2, clmMountedOnSa2 });
            dgvCcesa2.Location = new Point(4, 4);
            dgvCcesa2.Name = "dgvCcesa2";
            dgvCcesa2.Size = new Size(710, 474);
            dgvCcesa2.TabIndex = 1;
            // 
            // clmFileSystemSa2
            // 
            clmFileSystemSa2.HeaderText = "File System";
            clmFileSystemSa2.Name = "clmFileSystemSa2";
            // 
            // clmSizeSa2
            // 
            clmSizeSa2.HeaderText = "Size";
            clmSizeSa2.Name = "clmSizeSa2";
            // 
            // clmUsedSa2
            // 
            clmUsedSa2.HeaderText = "Used";
            clmUsedSa2.Name = "clmUsedSa2";
            // 
            // clmAvailableSa2
            // 
            clmAvailableSa2.HeaderText = "Available";
            clmAvailableSa2.Name = "clmAvailableSa2";
            // 
            // clmUsedPercentSa2
            // 
            clmUsedPercentSa2.HeaderText = "Used %";
            clmUsedPercentSa2.Name = "clmUsedPercentSa2";
            // 
            // clmMountedOnSa2
            // 
            clmMountedOnSa2.HeaderText = "Mounted On";
            clmMountedOnSa2.Name = "clmMountedOnSa2";
            // 
            // lblEsxiAndVmPmi
            // 
            lblEsxiAndVmPmi.AutoSize = true;
            lblEsxiAndVmPmi.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblEsxiAndVmPmi.Location = new Point(1065, 34);
            lblEsxiAndVmPmi.Name = "lblEsxiAndVmPmi";
            lblEsxiAndVmPmi.Size = new Size(241, 25);
            lblEsxiAndVmPmi.TabIndex = 1;
            lblEsxiAndVmPmi.Text = "ESXi and VMs health check";
            // 
            // lblFileSystemCheckPmi
            // 
            lblFileSystemCheckPmi.AutoSize = true;
            lblFileSystemCheckPmi.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblFileSystemCheckPmi.Location = new Point(287, 24);
            lblFileSystemCheckPmi.Name = "lblFileSystemCheckPmi";
            lblFileSystemCheckPmi.Size = new Size(163, 25);
            lblFileSystemCheckPmi.TabIndex = 0;
            lblFileSystemCheckPmi.Text = "File System Check";
            // 
            // tabStartupShutdownPt1
            // 
            tabStartupShutdownPt1.Location = new Point(4, 24);
            tabStartupShutdownPt1.Name = "tabStartupShutdownPt1";
            tabStartupShutdownPt1.Padding = new Padding(3);
            tabStartupShutdownPt1.Size = new Size(1651, 847);
            tabStartupShutdownPt1.TabIndex = 9;
            tabStartupShutdownPt1.Text = "Startup/Shutdown Pt1";
            tabStartupShutdownPt1.UseVisualStyleBackColor = true;
            // 
            // tabStartupShutdownPt2
            // 
            tabStartupShutdownPt2.Location = new Point(4, 24);
            tabStartupShutdownPt2.Name = "tabStartupShutdownPt2";
            tabStartupShutdownPt2.Padding = new Padding(3);
            tabStartupShutdownPt2.Size = new Size(1651, 847);
            tabStartupShutdownPt2.TabIndex = 10;
            tabStartupShutdownPt2.Text = "Startup/Shutdown Pt2";
            tabStartupShutdownPt2.UseVisualStyleBackColor = true;
            // 
            // tabConfiguration
            // 
            tabConfiguration.Controls.Add(gbxServerInstances);
            tabConfiguration.Controls.Add(lblPowerCLIPathLocation);
            tabConfiguration.Controls.Add(lblPowerCLIPathLabel);
            tabConfiguration.Controls.Add(lblFilePathLocation);
            tabConfiguration.Controls.Add(lblConfigFileLocationLabel);
            tabConfiguration.Controls.Add(gbxImportantVariables);
            tabConfiguration.Controls.Add(gbxComputerList);
            tabConfiguration.Controls.Add(gbxImportantOUs);
            tabConfiguration.Location = new Point(4, 24);
            tabConfiguration.Name = "tabConfiguration";
            tabConfiguration.Padding = new Padding(3);
            tabConfiguration.Size = new Size(1651, 847);
            tabConfiguration.TabIndex = 11;
            tabConfiguration.Text = "Configuration";
            tabConfiguration.UseVisualStyleBackColor = true;
            // 
            // lblPowerCLIPathLocation
            // 
            lblPowerCLIPathLocation.AutoSize = true;
            lblPowerCLIPathLocation.Location = new Point(492, 827);
            lblPowerCLIPathLocation.Name = "lblPowerCLIPathLocation";
            lblPowerCLIPathLocation.Size = new Size(64, 15);
            lblPowerCLIPathLocation.TabIndex = 131;
            lblPowerCLIPathLocation.Text = "Not Found";
            // 
            // lblPowerCLIPathLabel
            // 
            lblPowerCLIPathLabel.AutoSize = true;
            lblPowerCLIPathLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPowerCLIPathLabel.Location = new Point(328, 827);
            lblPowerCLIPathLabel.Name = "lblPowerCLIPathLabel";
            lblPowerCLIPathLabel.Size = new Size(136, 15);
            lblPowerCLIPathLabel.TabIndex = 130;
            lblPowerCLIPathLabel.Text = "PowerCLI Module Path:";
            // 
            // lblFilePathLocation
            // 
            lblFilePathLocation.AutoSize = true;
            lblFilePathLocation.Location = new Point(492, 807);
            lblFilePathLocation.Name = "lblFilePathLocation";
            lblFilePathLocation.Size = new Size(64, 15);
            lblFilePathLocation.TabIndex = 129;
            lblFilePathLocation.Text = "Not Found";
            // 
            // lblConfigFileLocationLabel
            // 
            lblConfigFileLocationLabel.AutoSize = true;
            lblConfigFileLocationLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblConfigFileLocationLabel.Location = new Point(328, 807);
            lblConfigFileLocationLabel.Name = "lblConfigFileLocationLabel";
            lblConfigFileLocationLabel.Size = new Size(158, 15);
            lblConfigFileLocationLabel.TabIndex = 128;
            lblConfigFileLocationLabel.Text = "Configuration File Location:";
            // 
            // gbxImportantVariables
            // 
            gbxImportantVariables.Controls.Add(btnSubmitVars);
            gbxImportantVariables.Controls.Add(txbSecurityGroupKW);
            gbxImportantVariables.Controls.Add(lblSecurityGroupKW);
            gbxImportantVariables.Location = new Point(908, 20);
            gbxImportantVariables.Name = "gbxImportantVariables";
            gbxImportantVariables.Size = new Size(503, 147);
            gbxImportantVariables.TabIndex = 127;
            gbxImportantVariables.TabStop = false;
            gbxImportantVariables.Text = "Important Variables";
            // 
            // btnSubmitVars
            // 
            btnSubmitVars.Location = new Point(171, 97);
            btnSubmitVars.Name = "btnSubmitVars";
            btnSubmitVars.Size = new Size(114, 33);
            btnSubmitVars.TabIndex = 2;
            btnSubmitVars.Text = "Submit Variables";
            btnSubmitVars.UseVisualStyleBackColor = true;
            btnSubmitVars.Click += btnSubmitVars_Click;
            // 
            // txbSecurityGroupKW
            // 
            txbSecurityGroupKW.Location = new Point(84, 68);
            txbSecurityGroupKW.Name = "txbSecurityGroupKW";
            txbSecurityGroupKW.Size = new Size(287, 23);
            txbSecurityGroupKW.TabIndex = 1;
            // 
            // lblSecurityGroupKW
            // 
            lblSecurityGroupKW.AllowDrop = true;
            lblSecurityGroupKW.Location = new Point(6, 27);
            lblSecurityGroupKW.Name = "lblSecurityGroupKW";
            lblSecurityGroupKW.Size = new Size(491, 38);
            lblSecurityGroupKW.TabIndex = 0;
            lblSecurityGroupKW.Text = "Variable to look for in each security groups \"Notes\" property to determine if it should be a selectable item in the Security Groups Checked ListBox when creating a new user.";
            // 
            // gbxComputerList
            // 
            gbxComputerList.Controls.Add(lblLinuxSelectionList);
            gbxComputerList.Controls.Add(cbxIsVm);
            gbxComputerList.Controls.Add(btnRemoveSelectedComputers);
            gbxComputerList.Controls.Add(lblOfficeExemptList);
            gbxComputerList.Controls.Add(btnAddOfficeExemptList);
            gbxComputerList.Controls.Add(lblCriticalLinuxList);
            gbxComputerList.Controls.Add(txbOfficeExemptList);
            gbxComputerList.Controls.Add(lblCriticalWindowsList);
            gbxComputerList.Controls.Add(cbxOfficeExemptList);
            gbxComputerList.Controls.Add(txbLinuxList);
            gbxComputerList.Controls.Add(cbxCriticalNasList);
            gbxComputerList.Controls.Add(btnAddLinuxList);
            gbxComputerList.Controls.Add(txbCriticalLinuxList);
            gbxComputerList.Controls.Add(btnAddCriticalNasList);
            gbxComputerList.Controls.Add(cbxCriticalLinuxList);
            gbxComputerList.Controls.Add(btnAddCriticalLinuxList);
            gbxComputerList.Controls.Add(btnAddCriticalWindowsList);
            gbxComputerList.Controls.Add(cbxLinuxList);
            gbxComputerList.Controls.Add(lblCriticalNasList);
            gbxComputerList.Controls.Add(txbCriticalNasList);
            gbxComputerList.Controls.Add(cbxCriticalWindowsList);
            gbxComputerList.Controls.Add(txbCriticalWindowsList);
            gbxComputerList.Location = new Point(459, 20);
            gbxComputerList.Name = "gbxComputerList";
            gbxComputerList.Size = new Size(433, 761);
            gbxComputerList.TabIndex = 126;
            gbxComputerList.TabStop = false;
            gbxComputerList.Text = "Computer List";
            // 
            // lblLinuxSelectionList
            // 
            lblLinuxSelectionList.AutoSize = true;
            lblLinuxSelectionList.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblLinuxSelectionList.Location = new Point(69, 16);
            lblLinuxSelectionList.Name = "lblLinuxSelectionList";
            lblLinuxSelectionList.Size = new Size(63, 28);
            lblLinuxSelectionList.TabIndex = 102;
            lblLinuxSelectionList.Text = "Linux";
            // 
            // cbxIsVm
            // 
            cbxIsVm.AutoSize = true;
            cbxIsVm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cbxIsVm.Location = new Point(149, 696);
            cbxIsVm.Name = "cbxIsVm";
            cbxIsVm.Size = new Size(124, 19);
            cbxIsVm.TabIndex = 127;
            cbxIsVm.Text = "Computer is a VM";
            cbxIsVm.UseVisualStyleBackColor = true;
            // 
            // btnRemoveSelectedComputers
            // 
            btnRemoveSelectedComputers.Location = new Point(18, 659);
            btnRemoveSelectedComputers.Name = "btnRemoveSelectedComputers";
            btnRemoveSelectedComputers.Size = new Size(409, 31);
            btnRemoveSelectedComputers.TabIndex = 128;
            btnRemoveSelectedComputers.Text = "Remove Selected Computers";
            btnRemoveSelectedComputers.UseVisualStyleBackColor = true;
            // 
            // lblOfficeExemptList
            // 
            lblOfficeExemptList.AutoSize = true;
            lblOfficeExemptList.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblOfficeExemptList.Location = new Point(256, 439);
            lblOfficeExemptList.Name = "lblOfficeExemptList";
            lblOfficeExemptList.Size = new Size(147, 28);
            lblOfficeExemptList.TabIndex = 113;
            lblOfficeExemptList.Text = "Office Exempt";
            // 
            // btnAddOfficeExemptList
            // 
            btnAddOfficeExemptList.Location = new Point(232, 630);
            btnAddOfficeExemptList.Name = "btnAddOfficeExemptList";
            btnAddOfficeExemptList.Size = new Size(187, 23);
            btnAddOfficeExemptList.TabIndex = 118;
            btnAddOfficeExemptList.Text = "Add Office Exemption";
            btnAddOfficeExemptList.UseVisualStyleBackColor = true;
            // 
            // lblCriticalLinuxList
            // 
            lblCriticalLinuxList.AutoSize = true;
            lblCriticalLinuxList.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblCriticalLinuxList.Location = new Point(49, 438);
            lblCriticalLinuxList.Name = "lblCriticalLinuxList";
            lblCriticalLinuxList.Size = new Size(136, 28);
            lblCriticalLinuxList.TabIndex = 103;
            lblCriticalLinuxList.Text = "Critical Linux";
            // 
            // txbOfficeExemptList
            // 
            txbOfficeExemptList.Location = new Point(232, 601);
            txbOfficeExemptList.Name = "txbOfficeExemptList";
            txbOfficeExemptList.Size = new Size(187, 23);
            txbOfficeExemptList.TabIndex = 117;
            // 
            // lblCriticalWindowsList
            // 
            lblCriticalWindowsList.AutoSize = true;
            lblCriticalWindowsList.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblCriticalWindowsList.Location = new Point(249, 20);
            lblCriticalWindowsList.Name = "lblCriticalWindowsList";
            lblCriticalWindowsList.Size = new Size(161, 25);
            lblCriticalWindowsList.TabIndex = 112;
            lblCriticalWindowsList.Text = "Critical Windows";
            // 
            // cbxOfficeExemptList
            // 
            cbxOfficeExemptList.FormattingEnabled = true;
            cbxOfficeExemptList.Location = new Point(232, 478);
            cbxOfficeExemptList.Name = "cbxOfficeExemptList";
            cbxOfficeExemptList.Size = new Size(187, 112);
            cbxOfficeExemptList.TabIndex = 115;
            // 
            // txbLinuxList
            // 
            txbLinuxList.Location = new Point(12, 378);
            txbLinuxList.Name = "txbLinuxList";
            txbLinuxList.Size = new Size(187, 23);
            txbLinuxList.TabIndex = 104;
            // 
            // cbxCriticalNasList
            // 
            cbxCriticalNasList.FormattingEnabled = true;
            cbxCriticalNasList.Location = new Point(235, 274);
            cbxCriticalNasList.Name = "cbxCriticalNasList";
            cbxCriticalNasList.Size = new Size(187, 94);
            cbxCriticalNasList.TabIndex = 117;
            // 
            // btnAddLinuxList
            // 
            btnAddLinuxList.Location = new Point(12, 409);
            btnAddLinuxList.Name = "btnAddLinuxList";
            btnAddLinuxList.Size = new Size(187, 23);
            btnAddLinuxList.TabIndex = 105;
            btnAddLinuxList.Text = "Add Linux Machine";
            btnAddLinuxList.UseVisualStyleBackColor = true;
            // 
            // txbCriticalLinuxList
            // 
            txbCriticalLinuxList.Location = new Point(12, 596);
            txbCriticalLinuxList.Name = "txbCriticalLinuxList";
            txbCriticalLinuxList.Size = new Size(187, 23);
            txbCriticalLinuxList.TabIndex = 106;
            // 
            // btnAddCriticalNasList
            // 
            btnAddCriticalNasList.Location = new Point(235, 409);
            btnAddCriticalNasList.Name = "btnAddCriticalNasList";
            btnAddCriticalNasList.Size = new Size(187, 23);
            btnAddCriticalNasList.TabIndex = 124;
            btnAddCriticalNasList.Text = "Add NAS";
            btnAddCriticalNasList.UseVisualStyleBackColor = true;
            // 
            // cbxCriticalLinuxList
            // 
            cbxCriticalLinuxList.FormattingEnabled = true;
            cbxCriticalLinuxList.Location = new Point(12, 478);
            cbxCriticalLinuxList.Name = "cbxCriticalLinuxList";
            cbxCriticalLinuxList.Size = new Size(187, 112);
            cbxCriticalLinuxList.TabIndex = 109;
            // 
            // btnAddCriticalLinuxList
            // 
            btnAddCriticalLinuxList.Location = new Point(12, 628);
            btnAddCriticalLinuxList.Name = "btnAddCriticalLinuxList";
            btnAddCriticalLinuxList.Size = new Size(187, 23);
            btnAddCriticalLinuxList.TabIndex = 107;
            btnAddCriticalLinuxList.Text = "Add Linux Machine";
            btnAddCriticalLinuxList.UseVisualStyleBackColor = true;
            // 
            // btnAddCriticalWindowsList
            // 
            btnAddCriticalWindowsList.Location = new Point(235, 203);
            btnAddCriticalWindowsList.Name = "btnAddCriticalWindowsList";
            btnAddCriticalWindowsList.Size = new Size(187, 23);
            btnAddCriticalWindowsList.TabIndex = 121;
            btnAddCriticalWindowsList.Text = "Add Windows Machine";
            btnAddCriticalWindowsList.UseVisualStyleBackColor = true;
            // 
            // cbxLinuxList
            // 
            cbxLinuxList.FormattingEnabled = true;
            cbxLinuxList.Location = new Point(10, 53);
            cbxLinuxList.Name = "cbxLinuxList";
            cbxLinuxList.Size = new Size(187, 310);
            cbxLinuxList.TabIndex = 108;
            // 
            // lblCriticalNasList
            // 
            lblCriticalNasList.AutoSize = true;
            lblCriticalNasList.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblCriticalNasList.Location = new Point(273, 233);
            lblCriticalNasList.Name = "lblCriticalNasList";
            lblCriticalNasList.Size = new Size(126, 28);
            lblCriticalNasList.TabIndex = 114;
            lblCriticalNasList.Text = "Critical NAS";
            // 
            // txbCriticalNasList
            // 
            txbCriticalNasList.Location = new Point(235, 378);
            txbCriticalNasList.Name = "txbCriticalNasList";
            txbCriticalNasList.Size = new Size(187, 23);
            txbCriticalNasList.TabIndex = 123;
            // 
            // cbxCriticalWindowsList
            // 
            cbxCriticalWindowsList.FormattingEnabled = true;
            cbxCriticalWindowsList.Location = new Point(235, 53);
            cbxCriticalWindowsList.Name = "cbxCriticalWindowsList";
            cbxCriticalWindowsList.Size = new Size(187, 94);
            cbxCriticalWindowsList.TabIndex = 116;
            // 
            // txbCriticalWindowsList
            // 
            txbCriticalWindowsList.Location = new Point(235, 167);
            txbCriticalWindowsList.Name = "txbCriticalWindowsList";
            txbCriticalWindowsList.Size = new Size(187, 23);
            txbCriticalWindowsList.TabIndex = 120;
            // 
            // gbxImportantOUs
            // 
            gbxImportantOUs.Controls.Add(btnAddSecurityGroupsOU);
            gbxImportantOUs.Controls.Add(cbxListSecurityGroupsOu);
            gbxImportantOUs.Controls.Add(lblSecurityGroups);
            gbxImportantOUs.Controls.Add(btnAddWindowsServersOu);
            gbxImportantOUs.Controls.Add(btnAddPatriotParkOu);
            gbxImportantOUs.Controls.Add(btnAddWorkstationOu);
            gbxImportantOUs.Controls.Add(cbxListWorkStationOu);
            gbxImportantOUs.Controls.Add(btnRemoveSelectedOus);
            gbxImportantOUs.Controls.Add(lblWorkstationOu);
            gbxImportantOUs.Controls.Add(cbxListWindowsServersOu);
            gbxImportantOUs.Controls.Add(lblPatriotParkOu);
            gbxImportantOUs.Controls.Add(lblWindowsServersOu);
            gbxImportantOUs.Controls.Add(cbxListPatriotParkOu);
            gbxImportantOUs.Location = new Point(8, 6);
            gbxImportantOUs.Name = "gbxImportantOUs";
            gbxImportantOUs.Size = new Size(432, 775);
            gbxImportantOUs.TabIndex = 102;
            gbxImportantOUs.TabStop = false;
            gbxImportantOUs.Text = "Important OU's";
            // 
            // btnAddSecurityGroupsOU
            // 
            btnAddSecurityGroupsOU.Location = new Point(10, 499);
            btnAddSecurityGroupsOU.Name = "btnAddSecurityGroupsOU";
            btnAddSecurityGroupsOU.Size = new Size(412, 23);
            btnAddSecurityGroupsOU.TabIndex = 108;
            btnAddSecurityGroupsOU.Text = "Add Security Groups OU";
            btnAddSecurityGroupsOU.UseVisualStyleBackColor = true;
            btnAddSecurityGroupsOU.Click += btnAddSecurityGroupsOU_Click;
            // 
            // cbxListSecurityGroupsOu
            // 
            cbxListSecurityGroupsOu.FormattingEnabled = true;
            cbxListSecurityGroupsOu.Location = new Point(10, 528);
            cbxListSecurityGroupsOu.Name = "cbxListSecurityGroupsOu";
            cbxListSecurityGroupsOu.Size = new Size(416, 76);
            cbxListSecurityGroupsOu.TabIndex = 107;
            // 
            // lblSecurityGroups
            // 
            lblSecurityGroups.AutoSize = true;
            lblSecurityGroups.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSecurityGroups.Location = new Point(6, 475);
            lblSecurityGroups.Name = "lblSecurityGroups";
            lblSecurityGroups.Size = new Size(162, 21);
            lblSecurityGroups.TabIndex = 106;
            lblSecurityGroups.Text = "Security Groups OU:";
            // 
            // btnAddWindowsServersOu
            // 
            btnAddWindowsServersOu.Location = new Point(10, 339);
            btnAddWindowsServersOu.Name = "btnAddWindowsServersOu";
            btnAddWindowsServersOu.Size = new Size(412, 23);
            btnAddWindowsServersOu.TabIndex = 104;
            btnAddWindowsServersOu.Text = "Add Windows Servers OU";
            btnAddWindowsServersOu.UseVisualStyleBackColor = true;
            btnAddWindowsServersOu.Click += btnAddWindowsServersOu_Click;
            // 
            // btnAddPatriotParkOu
            // 
            btnAddPatriotParkOu.Location = new Point(6, 193);
            btnAddPatriotParkOu.Name = "btnAddPatriotParkOu";
            btnAddPatriotParkOu.Size = new Size(412, 23);
            btnAddPatriotParkOu.TabIndex = 103;
            btnAddPatriotParkOu.Text = "Add Patriot Park OU";
            btnAddPatriotParkOu.UseVisualStyleBackColor = true;
            btnAddPatriotParkOu.Click += btnAddPatriotParkOu_Click;
            // 
            // btnAddWorkstationOu
            // 
            btnAddWorkstationOu.Location = new Point(10, 47);
            btnAddWorkstationOu.Name = "btnAddWorkstationOu";
            btnAddWorkstationOu.Size = new Size(412, 23);
            btnAddWorkstationOu.TabIndex = 102;
            btnAddWorkstationOu.Text = "Add Workstation OU";
            btnAddWorkstationOu.UseVisualStyleBackColor = true;
            btnAddWorkstationOu.Click += btnAddWorkStationOu_Click;
            // 
            // cbxListWorkStationOu
            // 
            cbxListWorkStationOu.FormattingEnabled = true;
            cbxListWorkStationOu.Location = new Point(10, 76);
            cbxListWorkStationOu.Name = "cbxListWorkStationOu";
            cbxListWorkStationOu.Size = new Size(416, 76);
            cbxListWorkStationOu.TabIndex = 85;
            // 
            // btnRemoveSelectedOus
            // 
            btnRemoveSelectedOus.Location = new Point(10, 615);
            btnRemoveSelectedOus.Name = "btnRemoveSelectedOus";
            btnRemoveSelectedOus.Size = new Size(416, 23);
            btnRemoveSelectedOus.TabIndex = 101;
            btnRemoveSelectedOus.Text = "Remove Selected OU's";
            btnRemoveSelectedOus.UseVisualStyleBackColor = true;
            // 
            // lblWorkstationOu
            // 
            lblWorkstationOu.AutoSize = true;
            lblWorkstationOu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblWorkstationOu.Location = new Point(6, 23);
            lblWorkstationOu.Name = "lblWorkstationOu";
            lblWorkstationOu.Size = new Size(136, 21);
            lblWorkstationOu.TabIndex = 78;
            lblWorkstationOu.Text = "Workstation OU:";
            // 
            // cbxListWindowsServersOu
            // 
            cbxListWindowsServersOu.FormattingEnabled = true;
            cbxListWindowsServersOu.Location = new Point(10, 370);
            cbxListWindowsServersOu.Name = "cbxListWindowsServersOu";
            cbxListWindowsServersOu.Size = new Size(416, 76);
            cbxListWindowsServersOu.TabIndex = 93;
            // 
            // lblPatriotParkOu
            // 
            lblPatriotParkOu.AutoSize = true;
            lblPatriotParkOu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblPatriotParkOu.Location = new Point(6, 169);
            lblPatriotParkOu.Name = "lblPatriotParkOu";
            lblPatriotParkOu.Size = new Size(132, 21);
            lblPatriotParkOu.TabIndex = 79;
            lblPatriotParkOu.Text = "Patriot Park OU:";
            // 
            // lblWindowsServersOu
            // 
            lblWindowsServersOu.AutoSize = true;
            lblWindowsServersOu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblWindowsServersOu.Location = new Point(6, 315);
            lblWindowsServersOu.Name = "lblWindowsServersOu";
            lblWindowsServersOu.Size = new Size(173, 21);
            lblWindowsServersOu.TabIndex = 80;
            lblWindowsServersOu.Text = "Windows Servers OU:";
            // 
            // cbxListPatriotParkOu
            // 
            cbxListPatriotParkOu.FormattingEnabled = true;
            cbxListPatriotParkOu.Location = new Point(6, 222);
            cbxListPatriotParkOu.Name = "cbxListPatriotParkOu";
            cbxListPatriotParkOu.Size = new Size(416, 76);
            cbxListPatriotParkOu.TabIndex = 89;
            // 
            // tabConsole
            // 
            tabConsole.Location = new Point(4, 24);
            tabConsole.Name = "tabConsole";
            tabConsole.Padding = new Padding(3);
            tabConsole.Size = new Size(1651, 847);
            tabConsole.TabIndex = 12;
            tabConsole.Text = "Console";
            tabConsole.UseVisualStyleBackColor = true;
            // 
            // btnUndockConsole
            // 
            btnUndockConsole.Location = new Point(591, 881);
            btnUndockConsole.Name = "btnUndockConsole";
            btnUndockConsole.Size = new Size(110, 39);
            btnUndockConsole.TabIndex = 0;
            btnUndockConsole.Text = "Undock Console";
            btnUndockConsole.UseVisualStyleBackColor = true;
            btnUndockConsole.Click += btnUndockConsole_Click;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(745, 881);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(110, 39);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Log Out";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // dtpLogStartDate
            // 
            dtpLogStartDate.Location = new Point(20, 48);
            dtpLogStartDate.Name = "dtpLogStartDate";
            dtpLogStartDate.Size = new Size(200, 23);
            dtpLogStartDate.TabIndex = 0;
            // 
            // dtpLogEndDate
            // 
            dtpLogEndDate.Location = new Point(232, 48);
            dtpLogEndDate.Name = "dtpLogEndDate";
            dtpLogEndDate.Size = new Size(215, 23);
            dtpLogEndDate.TabIndex = 1;
            // 
            // lblLogStartDate
            // 
            lblLogStartDate.AutoSize = true;
            lblLogStartDate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLogStartDate.Location = new Point(20, 30);
            lblLogStartDate.Name = "lblLogStartDate";
            lblLogStartDate.Size = new Size(176, 15);
            lblLogStartDate.TabIndex = 2;
            lblLogStartDate.Text = "Start Date/Time for log range:";
            // 
            // lblLogEndDate
            // 
            lblLogEndDate.AutoSize = true;
            lblLogEndDate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLogEndDate.Location = new Point(232, 30);
            lblLogEndDate.Name = "lblLogEndDate";
            lblLogEndDate.Size = new Size(168, 15);
            lblLogEndDate.TabIndex = 3;
            lblLogEndDate.Text = "End Date/Time for log range:";
            // 
            // chkLastHourOnly
            // 
            chkLastHourOnly.AutoSize = true;
            chkLastHourOnly.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkLastHourOnly.Location = new Point(453, 52);
            chkLastHourOnly.Name = "chkLastHourOnly";
            chkLastHourOnly.Size = new Size(107, 19);
            chkLastHourOnly.TabIndex = 4;
            chkLastHourOnly.Text = "Last Hour Only";
            chkLastHourOnly.UseVisualStyleBackColor = true;
            chkLastHourOnly.CheckedChanged += chkLastHourOnly_CheckedChanged;
            //
            // cmbLogPriority
            // 
            cmbLogPriority.FormattingEnabled = true;
            cmbLogPriority.Items.AddRange(new object[] { "All Levels", "Emergency", "Alert", "Critical", "Error", "Warning", "Notice", "Info", "Debug" });
            cmbLogPriority.Location = new Point(20, 108);
            cmbLogPriority.Name = "cmbLogPriority";
            cmbLogPriority.Size = new Size(200, 23);
            cmbLogPriority.TabIndex = 5;
            // 
            // lblPrioritySeverityLevel
            // 
            lblPrioritySeverityLevel.AutoSize = true;
            lblPrioritySeverityLevel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPrioritySeverityLevel.Location = new Point(20, 90);
            lblPrioritySeverityLevel.Name = "lblPrioritySeverityLevel";
            lblPrioritySeverityLevel.Size = new Size(133, 15);
            lblPrioritySeverityLevel.TabIndex = 6;
            lblPrioritySeverityLevel.Text = "Priority/Severity Level";
            // 
            // lblKeywordSearch
            // 
            lblKeywordSearch.AutoSize = true;
            lblKeywordSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKeywordSearch.Location = new Point(232, 90);
            lblKeywordSearch.Name = "lblKeywordSearch";
            lblKeywordSearch.Size = new Size(222, 15);
            lblKeywordSearch.TabIndex = 7;
            lblKeywordSearch.Text = "Enter Keyword to search for (Optional)";
            // 
            // txbLogKeyword
            // 
            txbLogKeyword.Location = new Point(232, 108);
            txbLogKeyword.Name = "txbLogKeyword";
            txbLogKeyword.Size = new Size(215, 23);
            txbLogKeyword.TabIndex = 8;
            // 
            // cbxCaseSensitive
            // 
            cbxCaseSensitive.AutoSize = true;
            cbxCaseSensitive.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            cbxCaseSensitive.Location = new Point(453, 110);
            cbxCaseSensitive.Name = "cbxCaseSensitive";
            cbxCaseSensitive.Size = new Size(105, 19);
            cbxCaseSensitive.TabIndex = 9;
            cbxCaseSensitive.Text = "Case Sensitive";
            cbxCaseSensitive.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(20, 146);
            label1.Name = "label1";
            label1.Size = new Size(124, 15);
            label1.TabIndex = 10;
            label1.Text = "Log Source Selection";
            // 
            // cbxLogSourceSelection
            // 
            cbxLogSourceSelection.FormattingEnabled = true;
            cbxLogSourceSelection.Items.AddRange(new object[] { "All", "System Logs (journalctl)", "Security/Authentication", "Directory Server - Errors", "Directory Server - Access", "Directory Server - Audit" });
            cbxLogSourceSelection.Location = new Point(20, 164);
            cbxLogSourceSelection.Name = "cbxLogSourceSelection";
            cbxLogSourceSelection.Size = new Size(200, 23);
            cbxLogSourceSelection.TabIndex = 11;
            // 
            // cbxServerSelection
            // 
            cbxServerSelection.FormattingEnabled = true;
            cbxServerSelection.Items.AddRange(new object[] { "All", "System Logs (journalctl)", "Security/Authentication", "Directory Server - Errors", "Directory Server - Access", "Directory Server - Audit" });
            cbxServerSelection.Location = new Point(232, 164);
            cbxServerSelection.Name = "cbxServerSelection";
            cbxServerSelection.Size = new Size(215, 23);
            cbxServerSelection.TabIndex = 13;
            // 
            // lblServerSelection
            // 
            lblServerSelection.AutoSize = true;
            lblServerSelection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblServerSelection.Location = new Point(232, 146);
            lblServerSelection.Name = "lblServerSelection";
            lblServerSelection.Size = new Size(100, 15);
            lblServerSelection.TabIndex = 12;
            lblServerSelection.Text = "Server Selection";
            // 
            // lblLogStatus
            // 
            lblLogStatus.AutoSize = true;
            lblLogStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLogStatus.Location = new Point(128, 201);
            lblLogStatus.Name = "lblLogStatus";
            lblLogStatus.Size = new Size(68, 15);
            lblLogStatus.TabIndex = 14;
            lblLogStatus.Text = "Log Status:";
            // 
            // lblLogStatusResults
            // 
            lblLogStatusResults.AutoSize = true;
            lblLogStatusResults.Font = new Font("Segoe UI", 9F);
            lblLogStatusResults.Location = new Point(202, 201);
            lblLogStatusResults.Name = "lblLogStatusResults";
            lblLogStatusResults.Size = new Size(102, 15);
            lblLogStatusResults.TabIndex = 15;
            lblLogStatusResults.Text = "Log Status Results";
            // 
            // rtbLogOutput
            // 
            rtbLogOutput.BackColor = SystemColors.MenuText;
            rtbLogOutput.ForeColor = SystemColors.Window;
            rtbLogOutput.Location = new Point(8, 222);
            rtbLogOutput.Name = "rtbLogOutput";
            rtbLogOutput.Size = new Size(550, 350);
            rtbLogOutput.TabIndex = 16;
            rtbLogOutput.Text = "";
            // 
            // btnFetchLogs
            // 
            btnFetchLogs.Location = new Point(110, 588);
            btnFetchLogs.Name = "btnFetchLogs";
            btnFetchLogs.Size = new Size(94, 37);
            btnFetchLogs.TabIndex = 17;
            btnFetchLogs.Text = "Fetch Logs";
            btnFetchLogs.UseVisualStyleBackColor = true;
            btnFetchLogs.Click += btnFetchLogs_Click;
            //
            // btnClearLogs
            // 
            btnClearLogs.Location = new Point(225, 588);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(94, 37);
            btnClearLogs.TabIndex = 18;
            btnClearLogs.Text = "Clear Logs";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;
            //
            // btnExportLogs
            // 
            btnExportLogs.Location = new Point(333, 588);
            btnExportLogs.Name = "btnExportLogs";
            btnExportLogs.Size = new Size(94, 37);
            btnExportLogs.TabIndex = 19;
            btnExportLogs.Text = "Export Logs";
            btnExportLogs.UseVisualStyleBackColor = true;
            btnExportLogs.Click += btnExportLogs_Click;
            //
            // gbxServerInstances
            // 
            gbxServerInstances.Controls.Add(btnSubmitServerInstance);
            gbxServerInstances.Controls.Add(txbLdapServerInstace2);
            gbxServerInstances.Controls.Add(lblLdapServerInstance2);
            gbxServerInstances.Controls.Add(txbLdapServerInstace1);
            gbxServerInstances.Controls.Add(lblLdapServerInstance1);
            gbxServerInstances.Location = new Point(908, 187);
            gbxServerInstances.Name = "gbxServerInstances";
            gbxServerInstances.Size = new Size(503, 155);
            gbxServerInstances.TabIndex = 132;
            gbxServerInstances.TabStop = false;
            gbxServerInstances.Text = "LDAP Server Instances";
            // 
            // lblLdapServerInstance1
            // 
            lblLdapServerInstance1.AutoSize = true;
            lblLdapServerInstance1.Location = new Point(2, 37);
            lblLdapServerInstance1.Name = "lblLdapServerInstance1";
            lblLdapServerInstance1.Size = new Size(127, 15);
            lblLdapServerInstance1.TabIndex = 0;
            lblLdapServerInstance1.Text = "LDAP Server Instance 1";
            // 
            // txbLdapServerInstace1
            // 
            txbLdapServerInstace1.Location = new Point(135, 34);
            txbLdapServerInstace1.Name = "txbLdapServerInstace1";
            txbLdapServerInstace1.Size = new Size(310, 23);
            txbLdapServerInstace1.TabIndex = 1;
            // 
            // txbLdapServerInstace2
            // 
            txbLdapServerInstace2.Location = new Point(135, 72);
            txbLdapServerInstace2.Name = "txbLdapServerInstace2";
            txbLdapServerInstace2.Size = new Size(310, 23);
            txbLdapServerInstace2.TabIndex = 3;
            // 
            // lblLdapServerInstance2
            // 
            lblLdapServerInstance2.AutoSize = true;
            lblLdapServerInstance2.Location = new Point(2, 75);
            lblLdapServerInstance2.Name = "lblLdapServerInstance2";
            lblLdapServerInstance2.Size = new Size(127, 15);
            lblLdapServerInstance2.TabIndex = 2;
            lblLdapServerInstance2.Text = "LDAP Server Instance 2";
            // 
            // btnSubmitServerInstance
            // 
            btnSubmitServerInstance.Location = new Point(192, 107);
            btnSubmitServerInstance.Name = "btnSubmitServerInstance";
            btnSubmitServerInstance.Size = new Size(114, 33);
            btnSubmitServerInstance.TabIndex = 3;
            btnSubmitServerInstance.Text = "Submit Variables";
            btnSubmitServerInstance.UseVisualStyleBackColor = true;
            btnSubmitServerInstance.Click += btnSubmitServerInstance_Click;
            // 
            // SAToolBelt
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1659, 976);
            Controls.Add(btnLogout);
            Controls.Add(tabControlMain);
            Controls.Add(btnUndockConsole);
            Name = "SAToolBelt";
            Text = "Lockheed Martin - SPICE - SA Toolbelt";
            KeyDown += SAToolBelt_KeyDown;
            tabControlMain.ResumeLayout(false);
            tabLogin.ResumeLayout(false);
            panelLogin.ResumeLayout(false);
            panelLogin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            tabAD.ResumeLayout(false);
            tabAD.PerformLayout();
            gbxDisableAccount.ResumeLayout(false);
            gbxDisableAccount.PerformLayout();
            gbxDeleteAccount.ResumeLayout(false);
            gbxUnlockAccount.ResumeLayout(false);
            gbxSingleUserSearch.ResumeLayout(false);
            gbxSingleUserSearch.PerformLayout();
            tabControlADResults.ResumeLayout(false);
            tabResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvUnifiedResults).EndInit();
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            tabMemberOf.ResumeLayout(false);
            tabMemberOf.PerformLayout();
            gbxLockedAccounts.ResumeLayout(false);
            gbxLockedAccounts.PerformLayout();
            gbxDisabledAccounts.ResumeLayout(false);
            gbxDisabledAccounts.PerformLayout();
            gbxExpiredAccounts.ResumeLayout(false);
            gbxExpiredAccounts.PerformLayout();
            gbxExpiringAccounts.ResumeLayout(false);
            gbxExpiringAccounts.PerformLayout();
            gbxChangePassword.ResumeLayout(false);
            gbxChangePassword.PerformLayout();
            gbxTestPassword.ResumeLayout(false);
            gbxTestPassword.PerformLayout();
            gbxAcntExpDate.ResumeLayout(false);
            tabLDAP.ResumeLayout(false);
            tabLDAP.PerformLayout();
            gbxUserAccountCreation.ResumeLayout(false);
            gbxUserAccountCreation.PerformLayout();
            tabLinuxTools.ResumeLayout(false);
            tabLinuxTools.PerformLayout();
            tabOnlineOffline.ResumeLayout(false);
            tabOnlineOffline.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvWorkstations).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPatriotPark).EndInit();
            tabSAPMIsSpice.ResumeLayout(false);
            tabSAPMIsSpice.PerformLayout();
            gbxLDAPReplicationChk.ResumeLayout(false);
            gbxLDAPReplicationChk.PerformLayout();
            dgvEsxiVmHealthChk.ResumeLayout(false);
            tabEsxiHealthPmi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvEsxiHealthCheck).EndInit();
            tabVmHealthChkPmi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvVmHealthCheck).EndInit();
            TcFileSystemCheck.ResumeLayout(false);
            tabCcelpro.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCcelpro1).EndInit();
            tabccesec1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCcesec1).EndInit();
            tabCcegitsvr1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCcegitsvr1).EndInit();
            tabccesa1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCcesa1).EndInit();
            tabCcesa2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCcesa2).EndInit();
            tabConfiguration.ResumeLayout(false);
            tabConfiguration.PerformLayout();
            gbxImportantVariables.ResumeLayout(false);
            gbxImportantVariables.PerformLayout();
            gbxComputerList.ResumeLayout(false);
            gbxComputerList.PerformLayout();
            gbxImportantOUs.ResumeLayout(false);
            gbxImportantOUs.PerformLayout();
            gbxServerInstances.ResumeLayout(false);
            gbxServerInstances.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControlMain;
        private TabPage tabLogin;
        private Panel panelLogin;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;
        private TabPage tabAD;
        private GroupBox gbxExpiringAccounts;
        private RadioButton rbExpiringAccounts0to30;
        private RadioButton rbExpiringAccounts31to60;
        private RadioButton rbExpiringAccounts61to90;
        private GroupBox gbxExpiredAccounts;
        private RadioButton rbExpiredAccounts0to30;
        private RadioButton rbExpiredAccounts31to60;
        private RadioButton rbExpiredAccounts61to90;
        private RadioButton rbExpiredAccounts90Plus;
        private GroupBox gbxDisabledAccounts;
        private RadioButton rbDisabledAccounts0to30;
        private RadioButton rbDisabledAccounts31to60;
        private RadioButton rbDisabledAccounts61to90;
        private RadioButton rbDisabledAccounts90Plus;
        private GroupBox gbxLockedAccounts;
        private RadioButton rbLockedAccountsOut;
        private GroupBox gbxSingleUserSearch;
        private RadioButton rbnSingleUserSearch;
        private TextBox txbUserName;
        private TextBox txbFirstName;
        private TextBox txbLastName;
        private Label lblUsersName;
        private Label lblLastName;
        private Label lblFirstName;
        private Button btnAdLoadAccounts;
        private TabControl tabControlADResults;
        private Label lblTestPassword;
        private GroupBox gbxTestPassword;
        private TextBox txbTestPassword;
        private Button btnShowTestPassword;
        private Button btnTestPassword;
        private TabPage tabResults;
        private TabPage tabGeneral;
        private Label lblGenFirstName;
        private Label lblFirstNameValue;
        private Label lblGenLastName;
        private Label lblLastNameValue;
        private Label lblLoginName;
        private Label lblLoginNameValue;
        private Label lblEmail;
        private Label lblEmailValue;
        private Label lblDescription;
        private Label lblDescriptionValue;
        private Label lblTelephoneNumber;
        private Label lblTelephoneNumberValue;
        private Label lblAccountExpiration;
        private Label lblAccountExpirationValue;
        private Label lblLastPasswordChange;
        private Label lblLastPasswordChangeValue;
        private Label lblLastLogin;
        private Label lblLastLoginValue;
        private Label lblHomeDrive;
        private Label lblHomeDriveValue;
        private Label lblLocked;
        private Label lblLockedValue;
        private Label lblOU;
        private Label lblOUValue;
        private Label lblGIDNumber;
        private Label lblGIDNumberValue;
        private Label lblUIDNumber;
        private Label lblUIDNumberValue;
        private TabPage tabMemberOf;
        private Label lblLoadedUser;
        private DataGridView dgvUnifiedResults;
        private DataGridViewTextBoxColumn colFullName;
        private DataGridViewTextBoxColumn colUserName;
        private DataGridViewTextBoxColumn colFirstName;
        private DataGridViewTextBoxColumn colLastName;
        private DataGridViewTextBoxColumn colLogonName;
        private DataGridViewTextBoxColumn colExpDate;
        private DataGridViewTextBoxColumn colDaysLeft;
        private DataGridViewTextBoxColumn colExpirationDate;
        private DataGridViewTextBoxColumn colDaysExpired;
        private DataGridViewTextBoxColumn colDaysDisabled;
        private DataGridViewTextBoxColumn colHomeDirExists;
        private DataGridViewTextBoxColumn colLockDate;
        private DataGridViewTextBoxColumn colUnlock;
        private GroupBox gbxChangePassword;
        private Label lblChangePassword;
        private Button btnPwChngShowPassword;
        private Label lblPwdRequirements;
        private Label lblFourteenChrs;
        private Label lblOneUppercase;
        private Label lblOneLowercase;
        private Label lblOneNumber;
        private Label lblOneSpecial;
        private Label lblNewPassword;
        private TextBox txbNewPassword;
        private Label lblConfirmNewPassword;
        private TextBox txbConfirmNewPassword;
        private Button btnClearPasswords;
        private Button btnSubmit;
        private CheckBox cbxUnlockAcnt;
        private Label lblActExpDate;
        private GroupBox gbxAcntExpDate;
        private DateTimePicker pkrAcntExpDateTimePicker;
        private Button btnAcntExeDateUpdate;
        private TabPage tabLDAP;
        private TabPage tabRemoteTools;
        private TabPage tabWindowsTools;
        private TabPage tabLinuxTools;
        private TabPage tabVMwareTools;
        private TabPage tabOnlineOffline;
        private TabPage tabSAPMIsSpice;
        private TabPage tabStartupShutdownPt1;
        private TabPage tabStartupShutdownPt2;
        private Button btnAdClear;
        private Button btnShowPassword;
        private Label lblUnlockAccount;
        private GroupBox gbxUnlockAccount;
        private Button btnUnlockAccount;
        private Label lblDeleteAccount;
        private GroupBox gbxDeleteAccount;
        private Button btnDeleteAccount;
        private Label lblDisableAccount;
        private GroupBox gbxDisableAccount;
        private TextBox txbDisabledReason;
        private Label lblDisabledReason;
        private Label lblDateDisabled;
        private Button btnDisable;
        private DateTimePicker dtpDisabledDate;
        private TextBox txbProcessedBy;
        private Label lblProcessedBy;
        private CheckedListBox clbMemberOf;
        private Button btnEditUsersGroups;
        private Label lblNoteLowerCase;
        private Label lblLdapFirstName;
        private Label lblLdapLastName;
        private Label lblLdapEmail;
        private Label lblLdapNtUserId;
        private Label lblLdapPhone;
        private Label lblNewUserAcntCreation;
        private Button btnLdapGetUid;
        private Button btnClearAccountCreationForm;
        private Button btnLdapCreateAccount;
        private Button btnLdapGenerate;
        private GroupBox gbxUserAccountCreation;
        private TextBox txbLdapLastName;
        private TextBox txbLdapEmail;
        private TextBox txbLdapNtUserId;
        private TextBox txbLdapPhone;
        private TextBox txbLdapLinuxUid;
        private TextBox txbLdapTempPass;
        private TextBox txbLdapFirstName;
        private Label lblLdapTempPass;
        private Label lblLdapLinuxUid;
        private Label lblCriticalNAS;
        private Label lblOfficeExempt;
        private Label lblCriticalWindows;
        private Label lblWorkstations;
        private DataGridView dgvWorkstations;
        private Label lblPatriotPark;
        private DataGridView dgvPatriotPark;
        private Label lblWindowsServers;
        private ListBox lbxOfficeExempt;
        private ListBox lbxCriticalWindows;
        private ListBox lbxCriticalNas;
        private ListBox lbxWindowsServers;
        private Button btnOnOffline;
        private TabPage tabConfiguration;
        private GroupBox gbxImportantOUs;
        private CheckedListBox cbxListWorkStationOu;
         private Label lblWorkstationOu;
        private CheckedListBox cbxListWindowsServersOu;
        private Label lblPatriotParkOu;
        private Label lblWindowsServersOu;
        private CheckedListBox cbxListPatriotParkOu;
        private Label lblCriticalNasList;
        private Label lblOfficeExemptList;
        private Label lblCriticalWindowsList;
        private CheckedListBox cbxCriticalLinuxList;
        private CheckedListBox cbxLinuxList;
        private Button btnAddCriticalLinuxList;
        private TextBox txbCriticalLinuxList;
        private Button btnAddLinuxList;
        private TextBox txbLinuxList;
        private Label lblCriticalLinuxList;
        private Label lblLinuxSelectionList;
        private CheckedListBox cbxCriticalNasList;
        private CheckedListBox cbxCriticalWindowsList;
        private CheckedListBox cbxOfficeExemptList;
        private TextBox txbOfficeExemptList;
        private Button btnAddOfficeExemptList;
        private Button btnAddCriticalNasList;
        private Button btnAddCriticalWindowsList;
        private TextBox txbCriticalNasList;
        private TextBox txbCriticalWindowsList;
        private Label lblCriticalLinux;
        private Label lblLinux;
        private ListBox lbxCriticalLinux;
        private ListBox lbxLinux;
        private Label lblEsxiAndVmPmi;
        private Label lblFileSystemCheckPmi;
        private TabControl dgvEsxiVmHealthChk;
        private TabPage tabEsxiHealthPmi;
        private TabPage tabVmHealthChkPmi;
        private TabControl TcFileSystemCheck;
        private TabPage tabCcelpro;
        private TabPage tabccesec1;
        private TabPage tabCcegitsvr1;
        private TabPage tabccesa1;
        private TabPage tabCcesa2;
        private DataGridView dgvCcelpro1;
        private DataGridViewTextBoxColumn clmFileSystemLpro1;
        private DataGridViewTextBoxColumn clmSizeLpro1;
        private DataGridViewTextBoxColumn clmUsedLpro1;
        private DataGridViewTextBoxColumn clmAvailableLpro1;
        private DataGridViewTextBoxColumn clmUsedPercentLpro1;
        private DataGridViewTextBoxColumn clmMountedOnLpro1;
        private DataGridView dgvCcesec1;
        private DataGridView dgvCcegitsvr1;
        private DataGridView dgvCcesa1;
        private DataGridView dgvCcesa2;
        private DataGridViewTextBoxColumn clmFileSystemSec1;
        private DataGridViewTextBoxColumn clmSizeSec1;
        private DataGridViewTextBoxColumn clmUsedSec1;
        private DataGridViewTextBoxColumn clmAvailableSec1;
        private DataGridViewTextBoxColumn clmUsedPercentSec1;
        private DataGridViewTextBoxColumn clmMountedOnSec1;
        private DataGridViewTextBoxColumn clmFileSystemSvr1;
        private DataGridViewTextBoxColumn clmSizeSvr1;
        private DataGridViewTextBoxColumn clmUsedSvr1;
        private DataGridViewTextBoxColumn clmAvailableSvr1;
        private DataGridViewTextBoxColumn clmUsedPercentSvr1;
        private DataGridViewTextBoxColumn clmMountedOnSvr1;
        private DataGridViewTextBoxColumn clmFileSystemSa1;
        private DataGridViewTextBoxColumn clmSizeSa1;
        private DataGridViewTextBoxColumn clmUsedSa1;
        private DataGridViewTextBoxColumn clmAvailableSa1;
        private DataGridViewTextBoxColumn clmUsedPercentSa1;
        private DataGridViewTextBoxColumn clmMountedOnSa1;
        private DataGridViewTextBoxColumn clmFileSystemSa2;
        private DataGridViewTextBoxColumn clmSizeSa2;
        private DataGridViewTextBoxColumn clmUsedSa2;
        private DataGridViewTextBoxColumn clmAvailableSa2;
        private DataGridViewTextBoxColumn clmUsedPercentSa2;
        private DataGridViewTextBoxColumn clmMountedOnSa2;
        private DataGridView dgvEsxiHealthCheck;
        private DataGridViewTextBoxColumn clmServerName;
        private DataGridViewTextBoxColumn clmState;
        private DataGridViewTextBoxColumn clmStatus;
        private DataGridViewTextBoxColumn clmCluster;
        private DataGridViewTextBoxColumn clmConsumedCpu;
        private DataGridViewTextBoxColumn clmConsumedMemory;
        private DataGridViewTextBoxColumn clmHaState;
        private DataGridViewTextBoxColumn clmUptime;
        private DataGridView dgvVmHealthCheck;
        private DataGridViewTextBoxColumn clmVmName;
        private DataGridViewTextBoxColumn clmPowerState;
        private DataGridViewTextBoxColumn clmVmStatus;
        private DataGridViewTextBoxColumn clmProvisionedSpace;
        private DataGridViewTextBoxColumn clmUsedSpace;
        private DataGridViewTextBoxColumn clmHostCpu;
        private DataGridViewTextBoxColumn clmHostMemory;
        private Button btnPerformHealthChk;
        private Button btnCheckFileSystem;
        private GroupBox gbxLDAPReplicationChk;
        private Label lblLastUpdateStartSa1;
        private Label lblLastUpdateEndSa1;
        private Label lblChangesSentSa1;
        private Label lblLastUpdateStartDataSa1;
        private Label lblLastUpdateEndDataSa1;
        private Label lblChangesSentDataSa1;
        private Label lblTargetCcesa1;
        private Label lblTargetCceSa2;
        private Button btnCheckRepHealth;
        private CheckBox cbxShowConsole;
        private GroupBox gbxComputerList;
        private CheckBox cbxIsVm;
        private Button btnAddWindowsServersOu;
        private Button btnAddPatriotParkOu;
        private Button btnAddWorkstationOu;
        private Button btnRemoveSelectedOus;
        private Button btnRemoveSelectedComputers;
        private Button btnLoadSelectedUser;
        private TabPage tabConsole;
        private Button btnUndockConsole;
        private Button btnAddSecurityGroupsOU;
        private CheckedListBox cbxListSecurityGroupsOu;
        private Label lblSecurityGroups;
        private Label lblDefaultSecurityGroup;
        private DataGridViewTextBoxColumn dgvPpComputerName;
        private DataGridViewTextBoxColumn clmPpUserName;
        private DataGridViewTextBoxColumn clmPpLocation;
        private DataGridViewTextBoxColumn clmWksComputerName;
        private DataGridViewTextBoxColumn clmWksUserName;
        private DataGridViewTextBoxColumn clmWksLocation;
        private Button btnLogout;
        private CheckedListBox cbxDefaultSecurityGroups;
        private TextBox txbSecurityGroupId;
        private Label lblDefaultSecurityGroupId;
        private TextBox txbSecurityGroupOther;
        private RadioButton rbnSecurityGroupOther;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private GroupBox gbxImportantVariables;
        private Label lblSecurityGroupKW;
        private Button btnSubmitVars;
        private TextBox txbSecurityGroupKW;
        private Label lblChangesSkippedSa1;
        private Label lblStatusForAgreementDataSa1;
        private Label lblStatusForAgreementSa1;
        private Label lblLastUpdateStatusSa1;
        private Label lblLastInitStartSa1;
        private Label lblReplicaEnabledSa1;
        private Label lblLastInitEndSa1;
        private Label lblLastInitStatusSa1;
        private Label lblUpdateInProgressSa1;
        private Label lblReplicationLagTimeSa1;
        private Label lblReplicationStatusSa1;
        private Label lblReapActiveSa1;
        private Label txbLastUpdateStatusDataSa1;
        private Label lblLastInitEndDataSa1;
        private Label lblLastInitStatusDataSa1;
        private Label lblLastInitStartDataSa1;
        private Label lblReapActiveDataSa1;
        private Label lblReplicationLagTimeDataSa1;
        private Label lblReplicationStatusDataSa1;
        private Label lblChangesSkippedDataSa1;
        private Label lblReplicaEnabledDataSa1;
        private Label lblUpdateInProgressDataSa1;
        private Label lblLastUpdateStartSa2;
        private Label lblLastUpdateStartDataSa2;
        private Label lblLastUpdateEndSa2;
        private Label lblLastUpdateEndDataSa2;
        private Label lblChangesSentSa2;
        private Label lblChangesSentDataSa2;
        private Label lblChangesSkippedSa2;
        private Label lblChangesSkippedDataSa2;
        private Label lblStatusForAgreementSa2;
        private Label lblStatusForAgreementDataSa2;
        private Label lblLastUpdateStatusSa2;
        private Label lblReplicationStatusDataSa2;
        private Label lblLastInitStartSa2;
        private Label lblLastInitStartDataSa2;
        private Label lblReplicaEnabledSa2;
        private Label lblReplicaEnabledDataSa2;
        private Label lblLastInitEndSa2;
        private Label lblLastInitEndDataSa2;
        private Label lblLastInitStatusSa2;
        private Label lblLastInitStatusDataSa2;
        private Label lblUpdateInProgressSa2;
        private Label lblUpdateInProgressDataSa2;
        private Label lblReplicationLagTimeSa2;
        private Label lblReplicationLagTimeDataSa2;
        private Label lblReplicationStatusSa2;
        private Label txbLastUpdateStatusDataSa2;
        private Label lblReapActiveSa2;
        private Label lblReapActiveDataSa2;
        private Label lblReplicaIDSa1;
        private Label lblReplicaIDDataSa1;
        private Label lblReplicaRootSa1;
        private Label lblReplicaStatusSa1;
        private Label lblReplicaStatusDataSa1;
        private Label lblReplicaRootDataSa1;
        private Label lblMaxCSNSa1;
        private Label lblMaxCSNDataSa1;
        private Label lblMaxCSNSa2;
        private Label lblMaxCSNDataSa2;
        private Label lblReplicaIDSa2;
        private Label lblReplicaIDDataSa2;
        private Label lblReplicaRootSa2;
        private Label lblReplicaStatusSa2;
        private Label lblReplicaStatusDataSa2;
        private Label lblReplicaRootDataSa2;
        private Label lblConfigFileLocationLabel;
        private Label lblFilePathLocation;
        private Label lblPowerCLIPathLabel;
        private Label lblPowerCLIPathLocation;
        private DateTimePicker dtpLogStartDate;
        private Label lblLogEndDate;
        private Label lblLogStartDate;
        private DateTimePicker dtpLogEndDate;
        private CheckBox chkLastHourOnly;
        private ComboBox cmbLogPriority;
        private Label lblPrioritySeverityLevel;
        private CheckBox cbxCaseSensitive;
        private TextBox txbLogKeyword;
        private Label lblKeywordSearch;
        private ComboBox cbxLogSourceSelection;
        private Label label1;
        private ComboBox cbxServerSelection;
        private Label lblServerSelection;
        private Button btnExportLogs;
        private Button btnClearLogs;
        private Button btnFetchLogs;
        private RichTextBox rtbLogOutput;
        private Label lblLogStatusResults;
        private Label lblLogStatus;
        private GroupBox gbxServerInstances;
        private Label lblLdapServerInstance1;
        private TextBox txbLdapServerInstace1;
        private Button btnSubmitServerInstance;
        private TextBox txbLdapServerInstace2;
        private Label lblLdapServerInstance2;
    }
}