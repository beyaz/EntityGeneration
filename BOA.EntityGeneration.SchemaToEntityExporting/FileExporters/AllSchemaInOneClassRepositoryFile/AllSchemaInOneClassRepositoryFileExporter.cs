using System.Collections.Generic;
using System.IO;
using System.Linq;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.BankingRepositoryFileExporting;
using BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.SharedFileExporting;
using BOA.EntityGeneration.ScriptModel;
using BOA.EntityGeneration.ScriptModel.Creators;
using Dapper;
using DotNetStringUtilities;
using Config = BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.AllSchemaInOneClassRepositoryFile.AllSchemaInOneClassRepositoryFileExporterConfig;

namespace BOA.EntityGeneration.SchemaToEntityExporting.FileExporters.AllSchemaInOneClassRepositoryFile
{
    class AllSchemaInOneClassRepositoryFileExporter : ContextContainer
    {
        #region Constants
        const string contractParameterName = "contract";
        #endregion

        #region Static Fields
        internal static readonly Config Config = AllSchemaInOneClassRepositoryFileExporterConfig.CreateFromFile();
        #endregion

        #region Fields
        readonly PaddedStringBuilder file = new PaddedStringBuilder();
        #endregion

        #region Properties
        string CamelCasedTableName => NamingMap.CamelCasedTableName;
        string ClassName           => NamingMap.Resolve(Config.ClassName);

        string FullClassName => NamespaceName + "." + ClassName;

        string NamespaceName => NamingMap.Resolve(Config.NamespaceName);

        string sharedRepositoryClassAccessPath => NamingMap.Resolve(Config.SharedRepositoryClassAccessPath);
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            

            SchemaExportStarted += InitializeNamingPattern;
            SchemaExportStarted += AddAssemblyReferencesToProject;
            SchemaExportStarted += WriteUsingList;
            SchemaExportStarted += EmptyLine;
            SchemaExportStarted += BeginNamespace;
            SchemaExportStarted += WriteEmbeddedClasses;
            SchemaExportStarted += EmptyLine;
            SchemaExportStarted += BeginClass;

            TableExportStarted += WriteClassMethods;

            SchemaExportFinished += WriteSchemaSequences;
            SchemaExportFinished += EndClass;
            SchemaExportFinished += EndNamespace;
            SchemaExportFinished += ExportFileToDirectory;
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
            file.BeginNamespace(NamespaceName);
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
            file.EndNamespace();
        }

        void ExportFileToDirectory()
        {
            var outputFilePath = NamingMap.Resolve(Config.OutputFilePath);

            var sourceCode = file.ToString();

            ProcessInfo.Text = "Exporting All In One Class repository...";

            Context.RepositoryProjectSourceFileNames.Add(Path.GetFileName(outputFilePath));

            FileSystem.WriteAllText(outputFilePath, sourceCode);
        }

        void InitializeNamingPattern()
        {
        }

        void WriteSchemaSequences()
        {
            ProcessInfo.Text = "Generating sequences";

            var sequenceNames = Context.Connection().Query<string>($"SELECT [name] FROM sys.SEQUENCES WHERE SCHEMA_NAME(schema_id) = '{SchemaName}'");
            foreach (var sequenceName in sequenceNames)
            {
                file.AppendLine($"public long Get{sequenceName.ToContractName()}NextValue()");
                file.OpenBracket();
                file.AppendLine($"return unitOfWork.GetSequenceNextValue(\"{SchemaName}\", \"{sequenceName}\");");
                file.CloseBracket();
            }
        }
        void WriteClassMethods()
        {
            WriteSelectByKeyMethod();
            WriteSelectByIndexMethods();
            WriteDeleteByKeyMethod();
            WriteUpdateByKeyMethod();
            WriteInsertMethod();
            WriteBulkInsertMethod();
            WriteSelectAllByValidFlagMethod();
            WriteSelectAllMethod();
        }

