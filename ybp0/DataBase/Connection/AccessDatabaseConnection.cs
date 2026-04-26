using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBase.Connection;

namespace DataBase
{
    public class AccessDatabaseConnection : IDataBaseConnection
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private const string AccessProvider = "Microsoft.ACE.OLEDB.12.0";

        public AccessDatabaseConnection()
        {
            #region HardcodedDB
            string dbFileName = "DB.accdb";
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Use the database file recreated in the bin directory (Output Path)
            _dbPath = Path.Combine(baseDir, "DataBase", dbFileName);
            
            if (!File.Exists(_dbPath))
            {
                // Fallback or detailed error for debugging
                throw new FileNotFoundException($"Database not found at: {_dbPath}");
            }
            _connectionString = $"Provider={AccessProvider};Data Source={_dbPath}";
            #endregion
            #region DebugDBCopy

            #endregion
        }
        private OleDbConnection GetConnection()
        {
            return new OleDbConnection(_connectionString);
        }

        public DataTable ExecuteQuery(string query, params object[] parameters)
        {
            using (var connection = new OleDbConnection(_connectionString))
            using (var command = CreateCommand(connection, query, parameters))
            using (var adapter = new OleDbDataAdapter(command))
            {
                connection.Open();
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }
        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            using (var connection = new OleDbConnection(_connectionString))
            using (var command = CreateCommand(connection, query, parameters))
            {
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
        public T ExecuteScalar<T>(string query, params object[] parameters)
        {
            using (var connection = new OleDbConnection(_connectionString))
            using (var command = CreateCommand(connection, query, parameters))
            {
                connection.Open();
                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return default(T);

                Type targetType = typeof(T);
                Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                object convertedValue = Convert.ChangeType(result, underlyingType);
                return (T)convertedValue;
            }
        }
        private OleDbCommand CreateCommand(OleDbConnection connection, string query, object[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;

            foreach (var param in parameters)
            {
                if (param is OleDbParameter oleDbParameter)
                {
                    command.Parameters.Add(oleDbParameter);
                    continue;
                }

                command.Parameters.AddWithValue("?", param ?? DBNull.Value);
            }

            return command;
        }


        public bool TableExists(string tableName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var schema = conn.GetSchema("Tables", new[] { null, null, tableName, "TABLE" });
                return schema.Rows.Count > 0;
            }
        }
        public bool ColumnExists(string tableName, string columnName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var schema = conn.GetSchema("Columns", new[] { null, null, tableName, columnName });
                return schema.Rows.Count > 0;
            }
        }

        public void EnsureIndexes(IEnumerable<(string IndexName, string TableName, string Columns, bool IsUnique)> indexes)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                DataTable existingIndexes = conn.GetSchema("Indexes");
                DataTable existingTables = conn.GetSchema("Tables");

                foreach ((string indexName, string tableName, string columns, bool isUnique) in indexes)
                {
                    bool tableExists = existingTables.Rows.Cast<DataRow>().Any(row =>
                        string.Equals(row["TABLE_NAME"]?.ToString(), tableName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(row["TABLE_TYPE"]?.ToString(), "TABLE", StringComparison.OrdinalIgnoreCase));

                    if (!tableExists)
                    {
                        continue;
                    }

                    bool exists = existingIndexes.Rows.Cast<DataRow>().Any(row =>
                        string.Equals(row["INDEX_NAME"]?.ToString(), indexName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(row["TABLE_NAME"]?.ToString(), tableName, StringComparison.OrdinalIgnoreCase));

                    if (exists)
                    {
                        continue;
                    }

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = $"{(isUnique ? "CREATE UNIQUE INDEX" : "CREATE INDEX")} [{indexName}] ON [{tableName}] ({columns})";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
