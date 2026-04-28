using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly ITraineeRepository _traineeRepository;

    public StatsController(IWorkoutSessionRepository workoutSessionRepository, ITraineeRepository traineeRepository)
    {
        _workoutSessionRepository = workoutSessionRepository;
        _traineeRepository = traineeRepository;
    }

    [HttpGet("trainee/{traineeId:int}")]
    public async Task<ActionResult<TraineeStatsViewModel>> GetTraineeStats(int traineeId)
    {
        Trainee? trainee = await _traineeRepository.GetByIdAsync(traineeId);
        if (trainee is null)
        {
            return NotFound();
        }

        List<WorkoutSession> sessions = (await _workoutSessionRepository.GetAllByUserIdAsync(traineeId))
            .Where(session => session.IsCompleted)
            .ToList();

        DateTime utcToday = DateTime.UtcNow.Date;
        int delta = ((int)utcToday.DayOfWeek + 6) % 7;
        DateTime startOfWeek = utcToday.AddDays(-delta);
        DateTime endOfWeek = startOfWeek.AddDays(7);

        int totalWorkouts = sessions.Count;
        int workoutsThisWeek = sessions.Count(session => session.SessionDate.Date >= startOfWeek && session.SessionDate.Date < endOfWeek);

        List<string> workedMuscles = sessions
            .SelectMany(session => session.Exercises ?? new List<WorkoutSessionExercise>())
            .SelectMany(exercise => new[] { exercise.MuscleGroup, exercise.SecondaryMuscleGroup })
            .Where(muscle => !string.IsNullOrWhiteSpace(muscle))
            .Select(muscle => muscle!)
            .ToList();

        string? mostWorkedMuscle = workedMuscles
            .GroupBy(muscle => muscle)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .FirstOrDefault();

        IReadOnlyList<string> bodyMapKeys = sessions
            .SelectMany(session => session.Exercises ?? new List<WorkoutSessionExercise>())
            .SelectMany(exercise => new[]
            {
                exercise.Exercise?.PrimaryMuscle?.BodyMapKey,
                exercise.Exercise?.SecondaryMuscle?.BodyMapKey
            })
            .Where(bodyMapKey => !string.IsNullOrWhiteSpace(bodyMapKey))
            .Distinct()
            .OrderBy(bodyMapKey => bodyMapKey)
            .ToList()!;

        return Ok(new TraineeStatsViewModel(totalWorkouts, workoutsThisWeek, mostWorkedMuscle, bodyMapKeys));
    }
}
