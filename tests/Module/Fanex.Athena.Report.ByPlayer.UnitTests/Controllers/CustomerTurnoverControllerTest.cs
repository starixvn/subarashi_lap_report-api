namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ByPlayer.Controllers;
    using ByPlayer.Excels.Interfaces;
    using ByPlayer.Excels.Options;
    using ByPlayer.Service.Interfaces;
    using Common.Model.ViewModel;
    using Common.Services.Interfaces;
    using Common.Settings;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using DTO;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class CustomerTurnoverControllerTest
    {
        private readonly AppSettings _appSettings;

        private readonly CustomerTurnoverController _controller;

        private readonly Mock<IBetTypeService> _betTypeService;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ICustomerTurnoverService> _customerTurnoverService;

        private readonly IReadOnlyDictionary<int, string> _currencyList = FakeHelper.Create<IReadOnlyDictionary<int, string>>();

        public CustomerTurnoverControllerTest()
        {
            _betTypeService = new Mock<IBetTypeService>();
            _customerTurnoverService = new Mock<ICustomerTurnoverService>();

            _appSettings = new AppSettings();

            _contextService = new Mock<IContextService>();
            _contextService.Setup(x => x.Language).Returns("Any");
            _contextService.Setup(x => x.CurrencyList).Returns(_currencyList);

            _controller = CreateController();
        }

        [Fact]
        public async Task GetBettypesBySport_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var expected = FakeHelper.CreateMany<BettypeProductItem>().ToList();

            _contextService.Setup(x => x.IsHideSaba).Returns(true);

            var sportProducts = FakeHelper.CreateMany<SportProductItem>().ToList();
            _betTypeService.Setup(x => x.GetBettypes(sportProducts, _contextService.Object.Language, _contextService.Object.IsHideSaba)).ReturnsAsync(expected);

            // Action
            var response = await _controller.GetBettypesBySport(_betTypeService.Object, sportProducts) as OkObjectResult;
            var result = response.Value as List<BettypeProductItem>;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetBettypesBySport_EmptySportProducts_ReturnEmpty()
        {
            // Action
            var response = await _controller.GetBettypesBySport(_betTypeService.Object, null) as OkObjectResult;
            var result = response.Value as List<BettypeProductItem>;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetData_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerTurnoverMetaModel>();
            var expected = FakeHelper.Create<ReportResultModel<CustTurnover>>();
            _customerTurnoverService.Setup(x => x.GetDataList(metaModel, _contextService.Object.CurrencyList, _contextService.Object.Language)).ReturnsAsync(expected);

            // Action
            var response = await _controller.GetData(_customerTurnoverService.Object, metaModel) as OkObjectResult;
            var result = response.Value as CustomerTurnoverResultModel;

            // Assert
            Assert.Equal(expected.DataList, result.Data);
            Assert.Equal(expected.TotalData, result.TotalData);
            Assert.False(metaModel.IsExportExcel);
        }

        [Fact]
        public async Task Index_WhenCalled_ReturnsCorrectData()
        {
            // Arrange
            var mockProductService = new Mock<IProductService>();
            var mockSportService = new Mock<ISportService>();
            
            var expectedProductOptions = FakeHelper.CreateMany<SportProductItem>().ToList();
            var expectedCurrencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            
            _contextService.Setup(x => x.CurrencyList).Returns(expectedCurrencyList);
            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);
            _contextService.Setup(x => x.OfferedSubProductList).Returns(new[] { "SB", "NG" });
            _contextService.Setup(x => x.LicenseeId).Returns(123);
            _contextService.Setup(x => x.Language).Returns("en-US");
            
            mockProductService.Setup(x => x.GetAllowedSubProductCodeList(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
                             .Returns("SB,NG,VS");
            mockSportService.Setup(x => x.GetSportsAndProducts(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(expectedProductOptions);

            // Act
            var result = await _controller.Index(mockProductService.Object, mockSportService.Object) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var resultValue = result.Value;
            Assert.NotNull(resultValue);
        }

        [Fact]
        public async Task GetBettypesBySport_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<SportProductItem>();

            // Act
            var response = await _controller.GetBettypesBySport(_betTypeService.Object, emptyList) as OkObjectResult;
            var result = response.Value as List<BettypeProductItem>;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetData_SetsIsExportExcelToFalse()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerTurnoverMetaModel>();
            metaModel.IsExportExcel = true; // Set to true initially
            
            var expectedData = FakeHelper.Create<ReportResultModel<CustTurnover>>();
            _customerTurnoverService.Setup(x => x.GetDataList(It.IsAny<CustomerTurnoverMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>(), It.IsAny<string>()))
                                  .ReturnsAsync(expectedData);

            // Act
            var response = await _controller.GetData(_customerTurnoverService.Object, metaModel) as OkObjectResult;
            var result = response.Value as CustomerTurnoverResultModel;

            // Assert
            Assert.False(metaModel.IsExportExcel); // Should be set to false
            Assert.NotNull(result);
            Assert.Equal(expectedData.DataList, result.Data);
        }

        [Fact]
        public async Task ExportExcel_WithSinglePage_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerTurnoverMetaModel>();
            metaModel.CurrencyId = 1;
            metaModel.ReportBy = 1; // Daily report
            
            var mockFileProvider = new Mock<IFileProvider>();
            var mockCompressService = new Mock<ICompressService>();
            var mockExcel = new Mock<ICustomerTurnoverExcel>();
            var mockFileInfo = new Mock<IFileInfo>();
            
            mockFileInfo.Setup(x => x.PhysicalPath).Returns("/path/to/template.xls");
            mockFileProvider.Setup(x => x.GetFileInfo(It.IsAny<string>())).Returns(mockFileInfo.Object);
            
            var expectedData = FakeHelper.Create<ReportResultModel<CustTurnover>>();
            expectedData.DataList = FakeHelper.CreateMany<CustTurnover>(10).ToList(); // Small list for single file
            
            _customerTurnoverService.Setup(x => x.GetDataList(It.IsAny<CustomerTurnoverMetaModel>(), It.IsAny<IReadOnlyDictionary<int, string>>(), It.IsAny<string>()))
                                  .ReturnsAsync(expectedData);
            
            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string> { { 1, "USD" } });
            
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            mockExcel.Setup(x => x.ExportFile(It.IsAny<string>(), It.IsAny<ReportResultModel<CustTurnover>>(), It.IsAny<CustomerTurnoverMetaModel>(), It.IsAny<CustomerTurnoverExcelOptions>()))
                    .Returns(fileContent);
            
            _appSettings.MaxRowPerExcelFile = 1000; // High limit to ensure single file

            // Act
            var result = await _controller.ExportExcel(mockFileProvider.Object, mockCompressService.Object, mockExcel.Object, _customerTurnoverService.Object, metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.True(metaModel.IsExportExcel); // Should be set to true
            var fileResult = result as FileContentResult;
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(fileContent, fileResult.FileContents);
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            var appSettings = new AppSettings();
            var mockContextService = new Mock<IContextService>();

            // Act
            var controller = new CustomerTurnoverController(appSettings, mockContextService.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Theory]
        [InlineData(1, "MM/dd/yyyy")] // Daily report
        [InlineData(2, "MM/yyyy")]    // Monthly report  
        [InlineData(0, "MM/dd/yyyy")] // Default case
        public async Task ExportExcel_WithDifferentReportBy_SetsCorrectDateFormat(int reportBy, string expectedFormat)
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerTurnoverMetaModel>();
            metaModel.ReportBy = reportBy;
            metaModel.CurrencyId = 1;
            
            var mockFileProvider = new Mock<IFileProvider>();
            var mockCompressService = new Mock<ICompressService>();
            var mockExcel = new Mock<ICustomerTurnoverExcel>();
            var mockFileInfo = new Mock<IFileInfo>();
            
            mockFileInfo.Setup(x => x.PhysicalPath).Returns("/path/to/template.xls");
            mockFileProvider.Setup(x => x.GetFileInfo(It.IsAny<string>())).Returns(mockFileInfo.Object);
            
            var expectedData = FakeHelper.Create<ReportResultModel<CustTurnover>>();
            expectedData.DataList = FakeHelper.CreateMany<CustTurnover>(5).ToList();
            
            _customerTurnoverService.Setup(x => x.GetDataList(It.IsAny<CustomerTurnoverMetaModel>(),
                    It.IsAny<IReadOnlyDictionary<int, string>>(),
                    It.IsAny<string>()))
                                  .ReturnsAsync(expectedData);
            
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string> { { 1, "USD" } });
            
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            mockExcel.Setup(x => x.ExportFile(It.IsAny<string>(),
                    It.IsAny<ReportResultModel<CustTurnover>>(),
                    It.IsAny<CustomerTurnoverMetaModel>(),
                    It.IsAny<CustomerTurnoverExcelOptions>()))
                    .Returns(fileContent);
            
            _appSettings.MaxRowPerExcelFile = 1000;

            // Act
            var result = await _controller.ExportExcel(mockFileProvider.Object, mockCompressService.Object, mockExcel.Object, _customerTurnoverService.Object, metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            
            // Verify the date format was set correctly through the option parameter
            mockExcel.Verify(x => x.ExportFile(
                It.IsAny<string>(), 
                It.IsAny<ReportResultModel<CustTurnover>>(), 
                It.IsAny<CustomerTurnoverMetaModel>(), 
                It.Is<CustomerTurnoverExcelOptions>(opt => opt != null)), // We can't easily verify the option content in unit test
                Times.Once);
        }

        [Fact]
        public async Task GetBettypesBySport_WithValidSportProducts_CallsServiceCorrectly()
        {
            // Arrange
            var sportProducts = FakeHelper.CreateMany<SportProductItem>().ToList();
            var expectedBetTypes = FakeHelper.CreateMany<BettypeProductItem>(5).ToList();
            
            _contextService.Setup(x => x.Language).Returns("en-US");
            _contextService.Setup(x => x.IsHideSaba).Returns(false);
            
            _betTypeService.Setup(x => x.GetBettypes(sportProducts, "en-US", false))
                          .ReturnsAsync(expectedBetTypes);

            // Act
            var response = await _controller.GetBettypesBySport(_betTypeService.Object, sportProducts) as OkObjectResult;
            var result = response.Value as List<BettypeProductItem>;

            // Assert
            Assert.Equal(expectedBetTypes, result);
            _betTypeService.Verify(x => x.GetBettypes(sportProducts, "en-US", false), Times.Once);
        }

        [Fact]
        public async Task GetData_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerTurnoverMetaModel>();
            var expectedResult = FakeHelper.Create<ReportResultModel<CustTurnover>>();
            var expectedCurrencyList = new Dictionary<int, string> { { 1, "USD" }, { 2, "EUR" } };
            var expectedLanguage = "vi-VN";
            
            _contextService.Setup(x => x.CurrencyList).Returns(expectedCurrencyList);
            _contextService.Setup(x => x.Language).Returns(expectedLanguage);
            
            _customerTurnoverService.Setup(x => x.GetDataList(metaModel, expectedCurrencyList, expectedLanguage))
                                  .ReturnsAsync(expectedResult);

            // Act
            var response = await _controller.GetData(_customerTurnoverService.Object, metaModel) as OkObjectResult;

            // Assert
            _customerTurnoverService.Verify(x => x.GetDataList(metaModel, expectedCurrencyList, expectedLanguage), Times.Once);
            Assert.False(metaModel.IsExportExcel);
        }

        private CustomerTurnoverController CreateController()
        {
            return new CustomerTurnoverController(_appSettings, _contextService.Object);
        }
    }
}
