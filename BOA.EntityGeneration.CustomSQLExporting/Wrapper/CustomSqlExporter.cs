using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using BOA.EntityGeneration.CustomSQLExporting.DataAccessDomain;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.AllInOneClassRepositoryExporting;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.BoaRepositoryExporting;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.CsprojEntityExporting;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.CsprojRepositoryExporting;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.EntityFileExporting;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedFileExporting;
using BOA.EntityGeneration.CustomSQLExporting.Models;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Wrapper
{
    class CustomSqlExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly CustomSqlExporterConfig Config = CustomSqlExporterConfig.CreateFromFile();
        #endregion

        #region Public Methods
        public void Export(string profileId)
        {
            Context.Input = new Input
            {
                ProfileId = profileId
            };

            if (profileId == "EMB_BATCH")
            {
                profileId = "EmbossingBatch";
            }

            Context.ProfileName = profileId;

            // initialize Naming Map
            Context.NamingMap.Push(nameof(NamingMap.ProfileName), ProfileName);


            
            Context.NamingMap.Push(nameof(NamingMap.EntityNamespace), Resolve(Config.EntityNamespace));
            Context.NamingMap.Push(nameof(NamingMap.RepositoryNamespace), Resolve(Config.RepositoryNamespace));

            var newNaming = new[]
            {
                "Collection",
                "Dispute",
                "ParameterManagement",
                "CreditCardLegalFollowup",
                "CreditCardLifecycle",
                "CreditCardIssuing",
                "CreditCardStatementBatch",
                "CreditCardTransaction",
                "EmbossingBatch",
                "CreditCardLimit"
            };

            if (newNaming.Contains(profileId))
            {
                Context.NamingMap.Push(nameof(NamingMap.EntityNamespace), Resolve("BOA.Types.Kernel.Card.CustomSqls.$(ProfileName)"));
                Context.NamingMap.Push(nameof(NamingMap.RepositoryNamespace), Resolve("BOA.Business.Kernel.Card.CustomSqls.$(ProfileName)"));
            }

            Context.NamingMap.Push(nameof(NamingMap.SlnDirectoryPath), Resolve(Config.SlnDirectoryPath));
            Context.NamingMap.Push(nameof(NamingMap.EntityProjectDirectory), Resolve(Config.EntityProjectDirectory));
            Context.NamingMap.Push(nameof(NamingMap.RepositoryProjectDirectory), Resolve(Config.RepositoryProjectDirectory));
            Context.NamingMap.Push(nameof(NamingMap.InputClassName), Config.InputClassName);
            Context.NamingMap.Push(nameof(NamingMap.ResultClassName), Config.ResultClassName);
            Context.NamingMap.Push(nameof(NamingMap.RepositoryClassName), Config.RepositoryClassName);

            ProcessInfo.Text = "Fetching profile informations...";


            var customSqlNamesInfProfile = Context.CreateDatabaseReader().GetCustomSqlNamesInfProfile();


            Context.OnProfileInfoInitialized();

            ProcessInfo.Total = customSqlNamesInfProfile.Count;

            var switchCaseIndex = 0;
            foreach (var objectId in customSqlNamesInfProfile)
            {
                ProcessInfo.Text    = $"Processing '{objectId}'";
                ProcessInfo.Current = switchCaseIndex;

                Context.CurrentObjectId = objectId;

                var projectCustomSqlInfoDataAccess = new ProjectCustomSqlInfoDataAccess
                {
                    ProfileId       = Context.Input.ProfileId,
                    Connection      = Context.Connection,
                    ObjectId        = objectId,
                    SwitchCaseIndex = switchCaseIndex++
                };

                Context.CustomSqlInfo = projectCustomSqlInfoDataAccess.GetCustomSqlInfo();

                InitializeCustomSqlNamingPattern();

                Context.OnCustomSqlInfoInitialized();
            }

            Context.OnProfileInfoRemove();

            ProcessInfo.Text = "Finished Successfully.";

            WaitTwoSecondForUserCanSeeSuccessMessage();
        }

        public IReadOnlyList<string> GetProfileNames()
        {
            var profileIdList = new List<string>();

            Database.CommandText = Config.SQL_GetProfileIdList;
            var reader = Database.ExecuteReader();
            while (reader.Read())
            {
                profileIdList.Add(reader["ProfileId"].ToString());
            }

            reader.Close();

            profileIdList.Add("*");

            return profileIdList;
        }

        public void InitializeContext()
        {
            Context = new Context();

            InitializeDatabaseConnection();

            // attach events
            Create<EntityFileExporter>().AttachEvents();
            Create<SharedFileExporter>().AttachEvents();

            if (Config.CanExportBoaRepository)
            {
                Create<BoaRepositoryFileExporter>().AttachEvents();
            }

            if (Config.CanExportAllInOneClassRepository)
            {
                Create<SourceFileExporter>().AttachEvents();
            }

            Create<EntityCsprojFileExporter>().AttachEvents();
            Create<RepositoryCsprojFileExporter>().AttachEvents();

            Context.ProfileInfoRemove += MsBuildQueue.Build;
        }
        #endregion

        #region Methods
        static void WaitTwoSecondForUserCanSeeSuccessMessage()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        void InitializeCustomSqlNamingPattern()
        {
            Context.NamingMap.Push(nameof(NamingMap.CamelCasedCustomSqlName), CustomSqlInfo.Name.ToContractName());

            var entityReferencedResultColumn = CustomSqlInfo.ResultColumns.FirstOrDefault(x => x.IsReferenceToEntity);
            if (entityReferencedResultColumn != null)
            {
                Context.NamingMap.Push(nameof(NamingMap.SchemaName), CustomSqlInfo.SchemaName);
                Context.NamingMap.Push(nameof(NamingMap.CamelCasedResultName), entityReferencedResultColumn.Name.ToContractName());

                Context.ReferencedEntityTypeNamingPattern = new ReferencedEntityTypeNamingPatternContract
                {
                    ReferencedEntityAccessPath       = Resolve(Config.ReferencedEntityTypeNamingPattern.ReferencedEntityAccessPath),
                    ReferencedEntityAssemblyPath     = Resolve(Config.ReferencedEntityTypeNamingPattern.ReferencedEntityAssemblyPath),
                    ReferencedEntityReaderMethodPath = Resolve(Config.ReferencedEntityTypeNamingPattern.ReferencedEntityReaderMethodPath),
                    ReferencedRepositoryAssemblyPath = Resolve(Config.ReferencedEntityTypeNamingPattern.ReferencedRepositoryAssemblyPath)
                };
            }
        }

        void InitializeDatabaseConnection()
        {
            Context.Connection = new SqlConnection(Config.ConnectionString);

            Context.Database = new SqlDatabase(Config.ConnectionString) {CommandTimeout = 1000 * 60 * 60};
        }
        #endregion
    }
}