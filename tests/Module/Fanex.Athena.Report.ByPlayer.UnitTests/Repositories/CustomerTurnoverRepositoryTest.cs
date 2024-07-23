using System.Linq;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Repository;
using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Repositories
{
    public class CustomerTurnoverRepositoryTest
    {
        protected const int CurrencyId = 123;

        protected const string CurrencyName = "aaaaaaaaa";

        private const string LanguageCode = "en";

        private readonly BalanceUp _balanceUp;

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Dictionary<int, string> _currencyList = new() { { CurrencyId, CurrencyName }, { 1, "UUS" }, };

        private readonly CustomerTurnoverMetaModel _metaModel = new()
        {
            Username = "UsernameTest"
        };

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        public CustomerTurnoverRepositoryTest()
        {
            _balanceUpService = new Mock<IBalanceUpService>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();

            _balanceUp = new BalanceUp
            {
                MoveBetsDay = DateTimeHelper.CreateDateTime(2017, 2, 2)
            };
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balanceUp);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("User from test", "User from test")]
        public async Task GetDataList_WhenCall_ParametersPassedCorrectly(string metaModelUser, string expectedUser)
        {
            // Arrange
            _metaModel.Username = metaModelUser;

            // Action
            await CreateRepository().GetDataList(_metaModel, _currencyList, LanguageCode);

            // Assert
            _objectDb.Verify(m => m.SetParameterValue("fromdate", _metaModel.FromDate), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("todate", _metaModel.ToDate), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("odds", _metaModel.Odds), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("reportby", _metaModel.ReportBy), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("username", expectedUser), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("movebetsday", _balanceUp.MoveBetsDay), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("Langkey", LanguageCode), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("PageSize", _metaModel.PageSize), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("PageIndex", _metaModel.PageIndex), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("currencyid", _metaModel.CurrencyId), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("IsExportExcel", _metaModel.IsExportExcel), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("showwin", _metaModel.ShowWin), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("showloss", _metaModel.ShowLoss), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("showdraw", _metaModel.ShowDraw), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("Product", _metaModel.ProductIds), Times.Once);
            _objectDb.Verify(m => m.SetParameterValue("Bettype", _metaModel.Bettypes), Times.Once);
        }

        [Fact]
        public async Task GetDataList_NoData_ReturnEmptyResult()
        {
            // Arrange
            _reader.Setup(x => x.Read()).Returns(false);
            _reader.Setup(x => x.NextResult()).Returns(false);

            // Action
            var result = await CreateRepository().GetDataList(_metaModel, _currencyList, LanguageCode);

            // Assert
            Assert.Empty(result.DataList);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public async Task GetDataList_WhenCall_DataReturnedCorrectly()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false).Returns(true).Returns(false);
            _reader.Setup(x => x.NextResult()).Returns(true);

            var expectedTotalData = new CustTurnover
            {
                TotalTransactions = 5,
                Turnover = 10m,
                TotalTurnover = 10m
            };
            var expectedResult = new CustTurnover
            {
                ExternalId = "NickNameTest",
                SystemId = "UserNameTest",
                Bettype = "BettypeTest",
                SportId = 1,
                ReportBy = DateTimeHelper.CreateDateTime(2016, 8, 31)
            };
            const int ExpectedTotalRow = 100;

            ConfigureReaderForTotalData(expectedTotalData, ExpectedTotalRow);
            ConfigureReaderForResult(expectedResult);

            // Action
            var result = await CreateRepository().GetDataList(_metaModel, _currencyList, LanguageCode);
            var firstItem = result.DataList.First();
            var totalData = result.TotalData;

            // Assert
            Assert.Equal(expectedResult.SportId, firstItem.SportId);
            Assert.Equal(expectedResult.ReportBy, firstItem.ReportBy);
            Assert.Equal(expectedResult.ExternalId, firstItem.ExternalId);
            Assert.Equal(expectedResult.SystemId, firstItem.SystemId);
            Assert.Equal(CurrencyName, firstItem.Currency);
            Assert.Equal(expectedResult.Bettype, firstItem.Bettype);

            // Call your AssertCommonData method with retrieved values
            AssertCommonData(firstItem, expectedResult);

            // Call your AssertCommonData method with retrieved total data
            AssertCommonData(totalData, expectedTotalData);

            // Assertion on result.TotalRows
            Assert.Equal(ExpectedTotalRow, result.TotalRows);
        }

        private static void AssertCommonData(CustTurnover actual, CustTurnover expected)
        {
            Assert.Equal(expected.TotalTransactions, actual.TotalTransactions);
            Assert.Equal(expected.Turnover, actual.Turnover);
            Assert.Equal(expected.CustomerWinLoss, actual.CustomerWinLoss);
            Assert.Equal(expected.CustomerComm, actual.CustomerComm);
            Assert.Equal(expected.EffectiveBettingValue, actual.EffectiveBettingValue);
        }

        private void ConfigureReaderForResult(CustTurnover expected)
        {
            _reader.Setup(r => r["RowNo"]).Returns(expected.Index);
            _reader.Setup(r => r["Turnover"]).Returns(expected.Turnover);
            _reader.Setup(r => r["sporttype"]).Returns(expected.SportId);
            _reader.Setup(r => r["ReportBy"]).Returns("20160831");
            _reader.Setup(r => r["nickname"]).Returns(expected.ExternalId);
            _reader.Setup(r => r["username"]).Returns(expected.SystemId);
            _reader.Setup(r => r["currencyid"]).Returns(CurrencyId);
            _reader.Setup(r => r["BetCount"]).Returns(expected.TotalTransactions);
            _reader.Setup(r => r["Bettype"]).Returns(expected.Bettype);
            _reader.Setup(r => r["Winlost"]).Returns(expected.CustomerWinLoss);
            _reader.Setup(r => r["BetEffective"]).Returns(expected.EffectiveBettingValue);
            _reader.Setup(r => r["PlayerComm"]).Returns(expected.CustomerComm);
        }

        private void ConfigureReaderForTotalData(CustTurnover expected, int expectedTotalRow)
        {
            _reader.Setup(r => r["SumBetCount"]).Returns(expected.TotalTransactions);
            _reader.Setup(r => r["SumTurnover"]).Returns(expected.Turnover);
            _reader.Setup(r => r["SumWinlost"]).Returns(expected.CustomerWinLoss);
            _reader.Setup(r => r["SumComm"]).Returns(expected.CustomerComm);
            _reader.Setup(r => r["SumBetEffective"]).Returns(expected.EffectiveBettingValue);

            _reader.Setup(r => r["TotalRows"]).Returns(expectedTotalRow);
        }

        private CustomerTurnoverRepository CreateRepository()
        {
            SetupObjectDb();
            return new CustomerTurnoverRepository(_balanceUpService.Object, _cacheRefAdapter.Object);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            DbSettingProviderManager.StartTestingSession().Run();
            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => _objectDb.Object);
        }
    }
}
