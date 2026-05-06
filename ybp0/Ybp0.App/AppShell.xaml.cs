using Ybp0.App.Pages;

namespace Ybp0.App;

public partial class AppShell : Shell
{
    public AppShell(LoginPage loginPage, HomePage homePage)
    {
        InitializeComponent();
        Items.Add(new ShellContent
        {
            Route = nameof(LoginPage),
            Content = loginPage
        });
        Items.Add(new ShellContent
        {
            Route = nameof(HomePage),
            Content = homePage
        });

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
    }
}
