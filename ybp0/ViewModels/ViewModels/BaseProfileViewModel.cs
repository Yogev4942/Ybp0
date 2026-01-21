using Models;
using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Base ViewModel for profile views, containing shared properties and logic.
    /// </summary>
    public abstract class BaseProfileViewModel : BaseViewModel
    {
        protected readonly IDatabaseService _database;
        protected readonly INavigationService _navigation;
        
        public User ActiveUser { get; }
        public User ViewedUser { get; }

        // Common properties for ViewedUser (binding convenience)
        public string Username => ViewedUser?.Username;
        public string Email => ViewedUser?.Email;
        public string Bio => ViewedUser?.Bio;
        public string Gender => ViewedUser?.Gender;
        public string JoinDateText => $"Joined {ViewedUser?.Joindate}";
        
        // User type labels - subclasses can override if needed
        public virtual string UserTypeLabel => ViewedUser is Trainer ? "Trainer" : "Trainee";
        
        // Avatar Helpers
        public string Initials => !string.IsNullOrEmpty(Username) && Username.Length >= 2 
            ? Username.Substring(0, 2).ToUpper() 
            : Username?[0].ToString().ToUpper();
        
        public string AvatarColor => GetColorForUser(Username);

        // Profile ownership
        public bool IsOwnProfile => ActiveUser?.Id == ViewedUser?.Id;

        // Common commands
        public ICommand EditProfileCommand { get; }

        protected BaseProfileViewModel(IDatabaseService database, INavigationService navigation, User activeUser, User viewedUser)
        {
            _database = database;
            _navigation = navigation;
            ActiveUser = activeUser;
            ViewedUser = viewedUser;

            EditProfileCommand = new RelayCommand(_ => _navigation.NavigateTo<EditProfileViewModel>(ActiveUser));
        }

        protected string GetColorForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return "#FF00BCD4";
            int hash = username.GetHashCode();
            var colors = new[] { "#FF00BCD4", "#FF9C27B0", "#FF4CAF50", "#FFFF9800", "#FF3F51B5", "#FFE91E63" };
            return colors[Math.Abs(hash) % colors.Length];
        }
    }
}
