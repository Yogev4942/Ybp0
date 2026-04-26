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

            var accessConnection = new global::DataBase.AccessDatabaseConnection();
            EnsureAccessSchema(accessConnection);
            return accessConnection;
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
            @"CREATE TABLE IF NOT EXISTS [MessagesTbl] (
                [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                [SenderId] INTEGER NOT NULL,
                [RecipientId] INTEGER NOT NULL,
                [MessageText] TEXT NOT NULL,
                [SentAt] TEXT NULL
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

        public static IReadOnlyList<string> SqliteCreateIndexStatements { get; } = new[]
        {
            @"CREATE INDEX IF NOT EXISTS [IX_PostTbl_OwnerId] ON [PostTbl] ([OwnerId])",
            @"CREATE INDEX IF NOT EXISTS [IX_LikesTbl_PostId] ON [LikesTbl] ([PostId])",
            @"CREATE INDEX IF NOT EXISTS [IX_LikesTbl_UserId_PostId] ON [LikesTbl] ([UserId], [PostId])",
            @"CREATE INDEX IF NOT EXISTS [IX_MessagesTbl_Sender_Recipient_SentAt] ON [MessagesTbl] ([SenderId], [RecipientId], [SentAt])",
            @"CREATE INDEX IF NOT EXISTS [IX_MessagesTbl_Recipient_Sender_SentAt] ON [MessagesTbl] ([RecipientId], [SenderId], [SentAt])",
            @"CREATE INDEX IF NOT EXISTS [IX_WorkoutsTbl_UserId] ON [WorkoutsTbl] ([UserId])",
            @"CREATE INDEX IF NOT EXISTS [IX_WorkoutExercisesTbl_WorkoutId] ON [WorkoutExercisesTbl] ([WorkoutId])",
            @"CREATE INDEX IF NOT EXISTS [IX_WorkoutSetsTbl_WorkoutExerciseId] ON [WorkoutSetsTbl] ([WorkoutExerciseId])",
            @"CREATE INDEX IF NOT EXISTS [IX_TraineesTbl_UserId] ON [TraineesTbl] ([UserId])",
            @"CREATE INDEX IF NOT EXISTS [IX_TraineesTbl_TrainerId] ON [TraineesTbl] ([TrainerId])",
            @"CREATE INDEX IF NOT EXISTS [IX_TrainersTbl_UserId] ON [TrainersTbl] ([UserId])",
            @"CREATE INDEX IF NOT EXISTS [IX_WeekPlansTbl_UserId] ON [WeekPlansTbl] ([UserId])",
            @"CREATE INDEX IF NOT EXISTS [IX_WeekPlanDaysTbl_WeekPlanId] ON [WeekPlanDaysTbl] ([WeekPlanId])"
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

            foreach (string statement in SqliteCreateIndexStatements)
            {
                connection.ExecuteNonQuery(statement);
            }
        }

        public static void EnsureAccessSchema(global::DataBase.AccessDatabaseConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            connection.EnsureIndexes(new[]
            {
                ("IX_PostTbl_OwnerId", "PostTbl", "[OwnerId]", false),
                ("IX_LikesTbl_PostId", "LikesTbl", "[PostId]", false),
                ("IX_LikesTbl_UserId_PostId", "LikesTbl", "[UserId], [PostId]", false),
                ("IX_MessagesTbl_Sender_Recipient_SentAt", "MessagesTbl", "[SenderId], [RecipientId], [SentAt]", false),
                ("IX_MessagesTbl_Recipient_Sender_SentAt", "MessagesTbl", "[RecipientId], [SenderId], [SentAt]", false),
                ("IX_WorkoutsTbl_UserId", "WorkoutsTbl", "[UserId]", false),
                ("IX_WorkoutExercisesTbl_WorkoutId", "WorkoutExercisesTbl", "[WorkoutId]", false),
                ("IX_WorkoutSetsTbl_WorkoutExerciseId", "WorkoutSetsTbl", "[WorkoutExerciseId]", false),
                ("IX_TraineesTbl_UserId", "TraineesTbl", "[UserId]", false),
                ("IX_TraineesTbl_TrainerId", "TraineesTbl", "[TrainerId]", false),
                ("IX_TrainersTbl_UserId", "TrainersTbl", "[UserId]", false),
                ("IX_WeekPlansTbl_UserId", "WeekPlansTbl", "[UserId]", false),
                ("IX_WeekPlanDaysTbl_WeekPlanId", "WeekPlanDaysTbl", "[WeekPlanId]", false)
            });
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
