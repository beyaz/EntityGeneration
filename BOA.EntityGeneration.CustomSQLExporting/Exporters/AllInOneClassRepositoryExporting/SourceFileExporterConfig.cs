namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.AllInOneClassRepositoryExporting
{
    class SourceFileExporterConfig
    {
        #region Public Properties
        public string ClassDefinitionBegin { get; set; }
        public string ClassName            { get; set; }
        public string EmbeddedCodes        { get; set; }

        public string[] ExtraAssemblyReferences         { get; set; }
        public string   MethodName                      { get; set; }
        public string   NamespaceName                   { get; set; }
        public string   OutputFilePath                  { get; set; }
        public string   SharedRepositoryClassAccessPath { get; set; }
        public string[] UsingLines                      { get; set; }
        #endregion

        #region Public Methods
        public static SourceFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<SourceFileExporterConfig>(nameof(Exporters), nameof(AllInOneClassRepositoryExporting));
        }
        #endregion
    }
}