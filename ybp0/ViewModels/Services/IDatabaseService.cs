using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public interface IDatabaseService
    {
        /// <summary>
        /// Returns a user if the username and password match, otherwise null.
        /// </summary>
        User GetUserByUsernameAndPassword(string username, string password);

        /// <summary>
        /// checks if username and password are correct
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool ValidateLogin(string username, string password);

        bool UserExist(string username, string email);
    }

}
