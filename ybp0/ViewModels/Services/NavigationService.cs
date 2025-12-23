using System;
using ViewModels.ViewModels;
using ViewModels;
using System.Collections.Generic;

namespace ViewModels
{
    public class NavigationService : INavigationService
    {
        private readonly Func<Type, object, BaseViewModel> _factory;
        private readonly Action<BaseViewModel> _setCurrentViewModel;
        private readonly Stack<BaseViewModel> _stack;
        private readonly Action _onLogin; // may be null

        public NavigationService(
            Func<Type, object, BaseViewModel> factory,
            Action<BaseViewModel> setCurrentViewModel,
            Action onLogin = null)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (setCurrentViewModel == null) throw new ArgumentNullException(nameof(setCurrentViewModel));

            _factory = factory;
            _setCurrentViewModel = setCurrentViewModel;
            _stack = new Stack<BaseViewModel>();
            _onLogin = onLogin;
        }

        public void NavigateTo<TViewModel>(object parameters = null) where TViewModel : BaseViewModel
        {
            NavigateTo(typeof(TViewModel), parameters);
        }

        public void NavigateTo(Type viewModelType, object parameters = null)
        {
            // validate type
            if (!typeof(BaseViewModel).IsAssignableFrom(viewModelType))
                throw new ArgumentException("Type must inherit from BaseViewModel", nameof(viewModelType));

            // create new viewmodel instance (factory handles DI)
            var vm = _factory(viewModelType, parameters);

            // push and make current
            _stack.Push(vm);
            _setCurrentViewModel(vm);
        }

        public void GoBack()
        {
            if (_stack.Count <= 1) return; // nothing to go back to

            // remove current
            _stack.Pop();

            // restore previous
            var previous = _stack.Peek();
            _setCurrentViewModel(previous);
        }

        public void OnLoginSuccess()
        {
            // call optional on-login callback (e.g. set IsLoggedIn = true in MainViewModel)
            if (_onLogin != null)
            {
                _onLogin();
            }

            // navigate to home (you can change to pass a parameter if needed)
        }
    }
}

