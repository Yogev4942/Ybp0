using DataBase.Mappers;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Access
{
    public class AccessTrainerRepository : ITrainerRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessTrainerRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public Trainer GetTrainerById(int userId)
        {
            string query = @"
                SELECT u.*, t.Specialization, t.HourlyRate, t.MaxTrainees, t.TotalTrainees, t.Rating, t.TotalRatings
                FROM UserTbl u
                INNER JOIN TrainersTbl t ON u.Id = t.UserId
                WHERE u.Id = ?";

            var dt = _database.ExecuteQuery(query, userId);
            return dt.Rows.Count > 0 ? UserMapper.MapTrainer(dt.Rows[0]) : null;
        }

        public List<Trainer> SearchTrainers(string searchQuery)
        {
            string query = @"
                SELECT u.*, t.Specialization, t.HourlyRate, t.MaxTrainees, t.TotalTrainees, t.Rating, t.TotalRatings
                FROM UserTbl u
                INNER JOIN TrainersTbl t ON u.Id = t.UserId
                WHERE u.IsTrainer = True AND u.Username LIKE ?";

            var dt = string.IsNullOrWhiteSpace(searchQuery)
                ? _database.ExecuteQuery("SELECT u.*, t.Specialization, t.HourlyRate, t.MaxTrainees, t.TotalTrainees, t.Rating, t.TotalRatings FROM UserTbl u INNER JOIN TrainersTbl t ON u.Id = t.UserId WHERE u.IsTrainer = True")
                : _database.ExecuteQuery(query, "%" + searchQuery.Trim() + "%");

            var results = new List<Trainer>();
            foreach (DataRow row in dt.Rows)
            {
                results.Add(UserMapper.MapTrainer(row));
            }
            return results;
        }

        public bool CreateTrainerProfile(int userId, string specialization, double hourlyRate, int maxTrainees)
        {
            int affected = _database.ExecuteNonQuery(
                "INSERT INTO TrainersTbl ([UserId], [Specialization], [HourlyRate], [MaxTrainees]) VALUES (?, ?, ?, ?)",
                userId, specialization, hourlyRate, maxTrainees
            );
            return affected > 0;
        }

        public bool UpdateTrainerProfile(Trainer trainer)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE TrainersTbl SET Specialization = ?, HourlyRate = ?, MaxTrainees = ? WHERE UserId = ?",
                trainer.Specialization, trainer.HourlyRate, trainer.MaxTrainees, trainer.Id
            );
            return affected > 0;
        }
    }
}
