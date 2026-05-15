using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Base ViewModel for registration views, containing shared fields and validation.
    /// </summary>
    public abstract class RegisterViewModel : BaseViewModel
    {
        protected readonly IDatabaseService _databaseService;
        protected readonly INavigationService _navigationService;

        // Basic user fields
        private string _username;
        private string _email;
        private string _password;
        private string _errorMessage;

        // Basic properties
        public string Username 
        { 
            get => _username; 
            set => SetProperty(ref _username, value); 
        }
        
        public string Email 
        { 
            get => _email; 
            set => SetProperty(ref _email, value); 
        }
        
        public string Password 
        { 
            get => _password; 
            set => SetProperty(ref _password, value); 
        }
        
        public string ErrorMsg 
        { 
            get => _errorMessage; 
            set => SetProperty(ref _errorMessage, value); 
        }

        public ICommand GoBackCommand { get; protected set; }
        public ICommand RegisterCommand { get; protected set; }

        protected RegisterViewModel(IDatabaseService databaseService, INavigationService navigationService)
        {
            _databaseService = databaseService;
            _navigationService = navigationService;
            GoBackCommand = new RelayCommand(_ => _navigationService.NavigateTo<LoginViewModel>());
        }

        /// <summary>
        /// Basic validation for required fields
        /// </summary>
        protected bool ValidateBasicFields()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email))
            {
                ErrorMsg = "Fill out all fields";
                return false;
            }

            // Check if user exists
            if (_databaseService.UserExist(Username, Email))
            {
                ErrorMsg = "Username or email already exists";
                return false;
            }

            ErrorMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// Navigate to login after successful registration
        /// </summary>
        protected void NavigateToLogin()
        {
            GoBackCommand.Execute(null);
        }
    }
}
