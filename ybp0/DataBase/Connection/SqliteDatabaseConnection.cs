using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Text.RegularExpressions;

namespace DataBase.Connection
{
    public class SqliteDatabaseConnection : IDataBaseConnection
    {
        private readonly string _connectionString;
        private static bool _sqliteInitialized;

        public SqliteDatabaseConnection(string dbPath)
        {
            if (!_sqliteInitialized)
            {
                Batteries_V2.Init();
                _sqliteInitialized = true;
            }

            _connectionString = $"Data Source={dbPath}";
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
    }
}
