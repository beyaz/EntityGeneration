using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOA.Common.Helpers;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.ScriptModel;
using BOA.EntityGeneration.ScriptModel.Creators;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.BankingRepositoryFileExporting
{
    class BoaRepositoryFileExporter : ContextContainer
    {
        #region Constants
        const string contractParameterName = "contract";
        #endregion

        #region Static Fields
        static readonly BoaRepositoryFileExporterConfig Config = BoaRepositoryFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder file = new PaddedStringBuilder();
        #endregion

        #region Properties
        string ClassName => NamingMap.Resolve(Config.ClassNamePattern);

        string RepositoryNamespace => NamingMap.RepositoryNamespace;

        string sharedRepositoryClassAccessPath => NamingMap.Resolve(Config.SharedRepositoryClassAccessPath);
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            SchemaExportStarted += AddAssemblyReferencesToProject;
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
        internal static void WriteDefaultValues(PaddedStringBuilder file, IReadOnlyDictionary<string, string> defaultValueMap, IReadOnlyList<IColumnInfo> parameters)
        {
            if (defaultValueMap == null)
            {
                return;
            }

            var contractInitializations = new List<string>();

            foreach (var columnInfo in parameters)
            {
                var          contractInstancePropertyName = columnInfo.ColumnName.ToContractName();
                const string contractInstanceName         = contractParameterName;

                var map = ConfigurationDictionaryCompiler.Compile(defaultValueMap, new Dictionary<string, string>
                {
                    {nameof(contractInstanceName), contractInstanceName},
                    {nameof(contractInstancePropertyName), contractInstancePropertyName}
                });

                var key = columnInfo.ColumnName;
                if (map.ContainsKey(key))
                {
                    contractInitializations.Add(map[key]);
                    continue;
                }

                key = columnInfo.DotNetType + ":" + columnInfo.ColumnName;
                if (map.ContainsKey(key))
                {
                    contractInitializations.Add(map[key]);
                }
            }

            if (contractInitializations.Any())
            {
                file.AppendLine();
                foreach (var item in contractInitializations)
                {
                    file.AppendLine(item);
                }
            }
        }

        void AddAssemblyReferencesToProject()
        {
            Context.RepositoryAssemblyReferences.AddRange(Config.ExtraAssemblyReferences);
        }

        void BeginNamespace()
        {
            file.BeginNamespace(RepositoryNamespace);
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
            var sourceCode = file.ToString();

            ProcessInfo.Text = "Exporting Boa repository...";

            var outputFilePath = Resolve(Config.OutputFilePath);

            Context.RepositoryProjectSourceFileNames.Add(Path.GetFileName(outputFilePath));

            FileSystem.WriteAllText(outputFilePath, sourceCode);
        }

        void WriteClass()
        {
            ContractCommentInfoCreator.Write(file, TableInfo);
            file.AppendLine($"public sealed class {ClassName} : ObjectHelper");
            file.OpenBracket();

            ContractCommentInfoCreator.Write(file, TableInfo);
            file.AppendLine($"public {ClassName}(ExecutionDataContext context) : base(context) {{ }}");

            WriteInsertMethod();
            WriteBulkInsertMethod();
            WriteSelectByKeyMethod();
            WriteSelectByIndexMethods();
            WriteSelectAllMethod();
            WriteSelectAllByValidFlagMethod();
            WriteUpdateByKeyMethod();
            WriteDeleteByKeyMethod();

            file.CloseBracket();
        }

        void WriteDeleteByKeyMethod()
        {
            if (!TableInfo.IsSupportSelectByKey)
            {
                return;
            }

            const string methodName = "Delete";

            var deleteByKeyInfo                  = DeleteInfoCreator.Create(TableInfo);
            var sqlParameters                    = deleteByKeyInfo.SqlParameters;
            var callerMemberPath                 = $"{RepositoryNamespace}.{ClassName}.{methodName}";
            var parameterDefinitionPart          = string.Join(", ", sqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));
            var sharedMethodInvocationParameters = string.Join(", ", sqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Deletes only one record by given primary keys.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public GenericResponse<int> {methodName}({parameterDefinitionPart})");
            file.OpenBracket();
            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Delete({sharedMethodInvocationParameters});");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine("return this.ExecuteNonQuery(CallerMemberPath, sqlInfo);");
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

            var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.Insert";

            var insertInfo = new InsertInfoCreator {ExcludedColumnNames = Config.ExcludedColumnNamesWhenInsertOperation}.Create(TableInfo);

            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Inserts new record into table.");
            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine($"///{Padding.ForComment} <para>Automatically initialize '{sequenceInfo.TargetColumnName.ToContractName()}' property by using '{sequenceInfo.Name}' sequence.</para>");
            }

            file.AppendLine("/// </summary>");
            file.AppendLine($"public GenericResponse<int> Insert({typeContractName} {contractParameterName})");
            file.OpenBracket();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();

            file.AppendLine("if (contract == null)");
            file.AppendLine("{");
            file.AppendLine("    return this.ContractCannotBeNull(CallerMemberPath);");
            file.AppendLine("}");

            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine();

                file.OpenBracket();

                file.AppendLine($"// Init sequence for {sequenceInfo.TargetColumnName.ToContractName()}");
                file.AppendLine();
                file.AppendLine($"const string sqlNextSequence = @\"SELECT NEXT VALUE FOR {sequenceInfo.Name}\";");
                file.AppendLine();
                file.AppendLine($"const string callerMemberPath = \"{callerMemberPath} -> sqlNextSequence -> {sequenceInfo.Name}\";");
                file.AppendLine();

                file.AppendLine("var responseSequence = this.ExecuteScalar<object>(callerMemberPath, new SqlInfo {CommandText = sqlNextSequence});");
                file.AppendLine("if (!responseSequence.Success)");
                file.AppendLine("{");
                file.AppendLine("    return this.SequenceFetchError(responseSequence, callerMemberPath);");
                file.AppendLine("}");
                file.AppendLine();

                var columnInfo = TableInfo.Columns.First(x => x.ColumnName == sequenceInfo.TargetColumnName);
                if (columnInfo.DotNetType == DotNetTypeName.DotNetInt32 || columnInfo.DotNetType == DotNetTypeName.DotNetInt32Nullable)
                {
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt32(responseSequence.Value);");
                }
                else if (columnInfo.DotNetType == DotNetTypeName.DotNetStringName)
                {
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToString(responseSequence.Value);");
                }
                else
                {
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt64(responseSequence.Value);");
                }

                file.CloseBracket();
            }

            if (insertInfo.SqlParameters.Any())
            {
                WriteDefaultValues(file, Config.DefaultValuesForInsertMethod, insertInfo.SqlParameters);
            }

            file.AppendLine();
            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Insert({contractParameterName});");

            file.AppendLine();
            if (TableInfo.HasIdentityColumn)
            {
                file.AppendLine("var response = this.ExecuteScalar<int>(CallerMemberPath, sqlInfo);");
                file.AppendLine();
                file.AppendLine($"{contractParameterName}.{TableInfo.IdentityColumn.ColumnName.ToContractName()} = response.Value;");
                file.AppendLine();
                file.AppendLine("return response;");
            }
            else
            {
                file.AppendLine("return this.ExecuteNonQuery(CallerMemberPath, sqlInfo);");
            }

            file.CloseBracket();
        }

        void WriteBulkInsertMethod()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.Insert";

            var insertInfo = new InsertInfoCreator {ExcludedColumnNames = Config.ExcludedColumnNamesWhenInsertOperation}.Create(TableInfo);

            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Inserts new record into table.");
            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine($"///{Padding.ForComment} <para>Automatically initialize '{sequenceInfo.TargetColumnName.ToContractName()}' property by using '{sequenceInfo.Name}' sequence.</para>");
            }

            file.AppendLine("/// </summary>");
            file.AppendLine($"public ResponseBase Insert(IList<{typeContractName}> contracts)");
            file.OpenBracket();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();

            file.AppendLine("if (contracts == null)");
            file.AppendLine("{");
            file.AppendLine("    return this.ContractCannotBeNull(CallerMemberPath);");
            file.AppendLine("}");

            
            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine();

                file.OpenBracket();



                file.AppendLine($"// Init sequence for {sequenceInfo.TargetColumnName.ToContractName()}");
                file.AppendLine($"const string callerMemberPath = \"{callerMemberPath} -> sqlNextSequence -> {sequenceInfo.Name}\";");
                file.AppendLine();

                file.AppendLine($"var responseSequence = this.GetSequenceList(callerMemberPath, \"{sequenceInfo.Name}\", contracts.Count);");
                file.AppendLine("if (!responseSequence.Success)");
                file.AppendLine("{");
                file.AppendLine("    return this.SequenceFetchError(responseSequence, callerMemberPath);");
                file.AppendLine("}");
                file.AppendLine();

                file.AppendLine("for(var i = 0; i < contracts.Count; i++)");
                file.OpenBracket();
                var columnInfo = TableInfo.Columns.First(x => x.ColumnName == sequenceInfo.TargetColumnName);
                if (columnInfo.DotNetType == DotNetTypeName.DotNetInt32 || columnInfo.DotNetType == DotNetTypeName.DotNetInt32Nullable)
                {
                    file.AppendLine($"contracts[i].{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt32(responseSequence.Value[i]);");
                }
                else if (columnInfo.DotNetType == DotNetTypeName.DotNetStringName)
                {
                    file.AppendLine($"contracts[i].{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToString(responseSequence.Value[i]);");
                }
                else
                {
                    file.AppendLine($"contracts[i].{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt64(responseSequence.Value[i]);");
                }
                file.CloseBracket();

               

                file.CloseBracket();
            }

            if (insertInfo.SqlParameters.Any())
            {
                file.AppendLine();
                file.AppendLine("foreach( var contract in contracts)");
                file.OpenBracket();
                WriteDefaultValues(file, Config.DefaultValuesForInsertMethod, insertInfo.SqlParameters);
                file.CloseBracket();
            }

            file.AppendLine();
            file.AppendLine($"var dt = new DataTable(\"{TableInfo.SchemaName}.{TableInfo.TableName}\");");
            foreach (var c in TableInfo.Columns)
            {
                var dataColumnName = $"\"{c.ColumnName}\"";

                if (c.SqlReaderMethod == SqlReaderMethods.GetBooleanNullableValueFromChar)
                {
                    file.AppendLine("dt.Columns.Add(new DataColumn(" + dataColumnName + ", typeof(string)) { AllowDBNull = true});");
                }
                else if (c.SqlReaderMethod == SqlReaderMethods.GetBooleanValueFromChar)
                {
                    file.AppendLine("dt.Columns.Add(new DataColumn(" + dataColumnName + ", typeof(string)));");
                }
                else if (c.IsNullable)
                {
                    file.AppendLine("dt.Columns.Add(new DataColumn(" + dataColumnName + ", typeof(" + c.DotNetType.RemoveFromEnd("?") + ")) { AllowDBNull = true});");
                }
                else
                {
                    file.AppendLine($"dt.Columns.Add({dataColumnName}, typeof({c.DotNetType}));");
                }
            }
            file.AppendLine();
            file.AppendLine("foreach (var contract in contracts)");
            file.OpenBracket();
            file.AppendLine("var dataRow = dt.NewRow();");
            file.AppendLine();

            foreach (var c in TableInfo.Columns)
            {
                var propertyName   = c.ColumnName.ToContractName();
                var dataColumnName = $"\"{c.ColumnName}\"";

                if (c.SqlReaderMethod == SqlReaderMethods.GetBooleanValueFromChar)
                {
                    file.AppendLine($"dataRow[{dataColumnName}] = contract.{propertyName} ? \"1\" : \"0\";");
                }
                else if (c.SqlReaderMethod == SqlReaderMethods.GetBooleanNullableValueFromChar)
                {
                    file.AppendLine($"if (contract.{propertyName} != null)");
                    file.OpenBracket();
                    file.AppendLine($"dataRow[{dataColumnName}] = contract.{propertyName}.Value ? \"1\" : \"0\";");
                    file.CloseBracket();
                }
                else if (c.IsNullable)
                {
                    file.AppendLine($"if (contract.{propertyName} != null)");
                    file.OpenBracket();
                    file.AppendLine($"dataRow[{dataColumnName}] = contract.{propertyName};");
                    file.CloseBracket();
                }
                else
                {
                    file.AppendLine($"dataRow[{dataColumnName}] = contract.{propertyName};");
                }
            }

            file.AppendLine("dt.Rows.Add(dataRow);");
            file.CloseBracket();

            file.AppendLine();
            
            file.AppendLine("return this.ExecuteBulkInsert(CallerMemberPath, dt);");

            file.CloseBracket();
        }

        void WriteSelectAllByValidFlagMethod()
        {
            if (!TableInfo.ShouldGenerateSelectAllByValidFlagMethodInBusinessClass)
            {
                return;
            }

            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.SelectByValidFlag";

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Selects all records in table {TableInfo.SchemaName}{TableInfo.TableName} where ValidFlag is true.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public GenericResponse<List<{typeContractName}>> SelectByValidFlag()");
            file.OpenBracket();

            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.SelectByValidFlag();");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine($"return this.ExecuteReaderToList<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

            file.CloseBracket();
        }

        void WriteSelectAllMethod()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.Select";

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Selects all records from table '{SchemaName}.{TableInfo.TableName}'.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public GenericResponse<List<{typeContractName}>> Select()");
            file.OpenBracket();

            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Select();");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine($"return this.ExecuteReaderToList<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

            file.CloseBracket();
        }

        void WriteSelectByIndexMethods()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            foreach (var indexIdentifier in TableInfo.UniqueIndexInfoList)
            {
                var indexInfo = SelectByIndexInfoCreator.Create(TableInfo, indexIdentifier);

                var parameterDefinitionPart = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

                var methodName = "SelectBy" + string.Join(string.Empty, indexInfo.SqlParameters.Select(x => $"{x.ColumnName.ToContractName()}"));

                file.AppendLine();
                file.AppendLine("/// <summary>");
                file.AppendLine($"///{Padding.ForComment} Selects records by given parameters.");
                file.AppendLine("/// </summary>");
                file.AppendLine($"public GenericResponse<{typeContractName}> {methodName}({parameterDefinitionPart})");
                file.OpenBracket();

                var sharedMethodInvocationParameters = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

                var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.{methodName}";

                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.{methodName}({sharedMethodInvocationParameters});");
                file.AppendLine();
                file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
                file.AppendLine();
                file.AppendLine($"return this.ExecuteReaderToContract<{typeContractName}>( CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

                file.CloseBracket();
            }

            foreach (var indexIdentifier in TableInfo.NonUniqueIndexInfoList)
            {
                var indexInfo = SelectByIndexInfoCreator.Create(TableInfo, indexIdentifier);

                var parameterDefinitionPart = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

                var methodName = "SelectBy" + string.Join(string.Empty, indexInfo.SqlParameters.Select(x => $"{x.ColumnName.ToContractName()}"));

                var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.{methodName}";

                file.AppendLine();
                file.AppendLine("/// <summary>");
                file.AppendLine($"///{Padding.ForComment} Selects records by given parameters.");
                file.AppendLine("/// </summary>");
                file.AppendLine($"public GenericResponse<List<{typeContractName}>> {methodName}({parameterDefinitionPart})");
                file.OpenBracket();

                var sharedMethodInvocationParameters = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.{methodName}({sharedMethodInvocationParameters});");
                file.AppendLine();
                file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
                file.AppendLine();
                file.AppendLine($"return this.ExecuteReaderToList<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

                file.CloseBracket();
            }
        }

        void WriteSelectByKeyMethod()
        {
            if (!TableInfo.IsSupportSelectByKey)
            {
                return;
            }

            var typeContractName       = TableEntityClassNameForMethodParametersInRepositoryFiles;
            var selectByPrimaryKeyInfo = SelectByPrimaryKeyInfoCreator.Create(TableInfo);

            var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.SelectByKey";

            var parameterPart = string.Join(", ", selectByPrimaryKeyInfo.SqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Selects record by primary keys.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public GenericResponse<{typeContractName}> SelectByKey({parameterPart})");
            file.AppendLine("{");
            file.PaddingCount++;

            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.SelectByKey({string.Join(", ", selectByPrimaryKeyInfo.SqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"))});");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine($"return this.ExecuteReaderToContract<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

            file.PaddingCount--;
            file.AppendLine("}");
        }

        void WriteUpdateByKeyMethod()
        {
            if (!TableInfo.IsSupportSelectByKey)
            {
                return;
            }

            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{RepositoryNamespace}.{ClassName}.Update";

            var updateInfo = UpdateByPrimaryKeyInfoCreator.Create(TableInfo);

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Updates only one record by given primary keys.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public GenericResponse<int> Update({typeContractName} {contractParameterName})");
            file.OpenBracket();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine("if (contract == null)");
            file.AppendLine("{");
            file.AppendLine("    return this.ContractCannotBeNull(CallerMemberPath);");
            file.AppendLine("}");

            if (updateInfo.SqlParameters.Any())
            {
                WriteDefaultValues(file, Config.DefaultValuesForUpdateByKeyMethod, updateInfo.SqlParameters);
            }

            file.AppendLine();
            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Update({contractParameterName});");
            file.AppendLine();
            file.AppendLine("return this.ExecuteNonQuery(CallerMemberPath, sqlInfo);");
            file.CloseBracket();
        }

        void WriteUsingList()
        {
            foreach (var line in Config.UsingLines)
            {
                file.AppendLine(NamingMap.Resolve(line));
            }

            if (Config.SchemaSpecificUsingLines.ContainsKey(Context.SchemaName))
            {
                foreach (var line in Config.SchemaSpecificUsingLines[Context.SchemaName])
                {
                    file.AppendLine(NamingMap.Resolve(line));
                }
            }
        }
        #endregion
    }
}