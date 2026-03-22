namespace Models
{
    public class WeekPlan : BaseEntity
    {
        private int userId;
        private string planName;

        public int UserId { get => userId; set => userId = value; }
        public string PlanName { get => planName; set => planName = value; }
    }
}