        void WriteSelectAllByValidFlagMethod()
        {
            if (!TableInfo.ShouldGenerateSelectAllByValidFlagMethodInBusinessClass)
            {
                return;
            }

            var methodName = $"Get{CamelCasedTableName}ByValidFlag";

            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{FullClassName}.{methodName}";

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Selects all records in table {TableInfo.SchemaName}{TableInfo.TableName} where ValidFlag is true.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public List<{typeContractName}> {methodName}()");
            file.OpenBracket();

            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.SelectByValidFlag();");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine($"return unitOfWork.ExecuteReaderToList<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

            file.CloseBracket();
        }

        void WriteSelectAllMethod()
        {
            var methodName = $"Get{CamelCasedTableName}";

            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{FullClassName}.{methodName}";

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Selects all records in table {TableInfo.SchemaName}{TableInfo.TableName}.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public List<{typeContractName}> {methodName}()");
            file.OpenBracket();

            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Select();");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine($"return unitOfWork.ExecuteReaderToList<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

            file.CloseBracket();
        }

        void WriteDeleteByKeyMethod()
        {
            if (!TableInfo.IsSupportSelectByKey)
            {
                return;
            }


            var methodName = "Delete" + CamelCasedTableName;

            var deleteByKeyInfo                  = DeleteInfoCreator.Create(TableInfo);
            var sqlParameters                    = deleteByKeyInfo.SqlParameters;
            var callerMemberPath                 = $"{FullClassName}.{methodName}";
            var parameterDefinitionPart          = string.Join(", ", sqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));
            var sharedMethodInvocationParameters = string.Join(", ", sqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Deletes only one record by given primary keys.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public void {methodName}({parameterDefinitionPart})");
            file.OpenBracket();
            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Delete({sharedMethodInvocationParameters});");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();

            var whereParameters = string.Join(", ", deleteByKeyInfo.SqlParameters.Select(column => $"@{column.ColumnName}: {{{column.ColumnName.AsMethodParameter()}}}"));

            file.AppendLine("var effectedRowCount = unitOfWork.ExecuteNonQuery(CallerMemberPath, sqlInfo);");
            file.AppendLine("if (effectedRowCount != 1)");
            file.OpenBracket();
            file.AppendLine($"throw new InvalidDataException($\"{{effectedRowCount}} row effected. Effected row count should be 1 when delete from table {TableInfo.SchemaName}.{TableInfo.TableName}. WhereParameters: {whereParameters} \");");
            file.CloseBracket();
            file.CloseBracket();

            

            
            file.AppendLine();
            file.AppendLine("/// <summary>");

            var keys = string.Join(", ", deleteByKeyInfo.SqlParameters.Select(column => column.ColumnName.ToContractName()));

            string PrepareDeleteParameters(IColumnInfo column)
            {
                if (Context.TableInfo.Columns.First(c=>c.ColumnName == column.ColumnName).SqlReaderMethod == SqlReaderMethods.GetBooleanValueFromChar)
                {
                    return $"contract.{column.ColumnName.ToContractName()} ? \"1\" : \"0\"";
                }
                return $"contract.{column.ColumnName.ToContractName()}";
            }

            file.AppendLine($"///{Padding.ForComment} Deletes only one record by given contract primary keys.({keys})");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public void {methodName}({TableEntityClassNameForMethodParametersInRepositoryFiles} contract)");
            file.OpenBracket();
            file.AppendLine("if (contract == null)");
            file.OpenBracket();
            file.AppendLine("throw new ArgumentNullException(nameof(contract));");
            file.CloseBracket();
            file.AppendLine($"{methodName}({string.Join(", ", deleteByKeyInfo.SqlParameters.Select(PrepareDeleteParameters))});");
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

            var methodName = "Create" + CamelCasedTableName;

            var callerMemberPath = $"{FullClassName}.{methodName}";

            var insertInfo = new InsertInfoCreator {ExcludedColumnNames = Config.ExcludedColumnNamesWhenInsertOperation}.Create(TableInfo);

            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Inserts new record into table.");
            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine($"///{Padding.ForComment} <para>Automatically initialize '{sequenceInfo.TargetColumnName.ToContractName()}' property by using '{sequenceInfo.Name}' sequence.</para>");
            }

