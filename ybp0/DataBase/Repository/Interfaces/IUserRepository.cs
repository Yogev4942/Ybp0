using DataBase.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface IUserRepository
    {
        UserData GetById(int userId);
        UserData GetByUsername(string username);
        UserData GetByUsernameAndPassword(string username, string password);
        List<UserData> GetAllUsers();

        bool UserExists(string username, string email);
        bool ValidateLogin(string username, string password);

        int CreateUser(UserData userData);

        bool UpdateUserCommon(int userId, string bio, string email);
        bool UpdateCurrentWeekPlanId(int userId, int weekPlanId);

        void DeleteUser(int userId);
    }
}
