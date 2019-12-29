using System.IO;
using System.Linq;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.AllInOneClassRepositoryExporting
{
    class SourceFileExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly SourceFileExporterConfig Config = SourceFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder file = new PaddedStringBuilder();
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            Context.ProfileInfoInitialized += AddAssemblyReferencesToProject;
            Context.ProfileInfoInitialized += UsingList;
            Context.ProfileInfoInitialized += EmptyLine;
            Context.ProfileInfoInitialized += BeginNamespace;
            Context.ProfileInfoInitialized += WriteEmbeddedClasses;
            Context.ProfileInfoInitialized += BeginClass;

            Context.CustomSqlInfoInitialized += WriteCustomSqlToMethod;

            Context.ProfileInfoRemove += EndClass;
            Context.ProfileInfoRemove += EndNamespace;
            Context.ProfileInfoRemove += ExportFileToDirectory;
        }
        #endregion

        #region Methods
        void AddAssemblyReferencesToProject()
        {
            Context.RepositoryAssemblyReferences.AddRange(Config.ExtraAssemblyReferences.Select(Resolve));
        }

        void BeginClass()
        {
            file.AppendAll(Resolve(Config.ClassDefinitionBegin));
            file.AppendLine();
            file.PaddingCount++; // enter class body
        }

        void BeginNamespace()
        {
            file.BeginNamespace(Resolve(Config.NamespaceName));
        }

        void EmptyLine()
        {
            file.AppendLine();
        }

        void EndClass()
        {
            file.CloseBracket();
        }

        void EndNamespace()
        {
            file.CloseBracket();
        }

        void ExportFileToDirectory()
        {
            ProcessInfo.Text = "Exporting All in one class repository.";

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

        void WriteCustomSqlToMethod()
        {
            var methodName = Resolve(Config.MethodName);

            var hasZeroInput = Context.CustomSqlInfo.Parameters.Count == 0;

            var key = $"{Resolve(Config.NamespaceName)}.{Resolve(Config.ClassName)}.{methodName}";

            var sharedRepositoryClassAccessPath = Resolve(Config.SharedRepositoryClassAccessPath);

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
                if (hasZeroInput)
                {
                    file.AppendLine($"public List<{resultContractName}> {methodName}({NamingMap.InputClassName} request = null)");
                    file.OpenBracket();
                    file.AppendLine($"request = request ?? new {NamingMap.InputClassName}();");
                    file.AppendLine();
                }
                else
                {
                    file.AppendLine($"public List<{resultContractName}> {methodName}({NamingMap.InputClassName} request)");
                    file.OpenBracket();
                }
                file.AppendLine($"const string CallerMemberPath = \"{key}\";");
                file.AppendLine();
                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.CreateSqlInfo(request);");
                file.AppendLine();
                file.AppendLine($"return unitOfWork.ExecuteReaderToList<{resultContractName}>(CallerMemberPath, sqlInfo, {readContractMethodPath});");
                file.CloseBracket();
            }
            else
            {
                if (hasZeroInput)
                {
                    file.AppendLine($"public {resultContractName} {methodName}({NamingMap.InputClassName} request)");
                    file.OpenBracket();
                    file.AppendLine($"request = request ?? new {NamingMap.InputClassName}();");
                    file.AppendLine();
                }
                else
                {
                    file.AppendLine($"public {resultContractName} {methodName}({NamingMap.InputClassName} request)");
                    file.OpenBracket();
                }

                
                file.AppendLine($"const string CallerMemberPath = \"{key}\";");
                file.AppendLine();
                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.CreateSqlInfo(request);");
                file.AppendLine();
                file.AppendLine($"return unitOfWork.ExecuteReaderToContract<{resultContractName}>(CallerMemberPath, sqlInfo, {readContractMethodPath});");

                file.CloseBracket();
            }

            file.AppendLine();
        }

        void WriteEmbeddedClasses()
        {
            file.AppendAll(Config.EmbeddedCodes);
            file.AppendLine();
        }
        #endregion
    }
}