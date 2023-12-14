using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class WeeklyLiveMatchControllerTest
    {
        private readonly WeeklyLiveMatchController _controller;
        private readonly Fixture _fixture = new();

        private readonly Mock<IWeeklyLiveMatchService> _service;

        public WeeklyLiveMatchControllerTest()
        {
            _service = new Mock<IWeeklyLiveMatchService>();
            _controller = new WeeklyLiveMatchController();
        }

        [Fact]
        public async Task Index_NoDataReturned_ReturnEmptyData()
        {
            // Arrange
            var metaModel = _fixture.Create<WeeklyLiveMatchMetaModel>();
            var expected = _fixture.Build<WeeklyLiveMatchResultModel>()
                                   .With(x => x.DataList, Enumerable.Empty<WeeklyLiveMatch>()).Create();
            _service.Setup(x => x.GetData(metaModel)).Returns(Task.FromResult(expected));

            // Action
            var response = await _controller.Index(_service.Object, metaModel) as OkObjectResult;
            var result = response.Value as WeeklyLiveMatchResponseModel;

            // Assert
            Assert.Empty(result.WeekHeaders);
            Assert.Equal(0, result.NumberOfWeeks);
            Assert.Empty(result.Data);
            Assert.Empty(result.WeeksTotalData);
        }

        [Fact]
        public async Task Index_ValidModel_ReturnCorrectData()
        {
            // Arrange
            var metaModel = _fixture.Create<WeeklyLiveMatchMetaModel>();
            var expected = _fixture.Create<WeeklyLiveMatchResultModel>();
            _service.Setup(x => x.GetData(metaModel)).Returns(Task.FromResult(expected));

            var weekHeaders = _fixture.CreateMany<string>().ToList();
            _service.Setup(x => x.GetWeekHeaders(metaModel.FromDate, metaModel.ToDate, expected.NumberOfWeeks))
                    .Returns(weekHeaders);

            // Action
            var response = await _controller.Index(_service.Object, metaModel) as OkObjectResult;
            var result = response.Value as WeeklyLiveMatchResponseModel;

            // Assert
            Assert.Equal(weekHeaders, result.WeekHeaders);
            Assert.Equal(expected.NumberOfWeeks, result.NumberOfWeeks);
            Assert.Equal(expected.DataList.Count(), result.Data.Length);
            Assert.Equal(expected.WeeksTotalData, result.WeeksTotalData);
        }

        [Fact]
        public async Task ExportExcel_WhenCall_ReturnFile()
        {
            // Arrange
            var metaModel = _fixture.Create<WeeklyLiveMatchMetaModel>();
            var data = _fixture.Create<WeeklyLiveMatchResultModel>();
            _service.Setup(x => x.GetData(metaModel)).Returns(Task.FromResult(data));

            var fileInfo = new Mock<IFileInfo>();
            var template = _fixture.Create<string>();
            fileInfo.Setup(x => x.PhysicalPath).Returns(template);

            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(x => x.GetFileInfo(It.IsAny<string>())).Returns(fileInfo.Object);

            var expectedFile = _fixture.CreateMany<byte>().ToArray();

            var excel = new Mock<IWeeklyLiveMatchExcel>();
            excel.Setup(x => x.Export(template, data, It.IsAny<WeeklyLiveMatchOptions>())).Returns(expectedFile);

            // Action
            var result = await _controller.ExportExcel(fileProvider.Object, excel.Object, _service.Object, metaModel);

            // Assert
            Assert.Equal(expectedFile, result.FileContents);
        }
    }
}
