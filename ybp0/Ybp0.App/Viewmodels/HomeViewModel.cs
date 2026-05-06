using System.Collections.ObjectModel;
using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class HomeViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private UserDto? _user;

    public HomeViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        OpenProfileCommand = new AsyncCommand(_navigation.GoToProfileAsync);
        RefreshCommand = new AsyncCommand(LoadAsync);
        Trainers = new ObservableCollection<TrainerDto>();
    }

    public ObservableCollection<TrainerDto> Trainers { get; }
    public ICommand OpenProfileCommand { get; }
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

    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            _user = _api.CurrentUser;

            Trainers.Clear();
            foreach (TrainerDto trainer in await _api.GetTrainersAsync())
            {
                Trainers.Add(trainer);
            }

            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(RoleLabel));
            OnPropertyChanged(nameof(FocusMetric));
            OnPropertyChanged(nameof(GoalMetric));
            OnPropertyChanged(nameof(Initial));
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
