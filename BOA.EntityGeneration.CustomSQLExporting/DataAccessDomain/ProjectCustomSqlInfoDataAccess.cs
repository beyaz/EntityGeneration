using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain;
using BOA.EntityGeneration.CustomSQLExporting.Models;
using BOA.EntityGeneration.DbModel;
using DotNetStringUtilities;
using CustomSqlInfo = BOA.EntityGeneration.CustomSQLExporting.Models.CustomSqlInfo;
using CustomSqlInfoResult = BOA.EntityGeneration.CustomSQLExporting.Models.CustomSqlInfoResult;

namespace BOA.EntityGeneration.CustomSQLExporting.DataAccessDomain
{
    /// <summary>
    ///     The project custom SQL information data access
    /// </summary>
    public class ProjectCustomSqlInfoDataAccess
    {
        #region Public Properties
        public IDbConnection Connection      { get; set; }
        public string        ObjectId        { get; set; }
        public string        ProfileId       { get; set; }
        public int           SwitchCaseIndex { get; set; }
        public DatabaseReader DatabaseReader { get; set; }
        #endregion

        

        #region Public Methods
        public CustomSqlInfo GetCustomSqlInfo()
        {
           

            var customSqlInfo = DatabaseReader.ReadCustomSqlInfo().ToCustomSqlInfo();

            customSqlInfo.Parameters = ReadInputParameters();

            customSqlInfo.ResultColumns = DatabaseReader.ReadResultColumns().Select(Mapper.ToCustomSqlInfoResult).ToList();

            if (customSqlInfo.ResultColumns.Any(item => item.IsReferenceToEntity) &&
                customSqlInfo.ResultColumns.Count == 1)
            {
                customSqlInfo.ResultContractIsReferencedToEntity = true;
            }

            Fill(customSqlInfo);

            customSqlInfo.SwitchCaseIndex = SwitchCaseIndex;

            return customSqlInfo;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Fills the specified custom SQL information.
        /// </summary>
        static void Fill(CustomSqlInfo customSqlInfo)
        {
            foreach (var item in customSqlInfo.ResultColumns)
            {
                if (item.IsReferenceToEntity)
                {
                    item.NameInDotnet     = item.Name.ToContractName();
                    item.DataTypeInDotnet = $"{customSqlInfo.SchemaName}.{item.Name.ToContractName()}Contract";

                    continue;
                }

                Fill(item);
            }
        }

        /// <summary>
        ///     Fills the specified item.
        /// </summary>
        static void Fill(CustomSqlInfoResult item)
        {
            item.NameInDotnet = item.Name.ToContractName();

            item.DataTypeInDotnet = GetDataTypeInDotnet(item.DataType, item.IsNullable);

            if (item.Name.EndsWith("_FLAG", StringComparison.OrdinalIgnoreCase))
            {
                var sqlDataTypeIsChar = item.DataType.EndsWith("char", StringComparison.OrdinalIgnoreCase);
                if (!sqlDataTypeIsChar)
                {
                    throw new InvalidOperationException($"{item.Name} column should be char.");
                }

                item.DataTypeInDotnet = DotNetTypeName.DotNetBool;
                item.SqlReaderMethod  = SqlReaderMethods.GetBooleanValue;
                if (item.IsNullable)
                {
                    item.DataTypeInDotnet = DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetBool);
                    item.SqlReaderMethod  = SqlReaderMethods.GetBooleanNullableValueFromChar;
                }
            }
            else
            {
                item.SqlReaderMethod = SqlDbTypeMap.GetSqlReaderMethod(item.DataType.ToUpperEN(), item.IsNullable);
            }
        }

        /// <summary>
        ///     Gets the data type in dotnet.
        /// </summary>
        static string GetDataTypeInDotnet(string dataType, bool isNullable)
        {
            if (dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                return "string";
            }

            if (dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase))
            {
                return "string";
            }

            var suffix = isNullable ? "?" : string.Empty;

            if (dataType.Equals("bigint", StringComparison.OrdinalIgnoreCase))
            {
                return "long" + suffix;
            }

            if (dataType.Equals("numeric", StringComparison.OrdinalIgnoreCase))
            {
                return "decimal" + suffix;
            }

            if (dataType.Equals("datetime", StringComparison.OrdinalIgnoreCase))
            {
                return "DateTime" + suffix;
            }

            if (dataType.Equals("Int16", StringComparison.OrdinalIgnoreCase))
            {
                return "short" + suffix;
            }

            if (dataType.Equals("smallint", StringComparison.OrdinalIgnoreCase))
            {
                return "short" + suffix;
            }

            if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                return "int" + suffix;
            }

            if (dataType.Equals("date", StringComparison.OrdinalIgnoreCase))
            {
                return "DateTime" + suffix;
            }

