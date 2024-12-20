using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class MatchWinLossControllerTest
    {
        private readonly Mock<IBaseCurrencyService> _baseCurrencyService;

        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<IMatchWinLossService> _matchWinLossService;

        private readonly Mock<IProductService> _productService;

        public MatchWinLossControllerTest()
        {
            _baseCurrencyService = new Mock<IBaseCurrencyService>();
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _contextService = new Mock<IContextService>();
            _matchWinLossService = new Mock<IMatchWinLossService>();
            _productService = new Mock<IProductService>();

            _contextService.Setup(x => x.LicenseeId).Returns(1);
            _contextService.Setup(x => x.Language).Returns("Any");
        }

        [Fact]
        public async Task Detail_WhenGet_ReturnCorrectResult()
        {
            // Arrange
            using var controller = CreateController();

            var metaModel = new MatchWinLossMetaModel
            {
                CurrencyId = 1,
                ProductCodeList = "SB,NG",
                MatchId = 1,
                LeagueId = 1
            };
            var dataList = new List<MatchWinLoss> { new(), };
            _matchWinLossService.Setup(x => x.GetDetailDataList(metaModel)).ReturnsAsync(dataList);

            // Action
            var response = await controller.GetDetailData(metaModel) as OkObjectResult;
            var result = response!.Value as IList<MatchWinLoss>;

            // Assert
            Assert.Equal(dataList, result);
        }

        [Theory]
        [InlineData(6, false)]
        [InlineData(8, true)]
        public async Task GetData_GetColumnCount_AllowCustomerComm_ReturnCorrectResult(
            int expected, bool isAllowCustomerComm)
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            var dataList = FakeHelper.CreateMany<MatchWinLoss>().ToList();

            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            _contextService.Setup(x => x.IsAllowedCustomerComm).Returns(isAllowCustomerComm);
            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(FakeHelper.CreateMany<ProductInfo>().ToList()));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.Equal(expected, result!.ColumnCount);
        }

        [Fact]
        public async Task GetData_GetColumnCount_AllowBa_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            var dataList = FakeHelper.CreateMany<MatchWinLoss>().ToList();

            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            var productInfos = FakeHelper.CreateMany<ProductInfo>().ToList();
            productInfos[0].ProductCode = Product.BA;
            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(productInfos));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.Equal(8, result!.ColumnCount);
        }

        [Fact]
        public async Task GetData_GetColumnCount_NotAllowBa_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            var dataList = FakeHelper.CreateMany<MatchWinLoss>().ToList();

            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(FakeHelper.CreateMany<ProductInfo>().ToList()));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.Equal(6, result!.ColumnCount);
        }

        [Fact]
        public async Task GetData_WhenGet_ReturnCorrectDataList()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            var dataList = FakeHelper.CreateMany<MatchWinLoss>().ToList();

            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(FakeHelper.CreateMany<ProductInfo>().ToList()));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.Equal(dataList, result!.Data);
        }

        [Fact]
        public async Task GetData_EmptyReportData_ReturnEmptyLeagueLeague()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();

            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(new List<MatchWinLoss>());

            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(FakeHelper.CreateMany<ProductInfo>().ToList()));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.Empty(result!.LeagueList);
        }

        [Fact]
        public async Task GetData_NotEmptyReportData_ReturnLeagueLeague()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();

            var dataList = FakeHelper.CreateMany<MatchWinLoss>().ToList();

            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(FakeHelper.CreateMany<ProductInfo>().ToList()));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.NotEmpty(result!.LeagueList);
        }

        [Fact]
        public async Task GetData_NotIncludeOutrightParlayProducts_ReturnEmptySpecialBetTypeResult()
        {
            // Arrange
            var productOptions = FakeHelper.CreateMany<ProductOption>().ToList();

            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            metaModel.ProductOptions = productOptions;

            var dataList = FakeHelper.CreateMany<MatchWinLoss>().ToList();
            _matchWinLossService.Setup(x => x.GetDataList(metaModel)).ReturnsAsync(dataList);

            var outRightParayList = FakeHelper.CreateMany<MatchWinLoss>().ToList();
            _matchWinLossService.Setup(x => x.GetOutrightParlayList(metaModel)).ReturnsAsync(outRightParayList);

            _productService.Setup(x => x.GetSingleWalletProductAccess(
                _contextService.Object.LicenseeId,
                FunctionNames.MatchWinLoss)).Returns(new ProductAccess(FakeHelper.CreateMany<ProductInfo>().ToList()));

            using var controller = CreateController();

            // Action
            var response = await controller.GetData(metaModel) as OkObjectResult;
            var result = response!.Value as MatchWinLossResultModel;

            // Assert
            Assert.Empty(result!.SpecialBetTypeResult);
        }

        [Fact]
        public async Task ExportExcel_InvalidBaseCurrency_ReturnDefaultFile()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            metaModel.CurrencyId = -999;
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string>());

            var fileProvider = new Mock<IFileProvider>();
            var excel = new Mock<IMatchWinLossExcel>();
            var file = FakeHelper.CreateMany<byte>().ToArray();
            excel.Setup(x => x.Export(It.IsAny<string>(), It.IsAny<DataSet>(), It.IsAny<MatchWinLossMetaModel>(),
                It.IsAny<MatchWinLossExcelOption>())).Returns(file);

            using var controller = CreateController();

            // Action
            var response = await controller.ExportExcel(fileProvider.Object, excel.Object, metaModel) as FileContentResult;
            var result = response!.FileContents;

            // Assert
            Assert.NotEqual(file, result);
        }

        [Fact]
        public async Task ExportExcel_ValidBaseCurrency_ReturnFile()
        {
            // Arrange
            var metaModel = FakeHelper.Create<MatchWinLossMetaModel>();
            metaModel.CurrencyId = -999;
            _contextService.Setup(x => x.CurrencyList).Returns(new Dictionary<int, string> { { metaModel.CurrencyId, "Any" } });
            _productService
               .Setup(x => x.GetSingleWalletProductAccess(It.IsAny<int>(), It.IsAny<string>()))
               .Returns(FakeHelper.Create<ProductAccess>());

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = new Mock<IFileInfo>();
            fileProvider.Setup(x => x.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);

            var excel = new Mock<IMatchWinLossExcel>();
            var file = FakeHelper.CreateMany<byte>().ToArray();
            excel.Setup(x => x.Export(It.IsAny<string>(), It.IsAny<DataSet>(), It.IsAny<MatchWinLossMetaModel>(),
                It.IsAny<MatchWinLossExcelOption>())).Returns(file);

            using var controller = CreateController();

            // Action
            var response = await controller.ExportExcel(fileProvider.Object, excel.Object, metaModel) as FileContentResult;
            var result = response!.FileContents;

            // Assert
            Assert.Equal(file, result);
        }

        private MatchWinLossController CreateController()
        {
            var controller = new MatchWinLossController(
                _cacheRefAdapter.Object,
                _contextService.Object,
                _productService.Object,
                _baseCurrencyService.Object,
                _matchWinLossService.Object);

            return controller;
        }
    }
}
