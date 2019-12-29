using System;
using BOA.EntityGeneration.DbModel;

namespace BOA.EntityGeneration.CustomSQLExporting.Models
{
    /// <summary>
    ///     The custom SQL information result
    /// </summary>
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

        /// <summary>
        ///     Gets or sets the SQL reader method.
        /// </summary>
        public SqlReaderMethods SqlReaderMethod { get; set; }
        #endregion
    }
}