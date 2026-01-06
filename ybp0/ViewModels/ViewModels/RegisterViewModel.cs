using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly INavigationService _navigationService;

        // Basic user fields
        private string _username;
        private string _email;
        private string _password;
        private string _errorMessage;

        // User type selection
        private string _selectedUserType = "Trainee";
        private bool _isTraineeSelected = true;
        private bool _isTrainerSelected = false;

        // Trainee-specific fields
        private string _fitnessGoal;
        private string _currentWeight;
        private string _height;

        // Trainer-specific fields
        private string _specialization;
        private string _hourlyRate;
        private string _maxTrainees;

        // Basic properties
        public string Username { get => _username; set => SetProperty(ref _username, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ErrorMsg { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        // User type selection properties
        public string SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                SetProperty(ref _selectedUserType, value);
                UpdateFieldVisibility();
            }
        }

        public bool IsTraineeSelected
        {
            get => _isTraineeSelected;
            set => SetProperty(ref _isTraineeSelected, value);
        }

        public bool IsTrainerSelected
        {
            get => _isTrainerSelected;
            set => SetProperty(ref _isTrainerSelected, value);
        }

        // Trainee-specific properties
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

        // Trainer-specific properties
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

        public RegisterViewModel(IDatabaseService databaseService, INavigationService navigationService)
        {
            this._databaseService = databaseService;
            this._navigationService = navigationService;
            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
            RegisterCommand = new RelayCommand(_ => Register());
        }

        public ICommand RegisterCommand { get; private set; }
        public ICommand GoBackCommand { get; private set; }

        private void UpdateFieldVisibility()
        {
            if (_selectedUserType == "Trainee")
            {
                IsTraineeSelected = true;
                IsTrainerSelected = false;
            }
            else if (_selectedUserType == "Trainer")
            {
                IsTraineeSelected = false;
                IsTrainerSelected = true;
            }
        }

        public bool Register()
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email))
            {
                ErrorMsg = "Fill out all fields";
                return false;
            }

            // Check if user exists
            if (_databaseService.UserExist(Username, Email))
            {
                ErrorMsg = "Username or email already exists";
                return false;
            }

            try
            {
                ErrorMsg = string.Empty;
                bool success = false;

                if (SelectedUserType == "Trainee")
                {
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

                    // Register as trainee
                    success = _databaseService.RegisterTrainee(
                        Username,
                        Email,
                        Password,
                        FitnessGoal,
                        weight,
                        height
                    );
                }
                else // Trainer
                {
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

                    // Register as trainer
                    success = _databaseService.RegisterTrainer(
                        Username,
                        Email,
                        Password,
                        Specialization,
                        hourlyRate,
                        maxTrainees
                    );
                }

                if (success)
                {
                    GoBackCommand.Execute(null);
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
