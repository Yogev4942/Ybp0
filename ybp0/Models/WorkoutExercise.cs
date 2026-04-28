using System.Collections.Generic;

namespace Models
{
    public class WorkoutExercise : BaseEntity
    {
        private int workoutId;
        private int exerciseId;
        private string exerciseName;
        private string muscleGroup;
        private string secondaryMuscleGroup;
        private int orderNumber;
        private List<WorkoutSet> sets = new List<WorkoutSet>();

        public int WorkoutId { get => workoutId; set => workoutId = value; }
        public int ExerciseId { get => exerciseId; set => exerciseId = value; }
        public string ExerciseName { get => exerciseName; set => exerciseName = value; }
        public string MuscleGroup { get => muscleGroup; set => muscleGroup = value; }
        public string SecondaryMuscleGroup { get => secondaryMuscleGroup; set => secondaryMuscleGroup = value; }
        public int OrderNumber { get => orderNumber; set => orderNumber = value; }
        public List<WorkoutSet> Sets { get => sets; set => sets = value; }
        public Workout Workout { get; set; }
        public Exercise Exercise { get; set; }
    }
}
