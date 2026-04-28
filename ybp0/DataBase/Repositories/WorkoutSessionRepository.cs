using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class WorkoutSessionRepository : IWorkoutSessionRepository
{
    private readonly AppDbContext _context;

    public WorkoutSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkoutSession>> GetAllAsync()
    {
        List<WorkoutSession> sessions = await Query()
            .OrderByDescending(session => session.SessionDate)
            .ThenByDescending(session => session.StartTime)
            .ToListAsync();

        return sessions.Select(PopulateExercises).ToList();
    }

    public async Task<WorkoutSession?> GetByIdAsync(int id)
    {
        WorkoutSession? session = await Query().FirstOrDefaultAsync(item => item.Id == id);
        return session is null ? null : PopulateExercises(session);
    }

    public async Task AddAsync(WorkoutSession item)
    {
        NormalizeSession(item);
        await _context.WorkoutSessions.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WorkoutSession item)
    {
        WorkoutSession? existing = await _context.WorkoutSessions
            .Include(session => session.SessionSets)
            .FirstOrDefaultAsync(session => session.Id == item.Id);

        if (existing is null)
        {
            return;
        }

        existing.UserId = item.UserId;
        existing.WorkoutId = item.WorkoutId;
        existing.WeekPlanDayId = item.WeekPlanDayId;
        existing.SessionDate = item.SessionDate;
        existing.StartTime = item.StartTime;
        existing.EndTime = item.EndTime;
        existing.IsCompleted = item.IsCompleted;
        existing.Mode = item.Mode;
        existing.WorkoutName = item.WorkoutName;

        _context.WorkoutSessionSets.RemoveRange(existing.SessionSets);
        existing.SessionSets = FlattenSessionExercises(item);

        NormalizeSession(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        WorkoutSession? session = await _context.WorkoutSessions.FirstOrDefaultAsync(item => item.Id == id);
        if (session is null)
        {
            return;
        }

        _context.WorkoutSessions.Remove(session);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WorkoutSession>> GetAllByUserIdAsync(int userId)
    {
        List<WorkoutSession> sessions = await Query()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.SessionDate)
            .ThenByDescending(session => session.StartTime)
            .ToListAsync();

        return sessions.Select(PopulateExercises).ToList();
    }

    public async Task<WorkoutSession?> GetActiveByUserIdAsync(int userId)
    {
        WorkoutSession? session = await Query()
            .Where(item => item.UserId == userId && !item.IsCompleted)
            .OrderByDescending(item => item.StartTime)
            .FirstOrDefaultAsync();

        return session is null ? null : PopulateExercises(session);
    }

    private IQueryable<WorkoutSession> Query()
    {
        return _context.WorkoutSessions
            .Include(session => session.Workout)
            .Include(session => session.WeekPlanDay)
            .Include(session => session.SessionSets)
                .ThenInclude(set => set.Exercise)
                    .ThenInclude(exercise => exercise.PrimaryMuscle)
            .Include(session => session.SessionSets)
                .ThenInclude(set => set.Exercise)
                    .ThenInclude(exercise => exercise.SecondaryMuscle);
    }

    private static WorkoutSession PopulateExercises(WorkoutSession session)
    {
        session.Exercises = (session.SessionSets ?? new List<WorkoutSessionSet>())
            .GroupBy(set => set.ExerciseId)
            .Select(group =>
            {
                WorkoutSessionSet firstSet = group.First();
                return new WorkoutSessionExercise
                {
                    WorkoutSessionId = session.Id,
                    ExerciseId = group.Key,
                    ExerciseName = firstSet.Exercise?.ExerciseName ?? string.Empty,
                    MuscleGroup = firstSet.Exercise?.PrimaryMuscle?.MuscleName ?? firstSet.Exercise?.MuscleGroup,
                    SecondaryMuscleGroup = firstSet.Exercise?.SecondaryMuscle?.MuscleName ?? firstSet.Exercise?.SecondaryMuscleGroup,
                    Exercise = firstSet.Exercise,
                    Sets = group.OrderBy(set => set.SetNumber).ToList()
                };
            })
            .OrderBy(exercise => exercise.ExerciseName)
            .ToList();

        return session;
    }

    private static void NormalizeSession(WorkoutSession session)
    {
        if (session.SessionDate == default)
        {
            session.SessionDate = DateTime.UtcNow.Date;
        }

        if (session.StartTime == default)
        {
            session.StartTime = DateTime.UtcNow;
        }

        session.SessionSets = FlattenSessionExercises(session);

        int index = 1;
        foreach (WorkoutSessionSet set in session.SessionSets.OrderBy(set => set.ExerciseId).ThenBy(set => set.SetNumber))
        {
            set.SetNumber = set.SetNumber <= 0 ? index : set.SetNumber;
            index++;
        }
    }

    private static List<WorkoutSessionSet> FlattenSessionExercises(WorkoutSession session)
    {
        if (session.Exercises is null || session.Exercises.Count == 0)
        {
            return session.SessionSets ?? new List<WorkoutSessionSet>();
        }

        return session.Exercises
            .SelectMany(exercise => (exercise.Sets ?? new List<WorkoutSessionSet>())
                .Select(set => new WorkoutSessionSet
                {
                    Id = set.Id,
                    WorkoutSessionId = session.Id,
                    ExerciseId = exercise.ExerciseId,
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    Weight = set.Weight,
                    IsCompleted = set.IsCompleted
                }))
            .OrderBy(set => set.ExerciseId)
            .ThenBy(set => set.SetNumber)
            .ToList();
    }
}
