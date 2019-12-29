using System.Collections.Generic;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.ScriptModel
{
    /// <summary>
    ///     The delete information
    /// </summary>
    public interface IDeleteInfo
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