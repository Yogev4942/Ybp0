using DataBase.Connection;
using DataBase.Repository.Access;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ViewModels.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITraineeRepository _traineeRepo;
        private readonly ITrainerRepository _trainerRepo;
        private readonly IExerciseRepository _exerciseRepo;
        private readonly IMuscleRepository _muscleRepo;
        private readonly IWorkoutRepository _workoutRepo;
        private readonly IWorkoutSessionRepository _workoutSessionRepo;
        private readonly IWeekPlanRepository _weekPlanRepo;
        private readonly ITrainerRequestRepository _trainerRequestRepo;
        private readonly IPostRepository _postRepo;
        private readonly ILikeRepository _likeRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IDataBaseConnection _connection;

        public IUserRepository UserRepo => _userRepo;
        public ITraineeRepository TraineeRepo => _traineeRepo;
        public ITrainerRepository TrainerRepo => _trainerRepo;
        public IExerciseRepository ExerciseRepo => _exerciseRepo;
        public IMuscleRepository MuscleRepo => _muscleRepo;
        public IWorkoutRepository WorkoutRepo => _workoutRepo;
        public IWorkoutSessionRepository WorkoutSessionRepo => _workoutSessionRepo;
        public IWeekPlanRepository WeekPlanRepo => _weekPlanRepo;
        public ITrainerRequestRepository TrainerRequestRepo => _trainerRequestRepo;
        public IPostRepository PostRepo => _postRepo;
        public ILikeRepository LikeRepo => _likeRepo;
        public IMessageRepository MessageRepo => _messageRepo;

        public DatabaseService()
        {
            _connection = DatabaseFilter.CreateConnection();
            _userRepo = new AccessUserRepository(_connection);
            _traineeRepo = new AccessTraineeRepository(_connection);
            _trainerRepo = new AccessTrainerRepository(_connection);
            _exerciseRepo = new AccessExerciseRepository(_connection);
            _muscleRepo = new AccessMusclesRepository(_connection);
            _workoutRepo = new AccessWorkoutRepository(_connection);
            _workoutSessionRepo = new AccessWorkoutSessionRepository(_connection);
            _weekPlanRepo = new AccessWeekPlanRepository(_connection);
            _trainerRequestRepo = new AccessTrainerRequestRepository(_connection);
            _postRepo = new AccessPostRepository(_connection);
            _likeRepo = new AccessLikeRepository(_connection);
            _messageRepo = new AccessMessageRepository(_connection);
        }

        public bool ValidateLogin(string username, string password)
        {
            return _userRepo.ValidateLogin(username, password);
        }

        public User GetUserById(int userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null)
            {
                return null;
            }

            return user.IsTrainer
                ? (User)(_trainerRepo.GetTrainerById(userId) ?? user)
                : (User)(_traineeRepo.GetTraineeById(userId) ?? user);
        }

        public Dictionary<int, User> GetUsersByIds(IEnumerable<int> userIds)
        {
            List<int> ids = userIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new Dictionary<int, User>();
            }

            Dictionary<int, User> baseUsers = _userRepo.GetByIds(ids);
            var result = new Dictionary<int, User>();

            foreach (int userId in ids)
            {
                if (!baseUsers.TryGetValue(userId, out User user) || user == null)
                {
                    continue;
                }

                result[userId] = user.IsTrainer
                    ? (User)(_trainerRepo.GetTrainerById(userId) ?? user)
                    : (User)(_traineeRepo.GetTraineeById(userId) ?? user);
            }

            return result;
        }

        public User GetUserByUsernameAndPassword(string username, string password)
        {
            var user = _userRepo.GetByUsernameAndPassword(username, password);
            if (user == null)
            {
                return null;
            }

            return user.IsTrainer
                ? (User)(_trainerRepo.GetTrainerById(user.Id) ?? user)
                : (User)(_traineeRepo.GetTraineeById(user.Id) ?? user);
        }

        public bool UserExist(string username, string email)
        {
            return _userRepo.UserExists(username, email);
        }

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

        public bool RegisterTrainee(string username, string email, string password, string fitnessGoal, double currentWeight, double height)
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
                if (userId == 0)
                {
                    return false;
                }

                int weekPlanId = _weekPlanRepo.CreateEmptyWeekPlan(userId, "My Week Plan");
                _userRepo.UpdateCurrentWeekPlanId(userId, weekPlanId);
                _traineeRepo.CreateTraineeProfile(userId, fitnessGoal, currentWeight, height);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainee error: {ex.Message}");
                return false;
            }
        }

        public bool RegisterTrainer(string username, string email, string password, string specialization, double hourlyRate, int maxTrainees)
        {
            try
            {
                var userData = new Trainer
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    IsTrainer = true
                };

                int userId = _userRepo.CreateUser(userData);
                if (userId == 0)
                {
                    return false;
                }

                _trainerRepo.CreateTrainerProfile(userId, specialization, hourlyRate, maxTrainees);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register trainer error: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUser(User user)
        {
            try
            {
                bool commonUpdated = _userRepo.UpdateUserCommon(user.Id, user.Bio, user.Email);
                if (!commonUpdated)
                {
                    return false;
                }

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

        public List<WeekPlanDay> GetWeekPlanDays(int weekPlanId)
        {
            return _weekPlanRepo.GetWeekPlanDays(weekPlanId);
        }

        public WeekPlanDay GetWeekPlanDayById(int weekPlanDayId)
        {
            return _weekPlanRepo.GetWeekPlanDayById(weekPlanDayId);
        }

        public WeekPlanDay GetWeekPlanDayForDate(int userId, DateTime date)
        {
            return _weekPlanRepo.GetWeekPlanDayForDate(userId, date);
        }

        public WeekPlanDay UpdateWeekPlanDayWorkout(int weekPlanDayId, int? workoutId, bool restDay)
        {
            return _weekPlanRepo.UpdateWeekPlanDayWorkout(weekPlanDayId, workoutId, restDay);
        }

        public Workout GetWorkoutById(int workoutId)
        {
            return _workoutRepo.GetWorkoutById(workoutId);
        }

        public Dictionary<int, Workout> GetWorkoutsByIds(IEnumerable<int> workoutIds)
        {
            return _workoutRepo.GetWorkoutsByIds(workoutIds);
        }

        public List<Workout> GetWorkoutsByUserId(int userId)
        {
            return _workoutRepo.GetWorkoutsByUserId(userId);
        }

        public int CreateWorkout(int userId, string workoutName)
        {
            return _workoutRepo.CreateWorkout(userId, workoutName);
        }

        public void UpdateWorkoutName(int workoutId, string workoutName)
        {
            _workoutRepo.UpdateWorkoutName(workoutId, workoutName);
        }

        public WorkoutExercise AddExerciseToWorkout(int workoutId, int exerciseId)
        {
            return _workoutRepo.AddExerciseToWorkout(workoutId, exerciseId);
        }

        public void RemoveExerciseFromWorkout(int workoutExerciseId)
        {
            _workoutRepo.RemoveExerciseFromWorkout(workoutExerciseId);
        }

        public WorkoutSet SaveWorkoutSet(int workoutExerciseId, int setNumber, int reps, double weight)
        {
            return _workoutRepo.SaveWorkoutSet(workoutExerciseId, setNumber, reps, weight);
        }

        public void DeleteWorkoutSet(int setId)
        {
            _workoutRepo.DeleteWorkoutSet(setId);
        }

        public WorkoutSession GetActiveSession(int userId)
        {
            return _workoutSessionRepo.GetActiveSession(userId);
        }

        public WorkoutSession GetSessionById(int workoutSessionId)
        {
            return _workoutSessionRepo.GetSessionById(workoutSessionId);
        }

        public List<WorkoutSession> GetCompletedSessionsByUserId(int userId, int maxCount = 120)
        {
            return _workoutSessionRepo.GetCompletedSessionsByUserId(userId, maxCount);
        }

        public WorkoutSession StartWorkoutSession(int userId, SessionMode mode, int? workoutId, int? weekPlanDayId)
        {
            return _workoutSessionRepo.StartWorkoutSession(userId, mode, workoutId, weekPlanDayId, DateTime.Now);
        }

        public WorkoutSession FinishWorkoutSession(int workoutSessionId)
        {
            return _workoutSessionRepo.FinishWorkoutSession(workoutSessionId, DateTime.Now);
        }

        public void CancelWorkoutSession(int workoutSessionId)
        {
            _workoutSessionRepo.CancelWorkoutSession(workoutSessionId);
        }

        public List<WorkoutSessionExercise> GetSessionExercises(int workoutSessionId)
        {
            return _workoutSessionRepo.GetSessionExercises(workoutSessionId);
        }

        public List<WorkoutSessionSet> GetSessionSets(int workoutSessionId, int exerciseId)
        {
            return _workoutSessionRepo.GetSessionSets(workoutSessionId, exerciseId);
        }

        public WorkoutSessionSet SaveSessionSet(int workoutSessionId, int exerciseId, int setNumber, int reps, double weight, bool isCompleted)
        {
            return _workoutSessionRepo.SaveSessionSet(workoutSessionId, exerciseId, setNumber, reps, weight, isCompleted);
        }

        public WorkoutSessionSet AddSessionSet(int workoutSessionId, int exerciseId, int reps, double weight)
        {
            return _workoutSessionRepo.AddSessionSet(workoutSessionId, exerciseId, reps, weight);
        }

        public void DeleteSessionSet(int setId)
        {
            _workoutSessionRepo.DeleteSessionSet(setId);
        }

        public void AddExerciseToWorkoutSession(int workoutSessionId, int exerciseId)
        {
            _workoutSessionRepo.AddExerciseToWorkoutSession(workoutSessionId, exerciseId);
        }

        public void RemoveExerciseFromWorkoutSession(int workoutSessionId, int exerciseId)
        {
            _workoutSessionRepo.RemoveExerciseFromWorkoutSession(workoutSessionId, exerciseId);
        }

        public List<Exercise> GetAllExercises()
        {
            return _exerciseRepo.GetAllExercises();
        }

        public List<Muscle> GetAllMuscles()
        {
            return _muscleRepo.GetAllMuscles();
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

        public HashSet<int> GetLikedPostIds(IEnumerable<int> postIds, int userId)
        {
            return _postRepo.GetLikedPostIds(postIds, userId);
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

        public void SendMessage(int senderId, int recipientId, string messageText)
        {
            _messageRepo.SendMessage(senderId, recipientId, messageText);
        }

        public List<Message> GetConversation(int userIdA, int userIdB)
        {
            return _messageRepo.GetConversation(userIdA, userIdB);
        }

        public Message GetLatestMessage(int userIdA, int userIdB)
        {
            return _messageRepo.GetLatestMessage(userIdA, userIdB);
        }

        public Dictionary<int, Message> GetLatestMessagesByContacts(int userId, IEnumerable<int> contactIds)
        {
            return _messageRepo.GetLatestMessagesByContacts(userId, contactIds);
        }

        public List<int> GetChatContactIds(int userId)
        {
            return _messageRepo.GetChatContactIds(userId);
        }
    }
}
