using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BOA.EntityGeneration.ConstantsProjectGeneration
{
    [TestClass]
    public class GeneratorTest
    {
        [TestMethod]
        public void When_profile_name_starts_with_CRD_it__should_starts_with_CreditCard()
        {
            Smocks.Smock.Run((context) =>
            {

                var generator = new Generator();
                generator.Generate();
            });
            
        }
    }
}
