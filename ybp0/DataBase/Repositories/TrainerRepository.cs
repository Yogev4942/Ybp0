using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class TrainerRepository : ITrainerRepository
{
    private readonly AppDbContext _context;

    public TrainerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Trainer>> GetAllAsync()
    {
        return await _context.Trainers
            .Include(trainer => trainer.AssignedTrainees)
            .OrderBy(trainer => trainer.Username)
            .ToListAsync();
    }

    public async Task<Trainer?> GetByIdAsync(int id)
    {
        return await _context.Trainers
            .Include(trainer => trainer.AssignedTrainees)
            .FirstOrDefaultAsync(trainer => trainer.Id == id);
    }

    public async Task AddAsync(Trainer item)
    {
        item.IsTrainer = true;
        if (item.JoinDate == default)
        {
            item.JoinDate = DateTime.UtcNow;
        }

        item.TrainerProfileId = await GetNextTrainerProfileIdAsync();
        await _context.Trainers.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Trainer item)
    {
        _context.Trainers.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Trainer? trainer = await _context.Trainers.FirstOrDefaultAsync(item => item.Id == id);
        if (trainer is null)
        {
            return;
        }

        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();
    }

    public async Task<Trainer?> GetByProfileIdAsync(int trainerProfileId)
    {
        return await _context.Trainers
            .Include(trainer => trainer.AssignedTrainees)
            .FirstOrDefaultAsync(trainer => trainer.TrainerProfileId == trainerProfileId);
    }

    public async Task<IEnumerable<Trainer>> SearchAsync(string? query)
    {
        IQueryable<Trainer> trainers = _context.Trainers.Include(trainer => trainer.AssignedTrainees);

        if (!string.IsNullOrWhiteSpace(query))
        {
            string search = query.Trim();
            trainers = trainers.Where(trainer =>
                trainer.Username.Contains(search) ||
                trainer.Specialization.Contains(search));
        }

        return await trainers.OrderBy(trainer => trainer.Username).ToListAsync();
    }

    private async Task<int> GetNextTrainerProfileIdAsync()
    {
        int? maxId = await _context.Trainers.MaxAsync(trainer => (int?)trainer.TrainerProfileId);
        return (maxId ?? 0) + 1;
    }
}
