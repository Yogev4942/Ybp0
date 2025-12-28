using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Models;
using System.Collections.ObjectModel;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for the Feed view.
    /// </summary>
    public class FeedViewModel : BaseViewModel
    {
        private readonly User _activeUser;
        private readonly IDatabaseService _databaseService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<PostViewModel> _posts;
        public ObservableCollection<PostViewModel> Posts
        {
            get => _posts;
            set => SetProperty(ref _posts, value);
        }

        public FeedViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _databaseService = database;
            _navigationService = navigation;
            _activeUser = user;

            // Initialize with Mock Data
            Posts = new ObservableCollection<PostViewModel>
            {
                new PostViewModel("GymRat99", "Just hit a new PR on bench press! 100kg let's go! 💪", "2 mins ago", "#FF00BCD4"),
                new PostViewModel("SarahFitness", "Morning cardio done. The sunrise was beautiful today. 🌅 #Run #Morning", "15 mins ago", "#FF9C27B0"),
                new PostViewModel("MikeLiftz", "Anyone want to join for a leg day session tomorrow at 5PM?", "1 hour ago", "#FF4CAF50"),
                new PostViewModel("YogaQueen", "Remember to stretch after your workouts! Flexibility is key to longevity.", "3 hours ago", "#FFFF9800"),
                new PostViewModel("CoachDave", "Hydration is not just about water, electrolytes matter too. Stay safe out there.", "5 hours ago", "#FF3F51B5"),
            };
        }
    }
}
