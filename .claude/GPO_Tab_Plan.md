# GPO Tab Implementation Plan
**Created:** 2026-03-09
**Status:** NOT STARTED — Ready to implement
**Feature Branch:** `claude/general-session-QzHli` (or whatever the current session branch is)

---

## What The User Wants

1. **Scrape** the domain DC's GPOs and save all settings to SQLite database.
2. **Refresh button** — re-scrape, update DB, produce a change report (`<old value> → <new value>`).
3. **Duplicate detection** — find the same setting configured across multiple GPOs, regardless of which OU they're linked to. Flags them as candidates for consolidation.
4. **Tree view** (left pane) — AD hierarchy showing OU structure with GPOs linked to each OU, like GPMC.msc displays it.
5. **Settings pane** (right pane) — click a GPO in the tree → right pane shows ALL settings in that GPO, no fluff.

**RSAT is installed** on the machines running this tool, so `GroupPolicy` PowerShell module is available.

---

## Key Existing Code to Build On

### Windows_Tools.cs — Already Has These GPO Methods (NO GUI Yet)
Located in `#region GPO Operations` (line ~298):

| Method | What It Does |
|---|---|
| `GetAllGposAsync(domain)` | Returns `GpoListResult` with all GPOs: name, ID, status, dates |
| `GetGpoReportXmlAsync(gpoName, domain)` | **THE KEY METHOD** — runs `Get-GPOReport -Name X -ReportType Xml`, returns full XML string with every setting |
| `GetGpoLinksForTargetAsync(targetDn, domain)` | Gets GPO links on a specific OU distinguished name |
| `GetGpoLinkLocationsAsync(gpoName, domain)` | Finds all OUs where a specific GPO is linked |
| `SearchGpoSettingsAsync(searchTerm, domain)` | Searches all GPO XMLs for a keyword |

**Inner classes already defined in Windows_Tools.cs:**
- `GpoInfo` — DisplayName, Id, DomainName, Owner, GpoStatus, Description, CreationTime, ModificationTime
- `GpoListResult` — Success, ErrorMessage, List\<GpoInfo\>
- `GpoLink` — GpoName, GpoId, Target, Enabled, Enforced, Order
- `GpoLinkResult` — Success, ErrorMessage, List\<GpoLink\>
- `GpoSettingInfo` — GpoName, Category, SettingName, SettingValue, SettingState
- `GpoSettingSearchResult` — SearchTerm, Success, ErrorMessage, List\<GpoSettingInfo\>

### DatabaseService.cs — SQLite Already in Use
- `Microsoft.Data.Sqlite` (v8.0.0) — already a package reference
- Existing `GPO_Processing` table is too basic — we're adding NEW tables (see schema below)
- Pattern for schema migration: add new `CREATE TABLE IF NOT EXISTS` calls in `InitializeDatabase()`

### AD_Service.cs — OU Enumeration
- Uses `System.DirectoryServices` — available for walking the OU tree
- Already has `DirectoryEntry` patterns to adapt for OU traversal

---

## New Files to Create

### 1. `GPO_Service.cs`
The main service class. Handles:
- Scraping all GPOs (calls `Windows_Tools.GetGpoReportXmlAsync` for each GPO)
- Parsing XML into structured settings
- Building the OU tree with GPO links
- Saving to SQLite
- Diffing against last snapshot → generating change report
- Querying for duplicate settings

**Constructor:**
```csharp
public GPO_Service(ConsoleForm consoleForm, Windows_Tools windowsTools, DatabaseService databaseService)
```

**Key Methods to Implement:**
```csharp
// Full scrape: get all GPOs, their XML, parse settings, build OU tree, save to DB
public async Task<GpoScrapeResult> ScrapeAllGposAsync(string domain = null)

// Get OU tree with GPO links for the TreeView
public async Task<List<OuNode>> GetOuTreeAsync(string domain = null)

// Get settings for one GPO from DB (no network hit — just DB read)
public List<GpoSettingRecord> GetSettingsForGpo(string gpoName)

// Compare current scrape to previous — returns list of changes
public List<GpoChangeRecord> DetectChanges(List<GpoSettingRecord> previous, List<GpoSettingRecord> current)

// Find settings that appear in 2+ GPOs with the same value
public List<GpoDuplicateRecord> FindDuplicateSettings()

// Generate formatted text report of changes
public string BuildChangeReport(List<GpoChangeRecord> changes)

// Parse Get-GPOReport XML into flat list of settings
private List<GpoSettingRecord> ParseGpoXml(string gpoName, string xml)
```

