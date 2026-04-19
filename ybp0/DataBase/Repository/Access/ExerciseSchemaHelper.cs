using System;
using DataBase.Connection;

namespace DataBase.Repository.Access
{
    internal static class ExerciseSchemaHelper
    {
        internal static string GetExerciseTable(IDataBaseConnection database)
        {
            if (database.TableExists("ExerciseTbl")) return "ExerciseTbl";
            if (database.TableExists("ExercisesTbl")) return "ExercisesTbl";
            return "ExercisesTbl";
        }

        internal static string GetMuscleTable(IDataBaseConnection database)
        {
            if (database.TableExists("MuscleTbl")) return "MuscleTbl";
            if (database.TableExists("MusclesTbl")) return "MusclesTbl";
            return null;
        }

        internal static string GetExerciseNameColumn(IDataBaseConnection database, string exerciseTable)
        {
            if (database.ColumnExists(exerciseTable, "ExerciseName")) return "ExerciseName";
            if (database.ColumnExists(exerciseTable, "Name")) return "Name";
            return "ExerciseName";
        }

        internal static string GetMuscleNameColumn(IDataBaseConnection database, string muscleTable)
        {
            if (string.IsNullOrWhiteSpace(muscleTable)) return null;
            if (database.ColumnExists(muscleTable, "MuscleName")) return "MuscleName";
            if (database.ColumnExists(muscleTable, "Name")) return "Name";
            return "MuscleName";
        }

        internal static string GetExerciseMuscleForeignKeyColumn(IDataBaseConnection database, string exerciseTable)
        {
            if (database.ColumnExists(exerciseTable, "PrimaryMuscle")) return "PrimaryMuscle";
            if (database.ColumnExists(exerciseTable, "PrimaryMuscleId")) return "PrimaryMuscleId";
            if (database.ColumnExists(exerciseTable, "MuscleId")) return "MuscleId";
            if (database.ColumnExists(exerciseTable, "MuscleTblId")) return "MuscleTblId";
            if (database.ColumnExists(exerciseTable, "MusclesTblId")) return "MusclesTblId";
            return null;
        }

        internal static string GetExerciseSecondaryMuscleForeignKeyColumn(IDataBaseConnection database, string exerciseTable)
        {
            if (database.ColumnExists(exerciseTable, "SecondaryMuscle")) return "SecondaryMuscle";
            if (database.ColumnExists(exerciseTable, "SecondaryMuscleId")) return "SecondaryMuscleId";
            return null;
        }

        internal static bool HasLegacyMuscleGroupColumn(IDataBaseConnection database, string exerciseTable)
        {
            return database.ColumnExists(exerciseTable, "MuscleGroup");
        }

        internal static string BuildExerciseProjectionSql(IDataBaseConnection database, string exerciseAlias, string joinAlias = "m", string secondaryJoinAlias = "sm")
        {
            string exerciseTable = GetExerciseTable(database);
            string muscleTable = GetMuscleTable(database);
            string exerciseNameColumn = GetExerciseNameColumn(database, exerciseTable);
            string muscleNameColumn = GetMuscleNameColumn(database, muscleTable);
            string primaryKey = GetExerciseMuscleForeignKeyColumn(database, exerciseTable);
            string secondaryKey = GetExerciseSecondaryMuscleForeignKeyColumn(database, exerciseTable);

            string sql = $"{exerciseAlias}.Id, {exerciseAlias}.[{exerciseNameColumn}] AS ExerciseName";

            // Primary Muscle
            if (!string.IsNullOrWhiteSpace(muscleTable) && !string.IsNullOrWhiteSpace(muscleNameColumn) && !string.IsNullOrWhiteSpace(primaryKey))
            {
                sql += $", {joinAlias}.[{muscleNameColumn}] AS MuscleGroup";
            }
            else if (HasLegacyMuscleGroupColumn(database, exerciseTable))
            {
                sql += $", {exerciseAlias}.[MuscleGroup] AS MuscleGroup";
            }
            else
            {
                sql += ", '' AS MuscleGroup";
            }

            // Secondary Muscle
            if (!string.IsNullOrWhiteSpace(muscleTable) && !string.IsNullOrWhiteSpace(muscleNameColumn) && !string.IsNullOrWhiteSpace(secondaryKey))
            {
                sql += $", {secondaryJoinAlias}.[{muscleNameColumn}] AS SecondaryMuscleGroup";
            }
            else
            {
                sql += ", '' AS SecondaryMuscleGroup";
            }

            return sql;
        }

        internal static System.Collections.Generic.List<string> BuildExerciseJoinSql(IDataBaseConnection database, string exerciseAlias, string joinAlias = "m", string secondaryJoinAlias = "sm")
        {
            var joins = new System.Collections.Generic.List<string>();
            string exerciseTable = GetExerciseTable(database);
            string muscleTable = GetMuscleTable(database);
            string primaryKey = GetExerciseMuscleForeignKeyColumn(database, exerciseTable);
            string secondaryKey = GetExerciseSecondaryMuscleForeignKeyColumn(database, exerciseTable);

            if (!string.IsNullOrWhiteSpace(muscleTable) && !string.IsNullOrWhiteSpace(primaryKey))
            {
                joins.Add($" LEFT JOIN [{muscleTable}] {joinAlias} ON {exerciseAlias}.[{primaryKey}] = {joinAlias}.Id ");
            }

            if (!string.IsNullOrWhiteSpace(muscleTable) && !string.IsNullOrWhiteSpace(secondaryKey))
            {
                joins.Add($" LEFT JOIN [{muscleTable}] {secondaryJoinAlias} ON {exerciseAlias}.[{secondaryKey}] = {secondaryJoinAlias}.Id ");
            }

            return joins;
        }
    }
}
