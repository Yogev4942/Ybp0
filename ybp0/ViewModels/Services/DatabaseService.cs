using DataBase;
using DataBase.Repository.Access;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Services
{
    public class DatabaseService : IDatabaseService
    {
        // Repository fields
        private readonly IUserRepository _userRepo;
        private readonly ITraineeRepository _traineeRepo;
        private readonly ITrainerRepository _trainerRepo;
        private readonly IExerciseRepository _exerciseRepo;
        private readonly IWorkoutSessionRepository _workoutSessionRepo;
        private readonly IWeekPlanRepository _weekPlanRepo;
        private readonly ITrainerRequestRepository _trainerRequestRepo;
        private readonly IPostRepository _postRepo;
        private readonly ILikeRepository _likeRepo;

        // Repository properties (exposed via IDatabaseService)
        public IUserRepository UserRepo => _userRepo;
        public ITraineeRepository TraineeRepo => _traineeRepo;
        public ITrainerRepository TrainerRepo => _trainerRepo;
        public IExerciseRepository ExerciseRepo => _exerciseRepo;
        public IWorkoutSessionRepository WorkoutSessionRepo => _workoutSessionRepo;
        public IWeekPlanRepository WeekPlanRepo => _weekPlanRepo;
        public ITrainerRequestRepository TrainerRequestRepo => _trainerRequestRepo;
        public IPostRepository PostRepo => _postRepo;
        public ILikeRepository LikeRepo => _likeRepo;

        public DatabaseService()
        {
            _userRepo = new AccessUserRepository();
            _traineeRepo = new AccessTraineeRepository();
            _trainerRepo = new AccessTrainerRepository();
            _exerciseRepo = new AccessExerciseRepository();
            _workoutSessionRepo = new AccessWorkoutSessionRepository();
            _weekPlanRepo = new AccessWeekPlanRepository();
            _trainerRequestRepo = new AccessTrainerRequestRepository();
            _postRepo = new AccessPostRepository();
            _likeRepo = new AccessLikeRepository();
        }

        #region LOGIN
        public bool ValidateLogin(string username, string password)
        {
            return _userRepo.ValidateLogin(username, password);
        }

        public User GetUserById(int userId)
        {
            // Get base user to determine type
            var user = _userRepo.GetById(userId);
            if (user == null) return null;

            // Return full profile based on type
            if (user.IsTrainer)
            {
                return _trainerRepo.GetTrainerById(userId) ?? user;
            }
            else
            {
                return _traineeRepo.GetTraineeById(userId) ?? user;
            }
        }

        public User GetUserByUsernameAndPassword(string username, string password)
        {
            var user = _userRepo.GetByUsernameAndPassword(username, password);
            if (user == null) return null;

            // Return full profile based on type
            if (user.IsTrainer)
            {
                return _trainerRepo.GetTrainerById(user.Id) ?? user;
            }
            else
            {
                return _traineeRepo.GetTraineeById(user.Id) ?? user;
            }
        }

        public bool UserExist(string username, string email)
        {
            return _userRepo.UserExists(username, email);
        }
        #endregion

        #region REGISTER
        public bool RegisterUser(string username, string email, string password)
        {
            try
            {
                var userData = new Trainee
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    IsTrainer = false
                };
                int userId = _userRepo.CreateUser(userData);
                return userId > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return false;
            }
        }

        public bool RegisterTrainee(string username, string email, string password,
                                     string fitnessGoal, double currentWeight, double height)
        {
            try
            {
                // 1. Create user record
                var userData = new Trainee
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    IsTrainer = false
                };
                int userId = _userRepo.CreateUser(userData);
                if (userId == 0) return false;

                // 2. Create empty week plan
                int weekPlanId = _weekPlanRepo.CreateEmptyWeekPlan(userId, "My Week Plan");

                // 3. Update user with week plan id
                _userRepo.UpdateCurrentWeekPlanId(userId, weekPlanId);

                // 4. Create trainee profile
                _traineeRepo.CreateTraineeProfile(userId, fitnessGoal, currentWeight, height);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainee error: {ex.Message}");
                return false;
            }
        }

        public bool RegisterTrainer(string username, string email, string password,
                                    string specialization, double hourlyRate, int maxTrainees)
        {
            try
            {
                // 1. Create user record
                var userData = new Trainer
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    IsTrainer = true
                };
                int userId = _userRepo.CreateUser(userData);
                if (userId == 0) return false;

                // 2. Create trainer profile
                _trainerRepo.CreateTrainerProfile(userId, specialization, hourlyRate, maxTrainees);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainer error: {ex.Message}");
                return false;
            }
        }
        #endregion

        public bool UpdateUser(User user)
        {
            try
            {
                // 1. Update common fields in UserTbl
                bool commonUpdated = _userRepo.UpdateUserCommon(user.Id, user.Bio, user.Email);
                if (!commonUpdated) return false;

                // 2. Update type-specific fields
                if (user is Trainee trainee)
                {
                    _traineeRepo.UpdateTraineeProfile(trainee);
                }
                else if (user is Trainer trainer)
                {
                    _trainerRepo.UpdateTrainerProfile(trainer);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user error: {ex.Message}");
                return false;
            }
        }

        #region SessionManagement
        public WorkoutSession GetOrCreateWorkoutSession(int userId, int weekPlanDayId, DateTime date)
        {
            return _workoutSessionRepo.GetOrCreateWorkoutSession(userId, weekPlanDayId, date);
        }

        public List<Exercise> GetSessionExercises(int workoutSessionId)
        {
            return _workoutSessionRepo.GetSessionExercises(workoutSessionId);
        }

        public List<SessionSet> GetSessionSets(int workoutSessionId, int exerciseId)
        {
            return _workoutSessionRepo.GetSessionSets(workoutSessionId, exerciseId);
        }

        public void SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight)
        {
            _workoutSessionRepo.SaveSessionSet(workoutSessionId, exerciseId, setNumber, reps, weight);
        }

        public void DeleteSessionSet(int setId)
        {
            _workoutSessionRepo.DeleteSessionSet(setId);
        }

        public DataTable GetWeekPlanDays(int weekPlanId)
        {
            return _weekPlanRepo.GetWeekPlanDays(weekPlanId);
        }
        #endregion

        #region ExerciseManagement
        public List<Exercise> GetAllExercises()
        {
            return _exerciseRepo.GetAllExercises();
        }
        #endregion

        #region WorkoutManagement
        public void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId)
        {
            _workoutSessionRepo.AddExerciseToWorkoutSession(workoutSessionId, exerciseId);
        }

        public void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId)
        {
            _workoutSessionRepo.RemoveExerciseFromWorkoutSession(workoutSessionId, exerciseId);
        }

        public int? GetWeekPlanOwnerUserId(int weekPlanId)
        {
            return _weekPlanRepo.GetWeekPlanOwnerUserId(weekPlanId);
        }

        public List<Trainee> GetTraineesByTrainerId(int trainerId)
        {
            return _traineeRepo.GetTraineesByTrainerId(trainerId);
        }

        public int? GetUserWeekPlanId(int userId)
        {
            return _weekPlanRepo.GetUserWeekPlanId(userId);
        }

        public int CreateEmptyWeekPlan(int userId, string planName)
        {
            return _weekPlanRepo.CreateEmptyWeekPlan(userId, planName);
        }
        #endregion

        #region TrainerRequestManagement
        public string GetTrainerRequestStatus(int traineeId, int trainerId)
        {
            return _trainerRequestRepo.GetTrainerRequestStatus(traineeId, trainerId);
        }

        public bool SendTrainerRequest(int traineeId, int trainerId)
        {
            return _trainerRequestRepo.SendTrainerRequest(traineeId, trainerId);
        }

        public bool HandleTrainerRequest(int traineeId, int trainerId, string status)
        {
            return _trainerRequestRepo.HandleTrainerRequest(traineeId, trainerId, status);
        }

        public List<Trainee> GetPendingRequests(int trainerId)
        {
            return _trainerRequestRepo.GetPendingRequests(trainerId);
        }
        #endregion

        #region FeedManagement
        public bool CreatePost(string header, string content, User user)
        {
            return _postRepo.CreatePost(header, content, user.Id);
        }

        public bool DeletePost(int postId)
        {
            return _postRepo.DeletePost(postId);
        }

        public ObservableCollection<Post> GetAllPosts()
        {
            return _postRepo.GetAllPosts();
        }

        public bool ToggleLike(int postId, int userId)
        {
            return _likeRepo.ToggleLike(postId, userId);
        }

        public int GetLikeCount(int postId)
        {
            return _likeRepo.GetLikeCount(postId);
        }

        public bool IsPostLikedByUser(int postId, int userId)
        {
            return _likeRepo.IsPostLikedByUser(postId, userId);
        }

        public List<Trainer> SearchTrainers(string query)
        {
            return _trainerRepo.SearchTrainers(query);
        }
        #endregion
    }
}
