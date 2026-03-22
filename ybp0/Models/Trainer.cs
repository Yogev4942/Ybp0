using System.Collections.Generic;

namespace Models
{
    public class Trainer : User
    {
        private int trainerProfileId;
        private string specialization;
        private double hourlyRate;
        private int maxTrainees;
        private int totalTrainees;
        private double rating;
        private int totalRatings;

        public int TrainerProfileId { get => trainerProfileId; set => trainerProfileId = value; }
        public string Specialization { get => specialization; set => specialization = value; }
        public double HourlyRate { get => hourlyRate; set => hourlyRate = value; }
        public int MaxTrainees { get => maxTrainees; set => maxTrainees = value; }
        public int TotalTrainees { get => totalTrainees; set => totalTrainees = value; }
        public double Rating { get => rating; set => rating = value; }
        public int TotalRatings { get => totalRatings; set => totalRatings = value; }

        public List<Trainee> AssignedTrainees { get; set; } = new List<Trainee>();

        public bool CanAcceptMoreTrainees => totalTrainees < maxTrainees;
        public string RatingDisplay => totalRatings > 0
            ? $"{rating:F1} ({totalRatings} reviews)"
            : "No ratings yet";

        public override string GetUserType() => "Trainer";
        public override bool CanCreatePosts() => true;
        public override bool CanAccessOtherUserData() => true;
        public override bool CanModifyOtherUserWorkouts() => true;

        public bool HasCapacity() => totalTrainees < maxTrainees;
    }
}
