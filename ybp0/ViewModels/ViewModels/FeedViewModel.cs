using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for the Feed view - stub for future implementation.
    /// </summary>
    public class FeedViewModel : BaseViewModel
    {
        private readonly User _activeUser;
        private readonly IDatabaseService _databaseService;
        private readonly INavigationService _navigationService;


        private string _welcomeMessage;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public FeedViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _databaseService = database;
            _navigationService = navigation;
            _activeUser = user;
            WelcomeMessage = "Feed coming soon!";
        }
    }
}
