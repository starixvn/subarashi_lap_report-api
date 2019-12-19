using System;
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
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Service
{
    public class CustomerWinLossServiceTest
    {
        private const int PageSizeExcel = int.MaxValue - 1000;

        private readonly BalanceUp _balance = new BalanceUp
        {
            MoveBetsDay = new DateTime(2016, 8, 10),
            BalanceUpDay = new DateTime(2016, 8, 31)
        };

        private readonly Mock<IBalanceUpService> _balanceUpService = new Mock<IBalanceUpService>();

        private readonly IFixture _fixture = new Fixture();

        public CustomerWinLossServiceTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _balanceUpService.Setup(bs => bs.GetBalanceUpAsync()).Returns(Task.FromResult(_balance));
        }

        [Theory]
        [InlineData(true, 5, PageSizeExcel)]
        [InlineData(false, 5, 5)]
        public async Task GetDataList_WhenCall_ParametersPassedCorrectly(bool isExportExcel, int pageSize, int expectedPageSize)
        {
            // Arrange
            var metaModel = new CustomerWinLossMetaModel
            {
                ProductOptions = GetProductOptions(),
                BaseCurrency = "UUS",
                PageSize = pageSize,
                IsExportExcel = isExportExcel
            };
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(false);
            var mockObjectDB = SetUpObjectDb(mockIDataReader);
            var service = new CustomerWinLossService(_balanceUpService.Object);

            // Action
            await service.GetDataList(metaModel, !isExportExcel);

            // Assert
            mockObjectDB.Verify(od => od.SetParameterValue("fromdate", metaModel.FromDate), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("todate", metaModel.ToDate), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("pagesize", expectedPageSize), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("pageindex", metaModel.PageIndex), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("movebetsday", _balance.MoveBetsDay), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("BaseCurrency", metaModel.BaseCurrency), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("showSB", true), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("showNG", false), Times.Once);
            mockObjectDB.Verify(od => od.SetParameterValue("CheckPromotionPrize", !isExportExcel), Times.Once);
        }

        [Fact]
        public async Task GetDataList_WhenCall_DataReturnedCorrectly()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);

            var expectedTotalData = _fixture.Create<CustomerWinLoss>();
            var expectedResult = _fixture.Create<CustomerWinLoss>();

            const int ExpectedTotalRow = 100;
            const int ExpectedPromotionPrize = 1;

            ConfigureReaderForResult(expectedResult, mockIDataReader);
            ConfigureReaderForTotalData(expectedTotalData, ExpectedTotalRow, ExpectedPromotionPrize, mockIDataReader);

            SetUpObjectDb(mockIDataReader);
            var service = new CustomerWinLossService(_balanceUpService.Object);

            // Action
            var result = await service.GetDataList(_fixture.Create<CustomerWinLossMetaModel>());
            var firstItem = result.DataList.First();

            // Assert
            Assert.Equal(firstItem.CustId, expectedResult.CustId);
            Assert.Equal(firstItem.ExternalId, expectedResult.ExternalId);
            Assert.Equal(firstItem.SystemId, expectedResult.SystemId);
            Assert.Equal(firstItem.BetEffective, expectedResult.BetEffective);
            Assert.Equal(firstItem.CustomerTotalWL, expectedResult.CustomerTotalWL);
            AssertCommonData(firstItem, expectedResult);

            AssertCommonData(result.TotalData, expectedTotalData);
            Assert.Equal(result.TotalData.LicenseeWL, expectedTotalData.LicenseeWL);

            Assert.Equal(result.TotalRows, ExpectedTotalRow);
            Assert.Equal(result.AdditionalInfo, ExpectedPromotionPrize.ToString());
        }

        [Fact]
        public async Task GetDataList_NoDataButHaveTotal_ReturnEmptyResult()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { false, true, false }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(true);

            const int ExpectedTotalRow = 10;
            const int ExpectedPromotionPrize = 1;
            var expectedTotalData = _fixture.Create<CustomerWinLoss>();
            ConfigureReaderForTotalData(expectedTotalData, ExpectedTotalRow, ExpectedPromotionPrize, mockIDataReader);

            SetUpObjectDb(mockIDataReader);
            var service = new CustomerWinLossService(_balanceUpService.Object);

            // Action
            var result = await service.GetDataList(_fixture.Create<CustomerWinLossMetaModel>());

            // Assert
            Assert.Empty(result.DataList);
            Assert.Equal(0, result.TotalRows);
            Assert.Equal(ExpectedPromotionPrize.ToString(), result.AdditionalInfo);
        }

        [Fact]
        public async Task GetDataList_NoTotalDataReturned_DataReturnedCorrectly()
        {
            // Arrange
            var mockIDataReader = new Mock<IDataReader>();
            mockIDataReader.Setup(dr => dr.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockIDataReader.Setup(dr => dr.NextResult()).Returns(false);
            var expectedResult = _fixture.Create<CustomerWinLoss>();

            ConfigureReaderForResult(expectedResult, mockIDataReader);
            SetUpObjectDb(mockIDataReader);
            var service = new CustomerWinLossService(_balanceUpService.Object);

            // Action
            var result = await service.GetDataList(_fixture.Create<CustomerWinLossMetaModel>());

            // Assert
            Assert.NotEmpty(result.DataList);
            Assert.Equal(0, result.TotalRows);
            Assert.Equal("0", result.AdditionalInfo);
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

        private static void ConfigureReaderForResult(CustomerWinLoss expected, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["CustID"]).Returns(expected.CustId);
            reader.Setup(r => r["nickname"]).Returns(expected.ExternalId);
            reader.Setup(r => r["username"]).Returns(expected.SystemId);
            MapReader(expected, false, reader);
        }

        private static void ConfigureReaderForTotalData(CustomerWinLoss expected, int expectedTotalRow, int expectedPromotionPrize, Mock<IDataReader> reader)
        {
            reader.Setup(r => r["TotalRows"]).Returns(expectedTotalRow);
            reader.Setup(r => r["PromotionPrize"]).Returns(expectedPromotionPrize);
            MapReader(expected, true, reader);
        }

        private static void MapReader(CustomerWinLoss expected, bool isSum, Mock<IDataReader> reader)
        {
            var prefix = isSum ? "sum" : string.Empty;
            var turnoverColumn = prefix + "turnover";
            var memberWinLostColumn = prefix + "memberwinlost";
            var betEffectiveColumn = prefix + "beteffective";
            var playerCommColumn = prefix + "playercomm";
            var licenseeWinLostColumn = prefix + "licenseewl";
            var bacommColumn = prefix + "commba";
            var winlossrbColumn = prefix + "winlossrb";
            var commrbColumn = prefix + "commrb";
            var amtduetobaColumn = prefix + "amtduetoba";
            var sumcustomertotalwlColumn = prefix + "customertotalwl";

            reader.Setup(r => r[turnoverColumn]).Returns(expected.Turnover);
            reader.Setup(r => r[prefix + "betcount"]).Returns(expected.TotalTransaction);
            reader.Setup(r => r[memberWinLostColumn]).Returns(expected.CustomerWL);
            reader.Setup(r => r[betEffectiveColumn]).Returns(expected.BetEffective);
            reader.Setup(r => r[playerCommColumn]).Returns(expected.PlayerComm);
            reader.Setup(r => r[licenseeWinLostColumn]).Returns(expected.LicenseeWL);
            reader.Setup(r => r[bacommColumn]).Returns(expected.BAComm);
            reader.Setup(r => r[winlossrbColumn]).Returns(15m);
            reader.Setup(r => r[commrbColumn]).Returns(5m);
            reader.Setup(r => r[amtduetobaColumn]).Returns(expected.AmtDueToBA);
            reader.Setup(r => r[sumcustomertotalwlColumn]).Returns(expected.CustomerTotalWL);
        }

        private static void AssertCommonData(CustomerWinLoss actual, CustomerWinLoss expected)
        {
            Assert.Equal(actual.Turnover, expected.Turnover);
            Assert.Equal(actual.TotalTransaction, expected.TotalTransaction);
            Assert.Equal(actual.CustomerWL, expected.CustomerWL);
            Assert.Equal(actual.PlayerComm, expected.PlayerComm);
        }
    }
}