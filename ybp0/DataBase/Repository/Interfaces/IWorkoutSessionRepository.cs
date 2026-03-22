using Models;
using System;
using System.Collections.Generic;

namespace DataBase.Repository.Interfaces
{
    public interface IWorkoutSessionRepository
    {
        WorkoutSession GetActiveSession(int userId);
        WorkoutSession GetSessionById(int workoutSessionId);
        WorkoutSession StartWorkoutSession(int userId, SessionMode mode, int? workoutId, int? weekPlanDayId, DateTime startTime);
        WorkoutSession FinishWorkoutSession(int workoutSessionId, DateTime endTime);
        void CancelWorkoutSession(int workoutSessionId);
        List<WorkoutSessionExercise> GetSessionExercises(int workoutSessionId);
        List<WorkoutSessionSet> GetSessionSets(int workoutSessionId, int exerciseId);
        WorkoutSessionSet SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight, bool isCompleted);
        WorkoutSessionSet AddSessionSet(int workoutSessionId, int exerciseId, int reps, double weight);
        void DeleteSessionSet(int setId);
        void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId);
        void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId);
    }
}
