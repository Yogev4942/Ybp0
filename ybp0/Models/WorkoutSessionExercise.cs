using System.Collections.Generic;

namespace Models
{
    public class WorkoutSessionExercise
    {
        private int workoutSessionId;
        private int exerciseId;
        private string exerciseName;
        private string muscleGroup;
        private string secondaryMuscleGroup;
        private List<WorkoutSessionSet> sets = new List<WorkoutSessionSet>();

        public int WorkoutSessionId { get => workoutSessionId; set => workoutSessionId = value; }
        public int ExerciseId { get => exerciseId; set => exerciseId = value; }
        public string ExerciseName { get => exerciseName; set => exerciseName = value; }
        public string MuscleGroup { get => muscleGroup; set => muscleGroup = value; }
        public string SecondaryMuscleGroup { get => secondaryMuscleGroup; set => secondaryMuscleGroup = value; }
        public List<WorkoutSessionSet> Sets { get => sets; set => sets = value; }
        public Exercise Exercise { get; set; }
    }
}
