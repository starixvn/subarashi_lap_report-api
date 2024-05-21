using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class LeagueWinLossRepositoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly BalanceUp _balance = new BalanceUp
        {
            MoveBetsDay = new DateTime(2016, 8, 10),
            BalanceUpDay = new DateTime(2016, 8, 31)
        };

        private readonly Dictionary<string, object> _expectedCommonData = new()
        {
            { "winlost", 100 },
            { "betcount", 50 },
            { "LicenseeWL", 200 },
            { "turnover", 150 },
            { "BAComm", 200 }
        };

        public LeagueWinLossRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(b => b.GetBalanceUpAsync()).Returns(Task.FromResult(_balance));

            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _cacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<string>(), It.IsAny<string>())).Returns(_fixture.Create<string>());
            _cacheRefAdapter.Setup(c => c.GetBettype(It.IsAny<int>(), false)).Returns(_fixture.Create<string>());
            _cacheRefAdapter.Setup(c => c.GetTeamName(It.IsAny<int>())).Returns(_fixture.Create<string>());
            _cacheRefAdapter.Setup(c => c.GetMatchName(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_fixture.Create<string>());
            _cacheRefAdapter.Setup(c => c.GetLeagueName(It.IsAny<int>())).Returns(_fixture.Create<string>());
            _cacheRefAdapter.Setup(c => c.GetLeagueName3rd(It.IsAny<int>())).Returns(_fixture.Create<string>());
        }

        public LeagueWinLossRepository CreateRepository()
        {
            return new LeagueWinLossRepository(_balanceUpService.Object, _cacheRefAdapter.Object);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(9, true)]
        public async Task GetLeagueWinlossDetail_WhenCall_ReturnCorrectData(int sportType, bool breakdown)
        {
            // Arrange
            var expectedLeagueData = new Dictionary<string, object>
            {
                { "LeagueId", 20156 },
                { "SportType", sportType }
            };
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedLeagueData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetLeagueWinlossDetail(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            Assert.Equal(breakdown ? 0 : 20156, data.LeagueId);
            Assert.Equal(sportType, data.SportType);
            Assert.Equal(breakdown, data.IsBreakdown);
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetExtraPayoutMainData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetLeagueWinlossDetail(metaModel);

            // Assert
            Assert.Empty(result.DataList);
        }

        [Fact]
        public async Task GetMatchWinlossDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var expectedMatchData = new Dictionary<string, object>
            {
                { "MatchId", 123 },
                { "HomeId", 2451 },
                { "AwayId", 5542 },
                { "IsNeutral", true },
                { "EventType", 1 }
            };
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedMatchData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetMatchWinlossDetail(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            Assert.Equal(123, data.Detail.FirstOrDefault().MatchId);
            Assert.Equal(1, data.Detail.FirstOrDefault().EventType);
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetMatchWinlossDetail_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetMatchWinlossDetail(metaModel);

            // Assert
            Assert.Empty(result.DataList.FirstOrDefault().Detail);
        }

        [Fact]
        public async Task GetBettypeWinlossDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var expectedMatchData = new Dictionary<string, object>
            {
                { "BetType", 123 }
            };
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedMatchData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetBettypeWinlossDetail(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            Assert.Equal(123, data.Detail.FirstOrDefault().BetTypeId);
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetBettypeWinlossDetail_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetBettypeWinlossDetail(metaModel);

            // Assert
            Assert.Empty(result.DataList.FirstOrDefault().Detail);
        }

        [Fact]
        public async Task GetLeagueWinlossCurrencyDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var expectedMatchData = new Dictionary<string, object>
            {
                { "Currency", 7 }
            };
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedMatchData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetLeagueWinlossCurrencyDetail(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            Assert.Equal(7, data.Currency);
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetLeagueWinlossCurrencyDetail_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetLeagueWinlossCurrencyDetail(metaModel);

            // Assert
            Assert.Empty(result.DataList);
        }

        [Fact]
        public async Task GetMatchWinlossCurrencyDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var expectedMatchData = new Dictionary<string, object>
            {
                { "Currency", 7 }
            };
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedMatchData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetMatchWinlossCurrencyDetail(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            Assert.Equal(7, data.Currency);
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetMatchWinlossCurrencyDetail_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetMatchWinlossCurrencyDetail(metaModel);

            // Assert
            Assert.Empty(result.DataList);
        }

        [Fact]
        public async Task GetBettypeWinlossCurrencyDetail_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var expectedMatchData = new Dictionary<string, object>
            {
                { "Currency", 7 }
            };
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            SetUpData(reader, _expectedCommonData);
            SetUpData(reader, expectedMatchData);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetBettypeWinlossCurrencyDetail(metaModel);
            var data = result.DataList.FirstOrDefault();

            // Assert
            Assert.Equal(7, data.Currency);
            AssertCommonData(data);
        }

        [Fact]
        public async Task GetBettypeWinlossCurrencyDetail_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            var repository = CreateRepository();

            // Action
            var result = await repository.GetBettypeWinlossCurrencyDetail(metaModel);

            // Assert
            Assert.Empty(result.DataList);
        }

        private static void SetUpData(Mock<IDataReader> reader, Dictionary<string, object> data)
        {
            foreach (var item in data)
            {
                reader.Setup(r => r[item.Key]).Returns(item.Value);
            }
        }

        private static void CreateObjectDb(Mock<IDataReader> mockDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);
        }

        public static void AssertCommonData(LeagueWinLoss data)
        {
            Assert.Equal(100, data.CustomerWL);
            Assert.Equal(50, data.BetCount);
            Assert.Equal(200, data.LicenseeWL);
            Assert.Equal(150, data.TurnOver);
            Assert.Equal(200, data.BAComm);
        }
    }
}