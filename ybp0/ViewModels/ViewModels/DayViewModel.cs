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
    public class DayViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly int _userId;

        private string _dayName;
        private int _weekPlanDayId;
        private DateTime _date;
        private string _workoutName;
        private bool _isRestDay;
        private int? _workoutSessionId;
        private ObservableCollection<ExerciseViewModel> _exercises;
        private string _accentColor;

        // Exercise picker properties
        private bool _isExercisePickerOpen;
        private ObservableCollection<Exercise> _availableExercises;
        private Exercise _selectedExerciseToAdd;

        public string AccentColor
        {
            get => _accentColor;
            set => SetProperty(ref _accentColor, value);
        }
        public string DayName
        {
            get => _dayName;
            set => SetProperty(ref _dayName, value);
        }
        public int WeekPlanDayId
        {
            get => _weekPlanDayId;
            set => SetProperty(ref _weekPlanDayId, value);
        }
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
        public string WorkoutName
        {
            get => _workoutName;
            set => SetProperty(ref _workoutName, value);
        }
        public bool IsRestDay
        {
            get => _isRestDay;
            set => SetProperty(ref _isRestDay, value);
        }
        public int? WorkoutSessionId
        {
            get => _workoutSessionId;
            set => SetProperty(ref _workoutSessionId, value);
        }
        public ObservableCollection<ExerciseViewModel> Exercises
        {
            get => _exercises;
            set => SetProperty(ref _exercises, value);
        }
        public bool IsExercisePickerOpen
        {
            get => _isExercisePickerOpen;
            set => SetProperty(ref _isExercisePickerOpen, value);
        }
        public ObservableCollection<Exercise> AvailableExercises
        {
            get => _availableExercises;
            set => SetProperty(ref _availableExercises, value);
        }
        public Exercise SelectedExerciseToAdd
        {
            get => _selectedExerciseToAdd;
            set
            {
                if (SetProperty(ref _selectedExerciseToAdd, value) && value != null)
                {
                    AddExercise(value);
                    SelectedExerciseToAdd = null;
                    IsExercisePickerOpen = false;
                }
            }
        }

        // Commands
        public ICommand OpenExercisePickerCommand { get; }
        public ICommand CloseExercisePickerCommand { get; }
        public ICommand RemoveExerciseCommand { get; }

        public DayViewModel(IDatabaseService dbService, int userId)
        {
            _dbService = dbService;
            _userId = userId;
            Exercises = new ObservableCollection<ExerciseViewModel>();
            AvailableExercises = new ObservableCollection<Exercise>();

            OpenExercisePickerCommand = new RelayCommand(_ => OpenExercisePicker());
            CloseExercisePickerCommand = new RelayCommand(_ => CloseExercisePicker());
            RemoveExerciseCommand = new RelayCommand(param => RemoveExercise(param as ExerciseViewModel));
        }

        private void OpenExercisePicker()
        {
            if (_dbService == null || !WorkoutSessionId.HasValue) return;

            // Load available exercises
            var allExercises = _dbService.GetAllExercises();
            
            // Filter out exercises already in this day
            var existingExerciseIds = Exercises.Select(e => e.ExerciseId).ToHashSet();
            var filteredExercises = allExercises.Where(e => !existingExerciseIds.Contains(e.Id)).ToList();

            AvailableExercises.Clear();
            foreach (var ex in filteredExercises)
            {
                AvailableExercises.Add(ex);
            }

            IsExercisePickerOpen = true;
        }

        private void CloseExercisePicker()
        {
            IsExercisePickerOpen = false;
        }

        public void AddExerciseFromModal(Exercise exercise)
        {
            AddExercise(exercise);
        }

        private void AddExercise(Exercise exercise)
        {
            if (_dbService == null) return;

            // Ensure session exists
            if (!WorkoutSessionId.HasValue)
            {
                var session = _dbService.GetOrCreateWorkoutSession(_userId, WeekPlanDayId, Date);
                if (session != null)
                {
                    WorkoutSessionId = session.Id;
                    IsRestDay = false; // It's no longer a rest day if we add an exercise
                    if (WorkoutName == "Rest Day" || WorkoutName == "No Workout")
                    {
                        WorkoutName = "Custom Workout";
                    }
                }
                else
                {
                    return; // Failed to create session
                }
            }

            // Add to database
            _dbService.AddExerciseToWorkoutSession(WorkoutSessionId.Value, exercise.Id);

            // Create ViewModel and add to collection
            string[] colors = { "#26A69A", "#66BB6A", "#42A5F5", "#AB47BC", "#FF9800", "#7E57C2" };
            string color = colors[Exercises.Count % colors.Length];

            var exerciseVm = new ExerciseViewModel(_dbService, WorkoutSessionId.Value, exercise, color);
            exerciseVm.RequestRemove += OnExerciseRequestRemove;
            Exercises.Add(exerciseVm);
        }

        public void RemoveExercise(ExerciseViewModel exerciseVm)
        {
            if (exerciseVm == null || _dbService == null || !WorkoutSessionId.HasValue) return;

            // Remove from database
            _dbService.RemoveExerciseFromWorkoutSession(WorkoutSessionId.Value, exerciseVm.ExerciseId);

            // Remove from collection
            Exercises.Remove(exerciseVm);
        }

        private void OnExerciseRequestRemove(object sender, EventArgs e)
        {
            RemoveExercise(sender as ExerciseViewModel);
        }

        public void LoadWorkoutForDay()
        {
        
            // Get or create workout session
            var session = _dbService.GetOrCreateWorkoutSession(_userId, WeekPlanDayId, Date);
            if (session == null)
            {
                Exercises.Clear();
                return;
            }

            WorkoutSessionId = session.Id;

            // Load exercises for this workout
            var exercises = _dbService.GetSessionExercises(session.Id);

            Exercises.Clear();
            
            // Color progression for exercises
            string[] colors = { "#26A69A", "#66BB6A", "#42A5F5", "#AB47BC", "#FF9800", "#7E57C2" };
            int colorIndex = 0;

            foreach (var exercise in exercises)
            {
                string color = colors[colorIndex % colors.Length];
                var exerciseVm = new ExerciseViewModel(_dbService, session.Id, exercise, color);
                exerciseVm.RequestRemove += OnExerciseRequestRemove;
                Exercises.Add(exerciseVm);
                colorIndex++;
                colorIndex = colorIndex % colors.Length;
            }
        }
    }
}

