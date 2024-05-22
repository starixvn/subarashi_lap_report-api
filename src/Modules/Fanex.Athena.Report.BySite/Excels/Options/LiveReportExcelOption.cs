using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public class LiveReportExcelOption : IConvertingOption
    {
        public bool CustomerCommVisible { get; set; }

        public bool BaVisible { get; set; }

        public bool HasAmountDueToColumn { get; set; }

        public bool IsConvert { get; set; } = true;

        public string BaseCurrency { get; set; } = @"EUR";

        public string CurrencyName { get; set; }
    }
}
