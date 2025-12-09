using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class WeekPlans : BaseEntity
    {
        private int userId;
        private string planname;

        public int UserId { get => userId; set => userId = value; }
        public string PlanName { get => planname; set => planname = value; }
    }
}
