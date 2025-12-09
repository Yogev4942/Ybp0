using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        // Example properties for the Home page
        private string _welcomeText = "Welcome to Gym Manager!";
        public string WelcomeText
        {
            get => _welcomeText;
            set => SetProperty(ref _welcomeText, value);
        }
    }
}
