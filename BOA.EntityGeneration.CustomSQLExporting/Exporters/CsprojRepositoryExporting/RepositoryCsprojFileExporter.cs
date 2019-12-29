using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.CsprojRepositoryExporting
{
    class RepositoryCsprojFileExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly RepositoryCsprojFileExporterConfig Config = RepositoryCsprojFileExporterConfig.CreateFromFile();
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            Context.ProfileInfoRemove += Export;
        }
        #endregion

        #region Methods
        void Export()
        {
            foreach (var item in Config.DefaultAssemblyReferences)
            {
                Context.RepositoryAssemblyReferences.Add(Resolve(item));
            }

            var csprojFileGenerator = new CsprojFileGenerator
            {
                FileSystem       = FileSystem,
                FileNames        = Context.RepositoryProjectSourceFileNames,
                NamespaceName    = NamingMap.RepositoryNamespace,
                IsClientDll      = false,
                ProjectDirectory = Context.NamingMap.RepositoryProjectDirectory,
                References       = Context.RepositoryAssemblyReferences
            };

            var csprojFilePath = csprojFileGenerator.Generate();

            MsBuildQueue.Push(csprojFilePath);
        }
        #endregion
    }
}