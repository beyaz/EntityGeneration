using System;

namespace BOA.EntityGeneration.ConstantsProjectGeneration
{
    [Serializable]
    public class ConstantsProjectGenerationConfig
    {
        #region Public Properties
        public string[] AssemblyReferences                      { get; set; }
        public string   ConnectionString                        { get; set; }
        public string   DataSourceProcedureFullName             { get; set; }
        public bool     IntegrateWithTFSAndCheckInAutomatically { get; set; }
        public string   NamespaceName                           { get; set; }
        public string OutputAssemblyName { get; set; }
        public string   ProjectDirectory                        { get; set; }
        public string   SourceCodeFileName                      { get; set; }
        public string[] UsingLines                              { get; set; }
        #endregion

        #region Public Methods
        public static ConstantsProjectGenerationConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<ConstantsProjectGenerationConfig>();
        }
        #endregion
    }
}