using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class WeekPlanDays : BaseEntity
    {
        private int weekplanId;
        private int dayOfWeek;
        private int workoutId;
        private bool restDay;

        public int WeekplanId { get => weekplanId; set => weekplanId = value; }
        public int DayOfWeek { get => dayOfWeek; set => dayOfWeek = value; }
        public int WorkoutId { get => workoutId; set => workoutId = value; }
        public bool RestDay { get => restDay; set => restDay = value; }
    }
}
