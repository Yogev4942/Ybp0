using DataBase.Connection;
using DataBase.Mappers;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataBase.Repository.Access
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly IDataBaseConnection _database;

        public TrainerRepository() : this(SqliteDatabaseConnection.CreateDefault())
        {
        }

        public TrainerRepository(IDataBaseConnection database)
        {
            _database = database ?? SqliteDatabaseConnection.CreateDefault();
            EnsureSchema();
        }

        public Trainer GetTrainerById(int userId)
        {
            string query = @"
                SELECT u.*, t.Id AS TrainerProfileId, t.Specialization, t.HourlyRate, t.MaxTrainees, t.TotalTrainees, t.Rating, t.TotalRatings
                FROM UserTbl u
                INNER JOIN TrainersTbl t ON u.Id = t.UserId
                WHERE u.Id = ?";

            var dt = _database.ExecuteQuery(query, userId);
            return dt.Rows.Count > 0 ? UserMapper.MapTrainer(dt.Rows[0]) : null;
        }

        public List<Trainer> SearchTrainers(string searchQuery)
        {
            string query = @"
                SELECT u.*, t.Id AS TrainerProfileId, t.Specialization, t.HourlyRate, t.MaxTrainees, t.TotalTrainees, t.Rating, t.TotalRatings
                FROM UserTbl u
                INNER JOIN TrainersTbl t ON u.Id = t.UserId
                WHERE u.IsTrainer = True AND u.Username LIKE ?";

            var dt = string.IsNullOrWhiteSpace(searchQuery)
                ? _database.ExecuteQuery("SELECT u.*, t.Id AS TrainerProfileId, t.Specialization, t.HourlyRate, t.MaxTrainees, t.TotalTrainees, t.Rating, t.TotalRatings FROM UserTbl u INNER JOIN TrainersTbl t ON u.Id = t.UserId WHERE u.IsTrainer = True")
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
                userId, specialization, hourlyRate, maxTrainees);
            return affected > 0;
        }

        public bool UpdateTrainerProfile(Trainer trainer)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE TrainersTbl SET Specialization = ?, HourlyRate = ?, MaxTrainees = ? WHERE UserId = ?",
                trainer.Specialization, trainer.HourlyRate, trainer.MaxTrainees, trainer.Id);
            return affected > 0;
        }

        public int? GetTraineeRatingForTrainer(int traineeUserId, int trainerUserId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT Rating
                  FROM TrainerRatingsTbl
                  WHERE TraineeUserId = ? AND TrainerUserId = ?",
                traineeUserId,
                trainerUserId);

            return dt.Rows.Count > 0 && dt.Rows[0]["Rating"] != DBNull.Value
                ? Convert.ToInt32(dt.Rows[0]["Rating"])
                : (int?)null;
        }

        public bool RateTrainer(int traineeUserId, int trainerUserId, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                return false;
            }

            int affected = _database.ExecuteNonQuery(
                @"INSERT OR REPLACE INTO TrainerRatingsTbl (TraineeUserId, TrainerUserId, Rating, RatedAt)
                  VALUES (?, ?, ?, ?)",
                traineeUserId,
                trainerUserId,
                rating,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (affected <= 0)
            {
                return false;
            }

            _database.ExecuteNonQuery(
                @"UPDATE TrainersTbl
                  SET Rating = (
                          SELECT AVG(Rating)
                          FROM TrainerRatingsTbl
                          WHERE TrainerUserId = ?
                      ),
                      TotalRatings = (
                          SELECT COUNT(*)
                          FROM TrainerRatingsTbl
                          WHERE TrainerUserId = ?
                      )
                  WHERE UserId = ?",
                trainerUserId,
                trainerUserId,
                trainerUserId);

            return true;
        }

        private void EnsureSchema()
        {
            _database.ExecuteNonQuery(
                @"CREATE TABLE IF NOT EXISTS [TrainerRatingsTbl] (
                    [TraineeUserId] INTEGER NOT NULL,
                    [TrainerUserId] INTEGER NOT NULL,
                    [Rating] INTEGER NOT NULL,
                    [RatedAt] TEXT NULL,
                    PRIMARY KEY ([TraineeUserId], [TrainerUserId])
                  )");

            _database.ExecuteNonQuery(
                @"CREATE INDEX IF NOT EXISTS [IX_TrainerRatingsTbl_TrainerUserId]
                  ON [TrainerRatingsTbl] ([TrainerUserId])");
        }
    }
}
