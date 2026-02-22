using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Interfaces
{
    public interface IExerciseRepository
    {
        List<Exercise> GetAllExercises();
    }
}
