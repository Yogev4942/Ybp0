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

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            _user = _api.CurrentUser;
            _plans = _user == null ? Array.Empty<WorkoutPlanDto>() : await _api.GetWorkoutPlansAsync();

            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(FocusMetric));
            OnPropertyChanged(nameof(GoalMetric));
            OnPropertyChanged(nameof(Initial));
            OnPropertyChanged(nameof(WorkoutPlansCountText));
            OnPropertyChanged(nameof(TotalExercisesText));
            OnPropertyChanged(nameof(LatestPlanTitle));
            OnPropertyChanged(nameof(LatestPlanSubtitle));
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
}
