using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeekPlansController : ControllerBase
{
    private readonly IWeekPlanRepository _weekPlanRepository;
    private readonly IUserRepository _userRepository;

    public WeekPlansController(IWeekPlanRepository weekPlanRepository, IUserRepository userRepository)
    {
        _weekPlanRepository = weekPlanRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeekPlanViewModel>>> GetAll([FromQuery] int? userId)
    {
        IEnumerable<WeekPlan> plans = userId.HasValue
            ? await _weekPlanRepository.GetByUserIdAsync(userId.Value)
            : await _weekPlanRepository.GetAllAsync();

        return Ok(plans.Select(ApiMappings.ToWeekPlanViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WeekPlanViewModel>> GetById(int id)
    {
        WeekPlan? plan = await _weekPlanRepository.GetByIdAsync(id);
        if (plan is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToWeekPlanViewModel(plan));
    }

    [HttpGet("day/{id:int}")]
    public async Task<ActionResult<WeekPlanDayViewModel>> GetDayById(int id)
    {
        WeekPlanDay? day = await _weekPlanRepository.GetDayByIdAsync(id);
        if (day is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToWeekPlanDayViewModel(day));
    }

    [HttpPost]
    public async Task<ActionResult<WeekPlanViewModel>> Post([FromBody] CreateWeekPlanRequest request)
    {
        if (await _userRepository.GetByIdAsync(request.UserId) is null)
        {
            return BadRequest("User does not exist.");
        }

        var plan = new WeekPlan
        {
            UserId = request.UserId,
            PlanName = request.PlanName,
            Days = request.Days?.Select(day => new WeekPlanDay
            {
                DayOfWeek = day.DayOfWeek,
                WorkoutId = day.WorkoutId,
                RestDay = day.RestDay
            }).ToList()
        };

        await _weekPlanRepository.AddAsync(plan);

        Models.User? user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is not null)
        {
            user.CurrentWeekPlanId = plan.Id;
            await _userRepository.UpdateAsync(user);
        }

        WeekPlan? created = await _weekPlanRepository.GetByIdAsync(plan.Id);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, ApiMappings.ToWeekPlanViewModel(created ?? plan));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WeekPlanViewModel>> Put(int id, [FromBody] UpdateWeekPlanRequest request)
    {
        WeekPlan? plan = await _weekPlanRepository.GetByIdAsync(id);
        if (plan is null)
        {
            return NotFound();
        }

        plan.PlanName = request.PlanName;
        plan.Days = request.Days?.Select(day => new WeekPlanDay
        {
            DayOfWeek = day.DayOfWeek,
            WorkoutId = day.WorkoutId,
            RestDay = day.RestDay
        }).ToList();

        await _weekPlanRepository.UpdateAsync(plan);
        WeekPlan? updated = await _weekPlanRepository.GetByIdAsync(id);
        return Ok(ApiMappings.ToWeekPlanViewModel(updated ?? plan));
    }

    [HttpPut("day/{id:int}")]
    public async Task<ActionResult<WeekPlanDayViewModel>> PutDay(int id, [FromBody] UpdateWeekPlanDayRequest request)
    {
        WeekPlanDay? day = await _weekPlanRepository.GetDayByIdAsync(id);
        if (day is null)
        {
            return NotFound();
        }

        day.WorkoutId = request.WorkoutId;
        day.RestDay = request.RestDay;

        WeekPlan? plan = await _weekPlanRepository.GetByIdAsync(day.WeekPlanId);
        if (plan is null)
        {
            return NotFound();
        }

        foreach (WeekPlanDay existingDay in plan.Days)
        {
            if (existingDay.Id != id)
            {
                continue;
            }

            existingDay.WorkoutId = request.WorkoutId;
            existingDay.RestDay = request.RestDay;
            break;
        }

        await _weekPlanRepository.UpdateAsync(plan);
        WeekPlanDay? updatedDay = await _weekPlanRepository.GetDayByIdAsync(id);
        return Ok(ApiMappings.ToWeekPlanDayViewModel(updatedDay ?? day));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        WeekPlan? plan = await _weekPlanRepository.GetByIdAsync(id);
        if (plan is null)
        {
            return NotFound();
        }

        await _weekPlanRepository.DeleteAsync(id);
        return NoContent();
    }
}
