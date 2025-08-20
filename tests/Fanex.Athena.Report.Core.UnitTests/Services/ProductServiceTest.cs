namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.ApiClients.Abstraction;
    using Core.Models.DTOs;
    using Core.Services;
    using Moq;
    using Report.UnitTests.Common;
    using Xunit;

    public class ProductServiceTest
    {
        private readonly Mock<ICommonApiClient> _commonApiClient;

        public ProductServiceTest()
        {
            _commonApiClient = new Mock<ICommonApiClient>();
        }

        private ProductService GetService() => new(_commonApiClient.Object);

        [Fact]
        public void GetProductList_ShouldReturnProductList()
        {
            // Arrange
            var licenseeId = FakeHelper.Create<int>();
            var functionConfigKey = FakeHelper.Create<string>();
            var expectedProducts = new List<ProductInfo> { new() { ProductCode = "SB" } };

            _commonApiClient.Setup(x => x.GetAsync<IList<ProductInfo>>(
                "api/productaccess",
                It.Is<object>(o => o.GetType().GetProperty("licenseeId").GetValue(o).Equals(licenseeId) &&
                                  o.GetType().GetProperty("functionKey").GetValue(o).Equals(functionConfigKey)),
                It.IsAny<int>()))
                .ReturnsAsync(expectedProducts);

            var service = GetService();

            // Act
            var result = service.GetProductList(licenseeId, functionConfigKey);

            // Assert
            Assert.Equal(expectedProducts, result);
            _commonApiClient.Verify(x => x.GetAsync<IList<ProductInfo>>(
                "api/productaccess",
                It.Is<object>(o => o.GetType().GetProperty("licenseeId").GetValue(o).Equals(licenseeId) &&
                                  o.GetType().GetProperty("functionKey").GetValue(o).Equals(functionConfigKey)),
                It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void GetProductName_ShouldReturnProductName()
        {
            // Arrange
            var productCode = FakeHelper.Create<string>();
            var expectedName = FakeHelper.Create<string>();

            _commonApiClient.Setup(x => x.GetAsync<string>(
                "api/productresource",
                It.Is<object>(o => o.GetType().GetProperty("productCode").GetValue(o).Equals(productCode)),
                It.IsAny<int>()))
                .ReturnsAsync(expectedName);

            var service = GetService();

            // Act
            var result = service.GetProductName(productCode);

            // Assert
            Assert.Equal(expectedName, result);
            _commonApiClient.Verify(x => x.GetAsync<string>(
                "api/productresource",
                It.Is<object>(o => o.GetType().GetProperty("productCode").GetValue(o).Equals(productCode)),
                It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetProductSetting_ShouldReturnProductSetting()
        {
            // Arrange
            var expectedSetting = new ProductSetting();

            _commonApiClient.Setup(x => x.GetAsync<ProductSetting>(
                "api/productSetting",
                null,
                It.IsAny<int>()))
                .ReturnsAsync(expectedSetting);

            var service = GetService();

            // Act
            var result = await service.GetProductSetting();

            // Assert
            Assert.Equal(expectedSetting, result);
            _commonApiClient.Verify(x => x.GetAsync<ProductSetting>(
                "api/productSetting",
                null,
                It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void GetProductAccess_ShouldReturnProductAccess()
        {
            // Arrange
            var licenseeId = FakeHelper.Create<int>();
            var functionConfigKey = FakeHelper.Create<string>();
            var products = new List<ProductInfo> { new() { ProductCode = "SB" } };

            _commonApiClient.Setup(x => x.GetAsync<IList<ProductInfo>>(
                "api/productaccess",
                It.Is<object>(o => o.GetType().GetProperty("licenseeId").GetValue(o).Equals(licenseeId) &&
                                  o.GetType().GetProperty("functionKey").GetValue(o).Equals(functionConfigKey)),
                It.IsAny<int>()))
                .ReturnsAsync(products);

            var service = GetService();

            // Act
            var result = service.GetProductAccess(licenseeId, functionConfigKey);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(products, result.ProductList);
        }

        [Fact]
        public void GetSingleWalletProductAccess_ShouldReturnSingleWalletProductAccess()
        {
            // Arrange
            var licenseeId = FakeHelper.Create<int>();
            var functionConfigKey = FakeHelper.Create<string>();
            var products = new List<ProductInfo>
            {
                new() { ProductCode = "SB", IsOfferedSingleWallet = true },
                new() { ProductCode = "VS", IsOfferedSingleWallet = false }
            };

            _commonApiClient.Setup(x => x.GetAsync<IList<ProductInfo>>(
                "api/productaccess",
                It.Is<object>(o => o.GetType().GetProperty("licenseeId").GetValue(o).Equals(licenseeId) &&
                                  o.GetType().GetProperty("functionKey").GetValue(o).Equals(functionConfigKey)),
                It.IsAny<int>()))
                .ReturnsAsync(products);

            var service = GetService();

            // Act
            var result = service.GetSingleWalletProductAccess(licenseeId, functionConfigKey);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.ProductList);
            Assert.Equal("SB", result.ProductList.First().ProductCode);
        }

        [Theory]
        [InlineData(null, "SB", "SB")]
        [InlineData(new[] { "VS", "NG" }, "SB", "SB,VS,NG")]
        [InlineData(new[] { "VS", "NG" }, "VS", "VS")]
        public void GetAllowedSubProductCodeList_ShouldReturnCorrectProductCodes(string[] offeredSubProductList, string productCodes, string expected)
        {
            // Arrange
            var service = GetService();

            // Act
            var result = service.GetAllowedSubProductCodeList(offeredSubProductList, productCodes);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsOfferCashOutProduct_ShouldReturnTrue_WhenCashOutProductExists()
        {
            // Arrange
            var licenseeId = FakeHelper.Create<int>();
            var products = new List<ProductInfo>
            {
                new() { ProductCode = "SB" },
                new() { ProductCode = "CO" }
            };

            _commonApiClient.Setup(x => x.GetAsync<IList<ProductInfo>>(
                "api/productaccess",
                It.Is<object>(o => o.GetType().GetProperty("licenseeId").GetValue(o).Equals(licenseeId)),
                It.IsAny<int>()))
                .ReturnsAsync(products);

            var service = GetService();

            // Act
            var result = service.IsOfferCashOutProduct(licenseeId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsOfferCashOutProduct_ShouldReturnFalse_WhenCashOutProductDoesNotExist()
        {
            // Arrange
            var licenseeId = FakeHelper.Create<int>();
            var products = new List<ProductInfo>
            {
                new() { ProductCode = "SB" },
                new() { ProductCode = "VS" }
            };

            _commonApiClient.Setup(x => x.GetAsync<IList<ProductInfo>>(
                "api/productaccess",
                It.Is<object>(o => o.GetType().GetProperty("licenseeId").GetValue(o).Equals(licenseeId)),
                It.IsAny<int>()))
                .ReturnsAsync(products);

            var service = GetService();

            // Act
            var result = service.IsOfferCashOutProduct(licenseeId);

            // Assert
            Assert.False(result);
        }
    }
}