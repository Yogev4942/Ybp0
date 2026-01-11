using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>(object parameters = null) where TViewModel : BaseViewModel;
        void NavigateTo(Type viewModelType, object parameter = null); // New Type-based method
        void NavigateToProfile(int userId);
        void GoBack();
        void OnLoginSuccess();   // keep this so LoginViewModel can trigger the behavior you liked
    }
}

