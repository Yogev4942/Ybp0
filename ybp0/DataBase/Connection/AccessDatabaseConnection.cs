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
            _dbPath = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\DataBase\DataBase", dbFileName));
            if (!File.Exists(_dbPath))
            {
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
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        private OleDbCommand CreateCommand(OleDbConnection connection, string query, object[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;

            foreach (var param in parameters)
            {
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
    }
}
