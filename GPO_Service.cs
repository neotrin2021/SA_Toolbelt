using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace SA_ToolBelt
{
    public class GPO_Service
    {
        private readonly ConsoleForm _consoleForm;
        private readonly Windows_Tools _windowsTools;
        private readonly DatabaseService _databaseService;

        public GPO_Service(ConsoleForm consoleForm, Windows_Tools windowsTools, DatabaseService databaseService)
        {
            _consoleForm = consoleForm;
            _windowsTools = windowsTools;
            _databaseService = databaseService;
        }

        // -------------------------------------------------------------------------
        // Data Models
        // -------------------------------------------------------------------------

        public class OuNode
        {
            public string Name { get; set; } = "";
            public string DistinguishedName { get; set; } = "";
            public string ParentDn { get; set; } = "";
            public List<string> LinkedGpoNames { get; set; } = new List<string>();
            public List<OuNode> Children { get; set; } = new List<OuNode>();
        }

        public class GpoSettingRecord
        {
            public string GpoName { get; set; } = "";
            public string Category { get; set; } = "";
            public string SettingName { get; set; } = "";
            public string SettingValue { get; set; } = "";
            public string SettingState { get; set; } = "";
        }

        public class GpoChangeRecord
        {
            public string GpoName { get; set; } = "";
            public string Category { get; set; } = "";
            public string SettingName { get; set; } = "";
            public string OldValue { get; set; } = "";
            public string NewValue { get; set; } = "";
            public string ChangeType { get; set; } = ""; // Added / Removed / Modified
        }

        public class GpoDuplicateRecord
        {
            public string SettingName { get; set; } = "";
            public string SettingValue { get; set; } = "";
            public List<string> GpoNames { get; set; } = new List<string>();
        }

        public class GpoScrapeResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = "";
            public int GposScraped { get; set; }
            public int SettingsSaved { get; set; }
            public DateTime ScrapeTime { get; set; }
        }

        // -------------------------------------------------------------------------
        // Scrape — Full Domain GPO Scan
        // -------------------------------------------------------------------------

        /// <summary>
        /// Scrapes all GPOs from the domain, parses their settings, builds the OU
        /// link map, and saves everything to the database. Returns a summary result.
        /// </summary>
        public async Task<GpoScrapeResult> ScrapeAllGposAsync(string domain = null)
        {
            var result = new GpoScrapeResult { ScrapeTime = DateTime.Now };

            try
            {
                _consoleForm?.WriteInfo("GPO Scrape: Starting full domain GPO scan...");

                // Step 1: Get all GPO names and IDs from the domain
                var gpoListResult = await _windowsTools.GetAllGposAsync(domain);
                if (!gpoListResult.Success || gpoListResult.Gpos.Count == 0)
                {
                    result.ErrorMessage = gpoListResult.ErrorMessage ?? "No GPOs found in domain.";
                    _consoleForm?.WriteError($"GPO Scrape failed: {result.ErrorMessage}");
                    return result;
                }

                var allGpos = gpoListResult.Gpos;
                _consoleForm?.WriteInfo($"GPO Scrape: Found {allGpos.Count} GPOs. Fetching settings...");

                // Step 2: Snapshot current settings before overwriting (for change detection)
                var previousSettings = LoadAllSettingsFromDb();

                // Step 3: For each GPO, fetch the XML report and parse settings
                var allNewSettings = new List<GpoSettingRecord>();
                int scraped = 0;

                foreach (var gpo in allGpos)
                {
                    _consoleForm?.WriteInfo($"  [{scraped + 1}/{allGpos.Count}] Scraping: {gpo.DisplayName}");
                    try
                    {
                        string xml = await _windowsTools.GetGpoReportXmlAsync(gpo.DisplayName, domain);
                        if (!string.IsNullOrWhiteSpace(xml))
                        {
                            var settings = ParseGpoXml(gpo.DisplayName, xml);
                            allNewSettings.AddRange(settings);
                        }
                        else
                        {
                            _consoleForm?.WriteWarning($"  No XML returned for GPO: {gpo.DisplayName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteWarning($"  Could not scrape GPO '{gpo.DisplayName}': {ex.Message}");
                    }
                    scraped++;
                }

                // Step 4: Build OU tree and link map
                _consoleForm?.WriteInfo("GPO Scrape: Building OU link map...");
                var ouNodes = await GetOuTreeAsync(domain, allGpos);

                // Step 5: Detect changes before saving new data
                var changes = DetectChanges(previousSettings, allNewSettings);

                // Step 6: Save everything to the database
                string scrapeTime = DateTime.Now.ToString("o");
                SaveSettingsToDb(allNewSettings, scrapeTime);
                SaveOuLinksToDb(ouNodes, scrapeTime);
                RecordScrapeHistory(scraped, allNewSettings.Count);

                // Step 7: Save changelog if anything changed
                if (changes.Count > 0)
                    SaveChangelogToDb(changes);

                result.Success = true;
                result.GposScraped = scraped;
                result.SettingsSaved = allNewSettings.Count;

                _consoleForm?.WriteSuccess($"GPO Scrape complete: {scraped} GPOs, {allNewSettings.Count} settings, {changes.Count} changes detected.");
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _consoleForm?.WriteError($"GPO Scrape failed: {ex.Message}");
            }

            return result;
        }

        // -------------------------------------------------------------------------
        // OU Tree Builder
        // -------------------------------------------------------------------------

        /// <summary>
        /// Walks the AD OU hierarchy using DirectoryServices, reads gPLink on each
        /// OU to find attached GPOs, and returns a recursive tree of OuNode objects.
        /// </summary>
        public async Task<List<OuNode>> GetOuTreeAsync(string domain = null,
            List<Windows_Tools.GpoInfo> knownGpos = null)
        {
            return await Task.Run(() =>
            {
                var roots = new List<OuNode>();

                try
                {
                    // Build a GUID → DisplayName lookup so we can resolve gPLink entries
                    var guidToName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (knownGpos != null)
                    {
                        foreach (var g in knownGpos)
                        {
                            if (!string.IsNullOrEmpty(g.Id))
                                guidToName[g.Id.Trim('{', '}')] = g.DisplayName;
                        }
                    }

                    string ldapRoot = string.IsNullOrEmpty(domain)
                        ? $"LDAP://{Environment.UserDomainName}"
                        : $"LDAP://{domain}";

                    using var rootEntry = new DirectoryEntry(ldapRoot);

                    // Also build guidToName directly from AD's CN=Policies,CN=System container.
                    // The CN of each GPO object IS the GUID in {XXXXXXXX-...} format — exactly
                    // what gPLink uses — so this lookup is always format-correct regardless of
                    // how PowerShell returns the Id type.
                    try
                    {
                        string rootDn = rootEntry.Properties["distinguishedName"]?.Value?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(rootDn))
                        {
                            using var policiesEntry = new DirectoryEntry($"LDAP://CN=Policies,CN=System,{rootDn}");
                            using var policySearcher = new DirectorySearcher(policiesEntry)
                            {
                                Filter = "(objectClass=groupPolicyContainer)",
                                SearchScope = SearchScope.OneLevel
                            };
                            policySearcher.PropertiesToLoad.AddRange(new[] { "cn", "displayName" });

                            foreach (SearchResult sr in policySearcher.FindAll())
                            {
                                string cn = sr.Properties["cn"]?.Count > 0
                                    ? sr.Properties["cn"][0]?.ToString()?.Trim('{', '}') ?? ""
                                    : "";
                                string displayName = sr.Properties["displayName"]?.Count > 0
                                    ? sr.Properties["displayName"][0]?.ToString() ?? ""
                                    : "";
                                if (!string.IsNullOrEmpty(cn) && !string.IsNullOrEmpty(displayName))
                                    guidToName[cn] = displayName;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleForm?.WriteWarning($"Could not enumerate GPO Policies container for name lookup: {ex.Message}");
                    }

                    // Build the root domain node
                    var domainNode = new OuNode
                    {
                        Name = rootEntry.Name ?? domain ?? Environment.UserDomainName,
                        DistinguishedName = rootEntry.Properties["distinguishedName"]?.Value?.ToString() ?? "",
                        ParentDn = "",
                        LinkedGpoNames = ExtractGpoNamesFromGpLink(
                            rootEntry.Properties["gPLink"]?.Value?.ToString(), guidToName)
                    };

                    RecurseOUs(rootEntry, domainNode, guidToName);
                    roots.Add(domainNode);
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"OU tree build failed: {ex.Message}");
                }

                return roots;
            });
        }

        private void RecurseOUs(DirectoryEntry parent, OuNode parentNode,
            Dictionary<string, string> guidToName)
        {
            try
            {
                using var searcher = new DirectorySearcher(parent)
                {
                    Filter = "(objectClass=organizationalUnit)",
                    SearchScope = SearchScope.OneLevel
                };
                searcher.PropertiesToLoad.AddRange(new[]
                    { "name", "distinguishedName", "gPLink" });

                foreach (SearchResult result in searcher.FindAll())
                {
                    string name = result.Properties["name"]?.Count > 0
                        ? result.Properties["name"][0]?.ToString() ?? ""
                        : "";
                    string dn = result.Properties["distinguishedName"]?.Count > 0
                        ? result.Properties["distinguishedName"][0]?.ToString() ?? ""
                        : "";
                    string gpLink = result.Properties["gPLink"]?.Count > 0
                        ? result.Properties["gPLink"][0]?.ToString() ?? ""
                        : "";

                    var node = new OuNode
                    {
                        Name = name,
                        DistinguishedName = dn,
                        ParentDn = parentNode.DistinguishedName,
                        LinkedGpoNames = ExtractGpoNamesFromGpLink(gpLink, guidToName)
                    };

                    using var childEntry = result.GetDirectoryEntry();
                    RecurseOUs(childEntry, node, guidToName);

                    parentNode.Children.Add(node);
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"Could not enumerate OUs under {parentNode.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses the gPLink attribute value and resolves GUIDs to GPO display names.
        /// gPLink format: [LDAP://CN={GUID},CN=Policies,...;flag][LDAP://...]
        /// flag: 0=enabled, 1=disabled, 2=enforced
        /// </summary>
        private List<string> ExtractGpoNamesFromGpLink(string gpLink,
            Dictionary<string, string> guidToName)
        {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(gpLink)) return names;

            // Extract all GUIDs from the gPLink string
            var matches = Regex.Matches(gpLink,
                @"\{([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})\}",
                RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                string guid = m.Groups[1].Value;
                if (guidToName.TryGetValue(guid, out string name))
                    names.Add(name);
                else
                    names.Add($"{{Unknown GPO: {guid}}}");
            }

            return names;
        }

        // -------------------------------------------------------------------------
        // XML Parser — Get-GPOReport XML → GpoSettingRecord list
        // -------------------------------------------------------------------------

        /// <summary>
        /// Parses the XML string returned by Get-GPOReport -ReportType Xml into a
        /// flat list of GpoSettingRecord objects. Uses LocalName comparisons to
        /// avoid dealing with the multiple namespaces in the GPO XML schema.
        /// </summary>
        public List<GpoSettingRecord> ParseGpoXml(string gpoName, string xml)
        {
            var settings = new List<GpoSettingRecord>();

            try
            {
                var doc = XDocument.Parse(xml);

                // Process both Computer and User configuration sections
                foreach (string scope in new[] { "Computer", "User" })
                {
                    var scopeElement = doc.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == scope);
                    if (scopeElement == null) continue;

                    var extensionDataElement = scopeElement.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "ExtensionData");
                    if (extensionDataElement == null) continue;

                    foreach (var extension in extensionDataElement.Elements()
                        .Where(e => e.Name.LocalName == "Extension"))
                    {
                        // The xsi:type attribute gives us the category (e.g. "q1:SecuritySettings")
                        string typeAttr = extension.Attributes()
                            .FirstOrDefault(a => a.Name.LocalName == "type")?.Value ?? "";
                        // Strip namespace prefix (e.g. "q1:SecuritySettings" → "SecuritySettings")
                        string category = $"{scope} Configuration \\ " +
                            (typeAttr.Contains(':')
                                ? typeAttr.Substring(typeAttr.IndexOf(':') + 1)
                                : (string.IsNullOrEmpty(typeAttr) ? "Unknown" : typeAttr));

                        var extracted = ExtractSettingsFromExtension(extension, gpoName, category);
                        settings.AddRange(extracted);
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"XML parse error for GPO '{gpoName}': {ex.Message}");
            }

            return settings;
        }

        /// <summary>
        /// Recursively walks an Extension XElement looking for named settings.
        /// Handles the most common GPO extension types generically.
        /// </summary>
        private List<GpoSettingRecord> ExtractSettingsFromExtension(
            XElement extension, string gpoName, string category)
        {
            var results = new List<GpoSettingRecord>();

            // Strategy: look for elements that have a Name child and at least one
            // of: State, Value, SettingNumber, SettingBoolean, KeyPath, etc.
            // This covers Security Settings, Registry Policies, Windows Firewall, Scripts, etc.

            var candidates = extension.Descendants()
                .Where(e => e.Elements().Any(c => c.Name.LocalName == "Name"));

            foreach (var element in candidates)
            {
                string settingName = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "Name")?.Value?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(settingName)) continue;

                // Try to find a value from common child element names
                string settingValue = "";
                string settingState = "";

                // Check for State (most Registry/ADMX policies)
                var stateEl = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "State");
                if (stateEl != null)
                    settingState = stateEl.Value?.Trim() ?? "";

                // Check for numeric setting (Security Settings)
                var numEl = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "SettingNumber");
                if (numEl != null)
                    settingValue = numEl.Value?.Trim() ?? "";

                // Check for boolean setting (Security Settings)
                var boolEl = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "SettingBoolean");
                if (boolEl != null)
                    settingValue = boolEl.Value?.Trim() ?? "";

                // Check for string value (many policy types)
                var valEl = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "Value");
                if (valEl != null && string.IsNullOrEmpty(settingValue))
                    settingValue = valEl.Value?.Trim() ?? "";

                // Check for Explain/Description (Registry policies sometimes have this)
                var explainEl = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "Explain");
                // We don't store explanations — just use it as confirmation the record is valid

                // Also grab DropDownList or ListBox values (multi-value settings)
                var dropDownEl = element.Elements()
                    .FirstOrDefault(c => c.Name.LocalName == "DropDownList");
                if (dropDownEl != null && string.IsNullOrEmpty(settingValue))
                {
                    var valueEl = dropDownEl.Elements()
                        .FirstOrDefault(c => c.Name.LocalName == "Value");
                    if (valueEl != null)
                        settingValue = valueEl.Elements()
                            .FirstOrDefault(c => c.Name.LocalName == "Name")?.Value?.Trim()
                            ?? valueEl.Value?.Trim() ?? "";
                }

                // If we still have no value, check for any child elements not already checked
                if (string.IsNullOrEmpty(settingValue) && string.IsNullOrEmpty(settingState))
                {
                    // Grab the first non-Name child's value as a fallback
                    var firstOtherChild = element.Elements()
                        .FirstOrDefault(c => c.Name.LocalName != "Name"
                                          && c.Name.LocalName != "Explain"
                                          && !string.IsNullOrWhiteSpace(c.Value));
                    if (firstOtherChild != null)
                        settingValue = firstOtherChild.Value.Trim();
                }

                // Only add if we have at least a name
                if (!string.IsNullOrWhiteSpace(settingName))
                {
                    results.Add(new GpoSettingRecord
                    {
                        GpoName = gpoName,
                        Category = category,
                        SettingName = settingName,
                        SettingValue = settingValue,
                        SettingState = settingState
                    });
                }
            }

            // Deduplicate: same GPO/Category/SettingName shouldn't appear twice
            return results
                .GroupBy(r => $"{r.GpoName}|{r.Category}|{r.SettingName}")
                .Select(g => g.First())
                .ToList();
        }

        // -------------------------------------------------------------------------
        // Change Detection
        // -------------------------------------------------------------------------

        /// <summary>
        /// Compares previous DB settings to new scraped settings.
        /// Returns a list of Added / Removed / Modified records.
        /// </summary>
        public List<GpoChangeRecord> DetectChanges(
            List<GpoSettingRecord> previous,
            List<GpoSettingRecord> current)
        {
            var changes = new List<GpoChangeRecord>();

            // Key = "GPOName|Category|SettingName"
            // Use a safe loop instead of .ToDictionary() to avoid crashing on duplicate DB rows
            var prevDict = new Dictionary<string, GpoSettingRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in previous)
            {
                string key = $"{r.GpoName}|{r.Category}|{r.SettingName}";
                if (!prevDict.ContainsKey(key))
                    prevDict[key] = r;
            }

            var currDict = new Dictionary<string, GpoSettingRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in current)
            {
                string key = $"{r.GpoName}|{r.Category}|{r.SettingName}";
                if (!currDict.ContainsKey(key))
                    currDict[key] = r;
            }

            // Find Modified and Added
            foreach (var kvp in currDict)
            {
                if (prevDict.TryGetValue(kvp.Key, out var old))
                {
                    // Setting existed before — check if value changed
                    string oldCombo = $"{old.SettingValue}|{old.SettingState}";
                    string newCombo = $"{kvp.Value.SettingValue}|{kvp.Value.SettingState}";
                    if (!string.Equals(oldCombo, newCombo, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add(new GpoChangeRecord
                        {
                            GpoName = kvp.Value.GpoName,
                            Category = kvp.Value.Category,
                            SettingName = kvp.Value.SettingName,
                            OldValue = FormatValue(old),
                            NewValue = FormatValue(kvp.Value),
                            ChangeType = "Modified"
                        });
                    }
                }
                else
                {
                    // New setting that didn't exist before
                    changes.Add(new GpoChangeRecord
                    {
                        GpoName = kvp.Value.GpoName,
                        Category = kvp.Value.Category,
                        SettingName = kvp.Value.SettingName,
                        OldValue = "",
                        NewValue = FormatValue(kvp.Value),
                        ChangeType = "Added"
                    });
                }
            }

            // Find Removed
            foreach (var kvp in prevDict)
            {
                if (!currDict.ContainsKey(kvp.Key))
                {
                    changes.Add(new GpoChangeRecord
                    {
                        GpoName = kvp.Value.GpoName,
                        Category = kvp.Value.Category,
                        SettingName = kvp.Value.SettingName,
                        OldValue = FormatValue(kvp.Value),
                        NewValue = "",
                        ChangeType = "Removed"
                    });
                }
            }

            return changes;
        }

        private string FormatValue(GpoSettingRecord r)
        {
            if (!string.IsNullOrEmpty(r.SettingState) && !string.IsNullOrEmpty(r.SettingValue))
                return $"{r.SettingState} / {r.SettingValue}";
            if (!string.IsNullOrEmpty(r.SettingState))
                return r.SettingState;
            return r.SettingValue ?? "";
        }

        // -------------------------------------------------------------------------
        // Duplicate Detection
        // -------------------------------------------------------------------------

        /// <summary>
        /// Queries GPO_Settings for the same SettingName + SettingValue appearing
        /// in 2 or more distinct GPOs. Returns sorted by duplicate count descending.
        /// </summary>
        public List<GpoDuplicateRecord> FindDuplicateSettings()
        {
            var results = new List<GpoDuplicateRecord>();

            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                string sql = @"
                    SELECT Setting_Name, Setting_Value,
                           GROUP_CONCAT(DISTINCT GPO_Name) AS GPO_Names,
                           COUNT(DISTINCT GPO_Name) AS GPO_Count
                    FROM GPO_Settings
                    WHERE Setting_Value IS NOT NULL AND Setting_Value != ''
                    GROUP BY Setting_Name, Setting_Value
                    HAVING COUNT(DISTINCT GPO_Name) > 1
                    ORDER BY GPO_Count DESC, Setting_Name";

                using var cmd = new SqliteCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string gpoNamesCsv = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    results.Add(new GpoDuplicateRecord
                    {
                        SettingName = reader.GetString(0),
                        SettingValue = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        GpoNames = gpoNamesCsv
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Duplicate detection failed: {ex.Message}");
            }

            return results;
        }

        // -------------------------------------------------------------------------
        // Report Builders
        // -------------------------------------------------------------------------

        /// <summary>
        /// Builds a human-readable change report from a list of GpoChangeRecord objects.
        /// </summary>
        public string BuildChangeReport(List<GpoChangeRecord> changes, DateTime? previousScrape = null)
        {
            if (changes.Count == 0)
                return $"GPO CHANGE REPORT — {DateTime.Now:yyyy-MM-dd HH:mm}\n" +
                       "=".PadRight(60, '=') + "\n\nNo changes detected since last scrape.";

            var sb = new StringBuilder();
            sb.AppendLine($"GPO CHANGE REPORT — {DateTime.Now:yyyy-MM-dd HH:mm}");
            if (previousScrape.HasValue)
                sb.AppendLine($"Compared to scrape: {previousScrape.Value:yyyy-MM-dd HH:mm}");
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine($"Total changes: {changes.Count}");
            sb.AppendLine();

            foreach (string changeType in new[] { "Modified", "Added", "Removed" })
            {
                var group = changes.Where(c => c.ChangeType == changeType).ToList();
                if (group.Count == 0) continue;

                sb.AppendLine($"── {changeType.ToUpper()} ({group.Count}) ──");
                sb.AppendLine();

                foreach (var c in group.OrderBy(x => x.GpoName).ThenBy(x => x.SettingName))
                {
                    sb.AppendLine($"  GPO:      {c.GpoName}");
                    sb.AppendLine($"  Category: {c.Category}");
                    sb.AppendLine($"  Setting:  {c.SettingName}");

                    if (changeType == "Modified")
                        sb.AppendLine($"  Change:   {c.OldValue}  →  {c.NewValue}");
                    else if (changeType == "Added")
                        sb.AppendLine($"  Value:    {c.NewValue}");
                    else
                        sb.AppendLine($"  Was:      {c.OldValue}");

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a human-readable duplicate settings report.
        /// </summary>
        public string BuildDuplicateReport(List<GpoDuplicateRecord> duplicates)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DUPLICATE SETTINGS REPORT — {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("=".PadRight(60, '='));

            if (duplicates.Count == 0)
            {
                sb.AppendLine("\nNo duplicate settings found. Your GPOs are clean.");
                return sb.ToString();
            }

            sb.AppendLine($"Found {duplicates.Count} setting(s) duplicated across multiple GPOs.");
            sb.AppendLine();

            foreach (var d in duplicates)
            {
                sb.AppendLine($"  [SETTING]  {d.SettingName}");
                sb.AppendLine($"  [VALUE]    {d.SettingValue}");
                sb.AppendLine($"  [FOUND IN] {string.Join(", ", d.GpoNames)}");
                sb.AppendLine("  [NOTE]     Consider consolidating into one GPO linked to a parent OU.");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // -------------------------------------------------------------------------
        // Database Read/Write
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns the last scrape datetime from GPO_Scrape_History, or null.
        /// </summary>
        public DateTime? GetLastScrapeTime()
        {
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var cmd = new SqliteCommand(
                    "SELECT Scrape_Time FROM GPO_Scrape_History ORDER BY Id DESC LIMIT 1",
                    connection);

                var val = cmd.ExecuteScalar();
                if (val == null || val == DBNull.Value) return null;

                if (DateTime.TryParse(val.ToString(), out DateTime dt))
                    return dt;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Reads all current settings from GPO_Settings into memory.
        /// Called before a refresh scrape so we can diff old vs new.
        /// </summary>
        public List<GpoSettingRecord> LoadAllSettingsFromDb()
        {
            var list = new List<GpoSettingRecord>();
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var cmd = new SqliteCommand(
                    "SELECT GPO_Name, Category, Setting_Name, Setting_Value, Setting_State FROM GPO_Settings",
                    connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new GpoSettingRecord
                    {
                        GpoName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Category = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        SettingName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        SettingValue = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        SettingState = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    });
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"Could not load existing GPO settings: {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// Returns all settings for a single GPO from the database (no network hit).
        /// </summary>
        public List<GpoSettingRecord> GetSettingsForGpo(string gpoName)
        {
            var list = new List<GpoSettingRecord>();
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var cmd = new SqliteCommand(
                    "SELECT GPO_Name, Category, Setting_Name, Setting_Value, Setting_State " +
                    "FROM GPO_Settings WHERE GPO_Name = @gpoName ORDER BY Category, Setting_Name",
                    connection);
                cmd.Parameters.AddWithValue("@gpoName", gpoName);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new GpoSettingRecord
                    {
                        GpoName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Category = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        SettingName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        SettingValue = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        SettingState = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    });
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load settings for GPO '{gpoName}': {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// Loads the OU tree from GPO_OU_Links table to reconstruct OuNode hierarchy.
        /// Used when the TreeView needs to be populated without a fresh network scrape.
        /// </summary>
        public List<OuNode> LoadOuTreeFromDb()
        {
            // Build flat list of all OU rows
            var allNodes = new Dictionary<string, OuNode>(StringComparer.OrdinalIgnoreCase);
            var rootNodes = new List<OuNode>();

            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var cmd = new SqliteCommand(
                    "SELECT OU_Name, OU_DN, OU_Parent_DN, GPO_Name FROM GPO_OU_Links ORDER BY OU_DN",
                    connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string ouDn = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    string parentDn = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string gpoName = reader.IsDBNull(3) ? "" : reader.GetString(3);

                    if (!allNodes.TryGetValue(ouDn, out var node))
                    {
                        node = new OuNode
                        {
                            Name = reader.IsDBNull(0) ? ouDn : reader.GetString(0),
                            DistinguishedName = ouDn,
                            ParentDn = parentDn
                        };
                        allNodes[ouDn] = node;
                    }

                    if (!string.IsNullOrEmpty(gpoName) && !node.LinkedGpoNames.Contains(gpoName))
                        node.LinkedGpoNames.Add(gpoName);
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load OU tree from database: {ex.Message}");
                return rootNodes;
            }

            // Wire up parent/child relationships
            foreach (var node in allNodes.Values)
            {
                if (!string.IsNullOrEmpty(node.ParentDn) &&
                    allNodes.TryGetValue(node.ParentDn, out var parentNode))
                {
                    parentNode.Children.Add(node);
                }
                else
                {
                    rootNodes.Add(node);
                }
            }

            return rootNodes;
        }

        private void SaveSettingsToDb(List<GpoSettingRecord> settings, string scrapeTime)
        {
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var transaction = connection.BeginTransaction();

                // Truncate and reload (full snapshot replacement)
                using (var del = new SqliteCommand("DELETE FROM GPO_Settings", connection, transaction))
                    del.ExecuteNonQuery();

                string insertSql = @"INSERT INTO GPO_Settings
                    (GPO_Name, Category, Setting_Name, Setting_Value, Setting_State, Last_Scraped)
                    VALUES (@gpo, @cat, @name, @val, @state, @scraped)";

                foreach (var s in settings)
                {
                    using var cmd = new SqliteCommand(insertSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@gpo", s.GpoName ?? "");
                    cmd.Parameters.AddWithValue("@cat", s.Category ?? "");
                    cmd.Parameters.AddWithValue("@name", s.SettingName ?? "");
                    cmd.Parameters.AddWithValue("@val", s.SettingValue ?? "");
                    cmd.Parameters.AddWithValue("@state", s.SettingState ?? "");
                    cmd.Parameters.AddWithValue("@scraped", scrapeTime);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                _consoleForm?.WriteInfo($"Saved {settings.Count} GPO settings to database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save GPO settings: {ex.Message}");
                throw;
            }
        }

        private void SaveOuLinksToDb(List<OuNode> roots, string scrapeTime)
        {
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var transaction = connection.BeginTransaction();

                using (var del = new SqliteCommand("DELETE FROM GPO_OU_Links", connection, transaction))
                    del.ExecuteNonQuery();

                SaveOuNodeToDb(roots, connection, transaction, scrapeTime);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save OU link data: {ex.Message}");
            }
        }

        private void SaveOuNodeToDb(List<OuNode> nodes, SqliteConnection connection,
            SqliteTransaction transaction, string scrapeTime)
        {
            string insertSql = @"INSERT INTO GPO_OU_Links
                (OU_Name, OU_DN, OU_Parent_DN, GPO_Name, Last_Scraped)
                VALUES (@ouName, @ouDn, @parentDn, @gpoName, @scraped)";

            foreach (var node in nodes)
            {
                // If no GPOs linked, insert one row with empty GPO_Name so the OU appears in the tree
                var gpos = node.LinkedGpoNames.Count > 0
                    ? node.LinkedGpoNames
                    : new List<string> { "" };

                foreach (string gpoName in gpos)
                {
                    using var cmd = new SqliteCommand(insertSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@ouName", node.Name);
                    cmd.Parameters.AddWithValue("@ouDn", node.DistinguishedName);
                    cmd.Parameters.AddWithValue("@parentDn", node.ParentDn ?? "");
                    cmd.Parameters.AddWithValue("@gpoName", gpoName);
                    cmd.Parameters.AddWithValue("@scraped", scrapeTime);
                    cmd.ExecuteNonQuery();
                }

                // Recurse children
                SaveOuNodeToDb(node.Children, connection, transaction, scrapeTime);
            }
        }

        private void SaveChangelogToDb(List<GpoChangeRecord> changes)
        {
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var transaction = connection.BeginTransaction();

                string changeTime = DateTime.Now.ToString("o");
                string insertSql = @"INSERT INTO GPO_Changelog
                    (Change_Time, GPO_Name, Category, Setting_Name, Old_Value, New_Value, Change_Type)
                    VALUES (@time, @gpo, @cat, @name, @old, @new, @type)";

                foreach (var c in changes)
                {
                    using var cmd = new SqliteCommand(insertSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@time", changeTime);
                    cmd.Parameters.AddWithValue("@gpo", c.GpoName ?? "");
                    cmd.Parameters.AddWithValue("@cat", c.Category ?? "");
                    cmd.Parameters.AddWithValue("@name", c.SettingName ?? "");
                    cmd.Parameters.AddWithValue("@old", c.OldValue ?? "");
                    cmd.Parameters.AddWithValue("@new", c.NewValue ?? "");
                    cmd.Parameters.AddWithValue("@type", c.ChangeType ?? "");
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                _consoleForm?.WriteInfo($"Saved {changes.Count} change records to GPO_Changelog.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save GPO changelog: {ex.Message}");
            }
        }

        private void RecordScrapeHistory(int gposScraped, int settingsCount)
        {
            try
            {
                using var connection = new SqliteConnection(
                    $"Data Source={_databaseService.DatabasePath}");
                connection.Open();

                using var cmd = new SqliteCommand(@"INSERT INTO GPO_Scrape_History
                    (Scrape_Time, GPOs_Scraped, Settings_Count, Notes)
                    VALUES (@time, @gpos, @settings, @notes)", connection);

                cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("o"));
                cmd.Parameters.AddWithValue("@gpos", gposScraped);
                cmd.Parameters.AddWithValue("@settings", settingsCount);
                cmd.Parameters.AddWithValue("@notes", "");
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"Could not record scrape history: {ex.Message}");
            }
        }
    }
}
