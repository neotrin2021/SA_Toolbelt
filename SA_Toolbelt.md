**SA_ToolBelt Documentation**

**Introduction**

SA_ToolBelt is a comprehensive Windows Forms application designed to streamline and centralize system administration tasks in enterprise environments. Built for experienced system administrators who need powerful, efficient tools to manage Active Directory, LDAP directory services, VMware infrastructure, Linux systems, and various IT infrastructure components.

---

**What is SA_ToolBelt?**

SA_ToolBelt is a multi-tabbed administrative interface that consolidates commonly-performed system administration tasks into a single, unified application. Rather than juggling multiple tools, command-line interfaces, and management consoles, administrators can perform critical operations from one centralized location.

The application features a secure authentication system that verifies domain administrator privileges before granting access to administrative functions, ensuring that only authorized personnel can perform sensitive operations. All configuration is stored in a portable SQLite database, making the application easy to deploy and migrate between workstations.

**Why SA_ToolBelt is Useful**

**Efficiency and Time Savings**

- Eliminates the need to switch between multiple administrative tools
- Provides quick access to frequently-used operations through an intuitive interface
- Reduces the time required to perform routine administrative tasks

**Centralized Management**

- Brings together Active Directory management, LDAP directory services, and infrastructure monitoring
- Offers a consistent interface across different types of administrative operations
- Maintains operational context as administrators move between different tasks

**Enhanced Productivity**

- Real-time feedback through integrated console logging
- Bulk operations for managing multiple users, groups, or systems simultaneously
- Advanced search and filtering capabilities to quickly locate specific resources

**Operational Safety**

- Built-in confirmation dialogs for destructive operations
- Comprehensive logging of all administrative actions
- Secure authentication preventing unauthorized access

---

**Enterprise-Ready Features**

- Designed for domain environments with proper authentication
- Supports complex organizational structures and large user bases
- Scalable architecture that can handle enterprise-level Active Directory environments
- Portable SQLite database for configuration storage and easy migration

**Target Audience**

**SA_ToolBelt is designed for:**

- System Administrators managing Windows domain environments
- IT Support Personnel performing user account and group management
- Network Administrators needing consolidated tools for infrastructure management
- Help Desk Teams requiring efficient user account troubleshooting capabilities

**Key Capabilities Overview**

The application is organized into specialized tabs, each focusing on specific administrative domains:

- **Active Directory Management**: Complete user and group lifecycle management
- **LDAP Management**: User account creation for LDAP/RHDS directory services
- **Online/Offline Monitoring**: Real-time network status monitoring and computer categorization
- **SAPMIsSpice**: ESXi/VM health checks, filesystem monitoring, and LDAP replication health
- **Configuration**: Mandatory settings management, OU configuration, and computer list maintenance
- **Console**: Color-coded operational logging with undockable floating window support

Each tab provides both basic operations for daily tasks and advanced features for complex administrative scenarios, making SA_ToolBelt suitable for both routine maintenance and specialized administrative projects.

---

**System Requirements**

**Operating System Requirements**

Supported Operating Systems:

- Windows 10 (version 1809 or later)
- Windows 11 (all versions)
- Windows Server 2019
- Windows Server 2022
- Windows Server 2025

**Architecture:**

- x64 (64-bit) architecture required

**Software Dependencies**

**Microsoft .NET Runtime:**

- .NET 8.0 Runtime is included with the self-contained deployment
- No separate runtime installation required

**PuTTY (Optional):**

- Plink.exe required for Linux log fetching and LDAP replication health checks
- Must be in the system PATH or in the application directory

**VMware PowerCLI (Optional):**

- Required for ESXi and VM health check operations
- Module path configured through the Configuration tab's mandatory settings
- Loaded automatically in the background after login

**Active Directory Requirements:**

- Domain-joined computer (required for AD authentication)
- Active Directory Domain Services must be accessible
- Network connectivity to domain controllers

---

**Hardware Requirements**

Minimum Requirements:

- Processor: 1 GHz or faster processor
- Memory: 2 GB RAM
- Storage: 100 MB available disk space
- Display: 1024 x 768 screen resolution
- Network: Ethernet or Wi-Fi connectivity to domain network

**Recommended Requirements:**

- Processor: Multi-core processor (2 GHz or faster)
- Memory: 4 GB RAM or more
- Storage: 500 MB available disk space (for logs and temporary files)
- Display: 1920 x 1080 or higher resolution
- Network: Gigabit network connection for optimal performance

**Domain and Network Requirements**

**Active Directory Environment:**

- Windows Server 2012 R2 or later domain functional level
- Domain administrator privileges required for full functionality
- DNS resolution to domain controllers

**Network Connectivity:**

- Port 389 (LDAP) access to domain controllers
- Port 636 (LDAPS) for secure LDAP operations
- Port 88 (Kerberos) for authentication
- Port 22 (SSH) for Linux remote management features
- Port 443 (HTTPS) for vCenter Server connectivity
- Additional ports may be required for remote management features

**Firewall Considerations:**

- Windows Firewall exceptions may be needed for remote operations
- Corporate firewall rules should allow AD authentication traffic
- Remote management features require appropriate port access
- ICMP must be allowed for Online/Offline ping monitoring

---

**User Account Requirements**

**Administrative Privileges:**

- Domain Administrator group membership required for full functionality
- Local administrator rights on the computer running SA_ToolBelt
- Appropriate delegated permissions for specific AD operations

**Security Considerations:**

- Account should follow principle of least privilege when possible
- Multi-factor authentication recommended where supported
- Regular password rotation per organizational security policy

**Performance Considerations**

**Network Latency:**

- Sub-100ms latency to domain controllers recommended
- High-latency connections may result in slower operation performance
- Timeout values may need adjustment in high-latency environments

---

**Login and Authentication**

**Application Startup**

When SA_ToolBelt launches, users are presented with a secure login interface that serves as the gateway to all administrative functions. The application implements a security-first approach, ensuring that only authorized domain administrators can access the powerful tools within.

**Initial Interface:**

- Only the Login tab is visible upon startup
- All other administrative tabs remain hidden until successful authentication
- Logout and Undock Console buttons are hidden until login
- Clean, focused interface prevents unauthorized access attempts

**Authentication Process**

**Required Credentials:**

