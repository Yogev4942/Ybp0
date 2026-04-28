using Models;
using System;
using System.Timers;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class SetViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly int? _workoutExerciseId;
        private readonly int? _workoutSessionId;
        private readonly int? _exerciseId;
        private readonly bool _isSessionMode;
        private readonly System.Timers.Timer _saveTimer;
        private bool _isInitializing;

        private int _id;
        private int _setNumber;
        private string _reps;
        private string _weight;
        private bool _isCompleted;
        private string _setColor;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int SetNumber
        {
            get => _setNumber;
            set => SetProperty(ref _setNumber, value);
        }

        public string Reps
        {
            get => _reps;
            set
            {
                if (SetProperty(ref _reps, value))
                {
                    TriggerAutoSave();
                }
            }
        }

        public string Weight
        {
            get => _weight;
            set
            {
                if (SetProperty(ref _weight, value))
                {
                    TriggerAutoSave();
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value) && _isSessionMode)
                {
                    TriggerAutoSave();
                }
            }
        }

        public bool IsSessionMode => _isSessionMode;

        public string SetColor
        {
            get => _setColor;
            set => SetProperty(ref _setColor, value);
        }

        public SetViewModel(IDatabaseService dbService, int workoutExerciseId, WorkoutSet workoutSet, string setColor = "#26A69A")
        {
            _dbService = dbService;
            _workoutExerciseId = workoutExerciseId;
            _setColor = setColor;
            _isSessionMode = false;
            _saveTimer = CreateSaveTimer();

            ApplyWorkoutSet(workoutSet);
        }

        public SetViewModel(IDatabaseService dbService, int workoutSessionId, int exerciseId, WorkoutSessionSet workoutSessionSet, string setColor = "#26A69A")
        {
            _dbService = dbService;
            _workoutSessionId = workoutSessionId;
            _exerciseId = exerciseId;
            _setColor = setColor;
            _isSessionMode = true;
            _saveTimer = CreateSaveTimer();

            ApplySessionSet(workoutSessionSet);
        }

        private System.Timers.Timer CreateSaveTimer()
        {
            var timer = new System.Timers.Timer(500);
            timer.AutoReset = false;
            timer.Elapsed += SaveTimerElapsed;
            return timer;
        }

        public void ApplyWorkoutSet(WorkoutSet workoutSet)
        {
            if (workoutSet == null)
            {
                return;
            }

            _isInitializing = true;
            Id = workoutSet.Id;
            SetNumber = workoutSet.SetNumber;
            Reps = workoutSet.Reps.ToString();
            Weight = workoutSet.Weight.ToString();
            IsCompleted = false;
            _isInitializing = false;
        }

        public void ApplySessionSet(WorkoutSessionSet workoutSessionSet)
        {
            if (workoutSessionSet == null)
            {
                return;
            }

            _isInitializing = true;
            Id = workoutSessionSet.Id;
            SetNumber = workoutSessionSet.SetNumber;
            Reps = workoutSessionSet.Reps.ToString();
            Weight = workoutSessionSet.Weight.ToString();
            IsCompleted = workoutSessionSet.IsCompleted;
            _isInitializing = false;
        }

        private void TriggerAutoSave()
        {
            if (_saveTimer == null)
            {
                return;
            }

            if (_isInitializing)
            {
                return;
            }

            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveTimerElapsed(object sender, ElapsedEventArgs e)
        {
            SaveToDatabase();
        }

        private void SaveToDatabase()
        {
            if (!int.TryParse(Reps, out int reps) || !double.TryParse(Weight, out double weight))
            {
                return;
            }

            try
            {
                if (_isSessionMode && _workoutSessionId.HasValue && _exerciseId.HasValue)
                {
                    WorkoutSessionSet savedSet = _dbService.SaveSessionSet(
                        _workoutSessionId.Value,
                        _exerciseId.Value,
                        SetNumber,
                        reps,
                        weight,
                        IsCompleted);

                    if (savedSet != null)
                    {
                        Id = savedSet.Id;
                    }
                }
                else if (_workoutExerciseId.HasValue)
                {
                    WorkoutSet savedSet = _dbService.SaveWorkoutSet(
                        _workoutExerciseId.Value,
                        SetNumber,
                        reps,
                        weight);

                    if (savedSet != null)
                    {
                        Id = savedSet.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving set: {ex.Message}");
            }
        }
    }
}
