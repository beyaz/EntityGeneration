using System.IO;
using System.Linq;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.ScriptModel;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedFileExporting
{
    class SharedFileExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly SharedFileExporterConfig Config = SharedFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder sb = new PaddedStringBuilder();
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            Context.ProfileInfoInitialized += WriteUsingList;
            Context.ProfileInfoInitialized += EmptyLine;
            Context.ProfileInfoInitialized += BeginNamespace;
            Context.ProfileInfoInitialized += WriteEmbeddedClasses;

            Context.CustomSqlInfoInitialized += EmptyLine;
            Context.CustomSqlInfoInitialized += BeginClass;
            Context.CustomSqlInfoInitialized += CreateSqlInfo;
            Context.CustomSqlInfoInitialized += EmptyLine;
            Context.CustomSqlInfoInitialized += WriteReadContract;
            Context.CustomSqlInfoInitialized += EndClass;

            Context.ProfileInfoRemove += EndNamespace;
            Context.ProfileInfoRemove += ExportFileToDirectory;
        }
        #endregion

        #region Methods
        void BeginClass()
        {
            sb.AppendLine($"public static class {NamingMap.RepositoryClassName}");
            sb.OpenBracket();
        }

        void BeginNamespace()
        {
            sb.AppendLine($"namespace {NamingMap.RepositoryNamespace}.Shared");
            sb.OpenBracket();
        }

        void CreateSqlInfo()
        {
            sb.AppendLine($"public static SqlInfo CreateSqlInfo({NamingMap.InputClassName} request)");
            sb.AppendLine("{");
            sb.PaddingCount++;

            sb.AppendLine("const string sql = @\"");
            sb.AppendAll(CustomSqlInfo.Sql);
            sb.AppendLine();
            sb.AppendLine("\";");
            sb.AppendLine();
            sb.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

            if (CustomSqlInfo.Parameters.Any())
            {
                sb.AppendLine();
                foreach (var item in CustomSqlInfo.Parameters)
                {
                    sb.AppendLine($"sqlInfo.AddInParameter(\"@{item.Name}\", SqlDbType.{item.SqlDbTypeName}, request.{item.ValueAccessPathForAddInParameter});");
                }
            }

            sb.AppendLine();
            sb.AppendLine("return sqlInfo;");

            sb.PaddingCount--;
            sb.AppendLine("}");
        }

        void EmptyLine()
        {
            sb.AppendLine();
        }

        void EndClass()
        {
            sb.CloseBracket();
        }

        void EndNamespace()
        {
            sb.CloseBracket();
        }

        void ExportFileToDirectory()
        {
            ProcessInfo.Text = "Exporting Shared repository.";

            var filePath = Resolve(Config.OutputFilePath);

            Context.RepositoryProjectSourceFileNames.Add(Path.GetFileName(filePath));

            FileSystem.WriteAllText(filePath, sb.ToString());
        }

        void WriteEmbeddedClasses()
        {
            sb.AppendAll(Config.EmbeddedCodes);
            sb.AppendLine();
        }

        void WriteReadContract()
        {
            if (CustomSqlInfo.ResultContractIsReferencedToEntity)
            {
                return;
            }

            sb.AppendLine("/// <summary>");
            sb.AppendLine($"///{Padding.ForComment}Maps reader columns to contract for '{CustomSqlInfo.Name}' sql.");
            sb.AppendLine("/// </summary>");
            sb.AppendLine($"public static void ReadContract(IDataReader reader, {NamingMap.ResultClassName} contract)");
            sb.OpenBracket();

            foreach (var item in CustomSqlInfo.ResultColumns)
            {
                if (item.IsReferenceToEntity)
                {
                    Context.RepositoryAssemblyReferences.Add(ReferencedEntityTypeNamingPattern.ReferencedRepositoryAssemblyPath);
                    Context.RepositoryAssemblyReferences.Add(ReferencedEntityTypeNamingPattern.ReferencedEntityAssemblyPath);

                    sb.AppendLine($"contract.{item.NameInDotnet} = new {ReferencedEntityTypeNamingPattern.ReferencedEntityAccessPath}();");
                    sb.AppendLine($"{ReferencedEntityTypeNamingPattern.ReferencedEntityReaderMethodPath}(reader, contract.{item.NameInDotnet});");
                    continue;
                }

                var readerMethodName = item.SqlReaderMethod.ToString();
                if (item.SqlReaderMethod == SqlReaderMethods.GetGUIDValue)
                {
                    readerMethodName = "GetGuidValue";
                }

                sb.AppendLine($"contract.{item.NameInDotnet} = reader.{readerMethodName}(\"{item.Name}\");");
            }

            sb.CloseBracket();
        }

        void WriteUsingList()
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine($"using {NamingMap.EntityNamespace};");
        }
        #endregion
    }
}