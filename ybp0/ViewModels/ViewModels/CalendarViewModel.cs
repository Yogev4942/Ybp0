using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ViewModels.ViewModels
{
    public abstract class CalendarViewModel : BaseViewModel
    {
        protected readonly IDatabaseService _dbService;
        protected readonly INavigationService _navService;
        protected readonly User _currUser;

        private int _userId;
        private int _weekPlanId;
        private ObservableCollection<DayViewModel> _days;
        private bool _isExerciseModalOpen;
        private DayViewModel _selectedDayForExercise;
        private ObservableCollection<Exercise> _allExercises;
        private List<Exercise> _exerciseCatalog;
        private Exercise _selectedExercise;
        private string _displayWeekPlanId;
        private string _weekPlanOwnerLabel;

        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public int WeekPlanId
        {
            get => _weekPlanId;
            set => SetProperty(ref _weekPlanId, value);
        }

        public ObservableCollection<DayViewModel> Days
        {
            get => _days;
            set => SetProperty(ref _days, value);
        }

        public bool IsExerciseModalOpen
        {
            get => _isExerciseModalOpen;
            set => SetProperty(ref _isExerciseModalOpen, value);
        }

        public DayViewModel SelectedDayForExercise
        {
            get => _selectedDayForExercise;
            set => SetProperty(ref _selectedDayForExercise, value);
        }

        public ObservableCollection<Exercise> AllExercises
        {
            get => _allExercises;
            set => SetProperty(ref _allExercises, value);
        }

        public Exercise SelectedExercise
        {
            get => _selectedExercise;
            set
            {
                if (SetProperty(ref _selectedExercise, value) && value != null)
                {
                    AddSelectedExerciseToDay();
                }
            }
        }

        public string DisplayWeekPlanId
        {
            get => _displayWeekPlanId;
            set
            {
                if (SetProperty(ref _displayWeekPlanId, value) &&
                    int.TryParse(value, out int newWeekPlanId) &&
                    newWeekPlanId != _weekPlanId)
                {
                    TryChangeWeekPlan(newWeekPlanId);
                }
            }
        }

        public string WeekPlanOwnerLabel
        {
            get => _weekPlanOwnerLabel;
            set => SetProperty(ref _weekPlanOwnerLabel, value);
        }

        public ICommand OpenExerciseModalCommand { get; }
        public ICommand CloseExerciseModalCommand { get; }

        protected CalendarViewModel(IDatabaseService dbService, INavigationService navigationService, User user)
        {
            _dbService = dbService;
            _navService = navigationService;
            _currUser = user;

            Days = new ObservableCollection<DayViewModel>();
            AllExercises = new ObservableCollection<Exercise>();
            OpenExerciseModalCommand = new RelayCommand(param => OpenExerciseModal(param as DayViewModel));
            CloseExerciseModalCommand = new RelayCommand(_ => CloseExerciseModal());

            int? weekPlanId = _currUser.CurrentWeekPlanId;
            if (!weekPlanId.HasValue)
            {
                weekPlanId = _dbService.GetUserWeekPlanId(_currUser.Id);
            }

            if (!weekPlanId.HasValue)
            {
                weekPlanId = _dbService.CreateEmptyWeekPlan(_currUser.Id, "My Week Plan");
            }

            LoadWeekPlan(_currUser.Id, weekPlanId.Value);
            DisplayWeekPlanId = weekPlanId.Value.ToString();
            WeekPlanOwnerLabel = "Your Plan";
        }

        public void LoadWeekPlan(int userId, int weekPlanId)
        {
            UserId = userId;
            WeekPlanId = weekPlanId;

            Days.Clear();

            DateTime baseDate = DateTime.Today;
            int currentDayOfWeek = (int)baseDate.DayOfWeek;
            List<WeekPlanDay> planDays = _dbService.GetWeekPlanDays(weekPlanId);
            var weekPlanDays = planDays.ToDictionary(day => day.DayOfWeek, StringComparer.OrdinalIgnoreCase);
            Dictionary<int, Workout> workoutsById = _dbService.GetWorkoutsByIds(
                planDays.Where(day => day.WorkoutId.HasValue).Select(day => day.WorkoutId.Value));

            for (int i = 0; i < 7; i++)
            {
                DateTime date = baseDate.AddDays(i - currentDayOfWeek);
                string dayName = date.DayOfWeek.ToString();

                var dayVm = new DayViewModel(_dbService, userId)
                {
                    DayName = dayName.ToUpperInvariant(),
                    Date = date
                };

                if (weekPlanDays.TryGetValue(dayName, out WeekPlanDay planDay))
                {
                    Workout workout = null;
                    if (planDay.WorkoutId.HasValue)
                    {
                        workoutsById.TryGetValue(planDay.WorkoutId.Value, out workout);
                    }

                    dayVm.ApplyWeekPlanDay(planDay, workout);
                }
                else
                {
                    dayVm.IsRestDay = true;
                    dayVm.WorkoutName = "Rest Day";
                }

                Days.Add(dayVm);
            }
        }

        protected virtual void TryChangeWeekPlan(int newWeekPlanId)
        {
            if (!CanUserModifyWeekPlan(newWeekPlanId))
            {
                _displayWeekPlanId = _weekPlanId.ToString();
                OnPropertyChanged(nameof(DisplayWeekPlanId));
                WeekPlanOwnerLabel = "Permission denied";
                return;
            }

            int? ownerUserId = _dbService.GetWeekPlanOwnerUserId(newWeekPlanId);
            if (!ownerUserId.HasValue)
            {
                WeekPlanOwnerLabel = "Invalid WeekPlan ID";
                _displayWeekPlanId = _weekPlanId.ToString();
                OnPropertyChanged(nameof(DisplayWeekPlanId));
                return;
            }

            LoadWeekPlan(ownerUserId.Value, newWeekPlanId);
            WeekPlanOwnerLabel = ownerUserId.Value == _currUser.Id ? "Your Plan" : "Trainee Plan";
        }

        protected virtual bool CanUserModifyWeekPlan(int weekPlanId)
        {
            int? ownerUserId = _dbService.GetWeekPlanOwnerUserId(weekPlanId);
            if (!ownerUserId.HasValue)
            {
                return false;
            }

            if (ownerUserId.Value == _currUser.Id)
            {
                return true;
            }

            if (_currUser.IsTrainer && _currUser.CanModifyOtherUserWorkouts())
            {
                var trainees = _dbService.GetTraineesByTrainerId(_currUser.Id);
                return trainees.Any(t => t.Id == ownerUserId.Value);
            }

            return false;
        }

        protected void OpenExerciseModal(DayViewModel day)
        {
            if (day == null)
            {
                return;
            }

            if (_exerciseCatalog == null)
            {
                _exerciseCatalog = _dbService.GetAllExercises();
            }

            AllExercises = new ObservableCollection<Exercise>(_exerciseCatalog);
            SelectedDayForExercise = day;
            SelectedExercise = null;
            IsExerciseModalOpen = true;
        }

        protected void CloseExerciseModal()
        {
            IsExerciseModalOpen = false;
            SelectedDayForExercise = null;
            SelectedExercise = null;
        }

        protected void AddSelectedExerciseToDay()
        {
            if (SelectedExercise == null || SelectedDayForExercise == null)
            {
                return;
            }

            SelectedDayForExercise.AddExerciseFromModal(SelectedExercise);
            CloseExerciseModal();
        }
    }
}
