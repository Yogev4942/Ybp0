namespace ViewModels.Api;

public record WorkoutSessionSetViewModel(
    int Id,
    int ExerciseId,
    int SetNumber,
    int Reps,
    double Weight,
    bool IsCompleted);

public record WorkoutSessionExerciseViewModel(
    int ExerciseId,
    string ExerciseName,
    string? MuscleGroup,
    string? SecondaryMuscleGroup,
    IReadOnlyList<WorkoutSessionSetViewModel> Sets);

public record WorkoutSessionViewModel(
    int Id,
    int UserId,
    int? WorkoutId,
    int? WeekPlanDayId,
    DateTime SessionDate,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsCompleted,
    string Mode,
    string? WorkoutName,
    IReadOnlyList<WorkoutSessionExerciseViewModel> Exercises);

public record CreateWorkoutSessionSetRequest(
    int SetNumber,
    int Reps,
    double Weight,
    bool IsCompleted);

public record CreateWorkoutSessionExerciseRequest(
    int ExerciseId,
    IReadOnlyList<CreateWorkoutSessionSetRequest> Sets);

public record CreateWorkoutSessionRequest(
    int UserId,
    int? WorkoutId,
    int? WeekPlanDayId,
    DateTime? SessionDate,
    DateTime? StartTime,
    DateTime? EndTime,
    bool IsCompleted,
    string Mode,
    string? WorkoutName,
    IReadOnlyList<CreateWorkoutSessionExerciseRequest> Exercises);

public record UpdateWorkoutSessionRequest(
    int? WorkoutId,
    int? WeekPlanDayId,
    DateTime SessionDate,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsCompleted,
    string Mode,
    string? WorkoutName,
    IReadOnlyList<CreateWorkoutSessionExerciseRequest> Exercises);
