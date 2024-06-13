using System.Linq;
using AutoFixture;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Resources.Constants;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Models
{
    public class ProductAccessTest
    {
        private readonly Fixture _fixture = new();

        [Fact]
        public void SelectProductOptions_WhenCallWithPredicate_ReturnProductOptions()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);
            var indicateProduct = productAccess.ProductList[1];

            var result = productAccess.SelectProductOptions(x => x.ProductCode == indicateProduct.ProductCode);

            Assert.Equal(1, result.Count);
            Assert.Equal(indicateProduct.ProductCode, result[0].Code);
            Assert.Equal(indicateProduct.ProductName, result[0].Name);
            Assert.True(result[0].Status);
        }

        [Fact]
        public void SelectProductOptions_WhenCall_ReturnProductOptions()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);

            var result = productAccess.SelectProductOptions();

            Assert.Equal(productAccess.ProductList.Count, result.Count);
            Assert.Equal(productAccess.ProductList[0].ProductCode, result[0].Code);
            Assert.Equal(productAccess.ProductList[0].ProductName, result[0].Name);
            Assert.True(result[0].Status);
            Assert.Equal(productAccess.ProductList[1].ProductCode, result[1].Code);
            Assert.Equal(productAccess.ProductList[1].ProductName, result[1].Name);
            Assert.True(result[1].Status);
            Assert.Equal(productAccess.ProductList[2].ProductCode, result[2].Code);
            Assert.Equal(productAccess.ProductList[2].ProductName, result[2].Name);
            Assert.True(result[2].Status);
        }

        [Fact]
        public void IsAllow_NotHaveProduct_ReturnFalse()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllow("Any");

            Assert.False(result);
        }

        [Fact]
        public void IsAllow_HaveProduct_ReturnTrue()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            products[0].ProductCode = Product.KN;
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllow(Product.KN);

            Assert.True(result);
        }

        [Fact]
        public void IsAllowKn_HaveKn_ReturnTrue()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            products[0].ProductCode = Product.KN;
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllowKn;

            Assert.True(result);
        }

        [Fact]
        public void IsAllowKn_NoKn_ReturnFalse()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllowKn;

            Assert.False(result);
        }

        [Fact]
        public void IsAllowSb_HaveSb_ReturnTrue()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            products[0].ProductCode = Product.SB;
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllowSb;

            Assert.True(result);
        }

        [Fact]
        public void IsAllowSb_NoSb_ReturnFalse()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllowSb;

            Assert.False(result);
        }

        [Fact]
        public void IsAllowVs_HaveVs_ReturnTrue()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            products[0].ProductCode = Product.VS;
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllowVs;

            Assert.True(result);
        }

        [Fact]
        public void IsAllowVs_NoVs_ReturnFalse()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllowVs;

            Assert.False(result);
        }

        [Fact]
        public void IsAllow_HaveCode_ReturnTrue()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            products[0].ProductCode = Product.VS;
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllow(products[0].ProductCode);

            Assert.True(result);
        }

        [Fact]
        public void IsAllow_NoCode_ReturnFalse()
        {
            var products = _fixture.CreateMany<ProductInfo>().ToList();
            var productAccess = new ProductAccess(products);

            var result = productAccess.IsAllow("Any");

            Assert.False(result);
        }
    }
}
