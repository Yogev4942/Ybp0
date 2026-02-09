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
        string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }
        private string statusMessage;
        string StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }
        private string header;
        string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }
        public CreatePostViewModel(IDatabaseService dbService, INavigationService navService,User currUser)
        {
            _dbService = dbService;
            _navService = navService;
            _currUser = currUser;
        }


        public ICommand CreatePostCommand { get; }

        public void CreatePost()
        {
            try
            {
                ValidatePost();
                _dbService.CreatePost(Header, Content, _currUser);
                StatusMessage = "Post created successfully";
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                // unexpected system error
                StatusMessage = "Unexpected error while creating post";
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
