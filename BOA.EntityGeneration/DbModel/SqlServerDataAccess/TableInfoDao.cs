using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using BOA.EntityGeneration.DbModel.Types;
using DotNetDatabaseAccessUtilities;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.DbModel.SqlServerDataAccess
{
    public class TableInfoDao
    {
        #region Static Fields
        static readonly SqlDbType[] AlreadySame =
        {
            SqlDbType.SmallDateTime,
            SqlDbType.TinyInt,
            SqlDbType.DateTime,
            SqlDbType.Date,
            SqlDbType.SmallInt,
            SqlDbType.Int,
            SqlDbType.Bit,
            SqlDbType.BigInt,
            SqlDbType.Money,
            SqlDbType.UniqueIdentifier,
            SqlDbType.Float,
            SqlDbType.SmallMoney,
            SqlDbType.Image,
            SqlDbType.VarBinary,
            SqlDbType.Binary,
            SqlDbType.Real,
            SqlDbType.NText,
            SqlDbType.Text,
            SqlDbType.Time,
            SqlDbType.Xml,
            SqlDbType.Timestamp
        };
        #endregion

        #region Public Properties
        public IDatabase       Database        { get; set; }
        public IndexInfoAccess IndexInfoAccess { get; set; }
        #endregion

        #region Public Methods
        public TableInfo GetInfo(string tableCatalog, string schema, string tableName)
        {
            if (!TableExists(schema, tableName))
            {
                throw new ArgumentException("TableNotFoundInDatabase:" + tableName);
            }

            var table = GetTableInformation(tableCatalog, schema, tableName);

            if (table.Rows.Count <= 0)
            {
                throw new ArgumentException("TableNotFoundInDatabase:" + tableName);
            }

            var columns = CreateColumns(table);

            var primaryKeyColumns = GetPrimaryColumns(schema, tableName);

            foreach (DataRow r in primaryKeyColumns.Rows)
            {
                var indexName = r[0].ToString().Trim();

                (from c in columns where c.ColumnName == indexName select c).First().IsPrimaryKey = true;
            }

            foreach (var c in columns)
            {
                c.Comment = GetColumnComment(schema, tableName, c.ColumnName);
            }

            return new TableInfo
            {
                CatalogName       = tableCatalog,
                SchemaName        = schema,
                TableName         = tableName,
                Columns           = columns,
                IdentityColumn    = columns.FirstOrDefault(c => c.IsIdentity),
                HasIdentityColumn = columns.Any(c => c.IsIdentity),
                PrimaryKeyColumns = columns.Where(c => c.IsPrimaryKey).ToList(),
                IndexInfoList     = IndexInfoAccess.GetIndexInformation(schema, tableName)
            };
        }
        #endregion

        #region Methods
        static ColumnInfo CreateColumn(DataRow row)
        {
            var columnName = row["COLUMN_NAME"].ToString();

            #region var dataType = row["DATA_TYPE"
            var dataType = row["DATA_TYPE"].ToString().ToUpperEN();

            dataType = ReadDataType(row, dataType, columnName);
            #endregion

            var isIdentity = row["IsIdentity"] + "" == "True";

            var isNullable = row["IS_NULLABLE"].ToString() == "YES";

            return new ColumnInfo
            {
                ColumnName      = columnName,
                DataType        = dataType,
                IsIdentity      = isIdentity,
                IsNullable      = isNullable,
                SqlDbType       = SqlDbTypeMap.GetSqlDbType(dataType),
                DotNetType      = SqlDbTypeMap.GetDotNetType(dataType, isNullable),
                SqlReaderMethod = SqlDbTypeMap.GetSqlReaderMethod(dataType, isNullable)
            };
        }

        static IReadOnlyList<ColumnInfo> CreateColumns(DataTable table)
        {
            var items = new List<ColumnInfo>();

            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                items.Add(CreateColumn(row));
            }

            return items;
        }

        static string ReadDataType(DataRow row, string dataType, string columnName)
        {
            if (dataType == "DATETIME2")
            {
                dataType = "DATETIME";
            }

            if (dataType == "VARCHAR" || dataType == "NVARCHAR" || dataType == "CHAR" || dataType == "NCHAR")
            {
                var characterLength = row["CHARACTER_MAXIMUM_LENGTH"].ToString();
                if (characterLength == "-1")
                {
                    characterLength = "MAX";
                }

                dataType = dataType + "(" + characterLength + ")";
            }
            else if (AlreadySame.Any(x => x.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase)))
            {
            }
            else if (dataType == "NUMERIC")
            {
                var numericPrecision = row["NUMERIC_PRECISION"] + "";
                var numericScale     = row["NUMERIC_SCALE"] + "";

                if (numericPrecision.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException(nameof(numericPrecision) + Path.VolumeSeparatorChar + nameof(columnName) + Path.VolumeSeparatorChar + columnName);
                }

                if (numericScale.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException(nameof(numericScale) + Path.VolumeSeparatorChar + nameof(columnName) + Path.VolumeSeparatorChar + columnName);
                }

                dataType = dataType + "(" + numericPrecision + "," + numericScale + ")";
            }
            else if (dataType == "DECIMAL")
            {
                var numericPrecision = row["NUMERIC_PRECISION"] + "";
                var numericScale     = row["NUMERIC_SCALE"] + "";

                if (numericPrecision.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException(nameof(numericPrecision) + Path.VolumeSeparatorChar + nameof(columnName) + Path.VolumeSeparatorChar + columnName);
                }

                if (numericScale.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException(nameof(numericScale) + Path.VolumeSeparatorChar + nameof(columnName) + Path.VolumeSeparatorChar + columnName);
                }

                dataType = "NUMERIC" + "(" + numericPrecision + "," + numericScale + ")";
            }
            else
            {
                throw new ArgumentException(dataType);
            }

            return dataType;
        }

        string GetColumnComment(string schemaName, string tableName, string columnName)
        {
            var sql = Database;

            sql.CommandText =
                $@"SELECT TOP 1 value
    FROM sys.extended_properties
   WHERE major_id = OBJECT_ID('{schemaName}.{tableName}') AND
         minor_id = COLUMNPROPERTY(major_id, '{columnName}', 'ColumnId')";

            return (string) sql.ExecuteScalar();
        }

        DataTable GetPrimaryColumns(string tableSchema, string tableName)
        {
            var sql = Database;

            sql.CommandText =
                @"SELECT Col.Column_Name from
    INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col ,
    (select NAME from dbo.sysobjects where xtype='u') AS A
WHERE
    Col.Constraint_Name = Tab.Constraint_Name
    AND Col.Table_Name = Tab.Table_Name
    AND Constraint_Type = 'PRIMARY KEY '
    AND Col.Table_Name = A.NAME
	AND Col.TABLE_SCHEMA = @TABLE_SCHEMA
	AND Col.TABLE_NAME = @TABLE_NAME";

            sql["TABLE_SCHEMA"] = tableSchema;
            sql["TABLE_NAME"]   = tableName;

            return sql.ExecuteReader().ToDataTable();
        }

        DataTable GetTableInformation(string tableCatalog, string tableSchema, string tableName)
        {
            var sql = Database;
            sql.CommandText =
                @"SELECT c.*,  (SELECT  TOP 1 i.name
				  FROM   sys.tables			AS t INNER JOIN
						 sys.indexes	    AS i ON t.object_id = i.object_id INNER JOIN
						 sys.index_columns  AS IC ON i.object_id = IC.object_id AND i.index_id = IC.index_id INNER JOIN
						 sys.all_columns    AS AC ON t.object_id = AC.object_id AND IC.column_id = AC.column_id

				  WHERE   IC.key_ordinal = 1 AND t.name = @TABLE_NAME AND AC.name = c.COLUMN_NAME ) AS IndexName,

			 (SELECT  TOP 1 AC.is_identity
				  FROM   sys.tables			 AS t INNER JOIN
						 sys.all_columns     AS AC ON t.object_id = AC.object_id

				  WHERE  t.name = @TABLE_NAME AND AC.name = c.COLUMN_NAME ) AS IsIdentity

  FROM INFORMATION_SCHEMA.COLUMNS c
 WHERE  TABLE_NAME    = @TABLE_NAME 
   AND (TABLE_CATALOG = @TABLE_CATALOG OR @TABLE_CATALOG IS NULL) 
   AND  TABLE_SCHEMA  = @TABLE_SCHEMA
";
            sql["TABLE_NAME"]    = tableName;
            sql["TABLE_CATALOG"] = tableCatalog;
            sql["TABLE_SCHEMA"]  = tableSchema;

            return sql.ExecuteReader().ToDataTable();
        }

        bool TableExists(string schemaName, string tableName)
        {
            var sql = Database;
            sql.CommandText = "SELECT COUNT(*) " +
                              "  FROM INFORMATION_SCHEMA.TABLES " +
                              " WHERE TABLE_NAME   = @tableName AND " +
                              "       TABLE_SCHEMA = @schemaName ";
            sql["tableName"]  = tableName;
            sql["schemaName"] = schemaName;
            return Convert.ToInt32(sql.ExecuteScalar()) > 0;
        }
        #endregion
    }
}