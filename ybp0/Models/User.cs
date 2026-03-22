using System;

namespace Models
{
    public abstract class User : BaseEntity
    {
        private string email;
        private string username;
        private string password;
        private DateTime joinDate;
        private bool isTrainer;
        private bool isAdmin;
        private string bio;
        private string gender;
        private int? currentWeekPlanId;

        public string Email { get => email; set => email = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public DateTime JoinDate { get => joinDate; set => joinDate = value; }
        public string Joindate
        {
            get => JoinDate == default(DateTime) ? string.Empty : JoinDate.ToShortDateString();
            set
            {
                if (DateTime.TryParse(value, out DateTime parsedDate))
                {
                    joinDate = parsedDate;
                }
            }
        }
        public string Bio { get => bio; set => bio = value; }
        public string Gender { get => gender; set => gender = value; }
        public bool IsTrainer { get => isTrainer; set => isTrainer = value; }
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public int? CurrentWeekPlanId { get => currentWeekPlanId; set => currentWeekPlanId = value; }

        public abstract string GetUserType();

        public virtual bool CanCreatePosts() => false;
        public virtual bool CanAccessOtherUserData() => false;
        public virtual bool CanModifyOtherUserWorkouts() => false;
    }
}
