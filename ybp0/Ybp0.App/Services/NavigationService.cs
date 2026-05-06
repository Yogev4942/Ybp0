using Ybp0.App.Pages;

namespace Ybp0.App.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public Task GoToLoginAsync()
    {
        SetRoot(_services.GetRequiredService<LoginPage>());
        return Task.CompletedTask;
    }

    public Task GoToRegisterAsync()
    {
        return CurrentNavigation.PushAsync(_services.GetRequiredService<RegisterPage>());
    }

    public Task GoToHomeAsync()
    {
        SetRoot(_services.GetRequiredService<HomePage>());
        return Task.CompletedTask;
    }

    public Task GoToProfileAsync()
    {
        return CurrentNavigation.PushAsync(_services.GetRequiredService<ProfilePage>());
    }

    private static INavigation CurrentNavigation
    {
        get
        {
            Page? page = Application.Current?.Windows.FirstOrDefault()?.Page;
            return page?.Navigation ?? throw new InvalidOperationException("No active navigation page is available.");
        }
    }

    private static void SetRoot(Page page)
    {
        Window window = Application.Current?.Windows.FirstOrDefault()
            ?? throw new InvalidOperationException("No active application window is available.");

        window.Page = new NavigationPage(page)
        {
            BarBackgroundColor = Colors.Transparent,
            BarTextColor = Colors.Transparent
        };
    }
}
