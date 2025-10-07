using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SA_ToolBelt
{
    public partial class AddGroupsForm : Form
    {
        private readonly string _username;
        private readonly AD_Service _adService;
        private readonly ConsoleForm _consoleForm;
        private bool _hasChanges = false;
        private List<string> _originalUserGroups = new List<string>();

        public AddGroupsForm(string username, AD_Service adService, ConsoleForm consoleForm)
        {
            InitializeComponent();
            _username = username;
            _adService = adService;
            _consoleForm = consoleForm;

            this.Text = $"Add Groups to {username}";
        }
        private bool ApplyGroupChanges()
        {
            try
            {
                if (!_hasChanges)
                {
                    _consoleForm.WriteInfo("No changes to apply.");
                    return true; // Success, no changes needed
                }

                // Get current groups from the UI
                var currentGroups = new List<string>();
                foreach (string group in clbUsersGroups.Items)
                {
                    currentGroups.Add(group);
                }

                // Calculate what needs to be added and removed
                var groupsToAdd = currentGroups.Except(_originalUserGroups).ToList();
                var groupsToRemove = _originalUserGroups.Except(currentGroups).ToList();

                int successCount = 0;
                int totalOperations = groupsToAdd.Count + groupsToRemove.Count;

                _consoleForm.WriteInfo($"Applying changes: {groupsToAdd.Count} to add, {groupsToRemove.Count} to remove");

                // Process group changes with proper context management
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    using (var user = UserPrincipal.FindByIdentity(context, _username))
                    {
                        if (user == null)
                        {
                            _consoleForm.WriteError($"User not found: {_username}");
                            return false;
                        }

                        // Add new groups
                        foreach (string groupName in groupsToAdd)
                        {
                            try
                            {
                                using (var group = GroupPrincipal.FindByIdentity(context, groupName))
                                {
                                    if (group != null)
                                    {
                                        group.Members.Add(user);
                                        group.Save();
                                        successCount++;
                                        _consoleForm.WriteSuccess($"Added user to group: {groupName}");
                                    }
                                    else
                                    {
                                        _consoleForm.WriteError($"Group not found: {groupName}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _consoleForm.WriteError($"Error adding user to group {groupName}: {ex.Message}");
                            }
                        }

                        // Remove from groups
                        foreach (string groupName in groupsToRemove)
                        {
                            try
                            {
                                using (var group = GroupPrincipal.FindByIdentity(context, groupName))
                                {
                                    if (group != null)
                                    {
                                        group.Members.Remove(user);
                                        group.Save();
                                        successCount++;
                                        _consoleForm.WriteSuccess($"Removed user from group: {groupName}");
                                    }
                                    else
                                    {
                                        _consoleForm.WriteError($"Group not found: {groupName}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _consoleForm.WriteError($"Error removing user from group {groupName}: {ex.Message}");
                            }
                        }
                    }
                }

                // Update tracking after operations
                if (successCount == totalOperations)
                {
                    // All operations succeeded - update original groups to current state
                    _originalUserGroups = new List<string>(currentGroups);
                    _hasChanges = false;
                    _consoleForm.WriteSuccess($"Successfully applied all {successCount} group changes.");
                    return true;
                }
                else
                {
                    _consoleForm.WriteWarning($"Applied {successCount} of {totalOperations} changes. Some operations failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error applying group changes: {ex.Message}");
                return false;
            }
        }

        public void PopulateCurrentUserGroups(List<string> userGroups)
        {
            clbUsersGroups.Items.Clear();
            foreach (string group in userGroups)
            {
                clbUsersGroups.Items.Add(group, false); // unchecked by default
            }

            // Store original groups for change tracking
            _originalUserGroups = new List<string>(userGroups);
            _hasChanges = false;
            btnApply.Enabled = false; // Start disabled
        }
        private void CheckForChanges()
        {
            // Get current groups in clbUsersGroups
            var currentGroups = new List<string>();
            foreach (string group in clbUsersGroups.Items)
            {
                currentGroups.Add(group);
            }

            // Compare with original groups
            bool hasChanges = false;

            // Check if counts are different
            if (currentGroups.Count != _originalUserGroups.Count)
            {
                hasChanges = true;
            }
            else
            {
                // Check if all groups are the same
                foreach (string group in currentGroups)
                {
                    if (!_originalUserGroups.Contains(group))
                    {
                        hasChanges = true;
                        break;
                    }
                }

                if (!hasChanges)
                {
                    foreach (string group in _originalUserGroups)
                    {
                        if (!currentGroups.Contains(group))
                        {
                            hasChanges = true;
                            break;
                        }
                    }
                }
            }

            _hasChanges = hasChanges;
            btnApply.Enabled = hasChanges;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                btnApply.Enabled = false;
                btnApply.Text = "Applying...";

                bool success = ApplyGroupChanges();

                if (success)
                {
                    btnApply.Enabled = false; // Keep disabled since no changes pending
                    _consoleForm.WriteSuccess("All group changes applied successfully. Apply button disabled until new changes are made.");
                }
                else
                {
                    btnApply.Enabled = true; // Re-enable if there were errors
                    _consoleForm.WriteWarning("Some group changes failed. Please review errors above and try again.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error in Apply button: {ex.Message}");
                btnApply.Enabled = true;
            }
            finally
            {
                btnApply.Text = "Apply";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                _consoleForm.WriteInfo("Group editing cancelled by user.");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error closing form: {ex.Message}");
                // Force close even if there's an error
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btnUserSearchGrpCopy_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = txbUserSearchSelect.Text.Trim();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _consoleForm.WriteWarning("Please enter a username to search for.");
                    return;
                }

                btnUserSearchGrpCopy.Enabled = false;
                btnUserSearchGrpCopy.Text = "Searching...";

                _consoleForm.WriteInfo($"Searching for users containing: {searchTerm}");

                // Clear existing items
                clbUserSearchResults.Items.Clear();

                // Search for users - using multiple fields to catch variations
                var matchingUsers = _adService.SearchUsersByMultipleFields(searchTerm, searchTerm, searchTerm);

                if (matchingUsers.Count == 0)
                {
                    _consoleForm.WriteWarning($"No users found matching '{searchTerm}'.");
                    return;
                }

                // Add matching users to the checked list box with display name and username
                foreach (var user in matchingUsers)
                {
                    string displayText = $"{user.DisplayName} ({user.SamAccountName})";
                    clbUserSearchResults.Items.Add(displayText, false);
                }

                _consoleForm.WriteSuccess($"Found {matchingUsers.Count} users matching '{searchTerm}'.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error searching for users: {ex.Message}");
            }
            finally
            {
                btnUserSearchGrpCopy.Enabled = true;
                btnUserSearchGrpCopy.Text = "Search";
            }
        }
        private async void btnGetAllGroups_Click(object sender, EventArgs e)
        {
            try
            {
                btnGetAllGroups.Enabled = false;
                btnGetAllGroups.Text = "Loading...";

                _consoleForm.WriteInfo("Getting all Active Directory groups...");

                // Clear existing items
                clbAllGroups.Items.Clear();

                // Get all groups as DTOs
                var allGroups = await _adService.GetAllGroupsAsync();

                // Populate UI with group names
                foreach (var group in allGroups)
                {
                    clbAllGroups.Items.Add(group.Name, false);
                }

                _consoleForm.WriteSuccess($"Loaded {allGroups.Count} groups from Active Directory.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error getting all groups: {ex.Message}");
            }
            finally
            {
                btnGetAllGroups.Enabled = true;
                btnGetAllGroups.Text = "Get all Groups";
            }
        }

        private void btnGetGroups_Click(object sender, EventArgs e)
        {
            try
            {
                if (clbUserSearchResults.CheckedItems.Count == 0)
                {
                    _consoleForm.WriteWarning("Please select a user to get groups for.");
                    return;
                }

                if (clbUserSearchResults.CheckedItems.Count > 1)
                {
                    _consoleForm.WriteWarning("Please select only one user to copy groups from.");
                    return;
                }

                btnGetGroups.Enabled = false;
                btnGetGroups.Text = "Loading...";

                // Get the selected user (extract username from display text)
                string selectedUserDisplay = clbUserSearchResults.CheckedItems[0].ToString();

                // Extract username and display name from "Display Name (username)" format
                int startIndex = selectedUserDisplay.LastIndexOf('(') + 1;
                int endIndex = selectedUserDisplay.LastIndexOf(')');
                string selectedUsername = selectedUserDisplay.Substring(startIndex, endIndex - startIndex);
                string displayName = selectedUserDisplay.Substring(0, selectedUserDisplay.LastIndexOf('(')).Trim();

                _consoleForm.WriteInfo($"Getting group memberships for user: {selectedUsername}");

                // Clear existing items
                clbExistingUsersGroups.Items.Clear();

                try
                {
                    // Get the user's groups using our DTO method
                    var userGroups = _adService.GetUserGroups(selectedUsername);

                    if (userGroups.Count == 0)
                    {
                        _consoleForm.WriteWarning($"No group memberships found for user: {selectedUsername}");
                        lblSelectedUsersGroups.Text = $"{displayName}'s Groups (None)";
                    }
                    else
                    {
                        // Add groups to the checked list box (unchecked by default)
                        foreach (var group in userGroups)
                        {
                            clbExistingUsersGroups.Items.Add(group.Name, false);
                        }

                        _consoleForm.WriteSuccess($"Loaded {userGroups.Count} group memberships for user: {displayName}");
                    }

                    // Update label with selected user's name
                    lblSelectedUsersGroups.Text = $"{displayName}'s Groups";
                }
                catch (Exception ex)
                {
                    _consoleForm.WriteError($"Error loading groups for {displayName}: {ex.Message}");
                    lblSelectedUsersGroups.Text = $"{displayName}'s Groups (Error)";
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error getting user groups: {ex.Message}");
                lblSelectedUsersGroups.Text = "<User>'s Groups";
            }
            finally
            {
                btnGetGroups.Enabled = true;
                btnGetGroups.Text = "Get Groups";
            }
        }
        private void btnAddFromUser_Click(object sender, EventArgs e)
        {
            try
            {
                if (clbExistingUsersGroups.CheckedItems.Count == 0)
                {
                    _consoleForm.WriteWarning("Please select at least one group to copy.");
                    return;
                }

                var selectedGroups = clbExistingUsersGroups.CheckedItems.Cast<string>();
                int addedCount = 0;
                int duplicateCount = 0;

                _consoleForm.WriteInfo($"Copying {clbExistingUsersGroups.CheckedItems.Count} selected groups from user to current user's group list...");

                foreach (string groupName in selectedGroups)
                {
                    // Check if group already exists in current user's groups
                    bool alreadyExists = false;
                    foreach (string existingGroup in clbUsersGroups.Items)
                    {
                        if (string.Equals(existingGroup, groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            alreadyExists = true;
                            duplicateCount++;
                            break;
                        }
                    }

                    // Add group if it doesn't already exist
                    if (!alreadyExists)
                    {
                        clbUsersGroups.Items.Add(groupName, false); // unchecked by default
                        addedCount++;
                        _consoleForm.WriteInfo($"Copied group to list: {groupName}");
                    }
                }

                // Summary message
                if (addedCount > 0)
                {
                    _consoleForm.WriteSuccess($"Copied {addedCount} groups to user's group list.");
                }

                if (duplicateCount > 0)
                {
                    _consoleForm.WriteWarning($"Skipped {duplicateCount} groups that user already has.");
                }

                // Uncheck all items in the source list after adding
                for (int i = 0; i < clbExistingUsersGroups.Items.Count; i++)
                {
                    clbExistingUsersGroups.SetItemChecked(i, false);
                }

                // Trigger change tracking for Apply button
                CheckForChanges();

                _consoleForm.WriteInfo("Group copy completed.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error copying groups from user: {ex.Message}");
            }
        }
        private async void RefreshAllGroupsList()
        {
            try
            {
                // Clear the main groups list
                clbAllGroups.Items.Clear();

                _consoleForm.WriteInfo("Loading all groups...");

                // FIXED: Use DTO method instead of direct AD objects
                var allGroups = await _adService.GetAllGroupsAsync();

                // Add them back to the list
                foreach (var group in allGroups)
                {
                    clbAllGroups.Items.Add(group.Name, false);
                }

                _consoleForm.WriteSuccess($"Loaded {allGroups.Count} groups.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error refreshing groups list: {ex.Message}");
            }
        }
        private void btnAddFromAvailable_Click(object sender, EventArgs e)
        {
            try
            {
                if (clbAllGroups.CheckedItems.Count == 0)
                {
                    _consoleForm.WriteWarning("Please select at least one group to add.");
                    return;
                }

                var selectedGroups = clbAllGroups.CheckedItems.Cast<string>();
                int addedCount = 0;
                int duplicateCount = 0;

                _consoleForm.WriteInfo($"Adding {clbAllGroups.CheckedItems.Count} selected groups to user's group list...");

                foreach (string groupName in selectedGroups)
                {
                    // Check if group already exists in user's groups
                    bool alreadyExists = false;
                    foreach (string existingGroup in clbUsersGroups.Items)
                    {
                        if (string.Equals(existingGroup, groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            alreadyExists = true;
                            duplicateCount++;
                            break;
                        }
                    }

                    // Add group if it doesn't already exist
                    if (!alreadyExists)
                    {
                        clbUsersGroups.Items.Add(groupName, false); // unchecked by default
                        addedCount++;
                        _consoleForm.WriteInfo($"Added group to list: {groupName}");
                    }
                }

                // Summary message
                if (addedCount > 0)
                {
                    _consoleForm.WriteSuccess($"Added {addedCount} groups to user's group list.");
                }

                if (duplicateCount > 0)
                {
                    _consoleForm.WriteWarning($"Skipped {duplicateCount} groups that user already has.");
                }

                // Uncheck all items in the source list after adding
                for (int i = 0; i < clbAllGroups.Items.Count; i++)
                {
                    clbAllGroups.SetItemChecked(i, false);
                }

                // Trigger change tracking for Apply button
                CheckForChanges();
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error adding groups: {ex.Message}");
            }
        }
        private void btnUnCheckAllGroups_Click(object sender, EventArgs e)
        {
            try
            {
                if (clbUsersGroups.Items.Count == 0)
                {
                    _consoleForm.WriteWarning("No groups to uncheck.");
                    return;
                }

                // Uncheck all items in the user's groups list
                for (int i = 0; i < clbUsersGroups.Items.Count; i++)
                {
                    clbUsersGroups.SetItemChecked(i, false);
                }

                _consoleForm.WriteInfo($"Unchecked all {clbUsersGroups.Items.Count} groups in user's group list.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error unchecking groups: {ex.Message}");
            }
        }

        private void btnRemoveCheckedGroups_Click(object sender, EventArgs e)
        {
            try
            {
                if (clbUsersGroups.CheckedItems.Count == 0)
                {
                    _consoleForm.WriteWarning("Please select at least one group to remove.");
                    return;
                }

                var checkedGroups = clbUsersGroups.CheckedItems.Cast<string>().ToList();
                int removedCount = 0;

                _consoleForm.WriteInfo($"Removing {checkedGroups.Count} selected groups from user's group list...");

                // Remove each checked group from the list
                foreach (string groupName in checkedGroups)
                {
                    clbUsersGroups.Items.Remove(groupName);
                    removedCount++;
                    _consoleForm.WriteInfo($"Removed group from list: {groupName}");
                }

                _consoleForm.WriteSuccess($"Removed {removedCount} groups from user's group list.");

                // Trigger change tracking for Apply button
                CheckForChanges();
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error removing groups: {ex.Message}");
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (_hasChanges)
                {
                    var result = MessageBox.Show(
                        "There are unsaved changes. Do you want to apply the changes before closing?",
                        "Unsaved Changes",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Apply changes first
                        bool applySuccess = ApplyGroupChanges();

                        if (applySuccess)
                        {
                            _consoleForm.WriteSuccess("Changes applied successfully. Closing form.");
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            _consoleForm.WriteError("Cannot close: Failed to apply some changes. Please fix errors and try again.");
                            return; // Don't close if apply failed
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        // Close without applying
                        _consoleForm.WriteInfo("Closing without applying changes.");
                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                    }
                    // If Cancel, do nothing (stay open)
                }
                else
                {
                    // No changes, close normally
                    _consoleForm.WriteInfo("No changes to apply. Closing form.");
                    this.DialogResult = DialogResult.Cancel; // No changes made
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error closing form: {ex.Message}");
                // Force close even if there's an error
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
        private void btnGroupSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = txbGroupSearch.Text.Trim();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _consoleForm.WriteWarning("Please enter a search term.");
                    return;
                }

                btnGroupSearch.Enabled = false;
                btnGroupSearch.Text = "Searching...";

                _consoleForm.WriteInfo($"Searching for groups containing: {searchTerm}");

                // Clear existing items
                clbAllGroups.Items.Clear();

                // Search for matching groups
                var matchingGroups = _adService.SearchGroups(searchTerm);

                // Add matching groups to the checked list box
                foreach (var group in matchingGroups)
                {
                    clbAllGroups.Items.Add(group.Name, false);
                }

                _consoleForm.WriteSuccess($"Found {matchingGroups.Count} groups matching '{searchTerm}'.");
            }
            catch (Exception ex)
            {
                _consoleForm.WriteError($"Error searching groups: {ex.Message}");
            }
            finally
            {
                btnGroupSearch.Enabled = true;
                btnGroupSearch.Text = "Search For Group";
            }
        }
    }
}
