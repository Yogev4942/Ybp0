using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class WorkoutSession : BaseEntity
    {
        private int userId;
        private int workoutId;
        private int? weekPlanDayId;
        private DateTime sessionDate;
        private bool completed;

        public int UserId { get => userId; set => userId = value; }
        public int WorkoutId { get => workoutId; set => workoutId = value; }
        public int? WeekPlanDayId { get => weekPlanDayId; set => weekPlanDayId = value; }
        public DateTime SessionDate { get => sessionDate; set => sessionDate = value; }
        public bool Completed { get => completed; set => completed = value; }
    }
}
