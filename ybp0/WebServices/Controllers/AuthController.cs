using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ITraineeRepository _traineeRepository;
    private readonly IWeekPlanRepository _weekPlanRepository;

    public AuthController(
        IUserRepository userRepository,
        ITrainerRepository trainerRepository,
        ITraineeRepository traineeRepository,
        IWeekPlanRepository weekPlanRepository)
    {
        _userRepository = userRepository;
        _trainerRepository = trainerRepository;
        _traineeRepository = traineeRepository;
        _weekPlanRepository = weekPlanRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserViewModel>> Login([FromBody] LoginRequest request)
    {
        User? user = await _userRepository.AuthenticateAsync(request.Username, request.Password);
        if (user is null)
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(ApiMappings.ToUserViewModel(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserViewModel>> Register([FromBody] RegisterUserRequest request)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            return Conflict("Username is already taken.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && await _userRepository.EmailExistsAsync(request.Email))
        {
            return Conflict("Email is already taken.");
        }

        string role = request.Role?.Trim().ToLowerInvariant() ?? string.Empty;
        if (role == "trainer")
        {
            if (string.IsNullOrWhiteSpace(request.Specialization))
            {
                return BadRequest("Specialization is required for trainer registration.");
            }

            var trainer = new Trainer
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Bio = request.Bio,
                Gender = request.Gender,
                Specialization = request.Specialization,
                HourlyRate = request.HourlyRate ?? 0,
                MaxTrainees = request.MaxTrainees ?? 10,
                TotalTrainees = 0,
                Rating = 0,
                TotalRatings = 0
            };

            await _trainerRepository.AddAsync(trainer);
            return CreatedAtAction(nameof(UsersController.GetById), "Users", new { id = trainer.Id }, ApiMappings.ToUserViewModel(trainer));
        }

        if (role != "trainee")
        {
            return BadRequest("Role must be either 'trainer' or 'trainee'.");
        }

        if (string.IsNullOrWhiteSpace(request.FitnessGoal) || !request.CurrentWeight.HasValue || !request.Height.HasValue)
        {
            return BadRequest("FitnessGoal, CurrentWeight and Height are required for trainee registration.");
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
            CurrentWeight = request.CurrentWeight.Value,
            Height = request.Height.Value
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
        return CreatedAtAction(nameof(UsersController.GetById), "Users", new { id = trainee.Id }, ApiMappings.ToUserViewModel(created ?? trainee));
    }
}
