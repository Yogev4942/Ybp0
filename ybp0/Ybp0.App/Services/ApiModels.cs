namespace Ybp0.App.Services;

public record LoginRequest(string Username, string Password);

public record RegisterUserRequest(
    string Username,
    string Email,
    string Password,
    string Role,
    string? Bio,
    string? Gender,
    string? FitnessGoal,
    double? CurrentWeight,
    double? Height,
    int? TrainerId,
    string? Specialization,
    double? HourlyRate,
    int? MaxTrainees);

public record UserDto(
    int Id,
    string Username,
    string? Email,
    DateTime JoinDate,
    string? Bio,
    string? Gender,
    bool IsTrainer,
    bool IsAdmin,
    int? CurrentWeekPlanId,
    int? TrainerProfileId,
    int? TraineeProfileId,
    int? TrainerId,
    string? FitnessGoal,
    double? CurrentWeight,
    double? Height,
    string? Specialization,
    double? HourlyRate,
    int? MaxTrainees,
    int? TotalTrainees,
    double? Rating,
    int? TotalRatings);

public record TrainerDto(
    int Id,
    int TrainerProfileId,
    string Username,
    string? Email,
    DateTime JoinDate,
    string? Bio,
    string? Gender,
    string? Specialization,
    double HourlyRate,
    int MaxTrainees,
    int TotalTrainees,
    double Rating,
    int TotalRatings);
