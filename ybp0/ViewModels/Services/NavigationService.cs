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

        // Cache for singleton ViewModels (page-level views)
        private readonly Dictionary<Type, BaseViewModel> _viewModelCache = new Dictionary<Type, BaseViewModel>();

        // Types that should be cached (singleton pattern)
        private static readonly HashSet<Type> _cachedTypes = new HashSet<Type>
        {
            typeof(HomeViewModel),
            typeof(CalendarViewModel),
            typeof(FeedViewModel)
        };

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

        public void NavigateToProfile(int userId)
        {
            // We need a way to fetch the user. 
            // Since navigation service doesn't have DB access directly (usually),
            // and MainViewModel's factory handles ViewModel creation which DOES have DB access,
            // we can just pass the userId as the parameter.
            NavigateTo(typeof(ProfileViewModel), userId);
        }

        public void NavigateTo(Type viewModelType, object parameters = null)
        {
            // validate type
            if (!typeof(BaseViewModel).IsAssignableFrom(viewModelType))
                throw new ArgumentException("Type must inherit from BaseViewModel", nameof(viewModelType));

            BaseViewModel current = _stack.Count > 0 ? _stack.Peek() : null;
            current?.OnNavigatedFrom();

            BaseViewModel vm;

            // Check if this type should be cached (singleton)
            if (_cachedTypes.Contains(viewModelType))
            {
                // Try to get from cache first
                if (!_viewModelCache.TryGetValue(viewModelType, out vm))
                {
                    // Create and cache if not found
                    vm = _factory(viewModelType, parameters);
                    _viewModelCache[viewModelType] = vm;
                }
            }
            else
            {
                // Non-cached types: always create new instance
                vm = _factory(viewModelType, parameters);
            }

            // push and make current
            _stack.Push(vm);
            _setCurrentViewModel(vm);
            vm.OnNavigatedTo();
        }

        public void GoBack()
        {
            if (_stack.Count <= 1) return; // nothing to go back to

            // remove current
            var current = _stack.Pop();
            current.OnNavigatedFrom();

            // restore previous
            var previous = _stack.Peek();
            _setCurrentViewModel(previous);
            previous.OnNavigatedTo();
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

        public void Logout()
        {
            if (_stack.Count > 0)
            {
                _stack.Peek().OnNavigatedFrom();
            }

            _stack.Clear();
            _viewModelCache.Clear();
            NavigateTo<LoginViewModel>();
        }
    }
}

