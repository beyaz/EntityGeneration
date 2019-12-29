using System.Collections.Generic;
using System.IO;
using DotNetSerializationUtilities;

namespace BOA.EntityGeneration.UI.Container
{
    public class ConfigurationHelper
    {
        #region Public Properties
        public static string ConfigurationDirectoryPath { get; set; }
        #endregion

        #region Public Methods
        public static T ReadConfig<T>(params string[] folders)
        {
            var items = new List<string>(folders);

            if (ConfigurationDirectoryPath != null)
            {
                items.Insert(0, ConfigurationDirectoryPath);
            }

            items.Add(typeof(T).Name + ".yaml");

            var filePath = string.Join(Path.DirectorySeparatorChar.ToString(), items);

            if (!File.Exists(filePath))
            {
                App.Trace($"File not found. FilePath: {filePath}");
                throw new FileNotFoundException(filePath);
            }
            return YamlHelper.DeserializeFromFile<T>(filePath);
        }
        #endregion
    }
}