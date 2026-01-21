using System;
using System.Collections.ObjectModel;
using Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Base ViewModel for Feed views, containing shared properties and logic.
    /// </summary>
    public abstract class BaseFeedViewModel : BaseViewModel
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

        protected BaseFeedViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _databaseService = database;
            _navigationService = navigation;
            _activeUser = user;
            
            LoadPosts();
        }

        /// <summary>
        /// Loads posts from the database. Override in subclasses if needed.
        /// </summary>
        protected virtual void LoadPosts()
        {
            // Initialize with Mock Data for now
            Posts = new ObservableCollection<PostViewModel>
            {
                new PostViewModel(_navigationService, 1, "GymRat99", "Just hit a new PR on bench press! 100kg let's go! 💪", "2 mins ago", "#FF00BCD4"),
                new PostViewModel(_navigationService, 2, "SarahFitness", "Morning cardio done. The sunrise was beautiful today. 🌅 #Run #Morning", "15 mins ago", "#FF9C27B0"),
                new PostViewModel(_navigationService, 3, "MikeLiftz", "Anyone want to join for a leg day session tomorrow at 5PM?", "1 hour ago", "#FF4CAF50"),
                new PostViewModel(_navigationService, 4, "YogaQueen", "Remember to stretch after your workouts! Flexibility is key to longevity.", "3 hours ago", "#FFFF9800"),
                new PostViewModel(_navigationService, 5, "CoachDave", "Hydration is not just about water, electrolytes matter too. Stay safe out there.", "5 hours ago", "#FF3F51B5"),
            };
        }
    }
}
