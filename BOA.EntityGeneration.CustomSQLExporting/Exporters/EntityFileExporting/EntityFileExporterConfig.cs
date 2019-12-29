using System.Collections.Generic;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.EntityFileExporting
{
    class EntityFileExporterConfig
    {
        #region Public Properties
        public string        OutputFilePath { get; set; }
        public IList<string> UsingLines     { get; set; }
        #endregion

        #region Public Methods
        public static EntityFileExporterConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<EntityFileExporterConfig>(nameof(Exporters), nameof(EntityFileExporting));
        }
        #endregion
    }
}