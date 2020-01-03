using System.Linq;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.EntityClassWriting;
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
            EntityClassCommentMapper.CreateFrom(tableInfo).Write(sb);
        }
        #endregion
    }

    public class EntityClassCommentMapper
    {
        #region Public Methods
        public static EntityClassComment CreateFrom(ITableInfo source)
        {
            return new EntityClassComment
            {
                SchemaName    = source.SchemaName,
                TableName     = source.TableName,
                IndexInfoList = source.IndexInfoList.ToList().ConvertAll(x => x.ToString())
            };
        }
        #endregion
    }
}