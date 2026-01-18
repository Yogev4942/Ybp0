using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    internal class TraineeData
    {
        int id;
        int userId;
        int trainerId;
        string fitnessGoal;
        int currentWeight;
        int height;

        public int Id { get => id; set => id = value; }
        public int UserId { get => userId; set => userId = value; }
        public int TrainerId { get => trainerId; set => trainerId = value; }
        public string FitnessGoal { get => fitnessGoal; set => fitnessGoal = value; }
        public int CurrentWeight { get => currentWeight; set => currentWeight = value; }
        public int Height { get => height; set => height = value; }
    }
}
