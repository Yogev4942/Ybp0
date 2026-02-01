using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Base ViewModel for Feed views, containing shared properties and logic.
    /// </summary>
    public abstract class FeedViewModel : BaseViewModel
    {
        protected readonly User _activeUser;
        protected readonly IDatabaseService _databaseService;
        protected readonly INavigationService _navigationService;

        private ObservableCollection<PostViewModel> _posts;
        public ObservableCollection<PostViewModel> Posts
        {
            get => _posts;
            set => SetProperty(ref _posts, value);
        }

        protected FeedViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _databaseService = database;
            _navigationService = navigation;
            _activeUser = user;
            NavigateToCreatePostCommand = new RelayCommand(NavigateToCreatePost);


        }
        ICommand NavigateToCreatePostCommand;

        private void NavigateToCreatePost(object parameter)
        {
            _navigationService.NavigateTo<PostViewModel>(parameter);
        }

    }
}
