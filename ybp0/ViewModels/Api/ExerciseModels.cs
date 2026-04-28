namespace ViewModels.Api;

public record ExerciseViewModel(
    int Id,
    string ExerciseName,
    int? PrimaryMuscleId,
    string? PrimaryMuscleName,
    int? SecondaryMuscleId,
    string? SecondaryMuscleName,
    string? MuscleGroup,
    string? SecondaryMuscleGroup);

public record CreateExerciseRequest(
    string ExerciseName,
    int? PrimaryMuscleId,
    int? SecondaryMuscleId);

public record UpdateExerciseRequest(
    string ExerciseName,
    int? PrimaryMuscleId,
    int? SecondaryMuscleId);
