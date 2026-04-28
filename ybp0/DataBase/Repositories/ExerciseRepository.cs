using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly AppDbContext _context;

    public ExerciseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync()
    {
        return await Query().OrderBy(exercise => exercise.ExerciseName).ToListAsync();
    }

    public Task<Exercise?> GetByIdAsync(int id)
    {
        return Query().FirstOrDefaultAsync(exercise => exercise.Id == id);
    }

    public async Task AddAsync(Exercise item)
    {
        await _context.Exercises.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Exercise item)
    {
        _context.Exercises.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Exercise? exercise = await _context.Exercises.FirstOrDefaultAsync(item => item.Id == id);
        if (exercise is null)
        {
            return;
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(int muscleGroupId)
    {
        return await Query()
            .Where(exercise => exercise.PrimaryMuscleId == muscleGroupId || exercise.SecondaryMuscleId == muscleGroupId)
            .OrderBy(exercise => exercise.ExerciseName)
            .ToListAsync();
    }

    private IQueryable<Exercise> Query()
    {
        return _context.Exercises
            .Include(exercise => exercise.PrimaryMuscle)
            .Include(exercise => exercise.SecondaryMuscle);
    }
}
