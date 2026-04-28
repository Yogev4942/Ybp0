using System;
using System.Collections.Generic;

namespace Models
{
    public class Trainee : User
    {
        private int traineeProfileId;
        private int? trainerId;
        private string fitnessGoal;
        private double currentWeight;
        private double height;

        public int TraineeProfileId { get => traineeProfileId; set => traineeProfileId = value; }
        public int? TrainerId { get => trainerId; set => trainerId = value; }
        public string FitnessGoal { get => fitnessGoal; set => fitnessGoal = value; }
        public double CurrentWeight { get => currentWeight; set => currentWeight = value; }
        public double Height { get => height; set => height = value; }
        public Trainer AssignedTrainer { get; set; }
        public List<Message> SentMessages { get; set; } = new List<Message>();
        public List<Message> ReceivedMessages { get; set; } = new List<Message>();

        public double BMI => height > 0 ? currentWeight / Math.Pow(height / 100.0, 2) : 0;

        public override string GetUserType() => "Trainee";
        public override bool CanCreatePosts() => false;
        public override bool CanAccessOtherUserData() => false;
        public override bool CanModifyOtherUserWorkouts() => false;
    }
}