**Inner Classes for GPO_Service:**
```csharp
public class OuNode
{
    public string Name { get; set; }
    public string DistinguishedName { get; set; }
    public List<string> LinkedGpoNames { get; set; } = new();
    public List<OuNode> Children { get; set; } = new();
}

public class GpoSettingRecord
{
    public string GpoName { get; set; }
    public string Category { get; set; }      // e.g. "Computer Configuration\Windows Settings\Security"
    public string SettingName { get; set; }
    public string SettingValue { get; set; }
    public string SettingState { get; set; }  // Enabled/Disabled/Not Configured
}

public class GpoChangeRecord
{
    public string GpoName { get; set; }
    public string Category { get; set; }
    public string SettingName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string ChangeType { get; set; }    // Added / Removed / Modified
}

public class GpoDuplicateRecord
{
    public string SettingName { get; set; }
    public string SettingValue { get; set; }
    public List<string> GpoNames { get; set; } = new();
    public string Recommendation { get; set; } // "Create one GPO, attach to parent OU"
}

public class GpoScrapeResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int GposScraped { get; set; }
    public int SettingsSaved { get; set; }
    public DateTime ScrapeTime { get; set; }
}
```

### 2. `GPO_Tab.cs`
Event handlers for the GPO tab. Follows the pattern of other partial classes or can be inline in `SA_ToolBelt.cs`.

Actually — given SA_ToolBelt.cs is already 6200+ lines, consider whether to add a new partial class file `SA_ToolBelt_GPO.cs` (partial class SAToolBelt) to keep GPO tab logic separate. **Recommended.**

---

## Database Schema — New Tables

Add to `InitializeDatabase()` in `DatabaseService.cs`:

```sql
-- Stores the actual GPO settings (current state)
CREATE TABLE IF NOT EXISTS GPO_Settings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GPO_Name TEXT NOT NULL,
    Category TEXT,
    Setting_Name TEXT NOT NULL,
    Setting_Value TEXT,
    Setting_State TEXT,
    Last_Scraped TEXT NOT NULL   -- ISO8601 datetime of last scrape
);

-- Stores OU → GPO link relationships for the tree view
CREATE TABLE IF NOT EXISTS GPO_OU_Links (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OU_Name TEXT NOT NULL,
    OU_DN TEXT NOT NULL,          -- Distinguished Name (full LDAP path)
    OU_Parent_DN TEXT,            -- Parent OU DN (for tree reconstruction)
    GPO_Name TEXT NOT NULL,
    GPO_GUID TEXT,
    Link_Enabled INTEGER,         -- 0 or 1
    Link_Enforced INTEGER,        -- 0 or 1
    Last_Scraped TEXT NOT NULL
);

-- Tracks each scrape run (for change detection)
CREATE TABLE IF NOT EXISTS GPO_Scrape_History (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Scrape_Time TEXT NOT NULL,
    GPOs_Scraped INTEGER,
    Settings_Count INTEGER,
    Notes TEXT
);

-- Change log: what changed between scrapes
CREATE TABLE IF NOT EXISTS GPO_Changelog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Change_Time TEXT NOT NULL,
    GPO_Name TEXT NOT NULL,
    Category TEXT,
    Setting_Name TEXT NOT NULL,
    Old_Value TEXT,
    New_Value TEXT,
    Change_Type TEXT NOT NULL     -- 'Added', 'Removed', 'Modified'
);
```

