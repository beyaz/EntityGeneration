using System;
using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.DbModel.Interfaces;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ScriptModel.Creators
{
    /// <summary>
    ///     The select by primary key information creator
    /// </summary>
    public static class SelectByPrimaryKeyInfoCreator
    {
        #region Public Methods
        /// <summary>
        ///     Creates the specified table information.
        /// </summary>
        public static ISelectByPrimaryKeyInfo Create(ITableInfo tableInfo)
        {
            var parameters = tableInfo.PrimaryKeyColumns;

            return new SelectByPrimaryKeyInfo
            {
                Sql           = GetSql(tableInfo, parameters),
                SqlParameters = parameters
            };
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the SQL.
        /// </summary>
        static string GetSql(ITableInfo tableInfo, IReadOnlyList<IColumnInfo> parameters)
        {
            var sb = new PaddedStringBuilder();

            sb.AppendLine("SELECT");
            sb.PaddingCount++;

            sb.AppendAll(string.Join("," + Environment.NewLine, tableInfo.Columns.Select(c => $"[{c.ColumnName}]")));
            sb.AppendLine();
            sb.AppendLine($"FROM [{tableInfo.SchemaName}].[{tableInfo.TableName}] WITH (NOLOCK)");
            sb.PaddingCount--;
            sb.AppendLine("WHERE");
            sb.PaddingCount++;

            sb.AppendAll(string.Join(" AND " + Environment.NewLine, parameters.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}")));
            sb.AppendLine();

            sb.PaddingCount--;

            return sb.ToString();
        }
        #endregion
    }
}