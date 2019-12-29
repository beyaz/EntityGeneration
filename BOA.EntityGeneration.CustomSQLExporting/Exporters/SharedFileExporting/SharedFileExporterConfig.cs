using Config = BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedFileExporting.SharedFileExporterConfig;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedFileExporting
{
    class SharedFileExporterConfig
    {
        #region Public Properties
        public string EmbeddedCodes { get; set; }

        public string OutputFilePath { get; set; }
        #endregion

        #region Public Methods
        public static Config CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<Config>(nameof(Exporters), nameof(SharedFileExporting));
        }
        #endregion
    }
}