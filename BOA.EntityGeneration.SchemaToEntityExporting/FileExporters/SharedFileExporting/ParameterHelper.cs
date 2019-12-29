using System.Data;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.SharedFileExporting
{
    static class ParameterHelper
    {
        #region Public Methods
        public static string GetValueForSqlInsert(IColumnInfo columnInfo, string contractVariableName = "contract")
        {
            //if (columnInfo.ColumnName == Names2.ROW_GUID)
            //{
            //    return $"{@"Guid.NewGuid().ToString().ToUpper(new System.Globalization.CultureInfo(""en-US"", false))"}";
            //}

            if (columnInfo.SqlDbType == SqlDbType.Char &&
                columnInfo.DotNetType == DotNetTypeName.DotNetBool)
            {
                return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()} ? \"1\" : \"0\"";
            }

            if (columnInfo.SqlDbType == SqlDbType.Char &&
                columnInfo.DotNetType == DotNetTypeName.DotNetBoolNullable)
            {
                return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()} == true ? \"1\" : \"0\"";
            }

            //if (columnInfo.ColumnName == Names2.INSERT_DATE || columnInfo.ColumnName == Names2.UPDATE_DATE)
            //{
            //    return "DateTime.Now";
            //}

            //if (columnInfo.ColumnName == Names2.INSERT_USER_ID || columnInfo.ColumnName == Names2.UPDATE_USER_ID)
            //{
            //    return "Context.ApplicationContext.Authentication.UserName";
            //}

            //if (columnInfo.ColumnName == Names2.INSERT_TOKEN_ID || columnInfo.ColumnName == Names2.UPDATE_TOKEN_ID)
            //{
            //    return "Convert.ToString(Context.EngineContext.MainBusinessKey)";
            //}

            return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()}";
        }

        public static string GetValueForSqlUpdate(IColumnInfo columnInfo, string contractVariableName = "contract")
        {
            if (columnInfo.SqlDbType == SqlDbType.Char &&
                columnInfo.DotNetType == DotNetTypeName.DotNetBool)
            {
                return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()} ? \"1\" : \"0\"";
            }

            if (columnInfo.SqlDbType == SqlDbType.Char &&
                columnInfo.DotNetType == DotNetTypeName.DotNetBoolNullable)
            {
                return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()} == true ? \"1\" : \"0\"";
            }

            //if (columnInfo.ColumnName == Names2.INSERT_DATE || columnInfo.ColumnName == Names2.UPDATE_DATE)
            //{
            //    return "DateTime.Now";
            //}

            //if (columnInfo.ColumnName == Names2.INSERT_USER_ID || columnInfo.ColumnName == Names2.UPDATE_USER_ID)
            //{
            //    return "Context.ApplicationContext.Authentication.UserName";
            //}

            //if (columnInfo.ColumnName == Names2.INSERT_TOKEN_ID || columnInfo.ColumnName == Names2.UPDATE_TOKEN_ID)
            //{
            //    return "Convert.ToString(Context.EngineContext.MainBusinessKey)";
            //}

            return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()}";
        }
        #endregion
    }
}