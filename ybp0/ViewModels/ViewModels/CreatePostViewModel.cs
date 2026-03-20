using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class CreatePostViewModel : BaseViewModel
    {
        protected readonly IDatabaseService _dbService;
        protected readonly INavigationService _navService;
        protected readonly User _currUser;
        private string content;
        public string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }
        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }
        private string header;
        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }
        public ICommand CreatePostCommand { get; }
        public ICommand CancelCommand { get; }

        public CreatePostViewModel(IDatabaseService dbService, INavigationService navService, User currUser)
        {
            _dbService = dbService;
            _navService = navService;
            _currUser = currUser;
            CreatePostCommand = new RelayCommand(_ => CreatePost());
            CancelCommand = new RelayCommand(_ => _navService.GoBack());
        }

        public void CreatePost()
        {
            try
            {
                ValidatePost();
                bool success = _dbService.CreatePost(Header, Content, _currUser);
                if (success)
                {
                    StatusMessage = "Post created successfully!";
                    // Navigate back to feed so the user sees the new post
                    _navService.GoBack();
                    //going back
                }
                else
                {
                    StatusMessage = "Failed to save post to database.";
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                // unexpected system error
                StatusMessage = "Unexpected error while creating post: " + ex.Message;
            }
        }
        private void ValidatePost()
        {
            if (string.IsNullOrWhiteSpace(Header))
                throw new ArgumentException("Fill Header Field");

            if (string.IsNullOrWhiteSpace(Content))
                throw new ArgumentException("Fill Content Field");
        }


    }
}