- Username: Domain administrator account username
- Password: Corresponding account password

**Authentication Flow:**

1. Enter domain administrator credentials
1. Click "Login" button or press Enter
1. Application verifies domain administrator group membership (Domain Admins, Enterprise Admins, or Built-in Administrator)
1. Application checks for the SQLite database configuration (see Configuration Setup below)
1. Upon successful authentication and configuration validation, all administrative tabs become available
1. Account counters begin populating with real-time data from the domain

**Security Features**

**Domain Administrator Verification:**

- Authentication goes beyond simple credential validation
- Application specifically verifies membership in domain administrator groups:
  - Domain Admins
  - Enterprise Admins
  - Built-in Administrator role
- Non-administrative domain accounts are denied access

**Password Security:**

- Password field uses secure masking (asterisk characters)
- "Show Password" button available for credential verification
  - Press and hold to temporarily reveal password
  - Automatically re-masks when button is released

**Session Management:**

- Authentication persists for the duration of the application session
- No automatic re-authentication required during normal operation
- Logout button available to return to the login screen without closing the application
- Logging out clears all stored credentials and resets the interface to its initial state

---

**Configuration Setup (First-Time Login)**

After successful authentication, the application checks the Windows Registry for the SQLite database location. There are three possible scenarios:

**Scenario 1: Database Found (Normal Login)**

- Registry key exists and points to a valid database file
- Application loads all settings from the database
- Mandatory settings textboxes are populated with stored values
- All administrative tabs become available
- VMware PowerCLI begins loading in the background
- Online/Offline status checks begin automatically

**Scenario 2: No Registry Key (First-Time Setup)**

- No registry entry exists (fresh installation)
- Only the Configuration tab is displayed
- All mandatory settings controls are disabled except the SQL Path browse button
- VCenter Server textbox is highlighted in red to indicate missing configuration
- User must browse to a folder location for the database:
  - If an existing database is found at the selected path, it is loaded automatically
  - If no database exists, all mandatory settings controls are enabled for first-time configuration
  - User fills in all required settings and clicks "Set All" to save

**Scenario 3: Registry Exists but Database Missing**

- Registry points to a path where the database file no longer exists
- Same behavior as Scenario 2 - user must browse to the correct location

---

**Login Interface Features**

**Keyboard Navigation:**

- Tab key moves between username and password fields
- Enter key in any field triggers authentication attempt

**Visual Feedback:**

- Login button changes to "Authenticating..." during verification process
- Clear success/failure messaging
- Professional blue border frame around login controls

**Error Handling:**

- Failed authentication displays clear error messages
- Network connectivity issues are reported appropriately
- Invalid credentials vs. insufficient privileges are distinguished

**Post-Authentication Behavior**

**Successful Login:**

- Welcome message confirms successful authentication
- All administrative tabs become immediately accessible
- Account counters begin populating with real-time data from domain
- VMware PowerCLI initialization starts in the background
- Online/Offline tab populates automatically

**Dynamic Counter Updates:**

- Upon successful login, the application queries Active Directory
- Radio button labels update with current account statistics:
  - Expiring accounts (by date ranges)
  - Expired accounts (by age categories)
  - Disabled accounts (by timeframes)
  - Currently locked accounts
- Counter updates provide immediate insight into domain health

---

**Logout**

The Logout button allows administrators to end their session without closing the application:

**Logout Process:**

1. Click the "Logout" button (visible in the toolbar after login)
1. Stored credentials are immediately cleared
1. Username and password fields are emptied
1. All administrative tabs are hidden
1. Application returns to the Login tab
1. Login button is reset and ready for a new session

**When to Use:**

- Switching to a different administrative account
- Stepping away from the workstation temporarily
- Ending an administrative session while keeping the application available

**Security Best Practices**

**Recommended Usage:**

- Use dedicated administrative accounts rather than personal accounts
- Ensure account follows organizational password policies
- Log out using the Logout button when the administrative session is complete
- Run application from secure administrative workstations only

**Network Security:**

- Application communicates directly with domain controllers
- All authentication traffic uses standard Windows security protocols
- No credentials are transmitted in plain text
- Kerberos authentication provides mutual authentication benefits

**Troubleshooting Authentication Issues**

**Common Issues and Solutions:**

**"Access Denied" Error:**

- Verify account has domain administrator privileges
- Check account is not disabled or locked
- Confirm account password is current

**"Cannot Connect to Active Directory" Error:**

- Verify network connectivity to domain controllers
- Check DNS resolution is functioning
- Ensure required ports are not blocked by firewall

**Application Unresponsive During Login:**

- Check network latency to domain controllers
- Verify domain controller availability
- Consider running from domain-joined computer with better connectivity

---

**Active Directory Management Tab**

**Overview**

The Active Directory tab serves as the central hub for user account lifecycle management and provides comprehensive tools for monitoring and maintaining domain user accounts. This tab offers both automated account discovery and detailed individual user management capabilities.

Upon successful authentication, the Active Directory tab automatically populates with real-time statistics showing the current state of domain accounts, providing immediate visibility into accounts requiring attention.

**Account Status Categories**

**Expiring Accounts**

The application provides three time-based categories for accounts approaching expiration:

**Accounts Expiring in 61 to 90 Days**

- Displays users whose accounts will expire within this timeframe
- Useful for advance planning and user notification
- Allows sufficient time for account extension or migration planning

**Accounts Expiring in 31 to 60 Days**

- Shows accounts requiring attention within the next month
- Ideal timeframe for initiating account renewal processes
- Provides adequate notice for user communication

**Accounts Expiring in 0 to 30 Days**

- Critical timeframe requiring immediate attention
- Users in this category may lose access soon
- Priority accounts for expedited processing

**Usage:**

1. Select the appropriate expiring accounts radio button
1. Click "Load" to retrieve matching accounts
1. Review accounts in the Results tab
1. Take appropriate action (extend accounts, notify users, etc.)

---

**Expired Accounts**

**Four categories organize accounts that have already expired:**

**Accounts Expired from 0 to 30 Days**

- Recently expired accounts that may need immediate reactivation
- Users likely still expecting access
- Priority for restoration if business justified

**Accounts Expired from 31 to 60 Days**

- Accounts expired for over a month
- May require verification before reactivation
- Consider if users still require access

