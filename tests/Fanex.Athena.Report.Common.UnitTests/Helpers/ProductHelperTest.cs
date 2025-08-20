namespace Fanex.Athena.Report.Common.UnitTests.Helpers
{
    using Common.Helpers;
    using Constants;
    using Xunit;

    public class ProductHelperTest
    {
        [Theory]
        [InlineData(81)]
        [InlineData(119)]
        [InlineData(100)]
        [InlineData(8101)]
        [InlineData(8120)]
        [InlineData(8112)]
        [InlineData(3601)]
        [InlineData(3610)]
        [InlineData(3606)]
        public void GetProductId_InNumberRange_ReturnNumberGame(int bettype)
        {
            // Arrange && Action
            var productCode = "AB";
            var result = ProductHelper.GetProductId(bettype, productCode);

            // Assert
            Assert.Equal(ProductTypes.NumberGame, result);
        }

        [Theory]
        [InlineData(1201)]
        [InlineData(1299)]
        [InlineData(1256)]
        public void GetProductId_InVirtualSportsRange_ReturnVirtualSports(int bettype)
        {
            // Arrange && Action
            var productCode = "AB";
            var result = ProductHelper.GetProductId(bettype, productCode);

            // Assert
            Assert.Equal(ProductTypes.VirtualSports, result);
        }

        [Theory]
        [InlineData(2700)]
        [InlineData(2899)]
        [InlineData(2768)]
        public void GetProductId_InVirtualSports2Range_ReturnVirtualSports2(int bettype)
        {
            // Arrange && Action
            var productCode = "AB";
            var result = ProductHelper.GetProductId(bettype, productCode);

            // Assert
            Assert.Equal(ProductTypes.VirtualSports, result);
        }
    }
}
