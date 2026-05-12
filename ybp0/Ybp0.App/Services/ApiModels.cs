namespace Ybp0.App.Services;

public class LoginRequest
{
    public LoginRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; set; }
    public string Password { get; set; }
}

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

public record WorkoutSetDto(int Id, int SetNumber, int Reps, double Weight);

public record WorkoutPlanExerciseDto(
    int Id,
    int ExerciseId,
    string? ExerciseName,
    string? MuscleGroup,
    string? SecondaryMuscleGroup,
    int OrderNumber,
    IReadOnlyList<WorkoutSetDto>? Sets);

public record WorkoutPlanDto(
    int Id,
    int UserId,
    string? WorkoutName,
    IReadOnlyList<WorkoutPlanExerciseDto>? WorkoutExercises);

public record ExerciseDto(
    int Id,
    string? ExerciseName,
    string? MuscleGroup,
    string? SecondaryMuscleGroup);

public record CreateWorkoutPlanRequest(int UserId, string WorkoutName);

public record RenameWorkoutPlanRequest(string WorkoutName);

public record AddWorkoutExerciseRequest(int ExerciseId);

public record SaveWorkoutSetRequest(int SetNumber, int Reps, double Weight);