**Accounts Expired from 61 to 90 Days**

- Longer-term expired accounts
- Likely candidates for permanent deactivation
- Review for business necessity before reactivation

**Accounts Expired 90+ Days**

- Long-term expired accounts
- Strong candidates for deletion after proper review
- May represent terminated employees or unused accounts

Usage:

1. Select the desired expired account timeframe
1. Click "Load" to retrieve accounts
1. Review each account's business necessity
1. Delete, disable, or reactivate as appropriate

---

**Disabled Accounts**

**Disabled account categories help identify accounts that have been manually deactivated:**

**Accounts Disabled < 30 Days**

- Recently disabled accounts
- May be temporary suspensions
- Review for reactivation potential

**Accounts Disabled from 30 to 59 Days**

- Medium-term disabled accounts
- Evaluate continued business need
- Consider permanent deletion if no longer needed

**Accounts Disabled from 60 to 89 Days**

- Longer-term disabled accounts
- Strong candidates for deletion
- Minimal likelihood of reactivation

**Accounts Disabled for 90+ Days**

- Long-term disabled accounts
- Priority candidates for permanent removal
- Likely represent terminated users or obsolete accounts

**Usage:**

1. Select the appropriate disabled accounts category
1. Click "Load" to populate results
1. Review accounts for deletion or reactivation
1. Clean up unnecessary disabled accounts to improve domain hygiene

---

**Locked Accounts**

**Accounts Locked Out**

- Displays users currently locked due to failed authentication attempts
- Indicates potential security issues or forgotten passwords
- Requires immediate attention to restore user access

**Common Causes:**

- Multiple failed password attempts
- Expired passwords with continued login attempts
- Potential security breaches or brute force attacks
- Users attempting login with old credentials

**Usage:**

1. Select "Accounts Locked Out" radio button
1. Click "Load" to view currently locked accounts
1. Review lock reasons and timing
1. Unlock legitimate accounts or investigate suspicious activity

**Single User Search**

**The Single User Search function provides detailed account management for individual users:**

**Search Capabilities:**

- Search by first name, last name, or username
- Multiple fields can be used simultaneously for refined searches
- Partial name matching supported (wildcards automatically applied)
- Typing in any search field automatically selects the Single User Search radio button

**Search Process:**

1. Select "Single User Search" radio button (or begin typing in a search field)
1. Enter search criteria in one or more fields:
  - First Name: User's given name
  - Last Name: User's surname
  - Username: SAM account name or login name
1. Click "Load" to execute search
1. Review results in the Results tab

---

**Results Handling:**

- Multiple matches display in the Results tab with basic information
- Single user results automatically switch to the General tab
- General tab provides comprehensive user details and management options

**Dynamic Counter Updates:**

Upon login, the application automatically queries Active Directory to populate real-time account statistics:

**Automatic Updates:**

- All radio button labels update with current account counts
- Provides immediate domain health overview
- Updates occur after successful authentication

**Counter Benefits:**

- Quick identification of accounts requiring attention
- Visual indication of domain maintenance needs
- Prioritization tool for administrative tasks

**Results Management**

**Results Tab:**

- Displays search results in tabular format
- Shows relevant columns based on search type
- Provides overview information for multiple accounts

**General Tab:**

- Detailed individual user information
- Account properties and settings
- Access to user modification functions

**Column Visibility:**

- Display adapts based on search type
- Expiring accounts show days remaining
- Expired accounts show days since expiration
- Disabled accounts show days disabled
- Locked accounts show lock date and unlock option

---

**Form Management**

**Clear Button**

The Clear button provides a quick way to reset the entire Active Directory interface to its initial state:

**Clear Button Functions:**

- Clears all Single User Search text fields (First Name, Last Name, Username)
- Unselects all radio buttons across all account categories
- Empties the Results tab DataGridView
- Resets all General tab labels to "N/A"
- Disables Single User Search textboxes (since no radio button is selected)

**When to Use Clear:**

- Starting a new search after completing previous tasks
- Clearing irrelevant results before beginning different account management
- Resetting the interface when switching between different types of operations
- Cleaning up the workspace for better focus

**Usage:**

1. Click the "Clear" button at any time
1. Interface immediately resets to startup state
1. Select new search criteria to begin fresh operations

The Clear function ensures a clean workspace and prevents confusion when switching between different account management tasks. This is particularly useful when performing multiple different types of account searches during a single session.

---

**Member Of Tab**

The Member Of tab provides comprehensive group membership management for the currently selected user. This tab is only populated when a user has been loaded through the Single User Search function or when viewing detailed results from other account searches.

**Primary Functions:**

**View Group Memberships**

- Displays all security groups the selected user belongs to
- Shows group names in an easy-to-scan checklist format
- Automatically populates when a user is loaded in the General tab

**Unified Group Management**

- "Edit Users Groups" button opens a comprehensive dialog for all group operations
- Single interface handles adding groups, removing groups, and copying from other users
- Change tracking ensures modifications are applied only when intended
- Apply/Cancel workflow prevents accidental changes

**Edit Groups Dialog Features:**

- **Current Groups Display**: CheckedListBox showing all available domain groups with the user's current memberships pre-checked
- **Add/Remove Groups**: Check or uncheck groups to add or remove the user from them
- **Copy Groups from Another User**: Search for another user by name, view their groups, and selectively copy group memberships
- **Change Detection**: The Apply button only enables when actual changes have been made compared to the original group membership
- **Get All Groups**: Button to retrieve all available groups in the domain for selection

**Usage Workflow:**

1. Load a user through Single User Search or account browsing
1. Switch to the Member Of tab to view current group memberships
1. Click "Edit Users Groups" to open the group management dialog
1. Make all desired changes (add, remove, copy groups) within the dialog
1. Click Apply to commit modifications to Active Directory
1. Dialog remains open for additional changes or can be closed when finished

---

**Password Management**

**Change Password Section**

The password change functionality provides secure password reset capabilities with real-time validation and optional account unlocking.

**Password Requirements:**

- Minimum 14 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

**Real-Time Validation:**

- Password requirements display as red labels initially
- Labels turn green as each requirement is met
- Visual feedback guides users to create compliant passwords
- Confirm password field shows green when passwords match and requirements are met

**Password Change Process:**

