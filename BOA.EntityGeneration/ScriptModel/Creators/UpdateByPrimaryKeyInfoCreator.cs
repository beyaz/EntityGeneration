using System;
using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.DbModel.Interfaces;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ScriptModel.Creators
{
    public static class UpdateByPrimaryKeyInfoCreator
    {
        #region Public Methods
        public static UpdateByPrimaryKeyInfo Create(ITableInfo tableInfo)
        {
            var whereParameters = tableInfo.PrimaryKeyColumns;

            var columnsWillBeUpdate = GetColumnsWillBeUpdate(tableInfo);

            return new UpdateByPrimaryKeyInfo
            {
                WhereParameters     = whereParameters,
                ColumnsWillBeUpdate = columnsWillBeUpdate,
                Sql                 = GetSql(tableInfo, columnsWillBeUpdate, whereParameters),
                SqlParameters       = columnsWillBeUpdate.Union(whereParameters).ToList()
            };
        }
        #endregion

        #region Methods
        static IReadOnlyList<IColumnInfo> GetColumnsWillBeUpdate(ITableInfo tableInfo)
        {
            var excludedColumnNames = tableInfo.PrimaryKeyColumns.Select(x => x.ColumnName).ToList();

            excludedColumnNames.Add(Names2.INSERT_DATE);
            excludedColumnNames.Add(Names2.INSERT_TOKEN_ID);
            excludedColumnNames.Add(Names2.INSERT_USER_ID);
            excludedColumnNames.Add(Names2.ROW_GUID);
            excludedColumnNames.Add(Names2.INSERT_USER_ID);

            return tableInfo.Columns.Where(c => !excludedColumnNames.Contains(c.ColumnName)).ToList();
        }

        static string GetSql(ITableInfo tableInfo, IReadOnlyList<IColumnInfo> columnsWillBeUpdate, IReadOnlyList<IColumnInfo> whereParameters)
        {
            var sb = new PaddedStringBuilder();

            sb.AppendLine($"UPDATE [{tableInfo.SchemaName}].[{tableInfo.TableName}] SET");
            sb.PaddingCount++;

            sb.AppendAll(string.Join("," + Environment.NewLine, columnsWillBeUpdate.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}")));
            sb.AppendLine();

            sb.PaddingCount--;
            sb.AppendLine("WHERE");
            sb.PaddingCount++;

            sb.AppendAll(string.Join(" AND " + Environment.NewLine, whereParameters.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}")));
            sb.AppendLine();

            sb.PaddingCount--;

            return sb.ToString();
        }
        #endregion
    }
}