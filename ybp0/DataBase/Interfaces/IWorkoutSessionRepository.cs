using Models;

namespace DataBase.Interfaces;

public interface IWorkoutSessionRepository
{
    Task<IEnumerable<WorkoutSession>> GetAllAsync();
    Task<WorkoutSession?> GetByIdAsync(int id);
    Task AddAsync(WorkoutSession item);
    Task UpdateAsync(WorkoutSession item);
    Task DeleteAsync(int id);
    Task<IEnumerable<WorkoutSession>> GetAllByUserIdAsync(int userId);
    Task<WorkoutSession?> GetActiveByUserIdAsync(int userId);
}
