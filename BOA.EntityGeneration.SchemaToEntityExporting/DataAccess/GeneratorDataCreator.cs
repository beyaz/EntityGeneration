using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.DbModel.Types;
using BOA.EntityGeneration.SchemaToEntityExporting.DataAccess.DbModelsImplementations;
using DotNetDatabaseAccessUtilities;
using ITableInfo = BOA.EntityGeneration.SchemaToEntityExporting.DbModels.ITableInfo;
using TableInfo = BOA.EntityGeneration.SchemaToEntityExporting.DataAccess.DbModelsImplementations.TableInfo;

namespace BOA.EntityGeneration.SchemaToEntityExporting.DataAccess
{
    /// <summary>
    ///     The generator data creator
    /// </summary>
    public class GeneratorDataCreator
    {
        #region Public Methods
        /// <summary>
        ///     Creates the specified table information.
        /// </summary>
        public static ITableInfo Create(string SqlSequenceInformationOfTable, IDatabase database, DbModel.Interfaces.ITableInfo tableInfo)
        {
            var uniqueIndexIdentifiers = tableInfo.IndexInfoList.Where(x => !x.IsPrimaryKey && x.IsUnique).ToList();

            var nonUniqueIndexIdentifiers = tableInfo.IndexInfoList.Where(x => !x.IsPrimaryKey && !x.IsUnique).ToList();

            var isSupportSelectByKey = tableInfo.PrimaryKeyColumns.Any();

            var isSupportSelectByIndex = nonUniqueIndexIdentifiers.Any();

            var SequenceList = new List<SequenceInfo>();
            if (SqlSequenceInformationOfTable == null)
            {
                SequenceList = new List<SequenceInfo>();
            }
            else
            {
                database.CommandText  = SqlSequenceInformationOfTable;
                database["schema"]    = tableInfo.SchemaName;
                database["tableName"] = tableInfo.TableName;

                SequenceList = database.ExecuteReader().ToList<SequenceInfo>().Where(x => tableInfo.Columns.Any(c => c.ColumnName == x.TargetColumnName)).ToList();
            }

            var data = new TableInfo
            {
                CatalogName            = tableInfo.CatalogName,
                Columns                = tableInfo.Columns.ToList().ConvertAll(ReEvaluate),
                HasIdentityColumn      = tableInfo.HasIdentityColumn,
                IdentityColumn         = tableInfo.IdentityColumn,
                IndexInfoList          = tableInfo.IndexInfoList,
                PrimaryKeyColumns      = tableInfo.PrimaryKeyColumns,
                SchemaName             = tableInfo.SchemaName,
                TableName              = tableInfo.TableName,
                UniqueIndexInfoList    = uniqueIndexIdentifiers,
                NonUniqueIndexInfoList = nonUniqueIndexIdentifiers,
                IsSupportSelectByKey   = isSupportSelectByKey,
                IsSupportSelectByIndex = isSupportSelectByIndex,
                SequenceList           = SequenceList
            };

            if (tableInfo.Columns.Any(x => x.ColumnName.Equals("VALID_FLAG", StringComparison.OrdinalIgnoreCase) && x.SqlDbType == SqlDbType.Char))
            {
                data.ShouldGenerateSelectAllByValidFlagMethodInBusinessClass = true;
            }

            return data;
        }
        #endregion

        #region Methods
        static IColumnInfo ReEvaluate(IColumnInfo columnInfo)
        {
            var item = ColumnInfo.CreateFrom(columnInfo);

            

            if (item.ColumnName.EndsWith("_FLAG", StringComparison.OrdinalIgnoreCase))
            {
                if (item.IsNullable)
                {
                    item.DotNetType      = DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetBool);
                    item.SqlReaderMethod = SqlReaderMethods.GetBooleanNullableValueFromChar;
                }
                else
                {
                    item.DotNetType      = DotNetTypeName.DotNetBool;
                    item.SqlReaderMethod = SqlReaderMethods.GetBooleanValueFromChar;
                }
            }

            return item;
        }
        #endregion
    }
}