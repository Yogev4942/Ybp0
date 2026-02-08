using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels
{
    public class CreatePostViewModel : BaseViewModel
    {
        protected readonly IDatabaseService _dbService;
        protected readonly INavigationService _navService;
        private string content;
        string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        private string header;
        string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }
        CreatePostViewModel(IDatabaseService dbService, INavigationService navService)
        {
            _dbService = dbService;
            _navService = navService;
        }
    }
}
