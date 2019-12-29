using System.Collections.Generic;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.AllSchemaInOneClassRepositoryFile
{
    class AllSchemaInOneClassRepositoryFileExporterConfig
    {
        #region Public Properties
        public string                     ClassDefinitionBegin                   { get; set; }
        public string                     ClassName                              { get; set; }
        public Dictionary<string, string> DefaultValuesForInsertMethod           { get; set; }
        public Dictionary<string, string> DefaultValuesForUpdateByKeyMethod      { get; set; }
        public string                     EmbeddedCodes                          { get; set; }
        public string[]                   ExcludedColumnNamesWhenInsertOperation { get; set; }
        public IList<string>              ExtraAssemblyReferences                { get; set; }

        public string                     NamespaceName { get; set; }
        public Dictionary<string, string> NamingPattern { get; set; }

        public string OutputFilePath { get; set; }

        public string        SharedRepositoryClassAccessPath { get; set; }
        public IList<string> UsingLines                      { get; set; }
        #endregion

        #region Public Methods
        public static AllSchemaInOneClassRepositoryFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<AllSchemaInOneClassRepositoryFileExporterConfig>(nameof(FileExporters), nameof(AllSchemaInOneClassRepositoryFile));
        }
        #endregion
    }
}