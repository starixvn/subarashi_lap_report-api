namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.DTOs.WinlossAnalysis;
    using BySite.MetaModels;
    using BySite.Repositories.Interfaces;
    using BySite.Services;
    using Common.Model.DTO;
    using Common.Services.Interfaces;
    using Core.Services.Interfaces;
    using Enums;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class WinlossAnalysisServiceTest
    {
        private readonly Mock<IBalanceUpService> _balanceUpService;
        private readonly Mock<ICurrencyService> _currencyService;
        private readonly Fixture _fixture = new();
        private readonly Mock<ILogger<WinlossAnalysisService>> _logger;
        private readonly Mock<INameService> _nameService;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<IWinlossAnalysisRepository> _repository;

        public WinlossAnalysisServiceTest()
        {
            _repository = new Mock<IWinlossAnalysisRepository>();
            _nameService = new Mock<INameService>();
            _balanceUpService = new Mock<IBalanceUpService>();
            _currencyService = new Mock<ICurrencyService>();
            _productService = new Mock<IProductService>();
            _logger = new Mock<ILogger<WinlossAnalysisService>>();
        }

        [Fact]
        public async Task GetRawDataAsync_WhenViewInTimeIsSettlement_CallsSettlementRepository()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.ViewInTime = ViewInTimeMode.Settlement;
            var expectedResult = _fixture.Create<WinlossAnalysisAggregateDTO>();
            var balanceUp = _fixture.Create<BalanceUp>();

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);
            _repository.Setup(x => x.GetDataBySettlementTimeCriteria(metaModel)).ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetRawDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataBySettlementTimeCriteria(metaModel), Times.Once);
            _repository.Verify(x => x.GetDataByAgencyCriteria(It.IsAny<WinlossAnalysisMetaModel>()), Times.Never);
            _repository.Verify(x => x.GetDataByPlayerCriteria(It.IsAny<WinlossAnalysisMetaModel>()), Times.Never);
        }

        [Fact]
        public async Task GetRawDataAsync_WhenNotSettlementAndAgencyCriteria_CallsAgencyRepository()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.ViewInTime = ViewInTimeMode.Financial;
            metaModel.SearchUserName = string.Empty;
            metaModel.IsVipOnly = false;
            metaModel.CustomerClass = "ALL";
            metaModel.OddsGroup = "ALL";
            var expectedResult = _fixture.Create<WinlossAnalysisAggregateDTO>();
            var balanceUp = _fixture.Create<BalanceUp>();

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);
            _repository.Setup(x => x.GetDataByAgencyCriteria(metaModel)).ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetRawDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataByAgencyCriteria(metaModel), Times.Once);
            _repository.Verify(x => x.GetDataByPlayerCriteria(It.IsAny<WinlossAnalysisMetaModel>()), Times.Never);
        }

        [Fact]
        public async Task GetRawDataAsync_WhenNotSettlementAndPlayerCriteria_CallsPlayerRepository()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.ViewInTime = ViewInTimeMode.Financial;
            metaModel.SearchUserName = "testuser";
            var expectedResult = _fixture.Create<WinlossAnalysisAggregateDTO>();
            var balanceUp = _fixture.Create<BalanceUp>();

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);
            _repository.Setup(x => x.GetDataByPlayerCriteria(metaModel)).ReturnsAsync(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetRawDataAsync(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
            _repository.Verify(x => x.GetDataByPlayerCriteria(metaModel), Times.Once);
            _repository.Verify(x => x.GetDataByAgencyCriteria(It.IsAny<WinlossAnalysisMetaModel>()), Times.Never);
        }

        [Fact]
        public async Task GetDataAsync_ReturnsWinlossAnalysisResultModel()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.ViewInTime = ViewInTimeMode.Settlement;
            metaModel.GroupMode = GroupByMode.Product;
            metaModel.BreakdownMode = BreakdownMode.Product;

            var aggregateData = new WinlossAnalysisAggregateDTO
            {
                SumTotal = _fixture.Create<SumTotalWinlossAnalysisDTO>(),
                MainData = _fixture.CreateMany<WinlossAnalysisDTO>(5),
                GroupSumData = _fixture.CreateMany<WinlossAnalysisDTO>(3)
            };
            var balanceUp = _fixture.Create<BalanceUp>();

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);
            _repository.Setup(x => x.GetDataBySettlementTimeCriteria(metaModel)).ReturnsAsync(aggregateData);
            _productService.Setup(x => x.GetProductName(It.IsAny<string>())).Returns("Test Product");

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(aggregateData.TotalRow, result.TotalRow);
            Assert.Equal(aggregateData.SumTotal, result.SumTotal);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetDataAsync_WhenBreakdownModeIsCurrency_OrdersByBreakDownFieldText()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.ViewInTime = ViewInTimeMode.Settlement;
            metaModel.GroupMode = GroupByMode.Currency;
            metaModel.BreakdownMode = BreakdownMode.Currency;

            var mainData = new List<WinlossAnalysisDTO>
            {
                new() { BreakBy = "1", RowIndex = 1 },
                new() { BreakBy = "2", RowIndex = 1 }
            };
            var groupSumData = new List<WinlossAnalysisDTO>
            {
                new() { GroupBy = "1", RowIndex = 1 }
            };

            var aggregateData = new WinlossAnalysisAggregateDTO
            {
                SumTotal = _fixture.Create<SumTotalWinlossAnalysisDTO>(),
                MainData = mainData,
                GroupSumData = groupSumData
            };
            var balanceUp = _fixture.Create<BalanceUp>();
            var currencies = new List<Currency>
            {
                new() { ID = 1, Name = "USD" },
                new() { ID = 2, Name = "EUR" }
            };

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);
            _repository.Setup(x => x.GetDataBySettlementTimeCriteria(metaModel)).ReturnsAsync(aggregateData);
            _currencyService.Setup(x => x.GetCurrencyListAsync()).ReturnsAsync(currencies);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetDataAsync_WhenBreakdownModeIsPlayer_SetsSystemIdAndExternalId()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.ViewInTime = ViewInTimeMode.Settlement;
            metaModel.GroupMode = GroupByMode.Player;
            metaModel.BreakdownMode = BreakdownMode.Player;

            var mainData = new List<WinlossAnalysisDTO>
            {
                new() { BreakBy = "SYS123#EXT456", RowIndex = 1 }
            };
            var groupSumData = new List<WinlossAnalysisDTO>
            {
                new() { GroupBy = "SYS123#EXT456", RowIndex = 1 }
            };

            var aggregateData = new WinlossAnalysisAggregateDTO
            {
                SumTotal = _fixture.Create<SumTotalWinlossAnalysisDTO>(),
                MainData = mainData,
                GroupSumData = groupSumData
            };
            var balanceUp = _fixture.Create<BalanceUp>();

            _balanceUpService.Setup(x => x.GetBalanceUpAsync()).ReturnsAsync(balanceUp);
            _repository.Setup(x => x.GetDataBySettlementTimeCriteria(metaModel)).ReturnsAsync(aggregateData);

            var service = CreateService();

            // Act
            var result = await service.GetDataAsync(metaModel);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            var treeItem = result.Data.First();
            Assert.Equal("SYS123", treeItem.KeyItem.SystemId);
            Assert.Equal("EXT456", treeItem.KeyItem.ExternalId);
        }

        private WinlossAnalysisService CreateService()
        {
            return new WinlossAnalysisService(
                _repository.Object,
                _nameService.Object,
                _balanceUpService.Object,
                _currencyService.Object,
                _productService.Object,
                _logger.Object);
        }
    }
}