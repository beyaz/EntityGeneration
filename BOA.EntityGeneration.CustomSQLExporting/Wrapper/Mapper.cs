using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain;

namespace BOA.EntityGeneration.CustomSQLExporting.Wrapper
{
    static class Mapper
    {
        #region Public Methods
        public static DatabaseReader CreateDatabaseReader(this Context source)
        {
            return new DatabaseReader
            {
                Connection = source.Connection,
                ProfileId  = source.Input.ProfileId,
                ObjectId   = source.CurrentObjectId
            };
        }
        #endregion
    }
}