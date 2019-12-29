using System;
using System.Collections.Generic;
using System.Linq;

namespace BOA.EntityGeneration
{
    public class CsprojFileGenerator
    {
        #region Public Properties
        public IReadOnlyList<string>       FileNames        { get; set; }
        public FileSystem                  FileSystem       { get; set; }
        public bool                        IsClientDll      { get; set; }
        public string                      NamespaceName    { get; set; }
        public string                      ProjectDirectory { get; set; }
        public IReadOnlyCollection<string> References       { get; set; }
        #endregion

        #region Public Methods
        public string Generate()
        {
            var csprojFilePath = $"{ProjectDirectory}{NamespaceName}.csproj";

            var assemblyReferences = "    " + string.Join(Environment.NewLine + "    ", References.Select(x => x.Trim()).Distinct());

            var fileNames = string.Join(Environment.NewLine + "    ", FileNames.Select(fileName => $"    <Compile Include=\"{fileName}\" />"));

            var postBuildEvents = new List<string> {@"        copy /y ""$(TargetDir)$(TargetName).*"" ""d:\boa\server\bin\""  "};
            if (IsClientDll)
            {
                postBuildEvents.Add(@"        copy /y ""$(TargetDir)$(TargetName).*"" ""d:\boa\client\bin\""  ");
                postBuildEvents.Add(@"        copy /y ""$(TargetDir)$(TargetName).*"" ""d:\boa\one\""  ");
            }

            var content = $@"

<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>{NamespaceName}</RootNamespace>
    <AssemblyName>{NamespaceName}</AssemblyName>
    <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <Deterministic>true</Deterministic>
    <SccProjectName>SAK</SccProjectName>
    <SccLocalPath>SAK</SccLocalPath>
    <SccAuxPath>SAK</SccAuxPath>
    <SccProvider>SAK</SccProvider>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>3</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>3</WarningLevel>
  </PropertyGroup>
  <ItemGroup>    
" + assemblyReferences + @"
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Properties\AssemblyInfo.cs"" />
" + fileNames + @"
  </ItemGroup>
  <ItemGroup />
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <PropertyGroup>
    <PostBuildEvent>
" + string.Join(Environment.NewLine, postBuildEvents) + @"
    </PostBuildEvent>
  </PropertyGroup>
</Project>

";

            FileSystem.WriteAllText(csprojFilePath, content.Trim());

            var assemblyInfoContent = $@"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(""{NamespaceName}"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""{NamespaceName}"")]
[assembly: AssemblyCopyright(""Copyright ©  2019"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]
";

            var assemblyInfoFilePath = $"{ProjectDirectory}\\Properties\\AssemblyInfo.cs";
            FileSystem.WriteAllText(assemblyInfoFilePath, assemblyInfoContent.Trim());

            return csprojFilePath;
        }
        #endregion
    }
}