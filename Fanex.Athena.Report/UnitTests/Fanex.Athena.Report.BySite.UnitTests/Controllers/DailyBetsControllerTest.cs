using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.Controllers
{
    public class DailyBetsControllerTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task Index_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var expectedData = _fixture.Create<ResultModel<AuditTicket>>();
            var service = new Mock<IDailyBetsService>();
            service.Setup(s => s.GetDataAsync(metaModel)).Returns(Task.FromResult(expectedData));

            var expectedResult = JsonConvert.SerializeObject(new
            {
                expectedData.DataList,
                Total = expectedData.TotalData,
                expectedData.TotalRows
            });

            var controller = new DailyBetsController();

            // Action
            var result = JsonConvert.SerializeObject(await controller.Index(metaModel, service.Object));

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Export_WhenCall_ReturnCorrectFileResult()
        {
            // Arrange
            var metaModel = _fixture.Create<DailyBetsMetaModel>();
            var fileName = CreateFile();
            var excelService = new Mock<IDailyBetsExcel>();
            excelService.Setup(s => s.Export(metaModel)).Returns(Task.FromResult(fileName));

            var controller = new DailyBetsController();

            // Action
            var result = await controller.Export(metaModel, excelService.Object) as FileStreamResult;

            // Assert
            Assert.Equal("application/octet-stream", result.ContentType);

            Assert.True(File.Exists(fileName));
            result.FileStream.Dispose();
            Assert.False(File.Exists(fileName));
        }

        private string CreateFile()
        {
            var rootPath = Directory.GetCurrentDirectory();
            var fileName = Path.Combine(rootPath, $"{Guid.NewGuid()}.csv");
            File.Create(fileName).Dispose();

            return fileName;
        }
    }
}