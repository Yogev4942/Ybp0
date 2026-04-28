using System.Collections.Generic;

namespace Models
{
    public class Workout : BaseEntity
    {
        private int userId;
        private string workoutName;
        private List<WorkoutExercise> workoutExercises = new List<WorkoutExercise>();

        public string WorkoutName { get => workoutName; set => workoutName = value; }
        public List<WorkoutExercise> WorkoutExercises { get => workoutExercises; set => workoutExercises = value; }
        public int UserId { get => userId; set => userId = value; }
        public User User { get; set; }
        public List<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    }
}
