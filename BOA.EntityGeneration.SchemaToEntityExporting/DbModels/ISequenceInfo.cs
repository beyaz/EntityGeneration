namespace BOA.EntityGeneration.SchemaToEntityExporting.DbModels
{
    /// <summary>
    ///     The sequence information
    /// </summary>
    public interface ISequenceInfo
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets or sets the name of the target column.
        /// </summary>
        string TargetColumnName { get; }
        #endregion
    }
}