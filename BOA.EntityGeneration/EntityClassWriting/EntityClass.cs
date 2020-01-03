using System.Collections.Generic;
using System.Linq;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.EntityClassWriting
{
   public class EntityClass
    {
        #region Public Properties
        public string                              ClassName                   { get; set; }
        public EntityClassComment                  EntityClassComment          { get; set; }
        public string                              EntityContractBaseClassName { get; set; }
        public IReadOnlyList<ContractPropertyInfo> PropertyList                { get; set; }
        #endregion

        #region Public Methods
        public void Write(PaddedStringBuilder file)
        {
            EntityClassComment.Write(file);

            var inheritancePart = ": ";

            if (EntityContractBaseClassName != null)
            {
                inheritancePart += EntityContractBaseClassName;
            }

            inheritancePart += ", ICloneable";

            file.AppendLine("[Serializable]");
            file.AppendLine($"public sealed class {ClassName} {inheritancePart}");
            file.OpenBracket();

            EntityClassComment.Write(file);
            file.AppendLine("// ReSharper disable once EmptyConstructor");
            file.AppendLine($"public {ClassName}()");
            file.OpenBracket();
            file.CloseBracket();

            file.AppendLine();
            file.AppendLine("#region Database Columns");

            foreach (var member in PropertyList)
            {
                file.AppendLine();
                WriteMember(member, file);
            }

            file.AppendLine();
            file.AppendLine("#endregion");

            file.AppendLine();
            file.AppendLine("public object Clone()");
            file.OpenBracket();
            file.AppendLine("return MemberwiseClone();");
            file.CloseBracket();

            file.CloseBracket(); // end of class
        }
        #endregion

        #region Methods
        static void WriteMember(ContractPropertyInfo member, PaddedStringBuilder file)
        {
            if (member.Comments.Any())
            {
                file.AppendLine("/// <summary>");

                var isFirstComment = true;
                foreach (var item in member.Comments)
                {
                    const string PaddingForComment = "     ";

                    if (isFirstComment)
                    {
                        isFirstComment = false;
                        file.AppendLine("///" + PaddingForComment + "" + item);
                    }
                    else
                    {
                        file.AppendLine("///" + PaddingForComment + "<para> " + item + " </para>");
                    }
                }

                file.AppendLine(@"/// </summary>");
            }

            file.AppendLine("public " + member.DotNetType + " " + member.PropertyName + " { get; set; }");
        }
        #endregion
    }
}