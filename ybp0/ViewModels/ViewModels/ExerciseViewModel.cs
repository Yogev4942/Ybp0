using Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class ExerciseViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly int? _workoutExerciseId;
        private readonly int? _workoutSessionId;
        private readonly int _exerciseId;
        private readonly string _setColor;
        private readonly bool _isSessionMode;

        private string _exerciseName;
        private string _muscleGroup;
        private string _secondaryMuscleGroup;
        private ObservableCollection<SetViewModel> _sets;

        public string ExerciseName
        {
            get => _exerciseName;
            set => SetProperty(ref _exerciseName, value);
        }

        public string MuscleGroup
        {
            get => _muscleGroup;
            set => SetProperty(ref _muscleGroup, value);
        }

        public string SecondaryMuscleGroup
        {
            get => _secondaryMuscleGroup;
            set => SetProperty(ref _secondaryMuscleGroup, value);
        }

        public ObservableCollection<SetViewModel> Sets
        {
            get => _sets;
            set => SetProperty(ref _sets, value);
        }

        public string SetColor => _setColor;
        public int ExerciseId => _exerciseId;
        public int? WorkoutExerciseId => _workoutExerciseId;
        public bool IsSessionMode => _isSessionMode;

        public event EventHandler RequestRemove;

        public ICommand AddSetCommand { get; }
        public ICommand RemoveSetCommand { get; }
        public ICommand RemoveExerciseCommand { get; }

        public ExerciseViewModel(IDatabaseService dbService, WorkoutExercise workoutExercise, string setColor = "#26A69A")
        {
            _dbService = dbService;
            _workoutExerciseId = workoutExercise.Id;
            _exerciseId = workoutExercise.ExerciseId;
            _setColor = setColor;
            _isSessionMode = false;

            ExerciseName = workoutExercise.ExerciseName;
            MuscleGroup = workoutExercise.MuscleGroup;
            SecondaryMuscleGroup = workoutExercise.SecondaryMuscleGroup;
            Sets = new ObservableCollection<SetViewModel>();

            AddSetCommand = new RelayCommand(_ => AddTemplateSet());
            RemoveSetCommand = new RelayCommand(param => RemoveTemplateSet(param as SetViewModel));
            RemoveExerciseCommand = new RelayCommand(_ => RequestRemove?.Invoke(this, EventArgs.Empty));

            LoadTemplateSets(workoutExercise);
        }

        public ExerciseViewModel(IDatabaseService dbService, int workoutSessionId, WorkoutSessionExercise workoutSessionExercise, string setColor = "#26A69A")
        {
            _dbService = dbService;
            _workoutSessionId = workoutSessionId;
            _exerciseId = workoutSessionExercise.ExerciseId;
            _setColor = setColor;
            _isSessionMode = true;

            ExerciseName = workoutSessionExercise.ExerciseName;
            MuscleGroup = workoutSessionExercise.MuscleGroup;
            SecondaryMuscleGroup = workoutSessionExercise.SecondaryMuscleGroup;
            Sets = new ObservableCollection<SetViewModel>();

            AddSetCommand = new RelayCommand(_ => AddSessionSet());
            RemoveSetCommand = new RelayCommand(param => RemoveSessionSet(param as SetViewModel));
            RemoveExerciseCommand = new RelayCommand(_ => RequestRemove?.Invoke(this, EventArgs.Empty));

            LoadSessionSets(workoutSessionExercise);
        }

        private void LoadTemplateSets(WorkoutExercise workoutExercise)
        {
            Sets.Clear();
            foreach (WorkoutSet workoutSet in workoutExercise.Sets.OrderBy(set => set.SetNumber))
            {
                Sets.Add(new SetViewModel(_dbService, _workoutExerciseId.Value, workoutSet, _setColor));
            }
        }

        private void LoadSessionSets(WorkoutSessionExercise workoutSessionExercise)
        {
            Sets.Clear();
            foreach (WorkoutSessionSet sessionSet in workoutSessionExercise.Sets.OrderBy(set => set.SetNumber))
            {
                Sets.Add(new SetViewModel(_dbService, _workoutSessionId.Value, _exerciseId, sessionSet, _setColor));
            }
        }

        private void AddTemplateSet()
        {
            if (!_workoutExerciseId.HasValue)
            {
                return;
            }

            int newSetNumber = Sets.Count == 0 ? 1 : Sets.Max(set => set.SetNumber) + 1;
            int reps = Sets.Count > 0 && int.TryParse(Sets.Last().Reps, out int parsedReps) ? parsedReps : 0;
            double weight = Sets.Count > 0 && double.TryParse(Sets.Last().Weight, out double parsedWeight) ? parsedWeight : 0;

            WorkoutSet savedSet = _dbService.SaveWorkoutSet(_workoutExerciseId.Value, newSetNumber, reps, weight);
            if (savedSet != null)
            {
                Sets.Add(new SetViewModel(_dbService, _workoutExerciseId.Value, savedSet, _setColor));
            }
        }

        private void AddSessionSet()
        {
            if (!_workoutSessionId.HasValue)
            {
                return;
            }

            int reps = Sets.Count > 0 && int.TryParse(Sets.Last().Reps, out int parsedReps) ? parsedReps : 0;
            double weight = Sets.Count > 0 && double.TryParse(Sets.Last().Weight, out double parsedWeight) ? parsedWeight : 0;

            WorkoutSessionSet savedSet = _dbService.AddSessionSet(_workoutSessionId.Value, _exerciseId, reps, weight);
            if (savedSet != null)
            {
                Sets.Add(new SetViewModel(_dbService, _workoutSessionId.Value, _exerciseId, savedSet, _setColor));
            }
        }

        private void RemoveTemplateSet(SetViewModel set)
        {
            if (set == null)
            {
                return;
            }

            if (set.Id > 0)
            {
                _dbService.DeleteWorkoutSet(set.Id);
            }

            Sets.Remove(set);
        }

        private void RemoveSessionSet(SetViewModel set)
        {
            if (set == null)
            {
                return;
            }

            if (set.Id > 0)
            {
                _dbService.DeleteSessionSet(set.Id);
            }

            Sets.Remove(set);
        }
    }
}