            if (dataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
            {
                return "bool" + suffix;
            }

            if (dataType.Equals("long", StringComparison.OrdinalIgnoreCase))
            {
                return "long" + suffix;
            }

            if (dataType.Equals("char", StringComparison.OrdinalIgnoreCase))
            {
                return "string";
            }

            if (dataType.Equals("object", StringComparison.OrdinalIgnoreCase))
            {
                return "object";
            }

            if (dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
            {
                return "decimal" + suffix;
            }

            if (SqlDbType.TinyInt.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                return DotNetTypeName.DotNetByte + suffix;
            }

            if (DotNetTypeName.DotNetTimeSpan.Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                return DotNetTypeName.DotNetTimeSpan + suffix;
            }

            var config = Config.FirstOrDefault(x => dataType.Equals(x.DataType, StringComparison.OrdinalIgnoreCase));
            if (config!=null)
            {
                return config.DotNetTypeName;
            }


            

            throw new NotImplementedException(dataType);
        }

        /// <summary>
        ///     Gets the name of the SQL database type.
        /// </summary>
        static SqlDbType GetSqlDbTypeName(string dataType)
        {
            if (dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.VarChar;
            }

            if (dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.VarChar;
            }

            if (dataType.Equals("bigint", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.BigInt;
            }

            if (dataType.Equals("numeric", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Decimal;
            }

            if (dataType.Equals("Int16", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.SmallInt;
            }

            if (dataType.Equals("smallint", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.SmallInt;
            }

            if (dataType.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("datetime", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.DateTime;
            }

            if (dataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Bit;
            }

            if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Int;
            }

            if (dataType.Equals("long", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.BigInt;
            }

            if (dataType.Equals("char", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Char;
            }

            var config = Config.FirstOrDefault(x => dataType.Equals(x.DataType, StringComparison.OrdinalIgnoreCase));
            if (config!=null)
            {
                return config.SqlDbType;
            }

            throw new NotImplementedException(dataType);
        }

        class ParameterConfig
        {
            public string DataType { get; set; }
            public SqlDbType SqlDbType { get; set; }
            public string DotNetTypeName { get; set; }
        }

        static readonly IReadOnlyList<ParameterConfig> Config = new List<ParameterConfig>
        {
            new ParameterConfig{ DataType = "COR.TpIntTable",       SqlDbType = SqlDbType.Structured, DotNetTypeName = "List<int>"},
            new ParameterConfig{ DataType = "COR.TpVarchar10Table", SqlDbType = SqlDbType.Structured, DotNetTypeName = "List<string>"},
            new ParameterConfig{ DataType = "COR.TpVarchar20Table", SqlDbType = SqlDbType.Structured, DotNetTypeName = "List<string>"},
            new ParameterConfig{ DataType = "COR.TpVarchar30Table", SqlDbType = SqlDbType.Structured, DotNetTypeName = "List<string>"},
            new ParameterConfig{ DataType = "COR.TpVarchar40Table", SqlDbType = SqlDbType.Structured, DotNetTypeName = "List<string>"},
            new ParameterConfig{ DataType = "COR.TpVarchar50Table", SqlDbType = SqlDbType.Structured, DotNetTypeName = "List<string>"}
        };

        /// <summary>
        ///     Reads the input parameters.
        /// </summary>
        IReadOnlyList<CustomSqlInfoParameter> ReadInputParameters()
        {
            var list = DatabaseReader.ReadInputParametersFromDatabase();

            return list.ToList().ConvertAll(x =>
            {
                var name       = x.Name;
                var dataType   = x.DataType;
                var isNullable = x.IsNullable;

                var cSharpPropertyTypeName = GetDataTypeInDotnet(dataType, isNullable);

                var cSharpPropertyName = name.ToContractName();

                var valueAccessPathForAddInParameter = cSharpPropertyName;

                var sqlDbTypeName = GetSqlDbTypeName(dataType);

                var isChar = sqlDbTypeName == SqlDbType.Char;

                var endsWithFlagSuffix = name.EndsWith("_FLAG", StringComparison.OrdinalIgnoreCase);
                if (endsWithFlagSuffix && isChar)
                {
                    if (isNullable)
                    {
                        cSharpPropertyTypeName           = DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetBool);
                        valueAccessPathForAddInParameter = valueAccessPathForAddInParameter + ".GetCharNullableValueFromBoolean()";
                    }
                    else
                    {
                        cSharpPropertyTypeName           = DotNetTypeName.DotNetBool;
                        valueAccessPathForAddInParameter = valueAccessPathForAddInParameter + ".GetCharValueFromBoolean()";
                    }
                }

                return new CustomSqlInfoParameter
                {
                    Name                             = name,
                    IsNullable                       = isNullable,
                    CSharpPropertyName               = cSharpPropertyName,
                    CSharpPropertyTypeName           = cSharpPropertyTypeName,
                    SqlDbTypeName                    = sqlDbTypeName,
                    ValueAccessPathForAddInParameter = valueAccessPathForAddInParameter,
                    IsStructured = sqlDbTypeName ==SqlDbType.Structured,
                    StructuredTypeName = x.DataType
                };
            });
        }
        #endregion
    }
}