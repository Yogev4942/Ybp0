using Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainer calendar views.
    /// Currently inherits all functionality from base class.
    /// Future trainer-specific features can be added here.
    /// </summary>
    public class TrainerCalendarViewModel : BaseCalendarViewModel
    {
        public TrainerCalendarViewModel(IDatabaseService dbService, INavigationService navigationService, User user)
            : base(dbService, navigationService, user)
        {
            // Trainer-specific initialization can be added here
        }

        // Future trainer-specific calendar features:
        // - Assign plans to trainees
        // - Template management
        // - Bulk exercise operations
        // - Trainee plan switching dropdown
    }
}
