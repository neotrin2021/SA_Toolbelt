using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SA_ToolBelt
{
    public partial class SAToolBelt : Form
    {
        // -------------------------------------------------------------------------
        // GPO Tab — Button Handlers
        // -------------------------------------------------------------------------

        private async void btnGpoScrape_Click(object sender, EventArgs e)
        {
            SetGpoButtonsEnabled(false);
            lblGpoStatus.Text = "Status: Scraping...";

            try
            {
                var result = await _gpoService.ScrapeAllGposAsync();

                if (result.Success)
                {
                    lblGpoStatus.Text = $"Last scraped: {result.ScrapeTime:yyyy-MM-dd HH:mm}  |  {result.GposScraped} GPOs  |  {result.SettingsSaved} settings";
                    rtbGpoReport.Text = $"Scrape complete — {result.GposScraped} GPOs, {result.SettingsSaved} settings captured.";
                    await PopulateGpoTreeAsync();
                }
                else
                {
                    lblGpoStatus.Text = "Status: Scrape failed";
                    rtbGpoReport.Text = $"Scrape failed:\n{result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                lblGpoStatus.Text = "Status: Error";
                rtbGpoReport.Text = $"Unexpected error during scrape:\n{ex.Message}";
                _consoleForm?.WriteError($"GPO Scrape exception: {ex.Message}");
            }
            finally
            {
                SetGpoButtonsEnabled(true);
            }
        }

        private async void BtnGpoRefresh_Click(object sender, EventArgs e)
        {
            SetGpoButtonsEnabled(false);
            lblGpoStatus.Text = "Status: Refreshing...";

            try
            {
                // Snapshot previous scrape time before we overwrite
                DateTime? previousScrapeTime = _gpoService.GetLastScrapeTime();

                // Load previous settings for diff
                var previousSettings = _gpoService.LoadAllSettingsFromDb();

                // Run full scrape (this saves new data + writes changelog internally)
                var result = await _gpoService.ScrapeAllGposAsync();

                if (result.Success)
                {
                    lblGpoStatus.Text = $"Last scraped: {result.ScrapeTime:yyyy-MM-dd HH:mm}  |  {result.GposScraped} GPOs  |  {result.SettingsSaved} settings";

                    // Build change report from diff
                    var newSettings = _gpoService.LoadAllSettingsFromDb();
                    var changes = _gpoService.DetectChanges(previousSettings, newSettings);
                    rtbGpoReport.Text = _gpoService.BuildChangeReport(changes, previousScrapeTime);

                    await PopulateGpoTreeAsync();
                }
                else
                {
                    lblGpoStatus.Text = "Status: Refresh failed";
                    rtbGpoReport.Text = $"Refresh failed:\n{result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                lblGpoStatus.Text = "Status: Error";
                rtbGpoReport.Text = $"Unexpected error during refresh:\n{ex.Message}";
                _consoleForm?.WriteError($"GPO Refresh exception: {ex.Message}");
            }
            finally
            {
                SetGpoButtonsEnabled(true);
            }
        }

        private void BtnGpoDuplicates_Click(object sender, EventArgs e)
        {
            SetGpoButtonsEnabled(false);
            lblGpoStatus.Text = "Status: Finding duplicates...";

            try
            {
                var duplicates = _gpoService.FindDuplicateSettings();
                rtbGpoReport.Text = _gpoService.BuildDuplicateReport(duplicates);
                lblGpoStatus.Text = $"Duplicate check complete — {duplicates.Count} duplicate setting(s) found.";
            }
            catch (Exception ex)
            {
                lblGpoStatus.Text = "Status: Error";
                rtbGpoReport.Text = $"Error during duplicate detection:\n{ex.Message}";
                _consoleForm?.WriteError($"GPO Duplicate detection exception: {ex.Message}");
            }
            finally
            {
                SetGpoButtonsEnabled(true);
            }
        }

        // -------------------------------------------------------------------------
        // GPO Tab — TreeView Handler
        // -------------------------------------------------------------------------

        private void TvwGpoTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is string gpoName && !string.IsNullOrEmpty(gpoName))
            {
                lvwGpoSettings.Items.Clear();

                try
                {
                    var settings = _gpoService.GetSettingsForGpo(gpoName);

                    lvwGpoSettings.BeginUpdate();
                    foreach (var s in settings)
                    {
                        var item = new ListViewItem(s.Category ?? "");
                        item.SubItems.Add(s.SettingName ?? "");
                        item.SubItems.Add(s.SettingValue ?? "");
                        item.SubItems.Add(s.SettingState ?? "");
                        lvwGpoSettings.Items.Add(item);
                    }
                    lvwGpoSettings.EndUpdate();
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Could not load settings for GPO '{gpoName}': {ex.Message}");
                }
            }
        }

        // -------------------------------------------------------------------------
        // GPO Tab — Tree Population
        // -------------------------------------------------------------------------

        private async Task PopulateGpoTreeAsync()
        {
            tvwGpoTree.Nodes.Clear();

            try
            {
                // Try to load tree from DB first (fast, no network hit)
                var roots = _gpoService.LoadOuTreeFromDb();

                if (roots.Count == 0)
                {
                    // Nothing in DB yet — do a live fetch
                    roots = await _gpoService.GetOuTreeAsync();
                }

                tvwGpoTree.BeginUpdate();
                foreach (var root in roots)
                {
                    var rootNode = BuildTreeNode(root);
                    tvwGpoTree.Nodes.Add(rootNode);
                }
                tvwGpoTree.EndUpdate();

                if (tvwGpoTree.Nodes.Count > 0)
                    tvwGpoTree.Nodes[0].Expand();
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"GPO tree population failed: {ex.Message}");
            }
        }

        private TreeNode BuildTreeNode(GPO_Service.OuNode ouNode)
        {
            var node = new TreeNode(ouNode.Name);
            node.ForeColor = Color.Black;

            // Add linked GPOs as child nodes (styled differently)
            foreach (string gpoName in ouNode.LinkedGpoNames)
            {
                if (string.IsNullOrWhiteSpace(gpoName)) continue;

                var gpoNode = new TreeNode(gpoName)
                {
                    ForeColor = Color.SteelBlue,
                    Tag = gpoName   // Tag used by TvwGpoTree_AfterSelect to identify GPO nodes
                };
                node.Nodes.Add(gpoNode);
            }

            // Recurse children
            foreach (var child in ouNode.Children)
            {
                node.Nodes.Add(BuildTreeNode(child));
            }

            return node;
        }

        // -------------------------------------------------------------------------
        // GPO Tab — Helpers
        // -------------------------------------------------------------------------

        private void SetGpoButtonsEnabled(bool enabled)
        {
            if (btnGpoScrape.InvokeRequired)
            {
                btnGpoScrape.Invoke(() => SetGpoButtonsEnabled(enabled));
                return;
            }
            btnGpoScrape.Enabled = enabled;
            btnGpoRefresh.Enabled = enabled;
            btnGpoDuplicates.Enabled = enabled;
        }

        private void InitializeGpoTab()
        {
            // Wire event handlers
            btnGpoScrape.Click += btnGpoScrape_Click;
            btnGpoRefresh.Click += BtnGpoRefresh_Click;
            btnGpoDuplicates.Click += BtnGpoDuplicates_Click;
            tvwGpoTree.AfterSelect += TvwGpoTree_AfterSelect;

            // Show last scrape time if data already exists
            var lastScrape = _gpoService?.GetLastScrapeTime();
            if (lastScrape.HasValue)
                lblGpoStatus.Text = $"Last scraped: {lastScrape.Value:yyyy-MM-dd HH:mm}";
        }
    }
}
