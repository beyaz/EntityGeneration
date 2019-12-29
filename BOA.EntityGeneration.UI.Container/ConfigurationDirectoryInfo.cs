using System.IO;
using DotNetSerializationUtilities;

namespace BOA.EntityGeneration.UI.Container
{
    public class ConfigurationDirectoryInfo
    {
        public string ConstantsProjectGeneration { get; set; }
        public string CustomSQLExporting         { get; set; }
        public string SchemaToEntityExporting    { get; set; }


        internal static string FilePath => Path.GetDirectoryName( typeof(ConfigurationDirectoryInfo).Assembly.Location) + Path.DirectorySeparatorChar +  nameof(ConfigurationDirectoryInfo) + ".yaml";

        internal static bool   Exists   => File.Exists(FilePath);

        public static ConfigurationDirectoryInfo CreateFromFile()
        {
            App.Trace($"Begin main config read started. Path: {FilePath}");
            var item =  YamlHelper.DeserializeFromFile<ConfigurationDirectoryInfo>(FilePath);
            App.Trace($"End main config read started. Path: {FilePath}");

            return item;
        }
    }
}