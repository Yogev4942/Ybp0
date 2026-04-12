using Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace ViewModels.ViewModels
{
    public class WorkoutPlansViewModel : BaseViewModel
    {
        private static readonly string[] PreviewDayNames =
        {
            "MONDAY",
            "TUESDAY",
            "WEDNESDAY",
            "THURSDAY",
            "FRIDAY",
            "SATURDAY",
            "SUNDAY"
        };

        private readonly IDatabaseService _dbService;
        private readonly User _currentUser;
        private readonly DispatcherTimer _renameTimer;

        private ObservableCollection<WorkoutPlanItemViewModel> _workoutPlans;
        private WorkoutPlanItemViewModel _selectedWorkoutPlan;
        private ObservableCollection<Exercise> _allExercises;
        private Exercise _selectedExerciseToAdd;
        private bool _isExerciseModalOpen;
        private string _editableWorkoutName;
        private string _previewDayName;
        private string _previewSummary;

        public ObservableCollection<WorkoutPlanItemViewModel> WorkoutPlans
        {
            get => _workoutPlans;
            set => SetProperty(ref _workoutPlans, value);
        }

        public WorkoutPlanItemViewModel SelectedWorkoutPlan
        {
            get => _selectedWorkoutPlan;
            set
            {
                if (SetProperty(ref _selectedWorkoutPlan, value))
                {
                    EditableWorkoutName = value?.WorkoutName ?? string.Empty;
                    UpdatePreviewState();
                    RaiseCommandState();
                }
            }
        }

        public ObservableCollection<Exercise> AllExercises
        {
            get => _allExercises;
            set => SetProperty(ref _allExercises, value);
        }

        public Exercise SelectedExerciseToAdd
        {
            get => _selectedExerciseToAdd;
            set
            {
                if (SetProperty(ref _selectedExerciseToAdd, value) && value != null)
                {
                    AddExerciseToSelectedWorkout(value);
                }
            }
        }

        public bool IsExerciseModalOpen
        {
            get => _isExerciseModalOpen;
            set => SetProperty(ref _isExerciseModalOpen, value);
        }

        public string EditableWorkoutName
        {
            get => _editableWorkoutName;
            set
            {
                if (SetProperty(ref _editableWorkoutName, value) && SelectedWorkoutPlan != null)
                {
                    SelectedWorkoutPlan.WorkoutName = value;
                    UpdatePreviewState();
                    ScheduleRename();
                }
            }
        }

        public string PreviewDayName
        {
            get => _previewDayName;
            set => SetProperty(ref _previewDayName, value);
        }

        public string PreviewSummary
        {
            get => _previewSummary;
            set => SetProperty(ref _previewSummary, value);
        }

        public bool HasSelectedWorkout => SelectedWorkoutPlan != null;

        public ICommand AddWorkoutPlanCommand { get; }
        public ICommand OpenExerciseModalCommand { get; }
        public ICommand CloseExerciseModalCommand { get; }

        public WorkoutPlansViewModel(IDatabaseService dbService, INavigationService navigationService, User currentUser)
        {
            _dbService = dbService;
            _currentUser = currentUser;

            WorkoutPlans = new ObservableCollection<WorkoutPlanItemViewModel>();
            AllExercises = new ObservableCollection<Exercise>();
            PreviewDayName = PreviewDayNames[0];
            PreviewSummary = "Create a workout plan and build it with your saved exercise template cards.";

            _renameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _renameTimer.Tick += RenameTimerTick;

            AddWorkoutPlanCommand = new RelayCommand(_ => AddWorkoutPlan());
            OpenExerciseModalCommand = new RelayCommand(_ => OpenExerciseModal(), _ => HasSelectedWorkout);
            CloseExerciseModalCommand = new RelayCommand(_ => CloseExerciseModal());

            RefreshWorkoutPlans();
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();
            RefreshWorkoutPlans(SelectedWorkoutPlan?.Id);
        }

        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();
            PersistWorkoutName();
            CloseExerciseModal();
        }

        private void RefreshWorkoutPlans(int? selectedWorkoutId = null)
        {
            var plans = _dbService.GetWorkoutsByUserId(_currentUser.Id)
                .Select(workout => CreatePlanItem(workout))
                .ToList();

            WorkoutPlans = new ObservableCollection<WorkoutPlanItemViewModel>(plans);

            WorkoutPlanItemViewModel selectedPlan = null;
            if (selectedWorkoutId.HasValue)
            {
                selectedPlan = WorkoutPlans.FirstOrDefault(plan => plan.Id == selectedWorkoutId.Value);
            }

            if (selectedPlan == null)
            {
                selectedPlan = WorkoutPlans.FirstOrDefault();
            }

            SelectedWorkoutPlan = selectedPlan;
            OnPropertyChanged(nameof(HasSelectedWorkout));
        }

        private WorkoutPlanItemViewModel CreatePlanItem(Workout workout)
        {
            var item = new WorkoutPlanItemViewModel
            {
                Id = workout.Id,
                WorkoutName = workout.WorkoutName
            };

            string[] colors = { "#26A69A", "#32B09B", "#43BAA1", "#5DC7AD", "#77D2BA", "#94DEC9" };
            int colorIndex = 0;

            foreach (WorkoutExercise exercise in workout.WorkoutExercises)
            {
                string color = colors[colorIndex % colors.Length];
                var exerciseVm = new ExerciseViewModel(_dbService, exercise, color);
                exerciseVm.RequestRemove += OnExerciseRequestRemove;
                item.Exercises.Add(exerciseVm);
                colorIndex++;
            }

            return item;
        }

        private void AddWorkoutPlan()
        {
            string workoutName = BuildNextWorkoutName();
            int newWorkoutId = _dbService.CreateWorkout(_currentUser.Id, workoutName);
            RefreshWorkoutPlans(newWorkoutId);
        }

        private string BuildNextWorkoutName()
        {
            int suffix = WorkoutPlans.Count + 1;
            string candidate = "New Workout";

            while (WorkoutPlans.Any(plan => string.Equals(plan.DisplayName, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                suffix++;
                candidate = $"New Workout {suffix}";
            }

            return candidate;
        }

        private void OpenExerciseModal()
        {
            if (!HasSelectedWorkout)
            {
                return;
            }

            var existingExerciseIds = SelectedWorkoutPlan.Exercises.Select(ex => ex.ExerciseId).ToHashSet();
            AllExercises = new ObservableCollection<Exercise>(
                _dbService.GetAllExercises().Where(exercise => !existingExerciseIds.Contains(exercise.Id)));

            SelectedExerciseToAdd = null;
            IsExerciseModalOpen = true;
        }

        private void CloseExerciseModal()
        {
            IsExerciseModalOpen = false;
            SelectedExerciseToAdd = null;
        }

        private void AddExerciseToSelectedWorkout(Exercise exercise)
        {
            if (exercise == null || !HasSelectedWorkout)
            {
                return;
            }

            _dbService.AddExerciseToWorkout(SelectedWorkoutPlan.Id, exercise.Id);
            CloseExerciseModal();
            ReloadSelectedWorkout();
        }

        private void ReloadSelectedWorkout()
        {
            if (!HasSelectedWorkout)
            {
                return;
            }

            int selectedId = SelectedWorkoutPlan.Id;
            RefreshWorkoutPlans(selectedId);
        }

        private void OnExerciseRequestRemove(object sender, EventArgs e)
        {
            if (!HasSelectedWorkout)
            {
                return;
            }

            var exerciseVm = sender as ExerciseViewModel;
            if (exerciseVm == null || !exerciseVm.WorkoutExerciseId.HasValue)
            {
                return;
            }

            _dbService.RemoveExerciseFromWorkout(exerciseVm.WorkoutExerciseId.Value);
            ReloadSelectedWorkout();
        }

        private void ScheduleRename()
        {
            _renameTimer.Stop();
            _renameTimer.Start();
        }

        private void RenameTimerTick(object sender, EventArgs e)
        {
            _renameTimer.Stop();
            PersistWorkoutName();
        }

        private void PersistWorkoutName()
        {
            if (!HasSelectedWorkout)
            {
                return;
            }

            string normalizedName = string.IsNullOrWhiteSpace(EditableWorkoutName)
                ? "Workout Plan"
                : EditableWorkoutName.Trim();

            if (!string.Equals(normalizedName, EditableWorkoutName, StringComparison.Ordinal))
            {
                _editableWorkoutName = normalizedName;
                OnPropertyChanged(nameof(EditableWorkoutName));
            }

            if (!string.Equals(SelectedWorkoutPlan.WorkoutName, normalizedName, StringComparison.Ordinal))
            {
                SelectedWorkoutPlan.WorkoutName = normalizedName;
            }

            _dbService.UpdateWorkoutName(SelectedWorkoutPlan.Id, normalizedName);
            UpdatePreviewState();
        }

        private void UpdatePreviewState()
        {
            int selectedIndex = SelectedWorkoutPlan == null ? 0 : Math.Max(WorkoutPlans.IndexOf(SelectedWorkoutPlan), 0);
            PreviewDayName = PreviewDayNames[selectedIndex % PreviewDayNames.Length];

            if (SelectedWorkoutPlan == null)
            {
                PreviewSummary = "Create a workout plan and build it with your saved exercise template cards.";
                return;
            }

            int exerciseCount = SelectedWorkoutPlan.Exercises.Count;
            PreviewSummary = exerciseCount == 0
                ? "This plan is empty. Use the add button to drop exercises into it."
                : $"{exerciseCount} exercise{(exerciseCount == 1 ? string.Empty : "s")} ready in this plan.";
        }

        private void RaiseCommandState()
        {
            OnPropertyChanged(nameof(HasSelectedWorkout));
            (OpenExerciseModalCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
