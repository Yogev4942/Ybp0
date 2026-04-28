using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutSessionsController : ControllerBase
{
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public WorkoutSessionsController(
        IWorkoutSessionRepository workoutSessionRepository,
        IUserRepository userRepository,
        IExerciseRepository exerciseRepository)
    {
        _workoutSessionRepository = workoutSessionRepository;
        _userRepository = userRepository;
        _exerciseRepository = exerciseRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkoutSessionViewModel>>> GetAll([FromQuery] int? userId)
    {
        IEnumerable<WorkoutSession> sessions = userId.HasValue
            ? await _workoutSessionRepository.GetAllByUserIdAsync(userId.Value)
            : await _workoutSessionRepository.GetAllAsync();

        return Ok(sessions.Select(ApiMappings.ToWorkoutSessionViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkoutSessionViewModel>> GetById(int id)
    {
        WorkoutSession? session = await _workoutSessionRepository.GetByIdAsync(id);
        if (session is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToWorkoutSessionViewModel(session));
    }

    [HttpGet("active/{userId:int}")]
    public async Task<ActionResult<WorkoutSessionViewModel>> GetActive(int userId)
    {
        WorkoutSession? session = await _workoutSessionRepository.GetActiveByUserIdAsync(userId);
        if (session is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToWorkoutSessionViewModel(session));
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutSessionViewModel>> Post([FromBody] CreateWorkoutSessionRequest request)
    {
        if (await _userRepository.GetByIdAsync(request.UserId) is null)
        {
            return BadRequest("User does not exist.");
        }

        foreach (CreateWorkoutSessionExerciseRequest exerciseRequest in request.Exercises)
        {
            if (await _exerciseRepository.GetByIdAsync(exerciseRequest.ExerciseId) is null)
            {
                return BadRequest($"Exercise {exerciseRequest.ExerciseId} does not exist.");
            }
        }

        if (!Enum.TryParse<SessionMode>(request.Mode, true, out SessionMode mode))
        {
            return BadRequest("Invalid session mode.");
        }

        var session = new WorkoutSession
        {
            UserId = request.UserId,
            WorkoutId = request.WorkoutId,
            WeekPlanDayId = request.WeekPlanDayId,
            SessionDate = request.SessionDate ?? DateTime.UtcNow.Date,
            StartTime = request.StartTime ?? DateTime.UtcNow,
            EndTime = request.EndTime,
            IsCompleted = request.IsCompleted,
            Mode = mode,
            WorkoutName = request.WorkoutName,
            Exercises = request.Exercises.Select(ToWorkoutSessionExercise).ToList()
        };

        await _workoutSessionRepository.AddAsync(session);

        WorkoutSession? created = await _workoutSessionRepository.GetByIdAsync(session.Id);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, ApiMappings.ToWorkoutSessionViewModel(created ?? session));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkoutSessionViewModel>> Put(int id, [FromBody] UpdateWorkoutSessionRequest request)
    {
        WorkoutSession? existing = await _workoutSessionRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<SessionMode>(request.Mode, true, out SessionMode mode))
        {
            return BadRequest("Invalid session mode.");
        }

        existing.WorkoutId = request.WorkoutId;
        existing.WeekPlanDayId = request.WeekPlanDayId;
        existing.SessionDate = request.SessionDate;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
        existing.IsCompleted = request.IsCompleted;
        existing.Mode = mode;
        existing.WorkoutName = request.WorkoutName;
        existing.Exercises = request.Exercises.Select(ToWorkoutSessionExercise).ToList();

        await _workoutSessionRepository.UpdateAsync(existing);
        WorkoutSession? updated = await _workoutSessionRepository.GetByIdAsync(id);
        return Ok(ApiMappings.ToWorkoutSessionViewModel(updated ?? existing));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        WorkoutSession? session = await _workoutSessionRepository.GetByIdAsync(id);
        if (session is null)
        {
            return NotFound();
        }

        await _workoutSessionRepository.DeleteAsync(id);
        return NoContent();
    }

    private static WorkoutSessionExercise ToWorkoutSessionExercise(CreateWorkoutSessionExerciseRequest request)
    {
        return new WorkoutSessionExercise
        {
            ExerciseId = request.ExerciseId,
            Sets = request.Sets.Select(set => new WorkoutSessionSet
            {
                ExerciseId = request.ExerciseId,
                SetNumber = set.SetNumber,
                Reps = set.Reps,
                Weight = set.Weight,
                IsCompleted = set.IsCompleted
            }).ToList()
        };
    }
}
