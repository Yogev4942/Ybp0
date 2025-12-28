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
        protected string profilePicture;
        protected string bio;
        protected int age;
        protected string gender;

        // Public properties
        public string Email { get => email; set => email = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Joindate { get => joindate; set => joindate = value; }
        public string ProfilePicture { get => profilePicture; set => profilePicture = value; }
        public string Bio { get => bio; set => bio = value; }
        public int Age { get => age; set => age = value; }
        public string Gender { get => gender; set => gender = value; }

        // Abstract method that each user type must implement
        public abstract string GetUserType();

        // Virtual methods that can be overridden
        public virtual bool CanCreatePosts() => false;
        public virtual bool CanAccessOtherUserData() => false;
        public virtual bool CanModifyOtherUserWorkouts() => false;
    }
}
