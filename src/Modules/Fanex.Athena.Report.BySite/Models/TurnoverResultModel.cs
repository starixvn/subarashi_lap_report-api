using Fanex.Athena.Report.BySite.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.BySite.Models
{
    public class TurnoverResultModel
    {
        public TurnoverDto TotalData { get; set; }

        public IList<TurnoverDto> Data { get; set; }
    }
}
