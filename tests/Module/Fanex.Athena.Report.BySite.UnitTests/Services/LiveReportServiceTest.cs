namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BySite.DTOs;
    using BySite.MetaModels;
    using BySite.Repositories.Interfaces;
    using BySite.Services;
    using Common.Extensions;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using Moq;
    using Resources.Constants;
    using Xunit;

    public class LiveReportServiceTest
    {
        private readonly IDictionary<int, string> _currencyList = new Dictionary<int, string> { { 20, "UUS" } };
        private readonly int[] _liveReportNoDetailLinkSports = { 1, 2, 3 };

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
            var metaModel = new LiveReportMetaModel { FromDate = new DateTime(2017, 2, 1), ToDate = new DateTime(2017, 1, 1) };

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
            var metaModel = new LiveReportMetaModel { FromDate = new DateTime(2017, 2, 1), ToDate = new DateTime(2017, 1, 1) };
            var products = Enumerable.Empty<ProductInfo>();

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportSummaryDetail(metaModel, _currencyList, products, _liveReportNoDetailLinkSports);

            // Assert
            Assert.Empty(result.DataBySport);
            Assert.Empty(result.DataByCurrency);
            Assert.Empty(result.DataByBetType);

            _repository.Verify(x => x.GetDetailBySport(metaModel, _liveReportNoDetailLinkSports), Times.Never);
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

            _repository.Setup(x => x.GetDetailBySport(metaModel, _liveReportNoDetailLinkSports)).ReturnsAsync(detailBySportData);
            _repository.Setup(x => x.GetDetailByCurrencyAndBettype(metaModel, _currencyList, products))
                       .ReturnsAsync(new Tuple<IList<LiveReport>, IList<LiveReportByBetType>>(detailByCurrencyData, detailByBettypeData));

            var service = CreateService();

            // Action
            var result = await service.GetLiveReportSummaryDetail(metaModel, _currencyList, products, _liveReportNoDetailLinkSports);

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

        [Fact]
        public async Task GetLiveReportWinLossByMatch_ValidMetaModel_ReturnCorrectData()
        {
            // Arrange
            var metaModel = new LiveReportMatchListMetalModel { SportId = SportType.AllBet };

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
            Assert.Contains("vs", result[0].MatchName);
        }

        private LiveReportService CreateService() => new(_cacheRefAdapter.Object, _repository.Object);
    }
}