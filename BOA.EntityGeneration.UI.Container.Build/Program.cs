﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotNetStringUtilities;
using Ionic.Zip;

namespace BOA.EntityGeneration.UI.Container.Build
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        #region Methods
        static IList<string> GetYamlFilesInProject(string projectDirectory)
        {
            return Directory.GetFiles(projectDirectory, "*.yaml", SearchOption.AllDirectories).Where(IsNotInOutputFolder).ToList();
        }

        static bool IsNotInOutputFolder(string path)
        {
            if (path.Contains(@"\bin\"))
            {
                return false;
            }

            if (path.Contains(@"\obj\"))
            {
                return false;
            }

            return true;
        }

        
        
        static void Main()
        {
             // GenerateSlnGeneratorBatFile();
             CreateZipFile();
        }

        static void GenerateSlnGeneratorBatFile()
        {
            SlnFileGenerator.ExportBatFile(@"D:\work\BOA.CardModules\Dev\AutoGeneratedCodes\", "BOA.Card.AutoGeneratedCodes");
        }

        static void CreateZipFile()
        {
            const string slnDirectory = @"D:\git\EntityGeneration\";

            const string zipFilePath = slnDirectory + @"BOA.EntityGeneration.UI.Container.Build\bin\debug\BOA.EntityGeneration.UI.Container.zip";

            File.Delete(zipFilePath);

            using (var zip = new ZipFile(zipFilePath))
            {
                var dir = slnDirectory + @"\BOA.EntityGeneration.UI.Container\bin\Debug\";

                var rootDirectoryInZip = string.Empty;
                zip.AddFile(dir + "BOA.EntityGeneration.UI.Container.exe", rootDirectoryInZip);
                zip.AddFile(dir + "BOA.TfsAccess.dll", rootDirectoryInZip);
                zip.AddFile(dir + "BOA.EntityGeneration.SchemaToEntityExporting.dll", rootDirectoryInZip);
                zip.AddFile(dir + "BOA.EntityGeneration.CustomSQLExporting.dll", rootDirectoryInZip);
                zip.AddFile(dir + "BOA.EntityGeneration.ConstantsProjectGeneration.dll", rootDirectoryInZip);
                zip.AddFile(dir + "BOA.EntityGeneration.dll", rootDirectoryInZip);
                zip.AddFile(dir + "DotNetSerializationUtilities.dll", rootDirectoryInZip);
                zip.AddFile(dir + "DotNetDatabaseAccessUtilities.dll", rootDirectoryInZip);
                zip.AddFile(dir + "DotNetStringUtilities.dll", rootDirectoryInZip);
                zip.AddFile(dir + "MahApps.Metro.dll", rootDirectoryInZip);
                zip.AddFile(dir + "ControlzEx.dll", rootDirectoryInZip);
                zip.AddFile(dir + "System.Windows.Interactivity.dll", rootDirectoryInZip);
                zip.AddFile(dir + "WpfControls.dll", rootDirectoryInZip);
                zip.AddFile(dir + "Dapper.dll", rootDirectoryInZip);

                dir = slnDirectory + @"BOA.EntityGeneration.ConstantsProjectGeneration\";

                foreach (var filePath in GetYamlFilesInProject(dir))
                {
                    var relativeFilePath = Path.GetDirectoryName(filePath.RemoveFromStart(dir));
                    zip.AddFile(filePath, "Configurations/ConstantsProjectGeneration/" + relativeFilePath);
                }

                dir = slnDirectory + @"BOA.EntityGeneration.CustomSQLExporting\";

                foreach (var filePath in GetYamlFilesInProject(dir))
                {
                    var relativeFilePath = Path.GetDirectoryName(filePath.RemoveFromStart(dir));
                    zip.AddFile(filePath, "Configurations/CustomSQLExporting/" + relativeFilePath);
                }

                dir = slnDirectory + @"BOA.EntityGeneration.SchemaToEntityExporting\";

                foreach (var filePath in GetYamlFilesInProject(dir))
                {
                    var relativeFilePath = Path.GetDirectoryName(filePath.RemoveFromStart(dir));
                    zip.AddFile(filePath, "Configurations/SchemaToEntityExporting/" + relativeFilePath);
                }

                zip.AddFile(slnDirectory + @"BOA.EntityGeneration.UI.Container\EntityGenerationUIContainerConfig.yaml", string.Empty);
                zip.AddFile(slnDirectory + @"BOA.EntityGeneration.UI.Container.Build\ConfigurationDirectoryInfo.yaml", string.Empty);

                zip.Save();
            }

            Process.Start("explorer.exe", $"/select, {zipFilePath}");
        }
        #endregion
    }
}