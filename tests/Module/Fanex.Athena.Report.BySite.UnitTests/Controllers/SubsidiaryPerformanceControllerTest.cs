namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.DTOs.SubsidiaryPerformance;
    using BySite.Excels.Interfaces;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Model.ViewModel;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class SubsidiaryPerformanceControllerTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<ISubsidiaryService> _subsidiaryService;
        private readonly Mock<ISubsidiaryPerformanceService> _subsidiaryPerformanceService;
        private readonly SubsidiaryPerformanceController _controller;

        public SubsidiaryPerformanceControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _subsidiaryService = new Mock<ISubsidiaryService>();
            _subsidiaryPerformanceService = new Mock<ISubsidiaryPerformanceService>();

            // Setup default context service values
            _contextService.Setup(x => x.LicenseeId).Returns(123);
            _contextService.Setup(x => x.SiteName).Returns("TestSite");
            _contextService.Setup(x => x.DefaultBaseCurrency).Returns("USD");
            _contextService.Setup(x => x.SubsidiaryViewType).Returns(2);
            _contextService.Setup(x => x.IsHideSaba).Returns(false);
            _contextService.Setup(x => x.UserLevel).Returns(0);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string> { { 1, "USD" } });
            _contextService.Setup(x => x.AgentList).Returns(new Dictionary<int, int[]> { { 1, new[] { 1001, 1002 } } });

            _controller = new SubsidiaryPerformanceController(
                _contextService.Object,
                _subsidiaryService.Object,
                _subsidiaryPerformanceService.Object);
        }

        [Fact]
        public async Task Index_WhenCalled_ReturnsCorrectData()
        {
            // Arrange
            var mockBaseCurrencyService = new Mock<IBaseCurrencyService>();
            var mockProductService = new Mock<IProductService>();
            var mockSubsidiaryService = new Mock<ISubsidiaryService>();

            // Create simple test data without circular references
            var expectedBaseCurrencies = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var expectedProductAccess = new ProductAccess(new List<ProductInfo>
            {
                new ProductInfo { ProductID = 1, ProductName = "SB", ProductCode = "SB" }
            });
            var expectedSubsidiaryTree = new List<CheckedItem>
            {
                new CheckedItem { ID = 1, Name = "Test1", Value = "Value1", Checked = true, Children = null },
                new CheckedItem { ID = 2, Name = "Test2", Value = "Value2", Checked = false, Children = null }
            };

            mockBaseCurrencyService.Setup(x => x.GetBaseCurrencyList(It.IsAny<bool>())).Returns(expectedBaseCurrencies);
            mockProductService.Setup(x => x.GetSingleWalletProductAccess(It.IsAny<int>(), It.IsAny<string>()))
                             .Returns(expectedProductAccess);
            mockSubsidiaryService.Setup(x => x.GetSubsidiaryTree(It.IsAny<string>(), It.IsAny<bool>()))
                               .ReturnsAsync(expectedSubsidiaryTree);

            // Act
            var result = await _controller.Index(mockBaseCurrencyService.Object, mockProductService.Object, mockSubsidiaryService.Object) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = result.Value;
            Assert.NotNull(resultValue);

            // Verify service calls were made
            mockBaseCurrencyService.Verify(x => x.GetBaseCurrencyList(It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task Data_WhenCalled_ReturnsCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedResult = _fixture.Create<SubsidiaryPerformanceResultModel>();
            var expectedAgentList = new Dictionary<int, IEnumerable<int>> { { 1, new[] { 1001, 1002 } } };

            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            
            _subsidiaryPerformanceService.Setup(x => x.GetData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Data(metaModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result.Value);
            
            // Verify that context values were assigned
            Assert.Equal(_contextService.Object.SiteName, metaModel.LicenseeName);
            Assert.Equal(_contextService.Object.IsHideSaba, metaModel.IsHideSaba);
            Assert.NotNull(metaModel.AgentListXml);
        }

        [Fact]
        public async Task DetailData_WithEmptyAgentListXml_ReturnsEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var emptyAgentList = new Dictionary<int, IEnumerable<int>>();

            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(emptyAgentList);

            // Act
            var result = await _controller.DetailData(metaModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = result.Value;
            Assert.NotNull(resultValue);
            
            // Should not call the service when agent list is empty
            _subsidiaryPerformanceService.Verify(x => x.GetDetailData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()), Times.Never);
        }

        [Fact]
        public async Task DetailData_WithValidAgentList_ReturnsDetailResult()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedAgentList = new Dictionary<int, IEnumerable<int>> { { 1, new[] { 1001, 1002 } } };
            var expectedResult = _fixture.Create<ReportResultModel<SubsidiaryPerformanceDetail>>();

            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            
            _subsidiaryPerformanceService.Setup(x => x.GetDetailData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.DetailData(metaModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = result.Value;
            Assert.NotNull(resultValue);
            
            _subsidiaryPerformanceService.Verify(x => x.GetDetailData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()), Times.Once);
        }

        [Fact]
        public async Task ExportExcel_WithNoData_ReturnsNoContent()
        {
            // Arrange
            var mockFileProvider = new Mock<IFileProvider>();
            var mockExcel = new Mock<ISubsidiaryPerformanceSummaryExcel>();
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            
            var emptyResult = new SubsidiaryPerformanceResultModel { Data = new List<SubsidiaryPerformanceGroup>() };
            var expectedAgentList = new Dictionary<int, IEnumerable<int>>();

            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            
            _subsidiaryPerformanceService.Setup(x => x.GetData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ExportExcel(mockFileProvider.Object, mockExcel.Object, metaModel);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ExportDetailExcel_WithNoData_ReturnsNoContent()
        {
            // Arrange
            var mockFileProvider = new Mock<IFileProvider>();
            var mockExcel = new Mock<ISubsidiaryPerformanceDetailExcel>();
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            
            var emptyResult = new ReportResultModel<SubsidiaryPerformanceDetail>();
            var expectedAgentList = new Dictionary<int, IEnumerable<int>>();

            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            
            _subsidiaryPerformanceService.Setup(x => x.GetDetailData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.ExportDetailExcel(mockFileProvider.Object, mockExcel.Object, metaModel);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            var mockContextService = new Mock<IContextService>();
            var mockSubsidiaryService = new Mock<ISubsidiaryService>();
            var mockSubsidiaryPerformanceService = new Mock<ISubsidiaryPerformanceService>();

            // Act
            var controller = new SubsidiaryPerformanceController(
                mockContextService.Object,
                mockSubsidiaryService.Object,
                mockSubsidiaryPerformanceService.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public async Task Data_WithSubsidiaryUserLevel_UsesContextAgentList()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedResult = _fixture.Create<SubsidiaryPerformanceResultModel>();
            
            _contextService.Setup(x => x.UserLevel).Returns(2); // UserLevelType.Subsidiary
            _contextService.Setup(x => x.AgentList).Returns(new Dictionary<int, int[]> { { 1, new[] { 1001, 1002 } } });
            
            _subsidiaryPerformanceService.Setup(x => x.GetData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Data(metaModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(metaModel.AgentListXml);
            
            // Should not call subsidiary service for agent list
            _subsidiaryService.Verify(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Data_WithNonSubsidiaryUserLevel_CallsSubsidiaryService()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedResult = _fixture.Create<SubsidiaryPerformanceResultModel>();
            var expectedAgentList = new Dictionary<int, IEnumerable<int>> { { 1, new[] { 1001, 1002 } } };

            _contextService.Setup(x => x.UserLevel).Returns(0); // UserLevelType.OneWorks
            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            _subsidiaryPerformanceService.Setup(x => x.GetData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Data(metaModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(metaModel.AgentListXml);
            
            // Should call subsidiary service for agent list
            _subsidiaryService.Verify(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Data_SetsCorrectMetaModelContextValues()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedResult = _fixture.Create<SubsidiaryPerformanceResultModel>();
            var expectedAgentList = new Dictionary<int, IEnumerable<int>> { { 1, new[] { 1001, 1002 } } };

            _contextService.Setup(x => x.SubsidiaryViewType).Returns(1); // SubsidiaryViewType.NotManagement
            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            _subsidiaryPerformanceService.Setup(x => x.GetData(It.IsAny<SubsidiaryPerformanceMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>()))
                                        .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Data(metaModel) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_contextService.Object.SiteName, metaModel.LicenseeName);
            Assert.Equal(_contextService.Object.IsHideSaba, metaModel.IsHideSaba);
        }

        [Fact]
        public async Task Data_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedResult = _fixture.Create<SubsidiaryPerformanceResultModel>();
            var expectedCurrencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var expectedAgentList = new Dictionary<int, IEnumerable<int>> { { 1, new[] { 1001, 1002 } } };

            _contextService.Setup(x => x.CurrencyList).Returns(expectedCurrencyList);
            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            _subsidiaryPerformanceService.Setup(x => x.GetData(metaModel, expectedCurrencyList))
                                        .ReturnsAsync(expectedResult);

            // Act
            var response = await _controller.Data(metaModel) as OkObjectResult;

            // Assert
            _subsidiaryPerformanceService.Verify(x => x.GetData(metaModel, expectedCurrencyList), Times.Once);
        }

        [Fact]
        public async Task DetailData_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var metaModel = _fixture.Create<SubsidiaryPerformanceMetaModel>();
            var expectedResult = _fixture.Create<ReportResultModel<SubsidiaryPerformanceDetail>>();
            var expectedCurrencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var expectedAgentList = new Dictionary<int, IEnumerable<int>> { { 1, new[] { 1001, 1002 } } };

            _contextService.Setup(x => x.CurrencyList).Returns(expectedCurrencyList);
            _subsidiaryService.Setup(x => x.GetCurrencyAgentList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                             .ReturnsAsync(expectedAgentList);
            _subsidiaryPerformanceService.Setup(x => x.GetDetailData(metaModel, expectedCurrencyList))
                                        .ReturnsAsync(expectedResult);

            // Act
            var response = await _controller.DetailData(metaModel) as OkObjectResult;

            // Assert
            _subsidiaryPerformanceService.Verify(x => x.GetDetailData(metaModel, expectedCurrencyList), Times.Once);
        }
    }
}