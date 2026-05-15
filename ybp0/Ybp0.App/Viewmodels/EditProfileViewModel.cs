using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class EditProfileViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private UserDto? _user;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _bio = string.Empty;
    private string _fitnessGoal = string.Empty;
    private string _currentWeight = string.Empty;
    private string _height = string.Empty;
    private string _specialization = string.Empty;
    private string _hourlyRate = string.Empty;
    private string _maxTrainees = string.Empty;
    private string _statusMessage = string.Empty;

    public EditProfileViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        BackCommand = new AsyncCommand(_navigation.GoBackAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
        SaveAccountCommand = new AsyncCommand(SaveAsync);
    }

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Bio { get => _bio; set => SetProperty(ref _bio, value); }
    public string FitnessGoal { get => _fitnessGoal; set => SetProperty(ref _fitnessGoal, value); }
    public string CurrentWeight { get => _currentWeight; set => SetProperty(ref _currentWeight, value); }
    public string Height { get => _height; set => SetProperty(ref _height, value); }
    public string Specialization { get => _specialization; set => SetProperty(ref _specialization, value); }
    public string HourlyRate { get => _hourlyRate; set => SetProperty(ref _hourlyRate, value); }
    public string MaxTrainees { get => _maxTrainees; set => SetProperty(ref _maxTrainees, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool IsTrainer => _user?.IsTrainer == true;
    public bool IsTrainee => _user?.IsTrainer == false;

    public ICommand BackCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAccountCommand { get; }

    public Task LoadAsync()
    {
        _user = _api.CurrentUser;
        Username = _user?.Username ?? string.Empty;
        Email = _user?.Email ?? string.Empty;
        Bio = _user?.Bio ?? string.Empty;
        FitnessGoal = _user?.FitnessGoal ?? string.Empty;
        CurrentWeight = _user?.CurrentWeight?.ToString("0.#") ?? string.Empty;
        Height = _user?.Height?.ToString("0.#") ?? string.Empty;
        Specialization = _user?.Specialization ?? string.Empty;
        HourlyRate = _user?.HourlyRate?.ToString("0.#") ?? string.Empty;
        MaxTrainees = _user?.MaxTrainees?.ToString() ?? string.Empty;
        StatusMessage = string.Empty;
        OnPropertyChanged(nameof(IsTrainer));
        OnPropertyChanged(nameof(IsTrainee));
        return Task.CompletedTask;
    }

    private Task SaveAsync()
    {
        return SaveProfileAsync();
    }

    private async Task SaveProfileAsync()
    {
        if (_user is null)
        {
            StatusMessage = "Sign in again before editing your profile.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            StatusMessage = "Username is required.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            _user = await _api.UpdateProfileAsync(new UpdateProfileRequest(
                _user.Id,
                Username.Trim(),
                string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                string.IsNullOrWhiteSpace(Bio) ? null : Bio.Trim(),
                string.IsNullOrWhiteSpace(FitnessGoal) ? null : FitnessGoal.Trim(),
                TryDouble(CurrentWeight),
                TryDouble(Height),
                string.IsNullOrWhiteSpace(Specialization) ? null : Specialization.Trim(),
                TryDouble(HourlyRate),
                TryInt(MaxTrainees)));

            StatusMessage = "Profile updated.";
            await LoadAsync();
        }
        catch (Exception exception)
        {
            StatusMessage = $"Could not save profile. {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static double? TryDouble(string value)
    {
        return double.TryParse(value, out double result) ? result : null;
    }

    private static int? TryInt(string value)
    {
        return int.TryParse(value, out int result) ? result : null;
    }
}
