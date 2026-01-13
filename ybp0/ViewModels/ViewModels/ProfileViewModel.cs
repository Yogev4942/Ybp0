using DataBase;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input; // Assuming RelayCommand and ICommand are in this namespace or similar

namespace ViewModels.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IDatabaseService _database;
        private readonly INavigationService _navigation;
        
        public User ActiveUser { get; }
        public User ViewedUser { get; }

        // Helper properties for ViewedUser (binding convenience)
        public string Username => ViewedUser?.Username;
        public string Email => ViewedUser?.Email;
        public string Bio => ViewedUser?.Bio;
        public string Gender => ViewedUser?.Gender;
        public string JoinDateText => $"Joined {ViewedUser?.Joindate}";
        public bool IsTrainer => ViewedUser is Trainer;
        public bool IsTrainee => ViewedUser is Trainee;
        
        public string UserTypeLabel => IsTrainer ? "Trainer" : "Trainee";
        
        // Avatar Helpers
        public string Initials => !string.IsNullOrEmpty(Username) && Username.Length >= 2 
            ? Username.Substring(0, 2).ToUpper() 
            : Username?[0].ToString().ToUpper();
        
        public string AvatarColor => GetColorForUser(Username);

        // Type-specific properties
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

        private string _requestStatus;
        public string RequestStatus
        {
            get => _requestStatus;
            set => SetProperty(ref _requestStatus, value);
        }

        public bool IsOwnProfile => ActiveUser?.Id == ViewedUser?.Id;
        public bool CanRequestTrainer => !ActiveUser.IsTrainer && ViewedUser.IsTrainer && !IsOwnProfile && RequestStatus == null;
        public bool IsRequestPending => RequestStatus == "Pending";
        public bool IsRequestApproved => RequestStatus == "Approved";

        public ICommand RequestTrainerCommand { get; }
        public ICommand ApproveRequestCommand { get; }
        public ICommand RejectRequestCommand { get; }
        public ICommand ToggleRequestsCommand { get; }
        public ICommand EditProfileCommand { get; }

        public ProfileViewModel(IDatabaseService database, INavigationService navigation, User activeUser, User viewedUser)
        {
            _database = database;
            _navigation = navigation;
            ActiveUser = activeUser;
            ViewedUser = viewedUser;

            RequestTrainerCommand = new RelayCommand(_ => RequestTrainer());
            ApproveRequestCommand = new RelayCommand(p => HandleRequest(p as Trainee, "Approved"));
            RejectRequestCommand = new RelayCommand(p => HandleRequest(p as Trainee, "Rejected"));
            ToggleRequestsCommand = new RelayCommand(_ => IsRequestsPopupOpen = !IsRequestsPopupOpen);
            EditProfileCommand = new RelayCommand(_ => _navigation.NavigateTo<EditProfileViewModel>(ActiveUser));

            LoadRequestStatus();
            LoadPendingRequests();
        }

        private void LoadPendingRequests()
        {
            if (IsOwnProfile && IsTrainer)
            {
                var list = _database.GetPendingRequests(ActiveUser.Id);
                PendingRequests = new ObservableCollection<Trainee>(list);
            }
        }

        private void LoadRequestStatus()
        {
            if (ActiveUser == null || ViewedUser == null) return;
            
            if (!ActiveUser.IsTrainer && ViewedUser.IsTrainer)
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

        private string GetColorForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return "#FF00BCD4";
            int hash = username.GetHashCode();
            var colors = new[] { "#FF00BCD4", "#FF9C27B0", "#FF4CAF50", "#FFFF9800", "#FF3F51B5", "#FFE91E63" };
            return colors[Math.Abs(hash) % colors.Length];
        }
    }
}
