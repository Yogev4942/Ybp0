using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainersController : ControllerBase
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IUserRepository _userRepository;

    public TrainersController(ITrainerRepository trainerRepository, IUserRepository userRepository)
    {
        _trainerRepository = trainerRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerViewModel>>> Get([FromQuery] string? search)
    {
        IEnumerable<Trainer> trainers = string.IsNullOrWhiteSpace(search)
            ? await _trainerRepository.GetAllAsync()
            : await _trainerRepository.SearchAsync(search);

        return Ok(trainers.Select(ApiMappings.ToTrainerViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrainerViewModel>> GetById(int id)
    {
        Trainer? trainer = await _trainerRepository.GetByIdAsync(id);
        if (trainer is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToTrainerViewModel(trainer));
    }

    [HttpGet("profile/{trainerProfileId:int}")]
    public async Task<ActionResult<TrainerViewModel>> GetByProfileId(int trainerProfileId)
    {
        Trainer? trainer = await _trainerRepository.GetByProfileIdAsync(trainerProfileId);
        if (trainer is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToTrainerViewModel(trainer));
    }

    [HttpPost]
    public async Task<ActionResult<TrainerViewModel>> Post([FromBody] CreateTrainerRequest request)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            return Conflict("Username is already taken.");
        }

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return Conflict("Email is already taken.");
        }

        var trainer = new Trainer
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            Bio = request.Bio,
            Gender = request.Gender,
            Specialization = request.Specialization,
            HourlyRate = request.HourlyRate,
            MaxTrainees = request.MaxTrainees
        };

        await _trainerRepository.AddAsync(trainer);
        return CreatedAtAction(nameof(GetById), new { id = trainer.Id }, ApiMappings.ToTrainerViewModel(trainer));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TrainerViewModel>> Put(int id, [FromBody] UpdateTrainerRequest request)
    {
        Trainer? trainer = await _trainerRepository.GetByIdAsync(id);
        if (trainer is null)
        {
            return NotFound();
        }

        trainer.Bio = request.Bio;
        trainer.Gender = request.Gender;
        trainer.Specialization = request.Specialization;
        trainer.HourlyRate = request.HourlyRate;
        trainer.MaxTrainees = request.MaxTrainees;
        trainer.TotalTrainees = request.TotalTrainees;
        trainer.Rating = request.Rating;
        trainer.TotalRatings = request.TotalRatings;

        await _trainerRepository.UpdateAsync(trainer);
        return Ok(ApiMappings.ToTrainerViewModel(trainer));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Trainer? trainer = await _trainerRepository.GetByIdAsync(id);
        if (trainer is null)
        {
            return NotFound();
        }

        await _trainerRepository.DeleteAsync(id);
        return NoContent();
    }
}
