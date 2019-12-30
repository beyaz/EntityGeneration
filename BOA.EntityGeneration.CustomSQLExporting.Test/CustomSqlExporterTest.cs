using BOA.EntityGeneration.CustomSQLExporting.Wrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smocks;

namespace BOA.EntityGeneration.CustomSQLExporting
{
    [TestClass]
    public class CustomSqlExporterTest
    {
        [TestMethod]
        public void When_input_parameter_ends_with_FLAG_and_value_type_is_char_one_it_should_generate_dot_net_boolean_property()
        {
            Smock.Run(context =>
            {
                var exporter = new CustomSqlExporter();

                exporter.Export(string.Empty);
            });
        }
    }
}