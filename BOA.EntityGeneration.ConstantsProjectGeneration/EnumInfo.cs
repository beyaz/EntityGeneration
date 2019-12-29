using System;

namespace BOA.EntityGeneration.ConstantsProjectGeneration
{
    [Serializable]
    public class EnumInfo
    {
        #region Public Properties
        public string ProfileName { get; set; }
        public string ClassName     { get; set; }
        public string NumberValue   { get; set; }
        public string PropertyName  { get; set; }
        public string StringValue   { get; set; }
        #endregion
    }
}