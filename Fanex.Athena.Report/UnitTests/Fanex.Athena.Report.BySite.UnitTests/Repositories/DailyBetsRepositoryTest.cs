using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.Extensions;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Repositories
{
    public class DailyBetsRepositoryTest
    {
        private readonly IFixture _fixture = new Fixture();

        public DailyBetsRepositoryTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();
        }

        [Fact]
        public async Task GetTotalDataAsync_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            SetUpData(reader);

            CreateObjectDb(reader);

            var repository = new DailyBetsRepository();

            // Action
            var result = await repository.GetTotalDataAsync(metaModel);

            // Assert
            Assert.Equal(1, result.totalRows);
            Assert.Equal(100m, result.totalData.Turnover);
            Assert.Equal(200m, result.totalData.WinLoss);
        }

        [Fact]
        public async Task GetTotalDataAsync_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            SetUpData(reader);

            CreateObjectDb(reader);

            var repository = new DailyBetsRepository();

            // Action
            var result = await repository.GetTotalDataAsync(metaModel);

            // Assert
            Assert.Equal(0, result.totalRows);
        }

        [Fact]
        public async Task GetTotalDataAsync_WhenCall_SetParameterCorrectly()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            SetUpData(reader);

            var objectDb = CreateObjectDb(reader);

            var repository = new DailyBetsRepository();

            // Action
            await repository.GetTotalDataAsync(metaModel);

            // Assert
            objectDb.Verify(db => db.SetParameterValue("SiteName", metaModel.SiteName), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("Date", metaModel.Date), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("ProductCodes", metaModel.ProductCodeList), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml()), Times.Once);
            objectDb.Verify(db => db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency), Times.Once);
        }

        private static void SetUpData(Mock<IDataReader> reader)
        {
            var expectedData = new Dictionary<string, object>
            {
                { "TotalRows", 1 },
                { "TotalTurnover", 100m },
                { "TotalWinLoss", 200m }
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