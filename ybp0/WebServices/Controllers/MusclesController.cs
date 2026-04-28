using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MusclesController : ControllerBase
{
    private readonly IMuscleRepository _muscleRepository;

    public MusclesController(IMuscleRepository muscleRepository)
    {
        _muscleRepository = muscleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MuscleViewModel>>> Get()
    {
        IEnumerable<Muscle> muscles = await _muscleRepository.GetAllAsync();
        return Ok(muscles.Select(ApiMappings.ToMuscleViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MuscleViewModel>> GetById(int id)
    {
        Muscle? muscle = await _muscleRepository.GetByIdAsync(id);
        if (muscle is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToMuscleViewModel(muscle));
    }

    [HttpPost]
    public async Task<ActionResult<MuscleViewModel>> Post([FromBody] CreateMuscleRequest request)
    {
        var muscle = new Muscle
        {
            MuscleName = request.MuscleName,
            BodyRegion = request.BodyRegion,
            DiagramZone = request.DiagramZone,
            BodyMapKey = request.BodyMapKey
        };

        await _muscleRepository.AddAsync(muscle);
        return CreatedAtAction(nameof(GetById), new { id = muscle.Id }, ApiMappings.ToMuscleViewModel(muscle));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MuscleViewModel>> Put(int id, [FromBody] CreateMuscleRequest request)
    {
        Muscle? muscle = await _muscleRepository.GetByIdAsync(id);
        if (muscle is null)
        {
            return NotFound();
        }

        muscle.MuscleName = request.MuscleName;
        muscle.BodyRegion = request.BodyRegion;
        muscle.DiagramZone = request.DiagramZone;
        muscle.BodyMapKey = request.BodyMapKey;

        await _muscleRepository.UpdateAsync(muscle);
        return Ok(ApiMappings.ToMuscleViewModel(muscle));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Muscle? muscle = await _muscleRepository.GetByIdAsync(id);
        if (muscle is null)
        {
            return NotFound();
        }

        await _muscleRepository.DeleteAsync(id);
        return NoContent();
    }
}
