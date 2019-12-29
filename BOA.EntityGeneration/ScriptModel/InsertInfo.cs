using System;
using System.Collections.Generic;
using BOA.EntityGeneration.DbModel.Interfaces;

namespace BOA.EntityGeneration.ScriptModel
{
    [Serializable]
    public class InsertInfo
    {
        #region Public Properties
        public string                     Sql           { get; set; }
        public IReadOnlyList<IColumnInfo> SqlParameters { get; set; }
        #endregion
    }
}