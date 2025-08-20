namespace Fanex.Athena.Report.BySite.UnitTests.DTOs
{
    using BySite.DTOs;
    using Xunit;

    public class AlertRuleTests
    {
        [Fact]
        public void JsonRuleItem_Properties_CanBeSetAndRetrieved()
        {
            var ruleItem = new JsonRuleItem
            {
                RuleName = "MaxBetAmount",
                Threshold = 1000.50m
            };

            Assert.Equal("MaxBetAmount", ruleItem.RuleName);
            Assert.Equal(1000.50m, ruleItem.Threshold);
        }

        [Fact]
        public void JsonRuleItem_DefaultValues_AreCorrect()
        {
            var ruleItem = new JsonRuleItem();

            Assert.Null(ruleItem.RuleName);
            Assert.Equal(0m, ruleItem.Threshold);
        }

        [Fact]
        public void JsonRuleUpdateItem_Properties_CanBeSetAndRetrieved()
        {
            var updateItem = new JsonRuleUpdateItem
            {
                RuleId = 42,
                Threshold = 2500.75m
            };

            Assert.Equal(42, updateItem.RuleId);
            Assert.Equal(2500.75m, updateItem.Threshold);
        }

        [Fact]
        public void JsonRuleUpdateItem_DefaultValues_AreCorrect()
        {
            var updateItem = new JsonRuleUpdateItem();

            Assert.Equal(0, updateItem.RuleId);
            Assert.Equal(0m, updateItem.Threshold);
        }

        [Theory]
        [InlineData(-100.5, 0.25)]
        [InlineData(0, 0)]
        [InlineData(1000, 2.5)]
        public void AlertRule_WithVariousValues_StoresCorrectly(decimal negativeProfit, decimal betRatio)
        {
            var alertRule = new AlertRule
            {
                NegativeProfit = negativeProfit,
                BetRatio = betRatio
            };

            Assert.Equal(negativeProfit, alertRule.NegativeProfit);
            Assert.Equal(betRatio, alertRule.BetRatio);
        }
    }
}