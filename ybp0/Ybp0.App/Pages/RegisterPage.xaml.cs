using Ybp0.App.Viewmodels;

namespace Ybp0.App.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = Ui.Mint;
        NavigationPage.SetHasNavigationBar(this, false);

        Entry user = BoundEntry("Username", nameof(RegisterViewModel.Username));
        Entry email = BoundEntry("Email", nameof(RegisterViewModel.Email), Keyboard.Email);
        Entry password = BoundEntry("Password", nameof(RegisterViewModel.Password));
        password.IsPassword = true;
        Picker role = new() { Title = "Role" };
        role.SetBinding(Picker.ItemsSourceProperty, nameof(RegisterViewModel.Roles));
        role.SetBinding(Picker.SelectedItemProperty, nameof(RegisterViewModel.SelectedRole));

        VerticalStackLayout traineeFields = new() { Spacing = 12 };
        traineeFields.SetBinding(IsVisibleProperty, nameof(RegisterViewModel.IsTrainee));
        traineeFields.Children.Add(Ui.Field(BoundEntry("Fitness goal", nameof(RegisterViewModel.FitnessGoal))));
        traineeFields.Children.Add(Ui.Field(BoundEntry("Weight kg", nameof(RegisterViewModel.CurrentWeight), Keyboard.Numeric)));
        traineeFields.Children.Add(Ui.Field(BoundEntry("Height cm", nameof(RegisterViewModel.Height), Keyboard.Numeric)));

        VerticalStackLayout trainerFields = new() { Spacing = 12 };
        trainerFields.SetBinding(IsVisibleProperty, nameof(RegisterViewModel.IsTrainer));
        trainerFields.Children.Add(Ui.Field(BoundEntry("Specialization", nameof(RegisterViewModel.Specialization))));
        trainerFields.Children.Add(Ui.Field(BoundEntry("Hourly rate", nameof(RegisterViewModel.HourlyRate), Keyboard.Numeric)));
        trainerFields.Children.Add(Ui.Field(BoundEntry("Max trainees", nameof(RegisterViewModel.MaxTrainees), Keyboard.Numeric)));

        Label error = Ui.Text(string.Empty, 13, Ui.Error);
        error.SetBinding(Label.TextProperty, nameof(RegisterViewModel.ErrorMessage));
        Button back = Ui.Link("Back to login");
        back.SetBinding(Button.CommandProperty, nameof(RegisterViewModel.GoToLoginCommand));
        Button create = Ui.Primary("Create account");
        create.SetBinding(Button.CommandProperty, nameof(RegisterViewModel.RegisterCommand));

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24, 34),
                Spacing = 18,
                Children =
                {
                    Ui.CardFrame(new VerticalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            back,
                            Ui.Text("Create account", 32, Ui.Ink, FontAttributes.Bold),
                            Ui.Text("Choose trainee or trainer and send the exact fields your API expects.", 14, Ui.Muted),
                            error,
                            Ui.Field(user),
                            Ui.Field(email),
                            Ui.Field(password),
                            Ui.Field(role),
                            traineeFields,
                            trainerFields,
                            create
                        }
                    }, 30)
                }
            }
        };
    }

    private static Entry BoundEntry(string placeholder, string path, Keyboard? keyboard = null)
    {
        Entry entry = new() { Placeholder = placeholder, Keyboard = keyboard ?? Keyboard.Default };
        entry.SetBinding(Entry.TextProperty, path);
        return entry;
    }
}
