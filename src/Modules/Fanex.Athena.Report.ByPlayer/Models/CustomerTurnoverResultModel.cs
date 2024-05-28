using System.Collections.Generic;
using Fanex.Athena.Report.ByPlayer.DTO;

namespace Fanex.Athena.Report.ByPlayer.Models
{
    public class CustomerTurnoverResultModel
    {
        public IEnumerable<CustTurnover> Data { get; set; }

        public CustTurnover TotalData { get; set; }

        public int TotalRow { get; set; }
    }
}