1. Load a user in the General tab
1. Enter new password in "New Password" field
1. Watch requirement labels turn green as criteria are met
1. Enter same password in "Confirm New Password" field
1. Submit button enables only when all requirements are satisfied
1. Optionally check "Unlock Account" to unlock account during password reset
1. Click "Submit" to apply password change

**Security Features:**

- Password fields are masked with asterisks by default
- "Show Password" button reveals password temporarily while pressed
- Confirmation dialog prevents accidental password changes
- Console logging tracks password change operations

---

**Test Password Section**

The test password functionality allows verification of user credentials without modifying anything.

**Usage:**

1. Ensure a user is loaded in the General tab
1. Enter password to test in the test password field
1. Click "Check" button to verify credentials
1. Result displays in console log (valid/invalid)
1. Password field clears automatically after test

**Use Cases:**

- Verify user knows their current password
- Test password validity before account unlock
- Confirm password changes were successful
- Troubleshoot authentication issues

**Account Management Functions**

**Unlock Account**

**Purpose:** Remove account lockout status for users locked due to failed login attempts

**Process:**

1. Load a locked user account in the General tab
1. Click "Unlock Account" button
1. Confirmation dialog appears
1. Account lockout status is immediately removed
1. User can attempt login again

**When to Use:**

- User account locked due to multiple failed password attempts
- Legitimate user forgotten password scenario
- After password reset to ensure account accessibility

---

**Disable Account**

**Purpose:** Disable a user account and move it to the designated Disabled Users OU

**Process:**

1. Load the user account in the General tab
1. Enter a reason for disabling in the "Disabled Reason" field
1. The "Processed By" field auto-populates with the logged-in administrator's username
1. Click "Disable" button
1. Account is disabled in Active Directory and moved to the configured Disabled Users OU

**When to Use:**

- Employee termination or separation
- Temporary account suspension pending investigation
- Account cleanup for users who no longer require access

**Set Account Expiration**

**Purpose:** Modify when user account will expire and become inaccessible

**Process:**

1. Load user account in the General tab
1. Use date/time picker to select new expiration date
1. Click "Update Expiration Date" button
1. Expiration date immediately updates in Active Directory
1. Updated date reflects in General tab display

**Common Scenarios:**

- Extending temporary account access
- Setting expiration for contractor accounts
- Adjusting expiration for role changes

**Delete Account**

**Purpose:** Permanently remove user account from Active Directory

**Critical Warnings:**

- This action cannot be undone
- All account data and group memberships are permanently lost
- Use extreme caution - consider disabling accounts instead

**Process:**

1. Load user account to be deleted in the General tab
1. Click "Delete Account" button
1. First confirmation dialog warns about permanent deletion
1. Second confirmation dialog provides final warning
1. Both confirmations must be accepted to proceed
1. Account is immediately and permanently removed

**Best Practices:**

- Always verify correct user is loaded before deletion
- Consider account disabling as alternative to deletion
- Document deletion reasoning per organizational policy
- Ensure account data backup if required by policy

---

**Clear Passwords Function**

**Purpose:** Reset password change interface to clean state

**Actions Performed:**

- Clears both password fields
- Resets all requirement labels to red (unmet)
- Disables Submit button
- Unchecks "Unlock Account" checkbox
- Resets confirm password color to default

**When to Use:**

- Starting over after password entry errors
- Switching between different users
- Clearing interface after successful password change

**Interface Navigation**

**Tab Selection:**

- Results tab displays multiple search results in tabular format
- General tab shows detailed single user information
- Member Of tab provides group membership management
- Automatic tab switching based on search results

**Console Integration:**

- All operations log to integrated console window
- Color-coded messages: green (success), red (error), yellow (warning), white (info)
- Detailed operation tracking for audit and troubleshooting
- Console remains visible throughout operations

---

**LDAP Management Tab**

**Overview**

The LDAP tab provides user account creation capabilities for LDAP directory services (RHDS - Red Hat Directory Server). This tab focuses specifically on creating new user accounts with proper Linux UID assignment and follows LDAP naming conventions requiring lowercase entries.

The tab implements a streamlined workflow for account creation with automatic username generation, UID management, comprehensive field validation, and optional security group assignment to ensure LDAP compliance.

**User Account Creation**

**Account Creation Form**

The LDAP tab presents a single grouped interface containing all fields required for new user account creation:

**Required Fields:**

- **First Name**: User's given name (must be lowercase)
- **Last Name**: User's surname (must be lowercase)
- **NT User ID**: Username for the account (automatically generated or manually entered)
- **Email Address**: Complete email address for the user
- **Phone**: User's contact telephone number
- **Linux UID**: Unique identifier for Linux systems (automatically assigned)
- **Temp Password**: Initial password for the new account

**Field Validation:**

- All entries must be in lowercase as required by LDAP standards
- Email address should follow standard email format
- Linux UID must be unique within the directory

---

**Username Generation**

The system provides automatic username generation following organizational standards:

**Generation Process:**

1. Enter First Name and Last Name
1. Click "Generate" button
1. System creates username using first letter of first name + complete last name
1. A temporary password is also auto-generated
1. Result appears in NT User ID field
1. Manual editing allowed if needed

**Generation Logic:**

- Takes first character of first name (lowercase)
- Combines with complete last name (lowercase)
- Removes special characters and spaces
- Validates minimum length requirements

**Example:**

- First Name: "john"
- Last Name: "smith"
- Generated Username: "jsmith"

**Linux UID Management**

The system automatically assigns unique Linux UIDs to prevent conflicts:

**UID Assignment Process:**

1. Click "Get UID" button next to Linux UID field
1. System queries existing LDAP directory for current UIDs
1. Excludes reserved UIDs (101, 276, 6000, 22941, 22942, 22943)
1. Calculates next available UID above existing maximum
1. Assigns UID with minimum value of 10000 for new accounts

---

**UID Validation:**

- Ensures uniqueness across the directory
- Maintains proper numbering sequence
- Prevents conflicts with system accounts
- Provides fallback to 10000 if calculation issues occur

**Security Group Assignment**

New user accounts can optionally be assigned to a default security group during creation:

**Default Security Groups Dropdown:**

- Populated automatically from the configured Security Groups OU (RHDS)
- Includes a "(None - Skip group assignment)" option as the default
- Groups are loaded asynchronously after login

