using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public abstract class User : BaseEntity
    {
        // Common properties for all users
        protected string email;
        protected string username;
        protected string password;
        protected string joindate;
        protected bool isTrainer;
        protected string bio;
        protected string birthdate;
        protected string gender;

        // Public properties
        public string Email { get => email; set => email = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Joindate { get => joindate; set => joindate = value; }
        public string Bio { get => bio; set => bio = value; }
        public string Gender { get => gender; set => gender = value; }
        public bool IsTrainer { get => isTrainer; set => isTrainer = value; }
        public string Birthdate { get => birthdate; set => birthdate = value; }

        // Abstract method that each user type must implement
        public abstract string GetUserType();

        // Virtual methods that can be overridden
        public virtual bool CanCreatePosts() => false;
        public virtual bool CanAccessOtherUserData() => false;
        public virtual bool CanModifyOtherUserWorkouts() => false;
    }
}
