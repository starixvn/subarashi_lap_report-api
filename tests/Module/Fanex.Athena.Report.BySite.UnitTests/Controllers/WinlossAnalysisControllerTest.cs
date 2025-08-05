namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.DTOs;
    using BySite.DTOs.WinlossAnalysis;
    using BySite.Excels.Interfaces;
    using BySite.Excels.Options;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Excel;
    using Common.Services.Interfaces;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using Models;
    using Moq;
    using Xunit;

    public class WinlossAnalysisControllerTest
    {
        private readonly Mock<IBaseCurrencyService> _baseCurrencyService;
        private readonly Mock<ICompressService> _compressService;
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<ICustomerClassService> _customerClassService;
        private readonly ExcelOptions _excelOptions;
        private readonly Mock<IWinlossAnalysisExcel> _excelService;
        private readonly Mock<IFileInfo> _fileInfo;
        private readonly Mock<IFileProvider> _fileProvider;
        private readonly Mock<IFileService> _fileService;
        private readonly Fixture _fixture = new();
        private readonly Mock<IOptions<ExcelOptions>> _options;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<IWinlossAnalysisService> _service;
        private readonly Mock<ISubsidiaryService> _subsidiaryService;

        public WinlossAnalysisControllerTest()
        {
            _service = new Mock<IWinlossAnalysisService>();
            _excelService = new Mock<IWinlossAnalysisExcel>();
            _fileService = new Mock<IFileService>();
            _fileProvider = new Mock<IFileProvider>();
            _contextService = new Mock<IContextService>();
            _compressService = new Mock<ICompressService>();
            _subsidiaryService = new Mock<ISubsidiaryService>();
            _productService = new Mock<IProductService>();
            _baseCurrencyService = new Mock<IBaseCurrencyService>();
            _customerClassService = new Mock<ICustomerClassService>();
            _options = new Mock<IOptions<ExcelOptions>>();
            _fileInfo = new Mock<IFileInfo>();
            _excelOptions = new ExcelOptions { MaximumRowPerFile = 10000 };

            _options.Setup(x => x.Value).Returns(_excelOptions);

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task Index_WhenCalled_ReturnsOkResultWithInitialData()
        {
            // Arrange
            var siteName = _fixture.Create<string>();
            var productAccess = new ProductAccess(new List<ProductInfo>
                {
                    new() { ProductCode = "SB", ProductName = "Sportsbook" }
                }
            );
            var subsidiaries = _fixture.CreateMany<CheckedItem>().ToList();
            var riskLevels = _fixture.CreateMany<RiskLevel>().ToList();

            _contextService.Setup(x => x.LicenseeId).Returns(1);
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string> { { 1, "USD" } });
            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(true);
            _contextService.Setup(x => x.DefaultBaseCurrency).Returns("USD");
            _contextService.Setup(x => x.SpecialPlatform).Returns(1);
            _contextService.Setup(x => x.IsInRoles(It.IsAny<string[]>())).Returns(true);
            _contextService.Setup(x => x.OfferedOddsGroupsList).Returns(new[] { "A", "B" });

            _productService.Setup(x => x.GetProductAccess(1, It.IsAny<string>())).Returns(productAccess);
            _productService.Setup(x => x.IsOfferCashOutProduct(1)).Returns(true);
            _subsidiaryService.Setup(x => x.GetWhiteLabelTree(siteName)).ReturnsAsync(subsidiaries);
            _baseCurrencyService.Setup(x => x.GetBaseCurrencyList(false)).Returns(new Dictionary<int, string> { { 1, "USD" } });
            _customerClassService.Setup(x => x.GetAllRiskLevel()).ReturnsAsync(riskLevels);

            var controller = CreateController();

            // Act
            var result = await controller.Index();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetData_WhenCalled_ReturnsServiceResult()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            var expectedResult = _fixture.Create<WinlossAnalysisResultModel>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(1);
            _contextService.Setup(x => x.SpecialPlatformType).Returns(1);
            _service.Setup(x => x.GetDataAsync(It.IsAny<WinlossAnalysisMetaModel>())).ReturnsAsync(expectedResult);

            var controller = CreateController();

            // Act
            var result = await controller.GetData(metaModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task GetData_SetsSpecialPlatformProperties()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            var expectedResult = _fixture.Create<WinlossAnalysisResultModel>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(5);
            _contextService.Setup(x => x.SpecialPlatformType).Returns(3);
            _service.Setup(x => x.GetDataAsync(It.IsAny<WinlossAnalysisMetaModel>())).ReturnsAsync(expectedResult);

            var controller = CreateController();

            // Act
            await controller.GetData(metaModel);

            // Assert
            Assert.Equal(5, metaModel.SpecialPlatform);
            Assert.Equal(3, metaModel.SpecialPlatformType);
        }

        [Fact]
        public async Task ExportExcel_WhenSinglePage_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
            metaModel.PageIndex = 1;
            metaModel.PageSize = 1000;

            var resultData = new WinlossAnalysisResultModel
            {
                Data = _fixture.CreateMany<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>>(10)
            };

            var excelContent = new byte[] { 1, 2, 3, 4, 5 };
            var templatePath = "/test/template.xls";

            _contextService.Setup(x => x.SpecialPlatform).Returns(1);
            _contextService.Setup(x => x.SpecialPlatformType).Returns(1);
            _service.Setup(x => x.GetDataAsync(It.IsAny<WinlossAnalysisMetaModel>())).ReturnsAsync(resultData);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(templatePath);
            _fileProvider.Setup(x => x.GetFileInfo("/ExcelTemplate/WinLossAnalysis.xls")).Returns(_fileInfo.Object);
            _excelService.Setup(x => x.Export(templatePath, resultData, It.IsAny<WinlossAnalysisExcelOption>())).Returns(excelContent);

            var controller = CreateController();

            // Act
            var result = await controller.ExportExcel(metaModel);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(excelContent, fileResult.FileContents);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
        }

        //[Fact]
        //public async Task ExportExcel_WhenMultiplePages_ReturnsZipFile()
        //{
        //    // Arrange
        //    var metaModel = _fixture.Create<WinlossAnalysisMetaModel>();
        //    metaModel.PageIndex = 2;
        //    metaModel.PageSize = 1000;
        //    _options.Setup(x => x.Value).Returns(new ExcelOptions { MaximumRowPerFile = 1 });

        //    var resultData = new WinlossAnalysisResultModel
        //    {
        //        Data = _fixture.CreateMany<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>>(10)
        //    };
        //    var zipPath = "/test/output.zip";
        //    var templatePath = "/test/template.xls";

        //    _contextService.Setup(x => x.SpecialPlatform).Returns(1);
        //    _contextService.Setup(x => x.SpecialPlatformType).Returns(1);
        //    _fileInfo.Setup(x => x.PhysicalPath).Returns(templatePath);
        //    _fileProvider.Setup(x => x.GetFileInfo("/ExcelTemplate/WinLossAnalysis.xls")).Returns(_fileInfo.Object);
        //    _service.Setup(x => x.GetDataAsync(It.IsAny<WinlossAnalysisMetaModel>())).ReturnsAsync(resultData);
        //    _compressService.Setup(x => x.CompressDataToFile(It.IsAny<IEnumerable<ZipEntry>>(), true)).Returns(zipPath);

        //    var controller = CreateController();

        //    // Act
        //    var result = await controller.ExportExcel(metaModel);

        //    // Assert
        //    Assert.IsType<FileStreamResult>(result);
        //}

        private WinlossAnalysisController CreateController()
        {
            return new WinlossAnalysisController(
                _service.Object,
                _excelService.Object,
                _fileService.Object,
                _fileProvider.Object,
                _contextService.Object,
                _compressService.Object,
                _options.Object,
                _subsidiaryService.Object,
                _productService.Object,
                _baseCurrencyService.Object,
                _customerClassService.Object);
        }
    }
}