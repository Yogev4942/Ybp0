using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DataTransferObjects
{
    internal class TrainerData
    {
        int id;
        int userid;
        string specialization;
        int hourlyRate;
        int maxTrainees;
        int totalTrainers;
        int rating;
        int totalRatings;

        public int Id { get => id; set => id = value; }
        public int Userid { get => userid; set => userid = value; }
        public string Specialization { get => specialization; set => specialization = value; }
        public int HourlyRate { get => hourlyRate; set => hourlyRate = value; }
        public int MaxTrainees { get => maxTrainees; set => maxTrainees = value; }
        public int TotalTrainers { get => totalTrainers; set => totalTrainers = value; }
        public int Rating { get => rating; set => rating = value; }
        public int TotalRatings { get => totalRatings; set => totalRatings = value; }
    }
}
