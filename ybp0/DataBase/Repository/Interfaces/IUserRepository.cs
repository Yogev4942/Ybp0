using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace DataBase.Repository.Interfaces
{
    public interface IUserRepository
    {
        User GetById(int userId);
        Dictionary<int, User> GetByIds(IEnumerable<int> userIds);
        User GetByUsername(string username);
        User GetByUsernameAndPassword(string username, string password);
        List<User> GetAllUsers();
        bool UserExists(string username, string email);
        bool ValidateLogin(string username, string password);
        int CreateUser(User userData);
        bool UpdateUserCommon(int userId, string bio, string email);
        bool UpdateCurrentWeekPlanId(int userId, int weekPlanId);
        void DeleteUser(int userId);
    }
}
