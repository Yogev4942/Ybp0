using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);

        Entry username = new() { Placeholder = "Username", ReturnType = ReturnType.Next };
        username.SetBinding(Entry.TextProperty, nameof(LoginViewModel.Username));

        Entry password = new() { Placeholder = "Password", IsPassword = true, ReturnType = ReturnType.Go };
        password.SetBinding(Entry.TextProperty, nameof(LoginViewModel.Password));

        Label error = Ui.Text(string.Empty, 13, Ui.Error);
        error.SetBinding(Label.TextProperty, nameof(LoginViewModel.ErrorMessage));

        Button signIn = Ui.Primary("Sign in");
        signIn.SetBinding(Button.CommandProperty, nameof(LoginViewModel.LoginCommand));

        Button register = Ui.Link("Create account");
        register.SetBinding(Button.CommandProperty, nameof(LoginViewModel.GoToRegisterCommand));

        ActivityIndicator busy = new() { Color = Ui.Teal };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(LoginViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(LoginViewModel.IsBusy));

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 18,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Ui.CardFrame(new VerticalStackLayout
                    {
                        Spacing = 18,
                        Children =
                        {
                            new Frame
                            {
                                Content = Ui.Text("YBP", 12, Ui.Teal, FontAttributes.Bold),
                                BackgroundColor = Color.FromArgb("#DDF7F0"),
                                BorderColor = Colors.Transparent,
                                CornerRadius = 18,
                                HasShadow = false,
                                Padding = new Thickness(12, 7),
                                HorizontalOptions = LayoutOptions.Start
                            },
                            Ui.Text("Welcome back", 34, Ui.Ink, FontAttributes.Bold),
                            Ui.Text("Sign in to keep your training moving.", 14, Ui.Muted),
                            error,
                            Ui.Field(username),
                            Ui.Field(password),
                            signIn,
                            new HorizontalStackLayout
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                Spacing = 6,
                                Children = { Ui.Text("New here?", 14, Ui.Muted), register }
                            },
                            busy
                        }
                    }, 30)
                }
            }
        };
    }
}
#pragma warning restore CS0618
