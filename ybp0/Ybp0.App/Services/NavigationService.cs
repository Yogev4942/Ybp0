using Ybp0.App.Pages;

namespace Ybp0.App.Services;

public class NavigationService : INavigationService
{
    public Task GoToLoginAsync()
    {
        return Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }

    public Task GoToRegisterAsync()
    {
        return Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    public Task GoToHomeAsync()
    {
        return Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }

    public Task GoToProfileAsync()
    {
        return Shell.Current.GoToAsync(nameof(ProfilePage));
    }
}
