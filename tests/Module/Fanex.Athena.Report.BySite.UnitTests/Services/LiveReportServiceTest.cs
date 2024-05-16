using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Routing;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    public class LiveReportServiceTest
    {
        private readonly IDictionary<int, string> _currencyList = new Dictionary<int, string> { { 20, "UUS" } };
        private Mock<ILiveReportRepository> _repository;
        private Mock<ICacheRefAdapter> _cacheRefAdapter;

        public LiveReportServiceTest()
        {
            _repository = new Mock<ILiveReportRepository>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
        }

        [Fact]
        public async Task GetLiveReportSummary_InvalidMetaModel_ReturnEmptyData()
        {
            // Arrange
            var metaModel = new LiveReportMetaModel { FromDate = new System.DateTime(2017, 2, 1), ToDate = new System.DateTime(2017, 1, 1) };

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportSummary(metaModel);

            // Assert
            Assert.Empty(result);
            _repository.Verify(x => x.GetLiveReportSummary(metaModel), Times.Never);
        }

        [Fact]
        public void GetLiveReportSummary_ValidMetaModel_ReturnDataFromDataAccess()
        {
            // Arrange
            var metaModel = new LiveReportMetaModel();

            var service = CreateService();

            // Action
            var result = service.GetLiveReportSummary(metaModel);

            // Assert
            _repository.Verify(x => x.GetLiveReportSummary(metaModel), Times.Once);
        }

        [Fact]
        public async Task GetLiveReportSumaryDetail_InvalidMetaModel_ReturnEmptyData()
        {
            // Arrange
            var metaModel = new LiveReportMetaModel { FromDate = new System.DateTime(2017, 2, 1), ToDate = new System.DateTime(2017, 1, 1) };
            var products = Enumerable.Empty<ProductInfo>();

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportSummaryDetail(metaModel, _currencyList, products);

            // Assert
            Assert.Empty(result.DataBySport);
            Assert.Empty(result.DataByCurrency);
            Assert.Empty(result.DataByBetType);
            
            _repository.Verify(x => x.GetDetailBySport(metaModel), Times.Never);
            _repository.Verify(x => x.GetDetailByCurrencyAndBettype(metaModel, _currencyList, products), Times.Never);
        }

        [Fact]
        public async Task GetLiveReportSumaryDetail_ValidMetaModel_ReturnCorrectData()
        {
            // Arrange
            var metaModel = new LiveReportMetaModel();
            var products = Enumerable.Empty<ProductInfo>();

            var detailBySportData = new List<LiveReportBySport>();
            var detailByCurrencyData = new List<LiveReport>();
            var detailByBettypeData = new List<LiveReportByBetType>();

            _repository.Setup(x => x.GetDetailBySport(metaModel)).ReturnsAsync(detailBySportData);
            _repository.Setup(x => x.GetDetailByCurrencyAndBettype(metaModel, _currencyList, products))
                       .ReturnsAsync(new Tuple<IList<LiveReport>, IList<LiveReportByBetType>>(detailByCurrencyData, detailByBettypeData));

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportSummaryDetail(metaModel, _currencyList, products);

            // Assert
            Assert.Equal(detailBySportData, result.DataBySport);
            Assert.Equal(detailByCurrencyData, result.DataByCurrency);
            Assert.Equal(detailByBettypeData, result.DataByBetType);
        }

        [Fact]
        public async Task GetLiveReportWinLossByMatch_InvalidMetaModel_ReturnEmptyData()
        {
            // Arrange
            var metaModel = new LiveReportMatchListMetalModel { FromDate = new DateTime(2017, 2, 1), ToDate = new DateTime(2017, 1, 1) };

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportWinLossByMatch(metaModel);

            // Assert
            Assert.Empty(result);

            _repository.Verify(x => x.GetLiveReportWinlossByMatch(metaModel), Times.Never);
        }

        [Theory]
        [InlineData(SportType.LiveCasino, "ShowTime")]
        [InlineData(SportType.AllBet, "vs")]
        public async Task GetLiveReportWinLossByMatch_ValidMetaModel_ReturnCorrectData(int sportType, string expectedMatchName)
        {
            // Arrange
            var metaModel = new LiveReportMatchListMetalModel { SportId = sportType };

            var dataList = new SortableList<LiveReportWinLossByMatch>
            {
                new() { ShowTime = "ShowTime" }
            };

            _repository.Setup(x => x.GetLiveReportWinlossByMatch(metaModel)).ReturnsAsync(dataList);

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportWinLossByMatch(metaModel);

            // Assert
            Assert.Equal(dataList.Count, result.Count);
            Assert.Contains(expectedMatchName, result[0].MatchName);
        }

        private LiveReportService CreateService() => new(_cacheRefAdapter.Object, _repository.Object);
    }
}
