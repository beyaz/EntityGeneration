using System.Collections.Generic;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.CsprojEntityExporting
{
    class EntityCsprojFileExporterConfig
    {
        #region Public Properties
        public IList<string> DefaultAssemblyReferences { get; set; }
        #endregion

        #region Public Methods
        public static EntityCsprojFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<EntityCsprojFileExporterConfig>(nameof(Exporters), nameof(CsprojEntityExporting));
        }
        #endregion
    }
}