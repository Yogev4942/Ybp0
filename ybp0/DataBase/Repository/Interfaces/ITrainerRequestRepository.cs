using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface ITrainerRequestRepository
    {
        string GetTrainerRequestStatus(int traineeUserId, int trainerUserId);
        bool SendTrainerRequest(int traineeUserId, int trainerUserId);
        bool HandleTrainerRequest(int traineeUserId, int trainerUserId, string status);
        List<Trainee> GetPendingRequests(int trainerUserId);
    }
}