**Assignment Process:**

1. Select a security group from the Default Security Groups dropdown (or leave as "None")
1. When account is created, the user is added to the selected group in both AD and RHDS
1. Console output confirms group assignment success or partial success
1. If no group is selected, the account is created without group membership

**Account Creation Process**

Complete account creation follows this workflow:

1. **Fill Required Fields**: Enter all user information (first name, last name, email, phone, temp password)
1. **Generate Username**: Use automatic generation or enter manually
1. **Assign UID**: Click "Get UID" for automatic assignment
1. **Select Security Group**: Choose a default group from the dropdown (optional)
1. **Validate Entries**: Ensure all fields are completed and lowercase
1. **Create Account**: Click "Create Account" to process
1. **Review Results**: Check console output for success/failure status

**Creation Details:**

- Account is created in both the RHDS directory and Active Directory
- User is added to the selected security group in both AD and RHDS (if a group was selected)
- All required fields must be completed
- Credentials must be authenticated (user must be logged into SA_ToolBelt)
- LDAP connectivity must be available
- UID must be unique

**Error Handling:**

- Missing field validation with user feedback
- LDAP connection error reporting
- UID conflict detection and resolution
- Comprehensive console logging of all operations

---

**Form Management**

**Clear Form Function:** The "Clear Form" button resets the entire interface:

- Clears all input fields
- Resets form to initial state
- Allows fresh account creation
- Maintains focus for efficient data entry

**Usage Scenarios:**

- Starting new account after completion
- Correcting errors across multiple fields
- Switching between different user accounts
- Preparing form for batch account creation

**Technical Requirements**

**Authentication:**

- Domain administrator authentication required
- LDAP connectivity through authenticated credentials
- Secure credential handling for directory operations

**LDAP Integration:**

- Direct LDAP directory services communication via RHDS
- Proper DN (Distinguished Name) construction
- Organizational Unit targeting for account placement
- Standard LDAP attribute mapping

**Input Validation:**

- Lowercase enforcement for LDAP compliance
- Field completion validation before submission
- UID uniqueness verification
- Email format validation

---

**Online/Offline Management Tab**

**Overview**

The Online/Offline tab provides real-time network status monitoring and computer categorization across the enterprise infrastructure. This tab displays all configured computers organized by function and location, with live ping status indicating accessibility for administrative operations.

The tab implements an intelligent sorting system that categorizes computers based on Organizational Unit membership and exception lists, providing administrators with immediate visibility into system availability across different operational categories.

**Computer Categories**

**Workstations**

Displays standard user workstations in a detailed DataGridView format showing computer assignments and user information:

**Display Format:**

- **Computer Name**: System hostname
- **User Name**: Assigned user extracted from AD description
- **Location**: Physical location parsed from AD description

**Description Parsing Logic:**

- Format: "Joe Doe - 128A - 2B - 4" or "Conf Rm 121"
- Multiple hyphens: Username before first hyphen, location after
- Single/no hyphens: Entire description becomes location
- Empty descriptions: Display as blank fields

**Color Coding:**

- **Light Green**: Computer responds to ping (Online)
- **Light Red**: Computer does not respond to ping (Offline)

**Data Source:**

- Computers retrieved from configured Workstation OUs
- Cross-referenced against Office Exempt exception list
- Office Exempt computers moved to separate category

---

**Patriot Park**

Specialized workstation category displayed in DataGridView format for specific facility computers:

**Display Format:**

- **Computer Name**: System hostname
- **User Name**: Assigned user from parsed description
- **Location**: Facility location information

**Description Parsing Logic:**

- Format: "Patriot Park 2831A - Joe Doe"
- Location before hyphen, username after hyphen
- Handles variations in description formatting
- Defaults to full description if parsing fails

**Color Coding:**

- **Light Green**: Computer online and accessible
- **Light Red**: Computer offline or unreachable

**Data Source:**

- Computers from configured Patriot Park OUs
- No exception list processing (all computers display in category)

**Windows Systems**

Standard Windows computers displayed in simple list format showing basic connectivity status:

**Display Format:**

- Computer name with online/offline status indicator
- Format: "COMPUTERNAME (Online)" or "COMPUTERNAME (Offline)"

**Sorting Logic:**

1. **Critical Windows**: Computers matching Critical Windows exception list
1. **Critical NAS**: Computers matching Critical NAS exception list
1. **Regular Windows**: All other Windows computers

---

**Data Source:**

- Computers retrieved from configured Windows OUs
- Sorted into subcategories based on exception lists

**Critical Windows**

High-priority Windows systems requiring special attention or monitoring:

**Display Characteristics:**

- Simple list format with online/offline status
- Separated from regular Windows systems for visibility
- Priority monitoring for critical infrastructure

**Assignment Method:**

- Manual configuration through Critical Windows Selection list
- Computers in this list take priority over other Windows categories

**Critical NAS**

Network Attached Storage and file server systems requiring priority monitoring:

**Display Characteristics:**

- List format with connectivity status
- Separated for storage infrastructure monitoring
- Critical for file access operations

**Assignment Method:**

- Manual configuration through Critical NAS Selection list
- Second priority after Critical Windows in sorting logic

**Gangs**

Specialized computer category for specific operational units:

**Display Characteristics:**

- Simple list format with online/offline status
- Direct assignment from configured OUs
- No exception list processing

---

**Data Source:**

- Computers from configured Gangs OUs
- All computers in category display without sorting

**Linux Systems**

Linux-based systems managed through configuration rather than Active Directory:

**Display Characteristics:**

- List format showing connectivity status
- Managed through manual configuration entries
- Separate from AD-based computer discovery

**Categories:**

- **Regular Linux**: Standard Linux systems
- **Critical Linux**: High-priority Linux infrastructure

**Data Source:**

- Manual configuration entries (not OU-based)
- Separate tracking for Linux and Critical Linux types

**Office Exempt**

Workstation computers excluded from standard processing and placed in separate monitoring category:

**Display Characteristics:**

- Simple list format with online/offline status
- Computers that would normally appear in Workstations DataGridView
- Exception handling for special-purpose workstations

**Assignment Logic:**

- Computers from Workstation OUs that match Office Exempt Selection list
- Takes priority over normal workstation categorization

---

