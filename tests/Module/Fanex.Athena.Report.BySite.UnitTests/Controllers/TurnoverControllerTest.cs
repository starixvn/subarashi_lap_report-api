namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BySite.Controllers;
    using BySite.Excels.Interfaces;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Settings;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Microsoft.Extensions.FileProviders;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

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
