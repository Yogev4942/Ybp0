using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class PostViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _content;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private string _timestamp;
        public string Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        private string _initials;
        public string Initials
        {
            get => _initials;
            set => SetProperty(ref _initials, value);
        }

        private string _avatarColor;
        public string AvatarColor
        {
            get => _avatarColor;
            set => SetProperty(ref _avatarColor, value);
        }

        public int OwnerId { get; }
        public ICommand NavigateToProfileCommand { get; }

        public PostViewModel(INavigationService navigation, int ownerId, string username, string content, string timestamp, string avatarColor)
        {
            _navigation = navigation;
            OwnerId = ownerId;
            Username = username;
            Content = content;
            Timestamp = timestamp;
            AvatarColor = avatarColor;
            Initials = !string.IsNullOrEmpty(username) && username.Length >= 2 
                ? username.Substring(0, 2).ToUpper() 
                : username?.FirstOrDefault().ToString().ToUpper();

            NavigateToProfileCommand = new RelayCommand(_ => _navigation.NavigateToProfile(OwnerId));
        }

        private readonly INavigationService _navigation;
    }
}
