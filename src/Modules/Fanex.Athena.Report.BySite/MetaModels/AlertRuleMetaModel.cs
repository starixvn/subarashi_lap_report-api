namespace Fanex.Athena.Report.BySite.MetaModels
{
    using Fanex.Athena.Report.BySite.DTOs;

    public class AlertRuleMetaModel
    {
        public int MonitoringID { get; set; }

        public int SettingId { get; set; }

        public string SettingName { get; set; }

        public byte ConditionType { get; set; }

        public string ConditionTypeName { get; set; }

        public string BaseCurrency { get; set; }

        public short NotificationInterval { get; set; }

        public AlertRule AlertRule { get; set; }

        public string RulesJson { get; set; }

        public int UserId { get; set; }

        public string IP { get; set; }
    }
}