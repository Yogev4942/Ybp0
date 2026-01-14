using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly INavigationService _navService;
        private readonly User _currUser;
        private int _userId;
        private int _weekPlanId;
        private ObservableCollection<DayViewModel> _days;

        // Modal Properties
        private bool _isExerciseModalOpen;
        private DayViewModel _selectedDayForExercise;
        private ObservableCollection<Exercise> _allExercises;
        private Exercise _selectedExercise;

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

        // Modal Public Properties
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

        // Commands
        public ICommand OpenExerciseModalCommand { get; }
        public ICommand CloseExerciseModalCommand { get; }
        // Display WeekPlanId as string for UI binding
        private string _displayWeekPlanId;
        public string DisplayWeekPlanId
        {
            get => _displayWeekPlanId;
            set
            {
                if (SetProperty(ref _displayWeekPlanId, value))
                {
                    // Attempt to validate and change weekplan when value changes
                    if (int.TryParse(value, out int newWeekPlanId) && newWeekPlanId != _weekPlanId)
                    {
                        TryChangeWeekPlan(newWeekPlanId);
                    }
                }
            }
        }

        // Label showing current WeekPlan owner
        private string _weekPlanOwnerLabel;
        public string WeekPlanOwnerLabel
        {
            get => _weekPlanOwnerLabel;
            set => SetProperty(ref _weekPlanOwnerLabel, value);
        }

        public CalendarViewModel(IDatabaseService dbService,INavigationService navigationService,User user)
        {
            _dbService = dbService;
            _navService = navigationService;
            _currUser = user;
            Days = new ObservableCollection<DayViewModel>();

            // Initialize Modal Commands
            OpenExerciseModalCommand = new RelayCommand(param => OpenExerciseModal(param as DayViewModel));
            CloseExerciseModalCommand = new RelayCommand(_ => CloseExerciseModal());

            int? weekPlanId = null;

            // 1. Priority: Assigned Plan (for Trainee)
            if (_currUser is Trainee trainee && trainee.CurrentWeekPlanId > 0)
            {
                weekPlanId = trainee.CurrentWeekPlanId;
            }

            // 2. Fallback: Any existing plan owned by user
            if (!weekPlanId.HasValue)
            {
                weekPlanId = _dbService.GetUserWeekPlanId(_currUser.Id);
            }

            // Load the weekplan
            LoadWeekPlan(_currUser.Id, weekPlanId.Value);
            DisplayWeekPlanId = weekPlanId.Value.ToString();
            WeekPlanOwnerLabel = "Your Plan";
        }



        public void LoadWeekPlan(int userId, int weekPlanId)
        {
            UserId = userId;
            WeekPlanId = weekPlanId;

            Days.Clear();

            // Day names in order (Sunday first)
            string[] dayNames = { "SUNDAY", "MONDAY", "TUESDAY", "WEDNSDAY", "THURSDAY", "FRIDAY", "SATURDAY" };

            // Load week plan days from database
            var weekPlanDays = GetWeekPlanDays(weekPlanId);

            DateTime baseDate = DateTime.Today;
            int currentDayOfWeek = (int)baseDate.DayOfWeek; // 0 = Sunday

            for (int i = 0; i < 7; i++)
            {
                var dayVm = new DayViewModel(_dbService, userId)
                {
                    DayName = dayNames[i],
                    Date = baseDate.AddDays(i - currentDayOfWeek) // Calculate date for this day
                };

                // Find matching week plan day
                var planDay = weekPlanDays.FirstOrDefault(d => d.DayOfWeek == i);
                
                if (planDay != null)
                {
                    dayVm.WeekPlanDayId = planDay.Id;
                    dayVm.IsRestDay = planDay.IsRestDay; // Use actual value from database
                    dayVm.WorkoutName = planDay.WorkoutName;

                    // Always try to load exercises (works for both templates and ad-hoc)
                    dayVm.LoadWorkoutForDay();
                }
                else
                {
                    dayVm.IsRestDay = true;
                    dayVm.WorkoutName = "No Workout";
                }

                Days.Add(dayVm);
            }
        }

        private List<WeekPlanDayData> GetWeekPlanDays(int weekPlanId)
        {
            var dt = _dbService.GetWeekPlanDays(weekPlanId);
            
            var days = new List<WeekPlanDayData>();
            if (dt == null) return days;

            foreach (System.Data.DataRow row in dt.Rows)
            {
                days.Add(new WeekPlanDayData
                {
                    Id = Convert.ToInt32(row["Id"]),
                    DayOfWeek = Convert.ToInt32(row["DayOfWeek"]),
                    WorkoutId = row["WorkoutId"] != DBNull.Value ? (int?)Convert.ToInt32(row["WorkoutId"]) : null,
                    IsRestDay = Convert.ToBoolean(row["RestDay"]),
                    WorkoutName = row["WorkoutName"]?.ToString() ?? "Rest Day"
                });
            }

            return days;
        }

        /// <summary>
        /// Validates and changes the weekplan if user has permission
        /// </summary>
        private void TryChangeWeekPlan(int newWeekPlanId)
        {
            if (!CanUserModifyWeekPlan(newWeekPlanId))
            {
                // Reset to current value if not allowed
                _displayWeekPlanId = _weekPlanId.ToString();
                OnPropertyChanged(nameof(DisplayWeekPlanId));
                WeekPlanOwnerLabel = "⚠ Permission denied";
                return;
            }

            // Find the owner user Id for this weekplan
            var ownerUserId = _dbService.GetWeekPlanOwnerUserId(newWeekPlanId);
            if (!ownerUserId.HasValue)
            {
                WeekPlanOwnerLabel = "⚠ Invalid WeekPlan ID";
                _displayWeekPlanId = _weekPlanId.ToString();
                OnPropertyChanged(nameof(DisplayWeekPlanId));
                return;
            }

            // Load the new weekplan
            LoadWeekPlan(ownerUserId.Value, newWeekPlanId);

            // Update owner label
            if (ownerUserId.Value == _currUser.Id)
            {
                WeekPlanOwnerLabel = "Your Plan";
            }
            else
            {
                WeekPlanOwnerLabel = "Trainee's Plan";
            }
        }

        /// <summary>
        /// Checks if the current user can modify the specified weekplan
        /// </summary>
        private bool CanUserModifyWeekPlan(int weekPlanId)
        {
            // Get owner of this weekplan
            var ownerUserId = _dbService.GetWeekPlanOwnerUserId(weekPlanId);
            if (!ownerUserId.HasValue)
            {
                return false; // Invalid weekplan
            }

            // User can modify their own weekplan
            if (ownerUserId.Value == _currUser.Id)
            {
                return true;
            }

            // Trainers can modify their trainees' weekplans
            if (_currUser.IsTrainer && _currUser.CanModifyOtherUserWorkouts())
            {
                var trainees = _dbService.GetTraineesByTrainerId(_currUser.Id);
                return trainees.Any(t => t.Id == ownerUserId.Value);
            }

            return false;
        }

        private void OpenExerciseModal(DayViewModel day)
        {
            if (day == null) return;
            
            // Load exercises every time we open to ensure we have fresh data
            AllExercises = new ObservableCollection<Exercise>(_dbService.GetAllExercises());
            
            SelectedDayForExercise = day;
            SelectedExercise = null; // Reset selection
            IsExerciseModalOpen = true;
        }

        private void CloseExerciseModal()
        {
            IsExerciseModalOpen = false;
            SelectedDayForExercise = null;
        }

        private void AddSelectedExerciseToDay()
        {
            if (SelectedExercise == null || SelectedDayForExercise == null) return;

            // Add the exercise to the selected day
            SelectedDayForExercise.AddExerciseFromModal(SelectedExercise);
            
            // Close the modal
            CloseExerciseModal();
        }

        private class WeekPlanDayData
        {
            public int Id { get; set; }
            public int DayOfWeek { get; set; }
            public int? WorkoutId { get; set; }
            public bool IsRestDay { get; set; }
            public string WorkoutName { get; set; }
        }
    }
}

