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
    using Models;
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

        private ExtraPayoutController BuildController() =>
            new ExtraPayoutController(_contextService.Object, _productService.Object);
    }
}
