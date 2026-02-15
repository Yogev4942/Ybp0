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
        public TraineeFeedViewModel(IDatabaseService database, INavigationService navigation, User user)
            : base(database, navigation, user)
        {
        }
    }
}
