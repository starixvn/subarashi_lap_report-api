using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class DailyBetDetailsControllerTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<IDailyBetDetailsService> _service;
        private readonly Mock<IFileProvider> _fileProvider;
        private readonly Mock<IDailyBetDetailsExcel> _excel;
        private readonly Mock<IFileInfo> _fileInfo;

        public DailyBetDetailsControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _service = new Mock<IDailyBetDetailsService>();
            _fileProvider = new Mock<IFileProvider>();
            _excel = new Mock<IDailyBetDetailsExcel>();
            _fileInfo = new Mock<IFileInfo>();
        }

        [Fact]
        public async Task Index_WhenCalled_ReturnsOkResultWithData()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var expectedData = _fixture.Create<ResultModel<DailyBetDetails>>();
            var currencyList = _fixture.Create<Dictionary<int, string>>();
            var siteName = _fixture.Create<string>();
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.Index(metaModel, _service.Object);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var resultValue = JsonConvert.SerializeObject(okResult.Value);
            var expectedValue = JsonConvert.SerializeObject(new
            {
                expectedData.DataList,
                expectedData.TotalRows
            });

            Assert.Equal(expectedValue, resultValue);
            Assert.Equal(currencyList, metaModel.CurrencyList);
            Assert.Equal(siteName, metaModel.SiteName);
            _service.Verify(x => x.GetDataAsync(metaModel, auditCurrencies), Times.Once);
        }

        [Fact]
        public async Task Export_WhenCalled_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var expectedData = _fixture.Create<ResultModel<DailyBetDetails>>();
            var expectedFileContent = _fixture.Create<byte[]>();
            var templatePath = "/ExcelTemplate/DailyBetDetails.xls";
            var physicalPath = "C:\\Templates\\DailyBetDetails.xls";
            var currencyList = _fixture.Create<Dictionary<int, string>>();
            var siteName = _fixture.Create<string>();
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ReturnsAsync(expectedData);
            _excel.Setup(x => x.Export(physicalPath, expectedData, It.IsAny<DailyBetDetailsMetaModel>()))
                .Returns(expectedFileContent);

            var controller = CreateController();

            // Act
            var result = await controller.Export(_service.Object, _fileProvider.Object, _excel.Object, metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedFileContent, result.FileContents);
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(currencyList, metaModel.CurrencyList);
            Assert.Equal(siteName, metaModel.SiteName);
            Assert.Equal(0, metaModel.PageIndex); // Should be set to 0 for export

            _fileProvider.Verify(x => x.GetFileInfo(templatePath), Times.Once);
            _service.Verify(x => x.GetDataAsync(metaModel, auditCurrencies), Times.Once);
            _excel.Verify(x => x.Export(physicalPath, expectedData, metaModel), Times.Once);
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_ExceptionPropagates()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var expectedException = new System.Exception("Service error");
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ThrowsAsync(expectedException);

            var controller = CreateController();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => controller.Index(metaModel, _service.Object));
            Assert.Equal("Service error", exception.Message);
        }

        [Fact]
        public async Task Export_WhenServiceThrowsException_ExceptionPropagates()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var expectedException = new System.Exception("Service error");
            var templatePath = "/ExcelTemplate/DailyBetDetails.xls";
            var physicalPath = "C:\\Templates\\DailyBetDetails.xls";
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ThrowsAsync(expectedException);

            var controller = CreateController();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => 
                controller.Export(_service.Object, _fileProvider.Object, _excel.Object, metaModel));
            Assert.Equal("Service error", exception.Message);
        }

        [Fact]
        public async Task Index_WithNullMetaModel_ThrowsArgumentNullException()
        {
            // Arrange
            var controller = CreateController();

            // Act & Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(() => controller.Index(null, _service.Object));
        }

        [Fact]
        public async Task Export_WithNullMetaModel_ThrowsArgumentNullException()
        {
            // Arrange
            var controller = CreateController();

            // Act & Assert
            await Assert.ThrowsAsync<System.NullReferenceException>(() => 
                controller.Export(_service.Object, _fileProvider.Object, _excel.Object, null));
        }

        [Fact]
        public async Task Index_AssignsMetaModelPropertiesCorrectly()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var expectedData = _fixture.Create<ResultModel<DailyBetDetails>>();
            var currencyList = _fixture.Create<Dictionary<int, string>>();
            var siteName = _fixture.Create<string>();
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            await controller.Index(metaModel, _service.Object);

            // Assert
            Assert.Equal(currencyList, metaModel.CurrencyList);
            Assert.Equal(siteName, metaModel.SiteName);
        }

        [Fact]
        public async Task Export_AssignsMetaModelPropertiesCorrectly()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            var originalPageIndex = metaModel.PageIndex;
            var expectedData = _fixture.Create<ResultModel<DailyBetDetails>>();
            var expectedFileContent = _fixture.Create<byte[]>();
            var templatePath = "/ExcelTemplate/DailyBetDetails.xls";
            var physicalPath = "C:\\Templates\\DailyBetDetails.xls";
            var currencyList = _fixture.Create<Dictionary<int, string>>();
            var siteName = _fixture.Create<string>();
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.CurrencyList).Returns(currencyList);
            _contextService.Setup(x => x.SiteName).Returns(siteName);
            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ReturnsAsync(expectedData);
            _excel.Setup(x => x.Export(physicalPath, expectedData, It.IsAny<DailyBetDetailsMetaModel>()))
                .Returns(expectedFileContent);

            var controller = CreateController();

            // Act
            await controller.Export(_service.Object, _fileProvider.Object, _excel.Object, metaModel);

            // Assert
            Assert.Equal(currencyList, metaModel.CurrencyList);
            Assert.Equal(siteName, metaModel.SiteName);
            Assert.Equal(0, metaModel.PageIndex); // Should be reset to 0 for export
            Assert.NotEqual(originalPageIndex, metaModel.PageIndex); // Verify it was changed
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Export_SetsPageIndexToZero_RegardlessOfOriginalValue(int originalPageIndex)
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetDetailsMetaModel>();
            metaModel.PageIndex = originalPageIndex;
            var expectedData = _fixture.Create<ResultModel<DailyBetDetails>>();
            var expectedFileContent = _fixture.Create<byte[]>();
            var templatePath = "/ExcelTemplate/DailyBetDetails.xls";
            var physicalPath = "C:\\Templates\\DailyBetDetails.xls";
            var auditCurrencies = _fixture.Create<List<AuditCurrency>>();

            _contextService.Setup(x => x.AuditCurrencies).Returns(auditCurrencies);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _service.Setup(x => x.GetDataAsync(It.IsAny<DailyBetDetailsMetaModel>(), auditCurrencies))
                .ReturnsAsync(expectedData);
            _excel.Setup(x => x.Export(physicalPath, expectedData, It.IsAny<DailyBetDetailsMetaModel>()))
                .Returns(expectedFileContent);

            var controller = CreateController();

            // Act
            await controller.Export(_service.Object, _fileProvider.Object, _excel.Object, metaModel);

            // Assert
            Assert.Equal(0, metaModel.PageIndex);
        }

        private DailyBetDetailsController CreateController()
        {
            return new DailyBetDetailsController(_contextService.Object);
        }
    }
}
