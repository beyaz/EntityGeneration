using BOA.EntityGeneration.DbModel.Interfaces;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.ScriptModel.Creators
{
    public static class ContractCommentInfoCreator
    {
        #region Public Methods
        public static ContractCommentInfo Create(ITableInfo tableInfo)
        {
            var sb = new PaddedStringBuilder();

            Write(sb, tableInfo);

            return new ContractCommentInfo
            {
                Comment = sb.ToString()
            };
        }

        public static void Write(PaddedStringBuilder sb, ITableInfo tableInfo)
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"///{Padding.ForComment}Entity contract for table {tableInfo.SchemaName}.{tableInfo.TableName}");

            foreach (var indexInfo in tableInfo.IndexInfoList)
            {
                sb.AppendLine($"///{Padding.ForComment}<para>{indexInfo}</para>");
            }

            sb.AppendLine("/// </summary>");
        }
        #endregion
    }
}