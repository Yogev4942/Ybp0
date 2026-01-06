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
            InitializeDatabase();
        }
        private void InitializeDatabase()
        {
            // Ensure tables exist (Access Schema)
            
            // 1. UserTbl
            if (!_database.TableExists("UserTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE UserTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        Username TEXT(255) NOT NULL UNIQUE,
                        Password TEXT(255) NOT NULL,
                        Email TEXT(255) NOT NULL UNIQUE,
                        JoinDate DATETIME DEFAULT Now(),
                        IsTrainer BIT DEFAULT 0,
                        ProfilePicture TEXT(255),
                        Bio MEMO,
                        Age INTEGER,
                        Gender TEXT(50)
                    )");
            }

            // 2. ExercisesTbl
            if (!_database.TableExists("ExercisesTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE ExercisesTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        ExerciseName TEXT(255) NOT NULL,
                        MuscleGroup TEXT(255)
                    )");
                // Seed will happen on first get
            }

            // 3. WorkoutsTbl
            if (!_database.TableExists("WorkoutsTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE WorkoutsTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        UserId INTEGER NOT NULL,
                        WorkoutName TEXT(255) NOT NULL,
                        CreatedByTrainerId INTEGER
                    )");
            }

            // 4. WorkoutSessionTbl
            if (!_database.TableExists("WorkoutSessionTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE WorkoutSessionTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        UserId INTEGER NOT NULL,
                        WorkoutId INTEGER NOT NULL,
                        WeekPlanDayId INTEGER,
                        SessionDate DATETIME NOT NULL,
                        Completed BIT DEFAULT 0
                    )");
            }
            else
            {
                // Check if 'Completed' column exists (Migration for existing DBs)
                if (!_database.ColumnExists("WorkoutSessionTbl", "Completed"))
                {
                    _database.ExecuteNonQuery("ALTER TABLE WorkoutSessionTbl ADD COLUMN Completed BIT DEFAULT 0");
                }
            }

            // 5. WorkoutSessionSetsTbl
            if (!_database.TableExists("WorkoutSessionSetsTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE WorkoutSessionSetsTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        WorkoutSessionId INTEGER NOT NULL,
                        ExerciseId INTEGER NOT NULL,
                        SetNumber INTEGER NOT NULL,
                        Reps INTEGER DEFAULT 0,
                        Weight DOUBLE DEFAULT 0
                    )");
            }

            // 6. WeekPlansTbl
             if (!_database.TableExists("WeekPlansTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE WeekPlansTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        UserId INTEGER NOT NULL,
                        PlanName TEXT(255) NOT NULL
                    )");
            }

            // 7. WeekPlanDaysTbl
            if (!_database.TableExists("WeekPlanDaysTbl"))
            {
                 _database.ExecuteNonQuery(@"
                    CREATE TABLE WeekPlanDaysTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        WeekPlanId INTEGER NOT NULL,
                        DayOfWeek INTEGER NOT NULL,
                        WorkoutId INTEGER,
                        RestDay BIT DEFAULT 0
                    )");
            }
             
             // 8. WorkoutExercisesTbl (Template - might be needed)
             if (!_database.TableExists("WorkoutExercisesTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE WorkoutExercisesTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        WorkoutId INTEGER NOT NULL,
                        ExerciseId INTEGER NOT NULL
                    )");
            }
             
             // 9. WorkoutSetsTbl (Template)
             if (!_database.TableExists("WorkoutSetsTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE WorkoutSetsTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        WorkoutExerciseId INTEGER NOT NULL,
                        SetNumber INTEGER NOT NULL,
                        Reps INTEGER DEFAULT 10,
                        Weight DOUBLE DEFAULT 0
                    )");
            }

             // 10. TraineesTbl
             if (!_database.TableExists("TraineesTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE TraineesTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        UserId INTEGER NOT NULL UNIQUE,
                        TrainerId INTEGER,
                        FitnessGoal MEMO,
                        CurrentWeight DOUBLE DEFAULT 0,
                        Height DOUBLE DEFAULT 0,
                        ActivityLevel TEXT(255),
                        CurrentWeekPlanId INTEGER,
                        IsActive BIT DEFAULT 1,
                        Notes MEMO
                    )");
            }
             
             // 11. TrainersTbl
             if (!_database.TableExists("TrainersTbl"))
            {
                _database.ExecuteNonQuery(@"
                    CREATE TABLE TrainersTbl (
                        Id AUTOINCREMENT PRIMARY KEY,
                        UserId INTEGER NOT NULL UNIQUE,
                        Specialization TEXT(255),
                        YearsOfExperience INTEGER DEFAULT 0,
                        Certifications MEMO,
                        HourlyRate DOUBLE DEFAULT 0,
                        MaxTrainees INTEGER DEFAULT 10,
                        WorkingHours MEMO,
                        Expertise MEMO,
                        TotalTrainees INTEGER DEFAULT 0,
                        Rating DOUBLE DEFAULT 0,
                        TotalRatings INTEGER DEFAULT 0
                    )");
            }
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
                // Insert into UserTbl only - simplified for now
                // JoinDate is formatted as string for Access compatibility
                string joinDateStr = DateTime.Now.ToString("yyyy-MM-dd");
                
                _database.ExecuteNonQuery(
                    @"INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer]) 
                      VALUES (?, ?, ?, ?, ?)",
                    username, email, password, joinDateStr, 0
                );

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainee error: {ex.Message}");
                throw; // Re-throw to see the actual error
            }
        }
        public bool RegisterTrainer(string username, string email, string password,
                                   string specialization, double hourlyRate, int maxTrainees)
        {
            try
            {
                // Insert into UserTbl only - simplified for now
                // JoinDate is formatted as string for Access compatibility
                string joinDateStr = DateTime.Now.ToString("yyyy-MM-dd");
                
                _database.ExecuteNonQuery(
                    @"INSERT INTO UserTbl ([Username], [Email], [Password], [JoinDate], [IsTrainer]) 
                      VALUES (?, ?, ?, ?, ?)",
                    username, email, password, joinDateStr, -1
                );

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainer error: {ex.Message}");
                throw; // Re-throw to see the actual error
            }
        }
        #endregion
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
                    SessionDate = Convert.ToDateTime(row["SessionDate"]),
                    Completed = Convert.ToBoolean(row["Completed"])
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
                "INSERT INTO WorkoutSessionTbl ([UserId], [WorkoutId], [WeekPlanDayId], [SessionDate], [Completed]) VALUES (?, ?, ?, ?, ?)",
                userId, workoutId, weekPlanDayId, date.Date, false
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
                    SessionDate = Convert.ToDateTime(row["SessionDate"]),
                    Completed = Convert.ToBoolean(row["Completed"])
                };
            }

            return null;
        }
        public void CompleteWorkoutSession(int sessionId)
        {
            _database.ExecuteNonQuery(
                "UPDATE WorkoutSessionTbl SET Completed = ? WHERE Id = ?",
                true, sessionId
            );
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
            // Insert new weekplan
            _database.ExecuteNonQuery(
                "INSERT INTO WeekPlansTbl (UserId, PlanName) VALUES (?, ?)",
                userId, planName
            );

            // Get the newly created weekplan ID
            var dt = _database.ExecuteQuery(
                "SELECT TOP 1 Id FROM WeekPlansTbl WHERE UserId = ? ORDER BY Id DESC",
                userId
            );

            int weekPlanId = Convert.ToInt32(dt.Rows[0]["Id"]);

            // Create 7 empty days (Sunday = 0, Saturday = 6)
            for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
            {
                _database.ExecuteNonQuery(
                    "INSERT INTO WeekPlanDaysTbl (WeekplanId, DayOfWeek, WorkoutId, RestDay) VALUES (?, ?, NULL, ?)",
                    weekPlanId, dayOfWeek, false
                );
            }

            return weekPlanId;
        }
        #endregion
    }
}
