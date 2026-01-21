using System.Windows.Input;
using Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainee feed views.
    /// Trainees can view and like posts, but cannot delete or pin them.
    /// </summary>
    public class TraineeFeedViewModel : FeedViewModel
    {
        // Trainee-specific: Like command only (no delete/pin)
        public ICommand LikePostCommand { get; }

        public TraineeFeedViewModel(IDatabaseService database, INavigationService navigation, User user)
            : base(database, navigation, user)
        {
            LikePostCommand = new RelayCommand(param =>
            {
                // TODO: Implement like functionality
                if (param is PostViewModel post)
                {
                    // Like the post
                }
            });
        }
    }
}
