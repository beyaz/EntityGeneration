using System;
using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.EntityClassWriting
{
    public class ContractPropertyInfoMapper
    {
        #region Public Methods
        public static IReadOnlyList<ContractPropertyInfo> Map(ITableInfo tableInfo)
        {
            return tableInfo.Columns.Select(info => CreateContractPropertyInfo(tableInfo, info)).ToList();
        }
        #endregion

        #region Methods
        static ContractPropertyInfo CreateContractPropertyInfo(ITableInfo tableInfo, IColumnInfo data)
        {
            var dotNetType = data.DotNetType;
            if (data.ColumnName == Names2.VALID_FLAG)
            {
                dotNetType = DotNetTypeName.DotNetBool;
            }

            var comment = data.Comment;

            var commentList = new List<string>();

            if (comment.HasValue())
            {
                commentList.AddRange(comment.Split(Environment.NewLine.ToCharArray()));
            }

            var extraComment = GetIndexComment(tableInfo, data);
            if (extraComment.HasValue())
            {
                commentList.Add(extraComment);
            }

            return new ContractPropertyInfo
            {
                PropertyName = data.ColumnName.ToContractName(),
                DotNetType   = dotNetType,
                Comments     = commentList
            };
        }

        static string GetIndexComment(ITableInfo tableInfo, IColumnInfo columnInfo)
        {
            var extraComment = string.Empty;

            var indexInfo = tableInfo.IndexInfoList.FirstOrDefault(x => x.ColumnNames.Contains(columnInfo.ColumnName));

            if (indexInfo != null)
            {
                extraComment = indexInfo.ToString();
            }

            return extraComment;
        }
        #endregion
    }
}