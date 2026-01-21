using Models;
using System;
using System.CodeDom;
using System.Net.NetworkInformation;
using System.Windows.Input;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private User _currUser;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public static IDatabaseService Database { get; } = new DatabaseService();
        public INavigationService Navigation { get; }

        public ICommand NavigateCommand { get; } // Generic generic command

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _initials;
        public string Initials
        {
            get => _initials;
            set => SetProperty(ref _initials, value);
        }

        private string _avatarColor;
        public string AvatarColor
        {
            get => _avatarColor;
            set => SetProperty(ref _avatarColor, value);
        }

        public MainViewModel()
        {
            // create navigation service with factory + setter + onLogin callback
            Navigation = new NavigationService(
                factory: CreateViewModel,
                setCurrentViewModel: vm =>
                {
                    CurrentViewModel = vm;

                    // Hide top nav on Login/Register, show otherwise
                    IsLoggedIn = !(vm is LoginViewModel) && !(vm is RegisterViewModel);
                },
                onLogin: () =>
                {
                    // called when OnLoginSuccess invoked from navigation service
                    // set IsLoggedIn here (optional, also handled above when current view changes)
                    IsLoggedIn = true;
                }
            );

            // Generic logic: Command Parameter can be Type or string
            NavigateCommand = new RelayCommand(param => 
            {
                if (param is Type type)
                {
                    Navigation.NavigateTo(type);
                }
                else if (param is string name)
                {
                    // Handle string-based navigation for views that can't use x:Type
                    switch (name)
                    {
                        case "Feed":
                            Navigation.NavigateTo<FeedViewModel>();
                            break;
                    }
                }
            });

            // start on login
            Navigation.NavigateTo<LoginViewModel>();
        }

        private BaseViewModel CreateViewModel(Type type, object parameter)
        {
            if (type == typeof(LoginViewModel))
                return new LoginViewModel(Database, Navigation);

            if (type == typeof(RegisterViewModel))
                return new RegisterViewModel(Database, Navigation);

            // Direct navigation to specific register view models
            if (type == typeof(TraineeRegisterViewModel))
                return new TraineeRegisterViewModel(Database, Navigation);

            if (type == typeof(TrainerRegisterViewModel))
                return new TrainerRegisterViewModel(Database, Navigation);

            if (type == typeof(HomeViewModel))
            {
                if (parameter is User user)
                {
                    _currUser = user;
                    UpdateUserDisplayData();
                }
                return new HomeViewModel(Database, Navigation, _currUser);
            }

            if (type == typeof(CalendarViewModel))
            {
                // Route to specific calendar view model based on user type
                if (_currUser.IsTrainer)
                    return new TrainerCalendarViewModel(Database, Navigation, _currUser);
                else
                    return new TraineeCalendarViewModel(Database, Navigation, _currUser);
            }

            if (type == typeof(FeedViewModel))
            {
                // Route to specific feed view model based on user type
                if (_currUser.IsTrainer)
                    return new TrainerFeedViewModel(Database, Navigation, _currUser);
                else
                    return new TraineeFeedViewModel(Database, Navigation, _currUser);
            }

            if (type == typeof(ProfileViewModel))
            {
                User targetUser = null;
                if (parameter is User user)
                    targetUser = user;
                else if (parameter is int userId)
                    targetUser = Database.GetUserById(userId);

                // Fallback to current user if nothing else works
                targetUser = targetUser ?? _currUser;
                
                // Route to specific profile view model based on TARGET user type
                if (targetUser.IsTrainer)
                    return new TrainerProfileViewModel(Database, Navigation, _currUser, targetUser);
                else
                    return new TraineeProfileViewModel(Database, Navigation, _currUser, targetUser);
            }

            if (type == typeof(EditProfileViewModel))
            {
                User targetUser = (parameter as User) ?? _currUser;
                return new EditProfileViewModel(Database, Navigation, targetUser);
            }

            // fallback
            return (BaseViewModel)Activator.CreateInstance(type);
        }

        private void UpdateUserDisplayData()
        {
            if (_currUser != null)
            {
                Username = _currUser.Username;
                Initials = !string.IsNullOrEmpty(Username) && Username.Length >= 2 
                    ? Username.Substring(0, 2).ToUpper() 
                    : Username?[0].ToString().ToUpper();
                AvatarColor = GetColorForUser(Username);
            }
        }

        private string GetColorForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return "#FF00BCD4";
            int hash = username.GetHashCode();
            var colors = new[] { "#FF00BCD4", "#FF9C27B0", "#FF4CAF50", "#FFFF9800", "#FF3F51B5", "#FFE91E63" };
            return colors[Math.Abs(hash) % colors.Length];
        }
    }

}