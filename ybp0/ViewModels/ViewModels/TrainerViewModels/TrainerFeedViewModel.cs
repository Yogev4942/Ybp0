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
            });

            DeletePostCommand = new RelayCommand(param =>
            {
                if (param is PostViewModel post)
                {
                    Posts.Remove(post);
                    // TODO: Also delete from database
                }
            });
        }

        private void DeletePost(PostViewModel post)
        {
            if (Posts.Contains(post))
            {
                Posts.Remove(post);
                // TODO: Database delete
            }
        }
    }
}
