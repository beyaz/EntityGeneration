using System;
using BOA.EntityGeneration.SchemaToEntityExporting.DbModels;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters
{
    class ContextContainer
    {
        #region Events
        protected event Action SchemaExportFinished
        {
            add => Context.SchemaExportFinished += value;
            remove => throw new NotImplementedException();
        }

        protected event Action SchemaExportStarted
        {
            add => Context.SchemaExportStarted += value;
            remove => throw new NotImplementedException();
        }

        protected event Action TableExportFinished
        {
            add => Context.TableExportFinished += value;
            remove => throw new NotImplementedException();
        }

        protected event Action TableExportStarted
        {
            add => Context.TableExportStarted += value;
            remove => throw new NotImplementedException();
        }
        #endregion

        #region Public Properties
        public Context Context { get; protected set; }

        public IDatabase Database => Context.Database;
        #endregion

        #region Properties
        protected FileSystem      FileSystem                                               => Context.FileSystem;
        protected MsBuildQueue    MsBuildQueue                                             => Context.MsBuildQueue;
        protected NamingMap       NamingMap                                                => Context.NamingMap;
        protected ProcessContract ProcessInfo                                              => Context.ProcessInfo;
        protected string          SchemaName                                               => Context.SchemaName;
        protected string          TableEntityClassNameForMethodParametersInRepositoryFiles => Context.TableEntityClassNameForMethodParametersInRepositoryFiles;
        protected ITableInfo      TableInfo                                                => Context.TableInfo;
        #endregion

        #region Methods
        protected T Create<T>() where T : ContextContainer, new()
        {
            return new T {Context = Context};
        }

        protected void PushNamingMap(string key, string value)
        {
            NamingMap.Push(key, value);
        }

        protected string Resolve(string value)
        {
            return NamingMap.Resolve(value);
        }
        #endregion
    }
}