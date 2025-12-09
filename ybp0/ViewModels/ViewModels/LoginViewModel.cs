using System;
using System.Windows.Input;
using Models;

namespace ViewModels.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly INavigationService _navigationService;

        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }

        public LoginViewModel(IDatabaseService databaseService, INavigationService navigationService)
        {
            _databaseService = databaseService;
            _navigationService = navigationService;

            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
            NavigateToSignUpCommand = new RelayCommand(OnNavigateToSignUp);
        }

        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnLogin(object parameter)
        {
            try
            {
                // Clear previous error message
                ErrorMessage = string.Empty;

                // Validate credentials using your database service
                bool isValid = _databaseService.ValidateLogin(Username, Password);

                if (isValid)
                {
                    // Get the full user object
                    User currentUser = _databaseService.GetUserByUsernameAndPassword(Username, Password);

                    if (currentUser != null)
                    {
                        _navigationService.OnLoginSuccess();
                        // Login successful - navigate to home/dashboard
                        _navigationService.NavigateTo<HomeViewModel>();

                        // Optional: Store current user somewhere for app-wide access
                        // CurrentUser.Instance = currentUser;
                    }
                    else
                    {
                        ErrorMessage = "Login failed. Please try again.";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }

                // Clear password field for security
                Password = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                // For debugging - remove in production
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
        }

        private void OnForgotPassword(object parameter)
        {
            // Navigate to forgot password view or show dialog
            // _navigationService.NavigateTo<ForgotPasswordViewModel>();

            // For now, show a message
            ErrorMessage = "Forgot password feature coming soon!";
        }

        private void OnNavigateToSignUp(object parameter)
        {
            _navigationService.NavigateTo<RegisterViewModel>();
            ErrorMessage = "Sign up feature coming soon!";
        }
    }
}