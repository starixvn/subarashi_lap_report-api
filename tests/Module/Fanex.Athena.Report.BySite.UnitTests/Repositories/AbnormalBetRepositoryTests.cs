namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    using System;
    using System.Data;
    using BySite.MetaModels;
    using BySite.Repositories;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Moq;
    using Xunit;

    public class AbnormalBetRepositoryTests
    {
        private readonly Mock<ICacheRefAdapter> _mockCacheRefAdapter;
        private readonly Mock<IIpService> _mockIpService;
        private readonly Mock<ISubIssueService> _mockSubIssueService;
        private readonly AbnormalBetRepository _abnormalBetRepository;

        public AbnormalBetRepositoryTests()
        {
            _mockCacheRefAdapter = new Mock<ICacheRefAdapter>();
            _mockIpService = new Mock<IIpService>();
            _mockSubIssueService = new Mock<ISubIssueService>();
            _abnormalBetRepository = new AbnormalBetRepository(_mockCacheRefAdapter.Object, _mockIpService.Object, _mockSubIssueService.Object);
        }

        [Fact]
        public void ReadSummaryData_ShouldReturnCorrectData()
        {
            // Arrange
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            reader.Setup(r => r["Live"]).Returns(1);
            reader.Setup(r => r["HomeId"]).Returns(1);
            reader.Setup(r => r["AwayId"]).Returns(2);
            reader.Setup(r => r["IsNeutral"]).Returns(false);
            reader.Setup(r => r["League"]).Returns(1);
            reader.Setup(r => r["Sport"]).Returns(1);
            reader.Setup(r => r["betType"]).Returns(1);
            reader.Setup(r => r["finalScore"]).Returns("1-0");
            reader.Setup(r => r["Date"]).Returns(new DateTime(2023, 1, 1));
            reader.Setup(r => r["TotalTransaction"]).Returns(10);
            reader.Setup(r => r["choice"]).Returns("h");
            reader.Setup(r => r["KOTime"]).Returns(new DateTime(2023, 1, 1));

            _mockCacheRefAdapter.Setup(c => c.GetMatchName(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<int>())).Returns("Match Name");
            _mockCacheRefAdapter.Setup(c => c.GetLeagueName(It.IsAny<object>(), It.IsAny<int>(), It.IsAny<string>())).Returns("League Name");
            _mockCacheRefAdapter.Setup(c => c.GetSportName(It.IsAny<object>(), It.IsAny<string>())).Returns("Sport Name");
            _mockCacheRefAdapter.Setup(c => c.GetBettype(It.IsAny<object>(), It.IsAny<bool>())).Returns("Bet Type");

            // Act
            var result = _abnormalBetRepository.ReadSummaryData(reader.Object, false);

            // Assert
            Assert.Single(result.DataList);
        }

        [Fact]
        public void ReadIpSummaryData_ShouldReturnCorrectData()
        {
            // Arrange
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            reader.Setup(r => r["iplogin"]).Returns("127.0.0.1");
            reader.Setup(r => r["ticketIPs"]).Returns("127.0.0.1,192.168.1.1");
            reader.Setup(r => r["externalID"]).Returns("ext1");
            reader.Setup(r => r["systemID"]).Returns("sys1");

            _mockIpService.Setup(i => i.GetIpAddress(It.IsAny<string>(), It.IsAny<bool>())).Returns((string ip, bool _) => ip);
            _mockIpService.Setup(i => i.GetIpLocation(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Location { CountryName = "Local" });

            // Act
            var result = _abnormalBetRepository.ReadIpSummaryData(reader.Object, true);

            // Assert
            Assert.Single(result.DataList);
            Assert.Equal("127.0.0.1, 192.168.1.1", result.DataList[0].TicketIP);
        }

        [Fact]
        public void ReadDetaiData_ShouldReturnCorrectData()
        {
            // Arrange
            var metaModel = new AbnormalBetMetaModel();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            reader.Setup(r => r["Live"]).Returns(1);
            reader.Setup(r => r["HomeId"]).Returns(1);
            reader.Setup(r => r["AwayId"]).Returns(2);
            reader.Setup(r => r["IsNeutral"]).Returns(false);
            reader.Setup(r => r["League"]).Returns(1);
            reader.Setup(r => r["Sport"]).Returns(1);
            reader.Setup(r => r["betType"]).Returns(1);
            reader.Setup(r => r["finalScore"]).Returns("1-0");
            reader.Setup(r => r["date"]).Returns(new DateTime(2023, 1, 1));
            reader.Setup(r => r["externalID"]).Returns("ext1");
            reader.Setup(r => r["systemID"]).Returns("sys1");
            reader.Setup(r => r["currency"]).Returns("USD");
            reader.Setup(r => r["refNo"]).Returns("ref1");
            reader.Setup(r => r["odds"]).Returns(1.5m);
            reader.Setup(r => r["stake"]).Returns(100m);
            reader.Setup(r => r["HDP"]).Returns(0.5m);
            reader.Setup(r => r["liveHomeScore"]).Returns(0);
            reader.Setup(r => r["liveAwayScore"]).Returns(0);
            reader.Setup(r => r["choice"]).Returns("h");
            reader.Setup(r => r["transTime"]).Returns(new DateTime(2023, 1, 1));
            reader.Setup(r => r["kOTime"]).Returns(new DateTime(2023, 1, 1));
            reader.Setup(r => r["ip"]).Returns("127.0.0.1");

            _mockCacheRefAdapter.Setup(c => c.GetMatchName(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<int>())).Returns("Match Name");
            _mockIpService.Setup(i => i.GetIpAddress(It.IsAny<string>(), It.IsAny<bool>())).Returns("127.0.0.1");
            _mockIpService.Setup(i => i.GetIpLocation(It.IsAny<string>(), It.IsAny<bool>())).Returns(new Location { CountryName = "Local" });

            // Act
            var result = _abnormalBetRepository.ReadDetaiData(reader.Object, metaModel);

            // Assert
            Assert.Single(result.DataList);
        }
    }
}
