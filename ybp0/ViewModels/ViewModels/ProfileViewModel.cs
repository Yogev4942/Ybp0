using DataBase;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly User _activeUser;
        private readonly IDatabaseService _database;
        private readonly INavigationService _navigation;

        public ProfileViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _activeUser = user;
            _navigation = navigation;
            _database = database;
        }
    }
}
