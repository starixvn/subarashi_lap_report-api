using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerBalanceControllerTest : BaseReportExcelTest
    {
        [Fact]
        public async Task GetData_WhenCall_ReturnsCorrectResult()
        {
            // Arrange
            var metaModel = new CustomerBalanceMetaModel();
            var service = new Mock<ICustomerBalanceService>();
            var expectedReportData = new ResultModel<CustomerBalance>();
            var expectedResult = new
            {
                Data = expectedReportData.DataList,
                Total = expectedReportData.TotalData,
                expectedReportData.TotalRows
            };

            service.Setup(s => s.GetDataList(metaModel)).Returns(Task.FromResult(expectedReportData));
            var controller = new CustomerBalanceController();

            // Action
            var result = await controller.GetData(metaModel, service.Object) as OkObjectResult;

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result.Value));
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnsCorrectFileAndFileIsDelete()
        {
            // Arrange
            const string ExpectedContentType = "application/octet-stream";
            var metaModel = new CustomerBalanceMetaModel();
            var excel = new Mock<ICustomerBalanceExcel>();
            var contextService = new Mock<IContextService>();
            var licenseeService = new Mock<ILicenseeService>();
            var fileName = CreateFile();

            excel.Setup(e => e.Export(metaModel)).Returns(Task.FromResult(fileName));
            var controller = new CustomerBalanceController();
            var expectedFileContent = File.ReadAllBytes(fileName);

            // Action
            var result = await controller.ExportExcel(metaModel, excel.Object, contextService.Object, licenseeService.Object) as FileStreamResult;
            var fileContentResult = GetStreamContent(result.FileStream);

            // Assert
            Assert.Equal(ExpectedContentType, result.ContentType);
            Assert.Equal(expectedFileContent, fileContentResult);
            Assert.True(File.Exists(fileName));
            result.FileStream.Dispose();
            Assert.False(File.Exists(fileName));
        }
    }
}