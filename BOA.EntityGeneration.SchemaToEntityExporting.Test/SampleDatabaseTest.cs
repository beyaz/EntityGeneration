using System.Diagnostics;
using System.IO;
using System.Linq;
using BOA.EntityGeneration.SchemaToEntityExporting.Exporters;
using FluentAssertions;
using Ionic.Zip;
using Microsoft.TeamFoundation.TestImpact.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BOA.EntityGeneration.SchemaToEntityExporting
{
    [TestClass]
    public class SampleDatabaseTest
    {
        const string TestDirectory = @"d:\temp\";

        static void DeleteFileIfExists(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            File.Delete(path);
        }

        static void DeleteDirectoryIfExists(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            Directory.Delete(path,true);
        }

        [TestInitialize]
        public void Setup()
        {
            Process.GetProcessesByName("sqlservr").FirstOrDefault()?.Kill();
            Process.GetProcessesByName("sqlwriter").FirstOrDefault()?.Kill();
            
            

            const string FileName = "SampleDatabase.mdf";

            DeleteFileIfExists(TestDirectory+ FileName);
            DeleteFileIfExists(TestDirectory +"SampleDatabase_log.ldf");

            DeleteDirectoryIfExists(TestDirectory);

            Directory.CreateDirectory(TestDirectory);
            
            File.Copy(FileName,TestDirectory+ FileName,true);

            using (var zip = new ZipFile(@"D:\git\EntityGeneration\BOA.EntityGeneration.SchemaToEntityExporting.Test\TestSolution.zip"))
            {
                
                zip.ExtractAll(TestDirectory);
            }

        }
        #region Public Methods
        [TestMethod]
        public void AllScenario()
        {
            
            


            ConfigurationHelper.ConfigurationDirectoryPath = "SampleDatabaseSchemaToEntityExportingConfigs";

            var schemaExporter = new SchemaExporter();

            schemaExporter.InitializeContext();

            schemaExporter.Database.CreateTables();

            schemaExporter.Context.MsBuildQueue.BuildAfterCodeGenerationIsCompleted = false;

            schemaExporter.Export("ERP");
           

            schemaExporter.Context.ErrorList.Should().BeEmpty();

        }
        #endregion

      
    }
}