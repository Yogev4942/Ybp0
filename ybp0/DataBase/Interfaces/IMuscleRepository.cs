using Models;

namespace DataBase.Interfaces;

public interface IMuscleRepository
{
    Task<IEnumerable<Muscle>> GetAllAsync();
    Task<Muscle?> GetByIdAsync(int id);
    Task AddAsync(Muscle item);
    Task UpdateAsync(Muscle item);
    Task DeleteAsync(int id);
}
