namespace Fanex.Athena.Report.BySite.DTOs
{
    public class AlertRule
    {
        public decimal NegativeProfit { get; set; }

        public decimal BetRatio { get; set; }
    }

    public class JsonRuleItem
    {
        public string RuleName { get; set; }

        public decimal Threshold { get; set; }
    }

    public class JsonRuleUpdateItem
    {
        public int RuleId { get; set; }

        public decimal Threshold { get; set; }
    }
}