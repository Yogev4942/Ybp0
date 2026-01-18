using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    internal class UserData
    {
        int id;
        string email;
        string username;
        string password;
        DateTime? joinDate;
        bool isTrainer;
        string bio;
        string gender;

        public int ID { get => id; set => id = value; }
        public string Email { get => email; set => email = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public DateTime? JoinDate { get => joinDate; set => joinDate = value; }
        public bool IsTrainer { get => isTrainer; set => isTrainer = value; }
        public string Bio { get => bio; set => bio = value; }
        public string Gender { get => gender; set => gender = value; }
    }
}
