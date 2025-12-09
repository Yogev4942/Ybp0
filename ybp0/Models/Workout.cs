using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Workout : BaseEntity
    {
        private int userId;
        private string workoutName;
        private List<WorkoutExercise> workoutExercises;

        public string WorkoutName { get => workoutName; set => workoutName = value; }
        public List<WorkoutExercise> WorkoutExercises { get => workoutExercises; set => workoutExercises = value; }
        public int UserId { get => userId; set => userId = value; }
    }
}
