using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    internal class TraineeData
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public int? TrainerId { get; set; }
        public string FitnessGoal { get; set; }
        public double CurrentWeight { get; set; }
        public double Height { get; set; }
        public int CurrentWeekPlanId { get; set; }

    }
}
