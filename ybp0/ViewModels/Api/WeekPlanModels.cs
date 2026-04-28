namespace ViewModels.Api;

public record WeekPlanDayViewModel(
    int Id,
    int WeekPlanId,
    string DayOfWeek,
    int? WorkoutId,
    string? WorkoutName,
    bool RestDay);

public record WeekPlanViewModel(
    int Id,
    int UserId,
    string PlanName,
    IReadOnlyList<WeekPlanDayViewModel> Days);

public record CreateWeekPlanDayRequest(
    string DayOfWeek,
    int? WorkoutId,
    bool RestDay);

public record CreateWeekPlanRequest(
    int UserId,
    string PlanName,
    IReadOnlyList<CreateWeekPlanDayRequest>? Days);

public record UpdateWeekPlanRequest(
    string PlanName,
    IReadOnlyList<CreateWeekPlanDayRequest>? Days);

public record UpdateWeekPlanDayRequest(
    int? WorkoutId,
    bool RestDay);
