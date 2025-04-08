namespace Fanex.Athena.Report.ByPlayer.UnitTests.Service
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ByPlayer.Service;
    using Common.Model.DTO;
    using Data;
    using DTO;
    using MetaModels;
    using Moq;
    using Xunit;

    public class CustomerPendingFundsServiceTest
    {
        private readonly CustomerPendingFundsMetaModel _metaModel;
        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        public CustomerPendingFundsServiceTest()
        {
            DbSettingProviderManager.StartTestingSession().Run();

            _metaModel = new CustomerPendingFundsMetaModel
            {
                ProductOptions = GetProductOptions(),
                BaseCurrency = "UUS"
            };

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public async Task GetDataList_WhenCall_ParametersPassedCorrectly()
        {
            // Action
            await CreateRepository().GetDataList(_metaModel);

            // Assert
            _objectDb.Verify(db => db.SetParameterValue("pagesize", _metaModel.PageSize), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("pageindex", _metaModel.PageIndex), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("AgentList", _metaModel.AgentListXml), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("BaseCurrency", _metaModel.BaseCurrency), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("ProductList", "SB"), Times.Once);
        }

        [Fact]
        public async Task GetDataList_WhenCall_DataReturnedCorrectly()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false).Returns(true).Returns(false);
            _reader.Setup(x => x.NextResult()).Returns(true);

            var expectedTotalData = new CustPendingFunds
            {
                BetCounts = 1,
                PendingFunds = 20m
            };

            var expectedResult = new CustPendingFunds
            {
                CustId = 1,
                ExternalId = "ExternalIdTest",
                SystemId = "SystemIdTest",
                BetCounts = 10,
                PendingFunds = 15m
            };
            const int ExpectedTotalRow = 100;

            ConfigureReaderForTotalData(expectedTotalData, ExpectedTotalRow);
            ConfigureReaderForResult(expectedResult);

            // Action
            var result = await CreateRepository().GetDataList(_metaModel);
            var item = result.DataList.First();

            // Assert
            Assert.Equal(expectedResult.CustId, item.CustId);
            Assert.Equal(expectedResult.ExternalId, item.ExternalId);
            Assert.Equal(expectedResult.SystemId, item.SystemId);
            Assert.Equal(expectedResult.BetCounts, item.BetCounts);
            Assert.Equal(expectedResult.PendingFunds, item.PendingFunds);

            Assert.Equal(expectedTotalData.BetCounts, result.TotalData.BetCounts);
            Assert.Equal(expectedTotalData.PendingFunds, result.TotalData.PendingFunds);

            Assert.Equal(ExpectedTotalRow, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_NoData_ReturnEmptyResult()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(false);
            _reader.Setup(x => x.NextResult()).Returns(false);
            // Action
            var result = await CreateRepository().GetDataList(_metaModel);

            // Assert
            Assert.Empty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_NoTotalDataReturned_DataReturnedCorrectly()
        {
            // Arrange
            var expectedResult = new CustPendingFunds
            {
                ExternalId = "ExternalIdTest",
                SystemId = "SystemIdTest"
            };

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.NextResult()).Returns(false);

            ConfigureReaderForResult(expectedResult);

            // Action
            var result = await CreateRepository().GetDataList(_metaModel);

            // Assert
            Assert.NotEmpty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        private IList<ProductOption> GetProductOptions() =>
            new List<ProductOption>
            {
                new() { Code = "SB", Status = true },
                new() { Code = "NG", Status = false }
            };

        private void ConfigureReaderForResult(CustPendingFunds expected)
        {
            _reader.Setup(r => r["CustID"]).Returns(expected.CustId);
            _reader.Setup(r => r["nickname"]).Returns(expected.ExternalId);
            _reader.Setup(r => r["username"]).Returns(expected.SystemId);
            _reader.Setup(r => r["BetCounts"]).Returns(expected.BetCounts);
            _reader.Setup(r => r["Outstanding"]).Returns(expected.PendingFunds);
        }

        private void ConfigureReaderForTotalData(CustPendingFunds expected, int expectedTotalRow)
        {
            _reader.Setup(r => r["SumBetCounts"]).Returns(expected.BetCounts);
            _reader.Setup(r => r["SumOutstanding"]).Returns(expected.PendingFunds);
            _reader.Setup(r => r["TotalRows"]).Returns(expectedTotalRow);
        }

        private CustomerPendingFundsService CreateRepository()
        {
            SetupObjectDb();
            return new CustomerPendingFundsService();
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