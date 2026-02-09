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
            Posts = new ObservableCollection<PostViewModel>();
            NavigateToCreatePostCommand = new RelayCommand(NavigateToCreatePost);

            LoadPosts();
        }

        protected void LoadPosts()
        {
            try
            {
                var posts = _databaseService.GetAllPosts();
                Posts.Clear();

                foreach (var post in posts)
                {
                    // Get the owner for this post
                    var owner = _databaseService.GetUserById(post.OwnerId);

                    // Convert Post to PostViewModel
                    var postViewModel = new PostViewModel(
                        post,
                        owner,
                        _activeUser,
                        _navigationService,
                        DeletePost // Pass delete action
                    );

                    Posts.Add(postViewModel);
                }
            }
            catch (Exception ex)
            {
                // TODO: Actually handle this error
            }
        }

        private void DeletePost(PostViewModel postVM)
        {
            // Delete from database
            //_databaseService.DeletePost(postVM.OwnerId); fill it

            // Remove from UI
            Posts.Remove(postVM);
        }

        public ICommand NavigateToCreatePostCommand { get; private set; }

        private void NavigateToCreatePost(object parameter)
        {
            _navigationService.NavigateTo<CreatePostViewModel>(parameter);
        }
    }
}
