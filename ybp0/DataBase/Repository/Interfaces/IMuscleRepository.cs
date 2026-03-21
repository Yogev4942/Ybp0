using Models;
using System.Collections.Generic;

namespace DataBase.Repository.Interfaces
{
    public interface IMuscleRepository
    {
        List<Muscle> GetAllMuscles();
    }
}
