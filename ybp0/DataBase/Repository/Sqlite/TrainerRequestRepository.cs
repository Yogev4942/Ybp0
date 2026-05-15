using DataBase.Connection;
using DataBase.Mappers;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataBase.Repository.Access
{
    public class TrainerRequestRepository : ITrainerRequestRepository
    {
        private readonly IDataBaseConnection _database;

        public TrainerRequestRepository() : this(SqliteDatabaseConnection.CreateDefault())
        {
        }

        public TrainerRequestRepository(IDataBaseConnection database)
        {
            _database = database ?? SqliteDatabaseConnection.CreateDefault();
        }

        private int? GetTraineeTableId(int userId)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM TraineesTbl WHERE UserId = ?", userId);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Id"]) : (int?)null;
        }

        private int? GetTrainerTableId(int userId)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM TrainersTbl WHERE UserId = ?", userId);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Id"]) : (int?)null;
        }

        public string GetTrainerRequestStatus(int traineeUserId, int trainerUserId)
        {
            var traineeId = GetTraineeTableId(traineeUserId);
            var trainerId = GetTrainerTableId(trainerUserId);
            if (traineeId == null || trainerId == null) return null;

            var dt = _database.ExecuteQuery(
                "SELECT Status FROM TrainerRequestsTbl WHERE TraineeUserId = ? AND TrainerUserId = ?",
                traineeId.Value, trainerId.Value);
            return dt.Rows.Count > 0 ? dt.Rows[0]["Status"].ToString() : null;
        }

        public bool SendTrainerRequest(int traineeUserId, int trainerUserId)
        {
            var traineeId = GetTraineeTableId(traineeUserId);
            var trainerId = GetTrainerTableId(trainerUserId);
            if (traineeId == null || trainerId == null) return false;

            if (GetTrainerRequestStatus(traineeUserId, trainerUserId) != null) return false;

            int affected = _database.ExecuteNonQuery(
                "INSERT INTO TrainerRequestsTbl (TraineeUserId, TrainerUserId, Status, RequestDate) VALUES (?, ?, ?, ?)",
                traineeId.Value, trainerId.Value, "Pending", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return affected > 0;
        }

        public bool HandleTrainerRequest(int traineeUserId, int trainerUserId, string status)
        {
            var traineeId = GetTraineeTableId(traineeUserId);
            var trainerId = GetTrainerTableId(trainerUserId);
            if (traineeId == null || trainerId == null) return false;

            int affected = _database.ExecuteNonQuery(
                "UPDATE TrainerRequestsTbl SET Status = ? WHERE TraineeUserId = ? AND TrainerUserId = ?",
                status, traineeId.Value, trainerId.Value);

            if (affected > 0 && status == "Approved")
            {
                _database.ExecuteNonQuery("UPDATE TraineesTbl SET TrainerId = ? WHERE UserId = ?", trainerId.Value, traineeUserId);
            }

            return affected > 0;
        }

        public List<Trainee> GetPendingRequests(int trainerUserId)
        {
            var trainerId = GetTrainerTableId(trainerUserId);
            if (trainerId == null) return new List<Trainee>();

            var dt = _database.ExecuteQuery(
                @"SELECT u.*, t.TrainerId, t.FitnessGoal, t.CurrentWeight, t.Height
                  FROM (UserTbl u 
                  INNER JOIN TraineesTbl t ON u.Id = t.UserId)
                  INNER JOIN TrainerRequestsTbl tr ON t.Id = tr.TraineeUserId
                  WHERE tr.TrainerUserId = ? AND tr.Status = 'Pending'",
                trainerId.Value);

            var trainees = new List<Trainee>();
            foreach (DataRow row in dt.Rows)
            {
                trainees.Add(UserMapper.MapTrainee(row));
            }

            return trainees;
        }
    }
}
