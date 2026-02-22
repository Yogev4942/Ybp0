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
    public class AccessTraineeRepository : ITraineeRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessTraineeRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public Trainee GetTraineeById(int userId)
        {
            string query = @"
                SELECT u.*, t.TrainerId, t.FitnessGoal, t.CurrentWeight, t.Height
                FROM UserTbl u
                INNER JOIN TraineesTbl t ON u.Id = t.UserId
                WHERE u.Id = ?";

            var dt = _database.ExecuteQuery(query, userId);
            return dt.Rows.Count > 0 ? UserMapper.MapTrainee(dt.Rows[0]) : null;
        }

        public List<Trainee> GetTraineesByTrainerId(int trainerId)
        {
            string query = @"
                SELECT u.*, t.TrainerId, t.FitnessGoal, t.CurrentWeight, t.Height
                FROM UserTbl u
                INNER JOIN TraineesTbl t ON u.Id = t.UserId
                WHERE t.TrainerId = ?";

            var dt = _database.ExecuteQuery(query, trainerId);
            var trainees = new List<Trainee>();
            foreach (DataRow row in dt.Rows)
            {
                trainees.Add(UserMapper.MapTrainee(row));
            }
            return trainees;
        }

        public bool CreateTraineeProfile(int userId, string fitnessGoal, double currentWeight, double height)
        {
            int affected = _database.ExecuteNonQuery(
                "INSERT INTO TraineesTbl ([UserId], [TrainerId], [FitnessGoal], [CurrentWeight], [Height]) VALUES (?, NULL, ?, ?, ?)",
                userId, fitnessGoal, currentWeight, height
            );
            return affected > 0;
        }

        public bool UpdateTraineeProfile(Trainee trainee)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE TraineesTbl SET FitnessGoal = ?, CurrentWeight = ?, Height = ? WHERE UserId = ?",
                trainee.FitnessGoal, trainee.CurrentWeight, trainee.Height, trainee.Id
            );
            return affected > 0;
        }

        public bool AssignTrainer(int traineeUserId, int trainerId)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE TraineesTbl SET TrainerId = ? WHERE UserId = ?",
                trainerId, traineeUserId
            );
            return affected > 0;
        }
    }
}
