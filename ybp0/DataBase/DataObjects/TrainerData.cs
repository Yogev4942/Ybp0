using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    public class TrainerData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; }
        public double HourlyRate { get; set; }
        public int MaxTrainees { get; set; }
        public int TotalTrainees { get; set; }
        public double Rating { get; set; }
        public int TotalRatings { get; set; }
    }
}
