using System;
using System.Data;
using BOA.Collections;
using BOA.EntityGeneration.CustomSQLExporting.Models;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.ContextManagement
{
    class Context
    {

        

        #region Fields
        public readonly AddOnlyList<string> EntityAssemblyReferences = new AddOnlyList<string>();
        public readonly NamingMap           NamingMap                = new NamingMap();
        #endregion

        #region Constructors
        public Context()
        {
            FileSystem              =  new FileSystem();
            FileSystem.ErrorOccured += error => { Errors.Add(error.ToString()); };

            MsBuildQueue = new MsBuildQueue
            {
                Trace   = trace => { ProcessInfo.Text = trace; },
                OnError = error => { Errors.Add(error.ToString()); }
            };
        }
        #endregion

        #region Public Properties
        public CustomSqlInfo                             CustomSqlInfo                     { get; set; }
        public SqlDatabase                               Database                          { get; set; }
        public AddOnlyList<string>                       Errors                            { get; } = new AddOnlyList<string>();
        public FileSystem                                FileSystem                        { get; }
        public MsBuildQueue                              MsBuildQueue                      { get; }
        public ProcessContract                           ProcessInfo                       { get; } = new ProcessContract();
        public string                                    ProfileName                       { get; set; }
        public ReferencedEntityTypeNamingPatternContract ReferencedEntityTypeNamingPattern { get; set; }
        public AddOnlyList<string>                       RepositoryAssemblyReferences      { get; } = new AddOnlyList<string>();
        public AddOnlyList<string>                       RepositoryProjectSourceFileNames  { get; } = new AddOnlyList<string>();
        public IDbConnection Connection { get; set; }
        #endregion

        #region CustomSqlInfoInitialized
        public event Action CustomSqlInfoInitialized;

        public void OnCustomSqlInfoInitialized()
        {
            CustomSqlInfoInitialized?.Invoke();
        }
        #endregion

        #region ProfileInfoInitialized
        public event Action ProfileInfoInitialized;

        public void OnProfileInfoInitialized()
        {
            ProfileInfoInitialized?.Invoke();
        }
        #endregion

        #region ProfileInfoRemove
        public event Action ProfileInfoRemove;

        public void OnProfileInfoRemove()
        {
            ProfileInfoRemove?.Invoke();
        }
        #endregion
    }
}