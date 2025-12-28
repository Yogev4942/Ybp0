using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class OtherDatabase
    {
        private readonly string dbPath;
        private readonly string connectionString;


        public OtherDatabase(string databaseFile)
        {
            // Ensure the directory exists
            var folder = Path.GetDirectoryName(databaseFile);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Create connection string
            connectionString = $"Data Source={databaseFile}";

            // Initialize the database (create tables if missing)
            InitializeDatabase();
        }

        public OtherDatabase()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fitness_app.db");
            connectionString = $"Data Source={dbPath}";

            // Just open connection to create DB if missing
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
            }

            // Initialize tables
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Enable foreign keys
                using (var pragma = connection.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA foreign_keys = ON;";
                    pragma.ExecuteNonQuery();
                }

                string[] tableCommands =
                {
                    // ============================================
                    // BASE USER TABLE (common fields for all users)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS UserTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL,
                        Email TEXT NOT NULL UNIQUE,
                        JoinDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UserType TEXT NOT NULL CHECK(UserType IN ('Trainer', 'Trainee')),
                        ProfilePicture TEXT,
                        Bio TEXT,
                        Age INTEGER,
                        Gender TEXT
                    );",

                    // ============================================
                    // TRAINER-SPECIFIC TABLE
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS TrainersTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL UNIQUE,
                        Specialization TEXT,
                        YearsOfExperience INTEGER DEFAULT 0,
                        Certifications TEXT,
                        HourlyRate REAL DEFAULT 0,
                        MaxTrainees INTEGER DEFAULT 10,
                        WorkingHours TEXT,
                        Expertise TEXT,
                        TotalTrainees INTEGER DEFAULT 0,
                        Rating REAL DEFAULT 0,
                        TotalRatings INTEGER DEFAULT 0,
                        FOREIGN KEY (UserId) REFERENCES UserTbl(Id) ON DELETE CASCADE
                    );",

                    // ============================================
                    // TRAINEE-SPECIFIC TABLE
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS TraineesTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL UNIQUE,
                        TrainerId INTEGER,
                        FitnessGoal TEXT,
                        CurrentWeight REAL DEFAULT 0,
                        Height REAL DEFAULT 0,
                        ActivityLevel TEXT CHECK(ActivityLevel IN ('Sedentary', 'LightlyActive', 'ModeratelyActive', 'VeryActive')),
                        CurrentWeekPlanId INTEGER,
                        IsActive INTEGER DEFAULT 1,
                        Notes TEXT,
                        FOREIGN KEY (UserId) REFERENCES UserTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (TrainerId) REFERENCES TrainersTbl(Id) ON DELETE SET NULL,
                        FOREIGN KEY (CurrentWeekPlanId) REFERENCES WeekPlansTbl(Id) ON DELETE SET NULL
                    );",

                    // ============================================
                    // EXERCISES TABLE
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS ExercisesTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ExerciseName TEXT NOT NULL,
                        MuscleGroup TEXT
                    );",

                    // ============================================
                    // WORKOUTS TABLE (template workouts)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WorkoutsTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        WorkoutName TEXT NOT NULL,
                        Description TEXT,
                        CreatedByTrainerId INTEGER,
                        IsPublic INTEGER DEFAULT 0,
                        CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (UserId) REFERENCES UserTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (CreatedByTrainerId) REFERENCES TrainersTbl(Id) ON DELETE SET NULL
                    );",

                    // ============================================
                    // WORKOUT EXERCISES (which exercises are in a workout)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WorkoutExercisesTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        WorkoutId INTEGER NOT NULL,
                        ExerciseId INTEGER NOT NULL,
                        ExerciseOrder INTEGER DEFAULT 0,
                        FOREIGN KEY (WorkoutId) REFERENCES WorkoutsTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (ExerciseId) REFERENCES ExercisesTbl(Id) ON DELETE CASCADE
                    );",

                    // ============================================
                    // WORKOUT SETS (template sets for each exercise in workout)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WorkoutSetsTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        WorkoutExerciseId INTEGER NOT NULL,
                        SetNumber INTEGER NOT NULL,
                        Reps INTEGER DEFAULT 10,
                        Weight REAL DEFAULT 0,
                        FOREIGN KEY (WorkoutExerciseId) REFERENCES WorkoutExercisesTbl(Id) ON DELETE CASCADE
                    );",

                    // ============================================
                    // WEEK PLANS (weekly workout schedule)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WeekPlansTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        PlanName TEXT NOT NULL,
                        CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        IsActive INTEGER DEFAULT 1,
                        FOREIGN KEY (UserId) REFERENCES UserTbl(Id) ON DELETE CASCADE
                    );",

                    // ============================================
                    // WEEK PLAN DAYS (which workout on which day)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WeekPlanDaysTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        WeekPlanId INTEGER NOT NULL,
                        DayOfWeek INTEGER NOT NULL CHECK(DayOfWeek BETWEEN 0 AND 6),
                        WorkoutId INTEGER,
                        RestDay INTEGER DEFAULT 0,
                        FOREIGN KEY (WeekPlanId) REFERENCES WeekPlansTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (WorkoutId) REFERENCES WorkoutsTbl(Id) ON DELETE SET NULL
                    );",

                    // ============================================
                    // WORKOUT SESSIONS (actual workout instances)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WorkoutSessionsTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        WorkoutId INTEGER NOT NULL,
                        WeekPlanDayId INTEGER,
                        SessionDate DATETIME NOT NULL,
                        Completed INTEGER DEFAULT 0,
                        FOREIGN KEY (UserId) REFERENCES UserTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (WorkoutId) REFERENCES WorkoutsTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (WeekPlanDayId) REFERENCES WeekPlanDaysTbl(Id) ON DELETE SET NULL
                    );",

                    // ============================================
                    // WORKOUT SESSION SETS (actual sets performed)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS WorkoutSessionSetsTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        WorkoutSessionId INTEGER NOT NULL,
                        ExerciseId INTEGER NOT NULL,
                        SetNumber INTEGER NOT NULL,
                        Reps INTEGER DEFAULT 0,
                        Weight REAL DEFAULT 0,
                        FOREIGN KEY (WorkoutSessionId) REFERENCES WorkoutSessionsTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (ExerciseId) REFERENCES ExercisesTbl(Id) ON DELETE CASCADE
                    );",

                    // ============================================
                    // FEED/POSTS TABLE
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS FeedTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PostOwnerId INTEGER NOT NULL,
                        Content TEXT NOT NULL,
                        PostTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (PostOwnerId) REFERENCES UserTbl(Id) ON DELETE CASCADE
                    );",

                    // ============================================
                    // TRAINER NOTES (private notes on trainees)
                    // ============================================
                    @"
                    CREATE TABLE IF NOT EXISTS TrainerNotesTbl (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TrainerId INTEGER NOT NULL,
                        TraineeId INTEGER NOT NULL,
                        NoteDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                        NoteType TEXT CHECK(NoteType IN ('Progress', 'Issue', 'Goal Update', 'General')),
                        NoteContent TEXT,
                        FOREIGN KEY (TrainerId) REFERENCES TrainersTbl(Id) ON DELETE CASCADE,
                        FOREIGN KEY (TraineeId) REFERENCES TraineesTbl(Id) ON DELETE CASCADE
                    );"
                };

                // Execute all table creation commands
                foreach (var sql in tableCommands)
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }

                // Create indexes for better performance
                CreateIndexes(connection);
            }
        }

        private void CreateIndexes(SqliteConnection connection)
        {
            string[] indexCommands =
            {
                "CREATE INDEX IF NOT EXISTS idx_trainee_trainerid ON TraineesTbl(TrainerId);",
                "CREATE INDEX IF NOT EXISTS idx_workout_userid ON WorkoutsTbl(UserId);",
                "CREATE INDEX IF NOT EXISTS idx_workoutsession_userid ON WorkoutSessionsTbl(UserId);",
                "CREATE INDEX IF NOT EXISTS idx_workoutsession_date ON WorkoutSessionsTbl(SessionDate);",
                "CREATE INDEX IF NOT EXISTS idx_weekplan_userid ON WeekPlansTbl(UserId);",
                "CREATE INDEX IF NOT EXISTS idx_feed_owner ON FeedTbl(PostOwnerId);"
            };

            foreach (var sql in indexCommands)
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Simple test function
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        // Helper method to execute queries (can be used by DatabaseService)
        public SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}