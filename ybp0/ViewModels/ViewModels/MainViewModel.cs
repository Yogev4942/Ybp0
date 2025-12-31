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

            if (type == typeof(HomeViewModel))
            {
                _currUser = (User)parameter;
                return new HomeViewModel(Database, Navigation, (User)parameter);
            }

            if (type == typeof(CalendarViewModel))
                return new CalendarViewModel(Database,Navigation, _currUser);

            if (type == typeof(FeedViewModel))
                return new FeedViewModel(Database,Navigation,_currUser);
            // fallback
            return (BaseViewModel)Activator.CreateInstance(type);
        }
    }

}