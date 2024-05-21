using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class NewCashOutWinLossControllerTest
    {
        private readonly Fixture _fixture = new Fixture();

        private const string PhysicalPath = "PhysicalPath";

        [Fact]
        public async Task GetData_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                    .With(x => x.DataList, new SortableList<CashOutWinLoss>() { new CashOutWinLoss { LicenseeWL = 100, CustomerWL = 200 } })
                    .Create();

            var controller = new NewCashOutWinLossController();
            var service = new Mock<INewCashOutWinLossService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = expectedData.DataList.ToList(),
                TotalCurrency = expectedData.TotalDataList.ToList(),
                TotalSummary = expectedData.TotalData,
                TotalRows = expectedData.TotalRows
            });

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetData(metaModel, service.Object));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetData_WhenCall_NoDataReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>().Create();

            var controller = new NewCashOutWinLossController();
            var service = new Mock<INewCashOutWinLossService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new NewCashOutWinLossResultModel
            {
                Data = new List<CashOutWinLoss>(),
                TotalCurrency = new List<CashOutWinLoss>(),
                TotalSummary = new CashOutWinLoss()
            });

            // Action
            var result = JsonConvert.SerializeObject(await controller.GetData(metaModel, service.Object));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<NewCashOutWinLossMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<CashOutWinLoss>>()
                    .With(x => x.DataList, new SortableList<CashOutWinLoss>() { new CashOutWinLoss { LicenseeWL = 100, CustomerWL = 200 } })
                    .Create();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/NewCashOutWinLoss.xls")).Returns(fileInfo);

            var service = new Mock<INewCashOutWinLossService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<INewCashOutWinLossExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var contextService = new Mock<IContextService>();
            var licenseeService = new Mock<ILicenseeService>();

            var controller = new NewCashOutWinLossController();

            // Assert
            var result = await controller.ExportExcel(
                service.Object,
                fileProvider.Object,
                excelService.Object,
                contextService.Object,
                licenseeService.Object,
                metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }
    }
}