using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ViewModels
{
    public class WorkoutViewModel : BaseViewModel
    {
        private ObservableCollection<ExerciseViewModel> _exercises;

        public ObservableCollection<ExerciseViewModel> Exercises
        {
            get => _exercises;
            set => SetProperty(ref _exercises, value);
        }

        public WorkoutViewModel()
        {
            Exercises = new ObservableCollection<ExerciseViewModel>();
        }
    }
}
