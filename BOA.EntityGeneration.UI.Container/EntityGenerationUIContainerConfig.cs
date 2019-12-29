using DotNetSerializationUtilities;

namespace BOA.EntityGeneration.UI.Container
{
    public class EntityGenerationUIContainerConfig
    {
        #region Public Properties
        public bool IntegrateWithTFSAndCheckInAutomatically { get; set; }

        public string[] ProfileNames { get; set; }
        public string[] SchemaNames  { get; set; }
        #endregion

        #region Public Methods
        public static EntityGenerationUIContainerConfig CreateFromFile()
        {
            return ConfigurationHelper.ReadConfig<EntityGenerationUIContainerConfig>();
        }
        #endregion
    }
}