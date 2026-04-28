using Models;

namespace DataBase.Interfaces;

public interface IExerciseRepository
{
    Task<IEnumerable<Exercise>> GetAllAsync();
    Task<Exercise?> GetByIdAsync(int id);
    Task AddAsync(Exercise item);
    Task UpdateAsync(Exercise item);
    Task DeleteAsync(int id);
    Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(int muscleGroupId);
}
