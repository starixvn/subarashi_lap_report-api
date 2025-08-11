namespace Fanex.Athena.Report.BySite.Models
{
    using System.Collections.Generic;
    using DTOs;

    public class TurnoverResultModel
    {
        public TurnoverDto TotalData { get; set; }

        public IList<TurnoverDto> Data { get; set; }
    }
}
