using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class TurnoverControllerTest
    {
        private const int LicenseeId = 1;

        private const string Language = "en";

        private readonly AppSettings _appSettings;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<ITurnoverService> _turnoverService;

        public TurnoverControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _appSettings = FakeHelper.Create<AppSettings>();
            _turnoverService = new Mock<ITurnoverService>();
        }

        [Fact]
        public async Task GetData_WhenCall_ReturnData()
        {
            // Arrange
            var controller = CreateController();

            var metaModel = FakeHelper.Create<TurnoverMetaModel>();
            var result = FakeHelper.CreateMany<TurnoverDto>().ToList();
            var dataPermissionService = new Mock<IDataPermissionService>();

            _contextService.Setup(x => x.IsFakeLicensee).Returns(true);
            _turnoverService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(result);

            // Action
            var response =
                await controller.GetData(_turnoverService.Object, dataPermissionService.Object, metaModel) as
                    OkObjectResult;
            var responseValue = response!.Value as TurnoverResultModel;

            // Assert
            Assert.Equal(result[0], responseValue.Data[0]);
            Assert.Equal(result[1], responseValue.Data[1]);
            Assert.Equal(result[2], responseValue.Data[2]);
            dataPermissionService.Verify(x => x.ClearDataByLimitPermission(It.IsAny<TurnoverDto>()),
                Times.Exactly(result.Count));
            _turnoverService.Verify(x => x.SumTotalData(result), Times.Once);
        }

        [Fact]
        public async Task GetData_EmptyMetalModel_AssignNullUsername()
        {
            // Arrange
            var controller = CreateController();
            var metaModel = FakeHelper.Create<TurnoverMetaModel>();
            metaModel.Username = "";

            var result = FakeHelper.CreateMany<TurnoverDto>().ToList();
            var dataPermissionService = new Mock<IDataPermissionService>();

            _turnoverService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(result);

            // Action
            var response =
                await controller.GetData(_turnoverService.Object, dataPermissionService.Object, metaModel) as
                    OkObjectResult;

            // Assert
            Assert.Null(metaModel.Username);
        }

        [Theory]
        [InlineData(true, " External ID ", null)]
        [InlineData(true, "abcd", "abcd")]
        [InlineData(true, "   ", null)]
        [InlineData(false, "  External ID or System ID ", null)]
        [InlineData(false, "External ID", "External ID")]
        [InlineData(false, "   ", null)]
        public async Task Excel_WhenCall_AssignCorrectUsername(bool isFakeLicensee, string inputUserName,
                                                               string expectedUserName)
        {
            // Arrange
            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = new Mock<IFileInfo>();
            fileProvider.Setup(x => x.GetFileInfo("/ExcelTemplate/Turnover.xls")).Returns(fileInfo.Object);
            var excel = new Mock<ITurnoverExcel>();

            _contextService.Setup(x => x.IsFakeLicensee).Returns(isFakeLicensee);
            var metaModel = new TurnoverMetaModel
            {
                Username = inputUserName
            };

            var controller = CreateController();

            // Action
            await controller.ExportExcel(fileProvider.Object, excel.Object, _turnoverService.Object, metaModel);

            // Assert
            Assert.Equal(expectedUserName, metaModel.Username);
        }

        private TurnoverController CreateController(string productCodeList = null)
        {
            var productList = string.IsNullOrEmpty(productCodeList) ?
                new List<ProductInfo>() :
                productCodeList.Split(',').Select(x => new ProductInfo
                {
                    ProductCode = x
                });
            _contextService.Setup(x => x.LicenseeId).Returns(LicenseeId);
            _contextService.Setup(x => x.Language).Returns(Language);

            var productService = new Mock<IProductService>();
            productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.Turnover)).Returns(new ProductAccess(productList.ToList()));

            _contextService.Setup(x => x.IsInRoles(Roles.HidePlayerWL_WL_Comm_Total)).Returns(true);

            var controller = new TurnoverController(_contextService.Object, _appSettings, productService.Object);

            return controller;
        }
    }
}
