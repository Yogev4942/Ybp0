using Models;
using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainee profile views.
    /// Contains trainee-specific properties like FitnessGoal, Weight, Height, BMI,
    /// and trainer request functionality.
    /// </summary>
    public class TraineeProfileViewModel : ProfileViewModel
    {
        private string _requestStatus;
        public string RequestStatus
        {
            get => _requestStatus;
            set => SetProperty(ref _requestStatus, value);
        }

        // Trainee-specific properties
        public override string UserTypeLabel => "Trainee";
        
        public string FitnessGoal => (ViewedUser as Trainee)?.FitnessGoal;
        public string CurrentWeightDisplay => $"{(ViewedUser as Trainee)?.CurrentWeight} kg";
        public string HeightDisplay => $"{(ViewedUser as Trainee)?.Height} cm";
        
        public string BMI 
        {
            get
            {
                var trainee = ViewedUser as Trainee;
                if (trainee == null || trainee.Height <= 0) return "N/A";
                double bmi = trainee.CurrentWeight / Math.Pow(trainee.Height / 100, 2);
                return bmi.ToString("F1");
            }
        }

        // Request trainer functionality (trainee viewing a trainer's profile)
        public bool CanRequestTrainer => ViewedUser is Trainer && !IsOwnProfile && RequestStatus == null;
        public bool IsRequestPending => RequestStatus == "Pending";
        public bool IsRequestApproved => RequestStatus == "Approved";

        public ICommand RequestTrainerCommand { get; }

        public TraineeProfileViewModel(IDatabaseService database, INavigationService navigation, User activeUser, User viewedUser)
            : base(database, navigation, activeUser, viewedUser)
        {
            RequestTrainerCommand = new RelayCommand(_ => RequestTrainer());
            LoadRequestStatus();
        }

        private void LoadRequestStatus()
        {
            if (ActiveUser == null || ViewedUser == null) return;
            
            // Trainee viewing a trainer's profile - check request status
            if (ViewedUser is Trainer)
            {
                RequestStatus = _database.GetTrainerRequestStatus(ActiveUser.Id, ViewedUser.Id);
            }
        }

        private void RequestTrainer()
        {
            if (_database.SendTrainerRequest(ActiveUser.Id, ViewedUser.Id))
            {
                RequestStatus = "Pending";
                OnPropertyChanged(nameof(CanRequestTrainer));
                OnPropertyChanged(nameof(IsRequestPending));
            }
        }
    }
}
