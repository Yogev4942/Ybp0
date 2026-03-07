using DataBase.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Access
{
    public class AccessWeekPlanRepository : IWeekPlanRepository
    {
        private readonly AccessDatabaseConnection _database;

        public AccessWeekPlanRepository()
        {
            _database = new AccessDatabaseConnection();
        }

        public int CreateEmptyWeekPlan(int userId, string planName)
        {
            _database.ExecuteNonQuery("INSERT INTO WeekPlansTbl (UserId, PlanName) VALUES (?, ?)", userId, planName);

            System.Threading.Thread.Sleep(100);

            var weekPlanDt = _database.ExecuteQuery("SELECT Id FROM WeekPlansTbl WHERE UserId = ? AND PlanName = ?", userId, planName);
            if (weekPlanDt.Rows.Count == 0) return 0;

            int weekPlanId = Convert.ToInt32(weekPlanDt.Rows[0]["Id"]);

            for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
            {
                _database.ExecuteNonQuery(
                    "INSERT INTO WeekPlanDaysTbl (WeekplanId, DayOfWeek, WorkoutId, RestDay) VALUES (?, ?, NULL, ?)",
                    weekPlanId, dayOfWeek, false
                );
            }

            // Update the User's CurrentWeekPlanId to the newly created plan
            _database.ExecuteNonQuery("UPDATE UserTbl SET CurrentWeekPlanId = ? WHERE Id = ?", weekPlanId, userId);

            return weekPlanId;
        }

        public int? GetUserWeekPlanId(int userId)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM WeekPlansTbl WHERE UserId = ?", userId);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Id"]) : (int?)null;
        }

        public int? GetWeekPlanOwnerUserId(int weekPlanId)
        {
            var dt = _database.ExecuteQuery("SELECT Id FROM UserTbl WHERE CurrentWeekPlanId = ?", weekPlanId);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Id"]) : (int?)null;
        }

        public DataTable GetWeekPlanDays(int weekPlanId)
        {
            return _database.ExecuteQuery(
                @"SELECT wpd.Id, wpd.DayOfWeek, wpd.WorkoutId, wpd.RestDay, w.WorkoutName
                  FROM WeekPlanDaysTbl wpd
                  LEFT JOIN WorkoutsTbl w ON wpd.WorkoutId = w.Id
                  WHERE wpd.WeekPlanId = ?", weekPlanId
            );
        }
    }
}
