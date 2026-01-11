using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public interface IDatabaseService
    {
        /// <summary>
        /// Returns a user if the username and password match, otherwise null.
        /// </summary>
        User GetUserByUsernameAndPassword(string username, string password);

        /// <summary>
        /// checks if username and password are correct
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool ValidateLogin(string username, string password);

        bool UserExist(string username, string email);

        bool RegisterUser(string username, string email, string password);

        bool RegisterTrainee(string username, string email, string password,
                             string fitnessGoal, double currentWeight, double height);

        bool RegisterTrainer(string username, string email, string password,
                             string specialization, double hourlyRate, int maxTrainees);

        // Session Management
        WorkoutSession GetOrCreateWorkoutSession(int userId, int weekPlanDayId, DateTime date);
        void CompleteWorkoutSession(int sessionId);

        // Exercise & Sets
        List<Exercise> GetSessionExercises(int workoutSessionId);
        List<SessionSet> GetSessionSets(int workoutSessionId, int exerciseId);
        void SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight);
        void DeleteSessionSet(int setId);

        // Week Plan
        System.Data.DataTable GetWeekPlanDays(int weekPlanId);

        // Exercise Management
        List<Exercise> GetAllExercises();
        void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId);
        void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId);

        // WeekPlan Validation
        int? GetWeekPlanOwnerUserId(int weekPlanId);
        List<Trainee> GetTraineesByTrainerId(int trainerId);

        // WeekPlan Management
        int? GetUserWeekPlanId(int userId);
        int CreateEmptyWeekPlan(int userId, string planName);
        // Trainer Request Management
        string GetTrainerRequestStatus(int traineeId, int trainerId);
        bool SendTrainerRequest(int traineeId, int trainerId);
        bool HandleTrainerRequest(int traineeId, int trainerId, string status);
        User GetUserById(int userId);
        List<Trainee> GetPendingRequests(int trainerId);
    }
}
