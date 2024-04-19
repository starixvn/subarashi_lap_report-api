namespace Fanex.Athena.Report.BetList.Models
{
    public class BetListSettingModel
    {
        public string Title { get; set; }

        public string BaseCurrency { get; set; }

        public bool IsServerPaging { get; set; }
        
        public bool IsAllowedCustomerCommision { get; set; }

        public string ExcelPath { get; set; }

        public string ExcelFileName { get; set; }
    }
}
