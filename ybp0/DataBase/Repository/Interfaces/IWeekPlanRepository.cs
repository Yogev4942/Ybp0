using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface IWeekPlanRepository
    {
        int CreateEmptyWeekPlan(int userId, string planName);
        int? GetUserWeekPlanId(int userId);
        int? GetWeekPlanOwnerUserId(int weekPlanId);
        DataTable GetWeekPlanDays(int weekPlanId);
    }
}
