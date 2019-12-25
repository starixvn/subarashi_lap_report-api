using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Extensions;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Resources;
using Fanex.Data;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class DailyBetsServiceTest
    {
        private readonly IFixture _fixture = new Fixture();

        private const string ProductCode = "SB";
        private const byte Status = 1;

        public DailyBetsServiceTest()
        {
            DbSettingProviderManager
             .StartTestingSession()
             .Run();
        }

        [Fact]
        public async Task GetDataAsync_GetMainData_PassParameterCorrectly()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            metaModel.SortBy = "Index";
            metaModel.SortOrder = "asc";
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);

            var expectedTotalData = _fixture.Create<(int totalRows, AuditTicket totalData)>();
            var repository = new Mock<IDailyBetsRepository>();
            repository.Setup(x => x.GetTotalDataAsync(metaModel)).Returns(Task.FromResult(expectedTotalData));

            var objectDb = CreateObjectDb(reader);
            var service = new DailyBetsService(repository.Object);

            // Action
            await service.GetDataAsync(metaModel);

            // Assert
            objectDb.Verify(x => x.SetParameterValue("SiteName", metaModel.SiteName));
            objectDb.Verify(x => x.SetParameterValue("Date", metaModel.Date));
            objectDb.Verify(x => x.SetParameterValue("ProductCodes", metaModel.ProductCodeList));
            objectDb.Verify(x => x.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml()));
            objectDb.Verify(x => x.SetParameterValue("PageIndex", metaModel.PageIndex));
            objectDb.Verify(x => x.SetParameterValue("PageSize", metaModel.PageSize));

            objectDb.Verify(x => x.SetParameterValue("SortBy", metaModel.SortBy));
            objectDb.Verify(x => x.SetParameterValue("SortOrder", metaModel.SortOrder));
        }

        [Fact]
        public async Task GetDataAsync_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);

            var expectedMainData = SetUpMainData(reader);
            CreateObjectDb(reader);

            var expectedTotalData = _fixture.Create<(int totalRows, AuditTicket totalData)>();
            var repository = new Mock<IDailyBetsRepository>();
            repository.Setup(x => x.GetTotalDataAsync(metaModel)).Returns(Task.FromResult(expectedTotalData));

            var service = new DailyBetsService(repository.Object);

            // Action
            var result = await service.GetDataAsync(metaModel);
            var dataItem = result.DataList.First();

            // Assert
            Assert.Equal(expectedTotalData.totalRows, result.TotalRows);
            Assert.Equal(expectedTotalData.totalData, result.TotalData);

            Assert.Equal(expectedMainData.BetType, dataItem.BetType);
            Assert.Equal(expectedMainData.CurrencyName, dataItem.CurrencyName);
            Assert.Equal(expectedMainData.Index, dataItem.Index);
            Assert.Equal(expectedMainData.PlayerName, dataItem.PlayerName);
            Assert.Equal(expectedMainData.ProductName, dataItem.ProductName);
            Assert.Equal(expectedMainData.RefNo, dataItem.RefNo);
            Assert.Equal(expectedMainData.Status, dataItem.Status);
            Assert.Equal(expectedMainData.TransTime, dataItem.TransTime);
            Assert.Equal(expectedMainData.Turnover, dataItem.Turnover);
            Assert.Equal(expectedMainData.WinLoss, dataItem.WinLoss);
        }

        private AuditTicket SetUpMainData(Mock<IDataReader> mockDataReader)
        {
            var mainData = _fixture.Create<AuditTicket>();
            mainData.ProductName = ViewResources.SB;
            mainData.Status = ViewResources.Won;

            var readerDataMapping = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "Index", mainData.Index },
                { "PlayerName", mainData.PlayerName },
                { "CurrencyName", mainData.CurrencyName },
                { "RefNo", mainData.RefNo },
                { "TransTime", mainData.TransTime },
                { "ProductCode", ProductCode },
                { "BetType", mainData.BetType },
                { "Turnover", mainData.Turnover },
                { "WinLoss", mainData.WinLoss },
                { "Status", Status },
            };

            mockDataReader.Setup(r => r[It.IsAny<string>()]).Returns<string>(x => readerDataMapping.ContainsKey(x) ? readerDataMapping[x] : string.Empty);

            return mainData;
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