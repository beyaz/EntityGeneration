using System.Collections.Generic;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.CsprojFileExporters
{
    class RepositoryCsprojFileExporterConfig
    {
        #region Public Properties
        public IList<string> DefaultAssemblyReferences { get; set; }
        #endregion

        #region Public Methods
        public static RepositoryCsprojFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<RepositoryCsprojFileExporterConfig>(nameof(FileExporters), nameof(CsprojFileExporters));
        }
        #endregion
    }
}