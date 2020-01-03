using System.Collections.Generic;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.EntityClassWriting
{
    public class EntityClassComment
    {
        #region Public Properties
        public IReadOnlyList<string> IndexInfoList { get; set; }
        public string                SchemaName    { get; set; }
        public string                TableName     { get; set; }
        #endregion

        #region Public Methods
        public void Write(PaddedStringBuilder sb)
        {
            const string Padding = "     ";
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"///{Padding}Entity contract for table {SchemaName}.{TableName}");

            foreach (var indexInfo in IndexInfoList)
            {
                sb.AppendLine($"///{Padding}<para>{indexInfo}</para>");
            }

            sb.AppendLine("/// </summary>");
        }
        #endregion
    }
}