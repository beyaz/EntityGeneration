using System;
using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain;
using BOA.EntityGeneration.CustomSQLExporting.Wrapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smocks;
using Smocks.Matching;

namespace BOA.EntityGeneration.CustomSQLExporting
{
    [TestClass]
    public class CustomSqlExporterTest
    {
        #region Public Methods
        [TestMethod]
        public void When_input_parameter_ends_with_FLAG_and_value_type_is_char_one_it_should_generate_dot_net_boolean_property()
        {
            var inputParameters = new List<ObjectParameterInfo>
            {
                new ObjectParameterInfo
                {
                    IsNullable = true,
                    DataType   = "bigint",
                    Name       = "aloha"
                },
                new ObjectParameterInfo
                {
                    IsNullable = true,
                    DataType   = "char",
                    Name       = "my_FLAG"
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

            var generatedFiles = Generate(inputParameters, resultColumns);

            generatedFiles.TypeCodes.Should().Contain("public bool? MyFlag { get; set; }");
            generatedFiles.SharedCodes.Should().Contain(@"sqlInfo.AddInParameter(""@my_FLAG"", SqlDbType.Char, request.MyFlag.GetCharNullableValueFromBoolean());");
        }

        [TestMethod]
        public void When_result_column_name_ends_with_FLAG_and_value_type_is_string_it_should_generate_dot_net_boolean_property_read()
        {
            var inputParameters = new List<ObjectParameterInfo>
            {
                new ObjectParameterInfo
                {
                    IsNullable = true,
                    DataType   = "bigint",
                    Name       = "aloha"
                },
                new ObjectParameterInfo
                {
                    IsNullable = true,
                    DataType   = "char",
                    Name       = "my_FLAG"
                }
            };

            var resultColumns = new List<CustomSqlInfoResult>
            {
                new CustomSqlInfoResult
                {
                    Name     = "y",
                    DataType = "int"
                },
                new CustomSqlInfoResult
                {
                    Name       = "ALOHA_FLAG",
                    DataType   = "varchar",
                    IsNullable = true
                }
            };

            var generatedFiles = Generate(inputParameters, resultColumns);

            generatedFiles.TypeCodes.Should().Contain("public bool? AlohaFlag {get; set;}");
            generatedFiles.SharedCodes.Should().Contain(@"contract.AlohaFlag = reader.GetBooleanNullableValueFromChar(""ALOHA_FLAG"");");
        }
        #endregion

        #region Methods
        static File Generate(List<ObjectParameterInfo> inputParameters, List<CustomSqlInfoResult> resultColumns)
        {
            File file = null;

            var currentDomain = AppDomain.CurrentDomain;

            Smock.Run(context =>
            {
                var exporter = new CustomSqlExporter();

                exporter.InitializeContext();

                var map = new Dictionary<string, string>();

                context.Setup(() => exporter.Context.FileSystem.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string path, string content) => { map.Add(path, content); });

                var customSqlInfo = new CustomSqlInfo
                {
                    Name = "Abc"
                };

                context.Setup(() => It.IsAny<DatabaseReader>().GetCustomSqlNamesInfProfile()).Returns(new List<string> {string.Empty});
                context.Setup(() => It.IsAny<DatabaseReader>().ReadCustomSqlInfo()).Returns(customSqlInfo);
                context.Setup(() => It.IsAny<DatabaseReader>().ReadInputParametersFromDatabase()).Returns(inputParameters);
                context.Setup(() => It.IsAny<DatabaseReader>().ReadResultColumns()).Returns(resultColumns);

                exporter.Export("Xyz");

                file = new File
                {
                    TypeCodes   = map[map.Keys.ToList()[0]],
                    SharedCodes = map[map.Keys.ToList()[1]]
                };

                currentDomain.SetData(nameof(file), file);
            });

            return (File) currentDomain.GetData(nameof(file));
        }
        #endregion

        [Serializable]
        class File
        {
            #region Public Properties
            public string SharedCodes { get; set; }
            public string TypeCodes   { get; set; }
            #endregion
        }
    }
}