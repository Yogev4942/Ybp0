using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class AccessDatabaseConnection
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        private const string AccessProvider = "Microsoft.ACE.OLEDB.12.0";

        public AccessDatabaseConnection()
        {
            string dbFileName = "DB.accdb";
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);

            _connectionString = $"Provider={AccessProvider};Data Source={_dbPath}";

            if (!File.Exists(_dbPath))
            {
                _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB", dbFileName);
                _connectionString = $"Provider={AccessProvider};Data Source={_dbPath}";
            }
        }

        private OleDbConnection GetConnection()
        {
            return new OleDbConnection(_connectionString);
        }

        public DataTable ExecuteQuery(string sql)
        {
            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            using (var adapter = new OleDbDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                conn.Open();
                adapter.Fill(dt);
                return dt;
            }
        }
        public DataTable ExecuteQuery(string sql, params object[] parameters)
        {
            using (var conn = new OleDbConnection(_connectionString))
            using (var cmd = new OleDbCommand(sql, conn))
            using (var adapter = new OleDbDataAdapter(cmd))
            {
                conn.Open();

                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue("?", p);

                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var conn = new OleDbConnection(_connectionString))
            using (var cmd = new OleDbCommand(sql, conn))
            {
                conn.Open();

                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue("?", p);

                return cmd.ExecuteNonQuery();
            }
        }


        public object ExecuteScalar(string sql)
        {
            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public object ExecuteScalar(string sql, params object[] parameters)
        {
            using (var conn = new OleDbConnection(_connectionString))
            using (var cmd = new OleDbCommand(sql, conn))
            {
                conn.Open();

                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue("?", p);

                return cmd.ExecuteScalar();
            }
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
