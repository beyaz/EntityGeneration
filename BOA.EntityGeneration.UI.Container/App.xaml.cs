using System;
using System.IO;
using System.Net.Mime;
using System.Windows;

namespace BOA.EntityGeneration.UI.Container
{
    public partial class App
    {
        #region Static Fields
        public static readonly   MainWindowModel                   Model;
        internal static readonly EntityGenerationUIContainerConfig Config;
        #endregion

        public static void Trace(string message)
        {
            try
            {
                var logFilePath = Path.GetDirectoryName( typeof(App).Assembly.Location) + Path.DirectorySeparatorChar +  "Log.txt";
                var fs           = new FileStream(logFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                var streamWriter = new StreamWriter(fs);
                streamWriter.BaseStream.Seek(0L, SeekOrigin.End);
                streamWriter.WriteLine(message);
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch
            {
                // ignored
            }
        }

        #region Constructors
        static App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException ;
            Trace("App started.");
            if (ConfigurationDirectoryInfo.Exists)
            {
                var configurationDirectoryInfo = ConfigurationDirectoryInfo.CreateFromFile();

                BOA.EntityGeneration.ConstantsProjectGeneration.ConfigurationHelper.ConfigurationDirectoryPath = configurationDirectoryInfo.ConstantsProjectGeneration;
                CustomSQLExporting.ConfigurationHelper.ConfigurationDirectoryPath                              = configurationDirectoryInfo.CustomSQLExporting;
                SchemaToEntityExporting.ConfigurationHelper.ConfigurationDirectoryPath                         = configurationDirectoryInfo.SchemaToEntityExporting;
                
            }

            Config = EntityGenerationUIContainerConfig.CreateFromFile();

            Model = new MainWindowModel
            {
                CheckinComment = CheckInCommentAccess.GetCheckInComment()
            };
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace(e.ToString());
        }

       
        #endregion
    }
}