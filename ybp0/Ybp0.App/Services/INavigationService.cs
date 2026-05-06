namespace Ybp0.App.Services;

public interface INavigationService
{
    Task GoToLoginAsync();
    Task GoToRegisterAsync();
    Task GoToHomeAsync();
    Task GoToProfileAsync();
}
