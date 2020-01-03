using System.Collections.Generic;
using BOA.EntityGeneration.ScriptModel;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.EntityClassWriting
{
    class EntityClassComment
    {
        #region Public Properties
        public IReadOnlyList<string> IndexInfoList { get; set; }
        public string                SchemaName    { get; set; }
        public string                TableName     { get; set; }
        #endregion

        #region Public Methods
        public void Write(PaddedStringBuilder sb)
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"///{Padding.ForComment}Entity contract for table {SchemaName}.{TableName}");

            foreach (var indexInfo in IndexInfoList)
            {
                sb.AppendLine($"///{Padding.ForComment}<para>{indexInfo}</para>");
            }

            sb.AppendLine("/// </summary>");
        }
        #endregion
    }
}