using System.IO;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using BOA.EntityGeneration.ScriptModel;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.BoaRepositoryExporting
{
    class BoaRepositoryFileExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly BoaRepositoryFileExporterConfig Config = BoaRepositoryFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder file = new PaddedStringBuilder();
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            var customSqlClassGenerator = Create<CustomSqlClassGenerator>();

            customSqlClassGenerator.AttachEvents();

            Context.ProfileInfoInitialized += UsingList;
            Context.ProfileInfoInitialized += EmptyLine;
            Context.ProfileInfoInitialized += BeginNamespace;
            Context.ProfileInfoInitialized += WriteEmbeddedClasses;

            Context.CustomSqlInfoInitialized += WriteBoaRepositoryClass;

            Context.ProfileInfoRemove += () =>
            {
                var proxyClass = customSqlClassGenerator.sb.ToString();
                file.AppendAll(proxyClass);
                file.AppendLine();
            };
            Context.ProfileInfoRemove += EndNamespace;
            Context.ProfileInfoRemove += ExportFileToDirectory;
        }
        #endregion

        #region Methods
        void BeginNamespace()
        {
            file.BeginNamespace(NamingMap.RepositoryNamespace);
        }

        void EmptyLine()
        {
            file.AppendLine();
        }

        void EndNamespace()
        {
            file.CloseBracket();
        }

        void ExportFileToDirectory()
        {
            ProcessInfo.Text = "Exporting BOA repository.";

            var filePath = Resolve(Config.OutputFilePath);

            Context.RepositoryProjectSourceFileNames.Add(Path.GetFileName(filePath));

            FileSystem.WriteAllText(filePath, file.ToString());
        }

        void UsingList()
        {
            foreach (var line in Config.UsingLines)
            {
                file.AppendLine(Resolve(line));
            }
        }

        void WriteBoaRepositoryClass()
        {
            var key = $"{NamingMap.RepositoryNamespace}.{NamingMap.RepositoryClassName}.Execute";

            var sharedRepositoryClassAccessPath = Resolve(Config.SharedRepositoryClassAccessPath);

            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment}Data access part of '{CustomSqlInfo.Name}' sql.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public sealed class {NamingMap.RepositoryClassName} : ObjectHelper");
            file.AppendLine("{");
            file.PaddingCount++;

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment}Data access part of '{CustomSqlInfo.Name}' sql.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public {NamingMap.RepositoryClassName}(ExecutionDataContext context) : base(context) {{}}");

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment}Data access part of '{CustomSqlInfo.Name}' sql.");
            file.AppendLine("/// </summary>");

            var resultContractName     = NamingMap.ResultClassName;
            var readContractMethodPath = $"{sharedRepositoryClassAccessPath}.ReadContract";

            if (CustomSqlInfo.ResultContractIsReferencedToEntity)
            {
                resultContractName     = ReferencedEntityTypeNamingPattern.ReferencedEntityAccessPath;
                readContractMethodPath = ReferencedEntityTypeNamingPattern.ReferencedEntityReaderMethodPath;

                Context.RepositoryAssemblyReferences.Add(ReferencedEntityTypeNamingPattern.ReferencedEntityAssemblyPath);
                Context.RepositoryAssemblyReferences.Add(ReferencedEntityTypeNamingPattern.ReferencedRepositoryAssemblyPath);
            }

            if (CustomSqlInfo.SqlResultIsCollection)
            {
                file.AppendLine($"public GenericResponse<List<{resultContractName}>> Execute({NamingMap.InputClassName} request)");
                file.OpenBracket();
                file.AppendLine($"const string CallerMemberPath = \"{key}\";");
                file.AppendLine();
                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.CreateSqlInfo(request);");
                file.AppendLine();
                file.AppendLine($"return this.ExecuteReaderToList<{resultContractName}>(CallerMemberPath, sqlInfo, {readContractMethodPath});");
                file.CloseBracket();
            }
            else
            {
                file.AppendLine($"public GenericResponse<{resultContractName}> Execute({NamingMap.InputClassName} request)");
                file.OpenBracket();
                file.AppendLine($"const string CallerMemberPath = \"{key}\";");
                file.AppendLine();
                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.CreateSqlInfo(request);");
                file.AppendLine();
                file.AppendLine($"return this.ExecuteReaderToContract<{resultContractName}>(CallerMemberPath, sqlInfo, {readContractMethodPath});");

                file.CloseBracket();
            }

            file.AppendLine();

            file.CloseBracket();
        }

        void WriteEmbeddedClasses()
        {
            file.AppendAll(Config.EmbeddedCodes);
            file.AppendLine();
        }
        #endregion
    }
}