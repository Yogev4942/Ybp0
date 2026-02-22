using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface IWorkoutSessionRepository
    {
        WorkoutSession GetOrCreateWorkoutSession(int userId, int weekPlanDayId, DateTime date);
        List<Exercise> GetSessionExercises(int workoutSessionId);
        List<SessionSet> GetSessionSets(int workoutSessionId, int exerciseId);
        void SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight);
        void DeleteSessionSet(int setId);
        void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId);
        void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId);
    }
}
