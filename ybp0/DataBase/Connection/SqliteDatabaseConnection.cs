using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DataBase.Connection
{
    public class SqliteDatabaseConnection : IDataBaseConnection
    {
        private readonly string _connectionString;
        private readonly string _dbPath;
        private static bool _sqliteInitialized;
        private static readonly object InitializationLock = new object();
        private static bool _defaultDatabaseInitialized;

        public SqliteDatabaseConnection(string dbPath)
        {
            if (!_sqliteInitialized)
            {
                Batteries_V2.Init();
                _sqliteInitialized = true;
            }

            _dbPath = dbPath;
            _connectionString = $"Data Source={dbPath}";
        }

        public static IDataBaseConnection CreateDefault()
        {
            string dbPath = GetDefaultDatabasePath();

            lock (InitializationLock)
            {
                if (!_defaultDatabaseInitialized)
                {
                    EnsureDirectoryExists(dbPath);
                    InitializeDefaultDatabase(dbPath);
                    _defaultDatabaseInitialized = true;
                }
            }

            return new SqliteDatabaseConnection(dbPath);
        }

        public DataTable ExecuteQuery(string query, params object[] parameters)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var command = CreateCommand(connection, query, parameters))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }

        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var command = CreateCommand(connection, query, parameters))
            {
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public T ExecuteScalar<T>(string query, params object[] parameters)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var command = CreateCommand(connection, query, parameters))
            {
                connection.Open();
                object result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    return default(T);
                }

                Type targetType = typeof(T);
                Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                object convertedValue = Convert.ChangeType(result, underlyingType);
                return (T)convertedValue;
            }
        }

        public bool TableExists(string tableName)
        {
            return ExecuteScalar<long>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type = ? AND name = ?",
                "table",
                tableName) > 0;
        }

        public bool ColumnExists(string tableName, string columnName)
        {
            using (var connection = new SqliteConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA table_info({QuoteIdentifier(tableName)})";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private SqliteCommand CreateCommand(SqliteConnection connection, string query, object[] parameters)
        {
            List<object> values = ExtractValues(parameters);
            string translatedQuery = TranslateQuery(query, values.Count);

            var command = connection.CreateCommand();
            command.CommandText = translatedQuery;

            for (int i = 0; i < values.Count; i++)
            {
                command.Parameters.AddWithValue($"@p{i}", values[i] ?? DBNull.Value);
            }

            return command;
        }

        private static List<object> ExtractValues(object[] parameters)
        {
            var values = new List<object>();

            if (parameters == null)
            {
                return values;
            }

            foreach (object parameter in parameters)
            {
                if (parameter is OleDbParameter oleDbParameter)
                {
                    values.Add(oleDbParameter.Value ?? DBNull.Value);
                }
                else
                {
                    values.Add(parameter ?? DBNull.Value);
                }
            }

            return values;
        }

        private static string TranslateQuery(string query, int parameterCount)
        {
            string translated = TranslateTopClause(query);
            translated = Regex.Replace(translated, @"\bTrue\b", "1", RegexOptions.IgnoreCase);
            translated = Regex.Replace(translated, @"\bFalse\b", "0", RegexOptions.IgnoreCase);

            for (int i = 0; i < parameterCount; i++)
            {
                translated = ReplaceFirst(translated, "?", $"@p{i}");
            }

            return translated;
        }

        private static string TranslateTopClause(string query)
        {
            Match match = Regex.Match(query, @"^\s*SELECT\s+TOP\s+(\d+)\s+", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return query;
            }

            string limit = match.Groups[1].Value;
            string translated = Regex.Replace(query, @"^\s*SELECT\s+TOP\s+\d+\s+", "SELECT ", RegexOptions.IgnoreCase);

            if (!Regex.IsMatch(translated, @"\bLIMIT\b", RegexOptions.IgnoreCase))
            {
                translated = translated.TrimEnd().TrimEnd(';') + $" LIMIT {limit}";
            }

            return translated;
        }

        private static string ReplaceFirst(string input, string oldValue, string newValue)
        {
            int index = input.IndexOf(oldValue, StringComparison.Ordinal);
            if (index < 0)
            {
                return input;
            }

            var builder = new StringBuilder();
            builder.Append(input.Substring(0, index));
            builder.Append(newValue);
            builder.Append(input.Substring(index + oldValue.Length));
            return builder.ToString();
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private static void InitializeDefaultDatabase(string dbPath)
        {
            bool sqliteExists = File.Exists(dbPath);
            var connection = new SqliteDatabaseConnection(dbPath);
            EnsureSchema(connection);

            if (!sqliteExists || IsSeedCandidate(connection))
            {
                SeedFromAccessIfAvailable(connection, GetDefaultAccessPath(dbPath));
            }
        }

        private static bool IsSeedCandidate(SqliteDatabaseConnection connection)
        {
            return connection.ExecuteScalar<long>("SELECT COUNT(*) FROM [UserTbl]") == 0 &&
                   connection.ExecuteScalar<long>("SELECT COUNT(*) FROM [ExercisesTbl]") == 0;
        }

        private static void SeedFromAccessIfAvailable(SqliteDatabaseConnection sqliteConnection, string accessPath)
        {
            if (!File.Exists(accessPath))
            {
                return;
            }

            var accessConnection = new global::DataBase.AccessDatabaseConnection(accessPath);
            foreach (string tableName in GetTableNames())
            {
                DataTable table = accessConnection.ExecuteQuery($"SELECT * FROM [{tableName}]");
                if (table.Rows.Count == 0)
                {
                    continue;
                }

                List<DataColumn> columns = new List<DataColumn>();
                foreach (DataColumn column in table.Columns)
                {
                    columns.Add(column);
                }

                string columnSql = string.Join(", ", columns.ConvertAll(column => $"[{NormalizeColumnName(tableName, column.ColumnName)}]"));
                string parameterSql = string.Join(", ", columns.ConvertAll(_ => "?"));

                foreach (DataRow row in table.Rows)
                {
                    object[] values = columns
                        .ConvertAll(column => NormalizeValueForSqlite(row[column.ColumnName]))
                        .ToArray();

                    sqliteConnection.ExecuteNonQuery(
                        $"INSERT INTO [{tableName}] ({columnSql}) VALUES ({parameterSql})",
                        values);
                }
            }
        }

        private static object NormalizeValueForSqlite(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DBNull.Value;
            }

            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }

            if (value is DateTime dateTimeValue)
            {
                return dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return value;
        }

        private static string NormalizeColumnName(string tableName, string columnName)
        {
            if (string.Equals(tableName, "WeekPlanDaysTbl", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(columnName, "WorkoutID", StringComparison.OrdinalIgnoreCase))
            {
                return "WorkoutId";
            }

            if (string.Equals(tableName, "WorkoutsTbl", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(columnName, "UserID", StringComparison.OrdinalIgnoreCase))
            {
                return "UserId";
            }

            return columnName;
        }

        private static void EnsureSchema(SqliteDatabaseConnection connection)
        {
            foreach (string statement in GetCreateTableStatements())
            {
                connection.ExecuteNonQuery(statement);
            }

            foreach (string statement in GetCreateIndexStatements())
            {
                connection.ExecuteNonQuery(statement);
            }
        }

        private static IEnumerable<string> GetCreateTableStatements()
        {
            return new[]
            {
                @"CREATE TABLE IF NOT EXISTS [AdminsTbl] ([×ž×–×”×”] INTEGER PRIMARY KEY AUTOINCREMENT)",
                @"CREATE TABLE IF NOT EXISTS [ExercisesTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [ExerciseName] TEXT NULL, [PrimaryMuscleId] INTEGER NULL, [SecondaryMuscleId] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [LikesTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [PostId] INTEGER NULL, [UserId] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [MessagesTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [SenderId] INTEGER NULL, [RecipientId] INTEGER NULL, [MessageText] TEXT NULL, [SentAt] TEXT NULL)",
                @"CREATE TABLE IF NOT EXISTS [MusclesTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [MuscleName] TEXT NULL, [BodyRegion] TEXT NULL, [DiagramZoneId] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [PostTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [OwnerId] INTEGER NULL, [Header] TEXT NULL, [Content] TEXT NULL, [PostTime] TEXT NULL)",
                @"CREATE TABLE IF NOT EXISTS [TraineesTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [UserId] INTEGER NULL, [TrainerId] INTEGER NULL, [FitnessGoal] TEXT NULL, [CurrentWeight] INTEGER NULL, [Height] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [TrainerRequestsTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [TraineeUserId] INTEGER NULL, [TrainerUserId] INTEGER NULL, [Status] TEXT NULL, [RequestDate] TEXT NULL)",
                @"CREATE TABLE IF NOT EXISTS [TrainersTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [UserId] INTEGER NULL, [Specialization] TEXT NULL, [HourlyRate] INTEGER NULL, [MaxTrainees] INTEGER NULL, [TotalTrainees] INTEGER NULL, [Rating] INTEGER NULL, [TotalRatings] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [UserTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [Email] TEXT NULL, [Username] TEXT NULL, [Password] TEXT NULL, [JoinDate] TEXT NULL, [IsTrainer] INTEGER NULL, [Bio] TEXT NULL, [Gender] TEXT NULL, [CurrentWeekPlanId] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [WeekPlanDaysTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [WeekPlanId] INTEGER NULL, [DayOfWeek] TEXT NULL, [WorkoutId] INTEGER NULL, [RestDay] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [WeekPlansTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [UserId] INTEGER NULL, [PlanName] TEXT NULL)",
                @"CREATE TABLE IF NOT EXISTS [WorkoutExercisesTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [WorkoutId] INTEGER NULL, [ExerciseId] INTEGER NULL, [OrderNumber] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [WorkoutSessionSetsTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [WorkoutSessionId] INTEGER NULL, [ExerciseId] INTEGER NULL, [SetNumber] INTEGER NULL, [Reps] INTEGER NULL, [Weight] INTEGER NULL, [IsCompleted] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [WorkoutSessionTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [UserId] INTEGER NULL, [WeekPlanDayId] INTEGER NULL, [WorkoutId] INTEGER NULL, [SessionDate] TEXT NULL, [IsCompleted] INTEGER NULL, [StartTime] TEXT NULL, [EndTime] TEXT NULL, [SessionMode] TEXT NULL)",
                @"CREATE TABLE IF NOT EXISTS [WorkoutSetsTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [WorkoutExerciseId] INTEGER NULL, [SetNumber] INTEGER NULL, [Reps] INTEGER NULL, [Weight] INTEGER NULL)",
                @"CREATE TABLE IF NOT EXISTS [WorkoutsTbl] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [UserId] INTEGER NULL, [WorkoutName] TEXT NULL)"
            };
        }

        private static IEnumerable<string> GetCreateIndexStatements()
        {
            return new[]
            {
                @"CREATE INDEX IF NOT EXISTS [IX_PostTbl_OwnerId] ON [PostTbl] ([OwnerId])",
                @"CREATE INDEX IF NOT EXISTS [IX_LikesTbl_PostId] ON [LikesTbl] ([PostId])",
                @"CREATE INDEX IF NOT EXISTS [IX_LikesTbl_UserId_PostId] ON [LikesTbl] ([UserId], [PostId])",
                @"CREATE INDEX IF NOT EXISTS [IX_MessagesTbl_Sender_Recipient_SentAt] ON [MessagesTbl] ([SenderId], [RecipientId], [SentAt])",
                @"CREATE INDEX IF NOT EXISTS [IX_MessagesTbl_Recipient_Sender_SentAt] ON [MessagesTbl] ([RecipientId], [SenderId], [SentAt])",
                @"CREATE INDEX IF NOT EXISTS [IX_WorkoutsTbl_UserId] ON [WorkoutsTbl] ([UserId])",
                @"CREATE INDEX IF NOT EXISTS [IX_WorkoutExercisesTbl_WorkoutId] ON [WorkoutExercisesTbl] ([WorkoutId])",
                @"CREATE INDEX IF NOT EXISTS [IX_WorkoutSetsTbl_WorkoutExerciseId] ON [WorkoutSetsTbl] ([WorkoutExerciseId])",
                @"CREATE INDEX IF NOT EXISTS [IX_TraineesTbl_UserId] ON [TraineesTbl] ([UserId])",
                @"CREATE INDEX IF NOT EXISTS [IX_TraineesTbl_TrainerId] ON [TraineesTbl] ([TrainerId])",
                @"CREATE INDEX IF NOT EXISTS [IX_TrainersTbl_UserId] ON [TrainersTbl] ([UserId])",
                @"CREATE INDEX IF NOT EXISTS [IX_WeekPlansTbl_UserId] ON [WeekPlansTbl] ([UserId])",
                @"CREATE INDEX IF NOT EXISTS [IX_WeekPlanDaysTbl_WeekPlanId] ON [WeekPlanDaysTbl] ([WeekPlanId])"
            };
        }

        private static IEnumerable<string> GetTableNames()
        {
            return new[]
            {
                "AdminsTbl",
                "ExercisesTbl",
                "LikesTbl",
                "MessagesTbl",
                "MusclesTbl",
                "PostTbl",
                "TraineesTbl",
                "TrainerRequestsTbl",
                "TrainersTbl",
                "UserTbl",
                "WeekPlanDaysTbl",
                "WeekPlansTbl",
                "WorkoutExercisesTbl",
                "WorkoutSessionSetsTbl",
                "WorkoutSessionTbl",
                "WorkoutSetsTbl",
                "WorkoutsTbl"
            };
        }

        private static void EnsureDirectoryExists(string dbPath)
        {
            string directory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string GetDefaultDatabasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase", "DB.sqlite");
        }

        private static string GetDefaultAccessPath(string sqlitePath)
        {
            return Path.Combine(Path.GetDirectoryName(sqlitePath), "DB.accdb");
        }
    }
}
