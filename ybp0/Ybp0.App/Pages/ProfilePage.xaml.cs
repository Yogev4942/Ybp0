using Ybp0.App.Viewmodels;
using Microsoft.Maui.Controls.Shapes;

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

        Button back = IconButton("←");
        back.SetBinding(Button.CommandProperty, nameof(ProfileViewModel.BackHomeCommand));

        Button edit = Ui.Primary("Edit profile");
        edit.HeightRequest = 44;
        edit.CornerRadius = 16;
        edit.SetBinding(Button.CommandProperty, nameof(ProfileViewModel.EditProfileCommand));

        Label name = Ui.Text(string.Empty, 28, Ui.Ink, FontAttributes.Bold);
        name.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Username));
        Label email = Ui.Text(string.Empty, 14, Ui.Muted);
        email.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Email));
        Label bio = Ui.Text(string.Empty, 14, Ui.Muted);
        bio.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Bio));
        Label role = Ui.Text(string.Empty, 16, Ui.Teal, FontAttributes.Bold);
        role.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Role));

        Button signOut = Ui.Primary("Sign out");
        signOut.SetBinding(Button.CommandProperty, nameof(ProfileViewModel.SignOutCommand));

        Label pageTitle = Ui.Text("Profile", 18, Ui.Ink, FontAttributes.Bold);

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
                    Padding = new Thickness(18, 12, 18, 0),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        new VerticalStackLayout
                        {
                            Spacing = 6,
                            Children =
                            {
                                back,
                                pageTitle
                            }
                        },
                        AddColumn(edit, 2)
                    }
                },
                AddRow(new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = new Thickness(24, 28, 24, 34),
                        Spacing = 18,
                        Children =
                        {
                            Ui.CardFrame(new VerticalStackLayout
                            {
                                Spacing = 18,
                                Children =
                                {
                                    new Frame
                                    {
                                        Content = role,
                                        BackgroundColor = Color.FromArgb("#DDF7F0"),
                                        BorderColor = Colors.Transparent,
                                        CornerRadius = 24,
                                        HasShadow = false,
                                        Padding = new Thickness(18, 10),
                                        HorizontalOptions = LayoutOptions.Start
                                    },
                                    new Grid
                                    {
                                        ColumnDefinitions =
                                        {
                                            new ColumnDefinition(GridLength.Auto),
                                            new ColumnDefinition(GridLength.Star)
                                        },
                                        ColumnSpacing = 16,
                                        Children =
                                        {
                                            Avatar(),
                                            AddColumn(new VerticalStackLayout { Spacing = 6, Children = { name, email } }, 1)
                                        }
                                    },
                                    Section("Bio", bio),
                                    new Grid
                                    {
                                        ColumnDefinitions =
                                        {
                                            new ColumnDefinition(GridLength.Star),
                                            new ColumnDefinition(GridLength.Star),
                                            new ColumnDefinition(GridLength.Star)
                                        },
                                        ColumnSpacing = 10,
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
                }, 1)
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

    private static Border Avatar()
    {
        Label initial = Ui.Text(string.Empty, 24, Colors.White, FontAttributes.Bold);
        initial.HorizontalTextAlignment = TextAlignment.Center;
        initial.VerticalTextAlignment = TextAlignment.Center;
        initial.SetBinding(Label.TextProperty, nameof(ProfileViewModel.Initial));

        return new Border
        {
            BackgroundColor = Ui.Teal,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 32 },
            HeightRequest = 64,
            WidthRequest = 64,
            Content = initial
        };
    }

    private static View Section(string title, View content)
    {
        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Ui.Text(title, 13, Ui.Teal, FontAttributes.Bold),
                content
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
