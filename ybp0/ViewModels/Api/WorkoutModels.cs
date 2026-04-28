namespace ViewModels.Api;

public record WorkoutSetViewModel(int Id, int SetNumber, int Reps, double Weight);

public record WorkoutExerciseViewModel(
    int Id,
    int ExerciseId,
    string ExerciseName,
    string? MuscleGroup,
    string? SecondaryMuscleGroup,
    int OrderNumber,
    IReadOnlyList<WorkoutSetViewModel> Sets);

public record WorkoutViewModel(
    int Id,
    int UserId,
    string WorkoutName,
    IReadOnlyList<WorkoutExerciseViewModel> WorkoutExercises);

public record CreateWorkoutSetRequest(int SetNumber, int Reps, double Weight);

public record CreateWorkoutExerciseRequest(
    int ExerciseId,
    int OrderNumber,
    IReadOnlyList<CreateWorkoutSetRequest> Sets);

public record CreateWorkoutRequest(
    int UserId,
    string WorkoutName,
    IReadOnlyList<CreateWorkoutExerciseRequest> WorkoutExercises);

public record UpdateWorkoutRequest(
    int UserId,
    string WorkoutName,
    IReadOnlyList<CreateWorkoutExerciseRequest> WorkoutExercises);
