using System.Collections.ObjectModel;
using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class HomeViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private UserDto? _user;
    private IReadOnlyList<WorkoutPlanDto> _plans = Array.Empty<WorkoutPlanDto>();

    public HomeViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        OpenProfileCommand = new AsyncCommand(_navigation.GoToProfileAsync);
        OpenWorkoutPlansCommand = new AsyncCommand(_navigation.GoToWorkoutPlansAsync);
        RefreshCommand = new AsyncCommand(LoadAsync);
        ActivityDays = new ObservableCollection<ActivityDayViewModel>();
        ExerciseProgressCards = new ObservableCollection<ExerciseProgressCardViewModel>();
    }

    public ICommand OpenProfileCommand { get; }
    public ICommand OpenWorkoutPlansCommand { get; }
    public ICommand RefreshCommand { get; }

    public string Greeting => _user is null ? "Hello" : $"Hello, {_user.Username}";
    public string RoleLabel => _user?.IsTrainer == true ? "Trainer workspace" : "Trainee workspace";
    public string FocusMetric => _user switch
    {
        { IsTrainer: true } trainer => $"{trainer.TotalTrainees ?? 0} trainees",
        { CurrentWeight: double weight } => $"{weight:0.#} kg",
        _ => "-- kg"
    };
    public string GoalMetric => _user?.IsTrainer == true ? $"{_user.Rating?.ToString("0.0") ?? "0.0"} rating" : _user?.FitnessGoal ?? "Build strength";
    public string Initial => string.IsNullOrWhiteSpace(_user?.Username) ? "Y" : _user!.Username[..1].ToUpperInvariant();
    public string WorkoutPlansCountText => $"{_plans.Count} plan{(_plans.Count == 1 ? string.Empty : "s")}";
    public string TotalExercisesText
    {
        get
        {
            int count = _plans.Sum(plan => plan.WorkoutExercises?.Count ?? 0);
            return $"{count} exercise{(count == 1 ? string.Empty : "s")}";
        }
    }
    public string LatestPlanTitle => _plans.FirstOrDefault()?.WorkoutName ?? "No workout plans yet";
    public string LatestPlanSubtitle => _plans.Count == 0 ? "Create your first plan and add exercises." : "Open plans to edit exercises.";
    public string WelcomeMessage => _user is null
        ? "Welcome back!"
        : _user.IsTrainer ? $"Welcome back, Coach {_user.Username}!" : $"Welcome back, {_user.Username}!";
    public string MonthSummary => "No workout history loaded";
    public string StatisticsSummary => ExerciseProgressCards.Count == 0
        ? "Complete workouts to unlock real progression data."
        : "Volume load = sets x reps x weight, a simple way to track progression over time.";

    public ObservableCollection<ActivityDayViewModel> ActivityDays { get; }
    public ObservableCollection<ExerciseProgressCardViewModel> ExerciseProgressCards { get; }

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            _user = _api.CurrentUser;
            _plans = _user == null ? Array.Empty<WorkoutPlanDto>() : await _api.GetWorkoutPlansAsync();
            BuildActivityDays();
            BuildProgressCards();

            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(FocusMetric));
            OnPropertyChanged(nameof(GoalMetric));
            OnPropertyChanged(nameof(Initial));
            OnPropertyChanged(nameof(WorkoutPlansCountText));
            OnPropertyChanged(nameof(TotalExercisesText));
            OnPropertyChanged(nameof(LatestPlanTitle));
            OnPropertyChanged(nameof(LatestPlanSubtitle));
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(MonthSummary));
            OnPropertyChanged(nameof(StatisticsSummary));
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not load home data. {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildActivityDays()
    {
        ActivityDays.Clear();
    }

    private void BuildProgressCards()
    {
        ExerciseProgressCards.Clear();
    }
}

public class ActivityDayViewModel
{
    public string DayLabel { get; set; } = string.Empty;
    public string WeekdayLabel { get; set; } = string.Empty;
    public Color FillColor { get; set; } = UiColor.Empty;
}

public class ExerciseProgressCardViewModel
{
    public string ExerciseName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public double LatestVolume { get; set; }
    public Color AccentColor { get; set; } = UiColor.Teal;
    public ObservableCollection<ExerciseVolumePointViewModel> Points { get; set; } = new();
}

public class ExerciseVolumePointViewModel
{
    public string Label { get; set; } = string.Empty;
    public string VolumeLabel { get; set; } = string.Empty;
    public double BarWidth { get; set; }
}

internal static class UiColor
{
    public static readonly Color Teal = Color.FromArgb("#26A69A");
    public static readonly Color Empty = Color.FromArgb("#DCEBE7");
}
