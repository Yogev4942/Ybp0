using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    internal class ExerciseData
    {
        int id;
        string exerciseName;
        string muscleGroup;

        public int Id { get => id; set => id = value; }
        public string ExerciseName { get => exerciseName; set => exerciseName = value; }
        public string MuscleGroup { get => muscleGroup; set => muscleGroup = value; }
    }
}
