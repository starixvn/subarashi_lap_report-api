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
