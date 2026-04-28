namespace ViewModels.Api;

public record TrainerViewModel(
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

public record CreateTrainerRequest(
    string Username,
    string Email,
    string Password,
    string? Bio,
    string? Gender,
    string Specialization,
    double HourlyRate,
    int MaxTrainees);

public record UpdateTrainerRequest(
    string? Bio,
    string? Gender,
    string Specialization,
    double HourlyRate,
    int MaxTrainees,
    int TotalTrainees,
    double Rating,
    int TotalRatings);
