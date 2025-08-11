namespace Fanex.Athena.Report.BySite.UnitTests.DTOs
{
    using AutoFixture;
    using BySite.DTOs;
    using Xunit;

    public class AlertRuleTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void AlertRule_CanBeInstantiated()
        {
            // Act
            var alertRule = new AlertRule();

            // Assert
            Assert.NotNull(alertRule);
        }

        [Fact]
        public void AlertRule_PropertiesCanBeSetAndGet()
        {
            // Arrange
            var alertRule = new AlertRule();
            var expectedNegativeProfit = _fixture.Create<decimal>();
            var expectedBetRatio = _fixture.Create<decimal>();

            // Act
            alertRule.NegativeProfit = expectedNegativeProfit;
            alertRule.BetRatio = expectedBetRatio;

            // Assert
            Assert.Equal(expectedNegativeProfit, alertRule.NegativeProfit);
            Assert.Equal(expectedBetRatio, alertRule.BetRatio);
        }

        [Fact]
        public void AlertRule_DefaultValues()
        {
            // Act
            var alertRule = new AlertRule();

            // Assert
            Assert.Equal(0m, alertRule.NegativeProfit);
            Assert.Equal(0m, alertRule.BetRatio);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        [InlineData(-999999.99)]
        public void AlertRule_NegativeProfit_AcceptsVariousValues(decimal negativeProfit)
        {
            // Arrange
            var alertRule = new AlertRule();

            // Act
            alertRule.NegativeProfit = negativeProfit;

            // Assert
            Assert.Equal(negativeProfit, alertRule.NegativeProfit);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(100.0)]
        public void AlertRule_BetRatio_AcceptsVariousValues(decimal betRatio)
        {
            // Arrange
            var alertRule = new AlertRule();

            // Act
            alertRule.BetRatio = betRatio;

            // Assert
            Assert.Equal(betRatio, alertRule.BetRatio);
        }

        [Fact]
        public void AlertRule_AllPropertiesCanBeSetInObjectInitializer()
        {
            // Arrange & Act
            var alertRule = new AlertRule
            {
                NegativeProfit = -1000.50m,
                BetRatio = 2.5m
            };

            // Assert
            Assert.Equal(-1000.50m, alertRule.NegativeProfit);
            Assert.Equal(2.5m, alertRule.BetRatio);
        }
    }

    public class JsonRuleItemTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void JsonRuleItem_CanBeInstantiated()
        {
            // Act
            var item = new JsonRuleItem();

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public void JsonRuleItem_PropertiesCanBeSetAndGet()
        {
            // Arrange
            var item = new JsonRuleItem();
            var expectedRuleName = _fixture.Create<string>();
            var expectedThreshold = _fixture.Create<decimal>();

            // Act
            item.RuleName = expectedRuleName;
            item.Threshold = expectedThreshold;

            // Assert
            Assert.Equal(expectedRuleName, item.RuleName);
            Assert.Equal(expectedThreshold, item.Threshold);
        }

        [Fact]
        public void JsonRuleItem_DefaultValues()
        {
            // Act
            var item = new JsonRuleItem();

            // Assert
            Assert.Null(item.RuleName);
            Assert.Equal(0m, item.Threshold);
        }
    }

    public class JsonRuleUpdateItemTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void JsonRuleUpdateItem_CanBeInstantiated()
        {
            // Act
            var item = new JsonRuleUpdateItem();

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public void JsonRuleUpdateItem_PropertiesCanBeSetAndGet()
        {
            // Arrange
            var item = new JsonRuleUpdateItem();
            var expectedRuleId = _fixture.Create<int>();
            var expectedThreshold = _fixture.Create<decimal>();

            // Act
            item.RuleId = expectedRuleId;
            item.Threshold = expectedThreshold;

            // Assert
            Assert.Equal(expectedRuleId, item.RuleId);
            Assert.Equal(expectedThreshold, item.Threshold);
        }

        [Fact]
        public void JsonRuleUpdateItem_DefaultValues()
        {
            // Act
            var item = new JsonRuleUpdateItem();

            // Assert
            Assert.Equal(0, item.RuleId);
            Assert.Equal(0m, item.Threshold);
        }

        [Fact]
        public void JsonRuleUpdateItem_AllPropertiesCanBeSetInObjectInitializer()
        {
            // Arrange & Act
            var item = new JsonRuleUpdateItem
            {
                RuleId = 123,
                Threshold = 456.78m
            };

            // Assert
            Assert.Equal(123, item.RuleId);
            Assert.Equal(456.78m, item.Threshold);
        }
    }
}
