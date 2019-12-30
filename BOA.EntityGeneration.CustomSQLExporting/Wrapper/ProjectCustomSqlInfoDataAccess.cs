using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BOA.EntityGeneration.CustomSQLExporting.Models;
using BOA.EntityGeneration.DbModel;
using DotNetDatabaseAccessUtilities;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Wrapper
{
    public class GetCustomSqlNamesInfProfileInput
    {
        public GetCustomSqlNamesInfProfileInput(IDatabase database, string profileId, CustomSqlExporterConfig config)
        {
            Database = database;
            ProfileId = profileId;
            Config = config;
        }
        

        public IDatabase Database { get; set; }
        public string ProfileId { get; set; }
        public CustomSqlExporterConfig Config { get; set; }
    }

    public class GetCustomSqlInfoInput
    {
        public GetCustomSqlInfoInput(IDatabase database, string profileId, string id, CustomSqlExporterConfig config, int switchCaseIndex)
        {
            Database = database;
            ProfileId = profileId;
            Id = id;
            Config = config;
            SwitchCaseIndex = switchCaseIndex;
        }

        public IDatabase Database { get; set; }
        public string ProfileId { get; set; }
        public string Id { get; set; }
        public CustomSqlExporterConfig Config { get; set; }
        public int SwitchCaseIndex { get; set; }
    }

    /// <summary>
    ///     The project custom SQL information data access
    /// </summary>
    public class ProjectCustomSqlInfoDataAccess
    {
        
        internal static CustomSqlInfo ReadFromDatabase(GetCustomSqlInfoInput customSqlInfoInput)
        {
            var customSqlInfo = new CustomSqlInfo
            {
                Name      = customSqlInfoInput.Id,
                ProfileId = customSqlInfoInput.ProfileId
            };

            customSqlInfoInput.Database.CommandText                           = customSqlInfoInput.Config.CustomSQL_Get_SQL_Item_Info;
            customSqlInfoInput.Database[nameof(customSqlInfoInput.ProfileId)] = customSqlInfoInput.ProfileId;
            customSqlInfoInput.Database[nameof(customSqlInfoInput.Id)]        = customSqlInfoInput.Id;

            var reader = customSqlInfoInput.Database.ExecuteReader();
            while (reader.Read())
            {
                customSqlInfo.Sql                   = reader[nameof(CustomSqlInfo.Sql)] + string.Empty;
                customSqlInfo.SchemaName            = reader[nameof(CustomSqlInfo.SchemaName)] + string.Empty;
                customSqlInfo.SqlResultIsCollection = Convert.ToBoolean(reader[nameof(CustomSqlInfo.SqlResultIsCollection)]);

                break;
            }

            reader.Close();

            return customSqlInfo;
        }

        #region Public Methods
        public static CustomSqlInfo GetCustomSqlInfo(GetCustomSqlInfoInput customSqlInfoInput)
        {
            var customSqlInfo = ReadFromDatabase(customSqlInfoInput);

            customSqlInfo.Parameters = ReadInputParameters(customSqlInfo, customSqlInfoInput.Database);

            customSqlInfo.ResultColumns = ReadResultColumns(customSqlInfo, customSqlInfoInput.Database);

            if (customSqlInfo.ResultColumns.Any(item => item.IsReferenceToEntity) &&
                customSqlInfo.ResultColumns.Count == 1)
            {
                customSqlInfo.ResultContractIsReferencedToEntity = true;
            }

            Fill(customSqlInfo);

            customSqlInfo.SwitchCaseIndex = customSqlInfoInput.SwitchCaseIndex;

            return customSqlInfo;
        }

        public static IReadOnlyList<string> GetCustomSqlNamesInfProfile(GetCustomSqlNamesInfProfileInput customSqlNamesInfProfileInput)
        {
            var objectIdList = new List<string>();

            customSqlNamesInfProfileInput.Database.CommandText        = customSqlNamesInfProfileInput.Config.CustomSQLNamesDefinedToProfileSql;
            customSqlNamesInfProfileInput.Database[nameof(customSqlNamesInfProfileInput.ProfileId)] = customSqlNamesInfProfileInput.ProfileId;

            var reader = customSqlNamesInfProfileInput.Database.ExecuteReader();
            while (reader.Read())
            {
                objectIdList.Add(reader["Id"].ToString());
            }

            reader.Close();

            return objectIdList;
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

            throw new NotImplementedException(dataType);
        }

        internal class ObjectParameterInfo
        {
            public string name { get; set; }
            public string dataType { get; set; }
            public bool isNullable { get; set; }
        }

        internal static IReadOnlyList<ObjectParameterInfo> ReadInputParametersFromDatabase(CustomSqlInfo customSqlInfo, IDatabase database)
        {
            var items = new List<ObjectParameterInfo>();

            database.CommandText = $"select parameterid,datatype,nullableflag from dbo.objectparameters WITH (NOLOCK) WHERE profileid = '{customSqlInfo.ProfileId}' AND objectid = '{customSqlInfo.Name}'";

            var reader = database.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new ObjectParameterInfo { 
                    name       = reader["parameterid"].ToString(),
                    dataType   = reader["datatype"].ToString(),
                    isNullable = reader["nullableflag"] + string.Empty == "1"
                });
            }

            reader.Close();

            return items;
        }

        /// <summary>
        ///     Reads the input parameters.
        /// </summary>
        static IReadOnlyList<CustomSqlInfoParameter> ReadInputParameters(CustomSqlInfo customSqlInfo, IDatabase database)
        {
            var items = new List<CustomSqlInfoParameter>();

            var list = ReadInputParametersFromDatabase(customSqlInfo,database);
            foreach (var y in list)
            {
                var name = y.name;
                var dataType = y.dataType;
                var isNullable = y.isNullable;

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

                var item = new CustomSqlInfoParameter
                {
                    Name                             = name,
                    IsNullable                       = isNullable,
                    CSharpPropertyName               = cSharpPropertyName,
                    CSharpPropertyTypeName           = cSharpPropertyTypeName,
                    SqlDbTypeName                    = sqlDbTypeName,
                    ValueAccessPathForAddInParameter = valueAccessPathForAddInParameter
                };

                items.Add(item);
            }


           

            return items;
        }

        /// <summary>
        ///     Reads the result columns.
        /// </summary>
        internal static IReadOnlyList<CustomSqlInfoResult> ReadResultColumns(CustomSqlInfo customSqlInfo, IDatabase database)
        {
            var items = new List<CustomSqlInfoResult>();

            database.CommandText = $"select resultid,datatype, nullableflag from dbo.objectresults WITH (NOLOCK) WHERE profileid = '{customSqlInfo.ProfileId}' AND objectid = '{customSqlInfo.Name}'";

            var reader = database.ExecuteReader();

            while (reader.Read())
            {
                var item = new CustomSqlInfoResult
                {
                    Name       = reader["resultid"].ToString(),
                    DataType   = reader["datatype"].ToString(),
                    IsNullable = reader["nullableflag"] + string.Empty == "1"
                };

                items.Add(item);
            }

            reader.Close();

            return items;
        }

        /// <summary>
        ///     Gets the by profile identifier list.
        /// </summary>
        IReadOnlyList<string> GetByProfileIdList(IDatabase database)
        {
            var items = new List<string>();

            database.CommandText = "SELECT profileid  from BOACard.dbo.profileskt WITH (NOLOCK)";
            var reader = database.ExecuteReader();
            while (reader.Read())

            {
                items.Add(reader["profileid"] + string.Empty);
            }

            reader.Close();

            return items;
        }
        #endregion
    }
}