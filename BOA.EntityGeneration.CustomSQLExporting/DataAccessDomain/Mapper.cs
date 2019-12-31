using BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain;
using CustomSqlInfo = BOA.EntityGeneration.CustomSQLExporting.Models.CustomSqlInfo;
using CustomSqlInfoResult = BOA.EntityGeneration.CustomSQLExporting.Models.CustomSqlInfoResult;

namespace BOA.EntityGeneration.CustomSQLExporting.DataAccessDomain
{
    static class Mapper
    {
        #region Public Methods
        public static DatabaseReader CreateDatabaseReader(this ProjectCustomSqlInfoDataAccess source)
        {
            return new DatabaseReader
            {
                Connection = source.Connection,
                ProfileId  = source.ProfileId,
                ObjectId   = source.ObjectId
            };
        }

        public static CustomSqlInfo ToCustomSqlInfo(this DatabaseAccessDomain.CustomSqlInfo source)
        {
            return new CustomSqlInfo
            {
                Name                  = source.Name,
                ProfileId             = source.ProfileId,
                SchemaName            = source.SchemaName,
                Sql                   = source.Sql,
                SqlResultIsCollection = source.SqlResultIsCollection
            };
        }

        public static CustomSqlInfoResult ToCustomSqlInfoResult(this DatabaseAccessDomain.CustomSqlInfoResult source)
        {
            return new CustomSqlInfoResult
            {
                DataType   = source.DataType,
                IsNullable = source.IsNullable,
                Name       = source.Name
            };
        }
        #endregion
    }
}