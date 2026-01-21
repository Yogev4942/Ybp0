using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainee registration.
    /// Contains trainee-specific fields: FitnessGoal, CurrentWeight, Height
    /// </summary>
    public class TraineeRegisterViewModel : BaseRegisterViewModel
    {
        // Trainee-specific fields
        private string _fitnessGoal;
        private string _currentWeight;
        private string _height;

        public string FitnessGoal
        {
            get => _fitnessGoal;
            set => SetProperty(ref _fitnessGoal, value);
        }

        public string CurrentWeight
        {
            get => _currentWeight;
            set => SetProperty(ref _currentWeight, value);
        }

        public string Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public TraineeRegisterViewModel(IDatabaseService databaseService, INavigationService navigationService)
            : base(databaseService, navigationService)
        {
            RegisterCommand = new RelayCommand(_ => Register());
        }

        public bool Register()
        {
            if (!ValidateBasicFields())
                return false;

            // Validate trainee-specific fields
            if (string.IsNullOrWhiteSpace(FitnessGoal))
            {
                ErrorMsg = "Please enter your fitness goal";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentWeight) ||
                !double.TryParse(CurrentWeight, out double weight) || weight <= 0)
            {
                ErrorMsg = "Please enter a valid weight";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Height) ||
                !double.TryParse(Height, out double height) || height <= 0)
            {
                ErrorMsg = "Please enter a valid height";
                return false;
            }

            try
            {
                // Register as trainee
                bool success = _databaseService.RegisterTrainee(
                    Username,
                    Email,
                    Password,
                    FitnessGoal,
                    weight,
                    height
                );

                if (success)
                {
                    NavigateToLogin();
                    return true;
                }
                else
                {
                    ErrorMsg = "Error creating user";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Register error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Register error: {ex}");
                return false;
            }
        }
    }
}
