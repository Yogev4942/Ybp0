using DataBase.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataBase.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly AppDbContext _context;

    public WorkoutRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Workout>> GetAllAsync()
    {
        return await Query()
            .OrderBy(workout => workout.WorkoutName)
            .ToListAsync();
    }

    public Task<Workout?> GetByIdAsync(int id)
    {
        return Query().FirstOrDefaultAsync(workout => workout.Id == id);
    }

    public async Task AddAsync(Workout item)
    {
        NormalizeWorkout(item);
        await _context.Workouts.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Workout item)
    {
        Workout? existing = await _context.Workouts
            .Include(workout => workout.WorkoutExercises)
            .ThenInclude(workoutExercise => workoutExercise.Sets)
            .FirstOrDefaultAsync(workout => workout.Id == item.Id);

        if (existing is null)
        {
            return;
        }

        existing.UserId = item.UserId;
        existing.WorkoutName = item.WorkoutName;

        _context.WorkoutSets.RemoveRange(existing.WorkoutExercises.SelectMany(workoutExercise => workoutExercise.Sets));
        _context.WorkoutExercises.RemoveRange(existing.WorkoutExercises);

        existing.WorkoutExercises = new List<WorkoutExercise>();
        foreach (WorkoutExercise exercise in item.WorkoutExercises ?? new List<WorkoutExercise>())
        {
            existing.WorkoutExercises.Add(CloneWorkoutExercise(exercise));
        }

        NormalizeWorkout(existing);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Workout? workout = await _context.Workouts.FirstOrDefaultAsync(item => item.Id == id);
        if (workout is null)
        {
            return;
        }

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Workout>> GetAllByTraineeIdAsync(int traineeId)
    {
        return await Query()
            .Where(workout => workout.UserId == traineeId)
            .OrderBy(workout => workout.WorkoutName)
            .ToListAsync();
    }

    private IQueryable<Workout> Query()
    {
        return _context.Workouts
            .Include(workout => workout.WorkoutExercises)
                .ThenInclude(workoutExercise => workoutExercise.Exercise)
                    .ThenInclude(exercise => exercise.PrimaryMuscle)
            .Include(workout => workout.WorkoutExercises)
                .ThenInclude(workoutExercise => workoutExercise.Exercise)
                    .ThenInclude(exercise => exercise.SecondaryMuscle)
            .Include(workout => workout.WorkoutExercises)
                .ThenInclude(workoutExercise => workoutExercise.Sets);
    }

    private static void NormalizeWorkout(Workout workout)
    {
        if (workout.WorkoutExercises is null)
        {
            workout.WorkoutExercises = new List<WorkoutExercise>();
            return;
        }

        int order = 1;
        foreach (WorkoutExercise exercise in workout.WorkoutExercises)
        {
            exercise.OrderNumber = exercise.OrderNumber <= 0 ? order : exercise.OrderNumber;
            exercise.Sets ??= new List<WorkoutSet>();

            int setNumber = 1;
            foreach (WorkoutSet set in exercise.Sets.OrderBy(set => set.SetNumber).ToList())
            {
                set.SetNumber = set.SetNumber <= 0 ? setNumber : set.SetNumber;
                setNumber++;
            }

            order++;
        }
    }

    private static WorkoutExercise CloneWorkoutExercise(WorkoutExercise source)
    {
        return new WorkoutExercise
        {
            ExerciseId = source.ExerciseId,
            ExerciseName = source.ExerciseName,
            MuscleGroup = source.MuscleGroup,
            SecondaryMuscleGroup = source.SecondaryMuscleGroup,
            OrderNumber = source.OrderNumber,
            Sets = (source.Sets ?? new List<WorkoutSet>())
                .OrderBy(set => set.SetNumber)
                .Select(set => new WorkoutSet
                {
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    Weight = set.Weight
                })
                .ToList()
        };
    }
}
