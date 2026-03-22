namespace Models
{
    public class WeekPlanDay : BaseEntity
    {
        private int weekPlanId;
        private string dayOfWeek;
        private int? workoutId;
        private bool restDay;
        private string workoutName;

        public int WeekPlanId { get => weekPlanId; set => weekPlanId = value; }
        public string DayOfWeek { get => dayOfWeek; set => dayOfWeek = value; }
        public int? WorkoutId { get => workoutId; set => workoutId = value; }
        public bool RestDay { get => restDay; set => restDay = value; }
        public string WorkoutName { get => workoutName; set => workoutName = value; }
        public Workout Workout { get; set; }
    }
}
