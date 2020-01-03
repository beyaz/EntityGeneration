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
            Map(tableInfo).Write(sb);
        }
        #endregion

        #region Methods
        static EntityClassComment Map(ITableInfo source)
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