using System.Collections.Generic;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.BankingRepositoryFileExporting
{
    class BoaRepositoryFileExporterConfig
    {
        #region Public Properties
        public string                     ClassNamePattern                       { get; set; }
        public Dictionary<string, string> DefaultValuesForInsertMethod           { get; set; }
        public Dictionary<string, string> DefaultValuesForUpdateByKeyMethod      { get; set; }
        public string                     EmbeddedCodes                          { get; set; }
        public string[]                   ExcludedColumnNamesWhenInsertOperation { get; set; }
        public string[]                   ExtraAssemblyReferences                { get; set; }
        public string                     OutputFilePath                         { get; set; }

        public Dictionary<string, string[]> SchemaSpecificUsingLines        { get; set; }
        public string                       SharedRepositoryClassAccessPath { get; set; }

        public string[] UsingLines { get; set; }
        #endregion

        #region Public Methods
        public static BoaRepositoryFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<BoaRepositoryFileExporterConfig>(nameof(FileExporters), nameof(BankingRepositoryFileExporting));
        }
        #endregion
    }
}