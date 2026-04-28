using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class TraineeRepository : ITraineeRepository
{
    private readonly AppDbContext _context;

    public TraineeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Trainee>> GetAllAsync()
    {
        return await _context.Trainees
            .Include(trainee => trainee.AssignedTrainer)
            .OrderBy(trainee => trainee.Username)
            .ToListAsync();
    }

    public async Task<Trainee?> GetByIdAsync(int id)
    {
        return await _context.Trainees
            .Include(trainee => trainee.AssignedTrainer)
            .FirstOrDefaultAsync(trainee => trainee.Id == id);
    }

    public async Task AddAsync(Trainee item)
    {
        item.IsTrainer = false;
        if (item.JoinDate == default)
        {
            item.JoinDate = DateTime.UtcNow;
        }

        item.TraineeProfileId = await GetNextTraineeProfileIdAsync();
        item.AssignedTrainer = await ResolveAssignedTrainerAsync(item.TrainerId);
        await _context.Trainees.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Trainee item)
    {
        item.AssignedTrainer = await ResolveAssignedTrainerAsync(item.TrainerId);
        _context.Trainees.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Trainee? trainee = await _context.Trainees.FirstOrDefaultAsync(item => item.Id == id);
        if (trainee is null)
        {
            return;
        }

        _context.Trainees.Remove(trainee);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Trainee>> GetByTrainerIdAsync(int trainerId)
    {
        return await _context.Trainees
            .Include(trainee => trainee.AssignedTrainer)
            .Where(trainee => trainee.TrainerId == trainerId)
            .OrderBy(trainee => trainee.Username)
            .ToListAsync();
    }

    public async Task<Trainee?> GetByProfileIdAsync(int traineeProfileId)
    {
        return await _context.Trainees
            .Include(trainee => trainee.AssignedTrainer)
            .FirstOrDefaultAsync(trainee => trainee.TraineeProfileId == traineeProfileId);
    }

    private async Task<int> GetNextTraineeProfileIdAsync()
    {
        int? maxId = await _context.Trainees.MaxAsync(trainee => (int?)trainee.TraineeProfileId);
        return (maxId ?? 0) + 1;
    }

    private Task<Trainer?> ResolveAssignedTrainerAsync(int? trainerProfileId)
    {
        if (!trainerProfileId.HasValue)
        {
            return Task.FromResult<Trainer?>(null);
        }

        return _context.Trainers.FirstOrDefaultAsync(trainer => trainer.TrainerProfileId == trainerProfileId.Value);
    }
}
