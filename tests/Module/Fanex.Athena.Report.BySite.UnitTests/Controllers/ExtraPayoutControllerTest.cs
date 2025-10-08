namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.DTOs;
    using BySite.Excels.Interfaces;
    using BySite.MetaModels;
    using BySite.Services.Interfaces;
    using Common.Extensions;
    using Common.Model.ViewModel;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using BySite.Models;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    public class ExtraPayoutControllerTest
    {
        private const string PhysicalPath = "PhysicalPath";

        private readonly Mock<IContextService> _contextService;

        private readonly Fixture _fixture = new();

        private readonly Mock<IProductService> _productService;

        public ExtraPayoutControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _productService = new Mock<IProductService>();
        }

        [Fact]
        public async Task GetData_HasData_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                                       .With(x => x.DataList,
                                            new SortableList<ExtraPayout>
                                                { new() { ProductCode = "SB", Turnover = 200, }, })
                                       .Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = expectedData.DataList.ToList(),
                TotalCurrency = expectedData.TotalDataList.ToList(),
                TotalSummary = expectedData.TotalData,
                expectedData.TotalRows
            });

            // Action
            var result =
                JsonConvert.SerializeObject((await controller.GetData(metaModel, service.Object) as OkObjectResult)
                   .Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayout>>().Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new ExtraPayoutResultModel
            {
                Data = new List<ExtraPayout>(),
                TotalCurrency = new List<ExtraPayout>(),
                TotalSummary = new ExtraPayout()
            });

            // Action
            var result =
                JsonConvert.SerializeObject((await controller.GetData(metaModel, service.Object) as OkObjectResult)
                   .Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetDetailData_HasData_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                                       .With(x => x.DataList,
                                            new SortableList<ExtraPayoutDetail>
                                                { new() { PromotionType = 99, Turnover = 200, }, })
                                       .Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = expectedData.DataList.ToList(),
                TotalSummary = expectedData.TotalData
            });

            // Action
            var result =
                JsonConvert.SerializeObject(
                    (await controller.GetDetailData(metaModel, service.Object) as OkObjectResult).Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetDetailData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>().Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                Data = new List<ExtraPayoutDetail>(),
                TotalSummary = new ExtraPayoutDetail()
            });

            // Action
            var result =
                JsonConvert.SerializeObject(
                    (await controller.GetDetailData(metaModel, service.Object) as OkObjectResult).Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayout>>()
                                       .With(x => x.DataList,
                                            new SortableList<ExtraPayout>
                                                { new() { ProductCode = "SB", Turnover = 200, }, })
                                       .Create();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/ExtraPayout.xls")).Returns(fileInfo);

            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<IExtraPayoutExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = BuildController();

            // Assert
            var result =
                await controller.ExportExcel(service.Object, fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }

        [Fact]
        public async Task ExportDetailExcel_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutDetailMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<ExtraPayoutDetail>>()
                                       .With(x => x.DataList,
                                            new SortableList<ExtraPayoutDetail>
                                                { new() { PromotionType = 99, Turnover = 200, }, })
                                       .Create();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = Mock.Of<IFileInfo>(x => x.PhysicalPath == PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/ExtraPayoutDetail.xls")).Returns(fileInfo);

            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<IExtraPayoutDetailExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = BuildController();

            // Assert
            var result =
                await controller.ExportDetailExcel(service.Object, fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
        }

        [Fact]
        public async Task GetOddsBoostBonusDetailData_HasData_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<OddsBoostBonusDetail>>()
                                       .With(x => x.DataList,
                                            new SortableList<OddsBoostBonusDetail>
                                                { new() { ExternalId = "qatest03", CustomerAmount = 4.08m }, })
                                       .Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetOddsBoostBonusDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            // Act
            var result = await controller.GetOddsBoostBonusDetailData(metaModel, service.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultModel = Assert.IsType<OddsBoostBonusDetailResultModel>(okResult.Value);
            Assert.Single(resultModel.Data);
            Assert.Equal("qatest03", resultModel.Data.First().ExternalId);
            Assert.Equal(4.08m, resultModel.Data.First().CustomerAmount);
        }

        [Fact]
        public async Task GetOddsBoostBonusDetailData_NoData_ReturnEmptyResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Build<SortableReportResultModel<OddsBoostBonusDetail>>()
                                       .With(x => x.DataList, new SortableList<OddsBoostBonusDetail>())
                                       .Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetOddsBoostBonusDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            // Act
            var result = await controller.GetOddsBoostBonusDetailData(metaModel, service.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultModel = Assert.IsType<OddsBoostBonusDetailResultModel>(okResult.Value);
            Assert.Empty(resultModel.Data);
            Assert.NotNull(resultModel.TotalSummary);
        }

        [Fact]
        public async Task GetOddsBoostBonusDetailData_WithTotalSummary_ReturnCorrectTotalSummary()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var totalSummary = new OddsBoostBonusDetail { CustomerAmount = 59.63m };
            var expectedData = _fixture.Build<SortableReportResultModel<OddsBoostBonusDetail>>()
                                       .With(x => x.DataList,
                                            new SortableList<OddsBoostBonusDetail>
                                            {
                                                new() { CustomerAmount = 4.08m },
                                                new() { CustomerAmount = 20.83m },
                                                new() { CustomerAmount = 34.72m }
                                            })
                                       .With(x => x.TotalData, totalSummary)
                                       .Create();

            var controller = BuildController();
            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetOddsBoostBonusDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            // Act
            var result = await controller.GetOddsBoostBonusDetailData(metaModel, service.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultModel = Assert.IsType<OddsBoostBonusDetailResultModel>(okResult.Value);
            Assert.Equal(3, resultModel.Data.Count);
            Assert.Equal(59.63m, resultModel.TotalSummary.CustomerAmount);
        }

        [Fact]
        public async Task ExportOddsBoostBonusDetailExcel_WhenCall_ReturnFileContent()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<OddsBoostBonusDetail>>();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.PhysicalPath).Returns(PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/OddsBoostBonusDetail.xls")).Returns(fileInfo.Object);

            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetOddsBoostBonusDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<IOddsBoostBonusDetailExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = BuildController();

            // Act
            var result = await controller.ExportOddsBoostBonusDetailExcel(service.Object, fileProvider.Object, excelService.Object, metaModel);

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(expectedFileContent, result.FileContents);
            Assert.True(metaModel.IsExportExcel);
        }

        [Fact]
        public async Task ExportOddsBoostBonusDetailExcel_VerifyServiceCalls_CallsCorrectMethods()
        {
            // Arrange
            var metaModel = _fixture.Create<ExtraPayoutMetaModel>();
            var expectedData = _fixture.Create<SortableReportResultModel<OddsBoostBonusDetail>>();

            var fileProvider = new Mock<IFileProvider>();
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.PhysicalPath).Returns(PhysicalPath);
            fileProvider.Setup(f => f.GetFileInfo("/ExcelTemplate/OddsBoostBonusDetail.xls")).Returns(fileInfo.Object);

            var service = new Mock<IExtraPayoutService>();
            service.Setup(r => r.GetOddsBoostBonusDetailDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var excelService = new Mock<IOddsBoostBonusDetailExcel>();
            var expectedFileContent = _fixture.Create<byte[]>();
            excelService.Setup(e => e.Export(PhysicalPath, expectedData, metaModel)).Returns(expectedFileContent);

            var controller = BuildController();

            // Act
            await controller.ExportOddsBoostBonusDetailExcel(service.Object, fileProvider.Object, excelService.Object, metaModel);

            // Assert
            service.Verify(s => s.GetOddsBoostBonusDetailDataAsync(metaModel), Times.Once);
            excelService.Verify(e => e.Export(PhysicalPath, expectedData, metaModel), Times.Once);
            fileProvider.Verify(f => f.GetFileInfo("/ExcelTemplate/OddsBoostBonusDetail.xls"), Times.Once);
        }

        private ExtraPayoutController BuildController() =>
            new ExtraPayoutController(_contextService.Object, _productService.Object);
    }
}