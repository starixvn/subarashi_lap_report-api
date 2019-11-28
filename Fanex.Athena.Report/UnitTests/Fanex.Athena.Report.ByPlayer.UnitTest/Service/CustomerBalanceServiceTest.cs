using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Service
{
    public class CustomerBalanceServiceTest
    {
        private readonly IFixture _fixture;

        public CustomerBalanceServiceTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetDataList_WhenCall_PassesCorrectParameter()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            var mockObjectDB = SetUpObjectDb(mockIDataReader);
            var metaModel = new CustomerBalanceMetaModel
            {
                ProductOptions = GetProductOptions()
            };
            var service = new CustomerBalanceService();

            // Action
            await service.GetDataList(metaModel);

            // Assert
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("fromdate", metaModel.FromDate), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("todate", metaModel.ToDate), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("topcount", metaModel.TopCount), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("orderitem", metaModel.OrderItem), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("pagesize", metaModel.PageSize), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("pageindex", metaModel.PageIndex), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("showSB", true), Times.Once);
            mockObjectDB.Verify(objectDb => objectDb.SetParameterValue("showNG", false), Times.Once);
        }

        [Fact]
        public async Task GetDataList_NoData_ReturnsEmptyResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            var service = new CustomerBalanceService();

            // Action
            var result = await service.GetDataList(metaModel);

            // Assert
            Assert.Empty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_NoTotalData_ReturnsCorrectResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            var expected = _fixture.Create<CustomerBalance>();
            ConfigureReaderForResult(expected, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            var service = new CustomerBalanceService();

            // Action
            var result = await service.GetDataList(metaModel);

            // Assert
            Assert.NotEmpty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_WhenCall_ReturnsCorrectResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);
            var expected = _fixture.Create<CustomerBalance>();
            var expectedTotalRow = _fixture.Create<int>();
            ConfigureReaderForResult(expected, mockIDataReader);
            ConfigureReaderForTotalData(expected, expectedTotalRow, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var metaModel = _fixture.Create<CustomerBalanceMetaModel>();
            var service = new CustomerBalanceService();

            // Action
            var result = await service.GetDataList(metaModel);
            var firstItem = result.DataList.First();

            // Assert
            Assert.Equal(expected.ExternalId, firstItem.ExternalId);
            Assert.Equal(expected.SystemId, firstItem.SystemId);
            Assert.True(IsEqualCommonField(expected, firstItem));
            Assert.True(IsEqualCommonField(expected, result.TotalData));
            Assert.Equal(expectedTotalRow, result.TotalRows);
        }

        private bool IsEqualCommonField(CustomerBalance expected, CustomerBalance actual)
        {
            return expected.OpeningBalance == actual.OpeningBalance
                && expected.Deposit == actual.Deposit
                && expected.Withdrawal == actual.Withdrawal
                && expected.Bonus == actual.Bonus
                && expected.Adjustment == actual.Adjustment
                && expected.Turnover == actual.Turnover
                && expected.DrawGame == actual.DrawGame
                && (expected.Turnover - expected.DrawGame) == (actual.Turnover - actual.DrawGame)
                && expected.PendingFunds == actual.PendingFunds
                && expected.WinLoss == actual.WinLoss
                && expected.PlayerComm == actual.PlayerComm
                && (expected.WinLoss + expected.PlayerComm) == (actual.WinLoss + actual.PlayerComm)
                && expected.AffiliateTransfer == actual.AffiliateTransfer
                && expected.ClosingBalance == actual.ClosingBalance
                && expected.TurnoverBA == actual.TurnoverBA
                && expected.CommBA == actual.CommBA
                && expected.AmountDueToBA == actual.AmountDueToBA;
        }

        private static Mock<IObjectDb> SetUpObjectDb(Mock<IDataReader> mockIDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockIDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }

        private IList<ProductOption> GetProductOptions()
        {
            return new List<ProductOption>
            {
                new ProductOption { Code = "SB", Status = true },
                new ProductOption { Code = "NG", Status = false }
            };
        }

        private static void ConfigureReaderForResult(CustomerBalance expected, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["nickname"]).Returns(expected.ExternalId);
            reader.Setup(r => r["username"]).Returns(expected.SystemId);
            MapReader(expected, string.Empty, reader);
            reader.Setup(r => r["TurnOverBA"]).Returns(expected.TurnoverBA);
            reader.Setup(r => r["CommBA"]).Returns(expected.CommBA);
            reader.Setup(r => r["AmountDueToBA"]).Returns(expected.AmountDueToBA);
        }

        private static void ConfigureReaderForTotalData(CustomerBalance expected, int expectedTotalRow, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["TotalRows"]).Returns(expectedTotalRow);
            MapReader(expected, "sum", reader);
            reader.Setup(r => r["sumturnoverba"]).Returns(expected.TurnoverBA);
            reader.Setup(r => r["sumcommba"]).Returns(expected.CommBA);
            reader.Setup(r => r["sumamountduetoba"]).Returns(expected.AmountDueToBA);
        }

        private static void MapReader(CustomerBalance expected, string prefix, Mock<IDataReader> reader)
        {
            var openingBalance = prefix + "openingbalance";
            var deposit = prefix + "deposit";
            var withdrawal = prefix + "withdrawal";
            var bonus = prefix + "bonus";
            var adjustment = prefix + "adjustment";
            var turnover = prefix + "turnover";
            var drawGame = prefix + "drawgame";
            var outstanding = prefix + "outstanding";
            var winloss = prefix + "winloss";
            var playerComm = prefix + "playercomm";
            var closingBalance = prefix + "closingbalance";
            var affiliateTransfer = prefix + "affiliatetransfer";

            reader.Setup(r => r[openingBalance]).Returns(expected.OpeningBalance);
            reader.Setup(r => r[deposit]).Returns(expected.Deposit);
            reader.Setup(r => r[withdrawal]).Returns(expected.Withdrawal);
            reader.Setup(r => r[bonus]).Returns(expected.Bonus);
            reader.Setup(r => r[adjustment]).Returns(expected.Adjustment);
            reader.Setup(r => r[turnover]).Returns(expected.Turnover);
            reader.Setup(r => r[drawGame]).Returns(expected.DrawGame);
            reader.Setup(r => r[outstanding]).Returns(expected.PendingFunds);
            reader.Setup(r => r[winloss]).Returns(expected.WinLoss);
            reader.Setup(r => r[playerComm]).Returns(expected.PlayerComm);
            reader.Setup(r => r[closingBalance]).Returns(expected.ClosingBalance);
            reader.Setup(r => r[affiliateTransfer]).Returns(expected.AffiliateTransfer);
        }
    }
}