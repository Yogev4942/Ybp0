using System;
using System.Windows.Input;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        public static IDatabaseService Database { get; } = new DatabaseService();
        public INavigationService Navigation { get; private set; }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            Navigation = new NavigationService(
     CreateViewModelWithParams,
     vm => CurrentViewModel = vm
 );

            CurrentViewModel = new LoginViewModel(Database, Navigation);

        }


        private void SetCurrentViewModel(object viewModel)
        {
            if (viewModel is BaseViewModel vm)
            {
                CurrentViewModel = vm;
            }
        }

        private void Navigate(object parameter)
        {
            if (parameter == null) return;

            if (parameter is Type targetType)
            {
                var vm = CreateViewModel(targetType);
                if (vm != null)
                    CurrentViewModel = vm;
            }
        }

        private BaseViewModel CreateViewModel(Type type)
        {
            if (type == typeof(LoginViewModel))
                return new LoginViewModel(Database, Navigation);

            if (type == typeof(HomeViewModel))
                return new HomeViewModel();

            if (type == typeof(CalendarViewModel))
                return new CalendarViewModel();

            if (type == typeof(RegisterViewModel))
                return new RegisterViewModel(Database,Navigation);

            return (BaseViewModel)Activator.CreateInstance(type);
        }
    }
}