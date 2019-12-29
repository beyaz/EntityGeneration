using System.Linq;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.ScriptModel.Creators
{
    /// <summary>
    ///     The delete information creator
    /// </summary>
    public static class DeleteInfoCreator
    {
        #region Public Methods
        /// <summary>
        ///     Creates the specified table information.
        /// </summary>
        public static IDeleteInfo Create(ITableInfo tableInfo)
        {
            var parameters = tableInfo.PrimaryKeyColumns;

            return new DeleteInfo
            {
                Sql           = $"DELETE FROM [{tableInfo.SchemaName}].[{tableInfo.TableName}] WHERE {string.Join(" AND ", parameters.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}"))}",
                SqlParameters = parameters
            };
        }
        #endregion
    }
}