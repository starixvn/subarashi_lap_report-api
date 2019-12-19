using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Microsoft.Extensions.FileProviders;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class PromotionPrizeControllerTest
    {
        private readonly Fixture _fixture = new Fixture();
        private Mock<IPromotionPrizeRepository> _repository;

        private const string PhysicalPath = "PhysicalPath";

        [Fact]
        public async Task Export_WhenCal_ReturnCorrectContentResult()
        {
            // Arrange
            var expectedData = _fixture.Create<IDictionary<string, PromotionPrizeResultModel>>();
            var metaModel = _fixture.Create<PromotionPrizeMetaModel>();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/PromotionPrize.xls")).Returns(fileInfo);

            var excelService = new Mock<IPromotionPrizeExcel>();
            var workbook = new Mock<IWorkbook>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(workbook.Object);

            var controller = CreateController();
            _repository.Setup(r => r.GetData(metaModel)).Returns(Task.FromResult(expectedData));

            // Action
            var result = await controller.Export(fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.NotNull(result.FileContents);
        }

        private PromotionPrizeController CreateController()
        {
            _repository = new Mock<IPromotionPrizeRepository>();

            return new PromotionPrizeController(_repository.Object);
        }
    }
}