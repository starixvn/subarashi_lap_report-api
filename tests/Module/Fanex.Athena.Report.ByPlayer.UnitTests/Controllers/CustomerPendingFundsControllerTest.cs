using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.Excels.Options;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Models;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerPendingFundsControllerTest
    {
        private readonly Mock<IContextService> _contextService;

        private readonly Mock<IProductService> _productService;

        public CustomerPendingFundsControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _productService = new Mock<IProductService>();
        }

        [Fact]
        public async Task Data_WhenCall_ReturnCorectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerPendingFundsMetaModel>();
            var result = FakeHelper.Create<ReportResultModel<CustPendingFunds>>();

            var customerPendingFundsService = new Mock<ICustomerPendingFundsService>();
            customerPendingFundsService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(result);

            var controller = CreateController();
            // Action
            var response = await controller.Data(customerPendingFundsService.Object, metaModel) as OkObjectResult;
            var responseValue = response!.Value as CustomerPendingFundsResultModel;

            // Assert
            Assert.Equal(result.DataList, responseValue!.Data);
            Assert.Equal(result.TotalData, responseValue.Total);
            Assert.Equal(result.TotalRows, responseValue.TotalRows);
        }

        [Theory] [AutoData]
        public async Task Excel_WhenCall_ReturnCorectResult(string template, CustomerPendingFundsMetaModel metaModel,
                                                            ReportResultModel<CustPendingFunds> result)
        {
            // Arrange
            var excelService = new Mock<ICustomerPendingFundsExcel>();

            var customerPendingFundsService = new Mock<ICustomerPendingFundsService>();
            customerPendingFundsService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(result);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(x => x.PhysicalPath).Returns(template);

            var file = new Mock<IFileProvider>();
            file.Setup(x => x.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerPendingFunds.xls"))
                .Returns(fileInfo.Object);

            var controller = CreateController();

            // Action
            var response =
                await controller.Excel(file.Object, excelService.Object, customerPendingFundsService.Object, metaModel)
                    as FileContentResult;

            // Assert
            Assert.NotNull(response);
            fileInfo.VerifyGet(x => x.PhysicalPath, Times.Once);
            excelService.Verify(x => x.Export(template, result, It.IsAny<CustomerPendingFundsExcelOptions>()));
        }

        private CustomerPendingFundsController CreateController() => new(_contextService.Object, _productService.Object);
    }
}
