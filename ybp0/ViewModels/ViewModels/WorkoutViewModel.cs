using Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace ViewModels.ViewModels
{
    public class WorkoutViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly INavigationService _navigationService;
        private readonly User _currentUser;
        private readonly DispatcherTimer _timer;
        private DateTime? _sessionStartedAt;

        private ObservableCollection<ExerciseViewModel> _exercises;
        private ObservableCollection<Workout> _savedWorkouts;
        private ObservableCollection<Exercise> _allExercises;
        private Workout _selectedSavedWorkout;
        private WorkoutSession _activeSession;
        private bool _isExerciseModalOpen;
        private Exercise _selectedExerciseToAdd;
        private string _elapsedTime;
        private string _sessionTitle;
        private string _sessionSubtitle;
        private string _todayPlanSummary;
        private bool _canStartPlannedWorkout;

        public ObservableCollection<ExerciseViewModel> Exercises
        {
            get => _exercises;
            set => SetProperty(ref _exercises, value);
        }

        public ObservableCollection<Workout> SavedWorkouts
        {
            get => _savedWorkouts;
            set => SetProperty(ref _savedWorkouts, value);
        }

        public ObservableCollection<Exercise> AllExercises
        {
            get => _allExercises;
            set => SetProperty(ref _allExercises, value);
        }

        public Workout SelectedSavedWorkout
        {
            get => _selectedSavedWorkout;
            set
            {
                if (SetProperty(ref _selectedSavedWorkout, value))
                {
                    RaiseCommandState();
                }
            }
        }

        public WorkoutSession ActiveSession
        {
            get => _activeSession;
            set
            {
                if (SetProperty(ref _activeSession, value))
                {
                    OnPropertyChanged(nameof(HasActiveSession));
                    RaiseCommandState();
                }
            }
        }

        public bool HasActiveSession => ActiveSession != null && !ActiveSession.IsCompleted;

        public bool IsExerciseModalOpen
        {
            get => _isExerciseModalOpen;
            set => SetProperty(ref _isExerciseModalOpen, value);
        }

        public Exercise SelectedExerciseToAdd
        {
            get => _selectedExerciseToAdd;
            set
            {
                if (SetProperty(ref _selectedExerciseToAdd, value) && value != null)
                {
                    AddExerciseToSession(value);
                }
            }
        }

        public string ElapsedTime
        {
            get => _elapsedTime;
            set => SetProperty(ref _elapsedTime, value);
        }

        public string SessionTitle
        {
            get => _sessionTitle;
            set => SetProperty(ref _sessionTitle, value);
        }

        public string SessionSubtitle
        {
            get => _sessionSubtitle;
            set => SetProperty(ref _sessionSubtitle, value);
        }

        public string TodayPlanSummary
        {
            get => _todayPlanSummary;
            set => SetProperty(ref _todayPlanSummary, value);
        }

        public bool CanStartPlannedWorkout
        {
            get => _canStartPlannedWorkout;
            set => SetProperty(ref _canStartPlannedWorkout, value);
        }

        public ICommand StartPlannedWorkoutCommand { get; }
        public ICommand StartSavedWorkoutCommand { get; }
        public ICommand StartFreestyleWorkoutCommand { get; }
        public ICommand FinishWorkoutCommand { get; }
        public ICommand StopWorkoutCommand { get; }
        public ICommand OpenExerciseModalCommand { get; }
        public ICommand CloseExerciseModalCommand { get; }

        public WorkoutViewModel(IDatabaseService dbService, INavigationService navigationService, User currentUser)
        {
            _dbService = dbService;
            _navigationService = navigationService;
            _currentUser = currentUser;

            Exercises = new ObservableCollection<ExerciseViewModel>();
            SavedWorkouts = new ObservableCollection<Workout>();
            AllExercises = new ObservableCollection<Exercise>();
            ElapsedTime = "00:00:00";
            SessionTitle = "Current Workout";
            SessionSubtitle = "Start a session from today's plan, a saved template, or freestyle mode.";

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += TimerTick;

            StartPlannedWorkoutCommand = new RelayCommand(_ => StartPlannedWorkout(), _ => CanStartPlannedWorkout);
            StartSavedWorkoutCommand = new RelayCommand(_ => StartSavedWorkout(), _ => SelectedSavedWorkout != null);
            StartFreestyleWorkoutCommand = new RelayCommand(_ => StartFreestyleWorkout());
            FinishWorkoutCommand = new RelayCommand(_ => FinishWorkout(), _ => HasActiveSession);
            StopWorkoutCommand = new RelayCommand(_ => StopWorkout(), _ => HasActiveSession);
            OpenExerciseModalCommand = new RelayCommand(_ => OpenExerciseModal(), _ => HasActiveSession);
            CloseExerciseModalCommand = new RelayCommand(_ => CloseExerciseModal());

            RefreshViewState();
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();
            RefreshViewState();
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
            StopTimer();
            CloseExerciseModal();

            if (!HasActiveSession)
            {
                return;
            }

            _dbService.CancelWorkoutSession(ActiveSession.Id);
            ActiveSession = null;
            Exercises.Clear();
            ElapsedTime = "00:00:00";
            SessionTitle = "Current Workout";
            SessionSubtitle = "Start a session from today's plan, a saved template, or freestyle mode.";
            _sessionStartedAt = null;
            RaiseCommandState();
        }

        private void RefreshViewState()
        {
            SavedWorkouts = new ObservableCollection<Workout>(_dbService.GetWorkoutsByUserId(_currentUser.Id));
            if (SelectedSavedWorkout == null)
            {
                SelectedSavedWorkout = SavedWorkouts.FirstOrDefault();
            }

            LoadTodayPlanSummary();

            WorkoutSession activeSession = _dbService.GetActiveSession(_currentUser.Id);
            if (activeSession != null)
            {
                LoadSession(activeSession);
            }
            else
            {
                StopTimer();
                ActiveSession = null;
                Exercises.Clear();
                SessionTitle = "Current Workout";
                SessionSubtitle = "Start a session from today's plan, a saved template, or freestyle mode.";
                ElapsedTime = "00:00:00";
                _sessionStartedAt = null;
            }

            RaiseCommandState();
        }

        private void LoadTodayPlanSummary()
        {
            WeekPlanDay todayPlanDay = _dbService.GetWeekPlanDayForDate(_currentUser.Id, DateTime.Today);
            if (todayPlanDay != null && todayPlanDay.WorkoutId.HasValue && !todayPlanDay.RestDay)
            {
                TodayPlanSummary = todayPlanDay.WorkoutName ?? "Planned workout ready";
                CanStartPlannedWorkout = true;
            }
            else
            {
                TodayPlanSummary = "No workout scheduled for today.";
                CanStartPlannedWorkout = false;
            }
        }

        private void StartPlannedWorkout()
        {
            WeekPlanDay todayPlanDay = _dbService.GetWeekPlanDayForDate(_currentUser.Id, DateTime.Today);
            if (todayPlanDay == null || !todayPlanDay.WorkoutId.HasValue || todayPlanDay.RestDay)
            {
                return;
            }

            WorkoutSession session = _dbService.StartWorkoutSession(
                _currentUser.Id,
                SessionMode.Plan,
                todayPlanDay.WorkoutId,
                todayPlanDay.Id);

            LoadSession(session);
        }

        private void StartSavedWorkout()
        {
            if (SelectedSavedWorkout == null)
            {
                return;
            }

            WorkoutSession session = _dbService.StartWorkoutSession(
                _currentUser.Id,
                SessionMode.Saved,
                SelectedSavedWorkout.Id,
                null);

            LoadSession(session);
        }

        private void StartFreestyleWorkout()
        {
            WorkoutSession session = _dbService.StartWorkoutSession(
                _currentUser.Id,
                SessionMode.Freestyle,
                null,
                null);

            LoadSession(session);
        }

        private void LoadSession(WorkoutSession session)
        {
            if (session == null)
            {
                return;
            }

            ActiveSession = session;
            _sessionStartedAt = ResolveSessionStart(session);
            SessionTitle = !string.IsNullOrWhiteSpace(session.WorkoutName)
                ? session.WorkoutName
                : GetSessionModeTitle(session.Mode);
            SessionSubtitle = GetSessionSubtitle(session);

            var exercises = _dbService.GetSessionExercises(session.Id);
            Exercises.Clear();

            string[] colors = { "#26A69A", "#32B09B", "#43BAA1", "#5DC7AD", "#77D2BA", "#94DEC9" };
            int colorIndex = 0;

            foreach (WorkoutSessionExercise exercise in exercises)
            {
                string color = colors[colorIndex % colors.Length];
                var exerciseVm = new ExerciseViewModel(_dbService, session.Id, exercise, color);
                exerciseVm.RequestRemove += OnExerciseRequestRemove;
                Exercises.Add(exerciseVm);
                colorIndex++;
            }

            StartTimer();
            RaiseCommandState();
        }

        private void FinishWorkout()
        {
            if (!HasActiveSession)
            {
                return;
            }

            _dbService.FinishWorkoutSession(ActiveSession.Id);
            CloseExerciseModal();
            RefreshViewState();
        }

        private void StopWorkout()
        {
            if (!HasActiveSession)
            {
                return;
            }

            _dbService.CancelWorkoutSession(ActiveSession.Id);
            CloseExerciseModal();
            RefreshViewState();
        }

        private void OpenExerciseModal()
        {
            if (!HasActiveSession)
            {
                return;
            }

            var existingExerciseIds = Exercises.Select(ex => ex.ExerciseId).ToHashSet();
            AllExercises = new ObservableCollection<Exercise>(
                _dbService.GetAllExercises().Where(ex => !existingExerciseIds.Contains(ex.Id)));

            SelectedExerciseToAdd = null;
            IsExerciseModalOpen = true;
        }

        private void CloseExerciseModal()
        {
            IsExerciseModalOpen = false;
            SelectedExerciseToAdd = null;
        }

        private void AddExerciseToSession(Exercise exercise)
        {
            if (exercise == null || !HasActiveSession)
            {
                return;
            }

            _dbService.AddExerciseToWorkoutSession(ActiveSession.Id, exercise.Id);
            CloseExerciseModal();
            LoadSession(_dbService.GetSessionById(ActiveSession.Id));
        }

        private void OnExerciseRequestRemove(object sender, EventArgs e)
        {
            if (!HasActiveSession)
            {
                return;
            }

            var exerciseVm = sender as ExerciseViewModel;
            if (exerciseVm == null)
            {
                return;
            }

            _dbService.RemoveExerciseFromWorkoutSession(ActiveSession.Id, exerciseVm.ExerciseId);
            LoadSession(_dbService.GetSessionById(ActiveSession.Id));
        }

        private void StartTimer()
        {
            UpdateElapsedTime();
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        private void StopTimer()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            UpdateElapsedTime();
        }

        private void UpdateElapsedTime()
        {
            if (!HasActiveSession)
            {
                ElapsedTime = "00:00:00";
                return;
            }

            DateTime startAnchor = _sessionStartedAt ?? ResolveSessionStart(ActiveSession);
            if (DateTime.Now < startAnchor)
            {
                startAnchor = DateTime.Now;
            }

            TimeSpan elapsed = DateTime.Now - startAnchor;
            ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
        }

        private static DateTime ResolveSessionStart(WorkoutSession session)
        {
            if (session == null)
            {
                return DateTime.Now;
            }

            DateTime sessionDate = session.SessionDate == default(DateTime)
                ? DateTime.Today
                : session.SessionDate.Date;

            DateTime startTime = session.StartTime == default(DateTime)
                ? sessionDate
                : session.StartTime;

            // Access can sometimes preserve only the time component. In that case,
            // anchor the timer to the session date plus the stored time-of-day.
            if (startTime.Date.Year < 2000 || startTime.Date != sessionDate)
            {
                return sessionDate.Add(startTime.TimeOfDay);
            }

            return startTime;
        }

        private void RaiseCommandState()
        {
            (StartPlannedWorkoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StartSavedWorkoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (FinishWorkoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (StopWorkoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (OpenExerciseModalCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private static string GetSessionModeTitle(SessionMode mode)
        {
            switch (mode)
            {
                case SessionMode.Plan:
                    return "Planned Workout";
                case SessionMode.Saved:
                    return "Saved Workout";
                default:
                    return "Freestyle Workout";
            }
        }

        private static string GetSessionSubtitle(WorkoutSession session)
        {
            switch (session.Mode)
            {
                case SessionMode.Plan:
                    return "Running today's scheduled workout.";
                case SessionMode.Saved:
                    return "Running a saved workout template.";
                default:
                    return "Freestyle session. Add exercises as you go.";
            }
        }
    }
}
