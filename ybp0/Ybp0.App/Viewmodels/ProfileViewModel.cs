using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private UserDto? _user;

    public ProfileViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        BackHomeCommand = new AsyncCommand(_navigation.GoToHomeAsync);
        SignOutCommand = new AsyncCommand(SignOutAsync);
    }

    public string Username => _user?.Username ?? "Profile";
    public string Email => _user?.Email ?? "No email";
    public string Bio => string.IsNullOrWhiteSpace(_user?.Bio) ? "No bio yet. Your profile details will appear here as you add them." : _user!.Bio!;
    public string Role => _user?.IsTrainer == true ? "Trainer" : "Trainee";
    public string Initial => string.IsNullOrWhiteSpace(_user?.Username) ? "Y" : _user!.Username[..1].ToUpperInvariant();
    public string FirstMetricTitle => _user?.IsTrainer == true ? "Specialization" : "Goal";
    public string FirstMetricValue => _user?.IsTrainer == true ? _user.Specialization ?? "Training" : _user?.FitnessGoal ?? "Strength";
    public string SecondMetricTitle => _user?.IsTrainer == true ? "Hourly rate" : "Weight";
    public string SecondMetricValue => _user?.IsTrainer == true ? $"{_user.HourlyRate ?? 0:0} ILS" : $"{_user?.CurrentWeight ?? 0:0.#} kg";
    public string ThirdMetricTitle => _user?.IsTrainer == true ? "Capacity" : "Height";
    public string ThirdMetricValue => _user?.IsTrainer == true ? $"{_user.MaxTrainees ?? 0} trainees" : $"{_user?.Height ?? 0:0.#} cm";

    public ICommand BackHomeCommand { get; }
    public ICommand SignOutCommand { get; }

    public Task LoadAsync()
    {
        _user = _api.CurrentUser;
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Bio));
        OnPropertyChanged(nameof(Role));
        OnPropertyChanged(nameof(Initial));
        OnPropertyChanged(nameof(FirstMetricTitle));
        OnPropertyChanged(nameof(FirstMetricValue));
        OnPropertyChanged(nameof(SecondMetricTitle));
        OnPropertyChanged(nameof(SecondMetricValue));
        OnPropertyChanged(nameof(ThirdMetricTitle));
        OnPropertyChanged(nameof(ThirdMetricValue));
        return Task.CompletedTask;
    }

    private async Task SignOutAsync()
    {
        _api.SignOut();
        await _navigation.GoToLoginAsync();
    }
}
