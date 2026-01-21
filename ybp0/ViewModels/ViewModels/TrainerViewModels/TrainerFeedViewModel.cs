using System.Windows.Input;
using Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainer feed views.
    /// Trainers have additional moderation capabilities: delete and pin posts.
    /// </summary>
    public class TrainerFeedViewModel : FeedViewModel
    {
        // Trainer-specific commands
        public ICommand LikePostCommand { get; }
        public ICommand DeletePostCommand { get; }
        public ICommand PinPostCommand { get; }

        public TrainerFeedViewModel(IDatabaseService database, INavigationService navigation, User user)
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

            DeletePostCommand = new RelayCommand(param =>
            {
                if (param is PostViewModel post)
                {
                    Posts.Remove(post);
                    // TODO: Also delete from database
                }
            });

            PinPostCommand = new RelayCommand(param =>
            {
                if (param is PostViewModel post)
                {
                    // TODO: Implement pin functionality
                    // Move post to top of feed or mark as pinned
                }
            });
        }
    }
}
