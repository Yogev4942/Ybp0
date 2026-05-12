using Ybp0.App.Viewmodels;
using Microsoft.Maui.Controls.Shapes;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class WorkoutPlansPage : ContentPage
{
    private readonly WorkoutPlansViewModel _viewModel;

    public WorkoutPlansPage(WorkoutPlansViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);
        Content = BuildContent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private View BuildContent()
    {
        Grid root = new()
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        root.Children.Add(new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(22, 26, 22, 108),
                Spacing = 18,
                Children =
                {
                    BuildHeader(),
                    BuildPlanner(),
                    BuildExerciseList(),
                    BuildError()
                }
            }
        });

        root.Children.Add(AddRow(BuildBottomNav(), 1));
        root.Children.Add(BuildExercisePicker());
        return root;
    }

    private View BuildHeader()
    {
        Label day = Ui.Text(string.Empty, 11, Ui.Muted, FontAttributes.Bold);
        day.SetBinding(Label.TextProperty, nameof(WorkoutPlansViewModel.PreviewDayName));

        Label summary = Ui.Text(string.Empty, 13, Ui.Muted);
        summary.SetBinding(Label.TextProperty, nameof(WorkoutPlansViewModel.PreviewSummary));

        Button add = new()
        {
            Text = "+",
            BackgroundColor = Ui.Dark,
            TextColor = Colors.White,
            CornerRadius = 22,
            WidthRequest = 44,
            HeightRequest = 44,
            FontSize = 24,
            Padding = 0
        };
        add.SetBinding(Button.CommandProperty, nameof(WorkoutPlansViewModel.AddPlanCommand));

        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        day,
                        Ui.Text("Your plan", 26, Ui.Ink, FontAttributes.Bold),
                        summary
                    }
                },
                AddColumn(add, 1)
            }
        };
    }

    private View BuildPlanner()
    {
        Picker picker = new()
        {
            Title = "Select plan",
            ItemDisplayBinding = new Binding(nameof(WorkoutPlanItem.WorkoutName)),
            TextColor = Ui.Ink,
            TitleColor = Ui.Muted,
            BackgroundColor = Colors.Transparent
        };
        picker.SetBinding(Picker.ItemsSourceProperty, nameof(WorkoutPlansViewModel.WorkoutPlans));
        picker.SetBinding(Picker.SelectedItemProperty, nameof(WorkoutPlansViewModel.SelectedPlan), BindingMode.TwoWay);

        Entry name = new()
        {
            Placeholder = "Workout name",
            TextColor = Ui.Ink,
            PlaceholderColor = Ui.Muted,
            BackgroundColor = Colors.Transparent,
            ReturnType = ReturnType.Done
        };
        name.SetBinding(Entry.TextProperty, nameof(WorkoutPlansViewModel.EditableWorkoutName), BindingMode.TwoWay);

        Button save = new()
        {
            Text = "Save",
            BackgroundColor = Ui.Teal,
            TextColor = Colors.White,
            CornerRadius = 17,
            HeightRequest = 42,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(16, 0)
        };
        save.SetBinding(Button.CommandProperty, nameof(WorkoutPlansViewModel.RenamePlanCommand));

        Button addExercise = new()
        {
            Text = "+ Add exercise",
            BackgroundColor = Color.FromArgb("#EEF8F5"),
            TextColor = Ui.Teal,
            CornerRadius = 17,
            HeightRequest = 42,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(16, 0)
        };
        addExercise.SetBinding(Button.CommandProperty, nameof(WorkoutPlansViewModel.OpenExercisePickerCommand));

        return Ui.CardFrame(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                picker,
                Ui.Field(name),
                new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    ColumnSpacing = 10,
                    Children =
                    {
                        addExercise,
                        AddColumn(save, 1)
                    }
                }
            }
        }, 26);
    }

    private View BuildExerciseList()
    {
        CollectionView list = new()
        {
            SelectionMode = SelectionMode.None,
            EmptyView = new Border
            {
                BackgroundColor = Color.FromArgb("#F8FFFC"),
                Stroke = Color.FromArgb("#22009688"),
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Padding = 16,
                Content = Ui.Text("No exercises available to add.", 14, Ui.Muted)
            },
            ItemTemplate = new DataTemplate(() =>
            {
                BoxView stripe = new()
                {
                    WidthRequest = 5,
                    HorizontalOptions = LayoutOptions.Start,
                    CornerRadius = 3
                };
                stripe.SetBinding(BoxView.ColorProperty, nameof(WorkoutPlanExerciseItem.AccentColor));

                Label name = Ui.Text(string.Empty, 17, Ui.Ink, FontAttributes.Bold);
                name.SetBinding(Label.TextProperty, nameof(WorkoutPlanExerciseItem.ExerciseName));

                Label muscle = Ui.Text(string.Empty, 12, Ui.Muted);
                muscle.SetBinding(Label.TextProperty, nameof(WorkoutPlanExerciseItem.MuscleGroup));

                Label sets = Ui.Text(string.Empty, 12, Ui.Teal, FontAttributes.Bold);
                sets.SetBinding(Label.TextProperty, nameof(WorkoutPlanExerciseItem.SetSummary));

                Button removeSet = SetAdjustButton("-");
                removeSet.SetBinding(Button.CommandProperty, new Binding(
                    nameof(WorkoutPlansViewModel.RemoveSetCommand),
                    source: _viewModel));
                removeSet.SetBinding(Button.CommandParameterProperty, ".");

                Button addSet = SetAdjustButton("+");
                addSet.SetBinding(Button.CommandProperty, new Binding(
                    nameof(WorkoutPlansViewModel.AddSetCommand),
                    source: _viewModel));
                addSet.SetBinding(Button.CommandParameterProperty, ".");

                Button remove = new()
                {
                    Text = "×",
                    BackgroundColor = Color.FromArgb("#F3F7F5"),
                    TextColor = Ui.Muted,
                    CornerRadius = 18,
                    WidthRequest = 36,
                    HeightRequest = 36,
                    Padding = 0,
                    FontSize = 20
                };
                remove.SetBinding(Button.CommandProperty, new Binding(
                    nameof(WorkoutPlansViewModel.RemoveExerciseCommand),
                    source: _viewModel));
                remove.SetBinding(Button.CommandParameterProperty, ".");

                return new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 18 },
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Auto),
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        ColumnSpacing = 14,
                        Padding = new Thickness(0, 14, 14, 14),
                        Children =
                        {
                            stripe,
                            AddColumn(new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    name,
                                    muscle,
                                    new HorizontalStackLayout
                                    {
                                        Spacing = 8,
                                        Children =
                                        {
                                            sets,
                                            removeSet,
                                            addSet
                                        }
                                    }
                                }
                            }, 1),
                            AddColumn(remove, 2)
                        }
                    }
                };
            })
        };
        list.SetBinding(ItemsView.ItemsSourceProperty, "SelectedPlan.Exercises");

        return new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Ui.Text("Exercises", 20, Ui.Ink, FontAttributes.Bold),
                list
            }
        };
    }

    private View BuildExercisePicker()
    {
        CollectionView list = new()
        {
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(() =>
            {
                Label name = Ui.Text(string.Empty, 16, Ui.Ink, FontAttributes.Bold);
                name.SetBinding(Label.TextProperty, nameof(ExerciseItem.ExerciseName));

                Label muscle = Ui.Text(string.Empty, 12, Ui.Muted);
                muscle.SetBinding(Label.TextProperty, nameof(ExerciseItem.MuscleGroup));

                Button add = new()
                {
                    Text = "+",
                    BackgroundColor = Ui.Teal,
                    TextColor = Colors.White,
                    CornerRadius = 17,
                    WidthRequest = 34,
                    HeightRequest = 34,
                    Padding = 0,
                    FontSize = 18
                };
                add.SetBinding(Button.CommandProperty, new Binding(
                    nameof(WorkoutPlansViewModel.AddExerciseCommand),
                    source: _viewModel));
                add.SetBinding(Button.CommandParameterProperty, ".");

                return new Border
                {
                    BackgroundColor = Color.FromArgb("#F8FFFC"),
                    Stroke = Color.FromArgb("#22009688"),
                    StrokeShape = new RoundRectangle { CornerRadius = 16 },
                    Padding = 14,
                    Margin = new Thickness(0, 0, 0, 8),
                    Content = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        Children =
                        {
                            new VerticalStackLayout { Spacing = 3, Children = { name, muscle } },
                            AddColumn(add, 1)
                        }
                    }
                };
            })
        };
        list.SetBinding(ItemsView.ItemsSourceProperty, nameof(WorkoutPlansViewModel.AvailableExercises));

        Button close = Ui.Link("Close");
        close.SetBinding(Button.CommandProperty, nameof(WorkoutPlansViewModel.CloseExercisePickerCommand));

        Border modal = new()
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 28 },
            Padding = 20,
            Margin = 24,
            VerticalOptions = LayoutOptions.Center,
            Content = new VerticalStackLayout
            {
                Spacing = 14,
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
                            Ui.Text("Add exercise", 22, Ui.Ink, FontAttributes.Bold),
                            AddColumn(close, 1)
                        }
                    },
                    list
                }
            }
        };

        Grid overlay = new()
        {
            BackgroundColor = Color.FromArgb("#88000000"),
            Children = { modal }
        };
        overlay.SetBinding(IsVisibleProperty, nameof(WorkoutPlansViewModel.IsExercisePickerOpen));
        Grid.SetRowSpan(overlay, 2);
        return overlay;
    }

    private View BuildBottomNav()
    {
        Button home = Ui.Nav("H");
        home.SetBinding(Button.CommandProperty, nameof(WorkoutPlansViewModel.BackHomeCommand));

        Button plan = Ui.Nav("W", true);

        return new Border
        {
            BackgroundColor = Ui.Dark,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 34 },
            Padding = 10,
            Margin = new Thickness(24, 0, 24, 24),
            HeightRequest = 72,
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

    private View BuildError()
    {
        Label error = Ui.Text(string.Empty, 13, Ui.Error);
        error.SetBinding(Label.TextProperty, nameof(WorkoutPlansViewModel.ErrorMessage));
        return error;
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

    private static Button SetAdjustButton(string text)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Color.FromArgb("#EEF8F5"),
            TextColor = Ui.Teal,
            CornerRadius = 12,
            WidthRequest = 24,
            HeightRequest = 24,
            MinimumWidthRequest = 24,
            MinimumHeightRequest = 24,
            Padding = 0,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold
        };
    }
}
#pragma warning restore CS0618
