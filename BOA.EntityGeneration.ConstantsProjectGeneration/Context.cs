using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BOA.Collections;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ConstantsProjectGeneration
{
    public class Context
    {
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
        public ConstantsProjectGenerationConfig Config { get; } = ConstantsProjectGenerationConfig.CreateFromFile();

        
        public IReadOnlyList<EnumInfo> EnumInfoList      { get; set; }
        public AddOnlyList<string>     Errors            { get; } = new AddOnlyList<string>();
        public PaddedStringBuilder     File              { get; } = new PaddedStringBuilder();
        public FileSystem              FileSystem        { get; } 
        public MsBuildQueue            MsBuildQueue      { get; }
        public ProcessContract         ProcessInfo       { get; } = new ProcessContract();
        #endregion

        #region Public Methods
        public IDbConnection CreateConnection()
        {
            var sqlConnection = new SqlConnection(Config.ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
        #endregion
    }
}