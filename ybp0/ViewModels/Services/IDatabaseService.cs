using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ViewModels
{
    public interface IDatabaseService
    {
        IUserRepository UserRepo { get; }
        ITraineeRepository TraineeRepo { get; }
        ITrainerRepository TrainerRepo { get; }
        IExerciseRepository ExerciseRepo { get; }
        IMuscleRepository MuscleRepo { get; }
        IWorkoutRepository WorkoutRepo { get; }
        IWorkoutSessionRepository WorkoutSessionRepo { get; }
        IWeekPlanRepository WeekPlanRepo { get; }
        ITrainerRequestRepository TrainerRequestRepo { get; }
        IPostRepository PostRepo { get; }
        ILikeRepository LikeRepo { get; }

        User GetUserByUsernameAndPassword(string username, string password);
        bool ValidateLogin(string username, string password);
        bool UserExist(string username, string email);
        bool RegisterUser(string username, string email, string password);
        bool RegisterTrainee(string username, string email, string password, string fitnessGoal, double currentWeight, double height);
        bool RegisterTrainer(string username, string email, string password, string specialization, double hourlyRate, int maxTrainees);
        bool UpdateUser(User user);
        User GetUserById(int userId);

        List<WeekPlanDay> GetWeekPlanDays(int weekPlanId);
        WeekPlanDay GetWeekPlanDayById(int weekPlanDayId);
        WeekPlanDay GetWeekPlanDayForDate(int userId, DateTime date);
        WeekPlanDay UpdateWeekPlanDayWorkout(int weekPlanDayId, int? workoutId, bool restDay);

        Workout GetWorkoutById(int workoutId);
        List<Workout> GetWorkoutsByUserId(int userId);
        int CreateWorkout(int userId, string workoutName);
        void UpdateWorkoutName(int workoutId, string workoutName);
        WorkoutExercise AddExerciseToWorkout(int workoutId, int exerciseId);
        void RemoveExerciseFromWorkout(int workoutExerciseId);
        WorkoutSet SaveWorkoutSet(int workoutExerciseId, int setNumber, int reps, double weight);
        void DeleteWorkoutSet(int setId);

        WorkoutSession GetActiveSession(int userId);
        WorkoutSession GetSessionById(int workoutSessionId);
        WorkoutSession StartWorkoutSession(int userId, SessionMode mode, int? workoutId, int? weekPlanDayId);
        WorkoutSession FinishWorkoutSession(int workoutSessionId);
        void CancelWorkoutSession(int workoutSessionId);
        List<WorkoutSessionExercise> GetSessionExercises(int workoutSessionId);
        List<WorkoutSessionSet> GetSessionSets(int workoutSessionId, int exerciseId);
        WorkoutSessionSet SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight, bool isCompleted);
        WorkoutSessionSet AddSessionSet(int workoutSessionId, int exerciseId, int reps, double weight);
        void DeleteSessionSet(int setId);
        void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId);
        void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId);

        List<Exercise> GetAllExercises();
        List<Muscle> GetAllMuscles();

        int? GetWeekPlanOwnerUserId(int weekPlanId);
        List<Trainee> GetTraineesByTrainerId(int trainerId);
        int? GetUserWeekPlanId(int userId);
        int CreateEmptyWeekPlan(int userId, string planName);

        string GetTrainerRequestStatus(int traineeId, int trainerId);
        bool SendTrainerRequest(int traineeId, int trainerId);
        bool HandleTrainerRequest(int traineeId, int trainerId, string status);
        List<Trainee> GetPendingRequests(int trainerId);

        bool CreatePost(string header, string content, User user);
        bool DeletePost(int postId);
        ObservableCollection<Post> GetAllPosts();
        bool ToggleLike(int postId, int userId);
        int GetLikeCount(int postId);
        bool IsPostLikedByUser(int postId, int userId);
        List<Trainer> SearchTrainers(string query);
    }
}
