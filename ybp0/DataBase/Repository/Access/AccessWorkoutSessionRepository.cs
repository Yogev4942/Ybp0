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
    public class AccessWorkoutSessionRepository : IWorkoutSessionRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessWorkoutSessionRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public WorkoutSession GetOrCreateWorkoutSession(int userId, int weekPlanDayId, DateTime date)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionTbl WHERE UserId = ? AND WeekPlanDayId = ? AND SessionDate = ?",
                userId, weekPlanDayId, date.Date
            );

            if (dt.Rows.Count > 0) return MapWorkoutSession(dt.Rows[0]);

            var dayDt = _database.ExecuteQuery("SELECT WorkoutId FROM WeekPlanDaysTbl WHERE Id = ?", weekPlanDayId);
            if (dayDt.Rows.Count == 0) return null;

            int workoutId;
            if (dayDt.Rows[0]["WorkoutId"] != DBNull.Value)
            {
                workoutId = Convert.ToInt32(dayDt.Rows[0]["WorkoutId"]);
            }
            else
            {
                _database.ExecuteNonQuery("INSERT INTO WorkoutsTbl (UserId, WorkoutName) VALUES (?, ?)", userId, "Ad-hoc Workout");
                System.Threading.Thread.Sleep(100);
                var wDt = _database.ExecuteQuery("SELECT TOP 1 Id FROM WorkoutsTbl WHERE UserId = ? ORDER BY Id DESC", userId);
                workoutId = Convert.ToInt32(wDt.Rows[0]["Id"]);
            }

            _database.ExecuteNonQuery(
                "INSERT INTO WorkoutSessionTbl ([UserId], [WorkoutId], [WeekPlanDayId], [SessionDate]) VALUES (?, ?, ?, ?)",
                userId, workoutId, weekPlanDayId, date.Date
            );

            System.Threading.Thread.Sleep(100);
            var newDt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionTbl WHERE UserId = ? AND WeekPlanDayId = ? AND SessionDate = ?",
                userId, weekPlanDayId, date.Date
            );

            return newDt.Rows.Count > 0 ? MapWorkoutSession(newDt.Rows[0]) : null;
        }

        public List<Exercise> GetSessionExercises(int workoutSessionId)
        {
            string exerciseTable = ExerciseSchemaHelper.GetExerciseTable(_database);
            string selectSql = ExerciseSchemaHelper.BuildExerciseProjectionSql(_database, "e");
            var joins = ExerciseSchemaHelper.BuildExerciseJoinSql(_database, "e");
            string fromClause = $"[{exerciseTable}] e INNER JOIN WorkoutSessionSetsTbl wss ON e.Id = wss.ExerciseId";
            foreach (var join in joins)
            {
                fromClause = $"({fromClause}) {join}";
            }

            var dt = _database.ExecuteQuery(
                $@"SELECT DISTINCT {selectSql}
                  FROM {fromClause}
                  WHERE wss.WorkoutSessionId = ?", workoutSessionId
            );

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
            return exercises;
        }

        public List<SessionSet> GetSessionSets(int workoutSessionId, int exerciseId)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId, exerciseId
            );

            var sets = new List<SessionSet>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows) sets.Add(MapSessionSet(row));
                return sets;
            }

            var sessionDt = _database.ExecuteQuery("SELECT WorkoutId FROM WorkoutSessionTbl WHERE Id = ?", workoutSessionId);
            if (sessionDt.Rows.Count == 0) return sets;
            int workoutId = Convert.ToInt32(sessionDt.Rows[0]["WorkoutId"]);

            var weDt = _database.ExecuteQuery(
                "SELECT Id FROM WorkoutExercisesTbl WHERE WorkoutId = ? AND ExerciseId = ?",
                workoutId, exerciseId
            );
            if (weDt.Rows.Count == 0) return sets;
            int workoutExerciseId = Convert.ToInt32(weDt.Rows[0]["Id"]);

            var templateDt = _database.ExecuteQuery("SELECT * FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ?", workoutExerciseId);

            foreach (DataRow row in templateDt.Rows)
            {
                int setNumber = Convert.ToInt32(row["SetNumber"]);
                int reps = Convert.ToInt32(row["Reps"]);
                double weight = Convert.ToDouble(row["Weight"]);

                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutSessionSetsTbl ([WorkoutSessionId], [ExerciseId], [SetNumber], [Reps], [Weight]) VALUES (?, ?, ?, ?, ?)",
                    workoutSessionId, exerciseId, setNumber, reps, weight
                );

                sets.Add(new SessionSet
                {
                    WorkoutSessionId = workoutSessionId,
                    ExerciseId = exerciseId,
                    SetNumber = setNumber,
                    Reps = reps,
                    Weight = weight
                });
            }
            return sets;
        }

        public void SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight)
        {
            var dt = _database.ExecuteQuery(
                "SELECT Id FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ? AND SetNumber = ?",
                workoutSessionId, exerciseId, setNumber
            );

            if (dt.Rows.Count > 0)
            {
                int id = Convert.ToInt32(dt.Rows[0]["Id"]);
                _database.ExecuteNonQuery("UPDATE WorkoutSessionSetsTbl SET Reps = ?, Weight = ? WHERE Id = ?", reps, weight, id);
            }
            else
            {
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

        public void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?", workoutSessionId, exerciseId);
            if (dt.Rows.Count == 0)
            {
                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutSessionSetsTbl ([WorkoutSessionId], [ExerciseId], [SetNumber], [Reps], [Weight]) VALUES (?, ?, ?, ?, ?)",
                    workoutSessionId, exerciseId, 1, 0, 0.0
                );
            }
        }

        public void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId)
        {
            _database.ExecuteNonQuery("DELETE FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?", workoutSessionId, exerciseId);
        }

        private WorkoutSession MapWorkoutSession(DataRow row)
        {
            return new WorkoutSession
            {
                Id = Convert.ToInt32(row["Id"]),
                UserId = Convert.ToInt32(row["UserId"]),
                WorkoutId = Convert.ToInt32(row["WorkoutId"]),
                WeekPlanDayId = row["WeekPlanDayId"] != DBNull.Value ? Convert.ToInt32(row["WeekPlanDayId"]) : (int?)null,
                SessionDate = Convert.ToDateTime(row["SessionDate"])
            };
        }

        private SessionSet MapSessionSet(DataRow row)
        {
            return new SessionSet
            {
                Id = Convert.ToInt32(row["Id"]),
                WorkoutSessionId = Convert.ToInt32(row["WorkoutSessionId"]),
                ExerciseId = Convert.ToInt32(row["ExerciseId"]),
                SetNumber = Convert.ToInt32(row["SetNumber"]),
                Reps = Convert.ToInt32(row["Reps"]),
                Weight = Convert.ToDouble(row["Weight"])
            };
        }
    }
}
