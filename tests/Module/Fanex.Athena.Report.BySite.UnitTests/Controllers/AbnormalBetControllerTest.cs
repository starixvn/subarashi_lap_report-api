namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.DTOs;
    using BySite.Excels.Interfaces;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Model.ViewModel;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    public class AbnormalBetControllerTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IAbnormalBetService> _abnormalBetService;
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<IFileProvider> _fileProvider;
        private readonly Mock<IAbnormalBetSummaryExcel> _summaryExcel;
        private readonly Mock<IAbnormalBetIpSummaryExcel> _ipSummaryExcel;
        private readonly Mock<IAbnormalBetDetailExcel> _detailExcel;
        private readonly Mock<IFileInfo> _fileInfo;

        public AbnormalBetControllerTest()
        {
            _abnormalBetService = new Mock<IAbnormalBetService>();
            _contextService = new Mock<IContextService>();
            _fileProvider = new Mock<IFileProvider>();
            _summaryExcel = new Mock<IAbnormalBetSummaryExcel>();
            _ipSummaryExcel = new Mock<IAbnormalBetIpSummaryExcel>();
            _detailExcel = new Mock<IAbnormalBetDetailExcel>();
            _fileInfo = new Mock<IFileInfo>();
        }

        [Fact]
        public async Task GetData_WhenCalled_ReturnsJsonResultWithData()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<AbnormalBetSummary>>();
            var specialPlatform = _fixture.Create<int>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _abnormalBetService.Setup(x => x.GetDataAsync(It.IsAny<AbnormalBetMetaModel>()))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            var result = await controller.GetData(metaModel);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result;
            var resultValue = JsonConvert.SerializeObject(jsonResult.Value);
            var expectedValue = JsonConvert.SerializeObject(new { data = expectedData.DataList });
            
            Assert.Equal(expectedValue, resultValue);
            Assert.Equal(specialPlatform, metaModel.SpecialPlatform);
            _abnormalBetService.Verify(x => x.GetDataAsync(metaModel), Times.Once);
        }

        [Fact]
        public async Task ExportSummaryExcel_WhenCalled_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<AbnormalBetSummary>>();
            var expectedFileContent = _fixture.Create<byte[]>();
            var templatePath = "/ExcelTemplate/AbnormalBetSummary.xls";
            var physicalPath = "C:\\Templates\\AbnormalBetSummary.xls";
            var specialPlatform = _fixture.Create<int>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _abnormalBetService.Setup(x => x.GetDataAsync(It.IsAny<AbnormalBetMetaModel>()))
                .ReturnsAsync(expectedData);
            _summaryExcel.Setup(x => x.Export(physicalPath, expectedData, It.IsAny<AbnormalBetMetaModel>()))
                .Returns(expectedFileContent);

            var controller = CreateController();

            // Act
            var result = await controller.ExportSummaryExcel(_fileProvider.Object, _summaryExcel.Object, metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedFileContent, result.FileContents);
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(specialPlatform, metaModel.SpecialPlatform);
            
            _fileProvider.Verify(x => x.GetFileInfo(templatePath), Times.Once);
            _abnormalBetService.Verify(x => x.GetDataAsync(metaModel), Times.Once);
            _summaryExcel.Verify(x => x.Export(physicalPath, expectedData, metaModel), Times.Once);
        }

        [Fact]
        public async Task ExportIpSummaryExcel_WhenCalled_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<AbnormalBetIpSummary>>();
            var expectedFileContent = _fixture.Create<byte[]>();
            var templatePath = "/ExcelTemplate/AbnormalBetIpSummary.xls";
            var physicalPath = "C:\\Templates\\AbnormalBetIpSummary.xls";
            var specialPlatform = _fixture.Create<int>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _abnormalBetService.Setup(x => x.GetDataAsync(It.IsAny<AbnormalBetMetaModel>()))
                .ReturnsAsync(expectedData);
            _ipSummaryExcel.Setup(x => x.Export(physicalPath, expectedData, It.IsAny<AbnormalBetMetaModel>()))
                .Returns(expectedFileContent);

            var controller = CreateController();

            // Act
            var result = await controller.ExportIpSummaryExcel(_fileProvider.Object, _ipSummaryExcel.Object, metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedFileContent, result.FileContents);
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(specialPlatform, metaModel.SpecialPlatform);
            
            _fileProvider.Verify(x => x.GetFileInfo(templatePath), Times.Once);
            _abnormalBetService.Verify(x => x.GetDataAsync(metaModel), Times.Once);
            _ipSummaryExcel.Verify(x => x.Export(physicalPath, expectedData, metaModel), Times.Once);
        }

        [Fact]
        public async Task ExportDetailExcel_WhenCalled_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<AbnormalBetDetail>>();
            var expectedFileContent = _fixture.Create<byte[]>();
            var templatePath = "/ExcelTemplate/AbnormalBetDetail.xls";
            var physicalPath = "C:\\Templates\\AbnormalBetDetail.xls";
            var specialPlatform = _fixture.Create<int>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _fileInfo.Setup(x => x.PhysicalPath).Returns(physicalPath);
            _fileProvider.Setup(x => x.GetFileInfo(templatePath)).Returns(_fileInfo.Object);
            _abnormalBetService.Setup(x => x.GetDataAsync(It.IsAny<AbnormalBetMetaModel>()))
                .ReturnsAsync(expectedData);
            _detailExcel.Setup(x => x.Export(physicalPath, expectedData, It.IsAny<AbnormalBetMetaModel>()))
                .Returns(expectedFileContent);

            var controller = CreateController();

            // Act
            var result = await controller.ExportDetailExcel(_fileProvider.Object, _detailExcel.Object, metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedFileContent, result.FileContents);
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(specialPlatform, metaModel.SpecialPlatform);
            
            _fileProvider.Verify(x => x.GetFileInfo(templatePath), Times.Once);
            _abnormalBetService.Verify(x => x.GetDataAsync(metaModel), Times.Once);
            _detailExcel.Verify(x => x.Export(physicalPath, expectedData, metaModel), Times.Once);
        }

        [Fact]
        public async Task GetData_WhenServiceThrowsException_ExceptionPropagates()
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            var expectedException = new Exception("Service error");

            _abnormalBetService.Setup(x => x.GetDataAsync(It.IsAny<AbnormalBetMetaModel>()))
                .ThrowsAsync(expectedException);

            var controller = CreateController();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => controller.GetData(metaModel));
            Assert.Equal("Service error", exception.Message);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(0)]
        public async Task GetData_WithDifferentSpecialPlatforms_AssignsCorrectly(int specialPlatform)
        {
            // Arrange
            var metaModel = _fixture.Create<AbnormalBetMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<AbnormalBetSummary>>();

            _contextService.Setup(x => x.SpecialPlatform).Returns(specialPlatform);
            _abnormalBetService.Setup(x => x.GetDataAsync(It.IsAny<AbnormalBetMetaModel>()))
                .ReturnsAsync(expectedData);

            var controller = CreateController();

            // Act
            await controller.GetData(metaModel);

            // Assert
            Assert.Equal(specialPlatform, metaModel.SpecialPlatform);
        }

        private AbnormalBetController CreateController()
        {
            return new AbnormalBetController(_abnormalBetService.Object, _contextService.Object);
        }
    }
}
