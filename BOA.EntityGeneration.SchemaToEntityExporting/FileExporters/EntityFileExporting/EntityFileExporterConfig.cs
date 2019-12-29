using System.Collections.Generic;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.EntityFileExporting
{
    class EntityFileExporterConfig
    {
        #region Public Properties
        public string[] AssemblyReferences { get; set; }
        public string   ClassName          { get; set; }
        public string   EntityContractBase { get; set; }

        public string NamespaceName { get; set; }

        public string OutputFilePath { get; set; }

        public ICollection<string> UsingLines { get; set; }
        #endregion

        #region Public Methods
        public static EntityFileExporterConfig LoadFromFile()
        {
            return ConfigurationHelper.ReadConfig<EntityFileExporterConfig>(nameof(FileExporters), nameof(EntityFileExporting));
        }
        #endregion
    }
}