using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SA_ToolBelt.WMI_Service;

namespace SA_ToolBelt
{
    public partial class SAToolBelt
    {
        // ── Service ──────────────────────────────────────────────
        private WMI_Service _wmiService;

        // ── Theme colours ────────────────────────────────────────
        private static readonly Color HeaderDark = Color.FromArgb(30, 58, 95);       // #1E3A5F - deep navy
        private static readonly Color AccentBlue = Color.FromArgb(74, 127, 181);     // #4A7FB5 - steel blue
        private static readonly Color AccentHover = Color.FromArgb(90, 145, 200);    // lighter hover
        private static readonly Color SurfaceLight = Color.FromArgb(245, 247, 250);  // #F5F7FA - soft grey
        private static readonly Color BorderColor = Color.FromArgb(200, 210, 225);   // subtle border
        private static readonly Color TextDark = Color.FromArgb(33, 37, 41);         // near-black
        private static readonly Color TextMuted = Color.FromArgb(108, 117, 125);     // muted grey
        private static readonly Color GoodGreen = Color.FromArgb(40, 167, 69);       // success green
        private static readonly Color WarnRed = Color.FromArgb(220, 53, 69);         // alert red
        private static readonly Color WarnAmber = Color.FromArgb(255, 193, 7);       // caution amber

        // ── Controls ─────────────────────────────────────────────
        // Header
        private Panel pnlWinToolsHeader;
        private Label lblWinToolsTitle;
        private Label lblWinToolsSubtitle;

        // Query section
        private GroupBox gbxBiosQuery;
        private Label lblBiosComputerName;
        private TextBox txbBiosComputerName;
        private Button btnQueryBios;
        private Button btnTestWmiConnection;
        private Button btnClearBiosResults;
        private Button btnExportBiosResults;
        private Label lblBiosQueryStatus;
        private Label lblBiosQueryStatusValue;
        private ProgressBar pgbBiosQuery;

        // System info
        private Panel pnlSystemInfo;
        private Label lblSystemInfoHeader;
        private Label lblManufacturerTag, lblManufacturerValue;
        private Label lblModelTag, lblModelValue;
        private Label lblSerialTag, lblSerialValue;
        private Label lblBiosVersionTag, lblBiosVersionValue;
        private Label lblBiosDateTag, lblBiosDateValue;
        private Label lblOsNameTag, lblOsNameValue;
        private Label lblOsVersionTag, lblOsVersionValue;
        private Label lblOsArchTag, lblOsArchValue;

        // Security
        private Panel pnlSecurityStatus;
        private Label lblSecurityHeader;
        private Label lblTpmPresentTag, lblTpmPresentValue;
        private Label lblTpmVersionTag, lblTpmVersionValue;
        private Label lblTpmEnabledTag, lblTpmEnabledValue;
        private Label lblTpmActivatedTag, lblTpmActivatedValue;
        private Label lblSecureBootTag, lblSecureBootValue;

        // HP BIOS Settings grid
        private Panel pnlHpBiosSettings;
        private Label lblHpBiosHeader;
        private Label lblBiosFilterTag;
        private TextBox txbBiosSettingsFilter;
        private DataGridView dgvHpBiosSettings;
        private DataGridViewTextBoxColumn colBiosCategory;
        private DataGridViewTextBoxColumn colBiosSettingName;
        private DataGridViewTextBoxColumn colBiosSettingValue;
        private Label lblBiosSettingsCount;

        // Cached query result for filtering
        private BiosQueryResult _lastBiosResult;

        // ══════════════════════════════════════════════════════════
        //  Tab Setup
        // ══════════════════════════════════════════════════════════

        private void SetupWindowsToolsTab()
        {
            tabWindowsTools.BackColor = SurfaceLight;
            tabWindowsTools.Padding = new Padding(0);

            BuildHeaderPanel();
            BuildQuerySection();
            BuildSystemInfoPanel();
            BuildSecurityPanel();
            BuildHpBiosSettingsPanel();
        }

