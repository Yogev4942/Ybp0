namespace ViewModels.Api;

public record UserViewModel(
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
