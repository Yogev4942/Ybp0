using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Mappers
{
    public static class UserMapper
    {
        public static User MapBaseUser(DataRow row)
        {
            // Determine the concrete type from the database flag
            bool isTrainer = row["IsTrainer"] != DBNull.Value && Convert.ToBoolean(row["IsTrainer"]);

            // Since User is abstract, instantiate the specific derived class
            User user = isTrainer ? (User)new Trainer() : (User)new Trainee();

            PopulateBaseProperties(user, row);
            return user;
        }

        public static Trainer MapTrainer(DataRow row)
        {
            var trainer = new Trainer();
            PopulateBaseProperties(trainer, row);

            trainer.Specialization = row["Specialization"]?.ToString();
            trainer.HourlyRate = row["HourlyRate"] != DBNull.Value ? Convert.ToDouble(row["HourlyRate"]) : 0;
            trainer.MaxTrainees = row["MaxTrainees"] != DBNull.Value ? Convert.ToInt32(row["MaxTrainees"]) : 10;
            trainer.TotalTrainees = row["TotalTrainees"] != DBNull.Value ? Convert.ToInt32(row["TotalTrainees"]) : 0;
            trainer.Rating = row["Rating"] != DBNull.Value ? Convert.ToDouble(row["Rating"]) : 0;
            trainer.TotalRatings = row["TotalRatings"] != DBNull.Value ? Convert.ToInt32(row["TotalRatings"]) : 0;

            return trainer;
        }

        public static Trainee MapTrainee(DataRow row)
        {
            var trainee = new Trainee();
            PopulateBaseProperties(trainee, row);

            trainee.TrainerId = row["TrainerId"] != DBNull.Value ? Convert.ToInt32(row["TrainerId"]) : (int?)null;
            trainee.FitnessGoal = row["FitnessGoal"]?.ToString();
            trainee.CurrentWeight = row["CurrentWeight"] != DBNull.Value ? Convert.ToDouble(row["CurrentWeight"]) : 0;
            trainee.Height = row["Height"] != DBNull.Value ? Convert.ToDouble(row["Height"]) : 0;

            return trainee;
        }

        private static void PopulateBaseProperties(User user, DataRow row)
        {
            user.Id = Convert.ToInt32(row["Id"]);
            user.Username = row["Username"].ToString();
            user.Email = row["Email"]?.ToString();
            user.Password = row["Password"].ToString();
            user.Joindate = row["JoinDate"]?.ToString();
            user.Bio = row["Bio"]?.ToString();
            user.Gender = row["Gender"]?.ToString();
            user.IsTrainer = row["IsTrainer"] != DBNull.Value && Convert.ToBoolean(row["IsTrainer"]);
            user.CurrentWeekPlanId = row["CurrentWeekPlanId"] != DBNull.Value ? Convert.ToInt32(row["CurrentWeekPlanId"]) : 0;
        }
    }
}
