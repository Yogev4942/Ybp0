using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DayViewModel(IDatabaseService dbService, int userId)
        {
            _dbService = dbService;
            _userId = userId;
            Exercises = new ObservableCollection<ExerciseViewModel>();
        }

        public void LoadWorkoutForDay()
        {
            if (IsRestDay)
            {
                Exercises.Clear();
                return;
            }

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
                Exercises.Add(exerciseVm);
                colorIndex++;
            }
        }
    }
}
