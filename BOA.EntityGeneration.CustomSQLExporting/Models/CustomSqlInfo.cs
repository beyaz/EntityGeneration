using System;
using System.Collections.Generic;

namespace BOA.EntityGeneration.CustomSQLExporting.Models
{
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
        ///     Gets or sets the parameters.
        /// </summary>
        public IReadOnlyList<CustomSqlInfoParameter> Parameters { get; set; }

        /// <summary>
        ///     Gets or sets the profile identifier.
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        ///     Gets or sets the result columns.
        /// </summary>
        public IReadOnlyList<CustomSqlInfoResult> ResultColumns { get; set; }

        public bool ResultContractIsReferencedToEntity { get; set; }

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

        /// <summary>
        ///     Gets or sets the index of the switch case.
        /// </summary>
        public int SwitchCaseIndex { get; set; }
        #endregion
    }
}