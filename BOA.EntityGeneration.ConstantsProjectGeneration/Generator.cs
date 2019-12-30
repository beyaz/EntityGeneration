using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ConstantsProjectGeneration
{
    class Generator
    {
        #region Public Properties
        public Context Context { get; } = new Context();
        #endregion

        #region Properties
        ConstantsProjectGenerationConfig Config      => Context.Config;
        PaddedStringBuilder              File        => Context.File;
        FileSystem                       FileSystem  => Context.FileSystem;
        ProcessContract                  ProcessInfo => Context.ProcessInfo;
        #endregion

        #region Public Methods
        public void Generate()
        {
            InitEnumInformationList();

            WriteContent();

            ExportFile();

            ExportCsProjFiles();

            Context.MsBuildQueue.Build();
        }
        #endregion

        #region Methods
        void AppendEnumPropertyToClass(IReadOnlyList<EnumInfo> propertyList)
        {
            var className = propertyList.First().ClassName.ToContractName();

            File.AppendLine("[Serializable]");
            File.AppendLine($"public class {className} : EnumBase<{className}, int>");
            File.OpenBracket();

            foreach (var item in propertyList)
            {
                File.AppendLine($"public static readonly {className} {item.PropertyName.ToContractName()} = new {className}(\"{item.StringValue}\", {item.NumberValue});");
            }

            File.AppendLine();
            File.AppendLine($"public {className}(string name, int value) : base(name, value)");
            File.OpenBracket();
            File.CloseBracket();
            File.AppendLine();
            File.AppendLine($"public static explicit operator {className}(string value)");
            File.OpenBracket();
            File.AppendLine($"return Parse<{className}>(value);");
            File.CloseBracket();

            File.CloseBracket();
        }

        void ExportCsProjFiles()
        {
            var csprojFileGenerator = new CsprojFileGenerator
            {
                FileSystem       = FileSystem,
                FileNames        = new List<string> {Config.SourceCodeFileName},
                NamespaceName    = Config.OutputAssemblyName,
                IsClientDll      = true,
                ProjectDirectory = Config.ProjectDirectory,
                References       = Config.AssemblyReferences
            };

            var csprojFilePath = csprojFileGenerator.Generate();

            Context.MsBuildQueue.Push(csprojFilePath);
        }

        void ExportFile()
        {
            ProcessInfo.Text = "Writing files.";

            var filePath = Config.ProjectDirectory + Config.SourceCodeFileName;

            FileSystem.WriteAllText(filePath, File.ToString());
        }

        void InitEnumInformationList()
        {
            using (var connection = Context.CreateConnection())
            {
                Context.EnumInfoList = connection.Query<EnumInfo>(Config.DataSourceProcedureFullName, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        static string Normalize(string profileName)
        {
            if (profileName.StartsWith("CRD_"))
            {
                profileName = profileName.Replace("CRD_", "CREDIT_CARD_");
            }

            return profileName;
        }
        void WriteContent()
        {
            foreach (var line in Config.UsingLines)
            {
                File.AppendLine(line);
            }

            File.AppendLine();

            var profileNames = Context.EnumInfoList.OrderBy(x => x.ProfileName).GroupBy(x => x.ProfileName).Select(x => x.Key).ToList();

            ProcessInfo.Total   = profileNames.Count;
            ProcessInfo.Current = 0;

            foreach (var profileName in profileNames)
            {
                ProcessInfo.Text = $"Exporting profile: {profileName}";

                var namespaceFullName = Config.NamespaceName.Replace("$(profileName)", Normalize(profileName).ToContractName());

                File.AppendLine($"namespace {namespaceFullName}");
                File.OpenBracket();

                var enumClassNameList = Context.EnumInfoList.Where(x => x.ProfileName == profileName).OrderBy(x => x.ClassName).GroupBy(x => x.ClassName).Select(x => x.Key).ToList();

                foreach (var className in enumClassNameList)
                {
                    File.AppendLine();
                    AppendEnumPropertyToClass(Context.EnumInfoList.Where(x => x.ClassName == className).ToList());
                }

                ProcessInfo.Current++;

                File.CloseBracket();
            }
        }
        #endregion
    }
}