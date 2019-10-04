using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerWinLossControllerTest
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
            service.Setup(s => s.GetDataList(metaModel)).Returns(Task.FromResult(expectedReportData));

            var expectedResult = new
            {
                expectedReportData.DataList,
                Total = expectedReportData.TotalData,
                expectedReportData.TotalRows,
                PromotionPrize = expectedReportData.AdditionalInfo
            };

            var controller = new CustomerWinLossController(service.Object);

            // Action
            var result = await controller.Index(metaModel);

            // Assert
            Assert.Equal(JsonConvert.SerializeObject(result), JsonConvert.SerializeObject(expectedResult));
        }

        [Fact]
        public async Task ExportExcel_WhenCall_CreateCorrectWorkbookObject()
        {
            // Arrange
            var metaModel = new CustomerWinLossMetaModel();
            var expectedReportData = new ResultModel<CustomerWinLoss>();
            var service = new Mock<ICustomerWinLossService>();
            service.Setup(s => s.GetDataList(metaModel)).Returns(Task.FromResult(expectedReportData));

            var physicalPath = @"D:\\CustomerWinLoss.xls";
            var fileInfo = Mock.Of<IFileInfo>(f => f.PhysicalPath == physicalPath);
            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(fp => fp.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerWinLoss.xls")).Returns(fileInfo);

            var workBook = new Mock<IWorkbook>();
            var excelService = new Mock<ICustomerWinLossExcel>();
            excelService.Setup(s => s.Export(physicalPath, expectedReportData, metaModel)).Returns(workBook.Object);

            var controller = new CustomerWinLossController(service.Object);

            // Action
            await controller.ExportExcel(fileProvider.Object, excelService.Object, metaModel);

            // Assert
            workBook.Verify(s => s.Write(It.IsAny<MemoryStream>()), Times.Once);
        }
    }
}