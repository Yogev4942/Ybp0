using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ViewModels.ViewModels
{
    public class ChatsViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly INavigationService _navigationService;
        private readonly User _currentUser;
        public ChatsViewModel(IDatabaseService dbService, INavigationService navigationService, User currentUser)
        {
            _dbService = dbService;
            _navigationService = navigationService;
            _currentUser = currentUser;
        }

    }
}
