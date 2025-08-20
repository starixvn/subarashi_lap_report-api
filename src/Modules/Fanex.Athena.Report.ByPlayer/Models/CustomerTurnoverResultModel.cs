namespace Fanex.Athena.Report.ByPlayer.Models
{
    using System.Collections.Generic;
    using DTO;

    public class CustomerTurnoverResultModel
    {
        public IEnumerable<CustTurnover> Data { get; set; }

        public CustTurnover TotalData { get; set; }

        public int TotalRow { get; set; }
    }
}
