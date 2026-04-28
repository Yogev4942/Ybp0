namespace ViewModels.Api;

public record TraineeViewModel(
    int Id,
    int TraineeProfileId,
    string Username,
    string? Email,
    DateTime JoinDate,
    string? Bio,
    string? Gender,
    int? TrainerId,
    string? TrainerName,
    string? FitnessGoal,
    double CurrentWeight,
    double Height,
    double Bmi);

public record CreateTraineeRequest(
    string Username,
    string Email,
    string Password,
    string? Bio,
    string? Gender,
    int? TrainerId,
    string FitnessGoal,
    double CurrentWeight,
    double Height);

public record UpdateTraineeRequest(
    int? TrainerId,
    string? Bio,
    string? Gender,
    string? FitnessGoal,
    double CurrentWeight,
    double Height);
