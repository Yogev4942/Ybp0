using Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainee calendar views.
    /// Currently inherits all functionality from base class.
    /// Future trainee-specific features can be added here.
    /// </summary>
    public class TraineeCalendarViewModel : BaseCalendarViewModel
    {
        public TraineeCalendarViewModel(IDatabaseService dbService, INavigationService navigationService, User user)
            : base(dbService, navigationService, user)
        {
            // Trainee-specific initialization can be added here
        }

        // Future trainee-specific calendar features:
        // - View-only mode when viewing trainer-assigned plans
        // - Progress tracking
        // - Workout completion marking
    }
}
