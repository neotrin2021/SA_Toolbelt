using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Net;
using System.IO;
using System.Linq;

namespace SA_ToolBelt
{
    public class RHDS_Service
    {
        #region Fields and Constructor

        private readonly ConsoleForm _consoleForm;
        private const string SERVER_NAME = "ccesa1.spectre.afspc.af.smil.mil";
        private const int LDAP_PORT = 389;
        private const string BASE_DN = "dc=spectre,dc=afspc,dc=af,dc=smil,dc=mil";

        public RHDS_Service(ConsoleForm consoleForm)
        {
            _consoleForm = consoleForm;
        }

        // Keep parameterless constructor for backward compatibility
        public RHDS_Service()
        {
            _consoleForm = null;
        }

        #endregion

        #region Authentication Shit

        private LdapConnection LDAP_Authentication()
        {
            string ldapUser;
            string ldapPassword;

            // ==========================================================
            // HARDCODED CREDENTIALS FOR TESTING - REMOVE WHEN DEPLOYING TO PRODUCTION
            // ==========================================================
            bool useHardcodedCreds = true; // Set to false when ready for production
            if (useHardcodedCreds)
            {
                ldapUser = "cn=Directory Manager";
                ldapPassword = "CCEP@ssw0rd1234";
            }
            else
            {
                // Use stored credentials from login
                if (!CredentialManager.IsAuthenticated)
                    throw new Exception("No credentials available. Please log in first.");

                ldapUser = CredentialManager.GetLdapUsername(); // Returns "cn=username"
                ldapPassword = CredentialManager.GetPassword();
            }
            // ==========================================================
            // END HARDCODED CREDENTIALS - DELETE ABOVE SECTION WHEN READY
            // ==========================================================

            try
            {
                NetworkCredential credentials = new NetworkCredential(ldapUser, ldapPassword);

                LdapConnection ldapAuth = new LdapConnection($"{SERVER_NAME}:{LDAP_PORT}");
                ldapAuth.SessionOptions.SecureSocketLayer = false;
                ldapAuth.SessionOptions.ProtocolVersion = 3;
                ldapAuth.AuthType = AuthType.Basic;
                ldapAuth.Bind(credentials);

                return ldapAuth;
            }
            catch (LdapException ex)
            {
                throw new Exception($"LDAP Authentication failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"General error during LDAP authentication: {ex.Message}");
            }
        }

        public LdapConnection GetLdapConnection()
        {
            try
            {
                _consoleForm?.WriteInfo("Establishing LDAP connection...");
                var connection = LDAP_Authentication();
                _consoleForm?.WriteSuccess("LDAP connection established successfully");
                return connection;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error establishing LDAP connection: {ex.Message}");
                throw;
            }
        }

        public LdapConnection GetSecureConnection()
        {
            try
            {
                return LDAP_Authentication();
            }
            catch (Exception ex) when (ex.Message.Contains("No credentials available"))
            {
                throw new Exception("LDAP connection failed: No user credentials available. Please log into SA_ToolBelt first.");
            }
            catch (LdapException ex)
            {
                throw new Exception($"LDAP Authentication failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"General error during LDAP authentication: {ex.Message}");
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (LdapConnection conn = LDAP_Authentication())
                {
                    conn.Bind();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region User Creation Shit

        /// <summary>
        /// Get the next available Linux UID from Directory Services
        /// Excludes specific system/reserved UIDs: 101, 276, 6000, 22941, 22942, 22943
        /// </summary>
        public string GetNextAvailableUid()
        {
            try
            {
                _consoleForm?.WriteInfo("Getting next available Linux UID from Directory Services...");

                using (var ldapConnection = GetSecureConnection())
                {
                    string distinguishedName = $"ou=people,{BASE_DN}";
                    string filter = "(uidNumber=*)";

                    var searchRequest = new SearchRequest(
                        distinguishedName,
                        filter,
                        System.DirectoryServices.Protocols.SearchScope.Subtree,
                        new string[] { "uidNumber" }
                    );

                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    var currentUIDNumbers = new List<int>();

                    foreach (SearchResultEntry entry in searchResponse.Entries)
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

                    // Filter out specific UIDs (system/reserved accounts)
                    int[] excludedUIDs = { 101, 276, 6000, 22941, 22942, 22943 };
                    var filteredUIDs = currentUIDNumbers.Where(uid => !excludedUIDs.Contains(uid)).ToList();

                    if (filteredUIDs.Count == 0)
                    {
                        _consoleForm?.WriteWarning("No existing UIDs found. Starting from default UID 10000");
                        return "10000";
                    }

                    // Get next available UID
                    int nextAvailableUID = filteredUIDs.Max() + 1;

                    _consoleForm?.WriteSuccess($"Next available UID: {nextAvailableUID}");
                    return nextAvailableUID.ToString();
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting next available UID: {ex.Message}");
                throw new Exception($"Error getting next available UID: {ex.Message}", ex);
            }
        }

        public void CreateNewUser(string ntUserId, string email, string firstName, string lastName,
                             string phone, string tempPassword, string linuxUid, string gidNumber, string securityGroup = null)
        {
            if (!CredentialManager.IsAuthenticated)
            {
                _consoleForm?.WriteError("Cannot create LDAP user: No authenticated credentials available. Please log into SA_ToolBelt first.");
                throw new InvalidOperationException("No authenticated credentials available.");
            }

            try
            {
                _consoleForm?.WriteInfo("Creating Linux Account...");

                var ldapConnection = GetSecureConnection();
                var futureDate = DateTime.Now.AddYears(1);
                var windowsEpoch = new DateTime(1601, 1, 1);
                var finalDate = (long)(futureDate - windowsEpoch).TotalSeconds * 10000000;

                string dnUser = $"uid={ntUserId},ou=People,{BASE_DN}";
                var directoryRequest = new AddRequest
                {
                    DistinguishedName = dnUser
                };

                // Add all attributes
                AddAttribute(directoryRequest, "ntUserDomainId", ntUserId);
                AddAttribute(directoryRequest, "userPassword", tempPassword);
                AddAttribute(directoryRequest, "homeDirectory", $"/home/{ntUserId}");
                AddAttribute(directoryRequest, "cn", $"{firstName} {lastName}");
                AddAttribute(directoryRequest, "ntUserCreateNewAccount", "true");
                AddAttribute(directoryRequest, "uid", ntUserId);
                AddAttribute(directoryRequest, "objectclass", new string[]
                {
                    "top", "person", "organizationalPerson", "inetOrgPerson",
                    "ntuser", "posixAccount", "account", "shadowaccount"
                });
                AddAttribute(directoryRequest, "mail", email);
                AddAttribute(directoryRequest, "gidNumber", gidNumber);
                AddAttribute(directoryRequest, "ntUserHomeDirDrive", "H");
                AddAttribute(directoryRequest, "loginShell", "/bin/bash");
                AddAttribute(directoryRequest, "telephoneNumber", phone);
                AddAttribute(directoryRequest, "sn", lastName);
                AddAttribute(directoryRequest, "givenName", firstName);
                AddAttribute(directoryRequest, "ntUserHomeDir", $"\\\\cce-data\\home\\{ntUserId}");
                AddAttribute(directoryRequest, "ntUserAcctExpires", finalDate.ToString());
                AddAttribute(directoryRequest, "uidNumber", linuxUid);

                ldapConnection.SendRequest(directoryRequest);

                _consoleForm?.WriteSuccess("Created user account with the following information:");
                _consoleForm?.WriteInfo($"  Username: {ntUserId}");
                _consoleForm?.WriteInfo($"  Full Name: {firstName} {lastName}");
                _consoleForm?.WriteInfo($"  Email: {email}");
                _consoleForm?.WriteInfo($"  Phone: {phone}");
                _consoleForm?.WriteInfo($"  Linux UID: {linuxUid}");
                _consoleForm?.WriteInfo($"  Home Directory: /home/{ntUserId}");
                _consoleForm?.WriteInfo($"  Windows Home Dir: \\\\cce-data\\home\\{ntUserId}");
                _consoleForm?.WriteInfo($"  Account Expires: {futureDate:yyyy-MM-dd}");
                _consoleForm?.WriteInfo("  Object Classes:");
                _consoleForm?.WriteInfo("    - top");
                _consoleForm?.WriteInfo("    - person");
                _consoleForm?.WriteInfo("    - organizationalPerson");
                _consoleForm?.WriteInfo("    - inetOrgPerson");
                _consoleForm?.WriteInfo("    - ntuser");
                _consoleForm?.WriteInfo("    - posixAccount");
                _consoleForm?.WriteInfo("    - account");
                _consoleForm?.WriteInfo("    - shadowaccount");

                if (!string.IsNullOrEmpty(securityGroup))
                {
                    _consoleForm?.WriteInfo($"Adding user to security group: {securityGroup}");
                    // Add user to Linux share_group
                    string dnGroup = $"cn={securityGroup},ou=Groups,{BASE_DN}";

                    // var modifyRequest = new ModifyRequest(dnGroup, DirectoryAttributeOperation.Add, "memberuid", ntUserId);
                    var modifyRequest = new ModifyRequest(dnGroup, DirectoryAttributeOperation.Add, "uniqueMember", dnUser);
                    ldapConnection.SendRequest(modifyRequest);
                    _consoleForm?.WriteSuccess($"Added user to {securityGroup}");
                }
                else
                {
                    _consoleForm?.WriteInfo("No security group specified. User created without group membership");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error creating user: {ex.Message}");
                throw;
            }
        }

        private void AddAttribute(AddRequest request, string attributeName, string attributeValue)
        {
            request.Attributes.Add(new DirectoryAttribute(attributeName, attributeValue));
        }

        private void AddAttribute(AddRequest request, string attributeName, string[] attributeValues)
        {
            request.Attributes.Add(new DirectoryAttribute(attributeName, attributeValues));
        }

        #endregion

        #region Group Management Shit

        /// <summary>
        /// Get all Directory Services groups a user belongs to
        /// </summary>
        public List<string> GetUserDSGroups(string username, string securityGroupsOU = null)
        {
            var userGroups = new List<string>();

            try
            {
                _consoleForm?.WriteInfo($"Getting Directory Services groups for user: {username}");

                using (var ldapConnection = GetSecureConnection())
                {
                    // Search for groups containing this user as memberuid
                    string searchBase = string.IsNullOrEmpty(securityGroupsOU)
                        ? $"ou=Groups,{BASE_DN}"
                        : securityGroupsOU;

                    var searchRequest = new SearchRequest(
                        searchBase,
                        $"(memberuid={username})",
                        System.DirectoryServices.Protocols.SearchScope.Subtree,
                        "cn"
                    );

                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    foreach (SearchResultEntry entry in searchResponse.Entries)
                    {
                        if (entry.Attributes.Contains("cn"))
                        {
                            string groupName = entry.Attributes["cn"][0].ToString();
                            userGroups.Add(groupName);
                        }
                    }

                    _consoleForm?.WriteSuccess($"Found {userGroups.Count} Directory Services groups for {username}");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting user groups: {ex.Message}");
                throw;
            }

            return userGroups;
        }

        /// <summary>
        /// Add user to a Directory Services group
        /// </summary>
        public bool AddUserToDSGroup(string username, string groupName, string securityGroupsOU = null)
        {
            try
            {
                _consoleForm?.WriteInfo($"Adding {username} to Directory Services group: {groupName}");

                using (var ldapConnection = GetSecureConnection())
                {
                    // Find the group DN
                    string groupDN = FindGroupDN(ldapConnection, groupName, securityGroupsOU);

                    if (string.IsNullOrEmpty(groupDN))
                    {
                        _consoleForm?.WriteError($"Group not found: {groupName}");
                        return false;
                    }

                    // Add user to group
                    var modifyRequest = new ModifyRequest(
                        groupDN,
                        DirectoryAttributeOperation.Add,
                        "memberuid",
                        username
                    );

                    ldapConnection.SendRequest(modifyRequest);
                    _consoleForm?.WriteSuccess($"Added {username} to DS group: {groupName}");
                    return true;
                }
            }
            catch (DirectoryOperationException ex) when (ex.Message.Contains("ATTRIBUTE_OR_VALUE_EXISTS"))
            {
                _consoleForm?.WriteWarning($"User {username} is already a member of {groupName}");
                return true; // Not really an error
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error adding user to group: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove user from a Directory Services group
        /// </summary>
        public bool RemoveUserFromDSGroup(string username, string groupName, string securityGroupsOU = null)
        {
            try
            {
                _consoleForm?.WriteInfo($"Removing {username} from Directory Services group: {groupName}");

                using (var ldapConnection = GetSecureConnection())
                {
                    // Find the group DN
                    string groupDN = FindGroupDN(ldapConnection, groupName, securityGroupsOU);

                    if (string.IsNullOrEmpty(groupDN))
                    {
                        _consoleForm?.WriteError($"Group not found: {groupName}");
                        return false;
                    }

                    // Remove user from group
                    var modifyRequest = new ModifyRequest(
                        groupDN,
                        DirectoryAttributeOperation.Delete,
                        "memberuid",
                        username
                    );

                    ldapConnection.SendRequest(modifyRequest);
                    _consoleForm?.WriteSuccess($"Removed {username} from DS group: {groupName}");
                    return true;
                }
            }
            catch (DirectoryOperationException ex) when (ex.Message.Contains("NO_SUCH_ATTRIBUTE"))
            {
                _consoleForm?.WriteWarning($"User {username} was not a member of {groupName}");
                return true; // Not really an error
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error removing user from group: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove user from ALL Directory Services groups in specified OU
        /// </summary>
        public int RemoveUserFromAllDSGroups(string username, string securityGroupsOU = null)
        {
            int groupsRemoved = 0;

            try
            {
                _consoleForm?.WriteInfo($"Removing {username} from ALL Directory Services groups...");

                using (var ldapConnection = GetSecureConnection())
                {
                    string searchBase = string.IsNullOrEmpty(securityGroupsOU)
                        ? $"ou=Groups,{BASE_DN}"
                        : securityGroupsOU;

                    // Search for all groups containing this user
                    var searchRequest = new SearchRequest(
                        searchBase,
                        $"(memberuid={username})",
                        System.DirectoryServices.Protocols.SearchScope.Subtree,
                        "cn"
                    );

                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    _consoleForm?.WriteInfo($"Found {searchResponse.Entries.Count} groups containing user {username}");

                    // Remove user from each group
                    foreach (SearchResultEntry entry in searchResponse.Entries)
                    {
                        try
                        {
                            string groupDN = entry.DistinguishedName;
                            string groupName = entry.Attributes["cn"][0].ToString();

                            _consoleForm?.WriteInfo($"Removing {username} from group: {groupName}");

                            var modifyRequest = new ModifyRequest(
                                groupDN,
                                DirectoryAttributeOperation.Delete,
                                "memberuid",
                                username
                            );

                            ldapConnection.SendRequest(modifyRequest);
                            groupsRemoved++;

                            _consoleForm?.WriteSuccess($"Removed {username} from DS group: {groupName}");
                        }
                        catch (Exception ex)
                        {
                            _consoleForm?.WriteError($"Error removing user from group {entry.DistinguishedName}: {ex.Message}");
                        }
                    }
                }

                if (groupsRemoved > 0)
                {
                    _consoleForm?.WriteSuccess($"Successfully removed {username} from {groupsRemoved} Directory Services groups");
                }
                else
                {
                    _consoleForm?.WriteInfo($"User {username} was not a member of any Directory Services groups");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error removing user from all groups: {ex.Message}");
                throw;
            }

            return groupsRemoved;
        }

        /// <summary>
        /// Get all Directory Services groups in a specific OU
        /// </summary>
        public List<string> GetAllDSGroupsInOU(string ouPath)
        {
            var groups = new List<string>();

            try
            {
                _consoleForm?.WriteInfo($"Getting all Directory Services groups from OU: {ouPath}");

                using (var ldapConnection = GetSecureConnection())
                {
                    var searchRequest = new SearchRequest(
                        ouPath,
                        "(objectClass=posixGroup)",
                        System.DirectoryServices.Protocols.SearchScope.Subtree,
                        "cn"
                    );

                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    foreach (SearchResultEntry entry in searchResponse.Entries)
                    {
                        if (entry.Attributes.Contains("cn"))
                        {
                            string groupName = entry.Attributes["cn"][0].ToString();
                            groups.Add(groupName);
                        }
                    }

                    _consoleForm?.WriteSuccess($"Found {groups.Count} Directory Services groups in OU");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting groups from OU: {ex.Message}");
                throw;
            }

            return groups;
        }

        /// <summary>
        /// Helper method to find a group's DN by name
        /// </summary>
        private string FindGroupDN(LdapConnection ldapConnection, string groupName, string securityGroupsOU = null)
        {
            try
            {
                string searchBase = string.IsNullOrEmpty(securityGroupsOU)
                    ? $"ou=Groups,{BASE_DN}"
                    : securityGroupsOU;

                var searchRequest = new SearchRequest(
                    searchBase,
                    $"(cn={groupName})",
                    System.DirectoryServices.Protocols.SearchScope.Subtree,
                    null
                );

                var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                if (searchResponse.Entries.Count > 0)
                {
                    return searchResponse.Entries[0].DistinguishedName;
                }

                return null;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error finding group DN: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Inactive Users Shit

        public List<string> GetInactiveUsers(int daysInactive = 60)
        {
            if (!CredentialManager.IsAuthenticated)
            {
                throw new InvalidOperationException("Cannot search for inactive users: No authenticated credentials available. Please log into SA_ToolBelt first.");
            }

            var results = new List<string>();

            try
            {
                _consoleForm?.WriteInfo($"Searching for users inactive for {daysInactive} days...");

                // Implementation would go here - searching LDAP for inactive users
                // This is a placeholder for the actual implementation

                _consoleForm?.WriteSuccess($"Found {results.Count} inactive users");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error while searching for inactive users: {ex.Message}");
                throw;
            }

            return results;
        }

        #endregion

        #region Directory Services Shit
        /// <summary>
        /// Browse Red Hat Directory Services for Organizational Units
        /// </summary>
        public List<string> BrowseOrganizationalUnits()
        {
            var organizationalUnits = new List<string>();

            try
            {
                _consoleForm?.WriteInfo("Browsing Directory Services for Organizational Units...");

                using (var ldapConnection = GetSecureConnection())
                {
                    var searchRequest = new SearchRequest(
                        BASE_DN,
                        "(objectClass=organizationalUnit)",
                        System.DirectoryServices.Protocols.SearchScope.Subtree,
                        "distinguishedName", "ou"
                    );

                    searchRequest.SizeLimit = 1000;

                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    foreach (SearchResultEntry entry in searchResponse.Entries)
                    {
                        string distinguishedName = entry.DistinguishedName;
                        organizationalUnits.Add(distinguishedName);
                    }
                }

                // Sort alphabetically for easier browsing
                organizationalUnits.Sort();

                _consoleForm?.WriteSuccess($"Found {organizationalUnits.Count} Organizational Units in Directory Services");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error browsing Organizational Units: {ex.Message}");
                throw new Exception($"Error browsing Organizational Units in RHDS: {ex.Message}", ex);
            }

            return organizationalUnits;
        }
        /// <summary>
        /// Get the Group ID (gidNumber) for a specific Security Group in Directory Services
        /// </summary>
        public string GetGroupGidNumber(string groupName, string securityGroupsOU = null)
        {
            try
            {
                _consoleForm?.WriteInfo($"Getting gidNumber for group: {groupName}");

                using (var ldapConnection = GetSecureConnection())
                {
                    var searchBase = string.IsNullOrEmpty(securityGroupsOU)
                        ? $"ou=Groups,{BASE_DN}"
                        : securityGroupsOU;

                    var searchRequest = new SearchRequest(
                        searchBase,
                        $"(cn={groupName})",
                        System.DirectoryServices.Protocols.SearchScope.Subtree,
                        "gidNumber"
                    );

                    var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    if (searchResponse.Entries.Count > 0)
                    {
                        var entry = searchResponse.Entries[0];

                        if (entry.Attributes.Contains("gidNumber"))
                        {
                            string gidNumber = entry.Attributes["gidNumber"][0].ToString();
                            _consoleForm?.WriteSuccess($"Found gidNumber: {gidNumber} for group: {groupName}");
                            return gidNumber;
                        }
                    }

                    _consoleForm?.WriteWarning($"No gidNumber found for group: {groupName}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error getting group gidNumber: {ex.Message}");
                throw new Exception($"Error getting gidNumber for Group {groupName}: {ex.Message}", ex);
            }
        }
        #endregion
    }
}