**Migration note:** The existing `GPO_Processing` table stays as-is (don't break existing code). These are NEW tables alongside it.

---

## UI Layout — `SA_ToolBelt.Designer.cs`

**New tab:** `tabGPO` — add to Designer alongside other tabs.

**Layout inside tabGPO:**

```
+-------------------------------------------------------------------+
| [Scrape GPOs]  [Refresh & Report]  [Find Duplicates]  Status: _  |
+----------------------------+--------------------------------------+
|  TreeView (tvwGpoTree)     |  ListView (lvwGpoSettings)           |
|                            |                                      |
|  spectre.my.domain.com     |  Category    | Setting Name | Value  |
|  ├─ Domain Root            |  ──────────────────────────────────  |
|  │   GPO: Default Domain   |  Comp Config | Firewall     | On     |
|  ├─ OU: Site 1             |  Comp Config | Password Len | 12     |
|  │   GPO: Site1-Policy     |  ...                                 |
|  │   GPO: Firewall-Deny    |                                      |
|  └─ OU: Site 2             |                                      |
|      GPO: Site2-Policy     |                                      |
|                            |                                      |
+----------------------------+--------------------------------------+
|  [Change Report pane - RichTextBox, collapsible/tabbed]           |
+-------------------------------------------------------------------+
```

**Control naming:**
- `tabGPO` — the TabPage
- `btnGpoScrape` — "Scrape GPOs" button (initial full scrape)
- `btnGpoRefresh` — "Refresh & Report" button
- `btnGpoDuplicates` — "Find Duplicates" button
- `lblGpoStatus` — status label (e.g., "Last scraped: 2026-03-09 10:45")
- `tvwGpoTree` — TreeView (left pane)
- `lvwGpoSettings` — ListView with columns: Category, Setting Name, Value, State (right pane)
- `rtbGpoReport` — RichTextBox (bottom pane) for change report / duplicate report output
- `splGpoHorizontal` — SplitContainer (left/right split)
- `splGpoVertical` — SplitContainer (top/bottom split for report pane)

**TreeView node structure:**
- Root node: domain name (e.g., `spectre.my.domain.com`)
- Child nodes: OU names
- Under each OU: GPO names (as child nodes, visually distinct — italic or different color)
- GPO nodes store the GPO name in `.Tag` property for click handler

---

## Wiring Into SA_ToolBelt.cs

### 1. Add `_gpoService` field (around line 28–36 where other services are declared):
```csharp
private GPO_Service _gpoService;
```

### 2. Instantiate in constructor (after `_windowsTools` is created, around line 72):
```csharp
_gpoService = new GPO_Service(_consoleForm, _windowsTools, _databaseService);
```

### 3. Add `tabGPO` to `ShowAllTabs()` (around line 292–304):
```csharp
tabControlMain.TabPages.Add(tabGPO);
```
Insert it in a logical order — after `tabWindowsTools`, before `tabOnlineOffline` suggested.

### 4. Register event handlers (in `InitializeComponent` region or Form constructor):
```csharp
btnGpoScrape.Click += BtnGpoScrape_Click;
btnGpoRefresh.Click += BtnGpoRefresh_Click;
btnGpoDuplicates.Click += BtnGpoDuplicates_Click;
tvwGpoTree.AfterSelect += TvwGpoTree_AfterSelect;
```

---

## Implementation Order (Step by Step)

### Phase 1 — Database Schema
1. Edit `DatabaseService.cs` → add 4 new tables to `InitializeDatabase()`
2. No new methods needed in DatabaseService — GPO_Service will use raw SQLite connections via `DatabaseService.DatabasePath`

### Phase 2 — GPO_Service.cs (core logic)
1. Create `GPO_Service.cs`
2. Implement `ScrapeAllGposAsync()`:
   - Call `_windowsTools.GetAllGposAsync()` to get list of all GPOs
   - For each GPO, call `_windowsTools.GetGpoReportXmlAsync(gpoName)` to get full XML
   - Call `ParseGpoXml()` to extract settings
   - Call `_windowsTools.GetGpoLinkLocationsAsync(gpoName)` to find where it's linked
   - Save all to `GPO_Settings` and `GPO_OU_Links` tables (truncate-and-reload pattern)
   - Record scrape in `GPO_Scrape_History`
3. Implement `ParseGpoXml()`:
   - The XML from Get-GPOReport has a known schema (see note below)
   - Use `System.Xml.Linq.XDocument` to parse
   - Walk `Computer.ExtensionData` and `User.ExtensionData` nodes
4. Implement `GetOuTreeAsync()`:
   - Use `System.DirectoryServices.DirectoryEntry` to walk OU tree
   - Read `gPLink` attribute from each OU (contains GPO GUIDs as string)
   - Cross-reference GUID to GPO names from `GetAllGposAsync()` or the DB
   - Return recursive `OuNode` list
5. Implement `GetSettingsForGpo()` — simple SQLite read
6. Implement `DetectChanges()` — diff old `GPO_Settings` against new (snapshot before overwrite, or use scrape history ID)
7. Implement `FindDuplicateSettings()` — SQL GROUP BY Setting_Name, Setting_Value HAVING COUNT(DISTINCT GPO_Name) > 1
8. Implement `BuildChangeReport()` — format changes as readable text

### Phase 3 — UI (Designer + SA_ToolBelt_GPO.cs)
1. Add `tabGPO` and all controls to `SA_ToolBelt.Designer.cs`
2. Create `SA_ToolBelt_GPO.cs` as a new partial class file
3. Implement `BtnGpoScrape_Click` — runs `ScrapeAllGposAsync()`, then `PopulateGpoTree()`
4. Implement `BtnGpoRefresh_Click` — snapshot old data, re-scrape, detect changes, display report in `rtbGpoReport`
5. Implement `BtnGpoDuplicates_Click` — call `FindDuplicateSettings()`, display in `rtbGpoReport`
6. Implement `TvwGpoTree_AfterSelect` — when a GPO node is clicked, call `GetSettingsForGpo()`, populate `lvwGpoSettings`
7. Implement `PopulateGpoTree()` — call `GetOuTreeAsync()`, build TreeView nodes recursively

### Phase 4 — Wiring & Integration
1. Add `tabGPO` to `ShowAllTabs()` in `SA_ToolBelt.cs`
2. Add `_gpoService` field and instantiation
3. Test compile (user builds on Windows)

---

## XML Parsing Notes (Get-GPOReport Schema)

The XML from `Get-GPOReport -ReportType Xml` has this structure:
```xml
<GPO xmlns:xsd="..." xmlns:xsi="..." ...>
  <Name>GPO Display Name</Name>
  <Computer>
    <ExtensionData>
      <Extension xsi:type="q1:SecuritySettings" ...>
        <q1:Account>
          <q1:Name>MinimumPasswordLength</q1:Name>
          <q1:SettingNumber>12</q1:SettingNumber>
        </q1:Account>
        <q1:SystemAccessPolicies>...</q1:SystemAccessPolicies>
      </Extension>
      <Extension xsi:type="q2:RegistrySettings" ...>
        <q2:Policy>
          <q2:Name>Firewall: Block all incoming connections</q2:Name>
          <q2:State>Enabled</q2:State>
          <q2:Explain>...</q2:Explain>
        </q2:Policy>
      </Extension>
    </ExtensionData>
  </Computer>
  <User>
    <ExtensionData>...</ExtensionData>
  </User>
</GPO>
```

The XML uses multiple namespaces per extension type. The practical approach:
- Use `XDocument.Parse(xml)` with `XNamespace` handling
- Extract all `Extension` nodes from both `Computer` and `User` sections
- For each Extension, extract the `xsi:type` as the Category
- Walk child elements extracting Name/Value/State pairs
- The namespaces vary by extension type — use `.LocalName` comparisons to avoid namespace issues

**Alternative simpler approach:** Use regex or `XElement.DescendantsAndSelf()` with `.LocalName` matching for known elements like `Name`, `State`, `Value`, `SettingNumber` — less brittle than namespace-aware xpath.

---

## Change Detection Strategy

**On Refresh:**
1. Before scraping, read current `GPO_Settings` into memory as `previousSettings`
2. Run full scrape → stores new data into `GPO_Settings` (truncate-and-reload)
3. Diff `previousSettings` vs `newSettings`:
   - Key = `(GPO_Name, Category, Setting_Name)`
   - If key exists in old but not new → `ChangeType = "Removed"`
   - If key exists in new but not old → `ChangeType = "Added"`
   - If key exists in both but value differs → `ChangeType = "Modified"`, record OldValue/NewValue
4. Insert all changes into `GPO_Changelog`
5. Call `BuildChangeReport()` → display in `rtbGpoReport`

---

## Duplicate Detection Query

```sql
SELECT
    s1.Setting_Name,
    s1.Setting_Value,
    GROUP_CONCAT(DISTINCT s1.GPO_Name) as GPO_Names,
    COUNT(DISTINCT s1.GPO_Name) as GPO_Count
FROM GPO_Settings s1
WHERE s1.Setting_Value IS NOT NULL AND s1.Setting_Value != ''
GROUP BY s1.Setting_Name, s1.Setting_Value
HAVING COUNT(DISTINCT s1.GPO_Name) > 1
ORDER BY GPO_Count DESC, s1.Setting_Name;
```

Report output format:
```
DUPLICATE SETTINGS REPORT - 2026-03-09 10:45
============================================

[SETTING] Firewall: Block Inbound
[VALUE]   Enabled
[FOUND IN] GPO-Site1-Firewall, GPO-Site2-Firewall
[NOTE] Consider consolidating into one GPO linked to a parent OU.

[SETTING] Minimum Password Length
[VALUE]   12
[FOUND IN] Default Domain Policy, Site1-Security
[NOTE] Consider consolidating into one GPO linked to a parent OU.
```

---

## Change Report Format

```
GPO CHANGE REPORT - 2026-03-09 10:47 (compared to: 2026-03-08 09:15)
====================================================================

MODIFIED:
  GPO: Default Domain Policy
  Category: Security Settings\Account Policies
  Setting: Maximum Password Age
  OLD: 90 days  →  NEW: 60 days

ADDED (new setting):
  GPO: Site1-Firewall
  Category: Computer Configuration\Windows Settings\Security\Windows Firewall
  Setting: Outbound Action (Domain Profile)
  VALUE: Block

REMOVED:
  GPO: Legacy-Deprecated-Policy
  [Entire GPO removed from domain]
```

---

## Gotchas & Watchouts

1. **`Get-GPOReport` for ALL GPOs** — can call with `-All` flag instead of per-GPO: `Get-GPOReport -All -ReportType Xml`. Returns XML for all GPOs at once. Faster but one huge XML blob. Alternatively loop per-GPO for progress feedback. **Recommend looping** so console shows progress.

2. **OU gPLink attribute format** — the `gPLink` attribute on a DirectoryEntry looks like:
   `[LDAP://cn={GUID1},cn=policies,...;0][LDAP://cn={GUID2},cn=policies,...;0]`
   Need to parse GUIDs out. The `;0` = enabled, `;1` = disabled, `;2` = enforced.

3. **Scrape time** — Could be slow if domain has many GPOs. Run on background thread (`Task.Run`), disable buttons during scrape, update status label.

4. **Air-gapped** — No internet. All PowerShell runs via in-process PowerShell SDK (already the pattern in Windows_Tools.cs). No NuGet install needed — all packages already referenced.

5. **SA_ToolBelt.Designer.cs is 5200+ lines** — When adding the tab, use Grep to find where `tabWindowsTools` is defined, insert `tabGPO` nearby. Do NOT read the whole file.

6. **partial class pattern** — `SA_ToolBelt_GPO.cs` should declare `public partial class SAToolBelt : Form` — same namespace, no `InitializeComponent` call.

7. **TreeView GPO nodes** — Distinguish OU nodes from GPO nodes visually. Use `TreeNode.ForeColor = Color.SteelBlue` for GPO nodes, store GPO name in `TreeNode.Tag`. Only GPO nodes trigger the settings panel.

8. **Thread safety** — UI updates from async methods must use `Invoke()` or `BeginInvoke()`. Follow existing pattern in SA_ToolBelt.cs.

---

## Files to Create/Modify Summary

| Action | File | What |
|---|---|---|
| CREATE | `GPO_Service.cs` | Core scrape/parse/diff/duplicate logic |
| CREATE | `SA_ToolBelt_GPO.cs` | Partial class with GPO tab event handlers |
| MODIFY | `SA_ToolBelt.Designer.cs` | Add `tabGPO` and all child controls |
| MODIFY | `SA_ToolBelt.cs` | Add `_gpoService`, wire `ShowAllTabs()`, constructor instantiation |
| MODIFY | `DatabaseService.cs` | Add 4 new GPO tables to `InitializeDatabase()` |

**No new NuGet packages needed.** Everything required is already in the .csproj.

---

## Quick Context Re-Entry Checklist (for future Claude)

If you're picking this up mid-session:
- [ ] Read `.claude/context_notes.json` — comprehensive project context
- [ ] Read `.claude/session_context.json` — user background & working style notes
- [ ] Read `.claude/GPO_Tab_Plan.md` (this file)
- [ ] Grep `Windows_Tools.cs` for `#region GPO Operations` — existing backend methods
- [ ] Grep `SA_ToolBelt.cs` for `ShowAllTabs` — where to add `tabGPO`
- [ ] Grep `DatabaseService.cs` for `CREATE TABLE` — where to add new tables
- [ ] Check git branch — should be `claude/general-session-QzHli` or current session branch
- [ ] DO NOT read SA_ToolBelt.cs or SA_ToolBelt.Designer.cs in full — they are 6200+ and 5200+ lines

**User facts:** Sharp SA, air-gapped classified domain (SPECTRE), RSAT installed, builds on Windows (you're on Linux — can't build). Calls Claude "Entity" sometimes. Uses ROFL a lot. Values straight talk. Will lovingly roast you if you screw something up.
