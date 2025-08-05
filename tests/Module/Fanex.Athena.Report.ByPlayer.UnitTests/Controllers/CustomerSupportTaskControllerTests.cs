using Fanex.Athena.Report.ByPlayer.Controllers;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    public class CustomerSupportTaskControllerTests
    {
        private readonly Mock<IContextService> _mockContextService;
        private readonly Mock<ICustomerSupportTaskExcel> _mockCustomerSupportTaskExcel;
        private readonly List<ICustomerSupportTaskExcel> _customerSupportTaskExcels;
        private readonly CustomerSupportTaskController _controller;

        public CustomerSupportTaskControllerTests()
        {
            _mockContextService = new Mock<IContextService>();
            _mockCustomerSupportTaskExcel = new Mock<ICustomerSupportTaskExcel>();
            _customerSupportTaskExcels = new List<ICustomerSupportTaskExcel> { _mockCustomerSupportTaskExcel.Object };
            _controller = new CustomerSupportTaskController(_customerSupportTaskExcels);
        }

        [Fact]
        public void Index_ReturnsOkResultWithCurrencies()
        {
            var currencyList = new Dictionary<int, string>
            {
                { 1, "USD" },
                { 2, "EUR" }
            };
            _mockContextService.Setup(c => c.CurrencyList).Returns(currencyList);

            var result = _controller.Index(_mockContextService.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var jsonString = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var jsonElement = System.Text.Json.JsonDocument.Parse(jsonString).RootElement;
            var currenciesProperty = jsonElement.GetProperty("currencies");
            Assert.Equal(2, currenciesProperty.GetArrayLength());
        }

        [Fact]
        public void Index_WithEmptyCurrencyList_ReturnsOkResultWithEmptyArray()
        {
            var currencyList = new Dictionary<int, string>();
            _mockContextService.Setup(c => c.CurrencyList).Returns(currencyList);

            var result = _controller.Index(_mockContextService.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var jsonString = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var jsonElement = System.Text.Json.JsonDocument.Parse(jsonString).RootElement;
            var currenciesProperty = jsonElement.GetProperty("currencies");
            Assert.Equal(0, currenciesProperty.GetArrayLength());
        }

        [Fact]
        public async Task ExportExcel_WithValidMetaModelAndHandler_ReturnsFileResult()
        {
            var metaModel = new CustomerSupportTaskMetaModel
            {
                SupportTask = 1,
                CurrencyIdList = "1,2",
                AgentListXml = "<agents></agents>"
            };

            var testFileName = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(testFileName, "test content");

                _mockCustomerSupportTaskExcel.Setup(e => e.Should(metaModel)).Returns(true);
                _mockCustomerSupportTaskExcel.Setup(e => e.CreateZipFile(metaModel)).ReturnsAsync(testFileName);

                var result = await _controller.ExportExcel(metaModel);

                Assert.IsType<FileStreamResult>(result);
                Assert.Equal("application/octet-stream", result.ContentType);
            }
            finally
            {
                if (File.Exists(testFileName))
                {
                    try { File.Delete(testFileName); } catch { }
                }
            }
        }

        [Fact]
        public async Task ExportExcel_WithNoMatchingHandler_ReturnsEmptyFileResult()
        {
            var metaModel = new CustomerSupportTaskMetaModel
            {
                SupportTask = 1,
                CurrencyIdList = "1,2",
                AgentListXml = "<agents></agents>"
            };

            _mockCustomerSupportTaskExcel.Setup(e => e.Should(metaModel)).Returns(false);

            var result = await _controller.ExportExcel(metaModel);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Empty(fileResult.FileContents);
        }

        [Fact]
        public async Task ExportExcel_WithMultipleHandlersFirstMatches_ReturnsFileFromFirstHandler()
        {
            var metaModel = new CustomerSupportTaskMetaModel
            {
                SupportTask = 1,
                CurrencyIdList = "1,2",
                AgentListXml = "<agents></agents>"
            };

            var mockSecondHandler = new Mock<ICustomerSupportTaskExcel>();
            var handlersWithMultiple = new List<ICustomerSupportTaskExcel>
            {
                _mockCustomerSupportTaskExcel.Object,
                mockSecondHandler.Object
            };
            var controllerWithMultiple = new CustomerSupportTaskController(handlersWithMultiple);

            var testFileName = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(testFileName, "test content");

                _mockCustomerSupportTaskExcel.Setup(e => e.Should(metaModel)).Returns(true);
                _mockCustomerSupportTaskExcel.Setup(e => e.CreateZipFile(metaModel)).ReturnsAsync(testFileName);
                mockSecondHandler.Setup(e => e.Should(metaModel)).Returns(true);

                var result = await controllerWithMultiple.ExportExcel(metaModel);

                Assert.IsType<FileStreamResult>(result);
                _mockCustomerSupportTaskExcel.Verify(e => e.CreateZipFile(metaModel), Times.Once);
                mockSecondHandler.Verify(e => e.CreateZipFile(It.IsAny<CustomerSupportTaskMetaModel>()), Times.Never);
            }
            finally
            {
                if (File.Exists(testFileName))
                {
                    try { File.Delete(testFileName); } catch { }
                }
            }
        }
    }
}