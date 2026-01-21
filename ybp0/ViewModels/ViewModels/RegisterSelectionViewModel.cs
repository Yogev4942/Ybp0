using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for the main registration selection screen.
    /// Allows the user to choose between registering as a Trainee or Trainer.
    /// </summary>
    public class RegisterSelectionViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public ICommand GoBackCommand { get; }
        public ICommand SelectTraineeCommand { get; }
        public ICommand SelectTrainerCommand { get; }

        public RegisterSelectionViewModel(IDatabaseService databaseService, INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
            
            SelectTraineeCommand = new RelayCommand(_ => 
                _navigationService.NavigateTo<TraineeRegisterViewModel>());
            
            SelectTrainerCommand = new RelayCommand(_ => 
                _navigationService.NavigateTo<TrainerRegisterViewModel>());
        }
    }
}
