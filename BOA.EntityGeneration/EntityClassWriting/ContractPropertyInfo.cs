using System.Collections.Generic;

namespace BOA.EntityGeneration.EntityClassWriting
{
    public class ContractPropertyInfo
    {
        #region Public Properties
        public IReadOnlyList<string> Comments     { get; set; }
        public string                DotNetType   { get; set; }
        public string                PropertyName { get; set; }
        #endregion
    }
}