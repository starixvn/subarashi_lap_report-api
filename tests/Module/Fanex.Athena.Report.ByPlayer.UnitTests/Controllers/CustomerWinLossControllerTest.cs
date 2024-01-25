using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerWinLossControllerTest : BaseReportExcelTest
    {
        [Fact]
        public async Task Index_WhenCall_ReturnCorrectData()
        {
            // Arrange
            var metaModel = new CustomerWinLossMetaModel();
            var dataList = new List<CustomerWinLoss> { new CustomerWinLoss { CustId = 1 } };
            var totalData = new CustomerWinLoss { Turnover = 500m };
            var expectedReportData = new ResultModel<CustomerWinLoss>
            {
                DataList = dataList,
                TotalData = totalData,
                TotalRows = 1,
                AdditionalInfo = "AdditionalInfo"
            };
            var service = new Mock<ICustomerWinLossService>();
            service.Setup(s => s.GetDataList(metaModel, true)).Returns(Task.FromResult(expectedReportData));

            var contextService = new Mock<IContextService>();
            var licenseeService = new Mock<ILicenseeService>();

            var expectedResult = new
            {
                expectedReportData.DataList,
                Total = expectedReportData.TotalData,
                expectedReportData.TotalRows,
                PromotionPrize = expectedReportData.AdditionalInfo
            };

            var controller = new CustomerWinLossController(service.Object, contextService.Object, licenseeService.Object);

            // Action
            var result = await controller.Index(metaModel) as OkObjectResult;

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result.Value));
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnsCorrectFileAndFileIsDeleted()
        {
            // Arrange
            var metaModel = new CustomerWinLossMetaModel();
            var expectedReportData = new ResultModel<CustomerWinLoss>();
            var service = new Mock<ICustomerWinLossService>();
            service.Setup(s => s.GetDataList(metaModel, false)).Returns(Task.FromResult(expectedReportData));

            var physicalPath = @"D:\\CustomerWinLoss.xlsx";
            var fileInfo = Mock.Of<IFileInfo>(f => f.PhysicalPath == physicalPath);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerWinLoss.xlsx")).Returns(fileInfo);

            var excelService = new Mock<ICustomerWinLossExcel>();
            var contextService = new Mock<IContextService>();
            var licenseeService = new Mock<ILicenseeService>();

            var fileName = CreateFile();
            var expectedFileContent = File.ReadAllBytes(fileName);

            excelService.Setup(s => s.Export(physicalPath, expectedReportData, metaModel)).Returns(fileName);

            var controller = new CustomerWinLossController(service.Object, contextService.Object, licenseeService.Object);

            // Action
            var result = await controller.ExportExcel(fileProvider.Object, excelService.Object, metaModel) as FileStreamResult;
            var fileContentResult = GetStreamContent(result.FileStream);

            // Assert
            Assert.Equal(expectedFileContent, fileContentResult);
            Assert.Equal("application/octet-stream", result.ContentType);

            Assert.True(File.Exists(fileName));
            result.FileStream.Dispose();
            Assert.False(File.Exists(fileName));
        }
    }
}