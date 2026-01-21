using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    /// <summary>
    /// Data Transfer Object for UserTbl - mirrors database structure
    /// </summary>
    public class UserData
    {
        public int Id {  get; set; }
        public string Email { get; set; }
        public string Username {  get; set; } 
        public string Password {  get; set; }
        public string JoinDate {  get; set; }
        public bool IsTrainer {  get; set; }
        public string Bio {  get; set; }
        public string Gender { get; set; }
        public int CurrentWeekPlanId {  get; set; }
    }
}
