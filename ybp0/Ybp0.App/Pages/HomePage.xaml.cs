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
                            BuildHero(),
                            BuildDashboardGrid(),
                            BuildProgression(),
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

    private View BuildHero()
    {
        Label welcome = Ui.Text(string.Empty, 30, Colors.White, FontAttributes.Bold);
        welcome.SetBinding(Label.TextProperty, nameof(HomeViewModel.WelcomeMessage));

        Label subtitle = Ui.Text("Your last month at a glance, with consistency blocks and strength progression.", 14, Color.FromArgb("#D9FFFA"));

        Label monthSummary = Ui.Text(string.Empty, 18, Colors.White, FontAttributes.Bold);
        monthSummary.SetBinding(Label.TextProperty, nameof(HomeViewModel.MonthSummary));

        return new Border
        {
            BackgroundColor = Ui.Teal,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 26 },
            Padding = new Thickness(24, 22),
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(new GridLength(220))
                },
                ColumnSpacing = 18,
                Children =
                {
                    new VerticalStackLayout { Spacing = 8, Children = { welcome, subtitle } },
                    AddColumn(new Border
                    {
                        BackgroundColor = Color.FromArgb("#2FFFFFFF"),
                        StrokeThickness = 0,
                        StrokeShape = new RoundRectangle { CornerRadius = 20 },
                        Padding = new Thickness(18, 14),
                        Content = new VerticalStackLayout
                        {
                            Spacing = 6,
                            Children =
                            {
                                Ui.Text("Monthly Focus", 12, Color.FromArgb("#D7FFFA")),
                                monthSummary
                            }
                        }
                    }, 1)
                }
            }
        };
    }

    private View BuildDashboardGrid()
    {
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

        Label stats = Ui.Text(string.Empty, 13, Ui.Muted);
        stats.SetBinding(Label.TextProperty, nameof(HomeViewModel.StatisticsSummary));

        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1.15, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.85, GridUnitType.Star))
            },
            ColumnSpacing = 14,
            Children =
            {
                BuildActivityCard(),
                AddColumn(new VerticalStackLayout
                {
                    Spacing = 14,
                    Children =
                    {
                        DashboardCard("Progress Metric", stats),
                        DashboardCard("Chest & Back", BuildPlanMini(openPlans))
                    }
                }, 1)
            }
        };
    }

    private View BuildActivityCard()
    {
        CollectionView days = new()
        {
            SelectionMode = SelectionMode.None,
            ItemsLayout = new GridItemsLayout(6, ItemsLayoutOrientation.Vertical)
            {
                HorizontalItemSpacing = 8,
                VerticalItemSpacing = 8
            },
            ItemTemplate = new DataTemplate(() =>
            {
                Label weekday = Ui.Text(string.Empty, 10, Ui.Muted, FontAttributes.Bold);
                weekday.SetBinding(Label.TextProperty, nameof(ActivityDayViewModel.WeekdayLabel));
                Label day = Ui.Text(string.Empty, 22, Ui.Ink, FontAttributes.Bold);
                day.SetBinding(Label.TextProperty, nameof(ActivityDayViewModel.DayLabel));

                Border tile = new()
                {
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 14 },
                    Padding = new Thickness(10, 8),
                    Content = new VerticalStackLayout { Spacing = 4, Children = { weekday, day } }
                };
                tile.SetBinding(Border.BackgroundColorProperty, nameof(ActivityDayViewModel.FillColor));
                return tile;
            })
        };
        days.SetBinding(ItemsView.ItemsSourceProperty, nameof(HomeViewModel.ActivityDays));

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = 20,
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    Ui.Text("Exercise Blocks", 24, Ui.Ink, FontAttributes.Bold),
                    Ui.Text("Completed workout history is not available from the current API yet.", 13, Ui.Muted),
                    days,
                    EmptyState("No activity data yet")
                }
            }
        };
    }

    private View BuildPlanMini(Button openPlans)
    {
        Label title = Ui.Text(string.Empty, 18, Ui.Ink, FontAttributes.Bold);
        title.SetBinding(Label.TextProperty, nameof(HomeViewModel.LatestPlanTitle));
        Label subtitle = Ui.Text(string.Empty, 13, Ui.Muted);
        subtitle.SetBinding(Label.TextProperty, nameof(HomeViewModel.LatestPlanSubtitle));
        Label planCount = Ui.Text(string.Empty, 13, Ui.Ink, FontAttributes.Bold);
        planCount.SetBinding(Label.TextProperty, nameof(HomeViewModel.WorkoutPlansCountText));
        Label exerciseCount = Ui.Text(string.Empty, 13, Ui.Ink, FontAttributes.Bold);
        exerciseCount.SetBinding(Label.TextProperty, nameof(HomeViewModel.TotalExercisesText));

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                title,
                subtitle,
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
                },
                openPlans
            }
        };
    }

    private View BuildProgression()
    {
        CollectionView cards = new()
        {
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(() =>
            {
                Label exercise = Ui.Text(string.Empty, 18, Ui.Ink, FontAttributes.Bold);
                exercise.SetBinding(Label.TextProperty, nameof(ExerciseProgressCardViewModel.ExerciseName));
                Label summary = Ui.Text(string.Empty, 13, Ui.Muted);
                summary.SetBinding(Label.TextProperty, nameof(ExerciseProgressCardViewModel.Summary));
                Label latest = Ui.Text(string.Empty, 12, Colors.White, FontAttributes.Bold);
                latest.SetBinding(Label.TextProperty, new Binding(nameof(ExerciseProgressCardViewModel.LatestVolume), stringFormat: "Latest {0:0}"));

                Border latestBadge = new()
                {
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Padding = new Thickness(12, 7),
                    Content = latest
                };
                latestBadge.SetBinding(Border.BackgroundColorProperty, nameof(ExerciseProgressCardViewModel.AccentColor));

                CollectionView points = new()
                {
                    SelectionMode = SelectionMode.None,
                    ItemsLayout = LinearItemsLayout.Vertical,
                    ItemTemplate = new DataTemplate(() =>
                    {
                        Label label = Ui.Text(string.Empty, 12, Ui.Muted);
                        label.SetBinding(Label.TextProperty, nameof(ExerciseVolumePointViewModel.Label));
                        Label volume = Ui.Text(string.Empty, 12, Ui.Ink, FontAttributes.Bold);
                        volume.SetBinding(Label.TextProperty, nameof(ExerciseVolumePointViewModel.VolumeLabel));
                        BoxView bar = new()
                        {
                            Color = Ui.Teal,
                            HeightRequest = 16,
                            CornerRadius = 8,
                            HorizontalOptions = LayoutOptions.Start
                        };
                        bar.SetBinding(VisualElement.WidthRequestProperty, nameof(ExerciseVolumePointViewModel.BarWidth));

                        return new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition(new GridLength(70)),
                                new ColumnDefinition(GridLength.Star),
                                new ColumnDefinition(new GridLength(48))
                            },
                            ColumnSpacing = 8,
                            Margin = new Thickness(0, 4),
                            Children =
                            {
                                label,
                                AddColumn(new Border
                                {
                                    BackgroundColor = Color.FromArgb("#E1F0EB"),
                                    StrokeThickness = 0,
                                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                                    HeightRequest = 16,
                                    Content = bar
                                }, 1),
                                AddColumn(volume, 2)
                            }
                        };
                    })
                };
                points.SetBinding(ItemsView.ItemsSourceProperty, nameof(ExerciseProgressCardViewModel.Points));

                return new Border
                {
                    BackgroundColor = Color.FromArgb("#F6FCFA"),
                    Stroke = Color.FromArgb("#D9EBE5"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 18 },
                    Padding = 16,
                    Margin = new Thickness(0, 0, 0, 12),
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
                                    new VerticalStackLayout { Spacing = 5, Children = { exercise, summary } },
                                    AddColumn(latestBadge, 1)
                                }
                            },
                            points
                        }
                    }
                };
            })
        };
        cards.SetBinding(ItemsView.ItemsSourceProperty, nameof(HomeViewModel.ExerciseProgressCards));

        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = 20,
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    Ui.Text("Exercise Progression", 24, Ui.Ink, FontAttributes.Bold),
                    Ui.Text("Real progression cards will appear after completed workout sessions are exposed to the app.", 13, Ui.Muted),
                    cards,
                    EmptyState("No progression data yet")
                }
            }
        };
    }

    private static Label EmptyState(string text)
    {
        return Ui.Text(text, 14, Ui.Muted, FontAttributes.Bold);
    }

    private static Border DashboardCard(string title, View body)
    {
        return new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = 20,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    Ui.Text(title, 22, Ui.Ink, FontAttributes.Bold),
                    body
                }
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
