using DataBase.Connection;
using DataBase.Mappers;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataBase.Repository.Access
{
    public class AccessUserRepository : IUserRepository
    {
        private readonly IDataBaseConnection _database;

        public AccessUserRepository() : this(DatabaseFilter.CreateConnection())
        {
        }

        public AccessUserRepository(IDataBaseConnection database)
        {
            _database = database ?? DatabaseFilter.CreateConnection();
        }

        public User GetById(int userId)
        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl WHERE Id = ?", userId);
            return dt.Rows.Count > 0 ? UserMapper.MapBaseUser(dt.Rows[0]) : null;
        }

        public User GetByUsername(string username)
        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl WHERE Username = ?", username);
            return dt.Rows.Count > 0 ? UserMapper.MapBaseUser(dt.Rows[0]) : null;
        }

        public User GetByUsernameAndPassword(string username, string password)
        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl WHERE Username = ? AND Password = ?", username, password);
            return dt.Rows.Count > 0 ? UserMapper.MapBaseUser(dt.Rows[0]) : null;
        }

        public List<User> GetAllUsers()
        {
            var dt = _database.ExecuteQuery("SELECT * FROM UserTbl");
            var users = new List<User>();
            foreach (DataRow row in dt.Rows)
            {
                users.Add(UserMapper.MapBaseUser(row));
            }

            return users;
        }

        public bool UserExists(string username, string email)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM UserTbl WHERE Username = ? OR Email = ?", username, email);
            return dt.Rows.Count > 0;
        }

        public bool ValidateLogin(string username, string password)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM UserTbl WHERE Username = ? AND Password = ?", username, password);
            return dt.Rows.Count > 0;
        }

        public int CreateUser(User userData)
        {
            int isTrainerFlag = userData.IsTrainer ? -1 : 0;
            string joinDate = DateTime.Now.ToString("yyyy-MM-dd");

            _database.ExecuteNonQuery(
                "INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer], [CurrentWeekPlanId]) VALUES (?, ?, ?, ?, ?, ?)",
                userData.Username,
                userData.Email ?? (object)DBNull.Value,
                userData.Password,
                joinDate,
                isTrainerFlag,
                DBNull.Value);

            System.Threading.Thread.Sleep(100);

            var dt = _database.ExecuteQuery(
                "SELECT Id FROM UserTbl WHERE Username = ? AND Email = ?",
                userData.Username,
                userData.Email ?? (object)DBNull.Value);

            if (dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0]["Id"]);
            }

            return 0;
        }

        public bool UpdateUserCommon(int userId, string bio, string email)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE UserTbl SET Bio = ?, Email = ? WHERE Id = ?",
                bio ?? (object)DBNull.Value, email ?? (object)DBNull.Value, userId);
            return affected > 0;
        }

        public bool UpdateCurrentWeekPlanId(int userId, int weekPlanId)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE UserTbl SET CurrentWeekPlanId = ? WHERE Id = ?",
                weekPlanId, userId);
            return affected > 0;
        }

        public void DeleteUser(int userId)
        {
            _database.ExecuteNonQuery("DELETE FROM UserTbl WHERE Id = ?", userId);
        }
    }
}
