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
    public class ExerciseViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly int _workoutSessionId;
        private readonly int _exerciseId;
        private readonly string _setColor;

        private string _exerciseName;
        private string _muscleGroup;
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

        public ObservableCollection<SetViewModel> Sets
        {
            get => _sets;
            set => SetProperty(ref _sets, value);
        }

        public string SetColor => _setColor;

        // Public property for exercise identification (needed by parent)
        public int ExerciseId => _exerciseId;

        // Event to request removal from parent DayViewModel
        public event EventHandler RequestRemove;

        public ICommand AddSetCommand { get; }
        public ICommand RemoveSetCommand { get; }
        public ICommand RemoveExerciseCommand { get; }

        public ExerciseViewModel(IDatabaseService dbService, int workoutSessionId, Exercise exercise, string setColor = "#26A69A")
        {
            _dbService = dbService;
            _workoutSessionId = workoutSessionId;
            _exerciseId = exercise.Id;
            _exerciseName = exercise.ExerciseName;
            _muscleGroup = exercise.MuscleGroup;
            _setColor = setColor;

            Sets = new ObservableCollection<SetViewModel>();

            AddSetCommand = new RelayCommand(param => AddSet());
            RemoveSetCommand = new RelayCommand(param => RemoveSet(param as SetViewModel));
            RemoveExerciseCommand = new RelayCommand(_ => RequestRemove?.Invoke(this, EventArgs.Empty));

            LoadSets();
        }

        private void LoadSets()
        {
            // Skip loading from DB if no database service (for sample data)
            if (_dbService == null) return;

            var sessionSets = _dbService.GetSessionSets(_workoutSessionId, _exerciseId);

            foreach (var sessionSet in sessionSets)
            {
                var setVm = new SetViewModel(_dbService, _workoutSessionId, _exerciseId, _setColor)
                {
                    Id = sessionSet.Id,
                    SetNumber = sessionSet.SetNumber,
                    Reps = sessionSet.Reps.ToString(),
                    Weight = sessionSet.Weight.ToString()
                };
                Sets.Add(setVm);
            }
        }

        private void AddSet()
        {
            int newSetNumber = Sets.Count + 1;
            
            // Copy values from last set if available
            string reps = Sets.Count > 0 ? Sets.Last().Reps : "0";
            string weight = Sets.Count > 0 ? Sets.Last().Weight : "0";

            var newSet = new SetViewModel(_dbService, _workoutSessionId, _exerciseId, _setColor)
            {
                SetNumber = newSetNumber,
                Reps = reps,
                Weight = weight
            };

            Sets.Add(newSet);

            // Trigger immediate save for the new set (skip if no DB service)
            if (_dbService != null && int.TryParse(reps, out int r) && double.TryParse(weight, out double w))
            {
                _dbService.SaveSessionSet(_workoutSessionId, _exerciseId, newSetNumber, r, w);
            }
        }

        private void RemoveSet(SetViewModel set)
        {
            if (set == null) return;

            // Delete from database if it has an ID (skip if no DB service)
            if (_dbService != null && set.Id > 0)
            {
                _dbService.DeleteSessionSet(set.Id);
            }

            Sets.Remove(set);

            // Re-number remaining sets
            for (int i = 0; i < Sets.Count; i++)
            {
                Sets[i].SetNumber = i + 1;
            }
        }
    }
}
