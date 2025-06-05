namespace Fanex.Athena.Report.BySite.DTOs
{
    public class MatchMonitoringSetting
    {
        public int SettingID { get; set; }

        public string SettingName { get; set; }

        public string ConditionTypeName { get; set; }

        public string BaseCurrency { get; set; }

        public short NotificationInterval { get; set; }

        public string Rules { get; set; }

        public AlertRule AlertRule { get; set; }
    }
}