using System;
using System.Collections.Generic;

namespace Models
{
    public class Trainer : User
    {
        // Trainer-specific properties
        private string specialization;        // "Strength Training", "Weight Loss", "Bodybuilding", etc.
        private int yearsOfExperience;
        private string certifications;        // Comma-separated certifications
        private double hourlyRate;
        private int maxTrainees;              // Maximum number of trainees they can handle
        private string workingHours;          // "Mon-Fri 6AM-8PM"
        private string expertise;             // Detailed expertise description
        private int totalTrainees;            // Current number of trainees
        private double rating;                // Average rating from trainees
        private int totalRatings;             // Number of ratings received

        public string Specialization { get => specialization; set => specialization = value; }
        public int YearsOfExperience { get => yearsOfExperience; set => yearsOfExperience = value; }
        public string Certifications { get => certifications; set => certifications = value; }
        public double HourlyRate { get => hourlyRate; set => hourlyRate = value; }
        public int MaxTrainees { get => maxTrainees; set => maxTrainees = value; }
        public string WorkingHours { get => workingHours; set => workingHours = value; }
        public string Expertise { get => expertise; set => expertise = value; }
        public int TotalTrainees { get => totalTrainees; set => totalTrainees = value; }
        public double Rating { get => rating; set => rating = value; }
        public int TotalRatings { get => totalRatings; set => totalRatings = value; }

        // Collection of assigned trainees
        public List<Trainee> AssignedTrainees { get; set; } = new List<Trainee>();

        // Calculated properties
        public bool CanAcceptMoreTrainees => totalTrainees < maxTrainees;
        public string RatingDisplay => totalRatings > 0 ?
            $"{rating:F1} ({totalRatings} reviews)" : "No ratings yet";

        public override string GetUserType() => "Trainer";

        // Trainers have special permissions
        public override bool CanCreatePosts() => true;
        public override bool CanAccessOtherUserData() => true;
        public override bool CanModifyOtherUserWorkouts() => true;

        // Trainer-specific methods
        public bool HasCapacity() => totalTrainees < maxTrainees;
    }
}