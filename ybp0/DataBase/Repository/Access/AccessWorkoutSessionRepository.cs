using DataBase.Connection;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace DataBase.Repository.Access
{
    public class AccessWorkoutSessionRepository : IWorkoutSessionRepository
    {
        private readonly IDataBaseConnection _database;

        public AccessWorkoutSessionRepository() : this(DatabaseFilter.CreateConnection())
        {
        }

        public AccessWorkoutSessionRepository(IDataBaseConnection database)
        {
            _database = database ?? DatabaseFilter.CreateConnection();
            EnsureSchema();
        }

        public WorkoutSession GetActiveSession(int userId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT TOP 1 ws.*, w.WorkoutName
                  FROM WorkoutSessionTbl ws
                  LEFT JOIN WorkoutsTbl w ON ws.WorkoutId = w.Id
                  WHERE ws.UserId = ? AND ws.IsCompleted = False
                  ORDER BY ws.StartTime DESC, ws.Id DESC",
                userId);

            return dt.Rows.Count > 0 ? MapWorkoutSession(dt.Rows[0]) : null;
        }

        public WorkoutSession GetSessionById(int workoutSessionId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT TOP 1 ws.*, w.WorkoutName
                  FROM WorkoutSessionTbl ws
                  LEFT JOIN WorkoutsTbl w ON ws.WorkoutId = w.Id
                  WHERE ws.Id = ?",
                workoutSessionId);

            return dt.Rows.Count > 0 ? MapWorkoutSession(dt.Rows[0]) : null;
        }

        public List<WorkoutSession> GetCompletedSessionsByUserId(int userId, int maxCount = 120)
        {
            var dt = _database.ExecuteQuery(
                $@"SELECT TOP {Math.Max(1, maxCount)} ws.*, w.WorkoutName
                   FROM WorkoutSessionTbl ws
                   LEFT JOIN WorkoutsTbl w ON ws.WorkoutId = w.Id
                   WHERE ws.UserId = ? AND ws.IsCompleted = True
                   ORDER BY ws.SessionDate DESC, ws.StartTime DESC, ws.Id DESC",
                userId);

            return dt.Rows.Cast<DataRow>()
                .Select(MapWorkoutSession)
                .ToList();
        }

        public WorkoutSession StartWorkoutSession(int userId, SessionMode mode, int? workoutId, int? weekPlanDayId, DateTime startTime)
        {
            WorkoutSession existingSession = GetActiveSession(userId);
            if (existingSession != null)
            {
                return existingSession;
            }

            bool hasSessionModeColumn = _database.ColumnExists("WorkoutSessionTbl", "SessionMode");
            InsertWorkoutSession(userId, mode, workoutId, weekPlanDayId, startTime, hasSessionModeColumn);

            System.Threading.Thread.Sleep(100);
            int sessionId = _database.ExecuteScalar<int>(
                "SELECT TOP 1 Id FROM WorkoutSessionTbl WHERE UserId = ? ORDER BY Id DESC",
                userId);

            if (workoutId.HasValue)
            {
                CopyTemplateSetsToSession(sessionId, workoutId.Value);
            }

            return GetSessionById(sessionId);
        }

        public WorkoutSession FinishWorkoutSession(int workoutSessionId, DateTime endTime)
        {
            _database.ExecuteNonQuery(
                @"DELETE FROM WorkoutSessionSetsTbl
                  WHERE WorkoutSessionId = ?
                    AND (IsCompleted = 0 OR IsCompleted IS NULL)",
                workoutSessionId);

            _database.ExecuteNonQuery(
                "UPDATE WorkoutSessionTbl SET EndTime = ?, IsCompleted = ? WHERE Id = ?",
                CreateParameter(OleDbType.Date, endTime),
                CreateParameter(OleDbType.Boolean, true),
                CreateParameter(OleDbType.Integer, workoutSessionId));

            return GetSessionById(workoutSessionId);
        }

        public void CancelWorkoutSession(int workoutSessionId)
        {
            _database.ExecuteNonQuery(
                "DELETE FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ?",
                workoutSessionId);

            _database.ExecuteNonQuery(
                @"DELETE FROM WorkoutSessionTbl
                  WHERE Id = ? AND (IsCompleted = 0 OR IsCompleted IS NULL)",
                workoutSessionId);
        }

        public List<WorkoutSessionExercise> GetSessionExercises(int workoutSessionId)
        {
            string exerciseTable = ExerciseSchemaHelper.GetExerciseTable(_database);
            string selectSql = ExerciseSchemaHelper.BuildExerciseProjectionSql(_database, "e");
            var joins = ExerciseSchemaHelper.BuildExerciseJoinSql(_database, "e");
            string fromClause = $"[{exerciseTable}] e INNER JOIN WorkoutSessionSetsTbl wss ON e.Id = wss.ExerciseId";
            foreach (string join in joins)
            {
                fromClause = $"({fromClause}) {join}";
            }

            var dt = _database.ExecuteQuery(
                $@"SELECT DISTINCT e.Id, {selectSql}
                   FROM {fromClause}
                   WHERE wss.WorkoutSessionId = ?
                   ORDER BY e.ExerciseName",
                workoutSessionId);

            return dt.Rows.Cast<DataRow>()
                .Select(row =>
                {
                    int exerciseId = Convert.ToInt32(row["Id"]);
                    return new WorkoutSessionExercise
                    {
                        ExerciseId = exerciseId,
                        ExerciseName = row["ExerciseName"]?.ToString(),
                        MuscleGroup = row["MuscleGroup"]?.ToString(),
                        SecondaryMuscleGroup = row["SecondaryMuscleGroup"]?.ToString(),
                        Sets = GetSessionSets(workoutSessionId, exerciseId)
                    };
                })
                .ToList();
        }

        public List<WorkoutSessionSet> GetSessionSets(int workoutSessionId, int exerciseId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT * FROM WorkoutSessionSetsTbl
                  WHERE WorkoutSessionId = ? AND ExerciseId = ?
                  ORDER BY SetNumber",
                workoutSessionId,
                exerciseId);

            return dt.Rows.Cast<DataRow>()
                .Select(MapSessionSet)
                .ToList();
        }

        public WorkoutSessionSet SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight, bool isCompleted)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT * FROM WorkoutSessionSetsTbl
                  WHERE WorkoutSessionId = ? AND ExerciseId = ? AND SetNumber = ?",
                workoutSessionId,
                exerciseId,
                setNumber);

            if (dt.Rows.Count > 0)
            {
                int setId = Convert.ToInt32(dt.Rows[0]["Id"]);
                _database.ExecuteNonQuery(
                    @"UPDATE WorkoutSessionSetsTbl
                      SET Reps = ?, Weight = ?, IsCompleted = ?
                      WHERE Id = ?",
                    reps,
                    weight,
                    isCompleted ? 1 : 0,
                    setId);

                return new WorkoutSessionSet
                {
                    Id = setId,
                    WorkoutSessionId = workoutSessionId,
                    ExerciseId = exerciseId,
                    SetNumber = setNumber,
                    Reps = reps,
                    Weight = weight,
                    IsCompleted = isCompleted
                };
            }

            _database.ExecuteNonQuery(
                @"INSERT INTO WorkoutSessionSetsTbl (WorkoutSessionId, ExerciseId, SetNumber, Reps, Weight, IsCompleted)
                  VALUES (?, ?, ?, ?, ?, ?)",
                workoutSessionId,
                exerciseId,
                setNumber,
                reps,
                weight,
                isCompleted ? 1 : 0);

            System.Threading.Thread.Sleep(100);
            int newSetId = _database.ExecuteScalar<int>(
                "SELECT TOP 1 Id FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? ORDER BY Id DESC",
                workoutSessionId);

            return new WorkoutSessionSet
            {
                Id = newSetId,
                WorkoutSessionId = workoutSessionId,
                ExerciseId = exerciseId,
                SetNumber = setNumber,
                Reps = reps,
                Weight = weight,
                IsCompleted = isCompleted
            };
        }

        public WorkoutSessionSet AddSessionSet(int workoutSessionId, int exerciseId, int reps, double weight)
        {
            int nextSetNumber = (_database.ExecuteScalar<int?>(
                @"SELECT MAX(SetNumber) FROM WorkoutSessionSetsTbl
                  WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId,
                exerciseId) ?? 0) + 1;

            return SaveSessionSet(workoutSessionId, exerciseId, nextSetNumber, reps, weight, false);
        }

        public void DeleteSessionSet(int setId)
        {
            _database.ExecuteNonQuery("DELETE FROM WorkoutSessionSetsTbl WHERE Id = ?", setId);
        }

        public void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId)
        {
            var existingDt = _database.ExecuteQuery(
                @"SELECT TOP 1 Id FROM WorkoutSessionSetsTbl
                  WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId,
                exerciseId);

            if (existingDt.Rows.Count == 0)
            {
                SaveSessionSet(workoutSessionId, exerciseId, 1, 0, 0, false);
            }
        }

        public void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId)
        {
            _database.ExecuteNonQuery(
                "DELETE FROM WorkoutSessionSetsTbl WHERE WorkoutSessionId = ? AND ExerciseId = ?",
                workoutSessionId,
                exerciseId);
        }

        private void EnsureSchema()
        {
            if (!_database.ColumnExists("WorkoutSessionTbl", "SessionMode"))
            {
                try
                {
                    _database.ExecuteNonQuery("ALTER TABLE WorkoutSessionTbl ADD COLUMN SessionMode TEXT(50)");
                }
                catch
                {
                }
            }
        }

        private void InsertWorkoutSession(int userId, SessionMode mode, int? workoutId, int? weekPlanDayId, DateTime startTime, bool hasSessionModeColumn)
        {
            var parameters = new List<object>
            {
                CreateParameter(OleDbType.Integer, userId),
                CreateParameter(OleDbType.Integer, workoutId),
                CreateParameter(OleDbType.Integer, weekPlanDayId),
                CreateParameter(OleDbType.Date, startTime.Date),
                CreateParameter(OleDbType.Date, startTime),
                CreateParameter(OleDbType.Date, null),
                CreateParameter(OleDbType.Boolean, false)
            };

            if (hasSessionModeColumn)
            {
                parameters.Add(CreateParameter(OleDbType.VarWChar, mode.ToString()));
                _database.ExecuteNonQuery(
                    @"INSERT INTO WorkoutSessionTbl (UserId, WorkoutId, WeekPlanDayId, SessionDate, StartTime, EndTime, IsCompleted, SessionMode)
                      VALUES (?, ?, ?, ?, ?, ?, ?, ?)",
                    parameters.ToArray());
                return;
            }

            _database.ExecuteNonQuery(
                @"INSERT INTO WorkoutSessionTbl (UserId, WorkoutId, WeekPlanDayId, SessionDate, StartTime, EndTime, IsCompleted)
                  VALUES (?, ?, ?, ?, ?, ?, ?)",
                parameters.ToArray());
        }

        private static OleDbParameter CreateParameter(OleDbType type, object value)
        {
            return new OleDbParameter
            {
                OleDbType = type,
                Value = value ?? DBNull.Value
            };
        }

        private void CopyTemplateSetsToSession(int workoutSessionId, int workoutId)
        {
            var workoutExerciseDt = _database.ExecuteQuery(
                "SELECT Id, ExerciseId FROM WorkoutExercisesTbl WHERE WorkoutId = ? ORDER BY OrderNumber, Id",
                workoutId);

            foreach (DataRow exerciseRow in workoutExerciseDt.Rows)
            {
                int workoutExerciseId = Convert.ToInt32(exerciseRow["Id"]);
                int exerciseId = Convert.ToInt32(exerciseRow["ExerciseId"]);

                var setDt = _database.ExecuteQuery(
                    "SELECT * FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ? ORDER BY SetNumber",
                    workoutExerciseId);

                foreach (DataRow setRow in setDt.Rows)
                {
                    _database.ExecuteNonQuery(
                        @"INSERT INTO WorkoutSessionSetsTbl (WorkoutSessionId, ExerciseId, SetNumber, Reps, Weight, IsCompleted)
                          VALUES (?, ?, ?, ?, ?, ?)",
                        workoutSessionId,
                        exerciseId,
                        Convert.ToInt32(setRow["SetNumber"]),
                        Convert.ToInt32(setRow["Reps"]),
                        Convert.ToDouble(setRow["Weight"]),
                        0);
                }
            }
        }

        private WorkoutSession MapWorkoutSession(DataRow row)
        {
            string rawMode = row.Table.Columns.Contains("SessionMode") && row["SessionMode"] != DBNull.Value
                ? row["SessionMode"].ToString()
                : null;

            return new WorkoutSession
            {
                Id = Convert.ToInt32(row["Id"]),
                UserId = Convert.ToInt32(row["UserId"]),
                WorkoutId = row["WorkoutId"] != DBNull.Value ? Convert.ToInt32(row["WorkoutId"]) : (int?)null,
                WeekPlanDayId = row["WeekPlanDayId"] != DBNull.Value ? Convert.ToInt32(row["WeekPlanDayId"]) : (int?)null,
                SessionDate = row["SessionDate"] != DBNull.Value ? Convert.ToDateTime(row["SessionDate"]) : DateTime.Today,
                StartTime = row["StartTime"] != DBNull.Value ? Convert.ToDateTime(row["StartTime"]) : DateTime.Now,
                EndTime = row["EndTime"] != DBNull.Value ? Convert.ToDateTime(row["EndTime"]) : (DateTime?)null,
                IsCompleted = row["IsCompleted"] != DBNull.Value && Convert.ToBoolean(row["IsCompleted"]),
                Mode = InferSessionMode(rawMode, row["WorkoutId"] != DBNull.Value, row["WeekPlanDayId"] != DBNull.Value),
                WorkoutName = row.Table.Columns.Contains("WorkoutName") && row["WorkoutName"] != DBNull.Value
                    ? row["WorkoutName"].ToString()
                    : null
            };
        }

        private static WorkoutSessionSet MapSessionSet(DataRow row)
        {
            return new WorkoutSessionSet
            {
                Id = Convert.ToInt32(row["Id"]),
                WorkoutSessionId = Convert.ToInt32(row["WorkoutSessionId"]),
                ExerciseId = Convert.ToInt32(row["ExerciseId"]),
                SetNumber = Convert.ToInt32(row["SetNumber"]),
                Reps = Convert.ToInt32(row["Reps"]),
                Weight = Convert.ToDouble(row["Weight"]),
                IsCompleted = row["IsCompleted"] != DBNull.Value && Convert.ToBoolean(row["IsCompleted"])
            };
        }

        private static SessionMode InferSessionMode(string rawMode, bool hasWorkoutId, bool hasWeekPlanDayId)
        {
            if (!string.IsNullOrWhiteSpace(rawMode) && Enum.TryParse(rawMode, true, out SessionMode parsedMode))
            {
                return parsedMode;
            }

            if (hasWeekPlanDayId)
            {
                return SessionMode.Plan;
            }

            if (hasWorkoutId)
            {
                return SessionMode.Saved;
            }

            return SessionMode.Freestyle;
        }
    }
}