**Status Monitoring**

**Online/Offline Detection**

Real-time network connectivity testing using high-performance parallel ping operations:

**Ping Implementation:**

- Asynchronous parallel ping operations
- 1-second timeout per computer
- Concurrent ping limiting (20 simultaneous operations)
- Optimized for enterprise-scale networks

**Status Indicators:**

- **(Online)**: Computer responds to ICMP ping
- **(Offline)**: Computer does not respond or unreachable

**Performance Optimization:**

- Batch processing of ping operations
- Semaphore-controlled concurrency
- Non-blocking UI during status checks

**Unreachable Computer Logging**

Automatic logging of offline computers for trend analysis and troubleshooting:

**Log File Details:**

- **File Location**: Configuration directory path
- **File Name**: "unreachable.csv"
- **Format**: Computer name, timestamp
- **Headers**: "Computer,Date,Time"

**Logging Triggers:**

- Failed ping responses
- Network timeout conditions
- Computer unreachable errors

---

**Operational Workflow**

**Initial Population**

Computer categorization and status checking occurs automatically upon successful application login:

**Population Process:**

1. **Authentication Verification**: Confirm domain administrator credentials
1. **Configuration Loading**: Read computer categories from the SQLite database
1. **OU Processing**: Query Active Directory for computers in configured OUs
1. **Master List Building**: Compile all computers with base categories
1. **Exception Processing**: Apply sorting rules based on exception lists
1. **Status Checking**: Perform parallel ping operations for connectivity
1. **UI Population**: Display categorized computers with status indicators

**Timing:**

- Executes once during application startup
- Provides immediate infrastructure overview
- No manual intervention required for initial display

**Manual Refresh**

Real-time status updates available through manual refresh operation:

**Refresh Process:**

1. Click "ReCheck Online/Offline Status" button
1. System maintains existing computer categorization
1. Performs fresh ping operations on all computers
1. Updates status indicators based on current connectivity
1. Refreshes unreachable computer logging

**Use Cases:**

- Verify system recovery after maintenance
- Check connectivity after network changes
- Monitor status during troubleshooting operations
- Confirm computer availability before remote operations

**Configuration Integration**

**Database-Driven Configuration**

Computer categories and exception lists are managed through the Configuration tab and stored in the SQLite database:

**Configuration Elements:**

- **Organizational Units**: OU paths for computer discovery
- **Exception Lists**: Manual computer assignments for special categories
- **Category Mappings**: Computer type assignments for sorting logic

**Dynamic Updates:**

- Configuration changes reflect on next refresh operation
- No application restart required for configuration updates
- Real-time adaptation to infrastructure changes

**Exception List Processing**

Sophisticated sorting logic handles multiple exception scenarios:

**Processing Priority:**

1. **Critical Windows**: Highest priority assignment
1. **Critical NAS**: Second priority for Windows computers
1. **Office Exempt**: Priority for Workstation computers
1. **Base Category**: Default assignment from OU membership

**Conflict Resolution:**

- Exception lists take priority over OU-based categorization
- Earlier processing priorities override later assignments
- Clear precedence rules prevent categorization conflicts

---

**Performance Characteristics**

**Scalability**

Designed for enterprise environments with hundreds of computers:

**Optimization Features:**

- Parallel OU queries reduce AD response time
- Concurrent ping operations minimize status check duration
- Efficient memory usage with DataTable structures
- Responsive UI through asynchronous operations

**Network Efficiency:**

- Batch ping operations reduce network overhead
- Configurable concurrency limits prevent network flooding
- Timeout controls balance speed with accuracy

---

**SAPMIsSpice Tab**

**Overview**

The SAPMIsSpice tab provides infrastructure health monitoring across three key areas: VMware ESXi/VM health, Linux filesystem utilization, and LDAP replication status. This tab consolidates critical infrastructure monitoring into a single interface, providing administrators with a comprehensive health overview.

**ESXi and VM Health Checks**

**ESXi Host Health**

Displays detailed health information for all ESXi hosts in the configured vCenter Server:

**Display Columns:**

- **Server Name**: ESXi host identifier
- **State**: Connection state (color-coded: green for Connected, red for other states)
- **Status**: Overall host status
- **Cluster**: Cluster membership
- **Consumed CPU**: CPU utilization percentage (color-coded: green < 60%, orange 60-80%, red > 80%)
- **Consumed Memory**: Memory utilization percentage (color-coded: green < 60%, orange 60-80%, red > 80%)
- **HA State**: High Availability status (Connected/Secondary or Running/Primary)
- **Uptime**: Days since last reboot

**VM Health**

Displays health information for all virtual machines:

**Display Columns:**

- **VM Name**: Virtual machine name
- **Power State**: Current power state (color-coded: green for PoweredOn, red for PoweredOff, orange for other)
- **VM Status**: Overall VM status (color-coded by severity)
- **Provisioned Space**: Total allocated disk space (GB)
- **Used Space**: Actual disk usage (GB)
- **Host CPU**: CPU allocation (MHz)
- **Host Memory**: Memory allocation (GB)

**Health Check Process:**

1. Click "Perform Health Check" button
1. Application connects to vCenter using configured credentials
1. ESXi host data is retrieved and displayed with color coding
1. VM data is retrieved and displayed with color coding
1. Previous results are cleared before new data is loaded
1. Console provides detailed progress and status messages

**Prerequisites:**

- VMware PowerCLI must be configured in mandatory settings
- VCenter Server must be accessible
- PowerCLI module loads automatically in the background after login

---

**File System Health Checks**

Monitors disk utilization on configured Linux servers via SSH:

**Monitored Servers:**

- Individual tabs for each configured server (e.g., ccelpro1, ccesec1, ccegitsvr1, ccesa1, ccesa2)
- Each server tab contains a DataGridView showing filesystem information

**Display Columns:**

- **FileSystem**: Filesystem device path
- **Size**: Total filesystem size
- **Used**: Space currently in use
- **Available**: Free space remaining
- **UsedPercent**: Percentage of space used
- **MountedOn**: Mount point path

**File System Check Process:**

1. Click "Check File System" button
1. Application connects to each server via SSH (using Plink)
1. Host keys are cached automatically to avoid interactive prompts
1. Disk information is retrieved from each server
1. Results populate in the corresponding server tab
1. Console logs progress for each server

