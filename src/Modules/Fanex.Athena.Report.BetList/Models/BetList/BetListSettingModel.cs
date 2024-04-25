using System;

namespace Fanex.Athena.Report.BetList.Models.BetList
{
    public class BetListSettingModel
    {
        public string Title { get; set; }

        public string BaseCurrency { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public bool IsServerPaging { get; set; }

        public bool IsAllowedCustomerCommision { get; set; }
    }
}
