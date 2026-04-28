namespace ViewModels.Api;

public record MuscleViewModel(
    int Id,
    string MuscleName,
    string? BodyRegion,
    int DiagramZone,
    string BodyMapKey);

public record CreateMuscleRequest(
    string MuscleName,
    string? BodyRegion,
    int DiagramZone,
    string BodyMapKey);
