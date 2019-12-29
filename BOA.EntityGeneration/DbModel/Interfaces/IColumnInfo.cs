using System.Data;

namespace BOA.EntityGeneration.DbModel.Interfaces
{
    /// <summary>
    ///     The column information
    /// </summary>
    public interface IColumnInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the column.
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        ///     Gets or sets the comment.
        /// </summary>
        string Comment { get; }

        /// <summary>
        ///     Gets or sets the type of the data.
        /// </summary>
        string DataType { get; }

        /// <summary>
        ///     Gets or sets the type of the dot net.
        /// </summary>
        string DotNetType { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is identity.
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        bool IsNullable { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is primary key.
        /// </summary>
        bool IsPrimaryKey { get; }

        /// <summary>
        ///     Gets or sets the name of the SQL database type.
        /// </summary>
        SqlDbType SqlDbType { get; }

        /// <summary>
        ///     Gets or sets the SQL reader method.
        /// </summary>
        SqlReaderMethods SqlReaderMethod { get; }
        #endregion
    }
}