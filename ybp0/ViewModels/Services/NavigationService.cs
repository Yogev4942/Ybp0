using System;
using ViewModels.ViewModels;
using ViewModels;
using System.Collections.Generic;

public class NavigationService : INavigationService
{
    private readonly Func<Type, object[], BaseViewModel> _factory;
    private readonly Action<BaseViewModel> _setCurrentViewModel;

    private readonly Stack<BaseViewModel> _navigationStack;
    private readonly Action _onLogin;


    public NavigationService(
        Func<Type, object[], BaseViewModel> factory,
        Action<BaseViewModel> setCurrentViewModel)
    {
        _factory = factory;
        _setCurrentViewModel = setCurrentViewModel;
        _navigationStack = new Stack<BaseViewModel>();
    }

    public void NavigateTo<TViewModel>(params object[] parameters)
        where TViewModel : BaseViewModel
    {
        var vm = _factory(typeof(TViewModel), parameters);
        _navigationStack.Push(vm);
        _setCurrentViewModel(vm);
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 1)
        {
            _navigationStack.Pop(); // remove current
            var previous = _navigationStack.Peek(); // restore previous
            _setCurrentViewModel(previous);
        }
    }
    public void OnLoginSuccess()
    {
        _onLogin?.Invoke();
        NavigateTo<HomeViewModel>();
    }
}