using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Services;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    public class LicenseeServiceTest
    {
        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        public LicenseeServiceTest()
        {
            DbSettingProviderManager
               .StartTestingSession()
               .Run();

            _objectDb = new Mock<IObjectDb>();
            _reader = new Mock<IDataReader>();
        }

        [Fact]
        public async Task HasPermissionOnData_CustomerIdAndTransactionIdAreZero_ReturnsTrue()
        {
            // Arrange
            const int custId = 0;
            const long transId = 0;
            var agentList = new Dictionary<int, int[]>();

            var service = new LicenseeService();

            // Action
            var result = await service.HasPermissionOnData(agentList, custId, transId);

            // Assert

            Assert.True(result);
        }

        [Fact]
        public async Task HasPermissionOnData_ContainAgents_ReturnsTrue()
        {
            // Arrange
            const int custId = 0;
            const long transId = 0;
            var agentList = new Dictionary<int, int[]>
            {
                { custId, new[] { 0, } }
            };

            var service = new LicenseeService();

            // Action
            var result = await service.HasPermissionOnData(agentList, custId, transId);

            // Assert

            Assert.True(result);
        }

        [Theory]
        [InlineData(3, 2)]
        [InlineData(1, 3)]
        public async Task HasPermissionOnData_NotContainAgents_ReturnsTrue(int currencyId, long custId)
        {
            // Arrange
            var agentList = new Dictionary<int, int[]>();

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x["CurrencyId"]).Returns(currencyId);
            _reader.Setup(x => x["CustId"]).Returns(custId);

            SetupObjectDb();
            var service = new LicenseeService();

            // Action
            var result = await service.HasPermissionOnData(agentList, 1, 2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsCustomerBelonging_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            const int custId = 1;
            const long transId = 2;

            const int expectedCurrencyId = 3;
            const int expectedCustId = 4;

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x["CurrencyId"]).Returns(expectedCurrencyId);
            _reader.Setup(x => x["CustId"]).Returns(expectedCustId);

            SetupObjectDb();
            var service = new LicenseeService();

            // Action
            var result = await service.IsCustomerBelonging(custId, transId);

            // Assert

            _objectDb.Verify(x => x.SetParameterValue(It.Is<string>(_ => _.Equals("transid")), transId));
            _objectDb.Verify(x => x.SetParameterValue(It.Is<string>(_ => _.Equals("custid")), custId));

            Assert.Equal(expectedCurrencyId, result.Item1);
            Assert.Equal(expectedCustId, result.Item2);
        }

        [Fact]
        public async Task CheckProductPermission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            const int productId = 3;
            const string licenseeName = "Name";
            _objectDb.Setup(obj => obj.ExecuteScalarAsync(null, null, default)).ReturnsAsync(true);

            SetupObjectDb();
            var service = new LicenseeService();

            // Action
            var result = await service.CheckProductPermission(licenseeName, productId);

            // Assert

            _objectDb.Verify(x => x.SetParameterValue(It.Is<string>(_ => _.Equals("productId")), productId));
            _objectDb.Verify(x => x.SetParameterValue(It.Is<string>(_ => _.Equals("licenseeName")), licenseeName));

            Assert.True(result);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);
        }
    }
}
