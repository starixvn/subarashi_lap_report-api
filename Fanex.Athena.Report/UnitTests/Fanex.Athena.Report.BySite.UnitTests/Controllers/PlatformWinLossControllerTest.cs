using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class PlatformWinLossControllerTest
    {
        private Mock<IPlatformWinLossRepository> _repository;

        private readonly Fixture _fixture = new Fixture();

        private const string PhysicalPath = "PhysicalPath";

        [Fact]
        public async Task GetSummaryData_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();
            var expectedResult = _fixture.Create<ResultModel<PlatformWinLossSummary>>();

            var controller = CreateController();
            _repository.Setup(r => r.GetSummaryDataAsync(metaModel)).Returns(Task.FromResult(expectedResult));

            // Action
            var result = await controller.GetSummaryData(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetMemberData_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();
            var expectedResult = _fixture.Create<SortableResultModel<PlatformWinLossMember>>();

            var controller = CreateController();
            _repository.Setup(r => r.GetMemberDataAsync(metaModel)).Returns(Task.FromResult(expectedResult));

            // Action
            var result = await controller.GetMemberData(metaModel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExportMemberData_WhenCall_ReturnCorrectContentResult()
        {
            // Arrange
            var expectedData = _fixture.Create<SortableResultModel<PlatformWinLossMember>>();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/PlatformWinLossMember.xls")).Returns(fileInfo);

            var excelService = new Mock<IPlatformWinLossMemberExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = CreateController();
            _repository.Setup(r => r.GetMemberDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            // Action
            var result = await controller.ExportMemberData(fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }

        [Fact]
        public async Task ExportSummaryData_WhenCall_ReturnCorrectContentResult()
        {
            // Arrange
            var expectedData = _fixture.Create<ResultModel<PlatformWinLossSummary>>();
            var metaModel = _fixture.Create<PlatformWinLossMetaModel>();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/PlatformWinLossSummary.xls")).Returns(fileInfo);

            var excelService = new Mock<IPlatformWinLossSummaryExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = CreateController();
            _repository.Setup(r => r.GetSummaryDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            // Action
            var result = await controller.ExportSummaryData(fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }

        private PlatformWinLossController CreateController()
        {
            _repository = new Mock<IPlatformWinLossRepository>();

            return new PlatformWinLossController(_repository.Object);
        }
    }
}