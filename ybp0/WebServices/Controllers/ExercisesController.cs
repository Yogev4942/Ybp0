using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IMuscleRepository _muscleRepository;

    public ExercisesController(IExerciseRepository exerciseRepository, IMuscleRepository muscleRepository)
    {
        _exerciseRepository = exerciseRepository;
        _muscleRepository = muscleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExerciseViewModel>>> Get()
    {
        IEnumerable<Exercise> exercises = await _exerciseRepository.GetAllAsync();
        return Ok(exercises.Select(ApiMappings.ToExerciseViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExerciseViewModel>> GetById(int id)
    {
        Exercise? exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToExerciseViewModel(exercise));
    }

    [HttpGet("by-muscle/{muscleGroupId:int}")]
    public async Task<ActionResult<IEnumerable<ExerciseViewModel>>> GetByMuscle(int muscleGroupId)
    {
        IEnumerable<Exercise> exercises = await _exerciseRepository.GetByMuscleGroupAsync(muscleGroupId);
        return Ok(exercises.Select(ApiMappings.ToExerciseViewModel));
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseViewModel>> Post([FromBody] CreateExerciseRequest request)
    {
        if (request.PrimaryMuscleId.HasValue && await _muscleRepository.GetByIdAsync(request.PrimaryMuscleId.Value) is null)
        {
            return BadRequest("Primary muscle does not exist.");
        }

        if (request.SecondaryMuscleId.HasValue && await _muscleRepository.GetByIdAsync(request.SecondaryMuscleId.Value) is null)
        {
            return BadRequest("Secondary muscle does not exist.");
        }

        var exercise = new Exercise
        {
            ExerciseName = request.ExerciseName,
            PrimaryMuscleId = request.PrimaryMuscleId,
            SecondaryMuscleId = request.SecondaryMuscleId
        };

        await _exerciseRepository.AddAsync(exercise);

        Exercise? created = await _exerciseRepository.GetByIdAsync(exercise.Id);
        return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, ApiMappings.ToExerciseViewModel(created ?? exercise));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExerciseViewModel>> Put(int id, [FromBody] UpdateExerciseRequest request)
    {
        Exercise? exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise is null)
        {
            return NotFound();
        }

        exercise.ExerciseName = request.ExerciseName;
        exercise.PrimaryMuscleId = request.PrimaryMuscleId;
        exercise.SecondaryMuscleId = request.SecondaryMuscleId;

        await _exerciseRepository.UpdateAsync(exercise);

        Exercise? updated = await _exerciseRepository.GetByIdAsync(id);
        return Ok(ApiMappings.ToExerciseViewModel(updated ?? exercise));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Exercise? exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise is null)
        {
            return NotFound();
        }

        await _exerciseRepository.DeleteAsync(id);
        return NoContent();
    }
}
