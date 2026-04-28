namespace ViewModels.Api;

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
