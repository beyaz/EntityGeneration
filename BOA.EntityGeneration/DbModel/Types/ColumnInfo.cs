using System;
using System.Data;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.DbModel.Types
{
    /// <summary>
    ///     The column information
    /// </summary>
    [Serializable]
    public sealed class ColumnInfo : IColumnInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name of the column.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///     Gets or sets the type of the data.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        ///     Gets or sets the type of the dot net.
        /// </summary>
        public string DotNetType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is identity.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets the name of the SQL database type.
        /// </summary>
        public SqlDbType SqlDbType { get; set; }

        /// <summary>
        ///     Gets or sets the SQL reader method.
        /// </summary>
        public SqlReaderMethods SqlReaderMethod { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Creates from.
        /// </summary>
        public static ColumnInfo CreateFrom(IColumnInfo info)
        {
            return new ColumnInfo
            {
                ColumnName      = info.ColumnName,
                Comment         = info.Comment,
                DataType        = info.DataType,
                DotNetType      = info.DotNetType,
                IsIdentity      = info.IsIdentity,
                IsNullable      = info.IsNullable,
                IsPrimaryKey    = info.IsPrimaryKey,
                SqlDbType       = info.SqlDbType,
                SqlReaderMethod = info.SqlReaderMethod
            };
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return DotNetType + " " + ColumnName;
        }
        #endregion
    }
}