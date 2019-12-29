using System;
using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.DbModel.Types;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ScriptModel.Creators
{
    public static class SelectAllInfoCreator
    {
        #region Public Methods
        public static SelectAllInfo Create(ITableInfo tableInfo)
        {
            return new SelectAllInfo
            {
                Sql           = GetSql(tableInfo),
                SqlParameters = new List<ColumnInfo>()
            };
        }

        public static void WriteSql(ITableInfo tableInfo, PaddedStringBuilder sb, string topCountParameterName = null)
        {
            if (topCountParameterName == null)
            {
                sb.AppendLine("SELECT ");
            }
            else
            {
                sb.AppendLine($"SELECT TOP({topCountParameterName}) ");
            }

            sb.PaddingCount++;

            var columnNames = tableInfo.Columns.Select(c => "[" + c.ColumnName + "]");

            sb.AppendAll(string.Join("," + Environment.NewLine, columnNames));
            sb.AppendLine();

            sb.PaddingCount--;
            sb.AppendLine($"FROM [{tableInfo.SchemaName}].[{tableInfo.TableName}] WITH(NOLOCK)");
        }
        #endregion

        #region Methods
        static string GetSql(ITableInfo tableInfo)
        {
            var sb = new PaddedStringBuilder();

            WriteSql(tableInfo, sb);

            return sb.ToString();
        }
        #endregion
    }
}