using DataBase.Connection;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataBase.Repository.Access
{
    public class AccessWorkoutRepository : IWorkoutRepository
    {
        private readonly IDataBaseConnection _database;

        public AccessWorkoutRepository() : this(DatabaseFilter.CreateConnection())
        {
        }

        public AccessWorkoutRepository(IDataBaseConnection database)
        {
            _database = database ?? DatabaseFilter.CreateConnection();
        }

        public Workout GetWorkoutById(int workoutId)
        {
            var workoutDt = _database.ExecuteQuery("SELECT * FROM WorkoutsTbl WHERE Id = ?", workoutId);
            if (workoutDt.Rows.Count == 0)
            {
                return null;
            }

            return BuildWorkouts(workoutDt).FirstOrDefault();
        }

        public Dictionary<int, Workout> GetWorkoutsByIds(IEnumerable<int> workoutIds)
        {
            List<int> ids = workoutIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new Dictionary<int, Workout>();
            }

            string placeholders = string.Join(", ", ids.Select(_ => "?"));
            var dt = _database.ExecuteQuery(
                $"SELECT * FROM WorkoutsTbl WHERE Id IN ({placeholders}) ORDER BY WorkoutName",
                ids.Cast<object>().ToArray());

            return BuildWorkouts(dt)
                .GroupBy(workout => workout.Id)
                .ToDictionary(group => group.Key, group => group.First());
        }

        public List<Workout> GetWorkoutsByUserId(int userId)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutsTbl WHERE UserId = ? ORDER BY WorkoutName",
                userId);

            return BuildWorkouts(dt);
        }

        public int CreateWorkout(int userId, string workoutName)
        {
            _database.ExecuteNonQuery(
                "INSERT INTO WorkoutsTbl (UserId, WorkoutName) VALUES (?, ?)",
                userId,
                workoutName);

            return _database.ExecuteScalar<int>(
                "SELECT TOP 1 Id FROM WorkoutsTbl WHERE UserId = ? AND WorkoutName = ? ORDER BY Id DESC",
                userId,
                workoutName);
        }

        public void UpdateWorkoutName(int workoutId, string workoutName)
        {
            _database.ExecuteNonQuery(
                "UPDATE WorkoutsTbl SET WorkoutName = ? WHERE Id = ?",
                workoutName,
                workoutId);
        }

        public WorkoutExercise AddExerciseToWorkout(int workoutId, int exerciseId)
        {
            var existingDt = _database.ExecuteQuery(
                "SELECT TOP 1 Id FROM WorkoutExercisesTbl WHERE WorkoutId = ? AND ExerciseId = ? ORDER BY OrderNumber",
                workoutId,
                exerciseId);

            int workoutExerciseId;
            if (existingDt.Rows.Count > 0)
            {
                workoutExerciseId = Convert.ToInt32(existingDt.Rows[0]["Id"]);
            }
            else
            {
                int nextOrderNumber = _database.ExecuteScalar<int?>(
                    "SELECT MAX(OrderNumber) FROM WorkoutExercisesTbl WHERE WorkoutId = ?",
                    workoutId) ?? 0;

                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutExercisesTbl (WorkoutId, ExerciseId, OrderNumber) VALUES (?, ?, ?)",
                    workoutId,
                    exerciseId,
                    nextOrderNumber + 1);

                workoutExerciseId = _database.ExecuteScalar<int>(
                    "SELECT TOP 1 Id FROM WorkoutExercisesTbl WHERE WorkoutId = ? AND ExerciseId = ? ORDER BY Id DESC",
                    workoutId,
                    exerciseId);

                _database.ExecuteNonQuery(
                    "INSERT INTO WorkoutSetsTbl (WorkoutExerciseId, SetNumber, Reps, Weight) VALUES (?, ?, ?, ?)",
                    workoutExerciseId,
                    1,
                    0,
                    0);
            }

            return GetWorkoutExercises(workoutId).FirstOrDefault(ex => ex.Id == workoutExerciseId);
        }

        public void RemoveExerciseFromWorkout(int workoutExerciseId)
        {
            _database.ExecuteNonQuery("DELETE FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ?", workoutExerciseId);
            _database.ExecuteNonQuery("DELETE FROM WorkoutExercisesTbl WHERE Id = ?", workoutExerciseId);
        }

        public List<WorkoutSet> GetWorkoutSets(int workoutExerciseId)
        {
            var dt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ? ORDER BY SetNumber",
                workoutExerciseId);

            return dt.Rows.Cast<DataRow>()
                .Select(MapWorkoutSet)
                .ToList();
        }

        public WorkoutSet SaveWorkoutSet(int workoutExerciseId, int setNumber, int reps, double weight)
        {
            var existingDt = _database.ExecuteQuery(
                "SELECT * FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ? AND SetNumber = ?",
                workoutExerciseId,
                setNumber);

            if (existingDt.Rows.Count > 0)
            {
                int setId = Convert.ToInt32(existingDt.Rows[0]["Id"]);
                _database.ExecuteNonQuery(
                    "UPDATE WorkoutSetsTbl SET Reps = ?, Weight = ? WHERE Id = ?",
                    reps,
                    weight,
                    setId);

                return new WorkoutSet
                {
                    Id = setId,
                    WorkoutExerciseId = workoutExerciseId,
                    SetNumber = setNumber,
                    Reps = reps,
                    Weight = weight
                };
            }

            _database.ExecuteNonQuery(
                "INSERT INTO WorkoutSetsTbl (WorkoutExerciseId, SetNumber, Reps, Weight) VALUES (?, ?, ?, ?)",
                workoutExerciseId,
                setNumber,
                reps,
                weight);

            int newSetId = _database.ExecuteScalar<int>(
                "SELECT TOP 1 Id FROM WorkoutSetsTbl WHERE WorkoutExerciseId = ? AND SetNumber = ? ORDER BY Id DESC",
                workoutExerciseId,
                setNumber);

            return new WorkoutSet
            {
                Id = newSetId,
                WorkoutExerciseId = workoutExerciseId,
                SetNumber = setNumber,
                Reps = reps,
                Weight = weight
            };
        }

        public void DeleteWorkoutSet(int setId)
        {
            _database.ExecuteNonQuery("DELETE FROM WorkoutSetsTbl WHERE Id = ?", setId);
        }

        private List<Workout> BuildWorkouts(DataTable workoutTable)
        {
            List<Workout> workouts = workoutTable.Rows.Cast<DataRow>()
                .Select(row => new Workout
                {
                    Id = Convert.ToInt32(row["Id"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    WorkoutName = row["WorkoutName"]?.ToString()
                })
                .ToList();

            if (workouts.Count == 0)
            {
                return workouts;
            }

            Dictionary<int, List<WorkoutExercise>> exercisesByWorkoutId = GetWorkoutExercisesByWorkoutIds(
                workouts.Select(workout => workout.Id));

            foreach (Workout workout in workouts)
            {
                if (exercisesByWorkoutId.TryGetValue(workout.Id, out List<WorkoutExercise> exercises))
                {
                    workout.WorkoutExercises = exercises;
                }
            }

            return workouts;
        }

        private Dictionary<int, List<WorkoutExercise>> GetWorkoutExercisesByWorkoutIds(IEnumerable<int> workoutIds)
        {
            List<int> ids = workoutIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new Dictionary<int, List<WorkoutExercise>>();
            }

            string exerciseTable = ExerciseSchemaHelper.GetExerciseTable(_database);
            string selectSql = ExerciseSchemaHelper.BuildExerciseProjectionSql(_database, "e");
            var joins = ExerciseSchemaHelper.BuildExerciseJoinSql(_database, "e");
            string fromClause = $"[{exerciseTable}] e INNER JOIN WorkoutExercisesTbl we ON e.Id = we.ExerciseId";
            foreach (string join in joins)
            {
                fromClause = $"({fromClause}) {join}";
            }

            string placeholders = string.Join(", ", ids.Select(_ => "?"));
            var dt = _database.ExecuteQuery(
                $@"SELECT we.Id AS WorkoutExerciseId, we.WorkoutId, we.ExerciseId, we.OrderNumber, {selectSql}
                   FROM {fromClause}
                   WHERE we.WorkoutId IN ({placeholders})
                   ORDER BY we.OrderNumber, e.ExerciseName",
                ids.Cast<object>().ToArray());

            Dictionary<int, List<WorkoutExercise>> exercisesByWorkoutId = dt.Rows.Cast<DataRow>()
                .Select(MapWorkoutExercise)
                .GroupBy(exercise => exercise.WorkoutId)
                .ToDictionary(group => group.Key, group => group.ToList());

            Dictionary<int, List<WorkoutSet>> setsByExerciseId = GetWorkoutSetsByWorkoutExerciseIds(
                dt.Rows.Cast<DataRow>().Select(row => Convert.ToInt32(row["WorkoutExerciseId"])));

            foreach (List<WorkoutExercise> exercises in exercisesByWorkoutId.Values)
            {
                foreach (WorkoutExercise exercise in exercises)
                {
                    if (setsByExerciseId.TryGetValue(exercise.Id, out List<WorkoutSet> sets))
                    {
                        exercise.Sets = sets;
                    }
                }
            }

            return exercisesByWorkoutId;
        }

        private List<WorkoutExercise> GetWorkoutExercises(int workoutId)
        {
            return GetWorkoutExercisesByWorkoutIds(new[] { workoutId })
                .TryGetValue(workoutId, out List<WorkoutExercise> exercises)
                ? exercises
                : new List<WorkoutExercise>();
        }

        private Dictionary<int, List<WorkoutSet>> GetWorkoutSetsByWorkoutExerciseIds(IEnumerable<int> workoutExerciseIds)
        {
            List<int> ids = workoutExerciseIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new Dictionary<int, List<WorkoutSet>>();
            }

            string placeholders = string.Join(", ", ids.Select(_ => "?"));
            var dt = _database.ExecuteQuery(
                $"SELECT * FROM WorkoutSetsTbl WHERE WorkoutExerciseId IN ({placeholders}) ORDER BY WorkoutExerciseId, SetNumber",
                ids.Cast<object>().ToArray());

            return dt.Rows.Cast<DataRow>()
                .Select(MapWorkoutSet)
                .GroupBy(set => set.WorkoutExerciseId)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        private WorkoutExercise MapWorkoutExercise(DataRow row)
        {
            var workoutExercise = new WorkoutExercise
            {
                Id = Convert.ToInt32(row["WorkoutExerciseId"]),
                WorkoutId = Convert.ToInt32(row["WorkoutId"]),
                ExerciseId = Convert.ToInt32(row["ExerciseId"]),
                ExerciseName = row["ExerciseName"]?.ToString(),
                MuscleGroup = row["MuscleGroup"]?.ToString(),
                SecondaryMuscleGroup = row["SecondaryMuscleGroup"]?.ToString(),
                OrderNumber = Convert.ToInt32(row["OrderNumber"])
            };
            return workoutExercise;
        }

        private static WorkoutSet MapWorkoutSet(DataRow row)
        {
            return new WorkoutSet
            {
                Id = Convert.ToInt32(row["Id"]),
                WorkoutExerciseId = Convert.ToInt32(row["WorkoutExerciseId"]),
                SetNumber = Convert.ToInt32(row["SetNumber"]),
                Reps = Convert.ToInt32(row["Reps"]),
                Weight = Convert.ToDouble(row["Weight"])
            };
        }
    }
}
