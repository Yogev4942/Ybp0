using Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainer profile views.
    /// Contains trainer-specific properties like Specialization, Rating, TraineeCount,
    /// pending request management, and request-sending for trainees viewing this profile.
    /// </summary>
    public class TrainerProfileViewModel : ProfileViewModel
    {
        private ObservableCollection<Trainee> _pendingRequests;
        public ObservableCollection<Trainee> PendingRequests
        {
            get => _pendingRequests;
            set => SetProperty(ref _pendingRequests, value);
        }

        private bool _isRequestsPopupOpen;
        public bool IsRequestsPopupOpen
        {
            get => _isRequestsPopupOpen;
            set => SetProperty(ref _isRequestsPopupOpen, value);
        }

        // Trainer-specific properties
        public override string UserTypeLabel => "Trainer";
        
        public string Specialization => (ViewedUser as Trainer)?.Specialization;
        public string TraineesDisplay => $"{(ViewedUser as Trainer)?.TotalTrainees} / {(ViewedUser as Trainer)?.MaxTrainees}";
        public string HourlyRateDisplay => $"${(ViewedUser as Trainer)?.HourlyRate}/hr";
        public double AverageRating => (ViewedUser as Trainer)?.Rating ?? 0;

        public bool IsAcceptingTrainees 
        {
            get 
            {
                var trainer = ViewedUser as Trainer;
                return trainer != null && trainer.TotalTrainees < trainer.MaxTrainees;
            }
        }

        // === Trainee viewing trainer: Request functionality ===
        private string _requestStatus;
        public string RequestStatus
        {
            get => _requestStatus;
            set => SetProperty(ref _requestStatus, value);
        }

        /// <summary>True when a trainee is viewing this trainer's profile (not the trainer's own profile)</summary>
        public bool IsTraineeViewing => !IsOwnProfile && !ActiveUser.IsTrainer;

        /// <summary>Show "Request Training" button when trainee hasn't sent a request yet</summary>
        public bool CanRequestTrainer => IsTraineeViewing && RequestStatus == null;

        /// <summary>Show "⏳ Request Pending" label</summary>
        public bool IsRequestPending => IsTraineeViewing && RequestStatus == "Pending";

        /// <summary>Show "✅ Your Trainer" label</summary>
        public bool IsRequestApproved => IsTraineeViewing && RequestStatus == "Approved";

        public ICommand RequestTrainerCommand { get; }

        // Commands for request management (trainer's own profile)
        public ICommand ApproveRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }
        public ICommand ToggleRequestsCommand { get; }

        public TrainerProfileViewModel(IDatabaseService database, INavigationService navigation, User activeUser, User viewedUser)
            : base(database, navigation, activeUser, viewedUser)
        {
            ApproveRequestCommand = new RelayCommand(p => HandleRequest(p as Trainee, "Approved"));
            RejectRequestCommand = new RelayCommand(p => HandleRequest(p as Trainee, "Rejected"));
            ToggleRequestsCommand = new RelayCommand(_ => IsRequestsPopupOpen = !IsRequestsPopupOpen);
            RequestTrainerCommand = new RelayCommand(_ => RequestTrainer());

            LoadPendingRequests();
            LoadRequestStatus();
        }

        private void LoadPendingRequests()
        {
            if (IsOwnProfile)
            {
                var list = _database.GetPendingRequests(ActiveUser.Id);
                PendingRequests = new ObservableCollection<Trainee>(list);
            }
        }

        private void LoadRequestStatus()
        {
            if (IsTraineeViewing)
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

        private void HandleRequest(Trainee trainee, string status)
        {
            if (trainee == null) return;
            if (_database.HandleTrainerRequest(trainee.Id, ActiveUser.Id, status))
            {
                PendingRequests.Remove(trainee);
                if (PendingRequests.Count == 0)
                    IsRequestsPopupOpen = false;
            }
        }
    }
}
