using System.Collections.Generic;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.ScriptModel
{
    /// <summary>
    ///     The select by primary key information
    /// </summary>
    public interface ISelectByPrimaryKeyInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the SQL.
        /// </summary>
        string Sql { get; }

        /// <summary>
        ///     Gets or sets the SQL parameters.
        /// </summary>
        IReadOnlyList<IColumnInfo> SqlParameters { get; }
        #endregion
    }
}