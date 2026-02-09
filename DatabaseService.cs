using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;

namespace SA_ToolBelt
{
    /// <summary>
    /// Manages all SQLite database operations for the SA Toolbelt.
    /// Handles creation, reading, writing, and migration of the configuration database.
    /// </summary>
    public class DatabaseService
    {
        private readonly ConsoleForm _consoleForm;
        private string _databasePath;

        private const string DB_FILENAME = "SA_Toolbelt.db";
        private const string REGISTRY_KEY_PATH = @"SOFTWARE\SA_Toolbelt";
        private const string REGISTRY_VALUE_NAME = "SqlPath";

        public string DatabasePath => _databasePath;

        public DatabaseService(ConsoleForm consoleForm)
        {
            _consoleForm = consoleForm;
            _databasePath = ResolveDatabasePath();
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
                    HomeDirectory TEXT NOT NULL DEFAULT ''
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

            _consoleForm?.WriteInfo("All database tables verified/created.");
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

            // Wait up to 5 seconds if the DB is locked by another operation
            // instead of immediately throwing an error
            using (var timeoutCmd = new SqliteCommand("PRAGMA busy_timeout=5000;", connection))
            {
                timeoutCmd.ExecuteNonQuery();
            }

            return connection;
        }

        #endregion

        #region Toolbelt_Config CRUD

        /// <summary>
        /// Saves or updates the toolbelt configuration (single-row table).
        /// </summary>
        public void SaveToolbeltConfig(string vCenterServer, string powerCliLocation, string sqlPath,
            string excludedOu, string disabledUsersOu, string homeDirectory)
        {
            try
            {
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
                                HomeDirectory = @homeDir
                                WHERE Id = (SELECT MIN(Id) FROM Toolbelt_Config)";

                            using (var cmd = new SqliteCommand(updateSql, connection))
                            {
                                cmd.Parameters.AddWithValue("@vCenter", vCenterServer ?? "");
                                cmd.Parameters.AddWithValue("@powerCli", powerCliLocation ?? "");
                                cmd.Parameters.AddWithValue("@sqlPath", sqlPath ?? "");
                                cmd.Parameters.AddWithValue("@excludedOu", excludedOu ?? "");
                                cmd.Parameters.AddWithValue("@disabledUsers", disabledUsersOu ?? "");
                                cmd.Parameters.AddWithValue("@homeDir", homeDirectory ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new row
                            string insertSql = @"INSERT INTO Toolbelt_Config
                                (VCenter_Server, PowerCLI_Location, Sql_Path, Excluded_OU, Disabled_Users_Ou, HomeDirectory)
                                VALUES (@vCenter, @powerCli, @sqlPath, @excludedOu, @disabledUsers, @homeDir)";

                            using (var cmd = new SqliteCommand(insertSql, connection))
                            {
                                cmd.Parameters.AddWithValue("@vCenter", vCenterServer ?? "");
                                cmd.Parameters.AddWithValue("@powerCli", powerCliLocation ?? "");
                                cmd.Parameters.AddWithValue("@sqlPath", sqlPath ?? "");
                                cmd.Parameters.AddWithValue("@excludedOu", excludedOu ?? "");
                                cmd.Parameters.AddWithValue("@disabledUsers", disabledUsersOu ?? "");
                                cmd.Parameters.AddWithValue("@homeDir", homeDirectory ?? "");
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
                    string sql = "SELECT VCenter_Server, PowerCLI_Location, Sql_Path, Excluded_OU, Disabled_Users_Ou, HomeDirectory FROM Toolbelt_Config LIMIT 1";

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
                                HomeDirectory = reader.GetString(5)
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

                // Copy the database to the new location
                File.Copy(_databasePath, newDbPath, overwrite: true);
                _consoleForm?.WriteInfo($"Database copied to: {newDbPath}");

                // Verify the copy worked
                if (!File.Exists(newDbPath))
                {
                    _consoleForm?.WriteError("Database copy verification failed.");
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
