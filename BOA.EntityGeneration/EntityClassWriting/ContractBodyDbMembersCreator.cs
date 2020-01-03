using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BOA.EntityGeneration.DbModel;
using BOA.EntityGeneration.DbModel.Interfaces;
using BOA.EntityGeneration.ScriptModel;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.EntityClassWriting
{
    class ContractDbMemberWriter
    {
        #region Public Properties
        public IReadOnlyList<string> Comments     { get; set; }
        public string                DotNetType   { get; set; }
        public string                PropertyName { get; set; }
        #endregion

        #region Properties
        static string PaddingForComment => "     ";
        #endregion

        #region Public Methods
        public void Write(StringBuilder sb)
        {
            if (Comments.Any())
            {
                sb.AppendLine("/// <summary>");

                var isFirstComment = true;
                foreach (var item in Comments)
                {
                    if (isFirstComment)
                    {
                        isFirstComment = false;
                        sb.AppendLine("///" + PaddingForComment + "" + item);
                    }
                    else
                    {
                        sb.AppendLine("///" + PaddingForComment + "<para> " + item + " </para>");
                    }
                }

                sb.AppendLine(@"/// </summary>");
            }

            sb.AppendLine("public " + DotNetType + " " + PropertyName + " { get; set; }");
        }
        #endregion
    }

    class ContractDbMemberWriterMapper
    {
        public static IReadOnlyList<ContractDbMemberWriter> Map(ITableInfo tableInfo)
        {
            return tableInfo.Columns.Select(info => CreateContractDbMemberWriter(tableInfo, info)).ToList();
        }

        static ContractDbMemberWriter CreateContractDbMemberWriter(ITableInfo tableInfo, IColumnInfo data)
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

            return new ContractDbMemberWriter
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
    }

    /// <summary>
    ///     The contract body database members creator
    /// </summary>
    public class ContractBodyDbMembersCreator
    {
        #region Properties
        /// <summary>
        ///     Gets the padding for comment.
        /// </summary>
        static string PaddingForComment => "     ";
        #endregion

        #region Public Methods
        /// <summary>
        ///     Creates the specified table information.
        /// </summary>
        public static ContractBodyDbMembers Create(ITableInfo TableInfo)
        {
            var sb = new StringBuilder();

            sb.AppendLine("#region Database Columns");

            var members = ContractDbMemberWriterMapper.Map(TableInfo);

            foreach (var member in members)
            {
                sb.AppendLine();
                member.Write(sb);
            }

            sb.AppendLine();
            sb.AppendLine("#endregion");

            return new ContractBodyDbMembers {PropertyDefinitions = sb.ToString()};
        }
        #endregion

        
    }
}