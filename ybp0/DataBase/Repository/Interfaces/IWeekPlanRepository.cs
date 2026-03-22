using Models;
using System;
using System.Collections.Generic;

namespace DataBase.Repository.Interfaces
{
    public interface IWeekPlanRepository
    {
        int CreateEmptyWeekPlan(int userId, string planName);
        int? GetUserWeekPlanId(int userId);
        int? GetWeekPlanOwnerUserId(int weekPlanId);
        List<WeekPlanDay> GetWeekPlanDays(int weekPlanId);
        WeekPlanDay GetWeekPlanDayById(int weekPlanDayId);
        WeekPlanDay GetWeekPlanDayForDate(int userId, DateTime date);
        WeekPlanDay UpdateWeekPlanDayWorkout(int weekPlanDayId, int? workoutId, bool restDay);
    }
}
