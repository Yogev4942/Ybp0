using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class WorkoutExercise : BaseEntity
    {
        Exercise exercise;
        private int sets;
        private int reps;
        private int weight;

        public Exercise Exercise { get => exercise; set => exercise = value; }
        public int Sets { get => sets; set => sets = value; }
        public int Reps { get => reps; set => reps = value; }
        public int Weight { get => weight; set => weight = value; }
    }
}
