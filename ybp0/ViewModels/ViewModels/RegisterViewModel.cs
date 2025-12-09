using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Username { get => _username; set => _username = value; }
        public string Email { get => _email; set => _email = value; }

        public RegisterViewModel(IDatabaseService databaseService, INavigationService navigationService)
        {
            this._databaseService = databaseService;
            this._navigationService = navigationService;
        }
    }
}
