using System.Collections.ObjectModel;
using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class WorkoutPlansViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private WorkoutPlanItem? _selectedPlan;
    private string _editableWorkoutName = string.Empty;
    private string _previewDayName = "MONDAY";
    private string _previewSummary = "Create a plan and fill it with exercise cards.";
    private bool _isExercisePickerOpen;

    public WorkoutPlansViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        LoadCommand = new AsyncCommand(LoadAsync);
        AddPlanCommand = new AsyncCommand(AddPlanAsync);
        RenamePlanCommand = new AsyncCommand(RenamePlanAsync);
        OpenExercisePickerCommand = new AsyncCommand(OpenExercisePickerAsync);
        CloseExercisePickerCommand = new Command(() => IsExercisePickerOpen = false);
        BackHomeCommand = new AsyncCommand(_navigation.GoToHomeAsync);
        AddExerciseCommand = new Command<ExerciseItem>(async exercise => await AddExerciseAsync(exercise));
        RemoveExerciseCommand = new Command<WorkoutPlanExerciseItem>(async exercise => await RemoveExerciseAsync(exercise));
        AddSetCommand = new Command<WorkoutPlanExerciseItem>(async exercise => await AddSetAsync(exercise));
        RemoveSetCommand = new Command<WorkoutPlanExerciseItem>(async exercise => await RemoveSetAsync(exercise));
    }

    public ObservableCollection<WorkoutPlanItem> WorkoutPlans { get; } = new();
    public ObservableCollection<ExerciseItem> AvailableExercises { get; } = new();

    public WorkoutPlanItem? SelectedPlan
    {
        get => _selectedPlan;
        set
        {
            if (SetProperty(ref _selectedPlan, value))
            {
                EditableWorkoutName = value?.WorkoutName ?? string.Empty;
                UpdatePreviewState();
                OnPropertyChanged(nameof(HasSelectedPlan));
            }
        }
    }

    public string EditableWorkoutName
    {
        get => _editableWorkoutName;
        set => SetProperty(ref _editableWorkoutName, value);
    }

    public string PreviewDayName
    {
        get => _previewDayName;
        set => SetProperty(ref _previewDayName, value);
    }

    public string PreviewSummary
    {
        get => _previewSummary;
        set => SetProperty(ref _previewSummary, value);
    }

    public bool IsExercisePickerOpen
    {
        get => _isExercisePickerOpen;
        set => SetProperty(ref _isExercisePickerOpen, value);
    }

    public bool HasSelectedPlan => SelectedPlan != null;

    public ICommand LoadCommand { get; }
    public ICommand AddPlanCommand { get; }
    public ICommand RenamePlanCommand { get; }
    public ICommand OpenExercisePickerCommand { get; }
    public ICommand CloseExercisePickerCommand { get; }
    public ICommand BackHomeCommand { get; }
    public ICommand AddExerciseCommand { get; }
    public ICommand RemoveExerciseCommand { get; }
    public ICommand AddSetCommand { get; }
    public ICommand RemoveSetCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            int? selectedId = SelectedPlan?.Id;
            IReadOnlyList<WorkoutPlanDto> plans = await _api.GetWorkoutPlansAsync();

            WorkoutPlans.Clear();
            foreach (WorkoutPlanDto plan in plans)
            {
                WorkoutPlans.Add(WorkoutPlanItem.FromDto(plan));
            }

            SelectedPlan = WorkoutPlans.FirstOrDefault(plan => plan.Id == selectedId)
                ?? WorkoutPlans.FirstOrDefault();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not load workout plans. {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddPlanAsync()
    {
        try
        {
            ErrorMessage = null;
            string name = BuildNextWorkoutName();
            WorkoutPlanDto created = await _api.CreateWorkoutPlanAsync(name);
            WorkoutPlanItem item = WorkoutPlanItem.FromDto(created);
            WorkoutPlans.Add(item);
            SelectedPlan = item;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not create workout plan. {exception.Message}";
        }
    }

    private async Task RenamePlanAsync()
    {
        if (SelectedPlan == null)
        {
            return;
        }

        string name = string.IsNullOrWhiteSpace(EditableWorkoutName)
            ? "Workout Plan"
            : EditableWorkoutName.Trim();

        try
        {
            WorkoutPlanDto updated = await _api.RenameWorkoutPlanAsync(SelectedPlan.Id, name);
            SelectedPlan.WorkoutName = updated.WorkoutName ?? name;
            EditableWorkoutName = SelectedPlan.WorkoutName;
            UpdatePreviewState();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not rename workout plan. {exception.Message}";
        }
    }

    private async Task OpenExercisePickerAsync()
    {
        if (SelectedPlan == null)
        {
            return;
        }

        try
        {
            IReadOnlySet<int> existingIds = SelectedPlan.Exercises.Select(exercise => exercise.ExerciseId).ToHashSet();
            IReadOnlyList<ExerciseDto> exercises = await _api.GetExercisesAsync();

            AvailableExercises.Clear();
            foreach (ExerciseDto exercise in exercises.Where(exercise => !existingIds.Contains(exercise.Id)))
            {
                AvailableExercises.Add(ExerciseItem.FromDto(exercise));
            }

            IsExercisePickerOpen = true;
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not load exercises. {exception.Message}";
        }
    }

    private async Task AddExerciseAsync(ExerciseItem? exercise)
    {
        if (SelectedPlan == null || exercise == null)
        {
            return;
        }

        try
        {
            WorkoutPlanExerciseDto added = await _api.AddExerciseToWorkoutPlanAsync(SelectedPlan.Id, exercise.Id);
            SelectedPlan.Exercises.Add(WorkoutPlanExerciseItem.FromDto(added, SelectedPlan.Exercises.Count));
            IsExercisePickerOpen = false;
            UpdatePreviewState();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not add exercise. {exception.Message}";
        }
    }

    private async Task RemoveExerciseAsync(WorkoutPlanExerciseItem? exercise)
    {
        if (SelectedPlan == null || exercise == null)
        {
            return;
        }

        try
        {
            await _api.DeleteWorkoutPlanExerciseAsync(exercise.Id);
            SelectedPlan.Exercises.Remove(exercise);
            UpdatePreviewState();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not remove exercise. {exception.Message}";
        }
    }

    private async Task AddSetAsync(WorkoutPlanExerciseItem? exercise)
    {
        if (exercise == null)
        {
            return;
        }

        try
        {
            WorkoutSetDto? previous = exercise.LastSet;
            int nextSetNumber = exercise.SetCount + 1;
            int reps = previous?.Reps ?? 0;
            double weight = previous?.Weight ?? 0;

            WorkoutSetDto added = await _api.AddWorkoutPlanSetAsync(exercise.Id, nextSetNumber, reps, weight);
            exercise.AddSet(added);
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not add set. {exception.Message}";
        }
    }

    private async Task RemoveSetAsync(WorkoutPlanExerciseItem? exercise)
    {
        if (exercise == null || exercise.LastSet == null)
        {
            return;
        }

        try
        {
            WorkoutSetDto set = exercise.LastSet;
            await _api.DeleteWorkoutPlanSetAsync(set.Id);
            exercise.RemoveSet(set);
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not remove set. {exception.Message}";
        }
    }

    private string BuildNextWorkoutName()
    {
        int suffix = WorkoutPlans.Count + 1;
        string candidate = "New Workout";

        while (WorkoutPlans.Any(plan => string.Equals(plan.WorkoutName, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"New Workout {suffix++}";
        }

        return candidate;
    }

    private void UpdatePreviewState()
    {
        string[] days = { "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY" };
        int selectedIndex = SelectedPlan == null ? 0 : Math.Max(WorkoutPlans.IndexOf(SelectedPlan), 0);
        PreviewDayName = days[selectedIndex % days.Length];

        int count = SelectedPlan?.Exercises.Count ?? 0;
        PreviewSummary = SelectedPlan == null
            ? "Create a plan and fill it with exercise cards."
            : count == 0
                ? "This plan is empty. Add exercises to build the session."
                : $"{count} exercise{(count == 1 ? string.Empty : "s")} ready in this plan.";
    }
}

public class WorkoutPlanItem : BaseViewModel
{
    private string _workoutName = "Workout Plan";

    public int Id { get; init; }
    public int UserId { get; init; }

    public string WorkoutName
    {
        get => _workoutName;
        set => SetProperty(ref _workoutName, value);
    }

    public ObservableCollection<WorkoutPlanExerciseItem> Exercises { get; } = new();

    public static WorkoutPlanItem FromDto(WorkoutPlanDto dto)
    {
        WorkoutPlanItem item = new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            WorkoutName = string.IsNullOrWhiteSpace(dto.WorkoutName) ? "Workout Plan" : dto.WorkoutName!
        };

        int index = 0;
        foreach (WorkoutPlanExerciseDto exercise in dto.WorkoutExercises ?? Array.Empty<WorkoutPlanExerciseDto>())
        {
            item.Exercises.Add(WorkoutPlanExerciseItem.FromDto(exercise, index++));
        }

        return item;
    }
}

public class WorkoutPlanExerciseItem : BaseViewModel
{
    private readonly List<WorkoutSetDto> _sets;
    private string _setSummary = "No sets yet";

    public WorkoutPlanExerciseItem(
        int id,
        int exerciseId,
        string exerciseName,
        string muscleGroup,
        string secondaryMuscleGroup,
        string accentColor,
        IEnumerable<WorkoutSetDto>? sets)
    {
        Id = id;
        ExerciseId = exerciseId;
        ExerciseName = exerciseName;
        MuscleGroup = muscleGroup;
        SecondaryMuscleGroup = secondaryMuscleGroup;
        AccentColor = accentColor;
        _sets = (sets ?? Array.Empty<WorkoutSetDto>())
            .OrderBy(set => set.SetNumber)
            .ToList();
        UpdateSetSummary();
    }

    public int Id { get; }
    public int ExerciseId { get; }
    public string ExerciseName { get; }
    public string MuscleGroup { get; }
    public string SecondaryMuscleGroup { get; }
    public string AccentColor { get; }
    public int SetCount => _sets.Count;
    public WorkoutSetDto? LastSet => _sets.OrderBy(set => set.SetNumber).LastOrDefault();

    public string SetSummary
    {
        get => _setSummary;
        private set => SetProperty(ref _setSummary, value);
    }

    public void AddSet(WorkoutSetDto set)
    {
        _sets.RemoveAll(existing => existing.Id == set.Id || existing.SetNumber == set.SetNumber);
        _sets.Add(set);
        _sets.Sort((left, right) => left.SetNumber.CompareTo(right.SetNumber));
        UpdateSetSummary();
    }

    public void RemoveSet(WorkoutSetDto set)
    {
        _sets.RemoveAll(existing => existing.Id == set.Id);
        UpdateSetSummary();
    }

    private void UpdateSetSummary()
    {
        SetSummary = SetCount == 0 ? "No sets yet" : $"{SetCount} set{(SetCount == 1 ? string.Empty : "s")}";
        OnPropertyChanged(nameof(SetCount));
        OnPropertyChanged(nameof(LastSet));
    }

    public static WorkoutPlanExerciseItem FromDto(WorkoutPlanExerciseDto dto, int index)
    {
        string[] colors = { "#009688", "#FFB84D", "#74C69D", "#8EC5FF", "#F7A8C8", "#95D5B2" };
        return new WorkoutPlanExerciseItem(
            dto.Id,
            dto.ExerciseId,
            string.IsNullOrWhiteSpace(dto.ExerciseName) ? "Exercise" : dto.ExerciseName!,
            string.IsNullOrWhiteSpace(dto.MuscleGroup) ? "General" : dto.MuscleGroup!,
            dto.SecondaryMuscleGroup ?? string.Empty,
            colors[index % colors.Length],
            dto.Sets);
    }
}

public record ExerciseItem(int Id, string ExerciseName, string MuscleGroup)
{
    public static ExerciseItem FromDto(ExerciseDto dto)
    {
        return new ExerciseItem(
            dto.Id,
            string.IsNullOrWhiteSpace(dto.ExerciseName) ? "Exercise" : dto.ExerciseName!,
            string.IsNullOrWhiteSpace(dto.MuscleGroup) ? "General" : dto.MuscleGroup!);
    }
}
