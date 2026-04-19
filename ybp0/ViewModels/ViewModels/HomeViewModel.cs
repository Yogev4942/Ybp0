using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ViewModels.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly User _activeUser;
        private readonly IDatabaseService _database;
        private readonly INavigationService _navigation;

        private string _welcomeMessage;
        private string _monthSummary;
        private string _statisticsSummary;
        private ObservableCollection<ActivityDayViewModel> _activityDays;
        private ObservableCollection<ExerciseProgressCardViewModel> _exerciseProgressCards;

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public string MonthSummary
        {
            get => _monthSummary;
            set => SetProperty(ref _monthSummary, value);
        }

        public string StatisticsSummary
        {
            get => _statisticsSummary;
            set => SetProperty(ref _statisticsSummary, value);
        }

        public ObservableCollection<ActivityDayViewModel> ActivityDays
        {
            get => _activityDays;
            set => SetProperty(ref _activityDays, value);
        }

        public ObservableCollection<ExerciseProgressCardViewModel> ExerciseProgressCards
        {
            get => _exerciseProgressCards;
            set => SetProperty(ref _exerciseProgressCards, value);
        }

        public string UserName => _activeUser?.Username ?? "Guest";

        private static readonly Random _random = new Random();

        public HomeViewModel(IDatabaseService database, INavigationService navigation, User user)
        {
            _activeUser = user;
            _navigation = navigation;
            _database = database;

            ActivityDays = new ObservableCollection<ActivityDayViewModel>();
            ExerciseProgressCards = new ObservableCollection<ExerciseProgressCardViewModel>();

            WelcomeMessage = GenerateWelcomeMessage();
            LoadDashboard();
        }

        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            List<WorkoutSession> completedSessions = _database.GetCompletedSessionsByUserId(_activeUser.Id, 180);
            BuildActivityCalendar(completedSessions);
            BuildProgressCards(completedSessions);
        }

        private void BuildActivityCalendar(List<WorkoutSession> completedSessions)
        {
            DateTime today = DateTime.Today;
            DateTime startDate = today.AddDays(-29);
            HashSet<DateTime> activeDates = completedSessions
                .Select(session => session.SessionDate.Date)
                .ToHashSet();

            ActivityDays.Clear();

            for (int i = 0; i < 30; i++)
            {
                DateTime currentDate = startDate.AddDays(i);
                bool isCompleted = activeDates.Contains(currentDate);

                ActivityDays.Add(new ActivityDayViewModel
                {
                    Date = currentDate,
                    DayLabel = currentDate.ToString("dd"),
                    WeekdayLabel = currentDate.ToString("ddd").ToUpperInvariant(),
                    IsCompleted = isCompleted,
                    FillColor = isCompleted ? "#26A69A" : "#DCEBE7"
                });
            }

            int completedDays = ActivityDays.Count(day => day.IsCompleted);
            MonthSummary = $"{completedDays} training day{(completedDays == 1 ? string.Empty : "s")} in the last 30 days";
        }

        private void BuildProgressCards(List<WorkoutSession> completedSessions)
        {
            var volumeByExercise = new Dictionary<string, List<ExerciseVolumePointViewModel>>(StringComparer.OrdinalIgnoreCase);

            foreach (WorkoutSession session in completedSessions
                .OrderBy(session => session.SessionDate)
                .ThenBy(session => session.StartTime))
            {
                foreach (WorkoutSessionExercise exercise in _database.GetSessionExercises(session.Id))
                {
                    double totalVolume = exercise.Sets.Sum(set => set.Reps * set.Weight);
                    if (totalVolume <= 0)
                    {
                        continue;
                    }

                    if (!volumeByExercise.TryGetValue(exercise.ExerciseName, out List<ExerciseVolumePointViewModel> points))
                    {
                        points = new List<ExerciseVolumePointViewModel>();
                        volumeByExercise[exercise.ExerciseName] = points;
                    }

                    points.Add(new ExerciseVolumePointViewModel
                    {
                        Date = session.SessionDate.Date,
                        Label = session.SessionDate.ToString("dd MMM"),
                        Volume = totalVolume
                    });
                }
            }

            var cards = volumeByExercise
                .Where(entry => entry.Value.Count >= 2)
                .Select(entry =>
                {
                    List<ExerciseVolumePointViewModel> points = entry.Value
                        .OrderBy(point => point.Date)
                        .ToList();

                    if (points.Count > 6)
                    {
                        points = points.Skip(points.Count - 6).ToList();
                    }

                    double maxVolume = points.Max(point => point.Volume);
                    foreach (ExerciseVolumePointViewModel point in points)
                    {
                        point.BarWidth = maxVolume <= 0 ? 0 : 220 * (point.Volume / maxVolume);
                        point.VolumeLabel = point.Volume.ToString("0");
                    }

                    double firstVolume = points.First().Volume;
                    double latestVolume = points.Last().Volume;
                    double delta = latestVolume - firstVolume;

                    return new ExerciseProgressCardViewModel
                    {
                        ExerciseName = entry.Key,
                        Summary = delta >= 0
                            ? $"+{delta:0} volume load from your first logged session"
                            : $"{delta:0} volume load from your first logged session",
                        Points = new ObservableCollection<ExerciseVolumePointViewModel>(points),
                        LatestVolume = latestVolume,
                        AccentColor = latestVolume >= firstVolume ? "#26A69A" : "#EF6C57"
                    };
                })
                .OrderByDescending(card => card.LatestVolume)
                .Take(4)
                .ToList();

            ExerciseProgressCards = new ObservableCollection<ExerciseProgressCardViewModel>(cards);
            StatisticsSummary = cards.Count == 0
                ? "Complete a couple of workouts to unlock progression cards."
                : "Volume load = sets x reps x weight, a simple way to track progression over time.";
        }

        private string GenerateWelcomeMessage()
        {
            var messages = _activeUser.IsTrainer
                ? new List<string>
                {
                    $"Welcome back, Coach {UserName}!",
                    $"Ready to lead the grind, {UserName}?",
                    $"Back in command, {UserName}.",
                    $"The gym missed you, Coach {UserName}."
                }
                : new List<string>
                {
                    $"Welcome back, {UserName}!",
                    $"Ready to crush it today, {UserName}?",
                    $"Back at it again, {UserName}.",
                    $"Another day, another gain, {UserName}."
                };

            return messages[_random.Next(messages.Count)];
        }
    }

    public class ActivityDayViewModel
    {
        public DateTime Date { get; set; }
        public string DayLabel { get; set; }
        public string WeekdayLabel { get; set; }
        public bool IsCompleted { get; set; }
        public string FillColor { get; set; }
    }

    public class ExerciseProgressCardViewModel
    {
        public string ExerciseName { get; set; }
        public string Summary { get; set; }
        public double LatestVolume { get; set; }
        public string AccentColor { get; set; }
        public ObservableCollection<ExerciseVolumePointViewModel> Points { get; set; }
    }

    public class ExerciseVolumePointViewModel
    {
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public double Volume { get; set; }
        public string VolumeLabel { get; set; }
        public double BarWidth { get; set; }
    }
}
