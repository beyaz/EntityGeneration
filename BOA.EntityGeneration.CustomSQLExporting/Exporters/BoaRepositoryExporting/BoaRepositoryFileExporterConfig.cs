namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.BoaRepositoryExporting
{
    class BoaRepositoryFileExporterConfig
    {
        #region Public Properties
        public string   EmbeddedCodes                   { get; set; }
        public string   OutputFilePath                  { get; set; }
        public string   SharedRepositoryClassAccessPath { get; set; }
        public string[] UsingLines                      { get; set; }
        #endregion

        #region Public Methods
        public static BoaRepositoryFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<BoaRepositoryFileExporterConfig>(nameof(Exporters), nameof(BoaRepositoryExporting));
        }
        #endregion
    }
}