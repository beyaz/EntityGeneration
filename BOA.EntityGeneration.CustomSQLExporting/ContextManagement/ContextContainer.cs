using BOA.Collections;
using BOA.EntityGeneration.CustomSQLExporting.Models;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.ContextManagement
{
    class ContextContainer
    {
        #region Public Properties
        public Context Context { get; set; }
        #endregion

        #region Properties
        protected CustomSqlInfo       CustomSqlInfo            => Context.CustomSqlInfo;
        protected IDatabase           Database                 => Context.Database;
        protected AddOnlyList<string> EntityAssemblyReferences => Context.EntityAssemblyReferences;
        protected FileSystem          FileSystem               => Context.FileSystem;
        protected MsBuildQueue        MsBuildQueue             => Context.MsBuildQueue;

        protected NamingMap                                 NamingMap                         => Context.NamingMap;
        protected ProcessContract                           ProcessInfo                       => Context.ProcessInfo;
        protected string                                    ProfileName                       => Context.ProfileName;
        protected ReferencedEntityTypeNamingPatternContract ReferencedEntityTypeNamingPattern => Context.ReferencedEntityTypeNamingPattern;
        #endregion

        #region Methods
        protected T Create<T>() where T : ContextContainer, new()
        {
            return new T {Context = Context};
        }

        protected string Resolve(string value)
        {
            return Context.NamingMap.Resolve(value);
        }
        #endregion
    }
}