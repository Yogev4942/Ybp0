using System.Net;
using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _selectedRole = "Trainee";
    private string _fitnessGoal = "Build strength";
    private string _currentWeight = "70";
    private string _height = "170";
    private string _specialization = "Strength training";
    private string _hourlyRate = "120";
    private string _maxTrainees = "10";

    public RegisterViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        RegisterCommand = new AsyncCommand(RegisterAsync);
        GoToLoginCommand = new AsyncCommand(_navigation.GoToLoginAsync);
    }

    public IReadOnlyList<string> Roles { get; } = new[] { "Trainee", "Trainer" };

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    public string SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (SetProperty(ref _selectedRole, value))
            {
                OnPropertyChanged(nameof(IsTrainer));
                OnPropertyChanged(nameof(IsTrainee));
            }
        }
    }

    public bool IsTrainer => SelectedRole == "Trainer";
    public bool IsTrainee => !IsTrainer;

    public string FitnessGoal { get => _fitnessGoal; set => SetProperty(ref _fitnessGoal, value); }
    public string CurrentWeight { get => _currentWeight; set => SetProperty(ref _currentWeight, value); }
    public string Height { get => _height; set => SetProperty(ref _height, value); }
    public string Specialization { get => _specialization; set => SetProperty(ref _specialization, value); }
    public string HourlyRate { get => _hourlyRate; set => SetProperty(ref _hourlyRate, value); }
    public string MaxTrainees { get => _maxTrainees; set => SetProperty(ref _maxTrainees, value); }

    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    private async Task RegisterAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username, email and password are required.";
            return;
        }

        try
        {
            IsBusy = true;

            if (IsTrainer)
            {
                if (!double.TryParse(HourlyRate, out double hourlyRate) || !int.TryParse(MaxTrainees, out int maxTrainees))
                {
                    ErrorMessage = "Hourly rate and max trainees must be numbers.";
                    return;
                }

                await _api.RegisterTrainerAsync(Username.Trim(), Email.Trim(), Password, Specialization.Trim(), hourlyRate, maxTrainees);
            }
            else
            {
                if (!double.TryParse(CurrentWeight, out double currentWeight) || !double.TryParse(Height, out double height))
                {
                    ErrorMessage = "Weight and height must be numbers.";
                    return;
                }

                await _api.RegisterTraineeAsync(Username.Trim(), Email.Trim(), Password, FitnessGoal.Trim(), currentWeight, height);
            }

            await _navigation.GoToHomeAsync();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            ErrorMessage = "That username or email is already taken.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not register. {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
