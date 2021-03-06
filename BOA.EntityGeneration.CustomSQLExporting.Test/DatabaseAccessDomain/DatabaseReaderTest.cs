﻿using System.Data;
using System.Data.SqlClient;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BOA.EntityGeneration.CustomSQLExporting.DatabaseAccessDomain
{
    [TestClass]
    public class DatabaseReaderTest
    {
        #region Properties
        static IDbConnection NewConnection => new SqlConnection(@"Server=srvxdev\zumrut;Database=BOACard;Trusted_Connection=True;");
        #endregion

        #region Public Methods
        [TestMethod]
        public void ShouldReadFromDatabase()
        {
            var databaseReader = new DatabaseReader
            {
                Connection = NewConnection,
                ProfileId  = "CreditCardLimit",
                ObjectId   = "get_customer_phone_and_gsm_by_customer_number"
            };

            var customSqlInfo = databaseReader.ReadCustomSqlInfo();

            customSqlInfo.Should().NotBeNull();
            customSqlInfo.Sql.Should().NotBeNullOrWhiteSpace();

            databaseReader.ReadInputParametersFromDatabase().Count.Should().BeGreaterThan(0);
            databaseReader.ReadInputParametersFromDatabase().Count.Should().BeGreaterThan(0);
            databaseReader.GetCustomSqlNamesInfProfile().Count.Should().BeGreaterThan(0);

            var access = new DataAccessDomain.ProjectCustomSqlInfoDataAccess
            {
                Connection = databaseReader.Connection,
                ProfileId  = databaseReader.ProfileId,
                ObjectId   = databaseReader.ObjectId,
                DatabaseReader = databaseReader
            };

            access.GetCustomSqlInfo().Should().NotBeNull();

        }

        
        #endregion
    }
}