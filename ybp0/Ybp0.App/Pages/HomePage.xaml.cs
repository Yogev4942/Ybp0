using Microsoft.Maui.Controls.Shapes;
using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children =
            {
                new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = new Thickness(22, 24, 22, 104),
                        Spacing = 14,
                        Children =
                        {
                            BuildHeader(),
                            BuildPlanSummary(),
                            BuildMetrics(),
                            BuildError()
                        }
                    }
                },
                AddRow(BuildBottomNav(), 1)
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private View BuildHeader()
    {
        Button profile = Ui.ProfileButton();
        profile.SetBinding(Button.CommandProperty, nameof(HomeViewModel.OpenProfileCommand));

        Label greeting = Ui.Text(string.Empty, 24, Ui.Ink, FontAttributes.Bold);
        greeting.SetBinding(Label.TextProperty, nameof(HomeViewModel.Greeting));

        Label role = Ui.Text(string.Empty, 13, Ui.Muted);
        role.SetBinding(Label.TextProperty, nameof(HomeViewModel.RoleLabel));

        Button refresh = new()
        {
            Text = "Refresh",
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Teal,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(8, 0),
            MinimumHeightRequest = 36
        };
        refresh.SetBinding(Button.CommandProperty, nameof(HomeViewModel.RefreshCommand));

        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12,
            Children =
            {
                profile,
                AddColumn(new VerticalStackLayout { Spacing = 2, Children = { greeting, role } }, 1),
                AddColumn(refresh, 2)
            }
        };
    }

    private View BuildPlanSummary()
    {
        Label title = Ui.Text(string.Empty, 22, Ui.Ink, FontAttributes.Bold);
        title.SetBinding(Label.TextProperty, nameof(HomeViewModel.LatestPlanTitle));

        Label subtitle = Ui.Text(string.Empty, 13, Ui.Muted);
        subtitle.SetBinding(Label.TextProperty, nameof(HomeViewModel.LatestPlanSubtitle));

        Label planCount = Ui.Text(string.Empty, 13, Ui.Ink, FontAttributes.Bold);
        planCount.SetBinding(Label.TextProperty, nameof(HomeViewModel.WorkoutPlansCountText));

        Label exerciseCount = Ui.Text(string.Empty, 13, Ui.Ink, FontAttributes.Bold);
        exerciseCount.SetBinding(Label.TextProperty, nameof(HomeViewModel.TotalExercisesText));

        Button openPlans = new()
        {
            Text = "Workout plans",
            BackgroundColor = Ui.Dark,
            TextColor = Colors.White,
            CornerRadius = 18,
            HeightRequest = 42,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(16, 0)
        };
        openPlans.SetBinding(Button.CommandProperty, nameof(HomeViewModel.OpenWorkoutPlansCommand));

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = 18,
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        Children =
                        {
                            new VerticalStackLayout { Spacing = 4, Children = { title, subtitle } },
                            AddColumn(openPlans, 1)
                        }
                    },
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Star)
                        },
                        ColumnSpacing = 10,
                        Children =
                        {
                            StatPill("Plans", planCount),
                            AddColumn(StatPill("Exercises", exerciseCount), 1)
                        }
                    }
                }
            }
        };
    }

    private View BuildMetrics()
    {
        Label focus = Ui.Text(string.Empty, 22, Ui.Ink, FontAttributes.Bold);
        focus.SetBinding(Label.TextProperty, nameof(HomeViewModel.FocusMetric));

        Label goal = Ui.Text(string.Empty, 16, Ui.Ink, FontAttributes.Bold);
        goal.SetBinding(Label.TextProperty, nameof(HomeViewModel.GoalMetric));

        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10,
            Children =
            {
                MetricCard("Current", focus),
                AddColumn(MetricCard("Goal", goal), 1)
            }
        };
    }

    private View BuildError()
    {
        Label error = Ui.Text(string.Empty, 13, Ui.Error);
        error.SetBinding(Label.TextProperty, nameof(HomeViewModel.ErrorMessage));
        return error;
    }

    private View BuildBottomNav()
    {
        Button home = Ui.Nav("H", true);
        Button plan = Ui.Nav("W");
        plan.SetBinding(Button.CommandProperty, nameof(HomeViewModel.OpenWorkoutPlansCommand));

        return new Border
        {
            BackgroundColor = Ui.Dark,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 32 },
            Padding = 8,
            Margin = new Thickness(24, 0, 24, 22),
            HeightRequest = 64,
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                Children =
                {
                    home,
                    AddColumn(plan, 1)
                }
            }
        };
    }

    private static Border StatPill(string label, Label value)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#F3FAF7"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(14, 10),
            Content = new VerticalStackLayout
            {
                Spacing = 3,
                Children =
                {
                    Ui.Text(label, 11, Ui.Muted, FontAttributes.Bold),
                    value
                }
            }
        };
    }

    private static Border MetricCard(string label, Label value)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            Padding = 16,
            MinimumHeightRequest = 96,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    Ui.Text(label, 12, Ui.Muted, FontAttributes.Bold),
                    value
                }
            }
        };
    }

    private static T AddColumn<T>(T view, int column) where T : View
    {
        Grid.SetColumn(view, column);
        return view;
    }

    private static T AddRow<T>(T view, int row) where T : View
    {
        Grid.SetRow(view, row);
        return view;
    }
}
#pragma warning restore CS0618