**Prerequisites:**

- Plink.exe must be available (in PATH or application directory)
- SSH connectivity to target Linux servers
- Login credentials are used for SSH authentication (domain prefix stripped automatically)

---

**LDAP Replication Health Check**

Monitors replication status between LDAP directory servers (Red Hat Directory Server):

**Monitored Attributes:**

For each server (SA1 and SA2):

- **Last Update Start/End**: Timestamps of the most recent replication update
- **Changes Sent**: Number of changes replicated
- **Last Init Start**: Last initialization timestamp
- **Replica Enabled**: Whether replication is active
- **Last Init Status**: Result of the last initialization
- **Update In Progress**: Whether an update is currently running
- **Replication Lag Time**: Delay between servers
- **Replication Status**: Overall replication health
- **Reap Active**: Whether cleanup operations are running
- **Replica ID**: Unique identifier for the replica
- **Replica Root**: Base DN being replicated
- **Max CSN**: Change Sequence Number (most recent change)

**Replication Check Process:**

1. Click "Check Replication Health" button
1. A credential dialog prompts for Linux SSH credentials and target hostname
1. Application connects via SSH and runs the `dsconf` replication monitor command
1. Output is parsed and displayed in labeled fields for each server
1. Console provides detailed status throughout the operation

**Prerequisites:**

- Plink.exe must be available
- SSH access to the LDAP directory server
- Directory Manager credentials for the `dsconf` command

---

**Configuration Tab**

**Overview**

The Configuration tab provides centralized management for all application settings. It is divided into two main areas: Mandatory Settings (required for core application functionality) and OU/Computer List Configuration (for organizing monitored infrastructure).

All configuration data is persisted in a SQLite database, with the database file path tracked in the Windows Registry.

**Mandatory Settings**

The Mandatory Settings group box contains all required configuration fields:

**VCenter Server**

- **Field**: Text input for the vCenter Server hostname or IP address
- **Verify Button**: Pings the server to confirm accessibility
- **Validation**: Server must respond to ping to pass validation
- **Purpose**: Defines the VMware vCenter Server for ESXi/VM health checks

**PowerCLI Module Location**

- **Field**: Text input for the network share or local path containing VMware PowerCLI
- **Browse Button**: Opens a folder browser dialog
- **Validation**: Path must exist and contain a "VMware.PowerCLI" subfolder
- **Purpose**: Points to the PowerCLI module used for VMware operations

**SQL Database Path**

- **Field**: Text input showing the current database folder location
- **Browse Button**: Opens a folder browser dialog
- **Behavior on Browse**:
  - If an existing database is found at the selected path, it is loaded immediately
  - If no database exists, a new one will be created when "Set All" is clicked
- **Purpose**: Defines where the SQLite database is stored

**Excluded OUs**

- **Field**: Dropdown/ComboBox showing configured excluded OUs
- **Add Button**: Opens an OU selection dialog to browse and add OUs to exclude
- **Purpose**: OUs excluded from Active Directory searches (e.g., service accounts, system OUs)
- **Storage**: Multiple OUs stored as pipe-delimited values in the database

**Disabled Users Location**

- **Field**: Text input showing the target OU for disabled accounts
- **Browse Button**: Opens an OU selection dialog
- **Purpose**: Defines where disabled user accounts are moved to during the disable operation

**Home Directory Location**

- **Field**: Text input for the Linux home directory base path
- **Purpose**: Base path used by the Linux Service for home directory operations

**Linux DS Server**

- **Field**: Text input for the Linux Directory Server path
- **Browse Button**: Opens a folder browser dialog
- **Purpose**: Path to the Linux Directory Server configuration

---

**Set All Button**

The "Set All" button validates and saves all mandatory settings at once:

**Validation Steps:**

1. VCenter Server is pinged to verify accessibility (field turns red if invalid)
1. PowerCLI path is checked for the VMware.PowerCLI subfolder (field turns red if invalid)
1. SQL path is verified as a valid directory (field turns red if invalid)

**On Successful Validation:**

