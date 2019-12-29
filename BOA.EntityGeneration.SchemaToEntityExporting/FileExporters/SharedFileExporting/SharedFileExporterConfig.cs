using System.Collections.Generic;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.SharedFileExporting
{
    class SharedFileExporterConfig
    {
        #region Public Properties
        public string              ClassNamePattern { get; set; }
        public string              ContractReadLine { get; set; }
        public string              EmbeddedCodes    { get; set; }
        public string              OutputFilePath   { get; set; }
        public ICollection<string> UsingLines       { get; set; }
        #endregion

        #region Public Methods
        public static SharedFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<SharedFileExporterConfig>(nameof(FileExporters), nameof(SharedFileExporting));
        }
        #endregion
    }
}