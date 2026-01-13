using DataBase;
using Models;
using System;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public class EditProfileViewModel : BaseViewModel
    {
        private readonly IDatabaseService _database;
        private readonly INavigationService _navigation;
        private readonly User _originalUser;

        // Common Properties
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _bio;
        public string Bio
        {
            get => _bio;
            set => SetProperty(ref _bio, value);
        }
        
        // Trainee Properties
        private string _fitnessGoal;
        public string FitnessGoal
        {
            get => _fitnessGoal;
            set => SetProperty(ref _fitnessGoal, value);
        }

        private double _currentWeight;
        public double CurrentWeight
        {
            get => _currentWeight;
            set => SetProperty(ref _currentWeight, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        // Trainer Properties
        private string _specialization;
        public string Specialization
        {
            get => _specialization;
            set => SetProperty(ref _specialization, value);
        }

        private double _hourlyRate;
        public double HourlyRate
        {
            get => _hourlyRate;
            set => SetProperty(ref _hourlyRate, value);
        }

        private int _maxTrainees;
        public int MaxTrainees
        {
            get => _maxTrainees;
            set => SetProperty(ref _maxTrainees, value);
        }

        // Flags
        public bool IsTrainer => _originalUser is Trainer;
        public bool IsTrainee => _originalUser is Trainee;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditProfileViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _database = database;
            _navigation = navigation;
            _originalUser = user;

            LoadUserData();

            SaveCommand = new RelayCommand(_ => SaveChanges());
            CancelCommand = new RelayCommand(_ => _navigation.GoBack());
        }

        private void LoadUserData()
        {
            if (_originalUser == null) return;

            Email = _originalUser.Email;
            Bio = _originalUser.Bio;

            if (IsTrainee && _originalUser is Trainee trainee)
            {
                FitnessGoal = trainee.FitnessGoal;
                CurrentWeight = trainee.CurrentWeight;
                Height = trainee.Height;
            }
            else if (IsTrainer && _originalUser is Trainer trainer)
            {
                Specialization = trainer.Specialization;
                HourlyRate = trainer.HourlyRate;
                MaxTrainees = trainer.MaxTrainees;
            }
        }

        private void SaveChanges()
        {
            // Update the original user object text properties
            // Note: In a real app we might want to validate unique email etc.
            _originalUser.Email = Email;
            _originalUser.Bio = Bio;

            if (IsTrainee && _originalUser is Trainee trainee)
            {
                trainee.FitnessGoal = FitnessGoal;
                trainee.CurrentWeight = CurrentWeight;
                trainee.Height = Height;
            }
            else if (IsTrainer && _originalUser is Trainer trainer)
            {
                trainer.Specialization = Specialization;
                trainer.HourlyRate = HourlyRate;
                trainer.MaxTrainees = MaxTrainees;
            }

            if (_database.UpdateUser(_originalUser))
            {
                // Navigate back on success
                // We might want to refresh the PreviousViewModel but typically it binds to the same User object instance 
                // or reloads it. Since we updated the instance in memory, it might reflect immediately if notified.
                _navigation.GoBack();
            }
            else
            {
                // Handle error (maybe show message)
            }
        }
    }
}
