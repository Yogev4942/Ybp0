using DataBase.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Repository.Access
{
    public class AccessUserRepository : IUserRepository
    {
        private readonly IDbConnection _database;

    }
}
