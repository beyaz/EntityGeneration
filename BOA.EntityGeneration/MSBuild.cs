using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BOA.Tasks
{
    /// <summary>
    ///     The ms build data
    /// </summary>
    [Serializable]
    public class MSBuildData
    {
        #region Public Properties
        public Exception BuildError { get; set; }

        /// <summary>
        ///     Gets the build output.
        /// </summary>
        public string BuildOutput { get; internal set; }

        /// <summary>
        ///     Gets or sets the project file path.
        /// </summary>
        public string ProjectFilePath { get; set; }

        /// <summary>
        ///     Gets the standard error.
        /// </summary>
        public string StandardError { get; internal set; }
        #endregion
    }

    /// <summary>
    ///     The ms build
    /// </summary>
    public class MSBuild
    {
        #region Public Methods
        /// <summary>
        ///     Builds the specified data.
        /// </summary>
        public static void Build(MSBuildData data)
        {
            var msbuildPath = GetMsBuildExePath();

            var arguments = new StringBuilder();

            arguments.Append(data.ProjectFilePath);

            if (data.ProjectFilePath.EndsWith(".sln"))
            {
                arguments.Append(" /p:Configuration=Debug /p:Platform=\"Any CPU\"");
            }

            var startInfo = new ProcessStartInfo(msbuildPath)
            {
                Arguments              = arguments.ToString(),
                ErrorDialog            = true,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            var process = Process.Start(startInfo);

            if (process == null)
            {
                throw new InvalidOperationException(nameof(process));
            }

            process.WaitForExit(6000);

            data.BuildOutput   = process.StandardOutput.ReadToEnd();
            data.StandardError = process.StandardError.ReadToEnd();

            var hasError = process.ExitCode > 0;
            if (hasError)
            {
                data.BuildError = new InvalidOperationException(data.StandardError + data.BuildOutput);
            }
        }

        /// <summary>
        ///     Gets the content of the bat file.
        /// </summary>
        public static string GetBatFileContent(MSBuildData data)
        {
            var sb = new StringBuilder();

            var msBuildExePath = GetMsBuildExePath();

            // ReSharper disable once ExpressionIsAlwaysNull
            var msBuildExeDirectory = Path.GetDirectoryName(msBuildExePath) + Path.DirectorySeparatorChar;

            sb.AppendLine($"SET PATH={msBuildExeDirectory}");

            sb.AppendLine($@"SET SolutionPath={data.ProjectFilePath}");

            if (data.ProjectFilePath.EndsWith(".sln"))
            {
                sb.AppendLine(@"MSbuild %SolutionPath% /p:Configuration=Debug /p:Platform=""Any CPU""");
            }
            else
            {
                sb.AppendLine(@"MSbuild %SolutionPath%");
            }

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the ms build executable path.
        /// </summary>
        static string GetMsBuildExePath()
        {
            var msbuildPath = $@"{ProgramFilesX86()}\MSBuild\14.0\Bin\MSBuild.exe";
            if (File.Exists(msbuildPath))
            {
                return msbuildPath;
            }

            msbuildPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
            if (File.Exists(msbuildPath))
            {
                return msbuildPath;
            }

            throw new InvalidOperationException($"{msbuildPath} is not found.");
        }

        /// <summary>
        ///     Programs the files X86.
        /// </summary>
        static string ProgramFilesX86()
        {
            if (8 == IntPtr.Size
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
        #endregion
    }
}