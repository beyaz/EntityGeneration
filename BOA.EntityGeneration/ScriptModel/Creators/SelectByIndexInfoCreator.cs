using System;
using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.DbModel.Interfaces;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ScriptModel.Creators
{
    public static class SelectByIndexInfoCreator
    {
        #region Public Methods
        public static SelectByIndexInfo Create(ITableInfo tableInfo, IIndexInfo indexInfo)
        {
            var sqlParameters = tableInfo.Columns.Where(x => indexInfo.ColumnNames.Contains(x.ColumnName)).ToList();

            return new SelectByIndexInfo
            {
                Sql           = GetSql(tableInfo, sqlParameters),
                SqlParameters = sqlParameters
            };
        }
        #endregion

        #region Methods
        static string GetSql(ITableInfo tableInfo, IReadOnlyList<IColumnInfo> sqlParameters)
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

            sb.AppendAll(string.Join(" AND " + Environment.NewLine, sqlParameters.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}")));
            sb.AppendLine();
            sb.PaddingCount--;

            return sb.ToString();
        }
        #endregion
    }
}