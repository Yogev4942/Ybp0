using System;
using System.Collections.Generic;

namespace Models
{
    public enum SessionMode
    {
        Plan,
        Saved,
        Freestyle
    }

    public class WorkoutSession : BaseEntity
    {
        private int userId;
        private int? workoutId;
        private int? weekPlanDayId;
        private DateTime sessionDate;
        private DateTime startTime;
        private DateTime? endTime;
        private bool isCompleted;
        private SessionMode mode;
        private string workoutName;
        private List<WorkoutSessionExercise> exercises = new List<WorkoutSessionExercise>();

        public int UserId { get => userId; set => userId = value; }
        public int? WorkoutId { get => workoutId; set => workoutId = value; }
        public int? WeekPlanDayId { get => weekPlanDayId; set => weekPlanDayId = value; }
        public DateTime SessionDate { get => sessionDate; set => sessionDate = value; }
        public DateTime StartTime { get => startTime; set => startTime = value; }
        public DateTime? EndTime { get => endTime; set => endTime = value; }
        public bool IsCompleted { get => isCompleted; set => isCompleted = value; }
        public SessionMode Mode { get => mode; set => mode = value; }
        public string WorkoutName { get => workoutName; set => workoutName = value; }
        public List<WorkoutSessionExercise> Exercises { get => exercises; set => exercises = value; }
        public User User { get; set; }
        public Workout Workout { get; set; }
        public WeekPlanDay WeekPlanDay { get; set; }
        public List<WorkoutSessionSet> SessionSets { get; set; } = new List<WorkoutSessionSet>();
    }
}
