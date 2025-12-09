using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Exercise : BaseEntity
    {
        private string exerciseName;
        private string muscleGroup;

        public string MuscleGroup { get => muscleGroup; set => muscleGroup = value; }
        public string ExerciseName { get => exerciseName; set => exerciseName = value; }
    }
}
