using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class User : BaseEntity
    {
        private string email;
        private string username;
        private string password;
        private string joindate;

        public string Email { get => email; set => email = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Joindate { get => joindate; set => joindate = value; }
    }
}
