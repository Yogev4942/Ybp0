using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for the Feed view - stub for future implementation.
    /// </summary>
    public class FeedViewModel : BaseViewModel
    {
        private string _welcomeMessage;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public FeedViewModel()
        {
            WelcomeMessage = "Feed coming soon!";
        }
    }
}
