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
    public class AccessExerciseRepository : IExerciseRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessExerciseRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public List<Exercise> GetAllExercises()
        {
            var dt = _database.ExecuteQuery("SELECT * FROM ExercisesTbl ORDER BY ExerciseName");
            var exercises = new List<Exercise>();

            foreach (DataRow row in dt.Rows)
            {
                exercises.Add(new Exercise
                {
                    Id = Convert.ToInt32(row["Id"]),
                    ExerciseName = row["ExerciseName"].ToString(),
                    MuscleGroup = row["MuscleGroup"]?.ToString()
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

            foreach (var ex in defaultExercises)
            {
                _database.ExecuteNonQuery("INSERT INTO ExercisesTbl (ExerciseName, MuscleGroup) VALUES (?, ?)", ex.Name, ex.Muscle);
            }
        }
    }
}
