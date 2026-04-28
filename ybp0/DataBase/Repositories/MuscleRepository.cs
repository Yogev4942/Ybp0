using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class MuscleRepository : IMuscleRepository
{
    private readonly AppDbContext _context;

    public MuscleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Muscle>> GetAllAsync()
    {
        return await _context.Muscles.OrderBy(muscle => muscle.Id).ToListAsync();
    }

    public Task<Muscle?> GetByIdAsync(int id)
    {
        return _context.Muscles.FirstOrDefaultAsync(muscle => muscle.Id == id);
    }

    public async Task AddAsync(Muscle item)
    {
        await _context.Muscles.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Muscle item)
    {
        _context.Muscles.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Muscle? muscle = await _context.Muscles.FirstOrDefaultAsync(item => item.Id == id);
        if (muscle is null)
        {
            return;
        }

        _context.Muscles.Remove(muscle);
        await _context.SaveChangesAsync();
    }
}
