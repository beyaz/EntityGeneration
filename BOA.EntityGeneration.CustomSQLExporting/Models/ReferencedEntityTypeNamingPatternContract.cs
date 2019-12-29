namespace BOA.EntityGeneration.CustomSQLExporting.Models
{
    public class ReferencedEntityTypeNamingPatternContract
    {
        #region Public Properties
        public string ReferencedEntityAccessPath       { get; set; }
        public string ReferencedEntityAssemblyPath     { get; set; }
        public string ReferencedEntityReaderMethodPath { get; set; }
        public string ReferencedRepositoryAssemblyPath { get; set; }
        #endregion
    }
}