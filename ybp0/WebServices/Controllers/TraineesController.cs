using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TraineesController : ControllerBase
{
    private readonly ITraineeRepository _traineeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWeekPlanRepository _weekPlanRepository;

    public TraineesController(
        ITraineeRepository traineeRepository,
        IUserRepository userRepository,
        IWeekPlanRepository weekPlanRepository)
    {
        _traineeRepository = traineeRepository;
        _userRepository = userRepository;
        _weekPlanRepository = weekPlanRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TraineeViewModel>>> Get()
    {
        IEnumerable<Trainee> trainees = await _traineeRepository.GetAllAsync();
        return Ok(trainees.Select(ApiMappings.ToTraineeViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TraineeViewModel>> GetById(int id)
    {
        Trainee? trainee = await _traineeRepository.GetByIdAsync(id);
        if (trainee is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToTraineeViewModel(trainee));
    }

    [HttpGet("by-trainer/{trainerId:int}")]
    public async Task<ActionResult<IEnumerable<TraineeViewModel>>> GetByTrainerId(int trainerId)
    {
        IEnumerable<Trainee> trainees = await _traineeRepository.GetByTrainerIdAsync(trainerId);
        return Ok(trainees.Select(ApiMappings.ToTraineeViewModel));
    }

    [HttpPost]
    public async Task<ActionResult<TraineeViewModel>> Post([FromBody] CreateTraineeRequest request)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            return Conflict("Username is already taken.");
        }

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return Conflict("Email is already taken.");
        }

        var trainee = new Trainee
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            Bio = request.Bio,
            Gender = request.Gender,
            TrainerId = request.TrainerId,
            FitnessGoal = request.FitnessGoal,
            CurrentWeight = request.CurrentWeight,
            Height = request.Height
        };

        await _traineeRepository.AddAsync(trainee);

        var weekPlan = new WeekPlan
        {
            UserId = trainee.Id,
            PlanName = "My Week Plan"
        };

        await _weekPlanRepository.AddAsync(weekPlan);
        trainee.CurrentWeekPlanId = weekPlan.Id;
        await _userRepository.UpdateAsync(trainee);

        Trainee? created = await _traineeRepository.GetByIdAsync(trainee.Id);
        return CreatedAtAction(nameof(GetById), new { id = trainee.Id }, ApiMappings.ToTraineeViewModel(created ?? trainee));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TraineeViewModel>> Put(int id, [FromBody] UpdateTraineeRequest request)
    {
        Trainee? trainee = await _traineeRepository.GetByIdAsync(id);
        if (trainee is null)
        {
            return NotFound();
        }

        trainee.TrainerId = request.TrainerId;
        trainee.Bio = request.Bio;
        trainee.Gender = request.Gender;
        trainee.FitnessGoal = request.FitnessGoal;
        trainee.CurrentWeight = request.CurrentWeight;
        trainee.Height = request.Height;

        await _traineeRepository.UpdateAsync(trainee);

        Trainee? updated = await _traineeRepository.GetByIdAsync(id);
        return Ok(ApiMappings.ToTraineeViewModel(updated ?? trainee));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Trainee? trainee = await _traineeRepository.GetByIdAsync(id);
        if (trainee is null)
        {
            return NotFound();
        }

        await _traineeRepository.DeleteAsync(id);
        return NoContent();
    }
}
