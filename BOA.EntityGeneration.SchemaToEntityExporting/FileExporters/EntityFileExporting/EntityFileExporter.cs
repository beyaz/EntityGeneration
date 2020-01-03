using System.IO;
using BOA.EntityGeneration.EntityClassWriting;
using BOA.EntityGeneration.ScriptModel.Creators;
using DotNetStringUtilities;


namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.EntityFileExporting
{
    class EntityFileExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly EntityFileExporterConfig Config = EntityFileExporterConfig.LoadFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder file = new PaddedStringBuilder();
        #endregion

        #region Properties
        string ClassName     => Resolve(Config.ClassName);
        string NamespaceName => Resolve(Config.NamespaceName);
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            SchemaExportStarted += InitializeNamingForSchema;
            SchemaExportStarted += WriteUsingList;
            SchemaExportStarted += EmptyLine;
            SchemaExportStarted += BeginNamespace;

            TableExportStarted += InitializeNamingForTable;
            TableExportStarted += WriteClass;

            SchemaExportFinished += EndNamespace;
            SchemaExportFinished += ExportFileToDirectory;
        }
        #endregion

        #region Methods
        void BeginNamespace()
        {
            file.BeginNamespace(NamespaceName);
        }

        void EmptyLine()
        {
            file.AppendLine();
        }

        void EndNamespace()
        {
            file.EndNamespace();
        }

        void ExportFileToDirectory()
        {
            ProcessInfo.Text = "Exporting Entity classes.";

            var filePath = Resolve(Config.OutputFilePath);

            Context.EntityProjectSourceFileNames.Add(Path.GetFileName(filePath));

            var content = file.ToString();

            Context.OnEntityFileContentCompleted(content);

            FileSystem.WriteAllText(filePath, content);
        }

        void InitializeNamingForSchema()
        {
            PushNamingMap(nameof(NamingMap.EntityNamespace), NamespaceName);
        }

        void InitializeNamingForTable()
        {
            PushNamingMap(nameof(NamingMap.EntityClassName), ClassName);
        }

        void WriteClass()
        {
            var entityClass = new EntityClass
            {
                EntityClassComment          = EntityClassCommentMapper.CreateFrom(TableInfo),
                EntityContractBaseClassName = Config.EntityContractBase,
                ClassName                   = ClassName,
                PropertyList                = ContractPropertyInfoMapper.Map(TableInfo)
            };
            entityClass.Write(file);
        }

        void WriteUsingList()
        {
            foreach (var line in Config.UsingLines)
            {
                file.AppendLine(line);
            }
        }
        #endregion
    }
}