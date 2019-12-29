using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BOA.EntityGeneration.SchemaToEntityExporting
{
    [TestClass]
    public class NamingTest
    {
        #region Public Methods
        [TestMethod]
        public void Support_BIGGER_AND_SEPARATED_NAMING()
        {
            "USER_NAME".ToContractName().Should().Be("UserName");
            "HATTORI_HANZO".ToContractName().Should().Be("HattoriHanzo");
            "STATUS".ToContractName().Should().Be("Status");
            "UserName".ToContractName().Should().Be("UserName");
            "UserName3".ToContractName().Should().Be("UserName3");
            "User_name".ToContractName().Should().Be("UserName");
        }
        #endregion
    }
}