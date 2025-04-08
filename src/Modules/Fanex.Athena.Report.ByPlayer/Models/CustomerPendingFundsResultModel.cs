namespace Fanex.Athena.Report.ByPlayer.Models
{
    using System.Collections.Generic;
    using DTO;

    public class CustomerPendingFundsResultModel
    {
        public IEnumerable<CustPendingFunds> Data { get; set; }

        public CustPendingFunds Total { get; set; }

        public int TotalRows { get; set; }
    }
}