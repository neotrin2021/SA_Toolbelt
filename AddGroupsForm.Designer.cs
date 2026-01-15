namespace SA_ToolBelt
{
    partial class AddGroupsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            clbAllGroups = new CheckedListBox();
            btnGetAllGroups = new Button();
            lblGroupSearch = new Label();
            txbGroupSearch = new TextBox();
            btnGroupSearch = new Button();
            btnApply = new Button();
            btnCancel = new Button();
            lblSearchSelectNotice = new Label();
            txbUserSearchSelect = new TextBox();
            btnUserSearchGrpCopy = new Button();
            clbUserSearchResults = new CheckedListBox();
            btnGetGroups = new Button();
            clbExistingUsersGroups = new CheckedListBox();
            btnAddFromUser = new Button();
            btnAddFromAvailable = new Button();
            clbUsersGroups = new CheckedListBox();
            lblAvailableGroups = new Label();
            lblUsersGroups = new Label();
            btnRemoveCheckedGroups = new Button();
            btnUnCheckAllGroups = new Button();
            btnClose = new Button();
            lblSelectedUsersGroups = new Label();
            SuspendLayout();
            // 
            // clbAllGroups
            // 
            clbAllGroups.FormattingEnabled = true;
            clbAllGroups.Location = new Point(12, 69);
            clbAllGroups.Name = "clbAllGroups";
            clbAllGroups.Size = new Size(208, 346);
            clbAllGroups.TabIndex = 0;
            // 
            // btnGetAllGroups
            // 
            btnGetAllGroups.Location = new Point(12, 421);
            btnGetAllGroups.Name = "btnGetAllGroups";
            btnGetAllGroups.Size = new Size(208, 34);
            btnGetAllGroups.TabIndex = 1;
            btnGetAllGroups.Text = "Get all Groups";
            btnGetAllGroups.UseVisualStyleBackColor = true;
            btnGetAllGroups.Click += btnGetAllGroups_Click;
            // 
            // lblGroupSearch
            // 
            lblGroupSearch.AutoSize = true;
            lblGroupSearch.Location = new Point(12, 473);
            lblGroupSearch.Name = "lblGroupSearch";
            lblGroupSearch.Size = new Size(98, 15);
            lblGroupSearch.TabIndex = 2;
            lblGroupSearch.Text = "Search for group:";
            // 
            // txbGroupSearch
            // 
            txbGroupSearch.Location = new Point(12, 491);
            txbGroupSearch.Name = "txbGroupSearch";
            txbGroupSearch.Size = new Size(212, 23);
            txbGroupSearch.TabIndex = 3;
            // 
            // btnGroupSearch
            // 
            btnGroupSearch.Location = new Point(12, 520);
            btnGroupSearch.Name = "btnGroupSearch";
            btnGroupSearch.Size = new Size(212, 33);
            btnGroupSearch.TabIndex = 4;
            btnGroupSearch.Text = "Search For Group";
            btnGroupSearch.UseVisualStyleBackColor = true;
            btnGroupSearch.Click += btnGroupSearch_Click;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(376, 608);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(75, 31);
            btnApply.TabIndex = 5;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(567, 608);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 31);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblSearchSelectNotice
            // 
            lblSearchSelectNotice.AutoSize = true;
            lblSearchSelectNotice.Location = new Point(787, 18);
            lblSearchSelectNotice.Name = "lblSearchSelectNotice";
            lblSearchSelectNotice.Size = new Size(260, 15);
            lblSearchSelectNotice.TabIndex = 7;
            lblSearchSelectNotice.Text = "Please search for and select a user to copy from:";
            // 
            // txbUserSearchSelect
            // 
            txbUserSearchSelect.Location = new Point(787, 36);
            txbUserSearchSelect.Name = "txbUserSearchSelect";
            txbUserSearchSelect.Size = new Size(208, 23);
            txbUserSearchSelect.TabIndex = 8;
            // 
            // btnUserSearchGrpCopy
            // 
            btnUserSearchGrpCopy.Location = new Point(787, 65);
            btnUserSearchGrpCopy.Name = "btnUserSearchGrpCopy";
            btnUserSearchGrpCopy.Size = new Size(208, 33);
            btnUserSearchGrpCopy.TabIndex = 9;
            btnUserSearchGrpCopy.Text = "Search";
            btnUserSearchGrpCopy.UseVisualStyleBackColor = true;
            btnUserSearchGrpCopy.Click += btnUserSearchGrpCopy_Click;
            // 
            // clbUserSearchResults
            // 
            clbUserSearchResults.FormattingEnabled = true;
            clbUserSearchResults.Location = new Point(787, 104);
            clbUserSearchResults.Name = "clbUserSearchResults";
            clbUserSearchResults.Size = new Size(208, 166);
            clbUserSearchResults.TabIndex = 10;
            // 
            // btnGetGroups
            // 
            btnGetGroups.Location = new Point(787, 323);
            btnGetGroups.Name = "btnGetGroups";
            btnGetGroups.Size = new Size(208, 33);
            btnGetGroups.TabIndex = 11;
            btnGetGroups.Text = "Get Groups";
            btnGetGroups.UseVisualStyleBackColor = true;
            btnGetGroups.Click += btnGetGroups_Click;
            // 
            // clbExistingUsersGroups
            // 
            clbExistingUsersGroups.FormattingEnabled = true;
            clbExistingUsersGroups.Location = new Point(787, 371);
            clbExistingUsersGroups.Name = "clbExistingUsersGroups";
            clbExistingUsersGroups.Size = new Size(208, 256);
            clbExistingUsersGroups.TabIndex = 12;
            // 
            // btnAddFromUser
            // 
            btnAddFromUser.Font = new Font("Segoe UI", 11F);
            btnAddFromUser.Location = new Point(609, 371);
            btnAddFromUser.Name = "btnAddFromUser";
            btnAddFromUser.Size = new Size(168, 29);
            btnAddFromUser.TabIndex = 13;
            btnAddFromUser.Text = "← Add";
            btnAddFromUser.UseVisualStyleBackColor = true;
            btnAddFromUser.Click += btnAddFromUser_Click;
            // 
            // btnAddFromAvailable
            // 
            btnAddFromAvailable.Font = new Font("Segoe UI", 11F);
            btnAddFromAvailable.Location = new Point(226, 241);
            btnAddFromAvailable.Name = "btnAddFromAvailable";
            btnAddFromAvailable.Size = new Size(163, 29);
            btnAddFromAvailable.TabIndex = 14;
            btnAddFromAvailable.Text = "Add →";
            btnAddFromAvailable.UseVisualStyleBackColor = true;
            btnAddFromAvailable.Click += btnAddFromAvailable_Click;
            // 
            // clbUsersGroups
            // 
            clbUsersGroups.FormattingEnabled = true;
            clbUsersGroups.Location = new Point(395, 69);
            clbUsersGroups.Name = "clbUsersGroups";
            clbUsersGroups.Size = new Size(208, 346);
            clbUsersGroups.TabIndex = 15;
            // 
            // lblAvailableGroups
            // 
            lblAvailableGroups.AutoSize = true;
            lblAvailableGroups.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblAvailableGroups.Location = new Point(35, 27);
            lblAvailableGroups.Name = "lblAvailableGroups";
            lblAvailableGroups.Size = new Size(157, 25);
            lblAvailableGroups.TabIndex = 16;
            lblAvailableGroups.Text = "Available Groups";
            // 
            // lblUsersGroups
            // 
            lblUsersGroups.AutoSize = true;
            lblUsersGroups.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblUsersGroups.Location = new Point(436, 27);
            lblUsersGroups.Name = "lblUsersGroups";
            lblUsersGroups.Size = new Size(124, 25);
            lblUsersGroups.TabIndex = 17;
            lblUsersGroups.Text = "Users Groups";
            // 
            // btnRemoveCheckedGroups
            // 
            btnRemoveCheckedGroups.Location = new Point(395, 462);
            btnRemoveCheckedGroups.Name = "btnRemoveCheckedGroups";
            btnRemoveCheckedGroups.Size = new Size(208, 36);
            btnRemoveCheckedGroups.TabIndex = 18;
            btnRemoveCheckedGroups.Text = "Remove Checked Groups";
            btnRemoveCheckedGroups.UseVisualStyleBackColor = true;
            btnRemoveCheckedGroups.Click += btnRemoveCheckedGroups_Click;
            // 
            // btnUnCheckAllGroups
            // 
            btnUnCheckAllGroups.Location = new Point(395, 419);
            btnUnCheckAllGroups.Name = "btnUnCheckAllGroups";
            btnUnCheckAllGroups.Size = new Size(208, 36);
            btnUnCheckAllGroups.TabIndex = 19;
            btnUnCheckAllGroups.Text = "Uncheck All Groups";
            btnUnCheckAllGroups.UseVisualStyleBackColor = true;
            btnUnCheckAllGroups.Click += btnUnCheckAllGroups_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(473, 608);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 31);
            btnClose.TabIndex = 20;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblSelectedUsersGroups
            // 
            lblSelectedUsersGroups.AutoSize = true;
            lblSelectedUsersGroups.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSelectedUsersGroups.Location = new Point(823, 299);
            lblSelectedUsersGroups.Name = "lblSelectedUsersGroups";
            lblSelectedUsersGroups.Size = new Size(135, 21);
            lblSelectedUsersGroups.TabIndex = 21;
            lblSelectedUsersGroups.Text = "<User>'s Groups";
            // 
            // AddGroupsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1054, 651);
            Controls.Add(lblSelectedUsersGroups);
            Controls.Add(btnClose);
            Controls.Add(btnUnCheckAllGroups);
            Controls.Add(btnRemoveCheckedGroups);
            Controls.Add(lblUsersGroups);
            Controls.Add(lblAvailableGroups);
            Controls.Add(clbUsersGroups);
            Controls.Add(btnAddFromAvailable);
            Controls.Add(btnAddFromUser);
            Controls.Add(clbExistingUsersGroups);
            Controls.Add(btnGetGroups);
            Controls.Add(clbUserSearchResults);
            Controls.Add(btnUserSearchGrpCopy);
            Controls.Add(txbUserSearchSelect);
            Controls.Add(lblSearchSelectNotice);
            Controls.Add(btnCancel);
            Controls.Add(btnApply);
            Controls.Add(btnGroupSearch);
            Controls.Add(txbGroupSearch);
            Controls.Add(lblGroupSearch);
            Controls.Add(btnGetAllGroups);
            Controls.Add(clbAllGroups);
            Name = "AddGroupsForm";
            Text = "Add Groups";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox clbAllGroups;
        private Button btnGetAllGroups;
        private Label lblGroupSearch;
        private TextBox txbGroupSearch;
        private Button btnGroupSearch;
        private Button btnApply;
        private Button btnCancel;
        private Label lblSearchSelectNotice;
        private TextBox txbUserSearchSelect;
        private Button btnUserSearchGrpCopy;
        private CheckedListBox clbUserSearchResults;
        private Button btnGetGroups;
        private CheckedListBox clbExistingUsersGroups;
        private Button btnAddFromUser;
        private Button btnAddFromAvailable;
        private CheckedListBox clbUsersGroups;
        private Label lblAvailableGroups;
        private Label lblUsersGroups;
        private Button btnRemoveCheckedGroups;
        private Button btnUnCheckAllGroups;
        private Button btnClose;
        private Label lblSelectedUsersGroups;
    }
}