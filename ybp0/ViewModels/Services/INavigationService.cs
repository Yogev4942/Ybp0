using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>(params object[] parameters) where TViewModel : BaseViewModel;
        void GoBack();
        void OnLoginSuccess();

    }
}
