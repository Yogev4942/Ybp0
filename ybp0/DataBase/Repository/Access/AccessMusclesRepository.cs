using DataBase.Connection;
using DataBase.Repository.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataBase.Repository.Access
{
    public class AccessMusclesRepository : IMuscleRepository
    {
        private readonly IDataBaseConnection _database;

        public AccessMusclesRepository() : this(DatabaseFilter.CreateConnection())
        {
        }

        public AccessMusclesRepository(IDataBaseConnection database)
        {
            _database = database ?? DatabaseFilter.CreateConnection();
        }

        public List<Muscle> GetAllMuscles()
        {
            string muscleTable = ExerciseSchemaHelper.GetMuscleTable(_database);
            if (string.IsNullOrWhiteSpace(muscleTable))
            {
                return new List<Muscle>();
            }

            string muscleNameColumn = ExerciseSchemaHelper.GetMuscleNameColumn(_database, muscleTable);
            string bodyRegionColumn = _database.ColumnExists(muscleTable, "BodyRegion") ? "BodyRegion" : null;
            string diagramZoneColumn = _database.ColumnExists(muscleTable, "DiagramZone") ? "DiagramZone" : null;

            string selectSql = $"SELECT Id, [{muscleNameColumn}] AS MuscleName";
            if (!string.IsNullOrWhiteSpace(bodyRegionColumn))
            {
                selectSql += $", [{bodyRegionColumn}] AS BodyRegion";
            }

            if (!string.IsNullOrWhiteSpace(diagramZoneColumn))
            {
                selectSql += $", [{diagramZoneColumn}] AS DiagramZone";
            }

            var dt = _database.ExecuteQuery($"{selectSql} FROM [{muscleTable}] ORDER BY [{muscleNameColumn}]");
            var muscles = new List<Muscle>();

            foreach (DataRow row in dt.Rows)
            {
                muscles.Add(new Muscle
                {
                    Id = Convert.ToInt32(row["Id"]),
                    MuscleName = row["MuscleName"]?.ToString(),
                    BodyRegion = dt.Columns.Contains("BodyRegion") ? row["BodyRegion"]?.ToString() : null,
                    DiagramZone = dt.Columns.Contains("DiagramZone") && row["DiagramZone"] != DBNull.Value
                        ? Convert.ToInt32(row["DiagramZone"])
                        : 0
                });
            }

            return muscles;
        }
    }
}
