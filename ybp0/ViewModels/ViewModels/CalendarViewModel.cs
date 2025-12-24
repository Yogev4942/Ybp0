using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly INavigationService _navService;
        private readonly User _currUser;
        private int _userId;
        private int _weekPlanId;
        private ObservableCollection<DayViewModel> _days;

        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public int WeekPlanId
        {
            get => _weekPlanId;
            set => SetProperty(ref _weekPlanId, value);
        }

        public ObservableCollection<DayViewModel> Days
        {
            get => _days;
            set => SetProperty(ref _days, value);
        }

        public CalendarViewModel(IDatabaseService dbService,INavigationService navigationService,User user)
        {
            _dbService = dbService;
            _navService = navigationService;
            _currUser = user;
            Days = new ObservableCollection<DayViewModel>();

            // ================================================
            // TODO: REMOVE THIS SAMPLE DATA WHEN CONNECTING TO DATABASE
            // Call LoadWeekPlan(userId, weekPlanId) instead
            LoadSampleData();
            // ================================================
        }

        // ================================================
        // SAMPLE DATA - DELETE THIS METHOD WHEN READY FOR DATABASE
        // ================================================
        private void LoadSampleData()
        {
            string[] dayNames = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
            string[] accentColors = { "#FF6B6B", "#26A69A", "#66BB6A", "#42A5F5", "#AB47BC", "#FF9800", "#7E57C2" };
            
            // Sample workout names (index 0 = Sunday rest day)
            string[] workoutNames = { "Rest Day", "Chest & Triceps", "Back & Biceps", "Legs", "Shoulders", "Full Body", "Cardio & Core" };
            bool[] restDays = { true, false, false, false, false, false, false };

            // Sample exercises per day
            string[][] sampleExercises = new string[][]
            {
                new string[] { },  // Sunday - rest
                new string[] { "Bench Press", "Dumbbell Flyes", "Tricep Dips" },
                new string[] { "Pull-ups", "Barbell Rows", "Bicep Curls" },
                new string[] { "Squats", "Leg Press", "Calf Raises" },
                new string[] { "Overhead Press", "Lateral Raises", "Face Pulls" },
                new string[] { "Deadlifts", "Push-ups", "Planks" },
                new string[] { "Running", "Ab Crunches", "Leg Raises" }
            };

            for (int i = 0; i < 7; i++)
            {
                var dayVm = new DayViewModel(_dbService, 1)
                {
                    DayName = dayNames[i],
                    Date = DateTime.Today.AddDays(i - (int)DateTime.Today.DayOfWeek),
                    WorkoutName = workoutNames[i],
                    IsRestDay = restDays[i],
                    AccentColor = accentColors[i]
                };

                // Add sample exercises
                if (!restDays[i])
                {
                    foreach (var exerciseName in sampleExercises[i])
                    {
                        var exerciseVm = new SampleExerciseViewModel(exerciseName, accentColors[i]);
                        dayVm.Exercises.Add(exerciseVm);
                    }
                }

                Days.Add(dayVm);
            }
        }

        // Simple sample exercise class - DELETE WHEN USING REAL DATABASE
        private class SampleExerciseViewModel : ExerciseViewModel
        {
            public SampleExerciseViewModel(string name, string color) 
                : base(null, 0, new Models.Exercise { Id = 0, ExerciseName = name }, color)
            {
                Sets = new ObservableCollection<SetViewModel>();
            }
        }
        // ================================================
        // END SAMPLE DATA
        // ================================================

        public void LoadWeekPlan(int userId, int weekPlanId)
        {
            UserId = userId;
            WeekPlanId = weekPlanId;

            Days.Clear();

            // Day names in order (Sunday first)
            string[] dayNames = { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };

            // Load week plan days from database
            var weekPlanDays = GetWeekPlanDays(weekPlanId);

            DateTime baseDate = DateTime.Today;
            int currentDayOfWeek = (int)baseDate.DayOfWeek; // 0 = Sunday

            for (int i = 0; i < 7; i++)
            {
                var dayVm = new DayViewModel(_dbService, userId)
                {
                    DayName = dayNames[i],
                    Date = baseDate.AddDays(i - currentDayOfWeek) // Calculate date for this day
                };

                // Find matching week plan day
                var planDay = weekPlanDays.FirstOrDefault(d => d.DayOfWeek == i);
                
                if (planDay != null)
                {
                    dayVm.WeekPlanDayId = planDay.Id;
                    dayVm.IsRestDay = planDay.IsRestDay;
                    dayVm.WorkoutName = planDay.WorkoutName;

                    if (!planDay.IsRestDay && planDay.WorkoutId.HasValue)
                    {
                        // Load workout for this day
                        dayVm.LoadWorkoutForDay();
                    }
                }
                else
                {
                    dayVm.IsRestDay = true;
                    dayVm.WorkoutName = "No Workout";
                }

                Days.Add(dayVm);
            }
        }

        private List<WeekPlanDayData> GetWeekPlanDays(int weekPlanId)
        {
            var dt = _dbService.GetWeekPlanDays(weekPlanId);
            
            var days = new List<WeekPlanDayData>();
            if (dt == null) return days;

            foreach (System.Data.DataRow row in dt.Rows)
            {
                days.Add(new WeekPlanDayData
                {
                    Id = Convert.ToInt32(row["Id"]),
                    DayOfWeek = Convert.ToInt32(row["DayOfWeek"]),
                    WorkoutId = row["WorkoutId"] != DBNull.Value ? (int?)Convert.ToInt32(row["WorkoutId"]) : null,
                    IsRestDay = Convert.ToBoolean(row["RestDay"]),
                    WorkoutName = row["WorkoutName"]?.ToString() ?? "Rest Day"
                });
            }

            return days;
        }

        private class WeekPlanDayData
        {
            public int Id { get; set; }
            public int DayOfWeek { get; set; }
            public int? WorkoutId { get; set; }
            public bool IsRestDay { get; set; }
            public string WorkoutName { get; set; }
        }
    }
}
