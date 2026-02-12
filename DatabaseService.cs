using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;

namespace SA_ToolBelt
{
    /// <summary>
    /// Manages all SQLite database operations for the SA Toolbelt.
    /// Handles creation, reading, writing, and migration of the configuration database.
    /// </summary>
    public class DatabaseService : IDisposable
    {
        private readonly ConsoleForm _consoleForm;
        private string _databasePath;

        private const string DB_FILENAME = "SA_Toolbelt.db";
        private const string REGISTRY_KEY_PATH = @"SOFTWARE\SA_Toolbelt";
        private const string REGISTRY_VALUE_NAME = "SqlPath";

        // Application-level lock file constants
        private const string LOCK_FILE_EXTENSION = ".applock";
        private const int HEARTBEAT_INTERVAL_MS = 60_000;       // Update heartbeat every 60 seconds
        private const int STALE_LOCK_THRESHOLD_MS = 180_000;    // 3 minutes without heartbeat = stale

        // Lock state
        private string _lockFilePath;
        private Timer _heartbeatTimer;
        private bool _lockAcquired;
        private DateTime _lockAcquiredTimeUtc;
        private bool _disposed;

        public string DatabasePath => _databasePath;
        public bool IsLockHeld => _lockAcquired;

        public DatabaseService(ConsoleForm consoleForm)
        {
            _consoleForm = consoleForm;
            _databasePath = ResolveDatabasePath();
            _lockFilePath = _databasePath + LOCK_FILE_EXTENSION;
        }

        #region Database Path Resolution

        /// <summary>
        /// Resolves the database path. Checks the registry first, then falls back to Documents.
        /// </summary>
        private string ResolveDatabasePath()
        {
            // First check the registry for a configured path
            string registryPath = GetSqlPathFromRegistry();
            if (!string.IsNullOrEmpty(registryPath))
            {
                string dbFile = Path.Combine(registryPath, DB_FILENAME);
                if (File.Exists(dbFile))
                {
                    _consoleForm?.WriteInfo($"Database found at registry path: {dbFile}");
                    return dbFile;
                }
                else
                {
                    _consoleForm?.WriteWarning($"Registry points to '{registryPath}' but database not found there. Falling back to Documents.");
                }
            }

            // Fall back to Documents\SA_Toolbelt
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string defaultFolder = Path.Combine(documentsPath, "SA_Toolbelt");
            return Path.Combine(defaultFolder, DB_FILENAME);
        }

