using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DataBase.Connection
{
    internal interface IDataBaseConnection
    {
        DataTable ExecuteQuery(string query, params object[] parameters);
        int ExecuteNonQuery(string query, params object[] parameters);
        T ExecuteScalar<T>(string query, params object[] parameters);
    }
}
