using DataBase.Connection;
using DataBase.DataTransferObjects;
using DataBase.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Access
{
    public class AccessUserRepository : IUserRepository
    {
        private readonly IDataBaseConnection _database;

        public AccessUserRepository(IDataBaseConnection database)
        {
            _database = database;
        }

        public UserData GetById(int userId)
        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl WHERE Id = ?", userId);
            if (dt.Rows.Count == 0) return null;
            return MapToUserData(dt.Rows[0]);
        }
        public UserData GetByUsername(string username)

        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl WHERE Username = ?", username);
            if (dt.Rows.Count == 0) return null;
            return MapToUserData(dt.Rows[0]);
        }
        public UserData GetByUsernameAndPassword(string username, string password)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );
            if (dt.Rows.Count == 0) return null;
            return MapToUserData(dt.Rows[0]);
        }
        public List<UserData> GetAllUsers()
        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl");
            var users = new List<UserData>();

            foreach (DataRow row in dt.Rows)
            {
                users.Add(MapToUserData(row));
            }

            return users;
        }
        public bool UserExists(string username, string email)
        {
            var dt = _database.ExecuteQuery(
                "SELECT Id FROM UserTbl WHERE Username = ? OR Email = ?",
                username, email
            );
            return dt.Rows.Count > 0;
        }
        public bool ValidateLogin(string username, string password)
        {
            var dt = _database.ExecuteQuery(
                "SELECT Id FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );
            return dt.Rows.Count > 0;
        }
        public int CreateUser(UserData userData)
        {
            try
            {
                // Insert into UserTbl
                _database.ExecuteNonQuery(
                    @"INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer], [Bio], [Gender], [CurrentWeekPlanId]) 
                      VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    userData.Username,
                    userData.Email,
                    userData.Password,
                    userData.JoinDate,
                    userData.IsTrainer ? -1 : 0, // Access uses -1 for True, 0 for False
                    userData.Bio ?? string.Empty,
                    userData.Gender ?? string.Empty,
                    DBNull.Value // CurrentWeekPlanId - will be updated later
                );

                // Small delay for Access to commit
                System.Threading.Thread.Sleep(100);

                // Get the new user ID
                var dt = _database.ExecuteQuery(
                    "SELECT Id FROM UserTbl WHERE Username = ? AND Email = ?",
                    userData.Username, userData.Email
                );

                if (dt.Rows.Count == 0) return 0;

                return Convert.ToInt32(dt.Rows[0]["Id"]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateUser error: {ex.Message}");
                return 0;
            }
        }
        public bool UpdateUserCommon(int userId, string bio, string email)
        {
            try
            {
                int rows = _database.ExecuteNonQuery(
                    "UPDATE UserTbl SET Bio = ?, Email = ? WHERE Id = ?",
                    bio ?? string.Empty,
                    email ?? string.Empty,
                    userId
                );
                return rows > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUserCommon error: {ex.Message}");
                return false;
            }
        }
        public bool UpdateCurrentWeekPlanId(int userId, int weekPlanId)
        {
            try
            {
                int rows = _database.ExecuteNonQuery(
                    "UPDATE UserTbl SET CurrentWeekPlanId = ? WHERE Id = ?",
                    weekPlanId,
                    userId
                );
                return rows > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateCurrentWeekPlanId error: {ex.Message}");
                return false;
            }
        }
        public void DeleteUser(int userId)
        {
            _database.ExecuteNonQuery("DELETE FROM UserTbl WHERE Id = ?", userId);
        }
        private UserData MapToUserData(DataRow row)
        {
            return new UserData
            {
                Id = Convert.ToInt32(row["Id"]),
                Username = row["Username"].ToString(),
                Email = row["Email"]?.ToString(),
                Password = row["Password"].ToString(),
                JoinDate = row["JoinDate"]?.ToString(),
                IsTrainer = row["IsTrainer"] != DBNull.Value && Convert.ToBoolean(row["IsTrainer"]),
                Bio = row["Bio"]?.ToString(),
                Gender = row["Gender"]?.ToString(),
                CurrentWeekPlanId = row["CurrentWeekPlanId"] != DBNull.Value ? Convert.ToInt32(row["CurrentWeekPlanId"]) : 0
            };
        }

    }
}
