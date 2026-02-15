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
        private readonly INavigationService _navigation;
        private readonly IDatabaseService _databaseService;
        private readonly int _currentUserId;

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

        private int _likeCount;
        public int LikeCount
        {
            get => _likeCount;
            set => SetProperty(ref _likeCount, value);
        }

        private bool _isLiked;
        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                if (SetProperty(ref _isLiked, value))
                    OnPropertyChanged(nameof(LikeButtonText));
            }
        }

        public string LikeButtonText => IsLiked ? "❤️" : "🤍";

        public int PostId { get; }
        public int OwnerId { get; }
        public bool CanDelete { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ToggleLikeCommand { get; }

        public PostViewModel(INavigationService navigation, IDatabaseService databaseService, int currentUserId,
            int postId, int ownerId, string username, string content, string timestamp, string avatarColor, 
            bool canDelete, int likeCount, bool isLiked, Action<PostViewModel> deleteAction = null)
        {
            _navigation = navigation;
            _databaseService = databaseService;
            _currentUserId = currentUserId;
            PostId = postId;
            OwnerId = ownerId;
            Username = username;
            Content = content;
            Timestamp = timestamp;
            AvatarColor = avatarColor;
            CanDelete = canDelete;
            LikeCount = likeCount;
            IsLiked = isLiked;
            Initials = !string.IsNullOrEmpty(username) && username.Length >= 2 
                ? username.Substring(0, 2).ToUpper() 
                : username?.FirstOrDefault().ToString().ToUpper();

            NavigateToProfileCommand = new RelayCommand(_ => _navigation.NavigateToProfile(OwnerId));
            ToggleLikeCommand = new RelayCommand(_ => ToggleLike());
            
            if (deleteAction != null)
            {
                DeleteCommand = new RelayCommand(_ => deleteAction(this));
            }
        }

        /// <summary>
        /// Constructor used by FeedViewModel.LoadPosts() — builds from domain models.
        /// </summary>
        public PostViewModel(Post post, User owner, User currUser, INavigationService navigationService, 
            IDatabaseService databaseService, Action<PostViewModel> deleteAction = null)
            : this(navigationService,
                   databaseService,
                   currUser.Id,
                   post.Id,
                   owner.Id,
                   owner.Username,
                   post.Content,
                   post.PostTime.ToString("MMM dd, yyyy"),
                   GetColorForUser(owner.Username),
                   currUser.Id == owner.Id,
                   post.LikeCount,
                   databaseService.IsPostLikedByUser(post.Id, currUser.Id),
                   deleteAction)
        {
        }

        private void ToggleLike()
        {
            bool nowLiked = _databaseService.ToggleLike(PostId, _currentUserId);
            IsLiked = nowLiked;
            LikeCount = _databaseService.GetLikeCount(PostId);
        }

        private static string GetColorForUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return "#FF00BCD4";
            int hash = username.GetHashCode();
            var colors = new[] { "#FF00BCD4", "#FF9C27B0", "#FF4CAF50", "#FFFF9800", "#FF3F51B5", "#FFE91E63" };
            return colors[Math.Abs(hash) % colors.Length];
        }
    }
}
