using System;
using System.IO;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerBalanceControllerTest
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
        public async Task ExportExcel_WhenCall_ReturnsCorrectFile()
        {
            // Arrange
            const string expectedContentType = "application/octet-stream";
            var metaModel = new CustomerBalanceMetaModel();
            var excel = new Mock<ICustomerBalanceExcel>();
            var fileName = $"D:\\{Guid.NewGuid()}.dat";
            var filestream = File.Create(fileName);
            filestream.Close();
            excel.Setup(e => e.Export(metaModel)).Returns(Task.FromResult(fileName));
            var controller = new CustomerBalanceController();
            var expectedFileContent = File.ReadAllBytes(fileName);

            // Action
            var result = await controller.ExportExcel(metaModel, excel.Object) as FileStreamResult;

            // Assert
            Assert.Equal(expectedContentType, result.ContentType);
            Assert.Equal(expectedFileContent, GetStreamContent(result.FileStream));
        }

        private byte[] GetStreamContent(Stream fileStream)
        {
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes);
            return bytes;
        }
    }
}