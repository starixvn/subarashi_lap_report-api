using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;

namespace Fanex.Athena.Report.BySite.Models
{
    public class TurnoverResultModel
    {
        public TurnoverDto TotalData { get; set; }

        public IList<TurnoverDto> Data { get; set; }
    }
}