        /// <summary>
        /// Reads the Sql_Path value from the Windows registry.
        /// </summary>
        public static string GetSqlPathFromRegistry()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH))
                {
                    return key?.GetValue(REGISTRY_VALUE_NAME) as string;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the Sql_Path value to the Windows registry under HKEY_CURRENT_USER.
        /// HKCU does not require admin privileges.
        /// </summary>
        public static void SetSqlPathInRegistry(string sqlPath)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
            {
                key.SetValue(REGISTRY_VALUE_NAME, sqlPath, RegistryValueKind.String);
            }
        }

        #endregion

        #region Database Initialization

        /// <summary>
        /// Ensures the database exists and all tables are created.
        /// Creates the database in Documents\SA_Toolbelt initially.
        /// </summary>
        public void InitializeDatabase()
        {
            try
            {
                string directory = Path.GetDirectoryName(_databasePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _consoleForm?.WriteInfo($"Created database directory: {directory}");
                }

                using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();
                    CreateTables(connection);
                }

                _consoleForm?.WriteSuccess($"Database initialized at: {_databasePath}");

                // Acquire the lock now that the database exists
                _lockFilePath = _databasePath + LOCK_FILE_EXTENSION;
                if (!AcquireLock())
                {
                    _consoleForm?.WriteWarning("Database initialized but could not acquire lock. Another process may be accessing it.");
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to initialize database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates all 5 tables if they do not already exist.
        /// </summary>
        private void CreateTables(SqliteConnection connection)
        {
            string[] createStatements = new string[]
            {
                @"CREATE TABLE IF NOT EXISTS Toolbelt_Config (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VCenter_Server TEXT NOT NULL DEFAULT '',
                    PowerCLI_Location TEXT NOT NULL DEFAULT '',
                    Sql_Path TEXT NOT NULL DEFAULT '',
                    Excluded_OU TEXT NOT NULL DEFAULT '',
                    Disabled_Users_Ou TEXT NOT NULL DEFAULT '',
                    HomeDirectory TEXT NOT NULL DEFAULT '',
                    Linux_Ds TEXT NOT NULL DEFAULT ''
                )",
                @"CREATE TABLE IF NOT EXISTS ComputerList (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Computername TEXT NOT NULL DEFAULT '',
                    Type TEXT NOT NULL DEFAULT '',
                    VMWare TEXT NOT NULL DEFAULT '',
                    Instructions TEXT NOT NULL DEFAULT ''
                )",
                @"CREATE TABLE IF NOT EXISTS LogConfiguration (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Server TEXT NOT NULL DEFAULT '',
                    server_instance TEXT NOT NULL DEFAULT ''
                )",
                @"CREATE TABLE IF NOT EXISTS ouConfiguration (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ou TEXT NOT NULL DEFAULT '',
                    MiddleName TEXT NOT NULL DEFAULT '',
                    keyword TEXT NOT NULL DEFAULT ''
                )",
                @"CREATE TABLE IF NOT EXISTS GPO_Processing (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AD_Location TEXT NOT NULL DEFAULT '',
                    GPO_Name TEXT NOT NULL DEFAULT '',
                    Policy_Location TEXT NOT NULL DEFAULT '',
                    Policy_Name TEXT NOT NULL DEFAULT '',
                    Setting TEXT NOT NULL DEFAULT '',
                    Value TEXT NOT NULL DEFAULT ''
                )"
            };

            foreach (string sql in createStatements)
            {
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Migration: Add Linux_Ds column to existing Toolbelt_Config tables
            MigrateToolbeltConfigLinuxDs(connection);

            _consoleForm?.WriteInfo("All database tables verified/created.");
        }

        /// <summary>
        /// Adds the Linux_Ds column to Toolbelt_Config if it doesn't already exist.
        /// Handles migration for databases created before this column was added.
        /// </summary>
        private void MigrateToolbeltConfigLinuxDs(SqliteConnection connection)
        {
            try
            {
                using (var cmd = new SqliteCommand("SELECT Linux_Ds FROM Toolbelt_Config LIMIT 1", connection))
                {
                    cmd.ExecuteScalar();
                }
            }
            catch (SqliteException)
            {
                using (var cmd = new SqliteCommand("ALTER TABLE Toolbelt_Config ADD COLUMN Linux_Ds TEXT NOT NULL DEFAULT ''", connection))
                {
                    cmd.ExecuteNonQuery();
                }
                _consoleForm?.WriteInfo("Migrated Toolbelt_Config: added Linux_Ds column.");
            }
        }

        #endregion

        #region Database Connection Helper

        private SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            // Enable WAL mode - allows concurrent reads while writing
            // and prevents most "database is locked" errors
            using (var walCmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection))
            {
                walCmd.ExecuteNonQuery();
            }

            // Wait up to 10 seconds if the DB is locked by another operation
            // instead of immediately throwing an error
            using (var timeoutCmd = new SqliteCommand("PRAGMA busy_timeout=10000;", connection))
            {
                timeoutCmd.ExecuteNonQuery();
            }

            return connection;
        }

        #endregion

        #region Database Lock Detection & Recovery

        /// <summary>
        /// Internal representation of the .applock file contents.
        /// </summary>
        private class LockFileInfo
        {
            public string MachineName { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public int ProcessId { get; set; }
            public DateTime AcquiredUtc { get; set; }
            public DateTime HeartbeatUtc { get; set; }
        }

        /// <summary>
        /// Reads and parses the .applock file. Returns null if it doesn't exist or can't be read.
        /// Uses FileShare.ReadWrite so it can read even while the heartbeat timer is writing.
        /// </summary>
        private LockFileInfo ReadLockFile()
        {
            if (string.IsNullOrEmpty(_lockFilePath) || !File.Exists(_lockFilePath))
                return null;

            try
            {
                string content;
                using (var fs = new FileStream(_lockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    content = reader.ReadToEnd();
                }

                var info = new LockFileInfo();
                foreach (var line in content.Split('\n'))
                {
                    var trimmed = line.Trim();
                    var eqIndex = trimmed.IndexOf('=');
                    if (eqIndex < 0) continue;

                    var key = trimmed.Substring(0, eqIndex);
                    var value = trimmed.Substring(eqIndex + 1);

                    switch (key)
                    {
                        case "MACHINE": info.MachineName = value; break;
                        case "USER": info.UserName = value; break;
                        case "PID": int.TryParse(value, out int pid); info.ProcessId = pid; break;
                        case "ACQUIRED": DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime acq); info.AcquiredUtc = acq; break;
                        case "HEARTBEAT": DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime hb); info.HeartbeatUtc = hb; break;
                    }
                }
                return info;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Writes the .applock file with current process info.
        /// When createNew is true, uses FileMode.CreateNew for atomic creation (fails if file exists).
        /// When false, overwrites the existing file (for heartbeat updates).
        /// </summary>
        private void WriteLockFile(bool createNew)
        {
            var now = DateTime.UtcNow;
            if (createNew)
                _lockAcquiredTimeUtc = now;

            var content = $"MACHINE={Environment.MachineName}\n" +
                          $"USER={Environment.UserName}\n" +
                          $"PID={Environment.ProcessId}\n" +
                          $"ACQUIRED={_lockAcquiredTimeUtc:O}\n" +
                          $"HEARTBEAT={now:O}";

            var mode = createNew ? FileMode.CreateNew : FileMode.Create;
            using (var fs = new FileStream(_lockFilePath, mode, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fs))
            {
                writer.Write(content);
            }
        }

        /// <summary>
        /// Determines if a lock is stale (owner is no longer running).
        /// Same machine: checks if the owning PID still exists.
        /// Different machine: checks if the heartbeat is older than the stale threshold (3 minutes).
        /// </summary>
        private bool IsLockStale(LockFileInfo lockInfo)
        {
            if (lockInfo == null) return true;

            // Same machine — we can directly check if the process is still alive
            if (string.Equals(lockInfo.MachineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Process.GetProcessById(lockInfo.ProcessId);
                    return false; // Process exists — lock is active
                }
                catch (ArgumentException)
                {
                    return true; // Process not found — lock is stale
                }
            }

            // Different machine — fall back to heartbeat age
            var heartbeatAge = DateTime.UtcNow - lockInfo.HeartbeatUtc;
            return heartbeatAge.TotalMilliseconds > STALE_LOCK_THRESHOLD_MS;
        }

        /// <summary>
        /// Called by the heartbeat timer every 60 seconds.
        /// Updates the heartbeat timestamp in the lock file so other instances know we're still alive.
        /// If the lock file was deleted (e.g., by a force-unlock), marks our lock as lost.
        /// </summary>
        private void RefreshHeartbeat(object state)
        {
            if (!_lockAcquired || string.IsNullOrEmpty(_lockFilePath)) return;

            try
            {
                if (!File.Exists(_lockFilePath))
                {
                    _lockAcquired = false;
                    _consoleForm?.WriteError("Lock file was removed by another process. Database write access has been lost.");
                    return;
                }

                WriteLockFile(createNew: false);
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteWarning($"Failed to refresh lock heartbeat: {ex.Message}");
            }
        }

        /// <summary>
        /// Acquires the application-level lock on the database.
        /// Creates a .applock file alongside the database with our process info and starts a heartbeat timer.
        /// Returns true if the lock was acquired, false if another active process holds it.
        /// </summary>
        public bool AcquireLock()
        {
            _lockFilePath = _databasePath + LOCK_FILE_EXTENSION;

            // If we already hold the lock, just refresh it
            if (_lockAcquired)
            {
                try { WriteLockFile(createNew: false); } catch { }
                return true;
            }

            // Check for an existing lock file
            var existingLock = ReadLockFile();
            if (existingLock != null)
            {
                if (!IsLockStale(existingLock))
                {
                    _consoleForm?.WriteWarning(
                        $"Database is locked by {existingLock.UserName} on {existingLock.MachineName} " +
                        $"(PID: {existingLock.ProcessId}, since {existingLock.AcquiredUtc:g} UTC).");
                    return false;
                }

                // Stale lock — clean it up
                _consoleForm?.WriteWarning(
                    $"Cleaning up stale lock from {existingLock.UserName} on {existingLock.MachineName} " +
                    $"(last heartbeat: {existingLock.HeartbeatUtc:g} UTC).");
                try { File.Delete(_lockFilePath); }
                catch (Exception ex)
                {
                    _consoleForm?.WriteError($"Could not remove stale lock file: {ex.Message}");
                    return false;
                }
            }

            // Create the lock file atomically
            try
            {
                WriteLockFile(createNew: true);
                _lockAcquired = true;

                // Start heartbeat timer — updates the lock file every 60 seconds
                _heartbeatTimer = new Timer(
                    RefreshHeartbeat,
                    null,
                    HEARTBEAT_INTERVAL_MS,
                    HEARTBEAT_INTERVAL_MS);

                _consoleForm?.WriteSuccess(
                    $"Database lock acquired by {Environment.UserName} on {Environment.MachineName} (PID: {Environment.ProcessId}).");
                return true;
            }
            catch (IOException)
            {
                // Another process created the file between our check and our create — race condition handled
                _consoleForm?.WriteWarning("Could not acquire database lock — another process grabbed it first.");
                return false;
            }
        }

        /// <summary>
        /// Releases the application-level lock. Stops the heartbeat timer and deletes the .applock file.
        /// </summary>
        public void ReleaseLock()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;

            if (_lockAcquired && !string.IsNullOrEmpty(_lockFilePath))
            {
                try
                {
                    if (File.Exists(_lockFilePath))
                    {
                        File.Delete(_lockFilePath);
                        _consoleForm?.WriteInfo("Database lock released.");
                    }
                }
                catch (Exception ex)
                {
                    _consoleForm?.WriteWarning($"Could not release database lock file: {ex.Message}");
                }
            }
            _lockAcquired = false;
        }

        /// <summary>
        /// Throws InvalidOperationException if the lock is not currently held.
        /// Called before every write operation to prevent concurrent database modifications.
        /// </summary>
        private void EnsureLockHeld()
        {
            if (!_lockAcquired)
            {
                var existingLock = ReadLockFile();
                string owner = existingLock != null
                    ? $"{existingLock.UserName} on {existingLock.MachineName} (PID: {existingLock.ProcessId})"
                    : "unknown";
                throw new InvalidOperationException(
                    $"Cannot write to database: lock not held. Current lock owner: {owner}. " +
                    "Another SA may have the toolbelt open, or the lock was lost.");
            }
        }

        /// <summary>
        /// Checks if the database is currently locked by another process using the .applock file.
        /// Returns true if the database is locked by someone else, false if it's available.
        /// </summary>
        public bool IsDatabaseLocked()
        {
            try
            {
                if (!File.Exists(_databasePath))
                    return false;

                // If we hold the lock, the DB is not "locked" from our perspective
                if (_lockAcquired)
                    return false;

                var lockInfo = ReadLockFile();
                if (lockInfo == null)
                    return false; // No lock file — database is available

                if (IsLockStale(lockInfo))
                {
                    _consoleForm?.WriteWarning(
                        $"Found stale lock from {lockInfo.UserName} on {lockInfo.MachineName}. " +
                        "It will be cleaned up automatically.");
                    return false; // Stale lock doesn't count
                }

                _consoleForm?.WriteWarning(
                    $"Database is locked by {lockInfo.UserName} on {lockInfo.MachineName} " +
                    $"(PID: {lockInfo.ProcessId}, since {lockInfo.AcquiredUtc:g} UTC).");
                return true;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Error checking database lock status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Force-unlocks the database by removing the .applock file and cleaning up SQLite journal files.
        /// Also verifies database integrity after cleanup.
        /// WARNING: The other SA's toolbelt session will lose write access.
        /// </summary>
        public bool ForceUnlockDatabase()
        {
            try
            {
                // Remove the application lock file
                if (File.Exists(_lockFilePath))
                {
                    var lockInfo = ReadLockFile();
                    File.Delete(_lockFilePath);
                    if (lockInfo != null)
                    {
                        _consoleForm?.WriteWarning(
                            $"Removed lock held by {lockInfo.UserName} on {lockInfo.MachineName} (PID: {lockInfo.ProcessId}).");
                    }
                    else
                    {
                        _consoleForm?.WriteInfo("Removed lock file.");
                    }
                }

                // Also clean up SQLite journal files in case they're stale
                string walFile = _databasePath + "-wal";
                string shmFile = _databasePath + "-shm";
                string journalFile = _databasePath + "-journal";

                if (File.Exists(walFile))
                {
                    File.Delete(walFile);
                    _consoleForm?.WriteInfo($"Deleted WAL file: {walFile}");
                }
                if (File.Exists(shmFile))
                {
                    File.Delete(shmFile);
                    _consoleForm?.WriteInfo($"Deleted SHM file: {shmFile}");
                }
                if (File.Exists(journalFile))
                {
                    File.Delete(journalFile);
                    _consoleForm?.WriteInfo($"Deleted journal file: {journalFile}");
                }

                // Verify the database is now accessible
                using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
                {
                    connection.Open();

                    using (var cmd = new SqliteCommand("PRAGMA integrity_check;", connection))
                    {
                        string result = cmd.ExecuteScalar()?.ToString();
                        if (result == "ok")
                        {
                            _consoleForm?.WriteSuccess("Database force-unlocked successfully. Integrity check passed.");

                            using (var walCmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection))
                            {
                                walCmd.ExecuteNonQuery();
                            }
                            return true;
                        }
                        else
                        {
                            _consoleForm?.WriteError($"Database integrity check failed after unlock: {result}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to force-unlock database: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the database is locked, prompts the user to force-unlock if needed,
        /// and acquires the lock for this session on success.
        /// Returns true if the database is accessible and the lock is held.
        /// Returns false if the database is still locked (user declined or unlock/acquire failed).
        /// </summary>
        public bool CheckAndHandleLock()
        {
            // If we already hold the lock, we're good
            if (_lockAcquired)
                return true;

            // Try to acquire the lock directly — handles stale lock cleanup automatically
            if (AcquireLock())
                return true;

            // Lock is held by another active process — read the info for the dialog
            var lockInfo = ReadLockFile();
            string ownerInfo = lockInfo != null
                ? $"{lockInfo.UserName} on {lockInfo.MachineName} (PID: {lockInfo.ProcessId})\nLocked since: {lockInfo.AcquiredUtc.ToLocalTime():g}\nLast heartbeat: {lockInfo.HeartbeatUtc.ToLocalTime():g}"
                : "Unknown process";

            _consoleForm?.WriteWarning($"Database lock detected. Owner: {ownerInfo}");

            var result = System.Windows.Forms.MessageBox.Show(
                $"The database is currently locked by another SA:\n\n{ownerInfo}\n\n" +
                "This means another SA has the toolbelt open and is actively using this database.\n\n" +
                "Would you like to force-unlock the database?\n\n" +
                "WARNING: The other SA's toolbelt will lose write access and may behave unexpectedly.",
                "Database Locked",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                bool unlocked = ForceUnlockDatabase();
                if (unlocked)
                {
                    // Now try to acquire the lock for ourselves
                    if (AcquireLock())
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "Database unlocked and lock acquired successfully. Integrity check passed.",
                            "Unlock Successful",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(
                            "Database was unlocked but could not acquire the lock.\n" +
                            "Another process may have grabbed it. Please try again.",
                            "Lock Acquisition Failed",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning);
                        return false;
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Failed to unlock the database. Please check the console for details.\n\n" +
                        "You may need to have the other SA close their toolbelt and try again.",
                        "Unlock Failed",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }

            _consoleForm?.WriteInfo("User declined to force-unlock the database.");
            return false;
        }

        #endregion

        #region Toolbelt_Config CRUD

        /// <summary>
        /// Saves or updates the toolbelt configuration (single-row table).
        /// </summary>
        public void SaveToolbeltConfig(string vCenterServer, string powerCliLocation, string sqlPath,
            string excludedOu, string disabledUsersOu, string homeDirectory, string linuxDs)
        {
            try
            {
                EnsureLockHeld();

                using (var connection = GetConnection())
                {
                    // Check if a config row already exists
                    using (var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Toolbelt_Config", connection))
                    {
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            // Update existing row
                            string updateSql = @"UPDATE Toolbelt_Config SET
                                VCenter_Server = @vCenter,
                                PowerCLI_Location = @powerCli,
                                Sql_Path = @sqlPath,
                                Excluded_OU = @excludedOu,
                                Disabled_Users_Ou = @disabledUsers,
                                HomeDirectory = @homeDir,
                                Linux_Ds = @linuxDs
                                WHERE Id = (SELECT MIN(Id) FROM Toolbelt_Config)";

                            using (var cmd = new SqliteCommand(updateSql, connection))
                            {
                                cmd.Parameters.AddWithValue("@vCenter", vCenterServer ?? "");
                                cmd.Parameters.AddWithValue("@powerCli", powerCliLocation ?? "");
                                cmd.Parameters.AddWithValue("@sqlPath", sqlPath ?? "");
                                cmd.Parameters.AddWithValue("@excludedOu", excludedOu ?? "");
                                cmd.Parameters.AddWithValue("@disabledUsers", disabledUsersOu ?? "");
                                cmd.Parameters.AddWithValue("@homeDir", homeDirectory ?? "");
                                cmd.Parameters.AddWithValue("@linuxDs", linuxDs ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new row
                            string insertSql = @"INSERT INTO Toolbelt_Config
                                (VCenter_Server, PowerCLI_Location, Sql_Path, Excluded_OU, Disabled_Users_Ou, HomeDirectory, Linux_Ds)
                                VALUES (@vCenter, @powerCli, @sqlPath, @excludedOu, @disabledUsers, @homeDir, @linuxDs)";

                            using (var cmd = new SqliteCommand(insertSql, connection))
                            {
                                cmd.Parameters.AddWithValue("@vCenter", vCenterServer ?? "");
                                cmd.Parameters.AddWithValue("@powerCli", powerCliLocation ?? "");
                                cmd.Parameters.AddWithValue("@sqlPath", sqlPath ?? "");
                                cmd.Parameters.AddWithValue("@excludedOu", excludedOu ?? "");
                                cmd.Parameters.AddWithValue("@disabledUsers", disabledUsersOu ?? "");
                                cmd.Parameters.AddWithValue("@homeDir", homeDirectory ?? "");
                                cmd.Parameters.AddWithValue("@linuxDs", linuxDs ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                _consoleForm?.WriteSuccess("Toolbelt configuration saved to database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save toolbelt config: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads the toolbelt configuration from the database.
        /// Returns null if no configuration exists.
        /// </summary>
        public ToolbeltConfig LoadToolbeltConfig()
        {
            try
            {
                if (!File.Exists(_databasePath))
                    return null;

                using (var connection = GetConnection())
                {
                    string sql = "SELECT VCenter_Server, PowerCLI_Location, Sql_Path, Excluded_OU, Disabled_Users_Ou, HomeDirectory, Linux_Ds FROM Toolbelt_Config LIMIT 1";

                    using (var cmd = new SqliteCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ToolbeltConfig
                            {
                                VCenterServer = reader.GetString(0),
                                PowerCLILocation = reader.GetString(1),
                                SqlPath = reader.GetString(2),
                                ExcludedOU = reader.GetString(3),
                                DisabledUsersOu = reader.GetString(4),
                                HomeDirectory = reader.GetString(5),
                                LinuxDs = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load toolbelt config: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a valid configuration exists in the database.
        /// </summary>
        public bool HasValidConfig()
        {
            if (!File.Exists(_databasePath))
                return false;

            var config = LoadToolbeltConfig();
            return config != null && !string.IsNullOrEmpty(config.VCenterServer);
        }

        #endregion

        #region ComputerList CRUD

        public void SaveComputerList(List<ComputerListEntry> entries)
        {
            try
            {
                EnsureLockHeld();

                using (var connection = GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    // Clear existing entries
                    using (var clearCmd = new SqliteCommand("DELETE FROM ComputerList", connection, transaction))
                    {
                        clearCmd.ExecuteNonQuery();
                    }

                    string insertSql = "INSERT INTO ComputerList (Computername, Type, VMWare, Instructions) VALUES (@name, @type, @vmware, @instructions)";

                    foreach (var entry in entries)
                    {
                        using (var cmd = new SqliteCommand(insertSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@name", entry.Computername ?? "");
                            cmd.Parameters.AddWithValue("@type", entry.Type ?? "");
                            cmd.Parameters.AddWithValue("@vmware", entry.VMWare ?? "");
                            cmd.Parameters.AddWithValue("@instructions", entry.Instructions ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                _consoleForm?.WriteSuccess($"Saved {entries.Count} computer list entries to database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save computer list: {ex.Message}");
                throw;
            }
        }

        public List<ComputerListEntry> LoadComputerList()
        {
            var entries = new List<ComputerListEntry>();
            try
            {
                if (!File.Exists(_databasePath)) return entries;

                using (var connection = GetConnection())
                {
                    string sql = "SELECT Computername, Type, VMWare, Instructions FROM ComputerList";

                    using (var cmd = new SqliteCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(new ComputerListEntry
                            {
                                Computername = reader.GetString(0),
                                Type = reader.GetString(1),
                                VMWare = reader.GetString(2),
                                Instructions = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load computer list: {ex.Message}");
            }
            return entries;
        }

        #endregion

        #region LogConfiguration CRUD

        public void SaveLogConfiguration(List<LogConfigEntry> entries)
        {
            try
            {
                EnsureLockHeld();

                using (var connection = GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    using (var clearCmd = new SqliteCommand("DELETE FROM LogConfiguration", connection, transaction))
                    {
                        clearCmd.ExecuteNonQuery();
                    }

                    string insertSql = "INSERT INTO LogConfiguration (Server, server_instance) VALUES (@server, @instance)";

                    foreach (var entry in entries)
                    {
                        using (var cmd = new SqliteCommand(insertSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@server", entry.Server ?? "");
                            cmd.Parameters.AddWithValue("@instance", entry.ServerInstance ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                _consoleForm?.WriteSuccess($"Saved {entries.Count} log configuration entries to database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save log configuration: {ex.Message}");
                throw;
            }
        }

        public List<LogConfigEntry> LoadLogConfiguration()
        {
            var entries = new List<LogConfigEntry>();
            try
            {
                if (!File.Exists(_databasePath)) return entries;

                using (var connection = GetConnection())
                {
                    string sql = "SELECT Server, server_instance FROM LogConfiguration";

                    using (var cmd = new SqliteCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(new LogConfigEntry
                            {
                                Server = reader.GetString(0),
                                ServerInstance = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load log configuration: {ex.Message}");
            }
            return entries;
        }

        #endregion

        #region ouConfiguration CRUD

        public void SaveOUConfiguration(List<OUConfigEntry> entries)
        {
            try
            {
                EnsureLockHeld();

                using (var connection = GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    using (var clearCmd = new SqliteCommand("DELETE FROM ouConfiguration", connection, transaction))
                    {
                        clearCmd.ExecuteNonQuery();
                    }

                    string insertSql = "INSERT INTO ouConfiguration (ou, MiddleName, keyword) VALUES (@ou, @middleName, @keyword)";

                    foreach (var entry in entries)
                    {
                        using (var cmd = new SqliteCommand(insertSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ou", entry.OU ?? "");
                            cmd.Parameters.AddWithValue("@middleName", entry.MiddleName ?? "");
                            cmd.Parameters.AddWithValue("@keyword", entry.Keyword ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                _consoleForm?.WriteSuccess($"Saved {entries.Count} OU configuration entries to database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save OU configuration: {ex.Message}");
                throw;
            }
        }

        public List<OUConfigEntry> LoadOUConfiguration()
        {
            var entries = new List<OUConfigEntry>();
            try
            {
                if (!File.Exists(_databasePath)) return entries;

                using (var connection = GetConnection())
                {
                    string sql = "SELECT ou, MiddleName, keyword FROM ouConfiguration";

                    using (var cmd = new SqliteCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(new OUConfigEntry
                            {
                                OU = reader.GetString(0),
                                MiddleName = reader.GetString(1),
                                Keyword = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load OU configuration: {ex.Message}");
            }
            return entries;
        }

        #endregion

        #region GPO_Processing CRUD

        public void SaveGPOProcessing(List<GPOProcessingEntry> entries)
        {
            try
            {
                EnsureLockHeld();

                using (var connection = GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    using (var clearCmd = new SqliteCommand("DELETE FROM GPO_Processing", connection, transaction))
                    {
                        clearCmd.ExecuteNonQuery();
                    }

                    string insertSql = @"INSERT INTO GPO_Processing (AD_Location, GPO_Name, Policy_Location, Policy_Name, Setting, Value)
                        VALUES (@adLoc, @gpoName, @policyLoc, @policyName, @setting, @value)";

                    foreach (var entry in entries)
                    {
                        using (var cmd = new SqliteCommand(insertSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@adLoc", entry.ADLocation ?? "");
                            cmd.Parameters.AddWithValue("@gpoName", entry.GPOName ?? "");
                            cmd.Parameters.AddWithValue("@policyLoc", entry.PolicyLocation ?? "");
                            cmd.Parameters.AddWithValue("@policyName", entry.PolicyName ?? "");
                            cmd.Parameters.AddWithValue("@setting", entry.Setting ?? "");
                            cmd.Parameters.AddWithValue("@value", entry.Value ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                _consoleForm?.WriteSuccess($"Saved {entries.Count} GPO processing entries to database.");
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to save GPO processing: {ex.Message}");
                throw;
            }
        }

        public List<GPOProcessingEntry> LoadGPOProcessing()
        {
            var entries = new List<GPOProcessingEntry>();
            try
            {
                if (!File.Exists(_databasePath)) return entries;

                using (var connection = GetConnection())
                {
                    string sql = "SELECT AD_Location, GPO_Name, Policy_Location, Policy_Name, Setting, Value FROM GPO_Processing";

                    using (var cmd = new SqliteCommand(sql, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(new GPOProcessingEntry
                            {
                                ADLocation = reader.GetString(0),
                                GPOName = reader.GetString(1),
                                PolicyLocation = reader.GetString(2),
                                PolicyName = reader.GetString(3),
                                Setting = reader.GetString(4),
                                Value = reader.GetString(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to load GPO processing: {ex.Message}");
            }
            return entries;
        }

        #endregion

        #region Database Migration (Move DB to new location)

        /// <summary>
        /// Moves the database from its current location to a new path.
        /// Updates the registry to point to the new location.
        /// </summary>
        public bool MoveDatabase(string newFolderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(newFolderPath))
                {
                    _consoleForm?.WriteError("Cannot move database: new path is empty.");
                    return false;
                }

                string newDbPath = Path.Combine(newFolderPath, DB_FILENAME);

                // If already at the target location, just update registry
                if (string.Equals(_databasePath, newDbPath, StringComparison.OrdinalIgnoreCase))
                {
                    SetSqlPathInRegistry(newFolderPath);
                    _consoleForm?.WriteInfo("Database already at target location. Registry updated.");
                    return true;
                }

                // Ensure target directory exists
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    _consoleForm?.WriteInfo($"Created target directory: {newFolderPath}");
                }

                // Release the lock at the old location before moving
                string oldLockPath = _lockFilePath;
                bool hadLock = _lockAcquired;
                ReleaseLock();

                // Copy the database to the new location
                File.Copy(_databasePath, newDbPath, overwrite: true);
                _consoleForm?.WriteInfo($"Database copied to: {newDbPath}");

                // Verify the copy worked
                if (!File.Exists(newDbPath))
                {
                    _consoleForm?.WriteError("Database copy verification failed.");
                    // Try to reacquire lock at old location
                    if (hadLock) AcquireLock();
                    return false;
                }

                // Delete the old database (only if it was in the default Documents location)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string defaultPath = Path.Combine(documentsPath, "SA_Toolbelt", DB_FILENAME);
                if (string.Equals(_databasePath, defaultPath, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(newDbPath, defaultPath, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        File.Delete(_databasePath);
                        _consoleForm?.WriteInfo($"Removed temporary database from: {_databasePath}");
                    }
                    catch (Exception delEx)
                    {
                        _consoleForm?.WriteWarning($"Could not remove old database file: {delEx.Message}");
                    }
                }

                // Update registry and internal path
                SetSqlPathInRegistry(newFolderPath);
                _databasePath = newDbPath;
                _lockFilePath = newDbPath + LOCK_FILE_EXTENSION;

                // Reacquire lock at the new location
                if (hadLock && !AcquireLock())
                {
                    _consoleForm?.WriteWarning("Database moved but could not reacquire lock at new location.");
                }

                _consoleForm?.WriteSuccess($"Database moved to: {newDbPath}");
                _consoleForm?.WriteSuccess($"Registry updated with new path: {newFolderPath}");
                return true;
            }
            catch (Exception ex)
            {
                _consoleForm?.WriteError($"Failed to move database: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ReleaseLock();
                }
                _disposed = true;
            }
        }

        #endregion
    }

    #region Data Transfer Objects

    public class ToolbeltConfig
    {
        public string VCenterServer { get; set; } = string.Empty;
        public string PowerCLILocation { get; set; } = string.Empty;
        public string SqlPath { get; set; } = string.Empty;
        public string ExcludedOU { get; set; } = string.Empty;
        public string DisabledUsersOu { get; set; } = string.Empty;
        public string HomeDirectory { get; set; } = string.Empty;
        public string LinuxDs { get; set; } = string.Empty;
    }

    public class ComputerListEntry
    {
        public string Computername { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string VMWare { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
    }

    public class LogConfigEntry
    {
        public string Server { get; set; } = string.Empty;
        public string ServerInstance { get; set; } = string.Empty;
    }

    public class OUConfigEntry
    {
        public string OU { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Keyword { get; set; } = string.Empty;
    }

    public class GPOProcessingEntry
    {
        public string ADLocation { get; set; } = string.Empty;
        public string GPOName { get; set; } = string.Empty;
        public string PolicyLocation { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string Setting { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    #endregion
}
