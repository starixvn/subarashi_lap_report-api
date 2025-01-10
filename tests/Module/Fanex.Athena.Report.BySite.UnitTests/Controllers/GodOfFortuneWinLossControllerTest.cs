using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class GodOfFortuneWinLossControllerTest
    {
        private readonly Fixture _fixture = new Fixture();

        private const string PhysicalPath = "PhysicalPath";

        [Fact]
        public async Task Index_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<GodOfFortuneWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
                    .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>() { new GodOfFortuneWinLoss { LicenseeWinLoss = 100, CustomerWinLoss = 200 } })
                    .Create();

            var controller = new GodOfFortuneWinLossController();
            var service = new Mock<IGodOfFortuneWinLossService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = expectedData.DataList.ToList(),
                Total = expectedData.TotalDataList.ToList(),
                SummaryTotal = expectedData.SummaryTotal.ToDictionary(d => d.Key, d => d.Value)
            });

            // Action
            var result = JsonConvert.SerializeObject((await controller.Index(metaModel, service.Object) as OkObjectResult).Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Index_WhenCall_NoDataReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<GodOfFortuneWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
                    .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>())
                    .Create();

            var controller = new GodOfFortuneWinLossController();
            var service = new Mock<IGodOfFortuneWinLossService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new GodOfFortuneWinLossResultModel
            {
                Data = new List<GodOfFortuneWinLoss>(),
                Total = new List<GodOfFortuneWinLoss>(),
                SummaryTotal = new Dictionary<string, GodOfFortuneWinLoss>(),
            });

            // Action
            var result = JsonConvert.SerializeObject(((await controller.Index(metaModel, service.Object)) as OkObjectResult).Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<GodOfFortuneWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<GodOfFortuneWinLoss>>()
                    .With(x => x.DataList, new SortableList<GodOfFortuneWinLoss>() { new GodOfFortuneWinLoss { LicenseeWinLoss = 100, CustomerWinLoss = 200 } })
                    .Create();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/GodOfFortuneWinLoss.xls")).Returns(fileInfo);

            var service = new Mock<IGodOfFortuneWinLossService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<IGodOfFortuneWinLossExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = new GodOfFortuneWinLossController();

            // Assert
            var result = await controller.ExportExcel(service.Object, fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }
    }
}