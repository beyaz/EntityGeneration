using System;
using System.Data;

namespace BOA.EntityGeneration.CustomSQLExporting.Models
{
    /// <summary>
    ///     The custom SQL information parameter
    /// </summary>
    [Serializable]
    public class CustomSqlInfoParameter
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the c sharp property.
        /// </summary>
        public string CSharpPropertyName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the c sharp property type.
        /// </summary>
        public string CSharpPropertyTypeName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the name of the SQL database type.
        /// </summary>
        public SqlDbType SqlDbTypeName { get; set; }

        /// <summary>
        ///     Gets or sets the value access path for add in parameter.
        /// </summary>
        public string ValueAccessPathForAddInParameter { get; set; }
        #endregion
    }
}