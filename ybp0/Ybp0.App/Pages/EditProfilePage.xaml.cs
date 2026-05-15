using Microsoft.Maui.Controls.Shapes;
using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

#pragma warning disable CS0618
public partial class EditProfilePage : ContentPage
{
    private readonly EditProfileViewModel _viewModel;

    public EditProfilePage(EditProfileViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        BackgroundColor = Color.FromArgb("#F8FAF9");
        NavigationPage.SetHasNavigationBar(this, false);

        Button back = IconButton("←");
        back.SetBinding(Button.CommandProperty, nameof(EditProfileViewModel.BackCommand));

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            Children =
            {
                new Grid
                {
                    Padding = new Thickness(18, 18, 18, 0),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star)
                    },
                    ColumnSpacing = 12,
                    Children =
                    {
                        back,
                        AddColumn(new VerticalStackLayout
                        {
                            Spacing = 2,
                            Children =
                            {
                                Ui.Text("Profile Settings", 26, Color.FromArgb("#004D40"), FontAttributes.Bold),
                                Ui.Text("Manage your public identity and account details.", 13, Ui.Muted)
                            }
                        }, 1)
                    }
                },
                AddRow(new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = new Thickness(24, 28, 24, 34),
                        Spacing = 16,
                        Children =
                        {
                            BuildPublicProfile(),
                            BuildAccountInfo(),
                            BuildStatus()
                        }
                    }
                }, 1)
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private static View BuildPublicProfile()
    {
        Editor bio = new()
        {
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 180,
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink,
            PlaceholderColor = Ui.Muted,
            Placeholder = "Tell people about you"
        };
        bio.SetBinding(Editor.TextProperty, nameof(EditProfileViewModel.Bio));

        Button save = Ui.Primary("Save profile changes");
        save.SetBinding(Button.CommandProperty, nameof(EditProfileViewModel.SaveCommand));

        VerticalStackLayout trainee = new()
        {
            Spacing = 12,
            Children =
            {
                Ui.Text("Fitness Snapshot", 18, Ui.Teal, FontAttributes.Bold),
                Field("Main Goal", nameof(EditProfileViewModel.FitnessGoal)),
                TwoFields("Weight (kg)", nameof(EditProfileViewModel.CurrentWeight), "Height (cm)", nameof(EditProfileViewModel.Height))
            }
        };
        trainee.SetBinding(IsVisibleProperty, nameof(EditProfileViewModel.IsTrainee));

        VerticalStackLayout trainer = new()
        {
            Spacing = 12,
            Children =
            {
                Ui.Text("Coaching Details", 18, Ui.Teal, FontAttributes.Bold),
                Field("Specialization", nameof(EditProfileViewModel.Specialization)),
                TwoFields("Hourly Rate", nameof(EditProfileViewModel.HourlyRate), "Max Trainees", nameof(EditProfileViewModel.MaxTrainees))
            }
        };
        trainer.SetBinding(IsVisibleProperty, nameof(EditProfileViewModel.IsTrainer));

        return Ui.CardFrame(new VerticalStackLayout
        {
            Spacing = 18,
            Children =
            {
                Ui.Text("Public Profile", 22, Ui.Ink, FontAttributes.Bold),
                Labeled("About Me", new Border
                {
                    BackgroundColor = Color.FromArgb("#F5F9F8"),
                    Stroke = Color.FromArgb("#CFD8DC"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Padding = 12,
                    Content = bio
                }),
                trainee,
                trainer,
                save
            }
        }, 22);
    }

    private static View BuildAccountInfo()
    {
        Button save = Ui.Primary("Update account details");
        save.SetBinding(Button.CommandProperty, nameof(EditProfileViewModel.SaveAccountCommand));

        return Ui.CardFrame(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                Ui.Text("Account Info", 22, Ui.Ink, FontAttributes.Bold),
                Field("Username", nameof(EditProfileViewModel.Username)),
                Field("Email Address", nameof(EditProfileViewModel.Email)),
                Ui.Text("Your email and username are used for login and identification.", 12, Ui.Muted),
                save
            }
        }, 22);
    }

    private static View BuildStatus()
    {
        Label status = Ui.Text(string.Empty, 13, Ui.Teal, FontAttributes.Bold);
        status.SetBinding(Label.TextProperty, nameof(EditProfileViewModel.StatusMessage));
        return status;
    }

    private static View Field(string label, string binding)
    {
        Entry entry = new()
        {
            BackgroundColor = Colors.Transparent,
            TextColor = Ui.Ink,
            PlaceholderColor = Ui.Muted
        };
        entry.SetBinding(Entry.TextProperty, binding);

        return Labeled(label, new Border
        {
            BackgroundColor = Color.FromArgb("#F5F9F8"),
            Stroke = Color.FromArgb("#CFD8DC"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(12, 2),
            Content = entry
        });
    }

    private static View TwoFields(string leftLabel, string leftBinding, string rightLabel, string rightBinding)
    {
        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 12,
            Children =
            {
                Field(leftLabel, leftBinding),
                AddColumn(Field(rightLabel, rightBinding), 1)
            }
        };
    }

    private static View Labeled(string label, View field)
    {
        return new VerticalStackLayout
        {
            Spacing = 7,
            Children =
            {
                Ui.Text(label, 13, Ui.Teal, FontAttributes.Bold),
                field
            }
        };
    }

    private static Button IconButton(string text)
    {
        return new Button
        {
            Text = text,
            BackgroundColor = Colors.White,
            TextColor = Ui.Ink,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 22,
            HeightRequest = 44,
            WidthRequest = 44,
            Padding = 0
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
