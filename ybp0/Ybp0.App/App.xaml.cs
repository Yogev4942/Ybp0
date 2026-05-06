using Ybp0.App.Pages;

namespace Ybp0.App;

public partial class App : Application
{
    private readonly LoginPage _loginPage;

    public App(LoginPage loginPage)
    {
        InitializeComponent();
        _loginPage = loginPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(_loginPage)
        {
            BarBackgroundColor = Colors.Transparent,
            BarTextColor = Colors.Transparent
        });
    }
}
