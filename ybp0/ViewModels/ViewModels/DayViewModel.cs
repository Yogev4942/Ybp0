using Models;
using System;
using System.Collections.ObjectModel;
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
        private int? _workoutId;
        private ObservableCollection<ExerciseViewModel> _exercises;
        private string _accentColor;

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

        public int? WorkoutId
        {
            get => _workoutId;
            set => SetProperty(ref _workoutId, value);
        }

        public ObservableCollection<ExerciseViewModel> Exercises
        {
            get => _exercises;
            set => SetProperty(ref _exercises, value);
        }

        public ICommand RemoveExerciseCommand { get; }

        public DayViewModel(IDatabaseService dbService, int userId)
        {
            _dbService = dbService;
            _userId = userId;
            _accentColor = "#26A69A";
            Exercises = new ObservableCollection<ExerciseViewModel>();
            RemoveExerciseCommand = new RelayCommand(param => RemoveExercise(param as ExerciseViewModel));
        }

        public void ApplyWeekPlanDay(WeekPlanDay weekPlanDay)
        {
            if (weekPlanDay == null)
            {
                return;
            }

            WeekPlanDayId = weekPlanDay.Id;
            WorkoutId = weekPlanDay.WorkoutId;
            IsRestDay = weekPlanDay.RestDay || !weekPlanDay.WorkoutId.HasValue;
            WorkoutName = !string.IsNullOrWhiteSpace(weekPlanDay.WorkoutName)
                ? weekPlanDay.WorkoutName
                : IsRestDay ? "Rest Day" : "Workout";

            LoadWorkoutForDay();
        }

        public void AddExerciseFromModal(Exercise exercise)
        {
            if (exercise == null)
            {
                return;
            }

            if (!WorkoutId.HasValue)
            {
                string workoutName = $"{DayName} Workout";
                int workoutId = _dbService.CreateWorkout(_userId, workoutName);
                _dbService.UpdateWeekPlanDayWorkout(WeekPlanDayId, workoutId, false);

                WorkoutId = workoutId;
                IsRestDay = false;
                WorkoutName = workoutName;
            }

            _dbService.AddExerciseToWorkout(WorkoutId.Value, exercise.Id);
            LoadWorkoutForDay();
        }

        public void RemoveExercise(ExerciseViewModel exerciseVm)
        {
            if (exerciseVm == null || !exerciseVm.WorkoutExerciseId.HasValue)
            {
                return;
            }

            _dbService.RemoveExerciseFromWorkout(exerciseVm.WorkoutExerciseId.Value);
            LoadWorkoutForDay();
        }

        public void LoadWorkoutForDay()
        {
            Exercises.Clear();

            if (!WorkoutId.HasValue)
            {
                IsRestDay = true;
                if (string.IsNullOrWhiteSpace(WorkoutName))
                {
                    WorkoutName = "Rest Day";
                }

                return;
            }

            Workout workout = _dbService.GetWorkoutById(WorkoutId.Value);
            if (workout == null)
            {
                IsRestDay = true;
                WorkoutName = "Rest Day";
                WorkoutId = null;
                return;
            }

            IsRestDay = false;
            WorkoutName = workout.WorkoutName;

            string[] colors = { "#26A69A", "#66BB6A", "#42A5F5", "#AB47BC", "#FF9800", "#7E57C2" };
            int colorIndex = 0;

            foreach (WorkoutExercise exercise in workout.WorkoutExercises)
            {
                string color = colors[colorIndex % colors.Length];
                var exerciseVm = new ExerciseViewModel(_dbService, exercise, color);
                exerciseVm.RequestRemove += OnExerciseRequestRemove;
                Exercises.Add(exerciseVm);
                colorIndex++;
            }
        }

        private void OnExerciseRequestRemove(object sender, EventArgs e)
        {
            RemoveExercise(sender as ExerciseViewModel);
        }
    }
}
