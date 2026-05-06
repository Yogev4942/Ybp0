using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);

        Label name = Ui.Text(string.Empty, 28, Ui.Ink, FontAttributes.Bold);
        name.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Username));
        Label email = Ui.Text(string.Empty, 14, Ui.Muted);
        email.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Email));
        Label bio = Ui.Text(string.Empty, 14, Ui.Muted);
        bio.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Bio));
        Label role = Ui.Text(string.Empty, 12, Ui.Teal, FontAttributes.Bold);
        role.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Role));

        Button home = Ui.Link("Back home");
        home.SetBinding(Button.CommandProperty, nameof(ProfileViewModel.BackHomeCommand));
        Button signOut = Ui.Primary("Sign out");
        signOut.SetBinding(Button.CommandProperty, nameof(ProfileViewModel.SignOutCommand));

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 34),
                Spacing = 18,
                Children =
                {
                    home,
                    Ui.CardFrame(new VerticalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            new Frame
                            {
                                Content = role,
                                BackgroundColor = Color.FromArgb("#DDF7F0"),
                                BorderColor = Colors.Transparent,
                                CornerRadius = 18,
                                HasShadow = false,
                                Padding = new Thickness(12, 7),
                                HorizontalOptions = LayoutOptions.Start
                            },
                            name,
                            email,
                            bio,
                            new Grid
                            {
                                ColumnDefinitions =
                                {
                                    new ColumnDefinition(GridLength.Star),
                                    new ColumnDefinition(GridLength.Star),
                                    new ColumnDefinition(GridLength.Star)
                                },
                                Children =
                                {
                                    Metric(nameof(ProfileViewModel.FirstMetricTitle), nameof(ProfileViewModel.FirstMetricValue), "#DDF7E8", 0),
                                    Metric(nameof(ProfileViewModel.SecondMetricTitle), nameof(ProfileViewModel.SecondMetricValue), "#DDF4F7", 1),
                                    Metric(nameof(ProfileViewModel.ThirdMetricTitle), nameof(ProfileViewModel.ThirdMetricValue), "#FFE9AD", 2)
                                }
                            },
                            signOut
                        }
                    }, 30)
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private static View Metric(string titlePath, string valuePath, string color, int column)
    {
        Label title = Ui.Text(string.Empty, 11, Ui.Muted, FontAttributes.Bold);
        title.SetBinding(Label.TextProperty, titlePath);
        Label value = Ui.Text(string.Empty, 15, Ui.Ink, FontAttributes.Bold);
        value.SetBinding(Label.TextProperty, valuePath);

        Frame frame = new()
        {
            Content = new VerticalStackLayout { Spacing = 4, Children = { title, value } },
            BackgroundColor = Color.FromArgb(color),
            BorderColor = Colors.Transparent,
            CornerRadius = 18,
            HasShadow = false,
            Padding = 12
        };
        Grid.SetColumn(frame, column);
        return frame;
    }
}
#pragma warning restore CS0618
