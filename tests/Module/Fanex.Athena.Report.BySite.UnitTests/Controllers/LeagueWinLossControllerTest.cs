using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class LeagueWinLossControllerTest
    {
        private readonly Fixture _fixture = new Fixture();
        private const string PhysicalPath = "PhysicalPath";

        public LeagueWinLossControllerTest()
        {
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetMainData_HasData_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                    .With(x => x.DataList, new SortableList<LeagueWinLoss>() { new LeagueWinLoss { SportType = 99, LicenseeWL = 200 } })
                    .Create();

            var service = new Mock<ILeagueWinLossService>();
            var context = new Mock<IContextService>();

            var controller = new LeagueWinLossController(service.Object, context.Object);
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = expectedData.DataList.ToList(),
                Total = expectedData.TotalData
            });

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetMainData(metaModel));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetMainData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>().Create();

            var service = new Mock<ILeagueWinLossService>();
            var context = new Mock<IContextService>();

            var controller = new LeagueWinLossController(service.Object, context.Object);
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = new List<LeagueWinLoss>(),
                Total = new LeagueWinLoss()
            });

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetMainData(metaModel));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetCurrencyData_HasData_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                    .With(x => x.DataList, new SortableList<LeagueWinLoss>() { new LeagueWinLoss { SportType = 99, LicenseeWL = 200 } })
                    .Create();

            var service = new Mock<ILeagueWinLossService>();
            var context = new Mock<IContextService>();

            var controller = new LeagueWinLossController(service.Object, context.Object);
            service.Setup(r => r.GetCurrencyDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new LeagueWinLossResultModel
            {
                Data = expectedData.DataList.ToList()
            });

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetCurrencyData(metaModel));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetCurrencyData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>().Create();

            var service = new Mock<ILeagueWinLossService>();
            var context = new Mock<IContextService>();

            var controller = new LeagueWinLossController(service.Object, context.Object);
            service.Setup(r => r.GetCurrencyDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = new List<LeagueWinLoss>(),
                Total = new LeagueWinLoss()
            });

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetCurrencyData(metaModel));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<LeagueWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<LeagueWinLoss>>()
                    .With(x => x.DataList, new SortableList<LeagueWinLoss>() { new LeagueWinLoss { SportType = 99, LicenseeWL = 200 } })
                    .Create();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/LeagueWinLoss.xls")).Returns(fileInfo);

            var service = new Mock<ILeagueWinLossService>();
            var context = new Mock<IContextService>();

            var controller = new LeagueWinLossController(service.Object, context.Object);
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<ILeagueWinLossExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            // Assert
            var result = await controller.ExportExcel(fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }
    }
}