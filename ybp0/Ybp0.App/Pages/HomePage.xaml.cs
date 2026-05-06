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

        Label greeting = Ui.Text(string.Empty, 24, Ui.Ink, FontAttributes.Bold);
        greeting.SetBinding(Label.TextProperty, nameof(HomeViewModel.Greeting));
        Label role = Ui.Text(string.Empty, 13, Ui.Muted);
        role.SetBinding(Label.TextProperty, nameof(HomeViewModel.RoleLabel));
        Label focus = Ui.Text(string.Empty, 24, Ui.Ink, FontAttributes.Bold);
        focus.SetBinding(Label.TextProperty, nameof(HomeViewModel.FocusMetric));
        Label goal = Ui.Text(string.Empty, 16, Ui.Ink, FontAttributes.Bold);
        goal.SetBinding(Label.TextProperty, nameof(HomeViewModel.GoalMetric));

        Button profile = Ui.Nav("○");
        profile.SetBinding(Button.CommandProperty, nameof(HomeViewModel.OpenProfileCommand));
        Button refresh = Ui.Link("Refresh");
        refresh.SetBinding(Button.CommandProperty, nameof(HomeViewModel.RefreshCommand));

        CollectionView trainers = new() { HeightRequest = 170, SelectionMode = SelectionMode.None };
        trainers.SetBinding(ItemsView.ItemsSourceProperty, nameof(HomeViewModel.Trainers));
        trainers.ItemTemplate = new DataTemplate(() =>
        {
            Label name = Ui.Text(string.Empty, 16, Ui.Ink, FontAttributes.Bold);
            name.SetBinding(Label.TextProperty, "Username");
            Label spec = Ui.Text(string.Empty, 12, Ui.Muted);
            spec.SetBinding(Label.TextProperty, "Specialization");
            return Ui.CardFrame(new VerticalStackLayout { Spacing = 3, Children = { name, spec } }, 18);
        });

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
                        Padding = new Thickness(24, 34, 24, 12),
                        Spacing = 18,
                        Children =
                        {
                            new Grid
                            {
                                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                                Children = { new VerticalStackLayout { Spacing = 2, Children = { greeting, role } }, profile }
                            },
                            Ui.CardFrame(new VerticalStackLayout
                            {
                                Spacing = 12,
                                Children =
                                {
                                    Ui.Text("Today", 12, Ui.Teal, FontAttributes.Bold),
                                    Ui.Text("Training dashboard", 30, Ui.Ink, FontAttributes.Bold),
                                    Ui.Text("Your API data appears here after login.", 14, Ui.Muted),
                                    new Grid
                                    {
                                        ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                                        Children =
                                        {
                                            Ui.CardFrame(new VerticalStackLayout { Children = { Ui.Text("Focus", 12, Ui.Muted), focus } }, 20),
                                            AddColumn(Ui.CardFrame(new VerticalStackLayout { Children = { Ui.Text("Goal", 12, Ui.Muted), goal } }, 20), 1)
                                        }
                                    },
                                    refresh
                                }
                            }, 30),
                            Ui.Text("Available trainers", 20, Ui.Ink, FontAttributes.Bold),
                            trainers
                        }
                    }
                },
                AddRow(new Frame
                {
                    BackgroundColor = Ui.Dark,
                    BorderColor = Colors.Transparent,
                    CornerRadius = 34,
                    HasShadow = false,
                    Padding = 10,
                    Margin = new Thickness(24, 0, 24, 24),
                    Content = new HorizontalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 16,
                        Children = { Ui.Nav("⌂", true), Ui.Nav("▦"), Ui.Nav("▥"), profile }
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
