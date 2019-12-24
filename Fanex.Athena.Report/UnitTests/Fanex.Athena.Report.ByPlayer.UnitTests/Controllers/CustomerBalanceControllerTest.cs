using System.IO;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerBalanceControllerTest : BaseReportExcelTest
    {
        [Fact]
        public async Task Index_WhenCall_ReturnsCorrectResult()
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
            var result = await controller.Index(metaModel, service.Object);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnsCorrectFileAndFileIsDeleted()
        {
            // Arrange
            const string expectedContentType = "application/octet-stream";
            var metaModel = new CustomerBalanceMetaModel();
            var excel = new Mock<ICustomerBalanceExcel>();
            var fileName = CreateFile();

            excel.Setup(e => e.Export(metaModel)).Returns(Task.FromResult(fileName));
            var controller = new CustomerBalanceController();
            var expectedFileContent = File.ReadAllBytes(fileName);

            // Action
            var result = await controller.ExportExcel(metaModel, excel.Object) as FileStreamResult;
            var fileContentResult = GetStreamContent(result.FileStream);

            // Assert
            Assert.Equal(expectedContentType, result.ContentType);
            Assert.Equal(expectedFileContent, fileContentResult);

            Assert.True(File.Exists(fileName));
            result.FileStream.Dispose();
            Assert.False(File.Exists(fileName));
        }
    }
}