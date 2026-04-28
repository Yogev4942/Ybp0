using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class WeekPlanRepository : IWeekPlanRepository
{
    private static readonly string[] DayNames =
    {
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    };

    private readonly AppDbContext _context;

    public WeekPlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WeekPlan>> GetAllAsync()
    {
        return await Query().OrderBy(plan => plan.Id).ToListAsync();
    }

    public Task<WeekPlan?> GetByIdAsync(int id)
    {
        return Query().FirstOrDefaultAsync(plan => plan.Id == id);
    }

    public async Task AddAsync(WeekPlan item)
    {
        item.Days = NormalizeDays(item.Days);
        await _context.WeekPlans.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WeekPlan item)
    {
        WeekPlan? existing = await _context.WeekPlans
            .Include(plan => plan.Days)
            .FirstOrDefaultAsync(plan => plan.Id == item.Id);

        if (existing is null)
        {
            return;
        }

        existing.UserId = item.UserId;
        existing.PlanName = item.PlanName;

        _context.WeekPlanDays.RemoveRange(existing.Days);
        existing.Days = NormalizeDays(item.Days);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        WeekPlan? plan = await _context.WeekPlans.FirstOrDefaultAsync(item => item.Id == id);
        if (plan is null)
        {
            return;
        }

        _context.WeekPlans.Remove(plan);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WeekPlan>> GetByUserIdAsync(int userId)
    {
        return await Query()
            .Where(plan => plan.UserId == userId)
            .OrderBy(plan => plan.Id)
            .ToListAsync();
    }

    public async Task<WeekPlanDay?> GetDayByIdAsync(int id)
    {
        return await _context.WeekPlanDays
            .Include(day => day.Workout)
            .FirstOrDefaultAsync(day => day.Id == id);
    }

    private IQueryable<WeekPlan> Query()
    {
        return _context.WeekPlans
            .Include(plan => plan.Days.OrderBy(day => day.Id))
            .ThenInclude(day => day.Workout);
    }

    private static List<WeekPlanDay> NormalizeDays(IEnumerable<WeekPlanDay>? days)
    {
        List<WeekPlanDay> normalized = (days ?? Enumerable.Empty<WeekPlanDay>()).ToList();
        if (normalized.Count == 0)
        {
            normalized = DayNames
                .Select(dayName => new WeekPlanDay
                {
                    DayOfWeek = dayName,
                    RestDay = true
                })
                .ToList();
        }

        return normalized;
    }
}
