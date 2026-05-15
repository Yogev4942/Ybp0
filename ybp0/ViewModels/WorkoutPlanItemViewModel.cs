using System.Collections.ObjectModel;

namespace ViewModels.ViewModels
{
    public class WorkoutPlanItemViewModel : BaseViewModel
    {
        private int _id;
        private string _workoutName;
        private ObservableCollection<ExerciseViewModel> _exercises;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string WorkoutName
        {
            get => _workoutName;
            set
            {
                if (SetProperty(ref _workoutName, value))
                {
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string DisplayName => string.IsNullOrWhiteSpace(WorkoutName) ? "Workout Plan" : WorkoutName;

        public ObservableCollection<ExerciseViewModel> Exercises
        {
            get => _exercises;
            set => SetProperty(ref _exercises, value);
        }

        public WorkoutPlanItemViewModel()
        {
            Exercises = new ObservableCollection<ExerciseViewModel>();
        }
    }
}
