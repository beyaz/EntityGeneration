using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Smocks;

namespace BOA.EntityGeneration.ConstantsProjectGeneration
{
    [TestClass]
    public class GeneratorTest
    {
        static void ShouldMatch(EnumInfo enumInfo, string expected)
        {
            Smock.Run(context =>
            {
                var generator = new Generator();
                context.Setup(() => generator.InitEnumInformationList()).Callback(() =>
                {
                    generator.Context.EnumInfoList = new[]
                    {
                        enumInfo
                    };
                });

                context.Setup(() => generator.ExportFile()).Callback(() => generator.Context.File.ToString().Trim().Should().BeEquivalentTo(expected.Trim()));

                generator.Generate();
            });
        }

        [TestMethod]
        public void When_profile_name_starts_with_CRD_it__should_starts_with_CreditCard()
        {
            var input = new EnumInfo
            {
                ClassName    = "A",
                NumberValue  = "5",
                ProfileName  = "CRD_XXX",
                PropertyName = "Prop1",
                StringValue  = "U"
            };

            const string expected = @"

using System;
using BOA.Card.Definitions;

namespace BOA.Types.Kernel.Card.Constants.CreditCardXxx
{

    [Serializable]
    public class A : EnumBase<A, int>
    {
        public static readonly A Prop1 = new A(""U"", 5);

        public A(string name, int value) : base(name, value)
        {
        }

        public static explicit operator A(string value)
        {
            return Parse<A>(value);
        }
    }
}


";

            ShouldMatch(input, expected);
        }
    }
}