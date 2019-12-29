using System.Collections.Generic;
using DotNetSerializationUtilities;
using Config = BOA.EntityGeneration.SchemaToEntityExporting.Exporters.SchemaExporterConfig;

namespace BOA.EntityGeneration.SchemaToEntityExporting.Exporters
{
    /// <summary>
    ///     The schema exporter configuration
    /// </summary>
    class SchemaExporterConfig
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets a value indicating whether this instance can export all schema in one class repository.
        /// </summary>
        public bool CanExportAllSchemaInOneClassRepository { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance can export boa repository.
        /// </summary>
        public bool CanExportBoaRepository { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance can export csharp project files.
        /// </summary>
        public bool CanExportCsharpProjectFiles { get; set; }

        /// <summary>
        ///     Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the entity project directory.
        /// </summary>
        public string EntityProjectDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the not exportable tables.
        /// </summary>
        public string[] NotExportableTables { get; set; }

        /// <summary>
        ///     Gets or sets the repository namespace.
        /// </summary>
        public string RepositoryNamespace { get; set; }

        /// <summary>
        ///     Gets or sets the repository project directory.
        /// </summary>
        public string RepositoryProjectDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the SLN directory path.
        /// </summary>
        public string SlnDirectoryPath { get; set; }

        /// <summary>
        ///     Gets or sets the SQL sequence information of table.
        /// </summary>
        public string SqlSequenceInformationOfTable { get; set; }

        /// <summary>
        ///     Gets or sets the table catalog.
        /// </summary>
        public string TableCatalog { get; set; }

        /// <summary>
        ///     Gets or sets the table naming pattern.
        /// </summary>
        public Dictionary<string, string> TableNamingPattern { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Creates from file.
        /// </summary>
        public static Config CreateFromFile(string filePath)
        {
            return YamlHelper.DeserializeFromFile<Config>(filePath);
        }

        /// <summary>
        ///     Creates from file.
        /// </summary>
        public static Config CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<Config>(nameof(Exporters));
        }
        #endregion
    }
}