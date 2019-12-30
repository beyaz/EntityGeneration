using System.Data;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.SharedFileExporting
{
    static class ParameterHelper
    {
        #region Public Methods
        public static string GetValueForSqlInfoParameter(IColumnInfo columnInfo, string contractVariableName = "contract")
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

            return $"{contractVariableName}.{columnInfo.ColumnName.ToContractName()}";
        }
        #endregion
    }
}