using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class MatchWinLossRepositoryTest
    {
        private readonly BalanceUp _balance = new()
        {
            MoveBetsDay = DateTimeHelper.CreateDateTime(2016, 8, 10),
            BalanceUpDay = DateTimeHelper.CreateDateTime(2016, 8, 31)
        };

        private readonly Mock<IBalanceUpService> _balanceUpService;

        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly MatchWinLossMetaModel _metaModel = new()
        {
            BaseCurrency = "USD",
            ProductCodeList = "SB",
            ProductOptions = new List<ProductOption>
            {
                new() { Code = "SB", Status = true, },
                new() { Code = "NG", Status = false, }
            },
            AgentListXml = "<xml>",
            Date = DateTimeHelper.CreateDateTime(2017, 1, 1),
            PageSize = 100
        };

        private readonly Mock<IObjectDb> _objectDb;

        private readonly Mock<IDataReader> _reader;

        public MatchWinLossRepositoryTest()
        {
            DbSettingProviderManager.StartTestingSession().Run();

            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balance);

            _reader = new Mock<IDataReader>();
            _objectDb = new Mock<IObjectDb>();
        }

        [Fact]
        public async Task GetDataList_WhenCall_ParametersPassedCorrectly()
        {
            // Arrange
            SetupObjectDb();

            // Action
            await MockRepository().GetDataList(_metaModel);

            // Assert
            _objectDb.Verify(x => x.SetParameterValue("AgentList", _metaModel.AgentListXml), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("Date", _metaModel.Date), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("MoveBetsDay", _balance.MoveBetsDay), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("ProductList", "SB"), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("BaseCurrency", _metaModel.BaseCurrency), Times.Once);
        }

        [Fact]
        public async Task SetToList_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "leagueid", 1 },
                { "matchid", 2 },
                { "Turnover", 100 },
                { "WinLost", 200 }
            };

            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal(expected["leagueid"], result[0].LeagueId);
            Assert.Equal(expected["matchid"], result[0].MatchId);
            Assert.Equal(expected["Turnover"].ToString(), result[0].Turnover.ToString());
            Assert.Equal(expected["WinLost"].ToString(), result[0].CustomerWL.ToString());
        }

        [Fact]
        public async Task SetToList_HasCustomerComm_ReturnCorrectData()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(1);
            _reader.Setup(x => x.GetName(0)).Returns("PlayerComm");

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "PlayerComm", 200 }
            };
            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(expected["PlayerComm"].ToString(), result[0].CustomerComm.ToString());
        }

        [Fact]
        public async Task SetToList_HasBetTypeColumn_ReturnCorrectData()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(1);
            _reader.Setup(x => x.GetName(0)).Returns("bettype");

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "bettype", 2 }
            };
            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(expected["bettype"], result[0].BetType);
        }

        [Fact]
        public async Task SetToList_HasProductNameColumn_ReturnCorrectData()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(1);
            _reader.Setup(x => x.GetName(0)).Returns("productname");

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "productname", "Test" }
            };
            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(expected["productname"], result[0].ProductName);
        }

        [Fact]
        public async Task SetToList_HasLiveIndicatorColumn_ReturnCorrectData()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(1);
            _reader.Setup(x => x.GetName(0)).Returns("liveindicator");

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "liveindicator", false }
            };
            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(expected["liveindicator"], result[0].IsLive);
        }

        [Fact]
        public async Task SetToList_HasSportTypeColumn_ReturnCorrectData()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(1);
            _reader.Setup(x => x.GetName(0)).Returns("sporttype");

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "sporttype", 1 }
            };
            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(expected["sporttype"].ToString(), result[0].SportType.ToString());
        }

        [Fact]
        public async Task SetToList_HasKickOffTimeColumn_ReturnCorrectData()
        {
            // Arrange
            _reader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            _reader.Setup(x => x.FieldCount).Returns(1);
            _reader.Setup(x => x.GetName(0)).Returns("KickoffTime");

            var expected = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "KickoffTime", DateTimeHelper.CreateDateTime(2017, 1, 2) }
            };
            SetUpData(_reader, expected);
            SetupObjectDb();

            // Action
            var result = await MockRepository().GetDataList(_metaModel);

            // Assert
            Assert.Equal(expected["KickoffTime"], result[0].KickOffTime);
        }

        [Fact]
        public async Task GetDetailDataSet_WhenCall_ParametersPassedCorrectly()
        {
            // Arrange
            SetupObjectDb();

            // Action
            await MockRepository().GetDetailDataSet(_metaModel);

            // Assert
            _objectDb.Verify(x => x.SetParameterValue("AgentList", _metaModel.AgentListXml), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("Date", _metaModel.Date), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("MoveBetsDay", _balance.MoveBetsDay), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("PageSize", _metaModel.PageSize), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("ProductList", "SB"), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("BaseCurrency", _metaModel.BaseCurrency), Times.Once);
        }

        [Fact]
        public async Task GetDetailDataList_WhenCall_ParametersPassedCorrectly()
        {
            // Arrange
            SetupObjectDb();

            // Action
            await MockRepository().GetDetailDataList(_metaModel);

            // Assert
            _objectDb.Verify(x => x.SetParameterValue("AgentList", _metaModel.AgentListXml), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("MatchId", _metaModel.MatchId), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("Date", _metaModel.Date), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("MoveBetsDay", _balance.MoveBetsDay), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("BalanceUpDay", _balance.BalanceUpDay), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("ProductList", "SB"), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("BaseCurrency", _metaModel.BaseCurrency), Times.Once);
        }

        [Fact]
        public async Task GetOutrightParlayList_WhenCall_ParametersPassedCorrectly()
        {
            // Arrange
            SetupObjectDb();

            // Action
            await MockRepository().GetOutrightParlayList(_metaModel);

            // Assert
            _objectDb.Verify(x => x.SetParameterValue("AgentList", _metaModel.AgentListXml), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("Date", _metaModel.Date), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("MoveBetsDay", _balance.MoveBetsDay), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("ProductList", "SB"), Times.Once);
            _objectDb.Verify(x => x.SetParameterValue("BaseCurrency", _metaModel.BaseCurrency), Times.Once);
        }

        private static void SetUpData(Mock<IDataReader> reader, IDictionary<string, object> expectedData)
        {
            reader.Setup(r => r[It.IsAny<string>()])
                  .Returns<string>(x => expectedData.ContainsKey(x) ? expectedData[x] : string.Empty);
        }

        private void SetupObjectDb()
        {
            _objectDb.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(),
                          It.IsAny<CancellationToken>()))
                     .Returns(Task.FromResult(_reader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y, z) => _objectDb.Object);
        }

        private MatchWinLossRepository MockRepository() => new(_balanceUpService.Object, _cacheRefAdapter.Object);
    }
}