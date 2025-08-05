using AutoFixture;
using Fanex.Athena.Report.BySite.DTOs;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.DTOs
{
    public class CurrencyTotalTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CurrencyTotal_CanBeInstantiated()
        {
            // Act
            var currencyTotal = new CurrencyTotal();

            // Assert
            Assert.NotNull(currencyTotal);
        }

        [Fact]
        public void CurrencyTotal_PropertiesCanBeSetAndGet()
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();
            var expectedCurrencyId = _fixture.Create<int>();
            var expectedCurrencyName = _fixture.Create<string>();
            var expectedSumAmount = _fixture.Create<decimal>();
            var expectedSumContributionAmount = _fixture.Create<decimal>();
            var expectedSumTipSharing = _fixture.Create<decimal>();
            var expectedSumTurnover = _fixture.Create<decimal>();

            // Act
            currencyTotal.CurrencyId = expectedCurrencyId;
            currencyTotal.CurrencyName = expectedCurrencyName;
            currencyTotal.SumAmount = expectedSumAmount;
            currencyTotal.SumContributionAmount = expectedSumContributionAmount;
            currencyTotal.SumTipSharing = expectedSumTipSharing;
            currencyTotal.SumTurnover = expectedSumTurnover;

            // Assert
            Assert.Equal(expectedCurrencyId, currencyTotal.CurrencyId);
            Assert.Equal(expectedCurrencyName, currencyTotal.CurrencyName);
            Assert.Equal(expectedSumAmount, currencyTotal.SumAmount);
            Assert.Equal(expectedSumContributionAmount, currencyTotal.SumContributionAmount);
            Assert.Equal(expectedSumTipSharing, currencyTotal.SumTipSharing);
            Assert.Equal(expectedSumTurnover, currencyTotal.SumTurnover);
        }

        [Fact]
        public void CurrencyTotal_DefaultValues()
        {
            // Act
            var currencyTotal = new CurrencyTotal();

            // Assert
            Assert.Equal(0, currencyTotal.CurrencyId);
            Assert.Null(currencyTotal.CurrencyName);
            Assert.Equal(0m, currencyTotal.SumAmount);
            Assert.Equal(0m, currencyTotal.SumContributionAmount);
            Assert.Equal(0m, currencyTotal.SumTipSharing);
            Assert.Equal(0m, currencyTotal.SumTurnover);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void CurrencyTotal_CurrencyId_AcceptsVariousValues(int currencyId)
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();

            // Act
            currencyTotal.CurrencyId = currencyId;

            // Assert
            Assert.Equal(currencyId, currencyTotal.CurrencyId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("GBP")]
        [InlineData("Very Long Currency Name")]
        public void CurrencyTotal_CurrencyName_AcceptsVariousValues(string currencyName)
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();

            // Act
            currencyTotal.CurrencyName = currencyName;

            // Assert
            Assert.Equal(currencyName, currencyTotal.CurrencyName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        [InlineData(-999999.99)]
        public void CurrencyTotal_SumAmount_AcceptsVariousValues(decimal sumAmount)
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();

            // Act
            currencyTotal.SumAmount = sumAmount;

            // Assert
            Assert.Equal(sumAmount, currencyTotal.SumAmount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        [InlineData(-999999.99)]
        public void CurrencyTotal_SumContributionAmount_AcceptsVariousValues(decimal sumContributionAmount)
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();

            // Act
            currencyTotal.SumContributionAmount = sumContributionAmount;

            // Assert
            Assert.Equal(sumContributionAmount, currencyTotal.SumContributionAmount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10.50)]
        [InlineData(5.25)]
        [InlineData(999.99)]
        [InlineData(-10.50)]
        public void CurrencyTotal_SumTipSharing_AcceptsVariousValues(decimal sumTipSharing)
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();

            // Act
            currencyTotal.SumTipSharing = sumTipSharing;

            // Assert
            Assert.Equal(sumTipSharing, currencyTotal.SumTipSharing);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000.50)]
        [InlineData(500.25)]
        [InlineData(9999999.99)]
        [InlineData(-1000.50)]
        public void CurrencyTotal_SumTurnover_AcceptsVariousValues(decimal sumTurnover)
        {
            // Arrange
            var currencyTotal = new CurrencyTotal();

            // Act
            currencyTotal.SumTurnover = sumTurnover;

            // Assert
            Assert.Equal(sumTurnover, currencyTotal.SumTurnover);
        }

        [Fact]
        public void CurrencyTotal_MultipleInstances_AreIndependent()
        {
            // Arrange
            var currencyTotal1 = new CurrencyTotal();
            var currencyTotal2 = new CurrencyTotal();

            // Act
            currencyTotal1.CurrencyId = 1;
            currencyTotal1.CurrencyName = "USD";
            currencyTotal1.SumAmount = 1000m;

            currencyTotal2.CurrencyId = 2;
            currencyTotal2.CurrencyName = "EUR";
            currencyTotal2.SumAmount = 2000m;

            // Assert
            Assert.Equal(1, currencyTotal1.CurrencyId);
            Assert.Equal("USD", currencyTotal1.CurrencyName);
            Assert.Equal(1000m, currencyTotal1.SumAmount);

            Assert.Equal(2, currencyTotal2.CurrencyId);
            Assert.Equal("EUR", currencyTotal2.CurrencyName);
            Assert.Equal(2000m, currencyTotal2.SumAmount);
        }

        [Fact]
        public void CurrencyTotal_AllPropertiesCanBeSetInObjectInitializer()
        {
            // Arrange & Act
            var currencyTotal = new CurrencyTotal
            {
                CurrencyId = 1,
                CurrencyName = "USD",
                SumAmount = 1500.75m,
                SumContributionAmount = 250.50m,
                SumTipSharing = 75.25m,
                SumTurnover = 1826.50m
            };

            // Assert
            Assert.Equal(1, currencyTotal.CurrencyId);
            Assert.Equal("USD", currencyTotal.CurrencyName);
            Assert.Equal(1500.75m, currencyTotal.SumAmount);
            Assert.Equal(250.50m, currencyTotal.SumContributionAmount);
            Assert.Equal(75.25m, currencyTotal.SumTipSharing);
            Assert.Equal(1826.50m, currencyTotal.SumTurnover);
        }

        [Fact]
        public void CurrencyTotal_CalculatedValues_WorkCorrectly()
        {
            // Arrange
            var currencyTotal = new CurrencyTotal
            {
                SumAmount = 1000m,
                SumContributionAmount = 100m,
                SumTipSharing = 50m,
                SumTurnover = 1150m
            };

            // Act
            var totalContributions = currencyTotal.SumContributionAmount + currencyTotal.SumTipSharing;
            var netAmount = currencyTotal.SumAmount - totalContributions;

            // Assert
            Assert.Equal(150m, totalContributions); // 100 + 50
            Assert.Equal(850m, netAmount); // 1000 - 150
            Assert.Equal(1150m, currencyTotal.SumTurnover);
        }
    }
}
