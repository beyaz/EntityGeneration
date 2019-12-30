using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
                    Name = "Abc"
                };

                context.Setup(() => ProjectCustomSqlInfoDataAccess.GetCustomSqlNamesInfProfile(It.IsAny<GetCustomSqlNamesInfProfileInput>())).Returns(new List<string> {string.Empty});

                context.Setup(() => ProjectCustomSqlInfoDataAccess.ReadFromDatabase(It.IsAny<GetCustomSqlInfoInput>())).Returns(customSqlInfo);

                var inputParameters = new List<ProjectCustomSqlInfoDataAccess.ObjectParameterInfo>
                {
                    new ProjectCustomSqlInfoDataAccess.ObjectParameterInfo
                    {
                        isNullable = true,
                        dataType   = "bigint",
                        name       = "aloha"
                    },
                    new ProjectCustomSqlInfoDataAccess.ObjectParameterInfo
                    {
                        isNullable = true,
                        dataType   = "char",
                        name       = "my_FLAG"
                    }
                };
                var resultColumns = new List<CustomSqlInfoResult>
                {
                    new CustomSqlInfoResult
                    {
                        Name     = "y",
                        DataType = "int"
                    }
                };


                context.Setup(() => ProjectCustomSqlInfoDataAccess.ReadInputParametersFromDatabase(It.IsAny<CustomSqlInfo>(), It.IsAny<IDatabase>())).Returns(inputParameters);
                context.Setup(() => ProjectCustomSqlInfoDataAccess.ReadResultColumns(It.IsAny<CustomSqlInfo>(), It.IsAny<IDatabase>())).Returns(resultColumns);
                

                exporter.Export("Xyz");

                map["y"].Should().Be("f");
            });
        }
    }
}