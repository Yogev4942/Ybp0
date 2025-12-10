using DataBase;
using Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Services
{
    public class DatabaseService : IDatabaseService
    {
        private Database _database;

        public DatabaseService()
        {
            _database = new Database();
        }

        public bool ValidateLogin(string username, string password)
        {
            string sql = $"SELECT * FROM UserTbl WHERE Username='{username}' AND Password='{password}'";

            var result = _database.ExecuteQuery(sql);

            return result.Rows.Count > 0;
        }
        public User GetUserByUsernameAndPassword(string username, string password)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );

            if (dt.Rows.Count == 0)
                return null;

            var row = dt.Rows[0];
            return new User
            {
                Id = Convert.ToInt32(row["Id"]),
                Username = row["Username"].ToString(),
                Email = row["Email"]?.ToString(),
                Password = row["Password"].ToString(),
                Joindate = row["Joindate"]?.ToString()
            };
        }
        public bool UserExist(string username, string email)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Email = ?",
                username, email
            );
            return dt.Rows.Count > 0;
        }

    }
}
