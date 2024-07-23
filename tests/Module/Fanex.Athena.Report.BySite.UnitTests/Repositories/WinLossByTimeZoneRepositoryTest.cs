namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BySite.Repositories;
    using Common.Helpers;
    using Common.Model.DTO;
    using Common.Services.Interfaces;
    using Data;
    using DTOs;
    using MetaModels;
    using Moq;
    using Report.UnitTests.Common;
    using Resources;
    using Resources.Constants;
    using Xunit;

    public class WinLossByTimeZoneRepositoryTest
    {
        private const byte GroupByProduct = 1;

        private const int ReportBySummary = 1;

        private const int ReportByMonthly = 3;

        private const int CurrencyId = 123;

        private const string CurrencyName = "aaaaaaaaa";

        private readonly BalanceUp _balance = new()
        {
            MoveBetsDay = DateTimeHelper.CreateDateTime(2017, 8, 25),
            BalanceUpDay = DateTimeHelper.CreateDateTime(2017, 8, 31)
        };

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        private readonly IReadOnlyDictionary<int, string> UserCurrencyList = new Dictionary<int, string>
        {
            { CurrencyId, CurrencyName }, { 1, "USD" }
        };

        public WinLossByTimeZoneRepositoryTest()
        {
            DbSettingProviderManager.StartTestingSession().Run();

            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balance);

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public async Task GetData_WhenCall_PassCorrectParams()
        {
            // Arrange
            _reader.Setup(x => x.Read()).Returns(false);

            var metaModel = GetDefaultMetaModel();

            // Action
            await CreateRepository().GetData(metaModel, UserCurrencyList);

            // Assert
            AssertParamsPassCorrectly(metaModel);
        }

        [Fact]
        public async Task GetData_ConvertAll_ReturnCorrectData()
        {
            // Arrange
            var metaModel = GetDefaultMetaModel();

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x["GroupBy"]).Returns(CurrencyId.ToString());
            _reader.Setup(x => x["BetCount"]).Returns(1);

            var expectedData = new WinLossByTimeZone
            {
                BetCount = 1,
                Turnover = 2,
                CustomerComm = 3,
                LicenseeWinLoss = 4,
                LicenseeComm = 5
            };
            MapBaseCurrencyReaderData(expectedData);

            // Action
            var result = await CreateRepository().GetData(metaModel, UserCurrencyList);
            var firstItem = result.DataList.ElementAt(0);
            var total = result.TotalData;

            // Assert
            Assert.Equal(CurrencyName, firstItem.GroupBy);
            AssertData(firstItem, expectedData);
            AssertData(total, expectedData);
        }

        [Fact]
        public async Task GetData_DoNotConvertAll_ReturnCorrectData()
        {
            // Arrange
            var metaModel = GetDefaultMetaModel();
            metaModel.BaseCurrency = "AAA";
            metaModel.IsConvertAll = false;

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);

            var expectedData = new WinLossByTimeZone
            {
                GroupBy = CurrencyId.ToString(),
                BetCount = 1,
                Turnover = 2,
                CustomerComm = 3,
                LicenseeWinLoss = 4,
                LicenseeComm = 5
            };
            MapReaderData(expectedData);

            var expectedBaseData = new WinLossByTimeZone
            {
                GroupBy = CurrencyId.ToString(),
                BetCount = expectedData.BetCount,
                Turnover = 20,
                CustomerComm = 30,
                LicenseeWinLoss = 40,
                LicenseeComm = 50
            };
            MapBaseCurrencyReaderData(expectedBaseData);

            // Action
            var result = await CreateRepository().GetData(metaModel, UserCurrencyList);
            var firstItem = result.DataList.ElementAt(0);
            var total = result.TotalData;

            // Assert
            Assert.Equal(CurrencyName, firstItem.GroupBy);
            AssertData(firstItem, expectedData);
            AssertData(total, expectedBaseData);
        }

        [Fact]
        public async Task GetData_ReportByIsNotSummary_ReturnCorrectReportByData()
        {
            // Arrange
            var metaModel = GetDefaultMetaModel();
            metaModel.ReportBy = ReportByMonthly;

            const string ReportBy = "20170102";
            _reader.Setup(x => x["reportby"]).Returns(ReportBy);

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);

            var data = new WinLossByTimeZone
            {
                GroupBy = CurrencyId.ToString()
            };
            MapReaderData(data);

            var baseData = FakeHelper.Create<WinLossByTimeZone>();
            MapBaseCurrencyReaderData(baseData);

            var expected = DateTimeHelper.CreateDateTime(2017, 1, 1);

            // Action
            var result = await CreateRepository().GetData(metaModel, UserCurrencyList);
            var firstItem = result.DataList.ElementAt(0);

            // Assert
            Assert.Equal(expected, firstItem.ReportBy);
        }

        [Fact]
        public async Task GetData_GroupByProduct_ReturnCorrectGroupByData()
        {
            // Arrange
            var metaModel = GetDefaultMetaModel();
            metaModel.GroupBy = GroupByProduct;

            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            var data = new WinLossByTimeZone
            {
                GroupBy = Product.BA
            };
            MapReaderData(data);

            var baseData = FakeHelper.Create<WinLossByTimeZone>();
            MapBaseCurrencyReaderData(baseData);

            // Action
            var result = await CreateRepository().GetData(metaModel, UserCurrencyList);
            var firstItem = result.DataList.ElementAt(0);

            // Assert
            Assert.Equal(ViewResources.ProductBA, firstItem.GroupBy);
        }

        private static void AssertData(WinLossByTimeZone actual, WinLossByTimeZone expected)
        {
            Assert.Equal(expected.BetCount, actual.BetCount);
            Assert.Equal(expected.Turnover, actual.Turnover);
            Assert.Equal(expected.CustomerComm, actual.CustomerComm);
            Assert.Equal(expected.LicenseeWinLoss, actual.LicenseeWinLoss);
            Assert.Equal(expected.LicenseeComm, actual.LicenseeComm);
        }

        private static WinLossByTimeZoneMetaModel GetDefaultMetaModel() =>
            new()
            {
                BaseCurrency = string.Empty,
                FromDate = DateTimeHelper.CreateDateTime(2017, 1, 1),
                ToDate = DateTimeHelper.CreateDateTime(2017, 2, 2),
                ProductCodeList = "SB,BA",
                AgentListXml = "<Param></Param>",
                TimeZone = -4,
                IsIncludeDrawData = true,
                ReportBy = ReportBySummary,
                GroupBy = 2,
                IsConvertAll = true
            };

        private void AssertParamsPassCorrectly(WinLossByTimeZoneMetaModel metaModel)
        {
            _objectDb.Verify(db => db.SetParameterValue("FromDate", metaModel.FromDate), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("ToDate", metaModel.ToDate), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("MoveBetsDay", _balance.MoveBetsDay), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("ProductList", metaModel.ProductCodeList), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("TimeZone", metaModel.TimeZone), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("ReportBy", metaModel.ReportBy), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("GroupBy", metaModel.GroupBy), Times.Once);
            _objectDb.Verify(db => db.SetParameterValue("IncludeDraw", metaModel.IsIncludeDrawData), Times.Once);
        }

        private void MapBaseCurrencyReaderData(WinLossByTimeZone data)
        {
            _reader.Setup(r => r["TurnoverBase"]).Returns(data.Turnover);
            _reader.Setup(r => r["PlayerCommBase"]).Returns(data.CustomerComm);
            _reader.Setup(r => r["LicenseeWLBase"]).Returns(data.LicenseeWinLoss);
            _reader.Setup(r => r["LicenseeCommBase"]).Returns(data.LicenseeComm);
        }

        private void MapReaderData(WinLossByTimeZone data)
        {
            _reader.Setup(r => r["GroupBy"]).Returns(data.GroupBy);
            _reader.Setup(r => r["BetCount"]).Returns(data.BetCount);
            _reader.Setup(r => r["Turnover"]).Returns(data.Turnover);
            _reader.Setup(r => r["PlayerComm"]).Returns(data.CustomerComm);
            _reader.Setup(r => r["LicenseeWL"]).Returns(data.LicenseeWinLoss);
            _reader.Setup(r => r["LicenseeComm"]).Returns(data.LicenseeComm);
        }

        private WinLossByTimeZoneRepository CreateRepository()
        {
            SetupObjectDb();
            return new WinLossByTimeZoneRepository(_balanceUpService.Object);
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