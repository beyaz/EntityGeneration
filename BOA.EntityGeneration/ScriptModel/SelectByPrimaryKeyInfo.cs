using System;
using System.Collections.Generic;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.ScriptModel
{
    /// <summary>
    ///     The select by primary key information
    /// </summary>
    [Serializable]
    public class SelectByPrimaryKeyInfo : ISelectByPrimaryKeyInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the SQL.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        ///     Gets or sets the SQL parameters.
        /// </summary>
        public IReadOnlyList<IColumnInfo> SqlParameters { get; set; }
        #endregion
    }
}