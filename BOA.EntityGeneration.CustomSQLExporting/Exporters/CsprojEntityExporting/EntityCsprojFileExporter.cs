using System.Collections.Generic;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.CsprojEntityExporting
{
    class EntityCsprojFileExporter : ContextContainer
    {
        #region Static Fields
        static readonly EntityCsprojFileExporterConfig Config = EntityCsprojFileExporterConfig.CreateFromFile();
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
            foreach (var reference in Config.DefaultAssemblyReferences)
            {
                EntityAssemblyReferences.Add(Resolve(reference));
            }

            var csprojFileGenerator = new CsprojFileGenerator
            {
                FileSystem       = FileSystem,
                FileNames        = new List<string> {"All.cs"},
                NamespaceName    = NamingMap.EntityNamespace,
                IsClientDll      = true,
                ProjectDirectory = Context.NamingMap.EntityProjectDirectory,
                References       = EntityAssemblyReferences
            };

            var csprojFilePath = csprojFileGenerator.Generate();

            MsBuildQueue.Push(csprojFilePath);
        }
        #endregion
    }
}