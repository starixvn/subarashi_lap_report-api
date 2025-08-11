namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.DTOs;
    using BySite.Excels.Interfaces;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Moq;
    using Xunit;

    public class PlayerSabaCoinsControllerTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IPlayerSabaCoinsService> _playerSabaCoinsService;
        private readonly Mock<IFileProvider> _fileProvider;
        private readonly Mock<IPlayerSabaCoinsExcel> _playerSabaCoinsExcel;
        private readonly Mock<IFileInfo> _fileInfo;

        public PlayerSabaCoinsControllerTest()
        {
            _playerSabaCoinsService = new Mock<IPlayerSabaCoinsService>();
            _fileProvider = new Mock<IFileProvider>();
            _playerSabaCoinsExcel = new Mock<IPlayerSabaCoinsExcel>();
            _fileInfo = new Mock<IFileInfo>();
        }

        [Fact]
        public async Task GetData_WhenCalled_ReturnsOkResultWithData()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var serviceResult = new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = new SortableList<PlayerSabaCoins>(_fixture.CreateMany<PlayerSabaCoins>(3)),
                TotalData = _fixture.Create<PlayerSabaCoins>(),
                TotalRows = 100
            };

            _playerSabaCoinsService.Setup(x => x.GetDataAsync(metaModel))
                                  .ReturnsAsync(serviceResult);

            var controller = CreateController();

            // Act
            var result = await controller.GetData(metaModel, _playerSabaCoinsService.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultModel = Assert.IsType<PlayerSabaCoinsResultModel>(okResult.Value);
            Assert.Equal(3, resultModel.Data.Count);
            Assert.Equal(serviceResult.TotalData, resultModel.TotalSummary);
            Assert.Equal(100, resultModel.TotalRows);
        }

        [Fact]
        public async Task GetData_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var serviceResult = new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = new SortableList<PlayerSabaCoins>(),
                TotalData = _fixture.Create<PlayerSabaCoins>(),
                TotalRows = 0
            };

            _playerSabaCoinsService.Setup(x => x.GetDataAsync(metaModel))
                                  .ReturnsAsync(serviceResult);

            var controller = CreateController();

            // Act
            await controller.GetData(metaModel, _playerSabaCoinsService.Object);

            // Assert
            _playerSabaCoinsService.Verify(x => x.GetDataAsync(metaModel), Times.Once);
        }

        [Fact]
        public async Task ExportExcel_WhenCalled_ReturnsFileContentResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var serviceResult = new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = new SortableList<PlayerSabaCoins>(_fixture.CreateMany<PlayerSabaCoins>(2)),
                TotalData = _fixture.Create<PlayerSabaCoins>(),
                TotalRows = 50
            };
            var templatePath = "/test/path/template.xls";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            _fileInfo.Setup(x => x.PhysicalPath).Returns(templatePath);
            _fileProvider.Setup(x => x.GetFileInfo("/ExcelTemplate/PlayerSabaCoins.xls"))
                         .Returns(_fileInfo.Object);
            _playerSabaCoinsService.Setup(x => x.GetDataAsync(metaModel))
                                  .ReturnsAsync(serviceResult);
            _playerSabaCoinsExcel.Setup(x => x.Export(templatePath, serviceResult, metaModel))
                                 .Returns(fileContent);

            var controller = CreateController();

            // Act
            var result = await controller.ExportExcel(
                _playerSabaCoinsService.Object,
                _fileProvider.Object,
                _playerSabaCoinsExcel.Object,
                metaModel);

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(fileContent, result.FileContents);
            Assert.Equal("application/octet-stream", result.ContentType);
        }

        [Fact]
        public async Task ExportExcel_CallsServicesWithCorrectParameters()
        {
            // Arrange
            var metaModel = _fixture.Create<PlayerSabaCoinsMetaModel>();
            var serviceResult = new SortableReportResultModel<PlayerSabaCoins>
            {
                DataList = new SortableList<PlayerSabaCoins>(),
                TotalData = _fixture.Create<PlayerSabaCoins>(),
                TotalRows = 0
            };
            var templatePath = "/test/path/template.xls";
            var fileContent = new byte[] { 1, 2, 3 };

            _fileInfo.Setup(x => x.PhysicalPath).Returns(templatePath);
            _fileProvider.Setup(x => x.GetFileInfo("/ExcelTemplate/PlayerSabaCoins.xls"))
                         .Returns(_fileInfo.Object);
            _playerSabaCoinsService.Setup(x => x.GetDataAsync(metaModel))
                                  .ReturnsAsync(serviceResult);
            _playerSabaCoinsExcel.Setup(x => x.Export(templatePath, serviceResult, metaModel))
                                 .Returns(fileContent);

            var controller = CreateController();

            // Act
            await controller.ExportExcel(
                _playerSabaCoinsService.Object,
                _fileProvider.Object,
                _playerSabaCoinsExcel.Object,
                metaModel);

            // Assert
            _playerSabaCoinsService.Verify(x => x.GetDataAsync(metaModel), Times.Once);
            _playerSabaCoinsExcel.Verify(x => x.Export(templatePath, serviceResult, metaModel), Times.Once);
            _fileProvider.Verify(x => x.GetFileInfo("/ExcelTemplate/PlayerSabaCoins.xls"), Times.Once);
        }

        private PlayerSabaCoinsController CreateController()
        {
            return new PlayerSabaCoinsController();
        }
    }
}