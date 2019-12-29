using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.ScriptModel;
using BOA.EntityGeneration.ScriptModel.Creators;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.SharedFileExporting
{
    class SharedFileExporter : ContextContainer
    {
        #region Static Fields
        static readonly SharedFileExporterConfig Config = SharedFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder file;
        #endregion

        #region Constructors
        public SharedFileExporter()
        {
            file = new PaddedStringBuilder();
        }
        #endregion

        #region Properties
        string ClassName => NamingMap.Resolve(Config.ClassNamePattern);
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            SchemaExportStarted += WriteUsingList;
            SchemaExportStarted += EmptyLine;
            SchemaExportStarted += BeginNamespace;
            SchemaExportStarted += WriteEmbeddedClasses;

            TableExportStarted += WriteClass;

            SchemaExportFinished += EndNamespace;
            SchemaExportFinished += ExportFileToDirectory;
        }
        #endregion

        #region Methods
        internal static string GetMethodName(SelectByIndexInfo indexInfo)
        {
            return "SelectBy" + string.Join(string.Empty, indexInfo.SqlParameters.Select(x => $"{x.ColumnName.ToContractName()}"));
        }

        void BeginNamespace()
        {
            file.BeginNamespace(NamingMap.RepositoryNamespace + ".Shared");
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
            var outputFilePath = Resolve(Config.OutputFilePath);

            var sourceCode = file.ToString();

            Context.OnSharedRepositoryFileContentCompleted(sourceCode);

            ProcessInfo.Text = "Exporting Shared repository...";

            Context.RepositoryProjectSourceFileNames.Add(Path.GetFileName(outputFilePath));


            FileSystem.WriteAllText(outputFilePath, sourceCode);
        }

        void WriteClass()
        {
            file.AppendLine($"public sealed class {ClassName}");
            file.OpenBracket();

            if (TableInfo.IsSupportSelectByKey)
            {
                file.AppendLine();
                WriteDeleteByKeyMethod();

                file.AppendLine();
                WriteSelectByKeyMethod();

                WriteUpdateByPrimaryKeyMethod();
            }

            WriteSelectByIndexMethods();

            WriteSelectAllMethod();

            if (TableInfo.ShouldGenerateSelectAllByValidFlagMethodInBusinessClass)
            {
                WriteSelectAllByValidFlagMethod();
            }

            WriteInsertMethod();

            WriteReadContractMethod();

            file.CloseBracket();
        }

        void WriteDeleteByKeyMethod()
        {
            var deleteInfo = DeleteInfoCreator.Create(TableInfo);

            var sqlParameters = deleteInfo.SqlParameters;

            var parameterPart = string.Join(", ", sqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

            file.AppendLine($"public static SqlInfo Delete({parameterPart})");
            file.OpenBracket();

            file.AppendLine($"const string sql = \"{deleteInfo.Sql}\";");
            file.AppendLine();

            file.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

            if (sqlParameters.Any())
            {
                file.AppendLine();
                foreach (var columnInfo in sqlParameters)
                {
                    file.AppendLine($"sqlInfo.AddInParameter(\"@{columnInfo.ColumnName}\", SqlDbType.{columnInfo.SqlDbType}, {columnInfo.ColumnName.AsMethodParameter()});");
                }
            }

            file.AppendLine();
            file.AppendLine("return sqlInfo;");

            file.CloseBracket();
        }

        void WriteEmbeddedClasses()
        {
            file.AppendAll(Config.EmbeddedCodes);
            file.AppendLine();
        }

        void WriteInsertMethod()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var insertInfo = new InsertInfoCreator().Create(TableInfo);

            var sqlParameters = insertInfo.SqlParameters;

            file.AppendLine($"public static SqlInfo Insert({typeContractName} contract)");
            file.OpenBracket();

            file.AppendLine("const string sql = @\"");
            file.AppendAll(insertInfo.Sql);
            file.AppendLine();
            if (TableInfo.HasIdentityColumn)
            {
                file.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS INT)");
            }

            file.AppendLine("\";");
            file.AppendLine();

            file.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

            if (sqlParameters.Any())
            {
                file.AppendLine();
                foreach (var columnInfo in sqlParameters)
                {
                    file.AppendLine($"sqlInfo.AddInParameter(\"@{columnInfo.ColumnName}\", SqlDbType.{columnInfo.SqlDbType}, {ParameterHelper.GetValueForSqlInsert(columnInfo)});");
                }
            }

            file.AppendLine();
            file.AppendLine("return sqlInfo;");

            file.CloseBracket();
        }

        void WriteReadContractMethod()
        {
            const string contractParameterName = "contract";

            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Reads one record from reader");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public static void ReadContract(IDataReader reader, {typeContractName} {contractParameterName})");
            file.AppendLine("{");
            file.PaddingCount++;

            foreach (var columnInfo in TableInfo.Columns)
            {
                var readerMethodName = columnInfo.SqlReaderMethod.ToString();
                if (columnInfo.SqlReaderMethod == SqlReaderMethods.GetGUIDValue)
                {
                    readerMethodName = "GetGuidValue";
                }

                var contractReadLine = Config.ContractReadLine
                                             .Replace("$(Contract)", contractParameterName)
                                             .Replace("$(PropertyName)", columnInfo.ColumnName.ToContractName())
                                             .Replace("$(ColumnName)", columnInfo.ColumnName)
                                             .Replace("$(SqlReaderMethod)", readerMethodName);

                file.AppendLine(contractReadLine);
            }

            file.PaddingCount--;
            file.AppendLine("}");
        }

        void WriteSelectAllByValidFlagMethod()
        {
            var sql = SelectAllInfoCreator.Create(TableInfo).Sql;

            file.AppendLine("public static SqlInfo SelectByValidFlag()");
            file.OpenBracket();

            file.AppendLine("const string sql = @\"");
            file.AppendAll(sql + " WHERE [VALID_FLAG] = '1'");
            file.AppendLine();
            file.AppendLine("\";");
            file.AppendLine();

            file.AppendLine("return new SqlInfo { CommandText = sql };");
            file.CloseBracket();
        }

        void WriteSelectAllMethod()
        {
            var sql = SelectAllInfoCreator.Create(TableInfo).Sql;

            file.AppendLine("public static SqlInfo Select()");
            file.OpenBracket();

            file.AppendLine("const string sql = @\"");
            file.AppendAll(sql);
            file.AppendLine();
            file.AppendLine("\";");
            file.AppendLine();

            file.AppendLine("return new SqlInfo { CommandText = sql };");
            file.CloseBracket();
        }

        void WriteSelectByIndexMethods()
        {
            var allIndexes = new List<IIndexInfo>();
            allIndexes.AddRange(TableInfo.NonUniqueIndexInfoList);
            allIndexes.AddRange(TableInfo.UniqueIndexInfoList);

            foreach (var indexIdentifier in allIndexes)
            {
                var indexInfo     = SelectByIndexInfoCreator.Create(TableInfo, indexIdentifier);
                var methodName    = GetMethodName(indexInfo);
                var parameterPart = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

                file.AppendLine();
                file.AppendLine("/// <summary>");
                file.AppendLine($"///{Padding.ForComment} Selects records by given parameters.");
                file.AppendLine("/// </summary>");
                file.AppendLine($"public static SqlInfo {methodName}({parameterPart})");
                file.OpenBracket();

                file.AppendLine("const string sql = @\"");
                file.AppendAll(indexInfo.Sql);
                file.AppendLine();
                file.AppendLine("\";");
                file.AppendLine();
                file.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

                if (indexInfo.SqlParameters.Any())
                {
                    file.AppendLine();
                    foreach (var columnInfo in indexInfo.SqlParameters)
                    {
                        file.AppendLine($"sqlInfo.AddInParameter(\"@{columnInfo.ColumnName}\", SqlDbType.{columnInfo.SqlDbType}, {columnInfo.ColumnName.AsMethodParameter()});");
                    }
                }

                file.AppendLine();
                file.AppendLine("return sqlInfo;");

                file.CloseBracket();
            }
        }

        void WriteSelectByKeyMethod()
        {
            var selectByPrimaryKeyInfo = SelectByPrimaryKeyInfoCreator.Create(TableInfo);

            var sqlParameters = selectByPrimaryKeyInfo.SqlParameters;
            var parameterPart = string.Join(", ", sqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

            file.AppendLine($"public static SqlInfo SelectByKey({parameterPart})");
            file.OpenBracket();

            file.AppendLine("const string sql = @\"");
            file.AppendAll(selectByPrimaryKeyInfo.Sql);
            file.AppendLine();
            file.AppendLine("\";");
            file.AppendLine();

            file.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

            if (sqlParameters.Any())
            {
                file.AppendLine();
                foreach (var columnInfo in sqlParameters)
                {
                    file.AppendLine($"sqlInfo.AddInParameter(\"@{columnInfo.ColumnName}\", SqlDbType.{columnInfo.SqlDbType}, {columnInfo.ColumnName.AsMethodParameter()});");
                }
            }

            file.AppendLine();
            file.AppendLine("return sqlInfo;");

            file.CloseBracket();
        }

        void WriteUpdateByPrimaryKeyMethod()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var updateInfo = UpdateByPrimaryKeyInfoCreator.Create(TableInfo);

            var sqlParameters = updateInfo.SqlParameters;

            file.AppendLine($"public static SqlInfo Update({typeContractName} contract)");
            file.OpenBracket();

            file.AppendLine("const string sql = @\"");
            file.AppendAll(updateInfo.Sql);
            file.AppendLine();
            file.AppendLine("\";");
            file.AppendLine();

            file.AppendLine("var sqlInfo = new SqlInfo { CommandText = sql };");

            if (sqlParameters.Any())
            {
                file.AppendLine();
                foreach (var columnInfo in sqlParameters)
                {
                    file.AppendLine($"sqlInfo.AddInParameter(\"@{columnInfo.ColumnName}\", SqlDbType.{columnInfo.SqlDbType}, {ParameterHelper.GetValueForSqlUpdate(columnInfo)});");
                }
            }

            file.AppendLine();
            file.AppendLine("return sqlInfo;");

            file.CloseBracket();
        }

        void WriteUsingList()
        {
            foreach (var line in Config.UsingLines)
            {
                file.AppendLine(NamingMap.Resolve(line));
            }
        }
        #endregion
    }
}