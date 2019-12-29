using System.Collections.Generic;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.DbModel.SqlServerDataAccess
{
    public static class SchemaInfo
    {
        #region Public Methods
        public static List<string> GetAllTableNamesInSchema(IDatabase database, string schema)
        {
            var sql = $@"

SELECT DISTINCT(TABLE_NAME) AS TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = '{schema}'

";
            database.CommandText = sql;

            var tableNames = new List<string>();

            var reader = database.ExecuteReader();
            while (reader.Read())
            {
                tableNames.Add(reader["TABLE_NAME"].ToString());
            }

            reader.Close();

            return tableNames;
        }
        #endregion
    }
}