using System.Data.SqlClient;
using System.Linq;
using BOA.EntityGeneration.DbModel.SqlServerDataAccess;
using BOA.EntityGeneration.SchemaToEntityExporting.DataAccess;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.AllSchemaInOneClassRepositoryFile;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.BankingRepositoryFileExporting;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.CsprojFileExporters;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.EntityFileExporting;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.SharedFileExporting;
using DotNetDatabaseAccessUtilities;

namespace BOA.EntityGeneration.SchemaToEntityExporting.Exporters
{
    /// <summary>
    ///     The schema exporter
    /// </summary>
    class SchemaExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly SchemaExporterConfig Config = SchemaExporterConfig.CreateFromFile();
        #endregion

        #region Public Methods
        /// <summary>
        ///     Exports the specified schema name.
        /// </summary>
        public void Export(string schemaName)
        {
            Context.SchemaName = schemaName;

            PushNamingMap(nameof(NamingMap.SlnDirectoryPath), Config.SlnDirectoryPath);
            PushNamingMap(nameof(NamingMap.SchemaName), schemaName);
            PushNamingMap(nameof(NamingMap.RepositoryNamespace), Resolve(Config.RepositoryNamespace));

            PushNamingMap(nameof(NamingMap.EntityProjectDirectory), Resolve(Config.EntityProjectDirectory));
            PushNamingMap(nameof(NamingMap.RepositoryProjectDirectory), Resolve(Config.RepositoryProjectDirectory));

            InitializeNamingPattern();

            Context.FireSchemaExportStarted();
            Context.OnSchemaExportFinished();
        }

        public void InitializeContext()
        {
            Context = new Context();

            InitializeDatabase();

            AttachEvents();
        }
        #endregion

        #region Methods
        void AttachEvents()
        {
            TableExportStarted += InitializeTableNamingPattern;

            Create<EntityFileExporter>().AttachEvents();

            Create<SharedFileExporter>().AttachEvents();

            if (Config.CanExportBoaRepository)
            {
                Create<BoaRepositoryFileExporter>().AttachEvents();
            }

            if (Config.CanExportAllSchemaInOneClassRepository)
            {
                Create<AllSchemaInOneClassRepositoryFileExporter>().AttachEvents();
            }

            SchemaExportStarted += ProcessAllTablesInSchema;

            if (Config.CanExportCsharpProjectFiles)
            {
                Create<EntityCsprojFileExporter>().AttachEvents();

                Create<RepositoryCsprojFileExporter>().AttachEvents();

                SchemaExportFinished += MsBuildQueue.Build;
            }
            
        }

       
        void InitializeDatabase()
        {
            Context.Connection = () => new SqlConnection(Config.ConnectionString);

            Context.Database = new SqlDatabase(Config.ConnectionString) {CommandTimeout = 1000 * 60 * 60};
        }

        void InitializeNamingPattern()
        {
        }

        void InitializeTableNamingPattern()
        {
            PushNamingMap(nameof(NamingMap.CamelCasedTableName), TableInfo.TableName.ToContractName());
        }

        bool IsReadyToExport(string tableName)
        {
            var fullTableName = $"{SchemaName}.{tableName}";

            var isNotExportable = Config.NotExportableTables?.Contains(fullTableName);
            if (isNotExportable == true)
            {
                return false;
            }

            return true;
        }

        void ProcessAllTablesInSchema()
        {
            var tableNames = SchemaInfo.GetAllTableNamesInSchema(Database, SchemaName).ToList();

            tableNames = tableNames.Where(IsReadyToExport).ToList();

            ProcessInfo.Total   = tableNames.Count;
            ProcessInfo.Current = 0;

            foreach (var tableName in tableNames)
            {
                ProcessInfo.Text = $"Generating codes for table '{tableName}'.";
                ProcessInfo.Current++;

                var tableInfoDao = new TableInfoDao {Database = Database, IndexInfoAccess = new IndexInfoAccess {Database = Database}};

                var tableInfoTemp = tableInfoDao.GetInfo(Config.TableCatalog, SchemaName, tableName);

                Context.TableInfo = GeneratorDataCreator.Create(Config.SqlSequenceInformationOfTable, Database, tableInfoTemp);

                Context.OnTableExportStarted();
                Context.OnTableExportFinished();
            }
        }
        #endregion
    }
}