using Models;
using System.Collections.Generic;

namespace DataBase.Repository.Interfaces
{
    public interface IWorkoutRepository
    {
        Workout GetWorkoutById(int workoutId);
        Dictionary<int, Workout> GetWorkoutsByIds(IEnumerable<int> workoutIds);
        List<Workout> GetWorkoutsByUserId(int userId);
        int CreateWorkout(int userId, string workoutName);
        void UpdateWorkoutName(int workoutId, string workoutName);
        WorkoutExercise AddExerciseToWorkout(int workoutId, int exerciseId);
        void RemoveExerciseFromWorkout(int workoutExerciseId);
        List<WorkoutSet> GetWorkoutSets(int workoutExerciseId);
        WorkoutSet SaveWorkoutSet(int workoutExerciseId, int setNumber, int reps, double weight);
        void DeleteWorkoutSet(int setId);
    }
}
