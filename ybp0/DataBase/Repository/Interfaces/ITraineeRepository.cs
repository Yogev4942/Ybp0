using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface ITraineeRepository
    {
        Trainee GetTraineeById(int userId);
        List<Trainee> GetTraineesByTrainerId(int trainerId);
        bool CreateTraineeProfile(int userId, string fitnessGoal, double currentWeight, double height);
        bool UpdateTraineeProfile(Trainee trainee);
        bool AssignTrainer(int traineeUserId, int trainerId);
    }
}
