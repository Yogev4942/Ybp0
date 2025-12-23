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

        public CalendarViewModel(IDatabaseService dbService,INavigationService navigationService)
        {
            _dbService = dbService;
            _navService = navigationService;
            Days = new ObservableCollection<DayViewModel>();
        }

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
