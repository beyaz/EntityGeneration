using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.EntityFileExporting;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.CsprojFileExporters
{
    class EntityCsprojFileExporter : ContextContainer
    {
        #region Public Methods
        public void AttachEvents()
        {
            SchemaExportFinished += Export;
        }
        #endregion

        #region Methods
        void Export()
        {
            var csprojFileGenerator = new CsprojFileGenerator
            {
                FileSystem       = FileSystem,
                FileNames        = Context.EntityProjectSourceFileNames,
                NamespaceName    = NamingMap.EntityNamespace,
                IsClientDll      = true,
                ProjectDirectory = NamingMap.EntityProjectDirectory,
                References       = EntityFileExporter.Config.AssemblyReferences
            };

            var csprojFilePath = csprojFileGenerator.Generate();

            MsBuildQueue.Push(csprojFilePath);
        }
        #endregion
    }
}