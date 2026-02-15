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
        // Trainer-specific commands (beyond base feed)
        public ICommand PinPostCommand { get; }

        public TrainerFeedViewModel(IDatabaseService database, INavigationService navigation, User user)
            : base(database, navigation, user)
        {
        }
    }
}
