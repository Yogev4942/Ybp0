using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface ITrainerRepository
    {
        Trainer GetTrainerById(int userId);
        List<Trainer> SearchTrainers(string query);
        bool CreateTrainerProfile(int userId, string specialization, double hourlyRate, int maxTrainees);
        bool UpdateTrainerProfile(Trainer trainer);
    }
}
