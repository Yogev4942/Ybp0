using Models;

namespace DataBase.Interfaces;

public interface IWeekPlanRepository
{
    Task<IEnumerable<WeekPlan>> GetAllAsync();
    Task<WeekPlan?> GetByIdAsync(int id);
    Task AddAsync(WeekPlan item);
    Task UpdateAsync(WeekPlan item);
    Task DeleteAsync(int id);
    Task<IEnumerable<WeekPlan>> GetByUserIdAsync(int userId);
    Task<WeekPlanDay?> GetDayByIdAsync(int id);
}
