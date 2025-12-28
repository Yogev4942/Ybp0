using System;
using System.Collections.Generic;

namespace Models
{
    enum ActivityLevel
    {
        Sedentary,
        LightlyActive,
        ModeratelyActive, 
        VeryActive
    }
    public class Trainee : User
    {
        // Trainee-specific properties
        private int? trainerId;
        private string fitnessGoal;           // "Weight Loss", "Muscle Gain", "Endurance", "General Fitness"
        private double currentWeight;
        private double height;
        private ActivityLevel activityLevel;         // "Sedentary", "Lightly Active", "Moderately Active", "Very Active"
        private int currentWeekPlanId;

        public int? TrainerId { get => trainerId; set => trainerId = value; }
        public string FitnessGoal { get => fitnessGoal; set => fitnessGoal = value; }
        public double CurrentWeight { get => currentWeight; set => currentWeight = value; }
        public double Height { get => height; set => height = value; }
        internal ActivityLevel ActivityLevel { get => activityLevel; set => activityLevel = value; }

        public int CurrentWeekPlanId { get => currentWeekPlanId; set => currentWeekPlanId = value; }
        public Trainer AssignedTrainer { get; set; }

        // Calculated properties
        public double BMI => height > 0 ? currentWeight / (height * height) : 0;

        public override string GetUserType() => "Trainee";

        // Trainees cannot create posts or access other user data
        public override bool CanCreatePosts() => false;
        public override bool CanAccessOtherUserData() => false;
        public override bool CanModifyOtherUserWorkouts() => false;
    }
}