using DataBase.Connection;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataBase.Repository.Access
{
    public class AccessWeekPlanRepository : IWeekPlanRepository
    {
        private static readonly string[] DayNames =
        {
            "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
        };

        private readonly IDataBaseConnection _database;

        public AccessWeekPlanRepository() : this(DatabaseFilter.CreateConnection())
        {
        }

        public AccessWeekPlanRepository(IDataBaseConnection database)
        {
            _database = database ?? DatabaseFilter.CreateConnection();
        }

        public int CreateEmptyWeekPlan(int userId, string planName)
        {
            _database.ExecuteNonQuery("INSERT INTO WeekPlansTbl (UserId, PlanName) VALUES (?, ?)", userId, planName);
            System.Threading.Thread.Sleep(100);

            int weekPlanId = _database.ExecuteScalar<int>(
                "SELECT TOP 1 Id FROM WeekPlansTbl WHERE UserId = ? ORDER BY Id DESC",
                userId);

            foreach (string dayName in DayNames)
            {
                _database.ExecuteNonQuery(
                    "INSERT INTO WeekPlanDaysTbl (WeekPlanId, DayOfWeek, WorkoutId, RestDay) VALUES (?, ?, ?, ?)",
                    weekPlanId,
                    dayName,
                    DBNull.Value,
                    true);
            }

            _database.ExecuteNonQuery("UPDATE UserTbl SET CurrentWeekPlanId = ? WHERE Id = ?", weekPlanId, userId);
            return weekPlanId;
        }

        public int? GetUserWeekPlanId(int userId)
        {
            var currentPlanId = _database.ExecuteScalar<int?>(
                "SELECT CurrentWeekPlanId FROM UserTbl WHERE Id = ?",
                userId);

            if (currentPlanId.HasValue && currentPlanId.Value > 0)
            {
                return currentPlanId.Value;
            }

            var firstPlanId = _database.ExecuteScalar<int?>(
                "SELECT TOP 1 Id FROM WeekPlansTbl WHERE UserId = ? ORDER BY Id",
                userId);

            return firstPlanId.HasValue && firstPlanId.Value > 0 ? firstPlanId.Value : (int?)null;
        }

        public int? GetWeekPlanOwnerUserId(int weekPlanId)
        {
            var ownerId = _database.ExecuteScalar<int?>(
                "SELECT UserId FROM WeekPlansTbl WHERE Id = ?",
                weekPlanId);

            return ownerId.HasValue && ownerId.Value > 0 ? ownerId.Value : (int?)null;
        }

        public List<WeekPlanDay> GetWeekPlanDays(int weekPlanId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT wpd.Id, wpd.WeekPlanId, wpd.DayOfWeek, wpd.WorkoutId, wpd.RestDay, w.WorkoutName
                  FROM WeekPlanDaysTbl wpd
                  LEFT JOIN WorkoutsTbl w ON wpd.WorkoutId = w.Id
                  WHERE wpd.WeekPlanId = ?",
                weekPlanId);

            return dt.Rows.Cast<DataRow>()
                .Select(MapWeekPlanDay)
                .OrderBy(day => GetDaySortOrder(day.DayOfWeek))
                .ToList();
        }

        public WeekPlanDay GetWeekPlanDayById(int weekPlanDayId)
        {
            var dt = _database.ExecuteQuery(
                @"SELECT wpd.Id, wpd.WeekPlanId, wpd.DayOfWeek, wpd.WorkoutId, wpd.RestDay, w.WorkoutName
                  FROM WeekPlanDaysTbl wpd
                  LEFT JOIN WorkoutsTbl w ON wpd.WorkoutId = w.Id
                  WHERE wpd.Id = ?",
                weekPlanDayId);

            return dt.Rows.Count > 0 ? MapWeekPlanDay(dt.Rows[0]) : null;
        }

        public WeekPlanDay GetWeekPlanDayForDate(int userId, DateTime date)
        {
            int? weekPlanId = GetUserWeekPlanId(userId);
            if (!weekPlanId.HasValue)
            {
                return null;
            }

            string dayName = date.DayOfWeek.ToString();
            string legacyDayIndex = ((int)date.DayOfWeek).ToString();

            var dt = _database.ExecuteQuery(
                @"SELECT TOP 1 wpd.Id, wpd.WeekPlanId, wpd.DayOfWeek, wpd.WorkoutId, wpd.RestDay, w.WorkoutName
                  FROM WeekPlanDaysTbl wpd
                  LEFT JOIN WorkoutsTbl w ON wpd.WorkoutId = w.Id
                  WHERE wpd.WeekPlanId = ?
                    AND (wpd.DayOfWeek = ? OR wpd.DayOfWeek = ?)",
                weekPlanId.Value,
                dayName,
                legacyDayIndex);

            return dt.Rows.Count > 0 ? MapWeekPlanDay(dt.Rows[0]) : null;
        }

        public WeekPlanDay UpdateWeekPlanDayWorkout(int weekPlanDayId, int? workoutId, bool restDay)
        {
            _database.ExecuteNonQuery(
                "UPDATE WeekPlanDaysTbl SET WorkoutId = ?, RestDay = ? WHERE Id = ?",
                workoutId.HasValue ? (object)workoutId.Value : DBNull.Value,
                restDay,
                weekPlanDayId);

            return GetWeekPlanDayById(weekPlanDayId);
        }

        private WeekPlanDay MapWeekPlanDay(DataRow row)
        {
            string rawDay = row["DayOfWeek"]?.ToString();
            return new WeekPlanDay
            {
                Id = Convert.ToInt32(row["Id"]),
                WeekPlanId = Convert.ToInt32(row["WeekPlanId"]),
                DayOfWeek = NormalizeDayOfWeek(rawDay),
                WorkoutId = row["WorkoutId"] != DBNull.Value ? Convert.ToInt32(row["WorkoutId"]) : (int?)null,
                RestDay = row["RestDay"] != DBNull.Value && Convert.ToBoolean(row["RestDay"]),
                WorkoutName = row["WorkoutName"] != DBNull.Value ? row["WorkoutName"].ToString() : null
            };
        }

        private static string NormalizeDayOfWeek(string rawDay)
        {
            if (string.IsNullOrWhiteSpace(rawDay))
            {
                return DayNames[0];
            }

            if (int.TryParse(rawDay, out int numericDay) && numericDay >= 0 && numericDay < DayNames.Length)
            {
                return DayNames[numericDay];
            }

            return rawDay;
        }

        private static int GetDaySortOrder(string dayName)
        {
            int index = Array.IndexOf(DayNames, NormalizeDayOfWeek(dayName));
            return index < 0 ? int.MaxValue : index;
        }
    }
}
