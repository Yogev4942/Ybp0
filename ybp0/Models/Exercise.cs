using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Exercise : BaseEntity
    {
        private int? primaryMuscleId;
        private int? secondaryMuscleId;
        private string exerciseName;
        private string muscleGroup;
        private string secondaryMuscleGroup;
        private Muscle primaryMuscle;
        private Muscle secondaryMuscle;

        public int? PrimaryMuscleId { get => primaryMuscleId; set => primaryMuscleId = value; }
        public int? SecondaryMuscleId { get => secondaryMuscleId; set => secondaryMuscleId = value; }
        public int? MuscleId { get => primaryMuscleId; set => primaryMuscleId = value; }
        public string MuscleGroup { get => muscleGroup; set => muscleGroup = value; }
        public string SecondaryMuscleGroup { get => secondaryMuscleGroup; set => secondaryMuscleGroup = value; }
        public string ExerciseName { get => exerciseName; set => exerciseName = value; }
        public Muscle PrimaryMuscle
        {
            get => primaryMuscle;
            set
            {
                primaryMuscle = value;
                primaryMuscleId = value?.Id;
                muscleGroup = value?.MuscleName;
            }
        }

        public Muscle SecondaryMuscle
        {
            get => secondaryMuscle;
            set
            {
                secondaryMuscle = value;
                secondaryMuscleId = value?.Id;
                secondaryMuscleGroup = value?.MuscleName;
            }
        }
    }
}
