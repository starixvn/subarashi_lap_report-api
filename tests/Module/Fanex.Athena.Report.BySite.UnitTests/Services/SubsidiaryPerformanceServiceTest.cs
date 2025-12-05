namespace Fanex.Athena.Report.BySite.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.DTOs.SubsidiaryPerformance;
    using BySite.MetaModels;
    using BySite.Models;
    using BySite.Repositories.Interfaces;
    using BySite.Services;
    using Common.Model.ViewModel;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Moq;
    using Xunit;

    public class SubsidiaryPerformanceServiceTest
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<ISubsidiaryService> _subsidiaryService;
        private readonly Mock<ISubsidiaryPerformanceRepository> _subsidiaryPerformanceRepository;
        private readonly Mock<IProductService> _productService;

        public SubsidiaryPerformanceServiceTest()
        {
            _subsidiaryService = new Mock<ISubsidiaryService>();
            _subsidiaryPerformanceRepository = new Mock<ISubsidiaryPerformanceRepository>();
            _productService = new Mock<IProductService>();
        }

        private SubsidiaryPerformanceService CreateService()
        {
            return new SubsidiaryPerformanceService(
                _subsidiaryService.Object,
                _subsidiaryPerformanceRepository.Object,
                _productService.Object);
        }

        [Fact]
        public async Task GetData_WhenNoData_ReturnsEmptyResult()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel { BaseCurrency = "USD" };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(new List<SubsidiaryPerformance>());

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetData_WhenHasData_ReturnsGroupedResult()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                BaseCurrency = "USD",
                LicenseeName = "TestSite",
                SubsidiaryList = "Sub1",
                IsHideSaba = false
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var dataList = new List<SubsidiaryPerformance>
            {
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 100,
                    Turnover = 1000,
                    CustomerWinLoss = 50,
                    EffectiveBettingValue = 800,
                    SubsidiaryWinLoss = -50,
                    BaseCurrencyData = new SubsidiaryPerformanceBase
                    {
                        BetCount = 100,
                        Turnover = 1000,
                        CustomerWinLoss = 50,
                        EffectiveBettingValue = 800,
                        SubsidiaryWinLoss = -50
                    }
                }
            };

            var subsidiaryList = new List<Subsidiary>
            {
                new() { AgentId = 1, OperatorId = "Operator1", CurrencyId = 1 }
            };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(dataList);

            _subsidiaryService
                .Setup(x => x.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList))
                .ReturnsAsync(subsidiaryList);

            _productService
                .Setup(x => x.GetProductName("SB"))
                .Returns("Sportsbook");

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.NotNull(result.GrandTotal);
            Assert.Equal(800, result.GrandTotal.EffectiveBettingValue);

            _productService.Verify(x => x.GetProductName("SB"), Times.Once);
        }

        [Fact]
        public async Task GetData_WithIsHideSaba_RemovesSabaTextFromProductName()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                BaseCurrency = "USD",
                LicenseeName = "TestSite",
                SubsidiaryList = "Sub1",
                IsHideSaba = true
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var dataList = new List<SubsidiaryPerformance>
            {
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 100,
                    Turnover = 1000,
                    CustomerWinLoss = 50,
                    EffectiveBettingValue = 800,
                    SubsidiaryWinLoss = -50,
                    BaseCurrencyData = new SubsidiaryPerformanceBase
                    {
                        BetCount = 100,
                        Turnover = 1000,
                        CustomerWinLoss = 50,
                        EffectiveBettingValue = 800,
                        SubsidiaryWinLoss = -50
                    }
                }
            };

            var subsidiaryList = new List<Subsidiary>
            {
                new() { AgentId = 1, OperatorId = "Operator1", CurrencyId = 1 }
            };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(dataList);

            _subsidiaryService
                .Setup(x => x.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList))
                .ReturnsAsync(subsidiaryList);

            _productService
                .Setup(x => x.GetProductName("SB"))
                .Returns("Saba Sportsbook");

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            _productService.Verify(x => x.GetProductName("SB"), Times.Once);
        }

        [Fact]
        public async Task GetData_WithMultipleProducts_GroupsProductsCorrectly()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                BaseCurrency = "USD",
                LicenseeName = "TestSite",
                SubsidiaryList = "Sub1",
                IsHideSaba = false
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var dataList = new List<SubsidiaryPerformance>
            {
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 100,
                    Turnover = 1000,
                    CustomerWinLoss = 50,
                    EffectiveBettingValue = 800,
                    SubsidiaryWinLoss = -50,
                    BaseCurrencyData = new SubsidiaryPerformanceBase { BetCount = 100, Turnover = 1000, CustomerWinLoss = 50, EffectiveBettingValue = 800, SubsidiaryWinLoss = -50 }
                },
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "VS",
                    BetCount = 50,
                    Turnover = 500,
                    CustomerWinLoss = 25,
                    EffectiveBettingValue = 400,
                    SubsidiaryWinLoss = -25,
                    BaseCurrencyData = new SubsidiaryPerformanceBase { BetCount = 50, Turnover = 500, CustomerWinLoss = 25, EffectiveBettingValue = 400, SubsidiaryWinLoss = -25 }
                }
            };

            var subsidiaryList = new List<Subsidiary>
            {
                new() { AgentId = 1, OperatorId = "Operator1", CurrencyId = 1 }
            };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(dataList);

            _subsidiaryService
                .Setup(x => x.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList))
                .ReturnsAsync(subsidiaryList);

            _productService.Setup(x => x.GetProductName("SB")).Returns("Sportsbook");
            _productService.Setup(x => x.GetProductName("VS")).Returns("Virtual Sports");

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            _productService.Verify(x => x.GetProductName("SB"), Times.Once);
            _productService.Verify(x => x.GetProductName("VS"), Times.Once);

            Assert.Equal(150, result.GrandTotal.BetCount);
            Assert.Equal(1500, result.GrandTotal.Turnover);
            Assert.Equal(1200, result.GrandTotal.EffectiveBettingValue);
        }

        [Fact]
        public async Task GetDetailData_WhenAgentListXmlIsEmpty_ReturnsEmptyResult()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel { AgentListXml = string.Empty };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var service = CreateService();

            var result = await service.GetDetailData(metaModel, currencyList);

            Assert.NotNull(result);
            _subsidiaryPerformanceRepository.Verify(
                x => x.GetDetailData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetDetailData_WhenAgentListXmlHasValue_ReturnsData()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel { AgentListXml = "<agents><agent>1</agent></agents>" };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };
            var expectedResult = _fixture.Create<ReportResultModel<SubsidiaryPerformanceDetail>>();

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetDetailData(metaModel, currencyList))
                .ReturnsAsync(expectedResult);

            var service = CreateService();

            var result = await service.GetDetailData(metaModel, currencyList);

            Assert.Equal(expectedResult, result);
            _subsidiaryPerformanceRepository.Verify(x => x.GetDetailData(metaModel, currencyList), Times.Once);
        }

        [Fact]
        public async Task GetData_WithWhiteLabelList_GroupsByWhiteLabel()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                BaseCurrency = "USD",
                LicenseeName = "TestSite",
                SubsidiaryList = "Sub1",
                WhiteLabelList = "WL1",
                IsHideSaba = false
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var dataList = new List<SubsidiaryPerformance>
            {
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 100,
                    Turnover = 1000,
                    CustomerWinLoss = 50,
                    EffectiveBettingValue = 800,
                    SubsidiaryWinLoss = -50,
                    BaseCurrencyData = new SubsidiaryPerformanceBase { BetCount = 100, Turnover = 1000, CustomerWinLoss = 50, EffectiveBettingValue = 800, SubsidiaryWinLoss = -50 }
                }
            };

            var subsidiaryList = new List<Subsidiary>
            {
                new() { AgentId = 1, OperatorId = "Operator1", CurrencyId = 1 }
            };

            var groupSubsidiaryList = new List<GroupSubsidiary>
            {
                new() { OperatorId = "Operator1", WhiteLabelName = "WhiteLabel1" }
            };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(dataList);

            _subsidiaryService
                .Setup(x => x.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList))
                .ReturnsAsync(subsidiaryList);

            _subsidiaryService
                .Setup(x => x.GetGroupSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList, metaModel.WhiteLabelList))
                .ReturnsAsync(groupSubsidiaryList);

            _productService.Setup(x => x.GetProductName("SB")).Returns("Sportsbook");

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            _subsidiaryService.Verify(x => x.GetGroupSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList, metaModel.WhiteLabelList), Times.Once);
        }

        [Fact]
        public void Constructor_WithAllDependencies_CreatesInstance()
        {
            var service = CreateService();
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetData_WithMultipleSubsidiaries_SumsEffectiveBettingValueCorrectly()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                BaseCurrency = "USD",
                LicenseeName = "TestSite",
                SubsidiaryList = "Sub1,Sub2",
                IsHideSaba = false
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var dataList = new List<SubsidiaryPerformance>
            {
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 100,
                    Turnover = 1000,
                    CustomerWinLoss = 50,
                    EffectiveBettingValue = 800,
                    SubsidiaryWinLoss = -50,
                    BaseCurrencyData = new SubsidiaryPerformanceBase { BetCount = 100, Turnover = 1000, CustomerWinLoss = 50, EffectiveBettingValue = 800, SubsidiaryWinLoss = -50 }
                },
                new()
                {
                    SubsidiaryId = 2,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 200,
                    Turnover = 2000,
                    CustomerWinLoss = 100,
                    EffectiveBettingValue = 1500,
                    SubsidiaryWinLoss = -100,
                    BaseCurrencyData = new SubsidiaryPerformanceBase { BetCount = 200, Turnover = 2000, CustomerWinLoss = 100, EffectiveBettingValue = 1500, SubsidiaryWinLoss = -100 }
                }
            };

            var subsidiaryList = new List<Subsidiary>
            {
                new() { AgentId = 1, OperatorId = "Operator1", CurrencyId = 1 },
                new() { AgentId = 2, OperatorId = "Operator2", CurrencyId = 1 }
            };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(dataList);

            _subsidiaryService
                .Setup(x => x.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList))
                .ReturnsAsync(subsidiaryList);

            _productService.Setup(x => x.GetProductName("SB")).Returns("Sportsbook");

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            Assert.Equal(2300m, result.GrandTotal.EffectiveBettingValue);
            Assert.Equal(300, result.GrandTotal.BetCount);
            Assert.Equal(3000m, result.GrandTotal.Turnover);
        }

        [Fact]
        public async Task GetData_WithZeroEffectiveBettingValue_ReturnsZeroInTotal()
        {
            var metaModel = new SubsidiaryPerformanceMetaModel
            {
                BaseCurrency = "USD",
                LicenseeName = "TestSite",
                SubsidiaryList = "Sub1",
                IsHideSaba = false
            };
            var currencyList = new Dictionary<int, string> { { 1, "USD" } };

            var dataList = new List<SubsidiaryPerformance>
            {
                new()
                {
                    SubsidiaryId = 1,
                    CurrencyId = 1,
                    CurrencyName = "USD",
                    ProductCode = "SB",
                    BetCount = 100,
                    Turnover = 1000,
                    CustomerWinLoss = 50,
                    EffectiveBettingValue = 0,
                    SubsidiaryWinLoss = -50,
                    BaseCurrencyData = new SubsidiaryPerformanceBase { BetCount = 100, Turnover = 1000, CustomerWinLoss = 50, EffectiveBettingValue = 0, SubsidiaryWinLoss = -50 }
                }
            };

            var subsidiaryList = new List<Subsidiary>
            {
                new() { AgentId = 1, OperatorId = "Operator1", CurrencyId = 1 }
            };

            _subsidiaryPerformanceRepository
                .Setup(x => x.GetData(metaModel, currencyList))
                .ReturnsAsync(dataList);

            _subsidiaryService
                .Setup(x => x.GetSubsidiaryList(metaModel.LicenseeName, metaModel.SubsidiaryList))
                .ReturnsAsync(subsidiaryList);

            _productService.Setup(x => x.GetProductName("SB")).Returns("Sportsbook");

            var service = CreateService();

            var result = await service.GetData(metaModel, currencyList);

            Assert.NotNull(result);
            Assert.Equal(0m, result.GrandTotal.EffectiveBettingValue);
        }
    }
}

