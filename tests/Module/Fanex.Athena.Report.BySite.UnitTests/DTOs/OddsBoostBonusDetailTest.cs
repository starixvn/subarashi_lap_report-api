namespace Fanex.Athena.Report.BySite.UnitTests.DTOs
{
    using System;
    using AutoFixture;
    using BySite.DTOs;
    using Xunit;

    public class OddsBoostBonusDetailTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void OddsBoostBonusDetail_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedDate = _fixture.Create<DateTime>();
            var expectedDateString = _fixture.Create<string>();
            var expectedExternalId = _fixture.Create<string>();
            var expectedSystemId = _fixture.Create<string>();
            var expectedRefNo = _fixture.Create<string>();
            var expectedProductCode = _fixture.Create<string>();
            var expectedProductName = _fixture.Create<string>();
            var expectedPromotionType = _fixture.Create<string>();
            var expectedCustomerAmount = _fixture.Create<decimal>();

            // Act
            var dto = new OddsBoostBonusDetail
            {
                Date = expectedDate,
                DateString = expectedDateString,
                ExternalId = expectedExternalId,
                SystemId = expectedSystemId,
                RefNo = expectedRefNo,
                ProductCode = expectedProductCode,
                ProductName = expectedProductName,
                PromotionType = expectedPromotionType,
                CustomerAmount = expectedCustomerAmount
            };

            // Assert
            Assert.Equal(expectedDate, dto.Date);
            Assert.Equal(expectedDateString, dto.DateString);
            Assert.Equal(expectedExternalId, dto.ExternalId);
            Assert.Equal(expectedSystemId, dto.SystemId);
            Assert.Equal(expectedRefNo, dto.RefNo);
            Assert.Equal(expectedProductCode, dto.ProductCode);
            Assert.Equal(expectedProductName, dto.ProductName);
            Assert.Equal(expectedPromotionType, dto.PromotionType);
            Assert.Equal(expectedCustomerAmount, dto.CustomerAmount);
        }

        [Fact]
        public void OddsBoostBonusDetail_DefaultConstructor_InitializesWithDefaultValues()
        {
            // Act
            var dto = new OddsBoostBonusDetail();

            // Assert
            Assert.Equal(default(DateTime), dto.Date);
            Assert.Null(dto.DateString);
            Assert.Null(dto.ExternalId);
            Assert.Null(dto.SystemId);
            Assert.Null(dto.RefNo);
            Assert.Null(dto.ProductCode);
            Assert.Null(dto.ProductName);
            Assert.Null(dto.PromotionType);
            Assert.Equal(default(decimal), dto.CustomerAmount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        public void OddsBoostBonusDetail_CustomerAmount_AcceptsVariousDecimalValues(decimal amount)
        {
            // Arrange & Act
            var dto = new OddsBoostBonusDetail { CustomerAmount = amount };

            // Assert
            Assert.Equal(amount, dto.CustomerAmount);
        }
    }
}
