using Models;

namespace DataBase.Interfaces;

public interface ITraineeRepository
{
    Task<IEnumerable<Trainee>> GetAllAsync();
    Task<Trainee?> GetByIdAsync(int id);
    Task AddAsync(Trainee item);
    Task UpdateAsync(Trainee item);
    Task DeleteAsync(int id);
    Task<IEnumerable<Trainee>> GetByTrainerIdAsync(int trainerId);
    Task<Trainee?> GetByProfileIdAsync(int traineeProfileId);
}
