using System.Collections.Generic;
using System.Linq;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Moq;
using NPOI.SS.UserModel;
using Xunit;

namespace Fanex.Athena.Report.Common.UnitTests.Excel
{
    public class ExcelHelperTest
    {
        [Fact]
        public void SetTextRowHeader_WhenCall_CellIsSetValueCorrectly()
        {
            // Arrange
            var headerText = new[]
            {
                "Header1",
                "Header2",
            };

            var cell = new Mock<ICell>();
            var row = new Mock<IRow>();
            row.Setup(r => r.Cells).Returns(Enumerable.Repeat(cell.Object, 2).ToList());

            // Action
            ExcelHelper.SetTextRowHeader(row.Object, headerText);

            // Assert
            cell.Verify(c => c.SetCellValue(headerText[0]), Times.Once);
            cell.Verify(c => c.SetCellValue(headerText[1]), Times.Once);
        }

        [Theory]
        [InlineData(100, 10, 10)]
        [InlineData(101, 10, 11)]
        [InlineData(95, 10, 10)]
        [InlineData(0, 10, 0)]
        [InlineData(1, 10, 1)]
        [InlineData(50, 25, 2)]
        public void GetNumberOfSheets_WhenCall_ReturnCorrectTotalSheet(int totalRow, int pageSize, int expected)
        {
            // Action
            var result = ExcelHelper.GetNumberOfSheets(totalRow, pageSize);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetTitleCurrencyName_WhenConvertIsTrue_ReturnsBaseCurrencyFormat()
        {
            // Arrange
            var convertingOption = new Mock<IConvertingOption>();
            convertingOption.Setup(x => x.IsConvert).Returns(true);
            convertingOption.Setup(x => x.BaseCurrency).Returns("usd");

            // Act
            var result = ExcelHelper.GetTitleCurrencyName(convertingOption.Object);

            // Assert
            Assert.Contains("USD", result);
        }

        [Fact]
        public void GetTitleCurrencyName_WhenConvertIsFalse_ReturnsCurrencyNameFormat()
        {
            // Arrange
            var convertingOption = new Mock<IConvertingOption>();
            convertingOption.Setup(x => x.IsConvert).Returns(false);
            convertingOption.Setup(x => x.CurrencyName).Returns("EUR");

            // Act
            var result = ExcelHelper.GetTitleCurrencyName(convertingOption.Object);

            // Assert
            Assert.Contains("EUR", result);
        }

        [Theory]
        [InlineData("USD", "EUR", "EUR", "USD")]
        [InlineData("", "GBP", "GBP", "GBP")]
        [InlineData(null, "JPY", "JPY", "JPY")]
        public void GetTitleBaseCurrency_WithVariousInputs_ReturnsCorrectFormat(string baseCurrency, string currencyName, string expectedCurrency, string expectedBase)
        {
            // Act
            var result = ExcelHelper.GetTitleBaseCurrency(baseCurrency, currencyName);

            // Assert
            Assert.Contains($"Currency: {expectedCurrency}", result);
            Assert.Contains($"Base Currency: {expectedBase}", result);
        }

        [Fact]
        public void GetLicenseeWlProduct_WithActiveProducts_ReturnsFormattedString()
        {
            // Arrange
            var productOptions = new List<ProductOption>
            {
                new() { Code = "SB", Status = true },
                new() { Code = "LC", Status = true },
                new() { Code = "AB", Status = false }
            };

            // Act
            var result = ExcelHelper.GetLicenseeWlProduct(productOptions);

            // Assert
            Assert.Contains("SB", result);
            Assert.Contains("LC", result);
            Assert.DoesNotContain("AB", result);
        }

        [Fact]
        public void GetLicenseeWlProduct_WithNoActiveProducts_ReturnsEmptyString()
        {
            // Arrange
            var productOptions = new List<ProductOption>
            {
                new() { Code = "SB", Status = false },
                new() { Code = "LC", Status = false }
            };

            // Act
            var result = ExcelHelper.GetLicenseeWlProduct(productOptions);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetLicenseeWlProduct_WithCaptionAndNoActiveProducts_ReturnsCaption()
        {
            // Arrange
            var productOptions = new List<ProductOption>
            {
                new() { Code = "SB", Status = false }
            };
            var caption = "Test Caption";

            // Act
            var result = ExcelHelper.GetLicenseeWlProduct(productOptions, caption);

            // Assert
            Assert.Contains(caption, result);
        }

        [Fact]
        public void GetLicenseeWlProduct_WithCaptionAndActiveProducts_ReturnsCaptionWithProducts()
        {
            // Arrange
            var productOptions = new List<ProductOption>
            {
                new() { Code = "SB", Status = true }
            };
            var caption = "Test Caption";

            // Act
            var result = ExcelHelper.GetLicenseeWlProduct(productOptions, caption);

            // Assert
            Assert.Contains(caption, result);
            Assert.Contains("SB", result);
        }

        [Fact]
        public void ExportEmptyFile_WhenCalled_ReturnsNonEmptyByteArray()
        {
            // Act
            var result = ExcelHelper.ExportEmptyFile();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void SetTextRowHeader_WithEmptyHeaderList_DoesNotThrow()
        {
            // Arrange
            var headerText = new string[0];
            var row = new Mock<IRow>();
            row.Setup(r => r.Cells).Returns(new List<ICell>());

            // Act & Assert
            var exception = Record.Exception(() => ExcelHelper.SetTextRowHeader(row.Object, headerText));
            Assert.Null(exception);
        }
    }
}