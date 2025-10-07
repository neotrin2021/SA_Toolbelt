using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Threading.Tasks;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace SA_ToolBelt
{
    public class AD_Service
    {
        private readonly string _domain;
        private readonly string _domainController;
        private readonly ConsoleForm _consoleForm;

        // Add these excluded OU paths
        private readonly HashSet<string> _excludedOUs = new HashSet<string>
        {
            "spectre.afspc.af.smil.mil/Users",
            "spectre.afspc.af.smil.mil/spectre/people/MGMT USERS",
            "spectre.afspc.af.smil.mil/spectre/people/Disabled Users"
        };

        public AD_Service(ConsoleForm consoleForm = null, string? domain = null, string? domainController = null)
        {
            _consoleForm = consoleForm;
            _domain = domain ?? Environment.UserDomainName;
            _domainController = domainController;
        }

        #region Get/Find Users

        /// <summary>
        /// Get user information by username
        /// </summary>
        public UserPrincipal GetUser(string username)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    return UserPrincipal.FindByIdentity(context, username);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user {username}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if user exists in Active Directory
        /// </summary>
        public bool UserExists(string username)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    return user != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all users in a specific OU or domain
        /// </summary>
        public List<UserPrincipal> GetAllUsers(string organizationalUnit = null)
        {
            var users = new List<UserPrincipal>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain, organizationalUnit))
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        if (result is UserPrincipal user)
                        {
                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving users: {ex.Message}", ex);
            }

            return users;
        }

        /// <summary>
        /// Search for users by display name or username
        /// </summary>
        public List<UserPrincipal> SearchUsers(string searchTerm)
        {
            var users = new List<UserPrincipal>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    var userFilter = new UserPrincipal(context)
                    {
                        DisplayName = $"*{searchTerm}*"
                    };

                    using (var searcher = new PrincipalSearcher(userFilter))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user)
                            {
                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching users: {ex.Message}", ex);
            }

            return users;
        }

        // Add this method to check if a user should be excluded based on their OU
        private bool ShouldExcludeUser(UserPrincipal user)
        {
            try
            {
                if (user?.DistinguishedName == null)
                    return false;

                // Check if the user's DN contains any of the excluded OU paths
                foreach (var excludedOU in _excludedOUs)
                {
                    if (user.DistinguishedName.Contains(excludedOU))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false; // If we can't determine, don't exclude
            }
        }
        /// <summary>
        /// Search for users by multiple criteria
        /// </summary>
        public List<UserPrincipal> SearchUsersByMultipleFields(string? loginName = null, string? firstName = null, string? lastName = null)
        {
            var users = new List<UserPrincipal>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    var userFilter = new UserPrincipal(context);

                    if (!string.IsNullOrEmpty(loginName))
                        userFilter.SamAccountName = $"*{loginName}*";

                    if (!string.IsNullOrEmpty(firstName))
                        userFilter.GivenName = $"*{firstName}*";

                    if (!string.IsNullOrEmpty(lastName))
                        userFilter.Surname = $"*{lastName}*";

                    using (var searcher = new PrincipalSearcher(userFilter))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user)
                            {
                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching users: {ex.Message}", ex);
            }

            return users;
        }
        #endregion

        #region Modify User Accounts
        /// <summary>
        /// Enable or disable a user account
        /// </summary>
        public bool SetUserAccountStatus(string username, bool enabled)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    if (user != null)
                    {
                        user.Enabled = enabled;
                        user.Save();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting account status for {username}: {ex.Message}", ex);
            }

            return false;
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        public bool ResetUserPassword(string username, string newPassword)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    if (user != null)
                    {
                        user.SetPassword(newPassword);
                        user.Save();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error resetting password for {username}: {ex.Message}", ex);
            }

            return false;
        }

        /// <summary>
        /// Unlock a user account
        /// </summary>
        public bool UnlockUserAccount(string username)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    if (user != null)
                    {
                        user.UnlockAccount();
                        user.Save();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error unlocking account for {username}: {ex.Message}", ex);
            }

            return false;
        }

        /// <summary>
        /// Set user account expiration date
        /// </summary>
        public bool SetUserExpirationDate(string username, DateTime? expirationDate)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    if (user != null)
                    {
                        user.AccountExpirationDate = expirationDate;
                        user.Save();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error setting expiration date for {username}: {ex.Message}", ex);
            }

            return false;
        }

        /// <summary>
        /// Delete user account (USE WITH EXTREME CAUTION)
        /// </summary>
        public bool DeleteUserAccount(string username)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    if (user != null)
                    {
                        user.Delete();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting account {username}: {ex.Message}", ex);
            }

            return false;
        }
        #endregion

        #region Expired, Expiring, Disabled, Locked

        /// <summary>
        /// Get the actual disabled date by parsing Description field first, then falling back to WhenChanged
        /// </summary>
        /// <param name="user">UserInfo object</param>
        /// <returns>DateTime when account was disabled, or null if not found</returns>
        public static DateTime? GetAccountDisabledDate(UserInfo user)
        {
            // First try to extract date from Description field using comprehensive parsing
            if (!string.IsNullOrWhiteSpace(user.Description))
            {
                // Common date patterns people use in descriptions
                var datePatterns = new[]
                {
            // MM/dd/yyyy or M/d/yyyy formats
            @"\b(\d{1,2})[/\-](\d{1,2})[/\-](\d{4})\b",
            
            // MM/dd/yy or M/d/yy formats  
            @"\b(\d{1,2})[/\-](\d{1,2})[/\-](\d{2})\b",
            
            // MMDDYYYY format (like 09082025)
            @"\b(\d{2})(\d{2})(\d{4})\b",
            
            // Month name formats (Sept 08, 2025 or September 8, 2025)
            @"\b(Jan|January|Feb|February|Mar|March|Apr|April|May|Jun|June|Jul|July|Aug|August|Sep|Sept|September|Oct|October|Nov|November|Dec|December)\s+(\d{1,2}),?\s+(\d{4})\b",
            
            // Reverse format: 2025-09-08 or 2025/09/08
            @"\b(\d{4})[/\-](\d{1,2})[/\-](\d{1,2})\b"
        };

                foreach (var pattern in datePatterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(user.Description, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        try
                        {
                            // Handle month name format
                            if (pattern.Contains("Jan|January"))
                            {
                                var monthStr = match.Groups[1].Value.ToLower();
                                var day = int.Parse(match.Groups[2].Value);
                                var year = int.Parse(match.Groups[3].Value);

                                var monthMap = new Dictionary<string, int>
                        {
                            {"jan", 1}, {"january", 1}, {"feb", 2}, {"february", 2}, {"mar", 3}, {"march", 3},
                            {"apr", 4}, {"april", 4}, {"may", 5}, {"jun", 6}, {"june", 6}, {"jul", 7}, {"july", 7},
                            {"aug", 8}, {"august", 8}, {"sep", 9}, {"sept", 9}, {"september", 9},
                            {"oct", 10}, {"october", 10}, {"nov", 11}, {"november", 11}, {"dec", 12}, {"december", 12}
                        };

                                if (monthMap.ContainsKey(monthStr))
                                {
                                    return new DateTime(year, monthMap[monthStr], day);
                                }
                            }
                            // Handle MMDDYYYY format
                            else if (match.Groups.Count == 4 && match.Groups[3].Value.Length == 4)
                            {
                                if (pattern.Contains(@"(\d{2})(\d{2})(\d{4})"))
                                {
                                    var month = int.Parse(match.Groups[1].Value);
                                    var day = int.Parse(match.Groups[2].Value);
                                    var year = int.Parse(match.Groups[3].Value);
                                    return new DateTime(year, month, day);
                                }
                                // Handle year-first format: 2025-09-08
                                else if (match.Groups[1].Value.Length == 4)
                                {
                                    var year = int.Parse(match.Groups[1].Value);
                                    var month = int.Parse(match.Groups[2].Value);
                                    var day = int.Parse(match.Groups[3].Value);
                                    return new DateTime(year, month, day);
                                }
                                // Handle MM/dd/yyyy format
                                else
                                {
                                    var month = int.Parse(match.Groups[1].Value);
                                    var day = int.Parse(match.Groups[2].Value);
                                    var yearStr = match.Groups[3].Value;

                                    var year = yearStr.Length == 2 ?
                                        (int.Parse(yearStr) > 50 ? 1900 + int.Parse(yearStr) : 2000 + int.Parse(yearStr)) :
                                        int.Parse(yearStr);

                                    return new DateTime(year, month, day);
                                }
                            }
                        }
                        catch
                        {
                            continue; // Try next pattern if this one fails
                        }
                    }
                }
            }

            // Fall back to WhenChanged if no valid date found in description
            return user.WhenChanged;
        }
        /// <summary>
        /// Get disabled users within a specific date range by parsing Description field dates
        /// Returns DisabledUserInfo objects containing user details and parsed disabled date
        /// </summary>
        public async Task<List<DisabledUserInfo>> GetDisabledUsersInDateRange(DateTime startDate, DateTime endDate)
        {
            var disabledUsers = new List<DisabledUserInfo>();

            await Task.Run(() =>
            {
                try
                {
                    using (var context = new PrincipalContext(ContextType.Domain, _domain))
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user)
                            {
                                try
                                {
                                    // Skip excluded OUs and usernames
                                    if (ShouldExcludeUser(user))
                                        continue;

                                    // Skip if account is enabled
                                    if (user.Enabled == true)
                                        continue;

                                    // Convert to UserInfo DTO first
                                    var userInfo = new UserInfo(user);

                                    // Get disabled date using the existing GetAccountDisabledDate method
                                    var disabledDate = GetAccountDisabledDate(userInfo);

                                    if (!disabledDate.HasValue)
                                        continue;

                                    // Check if disabled date falls in our range
                                    if (disabledDate.Value >= startDate && disabledDate.Value <= endDate)
                                    {
                                        disabledUsers.Add(new DisabledUserInfo
                                        {
                                            SamAccountName = userInfo.SamAccountName,
                                            GivenName = userInfo.GivenName,
                                            Surname = userInfo.Surname,
                                            LastModified = disabledDate.Value
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _consoleForm?.WriteError($"Error processing user {user.SamAccountName}: {ex.Message}");
                                }
                            }
                            result?.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Error searching disabled users: {ex.Message}");
                    throw new Exception($"Error searching disabled users: {ex.Message}", ex);
                }
            });

            return disabledUsers;
        }

        /// <summary>
        /// Get count of accounts expiring in date range
        /// </summary>
        public async Task<int> GetExpiringAccountsCountAsync(int minDays, int maxDays = 0)
        {
            try
            {
                var users = await GetUsersExpiringInDateRangeAsInfoAsync(minDays, maxDays);
                return users.Count;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting expiring accounts count: {ex.Message}");
                return 0;
            }
        }
        /// <summary>
        /// Get users expiring within specified date range as DTOs
        /// </summary>
        public async Task<List<UserInfo>> GetUsersExpiringInDateRangeAsInfoAsync(int daysFromNow, int daysToNow)
        {
            var users = new List<UserInfo>();
            var startDate = DateTime.Now.AddDays(daysFromNow);
            var endDate = DateTime.Now.AddDays(daysToNow);

            try
            {
                await Task.Run(() =>
                {
                    using (var context = new PrincipalContext(ContextType.Domain, _domain))
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user && user.AccountExpirationDate.HasValue)
                            {
                                try
                                {
                                    // Skip excluded OUs
                                    if (ShouldExcludeUser(user))
                                        continue;

                                    // Skip disabled accounts
                                    if (!user.Enabled.HasValue || !user.Enabled.Value)
                                        continue;

                                    var expDate = user.AccountExpirationDate.Value;
                                    if (expDate >= startDate && expDate <= endDate)
                                    {
                                        users.Add(new UserInfo(user));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _consoleForm?.WriteError($"Error processing user {user.SamAccountName}: {ex.Message}");
                                }
                            }
                            result.Dispose();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error retrieving expiring users: {ex.Message}");
                throw new Exception($"Error retrieving expiring users: {ex.Message}", ex);
            }

            return users;
        }
        /// <summary>
        /// Get count of expired accounts in date range
        /// </summary>
        public async Task<int> GetExpiredAccountsCountAsync(int minDays, int maxDays = 0)
        {
            try
            {
                var users = await GetUsersExpiredInDateRangeAsInfoAsync(minDays, maxDays);
                return users.Count;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting expired accounts count: {ex.Message}");
                return 0;
            }
        }
        /// <summary>
        /// Get users expired within specified date range as DTOs
        /// </summary>
        public async Task<List<UserInfo>> GetUsersExpiredInDateRangeAsInfoAsync(int daysAgo, int daysToAgo)
        {
            var users = new List<UserInfo>();
            var startDate = DateTime.Now.AddDays(-daysAgo);
            var endDate = DateTime.Now.AddDays(-daysToAgo);

            try
            {
                await Task.Run(() =>
                {
                    using (var context = new PrincipalContext(ContextType.Domain, _domain))
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user && user.AccountExpirationDate.HasValue)
                            {
                                try
                                {
                                    // Skip excluded OUs
                                    if (ShouldExcludeUser(user))
                                        continue;

                                    // Skip disabled accounts  
                                    if (!user.Enabled.HasValue || !user.Enabled.Value)
                                        continue;

                                    var expDate = user.AccountExpirationDate.Value;
                                    if (expDate >= startDate && expDate <= endDate && expDate < DateTime.Now)
                                    {
                                        users.Add(new UserInfo(user));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _consoleForm?.WriteError($"Error processing user {user.SamAccountName}: {ex.Message}");
                                }
                            }
                            result.Dispose();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error retrieving expired users: {ex.Message}");
                throw new Exception($"Error retrieving expired users: {ex.Message}", ex);
            }

            return users;
        }
        /// <summary>
        /// Get count of locked accounts (DTO-friendly version)
        /// </summary>
        public async Task<int> GetLockedAccountsCountAsync()
        {
            try
            {
                var allUsers = await GetAllUsersAsInfoAsync();
                return allUsers.Count(u => u.IsAccountLockedOut());
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting locked accounts count: {ex.Message}");
                return 0;
            }
        }
        /// <summary>
        /// DTO for disabled user information with parsed disabled date
        /// </summary>
        public class DisabledUserInfo
        {
            public string SamAccountName { get; set; }
            public string GivenName { get; set; }
            public string Surname { get; set; }
            public DateTime LastModified { get; set; }
        }
        #endregion

        #region Group Operations
        public class GroupInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string DistinguishedName { get; set; }
            public string DisplayName { get; set; }
            public DateTime? WhenCreated { get; set; }
            public DateTime? WhenChanged { get; set; }
            public string ManagedBy { get; set; }
            public string GroupType { get; set; }

            public GroupInfo() { }

            public GroupInfo(GroupPrincipal group)
            {
                Name = group.Name;
                Description = group.Description;
                DistinguishedName = group.DistinguishedName;
                DisplayName = group.DisplayName;

                // Get additional properties from DirectoryEntry
                try
                {
                    var entry = group.GetUnderlyingObject() as DirectoryEntry;
                    if (entry != null)
                    {
                        WhenCreated = GetDateProperty(entry, "whenCreated");
                        WhenChanged = GetDateProperty(entry, "whenChanged");
                        ManagedBy = GetProperty(entry, "managedBy");
                        GroupType = GetProperty(entry, "groupType");
                    }
                }
                catch (Exception)
                {
                    // If we can't get additional properties, just use what we have
                }
            }

            private string GetProperty(DirectoryEntry entry, string propertyName)
            {
                return entry.Properties.Contains(propertyName) && entry.Properties[propertyName].Count > 0
                    ? entry.Properties[propertyName].Value?.ToString() ?? ""
                    : "";
            }

            private DateTime? GetDateProperty(DirectoryEntry entry, string propertyName)
            {
                if (entry.Properties.Contains(propertyName) && entry.Properties[propertyName].Count > 0)
                {
                    var value = entry.Properties[propertyName].Value;
                    if (value is DateTime dateTime)
                        return dateTime;
                    if (value is long fileTime && fileTime > 0)
                        return DateTime.FromFileTime(fileTime);
                }
                return null;
            }
        }
        /// <summary>
        /// Get group information by group name
        /// </summary>
        public GroupPrincipal GetGroup(string groupName)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    return GroupPrincipal.FindByIdentity(context, groupName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving group {groupName}: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Get user groups as GroupInfo DTOs (100% DTO-compliant)
        /// </summary>
        public List<GroupInfo> GetUserGroups(string username)
        {
            var groupInfos = new List<GroupInfo>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var user = UserPrincipal.FindByIdentity(context, username))
                {
                    if (user != null)
                    {
                        var groups = user.GetGroups();

                        foreach (var group in groups)
                        {
                            try
                            {
                                if (group is GroupPrincipal groupPrincipal)
                                {
                                    // Convert to DTO immediately while context is alive
                                    groupInfos.Add(new GroupInfo(groupPrincipal));
                                }
                            }
                            catch (Exception ex)
                            {
                                _consoleForm?.WriteError($"Error processing group for user {username}: {ex.Message}");
                            }
                            finally
                            {
                                group?.Dispose(); // Ensure each group gets disposed
                            }
                        }

                        _consoleForm?.WriteInfo($"Retrieved {groupInfos.Count} groups for user {username}");
                    }
                    else
                    {
                        _consoleForm?.WriteWarning($"User not found: {username}");
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error retrieving groups for user {username}: {ex.Message}");
                throw new Exception($"Error retrieving groups for user {username}: {ex.Message}", ex);
            }

            return groupInfos;
        }
        /*
         * The below code is needed by AddGroupsForm.cs
         * 
         */
        /// <summary>
        /// Get all groups as GroupInfo DTOs
        /// </summary>
        public async Task<List<GroupInfo>> GetAllGroupsAsync()
        {
            var groupInfos = new List<GroupInfo>();

            try
            {
                await Task.Run(() =>
                {
                    using (var context = new PrincipalContext(ContextType.Domain, _domain))
                    using (var searcher = new PrincipalSearcher(new GroupPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            try
                            {
                                if (result is GroupPrincipal group)
                                {
                                    groupInfos.Add(new GroupInfo(group));
                                }
                            }
                            catch (Exception ex)
                            {
                                _consoleForm?.WriteError($"Error processing group {result.Name}: {ex.Message}");
                            }
                            finally
                            {
                                result?.Dispose();
                            }
                        }
                    }
                });

                _consoleForm?.WriteInfo($"Retrieved {groupInfos.Count} groups from Active Directory");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error retrieving all groups: {ex.Message}");
                throw new Exception($"Error retrieving all groups: {ex.Message}", ex);
            }

            return groupInfos;
        }

        /// <summary>
        /// Search for groups by name as GroupInfo DTOs
        /// </summary>
        public List<GroupInfo> SearchGroups(string searchTerm)
        {
            var groupInfos = new List<GroupInfo>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    var groupFilter = new GroupPrincipal(context)
                    {
                        Name = $"*{searchTerm}*"
                    };

                    using (var searcher = new PrincipalSearcher(groupFilter))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            try
                            {
                                if (result is GroupPrincipal group)
                                {
                                    groupInfos.Add(new GroupInfo(group));
                                }
                            }
                            catch (Exception ex)
                            {
                                _consoleForm?.WriteError($"Error processing group {result.Name}: {ex.Message}");
                            }
                            finally
                            {
                                result?.Dispose();
                            }
                        }
                    }
                }

                _consoleForm?.WriteInfo($"Found {groupInfos.Count} groups matching '{searchTerm}'");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error searching groups: {ex.Message}");
                throw new Exception($"Error searching groups: {ex.Message}", ex);
            }

            return groupInfos;
        }
        /*
         * The above code is needed by AddGroupsForm.cs
         * 
         */
        /// <summary>
        /// Remove user from all security groups (except Domain Users) and add to pending_removal group
        /// </summary>
        
        /// Method used when disabling users
        public bool RemoveUserFromAllGroupsAndAddToPendingRemoval(string username)
        {
            try
            {
                _consoleForm?.WriteInfo($"Starting group cleanup for user: {username}");

                int removedCount = 0;
                int failedCount = 0;

                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var user = UserPrincipal.FindByIdentity(context, username))
                {
                    if (user == null)
                    {
                        _consoleForm?.WriteError($"User not found: {username}");
                        return false;
                    }

                    // Get all current groups
                    var userGroups = user.GetGroups().ToList();
                    _consoleForm?.WriteInfo($"User is member of {userGroups.Count} groups");

                    // Remove from all groups (except Domain Users)
                    foreach (var group in userGroups)
                    {
                        try
                        {
                            if (group is GroupPrincipal groupPrincipal)
                            {
                                // Skip Domain Users (primary group, cannot be removed)
                                if (groupPrincipal.Name.Equals("Domain Users", StringComparison.OrdinalIgnoreCase))
                                {
                                    _consoleForm?.WriteInfo($"Skipping Domain Users (primary group)");
                                    continue;
                                }

                                // Remove user from this group
                                groupPrincipal.Members.Remove(user);
                                groupPrincipal.Save();
                                removedCount++;
                                _consoleForm?.WriteInfo($"Removed from group: {groupPrincipal.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            _consoleForm?.WriteWarning($"Could not remove from group {group.Name}: {ex.Message}");
                        }
                        finally
                        {
                            group?.Dispose();
                        }
                    }

                    _consoleForm?.WriteSuccess($"Removed user from {removedCount} security groups");

                    if (failedCount > 0)
                    {
                        _consoleForm?.WriteWarning($"Failed to remove from {failedCount} groups");
                    }

                    // Add to pending_removal group
                    try
                    {
                        using (var pendingRemovalGroup = GroupPrincipal.FindByIdentity(context, "pending_removal"))
                        {
                            if (pendingRemovalGroup != null)
                            {
                                // Check if already a member
                                bool alreadyMember = false;
                                foreach (var member in pendingRemovalGroup.Members)
                                {
                                    if (member.SamAccountName == username)
                                    {
                                        alreadyMember = true;
                                        break;
                                    }
                                }

                                if (!alreadyMember)
                                {
                                    pendingRemovalGroup.Members.Add(user);
                                    pendingRemovalGroup.Save();
                                    _consoleForm?.WriteSuccess("Added user to pending_removal group");
                                }
                                else
                                {
                                    _consoleForm?.WriteInfo("User already in pending_removal group");
                                }
                            }
                            else
                            {
                                _consoleForm?.WriteWarning("pending_removal group not found in AD - skipping");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Error adding to pending_removal group: {ex.Message}");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error removing user from groups: {ex.Message}");
                throw new Exception($"Error removing user from groups: {ex.Message}", ex);
            }
        }
        #endregion

        #region Computer Operations
        /// <summary>
        /// Load computers from ComputerList.csv file
        /// </summary>
        /// <param name="csvPath">Path to ComputerList.csv file</param>
        /// <returns>List of ComputerInfo objects from CSV</returns>
        public List<ComputerInfo> LoadComputersFromCSV(string csvPath = @"C:\SA_ToolBelt\Config\ComputerList.csv")
        {
            var computers = new List<ComputerInfo>();

            try
            {
                if (!File.Exists(csvPath))
                {
                    // Return empty list if file doesn't exist - don't throw exception
                    return computers;
                }

                var lines = File.ReadAllLines(csvPath);

                // Skip header row if it exists
                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(',');
                    if (columns.Length >= 4)
                    {
                        var computerInfo = new ComputerInfo
                        {
                            Name = columns[0].Trim(),
                            // Use Type as Description for CSV computers
                            // Description = columns[1].Trim(), // Type becomes description
                                                             // Store additional CSV data in custom properties or handle as needed
                        };

                        computers.Add(computerInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - return empty list
                Console.WriteLine($"Error loading CSV: {ex.Message}");
            }

            return computers;
        }
        /// <summary>
        /// Get computers from OU as DTOs with full information
        /// </summary>
        public List<ComputerInfo> GetComputersFromOUAsInfo(string ouPath)
        {
            var computers = new List<ComputerInfo>();

            try
            {
                string username = CredentialManager._username;
                string password = CredentialManager._password;

                using (var entry = new DirectoryEntry(ouPath, username, password))
                {
                    foreach (DirectoryEntry computer in entry.Children)
                    {
                        if (computer.SchemaClassName.Equals("computer", StringComparison.OrdinalIgnoreCase))
                        {
                            computers.Add(new ComputerInfo(computer));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving computers from OU {ouPath}: {ex.Message}", ex);
            }

            return computers;
        }

        /// <summary>
        /// Get computers from multiple OUs as DTOs
        /// </summary>
        public async Task<List<ComputerInfo>> GetComputersFromMultipleOUsAsync(IEnumerable<string> ouPaths )
        {
            var allComputers = new List<ComputerInfo>();

            var tasks = ouPaths.Select(async ouPath =>
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        return GetComputersFromOUAsInfo(ouPath);
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteError($"Failed to get computers from OU '{ouPath}': {ex.Message}");
                        return new List<ComputerInfo>();
                    }
                });
            });

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                allComputers.AddRange(result);
            }

            return allComputers;
        }
        /// <summary>
        /// Get single computer information as DTO
        /// </summary>
        public ComputerInfo GetComputerInfo(string computerName)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var computer = ComputerPrincipal.FindByIdentity(context, computerName))
                {
                    if (computer != null)
                    {
                        // Convert ComputerPrincipal to DirectoryEntry to get additional properties
                        var entry = computer.GetUnderlyingObject() as DirectoryEntry;
                        return new ComputerInfo(entry);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving computer info for {computerName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get computers from CheckedListBox items as DTOs (for configuration integration)
        /// </summary>
        public async Task<List<ComputerInfo>> GetComputersFromCheckedListBoxAsync(CheckedListBox checkedListBox)
        {
            var ouList = new List<string>();

            foreach (string item in checkedListBox.Items)
            {
                ouList.Add(item);
            }

            return await GetComputersFromMultipleOUsAsync(ouList);
        }

        /// <summary>
        /// Get all computers in domain as DTOs
        /// </summary>
        public async Task<List<ComputerInfo>> GetAllComputersAsync()
        {
            var computers = new List<ComputerInfo>();

            try
            {
                await Task.Run(() =>
                {
                    using (var context = new PrincipalContext(ContextType.Domain, _domain))
                    using (var searcher = new PrincipalSearcher(new ComputerPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is ComputerPrincipal computer)
                            {
                                try
                                {
                                    var entry = computer.GetUnderlyingObject() as DirectoryEntry;
                                    computers.Add(new ComputerInfo(entry));
                                }
                                catch (Exception ex)
                                {
                                    _consoleForm?.WriteError($"Error processing computer {computer.Name}: {ex.Message}");
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all computers: {ex.Message}", ex);
            }

            return computers;
        }

        /// <summary>
        /// Search for computers and return as ComputerInfo DTOs
        /// </summary>
        public List<ComputerInfo> SearchComputersAsInfo(string searchTerm)
        {
            var computerInfoList = new List<ComputerInfo>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    var computerFilter = new ComputerPrincipal(context)
                    {
                        Name = $"*{searchTerm}*"
                    };

                    using (var searcher = new PrincipalSearcher(computerFilter))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is ComputerPrincipal computer)
                            {
                                try
                                {
                                    var entry = computer.GetUnderlyingObject() as DirectoryEntry;
                                    computerInfoList.Add(new ComputerInfo(entry));
                                }
                                catch (Exception ex)
                                {
                                    _consoleForm?.WriteError($"Error processing computer {computer.Name}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching computers as info: {ex.Message}", ex);
            }

            return computerInfoList;
        }
        #endregion

        #region Authentication & Authorization

        /// <summary>
        /// Validate user credentials
        /// </summary>
        public bool ValidateCredentials(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    return context.ValidateCredentials(username, password);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating credentials for {username}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if current user is domain admin
        /// </summary>
        public bool IsCurrentUserDomainAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    // Check built-in administrator role first
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                        return true;

                    // Check UserClaims for group memberships (more reliable)
                    foreach (var claim in identity.UserClaims)
                    {
                        if (claim.Type == ClaimTypes.DenyOnlySid)
                        {
                            string sid = claim.Value;

                            // Check for Domain Admins, Enterprise Admins, etc.
                            if (sid.EndsWith("-512") ||  // Standard Domain Admins
                                sid.EndsWith("-519"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking domain admin status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if user is member of specific group
        /// </summary>
        public bool IsUserInGroup(string username, string groupName)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    if (user != null)
                    {
                        foreach (var group in user.GetGroups())
                        {
                            if (string.Equals(group.Name, groupName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get current domain information
        /// </summary>
        public string GetDomainInfo()
        {
            try
            {
                using (var domain = Domain.GetCurrentDomain())
                {
                    return $"Domain: {domain.Name}, Forest: {domain.Forest.Name}";
                }
            }
            catch (Exception ex)
            {
                return $"Error retrieving domain info: {ex.Message}";
            }
        }

        /// <summary>
        /// Test Active Directory connectivity
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    // Try to get current user as a connectivity test
                    var currentUser = UserPrincipal.FindByIdentity(context, Environment.UserName);
                    return currentUser != null;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Validate password against common requirements
        /// </summary>
        public static bool ValidatePasswordRequirements(string password, out List<string> errors)
        {
            errors = new List<string>();

            if (password.Length < 14)
                errors.Add("Password must be at least 14 characters");

            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain at least 1 uppercase letter");

            if (!password.Any(char.IsLower))
                errors.Add("Password must contain at least 1 lowercase letter");

            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain at least 1 number");

            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
                errors.Add("Password must contain at least 1 special character");

            return errors.Count == 0;
        }

        /// <summary>
        /// Test if a password meets requirements (returns validation result)
        /// </summary>
        public static string TestPassword(string password)
        {
            var isValid = ValidatePasswordRequirements(password, out var errors);

            if (isValid)
                return "Password meets all requirements";

            return "Password issues: " + string.Join(", ", errors);
        }

        #endregion

        #region Browse Active Directory for Organizational Units
        public List<string> BrowseOrganizationalUnits()
        {
            var organizationalUnits = new List<string>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                using (var directoryEntry = new DirectoryEntry($"LDAP://{_domain}"))
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.Filter = "(objectClass=organizationalUnit)";
                    directorySearcher.PropertiesToLoad.Add("distinguishedName");
                    directorySearcher.PropertiesToLoad.Add("name");
                    directorySearcher.PageSize = 1000;

                    var searchResults = directorySearcher.FindAll();

                    foreach (SearchResult result in searchResults)
                    {
                        if (result.Properties["distinguishedName"].Count > 0)
                        {
                            string distinguishedName = result.Properties["distinguishedName"][0].ToString();
                            organizationalUnits.Add(distinguishedName);
                        }
                    }
                }

                // Sort alphabetically for easier browsing
                organizationalUnits.Sort();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error browsing Organizational Units: {ex.Message}", ex);
            }

            return organizationalUnits;
        }
        #endregion
        
        #region User Info DTO structure
        public class UserInfo
        {
            public string SamAccountName { get; set; }
            public string GivenName { get; set; }
            public string Surname { get; set; }
            public string EmailAddress { get; set; }
            public DateTime? AccountExpirationDate { get; set; }
            public DateTime? LastLogon { get; set; }
            public DateTime? LastPasswordSet { get; set; }
            public bool? Enabled { get; set; }
            public string DistinguishedName { get; set; }
            public DateTime? LastBadPasswordAttempt { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string TelephoneNumber { get; set; }
            public string HomeDirectory { get; set; }
            public bool? IsLockedOut { get; set; }
            public DateTime? WhenChanged { get; set; }
            public string GidNumber { get; set; }
            public string UidNumber { get; set; }


            // Default constructor
            public UserInfo() { }

            // Constructor to populate from UserPrincipal while context is still active
            public UserInfo(UserPrincipal user)
            {
                SamAccountName = user.SamAccountName;
                GivenName = user.GivenName;
                Surname = user.Surname;
                EmailAddress = user.EmailAddress;
                AccountExpirationDate = user.AccountExpirationDate;
                LastLogon = user.LastLogon;
                LastPasswordSet = user.LastPasswordSet;
                Enabled = user.Enabled;
                DistinguishedName = user.DistinguishedName;
                LastBadPasswordAttempt = user.LastBadPasswordAttempt;
                DisplayName = user.DisplayName;
                Description = user.Description;
                TelephoneNumber = user.VoiceTelephoneNumber;
                HomeDirectory = user.HomeDirectory;
                IsLockedOut = user.IsAccountLockedOut();

                // Get WhenChanged from DirectoryEntry
                try
                {
                    using (var entry = user.GetUnderlyingObject() as DirectoryEntry)
                    {
                        if (entry != null)
                        {
                            entry.RefreshCache(new string[] { "WhenChanged", "gidNumber", "uidNumber" });

                            if (entry.Properties.Contains("whenChanged") &&
                                entry.Properties["whenChanged"].Count > 0)
                            {
                                var value = entry.Properties["whenChanged"].Value;
                                if (value is DateTime dateTime)
                                    WhenChanged = dateTime;
                                else if (value is long fileTime && fileTime > 0)
                                    WhenChanged = DateTime.FromFileTime(fileTime);
                            }

                            if (entry.Properties.Contains("gidNumber") &&
                                entry.Properties["gidNumber"].Count > 0)
                            {
                                GidNumber = entry.Properties["gidNumber"][0]?.ToString();
                            }

                            if (entry.Properties.Contains("uidNumber") &&
                                entry.Properties["uidNumber"].Count > 0)
                            {
                                UidNumber = entry.Properties["uidNumber"][0]?.ToString();
                            }
                        }
                            
                        /*
                        if (entry != null && entry.Properties.Contains("whenChanged") &&
                            entry.Properties["whenChanged"].Count > 0)
                        {
                            var value = entry.Properties["whenChanged"].Value;
                            if (value is DateTime dateTime)
                                WhenChanged = dateTime;
                            else if (value is long fileTime && fileTime > 0)
                                WhenChanged = DateTime.FromFileTime(fileTime);
                        }
                        */
                    }
                }
                catch (Exception)
                {
                    // If we can't get WhenChanged, just leave it null
                    WhenChanged = null;
                    GidNumber = null;
                    UidNumber = null;
                }
            }

            // Helper methods to mimic UserPrincipal behavior
            public bool IsAccountLockedOut()
            {
                return IsLockedOut ?? false;
            }

            // Helper to get full name
            public string GetFullName()
            {
                var fullName = $"{GivenName ?? ""} {Surname ?? ""}".Trim();
                return string.IsNullOrEmpty(fullName) ? "N/A" : fullName;
            }
        }
        
        /// <summary>
        /// Get user information as a DTO that can be used outside of AD context
        /// </summary>
        public UserInfo GetUserInfo(string username)
        {
            try
            {
                using (var user = GetUser(username))
                {
                    return user != null ? new UserInfo(user) : null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user info for {username}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Search for users and return as UserInfo DTOs
        /// </summary>
        public List<UserInfo> SearchUsersAsInfo(string searchTerm)
        {
            var userInfoList = new List<UserInfo>();

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    var userFilter = new UserPrincipal(context)
                    {
                        DisplayName = $"*{searchTerm}*"
                    };

                    using (var searcher = new PrincipalSearcher(userFilter))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user)
                            {
                                userInfoList.Add(new UserInfo(user));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching users as info: {ex.Message}", ex);
            }

            return userInfoList;
        }
        /// <summary>
        /// Get all users as UserInfo DTOs (async version for better performance)
        /// </summary>
        public async Task<List<UserInfo>> GetAllUsersAsInfoAsync(string organizationalUnit = null)
        {
            var users = new List<UserInfo>();

            try
            {
                await Task.Run(() =>
                {
                    using (var context = new PrincipalContext(ContextType.Domain, _domain, organizationalUnit))
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user)
                            {
                                // Skip excluded users
                                if (!ShouldExcludeUser(user))
                                {
                                    users.Add(new UserInfo(user));
                                }
                                user.Dispose(); // Clean up immediately
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all users as info: {ex.Message}", ex);
            }

            return users;
        }
        
       

        #endregion

        #region Computer Info DTO structure
        /// <summary>
        /// Data Transfer Object for computer information
        /// </summary>
        public class ComputerInfo
        {
            public string Name { get; set; }
            public string Office { get; set; }         // physicalDeliveryOfficeName (Office field)
            public string DisplayName { get; set; }    // displayName
            public string DistinguishedName { get; set; }
            public string OperatingSystem { get; set; }
            public string OperatingSystemVersion { get; set; }
            public DateTime? LastLogon { get; set; }
            public DateTime? WhenCreated { get; set; }
            public DateTime? WhenChanged { get; set; }
            public bool Enabled { get; set; }
            public string ManagedBy { get; set; }

            public ComputerInfo() { }

            public ComputerInfo(DirectoryEntry computer)
            {
                Name = GetProperty(computer, "name");
                Office = GetProperty(computer, "physicalDeliveryOfficeName");
                DistinguishedName = GetProperty(computer, "distinguishedName");
                OperatingSystem = GetProperty(computer, "operatingSystem");
                OperatingSystemVersion = GetProperty(computer, "operatingSystemVersion");
                LastLogon = GetDateProperty(computer, "lastLogonTimestamp");
                WhenCreated = GetDateProperty(computer, "whenCreated");
                WhenChanged = GetDateProperty(computer, "whenChanged");
                Enabled = !GetProperty(computer, "userAccountControl").Contains("2"); // Account disabled flag
                DisplayName = GetProperty(computer, "displayName");
                ManagedBy = GetProperty(computer, "managedBy");
            }

            private string GetProperty(DirectoryEntry entry, string propertyName)
            {
                return entry.Properties.Contains(propertyName) && entry.Properties[propertyName].Count > 0
                    ? entry.Properties[propertyName].Value?.ToString() ?? ""
                    : "";
            }

            private DateTime? GetDateProperty(DirectoryEntry entry, string propertyName)
            {
                if (entry.Properties.Contains(propertyName) && entry.Properties[propertyName].Count > 0)
                {
                    var value = entry.Properties[propertyName].Value;
                    if (value is DateTime dateTime)
                        return dateTime;
                    if (value is long fileTime && fileTime > 0)
                        return DateTime.FromFileTime(fileTime);
                }
                return null;
            }
        }
        #endregion

    }
}

