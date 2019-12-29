namespace BOA.EntityGeneration.SchemaToEntityExporting
{
    public class NamingMap : EntityGeneration.NamingMap
    {
        #region Public Properties
        public string CamelCasedTableName        => Resolve(nameof(CamelCasedTableName));
        public string EntityClassName            => Resolve(nameof(EntityClassName));
        public string EntityNamespace            => Resolve(nameof(EntityNamespace));
        public string EntityProjectDirectory     => Resolve(nameof(EntityProjectDirectory));
        public string RepositoryNamespace        => Resolve(nameof(RepositoryNamespace));
        public string RepositoryProjectDirectory => Resolve(nameof(RepositoryProjectDirectory));
        public string SchemaName                 => Resolve(nameof(SchemaName));
        public string SlnDirectoryPath           => Resolve(nameof(SlnDirectoryPath));
        #endregion
    }
}