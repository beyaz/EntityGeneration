using System.Collections.Generic;

namespace BOA.EntityGeneration.DbModel.Interfaces
{
    /// <summary>
    ///     The index information
    /// </summary>
    public interface IIndexInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the column names.
        /// </summary>
        IReadOnlyList<string> ColumnNames { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is clustered.
        /// </summary>
        bool IsClustered { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is non clustered.
        /// </summary>
        bool IsNonClustered { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is primary key.
        /// </summary>
        bool IsPrimaryKey { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is unique.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        string Name { get; }
        #endregion
    }
}