using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedClassExporting;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.ScriptModel;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedClassExporting
{
    class SharedClassWriterSqlParameter
    {
        public string    Name                             { get; set; }
        public SqlDbType SqlDbTypeName                    { get; set; }
        public string    ValueAccessPathForAddInParameter { get; set; }
    }
    class SharedClassWriterResultColumn
    {
        public bool             IsReferenceToEntity { get; set; }
        public string           NameInDotnet        { get; set; }
        public SqlReaderMethods SqlReaderMethod     { get; set; }
        public string           Name                { get; set; }
    }
    class SharedClassWriter
    {
        public PaddedStringBuilder                          Output                             { get; set; }
        public string                                       RepositoryClassName                { get; set; }
        public string                                       InputClassName                     { get; set; }
        public string                                       Sql                                { get; set; }
        public IReadOnlyList<SharedClassWriterSqlParameter> SqlParameters                      { get; set; }
        public bool                                         ResultContractIsReferencedToEntity { get; set; }
        public string                                       Name                               { get; set; }
        public string                                       ResultClassName                    { get; set; }
        public IReadOnlyList<SharedClassWriterResultColumn> ResultColumns                      { get; set; }
        public string                                       ReferencedEntityAccessPath, ReferencedEntityReaderMethodPath, ReferencedEntityAssemblyPath, ReferencedRepositoryAssemblyPath;
        public Action<string>                               AddRepositoryAssemblyReference;
    }
}
namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.SharedFileExporting
{
   
    class SharedFileExporter : ContextContainer
    {
        #region Static Fields
        internal static readonly SharedFileExporterConfig Config = SharedFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder Output = new PaddedStringBuilder();
        #endregion

        SharedClassWriter CreateNewSharedClassWriter()
        {
            return new SharedClassWriter
            {
                Output              = Output,
                RepositoryClassName = NamingMap.RepositoryClassName,
                InputClassName      = NamingMap.InputClassName,
                Sql                 = CustomSqlInfo.Sql,
                SqlParameters = CustomSqlInfo.Parameters.Select(x=>new SharedClassWriterSqlParameter
                {
                    Name                             = x.Name,
                    SqlDbTypeName                    = x.SqlDbTypeName,
                    ValueAccessPathForAddInParameter = x.ValueAccessPathForAddInParameter,
                }).ToList(),

                ResultContractIsReferencedToEntity = CustomSqlInfo.ResultContractIsReferencedToEntity,
                Name                               = CustomSqlInfo.Name,
                ResultClassName                    = NamingMap.ResultClassName
            };
        }

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
            Output.AppendLine($"public static class {NamingMap.RepositoryClassName}");
            Output.OpenBracket();
        }

        void BeginNamespace()
        {
            Output.AppendLine($"namespace {NamingMap.RepositoryNamespace}.Shared");
            Output.OpenBracket();
        }

        void CreateSqlInfo()
        {
            Output.AppendLine($"public static SqlInfo CreateSqlInfo({NamingMap.InputClassName} request)");
            Output.AppendLine("{");
            Output.PaddingCount++;

            Output.AppendLine("const string sql = @\"");
            Output.AppendAll(CustomSqlInfo.Sql);
            Output.AppendLine();
            Output.AppendLine("\";");
            Output.AppendLine();
            Output.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

            if (CustomSqlInfo.Parameters.Any())
            {
                Output.AppendLine();
                foreach (var item in CustomSqlInfo.Parameters)
                {
                    Output.AppendLine($"sqlInfo.AddInParameter(\"@{item.Name}\", SqlDbType.{item.SqlDbTypeName}, request.{item.ValueAccessPathForAddInParameter});");
                }
            }

            Output.AppendLine();
            Output.AppendLine("return sqlInfo;");

            Output.PaddingCount--;
            Output.AppendLine("}");
        }

        void EmptyLine()
        {
            Output.AppendLine();
        }

        void EndClass()
        {
            Output.CloseBracket();
        }

        void EndNamespace()
        {
            Output.CloseBracket();
        }

        void ExportFileToDirectory()
        {
            ProcessInfo.Text = "Exporting Shared repository.";

            var filePath = Resolve(Config.OutputFilePath);

            Context.RepositoryProjectSourceFileNames.Add(Path.GetFileName(filePath));

            FileSystem.WriteAllText(filePath, Output.ToString());
        }

        void WriteEmbeddedClasses()
        {
            Output.AppendAll(Config.EmbeddedCodes);
            Output.AppendLine();
        }

        void WriteReadContract()
        {
            if (CustomSqlInfo.ResultContractIsReferencedToEntity)
            {
                return;
            }

            Output.AppendLine("/// <summary>");
            Output.AppendLine($"///{Padding.ForComment}Maps reader columns to contract for '{CustomSqlInfo.Name}' sql.");
            Output.AppendLine("/// </summary>");
            Output.AppendLine($"public static void ReadContract(IDataReader reader, {NamingMap.ResultClassName} contract)");
            Output.OpenBracket();

            foreach (var item in CustomSqlInfo.ResultColumns)
            {
                if (item.IsReferenceToEntity)
                {
                    Context.RepositoryAssemblyReferences.Add(ReferencedEntityTypeNamingPattern.ReferencedRepositoryAssemblyPath);
                    Context.RepositoryAssemblyReferences.Add(ReferencedEntityTypeNamingPattern.ReferencedEntityAssemblyPath);

                    Output.AppendLine($"contract.{item.NameInDotnet} = new {ReferencedEntityTypeNamingPattern.ReferencedEntityAccessPath}();");
                    Output.AppendLine($"{ReferencedEntityTypeNamingPattern.ReferencedEntityReaderMethodPath}(reader, contract.{item.NameInDotnet});");
                    continue;
                }

                var readerMethodName = item.SqlReaderMethod.ToString();
                if (item.SqlReaderMethod == SqlReaderMethods.GetGUIDValue)
                {
                    readerMethodName = "GetGuidValue";
                }

                Output.AppendLine($"contract.{item.NameInDotnet} = reader.{readerMethodName}(\"{item.Name}\");");
            }

            Output.CloseBracket();
        }

        void WriteUsingList()
        {
            Output.AppendLine("using System;");
            Output.AppendLine("using System.Data;");
            Output.AppendLine("using System.Data.SqlClient;");
            Output.AppendLine("using System.Collections.Generic;");
            Output.AppendLine($"using {NamingMap.EntityNamespace};");
        }
        #endregion
    }
}