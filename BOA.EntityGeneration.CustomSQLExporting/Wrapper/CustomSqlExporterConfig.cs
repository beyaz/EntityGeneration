using BOA.EntityGeneration.CustomSQLExporting.Models;

namespace BOA.EntityGeneration.CustomSQLExporting.Wrapper
{
    /// <summary>
    ///     The custom SQL exporter configuration
    /// </summary>
    public class CustomSqlExporterConfig
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets a value indicating whether this instance can export all in one class repository.
        /// </summary>
        public bool CanExportAllInOneClassRepository { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance can export boa repository.
        /// </summary>
        public bool CanExportBoaRepository { get; set; }

        /// <summary>
        ///     Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }
        
        /// <summary>
        ///     Gets or sets the custom SQL names defined to profile SQL.
        /// </summary>
        public string CustomSQLNamesDefinedToProfileSql { get; set; }

        /// <summary>
        ///     Gets or sets the entity namespace.
        /// </summary>
        public string EntityNamespace { get; set; }

        /// <summary>
        ///     Gets or sets the entity project directory.
        /// </summary>
        public string EntityProjectDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the name of the input class.
        /// </summary>
        public string InputClassName { get; set; }

        /// <summary>
        ///     Gets or sets the referenced entity type naming pattern.
        /// </summary>
        public ReferencedEntityTypeNamingPatternContract ReferencedEntityTypeNamingPattern { get; set; }

        /// <summary>
        ///     Gets or sets the name of the repository class.
        /// </summary>
        public string RepositoryClassName { get; set; }

        /// <summary>
        ///     Gets or sets the repository namespace.
        /// </summary>
        public string RepositoryNamespace { get; set; }

        /// <summary>
        ///     Gets or sets the repository project directory.
        /// </summary>
        public string RepositoryProjectDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the name of the result class.
        /// </summary>
        public string ResultClassName { get; set; }

        /// <summary>
        ///     Gets or sets the SLN directory path.
        /// </summary>
        public string SlnDirectoryPath { get; set; }

        /// <summary>
        ///     Gets or sets the SQL get profile identifier list.
        /// </summary>
        public string SQL_GetProfileIdList { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Creates from file.
        /// </summary>
        public static CustomSqlExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<CustomSqlExporterConfig>(nameof(Wrapper));
        }
        #endregion
    }
}