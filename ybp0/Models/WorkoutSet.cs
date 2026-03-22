namespace Models
{
    public class WorkoutSet : BaseEntity
    {
        private int workoutExerciseId;
        private int setNumber;
        private int reps;
        private double weight;

        public int WorkoutExerciseId { get => workoutExerciseId; set => workoutExerciseId = value; }
        public int SetNumber { get => setNumber; set => setNumber = value; }
        public int Reps { get => reps; set => reps = value; }
        public double Weight { get => weight; set => weight = value; }
    }
}
