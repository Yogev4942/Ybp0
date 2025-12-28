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
            var result = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );

            return result.Rows.Count > 0;
        }
        public User GetUserByUsernameAndPassword(string username, string password)
        {
            // Get user from UserTbl
            var userDt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );

            if (userDt.Rows.Count == 0)
                return null;

            var userRow = userDt.Rows[0];
            int userId = Convert.ToInt32(userRow["Id"]);
            bool isTrainer = userRow["IsTrainer"] != DBNull.Value && Convert.ToBoolean(userRow["IsTrainer"]);

            User user;

            if (isTrainer)
            {
                // Create Trainer object
                var trainer = new Trainer
                {
                    Id = userId,
                    Username = userRow["Username"].ToString(),
                    Email = userRow["Email"]?.ToString(),
                    Password = userRow["Password"].ToString(),
                    Joindate = userRow["JoinDate"]?.ToString(),
                    Bio = userRow["Bio"]?.ToString(),
                    Birthdate = userRow["BirthDate"]?.ToString(),
                    Gender = userRow["Gender"]?.ToString(),
                    IsTrainer = true
                };

                // Get trainer-specific data from TrainersTbl
                var trainerDt = _database.ExecuteQuery(
                    "SELECT * FROM TrainersTbl WHERE UserId = ?",
                    userId
                );

                if (trainerDt.Rows.Count > 0)
                {
                    var trainerRow = trainerDt.Rows[0];
                    trainer.Specialization = trainerRow["Specialization"]?.ToString();
                    trainer.HourlyRate = trainerRow["HourlyRate"] != DBNull.Value ?
                        Convert.ToDouble(trainerRow["HourlyRate"]) : 0;
                    trainer.MaxTrainees = trainerRow["MaxTrainees"] != DBNull.Value ?
                        Convert.ToInt32(trainerRow["MaxTrainees"]) : 10;
                    trainer.TotalTrainees = trainerRow["TotalTrainees"] != DBNull.Value ?
                        Convert.ToInt32(trainerRow["TotalTrainees"]) : 0;
                    trainer.Rating = trainerRow["Rating"] != DBNull.Value ?
                        Convert.ToDouble(trainerRow["Rating"]) : 0;
                    trainer.TotalRatings = trainerRow["TotalRatings"] != DBNull.Value ?
                        Convert.ToInt32(trainerRow["TotalRatings"]) : 0;
                }

                user = trainer;
            }
            else
            {
                // Create Trainee object
                var trainee = new Trainee
                {
                    Id = userId,
                    Username = userRow["Username"].ToString(),
                    Email = userRow["Email"]?.ToString(),
                    Password = userRow["Password"].ToString(),
                    Joindate = userRow["JoinDate"]?.ToString(),
                    Bio = userRow["Bio"]?.ToString(),
                    Birthdate = userRow["BirthDate"]?.ToString(),
                    Gender = userRow["Gender"]?.ToString(),
                    IsTrainer = false
                };

                // Get trainee-specific data from TraineesTbl
                var traineeDt = _database.ExecuteQuery(
                    "SELECT * FROM TraineesTbl WHERE UserId = ?",
                    userId
                );

                if (traineeDt.Rows.Count > 0)
                {
                    var traineeRow = traineeDt.Rows[0];
                    trainee.TrainerId = traineeRow["TrainerId"] != DBNull.Value ?
                        Convert.ToInt32(traineeRow["TrainerId"]) : (int?)null;
                    trainee.FitnessGoal = traineeRow["FitnessGoal"]?.ToString();
                    trainee.CurrentWeight = traineeRow["CurrentWeight"] != DBNull.Value ?
                        Convert.ToDouble(traineeRow["CurrentWeight"]) : 0;
                    trainee.Height = traineeRow["Height"] != DBNull.Value ?
                        Convert.ToDouble(traineeRow["Height"]) : 0;
                    trainee.CurrentWeekPlanId = traineeRow["CurrentWeekPlanId"] != DBNull.Value ?
                        Convert.ToInt32(traineeRow["CurrentWeekPlanId"]) : 0;
                }

                user = trainee;
            }

            return user;
        }
        public bool UserExist(string username, string email)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? OR Email = ?",
                username, email
            );
            return dt.Rows.Count > 0;
        }
        public bool RegisterUser(string username, string email, string password)
        {
            try
            {
                int affectedRows = _database.ExecuteNonQuery(
                    "INSERT INTO [UserTbl] ([Username], [Email], [Password], [JoinDate], [IsTrainer]) VALUES (?, ?, ?, ?, ?)",
                    username,
                    email,
                    password,
                    DateTime.Today,
                    0
                );

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                // Optional: log error
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return false;
            }
        }

        public bool RegisterTrainee(string username, string email, string password,
                                   string fitnessGoal, double currentWeight, double height)
        {
            try
            {
                // Insert into UserTbl
                _database.ExecuteNonQuery(
                    @"INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer]) 
                      VALUES (?, ?, ?, ?, ?)",
                    username, email, password, DateTime.Now, false
                );

                // Get the new user ID
                var dt = _database.ExecuteQuery(
                    "SELECT TOP 1 Id FROM UserTbl WHERE Username = ? ORDER BY Id DESC",
                    username
                );

                if (dt.Rows.Count == 0)
                    return false;

                int userId = Convert.ToInt32(dt.Rows[0]["Id"]);

                // Insert into TraineesTbl
                _database.ExecuteNonQuery(
                    @"INSERT INTO TraineesTbl ([UserId], [FitnessGoal], [CurrentWeight], [Height], [IsActive]) 
                      VALUES (?, ?, ?, ?, ?)",
                    userId, fitnessGoal, currentWeight, height, true
                );

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainee error: {ex.Message}");
                return false;
            }
        }

        public bool RegisterTrainer(string username, string email, string password,
                                   string specialization, double hourlyRate, int maxTrainees)
        {
            try
            {
                // Insert into UserTbl
                _database.ExecuteNonQuery(
                    @"INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer]) 
                      VALUES (?, ?, ?, ?, ?)",
                    username, email, password, DateTime.Now, true
                );

                // Get the new user ID
                var dt = _database.ExecuteQuery(
                    "SELECT TOP 1 Id FROM UserTbl WHERE Username = ? ORDER BY Id DESC",
                    username
                );

                if (dt.Rows.Count == 0)
                    return false;

                int userId = Convert.ToInt32(dt.Rows[0]["Id"]);

                // Insert into TrainersTbl
                _database.ExecuteNonQuery(
                    @"INSERT INTO TrainersTbl ([UserId], [Specialization], [HourlyRate], [MaxTrainees], [TotalTrainees], [Rating], [TotalRatings]) 
                      VALUES (?, ?, ?, ?, ?, ?, ?)",
                    userId, specialization, hourlyRate, maxTrainees, 0, 0.0, 0
                );

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainer error: {ex.Message}");
                return false;
            }
        }

        // Session Management
        public WorkoutSession GetOrCreateWorkoutSession(int userId, int weekPlanDayId, DateTime date)
        {
            // Try to find existing session
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionsTbl WHERE UserId = ? AND WeekPlanDayId = ? AND SessionDate = ?",
                userId, weekPlanDayId, date.Date
            );

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new WorkoutSession
                {
                    Id = Convert.ToInt32(row["Id"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    WorkoutId = Convert.ToInt32(row["WorkoutId"]),
                    WeekPlanDayId = row["WeekPlanDayId"] != DBNull.Value ? Convert.ToInt32(row["WeekPlanDayId"]) : (int?)null,
                    SessionDate = Convert.ToDateTime(row["SessionDate"]),
                    Completed = Convert.ToBoolean(row["Completed"])
                };
            }

            // Get WorkoutId from WeekPlanDay
            var dayDt = _database.ExecuteQuery("SELECT WorkoutId FROM WeekPlanDaysTbl WHERE Id = ?", weekPlanDayId);
            if (dayDt.Rows.Count == 0) return null;
            
            int workoutId = Convert.ToInt32(dayDt.Rows[0]["WorkoutId"]);

            // Create new session
            _database.ExecuteNonQuery(
                "INSERT INTO WorkoutSessionsTbl ([UserId], [WorkoutId], [WeekPlanDayId], [SessionDate], [Completed]) VALUES (?, ?, ?, ?, ?)",
                userId, workoutId, weekPlanDayId, date.Date, false
            );

            // Get the created session
            var newDt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionsTbl WHERE UserId = ? AND WeekPlanDayId = ? AND SessionDate = ?",
                userId, weekPlanDayId, date.Date
            );

            if (newDt.Rows.Count > 0)
            {
                var row = newDt.Rows[0];
                return new WorkoutSession
                {
                    Id = Convert.ToInt32(row["Id"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    WorkoutId = Convert.ToInt32(row["WorkoutId"]),
                    WeekPlanDayId = row["WeekPlanDayId"] != DBNull.Value ? Convert.ToInt32(row["WeekPlanDayId"]) : (int?)null,
                    SessionDate = Convert.ToDateTime(row["SessionDate"]),
                    Completed = Convert.ToBoolean(row["Completed"])
                };
            }

            return null;
        }

        public void CompleteWorkoutSession(int sessionId)
        {
            _database.ExecuteNonQuery(
                "UPDATE WorkoutSessionsTbl SET Completed = ? WHERE Id = ?",
                true, sessionId
            );
        }

        public List<Exercise> GetSessionExercises(int workoutSessionId)
        {
            // Get workout from session
            var sessionDt = _database.ExecuteQuery("SELECT WorkoutId FROM WorkoutSessionsTbl WHERE Id = ?", workoutSessionId);
            if (sessionDt.Rows.Count == 0) return new List<Exercise>();

            int workoutId = Convert.ToInt32(sessionDt.Rows[0]["WorkoutId"]);

            // Get exercises for this workout
            var dt = _database.ExecuteQuery(
                @"SELECT e.* FROM ExercisesTbl e 
                  INNER JOIN WorkoutExercisesTbl we ON e.Id = we.ExerciseId 
                  WHERE we.WorkoutId = ?",
                workoutId
            );

            var exercises = new List<Exercise>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                exercises.Add(new Exercise
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ExerciseName = row["ExerciseName"].ToString(),
                    MuscleGroup = row["MuscleGroup"]?.ToString()
                });
            }
            return exercises;
        }

        public List<SessionSet> GetSessionSets(int workoutSessionId, int exerciseId)
        {
            // Check if session sets exist
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId, exerciseId
            );

            if (dt.Rows.Count > 0)
            {
                // Return existing session sets
                var sets = new List<SessionSet>();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    sets.Add(new SessionSet
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        WorkoutSessionId = Convert.ToInt32(row["WorkoutSessionId"]),
                        ExerciseId = Convert.ToInt32(row["ExerciseId"]),
                        SetNumber = Convert.ToInt32(row["SetNumber"]),
                        Reps = Convert.ToInt32(row["Reps"]),
                        Weight = Convert.ToDouble(row["Weight"])
                    });
                }
                return sets;
            }

            // No session sets found - copy from template
            var sessionDt = _database.ExecuteQuery("SELECT WorkoutId FROM WorkoutSessionsTbl WHERE Id = ?", workoutSessionId);
            if (sessionDt.Rows.Count == 0) return new List<SessionSet>();
            
            int workoutId = Convert.ToInt32(sessionDt.Rows[0]["WorkoutId"]);

            // Get WorkoutExerciseId
            var weDt = _database.ExecuteQuery(
                "SELECT Id FROM WorkoutExercisesTbl WHERE WorkoutId = ? AND ExerciseId = ?",
                workoutId, exerciseId
            );
            if (weDt.Rows.Count == 0) return new List<SessionSet>();
            
            int workoutExerciseId = Convert.ToInt32(weDt.Rows[0]["Id"]);

            // Get template sets
            var templateDt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ?",
                workoutExerciseId
            );

            var newSets = new List<SessionSet>();
            foreach (System.Data.DataRow row in templateDt.Rows)
            {
                int setNumber = Convert.ToInt32(row["SetNumber"]);
                int reps = Convert.ToInt32(row["Reps"]);
                double weight = Convert.ToDouble(row["Weight"]);

                // Insert into session sets
                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutSessionSetsTbl ([WorkoutSessionId], [ExerciseId], [SetNumber], [Reps], [Weight]) VALUES (?, ?, ?, ?, ?)",
                    workoutSessionId, exerciseId, setNumber, reps, weight
                );

                newSets.Add(new SessionSet
                {
                    WorkoutSessionId = workoutSessionId,
                    ExerciseId = exerciseId,
                    SetNumber = setNumber,
                    Reps = reps,
                    Weight = weight
                });
            }

            return newSets;
        }

        public void SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight)
        {
            // Check if set exists
            var dt = _database.ExecuteQuery(
                "SELECT Id FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ? AND SetNumber = ?",
                workoutSessionId, exerciseId, setNumber
            );

            if (dt.Rows.Count > 0)
            {
                // Update existing
                int id = Convert.ToInt32(dt.Rows[0]["Id"]);
                _database.ExecuteNonQuery(
                    "UPDATE WorkoutSessionSetsTbl SET Reps = ?, Weight = ? WHERE Id = ?",
                    reps, weight, id
                );
            }
            else
            {
                // Insert new
                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutSessionSetsTbl ([WorkoutSessionId], [ExerciseId], [SetNumber], [Reps], [Weight]) VALUES (?, ?, ?, ?, ?)",
                    workoutSessionId, exerciseId, setNumber, reps, weight
                );
            }
        }

        public void DeleteSessionSet(int setId)
        {
            _database.ExecuteNonQuery("DELETE FROM WorkoutSessionSetsTbl WHERE Id = ?", setId);
        }

        public System.Data.DataTable GetWeekPlanDays(int weekPlanId)
        {
            return _database.ExecuteQuery(
                @"SELECT wpd.Id, wpd.DayOfWeek, wpd.WorkoutId, wpd.RestDay, w.WorkoutName
                  FROM WeekPlanDaysTbl wpd
                  LEFT JOIN WorkoutsTbl w ON wpd.WorkoutId = w.Id
                  WHERE wpd.WeekPlanId = ?",
                weekPlanId
            );
        }

    }
}
