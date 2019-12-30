using System.Collections.Generic;
using System.Data;
using BOA.EntityGeneration.CustomSQLExporting.Models;
using BOA.EntityGeneration.CustomSQLExporting.Wrapper;
using DotNetDatabaseAccessUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smocks;
using Smocks.Matching;

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

                exporter.InitializeContext();

                var map = new Dictionary<string, string>();

                context.Setup(() => exporter.Context.FileSystem.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string path, string content) =>
                {
                    map.Add(path, content);
                });

                var customSqlInfo = new CustomSqlInfo
                {
                    Name = "Abc",
                    Parameters = new[]
                    {
                        new CustomSqlInfoParameter
                        {
                            Name          = "USER_ID",
                            IsNullable    = true,
                            SqlDbTypeName = SqlDbType.BigInt
                        }
                    },
                    ResultColumns = new List<CustomSqlInfoResult>
                    {
                        new CustomSqlInfoResult
                        {
                            Name     = "y",
                            DataType = "int"
                        }
                    }
                };

                context.Setup(() => ProjectCustomSqlInfoDataAccess.GetCustomSqlNamesInfProfile(It.IsAny<IDatabase>(), It.IsAny<string>(), It.IsAny<CustomSqlExporterConfig>())).Returns(new List<string> {string.Empty});
                context.Setup(() => ProjectCustomSqlInfoDataAccess.GetCustomSqlInfo(It.IsAny<IDatabase>(),
                                                                                    It.IsAny<string >(),
                                                                                    It.IsAny<string >(), 
                                                                                    It.IsAny<CustomSqlExporterConfig>() ,
                                                                                    It.IsAny<int>()))
                                                                                    .Returns(customSqlInfo);

                

                exporter.Export("Xyz");

                map["y"].Should().Be("f");
            });
        }
    }
}