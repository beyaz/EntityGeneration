using System;
using System.Collections.Generic;
using BOA.EntityGeneration.DbModel.Types;

namespace BOA.EntityGeneration.ScriptModel
{
    [Serializable]
    public class SelectAllInfo
    {
        #region Public Properties
        public string                    Sql           { get; set; }
        public IReadOnlyList<ColumnInfo> SqlParameters { get; set; }
        #endregion
    }
}