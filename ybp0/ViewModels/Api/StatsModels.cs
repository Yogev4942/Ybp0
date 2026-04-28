namespace ViewModels.Api;

public record TraineeStatsViewModel(
    int TotalWorkouts,
    int WorkoutsThisWeek,
    string? MostWorkedMuscle,
    IReadOnlyList<string> MusclesWorked);
