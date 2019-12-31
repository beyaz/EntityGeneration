using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain;

namespace BOA.EntityGeneration.CustomSQLExporting.Wrapper
{
    





    static class Mapper
    {

        public static BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain.DatabaseReader ToDatabaseReader(this Context source)
        {
            return new DatabaseReader
            {
                Connection = source.Connection,
                ProfileId  = source.Input.ProfileId,
                ObjectId   = source.Input.ObjectId
            };
        }
    }
}
