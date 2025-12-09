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
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Enable foreign keys
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            string[] tableCommands =
            {
                @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    Role TEXT NOT NULL CHECK(Role IN ('Trainer', 'Member'))
                );",

                @"
                CREATE TABLE IF NOT EXISTS Exercises (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    MuscleGroup TEXT NOT NULL
                );",

                @"
                CREATE TABLE IF NOT EXISTS Workouts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    IsPublic INTEGER DEFAULT 0,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );",

                @"
                CREATE TABLE IF NOT EXISTS WorkoutExercises (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WorkoutId INTEGER NOT NULL,
                    ExerciseId INTEGER NOT NULL,
                    Sets INTEGER DEFAULT 3,
                    Reps INTEGER DEFAULT 10,
                    Weight REAL DEFAULT 0,
                    FOREIGN KEY (WorkoutId) REFERENCES Workouts(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ExerciseId) REFERENCES Exercises(Id) ON DELETE CASCADE
                );",

                @"
                CREATE TABLE IF NOT EXISTS WeeklyPlans (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );",

                @"
                CREATE TABLE IF NOT EXISTS WeeklyPlanDays (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    WeeklyPlanId INTEGER NOT NULL,
                    DayOfWeek INTEGER NOT NULL CHECK(DayOfWeek BETWEEN 0 AND 6),
                    WorkoutId INTEGER,
                    FOREIGN KEY (WeeklyPlanId) REFERENCES WeeklyPlans(Id) ON DELETE CASCADE,
                    FOREIGN KEY (WorkoutId) REFERENCES Workouts(Id) ON DELETE SET NULL
                );"
            };

            // Execute all table creation commands
            foreach (var sql in tableCommands)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        // Simple test function
        public bool TestConnection()
        {
            try
            {
                var connection = new SqliteConnection(connectionString);
                connection.Open();
                return connection.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
