using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = Ui.SoftTeal;
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

        Button signIn = Ui.Primary("Login");
        signIn.SetBinding(Button.CommandProperty, nameof(LoginViewModel.LoginCommand));

        ActivityIndicator busy = new() { Color = Ui.Teal };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(LoginViewModel.IsBusy));
        busy.SetBinding(IsVisibleProperty, nameof(LoginViewModel.IsBusy));

        Grid page = new()
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(0.42, GridUnitType.Star)),
                new RowDefinition(new GridLength(0.58, GridUnitType.Star))
            },
            Children =
            {
                new GraphicsView
                {
                    Drawable = new LoginHeaderDrawable(),
                    HeightRequest = 360
                },
                AddRow(new Frame
                {
                    BackgroundColor = Ui.Card,
                    BorderColor = Colors.Transparent,
                    CornerRadius = 42,
                    HasShadow = false,
                    Padding = new Thickness(28, 38, 28, 24),
                    Margin = new Thickness(0, -42, 0, 0),
                    Content = new ScrollView
                    {
                        Content = new VerticalStackLayout
                        {
                            Spacing = 18,
                            Children =
                            {
                                new VerticalStackLayout
                                {
                                    Spacing = 2,
                                    Children =
                                    {
                                        Ui.Text("Sign in", 36, Ui.Ink, FontAttributes.Bold),
                                        new BoxView
                                        {
                                            Color = Ui.Teal,
                                            HeightRequest = 4,
                                            WidthRequest = 46,
                                            HorizontalOptions = LayoutOptions.Start
                                        }
                                    }
                                },
                                Ui.Text("Welcome back to your training space.", 14, Ui.Muted),
                                error,
                                Ui.Text("Username", 13, Ui.Muted, FontAttributes.Bold),
                                Ui.UnderlineField(username),
                                Ui.Text("Password", 13, Ui.Muted, FontAttributes.Bold),
                                Ui.UnderlineField(password),
                                new BoxView { HeightRequest = 22, Opacity = 0 },
                                signIn,
                                busy
                            }
                        }
                    }
                }, 1)
            }
        };

        Content = page;
    }

    private static T AddRow<T>(T view, int row) where T : View
    {
        Grid.SetRow(view, row);
        return view;
    }

    private sealed class LoginHeaderDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Ui.SoftTeal;
            canvas.FillRectangle(dirtyRect);

            canvas.StrokeColor = Color.FromArgb("#55FFFFFF");
            canvas.StrokeSize = 2;

            for (float y = -40; y < dirtyRect.Height + 80; y += 44)
            {
                PathF line = new();
                line.MoveTo(-20, y);

                for (float x = -20; x <= dirtyRect.Width + 40; x += 70)
                {
                    line.CurveTo(x + 20, y - 26, x + 52, y + 26, x + 70, y);
                }

                canvas.DrawPath(line);
            }

            canvas.FillColor = Ui.Card;
            PathF wave = new();
            wave.MoveTo(0, dirtyRect.Height - 58);
            wave.CurveTo(dirtyRect.Width * 0.22f, dirtyRect.Height - 96, dirtyRect.Width * 0.44f, dirtyRect.Height + 8, dirtyRect.Width * 0.66f, dirtyRect.Height - 36);
            wave.CurveTo(dirtyRect.Width * 0.82f, dirtyRect.Height - 70, dirtyRect.Width * 0.96f, dirtyRect.Height - 50, dirtyRect.Width, dirtyRect.Height - 58);
            wave.LineTo(dirtyRect.Width, dirtyRect.Height);
            wave.LineTo(0, dirtyRect.Height);
            wave.Close();
            canvas.FillPath(wave);
        }
    }
}
#pragma warning restore CS0618