            file.AppendLine("/// </summary>");
            file.AppendLine($"public void {methodName}({typeContractName} {contractParameterName})");
            file.OpenBracket();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();

            file.AppendLine("if (contract == null)");
            file.OpenBracket();
            file.AppendLine("throw new ArgumentNullException(nameof(contract));");
            file.CloseBracket();

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

                file.AppendLine("var responseSequence = unitOfWork.ExecuteScalar<object>(callerMemberPath, new SqlInfo {CommandText = sqlNextSequence});");
                file.AppendLine();

                var columnInfo = TableInfo.Columns.First(x => x.ColumnName == sequenceInfo.TargetColumnName);
                if (columnInfo.DotNetType == DotNetTypeName.DotNetInt32Nullable)
                {
                    file.AppendLine($"if({contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} == null)");
                    file.OpenBracket();
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt32(responseSequence);");
                    file.CloseBracket();
                }
                else if (columnInfo.DotNetType == DotNetTypeName.DotNetInt32)
                {
                    file.AppendLine($"if({contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} == 0)");
                    file.OpenBracket();
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt32(responseSequence);");
                    file.CloseBracket();
                }
                else if (columnInfo.DotNetType == DotNetTypeName.DotNetStringName)
                {
                    file.AppendLine($"if({contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} == null)");
                    file.OpenBracket();
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToString(responseSequence);");
                    file.CloseBracket();
                }
                else if (columnInfo.DotNetType == DotNetTypeName.DotNetInt64Nullable)
                {
                    file.AppendLine($"if({contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} == null)");
                    file.OpenBracket();
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt64(responseSequence);");
                    file.CloseBracket();
                }
                else
                {
                    file.AppendLine($"if({contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} == 0)");
                    file.OpenBracket();
                    file.AppendLine($"{contractParameterName}.{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt64(responseSequence);");
                    file.CloseBracket();
                }

