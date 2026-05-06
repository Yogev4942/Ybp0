using System.Net;
using System.Windows.Input;
using Ybp0.App.Services;

namespace Ybp0.App.Viewmodels;

public class LoginViewModel : BaseViewModel
{
    private readonly IApiService _api;
    private readonly INavigationService _navigation;
    private string _username = string.Empty;
    private string _password = string.Empty;

    public LoginViewModel(IApiService api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        LoginCommand = new AsyncCommand(LoginAsync);
        GoToRegisterCommand = new AsyncCommand(_navigation.GoToRegisterAsync);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    private async Task LoginAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter your username and password.";
            return;
        }

        try
        {
            IsBusy = true;
            await _api.LoginAsync(Username.Trim(), Password);
            await _navigation.GoToHomeAsync();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Invalid username or password.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not sign in. {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
