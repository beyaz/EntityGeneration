using System;
using System.IO;

namespace BOA.EntityGeneration
{
    class FileHelper
    {
        #region Public Methods
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
        }

        public static void WriteAllText(string path, string data)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            CreateDirectoryIfNotExists(Path.GetDirectoryName(path));

            File.Delete(path);

            var fs = new FileStream(path, FileMode.OpenOrCreate);
            var sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Close();
            fs.Close();
        }
        #endregion
    }
}