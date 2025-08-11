namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    using System;
    using System.Data;
    using AutoFixture;
    using BySite.MetaModels;
    using BySite.Repositories;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class AbnormalBetRepositoryTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;
        private readonly Mock<IIpService> _ipService;
        private readonly Mock<ISubIssueService> _subIssueService;

        public AbnormalBetRepositoryTest()
        {
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _ipService = new Mock<IIpService>();
            _subIssueService = new Mock<ISubIssueService>();
        }

        [Fact]
        public void ReadSummaryData_WithValidData_ReturnsCorrectResult()
        {
            // Arrange
            var dataReader = new Mock<IDataReader>();
            var isHideSaba = false;

            dataReader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            dataReader.Setup(x => x["Live"]).Returns(1);
            dataReader.Setup(x => x["HomeId"]).Returns(1);
            dataReader.Setup(x => x["AwayId"]).Returns(2);
            dataReader.Setup(x => x["IsNeutral"]).Returns(false);
            dataReader.Setup(x => x["League"]).Returns(1);
            dataReader.Setup(x => x["Sport"]).Returns(1);
            dataReader.Setup(x => x["betType"]).Returns(1);
            dataReader.Setup(x => x["finalScore"]).Returns("1-0");
            dataReader.Setup(x => x["Date"]).Returns(DateTime.Now);
            dataReader.Setup(x => x["TotalTransaction"]).Returns(10);
            dataReader.Setup(x => x["choice"]).Returns("h");
            dataReader.Setup(x => x["KOTime"]).Returns(DateTime.Now);

            _cacheRefAdapter.Setup(x => x.GetMatchName(It.IsAny<object>(), It.IsAny<object>(), false, 0))
                           .Returns("Team A vs Team B");
            _cacheRefAdapter.Setup(x => x.GetBettype(It.IsAny<object>(), false))
                           .Returns("Handicap");
            _cacheRefAdapter.Setup(x => x.GetSportName(It.IsAny<object>(), null))
                           .Returns("Football");
            _cacheRefAdapter.Setup(x => x.GetLeagueName(It.IsAny<object>(), 0, null))
                           .Returns("Premier League");

            var repository = CreateRepository();

            // Act
            var result = repository.ReadSummaryData(dataReader.Object, isHideSaba);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.DataList);
            Assert.Equal("Team A vs Team B", result.DataList[0].Match);
        }

        [Fact]
        public void ReadIpSummaryData_WithValidData_ReturnsCorrectResult()
        {
            // Arrange
            var dataReader = new Mock<IDataReader>();
            var isShowIpvn = true;

            dataReader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            dataReader.Setup(x => x["externalID"]).Returns("EXT123");
            dataReader.Setup(x => x["systemID"]).Returns("SYS123");
            dataReader.Setup(x => x["iplogin"]).Returns("192.168.1.1");
            dataReader.Setup(x => x["ticketIPs"]).Returns("192.168.1.1,192.168.1.2");

            _ipService.Setup(x => x.GetIpAddress(It.IsAny<string>(), It.IsAny<bool>()))
                      .Returns("192.168.1.1");
            _ipService.Setup(x => x.GetIpLocation(It.IsAny<string>(), It.IsAny<bool>()))
                      .Returns(new Location { CountryName = "US" });

            var repository = CreateRepository();

            // Act
            var result = repository.ReadIpSummaryData(dataReader.Object, isShowIpvn);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.DataList);
            Assert.Equal("EXT123", result.DataList[0].ExternalId);
            Assert.Equal("SYS123", result.DataList[0].SystemID);
        }

        [Fact]
        public void ReadDetaiData_WithValidData_ReturnsCorrectResult()
        {
            // Arrange
            var dataReader = new Mock<IDataReader>();
            var metaModel = FakeHelper.Create<AbnormalBetMetaModel>(x => x.IsShowIpvn, true);

            dataReader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            dataReader.Setup(x => x["Live"]).Returns(1);
            dataReader.Setup(x => x["HomeId"]).Returns(1);
            dataReader.Setup(x => x["AwayId"]).Returns(2);
            dataReader.Setup(x => x["IsNeutral"]).Returns(false);
            dataReader.Setup(x => x["League"]).Returns(1);
            dataReader.Setup(x => x["Sport"]).Returns(1);
            dataReader.Setup(x => x["betType"]).Returns(1);
            dataReader.Setup(x => x["finalScore"]).Returns("1-0");
            dataReader.Setup(x => x["date"]).Returns(DateTime.Now);
            dataReader.Setup(x => x["externalID"]).Returns("EXT123");
            dataReader.Setup(x => x["systemID"]).Returns("SYS123");
            dataReader.Setup(x => x["currency"]).Returns("USD");
            dataReader.Setup(x => x["refNo"]).Returns("REF123");
            dataReader.Setup(x => x["odds"]).Returns(1.5m);
            dataReader.Setup(x => x["stake"]).Returns(100m);
            dataReader.Setup(x => x["HDP"]).Returns(0.5m);
            dataReader.Setup(x => x["liveHomeScore"]).Returns(1);
            dataReader.Setup(x => x["liveAwayScore"]).Returns(0);
            dataReader.Setup(x => x["choice"]).Returns("h");
            dataReader.Setup(x => x["transTime"]).Returns(DateTime.Now);
            dataReader.Setup(x => x["kOTime"]).Returns(DateTime.Now);
            dataReader.Setup(x => x["ip"]).Returns("192.168.1.1");

            _cacheRefAdapter.Setup(x => x.GetMatchName(It.IsAny<object>(), It.IsAny<object>(), false, 0))
                           .Returns("Team A vs Team B");
            _cacheRefAdapter.Setup(x => x.GetBettype(It.IsAny<object>(), false))
                           .Returns("Handicap");
            _cacheRefAdapter.Setup(x => x.GetSportName(It.IsAny<object>(), null))
                           .Returns("Football");
            _cacheRefAdapter.Setup(x => x.GetLeagueName(It.IsAny<object>(), 0, null))
                           .Returns("Premier League");
            _ipService.Setup(x => x.GetIpAddress(It.IsAny<string>(), It.IsAny<bool>()))
                      .Returns("192.168.1.1");
            _ipService.Setup(x => x.GetIpLocation(It.IsAny<string>(), It.IsAny<bool>()))
                      .Returns(new Location { CountryName = "US" });

            var repository = CreateRepository();

            // Act
            var result = repository.ReadDetaiData(dataReader.Object, metaModel);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.DataList);
            Assert.Equal("EXT123", result.DataList[0].ExternalId);
            Assert.Equal("USD", result.DataList[0].Currency);
            Assert.Equal(1.5m, result.DataList[0].Odds);
        }

        [Fact]
        public void ReadSummaryData_WithParlayBetType_ReturnsEmptyKOTimeAndChoice()
        {
            // Arrange
            var dataReader = new Mock<IDataReader>();
            var isHideSaba = false;

            dataReader.SetupSequence(x => x.Read()).Returns(true).Returns(false);
            dataReader.Setup(x => x["Live"]).Returns(1);
            dataReader.Setup(x => x["HomeId"]).Returns(1);
            dataReader.Setup(x => x["AwayId"]).Returns(2);
            dataReader.Setup(x => x["IsNeutral"]).Returns(false);
            dataReader.Setup(x => x["League"]).Returns(1);
            dataReader.Setup(x => x["Sport"]).Returns(1);
            dataReader.Setup(x => x["betType"]).Returns(29); // Parlay bet type
            dataReader.Setup(x => x["finalScore"]).Returns("1-0");
            dataReader.Setup(x => x["Date"]).Returns(DateTime.Now);
            dataReader.Setup(x => x["TotalTransaction"]).Returns(10);
            dataReader.Setup(x => x["choice"]).Returns("h");
            dataReader.Setup(x => x["KOTime"]).Returns(DateTime.Now);

            _cacheRefAdapter.Setup(x => x.GetMatchName(It.IsAny<object>(), It.IsAny<object>(), false, 0))
                           .Returns("Team A vs Team B");
            _cacheRefAdapter.Setup(x => x.GetBettype(It.IsAny<object>(), false))
                           .Returns("Parlay");
            _cacheRefAdapter.Setup(x => x.GetSportName(It.IsAny<object>(), null))
                           .Returns("Football");
            _cacheRefAdapter.Setup(x => x.GetLeagueName(It.IsAny<object>(), 0, null))
                           .Returns("Premier League");

            var repository = CreateRepository();

            // Act
            var result = repository.ReadSummaryData(dataReader.Object, isHideSaba);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.DataList);
            Assert.Empty(result.DataList[0].KOTime);
            Assert.Empty(result.DataList[0].Choice);
            Assert.Empty(result.DataList[0].FinalScore);
        }

        private AbnormalBetRepository CreateRepository()
        {
            return new AbnormalBetRepository(_cacheRefAdapter.Object, _ipService.Object, _subIssueService.Object);
        }
    }
}