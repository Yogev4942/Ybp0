namespace Models
{
    public class WorkoutSessionSet : BaseEntity
    {
        private int workoutSessionId;
        private int exerciseId;
        private int setNumber;
        private int reps;
        private double weight;
        private bool isCompleted;

        public int WorkoutSessionId { get => workoutSessionId; set => workoutSessionId = value; }
        public int ExerciseId { get => exerciseId; set => exerciseId = value; }
        public int SetNumber { get => setNumber; set => setNumber = value; }
        public int Reps { get => reps; set => reps = value; }
        public double Weight { get => weight; set => weight = value; }
        public bool IsCompleted { get => isCompleted; set => isCompleted = value; }
    }
}
