namespace Fanex.Athena.Report.ByPlayer.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AutoFixture;
    using ByPlayer.Controllers;
    using ByPlayer.Excels.Interfaces;
    using Core.Services.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    public class CustomerSupportTaskControllerTest : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IContextService> _contextService;
        private readonly Mock<ICustomerSupportTaskExcel> _customerSupportTaskExcel;
        private readonly List<string> _filesToCleanup = new List<string>();

        public CustomerSupportTaskControllerTest()
        {
            _contextService = new Mock<IContextService>();
            _customerSupportTaskExcel = new Mock<ICustomerSupportTaskExcel>();
        }

        [Fact]
        public void Index_WhenCalled_ReturnsOkResultWithCurrencies()
        {
            // Arrange
            var currencies = new Dictionary<int, string>
            {
                { 1, "USD" },
                { 2, "EUR" },
                { 3, "GBP" }
            };

            _contextService.Setup(x => x.CurrencyList).Returns(currencies);

            var excels = new List<ICustomerSupportTaskExcel> { _customerSupportTaskExcel.Object };
            var controller = CreateController(excels);

            // Act
            var result = controller.Index(_contextService.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = okResult.Value;
            Assert.NotNull(resultValue);

            // Use reflection to check the currencies property
            var currenciesProperty = resultValue.GetType().GetProperty("currencies");
            Assert.NotNull(currenciesProperty);
            var currenciesArray = currenciesProperty.GetValue(resultValue) as KeyValuePair<int, string>[];
            Assert.NotNull(currenciesArray);
            Assert.Equal(3, currenciesArray.Length);
        }

        [Fact]
        public async Task ExportExcel_WhenNoHandlerFound_ReturnsEmptyFile()
        {
            // Arrange
            var metaModel = _fixture.Create<CustomerSupportTaskMetaModel>();
            _customerSupportTaskExcel.Setup(x => x.Should(metaModel)).Returns(false);

            var excels = new List<ICustomerSupportTaskExcel> { _customerSupportTaskExcel.Object };
            var controller = CreateController(excels);

            // Act
            var result = await controller.ExportExcel(metaModel);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Empty(fileResult.FileContents);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
        }

        [Fact]
        public async Task ExportExcel_WhenHandlerFound_ReturnsFileStream()
        {
            // Arrange
            var metaModel = _fixture.Create<CustomerSupportTaskMetaModel>();
            var tempFileName = Path.GetTempFileName();
            _filesToCleanup.Add(tempFileName);

            // Write some test content to the temp file
            await File.WriteAllTextAsync(tempFileName, "Test file content");

            _customerSupportTaskExcel.Setup(x => x.Should(metaModel)).Returns(true);
            _customerSupportTaskExcel.Setup(x => x.CreateZipFile(metaModel)).ReturnsAsync(tempFileName);

            var excels = new List<ICustomerSupportTaskExcel> { _customerSupportTaskExcel.Object };
            var controller = CreateController(excels);

            // Act
            var result = await controller.ExportExcel(metaModel);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", result.ContentType);
        }

        [Fact]
        public async Task ExportExcel_CallsCorrectHandler()
        {
            // Arrange
            var metaModel = _fixture.Create<CustomerSupportTaskMetaModel>();
            var tempFileName = Path.GetTempFileName();
            _filesToCleanup.Add(tempFileName);

            await File.WriteAllTextAsync(tempFileName, "Test content");

            _customerSupportTaskExcel.Setup(x => x.Should(metaModel)).Returns(true);
            _customerSupportTaskExcel.Setup(x => x.CreateZipFile(metaModel)).ReturnsAsync(tempFileName);

            var excels = new List<ICustomerSupportTaskExcel> { _customerSupportTaskExcel.Object };
            var controller = CreateController(excels);

            // Act
            await controller.ExportExcel(metaModel);

            // Assert
            _customerSupportTaskExcel.Verify(x => x.Should(metaModel), Times.Once);
            _customerSupportTaskExcel.Verify(x => x.CreateZipFile(metaModel), Times.Once);
        }

        [Fact]
        public async Task ExportExcel_WithMultipleHandlers_SelectsCorrectOne()
        {
            // Arrange
            var metaModel = _fixture.Create<CustomerSupportTaskMetaModel>();
            var tempFileName = Path.GetTempFileName();
            _filesToCleanup.Add(tempFileName);

            await File.WriteAllTextAsync(tempFileName, "Test content");

            var wrongHandler = new Mock<ICustomerSupportTaskExcel>();
            wrongHandler.Setup(x => x.Should(metaModel)).Returns(false);

            _customerSupportTaskExcel.Setup(x => x.Should(metaModel)).Returns(true);
            _customerSupportTaskExcel.Setup(x => x.CreateZipFile(metaModel)).ReturnsAsync(tempFileName);

            var excels = new List<ICustomerSupportTaskExcel> { wrongHandler.Object, _customerSupportTaskExcel.Object };
            var controller = CreateController(excels);

            // Act
            await controller.ExportExcel(metaModel);

            // Assert
            wrongHandler.Verify(x => x.Should(metaModel), Times.Once);
            wrongHandler.Verify(x => x.CreateZipFile(It.IsAny<CustomerSupportTaskMetaModel>()), Times.Never);
            _customerSupportTaskExcel.Verify(x => x.Should(metaModel), Times.Once);
            _customerSupportTaskExcel.Verify(x => x.CreateZipFile(metaModel), Times.Once);
        }

        private CustomerSupportTaskController CreateController(IEnumerable<ICustomerSupportTaskExcel> excels)
        {
            return new CustomerSupportTaskController(excels);
        }

        public void Dispose()
        {
            foreach (var file in _filesToCleanup)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore cleanup errors in tests
                    }
                }
            }
        }
    }
}