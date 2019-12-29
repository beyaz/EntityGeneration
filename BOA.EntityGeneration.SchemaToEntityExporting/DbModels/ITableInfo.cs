using System.Collections.Generic;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.SchemaToEntityExporting.DbModels
{
    /// <summary>
    ///     The table information
    /// </summary>
    public interface ITableInfo : DbModel.Interfaces.ITableInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is support select by index.
        /// </summary>
        bool IsSupportSelectByIndex { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is support select by key.
        /// </summary>
        bool IsSupportSelectByKey { get; }

        /// <summary>
        ///     Gets or sets the non unique index information list.
        /// </summary>
        IReadOnlyList<IIndexInfo> NonUniqueIndexInfoList { get; }

        /// <summary>
        ///     Gets or sets the sequence list.
        /// </summary>
        IReadOnlyList<ISequenceInfo> SequenceList { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether [should generate select all by valid flag method in business class].
        /// </summary>
        bool ShouldGenerateSelectAllByValidFlagMethodInBusinessClass { get; }

        /// <summary>
        ///     Gets or sets the unique index information list.
        /// </summary>
        IReadOnlyList<IIndexInfo> UniqueIndexInfoList { get; }
        #endregion
    }
}