        // ── Header Panel ─────────────────────────────────────────

        private void BuildHeaderPanel()
        {
            pnlWinToolsHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1651, 62),
                BackColor = HeaderDark,
                Dock = DockStyle.Top
            };

            lblWinToolsTitle = new Label
            {
                Text = "WINDOWS REMOTE BIOS INSPECTOR",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(18, 8),
                BackColor = Color.Transparent
            };

            lblWinToolsSubtitle = new Label
            {
                Text = "Query HP BIOS configuration, TPM status & system information from remote computers",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(170, 190, 220),
                AutoSize = true,
                Location = new Point(20, 38),
                BackColor = Color.Transparent
            };

            pnlWinToolsHeader.Controls.Add(lblWinToolsTitle);
            pnlWinToolsHeader.Controls.Add(lblWinToolsSubtitle);
            tabWindowsTools.Controls.Add(pnlWinToolsHeader);
        }

        // ── Query Section ────────────────────────────────────────

        private void BuildQuerySection()
        {
            gbxBiosQuery = CreateStyledGroupBox("Query Remote Computer", new Point(10, 72), new Size(365, 210));

            lblBiosComputerName = new Label
            {
                Text = "Computer Name or IP:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(14, 28)
            };

            txbBiosComputerName = new TextBox
            {
                Location = new Point(14, 50),
                Size = new Size(335, 26),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txbBiosComputerName.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { btnQueryBios.PerformClick(); e.SuppressKeyPress = true; }
            };

            btnQueryBios = CreateAccentButton("Query BIOS", new Point(14, 86), new Size(162, 36));
            btnQueryBios.Click += btnQueryBios_Click;

            btnTestWmiConnection = CreateAccentButton("Test Connection", new Point(184, 86), new Size(165, 36));
            btnTestWmiConnection.BackColor = Color.FromArgb(52, 58, 64); // dark grey
            btnTestWmiConnection.FlatAppearance.MouseOverBackColor = Color.FromArgb(73, 80, 87);
            btnTestWmiConnection.Click += btnTestWmiConnection_Click;

            btnClearBiosResults = CreateAccentButton("Clear", new Point(14, 130), new Size(100, 32));
            btnClearBiosResults.BackColor = Color.FromArgb(108, 117, 125);
            btnClearBiosResults.FlatAppearance.MouseOverBackColor = Color.FromArgb(130, 140, 150);
            btnClearBiosResults.Click += btnClearBiosResults_Click;

            btnExportBiosResults = CreateAccentButton("Export", new Point(122, 130), new Size(100, 32));
            btnExportBiosResults.BackColor = Color.FromArgb(40, 167, 69);
            btnExportBiosResults.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 185, 85);
            btnExportBiosResults.Click += btnExportBiosResults_Click;

            lblBiosQueryStatus = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(14, 172)
            };

            lblBiosQueryStatusValue = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AccentBlue,
                AutoSize = true,
                Location = new Point(60, 172)
            };

            pgbBiosQuery = new ProgressBar
            {
                Location = new Point(14, 192),
                Size = new Size(335, 6),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };

            gbxBiosQuery.Controls.AddRange(new Control[]
            {
                lblBiosComputerName, txbBiosComputerName,
                btnQueryBios, btnTestWmiConnection,
                btnClearBiosResults, btnExportBiosResults,
                lblBiosQueryStatus, lblBiosQueryStatusValue, pgbBiosQuery
            });

            tabWindowsTools.Controls.Add(gbxBiosQuery);
        }

        // ── System Info Panel ────────────────────────────────────

        private void BuildSystemInfoPanel()
        {
            pnlSystemInfo = CreateStyledPanel(new Point(10, 290), new Size(365, 264));

            lblSystemInfoHeader = CreateSectionHeader("SYSTEM INFORMATION", new Point(0, 0), pnlSystemInfo.Width);

            int y = 40;
            int rowH = 26;

            (lblManufacturerTag, lblManufacturerValue) = CreateInfoRow("Manufacturer", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblModelTag, lblModelValue) = CreateInfoRow("Model", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblSerialTag, lblSerialValue) = CreateInfoRow("Serial Number", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblBiosVersionTag, lblBiosVersionValue) = CreateInfoRow("BIOS Version", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblBiosDateTag, lblBiosDateValue) = CreateInfoRow("BIOS Date", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblOsNameTag, lblOsNameValue) = CreateInfoRow("Operating System", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblOsVersionTag, lblOsVersionValue) = CreateInfoRow("OS Version", "—", 14, y, pnlSystemInfo); y += rowH;
            (lblOsArchTag, lblOsArchValue) = CreateInfoRow("Architecture", "—", 14, y, pnlSystemInfo);

            pnlSystemInfo.Controls.Add(lblSystemInfoHeader);
            tabWindowsTools.Controls.Add(pnlSystemInfo);
        }

        // ── Security Status Panel ────────────────────────────────

        private void BuildSecurityPanel()
        {
            pnlSecurityStatus = CreateStyledPanel(new Point(10, 562), new Size(365, 194));

            lblSecurityHeader = CreateSectionHeader("SECURITY STATUS", new Point(0, 0), pnlSecurityStatus.Width);

            int y = 40;
            int rowH = 28;

            (lblTpmPresentTag, lblTpmPresentValue) = CreateInfoRow("TPM Present", "—", 14, y, pnlSecurityStatus); y += rowH;
            (lblTpmVersionTag, lblTpmVersionValue) = CreateInfoRow("TPM Version", "—", 14, y, pnlSecurityStatus); y += rowH;
            (lblTpmEnabledTag, lblTpmEnabledValue) = CreateInfoRow("TPM Enabled", "—", 14, y, pnlSecurityStatus); y += rowH;
            (lblTpmActivatedTag, lblTpmActivatedValue) = CreateInfoRow("TPM Activated", "—", 14, y, pnlSecurityStatus); y += rowH;
            (lblSecureBootTag, lblSecureBootValue) = CreateInfoRow("Secure Boot", "—", 14, y, pnlSecurityStatus);

            pnlSecurityStatus.Controls.Add(lblSecurityHeader);
            tabWindowsTools.Controls.Add(pnlSecurityStatus);
        }

        // ── HP BIOS Settings Panel ───────────────────────────────

        private void BuildHpBiosSettingsPanel()
        {
            pnlHpBiosSettings = CreateStyledPanel(new Point(383, 72), new Size(1258, 684));

            lblHpBiosHeader = CreateSectionHeader("HP BIOS SETTINGS", new Point(0, 0), pnlHpBiosSettings.Width);

            lblBiosFilterTag = new Label
            {
                Text = "Filter:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(14, 42)
            };

            txbBiosSettingsFilter = new TextBox
            {
                Location = new Point(60, 39),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Type to filter settings..."
            };
            txbBiosSettingsFilter.TextChanged += txbBiosSettingsFilter_TextChanged;

            lblBiosSettingsCount = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(350, 42)
            };

            // DataGridView
            dgvHpBiosSettings = new DataGridView
            {
                Location = new Point(8, 68),
                Size = new Size(1242, 608),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(230, 235, 240),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                RowTemplate = { Height = 30 },
                Font = new Font("Segoe UI", 9.5F),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = TextDark,
                    SelectionBackColor = Color.FromArgb(220, 235, 252),
                    SelectionForeColor = TextDark,
                    Padding = new Padding(6, 2, 6, 2)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 250, 252),
                    ForeColor = TextDark,
                    SelectionBackColor = Color.FromArgb(220, 235, 252),
                    SelectionForeColor = TextDark
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(44, 62, 80),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 4, 6, 4)
                },
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };

            colBiosCategory = new DataGridViewTextBoxColumn
            {
                HeaderText = "Category",
                Name = "colBiosCategory",
                Width = 200,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            colBiosSettingName = new DataGridViewTextBoxColumn
            {
                HeaderText = "Setting Name",
                Name = "colBiosSettingName",
                Width = 450,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            colBiosSettingValue = new DataGridViewTextBoxColumn
            {
                HeaderText = "Current Value",
                Name = "colBiosSettingValue",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            dgvHpBiosSettings.Columns.AddRange(colBiosCategory, colBiosSettingName, colBiosSettingValue);

            pnlHpBiosSettings.Controls.AddRange(new Control[]
            {
                lblHpBiosHeader, lblBiosFilterTag, txbBiosSettingsFilter,
                lblBiosSettingsCount, dgvHpBiosSettings
            });

            tabWindowsTools.Controls.Add(pnlHpBiosSettings);
        }

        // ══════════════════════════════════════════════════════════
        //  UI Factory Helpers
        // ══════════════════════════════════════════════════════════

        private GroupBox CreateStyledGroupBox(string text, Point location, Size size)
        {
            var gb = new GroupBox
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = HeaderDark,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            return gb;
        }

        private Panel CreateStyledPanel(Point location, Size size)
        {
            var panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Subtle border via Paint event
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            return panel;
        }

        private Label CreateSectionHeader(string text, Point location, int width)
        {
            return new Label
            {
                Text = "  " + text,
                Location = location,
                Size = new Size(width, 32),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Button CreateAccentButton(string text, Point location, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AccentBlue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AccentHover;
            return btn;
        }

        private (Label tag, Label value) CreateInfoRow(string tagText, string defaultValue, int x, int y, Panel parent)
        {
            var tag = new Label
            {
                Text = tagText + ":",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = false,
                Size = new Size(140, 22),
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var val = new Label
            {
                Text = defaultValue,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextDark,
                AutoSize = false,
                Size = new Size(200, 22),
                Location = new Point(x + 144, y),
                TextAlign = ContentAlignment.MiddleLeft
            };

            parent.Controls.Add(tag);
            parent.Controls.Add(val);
            return (tag, val);
        }

        // ══════════════════════════════════════════════════════════
        //  Event Handlers
        // ══════════════════════════════════════════════════════════

        private async void btnQueryBios_Click(object sender, EventArgs e)
        {
            string computerName = txbBiosComputerName.Text.Trim();
            if (string.IsNullOrEmpty(computerName))
            {
                _consoleForm?.WriteWarning("Please enter a computer name or IP address.");
                lblBiosQueryStatusValue.Text = "Enter a computer name";
                lblBiosQueryStatusValue.ForeColor = WarnAmber;
                return;
            }

            if (!CredentialManager.IsAuthenticated)
            {
                _consoleForm?.WriteError("Please log in first before querying remote BIOS.");
                lblBiosQueryStatusValue.Text = "Not authenticated";
                lblBiosQueryStatusValue.ForeColor = WarnRed;
                return;
            }

            try
            {
                SetQueryBusy(true, "Querying...");

                string username = CredentialManager.GetUsername();
                string password = CredentialManager.GetPassword();
                string domain = CredentialManager.GetDomain();

                var result = await _wmiService.QueryRemoteBiosAsync(computerName, username, password, domain);

                if (result.Success)
                {
                    _lastBiosResult = result;
                    PopulateBiosResults(result);
                    SetQueryBusy(false, "Query complete");
                    lblBiosQueryStatusValue.ForeColor = GoodGreen;
                }
                else
                {
                    SetQueryBusy(false, result.ErrorMessage);
                    lblBiosQueryStatusValue.ForeColor = WarnRed;
                    _consoleForm?.WriteError($"BIOS query failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                SetQueryBusy(false, "Error - see console");
                lblBiosQueryStatusValue.ForeColor = WarnRed;
                _consoleForm?.WriteError($"BIOS query exception: {ex.Message}");
            }
        }

        private async void btnTestWmiConnection_Click(object sender, EventArgs e)
        {
            string computerName = txbBiosComputerName.Text.Trim();
            if (string.IsNullOrEmpty(computerName))
            {
                _consoleForm?.WriteWarning("Please enter a computer name or IP address.");
                return;
            }

            if (!CredentialManager.IsAuthenticated)
            {
                _consoleForm?.WriteError("Please log in first.");
                return;
            }

            try
            {
                btnTestWmiConnection.Enabled = false;
                btnTestWmiConnection.Text = "Testing...";
                lblBiosQueryStatusValue.Text = "Testing WMI connectivity...";
                lblBiosQueryStatusValue.ForeColor = AccentBlue;

                string username = CredentialManager.GetUsername();
                string password = CredentialManager.GetPassword();
                string domain = CredentialManager.GetDomain();

                bool connected = await _wmiService.TestWmiConnectivity(computerName, username, password, domain);

                if (connected)
                {
                    lblBiosQueryStatusValue.Text = "Connection successful";
                    lblBiosQueryStatusValue.ForeColor = GoodGreen;
                    _consoleForm?.WriteSuccess($"WMI connection to {computerName} successful.");
                }
                else
                {
                    lblBiosQueryStatusValue.Text = "Connection failed";
                    lblBiosQueryStatusValue.ForeColor = WarnRed;
                    _consoleForm?.WriteError($"WMI connection to {computerName} failed. Check firewall/WinRM settings.");
                }
            }
            catch (Exception ex)
            {
                lblBiosQueryStatusValue.Text = "Connection error";
                lblBiosQueryStatusValue.ForeColor = WarnRed;
                _consoleForm?.WriteError($"Connection test error: {ex.Message}");
            }
            finally
            {
                btnTestWmiConnection.Enabled = true;
                btnTestWmiConnection.Text = "Test Connection";
            }
        }

        private void btnClearBiosResults_Click(object sender, EventArgs e)
        {
            _lastBiosResult = null;
            ClearBiosResults();
            lblBiosQueryStatusValue.Text = "Ready";
            lblBiosQueryStatusValue.ForeColor = AccentBlue;
            txbBiosSettingsFilter.Clear();
            _consoleForm?.WriteInfo("BIOS results cleared.");
        }

        private void btnExportBiosResults_Click(object sender, EventArgs e)
        {
            if (_lastBiosResult == null || !_lastBiosResult.Success)
            {
                _consoleForm?.WriteWarning("No BIOS results to export. Run a query first.");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv",
                Title = "Export BIOS Results",
                FileName = $"BIOS_{_lastBiosResult.ComputerName}_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var lines = new List<string>();

                    if (dialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        // CSV format
                        lines.Add("Section,Property,Value");
                        lines.Add($"System,Computer Name,{_lastBiosResult.ComputerName}");
                        lines.Add($"System,Manufacturer,{_lastBiosResult.Manufacturer}");
                        lines.Add($"System,Model,{_lastBiosResult.Model}");
                        lines.Add($"System,Serial Number,{_lastBiosResult.SerialNumber}");
                        lines.Add($"System,BIOS Version,{_lastBiosResult.BiosVersion}");
                        lines.Add($"System,BIOS Date,{_lastBiosResult.BiosDate}");
                        lines.Add($"System,OS,{_lastBiosResult.OSName}");
                        lines.Add($"System,OS Version,{_lastBiosResult.OSVersion}");
                        lines.Add($"System,Architecture,{_lastBiosResult.OSArchitecture}");
                        lines.Add($"Security,TPM Present,{_lastBiosResult.TpmPresent}");
                        lines.Add($"Security,TPM Version,{_lastBiosResult.TpmVersion}");
                        lines.Add($"Security,TPM Enabled,{_lastBiosResult.TpmEnabled}");
                        lines.Add($"Security,TPM Activated,{_lastBiosResult.TpmActivated}");
                        lines.Add($"Security,Secure Boot,{_lastBiosResult.SecureBootEnabled}");

                        foreach (var setting in _lastBiosResult.HpBiosSettings)
                        {
                            // Escape CSV values that might contain commas
                            string val = setting.CurrentValue.Contains(',')
                                ? $"\"{setting.CurrentValue}\""
                                : setting.CurrentValue;
                            lines.Add($"{setting.Category},{setting.Name},{val}");
                        }
                    }
                    else
                    {
                        // Text format
                        lines.Add($"═══════════════════════════════════════════════════════════");
                        lines.Add($"  BIOS Report: {_lastBiosResult.ComputerName}");
                        lines.Add($"  Generated:   {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        lines.Add($"═══════════════════════════════════════════════════════════");
                        lines.Add("");
                        lines.Add("── SYSTEM INFORMATION ──────────────────────────────────────");
                        lines.Add($"  Manufacturer:    {_lastBiosResult.Manufacturer}");
                        lines.Add($"  Model:           {_lastBiosResult.Model}");
                        lines.Add($"  Serial Number:   {_lastBiosResult.SerialNumber}");
                        lines.Add($"  BIOS Version:    {_lastBiosResult.BiosVersion}");
                        lines.Add($"  BIOS Date:       {_lastBiosResult.BiosDate}");
                        lines.Add($"  OS:              {_lastBiosResult.OSName}");
                        lines.Add($"  OS Version:      {_lastBiosResult.OSVersion}");
                        lines.Add($"  Architecture:    {_lastBiosResult.OSArchitecture}");
                        lines.Add("");
                        lines.Add("── SECURITY STATUS ─────────────────────────────────────────");
                        lines.Add($"  TPM Present:     {_lastBiosResult.TpmPresent}");
                        lines.Add($"  TPM Version:     {_lastBiosResult.TpmVersion}");
                        lines.Add($"  TPM Enabled:     {_lastBiosResult.TpmEnabled}");
                        lines.Add($"  TPM Activated:   {_lastBiosResult.TpmActivated}");
                        lines.Add($"  Secure Boot:     {_lastBiosResult.SecureBootEnabled}");

                        if (_lastBiosResult.HpBiosSettings.Count > 0)
                        {
                            lines.Add("");
                            lines.Add("── HP BIOS SETTINGS ────────────────────────────────────────");

                            var grouped = _lastBiosResult.HpBiosSettings
                                .OrderBy(s => s.Category)
                                .ThenBy(s => s.Name)
                                .GroupBy(s => s.Category);

                            foreach (var group in grouped)
                            {
                                lines.Add($"");
                                lines.Add($"  [{group.Key}]");
                                foreach (var setting in group)
                                {
                                    lines.Add($"    {setting.Name,-40} {setting.CurrentValue}");
                                }
                            }
                        }

                        lines.Add("");
                        lines.Add("═══════════════════════════════════════════════════════════");
                    }

                    System.IO.File.WriteAllLines(dialog.FileName, lines);
                    _consoleForm?.WriteSuccess($"BIOS results exported to: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Export failed: {ex.Message}");
                }
            }
        }

        private void txbBiosSettingsFilter_TextChanged(object sender, EventArgs e)
        {
            if (_lastBiosResult == null) return;
            PopulateHpBiosGrid(_lastBiosResult.HpBiosSettings, txbBiosSettingsFilter.Text.Trim());
        }

        // ══════════════════════════════════════════════════════════
        //  Data Population
        // ══════════════════════════════════════════════════════════

        private void PopulateBiosResults(BiosQueryResult result)
        {
            // System info
            lblManufacturerValue.Text = result.Manufacturer ?? "—";
            lblModelValue.Text = result.Model ?? "—";
            lblSerialValue.Text = result.SerialNumber ?? "—";
            lblBiosVersionValue.Text = result.BiosVersion ?? "—";
            lblBiosDateValue.Text = result.BiosDate ?? "—";
            lblOsNameValue.Text = result.OSName ?? "—";
            lblOsVersionValue.Text = result.OSVersion ?? "—";
            lblOsArchValue.Text = result.OSArchitecture ?? "—";

            // Security - with color indicators
            SetSecurityValue(lblTpmPresentValue, result.TpmPresent, "Yes");
            lblTpmVersionValue.Text = result.TpmVersion ?? "—";
            SetSecurityValue(lblTpmEnabledValue, result.TpmEnabled, "True");
            SetSecurityValue(lblTpmActivatedValue, result.TpmActivated, "True");
            SetSecurityValue(lblSecureBootValue, result.SecureBootEnabled, "Enabled");

            // HP BIOS settings grid
            if (result.IsHpMachine && result.HpBiosSettings.Count > 0)
            {
                PopulateHpBiosGrid(result.HpBiosSettings, "");
                lblHpBiosHeader.Text = "  HP BIOS SETTINGS";
            }
            else if (result.IsHpMachine)
            {
                dgvHpBiosSettings.Rows.Clear();
                lblBiosSettingsCount.Text = "HP machine detected but no BIOS settings retrieved";
                lblHpBiosHeader.Text = "  HP BIOS SETTINGS";
            }
            else
            {
                dgvHpBiosSettings.Rows.Clear();
                lblBiosSettingsCount.Text = $"Non-HP machine ({result.Manufacturer}) — HP BIOS namespace not available";
                lblHpBiosHeader.Text = "  BIOS SETTINGS (HP namespace not available)";
            }
        }

        private void PopulateHpBiosGrid(List<BiosSetting> settings, string filter)
        {
            dgvHpBiosSettings.Rows.Clear();

            var filtered = string.IsNullOrEmpty(filter)
                ? settings
                : settings.Where(s =>
                    s.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.CurrentValue.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.Category.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

            foreach (var setting in filtered.OrderBy(s => s.Category).ThenBy(s => s.Name))
            {
                dgvHpBiosSettings.Rows.Add(setting.Category, setting.Name, setting.CurrentValue);
            }

            lblBiosSettingsCount.Text = string.IsNullOrEmpty(filter)
                ? $"{settings.Count} settings"
                : $"{filtered.Count} of {settings.Count} settings";
        }

        private void SetSecurityValue(Label label, string value, string goodValue)
        {
            if (string.IsNullOrEmpty(value) || value == "—")
            {
                label.Text = "—";
                label.ForeColor = TextMuted;
                return;
            }

            label.Text = value;

            if (value.IndexOf(goodValue, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                label.ForeColor = GoodGreen;
                label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            else if (value.Equals("No", StringComparison.OrdinalIgnoreCase) ||
                     value.Equals("False", StringComparison.OrdinalIgnoreCase) ||
                     value.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            {
                label.ForeColor = WarnRed;
                label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            else
            {
                label.ForeColor = WarnAmber;
                label.Font = new Font("Segoe UI", 9.5F);
            }
        }

        private void ClearBiosResults()
        {
            // System info
            lblManufacturerValue.Text = "—";
            lblModelValue.Text = "—";
            lblSerialValue.Text = "—";
            lblBiosVersionValue.Text = "—";
            lblBiosDateValue.Text = "—";
            lblOsNameValue.Text = "—";
            lblOsVersionValue.Text = "—";
            lblOsArchValue.Text = "—";

            // Security
            foreach (var lbl in new[] { lblTpmPresentValue, lblTpmVersionValue, lblTpmEnabledValue,
                                        lblTpmActivatedValue, lblSecureBootValue })
            {
                lbl.Text = "—";
                lbl.ForeColor = TextDark;
                lbl.Font = new Font("Segoe UI", 9.5F);
            }

            // Grid
            dgvHpBiosSettings.Rows.Clear();
            lblBiosSettingsCount.Text = "";
        }

        private void SetQueryBusy(bool busy, string statusText)
        {
            btnQueryBios.Enabled = !busy;
            btnQueryBios.Text = busy ? "Querying..." : "Query BIOS";
            pgbBiosQuery.Visible = busy;
            lblBiosQueryStatusValue.Text = statusText;
            lblBiosQueryStatusValue.ForeColor = busy ? AccentBlue : TextDark;
        }
    }
}
