using System;
using System.Data;
using BOA.Collections;
using BOA.EntityGeneration.SchemaToEntityExporting.DbModels;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters
{
    class Context
    {
        #region Fields
        public readonly NamingMap NamingMap = new NamingMap();
        #endregion

        #region Constructors
        public Context()
        {
            FileSystem              =  new FileSystem();
            FileSystem.ErrorOccured += error => { ErrorList.Add(error.ToString()); };

            MsBuildQueue = new MsBuildQueue
            {
                Trace   = trace => { ProcessInfo.Text = trace; },
                OnError = error => { ErrorList.Add(error.ToString()); }
            };
        }
        #endregion

        #region Public Properties
        public IDatabase           Database                     { get; set; }
        public AddOnlyList<string> EntityProjectSourceFileNames { get; } = new AddOnlyList<string>();
        public AddOnlyList<string> ErrorList                    { get; } = new AddOnlyList<string>();
        public FileSystem          FileSystem                   { get; } 
        public MsBuildQueue        MsBuildQueue                 { get; }
        public ProcessContract     ProcessInfo                  { get; } = new ProcessContract();

        public AddOnlyList<string> RepositoryAssemblyReferences                             { get; } = new AddOnlyList<string>();
        public AddOnlyList<string> RepositoryProjectSourceFileNames                         { get; } = new AddOnlyList<string>();
        public string              SchemaName                                               { get; set; }
        public string              TableEntityClassNameForMethodParametersInRepositoryFiles => NamingMap.EntityClassName;
        public ITableInfo          TableInfo                                                { get; set; }
        public Func<IDbConnection> Connection { get; set; }
        #endregion

        #region TableExportFinished
        public event Action TableExportFinished;

        public void OnTableExportFinished()
        {
            TableExportFinished?.Invoke();
        }
        #endregion

        #region TableExportStarted
        public event Action TableExportStarted;

        public void OnTableExportStarted()
        {
            TableExportStarted?.Invoke();
        }
        #endregion

        #region SchemaExportStarted
        public event Action SchemaExportStarted;

        public void FireSchemaExportStarted()
        {
            SchemaExportStarted?.Invoke();
        }
        #endregion

        #region SchemaExportFinished
        public event Action SchemaExportFinished;

        public void OnSchemaExportFinished()
        {
            SchemaExportFinished?.Invoke();
        }
        #endregion

        #region EntityFileContentCompleted
        public event Action<string> EntityFileContentCompleted;

        public void OnEntityFileContentCompleted(string entityFileContent)
        {
            EntityFileContentCompleted?.Invoke(entityFileContent);
        }
        #endregion

        #region SharedRepositoryFileContentCompleted
        public event Action<string> SharedRepositoryFileContentCompleted;

        public void OnSharedRepositoryFileContentCompleted(string entityFileContent)
        {
            SharedRepositoryFileContentCompleted?.Invoke(entityFileContent);
        }
        #endregion
    }
}