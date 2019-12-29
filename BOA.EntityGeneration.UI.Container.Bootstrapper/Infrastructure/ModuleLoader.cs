namespace BOA.EntityGeneration.UI.Container.Infrastructure
{
    static class ModuleLoader
    {
        #region Public Methods
        public static void Load()
        {
            EmbeddedCompressedAssemblyReferencesResolver.Resolve("EmbeddedReferences.zip");
        }
        #endregion
    }
}