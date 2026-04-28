using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUserRepository _userRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public WorkoutsController(
        IWorkoutRepository workoutRepository,
        IUserRepository userRepository,
        IExerciseRepository exerciseRepository)
    {
        _workoutRepository = workoutRepository;
        _userRepository = userRepository;
        _exerciseRepository = exerciseRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkoutViewModel>>> GetAll([FromQuery] int? userId)
    {
        IEnumerable<Workout> workouts = userId.HasValue
            ? await _workoutRepository.GetAllByTraineeIdAsync(userId.Value)
            : await _workoutRepository.GetAllAsync();

        return Ok(workouts.Select(ApiMappings.ToWorkoutViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkoutViewModel>> GetById(int id)
    {
        Workout? workout = await _workoutRepository.GetByIdAsync(id);
        if (workout is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToWorkoutViewModel(workout));
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutViewModel>> Post([FromBody] CreateWorkoutRequest request)
    {
        if (await _userRepository.GetByIdAsync(request.UserId) is null)
        {
            return BadRequest("User does not exist.");
        }

        foreach (CreateWorkoutExerciseRequest exerciseRequest in request.WorkoutExercises)
        {
            if (await _exerciseRepository.GetByIdAsync(exerciseRequest.ExerciseId) is null)
            {
                return BadRequest($"Exercise {exerciseRequest.ExerciseId} does not exist.");
            }
        }

        var workout = new Workout
        {
            UserId = request.UserId,
            WorkoutName = request.WorkoutName,
            WorkoutExercises = request.WorkoutExercises.Select(ToWorkoutExercise).ToList()
        };

        await _workoutRepository.AddAsync(workout);

        Workout? created = await _workoutRepository.GetByIdAsync(workout.Id);
        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, ApiMappings.ToWorkoutViewModel(created ?? workout));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkoutViewModel>> Put(int id, [FromBody] UpdateWorkoutRequest request)
    {
        Workout? existing = await _workoutRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.UserId = request.UserId;
        existing.WorkoutName = request.WorkoutName;
        existing.WorkoutExercises = request.WorkoutExercises.Select(ToWorkoutExercise).ToList();

        await _workoutRepository.UpdateAsync(existing);
        Workout? updated = await _workoutRepository.GetByIdAsync(id);
        return Ok(ApiMappings.ToWorkoutViewModel(updated ?? existing));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Workout? workout = await _workoutRepository.GetByIdAsync(id);
        if (workout is null)
        {
            return NotFound();
        }

        await _workoutRepository.DeleteAsync(id);
        return NoContent();
    }

    private static WorkoutExercise ToWorkoutExercise(CreateWorkoutExerciseRequest request)
    {
        return new WorkoutExercise
        {
            ExerciseId = request.ExerciseId,
            OrderNumber = request.OrderNumber,
            Sets = request.Sets.Select(set => new WorkoutSet
            {
                SetNumber = set.SetNumber,
                Reps = set.Reps,
                Weight = set.Weight
            }).ToList()
        };
    }
}