- Database is initialized (created if it doesn't exist)
- All settings are saved to the Toolbelt_Config table
- Excluded OUs are stored as pipe-delimited values
- Registry is updated with the database path
- All application tabs become available
- Settings are applied to application services

**On Validation Failure:**

- Invalid fields are highlighted in red (LightCoral)
- Error message lists all fields that need correction
- Settings are not saved until all required fields pass validation

**OU Configuration**

The Configuration tab provides management for Organizational Units used by the Online/Offline tab:

**OU Categories:**

- **Workstation OUs**: CheckedListBox for workstation computer discovery
- **Patriot Park OUs**: CheckedListBox for Patriot Park facility computers
- **Windows Server OUs**: CheckedListBox for Windows server discovery
- **Security Groups OUs**: CheckedListBox for RHDS security group discovery

**OU Management:**

- Each category has an "Add" button that opens an OU selection dialog
- Security Groups uses the RHDS OU selection dialog (browses the RHDS directory tree)
- All other categories use the AD OU selection dialog
- "Remove Selected OUs" button removes checked items from all categories
- OU configurations are stored in the ouConfiguration database table

---

**Computer List Management**

Manual computer list configuration for special categories:

**Available Lists:**

- **Critical Linux List**: High-priority Linux systems for monitoring
- **Linux List**: Standard Linux systems for monitoring
- **Office Exempt List**: Workstations excluded from standard categorization
- **Critical Windows List**: High-priority Windows systems
- **Critical NAS List**: Network Attached Storage systems

**Management:**

- Each list has a text input field and an "Add" button
- Enter a computer name and click Add to include it in the category
- "VM" checkbox available to mark computers as virtual machines
- "Remove Selected Computers" button removes checked items from all lists
- Computer lists are stored in the ComputerList database table

**Linux Log Server Configuration**

Server instance configuration for Linux log fetching:

- **Server Instance 1 / Server Instance 2**: Text fields for LDAP server hostnames
- **Submit Button**: Saves the server configuration
- **Purpose**: Defines which servers appear in the log fetching server selection dropdown
- **Storage**: Saved in the LogConfiguration database table

---

**Console Window**

**Overview**

The Console provides real-time operational logging for all SA_ToolBelt operations. Every action, result, warning, and error is logged with timestamps and color coding, providing a comprehensive audit trail and troubleshooting resource.

**Color-Coded Messages**

All console output includes a timestamp prefix (HH:mm:ss format):

- **Green**: Successful operations (account created, settings saved, etc.)
- **Red**: Errors and failures (connection errors, validation failures, etc.)
- **Yellow**: Warnings (database locked, missing configuration, etc.)
- **White**: Informational messages (operation in progress, status updates, etc.)

**Undock/Dock Functionality**

The console can be used in two modes:

**Docked Mode (Default):**

- Console appears as the Console tab within the main application
- Integrated with the tabbed interface
- Shares screen space with other tabs

**Undocked (Floating) Mode:**

- Click the "Undock Console" button in the toolbar to detach the console
- Console becomes a separate floating window
- Can be positioned on a second monitor for continuous monitoring
- Main application tabs remain fully accessible without switching away from console output
- Click "Dock" button on the floating window to return the console to its tab

**Thread Safety:**

- Console output is thread-safe and can receive messages from background operations
- All writes are marshaled to the UI thread via Invoke when necessary
- Background operations (PowerCLI loading, ping checks, SSH commands) can log freely

**Window Behavior:**

- Closing the floating console window hides it rather than destroying it
- Console content is preserved when docking/undocking
- Auto-scrolls to the most recent message

---

**Linux Log Fetching**

**Overview**

The Linux log fetching functionality provides remote log retrieval from configured Linux servers via SSH. Logs are fetched using journalctl or file-based log reading, with filtering options for targeted log analysis.

**Server Selection**

- **Server Dropdown**: Lists configured log servers (defined in Configuration tab)
- **Log Source Dropdown**: Select the type of logs to retrieve:
  - All (journalctl - all system logs)
  - Security/Authentication (sshd and systemd-logind units)
  - Directory Server Errors (file-based: /var/log/dirsrv/{instance}/errors)
  - Directory Server Access (file-based: /var/log/dirsrv/{instance}/access)
  - Directory Server Audit (file-based: /var/log/dirsrv/{instance}/audit)

**Filtering Options**

- **Priority Level**: Filter by log severity (Emergency, Alert, Critical, Error, Warning, Notice, Info, Debug, or All Levels)
- **Keyword Search**: Text-based filtering with optional case sensitivity
- **Date Range**: Start and end date/time pickers for time-bounded searches
- **Last Hour Only**: Checkbox shortcut to fetch only the most recent hour of logs (disables date pickers when checked)

**Log Fetch Process:**

1. Select a server from the dropdown
1. Select the log source type
1. Configure filters (priority, keyword, date range) as needed
1. Click "Fetch Logs" button
1. Application connects via SSH and executes the appropriate command
1. Results display in the log output area with a line count summary
1. Status label updates with the number of lines fetched

**Log Management:**

- **Clear Logs**: Clears the log output display and resets the status
- **Export Logs**: Opens a Save File dialog to export displayed logs to a text file
  - Default filename: logs_YYYYMMDD_HHMMSS.txt
  - Supported formats: .txt, .log, or any file type

**Prerequisites:**

- Plink.exe must be available for SSH connectivity
- Login credentials are used for SSH authentication
- Servers must be configured in the Configuration tab's log server settings

---

**Database Architecture**

**Overview**

SA_ToolBelt uses a SQLite database for all configuration storage. The database file location is tracked in the Windows Registry at `HKCU\SOFTWARE\SA_Toolbelt\SqlPath`.

**Database Tables**

**Toolbelt_Config (Single Row)**

Stores the core application settings:

| Column | Purpose |
|---|---|
| VCenter_Server | VMware vCenter Server hostname/IP |
| PowerCLI_Location | Path to PowerCLI module directory |
| Sql_Path | Database folder path |
| Excluded_OU | Pipe-delimited list of excluded OU paths |
| Disabled_Users_Ou | Target OU for disabled user accounts |
| HomeDirectory | Linux home directory base path |

**ComputerList**

Stores manually configured computer entries for Online/Offline monitoring:

| Column | Purpose |
|---|---|
| Computername | Computer hostname |
| Type | Category (CriticalLinux, Linux, OfficeExempt, CriticalWindows, CriticalNAS) |
| VMWare | Whether the computer is a virtual machine |
| Instructions | Additional notes or instructions |

**LogConfiguration**

Stores Linux log server configuration:

| Column | Purpose |
|---|---|
| Server | Server hostname |
| server_instance | Directory Server instance name |

**ouConfiguration**

Stores OU paths for computer discovery:

| Column | Purpose |
|---|---|
| ou | Full OU distinguished name path |
| MiddleName | Category (Workstations, PatriotPark, WindowsServers, SecurityGroups) |
| keyword | Additional classification keyword |

**GPO_Processing**

Stores Group Policy Object configuration:

| Column | Purpose |
|---|---|
| AD_Location | Active Directory location path |
| GPO_Name | Group Policy Object name |
| Policy_Location | Policy file location |
| Policy_Name | Policy setting name |
| Setting | Policy setting identifier |
| Value | Policy setting value |

---

**Appendix: Services Architecture**

SA_ToolBelt uses a modular service architecture where each service handles a specific domain:

**AD_Service**

- Active Directory user and group operations
- Account search, creation, modification, and deletion
- Group membership management
- Excluded OU filtering for search operations

**RHDS_Service**

- Red Hat Directory Server operations
- LDAP user account creation
- UID management and assignment
- Security group management in RHDS

**Linux_Service**

- SSH operations via Plink
- Remote log fetching (journalctl and file-based)
- Filesystem disk information retrieval
- LDAP replication monitoring command execution
- Host key caching for non-interactive SSH sessions

**VMwareManager**

- PowerShell runspace management for PowerCLI
- ESXi host health data retrieval
- Virtual machine health data retrieval
- vCenter Server connection management
- Automatic runspace recovery after idle periods

**DatabaseService**

- SQLite database initialization and management
- Configuration read/write operations
- Database lock detection and handling
- Registry path management

**CredentialManager**

- Secure storage of login credentials for the session
- Provides credentials to all services that need authentication
- Credentials cleared on logout

**ConsoleForm**

- Color-coded operational logging
- Thread-safe message writing
- Undockable floating window support
- Timestamp-prefixed output
