using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.DbModel.SqlServerDataAccess
{
    public static class SequenceInfo
    {
        #region Public Methods
        public static bool HasSequenceLike(this IDatabase Database, string schema, string sequenceName)
        {
            Database.CommandText = $"SELECT TOP 1 1 FROM sys.SEQUENCES WHERE name = '{sequenceName}' AND SCHEMA_NAME(schema_id) = '{schema}'";

            return Database.ExecuteScalar() != null;
        }
        #endregion
    }
}