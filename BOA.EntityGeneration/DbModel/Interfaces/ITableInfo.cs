using System.Collections.Generic;

namespace BOA.EntityGeneration.DbModel.Interfaces
{
    /// <summary>
    ///     The table information
    /// </summary>
    public interface ITableInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the catalog.
        /// </summary>
        string CatalogName { get; }

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        IReadOnlyList<IColumnInfo> Columns { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has identity column.
        /// </summary>
        bool HasIdentityColumn { get; }

        /// <summary>
        ///     Gets or sets the identity column.
        /// </summary>
        IColumnInfo IdentityColumn { get; }

        /// <summary>
        ///     Gets or sets the index information list.
        /// </summary>
        IReadOnlyList<IIndexInfo> IndexInfoList { get; }

        /// <summary>
        ///     Gets or sets the primary key columns.
        /// </summary>
        IReadOnlyList<IColumnInfo> PrimaryKeyColumns { get; }

        /// <summary>
        ///     Gets or sets the name of the schema.
        /// </summary>
        string SchemaName { get; }

        /// <summary>
        ///     Gets or sets the name of the table.
        /// </summary>
        string TableName { get; }
        #endregion
    }
}