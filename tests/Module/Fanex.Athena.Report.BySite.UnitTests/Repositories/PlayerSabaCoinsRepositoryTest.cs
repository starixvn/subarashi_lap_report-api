using AutoFixture;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class PlayerSabaCoinsRepositoryTest
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly PlayerSabaCoinsRepository _repository;

        private readonly BalanceUp _balanceUp;

        public PlayerSabaCoinsRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();

            _balanceUp = new BalanceUp
            {
                MoveBetsDay = DateTimeHelper.CreateDateTime(2017, 2, 2)
            };
            _balanceUpService = new Mock<IBalanceUpService>();
            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(_balanceUp);

            _repository = new PlayerSabaCoinsRepository(_balanceUpService.Object);
        }

        [Fact]
        public async Task GetMainData_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            SetUpMainData(reader);
            CreateObjectDb(reader);

            // Action
            var result = (await _repository.GetMainData(metaModel)).FirstOrDefault();

            // Assert
            Assert.Equal(1, result.CustId);
            Assert.Equal(2, result.SabaId);
            Assert.Equal("test", result.Username);
            Assert.Equal("12Btest", result.ExternalId);
            Assert.Equal(100m, result.OpeningBalance);
            Assert.Equal(200m, result.Turnover);
            Assert.Equal(300m, result.Outstanding);
            Assert.Equal(400m, result.WinLoss);
            Assert.Equal(500m, result.ClosingBalance);
        }

        [Fact]
        public async Task GetMainData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            CreateObjectDb(reader);

            // Action
            var result = await _repository.GetMainData(metaModel);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMainData_WhenCall_SetParameterCorrectly()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            SetUpMainData(reader);
            var objectDb = CreateObjectDb(reader);

            // Action
            await _repository.GetMainData(metaModel);

            // Assert
            objectDb.Verify(db => db.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("fromdate", metaModel.FromDate.Date), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("todate", metaModel.ToDate.Date), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("Username", metaModel.Username), Times.Once);
        }

        [Fact]
        public async Task GetExtraInfoData_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.SetupSequence(r => r.Read()).Returns(true).Returns(false);
            SetUpExtraInfoData(reader);
            CreateObjectDb(reader);

            // Action
            var result = (await _repository.GetExtraInfoData(metaModel)).FirstOrDefault();

            // Assert
            Assert.Equal(1, result.CustId);
            Assert.Equal(2, result.SabaId);
            Assert.Equal(100m, result.RewardPromotionAmt);
            Assert.Equal(200m, result.ExchangeAmt);
        }

        [Fact]
        public async Task GetExtraInfoData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            CreateObjectDb(reader);

            // Action
            var result = await _repository.GetExtraInfoData(metaModel);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExtraInfoData_WhenCall_SetParameterCorrectly()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            SetUpExtraInfoData(reader);

            var objectDb = CreateObjectDb(reader);

            // Action
            await _repository.GetExtraInfoData(metaModel);

            // Assert
            objectDb.Verify(db => db.SetParameterValue("AgentList", metaModel.AgentListXml), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("fromdate", metaModel.FromDate.Date), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("todate", metaModel.ToDate.Date), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("Username", metaModel.Username), Times.Once);
            objectDb.Verify(x => x.SetParameterValue("movebetsday", _balanceUp.MoveBetsDay), Times.Once);
        }

        private static void SetUpMainData(Mock<IDataReader> reader)
        {
            var expectedData = new Dictionary<string, object>
            {
                { "custId", 1 },
                { "sabaId", 2 },
                { "username", "test" },
                { "nickname", "12Btest" },
                { "OpeningBalance", 100m },
                { "Turnover", 200m },
                { "Outstanding", 300m },
                { "WinLoss", 400m },
                { "ClosingBalance", 500m }
            };
            foreach (var item in expectedData)
            {
                reader.Setup(r => r[item.Key]).Returns(item.Value);
            }
        }

        private static void SetUpExtraInfoData(Mock<IDataReader> reader)
        {
            var expectedData = new Dictionary<string, object>
            {
                { "custId", 1 },
                { "sabaId", 2 },
                { "RewardPromotionAmt", 100m },
                { "ExchangeAmt", 200m }
            };
            foreach (var item in expectedData)
            {
                reader.Setup(r => r[item.Key]).Returns(item.Value);
            }
        }

        private static Mock<IObjectDb> CreateObjectDb(Mock<IDataReader> mockDataReader)
        {
            var mockObjectDB = new Mock<IObjectDb>();
            mockObjectDB.Setup(x => x.ExecuteReaderAsync(It.IsAny<object>(), It.IsAny<DbTransaction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockDataReader.Object));

            ObjectDbFactory.SwitchToTestingMode();
            ObjectDbFactory.Mock((x, y) => mockObjectDB.Object);

            return mockObjectDB;
        }
    }
}