using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainer registration.
    /// Contains trainer-specific fields: Specialization, HourlyRate, MaxTrainees
    /// </summary>
    public class TrainerRegisterViewModel : BaseRegisterViewModel
    {
        // Trainer-specific fields
        private string _specialization;
        private string _hourlyRate;
        private string _maxTrainees;

        public string Specialization
        {
            get => _specialization;
            set => SetProperty(ref _specialization, value);
        }

        public string HourlyRate
        {
            get => _hourlyRate;
            set => SetProperty(ref _hourlyRate, value);
        }

        public string MaxTrainees
        {
            get => _maxTrainees;
            set => SetProperty(ref _maxTrainees, value);
        }

        public TrainerRegisterViewModel(IDatabaseService databaseService, INavigationService navigationService)
            : base(databaseService, navigationService)
        {
            RegisterCommand = new RelayCommand(_ => Register());
        }

        public bool Register()
        {
            if (!ValidateBasicFields())
                return false;

            // Validate trainer-specific fields
            if (string.IsNullOrWhiteSpace(Specialization))
            {
                ErrorMsg = "Please enter your specialization";
                return false;
            }

            double hourlyRate = 0;
            if (!string.IsNullOrWhiteSpace(HourlyRate))
            {
                if (!double.TryParse(HourlyRate, out hourlyRate) || hourlyRate < 0)
                {
                    ErrorMsg = "Please enter a valid hourly rate";
                    return false;
                }
            }

            int maxTrainees = 10; // Default
            if (!string.IsNullOrWhiteSpace(MaxTrainees))
            {
                if (!int.TryParse(MaxTrainees, out maxTrainees) || maxTrainees <= 0)
                {
                    ErrorMsg = "Please enter a valid maximum trainees number";
                    return false;
                }
            }

            try
            {
                // Register as trainer
                bool success = _databaseService.RegisterTrainer(
                    Username,
                    Email,
                    Password,
                    Specialization,
                    hourlyRate,
                    maxTrainees
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
