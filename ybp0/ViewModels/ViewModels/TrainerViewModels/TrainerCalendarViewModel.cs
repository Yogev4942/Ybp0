using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel for Trainer calendar views.
    /// Adds trainee selection to manage trainees' workout plans.
    /// </summary>
    public class TrainerCalendarViewModel : CalendarViewModel
    {
        private ObservableCollection<Trainee> _trainees;
        private Trainee _selectedTrainee;
        private bool _isViewingTraineePlan;

        /// <summary>
        /// List of trainees assigned to this trainer
        /// </summary>
        public ObservableCollection<Trainee> Trainees
        {
            get => _trainees;
            set => SetProperty(ref _trainees, value);
        }

        /// <summary>
        /// Currently selected trainee (null = viewing own plan)
        /// </summary>
        public Trainee SelectedTrainee
        {
            get => _selectedTrainee;
            set
            {
                if (SetProperty(ref _selectedTrainee, value))
                {
                    OnTraineeSelected(value);
                }
            }
        }

        /// <summary>
        /// Whether we are currently viewing a trainee's plan
        /// </summary>
        public bool IsViewingTraineePlan
        {
            get => _isViewingTraineePlan;
            set => SetProperty(ref _isViewingTraineePlan, value);
        }

        /// <summary>
        /// Command to switch back to the trainer's own plan
        /// </summary>
        public ICommand ViewMyPlanCommand { get; }

        public TrainerCalendarViewModel(IDatabaseService dbService, INavigationService navigationService, User user)
            : base(dbService, navigationService, user)
        {
            ViewMyPlanCommand = new RelayCommand(_ => ViewMyPlan());

            // Load the trainer's trainees
            LoadTrainees();
        }

        private void LoadTrainees()
        {
            var traineeList = _dbService.GetTraineesByTrainerId(_currUser.Id);
            Trainees = new ObservableCollection<Trainee>(traineeList);
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();
            LoadTrainees();
            
            // Re-select if someone was selected, or fallback to my plan if they were removed
            if (_selectedTrainee != null)
            {
                var existing = Trainees.FirstOrDefault(t => t.Id == _selectedTrainee.Id);
                if (existing != null)
                {
                    _selectedTrainee = existing;
                    OnPropertyChanged(nameof(SelectedTrainee));
                }
                else
                {
                    ViewMyPlan();
                }
            }
        }

        private void OnTraineeSelected(Trainee trainee)
        {
            if (trainee == null)
            {
                ViewMyPlan();
                return;
            }

            // Load the selected trainee's week plan
            int? traineeWeekPlanId = trainee.CurrentWeekPlanId.HasValue && trainee.CurrentWeekPlanId.Value > 0
                ? trainee.CurrentWeekPlanId
                : _dbService.GetUserWeekPlanId(trainee.Id);

            if (traineeWeekPlanId.HasValue && traineeWeekPlanId.Value > 0)
            {
                LoadWeekPlan(trainee.Id, traineeWeekPlanId.Value);
                DisplayWeekPlanId = traineeWeekPlanId.Value.ToString();
                WeekPlanOwnerLabel = $"{trainee.Username}'s Plan";
                IsViewingTraineePlan = true;
            }
            else
            {
                // Trainee has no week plan yet — create one
                int newPlanId = _dbService.CreateEmptyWeekPlan(trainee.Id, "My Week Plan");
                LoadWeekPlan(trainee.Id, newPlanId);
                DisplayWeekPlanId = newPlanId.ToString();
                WeekPlanOwnerLabel = $"{trainee.Username}'s Plan (New)";
                IsViewingTraineePlan = true;
            }
        }

        private void ViewMyPlan()
        {
            _selectedTrainee = null;
            OnPropertyChanged(nameof(SelectedTrainee));

            int? myWeekPlanId = _currUser.CurrentWeekPlanId.HasValue && _currUser.CurrentWeekPlanId.Value > 0
                ? _currUser.CurrentWeekPlanId
                : _dbService.GetUserWeekPlanId(_currUser.Id);

            if (myWeekPlanId.HasValue && myWeekPlanId.Value > 0)
            {
                LoadWeekPlan(_currUser.Id, myWeekPlanId.Value);
                DisplayWeekPlanId = myWeekPlanId.Value.ToString();
            }

            WeekPlanOwnerLabel = "Your Plan";
            IsViewingTraineePlan = false;
        }
    }
}
