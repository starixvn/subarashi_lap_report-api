using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class ExtraPayoutControllerTest
    {
        private const string PhysicalPath = "PhysicalPath";
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task Index_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<ExtraPayout>>();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(s => s.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                expectedData.DataList,
                Total = expectedData.TotalData,
                expectedData.TotalRows
            });

            var controller = new ExtraPayoutController();

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetData(metaModel, service.Object));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Export_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();

            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                    .With(x => x.DataList, new SortableList<ExtraPayout>() { new ExtraPayout { CustId = 100, PromotionType = 200 } })
                    .Create();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/ExtraPayout.xls")).Returns(fileInfo);

            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<IExtraPayoutExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = new ExtraPayoutController();

            // Assert
            var result = await controller.ExportExcel(service.Object, fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }
    }
}