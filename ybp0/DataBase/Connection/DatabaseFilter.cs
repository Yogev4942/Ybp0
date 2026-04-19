using System;
using System.Collections.Generic;
using System.IO;

namespace DataBase.Connection
{
    public enum DatabaseProvider
    {
        Access,
        Sqlite
    }

    public static class DatabaseFilter
    {
        public static DatabaseProvider CurrentProvider { get; set; } = DatabaseProvider.Access;

        public static IDataBaseConnection CreateConnection()
        {
            if (CurrentProvider == DatabaseProvider.Sqlite)
            {
                var sqliteConnection = new SqliteDatabaseConnection(GetDefaultSqlitePath());
                EnsureSqliteSchema(sqliteConnection);
                return sqliteConnection;
            }

            return new global::DataBase.AccessDatabaseConnection();
        }

        public static IReadOnlyList<string> SqliteCreateTableStatements { get; } = new[]
        {
            @"CREATE TABLE IF NOT EXISTS [AdminsTbl] (
                [מזהה] INTEGER PRIMARY KEY AUTOINCREMENT
            )",
            @"CREATE TABLE IF NOT EXISTS [ExercisesTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [ExerciseName] TEXT NOT NULL,
                [PrimaryMuscleId] INTEGER NULL,
                [SecondaryMuscleId] INTEGER NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [LikesTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [PostId] INTEGER NOT NULL,
                [UserId] INTEGER NOT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [MusclesTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [MuscleName] TEXT NOT NULL,
                [BodyRegion] TEXT NULL,
                [DiagramZoneId] INTEGER NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [PostTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [OwnerId] INTEGER NOT NULL,
                [Header] TEXT NULL,
                [Content] TEXT NULL,
                [PostTime] TEXT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [TraineesTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [UserId] INTEGER NOT NULL,
                [TrainerId] INTEGER NULL,
                [FitnessGoal] TEXT NULL,
                [CurrentWeight] REAL NOT NULL DEFAULT 0,
                [Height] REAL NOT NULL DEFAULT 0
            )",
            @"CREATE TABLE IF NOT EXISTS [TrainerRequestsTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [TraineeUserId] INTEGER NOT NULL,
                [TrainerUserId] INTEGER NOT NULL,
                [Status] TEXT NOT NULL,
                [RequestDate] TEXT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [TrainersTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [UserId] INTEGER NOT NULL,
                [Specialization] TEXT NULL,
                [HourlyRate] REAL NOT NULL DEFAULT 0,
                [MaxTrainees] INTEGER NOT NULL DEFAULT 0,
                [TotalTrainees] INTEGER NOT NULL DEFAULT 0,
                [Rating] REAL NOT NULL DEFAULT 0,
                [TotalRatings] INTEGER NOT NULL DEFAULT 0
            )",
            @"CREATE TABLE IF NOT EXISTS [UserTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [Email] TEXT NULL,
                [Username] TEXT NOT NULL,
                [Password] TEXT NOT NULL,
                [JoinDate] TEXT NULL,
                [IsTrainer] INTEGER NOT NULL DEFAULT 0,
                [Bio] TEXT NULL,
                [Gender] TEXT NULL,
                [CurrentWeekPlanId] INTEGER NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [WeekPlanDaysTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [WeekPlanId] INTEGER NOT NULL,
                [DayOfWeek] TEXT NOT NULL,
                [WorkoutId] INTEGER NULL,
                [RestDay] INTEGER NOT NULL DEFAULT 1
            )",
            @"CREATE TABLE IF NOT EXISTS [WeekPlansTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [UserId] INTEGER NOT NULL,
                [PlanName] TEXT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [WorkoutExercisesTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [WorkoutId] INTEGER NOT NULL,
                [ExerciseId] INTEGER NOT NULL,
                [OrderNumber] INTEGER NOT NULL DEFAULT 0
            )",
            @"CREATE TABLE IF NOT EXISTS [WorkoutSessionSetsTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [WorkoutSessionId] INTEGER NOT NULL,
                [ExerciseId] INTEGER NOT NULL,
                [SetNumber] INTEGER NOT NULL,
                [Reps] INTEGER NOT NULL DEFAULT 0,
                [Weight] REAL NOT NULL DEFAULT 0,
                [IsCompleted] INTEGER NOT NULL DEFAULT 0
            )",
            @"CREATE TABLE IF NOT EXISTS [WorkoutSessionTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [UserId] INTEGER NOT NULL,
                [WeekPlanDayId] INTEGER NULL,
                [WorkoutId] INTEGER NULL,
                [SessionDate] TEXT NULL,
                [IsCompleted] INTEGER NOT NULL DEFAULT 0,
                [StartTime] TEXT NULL,
                [EndTime] TEXT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS [WorkoutSetsTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [WorkoutExerciseId] INTEGER NOT NULL,
                [SetNumber] INTEGER NOT NULL,
                [Reps] INTEGER NOT NULL DEFAULT 0,
                [Weight] REAL NOT NULL DEFAULT 0
            )",
            @"CREATE TABLE IF NOT EXISTS [WorkoutsTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [UserId] INTEGER NOT NULL,
                [WorkoutName] TEXT NULL
            )"
        };

        public static void EnsureSqliteSchema(IDataBaseConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            foreach (string statement in SqliteCreateTableStatements)
            {
                connection.ExecuteNonQuery(statement);
            }
        }

        private static string GetDefaultSqlitePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string databaseDirectory = Path.Combine(baseDirectory, "DataBase");

            if (!Directory.Exists(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }

            return Path.Combine(databaseDirectory, "DB.sqlite");
        }
    }
}
