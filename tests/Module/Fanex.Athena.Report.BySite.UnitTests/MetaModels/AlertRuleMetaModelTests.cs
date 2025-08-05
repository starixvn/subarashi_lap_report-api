using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.MetaModels
{
    public class AlertRuleMetaModelTests
    {
        [Fact]
        public void AlertRuleMetaModel_Properties_CanBeSetAndRetrieved()
        {
            var alertRule = new AlertRule
            {
                NegativeProfit = 100.50m,
                BetRatio = 0.75m
            };

            var metaModel = new AlertRuleMetaModel
            {
                MonitoringID = 1,
                SettingId = 2,
                SettingName = "Test Setting",
                ConditionType = 3,
                ConditionTypeName = "Critical",
                BaseCurrency = "USD",
                NotificationInterval = 60,
                AlertRule = alertRule,
                RulesJson = "{\"rules\":[]}",
                UserId = 123,
                Language = "en",
                IP = "192.168.1.1"
            };

            Assert.Equal(1, metaModel.MonitoringID);
            Assert.Equal(2, metaModel.SettingId);
            Assert.Equal("Test Setting", metaModel.SettingName);
            Assert.Equal(3, metaModel.ConditionType);
            Assert.Equal("Critical", metaModel.ConditionTypeName);
            Assert.Equal("USD", metaModel.BaseCurrency);
            Assert.Equal(60, metaModel.NotificationInterval);
            Assert.Equal(alertRule, metaModel.AlertRule);
            Assert.Equal("{\"rules\":[]}", metaModel.RulesJson);
            Assert.Equal(123, metaModel.UserId);
            Assert.Equal("en", metaModel.Language);
            Assert.Equal("192.168.1.1", metaModel.IP);
        }

        [Fact]
        public void AlertRuleMetaModel_DefaultValues_AreCorrect()
        {
            var metaModel = new AlertRuleMetaModel();

            Assert.Equal(0, metaModel.MonitoringID);
            Assert.Equal(0, metaModel.SettingId);
            Assert.Null(metaModel.SettingName);
            Assert.Equal(0, metaModel.ConditionType);
            Assert.Null(metaModel.ConditionTypeName);
            Assert.Null(metaModel.BaseCurrency);
            Assert.Equal(0, metaModel.NotificationInterval);
            Assert.Null(metaModel.AlertRule);
            Assert.Null(metaModel.RulesJson);
            Assert.Equal(0, metaModel.UserId);
            Assert.Null(metaModel.Language);
            Assert.Null(metaModel.IP);
        }

        [Fact]
        public void AlertRule_Properties_CanBeSetAndRetrieved()
        {
            var alertRule = new AlertRule
            {
                NegativeProfit = -500.25m,
                BetRatio = 1.5m
            };

            Assert.Equal(-500.25m, alertRule.NegativeProfit);
            Assert.Equal(1.5m, alertRule.BetRatio);
        }

        [Fact]
        public void AlertRule_DefaultValues_AreZero()
        {
            var alertRule = new AlertRule();

            Assert.Equal(0m, alertRule.NegativeProfit);
            Assert.Equal(0m, alertRule.BetRatio);
        }
    }
}