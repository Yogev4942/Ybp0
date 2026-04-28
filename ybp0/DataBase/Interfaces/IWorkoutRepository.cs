using Models;

namespace DataBase.Interfaces;

public interface IWorkoutRepository
{
    Task<IEnumerable<Workout>> GetAllAsync();
    Task<Workout?> GetByIdAsync(int id);
    Task AddAsync(Workout item);
    Task UpdateAsync(Workout item);
    Task DeleteAsync(int id);
    Task<IEnumerable<Workout>> GetAllByTraineeIdAsync(int traineeId);
}
