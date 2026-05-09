using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);

        Button profile = Ui.ProfileButton();
        profile.SetBinding(Button.CommandProperty, nameof(HomeViewModel.OpenProfileCommand));

        Label greeting = Ui.Text(string.Empty, 26, Ui.Ink, FontAttributes.Bold);
        greeting.SetBinding(Label.TextProperty, nameof(HomeViewModel.Greeting));

        Label role = Ui.Text(string.Empty, 13, Ui.Muted);
        role.SetBinding(Label.TextProperty, nameof(HomeViewModel.RoleLabel));

        Label focus = Ui.Text(string.Empty, 25, Ui.Ink, FontAttributes.Bold);
        focus.SetBinding(Label.TextProperty, nameof(HomeViewModel.FocusMetric));

        Label goal = Ui.Text(string.Empty, 16, Ui.Ink, FontAttributes.Bold);
        goal.SetBinding(Label.TextProperty, nameof(HomeViewModel.GoalMetric));

        Button refresh = Ui.Link("Refresh");
        refresh.SetBinding(Button.CommandProperty, nameof(HomeViewModel.RefreshCommand));

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 34, 24, 24),
                Spacing = 18,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Auto),
                            new ColumnDefinition(GridLength.Star)
                        },
                        ColumnSpacing = 14,
                        Children =
                        {
                            profile,
                            AddColumn(new VerticalStackLayout { Spacing = 2, Children = { greeting, role } }, 1)
                        }
                    },
                    Ui.CardFrame(new VerticalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            Ui.Text("Training dashboard", 30, Ui.Ink, FontAttributes.Bold),
                            Ui.Text("Ready for today's training.", 14, Ui.Muted),
                            new Grid
                            {
                                ColumnDefinitions =
                                {
                                    new ColumnDefinition(GridLength.Star),
                                    new ColumnDefinition(GridLength.Star)
                                },
                                ColumnSpacing = 12,
                                Children =
                                {
                                    Ui.CardFrame(new VerticalStackLayout
                                    {
                                        Spacing = 4,
                                        Children = { Ui.Text("Focus", 12, Ui.Muted), focus }
                                    }, 20),
                                    AddColumn(Ui.CardFrame(new VerticalStackLayout
                                    {
                                        Spacing = 4,
                                        Children = { Ui.Text("Goal", 12, Ui.Muted), goal }
                                    }, 20), 1)
                                }
                            },
                            refresh
                        }
                    }, 30),
                    Ui.CardFrame(new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            Ui.Text("Today", 20, Ui.Ink, FontAttributes.Bold),
                            Ui.Text("Keep the basics moving: check your profile, review your role, and stay consistent.", 14, Ui.Muted)
                        }
                    }, 26)
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private static T AddColumn<T>(T view, int column) where T : View
    {
        Grid.SetColumn(view, column);
        return view;
    }
}
#pragma warning restore CS0618
