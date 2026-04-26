using DataBase.Connection;
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
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly IDataBaseConnection _database;

        public ExerciseRepository() : this(SqliteDatabaseConnection.CreateDefault())
        {
        }

        public ExerciseRepository(IDataBaseConnection database)
        {
            _database = database ?? SqliteDatabaseConnection.CreateDefault();
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            string exerciseTable = ExerciseSchemaHelper.GetExerciseTable(_database);
            if (!_database.ColumnExists(exerciseTable, "SecondaryMuscleId") &&
                !_database.ColumnExists(exerciseTable, "SecondaryMuscle"))
            {
                try
                {
                    _database.ExecuteNonQuery($"ALTER TABLE [{exerciseTable}] ADD COLUMN [SecondaryMuscleId] INTEGER");
                }
                catch (Exception ex)
                {
                    // Log or handle (already exists or permission)
                    Console.WriteLine($"Error adding SecondaryMuscleId: {ex.Message}");
                }
            }
        }

        public List<Exercise> GetAllExercises()
        {
            string exerciseTable = ExerciseSchemaHelper.GetExerciseTable(_database);
            string exerciseNameColumn = ExerciseSchemaHelper.GetExerciseNameColumn(_database, exerciseTable);
            string selectSql = ExerciseSchemaHelper.BuildExerciseProjectionSql(_database, "e");
            var joins = ExerciseSchemaHelper.BuildExerciseJoinSql(_database, "e");
            string fromClause = $"[{exerciseTable}] e";
            foreach (var join in joins)
            {
                fromClause = $"({fromClause}) {join}";
            }

            var dt = _database.ExecuteQuery(
                $"SELECT {selectSql} FROM {fromClause} ORDER BY e.[{exerciseNameColumn}]");
            var exercises = new List<Exercise>();

            foreach (DataRow row in dt.Rows)
            {
                var primaryMuscleName = row["MuscleGroup"]?.ToString();
                var secondaryMuscleName = row["SecondaryMuscleGroup"]?.ToString();

                exercises.Add(new Exercise
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ExerciseName = row["ExerciseName"].ToString(),
                    PrimaryMuscleId = row.Table.Columns.Contains("PrimaryMuscle") && row["PrimaryMuscle"] != DBNull.Value
                        ? Convert.ToInt32(row["PrimaryMuscle"])
                        : row.Table.Columns.Contains("PrimaryMuscleId") && row["PrimaryMuscleId"] != DBNull.Value
                            ? Convert.ToInt32(row["PrimaryMuscleId"])
                            : row.Table.Columns.Contains("MuscleId") && row["MuscleId"] != DBNull.Value
                                ? Convert.ToInt32(row["MuscleId"])
                                : (int?)null,
                    SecondaryMuscleId = row.Table.Columns.Contains("SecondaryMuscle") && row["SecondaryMuscle"] != DBNull.Value
                        ? Convert.ToInt32(row["SecondaryMuscle"])
                        : row.Table.Columns.Contains("SecondaryMuscleId") && row["SecondaryMuscleId"] != DBNull.Value
                            ? Convert.ToInt32(row["SecondaryMuscleId"])
                            : (int?)null,
                    MuscleGroup = primaryMuscleName,
                    SecondaryMuscleGroup = secondaryMuscleName,
                    PrimaryMuscle = string.IsNullOrWhiteSpace(primaryMuscleName) ? null : new Muscle { MuscleName = primaryMuscleName },
                    SecondaryMuscle = string.IsNullOrWhiteSpace(secondaryMuscleName) ? null : new Muscle { MuscleName = secondaryMuscleName }
                });
            }

            if (exercises.Count == 0)
            {
                SeedExercises();
                return GetAllExercises();
            }

            return exercises;
        }

        private void SeedExercises()
        {
            var defaultExercises = new List<(string Name, string Muscle)>
            {
                ("Bench Press", "Chest"), ("Squat", "Legs"), ("Deadlift", "Back"),
                ("Overhead Press", "Shoulders"), ("Pull Up", "Back"), ("Dumbbell Row", "Back"),
                ("Lunges", "Legs"), ("Push Up", "Chest"), ("Bicep Curl", "Arms"),
                ("Tricep Extension", "Arms"), ("Plank", "Core"), ("Crunches", "Core"),
                ("Leg Press", "Legs"), ("Lat Pulldown", "Back"), ("Shoulder Fly", "Shoulders")
            };

            string exerciseTable = ExerciseSchemaHelper.GetExerciseTable(_database);
            string exerciseNameColumn = ExerciseSchemaHelper.GetExerciseNameColumn(_database, exerciseTable);
            string muscleTable = ExerciseSchemaHelper.GetMuscleTable(_database);
            string muscleNameColumn = ExerciseSchemaHelper.GetMuscleNameColumn(_database, muscleTable);
            string muscleForeignKeyColumn = ExerciseSchemaHelper.GetExerciseMuscleForeignKeyColumn(_database, exerciseTable);
            bool useNormalizedTables = !string.IsNullOrWhiteSpace(muscleTable) &&
                                      !string.IsNullOrWhiteSpace(muscleNameColumn) &&
                                      !string.IsNullOrWhiteSpace(muscleForeignKeyColumn);

            foreach (var ex in defaultExercises)
            {
                if (useNormalizedTables)
                {
                    int muscleId = EnsureMuscle(ex.Muscle, muscleTable, muscleNameColumn);
                    _database.ExecuteNonQuery(
                        $"INSERT INTO [{exerciseTable}] ([{exerciseNameColumn}], [{muscleForeignKeyColumn}]) VALUES (?, ?)",
                        ex.Name, muscleId);
                }
                else
                {
                    _database.ExecuteNonQuery(
                        $"INSERT INTO [{exerciseTable}] ([{exerciseNameColumn}], [MuscleGroup]) VALUES (?, ?)",
                        ex.Name, ex.Muscle);
                }
            }
        }

        private int EnsureMuscle(string muscleName, string muscleTable, string muscleNameColumn)
        {
            var existing = _database.ExecuteQuery(
                $"SELECT Id FROM [{muscleTable}] WHERE [{muscleNameColumn}] = ?",
                muscleName);

            if (existing.Rows.Count > 0)
            {
                return Convert.ToInt32(existing.Rows[0]["Id"]);
            }

            _database.ExecuteNonQuery(
                $"INSERT INTO [{muscleTable}] ([{muscleNameColumn}]) VALUES (?)",
                muscleName);

            var created = _database.ExecuteQuery(
                $"SELECT TOP 1 Id FROM [{muscleTable}] WHERE [{muscleNameColumn}] = ? ORDER BY Id DESC",
                muscleName);

            return Convert.ToInt32(created.Rows[0]["Id"]);
        }
    }
}
