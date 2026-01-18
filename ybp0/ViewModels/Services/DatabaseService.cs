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
        private AccessDatabaseConnection _database;

        public DatabaseService()
        {
            _database = new AccessDatabaseConnection();
        }

        #region LOGIN
        public bool ValidateLogin(string username, string password)
        {
            var result = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );

            return result.Rows.Count > 0;
        }
        public User GetUserById(int userId)
        {
            var userDt = _database.ExecuteQuery("SELECT * FROM UserTbl WHERE Id = ?", userId);
            if (userDt.Rows.Count == 0) return null;
            return MapUser(userDt.Rows[0]);
        }
        public User GetUserByUsernameAndPassword(string username, string password)
        {
            var userDt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? AND Password = ?",
                username, password
            );

            if (userDt.Rows.Count == 0) return null;
            return MapUser(userDt.Rows[0]);
        }

        private User MapUser(System.Data.DataRow userRow)
        {
            int userId = Convert.ToInt32(userRow["Id"]);
            bool isTrainer = userRow["IsTrainer"] != DBNull.Value && Convert.ToBoolean(userRow["IsTrainer"]);

            if (isTrainer)
            {
                var trainer = new Trainer
                {
                    Id = userId,
                    Username = userRow["Username"].ToString(),
                    Email = userRow["Email"]?.ToString(),
                    Password = userRow["Password"].ToString(),
                    Joindate = userRow["JoinDate"]?.ToString(),
                    Bio = userRow["Bio"]?.ToString(),
                    Gender = userRow["Gender"]?.ToString(),
                    IsTrainer = true
                };

                var trainerDt = _database.ExecuteQuery("SELECT * FROM TrainersTbl WHERE UserId = ?", userId);
                if (trainerDt.Rows.Count > 0)
                {
                    var trainerRow = trainerDt.Rows[0];
                    trainer.Specialization = trainerRow["Specialization"]?.ToString();
                    trainer.HourlyRate = trainerRow["HourlyRate"] != DBNull.Value ? Convert.ToDouble(trainerRow["HourlyRate"]) : 0;
                    trainer.MaxTrainees = trainerRow["MaxTrainees"] != DBNull.Value ? Convert.ToInt32(trainerRow["MaxTrainees"]) : 10;
                    trainer.TotalTrainees = trainerRow["TotalTrainees"] != DBNull.Value ? Convert.ToInt32(trainerRow["TotalTrainees"]) : 0;
                    trainer.Rating = trainerRow["Rating"] != DBNull.Value ? Convert.ToDouble(trainerRow["Rating"]) : 0;
                    trainer.TotalRatings = trainerRow["TotalRatings"] != DBNull.Value ? Convert.ToInt32(trainerRow["TotalRatings"]) : 0;
                }
                return trainer;
            }
            else
            {
                var trainee = new Trainee
                {
                    Id = userId,
                    Username = userRow["Username"].ToString(),
                    Email = userRow["Email"]?.ToString(),
                    Password = userRow["Password"].ToString(),
                    Joindate = userRow["JoinDate"]?.ToString(),
                    Bio = userRow["Bio"]?.ToString(),
                    Gender = userRow["Gender"]?.ToString(),
                    IsTrainer = false
                };

                var traineeDt = _database.ExecuteQuery("SELECT * FROM TraineesTbl WHERE UserId = ?", userId);
                if (traineeDt.Rows.Count > 0)
                {
                    var traineeRow = traineeDt.Rows[0];
                    trainee.TrainerId = traineeRow["TrainerId"] != DBNull.Value ? Convert.ToInt32(traineeRow["TrainerId"]) : (int?)null;
                    trainee.FitnessGoal = traineeRow["FitnessGoal"]?.ToString();
                    trainee.CurrentWeight = traineeRow["CurrentWeight"] != DBNull.Value ? Convert.ToDouble(traineeRow["CurrentWeight"]) : 0;
                    trainee.Height = traineeRow["Height"] != DBNull.Value ? Convert.ToDouble(traineeRow["Height"]) : 0;
                    trainee.CurrentWeekPlanId = traineeRow["CurrentWeekPlanId"] != DBNull.Value ? Convert.ToInt32(traineeRow["CurrentWeekPlanId"]) : 0;
                }
                return trainee;
            }
        }
        public bool UserExist(string username, string email)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM UserTbl WHERE Username = ? OR Email = ?",
                username, email
            );
            return dt.Rows.Count > 0;
        }
        #endregion
        #region REGISTER
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
                // 1. Insert into UserTbl
                int affectedRows = _database.ExecuteNonQuery(
                    "INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer]) VALUES (?, ?, ?, ?, ?)",
                    username,
                    email,
                    password,
                    DateTime.Now.ToString("yyyy-MM-dd"),
                    0
                );

                if (affectedRows == 0) return false;

                // 2. Small delay to ensure Access DB commits the record
                System.Threading.Thread.Sleep(100);

                // 3. Query back the full user record to get the actual ID
                var userDt = _database.ExecuteQuery(
                    "SELECT Id FROM UserTbl WHERE Username = ? AND Email = ?",
                    username, email
                );

                if (userDt.Rows.Count == 0) return false;
                int userId = Convert.ToInt32(userDt.Rows[0]["Id"]);

                // 4. Create empty week plan
                int weekPlanId = CreateEmptyWeekPlan(userId, "My Week Plan");

                // 5. Insert into TraineesTbl
                _database.ExecuteNonQuery(
                    "INSERT INTO TraineesTbl ([UserId], [TrainerId], [FitnessGoal], [CurrentWeight], [Height], [CurrentWeekPlanId]) VALUES (?, ?, ?, ?, ?, ?)",
                    userId,
                    DBNull.Value,  // TrainerId - NULL when registering, will be set when trainer accepts request
                    fitnessGoal,
                    currentWeight,
                    height,
                    weekPlanId
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
                // 1. Insert into UserTbl
                int affectedRows = _database.ExecuteNonQuery(
                    "INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer]) VALUES (?, ?, ?, ?, ?)",
                    username,
                    email,
                    password,
                    DateTime.Now.ToString("yyyy-MM-dd"),
                    -1  // IsTrainer = true
                );

                if (affectedRows == 0) return false;

                // 2. Small delay to ensure Access DB commits the record
                System.Threading.Thread.Sleep(100);

                // 3. Query back the full user record to get the actual ID
                var userDt = _database.ExecuteQuery(
                    "SELECT Id FROM UserTbl WHERE Username = ? AND Email = ?",
                    username, email
                );

                if (userDt.Rows.Count == 0) return false;
                int userId = Convert.ToInt32(userDt.Rows[0]["Id"]);

                // 4. Insert into TrainersTbl
                _database.ExecuteNonQuery(
                    "INSERT INTO TrainersTbl ([UserId], [Specialization], [HourlyRate], [MaxTrainees]) VALUES (?, ?, ?, ?)",
                    userId,
                    specialization,
                    hourlyRate,
                    maxTrainees
                );

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainer error: {ex.Message}");
                return false;
            }
        }
        #endregion

        public bool UpdateUser(User user)
        {
            try
            {
                // 1. Update Common Fields in UserTbl
                string updateCommonQuery = "UPDATE UserTbl SET Bio = ?, Email = ? WHERE Id = ?";
                int commonRows = _database.ExecuteNonQuery(updateCommonQuery, user.Bio, user.Email, user.Id);

                if (commonRows == 0) return false;

                // 2. Update Type-Specific Fields
                if (user is Trainee trainee)
                {
                    string updateTraineeQuery = "UPDATE TraineesTbl SET FitnessGoal = ?, CurrentWeight = ?, Height = ? WHERE UserId = ?";
                    _database.ExecuteNonQuery(updateTraineeQuery, 
                        trainee.FitnessGoal, 
                        trainee.CurrentWeight, 
                        trainee.Height, 
                        user.Id);
                }
                else if (user is Trainer trainer)
                {
                    string updateTrainerQuery = "UPDATE TrainersTbl SET Specialization = ?, HourlyRate = ?, MaxTrainees = ? WHERE UserId = ?";
                    _database.ExecuteNonQuery(updateTrainerQuery, 
                        trainer.Specialization, 
                        trainer.HourlyRate, 
                        trainer.MaxTrainees, 
                        user.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user error: {ex.Message}");
                return false;
            }
        }

        #region SessionManagement
        public WorkoutSession GetOrCreateWorkoutSession(int userId, int weekPlanDayId, DateTime date)
        {
            // Try to find existing session
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionTbl WHERE UserId = ? AND WeekPlanDayId = ? AND SessionDate = ?",
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
                    SessionDate = Convert.ToDateTime(row["SessionDate"])
                };
            }

            // Get WorkoutId from WeekPlanDay
            var dayDt = _database.ExecuteQuery("SELECT WorkoutId FROM WeekPlanDaysTbl WHERE Id = ?", weekPlanDayId);
            if (dayDt.Rows.Count == 0) return null;

            int workoutId;
            if (dayDt.Rows[0]["WorkoutId"] != DBNull.Value)
            {
                workoutId = Convert.ToInt32(dayDt.Rows[0]["WorkoutId"]);
            }
            else
            {
                // No workout assigned to this day (Rest day or empty)
                // Create a new ad-hoc workout for this session
                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutsTbl (UserId, WorkoutName) VALUES (?, ?)",
                    userId, "Ad-hoc Workout"
                );

                // Get the ID of the new workout
                var wDt = _database.ExecuteQuery("SELECT TOP 1 Id FROM WorkoutsTbl WHERE UserId = ? ORDER BY Id DESC", userId);
                workoutId = Convert.ToInt32(wDt.Rows[0]["Id"]);
            }

            // Create new session
            _database.ExecuteNonQuery(
                "INSERT INTO WorkoutSessionTbl ([UserId], [WorkoutId], [WeekPlanDayId], [SessionDate]) VALUES (?, ?, ?, ?)",
                userId, workoutId, weekPlanDayId, date.Date
            );

            // Get the created session
            var newDt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionTbl WHERE UserId = ? AND WeekPlanDayId = ? AND SessionDate = ?",
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
                    SessionDate = Convert.ToDateTime(row["SessionDate"])
                };
            }

            return null;
        }

        public List<Exercise> GetSessionExercises(int workoutSessionId)
        {
            // Query ACTUAL session sets (where ad-hoc exercises are stored)
            // NOT workout templates
            var dt = _database.ExecuteQuery(
                @"SELECT DISTINCT e.Id, e.ExerciseName, e.MuscleGroup 
                  FROM ExercisesTbl e 
                  INNER JOIN WorkoutSessionSetsTbl wss ON e.Id = wss.ExerciseId 
                  WHERE wss.WorkoutSessionId = ?",
                workoutSessionId
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
            try 
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
            }
            catch (Exception)
            {
                // In a real app log this
                return new List<SessionSet>();
            }

            // No session sets found - copy from template
            var sessionDt = _database.ExecuteQuery("SELECT WorkoutId FROM WorkoutSessionTbl WHERE Id = ?", workoutSessionId);
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
        #endregion
        #region ExerciseManagement
        public List<Exercise> GetAllExercises()
        {
            var dt = _database.ExecuteQuery("SELECT * FROM ExercisesTbl ORDER BY ExerciseName");
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

            // Auto-seed if empty
            if (exercises.Count == 0)
            {
                SeedExercises();
                // Recursively call to get the seeded exercises
                return GetAllExercises();
            }

            return exercises;
        }
        private void SeedExercises()
        {
            //demo
            var defaultExercises = new List<(string Name, string Muscle)>
            {
                ("Bench Press", "Chest"),
                ("Squat", "Legs"),
                ("Deadlift", "Back"),
                ("Overhead Press", "Shoulders"),
                ("Pull Up", "Back"),
                ("Dumbbell Row", "Back"),
                ("Lunges", "Legs"),
                ("Push Up", "Chest"),
                ("Bicep Curl", "Arms"),
                ("Tricep Extension", "Arms"),
                ("Plank", "Core"),
                ("Crunches", "Core"),
                ("Leg Press", "Legs"),
                ("Lat Pulldown", "Back"),
                ("Shoulder Fly", "Shoulders")
            };

            foreach (var ex in defaultExercises)
            {
                _database.ExecuteNonQuery(
                    "INSERT INTO ExercisesTbl (ExerciseName, MuscleGroup) VALUES (?, ?)",
                    ex.Name, ex.Muscle
                );
            }
        }
        #endregion
        #region WorkoutManagement
        public void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId)
        {
            // Check if this exercise already exists in session sets
            var existingDt = _database.ExecuteQuery(
                "SELECT Id FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId, exerciseId
            );

            if (existingDt.Rows.Count == 0)
            {
                // Add a default first set with 0 reps and 0 weight
                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutSessionSetsTbl ([WorkoutSessionId], [ExerciseId], [SetNumber], [Reps], [Weight]) VALUES (?, ?, ?, ?, ?)",
                    workoutSessionId, exerciseId, 1, 0, 0.0
                );
            }
        }
        public void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId)
        {
            // Delete all sets for this exercise in this session
            _database.ExecuteNonQuery(
                "DELETE FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId, exerciseId
            );
        }
        public int? GetWeekPlanOwnerUserId(int weekPlanId)
        {
            // Get the UserId from TraineesTbl where CurrentWeekPlanId matches
            var dt = _database.ExecuteQuery(
                "SELECT UserId FROM TraineesTbl WHERE CurrentWeekPlanId = ?",
                weekPlanId
            );

            if (dt.Rows.Count > 0)
            {
                return Convert.ToInt32(dt.Rows[0]["UserId"]);
            }
            return null;
        }
        public List<Trainee> GetTraineesByTrainerId(int trainerId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT u.*, t.* 
                  FROM UserTbl u 
                  INNER JOIN TraineesTbl t ON u.Id = t.UserId
                  WHERE t.TrainerId = ?",
                trainerId
            );

            var trainees = new List<Trainee>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                trainees.Add(new Trainee
                {
                    Id = Convert.ToInt32(row["UserId"]),
                    Username = row["Username"]?.ToString(),
                    Email = row["Email"]?.ToString(),
                    CurrentWeekPlanId = row["CurrentWeekPlanId"] != DBNull.Value ?
                        Convert.ToInt32(row["CurrentWeekPlanId"]) : 0
                });
            }
            return trainees;
        }
        public int? GetUserWeekPlanId(int userId)
        {
            var dt = _database.ExecuteQuery(
                "SELECT Id FROM WeekPlansTbl WHERE UserId = ?",
                userId
            );

            if (dt.Rows.Count == 0)
                return null;

            return Convert.ToInt32(dt.Rows[0]["Id"]);
        }
        public int CreateEmptyWeekPlan(int userId, string planName)
        {
            try
            {
                // Insert new weekplan
                _database.ExecuteNonQuery(
                    "INSERT INTO WeekPlansTbl (UserId, PlanName) VALUES (?, ?)",
                    userId,
                    planName
                );

                // Small delay to ensure Access DB commits the record
                System.Threading.Thread.Sleep(100);

                // Query back to get the newly created weekplan ID
                var weekPlanDt = _database.ExecuteQuery(
                    "SELECT Id FROM WeekPlansTbl WHERE UserId = ? AND PlanName = ?",
                    userId,
                    planName
                );

                if (weekPlanDt.Rows.Count == 0) return 0;
                int weekPlanId = Convert.ToInt32(weekPlanDt.Rows[0]["Id"]);

                // Create 7 empty days (Sunday = 0 to Saturday = 6)
                for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
                {
                    _database.ExecuteNonQuery(
                        "INSERT INTO WeekPlanDaysTbl (WeekplanId, DayOfWeek, WorkoutId, RestDay) VALUES (?, ?, NULL, ?)",
                        weekPlanId,
                        dayOfWeek,
                        false
                    );
                }

                return weekPlanId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create week plan error: {ex.Message}");
                throw;
            }
        }
        #endregion
        #region TrainerRequestManagement
        public string GetTrainerRequestStatus(int traineeId, int trainerId)
        {
            var dt = _database.ExecuteQuery(
                "SELECT Status FROM TrainerRequestsTbl WHERE TraineeUserId = ? AND TrainerUserId = ?",
                traineeId, trainerId
            );
            if (dt.Rows.Count > 0)
                return dt.Rows[0]["Status"].ToString();
            return null;
        }

        public bool SendTrainerRequest(int traineeId, int trainerId)
        {
            // Check if already exists
            var status = GetTrainerRequestStatus(traineeId, trainerId);
            if (status != null) return false;

            int affected = _database.ExecuteNonQuery(
                "INSERT INTO TrainerRequestsTbl (TraineeUserId, TrainerUserId, Status, RequestDate) VALUES (?, ?, ?, ?)",
                traineeId, trainerId, "Pending", DateTime.Now
            );
            return affected > 0;
        }

        public bool HandleTrainerRequest(int traineeId, int trainerId, string status)
        {
            int affected = _database.ExecuteNonQuery(
                "UPDATE TrainerRequestsTbl SET Status = ? WHERE TraineeUserId = ? AND TrainerUserId = ?",
                status, traineeId, trainerId
            );

            if (affected > 0 && status == "Approved")
            {
                // Link trainee to trainer in TraineesTbl
                _database.ExecuteNonQuery(
                    "UPDATE TraineesTbl SET TrainerId = ? WHERE UserId = ?",
                    trainerId, traineeId
                );
            }
            return affected > 0;
        }
        public List<Trainee> GetPendingRequests(int trainerId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT u.*, t.* 
                  FROM (UserTbl u 
                  INNER JOIN TraineesTbl t ON u.Id = t.UserId)
                  INNER JOIN TrainerRequestsTbl tr ON u.Id = tr.TraineeUserId
                  WHERE tr.TrainerUserId = ? AND tr.Status = 'Pending'",
                trainerId
            );

            var trainees = new List<Trainee>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                var trainee = (Trainee)MapUser(row);
                trainees.Add(trainee);
            }
            return trainees;
        }
        #endregion
    }
}
