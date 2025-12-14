using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly INavigationService _navigationService;

        
        private string _username;
        private string _email;
        private string _password;
        private string _errorMessage;

        public string Username { get => _username; set => SetProperty(ref _username, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ErrorMsg { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        public RegisterViewModel(IDatabaseService databaseService, INavigationService navigationService)
        {
            this._databaseService = databaseService;
            this._navigationService = navigationService;
            GoBackCommand = new RelayCommand(_ => _navigationService.NavigateTo<LoginViewModel>());
        }
        public ICommand RegisterCommand;
        public ICommand GoBackCommand { get; }

        public bool Register()
        {
            if (_databaseService.UserExist(Username,Email))
            {
                ErrorMsg = "Username or email exists";
                return false;
            }
            try
            {
                if (_databaseService.RegisterUser(Username,Email,Password))
                {
                    _navigationService.NavigateTo<HomeViewModel>();
                }
                else
                {
                    ErrorMsg = "Error Creating user";
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = "An error occurred during login. Please try again.";
                // For debugging - remove in production
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
            return true;
        }
    }
}
