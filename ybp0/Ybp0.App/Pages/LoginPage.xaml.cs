using Ybp0.App.Viewmodels;
using Microsoft.Maui.Controls.Shapes;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);

        Entry username = new()
        {
            Placeholder = "Username",
            ReturnType = ReturnType.Next,
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink,
            PlaceholderColor = Ui.Muted
        };
        username.SetBinding(Entry.TextProperty, nameof(LoginViewModel.Username));

        Entry password = new()
        {
            Placeholder = "Password",
            IsPassword = true,
            ReturnType = ReturnType.Go,
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink,
            PlaceholderColor = Ui.Muted
        };
        password.SetBinding(Entry.TextProperty, nameof(LoginViewModel.Password));

        Label error = Ui.Text(string.Empty, 13, Ui.Error);
        error.SetBinding(Label.TextProperty, nameof(LoginViewModel.ErrorMessage));

        Button signIn = Ui.Primary("Sign in");
        signIn.SetBinding(Button.CommandProperty, nameof(LoginViewModel.LoginCommand));

        ActivityIndicator busy = new() { Color = Ui.Teal };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(LoginViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(LoginViewModel.IsBusy));

        Content = new Grid
        {
            Padding = new Thickness(24),
            Children =
            {
                new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 30 },
                    Padding = new Thickness(24),
                    VerticalOptions = LayoutOptions.Center,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    Ui.Text("YBP", 32, Ui.Ink, FontAttributes.Bold),
                                    Ui.Text("Sign in to your training space.", 14, Ui.Muted)
                                }
                            },
                            error,
                            Field(username),
                            Field(password),
                            signIn,
                            busy
                        }
                    }
                }
            }
        };
    }

    private static Border Field(View content)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#F6FFFC"),
            Stroke = Color.FromArgb("#22009688"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(14, 4),
            Content = content
        };
    }
}
#pragma warning restore CS0618
