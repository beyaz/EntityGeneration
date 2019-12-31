using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain
{
    class DatabaseReader
    {
        #region Public Properties
        public IDbConnection Connection { get; set; }
        public string        ObjectId   { get; set; }
        public string        ProfileId  { get; set; }
        #endregion

        #region Public Methods
        public CustomSqlInfo ReadCustomSqlInfo()
        {
            var customSqlInfo = Connection.QuerySingle<CustomSqlInfo>(@"
    SELECT text AS Sql, 
           schemaname AS SchemaName, 
           CAST(resultcollectionflag AS INT) AS SqlResultIsCollection,
           @objectId AS [Name],
           @profileId AS ProfileId
      FROM dbo.objects WITH (NOLOCK)
     WHERE objecttype = 'CUSTOMSQL' 
       AND profileid  = @ProfileId 
       AND objectid   = @ObjectId
", new {ProfileId, ObjectId});

            return customSqlInfo;
        }
        #endregion



        internal static IReadOnlyList<ObjectParameterInfo> ReadInputParametersFromDatabase(CustomSqlInfo customSqlInfo, 
                                                                                           IDatabase     database,
                                                                                           IDbConnection connection)
        {

            var profileId = customSqlInfo.ProfileId;
            var objectId  = customSqlInfo.Name;

            var query = $@"
SELECT parameterid AS [Name],
       datatype,
       CAST(nullableflag as BIT) as [isNullable]
  from dbo.objectparameters WITH (NOLOCK) 
 WHERE profileid = @{nameof(profileId)}
  AND objectid   = @{nameof(objectId)}";

            return connection.Query<ObjectParameterInfo>(query, new {profileId, objectId}).ToList();

            
        }

        /// <summary>
        ///     Reads the result columns.
        /// </summary>
        internal static IReadOnlyList<CustomSqlInfoResult> ReadResultColumns(CustomSqlInfo customSqlInfo, IDatabase database, IDbConnection connection)
        {



            var profileId = customSqlInfo.ProfileId;
            var objectId  = customSqlInfo.Name;

            var query = $@"
SELECT resultid                  AS [Name],
       datatype                  AS [DataType],
       CAST(nullableflag as BIT) AS [IsNullable]
  from dbo.objectresults WITH (NOLOCK) 
 WHERE profileid = @{nameof(profileId)}
  AND objectid   = @{nameof(objectId)}";

            return connection.Query<CustomSqlInfoResult>(query, new {profileId, objectId}).ToList();
            
        }
    }

    /// <summary>
    ///     The custom SQL information
    /// </summary>
    [Serializable]
    public class CustomSqlInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the profile identifier.
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the schema.
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        ///     Gets or sets the SQL.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [SQL result is collection].
        /// </summary>
        public bool SqlResultIsCollection { get; set; }
        #endregion
    }

    [Serializable]
    internal class ObjectParameterInfo
    {
        #region Public Properties
        public string dataType   { get; set; }
        public bool   isNullable { get; set; }
        public string name       { get; set; }
        #endregion
    }

    [Serializable]
    public class CustomSqlInfoResult
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the type of the data.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        ///     Gets or sets the data type in dotnet.
        /// </summary>
        public string DataTypeInDotnet { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        public bool IsReferenceToEntity => DataType?.Equals("object", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the name in dotnet.
        /// </summary>
        public string NameInDotnet { get; set; }

        #endregion
    }
}