                file.CloseBracket();
            }

            if (insertInfo.SqlParameters.Any())
            {
                BoaRepositoryFileExporter.WriteDefaultValues(file, Config.DefaultValuesForInsertMethod, insertInfo.SqlParameters);
            }

            file.AppendLine();
            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Insert({contractParameterName});");

            file.AppendLine();
            if (TableInfo.HasIdentityColumn)
            {
                file.AppendLine("var identity = unitOfWork.ExecuteScalar<int>(CallerMemberPath, sqlInfo);");
                file.AppendLine();
                file.AppendLine($"{contractParameterName}.{TableInfo.IdentityColumn.ColumnName.ToContractName()} = identity;");
            }
            else
            {
                file.AppendLine("unitOfWork.ExecuteNonQuery(CallerMemberPath, sqlInfo);");
            }

            file.CloseBracket();
        }

         void WriteBulkInsertMethod()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var methodName = "Create" + CamelCasedTableName;

            var callerMemberPath = $"{FullClassName}.{methodName}";

            var insertInfo = new InsertInfoCreator {ExcludedColumnNames = Config.ExcludedColumnNamesWhenInsertOperation}.Create(TableInfo);

            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Inserts new record into table.");
            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine($"///{Padding.ForComment} <para>Automatically initialize '{sequenceInfo.TargetColumnName.ToContractName()}' property by using '{sequenceInfo.Name}' sequence.</para>");
            }

            file.AppendLine("/// </summary>");
            file.AppendLine($"public void {methodName}(IList<{typeContractName}> contracts)");
            file.OpenBracket();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();

            file.AppendLine("if (contracts == null)");
            file.OpenBracket();
            file.AppendLine("throw new ArgumentNullException(nameof(contracts));");
            file.CloseBracket();

            
            foreach (var sequenceInfo in TableInfo.SequenceList)
            {
                file.AppendLine();

                file.OpenBracket();



                file.AppendLine($"// Init sequence for {sequenceInfo.TargetColumnName.ToContractName()}");
                file.AppendLine($"const string callerMemberPath = \"{callerMemberPath} -> sqlNextSequence -> {sequenceInfo.Name}\";");
                file.AppendLine();

                file.AppendLine($"var sequenceList = unitOfWork.GetSequenceList(callerMemberPath, \"{sequenceInfo.Name}\", contracts.Count);");
                
                file.AppendLine();

                file.AppendLine("for(var i = 0; i < contracts.Count; i++)");
                file.OpenBracket();
                var columnInfo = TableInfo.Columns.First(x => x.ColumnName == sequenceInfo.TargetColumnName);
                if (columnInfo.DotNetType == DotNetTypeName.DotNetInt32 || columnInfo.DotNetType == DotNetTypeName.DotNetInt32Nullable)
                {
                    file.AppendLine($"contracts[i].{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt32(sequenceList[i]);");
                }
                else if (columnInfo.DotNetType == DotNetTypeName.DotNetStringName)
                {
                    file.AppendLine($"contracts[i].{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToString(sequenceList[i]);");
                }
                else
                {
                    file.AppendLine($"contracts[i].{sequenceInfo.TargetColumnName.ToContractName()} = Convert.ToInt64(sequenceList[i]);");
                }
                file.CloseBracket();

               

                file.CloseBracket();
            }

            if (insertInfo.SqlParameters.Any())
            {
                file.AppendLine();
                file.AppendLine("foreach( var contract in contracts)");
                file.OpenBracket();
                BoaRepositoryFileExporter.WriteDefaultValues(file, Config.DefaultValuesForInsertMethod, insertInfo.SqlParameters);
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
            
            file.AppendLine("unitOfWork.ExecuteBulkInsert(CallerMemberPath, dt);");

            file.CloseBracket();


            // make override
            file.AppendLine();
            file.AppendLine($"public void CreateBulk{CamelCasedTableName}(IList<{typeContractName}> contracts)");
            file.OpenBracket();
            file.AppendLine($"{methodName}(contracts);");
            file.CloseBracket();
        }

        void WriteSelectByIndexMethods()
        {
            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            

            foreach (var indexIdentifier in TableInfo.UniqueIndexInfoList)
            {
                var indexInfo = SelectByIndexInfoCreator.Create(TableInfo, indexIdentifier);

                var sharedRepositoryMethodName = SharedFileExporter.GetMethodName(indexInfo);

                var parameterDefinitionPart = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

                var methodName = "Get" + CamelCasedTableName + "By" + string.Join(string.Empty, indexInfo.SqlParameters.Select(x => $"{x.ColumnName.ToContractName()}"));
                
                file.AppendLine();
                file.AppendLine("/// <summary>");
                file.AppendLine($"///{Padding.ForComment} Selects records by given parameters.");
                file.AppendLine("/// </summary>");
                file.AppendLine($"public {typeContractName} {methodName}({parameterDefinitionPart})");
                file.OpenBracket();

                var sharedMethodInvocationParameters = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

                var callerMemberPath = $"{FullClassName}.{methodName}";

                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.{sharedRepositoryMethodName}({sharedMethodInvocationParameters});");
                file.AppendLine();
                file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
                file.AppendLine();
                file.AppendLine($"return unitOfWork.ExecuteReaderToContract<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

                file.CloseBracket();
            }

            foreach (var indexIdentifier in TableInfo.NonUniqueIndexInfoList)
            {
                var indexInfo = SelectByIndexInfoCreator.Create(TableInfo, indexIdentifier);

                var parameterDefinitionPart = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

                var methodName = "Get" + CamelCasedTableName + "By" + string.Join(string.Empty, indexInfo.SqlParameters.Select(x => $"{x.ColumnName.ToContractName()}"));

                var callerMemberPath = $"{FullClassName}.{methodName}";

                var sharedRepositoryMethodName = SharedFileExporter.GetMethodName(indexInfo);

                file.AppendLine();
                file.AppendLine("/// <summary>");
                file.AppendLine($"///{Padding.ForComment} Selects records by given parameters.");
                file.AppendLine("/// </summary>");
                file.AppendLine($"public List<{typeContractName}> {methodName}({parameterDefinitionPart})");
                file.OpenBracket();

                var sharedMethodInvocationParameters = string.Join(", ", indexInfo.SqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

                file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.{sharedRepositoryMethodName}({sharedMethodInvocationParameters});");
                file.AppendLine();
                file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
                file.AppendLine();
                file.AppendLine($"return unitOfWork.ExecuteReaderToList<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

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

            var sqlParameters = selectByPrimaryKeyInfo.SqlParameters;

            var parameterDefinitionPart = string.Join(", ", sqlParameters.Select(x => $"{x.DotNetType} {x.ColumnName.AsMethodParameter()}"));

            var methodName = "Get" + CamelCasedTableName + "By" + string.Join(string.Empty, sqlParameters.Select(x => $"{x.ColumnName.ToContractName()}"));
            
            // var methodName = "Get" + CamelCasedTableName + "ByKey";

            var callerMemberPath = $"{FullClassName}.{methodName}";

            file.AppendLine();
            file.AppendLine($"public {typeContractName} {methodName}({parameterDefinitionPart})");
            file.OpenBracket();

            var sharedMethodInvocationParameters = string.Join(", ", sqlParameters.Select(x => $"{x.ColumnName.AsMethodParameter()}"));

            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.SelectByKey({sharedMethodInvocationParameters});");
            file.AppendLine();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine($"return unitOfWork.ExecuteReaderToContract<{typeContractName}>(CallerMemberPath, sqlInfo, {sharedRepositoryClassAccessPath}.ReadContract);");

            file.CloseBracket();
        }

        void WriteUpdateByKeyMethod()
        {
            if (!TableInfo.IsSupportSelectByKey)
            {
                return;
            }

            var methodName = "Modify" + CamelCasedTableName;

            var typeContractName = TableEntityClassNameForMethodParametersInRepositoryFiles;

            var callerMemberPath = $"{FullClassName}.{methodName}";

            var updateInfo = UpdateByPrimaryKeyInfoCreator.Create(TableInfo);

            file.AppendLine();
            file.AppendLine("/// <summary>");
            file.AppendLine($"///{Padding.ForComment} Updates only one record by given primary keys.");
            file.AppendLine("/// </summary>");
            file.AppendLine($"public void {methodName}({typeContractName} {contractParameterName})");
            file.OpenBracket();
            file.AppendLine($"const string CallerMemberPath = \"{callerMemberPath}\";");
            file.AppendLine();
            file.AppendLine("if (contract == null)");
            file.OpenBracket();
            file.AppendLine($"throw new ArgumentNullException(nameof({contractParameterName}));");
            file.CloseBracket();

            if (updateInfo.SqlParameters.Any())
            {
                BoaRepositoryFileExporter.WriteDefaultValues(file, Config.DefaultValuesForUpdateByKeyMethod, updateInfo.SqlParameters);
            }

            var whereParameters = string.Join(", ", updateInfo.WhereParameters.Select(column => $"@{column.ColumnName}: {{contract.{column.ColumnName.ToContractName()}}}"));
           
            file.AppendLine();
            file.AppendLine($"var sqlInfo = {sharedRepositoryClassAccessPath}.Update({contractParameterName});");
            file.AppendLine();
            file.AppendLine("var effectedRowCount = unitOfWork.ExecuteNonQuery(CallerMemberPath, sqlInfo);");
            file.AppendLine("if (effectedRowCount != 1)");
            file.OpenBracket();
            file.AppendLine($"throw new InvalidDataException($\"{{effectedRowCount}} row effected. Effected row count should be 1 when update table {TableInfo.SchemaName}.{TableInfo.TableName}. WhereParameters: {whereParameters} \");");
            file.CloseBracket();
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