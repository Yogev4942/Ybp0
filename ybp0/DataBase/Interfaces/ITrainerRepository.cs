using Models;

namespace DataBase.Interfaces;

public interface ITrainerRepository
{
    Task<IEnumerable<Trainer>> GetAllAsync();
    Task<Trainer?> GetByIdAsync(int id);
    Task AddAsync(Trainer item);
    Task UpdateAsync(Trainer item);
    Task DeleteAsync(int id);
    Task<Trainer?> GetByProfileIdAsync(int trainerProfileId);
    Task<IEnumerable<Trainer>> SearchAsync(string? query);
}
